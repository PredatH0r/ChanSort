using System.Collections.Generic;
using System.Data;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  internal class DbChannel : ChannelInfo
  {
    private const int BITS_Tv = 0x10000;
    private const int BITS_Radio = 0x20000;
    private const int BITS_FavA = 0x100000;
    private const int BITS_FavB = 0x200000;
    private const int BITS_FavC = 0x400000;
    private const int BITS_FavD = 0x800000;
    private const int BITS_Locked = 0x20000000;

    internal int Bits;

    #region ctor()
    internal DbChannel(SignalSource source, IDataReader r, IDictionary<string, int> field, 
      DataRoot dataRoot, IDictionary<string,bool> encryptionInfo)
    {
      this.SignalSource = source;
      this.RecordIndex = r.GetInt32(field["channel_handle"]);

      this.Bits = r.GetInt32(field["list_bits"]);
      bool isTv = (Bits & BITS_Tv) != 0;
      bool isRadio = (Bits & BITS_Radio) != 0;
      bool isAnalog = (source & SignalSource.Analog) != 0;
      if (isAnalog && !isTv)
      {
        this.IsDeleted = true;
        return;
      }

      if (isTv) this.SignalSource |= SignalSource.Tv;
      if (isRadio) this.SignalSource |= SignalSource.Radio;
      this.Lock = (Bits & BITS_Locked) != 0;
      this.OldProgramNr = r.GetInt32(field["channel_number"]);
      this.Favorites = this.ParseFavorites(Bits);
      
      if (isAnalog)
        this.ReadAnalogData(r, field);
      else
        this.ReadDvbData(r, field, dataRoot, encryptionInfo);
    }
    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(IDataReader r, IDictionary<string, int> field)
    {
      this.Name = r.GetString(field["channel_label"]);
      this.FreqInMhz = (decimal)r.GetInt32(field["frequency"]) / 1000000;
    }
    #endregion

    #region ReadDvbData()
    protected void ReadDvbData(IDataReader r, IDictionary<string, int> field, DataRoot dataRoot, 
      IDictionary<string, bool> encryptionInfo)
    {
      string longName, shortName;
      this.GetChannelNames(r.GetString(field["channel_label"]), out longName, out shortName);
      this.Name = longName;
      this.ShortName = shortName;
      this.RecordOrder = r.GetInt32(field["channel_order"]);
      this.FreqInMhz = (decimal)r.GetInt32(field["frequency"]) / 1000;
      int serviceType = r.GetInt32(field["dvb_service_type"]);
      this.ServiceType = serviceType;
      this.OriginalNetworkId = r.GetInt32(field["onid"]);
      this.TransportStreamId = r.GetInt32(field["tsid"]);
      this.ServiceId = r.GetInt32(field["sid"]);
      int bits = r.GetInt32(field["list_bits"]);
      this.Favorites = this.ParseFavorites(bits);
      if ((this.SignalSource & SignalSource.Sat) != 0)
      {
        int satId = r.GetInt32(field["sat_id"]);
        var sat = dataRoot.Satellites.TryGet(satId);
        if (sat != null)
        {
          this.Satellite = sat.Name;
          this.SatPosition = sat.OrbitalPosition;
          int tpId = satId * 1000000 + (int)this.FreqInMhz;
          var tp = dataRoot.Transponder.TryGet(tpId);
          if (tp != null)
          {
            this.SymbolRate = tp.SymbolRate;
          }
        }
      }
      this.Encrypted = encryptionInfo.TryGet(this.Uid);      
    }
    #endregion

    #region GetChannelNames()
    private void GetChannelNames(string name, out string longName, out string shortName)
    {
      StringBuilder sbLong = new StringBuilder();
      StringBuilder sbShort = new StringBuilder();

      bool inShort = false;
      foreach (char c in name)
      {
        if (c == 0x86)
          inShort = true;
        else if (c == 0x87)
          inShort = false;
        if (c >= 0x80 && c <= 0x9F)
          continue;

        if (inShort)
          sbShort.Append(c);
        sbLong.Append(c);
      }

      longName = sbLong.ToString();
      shortName = sbShort.ToString();
    }
    #endregion

    #region ParseFavorites()
    private Favorites ParseFavorites(int bits)
    {
      Favorites fav = 0;
      if ((bits & BITS_FavA) != 0) fav |= Favorites.A;
      if ((bits & BITS_FavB) != 0) fav |= Favorites.B;
      if ((bits & BITS_FavC) != 0) fav |= Favorites.C;
      if ((bits & BITS_FavD) != 0) fav |= Favorites.D;
      return fav;
    }
    #endregion

    #region UpdateRawData()
    public override void UpdateRawData()
    {
      Bits &= ~(BITS_FavA | BITS_FavB | BITS_FavC | BITS_FavD | BITS_Locked);
      if ((this.Favorites & Favorites.A) != 0) Bits |= BITS_FavA;
      if ((this.Favorites & Favorites.B) != 0) Bits |= BITS_FavB;
      if ((this.Favorites & Favorites.C) != 0) Bits |= BITS_FavC;
      if ((this.Favorites & Favorites.D) != 0) Bits |= BITS_FavD;
      if (this.Lock) Bits |= BITS_Locked;
    }
    #endregion
  }
}
