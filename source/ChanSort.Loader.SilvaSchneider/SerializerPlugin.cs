using ChanSort.Api;

namespace ChanSort.Loader.SilvaSchneider
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName { get { return "Silva Schneider"; } }
    public string FileFilter { get { return "*.sdx"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
