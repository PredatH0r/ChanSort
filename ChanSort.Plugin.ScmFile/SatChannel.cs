using ChanSort.Api;

namespace ChanSort.Loader.ScmFile
{
  class SatChannel : ScmChannelBase
  {
    private const string _TransponderIndex = "offTransponderIndex";

    public SatChannel(int slot, DataMapping data, DataRoot dataRoot) : base(data)
    {
      this.InitCommonData(slot, SignalSource.DvbS, data);
      if (!this.InUse)
        return;

      this.InitDvbData(data);

      int transponderIndex = data.GetWord(_TransponderIndex);
      Transponder transponder = dataRoot.Transponder.TryGet(transponderIndex);
      if (transponder == null)
      {
        dataRoot.Warnings.AppendLine("Invalid transponder index: " + transponderIndex);
        return;
      }

      Satellite sat = transponder.Satellite;
      this.Satellite = sat.Name;
      this.SatPosition = sat.OrbitalPosition;
      this.Polarity = transponder.Polarity;
      this.SymbolRate = transponder.SymbolRate;
      this.FreqInMhz = transponder.FrequencyInMhz;
      this.ChannelOrTransponder = "";
    }
  }
}
