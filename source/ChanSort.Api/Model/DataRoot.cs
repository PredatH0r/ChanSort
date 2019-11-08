using System.Collections.Generic;
using System.Text;

namespace ChanSort.Api
{
  public class DataRoot
  {
    private readonly IList<ChannelList> channelLists = new List<ChannelList>();
    private readonly SerializerBase loader;

    public StringBuilder Warnings { get; } = new StringBuilder();
    public IDictionary<int, Satellite> Satellites { get; } = new Dictionary<int, Satellite>();
    public IDictionary<int, Transponder> Transponder { get; } = new Dictionary<int, Transponder>();
    public IDictionary<int, LnbConfig> LnbConfig { get; } = new Dictionary<int, LnbConfig>();
    public IEnumerable<ChannelList> ChannelLists => this.channelLists;
    public bool IsEmpty => this.channelLists.Count == 0;
    public bool NeedsSaving { get; set; }


    public Favorites SupportedFavorites => this.loader.Features.SupportedFavorites;
    public bool SortedFavorites => this.loader.Features.SortedFavorites;
    public bool MixedSourceFavorites => this.loader.Features.MixedSourceFavorites;
    public bool AllowGapsInFavNumbers => this.loader.Features.AllowGapsInFavNumbers;
    public bool DeletedChannelsNeedNumbers => this.loader.Features.DeletedChannelsNeedNumbers;

    public DataRoot(SerializerBase loader)
    {
      this.loader = loader;
    }

    #region AddSatellite()
    public virtual void AddSatellite(Satellite satellite)
    {
      this.Satellites.Add(satellite.Id, satellite);
    }
    #endregion

    #region AddTransponder()
    public virtual void AddTransponder(Satellite sat, Transponder trans)
    {
      trans.Satellite = sat;
      if (this.Transponder.ContainsKey(trans.Id))
      {
        this.Warnings.AppendFormat("Duplicate transponder data record for satellite #{0} with id {1}\r\n", sat?.Id, trans.Id);
        return;
      }
      if (sat != null)
        sat.Transponder.Add(trans.Id, trans);
      this.Transponder.Add(trans.Id, trans);
    }
    #endregion

    #region AddLnbConfig()
    public void AddLnbConfig(LnbConfig lnb)
    {
      this.LnbConfig.Add(lnb.Id, lnb);
    }
    #endregion

    #region AddChannelList()
    public virtual void AddChannelList(ChannelList list)
    {
      this.channelLists.Add(list);
      this.loader.Features.MixedSourceFavorites |= list.IsMixedSourceFavoritesList;
    }
    #endregion

    #region AddChannel()
    public virtual void AddChannel(ChannelList list, ChannelInfo channel)
    {
      if (list == null)
      {
        this.Warnings.AppendFormat("No list found to add channel '{0}'\r\n", channel);
        return;
      }
      string warning = list.AddChannel(channel);
      if (warning != null)
        this.Warnings.AppendLine(warning);
    }
    #endregion


    #region GetChannelList()
    public ChannelList GetChannelList(SignalSource searchMask)
    {
      foreach (var list in this.channelLists)
      {
        var listMask = list.SignalSource;

        if ((searchMask & SignalSource.MaskAnalogDigital) != 0 && (listMask & SignalSource.MaskAnalogDigital & searchMask) == 0) // digital/analog
          continue;
        if ((searchMask & SignalSource.MaskAntennaCableSat) != 0 && (listMask & SignalSource.MaskAntennaCableSat & searchMask) == 0) // air/cable/sat/ip
          continue;
        if ((searchMask & SignalSource.MaskTvRadioData) != 0 && (listMask & SignalSource.MaskTvRadioData & searchMask) == 0) // tv/radio/data
          continue;
        if ((searchMask & SignalSource.MaskProvider) != 0 && (listMask & SignalSource.MaskProvider) != (searchMask & SignalSource.MaskProvider)) // preset list
          continue;
        return list;
      }
      return null;
    }
    #endregion

    #region ValidateAfterLoad()
    public virtual void ValidateAfterLoad()
    {
      foreach (var list in this.ChannelLists)
      {
        if (list.IsMixedSourceFavoritesList)
          continue;

        // make sure that deleted channels have OldProgramNr = -1
        foreach (var chan in list.Channels)
        {
          if (chan.IsDeleted)
            chan.OldProgramNr = -1;
          else if (chan.OldProgramNr < 0) // old versions of ChanSort saved -1 and without setting IsDeleted
            chan.IsDeleted = true;
        }
      }
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
            channel.SetPosition(i, channel.GetOldPosition(i));
        }
      }
    }
    #endregion

    #region ValidateAfterSave()
    public virtual void ValidateAfterSave()
    {
      // set old numbers to match the new numbers
      // also make sure that deleted channels are either removed from the list or assigned the -1 prNr, depending on the loader's DeleteMode
      foreach (var list in this.ChannelLists)
      {
        for (int i = 0; i < list.Channels.Count; i++)
        {
          var chan = list.Channels[i];
          if (chan.IsDeleted)
          {
            if (this.loader.Features.DeleteMode == SerializerBase.DeleteMode.Physically)
              list.Channels.RemoveAt(i--);
            else
              chan.NewProgramNr = -1;
          }

          chan.OldProgramNr = chan.NewProgramNr;
          chan.OldFavIndex.Clear();
          chan.OldFavIndex.AddRange(chan.FavIndex);
        }
      }
    }
    #endregion

  }
}
