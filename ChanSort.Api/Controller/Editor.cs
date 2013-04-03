using System.Collections.Generic;
using System.Linq;

namespace ChanSort.Api
{
  public class Editor
  {
    public DataRoot DataRoot;
    public ChannelList ChannelList;
    private UnsortedChannelMode unsortedChannelMode;

#if false
    #region LoadDvbViewerFiles()
    private void LoadDvbViewerFiles()
    {
      List<string> fileList;
      Dictionary<string, string> satPosition;
      if (!LoadSatMappingFile(out satPosition, out fileList))
        return;

      foreach (var file in fileList)
      {
        try
        {
          string fullPath = Path.IsPathRooted(file)
                              ? file
                              : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), file);
          this.LoadDvbViewerChannelFile(fullPath, satPosition);
        }
        catch
        {
        }
      }
    }
    #endregion

    #region LoadSatMappingFile()
    private static bool LoadSatMappingFile(out Dictionary<string, string> satPosition, out List<string> fileList)
    {
      satPosition = null;
      fileList = null;
      string satMappingFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "sat-mapping.ini");
      if (!File.Exists(satMappingFile))
        return false;

      fileList = new List<string>();
      satPosition = new Dictionary<string, string>();
      string section = null;
      using (var stream = new StreamReader(satMappingFile, Encoding.UTF8))
      {
        string line;
        while ((line = stream.ReadLine()) != null)
        {
          line = line.Trim();
          if (line.Length == 0 || line.StartsWith(";"))
            continue;
          if (line.StartsWith("[") && line.EndsWith("]"))
          {
            section = line;
            continue;
          }
          if (section == "[Satellites]")
          {
            var parts = line.Split(new[] { '=' }, 2);
            if (parts.Length == 2)
              satPosition[parts[0].ToLower()] = parts[1].ToUpper();
          }
          else if (section == "[DvbViewerChannelFiles]")
          {
            fileList.Set(line);
          }
        }
      }
      return true;
    }
    #endregion

    #region LoadDvbViewerChannelFile()
    private void LoadDvbViewerChannelFile(string file, IDictionary<string, string> satPosition)
    {
      string[] unencrypted = { "18", "19", "26", "146", "154" };

      using (var stream = new StreamReader(file, Encoding.Default))
      {
        string line;
        bool first = true;
        while ((line = stream.ReadLine()) != null)
        {
          if (first)
          {
            first = false;
            continue;
          }
          var col = line.Split(new[] { '\t' });
          if (col.Length < 21)
            continue;

          string satId;
          if (!satPosition.TryGetValue(col[0].ToLower(), out satId))
            continue;

          StringBuilder uid = new StringBuilder();
          uid.Append("S").Append(satId).Append("-").Append(col[20]).Append("-").Append(col[18]).Append("-").Append(col[17]);

          ChannelInfo channel;
          if (!TvChannelByUid.TryGetValue(uid.ToString(), out channel))
            continue;
          channel.Encrypted = Array.IndexOf(unencrypted, col[19]) < 0;
        }
      }
    }
    #endregion
#endif

    #region AddChannels()

    public ChannelInfo AddChannels(IList<ChannelInfo> channels)
    {
      int count = channels.Count(channel => channel.NewProgramNr == 0);
      if (count == 0) return null;

      ChannelInfo lastInsertedChannel = null;
      int progNr = this.ChannelList.InsertProgramNumber;
      int relativeChannelNumber = 0;
      foreach(var channel in this.ChannelList.Channels)
      {
        if (channel.NewProgramNr != 0 && channel.NewProgramNr >= progNr)
        {
          int gap = count - (channel.NewProgramNr - progNr - relativeChannelNumber);
          if (gap <= 0)
            break;
          channel.NewProgramNr += gap;
          ++relativeChannelNumber;
        }
      }

      foreach (var channel in channels)
      {
        if (channel.NewProgramNr != 0)
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

    public void RemoveChannels(IList<ChannelInfo> channels)
    {
      if (channels.Count == 0) return;

      this.ChannelList.InsertProgramNumber = channels[0].NewProgramNr;
      foreach (var channel in channels)
      {
        if (channel.NewProgramNr == 0)
          continue;
        int prevNr = channel.NewProgramNr;
        foreach (var channel2 in this.ChannelList.Channels)
        {
          if (channel2.NewProgramNr > channel.NewProgramNr)
          {
            if (prevNr != -1 && channel2.NewProgramNr != prevNr + 1) // don't pull down numbers after a gap
              break;
            prevNr = channel2.NewProgramNr;
            --channel2.NewProgramNr;
          }
        }
        channel.NewProgramNr = 0;
      }

      this.DataRoot.NeedsSaving = true;
    }

    #endregion

    #region MoveChannels()

    public void MoveChannels(List<ChannelInfo> channels, bool up)
    {
      if (channels.Count == 0)
        return;
      if (up && channels[0].NewProgramNr < 2)
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
    public void SetSlotNumber(IList<ChannelInfo> channels, int slot, bool swap)
    {
      if (channels.Count == 0) return;
      if (swap)
      {
        foreach (var channel in channels)
        {
          if (slot != 0)
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
        this.RemoveChannels(channels);
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
        this.RemoveChannels(list);
        this.ChannelList.InsertProgramNumber = progNr++;
        this.AddChannels(list);
        this.DataRoot.NeedsSaving = true;
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

          if (appChannel.NewProgramNr <= 0 && mode == UnsortedChannelMode.Hide)
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
      if (channel.NewProgramNr != 0)
        return channel.NewProgramNr.ToString("d4");

      // eventually hide unsorted channels
      if (this.unsortedChannelMode == UnsortedChannelMode.Hide)
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
      if (prNr == 0)
      {
        if (appChannel.OldProgramNr != 0 && this.unsortedChannelMode != UnsortedChannelMode.Hide)
          prNr = ++maxPrNr;
      }
      return prNr;
    }
    #endregion

    #endregion
  }
}
