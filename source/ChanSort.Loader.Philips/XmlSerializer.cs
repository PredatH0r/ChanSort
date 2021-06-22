﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
{
  /*
    This loader supports 2 different kinds of XML files from Philips, the first in a "Repair" folder, the others in a "ChannelMap_100" (or later) folder

    Example from Repair\CM_TPM1013E_LA_CK.xml:
    <Channel>
    <Setup oldpresetnumber="1" presetnumber="1" name="Das Erste" ></Setup>
    <Broadcast medium="dvbc" frequency="410000" system="west" serviceID="1" ONID="41985" TSID="1101" modulation="256" symbolrate="6901000" bandwidth="Unknown"></Broadcast>
    </Channel>
     

   	<Channel>
    <Setup SatelliteName="0x54 0x00 0x55 0x00 0x52 0x00 0x4B 0x00 0x53 0x00 0x41 0x00 0x54 0x00 0x20 0x00 0x34 0x00 0x32 0x00 0x45 0x00 " ChannelNumber="1" ChannelName="0x54 0x00 0xC4 0x00 0xB0 0x00 0x56 0x00 0xC4 0x00 0xB0 0x00 0x42 0x00 0x55 0x00 0x20 0x00 0x53 0x00 0x50 0x00 0x4F 0x00 0x52 0x00 " ChannelLock="0" UserModifiedName="0" LogoID="0" UserModifiedLogo="0" LogoLock="0" UserHidden="0" FavoriteNumber="0" />
    <Broadcast ChannelType="3" Onid="1070" Tsid="43203" Sid="16001" Frequency="11794" Modulation="0" ServiceType="1" SymbolRate="27507" LNBNumber="38" Polarization="0" SystemHidden="0" />
    </Channel>
    
    Example from a ChannelMap_100\ChannelList\channellib\DVBC.xml:
    <Channel>
    <Setup ChannelNumber="2" ChannelName="0x50 0x00 0x61 0x00 0x72 0x00 0x61 0x00 0x6D 0x00 0x6F 0x00 0x75 0x00 0x6E 0x00 0x74 0x00 0x20 0x00 0x43 0x00 0x68 0x00 0x61 0x00 0x6E 0x00 0x6E 0x00 0x65 0x00 0x6C 0x00 " LogoID="0" ChannelLock="0" UserModifiedName="0" UserModifiedLogo="0" LogoLock="0" UserHidden="0" FavoriteNumber="0" Scramble="0"></Setup>
    <Broadcast ChannelType="3" Onid="1" Tsid="104" Sid="357" Frequency="386000000" Modulation="64" ServiceType="1" Bandwidth="8" SymbolRate="6900" DecoderType="2" SubType="0" NetworkID="0" StreamPriority="0" SystemHidden="0"></Broadcast>
    </Channel>
    
    Example from a ChannelMap_105\ChannelList\s2channellib\DVBS.xml:
    <Channel>
    <Setup SatelliteName="0x41 0x00 0x53 0x00 0x54 0x00 0x52 0x00 0x41 0x00 0x20 0x00 0x31 0x00 0x39 0x00 0x2E 0x00 0x32 0x00 0x45 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00" ChannelNumber="45" ChannelName="0x31 0x00 0x2D 0x00 0x32 0x00 0x2D 0x00 0x33 0x00 0x2E 0x00 0x74 0x00 0x76 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00" ChannelLock="0" UserModifiedName="0" LogoID="1441" UserModifiedLogo="0" LogoLock="0" UserHidden="0" FavoriteNumber="45" Scrambled="0"></Setup>
    <Broadcast UniqueID="14" ChannelType="3" Onid="133" Tsid="5" Sid="1" Frequency="12460" Modulation="8-VSB" ServiceType="1" SymbolRate="27500" LNBNumber="1" Polarization="1" SystemHidden="0"></Broadcast>
    </Channel>

    Example from a ChannelMap_110\ChannelList\channellib\DVBC.xml:
    <Channel>
    <Setup ChannelNumber="1" ChannelName="0x44 0x00 0x61 0x00 0x73 0x00 0x20 0x00 0x45 0x00 0x72 0x00 0x73 0x00 0x74 0x00 0x65 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00" ChannelLock="0" UserModifiedName="0" LogoID="0" UserModifiedLogo="0" LogoLock="0" UserHidden="0" FavoriteNumber="0" Scrambled="0"></Setup>
    <Broadcast UniqueID="12" ChannelType="3" Onid="1" Tsid="1101" Sid="1" Frequency="306" Modulation="256" ServiceType="1" Bandwidth="8" SymbolRate="6901000" DecoderType="2" NetworkID="43264" StreamPriority="0" SystemHidden="0"></Broadcast>
    </Channel>



    DVB-T and DVB-C share the same number range, so they are treated as a unified logical list

   */
  class XmlSerializer : SerializerBase
  {
    private readonly ChannelList analogChannels = new ChannelList(SignalSource.AnalogCT, "Analog C/T");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC, "DVB-C");
    private readonly ChannelList satChannels = new ChannelList(SignalSource.DvbS, "DVB-S");
    private readonly ChannelList allSatChannels = new ChannelList(SignalSource.DvbS, "DVB-S all");
    private readonly ChannelList favChannels = new ChannelList(SignalSource.All, "Favorites");

    private readonly List<FileData> fileDataList = new List<FileData>();
    private ChanLstBin chanLstBin;
    private readonly StringBuilder logMessages = new StringBuilder();
    private readonly IniFile ini;


    #region ctor()
    public XmlSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSaveAs = false;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = true;

      this.DataRoot.AddChannelList(this.analogChannels);
      this.DataRoot.AddChannelList(this.dvbtChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.satChannels);
      this.DataRoot.AddChannelList(this.allSatChannels);
      this.DataRoot.AddChannelList(this.favChannels);

      this.dvbtChannels.VisibleColumnFieldNames.Add("Source");
      this.dvbcChannels.VisibleColumnFieldNames.Add("Source");

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("PcrPid");
        list.VisibleColumnFieldNames.Remove("VideoPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("ShortName");
        list.VisibleColumnFieldNames.Remove("Provider");
      }

      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.OriginalNetworkId));
      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.TransportStreamId));
      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceId));
      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.SymbolRate));
      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ChannelOrTransponder));
      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.NetworkName));
      this.analogChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.NetworkOperator));


      this.favChannels.IsMixedSourceFavoritesList = true;

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      // read all files from a directory structure that looks like
      // ./CM_TPM1013E_LA_CK.xml
      // - or - 
      // ChannelMap_100/ChannelList/channellib/DVBC.xml
      // ChannelMap_100/ChannelList/channellib/DVBT.xml
      // ChannelMap_100/ChannelList/s2channellib/DVBS.xml
      // ChannelMap_100/ChannelList/s2channellib/DVBSall.xml
      // ChannelMap_100/ChannelList/chanLst.bin
      // + optionally
      // ChannelMap_100/ChannelList/channelFile.bin
      // ChannelMap_100/ChannelList/Favorite.xml
      // ChannelMap_100/ChannelList/satInfo.bin

      var dataFiles = new[] { @"channellib\DVBC.xml", @"channellib\DVBT.xml", @"s2channellib\DVBS.xml", @"s2channellib\DVBSall.xml", @"Favorite.xml" };

      // support for files in a ChannelMap_xxx directory structure
      bool isChannelMapFolderStructure = false;
      var dir = Path.GetDirectoryName(this.FileName);
      var dirName = Path.GetFileName(dir).ToLower();
      if (dirName == "channellib" || dirName == "s2channellib")
      {
        dir = Path.GetDirectoryName(dir);
        isChannelMapFolderStructure = true;
      }

      var binFile = Path.Combine(dir, "chanLst.bin");  // the .bin file is used as a proxy for the whole directory structure
      if (File.Exists(binFile))
      {
        this.FileName = binFile;
        isChannelMapFolderStructure = true;
        this.chanLstBin = new ChanLstBin();
        this.chanLstBin.Load(this.FileName, msg => this.logMessages.AppendLine(msg));
      }
      else if (Path.GetExtension(this.FileName).ToLower() == ".bin")
      {
        // older Philips models export a visible file like Repair\CM_T911_LA_CK.BIN and an invisible (hidden+system) .xml file with the same name
        var xmlPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(this.FileName) + ".xml");
        if (File.Exists(xmlPath))
        {
          try { File.SetAttributes(xmlPath, FileAttributes.Archive);}
          catch { /**/ }
          this.FileName = xmlPath;
        }
      }

      if (isChannelMapFolderStructure)
      {
        foreach (var file in dataFiles)
        {
          var fullPath = Path.GetFullPath(Path.Combine(dir, file));
          this.LoadFile(fullPath);
        }
        if (this.fileDataList.Count == 0)
          throw new FileLoadException("No XML files found in folder structure");
      }
      else
      {
        // otherwise load the single file that was originally selected by the user
        LoadFile(this.FileName);
      }
    }
    #endregion

    #region LoadFile()

    private void LoadFile(string fileName)
    {
      if (!File.Exists(fileName))
        return;

      // skip read-only files (like hidden read-only DVBSall.xml on a Philips 24PFS5535 from 2020 (along with)
      var info = new FileInfo(fileName);
      if ((info.Attributes & FileAttributes.ReadOnly) != 0)
      {
        this.logMessages.AppendLine($"Skipping read-only file {fileName}");
        return;
      }

      bool fail = false;
      var fileData = new FileData();
      try
      {
        fileData.path = fileName;
        fileData.doc = new XmlDocument();
        fileData.content = File.ReadAllBytes(fileName);
        fileData.textContent = Encoding.UTF8.GetString(fileData.content);
        fileData.newline = fileData.textContent.Contains("\r\n") ? "\r\n" : "\n";

        // indentation can be 0, 2 or 4 spaces
        var idx1 = fileData.textContent.IndexOf("<Channel>");
        var idx0 = fileData.textContent.LastIndexOf("\n", idx1+1);
        if (idx0 >= 0 && idx1 >= 0)
          fileData.indent = fileData.textContent.Substring(idx0 + 1, idx1 - idx0 - 1);
        else
          fileData.indent = fileData.textContent.Contains("  <") ? "  " : "";

        var settings = new XmlReaderSettings
        {
          CheckCharacters = false,
          IgnoreProcessingInstructions = true,
          ValidationFlags = XmlSchemaValidationFlags.None,
          DtdProcessing = DtdProcessing.Ignore
        };
        using (var reader = XmlReader.Create(new StringReader(fileData.textContent), settings))
        {
          fileData.doc.Load(reader);
        }
      }
      catch
      {
        fail = true;
      }

      var root = fileData.doc.FirstChild;
      if (root is XmlDeclaration)
        root = root.NextSibling;
      if (fail || root == null || (root.LocalName != "ChannelMap" && root.LocalName != "FavoriteListMAP"))
        throw new FileLoadException("\"" + fileName + "\" is not a supported Philips XML file");

      int rowId = 0;
      ChannelList curList = null;
      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "Channel":
            if (rowId == 0)
              curList = this.DetectFormatAndFeatures(fileData, child);
            if (curList != null)
              this.ReadChannel(fileData, curList, child, rowId++);
            break;
          case "FavoriteList":
            this.ReadFavList(child);
            break;
        }
      }
      this.fileDataList.Add(fileData);
    }
    #endregion

    #region DetectFormatAndFeatures()

    private ChannelList DetectFormatAndFeatures(FileData file, XmlNode node)
    {
      var setupNode = node["Setup"] ?? throw new FileLoadException("Missing Setup XML element");
      var bcastNode = node["Broadcast"] ?? throw new FileLoadException("Missing Broadcast XML element");

      var fname = Path.GetFileNameWithoutExtension(file.path).ToLower();
      var medium = bcastNode.GetAttribute("medium");
      if (medium == "" && fname.Length >= 4 && fname.StartsWith("dvb"))
        medium = fname;
      bool hasEncrypt = false;

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("ServiceType");
        list.VisibleColumnFieldNames.Add("ServiceTypeName");
      }

      if (setupNode.HasAttribute("ChannelName"))
      {
        file.formatVersion = 1;
        this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
        this.Features.MaxFavoriteLists = 1;

        var dtype = bcastNode.GetAttribute("DecoderType");
        if (dtype == "1")
          medium = "dvbt";
        else if (dtype == "2")
          medium = "dvbc";
        else if (dtype == "3")
          medium = "dvbs";

        hasEncrypt = setupNode.HasAttribute("Scrambled");
      }
      else if (setupNode.HasAttribute("name"))
      {
        file.formatVersion = 2;
        this.Features.FavoritesMode = FavoritesMode.None;
        foreach (var list in this.DataRoot.ChannelLists)
        {
          list.VisibleColumnFieldNames.Remove("Favorites");
          list.VisibleColumnFieldNames.Remove("Lock");
          list.VisibleColumnFieldNames.Remove("Hidden");
          list.VisibleColumnFieldNames.Remove("ServiceTypeName");
          list.VisibleColumnFieldNames.Remove("Encrypted");
        }
      }
      else
        throw new FileLoadException("Unknown data format");

      ChannelList chList = null;
      switch (medium)
      {
        case "analog":
          chList = this.analogChannels;
          break;
        case "dvbc":
          chList = this.dvbcChannels;
          break;
        case "dvbt":
          chList = this.dvbtChannels;
          break;
        case "dvbs":
          chList = this.satChannels;
          break;
        case "dvbsall":
          chList = this.allSatChannels;
          break;
      }

      if (!hasEncrypt)
        chList?.VisibleColumnFieldNames.Remove("Encrypted");

      return chList;
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(FileData file, ChannelList curList, XmlNode node, int rowId)
    {
      var setupNode = node["Setup"] ?? throw new FileLoadException("Missing Setup XML element");
      var bcastNode = node["Broadcast"] ?? throw new FileLoadException("Missing Broadcast XML element");
      var data = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var n in new[] {setupNode, bcastNode})
      {
        foreach(XmlAttribute attr in n.Attributes)
          data.Add(attr.LocalName, attr.Value);
      }

      if (!data.ContainsKey("UniqueID") || !int.TryParse(data["UniqueID"], out var uniqueId)) // UniqueId only exists in ChannelMap_105 and later
        uniqueId = rowId;
      var chan = new Channel(curList.SignalSource & SignalSource.MaskAdInput, rowId, uniqueId, setupNode);
      chan.OldProgramNr = -1;
      chan.IsDeleted = false;
      if (file.formatVersion == 1)
        this.ParseChannelFormat1(data, chan);
      else if (file.formatVersion == 2)
        this.ParseChannelFormat2(data, chan);

      if ((chan.SignalSource & SignalSource.MaskAdInput) == SignalSource.DvbT)
        chan.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(chan.FreqInMhz).ToString();
      else if ((chan.SignalSource & SignalSource.MaskAdInput) == SignalSource.DvbC)
        chan.ChannelOrTransponder = LookupData.Instance.GetDvbcChannelName(chan.FreqInMhz);

      DataRoot.AddChannel(curList, chan);
    }
    #endregion

    #region ParseChannelFormat1
    private void ParseChannelFormat1(Dictionary<string,string> data, Channel chan)
    {
      chan.Format = 1;
      chan.RawSatellite = data.TryGet("SatelliteName");
      chan.Satellite = DecodeName(chan.RawSatellite);
      chan.OldProgramNr = ParseInt(data.TryGet("ChannelNumber"));
      chan.RawName = data.TryGet("ChannelName");
      chan.Name = DecodeName(chan.RawName);
      chan.Lock = data.TryGet("ChannelLock") == "1";
      chan.Hidden = data.TryGet("UserHidden") == "1";
      var fav = ParseInt(data.TryGet("FavoriteNumber"));
      chan.SetOldPosition(1, fav == 0 ? -1 : fav);
      chan.OriginalNetworkId = ParseInt(data.TryGet("Onid"));
      chan.TransportStreamId = ParseInt(data.TryGet("Tsid"));
      chan.ServiceId = ParseInt(data.TryGet("Sid"));
      chan.FreqInMhz = ParseInt(data.TryGet("Frequency")); ;
      if (chan.FreqInMhz > 2000 && (chan.SignalSource & SignalSource.Sat) == 0)
        chan.FreqInMhz /= 1000;
      
      var st = ParseInt(data.TryGet("ServiceType"));
      chan.ServiceTypeName = st == 1 ? "TV" : "Radio";
      if (st == 1)
        chan.SignalSource |= SignalSource.Tv;
      else
        chan.SignalSource |= SignalSource.Radio;

      var decoderType = data.TryGet("DecoderType");
      if (decoderType == "1")
        chan.Source = "DVB-T";
      else if (decoderType == "2")
        chan.Source = "DVB-C";
      chan.SignalSource |= LookupData.Instance.IsRadioTvOrData(chan.ServiceType);
      chan.SymbolRate = ParseInt(data.TryGet("SymbolRate"));
      if (chan.SymbolRate > 100000) // DVB-S stores values in kSym, DVB-C stores it in Sym, DVB-T stores 0
        chan.SymbolRate /= 1000;
      if (data.TryGetValue("Polarization", out var pol))
        chan.Polarity = pol == "0" ? 'H' : 'V';
      chan.Hidden |= data.TryGet("SystemHidden") == "1";

      chan.Encrypted = data.TryGet("Scrambled") == "1"; // introduced in ChannelMap_105 format
    }
    #endregion

    #region ParseChannelFormat2
    private void ParseChannelFormat2(Dictionary<string, string> data, Channel chan)
    {
      chan.Format = 2;
      chan.OldProgramNr = ParseInt(data.TryGet("presetnumber"));
      chan.Name = data.TryGet("name");
      chan.RawName = chan.Name;
      chan.FreqInMhz = ParseInt(data.TryGet("frequency"));
      //if ((chan.SignalSource & SignalSource.Analog) != 0) // analog channels have some really strange values (e.g. 00080 - 60512) that I can't convert to a plausible freq range (48-856 MHz)
      //  chan.FreqInMhz /= 16;
      if (chan.FreqInMhz > 1200 && (chan.SignalSource & SignalSource.Sat) == 0)
        chan.FreqInMhz /= 1000;
      chan.ServiceId = ParseInt(data.TryGet("serviceID"));
      chan.OriginalNetworkId = ParseInt(data.TryGet("ONID"));
      chan.TransportStreamId = ParseInt(data.TryGet("TSID"));
      chan.ServiceType = ParseInt(data.TryGet("serviceType"));
      chan.SymbolRate = ParseInt(data.TryGet("symbolrate")) / 1000;
    }
    #endregion

    #region ReadFavList
    private void ReadFavList(XmlNode node)
    {
      int index = ParseInt(node.Attributes["Index"].InnerText);
      string name = DecodeName(node.Attributes["Name"].InnerText);
      this.Features.FavoritesMode = FavoritesMode.MixedSource;
      this.Features.MaxFavoriteLists = Math.Max(this.Features.MaxFavoriteLists, index);

      this.favChannels.SetFavListCaption(index - 1, name);

      if (this.favChannels.Count == 0)
      {
        foreach (var rootList in this.DataRoot.ChannelLists)
        {
          if (rootList.IsMixedSourceFavoritesList)
            continue;
          foreach (var chan in rootList.Channels)
          {
            favChannels.Channels.Add(chan);
            for (int i=0; i<this.DataRoot.FavListCount; i++)
              chan.SetOldPosition(i+1, -1);
          }
        }
      }

      foreach (XmlNode child in node.ChildNodes)
      {
        if (child.LocalName == "FavoriteChannel")
        {
          var uniqueId = ParseInt(child["UniqueID"].InnerText);
          var favNumber = ParseInt(child["FavNumber"].InnerText);
          var chan = this.favChannels.Channels.FirstOrDefault(ch => ch.RecordIndex == uniqueId);
          chan?.SetOldPosition(index, favNumber + 1);
        }
      }
    }
    #endregion

    #region DecodeName()
    private string DecodeName(string input)
    {
      if (input == null || !input.StartsWith("0x")) // fallback for unknown input
        return input;

      var hexParts = input.Split(' ');
      var buffer = new MemoryStream();

      foreach (var part in hexParts)
      {
        if (part == "")
          continue;
        buffer.WriteByte((byte)ParseInt(part));
      }

      return Encoding.Unicode.GetString(buffer.GetBuffer(), 0, (int) buffer.Length).TrimEnd('\x0');
    }
    #endregion

    #region GetDataFilePaths()
    public override IEnumerable<string> GetDataFilePaths()
    {
      return this.fileDataList.Select(f => f.path);
    }
    #endregion



    #region Save()

    public override void Save(string tvOutputFile)
    {
      // "Save As..." is not supported by this loader

      foreach (var list in this.DataRoot.ChannelLists)
      {
        if (list.IsMixedSourceFavoritesList)
          this.UpdateFavList();
        else
          this.UpdateChannelList(list);
      }

      foreach (var file in this.fileDataList)
      {
        if (Path.GetFileName(file.path).ToLowerInvariant().StartsWith("dvb"))
          this.ReorderNodes(file);
        this.SaveFile(file);
      }

      this.chanLstBin?.Save(this.FileName);
    }

    #endregion

    #region UpdateChannelList()
    private void UpdateChannelList(ChannelList list)
    {
      var sec = ini.GetSection("Map" + (this.chanLstBin?.VersionMajor ?? 0));
      var setFavoriteNumber = sec?.GetBool("setFavoriteNumber", false) ?? false;

      foreach (var channel in list.Channels)
      {
        var ch = channel as Channel;
        if (ch == null)
          continue; // might be a proxy channel from a reference list

        if (ch.IsDeleted || ch.NewProgramNr < 0)
        {
          ch.SetupNode.ParentNode.ParentNode.RemoveChild(ch.SetupNode.ParentNode);
          continue;
        }

        if (ch.Format == 1)
          this.UpdateChannelFormat1(ch, setFavoriteNumber);
        else if (ch.Format == 2)
          this.UpdateChannelFormat2(ch);
      }
    }
    #endregion

    #region UpdateChannelFormat1 and 2
    private void UpdateChannelFormat1(Channel ch, bool setFavoriteNumber)
    {
      ch.SetupNode.Attributes["ChannelNumber"].Value = ch.NewProgramNr.ToString();

      if (ch.IsNameModified)
      {
        ch.SetupNode.Attributes["ChannelName"].InnerText = EncodeName(ch.Name, (ch.SetupNode.Attributes["ChannelName"].InnerText.Length + 1) / 5, true);
        var attr = ch.SetupNode.Attributes["UserModifiedName"];
        if (attr != null)
          attr.InnerText = "1";
      }

      // ChannelMap_100 supports a single fav list and stores the favorite number directly here in the channel.
      // ChannelMap_105 and later always store the value 0 in the channel and instead use a separate Favorites.xml file.
      ch.SetupNode.Attributes["FavoriteNumber"].Value = setFavoriteNumber ? Math.Max(ch.GetPosition(1), 0).ToString() : "0";

      if (ch.OldProgramNr != ch.NewProgramNr)
      {
        var attr = ch.SetupNode.Attributes["UserReorderChannel"]; // introduced with format 110, but not always present
        if (attr != null)
          attr.InnerText = "1";
      }
    }

    private void UpdateChannelFormat2(Channel ch)
    {
      ch.SetupNode.Attributes["presetnumber"].Value = ch.NewProgramNr.ToString();
      if (ch.IsNameModified)
        ch.SetupNode.Attributes["name"].Value = ch.Name;
    }
    #endregion

    #region UpdateFavList
    private void UpdateFavList()
    {
      var favFile = this.fileDataList.FirstOrDefault(fd => Path.GetFileName(fd.path).ToLower() == "favorite.xml");
      if (favFile == null)
        return;

      int index = 0;
      foreach(XmlNode favListNode in favFile.doc["FavoriteListMAP"].ChildNodes)
      {
        ++index;
        favListNode.InnerXml = ""; // clear all <FavoriteChannel> child elements but keep the attributes of the current node
        var attr = favListNode.Attributes?["Name"];
        if (attr != null)
          attr.InnerText = EncodeName(this.favChannels.GetFavListCaption(index - 1), (attr.InnerText.Length + 1)/5, false);

        // increment fav list version, unless disabled in .ini file
        if (chanLstBin != null && (ini.GetSection("Map" + chanLstBin.VersionMajor)?.GetBool("incrementFavListVersion", true) ?? true))
        {
          attr = favListNode.Attributes?["Version"];
          if (attr != null && int.TryParse(attr.Value, out var version))
            attr.InnerText = (version + 1).ToString();
        }

        foreach (var ch in favChannels.Channels.OrderBy(ch => ch.GetPosition(index)))
        {
          var nr = ch.GetPosition(index);
          if (nr <= 0)
            continue;
          var uniqueIdNode = favFile.doc.CreateElement("UniqueID");
          uniqueIdNode.InnerText = ch.RecordIndex.ToString();
          var favNrNode = favFile.doc.CreateElement("FavNumber");
          favNrNode.InnerText = (nr-1).ToString();
          var channelNode = favFile.doc.CreateElement("FavoriteChannel");
          channelNode.AppendChild(uniqueIdNode);
          channelNode.AppendChild(favNrNode);
          favListNode.AppendChild(channelNode);
        }
      }
    }
    #endregion

    #region EncodeName
    private string EncodeName(string name, int numBytes, bool upperCaseHexDigits)
    {
      var bytes = Encoding.Unicode.GetBytes(name);
      var sb = new StringBuilder();
      var pattern = upperCaseHexDigits ? "0x{0:X2} " : "0x{0:x2} ";
      for (int i = 0; i < numBytes - 2; i++)
      {
        var b = i < bytes.Length ? bytes[i] : 0;
        sb.AppendFormat(pattern, b);
      }

      sb.Append("0x00 0x00"); // always add an end-of-string
      return sb.ToString();
    }
    #endregion

    #region ReorderNodes
    private void ReorderNodes(FileData file)
    {
      if (file.formatVersion != 1)
        return;

      var nodes = file.doc.DocumentElement.GetElementsByTagName("Channel");
      var list = new List<XmlElement>();
      foreach(var node in nodes)
        list.Add((XmlElement)node);
      foreach (var node in list)
        file.doc.DocumentElement.RemoveChild(node);
      foreach(var node in list.OrderBy(elem => int.Parse(elem["Setup"].Attributes["ChannelNumber"].InnerText)))
        file.doc.DocumentElement.AppendChild(node);
    }
    #endregion

    #region SaveFile()
    private void SaveFile(FileData file)
    {
      // by default .NET reformats the whole XML. These settings produce almost same format as the TV xml files use
      var xmlSettings = new XmlWriterSettings();
      xmlSettings.Encoding = new UTF8Encoding(false);
      xmlSettings.CheckCharacters = false;
      xmlSettings.Indent = true;
      xmlSettings.IndentChars = file.indent;
      xmlSettings.NewLineHandling = NewLineHandling.None;
      xmlSettings.NewLineChars = file.newline;
      xmlSettings.OmitXmlDeclaration = true;

      string xml;
      using (var sw = new StringWriter())
      {
        // write unmodified XML declaration (the DVB*.xml files use a different one than the Favorite.xml file)
        var i = file.textContent.IndexOf("?>");
        if (i >= 0)
          sw.Write(file.textContent.Substring(0, i + 2 + file.newline.Length));

        using (var w = new CustomXmlWriter(sw, xmlSettings, false))
        {
          file.doc.WriteTo(w);
          w.Flush();
          xml = sw.ToString();
        }
      }

      // append trailing newline, if the original file had one
      if (file.textContent.EndsWith(file.newline) && !xml.EndsWith(file.newline))
        xml += file.newline;

      var enc = new UTF8Encoding(false, false);
      File.WriteAllText(file.path, xml, enc);
    }
    #endregion

    public override string GetFileInformation()
    {
      return base.GetFileInformation() + this.logMessages.Replace("\n", "\r\n");
    }

    #region class FileData
    private class FileData
    {
      public string path;
      public XmlDocument doc;
      public byte[] content;
      public string textContent;
      public string newline;
      public string indent;
      public int formatVersion;
    }
    #endregion
  }
}
