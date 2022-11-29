using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.Loewe;

/*
 * The XML used in by this loader seems like a transformed version of the SQLite3 database format used by the Hisense/Loewe loader
 *
 *
 * The 2017 Hisense / Loewe data model for channel lists is a bit different than all other supported models and need some workarounds to be supported.
 * It is based on a flat "Services" table which doesn't hold program numbers and a FavoritesList/FavoritesItem table to assign numbers
 * to physical tuner lists and user favorite lists alike.
 * 
 * Physical channel lists (e.g. for $av, Astra, Hot Bird) have their own ChannelList in the channelList dictionary and use 
 * ChannelInfo.NewProgramNr to hold the program number. This doesn't allow the user to add services from other lists.
 * 
 * The user favorite lists (FAV1-FAV4) use the separate favList ChannelList filled with all services from all physical lists.
 * ChannelInfo.FavIndex[0-3] holds the information for the program numbers in FAV1-4. The value -1 is used to indicate "not included".
 * 
 * The $all list is hidden from the user and automatically updated to match the contents of all other lists (except $av and FAV1-4).
 * 
 * The $av list is hidden from the user and not updated at all.
 * 
 * This loader poses the following restrictions on the database:
 * - a service must not appear in more than one physical channel list ($all and FAV1-4 are not part of this restriction)
 * - a service can't appear more than once in any list
 *
 */

class Serializer : SerializerBase
{
  private XmlDocument doc;
  private readonly StringBuilder fileInfo = new();

  private readonly ChannelList mixedFavTv;
  private readonly ChannelList mixedFavRadio;

  /// <summary>
  ///   Fields of the ChannelInfo that will be shown in the UI
  /// </summary>
  private static readonly List<string> ColumnNames = new()
  {
    "OldPosition",
    "Position",
    "Source",
    "NewProgramNr",
    "Name",
    "ShortName",
    "Favorites",
    "Skip",
    "Lock",
    "Hidden",
    "Encrypted",
    "FreqInMhz",
    "OriginalNetworkId",
    "TransportStreamId",
    "ServiceId",
    //"ServiceType",
    "ServiceTypeName",
    "NetworkName",
    "Satellite"
    //        "SymbolRate"
  };

  /// <summary>
  ///   mapping of FavoriteList.Pid => ChannelList.
  ///   This dict does not include real user favorite lists (FAV1-FAV4).
  /// </summary>
  private readonly Dictionary<int, ChannelList> channelLists = new();

  /// <summary>
  ///   mapping of FavoriteList.Pid for $all and FAV1-4 => index of the internal favorite list within userFavList (0-3)
  ///   Pids that don't belong to the FAV1-4 are not included in this dictionary.
  /// </summary>
  private readonly Dictionary<int, int> favListIdToFavIndex = new();

  /// <summary>
  ///   mapping of Service.Pid => ChannelInfo
  /// </summary>
  private readonly Dictionary<long, ChannelInfo> channelsById = new();
  
  /// <summary>
  ///   FavoriteList.Pid of the $all list
  /// </summary>
  private int pidAll;

  /// <summary>
  ///   FavoriteList.Pid of the $av list
  /// </summary>
  private int pidAv;

  

  #region ctor()

  public Serializer(string inputFile) : base(inputFile)
  {
    Features.ChannelNameEdit = ChannelNameEditMode.All;
    Features.DeleteMode = DeleteMode.NotSupported;
    Features.CanSkipChannels = true;
    Features.CanLockChannels = true;
    Features.CanHideChannels = true;
    Features.CanHaveGaps = true;
    Features.FavoritesMode = FavoritesMode.MixedSource;

    this.mixedFavTv = new ChannelList(0, "TV");
    this.mixedFavTv.VisibleColumnFieldNames = ColumnNames;
    this.mixedFavTv.IsMixedSourceFavoritesList = true;

    this.mixedFavRadio = new ChannelList(0, "Radio");
    this.mixedFavRadio.VisibleColumnFieldNames = ColumnNames;
    this.mixedFavRadio.IsMixedSourceFavoritesList = true;
  }

  #endregion

  #region Load()

  public override void Load()
  {
    this.doc = new XmlDocument();
    doc.Load(this.FileName);
    var sl = doc["servicelist"];
    if (sl == null)
      throw LoaderException.TryNext("expected root element <servicelist>");

    var tuners = sl["tuners"];
    if (tuners == null)
      throw LoaderException.TryNext("missing <tuners> list");

    var services = sl["services"];
    if (services == null)
      throw LoaderException.TryNext("missing <services> list");
    
    var favorites = sl["favorites"];
    if (favorites == null)
      throw LoaderException.TryNext("missing <favorites> list");

    LoadTuners(tuners);
    LoadServices(services);
    LoadFavorites(favorites);

    Features.MaxFavoriteLists = this.favListIdToFavIndex.Count;
    this.channelLists.Add(0, mixedFavTv);
    this.channelLists.Add(0x8000, mixedFavRadio);
  }
  #endregion

  #region LoadTuners()

