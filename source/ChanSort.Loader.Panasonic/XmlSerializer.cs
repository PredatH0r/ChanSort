using System;
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

  Panasonic Android TVs (2020 and later) use the same unstandardized compressed .bin binary format as many Philips lists and some Sharp TVs.
  Additionally it exports a .xml file with very limited information that only includes channel numbers and channel names truncated at 8 characters.
  
  This truncation makes it impossible for a user to distinguish between channels that have longer names like "Sky Bundesliga ...", therefore
  this loader adds the "SvlId" as the ShortName. This SvlId is probably a "service list id" and refers to a a data record inside the .bin file.

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
      this.Features.CanSaveAs = true;
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
        throw new FileLoadException("File is not a supported Panasonic XML file");

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

    public override void Save(string tvOutputFile)
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
      using var w = XmlWriter.Create(tvOutputFile, xmlSettings);
      doc.WriteTo(w);
      this.FileName = tvOutputFile;
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
