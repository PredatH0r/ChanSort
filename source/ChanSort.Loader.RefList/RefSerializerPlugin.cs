using ChanSort.Api;

namespace ChanSort.Loader.RefList
{
  public class RefSerializerPlugin : ISerializerPlugin
  {
    public string PluginName => "ChanSort Reference List";

    public string FileFilter => "*.txt;*.chl";
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new RefSerializer(inputFile);
    }
  }
}