  private void LoadTuners(XmlElement tuners)
  {
    int index = 0;
    foreach (var child in tuners.ChildNodes)
    {
      if (child is not XmlElement e)
        continue;
      if (e.LocalName == "tuner")
        LoadTunerBaseData(e, index++);
      else if (e.LocalName == "dvbt2-tuner")
        LoadDvbTunerData(e);
    }
  }

  private void LoadTunerBaseData(XmlElement e, int index)
  {
    var a = e.Attributes;
    var id = int.Parse(a["TunerId"].InnerText);
    var t = new Transponder(id);
    t.OriginalNetworkId = int.Parse(a["Oid"].InnerText);
    t.TransportStreamId = int.Parse(a["Tid"].InnerText);

    if (e.LocalName == "dvbt-tuner" || e.LocalName == "dvbt2-tuner")
      t.SignalSource |= SignalSource.Antenna;
    else if (e.LocalName == "dvbc-tuner" || e.LocalName == "dvbc2-tuner")
      t.SignalSource |= SignalSource.Cable;
    else if (e.LocalName == "dvbs-tuner" || e.LocalName == "dvbs2-tuner")
      t.SignalSource |= SignalSource.Sat;

    DataRoot.Transponder.Add(id, t);
  }

  private void LoadDvbTunerData(XmlElement e)
  {
    var a = e.Attributes;
    var id = int.Parse(a["TunerId"].InnerText);
    if (!DataRoot.Transponder.TryGetValue(id, out var t))
      return;
    t.FrequencyInMhz = decimal.Parse(a["Frequency"].InnerText) / 1000;
  }

  #endregion

  #region LoadServices

  private void LoadServices(XmlElement services)
  {
    int index = 0;
    foreach (var child in services.ChildNodes)
    {
      if (child is not XmlElement e)
        continue;
      if (e.LocalName == "service")
        LoadServiceBaseData(e, index++);
      else if (e.LocalName == "analog-service")
        LoadServiceAnalogData(e);
      else if (e.LocalName == "dvb-service")
        LoadServiceDigitalData(e);
    }
  }

  private void LoadServiceBaseData(XmlElement e, int index)
  {
    var a = e.Attributes;
    var id = int.Parse(a["Pid"].InnerText);
    var c = new Channel(id);
    c.XmlElement = e;
    this.channelsById[id] = c;
    c.RecordOrder = index;
    c.Name = a["Name"].InnerText;
    c.Lock = a["ParentalLock"].InnerText == "1";
    c.Skip = a["Selectable"].InnerText == "0";
    c.ShortName = a["ShortName"].InnerText;
    c.Hidden = a["Visible"].InnerText == "0";

    ChannelList list;
    switch (int.Parse(a["MediaType"].InnerText))
    {
      case 1: c.SignalSource |= SignalSource.Tv;
        list = mixedFavTv;
        break;
      case 2: c.SignalSource |= SignalSource.Radio;
        list = mixedFavRadio;
        break;
      default:
        list = mixedFavTv;
        break;
    }

    list.AddChannel(c);
  }

  private void LoadServiceAnalogData(XmlElement e)
  {
    var a = e.Attributes;
    var id = int.Parse(a["ServiceId"].InnerText);
    if (!this.channelsById.TryGetValue(id, out var c))
      return;
    c.FreqInMhz = decimal.Parse(a["Frequency"].InnerText) / 1000;
    c.SignalSource |= SignalSource.Analog;
  }

  private void LoadServiceDigitalData(XmlElement e)
  {
    var a = e.Attributes;
    var id = int.Parse(a["ServiceId"].InnerText);
    if (!this.channelsById.TryGetValue(id, out var c))
      return;
    c.ServiceId = int.Parse(a["Sid"].InnerText);
    c.SignalSource |= SignalSource.Digital;
    
    var transponderId = int.Parse(a["TunerId"].InnerText);
    var t = DataRoot.Transponder.TryGet(transponderId);
    if (t != null)
    {
      c.Transponder = t;
      c.OriginalNetworkId = t.OriginalNetworkId;
      c.TransportStreamId = t.TransportStreamId;
      c.FreqInMhz = t.FrequencyInMhz;
      c.SignalSource |= t.SignalSource;
    }
  }
  #endregion

  #region LoadFavorites()

  private void LoadFavorites(XmlElement favorites)
  {
    foreach (var node in favorites.ChildNodes)
    {
      if (node is not XmlElement e)
        continue;
      if (e.LocalName == "favorite-list")
        LoadFavoriteList(e);
      else if (e.LocalName == "favorite-item")
        LoadFavoriteItem(e);
      else if (e.LocalName == "lcn")
        LoadLcnItem(e);
    }

    this.DataRoot.AddChannelList(this.mixedFavTv);
    this.DataRoot.AddChannelList(this.mixedFavRadio);
  }

