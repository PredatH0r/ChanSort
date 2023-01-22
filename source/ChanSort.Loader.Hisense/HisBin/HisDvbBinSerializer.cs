using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Hisense.HisBin;

/*
 * Loads Hisense HIS_DVB.BIN channel lists
 *
 * See also the his-dvb.h file in Information/FileStructures_for_HHD_Hex_Editor_Neo
 *
 * Some properties of these lists:
 * - channel records are physically ordered by their program number
 * - TV and radio are managed in separate lists, both starting at 1
 * - channel and provider names are raw DVB strings including control bytes
 */
public class HisDvbBinSerializer : SerializerBase
{
  private readonly ChannelList antTvChannels = new(SignalSource.DvbT | SignalSource.Tv, "Antenna TV");
  private readonly ChannelList antRadioChannels = new(SignalSource.DvbT | SignalSource.Radio, "Antenna Radio");
  private readonly ChannelList antDataChannels = new(SignalSource.DvbT | SignalSource.Data, "Antenna Data");
  private readonly ChannelList cabTvChannels = new(SignalSource.DvbC | SignalSource.Tv, "Cable TV");
  private readonly ChannelList cabRadioChannels = new(SignalSource.DvbC | SignalSource.Radio, "Cable Radio");
  private readonly ChannelList cabDataChannels = new(SignalSource.DvbC | SignalSource.Data, "Cable Data");
  private readonly ChannelList satTvChannels = new(SignalSource.DvbS | SignalSource.Tv, "Sat TV");
  private readonly ChannelList satRadioChannels = new(SignalSource.DvbS | SignalSource.Radio, "Sat Radio");
  private readonly ChannelList satDataChannels = new(SignalSource.DvbS | SignalSource.Data, "Sat Data");

  private SubListInfo[] subListInfos;
  
  private byte[] fileContent;
  private int headerRecordSize, antRecordSize, cabRecordSize, satRecordSize;

  private const string ERR_badFileFormat = "The content of the file doesn't match the expected format.";

  private IniFile ini;
  private DataMapping headerMapping, dvbMapping;
  private DvbStringDecoder dvbStringDecoder;

  #region class SubListInfo
  private struct SubListInfo
  {
    public int Count;
    public int Size;
    public ChannelList TvList;
    public ChannelList RadioList;
    public ChannelList DataList;
    public bool IsSat;

    public SubListInfo(int count, int size, ChannelList tvList, ChannelList radioList, ChannelList dataList, bool isSat)
    {
      Count = count;
      Size = size;
      TvList = tvList;
      RadioList = radioList;
      DataList = dataList;
      IsSat = isSat;
    }
  }
  #endregion

