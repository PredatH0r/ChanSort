using ChanSort.Api;

namespace ChanSort.Loader.DvbViewer
{
  public class DvbViewerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "DVBViewer (*.ini)";
    public string FileFilter => "*.ini";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
