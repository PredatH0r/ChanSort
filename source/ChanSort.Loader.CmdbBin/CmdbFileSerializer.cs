using System;
using System.CodeDom;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.CmdbBin
{
  public class CmdbFileSerializer : SerializerBase
  {
    private IniFile ini;
    private readonly ChannelList dvbsTv = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Sat TV");
    private readonly ChannelList dvbsRadio = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat Radio");
    private readonly ChannelList dvbsData = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat Data");
    private DvbStringDecoder dvbStringDecoder;
    private bool loaded;
    private readonly StringBuilder protocol = new StringBuilder();

    public CmdbFileSerializer(string inputFile) : base(inputFile)
    {
      this.Features.FavoritesMode = FavoritesMode.Flags;
      this.Features.MaxFavoriteLists = 1;
      this.Features.DeleteMode = DeleteMode.FlagWithoutPrNr; // TODO there can be lots of channels in each list with number 65534, which seems to indicate user-deleted

      this.DataRoot.AddChannelList(dvbsTv);
      this.DataRoot.AddChannelList(dvbsRadio);
      // this.DataRoot.AddChannelList(dvbsData); // there seem to be multiple data lists with Toshiba TVs which all have their own numbering starting at 1. Better don't show data channels at all than dupes
      this.ReadConfigurationFromIniFile();

      foreach (var list in this.DataRoot.ChannelLists)
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Hidden));
    }

    #region ReadConfigurationFromIniFile()
    private void ReadConfigurationFromIniFile()
    {
      string iniFile = this.GetType().Assembly.Location.ToLowerInvariant().Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      this.dvbStringDecoder = new DvbStringDecoder(this.DefaultEncoding);

      foreach (var file in Directory.GetFiles(Path.GetDirectoryName(this.FileName) ?? "."))
      {
        var lower = Path.GetFileName(file).ToLowerInvariant();
        switch (lower)
        {
          case "dtv_cmdb_2.bin":
            LoadFile(file, this.dvbsTv, this.dvbsRadio, this.dvbsData);
            break;
        }
      }

      if (!this.loaded)
        throw new FileLoadException("\"" + this.FileName + "\" does not belong to a dtv_cmdb_* file system");
    }
    #endregion

    #region LoadFile()
    private void LoadFile(string file, ChannelList tvList, ChannelList radioList, ChannelList dataList)
    {
      var data = File.ReadAllBytes(file);
      var fileName = Path.GetFileName(file).ToLowerInvariant();
      var sec = this.ini.GetSection($"{fileName}:{data.Length}");

      LoadBitmappedRecords(data, sec, "Satellite", ReadSatellite);
      LoadBitmappedRecords(data, sec, "Transponder", ReadTransponder);
      LoadBitmappedRecords(data, sec, "Channel", (map, index, len) => ReadChannel(map, tvList, radioList, dataList, index, len));

      this.loaded = true;
    }
    #endregion

    #region LoadBitmappedRecords()
    private void LoadBitmappedRecords(byte[] data, IniFile.Section sec, string recordType, Action<DataMapping, int, int> readRecord)
    {
      var lenRecord = sec.GetInt($"len{recordType}Record");
      var map = new DataMapping(this.ini.GetSection($"dvbs{recordType}:{lenRecord}"));
      map.DefaultEncoding = this.DefaultEncoding;
      map.SetDataPtr(data, sec.GetInt($"off{recordType}Record"));

      var off = sec.GetInt($"off{recordType}Bitmap");
      var len = sec.GetInt($"len{recordType}Bitmap");
      var count = sec.GetInt($"num{recordType}Record");
      int index = 0;
      for (int i = 0; i < len; i++)
      {
        var b = data[off + i];
        for (byte mask = 1; mask != 0; mask <<= 1)
        {
          if ((b & mask) != 0)
            readRecord(map, index, lenRecord);
          map.BaseOffset += lenRecord;
          if (++index >= count)
            break;
        }
      }
    }
    #endregion

    #region ReadSatellite()
    private void ReadSatellite(DataMapping map, int index, int lenRecord)
    {
      var sat = new Satellite(index);
      sat.Name = map.GetString("offName", map.Settings.GetInt("lenName"));
      this.DataRoot.AddSatellite(sat);
    }
    #endregion
    
    #region ReadTransponder()
    private void ReadTransponder(DataMapping map, int index, int lenRecord)
    {
      //var idx = map.GetWord("offTransponderIndex"); // seems to be some logical number, skipping a new numbers here and there

      var tp = new Transponder(index);
      var satIndex = map.GetWord("offSatelliteIndex");
      tp.Satellite = this.DataRoot.Satellites.TryGet(satIndex);
      tp.OriginalNetworkId = map.GetWord("offOriginalNetworkId");
      tp.TransportStreamId = map.GetWord("offTransportStreamId");
      tp.FrequencyInMhz = map.GetDword("offFreqInMhz");
      tp.SymbolRate = map.GetWord("offSymbolRate");
      this.DataRoot.AddTransponder(tp.Satellite, tp);
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(DataMapping chanMap, ChannelList tvList, ChannelList radioList, ChannelList dataList, int recordIndex, int recordLength)
    {
      var channelType = (int)chanMap.GetByte("offChannelType");
      if (channelType == 0) // some file format versions store the channel type in the upper nibble of a byte
        channelType = chanMap.GetByte("offChannelTypeOld") >> 4;
      var serviceType = chanMap.GetByte("offServiceType");

      if (chanMap.Settings.GetInt("offFav", -1) < 0)
        this.Features.FavoritesMode = FavoritesMode.None;

      ChannelList list;
      if (channelType != 0)
        list = channelType == 1 ? tvList : channelType == 2 ? radioList : dataList;
      else if (serviceType != 0)
      {
        var type = LookupData.Instance.IsRadioTvOrData(serviceType);
        list = type == SignalSource.Radio ? radioList : type == SignalSource.Tv ? tvList : dataList;
      }
      else
        list = tvList;

      var progNr = (int)chanMap.GetWord("offProgramNr");
      if (progNr == 0xFFFE)
        progNr = -2;

      var ch = new ChannelInfo(list.SignalSource, recordIndex, progNr, "");
      ch.ServiceType = serviceType;
      ch.ServiceTypeName = LookupData.Instance.GetServiceTypeDescription(ch.ServiceType);
      ch.ServiceId = chanMap.GetWord("offServiceId");
      ch.PcrPid = chanMap.GetWord("offPcrPid") & 0x1FFF;
      ch.AudioPid = chanMap.GetWord("offAudioPid") & 0x1FFF;
      ch.VideoPid = chanMap.GetWord("offVideoPid") & 0x1FFF;
      ch.Encrypted = chanMap.GetFlag("Encrypted", false);
      ch.Skip = chanMap.GetFlag("Skip", false);
      ch.Lock = chanMap.GetFlag("Locked", false);
      ch.Favorites = chanMap.GetFlag("Fav", false) ? Favorites.A : 0;

      var off = chanMap.BaseOffset + chanMap.GetOffsets("offName")[0];
      this.dvbStringDecoder.GetChannelNames(chanMap.Data, off, chanMap.Settings.GetInt("lenName"), out var longName, out var shortName);
      ch.Name = longName;
      ch.ShortName = shortName;

      var offProv = chanMap.GetOffsets("offProvider");
      if (offProv.Length > 0)
      {
        off = chanMap.BaseOffset + offProv[0];
        this.dvbStringDecoder.GetChannelNames(chanMap.Data, off, chanMap.Settings.GetInt("lenName"), out longName, out _);
        ch.Provider = longName;
      }

      var offDebug = chanMap.Settings.GetInt("offDebug");
      var lenDebug = chanMap.Settings.GetInt("lenDebug");
      ch.AddDebug(chanMap.Data, chanMap.BaseOffset + offDebug, lenDebug);

      var transponderIndex = chanMap.GetWord("offTransponderIndex");
      var tp = this.DataRoot.Transponder.TryGet(transponderIndex);
      if (tp != null)
      {
        ch.Transponder = tp;
        ch.OriginalNetworkId = tp.OriginalNetworkId;
        ch.TransportStreamId = tp.TransportStreamId;
        ch.FreqInMhz = tp.FrequencyInMhz;
        ch.SymbolRate = tp.SymbolRate;
        ch.Satellite = tp.Satellite?.Name;
      }
      
      this.DataRoot.AddChannel(list, ch);

      
      // validate checksum
      var calculated = CalcChecksum(chanMap.Data, chanMap.BaseOffset, recordLength - 4);
      var expected = BitConverter.ToInt32(chanMap.Data, chanMap.BaseOffset + recordLength - 4);
      if (calculated != expected)
        this.protocol.AppendFormat($"Data record has invalid checksum. Expected: {expected}, calculated: {calculated}\r\n");
    }
    #endregion

    #region CalcChecksum()
    private int CalcChecksum(byte[] data, int offset, int length)
    {
      int sum = 0;
      for (int i = 0; i < length; i++)
        sum += data[offset++];
      return sum;
    }
    #endregion


    #region Save()
    public override void Save(string tvOutputFile)
    {
      // save-as is not supported

      // TODO: currently hardcoded to support only dtv_cmdb_2.bin

      var data = File.ReadAllBytes(this.FileName); // filename is currently always the dtv_cmdb_2.bin, even if the user selected another file
      var config = this.ini.GetSection("dtv_cmdb_2.bin:" + data.Length);
      var lenChannelRecord = config.GetInt("lenChannelRecord");
      var sec = this.ini.GetSection($"dvbsChannel:{lenChannelRecord}");
      sec.Set("offChecksum", lenChannelRecord - 4);
      var mapping = new DataMapping(sec);
      
      var baseOffset = config.GetInt("offChannelRecord");

      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (var chan in list.Channels)
        {
          mapping.SetDataPtr(data, baseOffset + (int)chan.RecordIndex * lenChannelRecord);
          mapping.SetWord("offProgramNr", chan.IsDeleted ? 0xFFFE : chan.NewProgramNr);
          if (chan.IsDeleted) // undo the automatic number changes from the "File / Save" function
          {
            chan.NewProgramNr = -2;
            chan.IsDeleted = false;
          }

          mapping.SetFlag("Skip", chan.Skip);
          mapping.SetFlag("Lock", chan.Lock);
          mapping.SetFlag("Fav", chan.Favorites != 0);
          var sum = CalcChecksum(data, mapping.BaseOffset, lenChannelRecord - 4);
          mapping.SetDword("offChecksum", sum);
        }
      }

      File.WriteAllBytes(this.FileName, data);
    }
    #endregion


    #region GetFileInformation()
    public override string GetFileInformation()
    {
      return base.GetFileInformation() + "\n\n" + protocol;
    }
    #endregion
  }
}
