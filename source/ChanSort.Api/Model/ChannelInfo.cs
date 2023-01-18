using System;
using System.Collections.Generic;

namespace ChanSort.Api
{
  public class ChannelInfo
  {
    private const int MaxFavListsWithFlags = 8;

    private string uid;
    private string serviceTypeName;
    private int newProgramNr;
    
    public virtual bool IsDeleted { get; set; }
    public SignalSource SignalSource { get; set; }
    public string Source { get; set; }
    
    /// <summary>
    /// Index or ID of the data record
    /// </summary>
    public long RecordIndex { get; set; }

    /// <summary>
    /// logical record order (might be different from the index, like old LG TLL files with a linked list of record indices)
    /// </summary>
    public int RecordOrder { get; set; }

    /// <summary>
    /// original program number from the file, except for channels with IsDeleted==true, which will have the value -1
    /// </summary>
    public int OldProgramNr { get; set; }

    /// <summary>
    /// new program number or -1, if the channel isn't assigned a number or has IsDeleted==true
    /// </summary>
    public int NewProgramNr
    {
      get => newProgramNr;
      set
      {
        if (value == 0)
        {
        }
        newProgramNr = value;
      }
    }

    public string Name { get; set; }
    public string ShortName { get; set; }
    public bool Skip { get; set; }
    public bool Lock { get; set; }
    public bool Hidden { get; set; }
    public bool? Encrypted { get; set; }
    public string ChannelOrTransponder { get; set; }
    public string Satellite { get; set; }
    public decimal FreqInMhz { get; set; }
    public char Polarity { get; set; }
    public int ServiceId { get; set; }
    public int PcrPid { get; set; }
    public int VideoPid { get; set; }
    public int AudioPid { get; set; }
    public int OriginalNetworkId { get; set; }
    public int TransportStreamId { get; set; }
    public string Provider { get; set; }
    public int SymbolRate { get; set; }
    public int ServiceType { get; set; }
    public string SatPosition { get; set; }
    public Transponder Transponder { get; set; }
    
    /// <summary>
    /// Defines whether favorite lists have individual ordering (FavIndex, OldFavIndex) or if they use the main channel number (Favorites bitmask)
    /// </summary>
    public FavoritesMode FavMode { get; set; } 
    /// <summary>
    /// Bitmask in which favorite lists the channel is included
    /// </summary>
    public Favorites Favorites { get; set; }
    /// <summary>
    /// current number of the channel in the various favorite lists (if individual sorting is supported)
    /// </summary>
    private List<int> FavIndex { get; }
    /// <summary>
    /// original number of the channel in the various favorite lists (if individual sorting is supported)
    /// </summary>
    private List<int> OldFavIndex { get; }

    /// <summary>
    /// predefined LCN (logical channel number) assigned by TV firmware or cable/sat operator
    /// </summary>
    public int ProgramNrPreset { get; set; }

    public bool IsNameModified { get; set; }


    /// <summary>
    /// A proxy channel is inserted into the current channel list when there was no match for a reference list channel
    /// </summary>
    public bool IsProxy => this.RecordIndex == -1;

    /// <summary>
    /// can be used to store the offset of the raw data in the input file, e.g. for re-parsing names with different encoding
    /// </summary>
    public int RawDataOffset { get; set; }

    /// <summary>
    /// arbitrary information that can be shown in a UI column to assist in analyzing a file format while coding a plugin
    /// </summary>
    public string Debug { get; private set; }




    #region ctor()
    protected ChannelInfo()
    {
      this.OldProgramNr = -1;
      this.NewProgramNr = -1;
      this.FavIndex = new List<int>();
      this.OldFavIndex = new List<int>();
      this.Name = "";
      this.ShortName = "";
    }

