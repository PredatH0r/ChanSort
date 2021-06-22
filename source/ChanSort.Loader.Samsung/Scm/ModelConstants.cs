using ChanSort.Api;

namespace ChanSort.Loader.Samsung.Scm
{
  public enum FavoritesIndexMode
  {
    /// <summary>
    /// D model uses values 0 and 1
    /// </summary>
    Boolean = 0,
    /// <summary>
    /// E model uses -1 for not-a-fav and 1..x for a fav program number
    /// </summary>
    IndividuallySorted = 1,
    /// <summary>
    /// some F models and H series uses -1 for not-a-fav, but expects 1..x to match the main program number
    /// </summary>
    MainProgramnrIndex = 2
  }

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
    public readonly int ptcLength;
    public readonly int serviceProviderLength;
    public readonly int numFavorites;
    public readonly FavoritesIndexMode SortedFavorites;
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
      this.numFavorites = iniSection.GetInt("Favorites");
      this.SortedFavorites = (FavoritesIndexMode)iniSection.GetInt("SortedFavorites");
    }
  }
}
