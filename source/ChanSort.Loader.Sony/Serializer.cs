using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.Sony
{
  class Serializer : SerializerBase
  {
    private readonly ChannelList satChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv | SignalSource.Radio, "Sat");
    private XmlDocument doc;
    private byte[] content;
    private string textContent;
    private XmlNode sdbXml;
    private string format;

    #region Crc32Table
    private static readonly uint[] Crc32Table =
    {
      0, 79764919, 159529838, 222504665, 319059676, 398814059, 445009330, 507990021, 638119352, 583659535, 797628118, 726387553, 890018660, 835552979, 1015980042, 944750013,
      1276238704, 1221641927, 1167319070, 1095957929, 1595256236, 1540665371, 1452775106, 1381403509, 1780037320, 1859660671, 1671105958, 1733955601, 2031960084, 2111593891, 1889500026, 1952343757,
      2552477408, 2632100695, 2443283854, 2506133561, 2334638140, 2414271883, 2191915858, 2254759653, 3190512472, 3135915759, 3081330742, 3009969537, 2905550212, 2850959411, 2762807018, 2691435357,
      3560074640, 3505614887, 3719321342, 3648080713, 3342211916, 3287746299, 3467911202, 3396681109, 4063920168, 4143685023, 4223187782, 4286162673, 3779000052, 3858754371, 3904687514, 3967668269,
      881225847, 809987520, 1023691545, 969234094, 662832811, 591600412, 771767749, 717299826, 311336399, 374308984, 453813921, 533576470, 25881363, 88864420, 134795389, 214552010,
      2023205639, 2086057648, 1897238633, 1976864222, 1804852699, 1867694188, 1645340341, 1724971778, 1587496639, 1516133128, 1461550545, 1406951526, 1302016099, 1230646740, 1142491917, 1087903418,
      2896545431, 2825181984, 2770861561, 2716262478, 3215044683, 3143675388, 3055782693, 3001194130, 2326604591, 2389456536, 2200899649, 2280525302, 2578013683, 2640855108, 2418763421, 2498394922,
      3769900519, 3832873040, 3912640137, 3992402750, 4088425275, 4151408268, 4197601365, 4277358050, 3334271071, 3263032808, 3476998961, 3422541446, 3585640067, 3514407732, 3694837229, 3640369242,
      1762451694, 1842216281, 1619975040, 1682949687, 2047383090, 2127137669, 1938468188, 2001449195, 1325665622, 1271206113, 1183200824, 1111960463, 1543535498, 1489069629, 1434599652, 1363369299,
      622672798, 568075817, 748617968, 677256519, 907627842, 853037301, 1067152940, 995781531, 51762726, 131386257, 177728840, 240578815, 269590778, 349224269, 429104020, 491947555,
      4046411278, 4126034873, 4172115296, 4234965207, 3794477266, 3874110821, 3953728444, 4016571915, 3609705398, 3555108353, 3735388376, 3664026991, 3290680682, 3236090077, 3449943556, 3378572211,
      3174993278, 3120533705, 3032266256, 2961025959, 2923101090, 2868635157, 2813903052, 2742672763, 2604032198, 2683796849, 2461293480, 2524268063, 2284983834, 2364738477, 2175806836, 2238787779,
      1569362073, 1498123566, 1409854455, 1355396672, 1317987909, 1246755826, 1192025387, 1137557660, 2072149281, 2135122070, 1912620623, 1992383480, 1753615357, 1816598090, 1627664531, 1707420964,
      295390185, 358241886, 404320391, 483945776, 43990325, 106832002, 186451547, 266083308, 932423249, 861060070, 1041341759, 986742920, 613929101, 542559546, 756411363, 701822548,
      3316196985, 3244833742, 3425377559, 3370778784, 3601682597, 3530312978, 3744426955, 3689838204, 3819031489, 3881883254, 3928223919, 4007849240, 4037393693, 4100235434, 4180117107, 4259748804,
      2310601993, 2373574846, 2151335527, 2231098320, 2596047829, 2659030626, 2470359227, 2550115596, 2947551409, 2876312838, 2788305887, 2733848168, 3165939309, 3094707162, 3040238851, 2985771188
    };
    #endregion


    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanDeleteChannels = true;

      satChannels.VisibleColumnFieldNames.Remove("PcrPid");
      satChannels.VisibleColumnFieldNames.Remove("VideoPid");
      satChannels.VisibleColumnFieldNames.Remove("AudioPid");
      satChannels.VisibleColumnFieldNames.Remove("Lock");
      satChannels.VisibleColumnFieldNames.Remove("Skip");
      satChannels.VisibleColumnFieldNames.Remove("Provider");

      this.DataRoot.AddChannelList(this.satChannels);
    }
    #endregion

    #region DisplayName
    public override string DisplayName => "Sony sdb.xml loader";

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
        var tc2 = ReplaceInvalidXmlCharacters(textContent);
        if (tc2 != this.textContent)
          this.textContent = tc2;
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
      if (fail || root == null || root.LocalName != "SdbRoot")
        throw new FileLoadException("\"" + this.FileName + "\" is not a supported Sony XML file");

      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "SdbXml":
            this.ReadSdbXml(child);
            break;
          case "CheckSum":
            this.ReadChecksum(child);
            break;
        }
      }

      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      if (this.format != "1.1.0e")
      {
        satChannels.VisibleColumnFieldNames.Remove("Hidden");
        satChannels.VisibleColumnFieldNames.Remove("Satellite");
      }
    }
    #endregion

    #region ReadSdbXml()
    private void ReadSdbXml(XmlNode node)
    {
      this.sdbXml = node;

      this.format = null;
      var formatNode = node["FormatVer"];
      if (formatNode != null)
        this.format = formatNode.InnerText;
      else if ((formatNode = node["FormateVer"]) != null)
        this.format = formatNode.InnerText + "e";

      if (" 1.0.0 1.1.0e 1.1.0 1.2.0 ".IndexOf(" " + this.format + " ") < 0)
        throw new FileLoadException("Unsupported file format version: " + this.format);

      foreach(XmlNode child in node.ChildNodes)
      {
        var name = child.LocalName.ToLowerInvariant();
        if (name == "sdbgs")
          ReadSdb(child);
      }
    }
    #endregion

    #region ReadSdb
    private void ReadSdb(XmlNode node)
    {
      this.satChannels.ReadOnly = node["Editable"].InnerText != "T";

      this.ReadSatellites(node);
      this.ReadTransponder(node);
      if (this.format == "1.1.0e")
        this.ReadServices110e(node);
      else
        this.ReadServices(node);
    }
    #endregion

    #region ReadSatellites
    private void ReadSatellites(XmlNode node)
    {
      var satlRec = node["SATL_REC"];
      if (satlRec == null)
        return;
      var data = this.SplitLines(satlRec);
      var ids = data["ui2_satl_rec_id"];
      for (int i = 0, c = ids.Length; i < c; i++)
      {
        var sat = new Satellite(int.Parse(ids[i]));
        sat.Name = data["ac_sat_name"][i];
        var pos = int.Parse(data["i2_orb_pos"][i]);
        sat.OrbitalPosition = Math.Abs((decimal) pos / 10) + (pos < 0 ? "W" : "E");
        this.DataRoot.AddSatellite(sat);
      }
    }
    #endregion

    #region ReadTransponder
    private void ReadTransponder(XmlNode node)
    {
      var mux = node["Multiplex"] ?? throw new FileLoadException("Missing Multiplex XML element");

      var muxData = SplitLines(mux);
      var muxIds = this.format == "1.1.0e" ? muxData["MuxID"] : muxData["MuxRowId"];
      var rfParmData = this.format != "1.1.0e" ? SplitLines(mux["RfParam"]) : null;
      var dvbsData = rfParmData != null ? SplitLines(mux["RfParam"]["DvbS"]) : null;

      for (int i = 0, c = muxIds.Length; i < c; i++)
      {
        Satellite sat = null;
        var transp = new Transponder(int.Parse(muxIds[i]));
        if (this.format == "1.1.0e")
        {
          var satId = int.Parse(muxData["ui2_satl_rec_id"][i]);
          transp.FrequencyInMhz = int.Parse(muxData["SysFreq"][i]);
          transp.SymbolRate = int.Parse(muxData["ui4_sym_rate"][i]);
          transp.Polarity = muxData["e_pol"][i] == "1" ? 'H' : 'V';
          sat = DataRoot.Satellites[satId];
        }
        else
        {
          transp.OriginalNetworkId = intParse(muxData["Onid"][i]);
          transp.TransportStreamId = intParse(muxData["Tsid"][i]);
          transp.FrequencyInMhz = int.Parse(rfParmData["Freq"][i]) / 1000;
          transp.Polarity = dvbsData["Pola"][i] == "H_L" ? 'H' : 'V';
          transp.SymbolRate = int.Parse(dvbsData["SymbolRate"][i]) / 1000;
        }

        this.DataRoot.AddTransponder(sat, transp);
      }
    }
    #endregion

    #region ReadServices110e
    private void ReadServices110e(XmlNode node)
    {
      var tsDescrNode = node["TS_Descr"] ?? throw new FileLoadException("Missing TS_Descr XML element");
      var tsData = SplitLines(tsDescrNode);

      var serviceNode = node["Service"] ?? throw new FileLoadException("Missing Service XML element");
      var svcData = SplitLines(serviceNode);
      var dvbData = SplitLines(serviceNode["dvb_info"]);

      for (int i = 0, c = svcData["ui2_svl_rec_id"].Length; i < c; i++)
      {
        var chan = new Channel(SignalSource.DvbS, i, serviceNode.ChildNodes[i]);
        chan.OldProgramNr = int.Parse(svcData["ui2_svl_rec_id"][i]);
        chan.IsDeleted = svcData["b_deleted_by_user"][i] != "1";
        var nwMask = int.Parse(svcData["ui4_nw_mask"][i]);
        chan.Hidden = (nwMask & 8) == 0;
        chan.Encrypted = (nwMask & 2048) != 0;
        chan.Encrypted = dvbData["t_free_ca_mode"][i] == "1";
        chan.Favorites = (Favorites) ((nwMask & 0xF0) >> 4);
        chan.ServiceId = int.Parse(svcData["ui2_prog_id"][i]);
        chan.Name = svcData["Name"][i];
        var muxId = int.Parse(svcData["MuxID"][i]);
        var transp = this.DataRoot.Transponder[muxId];
        chan.Transponder = transp;
        if (transp != null)
        {
          chan.FreqInMhz = transp.FrequencyInMhz;
          chan.SymbolRate = transp.SymbolRate;
          chan.Polarity = transp.Polarity;
          chan.Satellite = transp.Satellite?.Name;
          chan.SatPosition = transp.Satellite?.OrbitalPosition;
        }

        var tsIdx = int.Parse(svcData["ui2_tsl_rec_id"][i]) - 1;
        chan.TransportStreamId = int.Parse(tsData["Tsid"][tsIdx]);
        chan.OriginalNetworkId = int.Parse(tsData["Onid"][tsIdx]);

        chan.ServiceType = int.Parse(dvbData["ui1_sdt_service_type"][i]);
        chan.SignalSource |= LookupData.Instance.IsRadioOrTv(chan.ServiceType);

        this.DataRoot.AddChannel(this.satChannels, chan);
      }
    }
    #endregion

    #region ReadServices
    private void ReadServices(XmlNode node)
    {
      var serviceNode = node["Service"] ?? throw new FileLoadException("Missing Service XML element");
      var svcData = SplitLines(serviceNode);

      var progNode = node["Programme"] ?? throw new FileLoadException("Missing Programme XML element");
      var progData = SplitLines(progNode);

      var map = new Dictionary<int, Channel>();
      for (int i = 0, c = svcData["ServiceRowId"].Length; i < c; i++)
      {
        var rowId = int.Parse(svcData["ServiceRowId"][i]);
        var chan = new Channel(SignalSource.DvbS, rowId, serviceNode.ChildNodes[i]);
        map[rowId] = chan;
        chan.OldProgramNr = -1;
        chan.IsDeleted = true;
        chan.ServiceType = int.Parse(svcData["Type"][i]);
        chan.OriginalNetworkId = intParse(svcData["Onid"][i]);
        chan.TransportStreamId = intParse(svcData["Tsid"][i]);
        chan.ServiceId = intParse(svcData["Sid"][i]);
        chan.Name = svcData["Name"][i];
        var muxId = int.Parse(svcData["MuxRowId"][i]);
        var transp = this.DataRoot.Transponder[muxId];
        chan.Transponder = transp;
        if (transp != null)
        {
          chan.FreqInMhz = transp.FrequencyInMhz;
          chan.SymbolRate = transp.SymbolRate;
          chan.Polarity = transp.Polarity;
        }
        chan.SignalSource |= LookupData.Instance.IsRadioOrTv(chan.ServiceType);
        var att = intParse(svcData["Attribute"][i]);
        chan.Encrypted = (att & 8) != 0;
        this.DataRoot.AddChannel(this.satChannels, chan);
      }

      for (int i = 0, c = progData["ServiceRowId"].Length; i < c; i++)
      {
        var rowId = int.Parse(progData["ServiceRowId"][i]);
        var chan = map.TryGet(rowId);
        if (chan == null)
          continue;
        chan.IsDeleted = false;
        chan.OldProgramNr = int.Parse(progData["No"][i]);
        var flag = int.Parse(progData["Flag"][i]);
        chan.Favorites = (Favorites)(flag & 0x0F);
      }
    }
    #endregion


    #region SplitLines
    private Dictionary<string, string[]> SplitLines(XmlNode parent)
    {
      var dict = new Dictionary<string, string[]>();
      foreach (XmlNode node in parent.ChildNodes)
      {
        if (node.Attributes["loop"] == null)
          continue;
        var lines = node.InnerText.Trim('\n').Split('\n');
        dict[node.LocalName] = lines;
      }

      return dict;
    }
    #endregion

    #region ReadChecksum()

    private void ReadChecksum(XmlNode node)
    {
      byte[] data;
      int start;
      int end;
      uint expectedCrc;

      if (this.format == "1.1.0e")
      {
        // files with the typo-element "<FormateVer>1.1.0</FormateVer>" differ in several ways from all other files (including <FormatVer>1.1.0</FormatVer>):
        // "\n" after the closing <SdbXml> Tag is included in the checksum, the checksum has no 0x prefix and the bytes are used as-is for the calculation, without any XML cleanup
        data = this.content;
        start = this.IndexOf("<SdbXml>");
        end = this.IndexOf("</SdbXml>") + 10; // including the \n at the end
        expectedCrc = uint.Parse(node.InnerText, NumberStyles.HexNumber); // no 0x prefix
      }
      else
      {
        start = this.textContent.IndexOf("<SdbXml>");
        end = this.textContent.IndexOf("</SdbXml>") + 9;
        var text = this.textContent.Substring(start, end - start);
        text = text.Replace("\r\n", "\n");
        text = text.Replace(" />", "/>");
        text = text.Replace("&lt;", "&#60;");
        text = text.Replace("&gt;", "&#62;");
        text = text.Replace("&quot;", "&#34;");
        text = text.Replace("&amp;", "&#38;");
        text = text.Replace("&apos;", "&#39;");
        text = text.Replace("'", "&#39;");
        data = Encoding.UTF8.GetBytes(text);
        start = 0;
        end = data.Length;
        expectedCrc = uint.Parse(node.InnerText.Substring(2), NumberStyles.HexNumber);
      }

      uint crc = 0xFFFFFFFF;
      for (int i = start; i < end; i++)
      {
        var b = data[i];
        crc = (crc << 8) ^ Crc32Table[b ^ (crc >> 24)];
      }
      crc = ~crc;

      if (crc != expectedCrc)
        throw new FileLoadException($"Invalid checksum: expected 0x{expectedCrc:x8}, calculated 0x{crc:x8}");
    }

    private int IndexOf(string marker)
    {
      var bytes = Encoding.ASCII.GetBytes(marker);
      var len = bytes.Length;
      int i = -1, c = this.content.Length - len;
      for (; ; )
      {
        i = Array.IndexOf(this.content, bytes[0], i + 1);
        if (i < 0)
          return -1;

        int j;
        for (j = 1; j < len; j++)
        {
          if (this.content[i + j] != bytes[j])
            break;
        }

        if (j == len)
          return i;
      }
    }
    #endregion




    #region Save()
    public override void Save(string tvOutputFile)
    {
      throw new NotImplementedException("Sorry, but Sony lists are currently read-only. Support for writing is coming soon.");
      foreach (var list in this.DataRoot.ChannelLists)
      {
        
        foreach (var channel in list.Channels)
        {
          var ch = channel as Channel;
          if (ch == null) continue; // ignore proxy channels from reference lists
          var nameBytes = Encoding.UTF8.GetBytes(ch.Name);
          bool nameNeedsEncoding = nameBytes.Length != ch.Name.Length;
          string mapType = "";
          
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
                node.InnerText = ch.IsDeleted || ch.IsDisabled ? "1" : "0";
                break;
              case "isDeleted":
                node.InnerText = ch.IsDeleted ? "1" : "0";
                break;
              case "isUserSelCHNo":
                if (ch.NewProgramNr != ch.OldProgramNr)
                  node.InnerText = "1";
                break;
              case "mapType":
                mapType = node.InnerText;
                break;
              case "mapAttr":
                if (mapType == "1")
                  node.InnerText = ((int) ch.Favorites).ToString();
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
        if (!xml.EndsWith("\r\n"))
          xml += "\r\n";
        File.WriteAllText(tvOutputFile, xml, settings.Encoding);
      }
    }
    #endregion


    #region intParse
    private int intParse(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return 0;
      if (input.StartsWith("0x"))
        return int.Parse(input.Substring(2), NumberStyles.HexNumber);
      if (int.TryParse(input, out var value))
        return value;
      return 0;
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
