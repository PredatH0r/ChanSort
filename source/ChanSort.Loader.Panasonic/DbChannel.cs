using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  internal class DbChannel : ChannelInfo
  {
    internal byte[] RawName;
    internal bool NonAscii;
    internal bool ValidUtf8 = true;

    internal int InternalProviderFlag2;

    #region ctor(IDataReader, ...)
    internal DbChannel(IDataReader r, IDictionary<string, int> field, DataRoot dataRoot, Encoding encoding)
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
      int len = 0;
      if (!r.IsDBNull(field["delivery"]))
      {
        len = (int)r.GetBytes(field["delivery"], 0, buffer, 0, 1000);
        this.AddDebug(buffer, 0, (int) len);
      }

      this.Skip = r.GetInt32(field["skip"]) != 0;
      this.Encrypted = r.GetInt32(field["free_CA_mode"]) != 0;
      this.Lock = r.GetInt32(field["child_lock"]) != 0;
      this.ParseFavorites(r, field);
      this.ReadNamesWithEncodingDetection(r, field, encoding);

      if (ntype == 10 || ntype == 14)
        this.ReadAnalogData(r, field);
      else
        this.ReadDvbData(r, field, dataRoot, buffer, len);
    }

    #endregion

    #region ctor(SignalSource, ...)
    public DbChannel(SignalSource signalSource, long id, int progNr, string name) : base(signalSource, id, progNr, name)
    {
    }
    #endregion

    #region ParseFavorites
    private void ParseFavorites(IDataReader r, IDictionary<string, int> field)
    {
      for (int i = 0; i < 4; i++)
      {
        int favIndex = r.GetInt32(field["profile" + (i + 1) + "index"]);
        if (favIndex > 0)
        {
          this.Favorites |= (Favorites) (1 << i);
          this.SetOldPosition(i+1, favIndex);
        }
      }
    }
    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(IDataReader r, IDictionary<string, int> field)
    {
      this.FreqInMhz = r.IsDBNull(field["freq"]) ? 0 : (decimal)r.GetInt32(field["freq"]) / 1000;
      this.ChannelOrTransponder = Tools.GetAnalogChannelNumber((int)this.FreqInMhz);
    }
    #endregion

    #region ReadDvbData()
    protected void ReadDvbData(IDataReader r, IDictionary<string, int> field, DataRoot dataRoot, byte[] delivery, int deliveryLength)
    {
      int stype = r.GetInt32(field["stype"]);
      this.SignalSource |= LookupData.Instance.IsRadioTvOrData(stype);
      this.ServiceType = stype;

      int freq = r.IsDBNull(field["freq"]) ? 0 : r.GetInt32(field["freq"]);
      if ((this.SignalSource & SignalSource.Sat) != 0)
      {
// ReSharper disable PossibleLossOfFraction
        this.FreqInMhz = freq/10;
// ReSharper restore PossibleLossOfFraction

        if (deliveryLength >= 12)
        {
          // Bytes 4-6 or 5-7 contain hex-encoded decimal digits for symbol rate. Bytes 10 and 11 are the sat orbital position.
          // The byte-order varies between files and neither the endian-ness of the file header nor any other field gives a reliable clue which order is used.
          // So I use bytes 0 and 3 as a heuristic to check for 0x01

          // 01 14 92 99 00 21 99 90 02 31 01 92 00 00 00
          var bigEndianSymbolRate = (delivery[5] >> 4) * 10000 + (delivery[5] & 0x0F) * 1000 + // 21 99 90 => 21999
                                    (delivery[6] >> 4) * 100 + (delivery[6] & 0x0F) * 10 + (delivery[7] >> 4);
          var bigEndianSatPosition = (delivery[10] >> 4) * 1000 + (delivery[10] & 0x0F) * 100 + (delivery[11] >> 4) * 10 + (delivery[11] & 0x0F); // 01 92 => 192

          // 50 94 14 01 90 99 21 00 22 31 92 01 00 00 00 00 00 
          // 04 54 10 01 10 50 27 00 04 09 30 01 00 00 00
          var littleEndianSymbolRate = (delivery[6] >> 4) * 10000 + (delivery[6] & 0x0F) * 1000 + // 21 99 90 => 21999
                                       (delivery[5] >> 4) * 100 + (delivery[5] & 0x0F) * 10 + (delivery[4] >> 4);
          var littleEndianSatPosition = (delivery[11] >> 4) * 1000 + (delivery[11] & 0x0F) * 100 + (delivery[10] >> 4) * 10 + (delivery[10] & 0x0F); // 92 01 => 192


          bool useBigEndian = delivery[0] == 1 ? delivery[3] != 1 || bigEndianSymbolRate >= 4000 && bigEndianSymbolRate <= 60000 && (bigEndianSymbolRate + 5) % 100 <= 10 : false;
          if (useBigEndian)
          {
            this.SymbolRate = bigEndianSymbolRate;
            this.SatPosition = ((decimal)bigEndianSatPosition / 10).ToString("n1");
          }
          else
          {
            this.SymbolRate = littleEndianSymbolRate;
            this.SatPosition = ((decimal)littleEndianSatPosition / 10).ToString("n1");
          }

          this.Satellite = this.SatPosition;
        }
        else
        {
          int satId = r.GetInt32(field["physical_ch"]) >> 12;
          var sat = dataRoot.Satellites.TryGet(satId);
          if (sat != null)
          {
            this.Satellite = sat.Name;
            this.SatPosition = sat.OrbitalPosition;
          }
        }

        this.Source = "DVB-S";
      }
      else
      {
        freq /= 1000;
        this.FreqInMhz = freq;
        this.ChannelOrTransponder = (this.SignalSource & SignalSource.Antenna) != 0 ? 
          LookupData.Instance.GetDvbtTransponder(freq).ToString() : 
          LookupData.Instance.GetDvbcTransponder(freq).ToString();
        this.Source = (this.SignalSource & SignalSource.Antenna) != 0 ? "DVB-T" : "DVB-C";
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
    private void ReadNamesWithEncodingDetection(IDataReader r, IDictionary<string, int> field, Encoding encoding)
    {
      byte[] buffer = new byte[100];
      int len = (int)r.GetBytes(field["sname"], 0, buffer, 0, buffer.Length);
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

      int len = Array.IndexOf<byte>(this.RawName, 0, 0, this.RawName.Length);
      if (len < 0)
        len = this.RawName.Length;
      if (len == 0)
        return;

      if (!GetRecommendedEncoding(ref encoding, out var startOffset, out var bytesPerChar)) 
        return;

      // single byte code pages might have UTF-8 code mixed in, so we have to parse it manually
      StringBuilder sb = new StringBuilder();
      this.NonAscii = false;
      this.ValidUtf8 = true;
      for (int i = startOffset; i < len; i+=bytesPerChar)
      {
        byte c = this.RawName[i];
        byte c2 = i + 1 < len ? this.RawName[i + 1] : (byte)0;
        byte c3 = i + 2 < len ? this.RawName[i + 2] : (byte)0;
        byte c4 = i + 4 < len ? this.RawName[i + 3] : (byte)0;
        if (c >= 0x80)
          NonAscii = true;

        if (c < 0x80)
          sb.Append((char) c);
        else if (c < 0xA0)
        {
          ValidUtf8 = false;
          sb.Append((char) c);
        }
        else if (bytesPerChar == 1)
        {
          if (c >= 0xC0 && c <= 0xDF && c2 >= 0x80 && c2 <= 0xBF) // 2 byte UTF-8
          {
            sb.Append((char)(((c & 0x1F) << 6) | (c2 & 0x3F)));
            ++i;
          }
          else if (c >= 0xE0 && c <= 0xEF && (c2 & 0xC0) == 0x80 && (c3 & 0xC0) == 0x80) // 3 byte UTF-8
          {
            sb.Append((char)(((c & 0x0F) << 12) | ((c2 & 0x3F) << 6) | (c3 & 0x3F)));
            i += 2;
          }
          else if (c >= 0xF0 && c <= 0xF7 && (c2 & 0xC0) == 0x80 && (c3 & 0xC0) == 0x80 && (c4 & 0xC0) == 0x80) // 4 byte UTF-8
          {
            sb.Append((char)(((c & 0x07) << 18) | ((c2 & 0x3F) << 12) | ((c3 & 0x3F) << 6) | (c4 & 0x3F)));
            i += 3;
          }
          else
          {
            ValidUtf8 = false;
            sb.Append(encoding.GetString(this.RawName, i, bytesPerChar));
          }
        }
        else
        {
          ValidUtf8 = false;
          sb.Append(encoding.GetString(this.RawName, i, bytesPerChar));
        }
      }

      this.GetChannelNames(sb.ToString(), out var longName, out var shortName);
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

    #region UpdateRawData()
    public void UpdateRawData(bool explicitUtf8, bool implicitUtf8)
    {
      if (IsNameModified)
      {
        var utf8 = Encoding.UTF8.GetBytes(this.Name);
        if (implicitUtf8)
          this.RawName = utf8;
        else if (explicitUtf8)
        {
          this.RawName = new byte[utf8.Length + 1];
          this.RawName[0] = 0x15; // DVB encoding ID for UTF8
          Array.Copy(utf8, 0, this.RawName, 1, utf8.Length);
        }
      }
    }
    #endregion
  }
}
