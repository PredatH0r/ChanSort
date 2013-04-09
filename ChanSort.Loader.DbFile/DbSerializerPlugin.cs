using ChanSort.Api;

namespace ChanSort.Loader.DbFile
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "Toshiba chmgt.db"; } }
    public string FileFilter { get { return "chmgt.db"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
