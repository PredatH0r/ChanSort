using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  class GcSerializer : SerializerBase
  {
    private readonly ChannelList atvChannels = new ChannelList(SignalSource.AnalogCT | SignalSource.TvAndRadio, "Analog");
    private readonly ChannelList dtvTvChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Tv, "DTV");
    private readonly ChannelList dtvRadioChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Radio, "Radio");
    private readonly ChannelList satTvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Sat-TV");
    private readonly ChannelList satRadioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat-Radio");
    private XmlDocument doc;
    private readonly DvbStringDecoder dvbStringDecoder = new DvbStringDecoder(Encoding.Default);
    private string modelName;
    private readonly Dictionary<int, string> satPositionByIndex = new Dictionary<int, string>();

    #region ctor()
    public GcSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      //this.Features.CanDeleteChannels = false;

      this.DataRoot.AddChannelList(this.atvChannels);
      this.DataRoot.AddChannelList(this.dtvTvChannels);
      this.DataRoot.AddChannelList(this.dtvRadioChannels);
      this.DataRoot.AddChannelList(this.satTvChannels);
      this.DataRoot.AddChannelList(this.satRadioChannels);
    }
    #endregion

    #region DisplayName
    public override string DisplayName { get { return "LG GlobalClone loader"; } }
    #endregion


    #region Load()

    public override void Load()
    {
      bool fail = false;
      try
      {
        this.doc = new XmlDocument();
        using (var reader = new StreamReader(new FileStream(this.FileName, FileMode.Open), Encoding.UTF8))
          doc.Load(reader);
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
    }
    #endregion

    #region ReadModelInfo()
    private void ReadModelInfo(XmlNode modelInfoNode)
    {
      var txt = Resources.GcSerializer_ReadModelInfo_ModelWarning;

      var regex = new System.Text.RegularExpressions.Regex(@"\d{2}([A-Z]{2})(\d{2})\d[0-9A-Z].*");
      foreach (XmlNode child in modelInfoNode.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "ModelName":
            this.modelName = child.InnerText;
            var match = regex.Match(this.modelName);
            if (match.Success)
            {
              var series = match.Groups[1].Value;
              if ((series == "LB" || series == "UB") && StringComparer.InvariantCulture.Compare(match.Groups[2].Value, "60") >= 0)
                txt = Resources.GcSerializer_webOsFirmwareWarning;
            }
            break;
        }
      }

      MessageBox.Show(txt, "LG GlobalClone editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        GcChannel ch = new GcChannel(analog ? SignalSource.AnalogCT | SignalSource.Tv : SignalSource.Digital, i, itemNode);
        this.ParseChannelInfoNodes(itemNode, ch);

        var list = this.DataRoot.GetChannelList(ch.SignalSource);
        this.DataRoot.AddChannel(list, ch);
      }
    }
    #endregion

    #region ParseChannelInfoNode()
    private void ParseChannelInfoNodes(XmlNode itemNode, ChannelInfo ch, bool onlyNames = false)
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
            ch.OldProgramNr = int.Parse(info.InnerText) & 0x3FFF;
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
          case "service_id":
            ch.ServiceId = int.Parse(info.InnerText);
            break;
          case "serviceType":
            ch.ServiceType = int.Parse(info.InnerText);
            ch.SignalSource |= LookupData.Instance.IsRadioOrTv(ch.ServiceType);
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

      // older GlobalClone files look like as if the <vchName> is Chinese, but it's a weired "binary inside UTF8 envelope" encoding:
      // A 3 byte UTF-8 envelope is used to encode 2 input bytes: 1110aaaa 10bbbbcc 10ccdddd represents the 16bit little endian integer aaaabbbbccccdddd, which represents bytes ccccdddd, aaaabbbb
      // If a remaining byte is >= 0x80, it is encoded in a 2 byte UTF-8 envelope: 110000aa 10aabbbb represents the byte aaaabbbb
      // If a remaining byte is < 0x80, it is encoded directly into a 1 byte UTF-8 char
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
          var ch = channel as GcChannel;
          if (ch == null) continue; // ignore proxy channels from reference lists

          foreach (XmlNode node in ch.XmlNode.ChildNodes)
          {
            switch (node.LocalName)
            {
              case "prNum":
                var nr = ch.NewProgramNr;
                if ((ch.SignalSource & SignalSource.Radio) != 0)
                  nr |= 0x4000;
                node.InnerText = nr.ToString();
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
              case "isDeleted":
                node.InnerText = ch.IsDeleted ? "1" : "0";
                break;
              case "isUserSelCHNo":
                node.InnerText = "1";
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
      using (StringWriter sw = new StringWriter())
      using (XmlWriter xw = XmlWriter.Create(sw, settings))
      {
        doc.Save(xw);
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n" + sw;
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
        foreach (var channel in list.Channels)
        {
          var gcChannel = channel as GcChannel;
          if (gcChannel != null)
            this.ParseChannelInfoNodes(gcChannel.XmlNode, channel, true);
        }
      }
    }
    #endregion

  }
}
