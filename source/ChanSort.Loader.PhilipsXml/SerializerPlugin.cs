using ChanSort.Api;

namespace ChanSort.Loader.PhilipsXml
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Philips .xml";
    public string FileFilter => "*.xml";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
