using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using ChanSort.Api;
using ICSharpCode.SharpZipLib.Zip;

namespace ChanSort.Loader.ScmFile
{
  class ScmSerializer : SerializerBase
  {
    private readonly Dictionary<string, ModelConstants> modelConstants = new Dictionary<string, ModelConstants>();
    private readonly MappingPool<DataMapping> analogMappings = new MappingPool<DataMapping>("Analog");
    private readonly MappingPool<DataMapping> dvbctMappings = new MappingPool<DataMapping>("DVB-C/T");
    private readonly MappingPool<DataMapping> dvbsMappings = new MappingPool<DataMapping>("DVB-S");
    private readonly MappingPool<DataMapping> hdplusMappings = new MappingPool<DataMapping>("AstraHD+");
    private readonly MappingPool<DataMapping> analogFineTuneMappings = new MappingPool<DataMapping>("FineTune");
    private readonly MappingPool<DataMapping> ptccableMappings = new MappingPool<DataMapping>("PTC");
    private readonly MappingPool<DataMapping> transponderMappings = new MappingPool<DataMapping>("TransponderDataBase");

    private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT|SignalSource.TvAndRadio, "analog Air");
    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC|SignalSource.TvAndRadio, "analog Cable");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT | SignalSource.Tv, "digital Air");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC | SignalSource.TvAndRadio, "digital Cable");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS | SignalSource.TvAndRadio, "Satellite");
    private readonly ChannelList hdplusChannels = new ChannelList(SignalSource.HdPlusD | SignalSource.TvAndRadio, "Astra HD+");
    
    private readonly Dictionary<int, decimal> avbtFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> avbcFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> dvbcFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> dvbtFrequency = new Dictionary<int, decimal>();
    
    private byte[] avbtFileContent;
    private byte[] avbcFileContent;
    private byte[] dvbtFileContent;
    private byte[] dvbcFileContent;
    private byte[] dvbsFileContent;
    private byte[] hdplusFileContent;
    private ModelConstants c;

    #region ctor()
    public ScmSerializer(string inputFile) : base(inputFile)
    {
      this.ReadConfigurationFromIniFile();
      this.Features.ChannelNameEdit = true;
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
          analogMappings.AddMapping(len, new DataMapping(section));
        else if (section.Name.StartsWith("DvbCT:"))
          dvbctMappings.AddMapping(len, new DataMapping(section));
        else if (section.Name.StartsWith("DvbS:"))
          dvbsMappings.AddMapping(len, new DataMapping(section));
        else if (section.Name.StartsWith("FineTune:"))
          analogFineTuneMappings.AddMapping(len, new DataMapping(section));
        else if (section.Name.StartsWith("AstraHDPlusD:"))
          hdplusMappings.AddMapping(len, new DataMapping(section));
        else if (section.Name.StartsWith("TransponderDataBase.dat:"))
          transponderMappings.AddMapping(len, new DataMapping(section));
        else if (section.Name.StartsWith("PTC:"))
          ptccableMappings.AddMapping(len, new DataMapping(section));
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
        ReadDvbTransponderFrequenciesFromPtc(zip, "PTCAIR", this.dvbtFrequency);
        ReadDvbctChannels(zip, "map-AirD", this.dvbtChannels, out this.dvbtFileContent, this.dvbtFrequency);
        ReadDvbTransponderFrequenciesFromPtc(zip, "PTCCABLE", this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-CableD", this.dvbcChannels, out this.dvbcFileContent, this.dvbcFrequency);
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
    private void ReadAnalogFineTuning(ZipFile zip)
    {
      int entrySize = c.avbtFineTuneLength;
      if (entrySize == 0)
        return;

      byte[] data = ReadFileContent(zip, "FineTune");
      if (data == null)
        return;

      var mapping = analogFineTuneMappings.GetMapping(c.avbtFineTuneLength);
      mapping.SetDataPtr(data, 0);
      int count = data.Length / c.avbtFineTuneLength;
      for (int i = 0; i < count; i++)
      {
        bool isCable = mapping.GetFlag("offIsCable", "maskIsCable"); // HACK: this is just a guess
        int slot = mapping.GetWord("offSlotNr");
        float freq = mapping.GetFloat("offFrequency");
        var dict = isCable ? avbcFrequency : avbtFrequency;
        dict[slot] = (decimal)freq;
        mapping.BaseOffset += c.avbtFineTuneLength;
      }
    }
    #endregion

    #region ReadAnalogChannels()
    private void ReadAnalogChannels(ZipFile zip, string fileName, ChannelList list, out byte[] data, Dictionary<int,decimal> freq)
    {
      data = null;
      int entrySize = c.avbtChannelLength;
      if (entrySize == 0)
        return;

      data = ReadFileContent(zip, fileName);
      if (data == null)
        return;

      this.DataRoot.AddChannelList(list);
      var rawChannel = analogMappings.GetMapping(entrySize);
      rawChannel.SetDataPtr(data, 0);
      int count = data.Length / entrySize;

      for (int slotIndex = 0; slotIndex < count; slotIndex++)
      {
        MapAnalogChannel(rawChannel, slotIndex, list, freq.TryGet(slotIndex));
        rawChannel.BaseOffset += entrySize;
      }
    }
    #endregion

    #region MapAnalogChannel()
    private void MapAnalogChannel(DataMapping rawChannel, int slotIndex, ChannelList list, decimal freq)
    {
      bool isCable = (list.SignalSource & SignalSource.Cable) != 0;
      AnalogChannel ci = new AnalogChannel(slotIndex, isCable, rawChannel, freq, c.favoriteNotSetValue);
      if (!ci.InUse)
        return;

      this.DataRoot.AddChannel(list, ci);
    }
    #endregion


    #region ReadDvbTransponderFrequenciesFromPtc()
    private void ReadDvbTransponderFrequenciesFromPtc(ZipFile zip, string file, IDictionary<int, decimal> table)
    {
      byte[] data = ReadFileContent(zip, file);
      if (data == null)
        return;

      var mapping = ptccableMappings.GetMapping(c.ptcLength);
      mapping.SetDataPtr(data, 0);
      int count = data.Length / c.ptcLength;
      for (int i = 0; i < count; i++)
      {
        int transp = mapping.GetWord("offChannelTransponder");
        float freq = mapping.GetFloat("offFrequency");
        table[transp] = (decimal)freq;
        mapping.BaseOffset += c.ptcLength;
      }
    }
    #endregion

    #region ReadDvbctChannels()
    private void ReadDvbctChannels(ZipFile zip, string fileName, ChannelList list, out byte[] data, Dictionary<int, decimal> frequency)
    {
      data = null;
      int entrySize = c.dvbtChannelLength;
      if (entrySize == 0)
        return;

      data = ReadFileContent(zip, fileName);
      if (data == null)
        return;

      bool isCable = (list.SignalSource & SignalSource.Cable) != 0;
      this.DataRoot.AddChannelList(list);
      DataMapping rawChannel = dvbctMappings.GetMapping(entrySize);
      rawChannel.SetDataPtr(data, 0);
      int count = data.Length / entrySize;
      for (int slotIndex = 0; slotIndex < count; slotIndex++)
      {
        DigitalChannel ci = new DigitalChannel(slotIndex, isCable, rawChannel, frequency, c.favoriteNotSetValue);
        if (ci.OldProgramNr != 0)
          this.DataRoot.AddChannel(list, ci);

        rawChannel.BaseOffset += entrySize;
      }
    }
    #endregion


    #region ReadSatellites()
    private void ReadSatellites(ZipFile zip)
    {
      byte[] data = ReadFileContent(zip, "SatDataBase.dat");
      if (data == null)
        return;

      SatelliteMapping satMapping = new SatelliteMapping(data, 4);
      int count = data.Length/this.c.dvbsSatelliteLength;
      for (int i = 0; i < count; i++)
      {
        if (satMapping.MagicMarker != 0x55)
          throw new IOException("Unknown SatDataBase.dat format");        
        string location = string.Format("{0}.{1}{2}", 
          satMapping.Longitude/10, satMapping.Longitude%10, satMapping.IsEast ? "E" : "W");

        Satellite satellite = new Satellite(satMapping.SatelliteNr);
        satellite.Name = satMapping.Name;
        satellite.OrbitalPosition = location;
        this.DataRoot.Satellites.Add(satMapping.SatelliteNr, satellite);

        satMapping.BaseOffset += this.c.dvbsSatelliteLength;
      }
    }
    #endregion

    #region ReadTransponder()

    private void ReadTransponder(ZipFile zip, string fileName)
    {
      byte[] data = ReadFileContent(zip, fileName);
      if (data == null)
        return;

      int count = (data.Length-4)/c.dvbsTransponderLength;
      var mapping = this.transponderMappings.GetMapping(c.dvbsTransponderLength);
      for (int i=0; i<count; i++)
      {
        mapping.SetDataPtr(data, 4 + i * c.dvbsTransponderLength);
        if (mapping.GetByte("offMagicByte") == 0)
          continue;

        int transponderNr = (int)mapping.GetDword("offTransponderIndex");
        if (transponderNr == 0)
          continue;

        int satelliteNr = (int)mapping.GetDword("offSatelliteIndex");
        var sat = this.DataRoot.Satellites.TryGet(satelliteNr);
        if (sat == null)
        {
          DataRoot.Warnings.Append(string.Format("Transponder #{0} references invalid satellite #{1}",
            transponderNr, satelliteNr));
          continue;
        }
        Transponder transponder = new Transponder(transponderNr);
        transponder.FrequencyInMhz = (uint)(mapping.GetDword("offFrequency")/1000);
        transponder.SymbolRate = (int) (mapping.GetDword("offSymbolRate")/1000);
        this.DataRoot.AddTransponder(sat, transponder);
      }
    }
    #endregion

    #region ReadDvbsChannels()
    private void ReadDvbsChannels(ZipFile zip)
    {
      this.dvbsFileContent = ReadFileContent(zip, "map-SateD");
      if (this.dvbsFileContent == null)
        return;

      this.DataRoot.AddChannelList(this.dvbsChannels);
      int entrySize = c.dvbsChannelLength;
      int count = this.dvbsFileContent.Length/entrySize;
      DataMapping mapping = dvbsMappings.GetMapping(entrySize);
      mapping.SetDataPtr(dvbsFileContent, 0);
      for (int slotIndex = 0; slotIndex < count; slotIndex++)
      {
        SatChannel ci = new SatChannel(slotIndex, SignalSource.StandardSat, mapping, this.DataRoot, c.favoriteNotSetValue);
        if (ci.InUse)
          this.DataRoot.AddChannel(this.dvbsChannels, ci);

        mapping.BaseOffset += entrySize;
      }
    }
    #endregion

    #region ReadAstraHdPlusChannels()
    private void ReadAstraHdPlusChannels(ZipFile zip)
    {
      this.hdplusFileContent = ReadFileContent(zip, "map-AstraHDPlusD");
      if (hdplusFileContent == null || c.hdplusChannelLength == 0)
        return;

      this.DataRoot.AddChannelList(this.hdplusChannels);
      int entrySize = c.hdplusChannelLength;
      int count = hdplusFileContent.Length / entrySize;
      DataMapping mapping = hdplusMappings.GetMapping(entrySize);
      mapping.SetDataPtr(hdplusFileContent, 0);
      for (int slotIndex = 0; slotIndex < count; slotIndex++)
      {
        SatChannel ci = new SatChannel(slotIndex, SignalSource.AstraHdPlus, mapping, this.DataRoot, c.favoriteNotSetValue);
        if (ci.InUse)
          this.DataRoot.AddChannel(this.hdplusChannels, ci);
        mapping.BaseOffset += entrySize;
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

    #region Save()
    public override void Save(string tvOutputFile, string csvOutputFile)
    {
      if (tvOutputFile != this.FileName)
      {
        File.Copy(this.FileName, tvOutputFile);
        this.FileName = tvOutputFile;
      }
      using (ZipFile zip = new ZipFile(tvOutputFile))
      {
        zip.BeginUpdate();
        this.SaveChannels(zip, "map-AirA", this.avbtChannels, ref this.avbtFileContent);
        this.SaveChannels(zip, "map-CableA", this.avbcChannels, ref this.avbcFileContent);
        this.SaveChannels(zip, "map-AirD", this.dvbtChannels, ref this.dvbtFileContent);
        this.SaveChannels(zip, "map-CableD", this.dvbcChannels, ref this.dvbcFileContent);
        this.SaveChannels(zip, "map-SateD", this.dvbsChannels, ref this.dvbsFileContent);
        this.SaveChannels(zip, "map-AstraHDPlusD", this.hdplusChannels, ref this.hdplusFileContent);
        zip.CommitUpdate();
      }
    }
    #endregion

    #region SaveChannels()
    private void SaveChannels(ZipFile zip, string fileName, ChannelList channels, ref byte[] fileContent)
    {
      if (fileContent == null)
        return;
      zip.Delete(fileName);

      string tempFilePath = Path.GetTempFileName();
      using (var stream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
      {
        this.WriteChannels(channels, fileContent, stream);
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
    private void WriteChannels(ChannelList list, byte[] fileContent, FileStream stream)
    {
      foreach (var channel in list.Channels)
      {
        channel.UpdateRawData();
        channel.OldProgramNr = channel.NewProgramNr;
      }

      stream.Write(fileContent, 0, fileContent.Length);
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
