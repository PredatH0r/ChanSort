using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "Panasonic *.db,*.bin"; } }
    public string FileFilter { get { return "*.db;*.bin"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
