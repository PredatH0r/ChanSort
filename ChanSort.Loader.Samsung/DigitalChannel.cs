using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung
{
  public class DigitalChannel : ScmChannelBase
  {
    private const string _ChannelOrTransponder = "offChannelTransponder";

    public DigitalChannel(int slot, bool isCable, DataMapping data,
      IDictionary<int, decimal> transpFreq, bool sortedFavorites) :
      base(data, sortedFavorites)
    {
      var signalSource = SignalSource.Digital;
      signalSource |= isCable ? SignalSource.Cable : SignalSource.Antenna;
      this.InitCommonData(slot, signalSource, data);
      this.InitDvbData(data);

      int transp = data.GetByte(_ChannelOrTransponder);
      decimal freq = transpFreq.TryGet(transp);
      if (freq == 0)
        freq = transp*8 + 106; // (106 = DVB-C; DVB-T=306?)

      this.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(freq).ToString();
      this.FreqInMhz = freq;
    }
  }
}
