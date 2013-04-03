using ChanSort.Api;

namespace ChanSort.Loader.ScmFile
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
    public readonly int ptcLength;
    public readonly int favoriteNotSetValue;

    public ModelConstants(IniFile.Section iniSection)
    {
      this.avbtChannelLength = iniSection.GetInt("map-AirA");
      this.dvbtChannelLength = iniSection.GetInt("map-AirD");
      this.dvbsChannelLength = iniSection.GetInt("map-SateD");
      this.ptcLength = iniSection.GetInt("PTC");
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
      this.favoriteNotSetValue = iniSection.GetInt("FavoriteNotSet");
    }
  }
}
