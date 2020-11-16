using ChanSort.Api;

namespace ChanSort.Loader.PhilipsBin
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Philips .bin/.dat";
    public string FileFilter => "*.bin;*.dat";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
