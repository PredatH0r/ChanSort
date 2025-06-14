﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;
using ChanSort.Loader.MediaTek;

namespace ChanSort.Loader.Philips
{
  /*
    This loader supports 2 different kinds of XML files from Philips, the first in a "Repair" folder, the others in a "ChannelMap_100" (or later) folder.
    TVs from the "Tornado" brand use a variation of the first (CM_*.xml) format, which wrap the ChannelMap within an extra "TvContents" root element and has 
    some minor differences within "Broadcast" and "Setup" elements.

    Example from Repair\CM_TPM1013E_LA_CK.xml:
    This "Repair" format comes with a visible .BIN file and a .xml file that has file system attributes "hidden" and "system".
    The TV seems to use the .BIN file as its primary source for setting up the internal list and then applies the .xml on top of it to reorder channels.
    It uses the "oldpresetnumber" from the XML to lookup the channel from the .BIN file and then apply the new "presetnumber".
    The .BIN file itself is compressed with some unknown cl_Zip compression / archive format and can't be edited with ChanSort.
    Deleting a channel is not possible by modifiying the .xml file. Omitting a channel only results in duplicate numbers with the TV still showing the missing channels at their old numbers.
    The channel nodes in the .XML must be kept in the original order with "oldpresetnumber" keeping the original value and only "presetnumber" being updated.
    There are (at least) two versions of this format. One starts with <ChannelMap> as the root node, one has a <ECSM> root node wrapping <ChannelMap> and other elements.

    <Channel>
    <Setup oldpresetnumber="1" presetnumber="1" name="Das Erste" ></Setup>
    <Broadcast medium="dvbc" frequency="410000" system="west" serviceID="1" ONID="41985" TSID="1101" modulation="256" symbolrate="6901000" bandwidth="Unknown"></Broadcast>
    </Channel>
  
    <Channel>
    <Setup presetnumber="1" name="Das Erste HD" ></Setup>
    <Broadcast medium="dvbs" satellitename="Astra1F-1L 19.2E" frequency="11494" system="west" serviceID="2604" ONID="1" TSID="1019" modulation="8-VSB" symbolrate="22000"></Broadcast>
    </Channel>

  
    Example from a Tornado __CHTB_DO_NOT_DELETE_.xml file:
    <Channel>
			<Broadcast medium="Dvbt" frequency="177500" system="" serviceID="1" ONID="2807" TSID="1" modulation="0" symbolrate="0" bandwidth="7MHz" servicetype="TV" />
			<Setup presetnumber="1" name="Aloula Ardhia" MajorChanNO="1" Attribute="1107361792" blank="0" skip="0" FreePKG="0" PayPKG1="0" PayPKG2="0" />
    </Channel>


    Newer channel lists from Philips contain multiple XML files with a different internal structure, which also varies based on the version number in the ChannelMap_xxx folder name.
    The official Philips Channel Editor 6.61.22 supports the binary file format 1.1 and 1.2 as well as the XML file format "ChannelMap_100" (but not 45, 105 nor 110).
    That editor keeps the channel lines in the .xml file in their original order but changes the ChannelNumber attribute so that the first record may have number 2, the second record may have 1.
    Other than that the Philips editor has some breaking modifications to the XML, e.g. replacing Modulation="8-VSB" with Modulation="8". 
    It adds indentation to the XML elements, changes hex digits to uppercase and adds 4 bytes to the SatelliteName (from 42 to 46).
    The Philips editor updates the checksums in chanLst.bin (but that file does not include the DVBS.xml file, only DVBT.xml, DVBC.xml an DVBSall.xml)
    
    The ChannelMap_100 formats can also be edited with Onka editor. Unlike the Philips editor, this one sorts the XML nodes by their new ChannelNumber. 
    Onka can also read/write formats 105 and 110, but removes all XML attributes that it doesn't know (and didn't exist in 100), like "Scramble"/"Scrambled" and "UserReorderChannel". 
    It adds an XML namespace, indentation, uses short closing tags and removes the <SatelliteListcopy> element from format 105/110.
    Onka does not update chanLst.bin (which isn't required when only DVBS.xml is modified since that file has no checksum in chanLst.bin)
    Nevertheless a user reported that swapping DVB-S channels 1 and 2 with Onka on a TV that uses this xml-only format 110 worked for him.

    There seem to be 3 different flavors of the "100" format:
    One has only .xml files in the channellib and s2channellib folders, does not indent lines in the .xml files, has a fixed number of bytes for channel and satellite names (padded with 0x00), has no "Scramble" attribute and values 1 and 0 for "Polarization".
    And a version that has dtv_cmdb_*.bin next to the .xml files, uses 4 spaces for indentation, only writes as many bytes for names as needed, has a "Scramble" attribute and uses values 1 and 2 for "Polarization". 
    While the first seems to work fine when XML nodes are reordered by their new programNr, the latter seems to get confused when the .bin and .xml files have different data record orders. This is still under investigation. 
    The Philips editor does not modify these .bin files, appends 0x00 padding to the channel names, changes indentation to 2 tabs and strips the Scramble attribute. It's likely it wasn't designed for this type of list.
  

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


    Version 120 and 125 (MediaTek platform for Google TVs) contain an additional MtkChannelList.xml file, which is also used by other brands.
    This file contains both a readable channel list as XML as well as a base64 encoded Java serialized Blob containing a cl_Zip compressed/encrypted binary channel list.
    A separate Loader module is used for this file to keep data in-sync.

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
    private IniFile.Section iniMapSection;
    private string polarizationValueForHorizontal = "1";
    private MediaTek.Serializer mtkSerializer;

    #region ctor()
    public XmlSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = true;

      this.DataRoot.AddChannelList(this.analogChannels);
      this.DataRoot.AddChannelList(this.dvbtChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.satChannels);
      this.DataRoot.AddChannelList(this.allSatChannels);
      //this.DataRoot.AddChannelList(this.favChannels); // format 100.0 does not support mixed source favs and adding it would automatically switch to mixed source fav mode

      this.dvbtChannels.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));
      this.dvbcChannels.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.PcrPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.VideoPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Skip));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
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
      // ChannelMap_105/ChannelList/Favorite.xml
      // ChannelMap_100/ChannelList/satInfo.bin
      // ChannelMap_120/ChannelList/MtkChannelList.xml

      var dataFiles = new[] { @"channellib\DVBC.xml", @"channellib\DVBT.xml", @"s2channellib\DVBS.xml", @"s2channellib\DVBSall.xml", @"Favorite.xml" };

      // support for files in a ChannelMap_xxx directory structure
      bool isChannelMapFolderStructure = false;
      var dir = Path.GetDirectoryName(this.FileName);
      var dirName = Path.GetFileName(dir).ToLowerInvariant();
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
        this.TvModelName = this.chanLstBin.ModelName;
        this.FileFormatVersion = $"{chanLstBin.VersionMajor}.{chanLstBin.VersionMinor}";
      }
      else if (Path.GetExtension(this.FileName).ToLowerInvariant() == ".bin")
      {
        // older Philips models export a visible file like Repair\CM_T911_LA_CK.BIN and an invisible (hidden+system) .xml file with the same name
        var xmlPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(this.FileName) + ".xml");
        if (File.Exists(xmlPath))
        {
          try { File.SetAttributes(xmlPath, FileAttributes.Archive);}
          catch(Exception ex) { this.logMessages.AppendLine("Failed to reset file attributes for " + xmlPath + ": " + ex.Message); }
          this.FileName = xmlPath;
          var baseName = Path.GetFileNameWithoutExtension(xmlPath).ToUpperInvariant();
          if (baseName.StartsWith("CM_"))
            baseName = baseName.Substring(3);
          this.TvModelName = baseName;
          this.FileFormatVersion = "Legacy XML";
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
          throw LoaderException.TryNext("No XML files found in folder structure");

        LoadAndValidateMtkChannelList(dir);

        // version 120 and 125 ignore the modified main channel numbers and only take changes from the favorites.xml
        //if ((chanLstBin?.VersionMajor ?? 0) >= 120)
        //{
        //  foreach (var list in this.DataRoot.ChannelLists)
        //  {
        //    if (!list.IsMixedSourceFavoritesList)
        //      list.ReadOnly = true;
        //  }
        //}
      }
      else
      {
        // otherwise load the single file that was originally selected by the user
        LoadFile(this.FileName);
      }

      if (this.Features.FavoritesMode == FavoritesMode.MixedSource)
        this.DataRoot.AddChannelList(this.favChannels);
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
        var idx = fileData.textContent.IndexOf('\n');
        fileData.newline = idx < 0 ? "" : idx > 0 && fileData.textContent[idx-1] == '\r' ? "\r\n" : "\n"; // there are Repair\*.xml files with <ECSM> root that use \n normally but contain a \n\r\n near the end

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

        var fn = Path.GetFileName(fileName).ToLowerInvariant();
        fileData.formatVersion = // first guess, will be set based on file content later
          fn.StartsWith("cm_") ? FormatVersion.RepairXml 
            : fn.StartsWith("__chtb") ? FormatVersion.TornadoChtb
            : FormatVersion.ChannelMapXml; 

        var xml = fileData.textContent;
        if (fileData.formatVersion == FormatVersion.RepairXml)
          xml = xml.Replace("&", "&amp;"); // Philips exports broken XML with unescaped & instead of &amp;
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        fileData.doc.Load(reader);
      }
      catch
      {
        fail = true;
      }

      var root = fileData.doc.FirstChild;
      if (root is XmlDeclaration)
        root = root.NextSibling;
      if (root?.LocalName == "ECSM")
        root = root.FirstChild;
      if (root?.LocalName == "TvContents")
        root = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n?.LocalName == "ChannelMap");
      if (fail || root == null || (root.LocalName != "ChannelMap" && root.LocalName != "FavoriteListMAP"))
        throw LoaderException.TryNext("\"" + fileName + "\" is not a supported Philips XML file");

      int rowId = 0;
      ChannelList curList = null;
      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "Channel":
            if (rowId == 0 || fileData.formatVersion == FormatVersion.TornadoChtb)
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
      var setupNode = node["Setup"] ?? throw LoaderException.Fail("Missing Setup XML element");
      var bcastNode = node["Broadcast"] ?? throw LoaderException.Fail("Missing Broadcast XML element");

      var fname = Path.GetFileNameWithoutExtension(file.path).ToLowerInvariant();
      var medium = bcastNode.GetAttribute("medium");
      if (medium == "" && fname.Length >= 4 && fname.StartsWith("dvb"))
        medium = fname;
      bool hasEncrypt = false;

      var ver = this.chanLstBin?.VersionMajor ?? 0;

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("ServiceType");
        list.VisibleColumnFieldNames.Add("ServiceTypeName");
      }

      if (setupNode.HasAttribute("name"))
      {
        file.formatVersion = setupNode.HasAttribute("MajorChanNO") ? FormatVersion.TornadoChtb: FormatVersion.RepairXml;
        if (file.formatVersion == FormatVersion.TornadoChtb)
          file.indent = "\t";
        this.iniMapSection = ini.GetSection("Repair_xml");
        this.Features.FavoritesMode = FavoritesMode.None;
        foreach (var list in this.DataRoot.ChannelLists)
        {
          list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Favorites));
          list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Lock));
          list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Hidden));
          list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceType));
          list.VisibleColumnFieldNames.Add("-" + nameof(ChannelInfo.ServiceTypeName));
          list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Encrypted));
        }
      }
      else if (setupNode.HasAttribute("ChannelName"))
      {
        file.formatVersion = FormatVersion.ChannelMapXml;
        this.Features.FavoritesMode = FavoritesMode.Flags;
        this.Features.MaxFavoriteLists = 1;

        var dtype = bcastNode.GetAttribute("DecoderType");
        if (dtype == "1")
          medium = "dvbt";
        else if (dtype == "2")
          medium = "dvbc";
        else if (dtype == "3")
          medium = "dvbs";

        hasEncrypt = setupNode.HasAttribute("Scramble") || setupNode.HasAttribute("Scrambled");
      }
      else
        throw LoaderException.Fail("Unknown data format");

      ChannelList chList = null;
      switch (medium.ToLowerInvariant())
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

      if (ver > 0)
        this.iniMapSection = ini.GetSection("Map" + ver);

      if (ver >= 105)
        this.Features.FavoritesMode = FavoritesMode.OrderedPerSource; // will be overridden when a Favorite.xml file is found
      else if (ver == 100)
      {
        // use special configs for version 100 variants
        var dir = Path.GetDirectoryName(this.FileName) ?? "";
        if (File.Exists(Path.Combine(dir, "atv_cmdb.bin")))
        {
          this.iniMapSection = ini.GetSection("Map100_cmdb.bin");
          this.FileFormatVersion += "/cmdb";
          this.polarizationValueForHorizontal = "1"; // TODO validate
        }
        else if (File.Exists(Path.Combine(dir, "channelFile.bin")))
        {
          this.iniMapSection = ini.GetSection("Map100_channelFile.bin");
          this.FileFormatVersion += "/channelFile";
        }

        if (this.iniMapSection?.GetBool("setReorderedFavNumber") ?? false)
          this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      }

      if (this.iniMapSection?.GetBool("allowDelete", false) ?? false)
        this.Features.DeleteMode = DeleteMode.Physically;

      return chList;
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(FileData file, ChannelList curList, XmlNode node, int rowId)
    {
      var setupNode = node["Setup"] ?? throw LoaderException.Fail("Missing Setup XML element");
      var bcastNode = node["Broadcast"] ?? throw LoaderException.Fail("Missing Broadcast XML element");
      var data = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var n in new[] {setupNode, bcastNode})
      {
        foreach(XmlAttribute attr in n.Attributes)
          data.Add(attr.LocalName, attr.Value);
      }

      if (!data.ContainsKey("UniqueID") || !long.TryParse(data["UniqueID"], out var uniqueId)) // UniqueId only exists in ChannelMap_105 and later
        uniqueId = rowId;
      var chan = new Channel(curList.SignalSource & SignalSource.MaskBcast, rowId, uniqueId, setupNode);
      chan.OldProgramNr = -1;
      chan.IsDeleted = false;
      if (file.formatVersion == FormatVersion.RepairXml || file.formatVersion == FormatVersion.TornadoChtb)
        this.ParseRepairXml(data, chan);
      else if (file.formatVersion == FormatVersion.ChannelMapXml)
        this.ParseChannelMapXml(data, chan);

      if ((chan.SignalSource & SignalSource.MaskBcast) == SignalSource.DvbT)
        chan.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(chan.FreqInMhz).ToString();
      else if ((chan.SignalSource & SignalSource.MaskBcast) == SignalSource.DvbC)
        chan.ChannelOrTransponder = LookupData.Instance.GetDvbcTransponder(chan.FreqInMhz).ToString();

      DataRoot.AddChannel(curList, chan);
    }
    #endregion

    #region ParseRepairXml()
    private void ParseRepairXml(Dictionary<string, string> data, Channel chan)
    {
      chan.Format = 1;
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
      chan.SymbolRate = ParseInt(data.TryGet("symbolrate"));
      if (chan.SymbolRate > 100000) // DVB-C/T specify it in Sym/s, DVB-S in kSym/sec
        chan.SymbolRate /= 1000;
      chan.Satellite = data.TryGet("satellitename")?.TrimEnd('@', '\n', '\r'); // the satellitename can have a "\n@" at the end
      chan.ServiceTypeName = data.TryGet("servicetype"); // only exists in Tornado CHTB format
    }
    #endregion

    #region ParseChannelMapXml()
    private void ParseChannelMapXml(Dictionary<string,string> data, Channel chan)
    {
      chan.Format = 2;
      chan.RawSatellite = data.TryGet("SatelliteName");
      chan.Satellite = DecodeName(chan.RawSatellite, NameType.Satellite);
      chan.OldProgramNr = ParseInt(data.TryGet("ChannelNumber"));
      chan.RawName = data.TryGet("ChannelName");
      chan.Name = DecodeName(chan.RawName, NameType.Channel);
      chan.Lock = data.TryGet("ChannelLock") == "1";
      chan.Hidden = data.TryGet("UserHidden") == "1"; // can be "3" instead of "0", at least in format 120
      var fav = ParseInt(data.TryGet("FavoriteNumber"));
      chan.SetOldPosition(1, fav == 0 ? -1 : fav);
      chan.OriginalNetworkId = ParseInt(data.TryGet("Onid"));
      chan.TransportStreamId = ParseInt(data.TryGet("Tsid"));
      chan.ServiceId = ParseInt(data.TryGet("Sid"));
      chan.FreqInMhz = ParseInt(data.TryGet("Frequency")); ;
      if (chan.FreqInMhz > 2000 && (chan.SignalSource & SignalSource.Sat) == 0)
        chan.FreqInMhz /= 1000;

      // version 120 stores actual DVB ServiceType values, earlier versions only 1=TV, 2=Radio
      var st = ParseInt(data.TryGet("ServiceType"));
      if (chanLstBin != null && chanLstBin.VersionMajor >= 120)
      {
        chan.ServiceType = st;
        chan.ServiceTypeName = LookupData.Instance.GetServiceTypeDescription(st);
        chan.SignalSource |= LookupData.Instance.IsRadioTvOrData(st);
      }
      else
      {
        chan.ServiceTypeName = st == 1 ? "TV" : "Radio";
        if (st == 1)
          chan.SignalSource |= SignalSource.Tv;
        else
          chan.SignalSource |= SignalSource.Radio;
      }

      chan.Source = (chan.SignalSource & SignalSource.Sat) != 0 ? "DVB-S" : (chan.SignalSource & SignalSource.Cable) != 0 ? "DVB-C" : (chan.SignalSource & SignalSource.Antenna) != 0 ? "DVB-T" : "";
      chan.SignalSource |= LookupData.Instance.IsRadioTvOrData(chan.ServiceType);
      chan.SymbolRate = ParseInt(data.TryGet("SymbolRate"));
      if (chan.SymbolRate > 100000) // DVB-S stores values in kSym, DVB-C stores it in Sym, DVB-T stores 0
        chan.SymbolRate /= 1000;
      if (data.TryGetValue("Polarization", out var pol))
        chan.Polarity = pol == polarizationValueForHorizontal ? 'H' : 'V';
      chan.Hidden |= data.TryGet("SystemHidden") == "1";

      chan.Encrypted = data.TryGet("Scramble") == "1" || data.TryGet("Scrambled") == "1"; // v100 sometimes contains a "Scramble", v105/v110 always contain "Scrambled"
    }
    #endregion

    #region ReadFavList()
    private void ReadFavList(XmlNode node)
    {
      int index = ParseInt(node.Attributes["Index"].InnerText);
      string name = DecodeName(node.Attributes["Name"].InnerText, NameType.FavList);
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
              chan.SetOldPosition(i + 1, -1);
          }
        }
      }

      int favNr = 0;
      foreach (XmlNode child in node.ChildNodes)
      {
        if (child.LocalName == "FavoriteChannel")
        {
          var uniqueId = ParseLong(child["UniqueID"].InnerText);
          //var favNumber = ParseDecimal(child["FavNumber"].InnerText); // this is a Decimal like "1.5" when the previous "0" was moved behind "1", which makes the new list start at 1 instead of 2
          var chan = this.favChannels.Channels.FirstOrDefault(ch => ch.RecordIndex == uniqueId && ch.GetOldPosition(index) <= 0);
          chan?.SetOldPosition(index, ++favNr);
        }
      }
    }
    #endregion

    #region DecodeName()
    private string DecodeName(string input, NameType nameType)
    {
      if (input == null || !input.StartsWith("0x")) // fallback for unknown input
        return input;

      // The Philips encodes names is a complete mess.
      // Two hex digits are combined to an integer with the low byte first and the high byte second.
      // If the high byte is 0xFF, it is treated as if it were 0x00.
      // If any of the high bytes is non-zero, the encoding is little-endian utf16.
      // If all high bytes are 0, some guesswork is needed to decode the sequence of odd bytes. It can be encoded as
      // - a valid utf8 sequence
      // - 1 byte per character in some undetermined locale
      // - 1 byte per character mixed with 2 bytes big-endian utf16 (v125)

      // according to https://github.com/PredatH0r/ChanSort/issues/347 Philips seems use a locale dependent encoding for favorite list names,
      // writing "0xAA 0x00" to the file for an 8 bit code point. Congratulations, well done!

      // Version 100: CAN use little-endian UTF16: 0x11 0x04 0x35 0x04 0x3B 0x04 0x30 0x04 0x40 0x04 0x43 0x04 0x41 0x04 0x4C 0x04 0x20 0x00 0x31 0x00 0x20 0x00 0x48 0x00 0x44 0x00 for "Беларусь 1 HD"
      // also 100: odd bytes contain UTF8, high are all 0: https://github.com/PredatH0r/ChanSort/issues/421:
      //   0x38 0x00 0x20 0x00 0xD0 0x00 0xBA 0x00 0xD0 0x00 0xB0 0x00 0xD0 0x00 0xBD 0x00 0xD0 0x00 0xB0 0x00 0xD0 0x00 0xBB 0x00 0x20 0x00 0x48 0x00 0x44 0x00 for "8 канал HD"

      // Version 120/125: Umlauts in channel names are encoded as 1 byte CP-1252/UTF16 code point + 0xFF as the second byte (i.e. for "Ä" it is 0xC4 0xFF instead of 0xC4 0x00)
      // Also 125: 0x62 0x00 0x65 0x00 0x49 0x00 0x4e 0x00 0x20 0x00 0x01 0x00 0x30 0x00 0x5a for "beIN İZ" where 0x01 0x00 0x30 0x00 refers to U+0130 "İ"


      var utf16 = new MemoryStream();
      var utf8 = new MemoryStream();

      bool isHighByte = false;
      int intValue = 0;
      bool invalidUtf8 = false;
      byte bigEndianUnicodeHighByte = 0;
      bool isBigEndianUtf16InOddBytes = false;
      var hexParts = input.Split(' ');
      foreach (var part in hexParts)
      {
        if (part == "")
          continue;
        var curByte = (byte)ParseInt(part);
        
        if (!isHighByte)
        {
          intValue = curByte;
          isHighByte = true;
          continue;
        }

        isHighByte = false;
        if (curByte == 0xff) // hack-around for version 120 where 0xFFxx is actually a CP1252 code point xx
          curByte = 0;

        intValue += curByte << 8;
        invalidUtf8 |= curByte != 0;

        if (intValue == 0) // break when reaching a 0x00 0x00 sequence
          break;

        if (!invalidUtf8)
          utf8.WriteByte((byte)intValue);

        if (isBigEndianUtf16InOddBytes) // special handling when a character < 32 was detected, which means we may have a "HI 00 LO 00" encoding for a UTF16 character
        {
          if (curByte == 0) // expected case where LO is followed by 00
          {
            utf16.WriteByte((byte)intValue);
            utf16.WriteByte(bigEndianUnicodeHighByte);
            bigEndianUnicodeHighByte = 0;
          }
          else // fallback to write full 4 byte sequence
          {
            utf16.WriteByte(bigEndianUnicodeHighByte);
            utf16.WriteByte(0);
            utf16.WriteByte((byte)(intValue >> 8));
            utf16.WriteByte(curByte);
          }

          isBigEndianUtf16InOddBytes = false;
        }
        else
        {
          if (intValue < 32) // an int < 32 is likely the high byte of a "HI 00 LO 00" encoded UTF16 character
          {
            isBigEndianUtf16InOddBytes = true;
            bigEndianUnicodeHighByte = (byte)intValue;
            invalidUtf8 = true;
          }
          else
          {
            utf16.WriteByte((byte)(intValue & 0xFF));
            utf16.WriteByte(curByte);
          }
        }
      }

      // in the FavList the name can be a random locale based on the country setting (other than CP-1252 or U-0000-00FF, i.e. turkish)
      if (nameType == NameType.FavList)
        return this.DefaultEncoding.GetString(utf8.GetBuffer(), 0, (int)utf8.Length).TrimGarbage();

      // best-effort utf8 decoding
      if (!invalidUtf8 && Tools.IsUtf8(utf8.GetBuffer(), 0, (int)utf8.Length))
        return Encoding.UTF8.GetString(utf8.GetBuffer(), 0, (int)utf8.Length).TrimGarbage();

      return Encoding.Unicode.GetString(utf16.GetBuffer(), 0, (int) utf16.Length).TrimGarbage();
    }
    #endregion

    #region LoadAndValidateMtkChannelList()
    private void LoadAndValidateMtkChannelList(string dir)
    {
      var path = Path.Combine(dir, "MtkChannelList.xml");
      if (!File.Exists(path))
        return;
      
      this.mtkSerializer = new Serializer(path);
      this.mtkSerializer.Load();
      foreach (var list1 in this.DataRoot.ChannelLists)
      {
        if (list1.Channels.Count == 0 || (list1.SignalSource & SignalSource.Analog) != 0)
          continue;

        var list2 = mtkSerializer.DataRoot.GetChannelList(list1.SignalSource);
        if (list2 == null)
          throw LoaderException.Fail("MtkChannelList.xml doesn't contain a list for " + list1.SignalSource);

        if (list1.Channels.Count != list2.Channels.Count)
          throw LoaderException.Fail("MtkChannelList.xml contains a different number of channels for " + list1.SignalSource);

        foreach (var ch1 in list1.Channels)
        {
          var others = list2.GetChannelByUid(ch1.Uid).Where(c => Math.Abs(ch1.FreqInMhz - c.FreqInMhz) < 2).ToList();
          if (others.Count == 0)
            throw LoaderException.Fail("MtkChannelList.xml doesn't contain a matching channel for " + ch1);
          if (others.Count > 1)
          {
            others = others.Where(c => c.OldProgramNr == ch1.OldProgramNr).ToList();
            if (others.Count != 1)
              throw LoaderException.Fail("MtkChannelList.xml contains multiple matching channel for " + ch1);
          }

          var ch2 = others[0];
          if (ch1.OldProgramNr != ch2.OldProgramNr)
            throw LoaderException.Fail("MtkChannelList.xml contains a different channel number for " + ch1);

          ((Channel)ch1).MtkChannel = ch2;
        }
      }
    }
    #endregion

    #region GetDataFilePaths()
    public override IEnumerable<string> GetDataFilePaths()
    {
      var list = new List<string>();
      if (this.chanLstBin != null)
        list.Add(this.FileName);
      list.AddRange(this.fileDataList.Select(f => f.path));

      if (this.mtkSerializer != null)
        list.AddRange(this.mtkSerializer.GetDataFilePaths());

      return list;
    }
    #endregion



    #region Save()

    public override void Save()
    {
      bool readOnly = false;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        if (list.IsMixedSourceFavoritesList)
          this.UpdateFavList();
        else
        {
          this.UpdateChannelList(list);
          readOnly |= list.ReadOnly;
        }
      }

      // It is unclear whether XML nodes must be sorted by the new program number or kept in the original order. This may be different for the various format versions.
      // Onka, which was made for the ChannelMap_100 flavor that doesn't export dtv_cmdb_2.bin files, reorders the XML nodes and users reported that it works.
      // The official Philips Editor 6.61.22 does not reorder the XML nodes and does not change dtv_cmdb_*.bin when editing a ChannelMap_100 folder. But it is unclear if this editor is designed to handle the cmdb flavor.

      // A user with a ChannelMap_100 export including a dtv_cmdb_2.bin reported, that the TV shows the reordered list in the menu, but tunes the channels based on the original numbers.
      // It's unclear if that happens because the XML was reordered and out-of-sync with the .bin, or if the TV always uses the .bin for tuning and XML edits are moot.
      // On top of that this TV messed up Umlauts during the import, despite ChanSort writing the exact same name data in hex-encoded UTF16. The result was as if the string was exported as UTF-8 bytes and then parsed with an 8-bit code page.
      var reorderNodes = this.iniMapSection?.GetBool("reorderRecordsByChannelNumber") ?? false;

      foreach (var file in this.fileDataList)
      {
        if (reorderNodes && (file.formatVersion == FormatVersion.RepairXml || Path.GetFileName(file.path).ToLowerInvariant().StartsWith("dvb")))
          this.ReorderNodes(file);

        this.SaveFile(file);
      }

      this.chanLstBin?.Save(this.FileName);

      // save the MtkChannelList.xml for format 120 and 125
      if (mtkSerializer != null && !readOnly)
      {
        foreach (var list in mtkSerializer.DataRoot.ChannelLists) // apply Philips' LCN-ReadOnly to MediaTek's ReadOnly
          list.ReadOnly = false;
        this.mtkSerializer.Save();
      }
    }

    #endregion

    #region UpdateChannelList()
    private void UpdateChannelList(ChannelList list)
    {
      if (list.ReadOnly)
        return;

#if false
      // ensure consecutive numbers for channels (seems to be a requirement for successful import of MtkChannelList.xml)
      if (this.chanLstBin?.VersionMajor is >= 120 and <= 125)
      {
        var ordered = list.Channels
          .Where(c => !c.IsProxy)
          .OrderBy(c => (c.SignalSource & SignalSource.Tv) != 0 ? 0 : 1)
          .ThenBy(c => c.NewProgramNr > 0 ? 0 : 1)
          .ThenBy(c => c.NewProgramNr)
          .ThenBy(c => c.OldProgramNr)
          .ToList();
        int i = 0;
        foreach (var ch in ordered)
        {
          if (ch.IsProxy)
            list.Channels.Remove(ch);
          else
            ch.NewProgramNr = ++i;
        }
      }
#endif

      var padChannelNameBytes = this.iniMapSection?.GetBool("padChannelName", true) ?? true;
      var setFavoriteNumber = this.iniMapSection?.GetBool("setFavoriteNumber", false) ?? false;
      var userReorderChannel = this.iniMapSection?.GetString("userReorderChannel") ?? "";
      var uppercaseHexDigits = !(this.iniMapSection?.GetBool("dvbXmlLowercaseHexDigits", false) ?? false);

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
          this.UpdateRepairXml(ch);
        else if (ch.Format == 2)
        {
          this.UpdateChannelMapXml(ch, padChannelNameBytes, setFavoriteNumber, userReorderChannel, uppercaseHexDigits);
          if (ch.MtkChannel != null)
            this.UpdateMtkChannel(ch);
        }
      }
    }
#endregion

    #region UpdateRepairXml()

    private void UpdateRepairXml(Channel ch)
    {
      ch.SetupNode.Attributes["presetnumber"].Value = ch.NewProgramNr.ToString();

      if (ch.SetupNode.Attributes["MajorChanNO"] != null) // only in the Tornado CHTB format
        ch.SetupNode.Attributes["MajorChanNO"].Value = ch.NewProgramNr.ToString();

      if (ch.IsNameModified)
        ch.SetupNode.Attributes["name"].Value = ch.Name;
    }
    #endregion

    #region UpdateChannelMapXml()
    private void UpdateChannelMapXml(Channel ch, bool paddedName, bool setFavoriteNumber, string userReorderChannel, bool uppercaseHexDigits)
    {
      var setup = ch.SetupNode.Attributes;
      setup["ChannelNumber"].Value = ch.NewProgramNr.ToString();

      if (ch.IsNameModified)
      {

        setup["ChannelName"].InnerText = EncodeName(ch.Name, 50, paddedName, uppercaseHexDigits);
        var attr = setup["UserModifiedName"];
        if (attr != null)
          attr.InnerText = "1";
      }

      setup["ChannelLock"].Value = ch.Lock ? "1" : "0";
      setup["UserHidden"].Value = ch.Hidden ? "1" : iniMapSection.GetInt("userHiddenDefaultValue", 0).ToString();

      // ChannelMap_100 supports a single fav list and stores the favorite number directly in the channel.
      // The official Philips editor allows to reorder favorites when switched to the "Favourite" list view

      // ChannelMap_105 and later always store the value 0 and instead use a separate Favorites.xml file with mixed-source channels.
      setup["FavoriteNumber"].Value = setFavoriteNumber ? Math.Max(ch.GetPosition(1), 0).ToString() : "0";

      // "UserReorderChannel" was introduced with format 110, but not always present.
      // It is unclear if this should be 0, 1, or removed for the import to work.
      // One user reported a 110 format file edited with Onka (which basically reverted the file to format 100 and removed UserReorderChannel) worked for him, 
      // while my attempt with ChanSort and setting UserReorderChannel=1 didn't work. But maybe that was due to other factors.
      var urc = setup["UserReorderChannel"];
      if (urc != null)
      {
        if (userReorderChannel == "")
          userReorderChannel = "0";
        switch (userReorderChannel)
        {
          case "delete":
          case "remove":
            urc.OwnerElement?.RemoveAttributeNode(urc);
            break;
          case "auto":
            if (ch.OldProgramNr != ch.NewProgramNr)
              urc.InnerText = "1";
            break;
          default:
            urc.InnerText = userReorderChannel;
            break;
        }
      }
    }

    #endregion

    #region UpdateMtkChannel()
    private void UpdateMtkChannel(Channel channel)
    {
      var mtk = channel.MtkChannel;
      mtk.Name = channel.Name;
      mtk.NewProgramNr = channel.NewProgramNr;
      mtk.Lock = channel.Lock;
      mtk.Skip = channel.Skip;
      mtk.Hidden = channel.Hidden;
    }
    #endregion

    #region UpdateFavList()
    private void UpdateFavList()
    {
      var favFile = this.fileDataList.FirstOrDefault(fd => Path.GetFileName(fd.path).ToLowerInvariant() == "favorite.xml");
      if (favFile == null)
        return;

      int index = 0;
      foreach(XmlNode favListNode in favFile.doc["FavoriteListMAP"].ChildNodes)
      {
        ++index;
        favListNode.InnerXml = ""; // clear all <FavoriteChannel> child elements but keep the attributes of the current node
        var attr = favListNode.Attributes?["Name"];
        if (attr != null)
          attr.InnerText = EncodeName(this.favChannels.GetFavListCaption(index - 1), (attr.InnerText.Length + 1)/5 /* 64 */, true, false);

        // increment fav list version, unless disabled in .ini file
        if (chanLstBin != null && (ini.GetSection("Map" + chanLstBin.VersionMajor)?.GetBool("incrementFavListVersion", true) ?? true))
        {
          attr = favListNode.Attributes?["Version"];
          if (attr != null && int.TryParse(attr.Value, out var version))
            attr.InnerText = (version + 1).ToString();
        }

        var uniqueIdFormat = (chanLstBin?.VersionMajor ?? 0) == 120 ? "d10" : "d"; // v120 writes 10 digits as uniqueID, including leading zeros; v100,v110 and v125 don't

        int favNr = 0;
        foreach (var ch in favChannels.Channels.OrderBy(ch => ch.GetPosition(index)))
        {
          var nr = ch.GetPosition(index);
          if (nr <= 0)
            continue;
          var uniqueIdNode = favFile.doc.CreateElement("UniqueID");
          uniqueIdNode.InnerText = ch.RecordIndex.ToString(uniqueIdFormat);
          var favNrNode = favFile.doc.CreateElement("FavNumber");
          favNrNode.InnerText = (++favNr).ToString();
          var channelNode = favFile.doc.CreateElement("FavoriteChannel");
          channelNode.AppendChild(uniqueIdNode);
          channelNode.AppendChild(favNrNode);
          
          // Version 120 also stores a <ChannelType> element in the XML, but not 100, 110, 115, 125
          if ((chanLstBin?.VersionMajor ?? 0) == 120)
          {
            var chanTypeNode = favFile.doc.CreateElement("ChannelType");
            string type = null;

            if ((ch.SignalSource & SignalSource.Sat) != 0) type = "TYPE_DVB_S"; // observed
            else if ((ch.SignalSource & SignalSource.Cable) != 0) type = "TYPE_DVB_C"; // fairly sure
            else if ((ch.SignalSource & SignalSource.Antenna) != 0) type = "TYPE_DVB_T"; // could also be T2 maybe

            if (type != null)
            {
              chanTypeNode.InnerText = type;
              channelNode.AppendChild(chanTypeNode);
            }
          }
          
          favListNode.AppendChild(channelNode);
        }
      }
    }
    #endregion

    #region EncodeName()
    private string EncodeName(string name, int maxBytes, bool padBytes, bool upperCaseHexDigits)
    {
      var bytes = Encoding.Unicode.GetBytes(name);
      var sb = new StringBuilder();
      var pattern = upperCaseHexDigits ? "0x{0:X2} " : "0x{0:x2} ";
      var len = Math.Min(bytes.Length, padBytes ? maxBytes-2 : maxBytes); // when padding, always add a 0x00 0x00 end-of-string
      int i;
      
      for (i = 0; i < len; i++)
        sb.AppendFormat(pattern, bytes[i]);
      if (padBytes)
      {
        for (; i < maxBytes; i++)
          sb.Append("0x00 ");
        sb.Remove(sb.Length - 1, 1);
      }

      return sb.ToString();
    }
    #endregion

    #region ReorderNodes()
    private void ReorderNodes(FileData file)
    {
      var progNrAttrib = file.formatVersion == FormatVersion.RepairXml ? "presetnumber" : "ChannelNumber";

      var nodes = file.doc.DocumentElement.GetElementsByTagName("Channel");
      var list = new List<XmlElement>();
      foreach(var node in nodes)
        list.Add((XmlElement)node);
      foreach (var node in list)
        file.doc.DocumentElement.RemoveChild(node);
      foreach(var node in list.OrderBy(elem => int.Parse(elem["Setup"].Attributes[progNrAttrib].InnerText)))
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
      if (file.formatVersion == FormatVersion.RepairXml)
        xml = xml.Replace("&amp;", "&"); // Philips uses broken XML with unescaped & instead of &amp;

      var enc = new UTF8Encoding(false, false);
      File.WriteAllText(file.path, xml, enc);
    }
    #endregion

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      return base.GetFileInformation() 
             + this.logMessages.Replace("\n", "\r\n")
             + "\r\n\r\n" 
             + this.mtkSerializer?.GetFileInformation();
    }
    #endregion


    #region enum FormatVersion

    private enum FormatVersion
    {
      RepairXml = 1, ChannelMapXml = 2, TornadoChtb = 3
    }
    #endregion

    #region enum NameType
    private enum NameType { Channel, Satellite, FavList }
    #endregion

    #region class FileData
    private class FileData
    {
      public string path;
      public XmlDocument doc;
      public byte[] content;
      public string textContent;
      public string newline;
      public string indent;
      public FormatVersion formatVersion;
    }
    #endregion
  }
}
