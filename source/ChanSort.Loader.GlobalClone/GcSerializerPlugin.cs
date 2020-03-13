using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  public class GcSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "LG GlobalClone";
    public string FileFilter => "*Clone*.tll;xx*.xml;xx*.tll";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new GcSerializer(inputFile);
    }
  }
}
