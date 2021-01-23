using ChanSort.Api;

namespace ChanSort.Loader.Sony
{
  public class SonyPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Sony (sdb.xml)";
    public string FileFilter => "*.xml";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
