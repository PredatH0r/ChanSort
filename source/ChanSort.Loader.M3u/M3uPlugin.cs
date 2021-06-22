using ChanSort.Api;

namespace ChanSort.Loader.M3u
{
  public class M3uPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "m3u (WinAmp, VLC, SAT>IP, ...)";
    public string FileFilter => "*.m3u";
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
