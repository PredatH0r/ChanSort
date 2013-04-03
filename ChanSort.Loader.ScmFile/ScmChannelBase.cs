using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.ScmFile
{
  public class ScmChannelBase : ChannelInfo
  {
    // common
    private const string _InUse = "InUse";
    private const string _ProgramNr = "offProgramNr";
    private const string _Name = "offName";
    private const string _NameLength = "offNameLength";
    private const string _ShortName = "offShortName";
    private const string _Favorites = "offFavorites";
    private const string _Deleted = "Deleted";
    private const string _Encrypted = "Encrypted";
    private const string _Lock = "Lock";
    private const string _Checksum = "offChecksum";

    // DVB
    private const string _ServiceId = "offServiceId";
    private const string _VideoPid = "offVideoPid";
    private const string _AudioPid = "offAudioPid";
    private const string _OriginalNetworkId = "offOriginalNetworkId";
    private const string _TransportStreamId = "offTransportStreamId";
    private const string _ServiceType = "offServiceType";
    private const string _SymbolRate = "offSymbolRate";

    private static readonly Encoding Utf16BigEndian = new UnicodeEncoding(true, false);

    protected readonly DataMapping mapping;
    protected readonly byte[] rawData;
    internal readonly int baseOffset;

    internal bool InUse { get; set; }

    protected ScmChannelBase(DataMapping data)
    {
      this.mapping = data;
      this.rawData = data.Data;
      this.baseOffset = data.BaseOffset;
      this.mapping.DefaultEncoding = Utf16BigEndian;
    }

    #region InitCommonData()
    protected void InitCommonData(int slot, SignalSource signalSource, DataMapping data)
    {
      this.InUse = data.GetFlag(_InUse);
      this.RecordIndex = slot;
      this.RecordOrder = slot;
      this.SignalSource = signalSource;
      this.SignalType = SignalType.Mixed;
      this.OldProgramNr = data.GetWord(_ProgramNr);
      this.Name = data.GetString(_Name, data.Settings.GetInt("lenName"));
      this.Favorites = this.ParseRawFavorites();
      this.Lock = data.GetFlag(_Lock);
      this.Encrypted = data.GetFlag(_Encrypted);
      this.IsDeleted = data.GetFlag(_Deleted);
    }
    #endregion

    #region ParseRawFavorites()
    private Favorites ParseRawFavorites()
    {
      var offsets = mapping.GetOffsets(_Favorites);
      if (offsets.Length == 1) // series B,C
        return (Favorites) mapping.GetByte(_Favorites);

      // series D,E
      byte fav = 0;
      byte mask = 0x01;
      foreach (int off in offsets)
      {
        if ((System.BitConverter.ToInt32(this.rawData, baseOffset + off) + 1) > 1) // -1 and 0 mean "not set"
          fav |= mask;
        mask <<= 1;
      }
      return (Favorites) fav;      
    }
    #endregion


    #region InitDvbData()
    protected void InitDvbData(DataMapping data)
    {
      this.ShortName = data.GetString(_ShortName, data.Settings.GetInt("lenShortName"));
      this.ServiceId = data.GetWord(_ServiceId);
      //this.PcrPid = data.GetWord(_PcrPid);
      this.VideoPid = data.GetWord(_VideoPid);
      this.AudioPid = data.GetWord(_AudioPid);
      this.OriginalNetworkId = data.GetWord(_OriginalNetworkId);
      this.TransportStreamId = data.GetWord(_TransportStreamId);
      this.ServiceType = data.GetByte(_ServiceType);
      this.SymbolRate = data.GetWord(_SymbolRate);
    }
    #endregion

    #region GetSignalType()
    protected SignalType GetSignalType(uint programNr)
    {
      if ((programNr & 0x4000) != 0)
        return SignalType.Radio;
      return SignalType.Tv;
    }
    #endregion

    #region UpdateRawData()
    public override void UpdateRawData()
    {
      mapping.SetDataPtr(this.rawData, this.baseOffset);
      mapping.SetWord(_ProgramNr, this.NewProgramNr);
      if (this.IsNameModified)
      {
        mapping.SetString(_Name, this.Name, mapping.Settings.GetInt("lenName"));
        mapping.SetByte(_NameLength, this.Name.Length * 2);
        this.IsNameModified = false;
      }
      this.UpdateRawFavorites();
      mapping.SetFlag(_Lock, this.Lock);
      if (this.NewProgramNr == 0)
        mapping.SetFlag(_Deleted, true);

      this.UpdateChecksum();
    }
    #endregion

    #region UpdateRawFavorites()
    private void UpdateRawFavorites()
    {
      var offsets = mapping.GetOffsets(_Favorites);
      if (offsets.Length == 1) // series B,C
      {
        mapping.SetByte(_Favorites, (byte)this.Favorites & 0x0F);
        return;
      }

      // series D,E
      byte fav = (byte)this.Favorites;
      byte mask = 0x01;
      foreach (int off in offsets)
      {
        this.rawData[baseOffset + off + 0] = (byte)((fav & mask) == 0 ? 0 : 1);
        this.rawData[baseOffset + off + 1] = 0;
        this.rawData[baseOffset + off + 2] = 0;
        this.rawData[baseOffset + off + 3] = 0;
        mask <<= 1;
      }
    }
    #endregion

    #region UpdateChecksum()
    private void UpdateChecksum()
    {
      var offChecksum = this.baseOffset + this.mapping.GetOffsets(_Checksum)[0];
      byte crc = 0;
      for (int i = this.baseOffset; i < offChecksum; i++)
        crc += this.rawData[i];
      this.rawData[offChecksum] = crc;
    }
    #endregion
  }
}
