using ChanSort.Api;

namespace ChanSort.Plugin.ScmFile
{
  internal class ModelConstants
  {
    public readonly int dvbsSatelliteLength;
    public readonly int dvbsTransponderLength;
    public readonly int dvbsChannelLength;
    public readonly int dvbtChannelLength;
    public readonly int avbtChannelLength;
    public readonly int hdplusChannelLength;
    public readonly int avbtFineTuneLength;
    public readonly int dvbtFineTuneLength;
    public readonly Favorites supportedFavorites;

    public ModelConstants(IniFile.Section iniSection)
    {
      this.avbtChannelLength = iniSection.GetInt("map-AirA");
      this.dvbtChannelLength = iniSection.GetInt("map-AirD");
      this.dvbsChannelLength = iniSection.GetInt("map-SateD");
      this.hdplusChannelLength = iniSection.GetInt("map-AstraHDPlusD");
      this.dvbsSatelliteLength = iniSection.GetInt("SatDataBase.dat");
      this.dvbsTransponderLength = iniSection.GetInt("TransponderDataBase.dat");
      this.avbtFineTuneLength = iniSection.GetInt("FineTune");
      this.dvbtFineTuneLength = iniSection.GetInt("FineTune_Digital");
      int numFavorites = iniSection.GetInt("Favorites");
      int mask = 0;
      for (int i = 0; i < numFavorites; i++)
        mask = (mask << 1) | 1;
      this.supportedFavorites = (Favorites)mask;
    }
  }
}
