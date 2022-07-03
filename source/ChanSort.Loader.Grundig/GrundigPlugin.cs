using ChanSort.Api;

namespace ChanSort.Loader.Grundig;

public class GrundigPlugin : ISerializerPlugin
{
  public string DllName { get; set; }
  public string PluginName => "Grundig (dvb*_config.xml)";
  public string FileFilter => "*.xml";

  public SerializerBase CreateSerializer(string inputFile)
  {
    return new Serializer(inputFile);
  }
}