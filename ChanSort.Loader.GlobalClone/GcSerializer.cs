using System.IO;
using System.Text;
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
    private DvbStringDecoder dvbStringDecoder = new DvbStringDecoder(Encoding.Default);

    #region ctor()
    public GcSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = false;

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
        doc.Load(this.FileName);
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
          case "CHANNEL":
            this.ReadChannelLists(child);
            break;
        }
      }
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
        foreach (XmlNode info in itemNode.ChildNodes)
          ParseChannelInfoNode(info, ch);

        var list = this.DataRoot.GetChannelList(ch.SignalSource);
        this.DataRoot.AddChannel(list, ch);
      }
    }
    #endregion

    #region ParseChannelInfoNode()
    private void ParseChannelInfoNode(XmlNode info, ChannelInfo ch)
    {
      switch (info.LocalName)
      {
        // common to ATV and DTV
        case "prNum":
          ch.OldProgramNr = int.Parse(info.InnerText) & 0x3FFF;
          break;
        case "vchName":
          var name = ParseName(info.InnerText);
          if (string.IsNullOrWhiteSpace(ch.Name) || !string.IsNullOrWhiteSpace(name)) // avoid overwriting valid name from <hexVchName> with empty <vchName>
            ch.Name = name;
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
        case "isBlocked":
          ch.Lock = int.Parse(info.InnerText) == 1;
          break;
        case "isSkipped":
          ch.Skip = int.Parse(info.InnerText) == 1;
          break;

        // ATV
        case "pllData":
          ch.FreqInMhz = (decimal)int.Parse(info.InnerText) / 20;
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
        case "isDisabled":
          ch.IsDeleted = int.Parse(info.InnerText) != 0;
          break;

        // not present in all XML files
        case "hexVchName":
          var bytes = Tools.HexDecode(info.InnerText);
          string longName, shortName;
          dvbStringDecoder.GetChannelNames(bytes, 0, bytes.Length, out longName, out shortName);
          ch.Name = longName;
          ch.ShortName = shortName;
          break;
      }
    }
    #endregion

    private string ParseName(string input)
    {
      return input;
    }

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
              case "isDisabled":
                node.InnerText = ch.IsDeleted ? "1" : "0";
                break;
              case "isUserSelCHNo":
                node.InnerText = "1";
                break;
            }
          }
        }
      }
      
      doc.Save(tvOutputFile);
    }
    #endregion

  }
}
