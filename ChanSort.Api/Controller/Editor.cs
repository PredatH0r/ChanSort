using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChanSort.Api
{
  public class Editor
  {
    public DataRoot DataRoot;
    public ChannelList ChannelList;
    private UnsortedChannelMode unsortedChannelMode;

    #region AddChannels()

    public ChannelInfo AddChannels(IList<ChannelInfo> channels)
    {
      int count = channels.Count(channel => channel.NewProgramNr == -1);
      if (count == 0) return null;

      ChannelInfo lastInsertedChannel = null;
      int progNr = this.ChannelList.InsertProgramNumber;
      int relativeChannelNumber = 0;
      int progNrCopy = progNr; // prevent "access to modified closure" warning
      foreach(var channel in this.ChannelList.Channels.Where(c => c.NewProgramNr>=progNrCopy).OrderBy(c=>c.NewProgramNr))
      {
        int gap = count - (channel.NewProgramNr - progNr - relativeChannelNumber);
        if (gap > 0)
        {
          channel.NewProgramNr += gap;
          ++relativeChannelNumber;
        }
      }

      foreach (var channel in channels)
      {
        if (channel.NewProgramNr != -1)
        {
          // TODO notify user
          continue;
        }

        channel.NewProgramNr = progNr++;
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

      this.ChannelList.InsertProgramNumber = channels[0].NewProgramNr;
      var orderedChannelList = this.ChannelList.Channels.Where(c => c.NewProgramNr != -1).OrderBy(c => c.NewProgramNr);
      foreach (var channel in channels)
      {
        if (channel.NewProgramNr == -1)
          continue;
        if (closeGap)
        {
          int prevNr = channel.NewProgramNr;
          foreach (var channel2 in orderedChannelList)
          {
            if (channel2.NewProgramNr > channel.NewProgramNr)
            {
              if (prevNr != -1 && channel2.NewProgramNr != prevNr + 1) // don't pull down numbers after a gap
                break;
              prevNr = channel2.NewProgramNr;
              --channel2.NewProgramNr;
            }
          }
        }
        channel.NewProgramNr = -1;
      }

      this.DataRoot.NeedsSaving = true;
    }

    #endregion

    #region MoveChannels()

    public void MoveChannels(IList<ChannelInfo> channels, bool up)
    {
      if (channels.Count == 0)
        return;
      if (up && channels[0].NewProgramNr <= this.ChannelList.FirstProgramNumber)
        return;

      int delta = (up ? - 1 : +1);
      foreach (var channel in channels)
      {
        int newProgramNr = channel.NewProgramNr + delta;
        ChannelInfo channelAtNewProgramNr = this.ChannelList.Channels.FirstOrDefault(ch => ch.NewProgramNr == newProgramNr);
        if (channelAtNewProgramNr != null)
          channelAtNewProgramNr.NewProgramNr -= delta;
        channel.NewProgramNr += delta;
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
      var programNumbers = selectedChannels.Select(ch => ch.NewProgramNr).ToList();
      for (int i = 0; i < sortedChannels.Count; i++)
        sortedChannels[i].NewProgramNr = programNumbers[i];
      
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
              other.NewProgramNr = channel.NewProgramNr;
          }
          channel.NewProgramNr = slot++;
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
      int progNr = channels.Min(ch => ch.NewProgramNr);
      foreach(var channel in channels)
      {
        if (channel.NewProgramNr == progNr)
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
          channel.NewProgramNr = -1;
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
          ChannelInfo tvChannel = tvChannels.FirstOrDefault(c => c.NewProgramNr == -1);
          if (tvChannel != null)
          {
            tvChannel.NewProgramNr = refChannel.OldProgramNr;
            tvChannel.Favorites = refChannel.Favorites;
            tvChannel.Skip = refChannel.Skip;
            tvChannel.Lock = refChannel.Lock;
            tvChannel.Hidden = refChannel.Hidden;
            tvChannel.IsDeleted = refChannel.IsDeleted;
          }
          else
          {
            tvChannel = new ChannelInfo(refChannel.SignalSource, refChannel.Uid, refChannel.OldProgramNr, refChannel.Name);
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

          int progNr = GetNewProgramNr(appChannel, ref maxProgNr);
          appChannel.NewProgramNr = progNr;
        }
      }
    }

    #region ChanSortCriteria()
    private string ChanSortCriteria(ChannelInfo channel)
    {
      // explicitly sorted
      if (channel.NewProgramNr != -1)
        return channel.NewProgramNr.ToString("d4");

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

    #region GetNewProgramNr()
    private int GetNewProgramNr(ChannelInfo appChannel, ref int maxPrNr)
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
  }
}
