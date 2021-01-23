using ChanSort.Api;

namespace ChanSort.Loader.SatcoDX
{
  public class SatcoDxPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "ITT/Medion/Nabo/ok./Peaq/Schaub-Lorenz/Silva-Schneider/Telefunken";
    public string FileFilter => "*.sdx";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
