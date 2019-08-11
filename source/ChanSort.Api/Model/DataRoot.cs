using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChanSort.Api
{
  public class DataRoot
  {
    private readonly IDictionary<int, Satellite> satellites = new Dictionary<int, Satellite>();
    private readonly IDictionary<int, Transponder> transponder = new Dictionary<int, Transponder>();
    private readonly IDictionary<int, LnbConfig> lnbConfig = new Dictionary<int, LnbConfig>();
    private readonly IList<ChannelList> channelLists = new List<ChannelList>();
    private readonly StringBuilder warnings = new StringBuilder();

    public StringBuilder Warnings { get { return this.warnings; } }
    public IDictionary<int, Satellite> Satellites { get { return this.satellites; } }
    public IDictionary<int, Transponder> Transponder { get { return this.transponder; } }
    public IDictionary<int, LnbConfig> LnbConfig { get { return this.lnbConfig; } }
    public IEnumerable<ChannelList> ChannelLists { get { return this.channelLists; } }
    public bool IsEmpty { get { return this.channelLists.Count == 0; } }
    public bool NeedsSaving { get; set; }
    public Favorites SupportedFavorites { get; set; }
    public bool SortedFavorites { get; set; }
    public bool MixedSourceFavorites { get; set; }
    public bool AllowGapsInFavNumbers { get; set; }
    public bool ShowDeletedChannels { get; set; }

    public bool DeletedChannelsNeedNumbers { get; set; }

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
        this.warnings.AppendFormat("Duplicate transponder data record for satellite #{0} with id {1}\r\n", sat?.Id, trans.Id);
        return;
      }
      if (sat != null)
        sat.Transponder.Add(trans.Id, trans);
      this.transponder.Add(trans.Id, trans);
    }
    #endregion

    #region AddLnbConfig()
    public void AddLnbConfig(LnbConfig lnb)
    {
      this.lnbConfig.Add(lnb.Id, lnb);
    }
    #endregion

    #region AddChannelList()
    public virtual void AddChannelList(ChannelList list)
    {
      this.channelLists.Add(list);
      this.MixedSourceFavorites |= list.IsMixedSourceFavoritesList;
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

        if ((listMask & 0x000F & searchMask) != (searchMask & 0x000F)) // digital/analog
          continue;
        if ((listMask & 0x00F0 & searchMask) != (searchMask & 0x00F0)) // air/cable/sat/ip
          continue;
        if ((listMask & 0x0F00 & searchMask) != (searchMask & 0x0F00)) // tv/radio
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
      int c = 0;
      if (this.MixedSourceFavorites || this.SortedFavorites)
      {
        for (int m = (int) this.SupportedFavorites; m != 0; m >>= 1)
          ++c;
      }

      foreach (var list in this.ChannelLists)
      {
        foreach (var channel in list.Channels)
        {
          for (int i = 0; i <= c; i++)
            channel.SetPosition(i, channel.IsDeleted && !this.DeletedChannelsNeedNumbers ? -1 : channel.GetOldPosition(i));
        }
      }
    }
    #endregion

    #region SetPrNrForDeletedChannels()
    public void SetPrNrForDeletedChannels()
    {
      if (this.DeletedChannelsNeedNumbers)
        return;

      // make sure that deleted channels have OldProgramNr = -1
      foreach (var list in this.ChannelLists)
      {
        if (list.IsMixedSourceFavoritesList)
          continue;
        foreach (var chan in list.Channels)
        {
          if (chan.IsDeleted)
          {
            chan.NewProgramNr = -1;
            chan.OldProgramNr = -1;
          }
          else if (chan.OldProgramNr == -1) // old versions of ChanSort saved -1 and without setting IsDeleted
            chan.IsDeleted = true;
        }
      }
    }
    #endregion
  }
}
