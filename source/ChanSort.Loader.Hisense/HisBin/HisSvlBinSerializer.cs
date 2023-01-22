using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Hisense.HisBin;

/*
 * Loads Hisense HIS_SVL.BIN channel lists
 *
 * This binary format is based on a customized MediaTek format, which means that there may be many incompatible
 * variants that can't be identified and distinguished easily.
 * This loader only supports the known Hisense variant with 304 bytes per channel in HIS_SVL.BIN.
 *
 * See also the his-svl.h file in Information/FileStructures_for_HHD_Hex_Editor_Neo
 *
 * Some properties of these lists:
 * - channel records are physically ordered by recordId, but not necessarily by channelId (holding the program number).
 * - it's unknown if TV, radio, data must be grouped together or if they can be mixed
 * - favorite lists allow mixing channels from different inputs and also radio and TV
 * - character encoding is implicit and can be UTF8 or latin-1
 */
public class HisSvlBinSerializer : SerializerBase
{
  private readonly ChannelList dvbtChannels = new (SignalSource.DvbT | SignalSource.Tv | SignalSource.Radio, "DVB-T");
  private readonly ChannelList dvbcChannels = new (SignalSource.DvbC | SignalSource.Tv | SignalSource.Radio, "DVB-C");
  private readonly ChannelList dvbsChannels = new (SignalSource.DvbS | SignalSource.Tv | SignalSource.Radio, "DVB-S");
  private readonly ChannelList favChannels = new(SignalSource.All, "Fav") { IsMixedSourceFavoritesList = true };

  private string favFileName;
  private byte[] svlFileContent;
  private byte[] tslFileContent;
  private const int MaxFileSize = 4 << 20; // 4 MB

  private int headerRecordSize, svlRecordSize;
  private int tSize, cSize, sSize;

  private const string ERR_fileTooBig = "The file size {0} is larger than the allowed maximum of {1}.";
  private const string ERR_badFileFormat = "The content of the file doesn't match the expected format.";

  private IniFile ini;
  private DataMapping headerMapping, svlMapping, tslMapping, dvbMapping, favMapping;
  private readonly Dictionary<int, Transponder> transponder = new ();

