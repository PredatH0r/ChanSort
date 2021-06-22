using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  class SatChannel : TllChannelBase
  {
    private const string _SatConfigIndex = "offSatelliteNr";
    private const string _TransponderIndex = "offTransponderIndex";

    public bool InUse { get; }

    public SatChannel(int order, int slot, DataMapping data, DataRoot dataRoot) : base(data)
    {
      this.InUse = data.GetWord(_SatConfigIndex) != 0xFFFF;
      if (!InUse)
        return;

      this.InitCommonData(slot, SignalSource.DvbS, data);
      this.InitDvbData(data);

      int transponderIndex = data.GetWord(_TransponderIndex);
      var transponder = dataRoot.Transponder.TryGet(transponderIndex);
      var sat = transponder.Satellite;

      this.Transponder = transponder;
      this.Satellite = sat.Name;
      this.SatPosition = sat.OrbitalPosition;
      this.RecordOrder = order;
      this.TransportStreamId = transponder.TransportStreamId;
      this.OriginalNetworkId = transponder.OriginalNetworkId;
      this.SymbolRate = transponder.SymbolRate;
      this.Polarity = transponder.Polarity;
      this.FreqInMhz = transponder.FrequencyInMhz;
    }
  }
}
