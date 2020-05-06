using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Linq;
using System.Text;
using ChanSort.Api;

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

    private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT, "Analog Air");
    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC, "Analog Cable");
    private readonly ChannelList avbxChannels = new ChannelList(SignalSource.AnalogCT, "Analog Air/Cable");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "Digital Air");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC, "Digital Cable");
    private readonly ChannelList dvbxChannels = new ChannelList(SignalSource.DvbCT, "Digital Air/Cable");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS, "Satellite");
    private readonly ChannelList primeChannels = new ChannelList(SignalSource.CablePrimeD, "Cable Prime");
    private readonly ChannelList hdplusChannels = new ChannelList(SignalSource.HdPlusD, "Astra HD+");
    private readonly ChannelList freesatChannels = new ChannelList(SignalSource.FreesatD, "Freesat");
    private readonly ChannelList tivusatChannels = new ChannelList(SignalSource.TivuSatD, "TivuSat");
    private readonly ChannelList canalDigitalChannels = new ChannelList(SignalSource.CanalDigitalSatD, "Canal Digital Sat");
    private readonly ChannelList digitalPlusChannels = new ChannelList(SignalSource.DigitalPlusD, "Canal+ Digital");
    private readonly ChannelList cyfraPlusChannels = new ChannelList(SignalSource.CyfraPlusD, "Cyfra+ Digital");
    
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
      this.Features.DeleteMode = DeleteMode.FlagWithPrNr;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.CleanUpChannelData = true;
      this.Features.EncryptedFlagEdit = true;
    }
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
      this.UnzipFileToTempFolder();
      
      DetectModelConstants(this.TempPath);
      Features.SupportedFavorites = c.supportedFavorites;
      Features.SortedFavorites = c.SortedFavorites == FavoritesIndexMode.IndividuallySorted;

      ReadAnalogFineTuning(this.TempPath);
      ReadAnalogChannels(this.TempPath, "map-AirA", this.avbtChannels, out this.avbtFileContent, this.avbtFrequency);
      ReadAnalogChannels(this.TempPath, "map-CableA", this.avbcChannels, out this.avbcFileContent, this.avbcFrequency);
      ReadAnalogChannels(this.TempPath, "map-AirCableMixedA", this.avbxChannels, out this.avbxFileContent, this.avbcFrequency);
      ReadDvbTransponderFrequenciesFromPtc(this.TempPath, "PTCAIR", this.dvbtFrequency);
      ReadDvbServiceProviders(this.TempPath);
      ReadDvbctChannels(this.TempPath, "map-AirD", this.dvbtChannels, out this.dvbtFileContent, this.dvbtFrequency);
      ReadDvbTransponderFrequenciesFromPtc(this.TempPath, "PTCCABLE", this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-CableD", this.dvbcChannels, out this.dvbcFileContent, this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-AirCableMixedD", this.dvbxChannels, out this.dvbxFileContent, this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-CablePrime_D", this.primeChannels, out this.primeFileContent, this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-FreesatD", this.freesatChannels, out this.freesatFileContent, this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-TivusatD", this.tivusatChannels, out this.tivusatFileContent, this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-CanalDigitalSatD", this.canalDigitalChannels, out this.canalDigitalFileContent, this.dvbcFrequency);
      ReadDvbctChannels(this.TempPath, "map-DigitalPlusD", this.digitalPlusChannels, out this.digitalPlusFileContent, this.dvbcFrequency);
      ReadSatellites(this.TempPath);
      ReadTransponder(this.TempPath, "UserTransponderDataBase.dat"); // read user data first so it has priority over overridden default transponsers
      ReadTransponder(this.TempPath, "TransponderDataBase.dat");
      ReadDvbsChannels(this.TempPath, "map-SateD", this.dvbsChannels, out this.dvbsFileContent, c.dvbsChannelLength);
      ReadDvbsChannels(this.TempPath, "map-CyfraPlusD", this.cyfraPlusChannels, out this.cyfraPlusFileContent, c.cyfraPlusChannelSize);
      ReadAstraHdPlusChannels(this.TempPath);        


      foreach (var list in this.DataRoot.ChannelLists)
      {
        if (list.VisibleColumnFieldNames == null)
          continue;
        list.VisibleColumnFieldNames.Add("PcrPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        if ((list.SignalSource & SignalSource.Sat) != 0)
        {
          var idx = list.VisibleColumnFieldNames.IndexOf("FreqInMhz");
          list.VisibleColumnFieldNames.Insert(idx+1, "Polarity");
        }
      }
    }
    #endregion

    #region DetectModelConstants()
    internal void DetectModelConstants(string tempDir)
    {
      if (DetectModelFromFileName()) return;
      if (DetectModelFromCloneInfoFile(tempDir)) return;
      if (DetectModelFromContentFileLengths(tempDir)) return;
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
            //var letter = match.Groups[2].Value;
            // E, F, H and some J models use same file format
            series = "E";
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
    private bool DetectModelFromCloneInfoFile(string tempDir)
    {
      byte[] cloneInfo = ReadFileContent(tempDir, "CloneInfo");
      if (cloneInfo == null)
      {
        this.c = this.modelConstants["Series:B"];
        return true;
      }

      if (cloneInfo.Length >= 9)
      {
        char series = (char) cloneInfo[8];
        if (series == 'B') // LTxxBxxx uses 1201 format. The 2009 B-models have no CloneInfo file, so we can tell the difference
          series = 'E';
        else if (series == 'C') // "C" usually means 1001 format, but there some with 1201 format: LTxxCxxx, HExxCxxx, ... so we can't decide here
          return false; 
        else if ("EFHJ".Contains(series)) // E, F, H, some J
          series = 'E'; 
        if (this.modelConstants.TryGetValue("Series:" + series, out this.c))
          return true;
      }
      return false;
    }
    #endregion

    #region DetectModelFromContentFileLengths()
    private bool DetectModelFromContentFileLengths(string zip)
    {
      string[] candidates = {
                              DetectModelFromAirAOrCableA(zip),
                              DetectModelFromAirDOrCableD(zip),
                              DetectModelFromSateD(zip),
                              DetectModelFromTranspoderDatabase(zip),
                              DetectModelFromAstraHdPlusD(zip)
                            };

      // E, F, H, some J and a few others use an identical format, which we all treat as "E"
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

      char series;
      if (validCandidates.Length == 1)
        series = validCandidates[0];
      else
      {
        using (var dlg = Api.View.Default?.CreateActionBox(""))
        {
          if (dlg == null) // during unit testing
            return false;
          dlg.Message = "File type could not be detected automatically.\nPlease choose the model series of your TV:";
          foreach(var cand in validCandidates)
            dlg.AddAction("Series " + cand, cand);
          dlg.AddAction("Cancel", 0);
          dlg.ShowDialog();
          if (dlg.SelectedAction == 0)
            return false;
          series = (char)dlg.SelectedAction;
        }
      }

      this.modelConstants.TryGetValue("Series:" + series, out this.c);
      return true;
    }

    #endregion

    #region GetFileInfo()
    private FileInfo GetFileInfo(string zipFolder, params string[] fileNames)
    {
      foreach (var fileName in fileNames)
      {
        var path = Path.Combine(zipFolder, fileName);
        var info = new FileInfo(path);
        if (info.Exists)
          return info;
      }

      return null;
    }
    #endregion

    #region DetectModelFromAirAOrCableA()
    private string DetectModelFromAirAOrCableA(string zip)
    {
      var info = GetFileInfo(zip, "map-AirA", "map-CableA");
      if (info == null)
        return null;

      var candidates = "";
      if (info.Length % 28000 == 0)
        candidates += "B";
      if (info.Length % 40000 == 0)
        candidates += "C";
      if (info.Length % 64000 == 0)
        candidates += "DE";
      return candidates;
    }
    #endregion

    #region DetectModelFromAirDOrCableD()
    private string DetectModelFromAirDOrCableD(string zip)
    {
      var entry = GetFileInfo(zip, "map-AirD", "map-CableD", "map-CablePrime_D", "map-FreesatD",
        "map-TivusatD", "map-CanalDigitalSatD", "map-DigitalPlusD");
      if (entry == null)
        return null;

      var candidates = "";
      if (entry.Length % 248 == 0)
        candidates += "B";
      if (entry.Length % 292 == 0)
        candidates += "C";
      if (entry.Length % 320 == 0)
        candidates += "DE";
      return candidates;
    }
    #endregion

    #region DetectModelFromSateD()
    private string DetectModelFromSateD(string zip)
    {
      var entry = this.GetFileInfo(zip, "map-SateD");
      if (entry == null)
        return null;

      var candidates = "";
      if (entry.Length % 144 == 0)
        candidates += "BC";
      if (entry.Length % 172 == 0)
        candidates += "D";
      if (entry.Length % 168 == 0)
        candidates += "E";
      return candidates;
    }
    #endregion

    #region DetectModelFromTranspoderDatabase()
    private string DetectModelFromTranspoderDatabase(string zip)
    {
      var entry = GetFileInfo(zip, "TransponderDatabase.dat");
      if (entry == null)
        return null;

      var size = entry.Length - 4;
      var candidates = "";
      if (size%49 == 0)
        candidates += "B";
      if (size%45 == 0)
        candidates += "CDE";
      return candidates;
    }
    #endregion

    #region DetectModelFromAstraHdPlusD()
    private string DetectModelFromAstraHdPlusD(string zip)
    {
      var entry = GetFileInfo(zip, "map-AstraHDPlusD");
      if (entry == null)
        return null;

      var size = entry.Length;
      string candidates = null;
      if (size % 212 == 0)
        candidates += "DE";
      return candidates;
    }
    #endregion


    #region ReadAnalogFineTuning()
    private void ReadAnalogFineTuning(string zip)
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
    private void ReadAnalogChannels(string zip, string fileName, ChannelList list, out byte[] data, Dictionary<int,decimal> freq)
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
    private void ReadDvbTransponderFrequenciesFromPtc(string zip, string file, IDictionary<int, decimal> table)
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
    private void ReadDvbServiceProviders(string zip)
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
    private void ReadDvbctChannels(string zip, string fileName, ChannelList list, out byte[] data, Dictionary<int, decimal> frequency)
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
        DigitalChannel ci = new DigitalChannel(slotIndex, source, rawChannel, this.DataRoot, frequency, c.SortedFavorites, this.serviceProviderNames);
        if (ci.InUse)
          this.DataRoot.AddChannel(list, ci);

        rawChannel.BaseOffset += entrySize;
      }
    }
    #endregion


    #region ReadSatellites()
    private void ReadSatellites(string zip)
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

    private void ReadTransponder(string zip, string fileName)
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
        transponder.Polarity = mapping.GetByte("offPolarity") == 0 ? 'H' : 'V';
        this.DataRoot.AddTransponder(sat, transponder);
      }
    }
    #endregion

    #region ReadDvbsChannels()
    private void ReadDvbsChannels(string zip, string filename, ChannelList channels, out byte[] fileContent, int entrySize)
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
    private void ReadAstraHdPlusChannels(string zip)
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
    private static byte[] ReadFileContent(string tempDir, string fileName)
    {
      var path = Path.Combine(tempDir, fileName);
      return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }
    #endregion

    #region Save()
    public override void Save(string tvOutputFile)
    {
      string zip = this.TempPath;
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
      this.ZipToOutputFile(tvOutputFile);
    }
    #endregion

    #region SaveChannels()
    private void SaveChannels(string zip, string fileName, ChannelList channels, byte[] fileContent)
    {
      if (fileContent == null)
        return;

      string tempFilePath = Path.Combine(zip, fileName);
      using (var stream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
      {
        this.WriteChannels(channels, fileContent, stream);
      }
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(ChannelList list, byte[] fileContent, FileStream stream)
    {
      foreach (var channel in list.Channels)
        channel.UpdateRawData();

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
      //if (this.avbtChannels.Count == 0 || this.avbcChannels.Count == 0)
      //  return;
      // TODO
    }

    #endregion

    // ------- testing -----------

    internal string Series => c.series;
    internal int AnalogChannelLength => c.avbtChannelLength;
    internal int DigitalChannelLength => c.dvbtChannelLength;
    internal int SatChannelLength => c.dvbsChannelLength;
    internal int HdPlusChannelLength => c.hdplusChannelLength;
    internal int SatDatabaseVersion { get; private set; }
  }
}
