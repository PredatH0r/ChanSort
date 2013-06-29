using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  internal class DbChannel : ChannelInfo
  {
    internal int Bits;

    #region ctor()
    internal DbChannel(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot)
    {
      this.RecordIndex = r.GetInt32(field["rowid"]);
      this.RecordOrder = r.GetInt32(field["major_channel"]);
      this.OldProgramNr = r.GetInt32(field["major_channel"]);
      int ntype = r.GetInt32(field["ntype"]);
      if (ntype == 1)
      {
        this.SignalSource |= SignalSource.DvbS;
        if (r.GetInt32(field["ya_svcid"]) >= 0)
          this.SignalSource |= SignalSource.Freesat;
      }
      else if (ntype == 2)
        this.SignalSource |= SignalSource.DvbT;
      else if (ntype == 3)
        this.SignalSource |= SignalSource.DvbC;
      else if (ntype == 10)
        this.SignalSource |= SignalSource.AnalogT | SignalSource.Tv;
      else if (ntype == 14)
        this.SignalSource |= SignalSource.AnalogC | SignalSource.Tv;

      byte[] buffer = new byte[1000];
      var len = r.GetBytes(field["delivery"], 0, buffer, 0, 1000);
      this.AddDebug(buffer, 0, (int) len);

      this.Skip = r.GetInt32(field["skip"]) != 0;
      this.Encrypted = r.GetInt32(field["free_CA_mode"]) != 0;
      this.Lock = r.GetInt32(field["child_lock"]) != 0;
      this.ParseFavorites(r, field);

      if (ntype == 10 || ntype == 14)
        this.ReadAnalogData(r, field);
      else
        this.ReadDvbData(r, field, dataRoot, buffer);
    }

    #endregion

    #region ParseFavorites
    private void ParseFavorites(SQLiteDataReader r, IDictionary<string, int> field)
    {
      for (int i = 0; i < 4; i++)
      {
        int favIndex = r.GetInt32(field["profile" + (i + 1) + "index"]);
        if (favIndex > 0)
          this.Favorites |= (Favorites)(1 << i);
      }
    }
    #endregion

    private void ReadAnalogData(SQLiteDataReader r, IDictionary<string, int> field)
    {
      this.Name = r.GetString(field["sname"]);
      this.FreqInMhz = r.IsDBNull(field["freq"]) ? 0 : (decimal)r.GetInt32(field["freq"]) / 1000;
      this.ChannelOrTransponder = Tools.GetAnalogChannelNumber((int)this.FreqInMhz);
    }

    #region ReadDvbData()
    protected void ReadDvbData(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, byte[] delivery)
    {
      string longName, shortName;
      this.GetChannelNames(r.GetString(field["sname"]), out longName, out shortName);
      this.Name = longName;
      this.ShortName = shortName;

      int stype = r.GetInt32(field["stype"]);
      this.SignalSource |= LookupData.Instance.IsRadioOrTv(stype);
      this.ServiceType = stype;

      int freq = r.GetInt32(field["freq"]);
      if ((this.SignalSource & SignalSource.Sat) != 0)
      {
// ReSharper disable PossibleLossOfFraction
        this.FreqInMhz = freq/10;
// ReSharper restore PossibleLossOfFraction
        int satId = r.GetInt32(field["physical_ch"]) >> 12;
        var sat = dataRoot.Satellites.TryGet(satId);
        if (sat != null)
        {
          this.Satellite = sat.Name;
          this.SatPosition = sat.OrbitalPosition;
        }
        if (delivery.Length >= 7)
        {
          this.SymbolRate = (delivery[5] >> 4)*10000 + (delivery[5] & 0x0F)*1000 +
                            (delivery[6] >> 4)*100 + (delivery[6] & 0x0F)*10;
        }
      }
      else
      {
        freq /= 1000;
        this.FreqInMhz = freq;
        this.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(freq).ToString();
        this.Satellite = (this.SignalSource & SignalSource.Antenna) != 0 ? "DVB-T" : "DVB-C";
      }

      this.OriginalNetworkId = r.GetInt32(field["onid"]);
      this.TransportStreamId = r.GetInt32(field["tsid"]);
      this.ServiceId = r.GetInt32(field["sid"]);
    }
    #endregion

    #region GetChannelNames()
    private void GetChannelNames(string name, out string longName, out string shortName)
    {
      StringBuilder sbLong = new StringBuilder();
      StringBuilder sbShort = new StringBuilder();

#if false
      //Encoding encoding = Encoding.Default;
      if (name.Length > 0 && name[0] < 0x20)
      {
        if (name.Length >= 3 && name[0] == 0x10)
          encoding = DvbStringDecoder.GetEncoding(name[0]);
      }
#endif
      bool inShort = false;
      foreach (char c in name)
      {
        if (c < 0x20)
          continue;
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

    #region UpdateRawData()
    public override void UpdateRawData()
    {
      
    }
    #endregion
  }
}
