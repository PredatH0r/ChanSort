using ChanSort.Api;

namespace ChanSort.Loader.Hisense2017
{
  public class HisDbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName => "Hisense servicelist.db";
    public string FileFilter => "*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new HisDbSerializer(inputFile);
    }
  }
}