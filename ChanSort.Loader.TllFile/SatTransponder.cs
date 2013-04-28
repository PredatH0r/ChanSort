using System.Globalization;
using ChanSort.Api;

namespace ChanSort.Loader.LG
{
  internal class SatTransponder
  {
    private const string _Frequency = "offFrequency";
    private const string _OriginalNetworkId = "offOriginalNetworkId";
    private const string _TransportStreamId = "offTransportStreamId";
    private const string _SymbolRate = "offSymbolRate";
    private const string _SatIndex = "offSatIndex";

    private readonly DataMapping mapping;
    private readonly byte[] data;
    private readonly int offset;
    private int symbolRate;

    public SatTransponder(DataMapping mapping)
    {
      this.mapping = mapping;
      this.data = mapping.Data;
      this.offset = mapping.BaseOffset;

      this.Frequency = mapping.GetWord(_Frequency);
      this.OriginalNetworkId = mapping.GetWord(_OriginalNetworkId);
      this.TransportStreamId = mapping.GetWord(_TransportStreamId);
      this.symbolRate = mapping.GetWord(_SymbolRate);
      string strFactor = mapping.Settings.GetString("symbolRateFactor");
      decimal factor;
      if (!string.IsNullOrEmpty(strFactor) && decimal.TryParse(strFactor, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out factor))
        this.symbolRate = (int)(this.symbolRate * factor);
      this.SatIndex = mapping.GetByte(_SatIndex);
    }

    public int Frequency { get; private set; }
    public int OriginalNetworkId { get; private set; }
    public int TransportStreamId { get; private set; }
    public int SatIndex { get; private set; }

    public int SymbolRate
    {
      get { return symbolRate; }
      set
      {
        mapping.SetDataPtr(this.data, this.offset);
        mapping.SetWord(_SymbolRate, value);
        this.symbolRate = value;
      }
    }
  }
}
