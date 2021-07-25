using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChanSort.Api
{
  public class Editor
  {
    public DataRoot DataRoot;
    public ChannelList ChannelList;
    public int SubListIndex;

    #region AddChannels()

    public ChannelInfo AddChannels(IList<ChannelInfo> channels)
    {
      int count = channels.Count(channel => channel.GetPosition(this.SubListIndex) == -1);
      if (count == 0) return null;

      ChannelInfo lastInsertedChannel = null;
      int progNr = this.ChannelList.InsertProgramNumber;
      int relativeChannelNumber = 0;
      int progNrCopy = progNr; // prevent "access to modified closure" warning
      foreach (var channel in this.ChannelList.Channels.Where(c => c.GetPosition(this.SubListIndex) >= progNrCopy).OrderBy(c => c.GetPosition(this.SubListIndex)))
      {
        var curPos = channel.GetPosition(this.SubListIndex);
        int gap = count - (curPos - progNr - relativeChannelNumber);
        if (gap > 0)
        {
          channel.SetPosition(this.SubListIndex, curPos + gap);
          ++relativeChannelNumber;
        }
      }

      foreach (var channel in channels)
      {
        if (channel.GetPosition(this.SubListIndex) != -1)
        {
          // TODO notify user
          continue;
        }

        channel.SetPosition(this.SubListIndex, progNr++);
        lastInsertedChannel = channel;
      }
      this.ChannelList.InsertProgramNumber += count;

      this.DataRoot.NeedsSaving |= lastInsertedChannel != null;
      return lastInsertedChannel;
    }

    #endregion

    #region RemoveChannels()

    public void RemoveChannels(IList<ChannelInfo> channels, bool closeGap)
    {
      if (channels.Count == 0) return;

      this.ChannelList.InsertProgramNumber = channels[0].GetPosition(this.SubListIndex);
      var orderedChannelList =
        this.ChannelList.Channels.Where(c => c.GetPosition(this.SubListIndex) != -1)
            .OrderBy(c => c.GetPosition(this.SubListIndex));
      foreach (var channel in channels)
      {
        if (channel.GetPosition(this.SubListIndex) == -1)
          continue;
        if (closeGap)
        {
          int prevNr = channel.GetPosition(this.SubListIndex);
          foreach (var channel2 in orderedChannelList)
          {
            if (channel2.GetPosition(this.SubListIndex) > channel.GetPosition(this.SubListIndex))
            {
              // ignore deleted and proxy channels (prevNr<0), broken channels (==0) and channels after a gap
              if (prevNr <= 0 || channel2.GetPosition(this.SubListIndex) != prevNr + 1)
                break;
              prevNr = channel2.GetPosition(this.SubListIndex);
              channel2.ChangePosition(this.SubListIndex, -1);
            }
          }
        }
        channel.SetPosition(this.SubListIndex, -1);
      }

      this.DataRoot.NeedsSaving = true;
    }

    #endregion

    #region MoveChannels()

    public void MoveChannels(IList<ChannelInfo> channels, bool up)
    {
      if (channels.Count == 0)
        return;

      if (up && channels[0].GetPosition(this.SubListIndex) <= this.ChannelList.FirstProgramNumber)
        return;
      if (channels.Any(ch => ch.NewProgramNr < 0))
        return;

      int maxNr = this.ChannelList.Channels.Count == 0 ? 0 : this.ChannelList.Channels.Max(c => c.GetPosition(this.SubListIndex));
      int delta = (up ? -1 : +1);
      foreach (var channel in (up ? channels : channels.Reverse()))
      {
        int newProgramNr = channel.GetPosition(this.SubListIndex) + delta;
        if (newProgramNr < 0) // can't move a channel up when it's not in the list
          continue;
        if (newProgramNr == 0) // "+" should work like "add at the end" when the channel is not in the list yet
        {
          newProgramNr = ++maxNr;
          channel.SetPosition(this.SubListIndex, newProgramNr);
          continue;
        }

        maxNr = Math.Max(maxNr, newProgramNr);
        ChannelInfo channelAtNewPos = this.ChannelList.Channels.FirstOrDefault(ch => ch.GetPosition(this.SubListIndex) == newProgramNr);
        if (channelAtNewPos != null)
          channelAtNewPos.ChangePosition(this.SubListIndex, -delta);
        channel.ChangePosition(this.SubListIndex, delta);
      }
      this.DataRoot.NeedsSaving = true;
    }

    #endregion

    #region SortSelectedChannels(), ChannelComparerForSortingByName()

    public void SortSelectedChannels(List<ChannelInfo> selectedChannels)
    {
      if (selectedChannels.Count == 0) return;
      var sortedChannels = new List<ChannelInfo>(selectedChannels);
      sortedChannels.Sort(this.ChannelComparerForSortingByName);
      var programNumbers = selectedChannels.Select(ch => ch.GetPosition(this.SubListIndex)).ToList();
      for (int i = 0; i < sortedChannels.Count; i++)
        sortedChannels[i].SetPosition(this.SubListIndex, programNumbers[i]);

      this.DataRoot.NeedsSaving = true;
    }

    private int ChannelComparerForSortingByName(ChannelInfo channel1, ChannelInfo channel2)
    {
      return channel1.Name.CompareTo(channel2.Name);
    }

    #endregion

    #region SetSlotNumber()

    public void SetSlotNumber(IList<ChannelInfo> channels, int slot, bool swap, bool closeGap)
    {
      if (channels.Count == 0) return;
      if (swap)
      {
        foreach (var channel in channels)
        {
          if (slot != -1)
          {
            var others = this.ChannelList.GetChannelByNewProgNr(slot);
            foreach (var other in others)
              other.SetPosition(this.SubListIndex, channel.GetPosition(this.SubListIndex));
          }
          channel.SetPosition(this.SubListIndex, slot++);
        }
      }
      else
      {
        this.RemoveChannels(channels, closeGap);
        this.ChannelList.InsertProgramNumber = slot;
        this.AddChannels(channels);
      }
      this.DataRoot.NeedsSaving = true;
    }

    #endregion

    #region RenumberChannels()

    public void RenumberChannels(List<ChannelInfo> channels)
    {
      if (channels.Count == 0) return;
      int progNr = channels.Min(ch => ch.GetPosition(this.SubListIndex));
      foreach (var channel in channels)
      {
        if (channel.GetPosition(this.SubListIndex) == progNr)
        {
          ++progNr;
          continue;
        }

        var list = new List<ChannelInfo>();
        list.Add(channel);
        this.RemoveChannels(list, false);
        this.ChannelList.InsertProgramNumber = progNr++;
        this.AddChannels(list);
        this.DataRoot.NeedsSaving = true;
      }
    }

    #endregion


    #region ApplyReferenceList()

    public void ApplyReferenceList(DataRoot refDataRoot)
    {
      foreach (var channelList in this.DataRoot.ChannelLists)
      {
        foreach (var channel in channelList.Channels)
          channel.SetPosition(0, -1);
      }

      StringBuilder log = new StringBuilder();
      foreach (var refList in refDataRoot.ChannelLists)
      {
        var tvList = this.DataRoot.GetChannelList(refList.SignalSource);
        if (tvList == null || tvList.SignalSource != refList.SignalSource)
        {
          log.AppendFormat("Skipped reference list {0}\r\n", refList.ShortCaption);
          continue;
        }
        ApplyReferenceList(refDataRoot, refList, -1, tvList, -1, true);
      }
    }

    /// <summary>
    /// Applies the ordering of a reference list to the TV list
    /// </summary>
    /// <param name="refDataRoot">object representing the whole reference list file</param>
    /// <param name="refList">the particular ChannelList to take the order from</param>
    /// <param name="refListPosIndex">the sublist index in the reference list that should be applied (0=main channel number, 1=fav1, ...)</param>
    /// <param name="tvList">the particular ChannelList to apply the order to</param>
    /// <param name="tvListPosIndex">the sublist index in the target list that should be changed (-1=main and all favs, 0=main channel number, 1=fav1, ...)</param>
    /// <param name="addProxyChannels">if true, a placeholder channel will be created in the tvList if there is no matching TV channel for a reference channel</param>
    /// <param name="positionOffset">can be used to shift the Pr# of the reference list so avoid conflicts with already assigned Pr# in the TV list</param>
    /// <param name="chanFilter">a function which is called for each channel in the reference list (with 2nd parameter=true) and TV list (2nd parameter=false) to determine if the channel is part of the reordering
    /// This is used to filter for analog/digital, radio/TV, antenna/cable/sat/ip
    /// </param>
    /// <param name="overwrite">controls whether Pr# will be reassigned to the reference channel when they are already in-use in the TV list</param>
    /// <param name="consecutive">when true, consecutive numbers will be used instead of the Pr# in the reference list when applying the order</param>
    public void ApplyReferenceList(DataRoot refDataRoot, ChannelList refList, int refListPosIndex, ChannelList tvList, int tvListPosIndex, bool addProxyChannels = true, int positionOffset = 0, Func<ChannelInfo, bool, bool> chanFilter = null, bool overwrite = true, bool consecutive = false)
    {
      // create Hashtable for exact channel lookup
      // the UID of a TV channel list contains a source-indicator (Analog, Cable, Sat), which may be undefined in the reference list)
      var onidTsidSid = new Dictionary<long, List<ChannelInfo>>();
      var chansByNewNr = new Dictionary<int, List<ChannelInfo>>();
      foreach (var channel in tvList.Channels)
      {
        var key = DvbKey(channel.OriginalNetworkId, channel.TransportStreamId, channel.ServiceId);
        if (key == 0)
          continue;
        var list = onidTsidSid.TryGet(key);
        if (list == null)
        {
          list = new List<ChannelInfo>();
          onidTsidSid.Add(key, list);
        }
        list.Add(channel);
      }

      var incNr = 1 + positionOffset;
      var refPos = refListPosIndex < 0 || !refDataRoot.SortedFavorites ? 0 : refListPosIndex;
      var refChannels = refList.Channels.OrderBy(ch => ch.GetOldPosition(refPos)).ToList();
      var newPos = Math.Max(0, tvListPosIndex);
      foreach (var refChannel in refChannels)
      {
        if (!(chanFilter?.Invoke(refChannel, true) ?? true))
          continue;

        var tvChannel = FindChannel(tvList, newPos, refChannel, onidTsidSid, overwrite);

        if (tvChannel != null)
        {
          if (!(chanFilter?.Invoke(tvChannel, false) ?? true))
            continue;

          var newNr = consecutive ? incNr++ : refChannel.GetOldPosition(refPos) + positionOffset;

          // handle conflicts when the number is already in-use
          if (chansByNewNr.TryGetValue(newNr, out var curChans))
          {
            if (!overwrite)
              continue;
            foreach (var chan in curChans)
              chan.SetPosition(newPos, -1);
          }
          else
          {
            curChans = new List<ChannelInfo>();
            chansByNewNr[newNr] = curChans;
          }
          curChans.Add(tvChannel);

          tvChannel.SetPosition(newPos, newNr);
          if (refDataRoot.CanSkip && this.DataRoot.CanSkip)
            tvChannel.Skip = refChannel.Skip;
          if (refDataRoot.CanLock && this.DataRoot.CanLock)
            tvChannel.Lock = refChannel.Lock;
          if (refDataRoot.CanHide && this.DataRoot.CanHide)
            tvChannel.Hidden = refChannel.Hidden;
          
          //tvChannel.IsDeleted = refChannel.IsDeleted;
          if ((tvChannel.SignalSource & SignalSource.Analog) != 0 && !string.IsNullOrEmpty(refChannel.Name))
          {
            tvChannel.Name = refChannel.Name;
            tvChannel.IsNameModified = true;
          }

          if (tvListPosIndex == -1)
            ApplyFavorites(refDataRoot, refChannel, tvChannel);
        }
        else if (addProxyChannels)
        {
          tvChannel = new ChannelInfo(refChannel.SignalSource, refChannel.Uid, refChannel.OldProgramNr, refChannel.Name);
          tvList.AddChannel(tvChannel);
        }
      }
    }

    private ChannelInfo FindChannel(ChannelList tvList, int subListIndex, ChannelInfo refChannel, Dictionary<long, List<ChannelInfo>> onidTsidSid, bool overwrite)
    {
      List<ChannelInfo> candidates;

      // try to find matching channels based on UID or ONID+TSID+SID+Transponder
      var channels = refChannel.Uid.EndsWith("0-0-0") ? new List<ChannelInfo>() : tvList.GetChannelByUid(refChannel.Uid).ToList();
      if (channels.Count == 0)
      {
        var key = DvbKey(refChannel.OriginalNetworkId, refChannel.TransportStreamId, refChannel.ServiceId);
        if (key != 0 && onidTsidSid.TryGetValue(key, out candidates))
          channels = candidates;

        // narrow the list down further when a transponder is also provided (i.e. the same channel can be received on multiple DVB-T frequencies)
        if (channels.Count > 1 && !string.IsNullOrEmpty(refChannel.ChannelOrTransponder))
        {
          candidates = channels.Where(ch => ch.ChannelOrTransponder == refChannel.ChannelOrTransponder).ToList();
          if (candidates.Count > 0)
            channels = candidates;
        }
      }
      

      candidates = channels.Where(c => c.GetPosition(subListIndex) == -1).ToList();
      if (candidates.Count > 0)
      {
        channels = candidates;
        if (channels.Count > 1)
        {
          candidates = channels.Where(ch => ch.IsDeleted == false).ToList();
          if (candidates.Count > 0)
            channels = candidates;
        }

        if (channels.Count > 0)
          return channels[0];
      }

      // try to find matching channels by name
      channels = tvList.GetChannelByName(refChannel.Name).Where(c => c.GetPosition(subListIndex) == -1).ToList();

      // if the reference list has information about a service type (tv/radio/data), then only consider channels matching it (or lacking service type information)
      var serviceType = refChannel.SignalSource & SignalSource.MaskTvRadioData;
      if (serviceType != 0 && serviceType != SignalSource.MaskTvRadioData)
      {
        channels = channels.Where(ch =>
        {
          var m = ch.SignalSource & SignalSource.MaskTvRadioData;
          return m == 0 || (m & serviceType) != 0;
        }).ToList();
      }

      if (channels.Count == 0)
        return null;

      if (channels.Count > 1)
      {
        // exact upper/lowercase matching (often there are channels like "DISCOVERY" and "Discovery")
        candidates = channels.Where(c => c.Name == refChannel.Name).ToList();
        if (candidates.Count > 0)
          channels = candidates;
      }
      if (channels.Count > 1)
      {
        // prefer unencrypted channels
        candidates = channels.Where(c => c.Encrypted.HasValue && c.Encrypted.Value == false).ToList();
        if (candidates.Count > 0)
          channels = candidates;
      }
      return channels[0];
    }

    long DvbKey(int onid, int tsid, int sid)
    {
      return ((long)onid << 32) | ((long)tsid << 16) | (long)sid;
    }

    private void ApplyFavorites(DataRoot refDataRoot, ChannelInfo refChannel, ChannelInfo tvChannel)
    {
      if (this.DataRoot.SortedFavorites)
      {
        if (!this.DataRoot.MixedSourceFavorites || refDataRoot.MixedSourceFavorites)
        {
          tvChannel.Favorites = refChannel.Favorites & DataRoot.SupportedFavorites;
          if (refDataRoot.SortedFavorites)
          {
            var c = Math.Min(refDataRoot.FavListCount, this.DataRoot.FavListCount);
            for (int i = 0; i < c; i++)
              tvChannel.SetPosition(i+1, refChannel.GetOldPosition(i+1));
          }
          else
            this.ApplyPrNrToFavLists(tvChannel);
        }
      }
      else
      {
        tvChannel.Favorites = refChannel.Favorites & DataRoot.SupportedFavorites;
        this.ApplyPrNrToFavLists(tvChannel);
      }
    }

    #endregion


    #region SetFavorites()
    public void SetFavorites(IList<ChannelInfo> list, int favIndex, bool set)
    {
      bool sortedFav = this.DataRoot.SortedFavorites;
      var favMask = (Favorites)(1 << favIndex);
      var favList = this.DataRoot.ChannelLists.FirstOrDefault(l => l.IsMixedSourceFavoritesList) ?? this.ChannelList;

      if (set)
      {
        int maxPosition = 0;
        if (sortedFav)
        {
          foreach (var channel in favList.Channels)
            maxPosition = Math.Max(maxPosition, channel.GetPosition(favIndex+1));
        }

        foreach (var channel in list)
        {
          if (sortedFav && channel.GetPosition(favIndex+1) == -1)
            channel.SetPosition(favIndex+1,++maxPosition);
          channel.Favorites |= favMask;
        }
      }
      else
      {
        foreach (var channel in list)
        {
          if (sortedFav && channel.GetPosition(favIndex+1) != -1)
            channel.SetPosition(favIndex+1, -1);
          channel.Favorites &= ~favMask;
        }

        // close gaps when needed
        if (sortedFav && !this.DataRoot.AllowGapsInFavNumbers)
        {
          int i = 0;
          foreach (var channel in favList.Channels.OrderBy(c => c.GetPosition(favIndex + 1)))
          {
            if (channel.GetPosition(favIndex+1) != -1)
              channel.SetPosition(favIndex+1, ++i);
          }
        }
      }
    }
    #endregion

    #region ApplyPrNrToFavLists()
    public void ApplyPrNrToFavLists()
    {
      if (!this.DataRoot.SortedFavorites)
        return;

      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach(var channel in list.Channels)
          this.ApplyPrNrToFavLists(channel);
      }
    }

    /// <summary>
    /// Set the number inside the favorites list to the same number as Pr#
    /// </summary>
    /// <param name="tvChannel"></param>
    private void ApplyPrNrToFavLists(ChannelInfo tvChannel)
    {
      var refMask = (ulong)tvChannel.Favorites;
      for (int i = 0; i < this.DataRoot.FavListCount; i++)
      {
        tvChannel.SetPosition(i+1, (refMask & 0x01) == 0 ? -1 : tvChannel.NewProgramNr);
        refMask >>= 1;
      }
    }

    #endregion

    #region SequentializeFavPos()
    /// <summary>
    /// Put the numbers in channel.FavIndex[i] in a sequential order, starting with 1
    /// </summary>
    /// <param name="channelList"></param>
    /// <param name="favCount">Number of favorite lists (i=0..favCount-1)</param>
    /// <returns>true if there were any changes</returns>
    public static bool SequentializeFavPos(ChannelList channelList, int favCount)
    {
      bool changed = false;
      for (int fav = 0; fav < favCount; fav++)
      {
        var list = channelList.Channels.Where(c => c.GetPosition(fav+1) >= 0).OrderBy(c => c.GetPosition(fav+1)).ToList();
        int i = 1;
        foreach (var channel in list)
        {
          if (channel.GetPosition(fav+1) != i)
          {
            channel.SetPosition(fav+1, ++i);
            changed = true;
          }
        }
      }
      return changed;
    }
    #endregion

  }
}
