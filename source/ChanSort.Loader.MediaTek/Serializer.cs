using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.MediaTek;

public class Serializer : SerializerBase
{
  /*
   * Some Android based TVs export an XML file with the format described below.
   * Examples are Philips channel list formats 120 and 125 and Sony BRAVIA 7 (2024).
   * However there are differences between Philips and Sony:
   * - Sony lacks a number of XML elements
   * - Sony uses separate lists for TV, radio and data, while Philips puts them in a combine list. This is controlled by the MultiBank-setting in <internal><scan>
   *
   * <service_list_transfer>
   *   <service_list_infos>
   *     <service_list_info service_list_id="...">
   *       <service_info>
   *         <major_channel_number>
   *         <user_edit_flag>
   *         <service_name>
   *         <sdt_service_type> 1=TV, 2=Radio
   *         <std_stream_content>
   *         <std_stream_content_ext>
   *         <std_stream_component_type>
   *         <record_id>service://SERVICE_LIST_GENERAL_SATELLITE/[service_list_id]/[major_channel_number]
   *         <visible_service>
   *
   *         The following elements exist in the Philips lists but not in the Sony's sdb.xml
   *
   *         <service_id> SID
   *         <transport_stream_id> TSID
   *         <network_id> NID
   *         <frequency> (DVB-S2: MHz)
   *         <original_network_id> ONID
   *         <symbol_rate>
   *         <modulation>
   *         <polarization>
   *         <lock>
   *         <scrambled> 0=false
   *         <satelliteName>
   *   <internal>
   *     <summary> (base64 encoded Java serialized binary)
   *     <scan> (base64 encoded Java serialized binary, containing several scan settings)
   *     <service_database> (base64 encoded Java serialized binary, which contains proprietary MediaTek compressed/encrypted cl_Zip data)
   */

  private XmlDocument doc;
  private byte[] content;
  private string textContent;
  private readonly StringBuilder fileInfo = new();
  private bool splitTvRadioData; // controlled by the MultiBank setting inside the <scan> Java serialized stream; Philips=false, Sony=true
  private bool usesLcn;
  public readonly Dictionary<string, string> ScanParameters = new();


  #region ctor()
  public Serializer(string inputFile) : base(inputFile)
  {
    this.Features.ChannelNameEdit = ChannelNameEditMode.All;
    this.Features.DeleteMode = DeleteMode.NotSupported;
    this.Features.FavoritesMode = FavoritesMode.None;
    this.Features.CanSkipChannels = false;
    this.Features.CanLockChannels = true;
    this.Features.CanHideChannels = false; // unclear how "visible_service" works (3 for normal channels, 1 for hidden?)
    this.Features.CanSaveAs = true;
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
    if (fail || root == null || root.LocalName != "service_list_transfer")
      throw LoaderException.TryNext("\"" + this.FileName + "\" is not a supported MediaTek XML file");

    var nodesByName = new Dictionary<string, XmlNode>();
    foreach (XmlNode child in root.ChildNodes)
      nodesByName[child.LocalName] = child;
    
    // read <internal><scan> first to determine this.splitTvRadioData
    if (nodesByName.TryGetValue("internal", out var node))
    {
      foreach (XmlNode childNode in node.ChildNodes)
      {
        if (childNode.LocalName == "scan")
          ReadScanElement(Convert.FromBase64String(childNode.InnerText));
      }
    }

    // now read the channels
    if (nodesByName.TryGetValue("service_list_infos", out node))
      ReadServiceListInfos(node);
  }
  #endregion

  #region ReadScanElement()