    /// <summary>
    /// Constructor for existing TV channel
    /// </summary>
    public ChannelInfo(SignalSource source, long index, int oldProgNr, string name) : this()
    {
      this.SignalSource = source;
      this.RecordIndex = index;
      this.RecordOrder = (int)index;
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

    public static string GetUid(SignalSource signalSource, decimal freqInMhz, int onid, int tsid, int sid, string channelOrTransponder)
    {
      if ((signalSource & SignalSource.Analog) != 0)
        return "A-0-" + (int)(freqInMhz * 20) + "-0";
      if ((signalSource & SignalSource.MaskAntennaCableSat) == SignalSource.Sat)
        return "S" + /*this.SatPosition + */ "-" + onid + "-" + tsid + "-" + sid;
      if ((signalSource & SignalSource.MaskAntennaCableSat) == SignalSource.Antenna || (signalSource & SignalSource.MaskAntennaCableSat) == SignalSource.Cable)
      {
        // ChannelOrTransponder is needed for DVB-T where the same ONID+TSID+SID can be received from 2 different radio transmitters, but on different frequencies/channels
        if (string.IsNullOrEmpty(channelOrTransponder))
          channelOrTransponder = ((signalSource & SignalSource.Antenna) != 0 ? LookupData.Instance.GetDvbtTransponder(freqInMhz) : LookupData.Instance.GetDvbcTransponder(freqInMhz)).ToString();
        return "C-" + onid + "-" + tsid + "-" + sid + "-" + channelOrTransponder;
      }
      
      return onid + "-" + tsid + "-" + sid;
    }

    /// <summary>
    /// The Uid is the preferred way of matching channels between the current channel list and a reference list.
    /// The basic format of this string was taken from a command line tool "TllSort" for LG TVs but then expanded beyond that
    /// in order to support the various file formats and the data provided in those.
    /// </summary>
    public string Uid
    {
      get => this.uid ??= GetUid(this.SignalSource, this.FreqInMhz, this.OriginalNetworkId, this.TransportStreamId, this.ServiceId, this.ChannelOrTransponder);
      set => this.uid = value;
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

    public string ServiceTypeName
    {
      get { return this.serviceTypeName ?? (this.serviceTypeName = LookupData.Instance.GetServiceTypeDescription(this.ServiceType)); }
      set { this.serviceTypeName = value; }
    }
    #endregion

    #region ParseFavString()
    public static Favorites ParseFavString(string value)
    {
      Favorites favMask = 0;
      foreach (Favorites fav in Enum.GetValues(typeof (Favorites)))
      {
        var favChar = fav.ToString()[0];
        if (value.IndexOf(favChar) >= 0)
          favMask |= fav;
      }
      return favMask;
    }
    #endregion

    #region AddDebug()
    public ChannelInfo AddDebug(byte val)
    {
      if (this.Debug == null)
        this.Debug = val.ToString("x2");
      else
        this.Debug += " " + val.ToString("x2");
      return this;
    }

    public ChannelInfo AddDebug(ushort val)
    {
      if (this.Debug == null)
        this.Debug = val.ToString("x4");
      else
        this.Debug += " " + val.ToString("x4");
      return this;
    }

    public ChannelInfo AddDebug(uint val)
    {
      if (this.Debug == null)
        this.Debug = val.ToString("x8");
      else
        this.Debug += " " + val.ToString("x8");
      return this;
    }

    public ChannelInfo AddDebug(byte[] data, int offset, int len)
    {
      for (int i = 0; i < len; i++)
        this.AddDebug(data[offset + i]);
      return this;
    }

    public ChannelInfo AddDebug(string val)
    {
      if (this.Debug == null)
        this.Debug = val;
      else
        this.Debug += " " + val;
      return this;
    }
    #endregion

    #region UpdateRawData()
    /// <summary>
    /// called during the saving procedure to update the external channel list data with the changes made by the user
    /// </summary>
    public virtual void UpdateRawData()
    {
    }
    #endregion

    #region ChangeEncoding()
    /// <summary>
    /// for file formats that allow characters in local code pages, this method should re-parse the raw data bytes for the given encoding
    /// </summary>
    /// <param name="encoding"></param>
    public virtual void ChangeEncoding(System.Text.Encoding encoding)
    {
    }
    #endregion

    #region GetPosition(), SetPosition(), ChangePosition()

    /// <summary>
    /// Gets the new channel number in the main channel list (index=0) or the various favorite lists (1..x)
    /// </summary>
    public int GetPosition(int subListIndex) => this.GetNewOrOldPosition(subListIndex, this.NewProgramNr, this.FavIndex);

    /// <summary>
    /// Gets the original channel number in the main channel list (index=0) or the various favorite lists (1..x)
    /// </summary>
    public int GetOldPosition(int subListIndex) => this.GetNewOrOldPosition(subListIndex, this.OldProgramNr, this.OldFavIndex);

    private int GetNewOrOldPosition(int subListIndex, int progNr, List<int> fav)
    {
      if (subListIndex < 0)
        return -1;
      if (subListIndex == 0)
        return progNr;
      if (this.FavMode == FavoritesMode.None)
        return -1;
      if (this.FavMode == FavoritesMode.Flags)
        return subListIndex > MaxFavListsWithFlags ? -1 : ((uint)this.Favorites & (1 << (subListIndex - 1))) == 0 ? 0 : progNr;
      if (subListIndex - 1 >= fav.Count)
        return -1;
      return fav[subListIndex - 1];
    }


    /// <summary>
    /// Sets the new channel number in the main channel list (index=0) or the various favorite lists (1..x)
    /// </summary>
    public void SetPosition(int subListIndex, int newPos) => this.SetNewOrOldPosition(subListIndex, newPos, n => this.NewProgramNr=n, this.FavIndex, f => this.Favorites = f);

    /// <summary>
    /// Sets the original channel number in the main channel list (index=0) or the various favorite lists (1..x)
    /// </summary>
    public void SetOldPosition(int subListIndex, int oldPos) => this.SetNewOrOldPosition(subListIndex, oldPos, n => this.OldProgramNr = n, this.OldFavIndex, null);

    private void SetNewOrOldPosition(int subListIndex, int newPos, Action<int> setMainPosition, List<int> fav, Action<Favorites> setFavFlags)
    {
      if (subListIndex < 0)
        return;
      if (subListIndex == 0)
        setMainPosition(newPos);
      else if (this.FavMode != FavoritesMode.None)
      {
        if (this.FavMode != FavoritesMode.Flags)
        {
          for (int i = fav.Count; i < subListIndex; i++)
            fav.Add(-1);
          fav[subListIndex - 1] = newPos;
        }

        if (setFavFlags != null && subListIndex <= MaxFavListsWithFlags)
        {
          int mask = 1 << (subListIndex - 1);
          if (newPos == -1)
            this.Favorites &= (Favorites) ~mask;
          else
            this.Favorites |= (Favorites) mask;
        }
      }
    }


    /// <summary>
    /// Internal helper method to adjust the main or favorite program number by a delta value
    /// </summary>
    internal void ChangePosition(int subListIndex, int delta)
    {
      if (subListIndex == 0)
        this.NewProgramNr += delta;
      else if (this.FavMode != FavoritesMode.None)
      {
        for (int i = this.FavIndex.Count; i < subListIndex; i++)
          this.FavIndex.Add(-1);
        this.FavIndex[subListIndex - 1] += delta;
      }
    }
    #endregion

    public void ResetFavorites()
    {
      this.FavIndex.Clear();
      this.Favorites = 0;
    }
  }
}
