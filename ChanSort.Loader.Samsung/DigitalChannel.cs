using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung
{
  public class DigitalChannel : ScmChannelBase
  {
    private const string _ChannelOrTransponder = "offChannelTransponder";

    public DigitalChannel(int slot, SignalSource signalSource, DataMapping data,
      IDictionary<int, decimal> transpFreq, bool sortedFavorites, IDictionary<int, string> providerNames) :
      base(data, sortedFavorites)
    {
      this.InitCommonData(slot, (SignalSource)((int)signalSource & ~(int)(SignalSource.TvAndRadio)), data);

      if (!this.InUse || this.OldProgramNr == 0)
        return;

      this.InitDvbData(data, providerNames);

      int transp = data.GetByte(_ChannelOrTransponder);
      decimal freq = transpFreq.TryGet(transp);
      if (freq == 0)
        freq = transp*8 + 106; // (106 = DVB-C; DVB-T=306?)

      this.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(freq).ToString();
      this.FreqInMhz = freq;
    }
  }
}