  private void LoadFavoriteList(XmlElement e)
  {
    var a = e.Attributes;
    var name = a["Name"].InnerText;
    var creator = a["Creator"].InnerText;
    var id = int.Parse(a["Pid"].InnerText);

    if (name == "$av")
      this.pidAv = id;
    else if (creator.StartsWith("User.") || name == "$all")
    {
      if (name == "$all")
        this.pidAll = id;

      var idx = this.favListIdToFavIndex.Count;
      this.favListIdToFavIndex[id] = idx + 1;
      this.mixedFavTv.SetFavListCaption(idx, name);
      this.mixedFavRadio.SetFavListCaption(idx, name);
      return;
    }

    var list = new ChannelList(0, name + " TV");
    list.VisibleColumnFieldNames = ColumnNames;
    channelLists.Add(id, list);
    DataRoot.AddChannelList(list);
    if (name.StartsWith("$av"))
    {
      list.ShortCaption = "A/V";
      list.ReadOnly = true;
      return;
    }

    list = new ChannelList(0, name + " Radio");
    list.VisibleColumnFieldNames = ColumnNames;
    channelLists.Add(id | 0x8000, list);
    DataRoot.AddChannelList(list);
  }

  private void LoadFavoriteItem(XmlElement e)
  {
    var a = e.Attributes;
    var listId = int.Parse(a["FavoriteId"].InnerText);
    var serviceId = int.Parse(a["ServiceId"].InnerText);
    var channelNo = int.Parse(a["ChannelNum"].InnerText);

    if (!this.channelsById.TryGetValue(serviceId, out var c))
      return;

    if (this.favListIdToFavIndex.TryGetValue(listId, out var listIdx))
      c.SetOldPosition(listIdx, channelNo);
    else
    {
      c.OldProgramNr = channelNo;
      ((Channel)c).PhysicalListId = listId;
    }

    if (listIdx == 0)
    {
      if ((c.SignalSource & SignalSource.Radio) != 0)
        listId |= 0x8000;
      var list = channelLists.TryGet(listId);
      DataRoot.AddChannel(list, c);
    }
  }

  private void LoadLcnItem(XmlElement e)
  {
    var a = e.Attributes;
    var listId = int.Parse(a["FavoriteId"].InnerText);
    if (!int.TryParse(a["ServiceId"].InnerText, out var serviceId))
      return;
    if (!int.TryParse(a["Lcn"].InnerText, out var lcn))
      return;

    if (!this.channelsById.TryGetValue(serviceId, out var c))
      return;

    c.ProgramNrPreset = lcn;
    if (this.favListIdToFavIndex.TryGetValue(listId, out var listIdx))
    {
      var list = channelLists.TryGet(listId);
      list.ReadOnly = true;
    }
    else if (this.channelLists.TryGetValue(listId, out var list))
    {
      list.ReadOnly = true;
    }
  }

  #endregion
  
  // Save

  #region Save()

  public override void Save()
  {
    var fav = this.doc["servicelist"]["favorites"];
    var elements = fav.GetElementsByTagName("favorite-item");
    var items = new List<XmlNode>();
    foreach(XmlNode node in elements)
      items.Add(node);
    foreach (var node in items)
      node.ParentNode.RemoveChild(node);

    elements = fav.GetElementsByTagName("lcn");
    var lcn = new List<XmlNode>();
    foreach (XmlNode node in elements)
      lcn.Add(node);
    foreach (var node in lcn)
      node.ParentNode.RemoveChild(node);

    var idVal = 0;
    var lists = new List<Tuple<int, ChannelList, int>>();
    foreach (var list in this.DataRoot.ChannelLists)
    {
      if (list.IsMixedSourceFavoritesList)
      {
        foreach (var entry in favListIdToFavIndex)
          lists.Add(Tuple.Create(entry.Key, list, entry.Value));
      }
      else
      {
        lists.Add(Tuple.Create(channelLists.FirstOrDefault(e => e.Value == list).Key, list, 0));
      }
    }

    //var listIds = this.channelLists.Keys.Union(this.favListIdToFavIndex.Keys).OrderBy(k => k).ToList();
    foreach (var tuple in lists)
    {
      var listId = tuple.Item1 & 0x7FFF;
      var list = tuple.Item2;
      var favIndex = tuple.Item3;
      foreach (var chan in list.Channels)
      {
        if (chan is not Channel c || c.IsProxy)
          continue;

        var chno = c.GetPosition(favIndex);
        if (chno < 0)
          continue;

        var e = doc.CreateElement("favorite-item");
        e.SetAttribute("Active", "0");
        e.SetAttribute("Attribute", "0");
        e.SetAttribute("ChannelNum", chno.ToString());
        e.SetAttribute("FavoriteId", listId.ToString());
        e.SetAttribute("Id", (++idVal).ToString());
        e.SetAttribute("OriginalFavoriteId", listId.ToString());
        e.SetAttribute("Selectable", "-1");
        e.SetAttribute("ServiceId", c.RecordIndex.ToString());
        e.SetAttribute("ServiceName", "");
        e.SetAttribute("Visible", "-1");
        fav.AppendChild(e);
      }
    }

    foreach (XmlNode item in lcn)
      fav.AppendChild(item);

    doc.Save(this.FileName);
  }
  
  #endregion

}

