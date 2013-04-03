using System.Collections.Generic;
using System.Text;

namespace ChanSort.Api
{
  public class DataRoot
  {
    private readonly IDictionary<int, Satellite> satellites = new Dictionary<int, Satellite>();
    private readonly IDictionary<int, Transponder> transponder = new Dictionary<int, Transponder>();
    private readonly IList<ChannelList> channelLists = new List<ChannelList>();
    private readonly StringBuilder warnings = new StringBuilder();

    public StringBuilder Warnings { get { return this.warnings; } }
    public IDictionary<int, Satellite> Satellites { get { return this.satellites; } }
    public IDictionary<int, Transponder> Transponder { get { return this.transponder; } }
    public ICollection<ChannelList> ChannelLists { get { return this.channelLists; } }
    public bool IsEmpty { get { return this.channelLists.Count == 0; } }
    public bool NeedsSaving { get; set; }
    public Favorites SupportedFavorites { get; set; }

    public DataRoot()
    {
      this.SupportedFavorites = Favorites.A | Favorites.B | Favorites.C | Favorites.D;
    }

    #region AddSatellite()
    public virtual void AddSatellite(Satellite satellite)
    {
      this.satellites.Add(satellite.Id, satellite);
    }
    #endregion

    #region AddTransponder()
    public virtual void AddTransponder(Satellite sat, Transponder trans)
    {
      trans.Satellite = sat;
      if (this.transponder.ContainsKey(trans.Id))
      {
        this.warnings.AppendFormat("Duplicate transponder data record for satellite #{0} with id {1}\r\n", sat.Id, trans.Id);
        return;
      }
      sat.Transponder.Add(trans.Id, trans);
      this.transponder.Add(trans.Id, trans);
    }
    #endregion

    #region AddChannelList()
    public virtual void AddChannelList(ChannelList list)
    {
      this.channelLists.Add(list);
    }
    #endregion

    #region AddChannel()
    public virtual void AddChannel(ChannelList list, ChannelInfo channel)
    {
      string warning = list.AddChannel(channel);
      if (warning != null)
        this.Warnings.AppendLine(warning);
    }
    #endregion


    #region GetChannelList()
    public ChannelList GetChannelList(SignalSource signalSource, SignalType signalType, bool createIfNotExists)
    {
      foreach (var list in this.channelLists)
      {
        if ((list.SignalSource&SignalSource.Digital) != (signalSource&SignalSource.Digital)) // match digital/analog
          continue;
        if (((int)list.SignalSource & 0x0F & (int)signalSource) == 0) // match any of cable/terrestrial/satellite
          continue;
        if (list.SignalType == signalType || list.SignalType == SignalType.Mixed) // match radio/tv/any
          return list;
      }
      ChannelList newList = new ChannelList(signalSource, signalType);
      if (createIfNotExists)
        this.AddChannelList(newList);
      return newList;
    }
    #endregion
  }
}
