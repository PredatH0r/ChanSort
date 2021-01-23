using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  internal class LnbConfig : Api.LnbConfig
  {
    public Satellite Satellite { get; private set; }
    
    public LnbConfig(DataMapping mapping, DataRoot dataRoot)
    {
      this.Id = mapping.GetByte("SettingId");
      if (this.Id == 0)
        return;
      int satIndex = mapping.GetByte("SatIndex");
      this.Satellite = dataRoot.Satellites[satIndex];
    }
  }
}
