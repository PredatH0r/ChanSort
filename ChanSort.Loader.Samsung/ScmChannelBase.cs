using System;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung
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
    private readonly bool sortedFavorites;

    protected readonly DataMapping mapping;
    protected readonly byte[] rawData;
    internal readonly int baseOffset;

    internal bool InUse { get; set; }

    protected ScmChannelBase(DataMapping data, bool sortedFavorites)
    {
      this.mapping = data;
      this.rawData = data.Data;
      this.baseOffset = data.BaseOffset;
      this.mapping.DefaultEncoding = Utf16BigEndian;
      this.sortedFavorites = sortedFavorites;
    }

    #region InitCommonData()
    protected void InitCommonData(int slot, SignalSource signalSource, DataMapping data)
    {
      this.InUse = data.GetFlag(_InUse);
      this.RecordIndex = slot;
      this.RecordOrder = slot;
      this.SignalSource = signalSource;
      this.OldProgramNr = (short)data.GetWord(_ProgramNr);
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

      // series D,E,F
      byte fav = 0;
      byte mask = 0x01;
      int favIndex = 0;
      foreach (int off in offsets)
      {
        int favValue = BitConverter.ToInt32(this.rawData, baseOffset + off);
        if (sortedFavorites && favValue != -1 || !sortedFavorites && favValue != 0)
          fav |= mask;
        if (sortedFavorites)
          this.FavIndex[favIndex] = favValue;
        mask <<= 1;
        ++favIndex;
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

      this.SignalSource |= LookupData.Instance.IsRadioOrTv(this.ServiceType);
    }
    #endregion

    #region UpdateRawData()
    public override void UpdateRawData()
    {
      mapping.SetDataPtr(this.rawData, this.baseOffset);
      mapping.SetFlag(_InUse, this.InUse);
      if (this.NewProgramNr >= 0)
        mapping.SetWord(_ProgramNr, this.NewProgramNr);
      if (this.IsNameModified)
      {
        int bytes = mapping.SetString(_Name, this.Name, mapping.Settings.GetInt("lenName"));
        mapping.SetByte(_NameLength, bytes);
        this.IsNameModified = false;
      }
      this.UpdateRawFavorites();
      mapping.SetFlag(_Lock, this.Lock);
      if (this.NewProgramNr == -1)
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

      // series D,E,F
      byte fav = (byte)this.Favorites;
      byte mask = 0x01;
      int favIndex = 0;
      foreach (int off in offsets)
      {
        int favValue;
        if (this.sortedFavorites)
          favValue = (fav & mask) != 0 ? this.FavIndex[favIndex] : -1;
        else
          favValue = (fav & mask) != 0 ? 1 : 0;
        Array.Copy(BitConverter.GetBytes(favValue), 0, this.rawData, baseOffset + off, 4);
        mask <<= 1;
        ++favIndex;
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
