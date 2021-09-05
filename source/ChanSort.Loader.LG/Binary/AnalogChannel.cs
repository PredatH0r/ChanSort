using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  public class AnalogChannel : TllChannelBase
  {
    private const string _SignalSource = "offSignalSource";
    private const string _Freqency = "offPcrPid";
    private const string _FreqBand = "offVideoPid";

    public AnalogChannel(int slot, DataMapping data) : base(data)
    {
      var signalSource = SignalSource.Analog;
      signalSource |= data.GetByte(_SignalSource) == 0 ? SignalSource.Antenna : SignalSource.Cable;

      this.InitCommonData(slot, signalSource, data);

      this.FreqInMhz = (decimal)data.GetWord(_Freqency) / 20;
      int channelAndBand = data.GetWord(_FreqBand);
      this.ChannelOrTransponder = ((channelAndBand>>8) == 0 ? "E" : "S") + (channelAndBand&0xFF).ToString("d2");
    }
  }
}
