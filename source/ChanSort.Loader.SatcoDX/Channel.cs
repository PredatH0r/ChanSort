using System;
using System.Globalization;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.SatcoDX
{
  internal class Channel : ChannelInfo
  {
    private byte[] data;
    public int FileOffset { get; }
    public int Length { get; }

    #region ctor()

    internal Channel(int pos, string line, byte[] data, int start, int length, DvbStringDecoder decoder)
    {
      this.data = data;
      this.FileOffset = start;
      this.Length = length;
      this.RecordIndex = pos;
      this.RecordOrder = this.OldProgramNr = pos + 1;

      if (!line.StartsWith("SATCODX"))
        throw LoaderException.Fail("Only SAT channels are supported");
      if (line.Length < 106)
        throw LoaderException.Fail("Unrecognized channel format");

      // 10-27: satellite name
      this.Satellite = line.Substring(10, 18);

      // 28: channel type
      var type = line[28];
      this.SignalSource = SignalSource.Digital | SignalSource.Sat | (type == 'T' ? SignalSource.Tv : type == 'R' ? SignalSource.Radio : 0);
      this.ServiceTypeName = type == 'T' ? "TV" : type == 'R' ? "Radio" : type == 'D' ? "Data" : "Other";

      // 29-32: broadcast system

      // 33-41: frequency in kHz
      if (int.TryParse(line.Substring(33, 9), out var khz))
        this.FreqInMhz = (decimal)khz / 1000;

      // 42: polarity
      this.Polarity = line[42] == '1' ? 'H' : 'V';

      // 43-50 + 115-126 in version 103 or 115-131 in version 105: channel name
      this.ParseName(decoder);

      // 51-54: sat position
      var spos = line.Substring(51, 4).TrimStart('0');
      this.SatPosition = spos.Substring(0, spos.Length - 1) + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + spos.Substring(spos.Length - 1);

      // 69-73: symbol rate
      if (int.TryParse(line.Substring(69, 5), out var symrate))
        this.SymbolRate = symrate;

      // 74: FEC 0=-, 1=1/2, 2=2/3, 3=3/4, 5=5/6, 7=7/8

      // 75-78: vpid or ____
      // 79-82: apid or ____
      // 83-86: pcrpid or ____

      // 87-91: sid
      if (int.TryParse(line.Substring(87, 5), out var sid))
        this.ServiceId = sid;

      // 92-96: nid / onid
      if (int.TryParse(line.Substring(92, 5), out var onid))
        this.OriginalNetworkId = onid;

      // 97-101: tsid
      if (int.TryParse(line.Substring(97, 5), out var tsid))
        this.TransportStreamId = tsid;

      // 102-104: language
      // 106-107: country code
      // 108-110: language code
      // 111-114: crypto code
    }

    #endregion

    #region ParseName()

    /// <summary>
    /// SATCODX103 files can contain channel names with unspecified implicit encoding, so we support reparsing based on a user selected default code page
    /// </summary>
    /// <param name="decoder"></param>
    public void ParseName(DvbStringDecoder decoder)
    {
      var length = this.Length;
      var start = this.FileOffset;

      // 43-50 + 115-126 in version 103 or 115-131 in version 105: channel name
      byte[] nameBytes = new byte[8 + 17];
      var nameLen2 = Math.Min(length - 115, 17); // version 103 has 12 extra bytes for channel name, version 105 has 17
      Array.Copy(data, start + 43, nameBytes, 0, 8);
      Array.Copy(data, start + 115, nameBytes, 8, nameLen2);
      
      // I have seen format 103 files using only implicit CP1252 encoding for Umlauts, as well as format 105 with implicit UTF-8/explicit DVB-encoding
      var oldDefaultEncoding = decoder.DefaultEncoding;
      if (nameLen2 > 12)
        decoder.DefaultEncoding = Encoding.UTF8;
      decoder.GetChannelNames(nameBytes, 0, nameBytes.Length, out var longName, out var shortName);
      decoder.DefaultEncoding = oldDefaultEncoding;
      this.Name = longName.TrimEnd();
      this.ShortName = shortName.TrimEnd();
    }
    #endregion

    #region Export()
    public void Export(byte[] buffer, Encoding encoding)
    {
      Array.Copy(this.data, this.FileOffset, buffer, 0, this.Length + 1);
      if (!this.IsNameModified)
        return;

      // 43-50 + 115-126 in version 103 or 115-131 in version 105: channel name
      var bytes = encoding.GetBytes(this.Name);
      Tools.MemSet(buffer, 43, 32, 8);
      Tools.MemSet(buffer, 115, 32, buffer.Length - 115 -1);
      Array.Copy(bytes, 0, buffer, 43, Math.Min(bytes.Length, 8));
      if (bytes.Length > 8)
        Array.Copy(bytes, 8, buffer, 115, Math.Min(bytes.Length - 8, this.Length - 115 - 1));
    }
    #endregion
  }
}