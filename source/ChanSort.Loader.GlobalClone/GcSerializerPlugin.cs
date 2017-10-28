using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  public class GcSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName { get { return "LG GlobalClone"; } }
    public string FileFilter { get { return "*Clone*.tll;xx*.xml"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new GcSerializer(inputFile);
    }
  }
}
