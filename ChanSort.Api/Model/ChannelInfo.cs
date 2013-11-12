using System;
using System.Collections.Generic;

namespace ChanSort.Api
{
  public class ChannelInfo
  {
    private const int MAX_FAV_LISTS = 5;

    private string uid;
    /// <summary>
    /// List of channels that have the same UID as this channel and were not added to the channel list directly
    /// </summary>
    public readonly List<ChannelInfo> Duplicates = new List<ChannelInfo>();

    public virtual bool IsDeleted { get; set; }
    public SignalSource SignalSource { get; set; }
    public int RecordIndex { get; set; }
    public int RecordOrder { get; set; }
    public int OldProgramNr { get; set; }
    public int NewProgramNr { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public Favorites Favorites { get; set; }
    public bool Skip { get; set; }
    public bool Lock { get; set; }
    public bool Hidden { get; set; }
    public bool? Encrypted { get; set; }
    public string ChannelOrTransponder { get; set; }
    public string Satellite { get; set; }
    public decimal FreqInMhz { get; set; }
    public char Polarity { get; set; }
    public int ServiceId { get; set; }
    public int VideoPid { get; set; }
    public int AudioPid { get; set; }
    public int OriginalNetworkId { get; set; }
    public int TransportStreamId { get; set; }
    public int SymbolRate { get; set; }
    public int ServiceType { get; set; }
    public string Debug { get; private set; }
    public string SatPosition { get; set; }
    public Transponder Transponder { get; set; }
    public IList<int> FavIndex { get; private set; }
    public int ProgramNrPreset { get; set; }

    public bool IsNameModified { get; set; }

    #region ctor()
    protected ChannelInfo()
    {
      this.NewProgramNr = -1;
      this.FavIndex = new List<int>(MAX_FAV_LISTS);
      for (int i = 0; i < MAX_FAV_LISTS; i++)
        this.FavIndex.Add(-1);
    }

    /// <summary>
    /// Constructor for exiting TV channel
    /// </summary>
    public ChannelInfo(SignalSource source, int index, int oldProgNr, string name) : this()
    {
      this.SignalSource = source;
      this.RecordIndex = index;
      this.RecordOrder = index;
      this.NewProgramNr = -1;
      this.OldProgramNr = oldProgNr;
      this.Name = name;
      this.Encrypted = null;
    }

    /// <summary>
    /// Constructor for reference list channels which no longer exist in TV list
    /// </summary>
    public ChannelInfo(SignalSource source, string uid, int newProgNr, string name) : this()
    {
      this.SignalSource = source;
      this.Uid = uid;
      this.RecordIndex = -1;
      this.RecordOrder = -1;
      this.OldProgramNr = -1;
      this.NewProgramNr = newProgNr;
      this.Name = name;
      this.Encrypted = null;
    }
    #endregion

    #region Uid
    public string Uid
    {
      get
      {
        if (this.uid == null)
        {
          if ((this.SignalSource & SignalSource.Digital) == 0)
            this.uid = "A-0-" + (int)(this.FreqInMhz*20) + "-0";
          else if ((this.SignalSource & SignalSource.Sat) != 0)
            this.uid = "S" + this.SatPosition + "-" + this.OriginalNetworkId + "-" + this.TransportStreamId + "-" + this.ServiceId;
          else
            this.uid = "C-" + this.OriginalNetworkId + "-" + this.TransportStreamId + "-" + this.ServiceId + "-" + this.ChannelOrTransponder;
        }
        return this.uid;
      }
      set { this.uid = value; }
    }
    #endregion

    #region ToString(), Equals(), GetHashCode()

    public override string ToString()
    {
      string nr = this.NewProgramNr != -1 ? this.NewProgramNr.ToString() : "@" + this.RecordIndex;
      return nr + ": " + this.Name;
    }

    public override bool Equals(object obj)
    {
      //ChannelInfo that = obj as ChannelInfo;
      //return that != null && this.Uid == that.Uid && this.OldProgramNr == that.OldProgramNr;
      return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
      return this.Uid.GetHashCode() + this.OldProgramNr;
    }

    #endregion

    #region NetworkName, NetworkOperator
    public string NetworkName
    {
      get
      {
        var network = LookupData.Instance.GetNetwork(this.OriginalNetworkId);
        return network == null ? null : network.Name;
      }
    }

    public string NetworkOperator
    {
      get
      {
        var network = LookupData.Instance.GetNetwork(this.OriginalNetworkId);
        return network == null ? null : network.Operator;
      }
    }
    #endregion

    #region ServiceTypeName
    public string ServiceTypeName { get { return LookupData.Instance.GetServiceTypeDescription(this.ServiceType); } }
    #endregion

    #region GetFavString()
    public static string GetFavString(Favorites favorites)
    {
      string sep = "";
      string text = "";
      foreach (Favorites favMask in Enum.GetValues(typeof(Favorites)))
      {
        if ((favorites & favMask) != 0)
        {
          text += sep + favMask.ToString();
          sep = ",";
        }
      }
      return text;
    }
    #endregion

    #region ParseFavString()
    public static Favorites ParseFavString(string value)
    {
      Favorites favMask = 0;
      foreach (Favorites fav in Enum.GetValues(typeof (Favorites)))
      {
        foreach (char c in value)
        {
          if (c == fav.ToString()[0])
          {
            favMask |= fav;
            break;
          }
        }
      }
      return favMask;
    }
    #endregion

    #region AddDebug()
    public void AddDebug(byte val)
    {
      if (this.Debug == null)
        this.Debug = val.ToString("x2");
      else
        this.Debug += " " + val.ToString("x2");
    }

    public void AddDebug(ushort val)
    {
      if (this.Debug == null)
        this.Debug = val.ToString("x2");
      else
        this.Debug += " " + val.ToString("x4");
    }

    public void AddDebug(byte[] data, int offset, int len)
    {
      for (int i = 0; i < len; i++)
        this.AddDebug(data[offset + i]);
    }
    #endregion

    #region UpdateRawData()
    public virtual void UpdateRawData()
    {
    }
    #endregion

    #region ChangeEncoding()
    public virtual void ChangeEncoding(System.Text.Encoding encoding)
    {
    }
    #endregion

    #region GetPosition(), SetPosition(), ChangePosition()

    public int GetPosition(int subListIndex)
    {
      return subListIndex == 0 ? this.NewProgramNr : this.FavIndex[subListIndex - 1];
    }

    public void SetPosition(int subListIndex, int newPos)
    {
      if (subListIndex == 0)
        this.NewProgramNr = newPos;
      else
      {
        this.FavIndex[subListIndex - 1] = newPos;
        int mask = 1 << (subListIndex - 1);
        if (newPos == -1)
          this.Favorites &= (Favorites)~mask;
        else
          this.Favorites |= (Favorites)mask;
      }
    }

    internal void ChangePosition(int subListIndex, int delta)
    {
      if (subListIndex == 0)
        this.NewProgramNr += delta;
      else
        this.FavIndex[subListIndex - 1] += delta;      
    }
    #endregion
  }
}
