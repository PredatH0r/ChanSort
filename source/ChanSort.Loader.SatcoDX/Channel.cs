using System;
using System.Globalization;
using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.SatcoDX
{
  internal class Channel : ChannelInfo
  {
    public int FileOffset { get; }
    public int Length { get; }

    #region ctor()

    internal Channel(int pos, string line, byte[] data, int start, int length, DvbStringDecoder decoder)
    {
      this.FileOffset = start;
      this.Length = length;
      this.RecordIndex = pos;
      this.RecordOrder = this.OldProgramNr = pos + 1;

      if (!line.StartsWith("SATCODX"))
        throw new FileLoadException("Only SAT channels are supported");
      if (line.Length < 106)
        throw new FileLoadException("Unrecognized channel format");

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

      // 43-50 + (115-126 or 115-131): channel name
      byte[] nameBytes = new byte[8+17];
      var nameLen2 = Math.Min(length - 115, 17); // version 103 has 12 extra bytes for channel name, version 105 has 17
      Array.Copy(data, start + 43, nameBytes, 0, 8);
      Array.Copy(data, start + 115, nameBytes, 8, nameLen2);
      decoder.GetChannelNames(nameBytes,0, nameBytes.Length, out var longName, out var shortName);
      this.Name = longName.TrimEnd();
      this.ShortName = shortName.TrimEnd();

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
  }
}