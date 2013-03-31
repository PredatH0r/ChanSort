using ChanSort.Api;

namespace ChanSort.Plugin.ScmFile
{
  public class ScmSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "Samsung *.scm Loader"; } }
    public string FileFilter { get { return "*.scm"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new ScmSerializer(inputFile);
    }
  }
}
