using System.Text;

namespace ChanSort.Api
{
  public unsafe class ChannelMappingBase : DataMapping
  {
    protected const string offInUse = "offInUse";
    protected const string maskInUse = "maskInUse";
    protected const string offProgramNr = "offProgramNr";
    protected const string offName = "offName";
    protected const string offNameLength = "offNameLength";
    protected const string lenName = "lenName";
    protected const string offSkip = "offSkip";
    protected const string maskSkip = "maskSkip";
    protected const string offLock = "offLock";
    protected const string maskLock = "maskLock";
    protected const string offLockSkipHide = "offHide";
    protected const string maskHide = "maskHide";
    protected const string offFavorites = "offFavorites";
    private const string offDeleted = "offDeleted";
    private const string maskDeleted = "maskDeleted";

    #region ctor()
    public ChannelMappingBase(IniFile.Section settings, int length, Encoding stringEncoding) : 
      base(settings, length, stringEncoding)
    {
    }
    #endregion

    #region InUse
    public virtual bool InUse
    {
      get 
      {
        var val = this.GetOffsets(offInUse);
        return val.Length == 0 || this.GetFlag(offInUse, maskInUse); 
      }
    }
    #endregion

    #region ProgramNr
    public virtual ushort ProgramNr
    {
      get { return this.GetWord(offProgramNr); }
      set { this.SetWord(offProgramNr, value); }
    }
    #endregion

    #region NameLength
    public virtual int NameLength
    {
      get
      {
        var off = this.GetOffsets(offNameLength);
        if (off.Length > 0)
          return this.GetWord(off[0]);
        return MaxNameLength;
      }
      set { this.SetByte(offNameLength, (byte) value); }
    }
    #endregion

    #region MaxNameLength
    public virtual int MaxNameLength
    {
      get
      {
        var off = this.GetOffsets(lenName); // not an offset!
        return off.Length > 0 ? off[0] : 0;
      }
    }
    #endregion

    #region Name
    public virtual string Name
    {
      get { return this.GetString(offName, this.NameLength); }
// ReSharper disable ValueParameterNotUsed
      set { }
// ReSharper restore ValueParameterNotUsed
    }
    #endregion

    #region NamePtr

    public virtual byte* NamePtr
    {
      get { return this.DataPtr + this.GetOffsets(offName)[0]; }
      set
      {
        int maxLen = this.MaxNameLength - 1;
        if (maxLen == 0)
          maxLen = this.NameLength;
        foreach (int off in this.GetOffsets(offName))
        {
          int i;
          for (i = 0; i < maxLen && value[i] != 0; i++)
            this.DataPtr[off + i] = value[i];
          for (; i <= maxLen; i++)
            this.DataPtr[off + i] = 0;
        }
      }
    }
    #endregion

    #region ShortName
    public virtual string ShortName { get; set; }
    #endregion

    #region Skip
    public virtual bool Skip
    {
      get { return this.GetFlag(offSkip, maskSkip); }
      set { this.SetFlag(offSkip, maskSkip, value); }
    }
    #endregion

    #region Lock
    public virtual bool Lock
    {
      get { return this.GetFlag(offLock, maskLock); }
      set { this.SetFlag(offLock, maskLock, value); }
    }
    #endregion

    #region Hide
    public virtual bool Hide
    {
      get { return this.GetFlag(offLockSkipHide, maskHide); }
      set { this.SetFlag(offLockSkipHide, maskHide, value); }
    }
    #endregion

    #region Favorites
    public virtual Favorites Favorites
    {
      get { return (Favorites) this.GetByte(offFavorites); }
      set { this.SetByte(offFavorites, (byte) value); }
    }
    #endregion

    #region IsDeleted
    public virtual bool IsDeleted
    {
      get { return this.GetFlag(offDeleted, maskDeleted); }
      set { this.SetFlag(offDeleted, maskDeleted, value); }
    }
    #endregion
  }

}
