using System;
using System.Collections.Generic;
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
    private const string _IsActive = "IsActive";
    private const string _Deleted = "Deleted";
    private const string _Encrypted = "Encrypted";
    private const string _Lock = "Lock";
    private const string _Checksum = "offChecksum";

    // DVB
    private const string _Skip = "Skip";
    private const string _Hidden = "offHidden";
    private const string _HiddenAlt = "offHiddenAlt";
    private const string _ServiceId = "offServiceId";
    private const string _PcrPid = "offPcrPid";
    private const string _VideoPid = "offVideoPid";
    private const string _AudioPid = "offAudioPid";
    private const string _OriginalNetworkId = "offOriginalNetworkId";
    private const string _TransportStreamId = "offTransportStreamId";
    private const string _ServiceType = "offServiceType";
    private const string _SymbolRate = "offSymbolRate";
    private const string _ServiceProviderId = "offServiceProviderId";

    private static readonly Encoding Utf16BigEndian = new UnicodeEncoding(true, false);
    private readonly FavoritesIndexMode sortedFavorites;

    protected readonly DataMapping mapping;
    protected readonly byte[] rawData;
    internal readonly int baseOffset;

    internal bool InUse { get; set; }

    protected ScmChannelBase(DataMapping data, FavoritesIndexMode sortedFavorites)
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
      this.InUse = data.GetFlag(_InUse, true);
      this.RecordIndex = slot;
      this.RecordOrder = slot;
      this.SignalSource = signalSource;
      this.OldProgramNr = (short)data.GetWord(_ProgramNr);
      this.Name = data.GetString(_Name, data.Settings.GetInt("lenName"));
      this.Favorites = this.ParseRawFavorites();
      this.Lock = data.GetFlag(_Lock);
      int hiddenPrimary = data.GetByte(_Hidden);
      if (hiddenPrimary == 255)
        this.Hidden = data.GetByte(_HiddenAlt) != 0;
      else
        this.Hidden = hiddenPrimary != 0;
      this.Skip = data.GetFlag(_Skip);
      this.Encrypted = data.GetFlag(_Encrypted);
      this.IsDeleted = data.GetFlag(_Deleted, false) || !data.GetFlag(_IsActive, true);
      if (this.IsDeleted)
        this.OldProgramNr = -1;
      this.AddDebug(data.Data, data.BaseOffset + 25, 3);
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
        if (sortedFavorites == FavoritesIndexMode.Boolean && favValue != 0)
          fav |= mask;
        else if (sortedFavorites != FavoritesIndexMode.Boolean && favValue != -1)
          fav |= mask;
        if (sortedFavorites == FavoritesIndexMode.IndividuallySorted)
          this.FavIndex[favIndex] = favValue;
        mask <<= 1;
        ++favIndex;
      }
      return (Favorites) fav;      
    }
    #endregion


    #region InitDvbData()
    protected void InitDvbData(DataMapping data, IDictionary<int, string> providerNames)
    {
      this.ShortName = data.GetString(_ShortName, data.Settings.GetInt("lenShortName"));
      this.ServiceId = data.GetWord(_ServiceId);
      this.PcrPid = data.GetWord(_PcrPid);
      this.VideoPid = data.GetWord(_VideoPid);
      this.AudioPid = data.GetWord(_AudioPid);
      this.OriginalNetworkId = data.GetWord(_OriginalNetworkId);
      this.TransportStreamId = data.GetWord(_TransportStreamId);
      this.ServiceType = data.GetByte(_ServiceType);
      this.SymbolRate = data.GetWord(_SymbolRate);
      if (data.Settings.GetInt(_ServiceProviderId, -1) != -1)
      {
        int source = -1;
        if ((this.SignalSource & SignalSource.MaskProvider) == SignalSource.Freesat)
          source = 4;
        else if ((this.SignalSource & SignalSource.MaskProvider) == SignalSource.TivuSat)
          source = 6;
        else if ((this.SignalSource & SignalSource.Antenna) != 0)
          source = 0;
        else if ((this.SignalSource & SignalSource.Cable) != 0)
          source = 1;
        else if ((this.SignalSource & SignalSource.Sat) != 0)
          source = 3;
        int providerId = data.GetWord(_ServiceProviderId);
        this.Provider = providerNames.TryGet((source << 16) + providerId);
      }
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
      mapping.SetFlag(_Deleted, this.NewProgramNr < 0);
      mapping.SetFlag(_IsActive, this.NewProgramNr >= 0);
      mapping.SetFlag(_Skip, this.Skip);
      mapping.SetByte(this.mapping.GetByte(_Hidden) != 255 ? _Hidden : _HiddenAlt, this.Hidden ? 1 : 0);
      if (this.Encrypted != null)
        mapping.SetFlag(_Encrypted, this.Encrypted.Value);
      this.UpdateChecksum();
    }
    #endregion

    #region EraseRawData()
    internal virtual void EraseRawData()
    {
      int len = this.mapping.Settings.GetInt("offChecksum") + 1;
      Tools.MemSet(this.rawData, this.baseOffset, 0, len);      
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
        if (this.sortedFavorites == FavoritesIndexMode.Boolean) // D series
          favValue = (fav & mask) != 0 ? 1 : 0; // D series
        else if (this.sortedFavorites == FavoritesIndexMode.IndividuallySorted) // E series (and some F models with early firmware)
          favValue = (fav & mask) != 0 ? this.FavIndex[favIndex] : -1;
        else
          favValue = (fav & mask) != 0 ? this.NewProgramNr : -1; // F series (newer models/firmware), H series
          
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
