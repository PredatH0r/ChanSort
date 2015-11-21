using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName => "Toshiba *.zip";
    public string FileFilter => "Hotel*.zip";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
