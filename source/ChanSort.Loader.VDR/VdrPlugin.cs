using ChanSort.Api;

namespace ChanSort.Loader.VDR
{
  public class VdrPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Linux VDR (*.conf)";
    public string FileFilter => "*.conf";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
