using ChanSort.Api;

namespace ChanSort.Loader.ScmFile
{
  internal class AnalogChannel : ScmChannelBase
  {
    private const string _Skip = "Skip";
    private const string _Frequency = "offFrequency";

    #region ctor()

    public AnalogChannel(int slot, SignalSource signalSource, DataMapping mapping, decimal freq) : base(mapping)
    {
      this.InitCommonData(slot, signalSource, mapping);

      this.FreqInMhz = (decimal)mapping.GetFloat(_Frequency); // C,D,E series have the value in the data record
      if (this.FreqInMhz == 0) // for B series take it from the Tuning table
        this.FreqInMhz = freq;
      if (this.FreqInMhz == 0) // fallback since Freq is part of the UID and requires a unique value
        this.FreqInMhz = slot;
      this.Skip = mapping.GetFlag(_Skip);
      this.ChannelOrTransponder = "";
    }

    #endregion

  }

}
