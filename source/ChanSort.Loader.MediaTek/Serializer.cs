using ChanSort.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

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
   *     <service_database> (base64 encoded Java serialized binary, which contains proprietary MediaTek compressed cl_Zip data)
   */

  private XmlDocument doc;
  private byte[] content;
  private string textContent;
  private readonly StringBuilder fileInfo = new();
  private bool splitTvRadioData; // controlled by the MultiBank setting inside the <scan> Java serialized stream; Philips=false, Sony=true
  private bool usesLcn;
  private byte[] scanData;
  public readonly Dictionary<string, string> ScanParameters = new();
  public bool DecodeSvl { get; set; } = true; // used with StatsCollector tools to ignore errors in the binary data

  private List<SvlTable> svlTables; // one per service list (e.g. satellite + terrestrial), null if there is no binary data
  private readonly Dictionary<string, SvlTable> svlByListId = new();
  private readonly Dictionary<(string, int), byte[]> svlRecordById = new(); // key: (service list id, record_id)
  private XmlElement serviceDatabaseNode;
  private byte[] serviceDatabaseData;

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
        else if (childNode.LocalName == "service_database")
          ReadServiceDatabase(childNode);
      }

      if (this.DeveloperMode)
        this.WriteDebugFiles(this.FileName);
    }

    // now read the channels
    if (nodesByName.TryGetValue("service_list_infos", out node))
      ReadServiceListInfos(node);

    // verify that there is a 1:1 mapping between the number of service_info elements and the number of svl records
    var xmlChannelCount = this.DataRoot.ChannelLists.SelectMany(l => l.Channels).Count();
    if (svlTables != null && svlTables.Sum(t => t.Records.Count) != xmlChannelCount)
      throw LoaderException.Fail("Mismatching channel count in text and binary data");
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
     * To detect values, we look for: (uiLen "com.[mediatek|sony].dtv.broadcast.middleware.scan.engine.ScanSettings$<name>") \x00{8} \x12 \x00\x00\x78\x71 \x00\x7e \x00\x?? \x74 (uiLen "<value>")
     */

    this.scanData = data;
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
        splitTvRadioData |= value == "SEPARATE_TV_RADIO_DATA"; // can also be "COMMON"
      else if (name == "LcnType")
        usesLcn |= value != "LCNS_DISABLED";
    }
  }
  #endregion

  #region ReadServiceDatabase()
  private void ReadServiceDatabase(XmlNode xmlNode)
  {
    if (!this.DecodeSvl)
      return;
    this.serviceDatabaseNode = (XmlElement)xmlNode;
    this.serviceDatabaseData = Convert.FromBase64String(xmlNode.InnerText);

    this.LoadSvlTables();
    if (this.svlTables != null)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None; // names live in the binary name pool, untested
      // Philips exports MultiBank=COMMON and numbers TV/radio/data in one range, its XmlSerializer expects a single list per source even though the records have different groups.
      // MultiBank alone isn't reliable, so COMMON is only trusted when no program number is used by more than one group
      var isCommon = this.ScanParameters.TryGetValue("MultiBank", out var multiBank) && multiBank == "COMMON"
        && svlTables.All(t => t.Records.Where(r => SvlTable.GetProgramNr(r) > 0).GroupBy(SvlTable.GetProgramNr).All(g => g.Select(SvlTable.GetGroup).Distinct().Count() == 1));
      if (!isCommon)
        this.splitTvRadioData = svlTables.SelectMany(t => t.Records).Select(SvlTable.GetGroup).Distinct().Count() > 1;
    }
  }

  private void LoadSvlTables()
  {
    var tables = SvlTable.LoadAll(serviceDatabaseData);
    this.svlTables = tables.Count == 0 ? null : tables;
    this.svlByListId.Clear();
    this.svlRecordById.Clear();
    foreach (var table in tables)
    {
      if (table.ServiceListId == null)
        throw LoaderException.Fail("Svl: service list id not found");
      svlByListId[table.ServiceListId] = table;
      foreach (var rec in table.Records)
        svlRecordById[(table.ServiceListId, SvlTable.GetRecordId(rec))] = rec;
    }
  }

  /// <summary>"service://SERVICE_LIST_GENERAL_SATELLITE/17/1" -> "SERVICE_LIST_GENERAL_SATELLITE/17"</summary>
  private static string GetServiceListId(XmlElement si)
  {
    var uri = si.GetElementString("record_id") ?? "";
    var start = uri.IndexOf("://", StringComparison.Ordinal) + 3;
    var end = uri.LastIndexOf('/');
    return start >= 3 && end > start ? uri.Substring(start, end - start) : "";
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


    var listId = GetServiceListId(si);
    if (svlRecordById.TryGetValue((listId, recId), out var rec))
    {
      chan.ServiceId = SvlTable.GetServiceId(rec);  // Sony XML has no <service_id>, useful for reference lists
      if (splitTvRadioData)                         // the TV numbers TV/radio/data by the group byte,
      {                                             // not by sdt_service_type (e.g. types 4, 27, 32 are TV)
        ss |= SvlTable.GetGroup(rec) switch 
        {
          0x81 => SignalSource.Tv,
          0x82 => SignalSource.Radio,
          _ => SignalSource.Data
        };
      }
    }
    else if (splitTvRadioData)
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

    // validate consistency with svl
    if (svlTables != null)
    {
      if (!svlByListId.TryGetValue(listId, out var table))
        throw LoaderException.Fail($"No binary data for service list {listId}");
      if (idx >= table.Records.Count)
        throw LoaderException.Fail($"Text data contains more channels than binary data: >={idx}");
      var data = table.Records[idx];
      if (SvlTable.GetRecordId(data) != chan.RecordIndex)
        throw LoaderException.Fail($"Inconsistent record id in text and binary data ({chan.RecordIndex} vs {SvlTable.GetRecordId(data)})");
      if (SvlTable.GetProgramNr(data) != chan.OldProgramNr)
        throw LoaderException.Fail($"Inconsistent program numbers in text and binary data ({chan.OldProgramNr} vs {SvlTable.GetProgramNr(data)})");
      if (SvlTable.GetServiceId(data) != chan.ServiceId)
        throw LoaderException.Fail($"Inconsistent program numbers in text and binary data ({chan.ServiceId} vs {SvlTable.GetServiceId(data)})");
    }

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
    if (this.svlTables != null)
      UpdateSvlAndXml();
    else
      UpdateXmlOnly();

    var filePath = this.SaveAsFileName ?? this.FileName;
    var settings = new XmlWriterSettings();
    settings.Indent = true;
    settings.Encoding = new UTF8Encoding(false);
    using var sw = new StringWriter();
    using var w = XmlWriter.Create(sw, settings);
    this.doc.WriteTo(w);
    w.Flush();
    File.WriteAllText(filePath, sw.ToString().Replace(" />", "/>"), settings.Encoding);
    this.FileName = filePath;

    if (this.DeveloperMode)
      this.WriteDebugFiles(this.FileName);
  }
  #endregion

  #region UpdateXmlOnly()
  private void UpdateXmlOnly()
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
  }
  #endregion

  #region UpdateSvlAndXml()
  private void UpdateSvlAndXml()
  {
    // 1) new numbers into the binary records (adjusts the per-record hash)
    //    - Map channel -> record by record_id (ch.RecordIndex = record_id from the XML, see ReadChannel),
    //      never by list position: with split TV/radio/data lists, list positions and record positions differ.
    //    - NewProgramNr == -1 means "no new number" (unsorted channel). Such channels keep their current number.
    //      Duplicate numbers are kept in their order; the TV exports such duplicates itself.
    //      Normally ChanSort appends unsorted channels before saving (DeleteMode.NotSupported).
    //    - Numbers must be unique per group (TV / radio / data = per list when splitTvRadioData is set).
    //      The same number may exist once per group: ChanSort's DVB-C test file numbers each group from 1,
    //      the BRAVIA 8 II DVB-S export uses TV 1.., radio 7201.., data 7587.. Moving a group to another range
    //      than the TV assigned is untested.
    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var chan in list.Channels)
      {
        if (chan is not Channel ch || ch.IsProxy || !svlRecordById.TryGetValue((GetServiceListId(ch.Xml), (int)ch.RecordIndex), out var rec))
          continue;
        if (ch.NewProgramNr < 1)
          continue;
        SvlTable.SetProgramNr(rec, ch.NewProgramNr);
        ch.Xml["major_channel_number"]!.InnerText = ch.NewProgramNr.ToString();
        // user_edit_flag and lcn_type were left untouched in the file that the TV accepted
      }
    }

    // 2) sort, recompress, fix lengths and checksums (all tables of all service lists)
    var orders = SvlTable.SaveAll(serviceDatabaseData, svlTables, out var serviceDatabase);

    // 3) reorder <service_info> elements of each service list to match the binary record order
    var byId = new Dictionary<(string, int), XmlElement>();
    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var chan in list.Channels)
      {
        if (chan is Channel ch && !ch.IsProxy)
          byId[(GetServiceListId(ch.Xml), (int)ch.RecordIndex)] = ch.Xml;
      }
    }

    foreach (var table in svlTables)
    {
      var order = orders[table];
      if (order.Count == 0)
        continue;
      var parent = (XmlElement)byId[(table.ServiceListId, order[0])].ParentNode;
      foreach (var id in order)
        parent!.RemoveChild(byId[(table.ServiceListId, id)]);
      foreach (var id in order)
        parent!.AppendChild(byId[(table.ServiceListId, id)]);
    }

    // 4) Base64 like Java's MIME encoder: 76 chars per line, "\n", trailing "\n"
    serviceDatabaseNode.InnerText = Convert.ToBase64String(serviceDatabase, Base64FormattingOptions.InsertLineBreaks).Replace("\r\n", "\n") + "\n";

    // 5) reload the tables: after SaveAll() the offsets of all but the last table are outdated
    this.serviceDatabaseData = serviceDatabase;
    this.LoadSvlTables();
  }
  #endregion

  #region WriteDebugFiles()
  internal void WriteDebugFiles(string baseName)
  {
    this.DeleteDebugFiles(baseName);
    
    if (this.serviceDatabaseData != null)
      File.WriteAllBytes(baseName + "_service_database.bin", this.serviceDatabaseData);

    if (this.scanData != null)
      File.WriteAllBytes(this.FileName + "_scan.bin", this.scanData);


    if (this.svlTables == null)
      return;

    using (var file = File.Create(baseName + "_service_records.bin"))
    {
      foreach (var rec in svlTables.SelectMany(t => t.Records))
        file.Write(rec, 0, rec.Length);
    }

    using (var file = new StreamWriter(baseName + "_names.txt"))
    {
      foreach (var name in this.svlTables.SelectMany(t => t.Names))
        file.WriteLine(name == null || name.Length < 2 ? null : Encoding.UTF8.GetString(name, 2, name.Length - 2)); // first 2 bytes are the length as u16-BE
    }
  }
  #endregion

  #region DeleteDebugFiles()
  internal void DeleteDebugFiles(string baseName = null)
  {
    baseName ??= this.FileName;
    Tools.Try(() => File.Delete(baseName + "_scan.bin"));
    Tools.Try(() => File.Delete(baseName + "_service_database.bin"));
    Tools.Try(() => File.Delete(baseName + "_names.txt"));
    Tools.Try(() => File.Delete(baseName + "_service_records.bin"));
  }
  #endregion
}