  #region ctor()
  public HisSvlBinSerializer(string inputFile) : base(inputFile)
  {
    this.Features.ChannelNameEdit = ChannelNameEditMode.All;
    this.Features.CanSkipChannels = true;
    this.Features.CanLockChannels = true;
    this.Features.CanHideChannels = false;
    this.Features.FavoritesMode = FavoritesMode.MixedSource;
    this.Features.MaxFavoriteLists = 4;
    this.Features.DeleteMode = DeleteMode.Physically;
    this.Features.CanHaveGaps = false;
    this.Features.AllowGapsInFavNumbers = false;
    this.ReadConfigurationFromIniFile();

    this.DataRoot.AddChannelList(dvbcChannels);
    this.DataRoot.AddChannelList(dvbtChannels);
    this.DataRoot.AddChannelList(dvbsChannels);
    this.DataRoot.AddChannelList(favChannels);
    foreach (var list in this.DataRoot.ChannelLists)
    {
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.PcrPid));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.VideoPid));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Satellite));
      list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.ServiceType));
    }
  }
  #endregion

  #region ReadConfigurationFromIniFile()

  private void ReadConfigurationFromIniFile()
  {
    string iniFile = this.GetType().Assembly.Location.ToLower().Replace(".dll", ".ini");
    this.ini = new IniFile(iniFile);
    this.headerMapping = new DataMapping(ini.GetSection("Header"));
    this.headerRecordSize = headerMapping.Settings.GetInt("RecordSize");

    this.svlMapping = new DataMapping(ini.GetSection("SVL_Record"));
    this.svlMapping.DefaultEncoding = this.DefaultEncoding;
    this.tslMapping = new DataMapping(ini.GetSection("TSL_Record"));
    this.tslMapping.DefaultEncoding = this.DefaultEncoding;
    this.dvbMapping = new DataMapping(ini.GetSection("DVB_Data"));
    this.dvbMapping.DefaultEncoding = this.DefaultEncoding;
    this.favMapping = new DataMapping(ini.GetSection("FAV_Record"));

    this.svlRecordSize = this.svlMapping.Settings.GetInt("RecordSize");
  }
  #endregion


  #region Load()

  public override void Load()
  {
    var dir = Path.GetDirectoryName(this.FileName);
    var name = Path.GetFileNameWithoutExtension(this.FileName);
    var i = name.LastIndexOf('_');
    var basename = i < 0 ? name : name.Substring(0, i);
    this.FileName = Path.Combine(dir, basename + "_SVL.BIN");
    this.LoadTslFile(Path.Combine(dir, basename + "_TSL.BIN"));
    this.LoadSvlFile(this.FileName);
    this.favFileName = Path.Combine(dir, basename + "_FAV.BIN");
    this.LoadFavFile(this.favFileName);
  }

  #endregion

  #region LoadTslFile()
  private void LoadTslFile(string fileName)
  {
    long fileSize = new FileInfo(fileName).Length;
    if (fileSize > MaxFileSize)
      throw new FileLoadException(string.Format(ERR_fileTooBig, fileSize, MaxFileSize));
    this.tslFileContent = File.ReadAllBytes(fileName);
    int off = 0;

    tSize = this.ReadHeader(tslFileContent, ref off);
    cSize = this.ReadHeader(tslFileContent, ref off);
    sSize = this.ReadHeader(tslFileContent, ref off);
    this.ReadTransponder(ref off, tSize, 1, 1000000);
    this.ReadTransponder(ref off, cSize, 2, 1000000);
    this.ReadTransponder(ref off, sSize, 3, 1);
  }
  #endregion

  #region ReadTransponder()
  private void ReadTransponder(ref int off, int size, int table, int freqFactor)
  {
    int recordSize = tslMapping.Settings.GetInt("RecordSize");
    if (size % recordSize != 0)
      throw new FileLoadException(ERR_badFileFormat);
    int count = size / recordSize;
    if (count == 0)
      return;

    for (int i = 0; i < count; i++)
    {
      tslMapping.SetDataPtr(tslFileContent, off);
      var id = (table << 16) + tslMapping.GetWord("ID");
      var trans = new Transponder(id);
      trans.FrequencyInMhz = (decimal)tslMapping.GetDword("Frequency") / freqFactor;
      var sym = tslMapping.GetDword("SymbolRate");
      if (sym == 0)
        sym = tslMapping.GetDword("DvbsSymbolRate");
      trans.SymbolRate = (int)(sym > 1000000 ? sym / 1000 : sym);
      trans.OriginalNetworkId = tslMapping.GetWord("Onid");
      if (trans.OriginalNetworkId == 0) // some files have Onid=0 but provide a Nid, which seems to be the Onid
        trans.OriginalNetworkId = tslMapping.GetWord("Nid");
      trans.TransportStreamId = tslMapping.GetWord("Tsid");
      trans.Name = tslMapping.GetString("Name", tslMapping.Settings.GetInt("NameLength"));
      var z = trans.Name.IndexOf('\0');
      if (z >= 0)
        trans.Name = trans.Name.Substring(0, z);
      this.transponder.Add(id, trans);
      off += recordSize;
    }
  }
  #endregion

  #region LoadSvlFile()

  private void LoadSvlFile(string fileName)
  {
    long fileSize = new FileInfo(fileName).Length;
    if (fileSize > MaxFileSize)
      throw new FileLoadException(string.Format(ERR_fileTooBig, fileSize, MaxFileSize));
    this.svlFileContent = File.ReadAllBytes(this.FileName);
    int off = 0;

    tSize = this.ReadHeader(svlFileContent, ref off);
    cSize = this.ReadHeader(svlFileContent, ref off);
    sSize = this.ReadHeader(svlFileContent, ref off);
    this.ReadChannelList(ref off, tSize, 1, dvbtChannels);
    this.ReadChannelList(ref off, cSize, 2, dvbcChannels);
    this.ReadChannelList(ref off, sSize, 3, dvbsChannels);
  }
  #endregion

  #region ReadHeader()
  private int ReadHeader(byte[] data, ref int off)
  {
    if (off + this.headerRecordSize > data.Length)
      throw new FileLoadException(ERR_badFileFormat);
    this.headerMapping.SetDataPtr(data, off);
    var blockSize = (int)this.headerMapping.GetDword("BlockSize");
    if (off + blockSize > data.Length)
      throw new FileLoadException(ERR_badFileFormat);

    off += this.headerRecordSize;
    return blockSize;
  }
  #endregion

  #region ReadChannelList()
  private void ReadChannelList(ref int off, int size, int table, ChannelList channels)
  {
    int recordSize = svlMapping.Settings.GetInt("RecordSize");
    if (size % recordSize != 0)
      throw new FileLoadException(ERR_badFileFormat);
    int channelCount = size / recordSize;
    if (channelCount == 0)
      return;

    var broadcastDataOffset = svlMapping.Settings.GetInt("BroadcastSystemData");
    var nameLength = svlMapping.Settings.GetInt("NameLength");
    var source = channels.SignalSource & (SignalSource.MaskAnalogDigital | SignalSource.MaskAntennaCableSat);
    for (int i = 0; i < channelCount; i++)
    {
      svlMapping.SetDataPtr(svlFileContent, off);
      dvbMapping.SetDataPtr(svlFileContent, off + broadcastDataOffset);
      var ci = ReadChannel(source, i, nameLength);
      if (ci != null)
      {
        this.DataRoot.AddChannel(channels, ci);
        this.favChannels.AddChannel(ci);
      }

      off += recordSize;
    }
  }
  #endregion

  #region ReadChannel()
  private ChannelInfo ReadChannel(SignalSource source, int index, int nameLength)
  {
    var id = svlMapping.GetWord("RecordId");
    ChannelInfo ci = new ChannelInfo(source, id, 0, "");
    ci.RecordOrder = index;
    ci.OldProgramNr = svlMapping.GetWord("ChannelId") >> 2;
    ci.RawDataOffset = svlMapping.BaseOffset;

    var nwMask = svlMapping.GetDword("NwMask");
    ci.Skip = (nwMask & svlMapping.Settings.GetInt("NwMask_Skip")) == 0; // reverse logic!
    ci.Lock = (nwMask & svlMapping.Settings.GetInt("NwMask_Lock")) != 0;
    for (int i = 1; i <= 4; i++)
    {
      bool isFav = (nwMask & svlMapping.Settings.GetInt("NwMask_Fav" + i)) != 0;
      if (isFav)
        ci.Favorites |= (Favorites)(1 << (i-1));
    }
    ci.Encrypted = (nwMask & svlMapping.Settings.GetInt("NwMask_Encrypted")) != 0;

    ci.Name = ReadString(svlMapping, "Name", nameLength);

    var serviceType = svlMapping.GetByte("ServiceType");
    if (serviceType == 1)
    {
      ci.SignalSource |= SignalSource.Tv;
      ci.ServiceTypeName = "TV";
    }
    else if (serviceType == 2)
    {
      ci.SignalSource |= SignalSource.Radio;
      ci.ServiceTypeName = "Radio";
    }
    else
    {
      ci.ServiceTypeName = "Data";
    }

    ci.ServiceId = svlMapping.GetWord("ServiceId");

    int transpTableId = svlMapping.GetWord("TslTableId");
    int transpRecordId = svlMapping.GetWord("TslRecordId");
    var transpId = (transpTableId << 16) + transpRecordId;
    var transp = this.transponder.TryGet(transpId);
    if (transp != null)
    {
      ci.Transponder = transp;
      ci.FreqInMhz = transp.FrequencyInMhz;
      ci.SymbolRate = transp.SymbolRate;
      switch (ci.SymbolRate) // can be either an enum for bandwidth 1=6 MHz, 2=7 MHz, 3=8 MHz - or the symbol rate
      {
        case 1: ci.SymbolRate = 6000; break;
        case 2: ci.SymbolRate = 7000; break;
        case 3: ci.SymbolRate = 8000; break;
      }
      ci.OriginalNetworkId = transp.OriginalNetworkId;
      ci.TransportStreamId = transp.TransportStreamId;
      ci.Provider = transp.Name;
    }

    var bcastType = svlMapping.GetByte("BroadcastType");
    if (bcastType == 1)
      ReadAnalogData(ci);
    else if (bcastType == 2)
      ReadDvbData(ci);

    return ci;
  }
  #endregion

  #region ReadAnalogData()
  private void ReadAnalogData(ChannelInfo ci)
  {

  }
  #endregion

  #region ReadDvbData()
  private void ReadDvbData(ChannelInfo ci)
  {
    var mask = dvbMapping.GetDword("LinkageMask");
    var tsFlag = dvbMapping.Settings.GetInt("LinkageMask_Ts");

    if ((mask & tsFlag) != 0)
    {
      ci.OriginalNetworkId = dvbMapping.GetWord("Onid");
      ci.TransportStreamId = dvbMapping.GetWord("Tsid");
      ci.ServiceId = dvbMapping.GetWord("Sid");
    }

    if ((ci.SignalSource & (SignalSource.MaskAnalogDigital | SignalSource.MaskAntennaCableSat)) == SignalSource.DvbC)
    {
      ci.OriginalNetworkId = dvbMapping.GetWord("DvbcOnid");
      ci.TransportStreamId = dvbMapping.GetWord("DvbcTsid");
    }

    if ((ci.SignalSource & SignalSource.DvbT) == SignalSource.DvbT)
      ci.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(ci.FreqInMhz).ToString();
    else if ((ci.SignalSource & SignalSource.DvbC) == SignalSource.DvbC)
      ci.ChannelOrTransponder = LookupData.Instance.GetDvbcChannelName(ci.FreqInMhz).ToString();

    ci.ServiceType = dvbMapping.GetByte("ServiceType");
    if (ci.ServiceType != 0)
      ci.ServiceTypeName = LookupData.Instance.GetServiceTypeDescription(ci.ServiceType);

    ci.ShortName = dvbMapping.GetString("ShortName", dvbMapping.Settings.GetInt("ShortNameLength"));
  }
  #endregion

  #region ReadString()
  private string ReadString(DataMapping mapping, string name, int nameLength)
  {
    var str = mapping.GetString(name, nameLength);
    int term = str.IndexOf('\0');
    if (term >= 0)
      str = str.Substring(0, term);
    return str;
  }
  #endregion

  #region LoadFavFile()
  private void LoadFavFile(string filename)
  {
    if (!File.Exists(filename))
      return;
    var content = File.ReadAllBytes(filename);
    var favListSizes = new int[4];
    for (int i = 0; i < 4; i++)
      favListSizes[i] = BitConverter.ToInt32(content, i * 4);

    var recSize = favMapping.Settings.GetInt("RecordSize");
    var dispNumLen = favMapping.Settings.GetInt("DisplayNumberLength");
    favMapping.SetDataPtr(content, 16 - recSize);
    for (int i = 0; i < 4; i++)
    {
      for (int j = 0, c = favListSizes[i] / recSize; j < c; j++)
      {
        favMapping.BaseOffset += recSize;

        //if (favMapping.BaseOffset + recSize >= content.Length) // 
        //  break;

        var tblId = favMapping.GetWord("SvlTableId");
        var recId = favMapping.GetWord("SvlRecordId");

        var list = tblId == 1 ? dvbtChannels : tblId == 2 ? dvbcChannels : tblId == 3 ? dvbsChannels : null;
        if (list == null) // should never happen
          continue;

        var chan = list.GetChannelById(recId);
        if (chan == null) // should never happen
          continue;

        var dispNr = favMapping.GetString("DisplayNumber", dispNumLen);
        var nr = int.Parse(dispNr);
        chan.SetOldPosition(1 + i, nr);
      }
    }
  }
  #endregion

  // Saving ====================================

  #region Save()
  public override void Save()
  {
    this.svlFileContent = GetNewSvlContent();
    var favFileContent = GetNewFavContent();

    File.WriteAllBytes(this.FileName, this.svlFileContent);
    File.WriteAllBytes(this.favFileName, favFileContent);
  }
  #endregion

  #region GetNewSvlContent()
  public byte[] GetNewSvlContent()
  {
    using var mem = new MemoryStream(this.svlFileContent.Length);
    using var writer = new BinaryWriter(mem);
    writer.Write(this.svlFileContent, 0, this.headerRecordSize * 3);

    var maskSkip = svlMapping.Settings.GetInt("NwMask_Skip");
    var maskLock = svlMapping.Settings.GetInt("NwMask_Lock");
    var maskFav1 = svlMapping.Settings.GetInt("NwMask_Fav1");
    var maskFav2 = svlMapping.Settings.GetInt("NwMask_Fav2");
    var maskFav3 = svlMapping.Settings.GetInt("NwMask_Fav3");
    var maskFav4 = svlMapping.Settings.GetInt("NwMask_Fav4");
    var maskClear = ~(maskSkip | maskLock | maskFav1 | maskFav2 | maskFav3 | maskFav4);

    int iList = -1;
    foreach (var list in this.DataRoot.ChannelLists)
    {
      ++iList;
      if (list.IsMixedSourceFavoritesList)
        continue;
      var order = list.Channels.OrderBy(c => c, new DelegateComparer<ChannelInfo>(OrderChannelsComparer)).ToList();
      int newId = 1;
      foreach (var channel in order)
      {
        // copy original data
        var offset = writer.BaseStream.Position;
        writer.Write(this.svlFileContent, channel.RawDataOffset, svlRecordSize);
        writer.Flush();
        
        // prepare to overwrite with some new values
        svlMapping.SetDataPtr(mem.GetBuffer(), (int)offset);
        svlMapping.SetWord("RecordId", newId);

        int val = svlMapping.GetWord("ChannelId");
        val = (val & 0x03) | (channel.NewProgramNr << 2);
        svlMapping.SetWord("ChannelId", val);

        var nwMask = (int)svlMapping.GetDword("NwMask") & maskClear;
        nwMask |= channel.Skip ? 0 : maskSkip; // reverse meaning
        nwMask |= channel.Lock ? maskLock : 0;
        nwMask |= (channel.Favorites & Favorites.A) != 0 ? maskFav1 : 0;
        nwMask |= (channel.Favorites & Favorites.B) != 0 ? maskFav2 : 0;
        nwMask |= (channel.Favorites & Favorites.C) != 0 ? maskFav3 : 0;
        nwMask |= (channel.Favorites & Favorites.D) != 0 ? maskFav4 : 0;
        svlMapping.SetDword("NwMask", nwMask);

        channel.RecordIndex = newId++;
      }

      // update data block size in header
      headerMapping.SetDataPtr(mem.GetBuffer(), iList * this.headerRecordSize);
      headerMapping.SetDword("BlockSize", order.Count * svlRecordSize);
    }

    var buffer = new byte[mem.Length];
    Tools.MemCopy(mem.GetBuffer(), 0, buffer, 0, (int)mem.Length);
    return buffer;
  }
  #endregion

  #region OrderChannelsComparer()
  private int OrderChannelsComparer(ChannelInfo a, ChannelInfo b)
  {
    // TV before radio before data
    var v1 = a.SignalSource & SignalSource.MaskTvRadioData;
    var v2 = b.SignalSource & SignalSource.MaskTvRadioData;
    var c = v1.CompareTo(v2);
    if (c != 0)
      return c;

    // deleted channels to the end
    if (a.NewProgramNr < 0)
      return b.NewProgramNr == 0 ? a.RecordOrder.CompareTo(b.RecordOrder) : +1;
    if (b.NewProgramNr < 0)
      return -1;

    return a.NewProgramNr.CompareTo(b.NewProgramNr);
  }
  #endregion

  #region GetNewFavContent()
  private byte[] GetNewFavContent()
  {
    using var mem = new MemoryStream();
    using var writer = new BinaryWriter(mem);

    for (int i=0; i<4; i++)
      writer.Write(0);

    var favRecordSize = favMapping.Settings.GetInt("RecordSize");
    var tmp = new byte[favRecordSize];
    favMapping.SetDataPtr(tmp, 0);

    var nameLength = favMapping.Settings.GetInt("ChannelNameLength");
    var dispNumLength = favMapping.Settings.GetInt("DisplayNumberLength");

    for (int i = 1; i <= 4; i++)
    {
      var order = this.favChannels.Channels.Where(ch => ch.GetPosition(i) >= 0).OrderBy(ch => ch.GetPosition(i)).ToList();

      foreach (var channel in order)
      {
        tmp.MemSet(0, 0,  tmp.Length);
        var mask = channel.SignalSource & SignalSource.MaskTvRadioData;
        var tblId = (mask & SignalSource.Antenna) != 0 ? 1 : (mask & SignalSource.Cable) != 0 ? 2 : 3;
        favMapping.SetWord("SvlTableId", tblId);
        favMapping.SetWord("SvlRecordId", (int)channel.RecordIndex);
        favMapping.SetString("DisplayNumber", channel.GetPosition(i).ToString(), dispNumLength);
        favMapping.SetString("ChannelName", channel.Name, nameLength);
        writer.Write(tmp);
      }

      // update header
      writer.Flush();
      var off = mem.Position;
      mem.Seek((i-1) * 4, SeekOrigin.Begin);
      writer.Write(order.Count * favRecordSize);
      writer.Flush();
      mem.Seek(off, SeekOrigin.Begin);
    }

    tmp = new byte[mem.Length];
    Tools.MemCopy(mem.GetBuffer(), 0, tmp, 0, tmp.Length);
    return tmp;
  }
  #endregion

  // Infrastructure ============================

  #region GetDataFilePaths()
  /// <summary>
  /// Files that need backup
  /// </summary>
  /// <returns></returns>
  public override IEnumerable<string> GetDataFilePaths()
  {
    return new[] { this.FileName, this.favFileName };
  }
  #endregion

  #region DefaultEncoding
  public override Encoding DefaultEncoding
  {
    get => base.DefaultEncoding;
    set
    {
      if (value == this.DefaultEncoding)
        return;
      base.DefaultEncoding = value;
      this.svlMapping.DefaultEncoding = value;
      this.tslMapping.DefaultEncoding = value;
      this.dvbMapping.DefaultEncoding = value;
      this.ReparseNames();
    }
  }
  #endregion

  #region ReparseNames()
  private void ReparseNames()
  {
    var nameLength = svlMapping.Settings.GetInt("NameLength");
    var shortNameLength = dvbMapping.Settings.GetInt("ShortNameLength");
    var dvbOffset = svlMapping.Settings.GetInt("BroadcastSystemData");

    foreach (var list in this.DataRoot.ChannelLists)
    {
      if (list.IsMixedSourceFavoritesList)
        continue;
      foreach (var chan in list.Channels)
      {
        svlMapping.BaseOffset = chan.RawDataOffset;
        chan.Name = ReadString(svlMapping, "Name", nameLength);

        if ((chan.SignalSource & SignalSource.Digital) == 0)
          continue;

        dvbMapping.BaseOffset = chan.RawDataOffset + dvbOffset;
        chan.ShortName = ReadString(dvbMapping, "ShortName", shortNameLength);
      }
    }
  }
  #endregion
}
