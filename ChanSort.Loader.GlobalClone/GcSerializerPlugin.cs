using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  public class GcSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "LG GlobalClone"; } }
    public string FileFilter { get { return "*Clone*.tll"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new GcSerializer(inputFile);
    }
  }
}
