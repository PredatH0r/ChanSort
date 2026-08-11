using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.Android
{
  public class NovalerXmlSerializer : SerializerBase
  {
    private readonly ChannelList dvbsTvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "DVB-S TV");
    private XmlDocument doc;
    private bool hasBom;
    private string newline = "\n";

    public NovalerXmlSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = false;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.FavoritesMode = FavoritesMode.Flags;
      this.Features.MaxFavoriteLists = 8;
      this.Features.CanEditFavListNames = false;
      this.Features.CanHaveGaps = true;

      this.DataRoot.AddChannelList(this.dvbsTvChannels);

      this.dvbsTvChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
      this.dvbsTvChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceTypeName));
      this.dvbsTvChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ChannelOrTransponder));
      this.dvbsTvChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
    }

    public override void Load()
    {
      var content = File.ReadAllBytes(this.FileName);
      this.hasBom = content.Length >= 3 && content[0] == 0xef && content[1] == 0xbb && content[2] == 0xbf;
      var text = Encoding.UTF8.GetString(content, this.hasBom ? 3 : 0, content.Length - (this.hasBom ? 3 : 0));
      this.newline = text.Contains("\r\n") ? "\r\n" : "\n";

      this.doc = new XmlDocument { PreserveWhitespace = true };
      try
      {
        this.doc.LoadXml(text);
      }
      catch
      {
        throw LoaderException.TryNext(ERR_UnknownFormat);
      }

      var root = this.doc.DocumentElement;
      if (root == null || root.LocalName != "LIST" || root.SelectSingleNode("video_list/channel") == null)
        throw LoaderException.TryNext(ERR_UnknownFormat);

      var index = 0;
      foreach (XmlElement element in root.SelectNodes("video_list/channel"))
      {
        var name = GetString(element, "name");
        var oldProgNr = GetInt(element, "lcn", index + 1);
        var channel = new Channel(element, index, oldProgNr, name)
        {
          SignalSource = SignalSource.DvbS | SignalSource.Tv,
          Lock = GetInt(element, "locked") != 0,
          Favorites = (Favorites)(byte)GetInt(element, "fav"),
          Encrypted = GetInt(element, "ca_type") != 0,
          OriginalNetworkId = GetInt(element, "network_id"),
          TransportStreamId = GetInt(element, "stream_id"),
          ServiceId = GetInt(element, "service_id"),
          PcrPid = GetInt(element, "pcr"),
          VideoPid = GetInt(element, "video"),
          AudioPid = GetInt(element, "audio"),
          ServiceType = GetInt(element, "video_type"),
          FreqInMhz = GetInt(element, "freq"),
          SymbolRate = GetInt(element, "symbolrate"),
          Polarity = GetString(element, "polar").Length > 0 ? GetString(element, "polar")[0] : ' ',
          SatPosition = GetString(element, "angel"),
          Satellite = GetString(element, "angel"),
          RecordOrder = index
        };
        channel.NewProgramNr = oldProgNr;
        this.DataRoot.AddChannel(this.dvbsTvChannels, channel);
        ++index;
      }
    }

    public override void Save()
    {
      foreach (var channel in this.dvbsTvChannels.Channels)
      {
        if (channel.IsProxy)
          continue;

        var ch = (Channel)channel;
        if (ch.IsDeleted)
          ch.Element.ParentNode?.RemoveChild(ch.Element);
        else
        {
          ch.Element.SetAttribute("name", ch.Name);
          ch.Element.SetAttribute("lcn", ch.NewProgramNr.ToString(CultureInfo.InvariantCulture));
          ch.Element.SetAttribute("locked", ch.Lock ? "1" : "0");
          ch.Element.SetAttribute("fav", ((int)ch.Favorites).ToString(CultureInfo.InvariantCulture));
        }
      }

      var settings = new XmlWriterSettings
      {
        Encoding = new UTF8Encoding(this.hasBom),
        Indent = false,
        NewLineChars = this.newline,
        OmitXmlDeclaration = false
      };
      using var writer = XmlWriter.Create(this.FileName, settings);
      this.doc.Save(writer);
    }

    private static int GetInt(XmlElement element, string name, int defaultValue = 0)
    {
      return int.TryParse(element.GetAttribute(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    }

    private static string GetString(XmlElement element, string name)
    {
      return element.GetAttribute(name) ?? "";
    }

    private class Channel : ChannelInfo
    {
      public Channel(XmlElement element, int index, int oldProgNr, string name) : base(SignalSource.DvbS | SignalSource.Tv, index, oldProgNr, name)
      {
        this.Element = element;
      }

      public XmlElement Element { get; }
    }
  }
}
