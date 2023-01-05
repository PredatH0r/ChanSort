using ChanSort.Api;

namespace ChanSort.Loader.DBM
{
  public class DbmPlugin : ISerializerPlugin
  {
    /*
     * Various brands use variations of an underlying .DBM binary file format for DVB-C and DVB-S tuners.
     *
     * Known models include Xoro, TechniSat, ...
     */

    public string DllName { get; set; }
    public string PluginName => "DBM (Xoro, TechniSat, ...)";
    public string FileFilter => "*.dbm";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbmSerializer(inputFile);
    }
  }
}
