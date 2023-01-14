using System;
using System.Collections.Generic;
using System.Linq;

namespace ChanSort.Api
{
  public class ChannelList
  {
    private readonly Dictionary<string, IList<ChannelInfo>> channelByUid = new ();
    private readonly Dictionary<int, ChannelInfo> channelByProgNr = new ();
    private readonly Dictionary<string, IList<ChannelInfo>> channelByName = new ();
    private readonly Dictionary<long, ChannelInfo> channelById = new();
    private int insertProgramNr = 1;
    private int duplicateUidCount;
    private int duplicateProgNrCount;

    public static List<string> DefaultVisibleColumns { get; set; } = new List<string>(); // initialized by MainForm

    public ChannelList(SignalSource source, string caption)
    {
      if ((source & SignalSource.MaskAnalogDigital) == 0)
        source |= SignalSource.MaskAnalogDigital;
      if ((source & SignalSource.MaskAntennaCableSat) == 0)
        source |= SignalSource.MaskAntennaCableSat;
      if ((source & SignalSource.MaskTvRadioData) == 0)
        source |= SignalSource.MaskTvRadioData;

      this.SignalSource = source;
      this.ShortCaption = caption;
      this.FirstProgramNumber = (source & SignalSource.Digital) != 0 ? 1 : 0;
      this.VisibleColumnFieldNames = DefaultVisibleColumns.ToList(); // create copy of default list, so it can be modified
    }

    public string ShortCaption { get; set; }
    public SignalSource SignalSource { get; set; }
    public IList<ChannelInfo> Channels { get; } = new List<ChannelInfo>();

    public int Count => Channels.Count;
    public int DuplicateUidCount => duplicateUidCount;
    public int DuplicateProgNrCount => duplicateProgNrCount;
    public bool ReadOnly { get; set; }
    public int MaxChannelNameLength { get; set; }
    public int PresetProgramNrCount { get; private set; }

    public IList<string> VisibleColumnFieldNames;

    /// <summary>
    /// Set for helper lists used to manage favorites mixed from all input sources.
    /// When true, the UI won't show the "Pr#" tab but will show "Fav A-D" tabs and a "Source" column.
    /// </summary>
    public bool IsMixedSourceFavoritesList { get; set; }

    #region Caption
    public string Caption
    {
      get 
      { 
        string cap = this.ShortCaption;
        int validChannelCount = this.Channels.Count(ch => ch.OldProgramNr != -1);
        return cap + " (" + validChannelCount + ")";
      }
    }
    #endregion

    #region InsertProgramNumber
    public int InsertProgramNumber
    {
      get { return this.Count == 0 ? 1 : this.insertProgramNr; }
      set { this.insertProgramNr = Math.Max(this.FirstProgramNumber, value); }
    }
    #endregion

    public int FirstProgramNumber { get; set; }

    #region AddChannel()
    public string AddChannel(ChannelInfo ci)
    {
      IList<ChannelInfo> others;
      if (!this.channelByUid.TryGetValue(ci.Uid, out others))
      {
        others = new List<ChannelInfo>();
        this.channelByUid.Add(ci.Uid, others);
      }
      if (others.Count > 0 && !ci.IsDeleted)
        ++duplicateUidCount;
      others.Add(ci);

      string warning2 = null;
      bool isDupeProgNr = false;
      if (ci.OldProgramNr >= 0)
      {
        ChannelInfo other;
        this.channelByProgNr.TryGetValue(ci.OldProgramNr, out other);
        if (other != null)
        {
          var format = Resources.ChannelList_ProgramNrAssignedToMultipleChannels.Replace("{1}", "{1,5}");
          warning2 = string.Format(format,
                                  this.ShortCaption, ci.OldProgramNr, other.RecordIndex, other.Name, ci.RecordIndex, ci.Name);
          ++duplicateProgNrCount;
          isDupeProgNr = true;
        }
      }

      if (!isDupeProgNr)
        this.channelByProgNr[ci.OldProgramNr] = ci;

      var lowerName = (ci.Name ?? "").ToLowerInvariant().Trim();
      var byNameList = this.channelByName.TryGet(lowerName);
      if (byNameList == null)
      {
        byNameList = new List<ChannelInfo>();
        this.channelByName[lowerName] = byNameList;
      }
      byNameList.Add(ci);

      if (ci.ProgramNrPreset != 0)
        ++this.PresetProgramNrCount;

      this.channelById[ci.RecordIndex] = ci;

      this.Channels.Add(ci);
      
      return warning2;
    }
    #endregion

    #region GetChannelByUid()
    public IList<ChannelInfo> GetChannelByUid(string uid)
    {
      this.channelByUid.TryGetValue(uid, out var channel);
      return channel ?? new List<ChannelInfo>(0);
    }
    #endregion

    #region ToString()
    public override string ToString()
    {
      return this.Caption;
    }
    #endregion

    #region GetChannelById()
    public ChannelInfo GetChannelById(long id) => this.channelById.TryGet(id);
    #endregion

    #region GetChannelByName()
    public IEnumerable<ChannelInfo> GetChannelByName(string name)
    {
      name ??= "";
      var hits = this.channelByName.TryGet(name.ToLowerInvariant().Trim());
      return hits ?? new List<ChannelInfo>();
    }
    #endregion

    #region GetChannelByNewProgNr()
    public IList<ChannelInfo> GetChannelByNewProgNr(int newProgNr)
    {
      return this.Channels.Where(c => c.NewProgramNr == newProgNr).ToList();
    }
    #endregion

    #region GetChannelsByNewOrder()
    public IList<ChannelInfo> GetChannelsByNewOrder()
    {
      return this.Channels.OrderBy(c => c.NewProgramNr).ToList();
    }
    #endregion

    #region RemoveChannel()
    public void RemoveChannel(ChannelInfo channel)
    {
      this.Channels.Remove(channel);
      var list = this.channelByUid.TryGet(channel.Uid);
      if (list != null && list.Contains(channel))
        list.Remove(channel);
      list = this.channelByName.TryGet(channel.Name);
      if (list != null && list.Contains(channel))
        list.Remove(channel);
      var chan = this.channelByProgNr.TryGet(channel.OldProgramNr);
      if (ReferenceEquals(chan, channel))
        this.channelByProgNr.Remove(channel.OldProgramNr);
    }
    #endregion


    #region Get/SetFavListCaption()

    private readonly Dictionary<int, string> favListCaptions = new Dictionary<int, string>();

    public void SetFavListCaption(int favIndex, string caption)
    {
      favListCaptions[favIndex] = caption;
    }

    public string GetFavListCaption(int favIndex, bool asTabCaption = false)
    {
      if (favIndex < 0)
        return "";
      var hasCaption = favListCaptions.TryGetValue(favIndex, out var caption);
      if (!asTabCaption)
        return caption;
      var letter = favIndex < 26 ? ((char)('A' + favIndex)).ToString() : (favIndex + 1).ToString();
      return hasCaption && !string.IsNullOrEmpty(caption) ? letter + ": " + caption : "Fav " + letter;
    }
    #endregion

  }
}
