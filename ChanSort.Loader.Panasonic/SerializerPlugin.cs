using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "Panasonic *.db"; } }
    public string FileFilter { get { return "*.db"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
