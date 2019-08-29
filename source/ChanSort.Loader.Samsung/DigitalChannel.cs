using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung
{
  public class DigitalChannel : ScmChannelBase
  {
    private const string _ChannelOrTransponder = "offChannelTransponder";

    public DigitalChannel(int slot, SignalSource signalSource, DataMapping data,
      IDictionary<int, decimal> transpFreq, FavoritesIndexMode sortedFavorites, IDictionary<int, string> providerNames) :
      base(data, sortedFavorites)
    {
      this.InitCommonData(slot, signalSource & ~SignalSource.MaskTvRadioData, data);

      if (!this.InUse || this.OldProgramNr == 0)
        return;

      this.InitDvbData(data, providerNames);

      int transp = data.GetByte(_ChannelOrTransponder);
      decimal freq = transpFreq.TryGet(transp);
      if (freq == 0)
      {
        if ((this.SignalSource & SignalSource.Antenna) != 0)
          freq = transp * 8 + 306;
        else if ((this.SignalSource & SignalSource.Cable) != 0)
          freq = transp * 8 + 106;
      }

      this.ChannelOrTransponder = transp.ToString();
      this.FreqInMhz = freq;
    }
  }
}
