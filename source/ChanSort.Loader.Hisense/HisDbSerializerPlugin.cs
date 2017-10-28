using ChanSort.Api;

namespace ChanSort.Loader.Hisense
{
  public class HisDbSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Hisense channel.db";
    public string FileFilter => "*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new HisDbSerializer(inputFile);
    }
  }
}