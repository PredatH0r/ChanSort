using ChanSort.Api;

namespace ChanSort.Loader.TllFile
{
  public class AnalogChannel : TllChannelBase
  {
    private const string _Freqency = "offPcrPid";
    private const string _FreqBand = "offVideoPid";

    public AnalogChannel(int slot, DataMapping data) : base(data)
    {
      this.InitCommonData(slot, SignalSource.AnalogCT, data);

      this.FreqInMhz = (decimal)data.GetWord(_Freqency) / 20;
      int channelAndBand = data.GetWord(_FreqBand);
      this.ChannelOrTransponder = ((channelAndBand>>8) == 0 ? "E" : "S") + (channelAndBand&0xFF).ToString("d2");
    }
  }
}
