using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "Toshiba *.zip"; } }
    public string FileFilter { get { return "*.zip"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
