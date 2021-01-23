using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung.Scm
{
  public class DigitalChannel : ScmChannelBase
  {
    private const string _ChannelOrTransponder = "offChannelTransponder";

    public DigitalChannel(int slot, SignalSource signalSource, DataMapping data, DataRoot dataRoot,
      IDictionary<int, decimal> transpFreq, FavoritesIndexMode sortedFavorites, IDictionary<int, string> providerNames) :
      base(data, sortedFavorites)
    {
      this.InitCommonData(slot, signalSource & ~SignalSource.MaskTvRadioData, data);

      if (!this.InUse)
        return;

      // "InUse" and "IsDeleted" are not always guessed correctly. If PrNr=0, the channel contains garbage
      if (this.OldProgramNr == 0)
      {
        this.InUse = false;
        return;
      }

      this.InitDvbData(data, providerNames);

      decimal freq = 0;
      int transp = data.GetByte(_ChannelOrTransponder);
      if (dataRoot.Transponder.TryGetValue(transp, out var tp))
        this.Polarity = tp.Polarity; 
      freq = transpFreq.TryGet(transp);
      if (freq == 0 && tp != null)
        freq = tp.FrequencyInMhz;
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
