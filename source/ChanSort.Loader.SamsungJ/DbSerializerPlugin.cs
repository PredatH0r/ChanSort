using ChanSort.Api;

namespace ChanSort.Loader.SamsungJ
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Samsung J-K series";
    public string FileFilter => "*.zip"; // "channel_list_t*.zip";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
