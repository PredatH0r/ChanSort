using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using ChanSort.Api;
using ICSharpCode.SharpZipLib.Zip;

namespace ChanSort.Loader.Samsung
{
  internal class ScmSerializer : SerializerBase
  {
    private readonly Dictionary<string, ModelConstants> modelConstants = new Dictionary<string, ModelConstants>();
    private readonly MappingPool<DataMapping> analogMappings = new MappingPool<DataMapping>("Analog");
    private readonly MappingPool<DataMapping> dvbctMappings = new MappingPool<DataMapping>("DVB-C/T");
    private readonly MappingPool<DataMapping> dvbsMappings = new MappingPool<DataMapping>("DVB-S");
    private readonly MappingPool<DataMapping> hdplusMappings = new MappingPool<DataMapping>("AstraHD+");
    private readonly MappingPool<DataMapping> analogFineTuneMappings = new MappingPool<DataMapping>("FineTune");
    private readonly MappingPool<DataMapping> ptccableMappings = new MappingPool<DataMapping>("PTC");
    private readonly MappingPool<DataMapping> transponderMappings = new MappingPool<DataMapping>("TransponderDataBase");
    private readonly MappingPool<DataMapping> serviceProviderMappings = new MappingPool<DataMapping>("ServiceProvider");

    private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT|SignalSource.TvAndRadio, "Analog Air");
    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC|SignalSource.TvAndRadio, "Analog Cable");
    private readonly ChannelList avbxChannels = new ChannelList(SignalSource.AnalogCT | SignalSource.TvAndRadio, "Analog Air/Cable");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT | SignalSource.Tv, "Digital Air");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC | SignalSource.TvAndRadio, "Digital Cable");
    private readonly ChannelList dvbxChannels = new ChannelList(SignalSource.DvbCT | SignalSource.TvAndRadio, "Digital Air/Cable");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS | SignalSource.TvAndRadio, "Satellite");
    private readonly ChannelList primeChannels = new ChannelList(SignalSource.CablePrimeD | SignalSource.TvAndRadio, "Cable Prime");
    private readonly ChannelList hdplusChannels = new ChannelList(SignalSource.HdPlusD | SignalSource.TvAndRadio, "Astra HD+");
    private readonly ChannelList freesatChannels = new ChannelList(SignalSource.FreesatD | SignalSource.TvAndRadio, "Freesat");
    private readonly ChannelList tivusatChannels = new ChannelList(SignalSource.TivuSatD | SignalSource.TvAndRadio, "TivuSat");
    private readonly ChannelList canalDigitalChannels = new ChannelList(SignalSource.CanalDigitalSatD | SignalSource.TvAndRadio, "Canal Digital Sat");
    private readonly ChannelList digitalPlusChannels = new ChannelList(SignalSource.DigitalPlusD | SignalSource.TvAndRadio, "Canal+ Digital");
    private readonly ChannelList cyfraPlusChannels = new ChannelList(SignalSource.CyfraPlusD | SignalSource.TvAndRadio, "Cyfra+ Digital");
    
    private readonly Dictionary<int, decimal> avbtFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> avbcFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> dvbcFrequency = new Dictionary<int, decimal>();
    private readonly Dictionary<int, decimal> dvbtFrequency = new Dictionary<int, decimal>();
    
    private byte[] avbtFileContent;
    private byte[] avbcFileContent;
    private byte[] avbxFileContent;
    private byte[] dvbtFileContent;
    private byte[] dvbcFileContent;
    private byte[] dvbxFileContent;
    private byte[] dvbsFileContent;
    private byte[] hdplusFileContent;
    private byte[] primeFileContent;
    private byte[] freesatFileContent;
    private byte[] tivusatFileContent;
    private byte[] canalDigitalFileContent;
    private byte[] digitalPlusFileContent;
    private byte[] cyfraPlusFileContent;
    private ModelConstants c;
    private Dictionary<int, string> serviceProviderNames;

    #region ctor()
    public ScmSerializer(string inputFile) : base(inputFile)
    {
      this.ReadConfigurationFromIniFile();
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CleanUpChannelData = true;
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
        else if (section.Name.StartsWith("ServiceProvider"))
          serviceProviderMappings.AddMapping(len, new DataMapping(section));
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
        DataRoot.SortedFavorites = c.SortedFavorites == FavoritesIndexMode.IndividuallySorted;

        ReadAnalogFineTuning(zip);
        ReadAnalogChannels(zip, "map-AirA", this.avbtChannels, out this.avbtFileContent, this.avbtFrequency);
        ReadAnalogChannels(zip, "map-CableA", this.avbcChannels, out this.avbcFileContent, this.avbcFrequency);
        ReadAnalogChannels(zip, "map-AirCableMixedA", this.avbxChannels, out this.avbxFileContent, this.avbcFrequency);
        ReadDvbTransponderFrequenciesFromPtc(zip, "PTCAIR", this.dvbtFrequency);
        ReadDvbServiceProviders(zip);
        ReadDvbctChannels(zip, "map-AirD", this.dvbtChannels, out this.dvbtFileContent, this.dvbtFrequency);
        ReadDvbTransponderFrequenciesFromPtc(zip, "PTCCABLE", this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-CableD", this.dvbcChannels, out this.dvbcFileContent, this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-AirCableMixedD", this.dvbxChannels, out this.dvbxFileContent, this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-CablePrime_D", this.primeChannels, out this.primeFileContent, this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-FreesatD", this.freesatChannels, out this.freesatFileContent, this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-TivusatD", this.tivusatChannels, out this.tivusatFileContent, this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-CanalDigitalSatD", this.canalDigitalChannels, out this.canalDigitalFileContent, this.dvbcFrequency);
        ReadDvbctChannels(zip, "map-DigitalPlusD", this.digitalPlusChannels, out this.digitalPlusFileContent, this.dvbcFrequency);
        ReadSatellites(zip);
        ReadTransponder(zip, "TransponderDataBase.dat");
        ReadTransponder(zip, "UserTransponderDataBase.dat");
        ReadDvbsChannels(zip, "map-SateD", this.dvbsChannels, out this.dvbsFileContent, c.dvbsChannelLength);
        ReadDvbsChannels(zip, "map-CyfraPlusD", this.cyfraPlusChannels, out this.cyfraPlusFileContent, c.cyfraPlusChannelSize);
        ReadAstraHdPlusChannels(zip);        
      }
    }
    #endregion

    #region DetectModelConstants()
    internal void DetectModelConstants(ZipFile zip)
    {
      if (DetectModelFromFileName()) return;
      if (DetectModelFromCloneInfoFile(zip)) return;
      if (DetectModelFromContentFileLengths(zip)) return;
      throw new FileLoadException("Unable to determine TV model from file content or name");
    }
    #endregion

    #region DetectModelFromFileName()
    private bool DetectModelFromFileName()
    {
      string file = Path.GetFileName(this.FileName)??"";
      System.Text.RegularExpressions.Regex regex =
        new System.Text.RegularExpressions.Regex("channel_list_([A-Z]{2}[0-9]{2}|BD-)([A-Z])[0-9A-Z]+_([0-9]{4}).*\\.scm");
      var match = regex.Match(file);
      if (match.Success)
      {
        string series;
        switch (match.Groups[3].Value)
        {
          case "1001": series = "C"; break;
          case "1101": series = "D"; break;
          case "1201":
            var letter = match.Groups[2].Value;

            // F, H and low-end J series use same file format            
            series = letter == "E" ? "E" : "F";
            break;
          default:
            return false;
        }
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
        if (series == 'B') // LTxxBxxx uses E/F format. the old B-series has no CloneInfo file, so we can tell the difference
          series = 'F';
        else if (series == 'C') // there are models with a C that are actually F: LTxxCxxx, HExxCxxx, ... so we can't decide here
          return false; 
        else if (series >= 'F') // F, H, low-end J
          series = 'F'; 
        if (this.modelConstants.TryGetValue("Series:" + series, out this.c))
          return true;
      }
      return false;
    }
    #endregion

    #region DetectModelFromContentFileLengths()
    private bool DetectModelFromContentFileLengths(ZipFile zip)
    {
      string[] candidates = {
                              DetectModelFromAirAOrCableA(zip),
                              DetectModelFromAirDOrCableD(zip),
                              DetectModelFromSateD(zip),
                              DetectModelFromTranspoderDatabase(zip),
                              DetectModelFromAstraHdPlusD(zip)
                            };

      // E, F, B(2013), H, low-end J series use an identical format, so we only care about one here      
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

      if (validCandidates.Length == 0)
        return false;

      var series = validCandidates[0];
      if (series == 'E') // E allows favorites to be individually sorted, while F-J require them to match the main program nr
        series = 'F';    // since we can't tell the difference from the format, we use the safe F/H/J logic, which also works for E
      this.modelConstants.TryGetValue("Series:" + series, out this.c);
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
      var entry = zip.GetEntry("map-AirD") ?? zip.GetEntry("map-CableD") ?? zip.GetEntry("map-CablePrime_D") ?? zip.GetEntry("map-FreesatD")
        ?? zip.GetEntry("map-TivusatD") ?? zip.GetEntry("map-CanalDigitalSatD") ?? zip.GetEntry("map-DigitalPlusD");
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

    #region DetectModelFromAstraHdPlusD()
    private string DetectModelFromAstraHdPlusD(ZipFile zip)
    {
      var entry = zip.GetEntry("map-AstraHDPlusD");
      if (entry == null)
        return null;

      var size = entry.Size;
      string candidates = null;
      if (size % 212 == 0)
        candidates += "DE";
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
      list.MaxChannelNameLength = rawChannel.Settings.GetInt("lenName")/2;
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
      AnalogChannel ci = new AnalogChannel(slotIndex, isCable, rawChannel, freq, c.SortedFavorites);
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

    #region ReadDvbServiceProviders()
    private void ReadDvbServiceProviders(ZipFile zip)
    {
      this.serviceProviderNames = new Dictionary<int, string>();
      var data = ReadFileContent(zip, "ServiceProviders");
      if (data == null) return;

      if (data.Length % c.serviceProviderLength != 0) return;
      var mapping = serviceProviderMappings.GetMapping(c.serviceProviderLength, false);
      if (mapping == null) return;
      int count = data.Length/c.serviceProviderLength;
      var enc = new UnicodeEncoding(true, false);
      var offName = mapping.Settings.GetInt("offName");
      for (int i = 0; i < count; i++)
      {
        mapping.SetDataPtr(data, i*c.serviceProviderLength);
        int source = mapping.GetWord("offSignalSource");
        int index = mapping.GetWord("offIndex");
        int len = System.Math.Min(mapping.GetWord("offLenName"), c.serviceProviderLength - offName);
        var name = len < 2 ? "" : enc.GetString(data, mapping.BaseOffset + offName, len);
        this.serviceProviderNames[(source << 16) + index] = name;
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

      this.DataRoot.AddChannelList(list);
      var source = list.SignalSource;
      DataMapping rawChannel = dvbctMappings.GetMapping(entrySize);
      list.MaxChannelNameLength = rawChannel.Settings.GetInt("lenName") / 2;
      rawChannel.SetDataPtr(data, 0);
      int count = data.Length / entrySize;
      for (int slotIndex = 0; slotIndex < count; slotIndex++)
      {
        DigitalChannel ci = new DigitalChannel(slotIndex, source, rawChannel, frequency, c.SortedFavorites, this.serviceProviderNames);
        if (ci.InUse && !ci.IsDeleted && ci.OldProgramNr > 0)
          this.DataRoot.AddChannel(list, ci);

        rawChannel.BaseOffset += entrySize;
      }
    }
    #endregion


    #region ReadSatellites()
    private void ReadSatellites(ZipFile zip)
    {
      byte[] data = ReadFileContent(zip, "SatDataBase.dat");
      if (data == null || data.Length < 4)
        return;

      this.SatDatabaseVersion = System.BitConverter.ToInt32(data, 0);
      SatelliteMapping satMapping = new SatelliteMapping(data, 4);
      int count = data.Length/this.c.dvbsSatelliteLength;
      for (int i = 0; i < count; i++)
      {
        if (satMapping.MagicMarker == 'U')
        {
          string location = string.Format("{0}.{1}{2}",
            satMapping.Longitude / 10, satMapping.Longitude % 10, satMapping.IsEast ? "E" : "W");

          Satellite satellite = new Satellite(satMapping.SatelliteNr);
          satellite.Name = satMapping.Name;
          satellite.OrbitalPosition = location;
          this.DataRoot.Satellites.Add(satMapping.SatelliteNr, satellite);
        }
        else if (satMapping.MagicMarker != 'E')
          throw new FileLoadException("Unknown SatDataBase.dat format");

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
    private void ReadDvbsChannels(ZipFile zip, string filename, ChannelList channels, out byte[] fileContent, int entrySize)
    {
      fileContent = ReadFileContent(zip, filename);
      if (fileContent == null)
        return;

      this.DataRoot.AddChannelList(channels);
      int count = fileContent.Length/entrySize;
      DataMapping mapping = dvbsMappings.GetMapping(entrySize);
      channels.MaxChannelNameLength = mapping.Settings.GetInt("lenName") / 2;
      mapping.SetDataPtr(fileContent, 0);
      for (int slotIndex = 0; slotIndex < count; slotIndex++)
      {
        SatChannel ci = new SatChannel(slotIndex, channels.SignalSource, mapping, this.DataRoot, c.SortedFavorites, this.serviceProviderNames);
        if (ci.InUse)
          this.DataRoot.AddChannel(channels, ci);

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
        SatChannel ci = new SatChannel(slotIndex, SignalSource.AstraHdPlus, mapping, this.DataRoot, c.SortedFavorites, this.serviceProviderNames);
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
    public override void Save(string tvOutputFile)
    {
      if (tvOutputFile != this.FileName)
      {
        File.Copy(this.FileName, tvOutputFile, true);
        this.FileName = tvOutputFile;
      }
      using (ZipFile zip = new ZipFile(tvOutputFile))
      {
        zip.BeginUpdate();
        this.SaveChannels(zip, "map-AirA", this.avbtChannels, this.avbtFileContent);
        this.SaveChannels(zip, "map-CableA", this.avbcChannels, this.avbcFileContent);
        this.SaveChannels(zip, "map-AirCableMixedA", this.avbxChannels, this.avbxFileContent);
        this.SaveChannels(zip, "map-AirD", this.dvbtChannels, this.dvbtFileContent);
        this.SaveChannels(zip, "map-CableD", this.dvbcChannels, this.dvbcFileContent);
        this.SaveChannels(zip, "map-AirCableMixedD", this.dvbxChannels, this.dvbxFileContent);
        this.SaveChannels(zip, "map-SateD", this.dvbsChannels, this.dvbsFileContent);
        this.SaveChannels(zip, "map-AstraHDPlusD", this.hdplusChannels, this.hdplusFileContent);
        this.SaveChannels(zip, "map-CablePrime_D", this.primeChannels, this.primeFileContent);
        this.SaveChannels(zip, "map-FreesatD", this.freesatChannels, this.freesatFileContent);
        this.SaveChannels(zip, "map-TivusatD", this.tivusatChannels, this.tivusatFileContent);
        this.SaveChannels(zip, "map-CanalDigitalSatD", this.canalDigitalChannels, this.canalDigitalFileContent);
        this.SaveChannels(zip, "map-DigitalPlusD", this.digitalPlusChannels, this.digitalPlusFileContent);
        this.SaveChannels(zip, "map-CyfraPlusD", this.cyfraPlusChannels, this.cyfraPlusFileContent);
        zip.CommitUpdate();
      }
    }
    #endregion

    #region SaveChannels()
    private void SaveChannels(ZipFile zip, string fileName, ChannelList channels, byte[] fileContent)
    {
      if (fileContent == null)
        return;
      zip.Delete(fileName);

      string tempFilePath = Path.GetTempFileName();
      using (var stream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
      {
        this.WriteChannels(channels, fileContent, stream);
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

    // -------- cleanup --------

    #region CleanUpChannelData()
    public override string CleanUpChannelData()
    {
      StringBuilder log = new StringBuilder();
      RemoveChannelsWithServiceType0(log);
      RemoveDuplicateAnalogList(log);
      return log.ToString();
    }

    private void RemoveChannelsWithServiceType0(StringBuilder log)
    {
      foreach (var list in this.DataRoot.ChannelLists)
      {
        if ((list.SignalSource & SignalSource.Digital) == 0)
          continue;
        var listOfChannels = new List<ChannelInfo>(list.Channels);
        foreach (var chan in listOfChannels)
        {
          ScmChannelBase channel = chan as ScmChannelBase;
          if (channel == null) // ignore proxy channels (which only exist in loaded reference list)
            continue;
          if (channel.ServiceType == 0)
          {
            channel.EraseRawData();
            list.Channels.Remove(channel);
            log.AppendFormat("{0} channel at index {1} (Pr# {2} \"{3}\") was erased due to invalid service type 0\r\n",
                             list.ShortCaption, channel.RecordIndex, channel.OldProgramNr, channel.Name);
          }
        }
      }
    }

    private void RemoveDuplicateAnalogList(StringBuilder log)
    {
      if (this.avbtChannels.Count == 0 || this.avbcChannels.Count == 0)
        return;
      // TODO
    }

    #endregion

    // ------- testing -----------

    internal string Series { get { return c.series; } }
    internal int AnalogChannelLength { get { return c.avbtChannelLength; } }
    internal int DigitalChannelLength { get { return c.dvbtChannelLength; } }
    internal int SatChannelLength { get { return c.dvbsChannelLength; } }
    internal int HdPlusChannelLength { get { return c.hdplusChannelLength; } }
    internal int SatDatabaseVersion { get; private set; }
  }
}
