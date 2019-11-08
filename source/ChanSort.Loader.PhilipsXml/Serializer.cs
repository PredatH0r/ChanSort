using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.PhilipsXml
{
  /*
    This loader supports 2 different kinds of XML files from Philips.
    
    I had a test file "DVBS.xml" with satellite channels and entries like:
   	<Channel>
      <Setup SatelliteName="0x54 0x00 0x55 0x00 0x52 0x00 0x4B 0x00 0x53 0x00 0x41 0x00 0x54 0x00 0x20 0x00 0x34 0x00 0x32 0x00 0x45 0x00 " ChannelNumber="1" ChannelName="0x54 0x00 0xC4 0x00 0xB0 0x00 0x56 0x00 0xC4 0x00 0xB0 0x00 0x42 0x00 0x55 0x00 0x20 0x00 0x53 0x00 0x50 0x00 0x4F 0x00 0x52 0x00 " ChannelLock="0" UserModifiedName="0" LogoID="0" UserModifiedLogo="0" LogoLock="0" UserHidden="0" FavoriteNumber="0" />
      <Broadcast ChannelType="3" Onid="1070" Tsid="43203" Sid="16001" Frequency="11794" Modulation="0" ServiceType="1" SymbolRate="27507" LNBNumber="38" Polarization="0" SystemHidden="0" />
    </Channel>

    The other file was "CM_TPM1013E_LA_CK.xml" with entries like:
    <Channel>
    <Setup oldpresetnumber="1" presetnumber="1" name="Das Erste" ></Setup>
    <Broadcast medium="dvbc" frequency="410000" system="west" serviceID="1" ONID="41985" TSID="1101" modulation="256" symbolrate="6901000" bandwidth="Unknown"></Broadcast>
    </Channel>
     
   */
  class Serializer : SerializerBase
  {
    private readonly ChannelList terrChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
    private readonly ChannelList cableChannels = new ChannelList(SignalSource.DvbC, "DVB-C");
    private readonly ChannelList satChannels = new ChannelList(SignalSource.DvbS, "DVB-S");

    private XmlDocument doc;
    private byte[] content;
    private string textContent;
    private string newline;
    private int formatVersion;

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;

      this.DataRoot.AddChannelList(this.terrChannels);
      this.DataRoot.AddChannelList(this.cableChannels);
      this.DataRoot.AddChannelList(this.satChannels);

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("PcrPid");
        list.VisibleColumnFieldNames.Remove("VideoPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("ShortName");
        list.VisibleColumnFieldNames.Remove("Provider");
      }
    }
    #endregion


    #region Load()

    public override void Load()
    {
      bool fail = false;
      try
      {
        this.doc = new XmlDocument();
        this.content = File.ReadAllBytes(this.FileName);
        this.textContent = Encoding.UTF8.GetString(this.content);
        this.newline = this.textContent.Contains("\r\n") ? "\r\n" : "\n";

        var settings = new XmlReaderSettings
        {
          CheckCharacters = false,
          IgnoreProcessingInstructions = true,
          ValidationFlags = XmlSchemaValidationFlags.None,
          DtdProcessing = DtdProcessing.Ignore
        };
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
      if (fail || root == null || root.LocalName != "ChannelMap")
        throw new FileLoadException("\"" + this.FileName + "\" is not a supported Philips XML file");


      int rowId = 0;
      ChannelList curList = null;
      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "Channel":
            if (rowId == 0)
              curList = this.DetectFormatAndFeatures(child);
            if (curList != null)
              this.ReadChannel(curList, child, rowId++);
            break;
        }
      }
    }
    #endregion

    #region DetectFormatAndFeatures()

    private ChannelList DetectFormatAndFeatures(XmlNode node)
    {
      var setupNode = node["Setup"] ?? throw new FileLoadException("Missing Setup XML element");
      var bcastNode = node["Broadcast"] ?? throw new FileLoadException("Missing Broadcast XML element");

      var fname = Path.GetFileNameWithoutExtension(this.FileName).ToLower();
      var medium = bcastNode.GetAttribute("medium");
      if (medium == "" && fname.Length == 4 && fname.StartsWith("dvb"))
        medium = fname;
      bool hasEncrypt = false;

      if (setupNode.HasAttribute("ChannelName"))
      {
        this.formatVersion = 1;
        this.Features.SupportedFavorites = Favorites.A;
        this.Features.SortedFavorites = true;

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
        this.formatVersion = 2;
        this.Features.SupportedFavorites = 0;
        this.Features.SortedFavorites = false;
        foreach (var list in this.DataRoot.ChannelLists)
        {
          list.VisibleColumnFieldNames.Remove("Favorites");
          list.VisibleColumnFieldNames.Remove("Lock");
          list.VisibleColumnFieldNames.Remove("Hidden");
          list.VisibleColumnFieldNames.Remove("ServiceType");
          list.VisibleColumnFieldNames.Remove("ServiceTypeName");
          list.VisibleColumnFieldNames.Remove("Encrypted");
        }
      }
      else
        throw new FileLoadException("Unknown data format");

      ChannelList chList = null;
      switch (medium)
      {
        case "dvbt":
          chList = this.terrChannels;
          break;
        case "dvbc":
          chList = this.cableChannels;
          break;
        case "dvbs":
          chList = this.satChannels;
          break;
      }

      if (!hasEncrypt)
        chList?.VisibleColumnFieldNames.Remove("Encrypted");

      return chList;
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(ChannelList curList, XmlNode node, int rowId)
    {
      var setupNode = node["Setup"] ?? throw new FileLoadException("Missing Setup XML element");
      var bcastNode = node["Broadcast"] ?? throw new FileLoadException("Missing Broadcast XML element");
      var data = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var n in new[] {setupNode, bcastNode})
      {
        foreach(XmlAttribute attr in n.Attributes)
          data.Add(attr.LocalName, attr.Value);
      }
      
      var chan = new Channel(curList.SignalSource & SignalSource.MaskAdInput, rowId, rowId, setupNode);
      chan.OldProgramNr = -1;
      chan.IsDeleted = false;
      if (this.formatVersion == 1)
        this.ParseChannelFormat1(data, chan);
      else if (this.formatVersion == 2)
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
      chan.RawSatellite = data.TryGet("SatelliteName");
      chan.Satellite = DecodeName(chan.RawSatellite);
      chan.OldProgramNr = ParseInt(data.TryGet("ChannelNumber"));
      chan.RawName = data.TryGet("ChannelName");
      chan.Name = DecodeName(chan.RawName);
      chan.Lock = data.TryGet("ChannelLock") == "1";
      chan.Hidden = data.TryGet("UserHidden") == "1";
      var fav = ParseInt(data.TryGet("FavoriteNumber"));
      chan.FavIndex[0] = fav == 0 ? -1 : fav;
      chan.OriginalNetworkId = ParseInt(data.TryGet("Onid"));
      chan.TransportStreamId = ParseInt(data.TryGet("Tsid"));
      chan.ServiceId = ParseInt(data.TryGet("Sid"));
      var freq = ParseInt(data.TryGet("Frequency"));
      chan.FreqInMhz = freq;
      chan.ServiceType = ParseInt(data.TryGet("ServiceType"));
      chan.SignalSource |= LookupData.Instance.IsRadioOrTv(chan.ServiceType);
      chan.SymbolRate = ParseInt(data.TryGet("SymbolRate"));
      if (data.TryGetValue("Polarization", out var pol))
        chan.Polarity = pol == "0" ? 'H' : 'V';
      chan.Hidden |= data.TryGet("SystemHidden") == "1";

      chan.Encrypted = data.TryGet("Scrambled") == "1"; // doesn't exist in all format versions
    }
    #endregion

    #region ParseChannelFormat2
    private void ParseChannelFormat2(Dictionary<string, string> data, Channel chan)
    {
      chan.OldProgramNr = ParseInt(data.TryGet("presetnumber"));
      chan.Name = data.TryGet("name");
      chan.RawName = chan.Name;
      chan.FreqInMhz = ParseInt(data.TryGet("frequency"));
      if (chan.FreqInMhz > 100000)
        chan.FreqInMhz /= 1000;
      chan.ServiceId = ParseInt(data.TryGet("serviceID"));
      chan.OriginalNetworkId = ParseInt(data.TryGet("ONID"));
      chan.TransportStreamId = ParseInt(data.TryGet("TSID"));
      chan.ServiceType = ParseInt(data.TryGet("serviceType"));
      chan.SymbolRate = ParseInt(data.TryGet("symbolrate")) / 1000;
    }
    #endregion

    #region ParseInt()
    private int ParseInt(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return 0;
      if (input.Length > 2 && input[0] == '0' && char.ToLower(input[1]) == 'x')
        return int.Parse(input.Substring(2), NumberStyles.HexNumber);
      if (int.TryParse(input, out var value))
        return value;
      return 0;
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
        if (part == "" || part == "0x00")
          continue;
        buffer.WriteByte((byte)ParseInt(part));
      }

      return this.DefaultEncoding.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
    }
    #endregion

    #region DefaultEncoding
    public override Encoding DefaultEncoding
    {
      get => base.DefaultEncoding;
      set
      {
        if (value == this.DefaultEncoding)
          return;
        base.DefaultEncoding = value;
        this.ChangeEncoding();
      }
    }
    #endregion

    #region ChangeEncoding
    private void ChangeEncoding()
    {
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (var channel in list.Channels)
        {
          if (!(channel is Channel ch))
            continue;
          ch.Name = this.DecodeName(ch.RawName);
          ch.Satellite = this.DecodeName(ch.RawSatellite);
        }
      }
    }
    #endregion

    #region Save()
    public override void Save(string tvOutputFile)
    {
      foreach (var list in this.DataRoot.ChannelLists)
        this.UpdateChannelList(list);

      // by default .NET reformats the whole XML. These settings produce almost same format as the TV xml files use
      var xmlSettings = new XmlWriterSettings();
      xmlSettings.Encoding = this.DefaultEncoding;
      xmlSettings.CheckCharacters = false;
      xmlSettings.Indent = true;
      xmlSettings.IndentChars = "";
      xmlSettings.NewLineHandling = NewLineHandling.None;
      xmlSettings.NewLineChars = this.newline;
      xmlSettings.OmitXmlDeclaration = false;

      string xml;
      using (var sw = new StringWriter())
      using (var w = new CustomXmlWriter(sw, xmlSettings, false))
      {
        this.doc.WriteTo(w);
        w.Flush();
        xml = sw.ToString();
      }

      var enc = new UTF8Encoding(false, false);
      File.WriteAllText(tvOutputFile, xml, enc);
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

        if (ch.IsDeleted || ch.NewProgramNr < 0)
        {
          ch.SetupNode.ParentNode.ParentNode.RemoveChild(ch.SetupNode.ParentNode);
          continue;
        }

        if (this.formatVersion == 1)
          this.UpdateChannelFormat1(ch);
        else if (this.formatVersion == 2)
          this.UpdateChannelFormat2(ch);
      }
    }
    #endregion

    #region UpdateChannelFormat1 and 2
    private void UpdateChannelFormat1(Channel ch)
    {
      ch.SetupNode.Attributes["ChannelNumber"].Value = ch.NewProgramNr.ToString();
      if (ch.IsNameModified)
        ch.SetupNode.Attributes["ChannelName"].Value = EncodeName(ch.Name);
      ch.SetupNode.Attributes["FavoriteNumber"].Value = Math.Max(ch.FavIndex[0], 0).ToString();
    }

    private void UpdateChannelFormat2(Channel ch)
    {
      ch.SetupNode.Attributes["presetnumber"].Value = ch.NewProgramNr.ToString();
      if (ch.IsNameModified)
        ch.SetupNode.Attributes["name"].Value = ch.Name;
    }
    #endregion

    #region EncodeName
    private string EncodeName(string name)
    {
      var bytes = this.DefaultEncoding.GetBytes(name);
      var sb = new StringBuilder();
      foreach (var b in bytes)
        sb.Append($"0x{b:X2} 0x00 ");
      return sb.ToString();
    }
    #endregion
  }
}