  private static readonly byte[] EnumMarker = [0, 0, 0, 0, 0, 0, 0, 0, 0x12, 0, 0, 0x78, 0x71, 0, 0x7e, 0]; // , 0x0e, 0x74 philips; , 0x14, 0x74 sony;
  private void ReadScanElement(byte[] data)
  {
    /*
     * The base64 encoded <scan> element contains serialized Java objects.
     * The exact binary data layout is unknown and varies between brands and maybe firmware versions.
     * Some data in it gives clues about LCNs are used and whether a FULL scan was used to setup the channel list, whether TV,radio and data channels are in a combined list or separated, ...
     *
     * To detectd values, we look for: (uiLen "com.[mediatek|sony].dtv.broadcast.middleware.scan.engine.ScanSettings$<name>") \x00{8} \x12 \x00\x00\x78\x71 \x00\x7e \x00\x?? \x74 (uiLen "<value>")
     */

    var str = Encoding.ASCII.GetString(data);
    for (int idx = str.IndexOf("com.", StringComparison.InvariantCulture); idx >= 2; idx = str.IndexOf("com.", idx, StringComparison.InvariantCulture))
    {
      // get the setting name
      var len = data[idx - 2] * 256 + data[idx - 1];
      var name = str.Substring(idx, len);
      var i = name.IndexOf('$'); // only care about the name part after the $-sign
      if (i >= 0)
        name = name.Substring(i + 1);

      // check for the EnumMarker, followed by 2 bytes (first of them varies between Philips and sony)
      idx += len;
      if (idx + EnumMarker.Length + 2 >= data.Length)
        continue;
      if (Tools.MemComp(data, idx, EnumMarker) != 0)
        continue;
      idx += EnumMarker.Length + 2;

      // get the enum value
      len = data[idx] * 256 + data[idx + 1];
      idx += 2;
      if (idx + len >= data.Length)
        continue;
      var value = str.Substring(idx, len);
      idx += len;

      this.ScanParameters[name] = value;
      this.fileInfo.AppendLine($"{name}: {value}");

      // handle relevant settings
      if (name == "MultiBank")
        splitTvRadioData |= value == "SEPARATE_TV_RADIO_DATA";
      else if (name == "LcnType")
        usesLcn |= value != "LCNS_DISABLED";
    }
  }
  #endregion

  #region ReadServiceListInfos()
  private void ReadServiceListInfos(XmlNode serviceListInfosNode)
  {
    foreach (var sli in serviceListInfosNode.ChildNodes)
    {
      if (sli is XmlElement serviceListInfo)
        this.ReadServiceList(serviceListInfo);
    }

    foreach (var list in this.DataRoot.ChannelLists)
    {
      list.VisibleColumnFieldNames = ChannelList.DefaultVisibleColumns.ToList();
      list.VisibleColumnFieldNames.Remove("PcrPid");
      list.VisibleColumnFieldNames.Remove("VideoPid");
      list.VisibleColumnFieldNames.Remove("AudioPid");
      list.VisibleColumnFieldNames.Remove("ShortName");
    }
  }
  #endregion

  #region ReadServiceList()
  private void ReadServiceList(XmlElement node)
  {
    var ss = SignalSource.Dvb;
    var slt = node.GetAttribute("service_list_type");
    if (slt.Contains("SATELLITE"))
      ss |= SignalSource.Sat;
    else if (slt.Contains("CABLE"))
      ss |= SignalSource.Cable;
    else if (slt.Contains("TERR"))
      ss |= SignalSource.Antenna;

    // service_list_id example: SERVICE_LIST_GENERAL_SATELLITE/17
    //var serviceListId = node.GetAttribute("service_list_id");

    int idx = 0;
    foreach (var child in node.ChildNodes)
    {
      if (!(child is XmlElement si && si.LocalName == "service_info"))
        continue;

      ReadChannel(si, ss, idx++);
    }
  }
  #endregion

  #region ReadChannel()

