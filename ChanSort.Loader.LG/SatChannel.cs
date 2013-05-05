using ChanSort.Api;

namespace ChanSort.Loader.LG
{
  class SatChannel : TllChannelBase
  {
    private const string _SatConfigIndex = "offSatelliteNr";
    private const string _TransponderIndex = "offTransponderIndex";
    private const string _ProgramNrPreset = "offProgramNrPreset";

    public bool InUse { get; private set; }
    public int ProgramNrPreset { get; private set; }

    public SatChannel(int order, int slot, DataMapping data, DataRoot dataRoot) : base(data)
    {
      this.InUse = data.GetWord(_SatConfigIndex) != 0xFFFF;
      if (!InUse)
        return;

      this.InitCommonData(slot, SignalSource.DvbS, data);
      this.InitDvbData(data);

      this.ProgramNrPreset = data.GetWord(_ProgramNrPreset);
      int transponderIndex = data.GetWord(_TransponderIndex);
      Transponder transponder = dataRoot.Transponder.TryGet(transponderIndex);
      Satellite sat = transponder.Satellite;

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

    public override void UpdateRawData()
    {
      base.UpdateRawData();
#if false
      bool deleted = this.NewProgramNr == -1;
      if (deleted)
        mapping.SetWord(_SatConfigIndex, 0xFFFF);
#endif
    }
  }
}
