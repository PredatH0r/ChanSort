using ChanSort.Api;

namespace ChanSort.Loader.Chmax
{
  public class ChmaxPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "chmax (*.chl)";
    public string FileFilter => "*.chl";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
