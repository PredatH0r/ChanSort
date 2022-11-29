using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.Grundig
{
  class Serializer : SerializerBase
  {
    private readonly ChannelList terrChannels = new ChannelList(SignalSource.Antenna | SignalSource.TvAndData, "Antenna TV");
    private readonly ChannelList cableChannels = new ChannelList(SignalSource.Cable | SignalSource.TvAndData, "Cable TV");
    private readonly ChannelList satChannels = new ChannelList(SignalSource.Sat | SignalSource.TvAndData, "Satellite TV");
    private readonly ChannelList terrChannelsRadio = new ChannelList(SignalSource.Antenna | SignalSource.Radio, "Antenna Radio");
    private readonly ChannelList cableChannelsRadio = new ChannelList(SignalSource.Cable | SignalSource.Radio, "Cable Radio");
    private readonly ChannelList satChannelsRadio = new ChannelList(SignalSource.Sat | SignalSource.Radio, "Satellite Radio");

    private readonly List<FileData> fileDataList = new List<FileData>();
    private readonly StringBuilder logMessages = new StringBuilder();


    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = false;
      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      this.Features.MaxFavoriteLists = 4;


      this.DataRoot.AddChannelList(this.terrChannels);
      this.DataRoot.AddChannelList(this.cableChannels);
      this.DataRoot.AddChannelList(this.satChannels);
      this.DataRoot.AddChannelList(this.terrChannelsRadio);
      this.DataRoot.AddChannelList(this.cableChannelsRadio);
      this.DataRoot.AddChannelList(this.satChannelsRadio);

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceTypeName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.PcrPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.VideoPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
      }

      this.terrChannels.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));
      this.cableChannels.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));
      this.terrChannelsRadio.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));
      this.cableChannelsRadio.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));
    }
    #endregion

    #region Load()
    public override void Load()
    {
      // read all files from a directory structure that looks like
      // My_Channellist\dvbs_config.xml
      // My_Channellist\dvbc_config.xml
      // My_Channellist\dvbt_config.xml


      var dataFiles = new[] { "dvbt_config.xml", "dvbc_config.xml", "dvbs_config.xml" };
      var dir = Path.GetDirectoryName(this.FileName) ?? "";
      foreach (var file in dataFiles)
      {
        var fullPath = Path.GetFullPath(Path.Combine(dir, file));
        this.LoadFile(fullPath);
      }
      if (this.fileDataList.Count == 0)
        throw LoaderException.TryNext("No dvb*_config.xml files found in folder structure");
    }
    #endregion

    #region LoadFile()

    private void LoadFile(string fileName)
    {
      if (!File.Exists(fileName))
        return;
      bool fail = false;
      var fileData = new FileData();
      try
      {
        var content = File.ReadAllBytes(fileName);
        fileData.path = fileName;
        fileData.hasBom = content.Length >= 3 && content[0] == 0xef && content[1] == 0xbb && content[2] == 0xbf;
        var textContent = Encoding.UTF8.GetString(content, fileData.hasBom ? 3 : 0, content.Length - (fileData.hasBom ? 3 : 0));
        
        // some files contain unescaped characters like \x10, which causes XML parsing to fail
        var sb = new StringBuilder(textContent.Length);
        foreach (var ch in textContent)
        {
          if (ch < 32 && ch != '\n' && ch != '\r' && ch != '\t')
            sb.Append($"&#x{(int)ch:x2};");
          else
            sb.Append(ch);
        }
        textContent = sb.ToString();

        fileData.newline = textContent.Contains("\r\n") ? "\r\n" : "\n";
        fileData.indent = textContent.Contains("  <");
        fileData.doc = new XmlDocument();
        fileData.doc.PreserveWhitespace = true;

        var settings = new XmlReaderSettings
        {
          CheckCharacters = false,
          IgnoreProcessingInstructions = true,
          ValidationFlags = XmlSchemaValidationFlags.None,
          DtdProcessing = DtdProcessing.Ignore
        };
        using var reader = XmlReader.Create(new StringReader(textContent), settings);
        fileData.doc.Load(reader);
      }
      catch
      {
        fail = true;
      }

      var root = fileData.doc.FirstChild;
      if (root is XmlDeclaration)
        root = root.NextSibling;
      while (root.LocalName == "#whitespace")
        root = root.NextSibling;
      if (fail || root == null || root.LocalName != "CONFIG")
        throw LoaderException.Fail("\"" + fileName + "\" is not a supported Grundig XML file");

      int transponderId = 0;
      int chanId = 0;
      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "Digital":
            ReadDigitalChannels(child, transponderId, ref chanId);
            break;
          case "Analog":
            ReadAnalogChannels(child, ref chanId);
            break;
        }
      }
      this.fileDataList.Add(fileData);
    }
    #endregion

    #region ReadAnalogChannels

    private void ReadAnalogChannels(XmlNode analog, ref int chanId)
    {
      SignalSource src;
      var type = analog.Attributes?["type"]?.InnerText;
      switch (type)
      {
        case "DVBT_ANALOG":
          src = SignalSource.AnalogT;
          break;
        case "DVBC_ANALOG":
          src = SignalSource.AnalogC;
          break;
        default:
          logMessages.AppendFormat("skipped unsupported analog source type: " + type);
          return;
      }

      foreach (XmlNode service in analog["channels"].ChildNodes)
      {
        if (service.LocalName != "service")
          continue;

        var c = new Channel(src, chanId, chanId, service);
        c.Source = type;
        c.FreqInMhz = Int32.Parse(service.Attributes["frq"].InnerText) / 20m;
        c.Hidden = service.Attributes["hid"].InnerText == "1";
        ReadCommonChannelData(c, service);
        var list = this.DataRoot.GetChannelList(src);
        this.DataRoot.AddChannel(list, c);
      }
    }
    #endregion

    #region ReadDigitalChannels
    private void ReadDigitalChannels(XmlNode digital, int transponderId, ref int chanId)
    {
      SignalSource src;
      decimal freqDivider;
      var type = digital.Attributes?["type"]?.InnerText;
      switch (type)
      {
        case "DVBC":
          src = SignalSource.DvbC;
          freqDivider = 1000;
          break;
        case "DVBT":
          src = SignalSource.DvbT;
          freqDivider = 1000;
          break;
        case "DVBS":
          src = SignalSource.DvbS;
          freqDivider = 1;
          break;
        default:
          logMessages.AppendFormat("skipped unsupported digital source type: " + type);
          return;
      }

      var channels = digital["channels"];
      if (channels == null)
        return;
      foreach (XmlNode networkNode in channels.ChildNodes)
      {
        if (networkNode.LocalName != "network")
          continue;
        var provider = networkNode.Attributes["nwname"]?.InnerText;
        foreach (XmlNode mux in networkNode.ChildNodes)
        {
          if (mux.LocalName != "mux")
            continue;
          var t = CreateTransponder(mux, ref transponderId, freqDivider);
          foreach (XmlNode service in mux.ChildNodes)
          {
            if (service.LocalName != "service")
              continue;

            var c = CreateChannel(service, src, t, ref chanId);
            if (c == null)
              continue;
            c.Source = type;
            c.Provider = provider;

            var list = this.DataRoot.GetChannelList(c.SignalSource);
            this.DataRoot.AddChannel(list, c);
            ++chanId;
          }
        }
      }
    }
    #endregion

    #region CreateChannel
    private Channel CreateChannel(XmlNode service, SignalSource src, Transponder transponder, ref int chanId)
    {
      var c = new Channel(src, chanId, chanId, service);
      c.Transponder = transponder;
      c.Polarity = transponder.Polarity;
      c.FreqInMhz = transponder.FrequencyInMhz;
      c.SymbolRate = transponder.SymbolRate;
      c.OriginalNetworkId = transponder.OriginalNetworkId;
      c.TransportStreamId = transponder.TransportStreamId;
      ReadCommonChannelData(c, service);
      c.Hidden = service.Attributes["vis"].InnerText == "0";
      c.ServiceId = Int32.Parse(service.Attributes["sid"].InnerText);
      c.Encrypted = service.Attributes["ca"].InnerText == "1";
      c.IsDeleted = service.Attributes["del"].InnerText == "1";
      var typ = service.Attributes["typ"].InnerText;
      c.SignalSource |= typ == "1" ? SignalSource.Tv : typ == "2" ? SignalSource.Radio : SignalSource.Data;
      return c;
    }

    #endregion

    #region ReadCommonChannelData
    private void ReadCommonChannelData(Channel c, XmlNode service)
    {
      c.OldProgramNr = Int32.Parse(service.Attributes["num"].InnerText);
      for (int f = 1; f <= 4; f++)
      {
        var n = Int32.Parse(service.Attributes["f" + f].InnerText);
        c.SetOldPosition(f,  n == 0 ? -1 : n);
      }
      c.Lock = service.Attributes["lck"].InnerText == "1";
      c.Skip = service.Attributes["skp"].InnerText == "1";
      c.Name = service.Attributes["name"].InnerText;
    }
    #endregion

    #region CreateTransponder
    private Transponder CreateTransponder(XmlNode mux, ref int transponderId, decimal freqDivider)
    {
      var t = new Transponder(++transponderId);
      t.Polarity = mux.Attributes["pol"].InnerText == "1" ? 'H' : 'V';
      t.SymbolRate = Int32.Parse(mux.Attributes["sym"].InnerText);
      t.FrequencyInMhz = Int32.Parse(mux.Attributes["frq"].InnerText) / freqDivider;
      t.OriginalNetworkId = Int32.Parse(mux.Attributes["onid"].InnerText);
      t.TransportStreamId = Int32.Parse(mux.Attributes["tsid"].InnerText);
      return t;
    }
    #endregion



    #region Save()

    public override void Save()
    {
      foreach (var list in this.DataRoot.ChannelLists)
        this.UpdateChannelList(list);

      foreach (var file in this.fileDataList)
        this.SaveFile(file);
    }

    #endregion

    #region SaveFile()
    private void SaveFile(FileData file)
    {
      // use xmlWriterSettings and some post-processing to maintain the original white spacing as much as possible (new line characters, indentation, empty element close tag, ...),
      // so that the original and modified files can be hex-compared
      // From the 2 test files available so far, one only has \n after the XML processing instruction and the document end, all other white spaces are removed.
      // The other file uses \r\n after all start/end tags and 2 spaces for indentation
      var xmlSettings = new XmlWriterSettings();
      xmlSettings.Encoding = this.DefaultEncoding;
      xmlSettings.CheckCharacters = false;
      xmlSettings.Indent = file.indent;
      xmlSettings.IndentChars = "  ";
      xmlSettings.NewLineHandling = NewLineHandling.Replace;
      xmlSettings.NewLineChars = file.newline;
      xmlSettings.OmitXmlDeclaration = false;

      using var sw = new StringWriter();
      using var w = XmlWriter.Create(sw, xmlSettings);
      file.doc.WriteTo(w);
      w.Flush();
      var xml = sw.ToString();

      if (!file.indent)
      {
        xml = xml.Replace(" />", "/>");
        
        // replace escaped characters with unescaped ones (invalid XML, but that's how Grundig does it)
        var sb = new StringBuilder(xml.Length);
        for (int i = 0, c = xml.Length - 5; i < c; i++)
        {
          if (xml[i] == '&' && xml[i + 1] == '#' && xml[i + 2] == 'x' && xml[i + 5] == ';')
          {
            sb.Append((char)int.Parse(xml.Substring(i + 3, 2), NumberStyles.HexNumber));
            i += 5;
          }
          else
            sb.Append(xml[i]);
        }

        var trail = Math.Min(5, xml.Length);
        sb.Append(xml, xml.Length - trail, trail);
        xml = sb.ToString();
      }

      var enc = new UTF8Encoding(file.hasBom, false);
      File.WriteAllText(file.path, xml, enc);
    }
    #endregion

    #region UpdateChannelList()
    private void UpdateChannelList(ChannelList list)
    {
      foreach (var channel in list.Channels)
      {
        var ch = channel as Channel;
        if (ch == null)
          continue; // might be a proxy channel from a reference list

        if (ch.IsDeleted)
          continue; 
        if (ch.NewProgramNr < 0)
        {
          if ((ch.SignalSource & SignalSource.Digital) != 0)
            ch.IsDeleted = true;
          else
          {
            // analog channels can only be physically removed (no "del" attribute)
            ch.Node.ParentNode.RemoveChild(ch.Node);
            continue;
          }
        }

        this.UpdateChannel(ch);
      }
    }
    #endregion

    #region UpdateChannel
    private void UpdateChannel(Channel ch)
    {
      var att = ch.Node.Attributes;

      if (ch.IsDeleted)
      {
        att["del"].InnerText = "1";
        return; // "num" stays as-is and can be a dupe
      }

      att["num"].InnerText = ch.NewProgramNr.ToString();
      if (ch.IsNameModified)
        att["name"].Value = ch.Name;
      for (int i=1; i<=4; i++)
        att["f"+i].Value = Math.Max(0, ch.GetPosition(i)).ToString(); // convert -1 to 0
      att["skp"].InnerText = ch.Skip ? "1" : "0";
      att["lck"].InnerText = ch.Lock ? "1" : "0";
      if ((ch.SignalSource & SignalSource.Digital) != 0)
        att["vis"].InnerText = ch.Hidden ? "0" : "1";
      else
        att["hid"].InnerText = ch.Hidden ? "1" : "0";
    }

    #endregion

    #region GetDataFilePaths()
    public override IEnumerable<string> GetDataFilePaths()
    {
      return this.fileDataList.Select(fd => fd.path);
    }
    #endregion

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      return base.GetFileInformation() + this.logMessages.Replace("\n", "\r\n");
    }
    #endregion


    #region class FileData
    private class FileData
    {
      public string path;
      public bool hasBom;
      public string newline;
      public bool indent;
      public XmlDocument doc;
    }
    #endregion
  }
}