  private void ReadChannel(XmlElement si, SignalSource ss, int idx)
  {
    // record_id example: service://SERVICE_LIST_GENERAL_SATELLITE/17/1
    var recIdUri = si.GetElementString("record_id") ?? "";
    var i = recIdUri.LastIndexOf('/');
    var recId = int.Parse("0" + recIdUri.Substring(i + 1));

    var chan = new Channel(ss, recId, -1, "", si);
    chan.RecordOrder = idx;

    chan.OldProgramNr = si.GetElementInt("major_channel_number");
    // user_edit_flag ("none" in all observed records, must be "update" for the TV to process the record)
    chan.Name = si.GetElementString("service_name");
    chan.ServiceType = si.GetElementInt("sdt_service_type");
    chan.Hidden = si.GetElementInt("visible_service") != 3; // visible_service ("3" in most observed record, "1" in some others)
    chan.ServiceId = si.GetElementInt("service_id");
    chan.TransportStreamId = si.GetElementInt("transport_stream_id");
    chan.FreqInMhz = si.GetElementInt("frequency");
    chan.OriginalNetworkId = si.GetElementInt("original_network_id");
    chan.SymbolRate = si.GetElementInt("symbol_rate");
    // modulation (not used by ChanSort)
    var pol = si.GetElementInt("polarization");
    chan.Polarity = pol == 1 ? 'H' : pol == 2 ? 'V' : '\0';
    chan.Lock = si.GetElementInt("lock") != 0;
    chan.Encrypted = si.GetElementInt("scrambled") != 0;
    chan.Satellite = si.GetElementString("satelliteName");

    if ((ss & SignalSource.Antenna) != 0)
      chan.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(chan.FreqInMhz).ToString();
    else if ((ss & SignalSource.Cable) != 0)
      chan.ChannelOrTransponder = LookupData.Instance.GetDvbcTransponder(chan.FreqInMhz).ToString();


    if (splitTvRadioData)
      ss |= LookupData.Instance.IsRadioTvOrData(chan.ServiceType);
    else
      ss |= SignalSource.Tv | SignalSource.Radio | SignalSource.Data;


    var list = DataRoot.GetChannelList(ss);
    if (list == null)
    {
      var name = (ss & SignalSource.Antenna) != 0 ? "Antenna" : (ss & SignalSource.Cable) != 0 ? "Cable" : (ss & SignalSource.Sat) != 0 ? "Sat" : (ss & SignalSource.Ip) != 0 ? "IP" : "Other";
      if (splitTvRadioData)
        name += " " + ((ss & SignalSource.Tv) != 0 ? " TV" : (ss & SignalSource.Radio) != 0 ? " Radio" : " Data");
        
      list = new ChannelList(ss, name);
      if (this.usesLcn)
        list.ReadOnly = true;
      this.DataRoot.AddChannelList(list);
    }

    var elements = si.GetElementsByTagName("major_channel_number", si.NamespaceURI);
    list.ReadOnly |= elements.Count == 1 && elements[0].Attributes!["editable", si.NamespaceURI].InnerText == "false";

    list.AddChannel(chan);
    chan.SignalSource = ss;
  }
  #endregion


  #region GetFileInformation()

  public override string GetFileInformation()
  {
    var txt = base.GetFileInformation();
    return txt + "\n\n" + this.fileInfo;
  }

  #endregion


  #region Save()
  public override void Save()
  {
    // if splitTvRadioData is set, the 3 lists must be recombined and sorted together as a single list; there may still be multiple lists depending on input sources (DVB-T/C/S)
    var recombinedLists = new Dictionary<SignalSource, List<ChannelInfo>>();
    foreach (var list in this.DataRoot.ChannelLists)
    {
      if (list.Channels.Count == 0 || list.ReadOnly)
        continue;

      if (this.splitTvRadioData)
      {
        if (!recombinedLists.TryGetValue(list.SignalSource & ~SignalSource.MaskTvRadioData, out var combinedList))
        {
          combinedList = new List<ChannelInfo>();
          recombinedLists[list.SignalSource & ~SignalSource.MaskTvRadioData] = combinedList;
        }

        combinedList.AddRange(list.Channels);
      }
      else
      {
        recombinedLists.Add(list.SignalSource, list.Channels.ToList());
      }
    }

    // sort the channels in the recombined lists
    foreach (var list in recombinedLists.Values)
    {
      XmlElement serviceListInfoNode = null;
      foreach (var chan in list.OrderBy(c => c.NewProgramNr).ThenBy(c => c.OldProgramNr).ThenBy(c => c.RecordIndex))
      {
        if (chan is not Channel ch || ch.IsProxy)
          continue;

        var si = ch.Xml;

        // reorder nodes physically: first remove all, then add them 1-by-1
        if (serviceListInfoNode == null)
        {
          serviceListInfoNode = (XmlElement)si.ParentNode;
          while (serviceListInfoNode!.HasChildNodes)
            serviceListInfoNode.RemoveChild(serviceListInfoNode.FirstChild);

          serviceListInfoNode.SetAttribute("lcn_type", "LCNS_ENABLED");
        }
        serviceListInfoNode.AppendChild(si);

        si["major_channel_number"]!.InnerText = ch.NewProgramNr.ToString();
        si["user_edit_flag"]!.InnerText = "update";
        if (ch.IsNameModified)
          si["service_name"]!.InnerText = ch.Name;
        // si["visible_service"]!.InnerText = ch.Hidden ? "1" : "3"; // reported to have no effect in Philips v125 lists
        if (si["lock"] != null) // Sony lists don't have this elements
          si["lock"].InnerText = ch.Lock ? "1" : "0";
      }
    }

    var filePath = this.SaveAsFileName ?? this.FileName;
    var settings = new XmlWriterSettings();
    settings.Indent = true;
    settings.Encoding = new UTF8Encoding(false);
    using var w = XmlWriter.Create(filePath, settings);
    this.doc.WriteTo(w);
    this.FileName = filePath;
  }
  #endregion
}
