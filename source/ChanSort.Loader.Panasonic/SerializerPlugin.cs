using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName { get { return "Panasonic"; } }
    public string FileFilter { get { return "*.db;*.bin"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