  #region ctor()
  public HisDvbBinSerializer(string inputFile) : base(inputFile)
  {
    this.Features.ChannelNameEdit = ChannelNameEditMode.None;
    this.Features.CanSkipChannels = true;
    this.Features.CanLockChannels = true;
    this.Features.CanHideChannels = false;
    this.Features.FavoritesMode = FavoritesMode.Flags;
    this.Features.MaxFavoriteLists = 1;
    this.Features.DeleteMode = DeleteMode.Physically;
    this.Features.CanHaveGaps = true;
    this.Features.AllowGapsInFavNumbers = false;
    this.ReadConfigurationFromIniFile();

    this.DataRoot.AddChannelList(antTvChannels);
    this.DataRoot.AddChannelList(antRadioChannels);
    this.DataRoot.AddChannelList(antDataChannels);
    this.DataRoot.AddChannelList(cabTvChannels);
    this.DataRoot.AddChannelList(cabRadioChannels);
    this.DataRoot.AddChannelList(cabDataChannels);
    this.DataRoot.AddChannelList(satTvChannels);
    this.DataRoot.AddChannelList(satRadioChannels);
    this.DataRoot.AddChannelList(satDataChannels);
    foreach (var list in this.DataRoot.ChannelLists)
    {
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ChannelOrTransponder));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Encrypted));
      list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.ServiceType));
    }
  }
  #endregion

  #region ReadConfigurationFromIniFile()

  private void ReadConfigurationFromIniFile()
  {
    string iniFile = this.GetType().Assembly.Location.ToLower().Replace(".dll", ".ini");
    this.ini = new IniFile(iniFile);
    this.headerMapping = new DataMapping(ini.GetSection("HIS_DVB.BIN"));
    this.headerRecordSize = headerMapping.Settings.GetInt("HeaderSize");

    this.dvbMapping = new DataMapping(ini.GetSection("HIS_DVB.BIN_Record"));
    this.antRecordSize = this.dvbMapping.Settings.GetInt("RecordSizeDvbT");
    this.cabRecordSize = this.dvbMapping.Settings.GetInt("RecordSizeDvbC");
    this.satRecordSize = this.dvbMapping.Settings.GetInt("RecordSizeDvbS");
    this.dvbMapping.DefaultEncoding = this.DefaultEncoding;
  }
  #endregion


  #region Load()

  public override void Load()
  {
    this.fileContent = File.ReadAllBytes(this.FileName);

    this.headerMapping.SetDataPtr(this.fileContent, 0);

    var antChannelCount = this.headerMapping.GetWord("NumChannelsDvbT");
    var cabChannelCount = this.headerMapping.GetWord("NumChannelsDvbC");
    var satChannelCount = this.headerMapping.GetWord("NumChannelsDvbS");

    var expectedSize = headerRecordSize + antChannelCount * antRecordSize + cabChannelCount * cabRecordSize + satChannelCount * satRecordSize;
    if (this.fileContent.Length != expectedSize)
      throw new FileLoadException(ERR_badFileFormat);

    this.dvbStringDecoder = new DvbStringDecoder(this.DefaultEncoding);

    var nameOffset = dvbMapping.Settings.GetInt("Name");
    var nameLength = dvbMapping.Settings.GetInt("NameLength");
    var providerOffset = dvbMapping.Settings.GetInt("Provider");
    var providerLength = dvbMapping.Settings.GetInt("ProviderLength");

    this.subListInfos = new SubListInfo[]
    {
      new (antChannelCount, antRecordSize, antTvChannels, antRadioChannels, antDataChannels, false),
      new (cabChannelCount, cabRecordSize, cabTvChannels, cabRadioChannels, cabDataChannels, false),
      new (satChannelCount, satRecordSize, satTvChannels, satRadioChannels, satDataChannels, true)
    };

    var off = headerRecordSize;
    foreach (var info in this.subListInfos)
    {
      for (int index = 0; index < info.Count; index++)
      {
        dvbMapping.SetDataPtr(fileContent, off);
        var ci = ReadChannel(index, info.IsSat, nameOffset, nameLength, providerOffset, providerLength);
        if (ci != null)
        {
          var src = ci.SignalSource & SignalSource.MaskTvRadioData;
          var channels = src == SignalSource.Tv ? info.TvList : src == SignalSource.Radio ? info.RadioList : info.DataList;
          this.DataRoot.AddChannel(channels, ci);
        }

        off += info.Size;
      }
    }
  }
  #endregion

  #region ReadChannel()
  private ChannelInfo ReadChannel(int index, bool isSat, int nameOffset, int nameLength, int providerOffset, int providerLength)
  {
    ChannelInfo ci = new ChannelInfo(0, index, 0, "");
    ci.RawDataOffset = dvbMapping.BaseOffset;

    var type = dvbMapping.GetByte("TvRadioData");
    if (type == 1)
      ci.SignalSource |= SignalSource.Tv;
    else if (type == 2)
      ci.SignalSource |= SignalSource.Radio;
    else
      ci.SignalSource |= SignalSource.Data;

    ci.Favorites = dvbMapping.GetByte("Fav") != 0 ? Favorites.A : 0;
    ci.Skip = dvbMapping.GetByte("Skip") != 0;
    ci.Lock = dvbMapping.GetByte("Lock") != 0;
    ci.ServiceType = dvbMapping.GetByte("ServiceType");
    ci.PcrPid = dvbMapping.GetWord("PcrPid");
    ci.VideoPid = dvbMapping.GetWord("VideoPid");
    ci.OldProgramNr = dvbMapping.GetWord("ProgNum");
    ci.ServiceId = dvbMapping.GetWord("ServiceId");
    ci.AudioPid = dvbMapping.GetWord("AudioPid");

    this.dvbStringDecoder.GetChannelNames(fileContent, ci.RawDataOffset + nameOffset, nameLength, out var longName, out var shortName);
    ci.Name = longName;
    ci.ShortName = shortName;

    this.dvbStringDecoder.GetChannelNames(fileContent, ci.RawDataOffset + providerOffset, providerLength, out var provider, out _);
    ci.Provider = provider;

    ci.TransportStreamId = dvbMapping.GetWord("Tsid");
    ci.OriginalNetworkId = dvbMapping.GetWord("Onid");
    ci.FreqInMhz = dvbMapping.GetDword("Frequency");
    if (ci.FreqInMhz > 20000) // DVB-C/T has value in kHZ, DVB-S in MHz
      ci.FreqInMhz /= 1000;
    ci.SymbolRate = dvbMapping.GetWord("SymbolRate");

    if (isSat)
      ci.Satellite = dvbMapping.GetString("SatName", dvbMapping.Settings.GetInt("SatNameLength"));

    return ci;
  }
  #endregion


  // Saving ====================================

  #region Save()
  public override void Save()
  {
    using var mem = new MemoryStream(this.fileContent.Length);
    using var writer = new BinaryWriter(mem);
    writer.Write(this.fileContent, 0, this.headerRecordSize);

    foreach (var info in this.subListInfos)
    {
      int newIndex = 0;
      foreach (var list in new[] { info.TvList, info.RadioList, info.DataList })
      {
        var order = list.Channels.OrderBy(c => c, new DelegateComparer<ChannelInfo>(OrderChannelsComparer)).ToList();

        foreach (var channel in order)
        {
          if (channel.IsDeleted)
            continue;

          // copy original data
          var offset = writer.BaseStream.Position;
          writer.Write(this.fileContent, channel.RawDataOffset, info.Size);
          writer.Flush();

          // prepare to overwrite with some new values
          dvbMapping.SetDataPtr(mem.GetBuffer(), (int)offset);
          dvbMapping.SetWord("ProgNum", channel.NewProgramNr);
          dvbMapping.SetByte("Skip", channel.Skip ? 1 : 0);
          dvbMapping.SetByte("Lock", channel.Lock ? 1 : 0);
          dvbMapping.SetByte("Fav", channel.Favorites != 0 ? 1 : 0);

          channel.RecordIndex = newIndex++;
          channel.RawDataOffset = (int)offset;
        }

        // update number of channels in header
        headerMapping.SetDataPtr(mem.GetBuffer(), 0);
        headerMapping.SetDword("NumSatChannels", newIndex);
      }
    }

    // write to file
    this.fileContent = new byte[mem.Length];
    Tools.MemCopy(mem.GetBuffer(), 0, this.fileContent, 0, (int)mem.Length);
    File.WriteAllBytes(this.FileName, this.fileContent);
  }
  #endregion

  #region OrderChannelsComparer()
  private int OrderChannelsComparer(ChannelInfo a, ChannelInfo b)
  {
    // deleted channels to the end
    if (a.NewProgramNr < 0)
      return b.NewProgramNr == 0 ? a.RecordIndex.CompareTo(b.RecordIndex) : +1;
    if (b.NewProgramNr < 0)
      return -1;

    return a.NewProgramNr.CompareTo(b.NewProgramNr);
  }
  #endregion

  
  // Infrastructure ============================

  #region DefaultEncoding
  public override Encoding DefaultEncoding
  {
    get => base.DefaultEncoding;
    set
    {
      if (value == this.DefaultEncoding)
        return;
      base.DefaultEncoding = value;
      this.dvbMapping.DefaultEncoding = value;

      if (this.dvbStringDecoder != null)
      {
        this.dvbStringDecoder.DefaultEncoding = value;
        this.ReparseNames();
      }
    }
  }
  #endregion

  #region ReparseNames()
  private void ReparseNames()
  {
    var nameOffset = dvbMapping.Settings.GetInt("Name");
    var nameLength = dvbMapping.Settings.GetInt("NameLength");
    var providerOffset = dvbMapping.Settings.GetInt("Provider");
    var providerLength = dvbMapping.Settings.GetInt("ProviderLength");

    foreach (var list in this.DataRoot.ChannelLists)
    {
      if (list.IsMixedSourceFavoritesList)
        continue;
      foreach (var chan in list.Channels)
      {
        dvbMapping.BaseOffset = chan.RawDataOffset;
        
        this.dvbStringDecoder.GetChannelNames(this.fileContent, chan.RawDataOffset + nameOffset, nameLength, out var longName, out var shortName);
        chan.Name = longName;
        chan.ShortName = shortName;

        this.dvbStringDecoder.GetChannelNames(this.fileContent, chan.RawDataOffset + providerOffset, providerLength, out var provider, out _);
        chan.Provider = provider;
      }
    }
  }
  #endregion
}
