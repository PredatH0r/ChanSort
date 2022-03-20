using ChanSort.Api;

namespace ChanSort.Loader.Loewe
{
  // The servicelist.db files are handled by the Hisense loader, which shares the same file format

  public class LoewePlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Loewe (servicelist.xml)";
    public string FileFilter => "*.xml";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
