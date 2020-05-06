using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ChanSort.Api;
using System.Windows.Forms;

namespace ChanSort.Loader.GlobalClone
{
  class GcXmlSerializer : SerializerBase
  {
    private readonly ChannelList atvChannels = new ChannelList(SignalSource.AnalogCT, "Analog");
    private readonly ChannelList dtvTvChannels = new ChannelList(SignalSource.DvbCT | SignalSource.TvAndData, "DTV");
    private readonly ChannelList dtvRadioChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Radio, "Radio");
    private readonly ChannelList satTvChannels = new ChannelList(SignalSource.DvbS | SignalSource.TvAndData, "Sat-TV");
    private readonly ChannelList satRadioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat-Radio");
    private XmlDocument doc;
    private readonly DvbStringDecoder dvbStringDecoder = new DvbStringDecoder(Encoding.Default);
    private string modelName;
    private readonly Dictionary<int, string> satPositionByIndex = new Dictionary<int, string>();
    private bool usesBinaryDataInUtf8Envelope = false;

    #region ctor()
    public GcXmlSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.FlagWithoutPrNr;
      this.Features.CanHaveGaps = true;
      this.Features.CanSaveAs = true;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;

      this.DataRoot.AddChannelList(this.atvChannels);
      this.DataRoot.AddChannelList(this.dtvTvChannels);
      this.DataRoot.AddChannelList(this.dtvRadioChannels);
      this.DataRoot.AddChannelList(this.satTvChannels);
      this.DataRoot.AddChannelList(this.satRadioChannels);
    }
    #endregion

    #region Load()

    public override void Load()
    {
      bool fail = false;
      try
      {
        this.doc = new XmlDocument();
        string textContent = File.ReadAllText(this.FileName, Encoding.UTF8);
        if (textContent[0] != '<')
          throw new FileLoadException("Invalid GlobalClone/XML file format. Maybe a binary xx*.TLL file?", this.FileName);
        textContent = ReplaceInvalidXmlCharacters(textContent);
        var settings = new XmlReaderSettings { CheckCharacters = false };
        using (var reader = XmlReader.Create(new StringReader(textContent), settings))
        {
          doc.Load(reader);
        }
      }
      catch
      {
        fail = true;
      }

      var root = doc.FirstChild;
      if (root is XmlDeclaration)
        root = root.NextSibling;
      if (fail || root == null || root.LocalName != "TLLDATA")
        throw new FileLoadException("\"" + this.FileName + "\" is not a supported GlobalClone XML file");

      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "ModelInfo":
            this.ReadModelInfo(child);
            break;
          case "SatelliteDB":
            this.ReadSatelliteDB(child);
            break;
          case "CHANNEL":
            this.ReadChannelLists(child);
            break;
        }
      }

      this.Features.ChannelNameEdit = usesBinaryDataInUtf8Envelope ? ChannelNameEditMode.Analog : ChannelNameEditMode.All;
    }
    #endregion

    #region ReadModelInfo()
    private void ReadModelInfo(XmlNode modelInfoNode)
    {
      // show warning about broken import function in early webOS firmware
      var regex = new System.Text.RegularExpressions.Regex(@"\d{2}([A-Z]{2})(\d{2})\d[0-9A-Z].*");
      var series = "";
      foreach (XmlNode child in modelInfoNode.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "ModelName":
            this.modelName = child.InnerText;
            var match = regex.Match(this.modelName);
            if (match.Success)
            {
              series = match.Groups[1].Value;
              if ((series == "LB" || series == "UB") && StringComparer.InvariantCulture.Compare(match.Groups[2].Value, "60") >= 0)
                Api.View.Default.MessageBox(Resources.GcSerializer_webOsFirmwareWarning, "LG GlobalClone", (int)MessageBoxButtons.OK, (int)MessageBoxIcon.Information);
            }
            break;
        }
      }

      // ask whether binary TLL file should be deleted
      var dir = Path.GetDirectoryName(this.FileName) ?? ".";
      var binTlls = Directory.GetFiles(dir, "xx" + series + "*.tll");
      if (binTlls.Length > 0 && !(binTlls.Length == 1 && Path.GetFileName(binTlls[0]).ToLower() == Path.GetFileName(this.FileName).ToLower()))
      {
        var txt = Resources.GcSerializer_ReadModelInfo_ModelWarning;
        if (Api.View.Default.MessageBox(txt, "LG GlobalClone", (int)MessageBoxButtons.YesNo, (int)MessageBoxIcon.Information) == (int)DialogResult.Yes)
        {
          foreach (var file in binTlls)
            File.Move(file, file + "_bak");
        }
      }
    }
    #endregion

    #region ReadSatelliteDB()
    private void ReadSatelliteDB(XmlNode node)
    {
      foreach (XmlNode child in node.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "SATDBInfo":
            this.ReadSatDbInfo(child);
            break;
        }
      }      
    }

    private void ReadSatDbInfo(XmlNode node)
    {
      foreach (XmlNode child in node.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "SatRecordInfo":
            int i = 0;
            foreach (XmlNode satNode in child.ChildNodes)
              this.ReadSatRecordInfo(i++, satNode);
            break;
        }
      }
    }

    private void ReadSatRecordInfo(int i, XmlNode satRecordInfoNode)
    {
      string orbitalPos = "";
      foreach (XmlNode child in satRecordInfoNode.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "Angle":
            orbitalPos += child.InnerText;
            break;
          case "AnglePrec":
            orbitalPos += "." + child.InnerText;
            break;
          case "DirEastWest":
            orbitalPos += child.InnerText == "0" ? "W" : "E";
            break;
        }
      }
      this.satPositionByIndex[i] = orbitalPos;
    }
    #endregion

    #region ReadChannelLists()
    private void ReadChannelLists(XmlNode channelNode)
    {
      foreach (XmlNode chanListNode in channelNode.ChildNodes)
      {
        switch (chanListNode.LocalName)
        {
          case "ATV":
            this.ReadChannelList(chanListNode, true);
            break;
          case "DTV":
            this.ReadChannelList(chanListNode, false);
            break;
          case "DTVATV":
            // TODO: US DTV_ATSC files contain such lists
            break;
        }
      }

      // when the user selects a predefined "provider" during the TV's setup, an empty list will be exported and can't be edited
      int total = 0;
      foreach (var list in this.DataRoot.ChannelLists)
        total += list.Channels.Count;
      if (total == 0)
      {
        Api.View.Default.MessageBox(Resources.GcSerializer_ReadChannelLists_NoChannelsMsg, Resources.GcSerializer_ReadChannelLists_NoChannelsCap, 
          (int)MessageBoxButtons.OK, (int)MessageBoxIcon.Exclamation);
      }
    }
    #endregion

    #region ReadChannelList()
    private void ReadChannelList(XmlNode node, bool analog)
    {
      int i = -1;
      foreach (XmlNode itemNode in node.ChildNodes)
      {
        if (itemNode.LocalName != "ITEM")
          continue;
        ++i;
        var ch = new GcChannel<XmlNode>(analog ? SignalSource.AnalogCT | SignalSource.Tv : SignalSource.Digital, i, itemNode);
        this.ParseChannelInfoNodes(itemNode, ch);

        var list = this.DataRoot.GetChannelList(ch.SignalSource);
        this.DataRoot.AddChannel(list, ch);
      }
    }
    #endregion

    #region ParseChannelInfoNode()
    private void ParseChannelInfoNodes(XmlNode itemNode, GcChannel<XmlNode> ch, bool onlyNames = false)
    {
      bool hasHexName = false;
      int mapType = 0;
      foreach (XmlNode info in itemNode.ChildNodes)
      {
        if (onlyNames && info.LocalName != "vchName" && info.LocalName != "hexVchName")
          continue;
        
        switch (info.LocalName)
        {
            // common to ATV and DTV
          case "prNum":
            ch.OldProgramNr = int.Parse(info.InnerText);
            if (ch.OldProgramNr != -1) // older versions of ChanSort accidentally saved -1 instead of IsDeleted=1
              ch.OldProgramNr &= 0x3FFF;
            break;
          case "vchName":
            // In old file format versions, this field contains binary data stuffed into UTF8 envelopes. that data is correct
            // In newer file formats, this field contains plain text but fails to hold localized characters. The hexVchName field, if present, contains the correct data then.
            if (!hasHexName)
              ch.Name = ParseName(info.InnerText);
            break;
          case "sourceIndex":
            var source = int.Parse(info.InnerText);
            if (source == 2)
              ch.SignalSource |= SignalSource.Cable;
            else if (source == 7)
              ch.SignalSource |= SignalSource.Sat;
            else
              ch.SignalSource |= SignalSource.Antenna;
            break;
          case "mapType":
            mapType = int.Parse(info.InnerText);
            break;
          case "mapAttr":
            if (mapType == 1)
              ch.Favorites = (Favorites) int.Parse(info.InnerText);
            break;
          case "isBlocked":
            ch.Lock = int.Parse(info.InnerText) == 1;
            break;
          case "isSkipped":
            ch.Skip = int.Parse(info.InnerText) == 1;
            break;

            // ATV
          case "pllData":
            ch.FreqInMhz = (decimal) int.Parse(info.InnerText)/20;
            break;

            // DTV
          case "original_network_id":
            ch.OriginalNetworkId = int.Parse(info.InnerText);
            break;
          case "transport_id":
            ch.TransportStreamId = int.Parse(info.InnerText);
            break;
          case "service_id": // also same value in "programNo"
            ch.ServiceId = int.Parse(info.InnerText);
            break;
          case "serviceType":
            ch.ServiceType = int.Parse(info.InnerText);
            ch.SignalSource |= LookupData.Instance.IsRadioTvOrData(ch.ServiceType);
            break;
          case "frequency":
            ch.FreqInMhz = int.Parse(info.InnerText);
            if ((ch.SignalSource & SignalSource.Sat) == 0)
              ch.FreqInMhz /= 1000;
            break;
          case "isInvisable": // that spelling error is part of the XML
            ch.Hidden = int.Parse(info.InnerText) == 1;
            break;
          case "isNumUnSel":
            // ?
            break;
          case "isDisabled":
            ch.IsDisabled = int.Parse(info.InnerText) != 0;
            break;
          case "isDeleted":
            ch.IsDeleted = int.Parse(info.InnerText) != 0;
            break;
          case "usSatelliteHandle":
            int satIndex = int.Parse(info.InnerText);
            string satPos = this.satPositionByIndex.TryGet(satIndex);
            ch.SatPosition = satPos ?? satIndex.ToString(); // fallback to ensure unique UIDs
            ch.Satellite = satPos;
            break;           

            // not present in all XML files. if present, the <vchName> might be empty or corrupted
          case "hexVchName":
            var bytes = Tools.HexDecode(info.InnerText);
            string longName, shortName;
            dvbStringDecoder.GetChannelNames(bytes, 0, bytes.Length, out longName, out shortName);
            ch.Name = longName;
            ch.ShortName = shortName;
            hasHexName = true;
            break;
          default:
            if (info.LocalName.StartsWith("favoriteIdx"))
            {
              int n = info.LocalName[11] - 'A';
              var mask = 1 << n;
              this.Features.SupportedFavorites |= (Favorites)mask;
              this.Features.SortedFavorites = true;
              if (((int)ch.Favorites & mask) != 0) // xml element holds bad index data (250) when fav is not set
                ch.SetOldPosition(n + 1, int.Parse(info.InnerText));
            }
            break;
        }
      }
    }
    #endregion

    #region ParseName()
    private string ParseName(string input)
    {
      var bytes = Encoding.UTF8.GetBytes(input);
      if (bytes.Length == 0 || bytes[0] < 0xC0)
        return input;

      this.usesBinaryDataInUtf8Envelope = true;

      // older GlobalClone files look like as if the <vchName> is Chinese, but it's a weired "binary inside UTF8 envelope" encoding:
      // A 3 byte UTF-8 envelope is used to encode 2 input bytes: 1110aaaa 10bbbbcc 10ccdddd represents the 16bit little endian integer aaaabbbbccccdddd, which represents bytes ccccdddd, aaaabbbb
      // If a remaining byte is >= 0x80, it is encoded in a 2 byte UTF-8 envelope: 110000aa 10aabbbb represents the byte aaaabbbb
      // If a remaining byte is < 0x80, it is encoded directly into a 1 byte UTF-8 char. (This can cause invalid XML files for values < 0x20.)
      using (MemoryStream ms = new MemoryStream(40))
      {
        for (int i = 0, c = bytes.Length; i < c; i++)
        {
          int b0 = bytes[i + 0];
          if (b0 >= 0xE0) // 3-byte UTF envelope for 2 input bytes
          {
            int b1 = bytes[i + 1];
            int b2 = bytes[i + 2];
            int ch1 = ((b1 & 0x03) << 6) | (b2 & 0x3F);
            int ch2 = ((b0 & 0x0F) << 4) | ((b1 & 0x3C) >> 2);
            ms.WriteByte((byte) ch1);
            ms.WriteByte((byte) ch2);
            i += 2;
          }
          else if (b0 >= 0xC0) // 2-byte UTF envelope for 1 input byte >= 0x80
          {
            int b1 = bytes[i + 1];
            int ch = ((b0 & 0x03) << 6) | (b1 & 0x3F);
            ms.WriteByte((byte)ch);
            i++;
          }
          else if (b0 < 0x80) // 1-byte UTF envelope for 1 input byte < 0x80
            ms.WriteByte(bytes[i]);
        }

        string longName, shortName;
        this.dvbStringDecoder.GetChannelNames(ms.GetBuffer(), 0, (int)ms.Length, out longName, out shortName);
        return longName;
      }      
    }
    #endregion

    #region Save()
    public override void Save(string tvOutputFile)
    {
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (var channel in list.Channels)
        {
          var ch = channel as GcChannel<XmlNode>;
          if (ch == null) continue; // ignore proxy channels from reference lists
          var nameBytes = Encoding.UTF8.GetBytes(ch.Name);
          bool nameNeedsEncoding = nameBytes.Length != ch.Name.Length;
          string mapType = "";
          
          foreach (XmlNode node in ch.Node.ChildNodes)
          {
            switch (node.LocalName)
            {
              case "prNum":
                var nr = ch.NewProgramNr;
                if ((ch.SignalSource & SignalSource.Radio) != 0)
                  nr |= 0x4000;
                node.InnerText = nr.ToString();
                break;
              case "hexVchName":
                if (channel.IsNameModified)
                  node.InnerText = (nameNeedsEncoding ? "15" : "") + Tools.HexEncode(nameBytes); // 0x15 = DVB encoding indicator for UTF-8
                break;
              case "notConvertedLengthOfVchName":
                if (channel.IsNameModified)
                  node.InnerText = ((nameNeedsEncoding ? 1 : 0) + ch.Name.Length).ToString();
                break;
              case "vchName":
                if (channel.IsNameModified)
                  node.InnerText = nameNeedsEncoding ? " " : ch.Name;
                if (node.InnerText == "") // XmlTextReader removed the required space from empty channel names
                  node.InnerText = " ";
                break;
              case "isInvisable":
                node.InnerText = ch.Hidden ? "1" : "0";
                break;
              case "isBlocked":
                node.InnerText = ch.Lock ? "1" : "0";
                break;
              case "isSkipped":
                node.InnerText = ch.Skip ? "1" : "0";
                break;
              case "isNumUnSel":
                // ?
                break;
              case "isDisabled":
                node.InnerText = ch.IsDisabled /* || ch.IsDeleted */ ? "1" : "0";
                break;
              case "isDeleted":
                node.InnerText = ch.IsDeleted ? "1" : "0";
                break;
              case "isUserSelCHNo":
                if (ch.NewProgramNr != ch.OldProgramNr)
                  node.InnerText = ch.IsDeleted ? "0" : "1";
                break;
              case "mapType":
                mapType = node.InnerText;
                if (int.TryParse(mapType, out int value))
                {
                  if (ch.IsDeleted)
                    value |= 0x02; // all channels that have isDeleted=1 had mapType=0x03, all other channels had mapType=0x01
                  else
                    value &= ~0x02;
                  node.InnerText = value.ToString();
                }
                break;
              case "mapAttr":
                if (mapType == "1")
                  node.InnerText = ((int) ch.Favorites).ToString();
                break;
              default:
                if (node.LocalName.StartsWith("favoriteIdx"))
                {
                  int n = node.LocalName[11] - 'A';
                  var idx = ch.GetPosition(n + 1);
                  if (idx <= 0)
                    idx = 250; // this weird value is used by the TV when the fav is not set
                  node.InnerText = idx.ToString();
                }
                break;
            }
          }
        }
      }

      // by default .NET reformats the whole XML. These settings produce the same format as the TV xml files use
      var settings = new XmlWriterSettings();
      settings.Encoding = new UTF8Encoding(false);
      settings.Indent = true;
      settings.NewLineChars = "\r\n";
      settings.NewLineHandling = NewLineHandling.Replace;
      settings.OmitXmlDeclaration = true;
      settings.IndentChars = "";
      settings.CheckCharacters = false;
      using (StringWriter sw = new StringWriter())
      using (XmlWriter xw = XmlWriter.Create(sw, settings))
      {
        doc.Save(xw);
        xw.Flush();
        string xml = RestoreInvalidXmlCharacters(sw.ToString());
        xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n" + xml;
        xml = xml.Replace("<ATV></ATV>\r\n", "<ATV>\r\n</ATV>\r\n");
        xml = xml.Replace("<DTV></DTV>\r\n", "<DTV>\r\n</DTV>\r\n");
        xml = xml.Replace("<hexAszTkgsMessage type=\"0\"></hexAszTkgsMessage>", "<hexAszTkgsMessage type=\"0\"> </hexAszTkgsMessage>");
        xml = xml.Replace("<aszTkgsMessage type=\"0\"></aszTkgsMessage>", "<aszTkgsMessage type=\"0\"> </aszTkgsMessage>");

        if (!xml.EndsWith("\r\n"))
          xml += "\r\n";
        File.WriteAllText(tvOutputFile, xml, settings.Encoding);
      }
    }
    #endregion

    #region DefaultEncoding
    public override Encoding DefaultEncoding
    {
      get { return base.DefaultEncoding; }
      set
      {
        if (ReferenceEquals(value, this.DefaultEncoding))
          return;
        base.DefaultEncoding = value;
        this.dvbStringDecoder.DefaultEncoding = value;
        this.ChangeEncoding();
      }
    }
    #endregion

    #region ChangeEncoding()
    private void ChangeEncoding()
    {
      foreach (var list in this.DataRoot.ChannelLists)
      {
        if (list.IsMixedSourceFavoritesList)
          continue;
        foreach (var channel in list.Channels)
        {
          var gcChannel = channel as GcChannel<XmlNode>;
          if (gcChannel != null)
            this.ParseChannelInfoNodes(gcChannel.Node, gcChannel, true);
        }
      }
    }
    #endregion


    #region ReplaceInvalidXmlCharacters()
    private string ReplaceInvalidXmlCharacters(string input)
    {
      StringBuilder output = new StringBuilder();
      foreach (var c in input)
      {
        if (c >= ' ' || c == '\r' || c == '\n' || c == '\t')
          output.Append(c);
        else
          output.AppendFormat("&#x{0:d}{1:d};", c >> 4, c & 0x0F);
      }
      return output.ToString();
    }
    #endregion

    #region RestoreInvalidXmlCharacters()
    private string RestoreInvalidXmlCharacters(string input)
    {
      StringBuilder output = new StringBuilder();
      int prevIdx = 0;
      while(true)
      {
        int nextIdx = input.IndexOf("&#", prevIdx);
        if (nextIdx < 0)
          break;
        output.Append(input, prevIdx, nextIdx - prevIdx);

        int numBase = 10;
        char inChar;
        int outChar = 0;
        for (nextIdx += 2; (inChar=input[nextIdx]) != ';'; nextIdx++)
        {
          if (inChar == 'x' || inChar == 'X')
            numBase = 16;
          else
            outChar = outChar*numBase + HexNibble(inChar);
        }
        var binChar = (char)outChar;
        output.Append(binChar);
        prevIdx = nextIdx + 1;
      }
      output.Append(input, prevIdx, input.Length - prevIdx);
      return output.ToString();
    }

    private int HexNibble(char hexDigit)
    {
      return hexDigit >= '0' && hexDigit <= '9' ? hexDigit - '0' : (Char.ToUpper(hexDigit) - 'A') + 10;
    }
    #endregion
  }
}
