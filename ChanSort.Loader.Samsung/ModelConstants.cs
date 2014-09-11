using ChanSort.Api;

namespace ChanSort.Loader.Samsung
{
  internal class ModelConstants
  {
    public readonly string series;
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
    public readonly int serviceProviderLength;
    public readonly bool SortedFavorites;
    public readonly int cyfraPlusChannelSize;

    public ModelConstants(IniFile.Section iniSection)
    {
      this.series = iniSection.Name.Substring(iniSection.Name.Length - 1);
      this.avbtChannelLength = iniSection.GetInt("map-AirA");
      this.dvbtChannelLength = iniSection.GetInt("map-AirD");
      this.dvbsChannelLength = iniSection.GetInt("map-SateD");
      this.hdplusChannelLength = iniSection.GetInt("map-AstraHDPlusD");
      this.cyfraPlusChannelSize = iniSection.GetInt("map-CyfraPlusD");
      this.ptcLength = iniSection.GetInt("PTC");
      this.dvbsSatelliteLength = iniSection.GetInt("SatDataBase.dat");
      this.dvbsTransponderLength = iniSection.GetInt("TransponderDataBase.dat");
      this.avbtFineTuneLength = iniSection.GetInt("FineTune");
      this.dvbtFineTuneLength = iniSection.GetInt("FineTune_Digital");
      this.serviceProviderLength = iniSection.GetInt("ServiceProvider", 108);
      int numFavorites = iniSection.GetInt("Favorites");
      int mask = 0;
      for (int i = 0; i < numFavorites; i++)
        mask = (mask << 1) | 1;
      this.supportedFavorites = (Favorites)mask;
      this.SortedFavorites = iniSection.GetInt("SortedFavorites") != 0;
    }
  }
}
