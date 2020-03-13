using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  internal class DbChannel : ChannelInfo
  {
    internal byte[] RawName;

    #region ctor()
    internal DbChannel(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, Encoding encoding)
    {
      this.RecordIndex = r.GetInt32(field["rowid"]);
      this.RecordOrder = r.GetInt32(field["major_channel"]);
      this.OldProgramNr = r.GetInt32(field["major_channel"]);
      if (this.OldProgramNr == 1178)
      {
      }
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
      else if (ntype == 15)
        this.SignalSource |= SignalSource.SatIP;

      byte[] buffer = new byte[1000];
      var len = r.GetBytes(field["delivery"], 0, buffer, 0, 1000);
      this.AddDebug(buffer, 0, (int) len);

      this.Skip = r.GetInt32(field["skip"]) != 0;
      this.Encrypted = r.GetInt32(field["free_CA_mode"]) != 0;
      this.Lock = r.GetInt32(field["child_lock"]) != 0;
      this.ParseFavorites(r, field);
      this.ReadNamesWithEncodingDetection(r, field, encoding);

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
        {
          this.Favorites |= (Favorites) (1 << i);
          this.OldFavIndex[i] = favIndex;
        }
      }
    }
    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(SQLiteDataReader r, IDictionary<string, int> field)
    {
      this.FreqInMhz = r.IsDBNull(field["freq"]) ? 0 : (decimal)r.GetInt32(field["freq"]) / 1000;
      this.ChannelOrTransponder = Tools.GetAnalogChannelNumber((int)this.FreqInMhz);
    }
    #endregion

    #region ReadDvbData()
    protected void ReadDvbData(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, byte[] delivery)
    {
      int stype = r.GetInt32(field["stype"]);
      this.SignalSource |= LookupData.Instance.IsRadioTvOrData(stype);
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
        this.ChannelOrTransponder = (this.SignalSource & SignalSource.Antenna) != 0 ? 
          LookupData.Instance.GetDvbtTransponder(freq).ToString() : 
          LookupData.Instance.GetDvbcTransponder(freq).ToString();
        this.Satellite = (this.SignalSource & SignalSource.Antenna) != 0 ? "DVB-T" : "DVB-C";
      }

      this.OriginalNetworkId = r.GetInt32(field["onid"]);
      this.TransportStreamId = r.GetInt32(field["tsid"]);
      this.ServiceId = r.GetInt32(field["sid"]);
    }
    #endregion

    #region ReadNamesWithEncodingDetection()
    /// <summary>
    /// Character encoding is a mess here. Code pages mixed with UTF-8 and raw data
    /// </summary>
    private void ReadNamesWithEncodingDetection(SQLiteDataReader r, IDictionary<string, int> field, Encoding encoding)
    {
      byte[] buffer = new byte[100];
      int len = (int)r.GetBytes(field["sname"], 0, buffer, 0, buffer.Length);
      int end = Array.IndexOf<byte>(buffer, 0, 0, len);
      if (end >= 0)
        len = end;
      this.RawName = new byte[len];
      Array.Copy(buffer, 0, this.RawName, 0, len);
      this.ChangeEncoding(encoding);      
    }
    #endregion

    #region ChangeEncoding()
    public override void ChangeEncoding(Encoding encoding)
    {
      // the encoding of channel names is a complete mess:
      // it can be UTF-8
      // it can be as specified by the DVB-encoding with a valid code page selector byte
      // it can have a DVB-encoded code page selector, but ignores the CP and use UTF-8 regardless
      // it can be code page encoded without any clue to what the code page is
      // it can have DVB-control characters inside an UTF-8 stream

      if (RawName.Length == 0)
        return;

      int startOffset;
      int bytesPerChar;
      if (!GetRecommendedEncoding(ref encoding, out startOffset, out bytesPerChar)) 
        return;

      // single byte code pages might have UTF-8 code mixed in, so we have to parse it manually
      StringBuilder sb = new StringBuilder();
      for (int i = startOffset; i < this.RawName.Length; i+=bytesPerChar)
      {
        byte c = this.RawName[i];
        byte c2 = i + 1 < this.RawName.Length ? this.RawName[i + 1] : (byte)0;
        if (c < 0xA0)
          sb.Append((char)c);
        else if (bytesPerChar == 1 && c >= 0xC0 && c <= 0xDF && c2 >= 0x80 && c2 <= 0xBF) // 2 byte UTF-8
        {
          sb.Append((char)(((c & 0x1F) << 6) | (c2 & 0x3F)));
          ++i;
        }
        else
          sb.Append(encoding.GetString(this.RawName, i, bytesPerChar));
      }

      string longName, shortName;
      this.GetChannelNames(sb.ToString(), out longName, out shortName);
      this.Name = longName;
      this.ShortName = shortName;
    }
    #endregion

    #region GetRecommendedEncoding()
    private bool GetRecommendedEncoding(ref Encoding encoding, out int startOffset, out int bytesPerChar)
    {
      startOffset = 0;
      bytesPerChar = 1;
      if (RawName[0] < 0x10) // single byte character sets
      {
        encoding = DvbStringDecoder.GetEncoding(RawName[0]);
        startOffset = 1;
      }
      else if (RawName[0] == 0x10) // prefix for 16 bit code page ID with single byte character sets
      {
        if (RawName.Length < 3) return false;
        encoding = DvbStringDecoder.GetEncoding(0x100000 + RawName[1]*256 + RawName[2]);
        startOffset = 3;
      }
      else if (RawName[0] == 0x15) // UTF-8
      {
        encoding = Encoding.UTF8;
        startOffset = 1;
      }
      else if (RawName[0] < 0x20) // various 2-byte character sets
      {
        encoding = DvbStringDecoder.GetEncoding(RawName[0]);
        startOffset = 1;
        bytesPerChar = 2;
      }
      return true;
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
        if (c < 0x20)
          continue;
        if (c == 0x86 || c == '\uE086')
          inShort = true;
        else if (c == 0x87 || c == '\uE087')
          inShort = false;
        if (c >= 0x80 && c <= 0x9F || c>='\uE080' && c<='\uE09F')
          continue;

        if (inShort)
          sbShort.Append(c);
        sbLong.Append(c);
      }

      longName = sbLong.ToString();
      shortName = sbShort.ToString();
    }
    #endregion
  }
}
