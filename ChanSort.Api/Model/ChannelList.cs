using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChanSort.Api
{
  public class ChannelList
  {
    private readonly SignalSource source;
    private readonly SignalType type;
    private readonly IList<ChannelInfo> channels = new List<ChannelInfo>();
    private readonly Dictionary<string, IList<ChannelInfo>> channelByUid = new Dictionary<string, IList<ChannelInfo>>();
    private readonly Dictionary<int, ChannelInfo> channelByProgNr = new Dictionary<int, ChannelInfo>();
    private readonly Dictionary<string, IList<ChannelInfo>> channelByName = new Dictionary<string, IList<ChannelInfo>>();
    private int insertProgramNr = 1;
    private int duplicateUidCount;
    private int duplicateProgNrCount;

    public ChannelList(SignalSource source, SignalType type)
    {
      this.source = source;
      this.type = type;
    }

    public SignalSource SignalSource { get { return this.source; } }
    public SignalType SignalType { get { return this.type; } }
    public IList<ChannelInfo> Channels { get { return this.channels; } }
    public int Count { get { return channels.Count; } }
    public int DuplicateUidCount { get { return duplicateUidCount; } }
    public int DuplicateProgNrCount { get { return duplicateProgNrCount; } }

    #region ShortCaption
    public string ShortCaption
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        switch (this.source)
        {
          case SignalSource.AnalogC: sb.Append("Analog Cable"); break;
          case SignalSource.AnalogT: sb.Append("Analog Antenna"); break;
          case SignalSource.AnalogCT: sb.Append("Analog Cable/Antenna"); break;
          case SignalSource.DvbC: sb.Append("DVB-C"); break;
          case SignalSource.DvbT: sb.Append("DVB-T"); break;
          case SignalSource.DvbCT: sb.Append("DVB-C/T"); break;
          case SignalSource.DvbS: sb.Append("DVB-S"); break;
          case SignalSource.HdPlusD: sb.Append("Astra HD Plus"); break;
          default: sb.Append(this.source.ToString()); break;
        }
        switch (this.type)
        {
          case SignalType.Tv: sb.Append(" - TV"); break;
          case SignalType.Radio: sb.Append(" - Radio"); break;
          case SignalType.Mixed: break;
          default: sb.Append(this.type.ToString()); break;
        }
        return sb.ToString();
      }
    }
    #endregion

    #region Caption
    public string Caption
    {
      get 
      { 
        string cap = this.ShortCaption;
        int validChannelCount = this.Channels.Count(ch => ch.OldProgramNr != 0);
        return cap + " (" + validChannelCount + ")";
      }
    }
    #endregion

    #region InsertProgramNumber
    public int InsertProgramNumber
    {
      get { return this.Count == 0 ? 1 : this.insertProgramNr; }
      set { this.insertProgramNr = Math.Max(1, value); }
    }
    #endregion

    #region AddChannel()
    public string AddChannel(ChannelInfo ci)
    {
      IList<ChannelInfo> others;
      if (this.channelByUid.TryGetValue(ci.Uid, out others))
      {
        ++duplicateUidCount;
        ChannelInfo twin = others.FirstOrDefault(c => c.OldProgramNr == ci.OldProgramNr);
        if (twin != null)
        {
          string warning = null;
          if (ci.OldProgramNr != 0)
          {
            warning = string.Format(Resources.ChannelList_AddChannel__DuplicateUid,
                                     this.ShortCaption, ci.Name, twin.RecordIndex, twin.OldProgramNr, ci.RecordIndex,
                                     ci.OldProgramNr);
          }
          twin.Duplicates.Add(ci);
          ci.OldProgramNr = 0;
          return warning;
        }
      }
      else
      {
        others = new List<ChannelInfo>();
        this.channelByUid.Add(ci.Uid, others);
        others.Add(ci);
      }

      string warning2 = null;
      bool isDupeProgNr = false;
      if (ci.OldProgramNr != 0)
      {
        ChannelInfo other;
        this.channelByProgNr.TryGetValue(ci.OldProgramNr, out other);
        if (other != null)
        {
          warning2 = string.Format(Resources.ChannelList_ProgramNrAssignedToMultipleChannels,
                                  this.ShortCaption, ci.OldProgramNr, other.RecordIndex, other.Name, ci.RecordIndex, ci.Name);
          ++duplicateProgNrCount;
          isDupeProgNr = true;
        }
      }

      if (!isDupeProgNr)
        this.channelByProgNr[ci.OldProgramNr] = ci;

      var lowerName = ci.Name.ToLower().Trim();
      var byNameList = this.channelByName.TryGet(lowerName);
      if (byNameList == null)
      {
        byNameList = new List<ChannelInfo>();
        this.channelByName[lowerName] = byNameList;
      }
      byNameList.Add(ci);

      this.channels.Add(ci);
      
      return warning2;
    }
    #endregion

    #region GetChannelByUid()
    public IList<ChannelInfo> GetChannelByUid(string uid)
    {
      IList<ChannelInfo> channel;
      this.channelByUid.TryGetValue(uid, out channel);
      return channel ?? new List<ChannelInfo>(0);
    }
    #endregion

    #region ToString()
    public override string ToString()
    {
      return this.Caption;
    }
    #endregion

    #region GetChannelByName()
    public IEnumerable<ChannelInfo> GetChannelByName(string name)
    {
      var hits = this.channelByName.TryGet(name.ToLower().Trim());
      return hits ?? new List<ChannelInfo>();
    }
    #endregion

    #region GetChannelByNewProgNr()
    public IList<ChannelInfo> GetChannelByNewProgNr(int newProgNr)
    {
      return this.channels.Where(c => c.NewProgramNr == newProgNr).ToList();
    }
    #endregion
  }
}
