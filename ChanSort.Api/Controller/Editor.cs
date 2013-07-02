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
      foreach (
        var channel in
          this.ChannelList.Channels.Where(c => c.GetPosition(this.SubListIndex) >= progNrCopy)
              .OrderBy(c => c.GetPosition(this.SubListIndex)))
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
              if (prevNr != -1 && channel2.GetPosition(this.SubListIndex) != prevNr + 1)
                // don't pull down numbers after a gap
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
          channel.SetPosition(this.SubListIndex, -1);
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
        foreach (var refChannel in refList.Channels)
        {
          var tvChannels = tvList.GetChannelByUid(refChannel.Uid);
          ChannelInfo tvChannel = tvChannels.FirstOrDefault(c => c.GetPosition(this.SubListIndex) == -1);
          if (tvChannel != null)
          {
            tvChannel.SetPosition(this.SubListIndex, refChannel.OldProgramNr);
            tvChannel.Favorites = refChannel.Favorites;
            tvChannel.Skip = refChannel.Skip;
            tvChannel.Lock = refChannel.Lock;
            tvChannel.Hidden = refChannel.Hidden;
            tvChannel.IsDeleted = refChannel.IsDeleted;
          }
          else
          {
            tvChannel = new ChannelInfo(refChannel.SignalSource, refChannel.Uid, refChannel.OldProgramNr,
                                        refChannel.Name);
            tvList.AddChannel(tvChannel);
          }
        }
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

          if (appChannel.NewProgramNr == -1 && mode == UnsortedChannelMode.MarkDeleted)
            continue;

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
  }
}
