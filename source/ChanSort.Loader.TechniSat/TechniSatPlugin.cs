using ChanSort.Api;

namespace ChanSort.Loader.TechniSat;

public class TechniSatPlugin : ISerializerPlugin
{
  public string DllName { get; set; }
  public string PluginName => "TechniSat";
  public string FileFilter => "*.cdp;*.csv";

  public SerializerBase CreateSerializer(string inputFile)
  {
    return new TechniSatSerializer(inputFile);
  }
}
