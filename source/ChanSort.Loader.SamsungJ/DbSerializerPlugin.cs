using ChanSort.Api;

namespace ChanSort.Loader.SamsungJ
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName => "Samsung J-Series (*.zip)";
    public string FileFilter => "*.zip"; // "channel_list_t*.zip";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
