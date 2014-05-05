using ChanSort.Api;

namespace ChanSort.Loader.VDR
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "VDR Channels *.conf"; } }
    public string FileFilter { get { return "*.conf"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
