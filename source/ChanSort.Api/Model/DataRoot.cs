using System.Collections.Generic;
using System.Linq;
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
    public bool DeletedChannelsNeedNumbers => this.loader.Features.DeleteMode == SerializerBase.DeleteMode.FlagWithPrNr;
    public bool CanSkip => this.loader.Features.CanSkipChannels;
    public bool CanLock => this.loader.Features.CanLockChannels;
    public bool CanHide => this.loader.Features.CanHideChannels;
    public bool CanEditFavListName => this.loader.Features.CanEditFavListNames;

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
        bool hasPolarity = false;
        foreach (var chan in list.Channels)
        {
          if (chan.IsDeleted)
            chan.OldProgramNr = -1;
          else
          {
            if (chan.OldProgramNr < 0) // old versions of ChanSort saved -1 and without setting IsDeleted
              chan.IsDeleted = true;
            hasPolarity |= chan.Polarity == 'H' || chan.Polarity == 'V';
          }
        }
        if (!hasPolarity)
          list.VisibleColumnFieldNames.Remove("Polarity");
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

    #region AssignNumbersToUnsortedAndDeletedChannels()

    public void AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode mode)
    {
      foreach (var list in this.ChannelLists)
      {
        if (list.IsMixedSourceFavoritesList)
          continue;

        // sort the channels by assigned numbers, then unassigned by original order or alphabetically, then deleted channels
        var sortedChannels = list.Channels.OrderBy(ch => ChanSortCriteria(ch, mode));
        int maxProgNr = 0;

        foreach (var appChannel in sortedChannels)
        {
          if (appChannel.IsProxy)
            continue;

          if (appChannel.NewProgramNr == -1)
          {
            if (mode == UnsortedChannelMode.Delete)
              appChannel.IsDeleted = true;
            else // append (hidden if possible)
            {
              appChannel.Hidden = true;
              appChannel.Skip = true;
            }

            // assign a valid number or 0 .... because -1 will never be a valid value for the TV
            appChannel.NewProgramNr = mode != UnsortedChannelMode.Delete || this.DeletedChannelsNeedNumbers ? ++maxProgNr : 0;
          }
          else
          {
            appChannel.IsDeleted = false;
            if (appChannel.NewProgramNr > maxProgNr)
              maxProgNr = appChannel.NewProgramNr;
          }
        }
      }
    }

    private string ChanSortCriteria(ChannelInfo channel, UnsortedChannelMode mode)
    {
      // explicitly sorted
      var pos = channel.NewProgramNr;
      if (pos != -1)
        return pos.ToString("d5");

      // eventually hide unsorted channels
      if (mode == UnsortedChannelMode.Delete)
        return "Z" + channel.RecordIndex.ToString("d5");

      // eventually append in old order
      if (mode == UnsortedChannelMode.AppendInOrder)
        return "B" + channel.OldProgramNr.ToString("d5");

      // sort alphabetically, with "." and "" on the bottom
      if (channel.Name == ".")
        return "B";
      if (channel.Name == "")
        return "C";
      return "A" + channel.Name;
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


    #region Get/SetFavListCaption()

    private readonly Dictionary<int,string> favListCaptions = new Dictionary<int, string>();

    public void SetFavListCaption(int favIndex, string caption)
    {
      favListCaptions[favIndex] = caption;
    }

    public string GetFavListCaption(int favIndex, bool asTabCaption = false)
    {
      var hasCaption = favListCaptions.TryGetValue(favIndex, out var caption);
      if (!asTabCaption)
        return caption;
      var letter = (char)('A' + favIndex);
      return  hasCaption && !string.IsNullOrEmpty(caption) ? letter + ": " + caption : "Fav " + letter;
    }
    #endregion
  }
}
