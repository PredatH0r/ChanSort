using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.CmdbBin
{
  public class CmdbFileSerializer : SerializerBase
  {
    private IniFile ini;
    private readonly MappingPool<DataMapping> satMappings = new MappingPool<DataMapping>("dtv_cmdb_2.bin");
    private readonly ChannelList dvbsTv = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Sat TV");
    private readonly ChannelList dvbsRadio = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat Radio");
    private readonly ChannelList dvbsData = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat Data");
    private DvbStringDecoder dvbStringDecoder;
    private bool loaded = false;

    public CmdbFileSerializer(string inputFile) : base(inputFile)
    {
      this.DataRoot.AddChannelList(dvbsTv);
      this.DataRoot.AddChannelList(dvbsRadio);
      // this.DataRoot.AddChannelList(dvbsData); // there seem to be multiple data lists with Toshiba TVs which all have their own numbering starting at 1. Better don't show data channels at all than dupes
      this.ReadConfigurationFromIniFile();
    }

    #region ReadConfigurationFromIniFile()

    private void ReadConfigurationFromIniFile()
    {
      string iniFile = this.GetType().Assembly.Location.ToLowerInvariant().Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);

      foreach (var section in ini.Sections)
      {
        int idx = section.Name.IndexOf(":");
        if (idx < 0)
          continue;
        string recordLength = idx < 0 ? "" : section.Name.Substring(idx + 1);
        if (section.Name.StartsWith("dtv_cmdb_2"))
          satMappings.AddMapping(recordLength, new DataMapping(section));
      }
    }
    #endregion

    #region Load()
    public override void Load()
    {
      this.dvbStringDecoder = new DvbStringDecoder(this.DefaultEncoding);

      foreach (var file in Directory.GetFiles(Path.GetDirectoryName(this.FileName)))
      {
        var lower = Path.GetFileName(file).ToLowerInvariant();
        var size = (int)new FileInfo(file).Length;
        switch (lower)
        {
          case "dtv_cmdb_2.bin":
            LoadFile(file, this.dvbsTv, this.dvbsRadio, this.dvbsData, this.satMappings.GetMapping(size));
            break;
        }
      }

      if (!this.loaded)
        throw new FileLoadException("\"" + this.FileName + "\" does not belong to a dtv_cmdb_* file system");
    }
    #endregion

    #region LoadFile()
    private void LoadFile(string file, ChannelList tvList, ChannelList radioList, ChannelList dataList, DataMapping fileMapping)
    {
      var data = File.ReadAllBytes(file);
      var sec = fileMapping.Settings;

      LoadBitmappedRecords(data, sec, "Satellite", ReadSatellite);
      LoadBitmappedRecords(data, sec, "Transponder", ReadTransponder);
      LoadBitmappedRecords(data, sec, "Channel", (map, index) => ReadChannel(map, tvList, radioList, dvbsData, index));

      this.loaded = true;
    }
    #endregion

    #region LoadBitmappedRecords()
    private void LoadBitmappedRecords(byte[] data, IniFile.Section sec, string recordType, Action<DataMapping, int> readRecord)
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
            readRecord(map, index);
          map.BaseOffset += lenRecord;
          if (++index >= count)
            break;
        }
      }
    }
    #endregion

    private void ReadSatellite(DataMapping map, int index)
    {
      var sat = new Satellite(index);
      sat.Name = map.GetString("offName", map.Settings.GetInt("lenName"));
      this.DataRoot.AddSatellite(sat);
    }

    private void ReadTransponder(DataMapping map, int index)
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

    #region ReadChannel()
    private void ReadChannel(DataMapping chanMap, ChannelList tvList, ChannelList radioList, ChannelList dataList, int recordIndex)
    {
      var channelType = (int)chanMap.GetByte("offChannelType");
      if (channelType == 0) // some file format versions store the channel type in the upper nibble of a byte
        channelType = chanMap.GetByte("offChannelTypeOld") >> 4;
      var serviceType = chanMap.GetByte("offServiceType");
      var apid = chanMap.GetWord("offAudioPid") & 0x1FFF;
      var vpid = chanMap.GetWord("offVideoPid") & 0x1FFF;

      ChannelList list;
      if (channelType != 0)
        list = channelType == 1 ? tvList : channelType == 2 ? radioList : dataList;
      else if (serviceType != 0)
      {
        var type = LookupData.Instance.IsRadioTvOrData(serviceType);
        list = type == SignalSource.Radio ? radioList : type == SignalSource.Tv ? tvList : dataList;
      }
      else
      {
        //list = vpid != 0 && vpid != 0x1FFF ? tvList : apid != 0 && apid != 0x1FFF ? radioList : dataList;
        list = tvList;
      }

      var progNr = (int)chanMap.GetWord("offProgramNr");
      if (progNr == 0xFFFE)
        progNr = -2;

      var ch = new ChannelInfo(list.SignalSource, recordIndex, progNr, "");
      ch.ServiceType = serviceType;
      ch.ServiceTypeName = Api.LookupData.Instance.GetServiceTypeDescription(ch.ServiceType);
      ch.PcrPid = chanMap.GetWord("offPcrPid") & 0x1FFF;
      ch.ServiceId = chanMap.GetWord("offServiceId");
      ch.AudioPid = apid;
      ch.VideoPid = vpid;

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
    }
    #endregion

    public override void Save(string tvOutputFile)
    {
      throw new NotImplementedException();
    }
  }
}
