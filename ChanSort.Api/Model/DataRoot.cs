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
      if (list == null)
      {
        warnings.AppendFormat("No list found to add channel '{0}'\r\n", channel);
        return;
      }
      string warning = list.AddChannel(channel);
      if (warning != null)
        this.Warnings.AppendLine(warning);
    }
    #endregion


    #region GetChannelList()
    public ChannelList GetChannelList(SignalSource criteriaMask)
    {
      foreach (var list in this.channelLists)
      {
        uint searchMask = (uint)criteriaMask;
        uint listMask = (uint) list.SignalSource;

        if ((listMask & 0x000F & searchMask) == 0) // digital/analog
          continue;
        if ((listMask & 0x00F0 & searchMask) == 0) // air/cable/sat
          continue;
        if ((listMask & 0x0F00 & searchMask) == 0) // tv/radio
          continue;
        if ((listMask & 0xF000) != (searchMask & 0xF000)) // preset list
          continue;
        return list;
      }
      return null;
    }
    #endregion

    #region ApplyCurrentProgramNumbers()
    public void ApplyCurrentProgramNumbers()
    {
      foreach (var list in this.ChannelLists)
      {
        foreach (var channel in list.Channels)
          channel.NewProgramNr = channel.OldProgramNr;
      }
    }
    #endregion
  }
}
