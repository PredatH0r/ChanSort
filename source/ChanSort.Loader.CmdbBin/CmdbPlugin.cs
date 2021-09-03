using ChanSort.Api;

namespace ChanSort.Loader.CmdbBin
{
  public class CmdbPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "cmdb .bin";
    public string FileFilter => "*.bin";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new CmdbFileSerializer(inputFile);
    }
  }
}
