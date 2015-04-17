using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  public class DbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "Samsung J-Series"; } }
    public string FileFilter { get { return "channel_list*-1200*.zip"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DbSerializer(inputFile);
    }
  }
}
