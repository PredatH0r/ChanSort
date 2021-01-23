using ChanSort.Api;

namespace ChanSort.Loader.Samsung.Scm
{
  internal class AnalogChannel : ScmChannelBase
  {
    private const string _Frequency = "offFrequency";

    #region ctor()

    public AnalogChannel(int slot, bool isCable, DataMapping mapping, decimal freq, FavoritesIndexMode sortedFavorites) :
      base(mapping, sortedFavorites)
    {
      var signalSource = SignalSource.Analog | SignalSource.Tv;
      signalSource |= isCable ? SignalSource.Cable : SignalSource.Antenna;
      this.InitCommonData(slot, signalSource, mapping);

      var floatFreq = mapping.GetFloat(_Frequency);
      if (!float.IsNaN(floatFreq) && floatFreq > 100 && floatFreq < 2000) // some files seem to have a value way off scale
        this.FreqInMhz = (decimal)floatFreq; // C,D,E series have the value in the data record
      if (this.FreqInMhz == 0) // for B series take it from the Tuning table
        this.FreqInMhz = freq;
      if (this.FreqInMhz == 0) // fallback since Freq is part of the UID and requires a unique value
        this.FreqInMhz = slot;

      this.ChannelOrTransponder = "";
    }

    #endregion
  }

}
