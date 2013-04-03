using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public class DtvChannel : TllChannelBase
  {
    private const string _ChannelOrTransponder = "offChannelTransponder";
    private const string _FrequencyLong = "offFrequencyLong";

    /*
    offFavorites2 = 134
    offAudioPid2 = 182
    */

    public DtvChannel(int slot, DataMapping data) : base(data)
    {
      this.InitCommonData(slot, SignalSource.DvbCT, data);
      this.InitDvbData(data);

      this.ChannelOrTransponder = data.GetByte(_ChannelOrTransponder).ToString("d2");
      this.FreqInMhz = (decimal)data.GetDword(_FrequencyLong) / 1000;
    }
  }
}
