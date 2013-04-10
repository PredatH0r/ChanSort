using ChanSort.Api;

namespace ChanSort.Loader.DbFile
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
