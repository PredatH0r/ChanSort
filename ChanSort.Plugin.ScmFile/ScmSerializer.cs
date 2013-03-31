using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ChanSort.Api;
using ICSharpCode.SharpZipLib.Zip;

namespace ChanSort.Plugin.ScmFile
{
  class ScmSerializer : SerializerBase
  {
    private readonly Dictionary<string, ModelConstants> modelConstants = new Dictionary<string, ModelConstants>();
    private readonly MappingPool<AnalogChannelDataMapping> analogMappings = new MappingPool<AnalogChannelDataMapping>("Analog");
    private readonly MappingPool<DvbCtChannelDataMapping> dvbctMappings = new MappingPool<DvbCtChannelDataMapping>("DVB-C/T");
    private readonly MappingPool<DvbSChannelDataMapping> dvbsMappings = new MappingPool<DvbSChannelDataMapping>("DVB-S");
    private readonly MappingPool<DvbSChannelDataMapping> hdplusMappings = new MappingPool<DvbSChannelDataMapping>("AstraHDPlus");
    private readonly MappingPool<DataMapping> analogFineTuneMappings = new MappingPool<DataMapping>("FineTune");
    private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT, SignalType.Mixed);
    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC, SignalType.Mixed);
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC, SignalType.Mixed);
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, SignalType.Mixed);
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS, SignalType.Mixed);
    private readonly ChannelList hdplusChannels = new ChannelList(SignalSource.HdPlusD, SignalType.Mixed);
    private readonly Dictionary<int, decimal> avbtFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> avbcFrequency = new Dictionary<int, decimal>();
    private byte[] avbtFileContent;
    private byte[] avbcFileContent;
    private byte[] dvbtFileContent;
    private byte[] dvbcFileContent;
    private byte[] dvbsFileContent;
    private byte[] hdplusFileContent;
    private UnsortedChannelMode unsortedChannelMode;
    private ModelConstants c;

    #region ctor()
    public ScmSerializer(string inputFile) : base(inputFile)
    {
      this.ReadConfigurationFromIniFile();
    }
    #endregion

    #region DisplayName
    public override string DisplayName { get { return "Samsung *.scm Loader"; } }
    #endregion

    #region ReadConfigurationFromIniFile()
    private void ReadConfigurationFromIniFile()
    {
      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      IniFile ini = new IniFile(iniFile);
      foreach (var section in ini.Sections)
      {
        int idx = section.Name.IndexOf(":");
        int len=0;
        if (idx >= 0)
          int.TryParse(section.Name.Substring(idx + 1), out len);

        if (section.Name.StartsWith("Series:"))
          modelConstants.Add(section.Name, new ModelConstants(section));
        else if (section.Name.StartsWith("Analog:"))
          analogMappings.AddMapping(new AnalogChannelDataMapping(section, len));
        else if (section.Name.StartsWith("DvbCT:"))
          dvbctMappings.AddMapping(new DvbCtChannelDataMapping(section, len));
        else if (section.Name.StartsWith("DvbS:"))
          dvbsMappings.AddMapping(new DvbSChannelDataMapping(section, len));
        else if (section.Name.StartsWith("FineTune:"))
          analogFineTuneMappings.AddMapping(new DataMapping(section, len, null));
        else if (section.Name.StartsWith("AstraHDPlusD:"))
          hdplusMappings.AddMapping(new DvbSChannelDataMapping(section, len));
      }
    }
    #endregion

    #region Load()
    public override void Load()
    {
      using (ZipFile zip = new ZipFile(this.FileName))
      {
        DetectModelConstants(zip);
        DataRoot.SupportedFavorites = c.supportedFavorites;

        ReadAnalogFineTuning(zip);
        ReadAnalogChannels(zip, "map-AirA", this.avbtChannels, out this.avbtFileContent, this.avbtFrequency);
        ReadAnalogChannels(zip, "map-CableA", this.avbcChannels, out this.avbcFileContent, this.avbcFrequency);
        ReadDvbctChannels(zip, "map-AirD", this.dvbtChannels, out this.dvbtFileContent);
        ReadDvbctChannels(zip, "map-CableD", this.dvbcChannels, out this.dvbcFileContent);
        ReadSatellites(zip);
        ReadTransponder(zip, "TransponderDataBase.dat");
        ReadTransponder(zip, "UserTransponderDataBase.dat");
        ReadDvbsChannels(zip);
        ReadAstraHdPlusChannels(zip);
      }
    }
    #endregion

    #region DetectModelConstants()
    private void DetectModelConstants(ZipFile zip)
    {
      if (DetectModelFromCloneInfoFile(zip)) return;
      if (DetectModelFromContentFileLengths(zip)) return;
      if (DetectModelFromFileName()) return;
      throw new IOException("Unable to determine TV model from file content or name");
    }
    #endregion

    #region DetectModelFromFileName()
    private bool DetectModelFromFileName()
    {
      string file = Path.GetFileName(this.FileName);
      System.Text.RegularExpressions.Regex regex =
        new System.Text.RegularExpressions.Regex("channel_list_[A-Z]{2}[0-9]{2}([A-Z])[0-9A-Z]+_[0-9]+\\.scm");
      var match = regex.Match(file);
      if (match.Success)
      {
        string series = match.Groups[1].Value;
        if (this.modelConstants.TryGetValue("Series:" + series, out this.c))
          return true;
      }
      return false;
    }
    #endregion

    #region DetectModelFromCloneInfoFile()
    private bool DetectModelFromCloneInfoFile(ZipFile zip)
    {
      byte[] cloneInfo = ReadFileContent(zip, "CloneInfo");
      if (cloneInfo == null)
      {
        this.c = this.modelConstants["Series:B"];
        return true;
      }

      if (cloneInfo.Length >= 9)
      {
        char series = (char) cloneInfo[8];
        if (this.modelConstants.TryGetValue("Series:" + series, out this.c))
          return true;
      }
      return false;
    }
    #endregion

    #region DetectModelFromContentFileLengths()
    private bool DetectModelFromContentFileLengths(ZipFile zip)
    {
      string[] candidates = new[]
                              {
                                DetectModelFromAirAOrCableA(zip),
                                DetectModelFromAirDOrCableD(zip),
                                DetectModelFromSateD(zip),
                                DetectModelFromTranspoderDatabase(zip)
                              };

      string validCandidates = "BCDE";
      foreach (var candidateList in candidates)
      {
        if (candidateList == null)
          continue;
        string newValidCandidats = "";
        foreach (var candidate in candidateList)
        {
          if (validCandidates.Contains(candidate))
            newValidCandidats += candidate;
        }
        validCandidates = newValidCandidats;
      }

      if (validCandidates.Length != 1)
        return false;

      this.modelConstants.TryGetValue("Series:" + validCandidates, out this.c);
      return true;
    }

    #endregion

    #region DetectModelFromAirAOrCableA()
    private string DetectModelFromAirAOrCableA(ZipFile zip)
    {
      var entry = zip.GetEntry("map-AirA") ?? zip.GetEntry("map-CableA");
      if (entry == null)
        return null;

      var candidates = "";
      if (entry.Size % 28000 == 0)
        candidates += "B";
      if (entry.Size % 40000 == 0)
        candidates += "C";
      if (entry.Size % 64000 == 0)
        candidates += "DE";
      return candidates;
    }
    #endregion

    #region DetectModelFromAirDOrCableD()
    private string DetectModelFromAirDOrCableD(ZipFile zip)
    {
      var entry = zip.GetEntry("map-AirD") ?? zip.GetEntry("map-CableD");
      if (entry == null)
        return null;

      var candidates = "";
      if (entry.Size % 248 == 0)
        candidates += "B";
      if (entry.Size % 292 == 0)
        candidates += "C";
      if (entry.Size % 320 == 0)
        candidates += "DE";
      return candidates;
    }
    #endregion

    #region DetectModelFromSateD()
    private string DetectModelFromSateD(ZipFile zip)
    {
      var entry = zip.GetEntry("map-SateD");
      if (entry == null)
        return null;

      var candidates = "";
      if (entry.Size % 144 == 0)
        candidates += "BC";
      if (entry.Size % 172 == 0)
        candidates += "D";
      if (entry.Size % 168 == 0)
        candidates += "E";
      return candidates;
    }
    #endregion

    #region DetectModelFromTranspoderDatabase()
    private string DetectModelFromTranspoderDatabase(ZipFile zip)
    {
      var entry = zip.GetEntry("TransponderDatabase.dat");
      if (entry == null)
        return null;

      var size = entry.Size - 4;
      var candidates = "";
      if (size%49 == 0)
        candidates += "B";
      if (size%45 == 0)
        candidates += "CDE";
      return candidates;
    }
    #endregion


    #region ReadAnalogFineTuning()
    private unsafe void ReadAnalogFineTuning(ZipFile zip)
    {
      int entrySize = c.avbtFineTuneLength;
      if (entrySize == 0)
        return;

      byte[] data = ReadFileContent(zip, "FineTune");
      if (data == null)
        return;

      var mapping = analogFineTuneMappings.GetMapping(c.avbtFineTuneLength);
      fixed (byte* ptr = data)
      {
        mapping.DataPtr = ptr;
        int count = data.Length / mapping.DataLength;
        for (int i = 0; i < count; i++)
        {
          bool isCable = mapping.GetFlag("offIsCable", "maskIsCable"); // HACK: this is just a guess
          int slot = mapping.GetWord("offSlotNr");
          float freq = mapping.GetFloat("offFrequency");
          var dict = isCable ? avbcFrequency : avbtFrequency;
          dict[slot] = (decimal)freq;
          mapping.Next();
        }
      }
    }
    #endregion

    #region ReadAnalogChannels()
    private unsafe void ReadAnalogChannels(ZipFile zip, string fileName, ChannelList list, out byte[] data, Dictionary<int,decimal> freq)
    {
      data = null;
      int entrySize = c.avbtChannelLength;
      if (entrySize == 0)
        return;

      data = ReadFileContent(zip, fileName);
      if (data == null)
        return;

      this.DataRoot.AddChannelList(list);
      fixed (byte* ptr = data)
      {
        var rawChannel = analogMappings.GetMapping(entrySize);
        rawChannel.DataPtr = ptr;
        int count = data.Length / entrySize;

        for (int slotIndex = 0; slotIndex < count; slotIndex++)
        {
          MapAnalogChannel(rawChannel, slotIndex, list, freq);
          rawChannel.DataPtr += entrySize;
        }
      }
    }
    #endregion

    #region MapAnalogChannel()
    private unsafe void MapAnalogChannel(AnalogChannelDataMapping rawChannel, int slotIndex, ChannelList list, Dictionary<int,decimal> freq)
    {
      if (!rawChannel.InUse)
        return;

      if (rawChannel.Checksum != CalcChecksum(rawChannel.DataPtr, c.avbtChannelLength))
        this.DataRoot.Warnings.AppendFormat("{0}: Incorrect checksum for channel index {1}\r\n", list, slotIndex);

      ChannelInfo ci = new ChannelInfo(list.SignalSource, list.SignalType, slotIndex, rawChannel.ProgramNr, rawChannel.Name);
      ci.FreqInMhz = (decimal)Math.Round(rawChannel.Frequency, 2);
      if (ci.FreqInMhz == 0)
        ci.FreqInMhz = freq.TryGet(ci.RecordIndex);
      if (ci.FreqInMhz == 0)
        ci.FreqInMhz = slotIndex;
      ci.Lock = rawChannel.Lock;
      ci.Skip = rawChannel.Skip;
      ci.Favorites = rawChannel.Favorites;

      this.DataRoot.AddChannel(list, ci);
    }

    #endregion


    #region ReadDvbctChannels()
    private unsafe void ReadDvbctChannels(ZipFile zip, string fileName, ChannelList list, out byte[] data)
    {
      data = null;
      int entrySize = c.dvbtChannelLength;
      if (entrySize == 0)
        return;

      data = ReadFileContent(zip, fileName);
      if (data == null)
        return;
      
      this.DataRoot.AddChannelList(list);
      fixed (byte* ptr = data)
      {
        DvbCtChannelDataMapping rawChannel = dvbctMappings.GetMapping(entrySize);
        rawChannel.DataPtr = ptr;
        int count = data.Length / entrySize;
        for (int slotIndex = 0; slotIndex < count; slotIndex++)
        {
          MapDvbctChannel(rawChannel, slotIndex, list);
          rawChannel.Next();
        }
      }
    }
    #endregion

    #region MapDvbctChannel()
    private unsafe void MapDvbctChannel(DvbCtChannelDataMapping rawChannel, int slotIndex, ChannelList list)
    {
      if (rawChannel.ProgramNr == 0)
        return;
      if (rawChannel.Checksum != CalcChecksum(rawChannel.DataPtr, rawChannel.DataLength))
        this.DataRoot.Warnings.AppendFormat("{0}: Incorrect checksum for channel index {1}\r\n", list, slotIndex);

      ChannelInfo ci = new ChannelInfo(list.SignalSource, list.SignalType, slotIndex, rawChannel.ProgramNr, rawChannel.Name);
      ci.VideoPid = rawChannel.VideoPid;
      ci.ServiceId = rawChannel.ServiceId;
      ci.ServiceType = rawChannel.ServiceType;
      ci.TransportStreamId = rawChannel.TransportStreamId;
      ci.FreqInMhz = LookupData.Instance.GetDvbtTransponderFrequency(rawChannel.ChannelOrTransponder);
      ci.OriginalNetworkId = rawChannel.OriginalNetworkId;
      ci.Favorites = rawChannel.Favorites;
      ci.Lock = rawChannel.Lock;
      ci.ShortName = rawChannel.ShortName;
      ci.SymbolRate = rawChannel.SymbolRate;
      ci.Encrypted = rawChannel.Encrypted;
      ci.ChannelOrTransponder = rawChannel.ChannelOrTransponder.ToString();

      this.DataRoot.AddChannel(list, ci);
    }

    #endregion


    #region ReadSatellites()
    private unsafe void ReadSatellites(ZipFile zip)
    {
      byte[] data = ReadFileContent(zip, "SatDataBase.dat");
      if (data == null)
        return;

      var utf16Encoding = new UnicodeEncoding(false, false);
      fixed (byte* ptr = data)
      {
        int count = data.Length/this.c.dvbsSatelliteLength;
        for (int i = 0; i < count; i++)
        {
          SatDataBase* sat = (SatDataBase*)(ptr + 4 + i * this.c.dvbsSatelliteLength);
          if (sat->Magic0x55 != 0x55)
            throw new IOException("Unknown SatDataBase.dat format");
          string name = utf16Encoding.GetString((byte*)sat->Name, 64*2);
          string location = string.Format("{0}.{1}{2}", sat->LongitudeTimes10/10, sat->LongitudeTimes10%10, sat->IsWest != 0 ? "W" : "E");

          Satellite satellite = new Satellite(sat->SatelliteNr);
          satellite.Name = name;
          satellite.OrbitalPosition = location;
          this.DataRoot.Satellites.Add(sat->SatelliteNr, satellite);
        }
      }
    }
    #endregion

    #region ReadTransponder()

    private unsafe void ReadTransponder(ZipFile zip, string fileName)
    {
      byte[] data = ReadFileContent(zip, fileName);
      if (data == null)
        return;

      fixed (byte* ptr = data)
      {
        int count = data.Length/c.dvbsTransponderLength;
        for (int i=0; i<count; i++)
        {
          int baseOffset = 4 + i*c.dvbsTransponderLength;
          TransponderDataBase* trans = (TransponderDataBase*) (ptr + baseOffset);
          if (trans->Magic0x55 == 0)
            continue;

          if (trans->TransponderNr == 0)
            continue;

          var sat = this.DataRoot.Satellites.TryGet(trans->SatelliteNr);
          if (sat == null)
          {
            DataRoot.Warnings.Append(string.Format("Transponder #{0} references invalid satellite #{1}",
                                                   trans->TransponderNr, trans->SatelliteNr));
            continue;
          }
          Transponder transponder = new Transponder(trans->TransponderNr);
          transponder.FrequencyInMhz = (uint) (trans->Frequency/1000);
          transponder.Polarity = trans->IsVerticalPolarity == 1 ? 'V' : 'H';
          transponder.SymbolRate = trans->SymbolRate/1000;
          this.DataRoot.AddTransponder(sat, transponder);
        }
      }
    }
    #endregion

    #region ReadDvbsChannels()
    private unsafe void ReadDvbsChannels(ZipFile zip)
    {
      this.dvbsFileContent = ReadFileContent(zip, "map-SateD");
      if (this.dvbsFileContent == null)
        return;

      this.DataRoot.AddChannelList(this.dvbsChannels);
      fixed (byte* ptr = this.dvbsFileContent)
      {
        int entrySize = c.dvbsChannelLength;
        int count = this.dvbsFileContent.Length/entrySize;
        DvbSChannelDataMapping mapping = dvbsMappings.GetMapping(entrySize);
        mapping.DataPtr = ptr;
        for (int slotIndex = 0; slotIndex < count; slotIndex++)
        {
          MapDvbsChannel(this.dvbsChannels, mapping, slotIndex);
          mapping.Next();
        }
      }
    }
    #endregion

    #region MapDvbsChannel()
    private void MapDvbsChannel(ChannelList channelList, DvbSChannelDataMapping rawChannel, int slotIndex)
    {
      if (!rawChannel.InUse)
        return;

      Transponder transponder = this.DataRoot.Transponder.TryGet(rawChannel.TransponderNr);
      Satellite satellite = transponder == null ? null : transponder.Satellite;
      string satPosition = satellite != null ? satellite.OrbitalPosition : "#" + rawChannel.SatelliteNr;
      
      string satName = satellite != null ? satellite.Name : null;

      ChannelInfo ci = new ChannelInfo(channelList.SignalSource, SignalType.Mixed, slotIndex, rawChannel.ProgramNr, rawChannel.Name);
      ci.Satellite = satName;     
      ci.SatPosition = satPosition;
      ci.VideoPid = rawChannel.VideoPid;
      ci.ServiceId = rawChannel.ServiceId;
      ci.ServiceType = rawChannel.ServiceType;
      ci.TransportStreamId = rawChannel.TransportStreamId;
      ci.OriginalNetworkId = rawChannel.OriginalNetworkId;
      ci.Favorites = rawChannel.Favorites;
      ci.Encrypted = rawChannel.Encrypted;
      ci.Lock = rawChannel.Lock;
      ci.ShortName = rawChannel.ShortName;

      if (transponder != null)
      {
        ci.Transponder = transponder;
        ci.FreqInMhz = transponder.FrequencyInMhz;
        ci.ChannelOrTransponder = this.GetTransponderChannelNumber((ushort)transponder.FrequencyInMhz);
        ci.SymbolRate = transponder.SymbolRate;
        ci.Polarity = transponder.Polarity;
      }

      this.DataRoot.AddChannel(channelList, ci);
    }

    #endregion

    #region GetTransponderChannelNumber()
    private string GetTransponderChannelNumber(ushort frequency)
    {
      int nr = LookupData.Instance.GetTransponderNumber(frequency);
      return nr <= 0 ? "" : nr.ToString("d3");
    }
    #endregion


    #region ReadAstraHdPlusChannels()
    private unsafe void ReadAstraHdPlusChannels(ZipFile zip)
    {
      this.hdplusFileContent = ReadFileContent(zip, "map-AstraHDPlusD");
      if (hdplusFileContent == null || c.hdplusChannelLength == 0)
        return;

      this.DataRoot.AddChannelList(this.hdplusChannels);
      fixed (byte* ptr = hdplusFileContent)
      {
        int entrySize = c.hdplusChannelLength;
        int count = hdplusFileContent.Length / entrySize;
        DvbSChannelDataMapping mapping = hdplusMappings.GetMapping(entrySize);
        mapping.DataPtr = ptr;
        for (int slotIndex = 0; slotIndex < count; slotIndex++)
        {
          MapDvbsChannel(this.hdplusChannels, mapping, slotIndex);
          mapping.Next();
        }
      }
    }
    #endregion


    #region ReadFileContent()
    private static byte[] ReadFileContent(ZipFile zip, string fileName)
    {
      var entry = zip.GetEntry(fileName);
      if (entry == null)
        return null;
      byte[] data = new byte[entry.Size];
      using (var stream = zip.GetInputStream(entry))
      {
        stream.Read(data, 0, data.Length);
      }
      return data;
    }
    #endregion


    private unsafe delegate void UpdateFunc(ChannelInfo channelInfo, byte* fileContent);

    #region Save()
    public override unsafe void Save(string tvOutputFile, string csvOutputFile, UnsortedChannelMode unsortedMode)
    {
      this.unsortedChannelMode = unsortedMode;
      if (tvOutputFile != this.FileName)
      {
        File.Copy(this.FileName, tvOutputFile);
        this.FileName = tvOutputFile;
      }
      using (ZipFile zip = new ZipFile(tvOutputFile))
      {
        zip.BeginUpdate();
        this.SaveChannels(zip, "map-AirA", this.avbtChannels, this.UpdateAnalogChannel, ref this.avbtFileContent);
        this.SaveChannels(zip, "map-CableA", this.avbcChannels, this.UpdateAnalogChannel, ref this.avbcFileContent);
        this.SaveChannels(zip, "map-AirD", this.dvbtChannels, this.UpdateDvbctChannel, ref this.dvbtFileContent);
        this.SaveChannels(zip, "map-CableD", this.dvbcChannels, this.UpdateDvbctChannel, ref this.dvbcFileContent);
        this.SaveChannels(zip, "map-SateD", this.dvbsChannels, this.UpdateDvbsChannel, ref this.dvbsFileContent);
        this.SaveChannels(zip, "map-AstraHDPlusD", this.hdplusChannels, this.UpdateHdPlusChannel, ref this.hdplusFileContent);
        zip.CommitUpdate();
      }
    }
    #endregion

    #region SaveChannels()
    private unsafe void SaveChannels(ZipFile zip, string fileName, ChannelList channels, UpdateFunc updateChannel, ref byte[] fileContent)
    {
      if (fileContent == null)
        return;
      zip.Delete(fileName);

      string tempFilePath = Path.GetTempFileName();
      using (var stream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
      {
        this.WriteChannels(channels, updateChannel, fileContent, stream);
        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        int size = (int)new FileInfo(tempFilePath).Length;
        fileContent = new byte[size];
        stream.Read(fileContent, 0, size);
      }

      zip.Add(tempFilePath, fileName);
    }
    #endregion

    #region WriteChannels()
    private unsafe void WriteChannels(ChannelList list, UpdateFunc updateChannel, byte[] fileContent, FileStream stream)
    {
      fixed (byte* ptr = fileContent)
      {
        int maxSlot = 0;
        foreach (var channel in list.Channels.OrderBy(ChannelComparer))
        {
          if (channel.RecordIndex < 0) // channel only exists in reference list
            continue;

          int newProgNr;
          if (channel.NewProgramNr == 0 && this.unsortedChannelMode == UnsortedChannelMode.Hide)
            newProgNr = 0;
          else
          {
            newProgNr = channel.NewProgramNr != 0 ? channel.NewProgramNr : ++maxSlot;
            if (newProgNr > maxSlot)
              maxSlot = channel.NewProgramNr;
          }

          var channels = new List<ChannelInfo>();
          channels.Add(channel);
          channels.AddRange(channel.Duplicates);
          foreach (var channelInstance in channels)
          {
            channelInstance.OldProgramNr = newProgNr;
            updateChannel(channelInstance, ptr);
          }
        }

        stream.Write(fileContent, 0, fileContent.Length);
#if false
        if (count == 0 || count%padToPageSize != 0)
        {
          byte[] padding = new byte[(padToPageSize - count%padToPageSize) * entrySize];
          stream.Write(padding, 0, padding.Length);
        }
#endif
      }
    }
    #endregion

    #region UpdateAnalogChannel()
    private unsafe void UpdateAnalogChannel(ChannelInfo channel, byte* ptr)
    {
      AnalogChannelDataMapping raw = analogMappings.GetMapping(c.avbtChannelLength);
      int startOffset = channel.RecordIndex * raw.DataLength;
      raw.DataPtr = ptr + startOffset;
      raw.ProgramNr = (ushort)channel.OldProgramNr;
      raw.Favorites = channel.Favorites;
      raw.Lock = channel.Lock;
      raw.Checksum = CalcChecksum(raw.DataPtr, raw.DataLength);
    }
    #endregion

    #region UpdateDvbctChannel()
    private unsafe void UpdateDvbctChannel(ChannelInfo channel, byte* ptr)
    {
      DvbCtChannelDataMapping raw = dvbctMappings.GetMapping(c.dvbtChannelLength);
      int startOffset = channel.RecordIndex * raw.DataLength;
      raw.DataPtr = ptr + startOffset;
      raw.ProgramNr = (ushort)channel.OldProgramNr;
      raw.Favorites = channel.Favorites;
      raw.Lock = channel.Lock;
      raw.Checksum = CalcChecksum(raw.DataPtr, raw.DataLength);
    }
    #endregion

    #region UpdateDvbsChannel()
    private unsafe void UpdateDvbsChannel(ChannelInfo channel, byte* ptr)
    {
      DvbSChannelDataMapping raw = dvbsMappings.GetMapping(c.dvbsChannelLength);
      int startOffset = channel.RecordIndex*raw.DataLength;
      raw.DataPtr = (ptr + startOffset);
      raw.ProgramNr = (ushort)channel.OldProgramNr;
      raw.Favorites = channel.Favorites;
      raw.Lock = channel.Lock;
      raw.Checksum = CalcChecksum(raw.DataPtr, raw.DataLength);
    }
    #endregion

    #region UpdateHdPlusChannel()
    private unsafe void UpdateHdPlusChannel(ChannelInfo channel, byte* ptr)
    {
      DvbSChannelDataMapping raw = hdplusMappings.GetMapping(c.hdplusChannelLength);
      int startOffset = channel.RecordIndex * raw.DataLength;
      raw.DataPtr = (ptr + startOffset);
      raw.ProgramNr = (ushort)channel.OldProgramNr;
      raw.Favorites = channel.Favorites;
      raw.Lock = channel.Lock;
      raw.Checksum = CalcChecksum(raw.DataPtr, raw.DataLength);
    }
    #endregion

    #region CalcChecksum()
    private static unsafe byte CalcChecksum(byte* ptr, int length)
    {
      byte checksum = 0;
      for (int i = 1; i < length; i++)
        checksum += *ptr++;
      return checksum;
    }
    #endregion

    #region ChannelComparer()
    private string ChannelComparer(ChannelInfo channel)
    {
      if (channel.NewProgramNr != 0)
        return "A" + channel.NewProgramNr.ToString("d4");
      if (channel.OldProgramNr != 0)
      {
        if (this.unsortedChannelMode == UnsortedChannelMode.AppendInOrder)
          return "B" + channel.OldProgramNr.ToString("d4");
        return "B" + channel.Name;
      }
      return "Z";
    }
    #endregion

    public override void ShowDeviceSettingsForm(object parentWindow)
    {
      MessageBox.Show((Form) parentWindow, "Sorry, ChanSort currently doesn't support any settings for your TV model.",
                      "Device Settings",
                      MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
  }
}
