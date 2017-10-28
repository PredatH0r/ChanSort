using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Toshiba";
    public string FileFilter => "*.zip";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
