using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  public class TllChannelBase : ChannelInfo
  {
    // common
    protected const string _ProgramNr = "offProgramNr";
    protected const string _ProgramNr2 = "offProgramNr2"; // not for DVB-S
    protected const string _ProgramNrPreset = "offProgramNrPreset";
    protected const string _Name = "offName";
    protected const string _NameLength = "offNameLength";
    protected const string _Favorites = "offFavorites";   // not for DVB-S (which only uses Favorite2)
   
    protected const string _Deleted = "Deleted";
    protected const string _Favorites2 = "offFavorites2";
    protected const string _Encrypted = "Encrypted";

    protected const string _Lock = "Lock";
    protected const string _Skip = "Skip";
    protected const string _Hide = "Hide";
    protected const string _Moved = "ProgNrCustomized";

    // DVB
    protected const string _ServiceId = "offServiceId";
    protected const string _VideoPid = "offVideoPid";
    protected const string _AudioPid = "offAudioPid";
    protected const string _OriginalNetworkId = "offOriginalNetworkId";
    protected const string _TransportStreamId = "offTransportStreamId";
    protected const string _ServiceType = "offServiceType";

    protected readonly DataMapping mapping;
    protected readonly byte[] rawData;
    internal int baseOffset;

    protected TllChannelBase(DataMapping data)
    {
      this.mapping = data;
      this.rawData = data.Data;
      this.baseOffset = data.BaseOffset;
    }

    #region InitCommonData()
    protected void InitCommonData(int slot, SignalSource signalSource, DataMapping data)
    {
      this.RecordIndex = slot;
      var nr = data.GetWord(_ProgramNr);
      this.SignalSource = signalSource | ((nr & 0x4000) == 0 ? SignalSource.Tv : SignalSource.Radio);
      this.OldProgramNr = (nr & 0x3FFF);

      this.ParseNames();

      this.Favorites = (Favorites)((data.GetByte(_Favorites2) & 0x3C) >> 2);
      this.Lock = data.GetFlag(_Lock, false);
      this.Skip = data.GetFlag(_Skip, false);
      this.Hidden = data.GetFlag(_Hide, false);
      this.Encrypted = data.GetFlag(_Encrypted, false);
      this.IsDeleted = data.GetFlag(_Deleted, false);
    }
    #endregion

    #region InitDvbData()
    protected void InitDvbData(DataMapping data)
    {
      this.ServiceId = data.GetWord(_ServiceId);
      //this.PcrPid = data.GetWord(_PcrPid);
      this.VideoPid = data.GetWord(_VideoPid);
      this.AudioPid = data.GetWord(_AudioPid);
      this.OriginalNetworkId = data.GetWord(_OriginalNetworkId);
      this.TransportStreamId = data.GetWord(_TransportStreamId);
      this.ServiceType = data.GetByte(_ServiceType);
      this.ProgramNrPreset = data.GetWord(_ProgramNrPreset);
    }
    #endregion

    #region ParseNames()
    private void ParseNames()
    {
      mapping.SetDataPtr(this.rawData, this.baseOffset);
      DvbStringDecoder dec = new DvbStringDecoder(mapping.DefaultEncoding);
      string longName, shortName;
      dec.GetChannelNames(this.rawData, this.baseOffset + mapping.GetOffsets(_Name)[0], mapping.GetByte(_NameLength), 
        out longName, out shortName);
      this.Name = longName;
      this.ShortName = shortName;      
    }
    #endregion

    #region UpdateRawData()
    public override void UpdateRawData()
    {
      mapping.SetDataPtr(this.rawData, this.baseOffset);
      int progNr = this.NewProgramNr == -1 ? 0 : this.NewProgramNr;
      mapping.SetWord(_ProgramNr, progNr | ((this.SignalSource & SignalSource.Radio) != 0 ? 0x4000 : 0));
      mapping.SetWord(_ProgramNr2, (mapping.GetWord(_ProgramNr2) & 0x0003) | (progNr << 2));
      mapping.SetWord(_ProgramNrPreset, 0);
      if (this.IsNameModified)
      {
        mapping.SetString(_Name, this.Name, 40);
        mapping.SetByte(_NameLength, this.Name.Length);
        this.IsNameModified = false;
      }
      mapping.SetByte(_Favorites2, (mapping.GetByte(_Favorites2)) & 0xC3 | ((byte) this.Favorites << 2));
      mapping.SetByte(_Favorites, (mapping.GetByte(_Favorites) & 0xF0) | (byte)this.Favorites);
      mapping.SetFlag(_Skip, this.Skip);
      mapping.SetFlag(_Lock, this.Lock);
      mapping.SetFlag(_Hide, this.Hidden);
      if (this.IsDeleted)
      {
        mapping.SetFlag(_Deleted, true);
        mapping.SetByte("off" + _Moved, 0); //skip,lock,hide,moved
      }
      else if ((this.SignalSource&SignalSource.Digital) != 0)
      {
        mapping.SetFlag(_Moved, true);
      }

      this.OldProgramNr = this.NewProgramNr;
    }
    #endregion

    #region ChangeEncoding()
    public override void ChangeEncoding(System.Text.Encoding encoding)
    {
      this.mapping.DefaultEncoding = encoding;
      this.ParseNames();
    }
    #endregion

    internal byte[] RawDataBuffer => this.rawData;
  }
}
