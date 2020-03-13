using System;
using System.Globalization;
using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.SilvaSchneider
{
  internal class Channels : ChannelInfo
  {
    public int FileOffset { get; }
    public int Length { get; }

    #region ctor()

    internal Channels(int pos, string line, byte[] data, int start, int length, DvbStringDecoder decoder)
    {
      this.FileOffset = start;
      this.Length = length;
      this.RecordIndex = pos;
      this.RecordOrder = this.OldProgramNr = pos + 1;

      if (!line.StartsWith("SATCODX"))
        throw new FileLoadException("Only SAT channels are supported");
      if (line.Length < 106)
        throw new FileLoadException("Unrecognized channel format");

      this.Satellite = line.Substring(10, 18);

      var type = line[28];
      this.SignalSource = SignalSource.Digital | SignalSource.Sat | (type == 'T' ? SignalSource.Tv : type == 'R' ? SignalSource.Radio : 0);
      this.ServiceType = type == 'T' ? 1 : type == 'R' ? 2 : 0; // 1=SD-TV, 2=Radio

      if (int.TryParse(line.Substring(34, 5), out var mhz))
        this.FreqInMhz = mhz;

      this.Polarity = line[39] == '1' ? 'H' : 'V';

      byte[] nameBytes = new byte[8+17];
      var nameLen2 = Math.Min(length - 115, 17); // version 103 has 12 extra bytes for channel name, version 105 has 17
      Array.Copy(data, start + 43, nameBytes, 0, 8);
      Array.Copy(data, start + 115, nameBytes, 8, nameLen2);
      decoder.GetChannelNames(nameBytes,0, nameBytes.Length, out var longName, out var shortName);
      this.Name = longName.TrimEnd();
      this.ShortName = shortName.TrimEnd();

      var spos = line.Substring(51, 4).TrimStart('0');
      this.SatPosition = spos.Substring(0, spos.Length - 1) + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + spos.Substring(spos.Length - 1);

      if (int.TryParse(line.Substring(69, 5), out var symrate))
        this.SymbolRate = symrate;

      if (int.TryParse(line.Substring(87, 5), out var sid))
        this.ServiceId = sid;

      if (int.TryParse(line.Substring(92, 5), out var onid))
        this.OriginalNetworkId = onid;

      if (int.TryParse(line.Substring(97, 5), out var tsid))
        this.TransportStreamId = tsid;
    }

    #endregion
  }
}