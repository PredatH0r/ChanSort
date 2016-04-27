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
    private UnsortedChannelMode unsortedChannelMode;

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

      int delta = (up ? -1 : +1);
      foreach (var channel in (up ? channels : channels.Reverse()))
      {
        int newProgramNr = channel.GetPosition(this.SubListIndex) + delta;
        ChannelInfo channelAtNewPos =
          this.ChannelList.Channels.FirstOrDefault(ch => ch.GetPosition(this.SubListIndex) == newProgramNr);
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
        if (tvList == null)
        {
          log.AppendFormat("Skipped reference list {0}\r\n", refList.ShortCaption);
          continue;
        }
        ApplyReferenceList(refDataRoot, refList, tvList);
      }
    }

    public void ApplyReferenceList(DataRoot refDataRoot, ChannelList refList, ChannelList tvList, bool addProxyChannels = true, int positionOffset = 0, Predicate<ChannelInfo> chanFilter = null)
    {
      foreach (var refChannel in refList.Channels)
      {
        if (!(chanFilter?.Invoke(refChannel) ?? true))
          continue;

        var tvChannels = tvList.GetChannelByUid(refChannel.Uid);
        if (tvChannels.Count == 0 && !string.IsNullOrWhiteSpace(refChannel.Name))
          tvChannels = tvList.GetChannelByName(refChannel.Name).ToList();
        ChannelInfo tvChannel = tvChannels.FirstOrDefault(c => c.GetPosition(0) == -1);
        if (tvChannel == null && tvChannels.Count > 0)
          tvChannel = tvChannels[0];

        if (tvChannel != null)
        {
          if (!(chanFilter?.Invoke(tvChannel) ?? true))
            continue;

          var curChans = tvList.GetChannelByNewProgNr(refChannel.OldProgramNr + positionOffset);
          foreach (var chan in curChans)
            chan.NewProgramNr = -1;

          tvChannel.SetPosition(0, refChannel.OldProgramNr + positionOffset);
          tvChannel.Skip = refChannel.Skip;
          tvChannel.Lock = refChannel.Lock;
          tvChannel.Hidden = refChannel.Hidden;
          tvChannel.IsDeleted = refChannel.IsDeleted;
          if ((tvChannel.SignalSource & SignalSource.Analog) != 0 && !string.IsNullOrEmpty(refChannel.Name))
          {
            tvChannel.Name = refChannel.Name;
            tvChannel.IsNameModified = true;
          }

          ApplyFavorites(refDataRoot, refChannel, tvChannel);
        }
        else if (addProxyChannels)
        {
          tvChannel = new ChannelInfo(refChannel.SignalSource, refChannel.Uid, refChannel.OldProgramNr, refChannel.Name);
          tvList.AddChannel(tvChannel);
        }
      }
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
            var c = Math.Min(refChannel.FavIndex.Count, tvChannel.FavIndex.Count);
            for (int i = 0; i < c; i++)
              tvChannel.FavIndex[i] = refChannel.FavIndex[i];
          }
          else
            this.ApplyPrNrToFavLists(tvChannel);
        }
      }
      else
      {
        tvChannel.Favorites = refChannel.Favorites & DataRoot.SupportedFavorites;
      }
    }

    #endregion

    #region AutoNumberingForUnassignedChannels()

    public void AutoNumberingForUnassignedChannels(UnsortedChannelMode mode)
    {
      this.unsortedChannelMode = mode;
      foreach (var list in DataRoot.ChannelLists)
      {
        var sortedChannels = list.Channels.OrderBy(ChanSortCriteria).ToList();
        int maxProgNr = 0;

        foreach (var appChannel in sortedChannels)
        {
          if (appChannel.RecordIndex < 0)
            continue;

          if (appChannel.NewProgramNr == -1)
          {
            if (mode == UnsortedChannelMode.MarkDeleted)
              continue;
            appChannel.Hidden = true;
            appChannel.Skip = true;
          }

          int progNr = GetNewPogramNr(appChannel, ref maxProgNr);
          appChannel.NewProgramNr = progNr;
        }
      }
    }

    #region ChanSortCriteria()

    private string ChanSortCriteria(ChannelInfo channel)
    {
      // explicitly sorted
      if (channel.GetPosition(this.SubListIndex) != -1)
        return channel.GetPosition(this.SubListIndex).ToString("d4");

      // eventually hide unsorted channels
      if (this.unsortedChannelMode == UnsortedChannelMode.MarkDeleted)
        return "Z";

      // eventually append in old order
      if (this.unsortedChannelMode == UnsortedChannelMode.AppendInOrder)
        return "B" + channel.OldProgramNr.ToString("d4");

      // sort alphabetically, with "." and "" on the bottom
      if (channel.Name == ".")
        return "B";
      if (channel.Name == "")
        return "C";
      return "A" + channel.Name;
    }

    #endregion

    #region GetNewPogramNr()

    private int GetNewPogramNr(ChannelInfo appChannel, ref int maxPrNr)
    {
      int prNr = appChannel.NewProgramNr;
      if (prNr > maxPrNr)
        maxPrNr = prNr;
      if (prNr == -1)
      {
        if (appChannel.OldProgramNr != -1 && this.unsortedChannelMode != UnsortedChannelMode.MarkDeleted)
          prNr = ++maxPrNr;
      }
      return prNr;
    }

    #endregion

    #endregion

    #region SetFavorites()
    public void SetFavorites(List<ChannelInfo> list, Favorites favorites, bool set)
    {
      bool sortedFav = this.DataRoot.SortedFavorites;
      int favIndex = 0;
      if (sortedFav)
      {
        for (int mask = (int) favorites; (mask & 1) == 0; mask >>= 1)
          ++favIndex;
      }

      if (set)
      {
        int maxPosition = 0;
        if (sortedFav)
        {
          foreach (var channel in this.ChannelList.Channels)
            maxPosition = Math.Max(maxPosition, channel.FavIndex[favIndex]);
        }

        foreach (var channel in list)
        {
          if (sortedFav && channel.FavIndex[favIndex] == -1)
            channel.FavIndex[favIndex] = ++maxPosition;
          channel.Favorites |= favorites;
        }
      }
      else
      {
        foreach (var channel in list)
        {
          if (sortedFav && channel.FavIndex[favIndex] != -1)
          {
            channel.FavIndex[favIndex] = -1;
            // TODO close gap by pulling down higher numbers
          }
          channel.Favorites &= ~favorites;
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
      var supMask = (int)this.DataRoot.SupportedFavorites;
      var refMask = (int)tvChannel.Favorites;
      for (int i = 0; supMask != 0; i++)
      {
        tvChannel.FavIndex[i] = (refMask & 0x01) == 0 ? -1 : tvChannel.OldProgramNr;
        supMask >>= 1;
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
        var list = channelList.Channels.Where(c => c.FavIndex[fav] >= 0).OrderBy(c => c.FavIndex[fav]).ToList();
        int i = 1;
        foreach (var channel in list)
        {
          if (channel.FavIndex[fav] != i)
          {
            channel.FavIndex[fav] = ++i;
            changed = true;
          }
        }
      }
      return changed;
    }
    #endregion

  }
}
