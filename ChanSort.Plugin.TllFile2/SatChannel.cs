using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  class SatChannel : TllChannelBase
  {
    private const string _SatConfigIndex = "offSatelliteNr";
    private const string _TransponderIndex = "offTransponderIndex";

    public bool InUse { get; private set; }

    public SatChannel(int order, int slot, DataMapping data, DataRoot dataRoot) : base(data)
    {
      this.InUse = data.GetWord(_SatConfigIndex) != 0xFFFF;
      if (!InUse)
        return;

      this.InitCommonData(slot, SignalSource.DvbS, data);
      this.InitDvbData(data);

      int transponderIndex = data.GetWord(_TransponderIndex);
      Transponder transponder = dataRoot.Transponder.TryGet(transponderIndex);
      Satellite sat = transponder.Satellite;

      this.Satellite = sat.Name;
      this.SatPosition = sat.OrbitalPosition;
      this.RecordOrder = order;
      this.TransportStreamId = transponder.TransportStreamId;
      this.OriginalNetworkId = transponder.OriginalNetworkId;
      this.SymbolRate = transponder.SymbolRate;
      this.Polarity = transponder.Polarity;
      this.FreqInMhz = transponder.FrequencyInMhz;
    }

    public override void UpdateRawData()
    {
      base.UpdateRawData();

//      bool deleted = this.NewProgramNr == 0;
//      if (deleted)
 //       mapping.SetWord(_SatConfigIndex, 0xFFFF);
    }
  }
}
