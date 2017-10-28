using ChanSort.Api;

namespace ChanSort.Loader.Samsung
{
  public class ScmSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName { get { return "Samsung B-H series"; } }
    public string FileFilter { get { return "*.scm"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new ScmSerializer(inputFile);
    }
  }
}
