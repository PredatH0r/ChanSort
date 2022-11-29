using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.CmdbBin
{
  public class CmdbFileSerializer : SerializerBase
  {
    private IniFile ini;
    private readonly List<string> files = new();
    private readonly ChannelList avbtTv = new (SignalSource.AnalogT | SignalSource.Tv, "Analog Antenna TV");
    private readonly ChannelList avbcTv = new (SignalSource.AnalogC | SignalSource.Tv, "Analog Cable TV");
    private readonly ChannelList dvbsTv = new (SignalSource.DvbS | SignalSource.Tv, "Sat TV");
    private readonly ChannelList dvbsRadio = new (SignalSource.DvbS | SignalSource.Radio, "Sat Radio");
    private readonly ChannelList dvbsData = new (SignalSource.DvbS | SignalSource.Radio, "Sat Data");
    private DvbStringDecoder dvbStringDecoder;
    private bool loaded;
    private readonly StringBuilder protocol = new ();

    public CmdbFileSerializer(string inputFile) : base(inputFile)
    {
      this.Features.FavoritesMode = FavoritesMode.Flags;
      this.Features.MaxFavoriteLists = 1;
      this.Features.DeleteMode = DeleteMode.FlagWithoutPrNr; // TODO there can be lots of channels in each list with number 65534, which seems to indicate user-deleted
      this.Features.ChannelNameEdit = ChannelNameEditMode.Analog;

      this.DataRoot.AddChannelList(avbtTv);
      this.DataRoot.AddChannelList(avbcTv);
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
          case "atv_cmdb.bin":
            LoadFile(file, this.avbtTv, null, null);
            break;
          case "atv_cmdb_cable.bin":
            LoadFile(file, this.avbcTv, null, null);
            break;
        }
      }

      if (!this.loaded)
        throw LoaderException.Fail("\"" + this.FileName + "\" does not belong to a supported dtv_cmdb_* file system");
    }
    #endregion

    #region LoadFile()
    private void LoadFile(string file, ChannelList tvList, ChannelList radioList, ChannelList dataList)
    {
      var data = File.ReadAllBytes(file);
      var fileName = Path.GetFileName(file).ToLowerInvariant();
      var secId = $"{fileName}:{data.Length}";
      var sec = this.ini.GetSection(secId);
      if (sec == null)
      {
        protocol.AppendLine("Skipped file with unknown data size: " + secId);
        return;
      }

      if ((tvList.SignalSource & SignalSource.Analog) != 0)
      {
        var seq = LoadAnalogProgramNumbers(data, sec);
        LoadBitmappedRecords(data, sec, "avb", "Channel", (map, index, len) => ReadAnalogChannel(map, tvList, seq, index, len));
      }
      else
      {
        LoadBitmappedRecords(data, sec, "dvbs", "Satellite", ReadSatellite);
        LoadBitmappedRecords(data, sec, "dvbs", "Transponder", ReadTransponder);
        LoadBitmappedRecords(data, sec, "dvbs", "Channel", (map, index, len) => ReadDigitalChannel(map, tvList, radioList, dataList, index, len));
      }

      this.loaded = true;
      this.files.Add(file);
    }
    #endregion

    #region LoadAnalogProgramNumbers()
    private byte[] LoadAnalogProgramNumbers(byte[] data, IniFile.Section sec)
    {
      var off = sec.GetInt("offProgNrList");
      var len = sec.GetInt("lenProgNrList");
      var bytes = new byte[len];
      Array.Copy(data, off, bytes, 0, len);
      return bytes;
    }

    #endregion

    #region LoadBitmappedRecords()
    private void LoadBitmappedRecords(byte[] data, IniFile.Section sec, string recordSectionPrefix, string recordType, Action<DataMapping, int, int> readRecord)
    {
      var lenRecord = sec.GetInt($"len{recordType}Record");
      var map = new DataMapping(this.ini.GetSection($"{recordSectionPrefix}{recordType}:{lenRecord}"));
      map.DefaultEncoding = this.DefaultEncoding;
      map.SetDataPtr(data, sec.GetInt($"off{recordType}Record"));

      var off = sec.GetInt($"off{recordType}Bitmap");
      var len = sec.GetInt($"len{recordType}Bitmap");
      var count = sec.GetInt($"num{recordType}Record", short.MaxValue);
      int index = 0;
      for (int i = 0; i < len; i++)
      {
        var b = data[off + i];
        for (byte mask = 1; mask != 0; mask <<= 1)
        {
          if ((b & mask) != 0)
            readRecord(map, index, lenRecord);
          map.BaseOffset += lenRecord;
          ++index;
          if (index >= count)
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
    
    #region ReadAnalogChannel
    private void ReadAnalogChannel(DataMapping chanMap, ChannelList list, byte[] progNrList, int recordIndex, int recordLength)
    {
      var channelNameLength = chanMap.Settings.GetInt("lenName");
      var name = chanMap.GetString("offName", channelNameLength).TrimEnd(new[] { '\0', ' ' });
      var progNr = Array.IndexOf(progNrList, (byte)recordIndex) + 1;
      var ch = new ChannelInfo(list.SignalSource, recordIndex, progNr, name);
      ch.FreqInMhz = (decimal)chanMap.GetWord("offFrequency") * 50 / 1000;
      //if ((list.SignalSource & SignalSource.Cable) != 0)
      //  ch.ChannelOrTransponder = LookupData.Instance.GetDvbcChannelName(ch.FreqInMhz + ((list.SignalSource & SignalSource.Analog) != 0 ? 2.75m : 0)); // 
      this.DataRoot.AddChannel(list, ch);
    }
    #endregion

    #region ReadDigitalChannel()
    private void ReadDigitalChannel(DataMapping chanMap, ChannelList tvList, ChannelList radioList, ChannelList dataList, int recordIndex, int recordLength)
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

    public override void Save()
    {
      foreach (var path in this.files)
      {
        var name = Path.GetFileName(path).ToLowerInvariant();
        switch (name)
        {
          case "dtv_cmdb_2.bin":
            SaveDtvCmdb(path, "dvbsChannel", SignalSource.DvbS);
            break;
          case "atv_cmdb.bin":
            SaveAtvCmdb(path, "avbChannel", this.avbtTv);
            break;
          case "atv_cmdb_cable.bin":
            SaveAtvCmdb(path, "avbChannel", this.avbcTv);
            break;
        }
      }
    }
    #endregion

    #region SaveDtvCmdb()
    private void SaveDtvCmdb(string path, string channelSectionName, SignalSource sourceMask)
    {
      var data = File.ReadAllBytes(path);
      var name = Path.GetFileName(path).ToLowerInvariant();
      var config = this.ini.GetSection(name + ":" + data.Length);
      var lenChannelRecord = config.GetInt("lenChannelRecord");
      var sec = this.ini.GetSection($"{channelSectionName}:{lenChannelRecord}");
      sec.Set("offChecksum", lenChannelRecord - 4);
      var mapping = new DataMapping(sec);
      
      var baseOffset = config.GetInt("offChannelRecord");

      foreach (var list in this.DataRoot.ChannelLists)
      {
        if ((list.SignalSource & (SignalSource.MaskAnalogDigital | SignalSource.MaskAntennaCableSat)) != sourceMask)
          continue;
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

      File.WriteAllBytes(path, data);
    }
    #endregion

    #region SaveAtvCmdb()
    private void SaveAtvCmdb(string path, string channelSectionName, ChannelList list)
    {
      var data = File.ReadAllBytes(path);
      var name = Path.GetFileName(path).ToLowerInvariant();
      var config = this.ini.GetSection(name + ":" + data.Length);
      var lenChannelRecord = config.GetInt("lenChannelRecord");

      var offProgNrList = config.GetInt("offProgNrList");
      var lenProgNrList = config.GetInt("lenProgNrList");
      

      var sec = this.ini.GetSection($"{channelSectionName}:{lenChannelRecord}");
      var mapping = new DataMapping(sec);
      var offChannelBitmap = config.GetInt("offChannelBitmap");
      var offChannelRecord = config.GetInt("offChannelRecord");

      var maxNameLen = mapping.Settings.GetInt("lenName");

      data.MemSet(offProgNrList, 0xFD, lenProgNrList);

      foreach (var chan in list.Channels)
      {
        if (chan.NewProgramNr > 0 && chan.NewProgramNr < lenProgNrList)
          data[offProgNrList + chan.NewProgramNr - 1] = (byte)chan.RecordIndex;

        if (chan.IsNameModified)
        {
          mapping.SetDataPtr(data, offChannelRecord + (int)chan.RecordIndex * lenChannelRecord);
          mapping.SetString("offName", chan.Name, maxNameLen);
        }

        if (chan.IsDeleted)
        {
          var idx = (int)chan.RecordIndex;
          data[offChannelBitmap + idx / 8] &= (byte) ~(1 << (idx & 0x07));
        }
      }

      File.WriteAllBytes(path, data);
    }
    #endregion

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      return base.GetFileInformation() + "\n\n" + protocol;
    }
    #endregion

    public override IEnumerable<string> GetDataFilePaths() => this.files.ToList(); // these files will be backed up / restored
  }
}
