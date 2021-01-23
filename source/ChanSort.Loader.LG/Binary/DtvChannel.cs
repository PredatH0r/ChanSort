using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  public class DtvChannel : TllChannelBase
  {
    private const string _SignalSource = "offSignalSource";
    private const string _ChannelOrTransponder = "offChannelTransponder";
    private const string _FrequencyLong = "offFrequencyLong";

    /*
    offFavorites2 = 134
    offAudioPid2 = 182
    */

    public DtvChannel(int slot, DataMapping data) : base(data)
    {
      var signalSource = SignalSource.Digital;
      signalSource |= data.GetByte(_SignalSource) == 1 ? SignalSource.Antenna : SignalSource.Cable;
      this.InitCommonData(slot, signalSource, data);
      this.InitDvbData(data);

      int channel = data.GetByte(_ChannelOrTransponder);
      this.ChannelOrTransponder = channel.ToString("d2");
// ReSharper disable PossibleLossOfFraction
      this.FreqInMhz = (data.GetDword(_FrequencyLong)+10) / 1000;
// ReSharper restore PossibleLossOfFraction
      if (this.FreqInMhz == 0)
        this.FreqInMhz = LookupData.Instance.GetDvbtFrequency(channel);
    }
  }
}
