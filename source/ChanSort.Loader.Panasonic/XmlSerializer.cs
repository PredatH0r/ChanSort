using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  /*

  MediaTek based Android TVs (e.g. Panasonic 2020 and later, Nokia, ...) use the same unstandardized compressed .bin binary format as many 
  Philips lists and some Sharp TVs.
  Additionally it exports a .xml file with very limited information that only includes channel numbers and channel names truncated at 8 bytes. 
  
  This truncation makes it impossible for a user to distinguish between channels that have longer names like "Sky Bundesliga ...", therefore
  this loader adds the "SvlId" as the ShortName. This SvlId is probably a "service list id" and refers to a a data record inside the .bin file.
  The truncation can also happen in the middle of a multi-type UTF-8 character sequence. Non-latin characters, including German umlauts or all
  cyrillic characters require 2 bytes/character, effectively reducing the channel name length to 4-8 characters.

  Another severe issue with these files is that XML special characters in channel names are not escaped properly. Some preprocessing is required
  in order to guess if a "&" is meant as an &amp; data value or an XML attribute. It's likely that < and > inside channel names have the same problem.

  When the TV has channels from various sources, it is not possible to determine to which internal source a channel belongs, making sorting of 
  sub-lists more or less impossible.

  */
  class XmlSerializer : SerializerBase
  {
    private readonly Dictionary<string, ChannelList> channelLists = new();
    public XmlDocument doc;
    public readonly IniFile ini;

    #region ctor()
    public XmlSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = false;
      this.Features.CanHideChannels = false;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.AllowGapsInFavNumbers = true;
      this.Features.CanEditFavListNames = false;

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);
    }
    #endregion

    #region GetOrCreateList()
    private ChannelList GetOrCreateList(string name)
    {
      if (this.channelLists.TryGetValue(name, out var list))
        return list;
      
      list = new ChannelList(SignalSource.All, name);
      this.channelLists[name] = list;
      var cols = list.VisibleColumnFieldNames;
      cols.Clear();
      cols.Add("Position");
      cols.Add("OldPosition");
      cols.Add(nameof(ChannelInfo.Name));
      cols.Add(nameof(ChannelInfo.ShortName));
      this.DataRoot.AddChannelList(list);
      return list;
    }
    #endregion

    #region Load()
    public override void Load()
    {
      bool fail = false;
      try
      {
        doc = new XmlDocument();
        var content = File.ReadAllBytes(this.FileName);
        var textContent = Encoding.UTF8.GetString(content);
        textContent = FixUnescapedXmlChars(textContent);

        var settings = new XmlReaderSettings
        {
          CheckCharacters = false,
          IgnoreProcessingInstructions = true,
          ValidationFlags = XmlSchemaValidationFlags.None,
          DtdProcessing = DtdProcessing.Ignore
        };
        using var reader = XmlReader.Create(new StringReader(textContent), settings);
        doc.Load(reader);
      }
      catch
      {
        fail = true;
      }

      var root = doc.FirstChild;
      if (root is XmlDeclaration)
        root = root.NextSibling;
      if (fail || root == null || root.LocalName != "ChannelList" || !root.HasChildNodes || root.ChildNodes[0].LocalName != "ChannelInfo")
        throw LoaderException.TryNext("File is not a supported Panasonic XML file");

      foreach (XmlNode child in root.ChildNodes)
      {
        switch (child.LocalName)
        {
          case "ChannelInfo":
            this.ReadChannel(child);
            break;
        }
      }
    }
    #endregion

    #region FixUnescapedXmlChars()
    private string FixUnescapedXmlChars(string textContent)
    {
      var sb = new StringBuilder((int)(textContent.Length * 1.1));
      var inQuotes = false;
      foreach (var c in textContent)
      {
        if (c == '\"')
          inQuotes = !inQuotes;

        if (c == '&' && inQuotes)
          sb.Append("&amp;");
        else if (c == '<' && inQuotes)
          sb.Append("&lt;");
        else if (c == '>' && inQuotes)
          sb.Append("&gt;");
        else
          sb.Append(c);
      }

      return sb.ToString();
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(XmlNode node)
    {
      var channelType = node.Attributes?["ChannelType"]?.InnerText;
      var list = this.GetOrCreateList(channelType);
      var chan = new XmlChannel(list.Count, node);
      DataRoot.AddChannel(list, chan);
    }
    #endregion


    #region Save()

    public override void Save()
    {
      var sec = ini.GetSection("channel_list.xml");
      var reorder = sec?.GetBool("reorderRecordsByChannelNumber", true) ?? true;
      var setIsModified = sec?.GetBool("setIsModified", false) ?? false;

      foreach (var list in this.DataRoot.ChannelLists)
      {
        var seq = reorder ? list.Channels.OrderBy(c => c.NewProgramNr).ThenBy(c => c.RecordIndex).ToList() : list.Channels;
        XmlNode prevNode = null;
        foreach (var chan in seq)
        {
          if (chan is not XmlChannel ch)
            continue;
          ch.Node.Attributes["ChannelNumber"].InnerText = ch.NewProgramNr.ToString();
          if (setIsModified && ch.NewProgramNr != ch.OldProgramNr)
            ch.Node.Attributes["IsModified"].InnerText = "1";
          if (reorder)
          {
            var parent = ch.Node.ParentNode;
            parent.RemoveChild(ch.Node);
            parent.InsertAfter(ch.Node, prevNode);
            prevNode = ch.Node;
          }
        }
      }

      var xmlSettings = new XmlWriterSettings();
      xmlSettings.Encoding = new UTF8Encoding(false);
      xmlSettings.CheckCharacters = false;
      xmlSettings.Indent = true;
      xmlSettings.IndentChars = "";
      xmlSettings.NewLineHandling = NewLineHandling.None;
      xmlSettings.NewLineChars = "\n";
      xmlSettings.OmitXmlDeclaration = true;

      // write to a string so that we can patch the result to be binary identical to the original file (if there are no changes)
      using var stringWriter = new StringWriter();
      using var w = XmlWriter.Create(stringWriter, xmlSettings);
      doc.WriteTo(w);
      w.Flush();
      stringWriter.Write('\n'); // original file has a trailing \x0A
      var xml = stringWriter.ToString();
      xml = UnescapeXmlChars(xml); // create same broken XML as the original export with unescaped entities
      xml = xml.Replace(" />", "/>"); // original file has no space before the element end
      File.WriteAllText(this.FileName, xml, xmlSettings.Encoding);
    }

    #endregion

    #region UnescapeXmlChars()
    /// <summary>
    /// Generate the same broken XML with unescaped XML-entities as the original Panasonic XML export does (i.e. literal '&' character in channel names)
    /// </summary>
    private string UnescapeXmlChars(string xml)
    {
      bool inQuotes = false;
      bool inEntity = false;
      string entity = "";
      var sb = new StringBuilder(xml.Length);
      foreach (var c in xml)
      {
        if (inEntity)
        {
          if (c == ';')
          {
            switch (entity)
            {
              case "lt":
                sb.Append("<");
                break;
              case "gt":
                sb.Append(">");
                break;
              case "amp":
                sb.Append("&");
                break;
            }

            inEntity = false;
          }
          else
            entity += c;
          continue;
        }

        if (c == '"')
          inQuotes = !inQuotes;

        if (c == '&' && inQuotes)
        {
          inEntity = true;
          entity = "";
          continue;
        }

        sb.Append(c);
      }

      return sb.ToString();
    }
    #endregion


    #region class XmlChannel

    class XmlChannel : ChannelInfo
    {
      internal XmlNode Node;

      public XmlChannel(int index, XmlNode node) : base(0, index, 0, null)
      {
        this.Node = node;

        this.OldProgramNr = int.Parse(node.Attributes["ChannelNumber"]?.InnerText);
        this.Name = node.Attributes["ChannelName"].InnerText;
        var svlId = node.Attributes["SvlId"].InnerText;
        this.ShortName = $"SvlId: {svlId}";
      }

    }
    #endregion
  }
}
