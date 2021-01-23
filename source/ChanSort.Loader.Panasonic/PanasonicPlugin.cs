using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  public class PanasonicPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Panasonic (*.db, *.bin)";
    public string FileFilter => "*.db;*.bin";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
