using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.ScmFile
{
  public class DigitalChannel : ScmChannelBase
  {
    private const string _ChannelOrTransponder = "offChannelTransponder";

    public DigitalChannel(int slot, bool isCable, DataMapping data, 
      IDictionary<int, decimal> transpFreq, int favoriteNotSetValue) : 
      base(data, favoriteNotSetValue)
    {
      var signalSource = SignalSource.Digital;
      signalSource |= isCable ? SignalSource.Cable : SignalSource.Antenna;
      this.InitCommonData(slot, signalSource, data);
      this.InitDvbData(data);

      int transp = data.GetByte(_ChannelOrTransponder);
      decimal freq = transpFreq.TryGet(transp);
      if (freq == 0)
        freq = transp*8 + 106;

      this.ChannelOrTransponder = LookupData.Instance.GetDvbtChannel(freq).ToString();
      this.FreqInMhz = freq;
    }
  }
}
