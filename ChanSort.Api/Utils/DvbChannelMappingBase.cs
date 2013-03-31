using System.Text;

namespace ChanSort.Api
{
  public class DvbChannelMappingBase : ChannelMappingBase
  {
    protected const string offChannelTransponder = "offChannelTransponder";
    protected const string offPcrPid = "offPcrPid";
    protected const string offVideoPid = "offVideoPid";
    protected const string offAudioPid = "offAudioPid";
    protected const string offServiceId = "offServiceId";
    protected const string offOriginalNetworkId = "offOriginalNetworkId";
    protected const string offTransportStreamId = "offTransportStreamId";
    protected const string offServiceType = "offServiceType";
    protected const string offSymbolRate = "offSymbolRate";
    protected const string offEncrypted = "offEncrypted";
    protected const string maskEncrypted = "maskEncrypted";
    protected const string offShortName = "offShortName";
    protected const string offShortNameLength = "offShortNameLength";
    protected const string lenShortName = "lenShortName";


    #region ctor()
    public DvbChannelMappingBase(IniFile.Section settings, int dataLength, Encoding stringEncoding) 
      : base(settings, dataLength, stringEncoding)
    {
    }
    #endregion

    #region ChannelOrTransponder
    public virtual byte ChannelOrTransponder
    {
      get { return this.GetByte(offChannelTransponder); }
      set { this.SetByte(offChannelTransponder, value); }
    }
    #endregion

    #region ShortName
    public int ShortNameLength
    {
      get
      {
        var off = this.GetOffsets(offShortNameLength);
        if (off.Length > 0)
          return this.GetWord(off[0]);
        off = this.GetOffsets(lenShortName); // not an offset!
        return off.Length > 0 ? off[0] : 0;
      }      
    }

    public override string ShortName { get { return this.GetString(offShortName, this.ShortNameLength); } }
    #endregion

    #region PcrPid
    public virtual ushort PcrPid
    {
      get { return this.GetWord(offPcrPid); }
      set { this.SetWord(offPcrPid, value); }
    }
    #endregion

    #region VideoPid
    public virtual ushort VideoPid
    {
      get { return this.GetWord(offVideoPid); }
      set { this.SetWord(offVideoPid, value); }
    }
    #endregion

    #region AudioPid
    public virtual ushort AudioPid
    {
      get { return this.GetWord(offAudioPid); }
      set { this.SetWord(offAudioPid, value); }
    }
    #endregion

    #region ServiceId
    public virtual ushort ServiceId
    {
      get { return this.GetWord(offServiceId); }
      set { this.SetWord(offServiceId, value); }
    }
    #endregion

    #region OriginalNetworkId
    public virtual ushort OriginalNetworkId
    {
      get { return this.GetWord(offOriginalNetworkId); }
      set { this.SetWord(offOriginalNetworkId, value); }
    }
    #endregion

    #region TransportStreamId
    public virtual ushort TransportStreamId
    {
      get { return this.GetWord(offTransportStreamId); }
      set { this.SetWord(offTransportStreamId, value); }
    }
    #endregion

    #region ServiceType
    public virtual byte ServiceType
    {
      get { return this.GetByte(offServiceType); }
      set { this.SetByte(offServiceType, value); }
    }
    #endregion

    #region SymbolRate
    public virtual ushort SymbolRate
    {
      get { return this.GetWord(offSymbolRate); }
      set { this.SetWord(offSymbolRate, value); }
    }
    #endregion

    #region Encrypted
    public virtual bool Encrypted
    {
      get { return this.GetFlag(offEncrypted, maskEncrypted); }
    }
    #endregion
  }
}
