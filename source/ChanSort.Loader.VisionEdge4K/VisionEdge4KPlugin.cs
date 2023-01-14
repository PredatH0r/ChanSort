using ChanSort.Api;

namespace ChanSort.Loader.VisionEdge4K
{
  public class VisionEdge4KPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Vision EDGE 4K (*.db)";
    public string FileFilter => "*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new VisionEdge4KSerializer(inputFile);
    }
  }
}