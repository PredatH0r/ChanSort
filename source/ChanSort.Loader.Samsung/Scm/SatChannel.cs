using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung.Scm
{
  class SatChannel : ScmChannelBase
  {
    private const string _TransponderIndex = "offTransponderIndex";

    public SatChannel(int slot, SignalSource presetList, DataMapping data, DataRoot dataRoot, FavoritesIndexMode sortedFavorites, IDictionary<int,string> providerNames) :
      base(data, sortedFavorites)
    {
      this.InitCommonData(slot, SignalSource.DvbS | presetList, data);
      if (!this.InUse)
      {
        this.IsDeleted = true;
        return;
      }

      this.InitDvbData(data, providerNames);

      int transponderIndex = data.GetWord(_TransponderIndex);
      Transponder transponder = dataRoot.Transponder.TryGet(transponderIndex);
      if (transponder == null)
      {
        var list = dataRoot.GetChannelList(this.SignalSource|SignalSource.Tv);
        dataRoot.Warnings.AppendFormat("{0} channel record #{1} (Pr# {2} \"{3}\") contains invalid transponder index {4}\r\n",
          list.ShortCaption, slot, this.OldProgramNr, this.Name, transponderIndex);
        return;
      }

      Satellite sat = transponder.Satellite;
      this.Satellite = sat.Name;
      this.SatPosition = sat.OrbitalPosition;
      this.Polarity = transponder.Polarity;
      this.SymbolRate = transponder.SymbolRate;
      this.FreqInMhz = transponder.FrequencyInMhz;
      this.ChannelOrTransponder = "";
    }

    public override void UpdateRawData()
    {
      if (this.IsDeleted) // "deleted" flag is currently unknown for sat channels
        this.InUse = false;
      base.UpdateRawData();
    }
  }
}
