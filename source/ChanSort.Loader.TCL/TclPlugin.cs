using ChanSort.Api;

namespace ChanSort.Loader.TCL
{
  public class TclPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "TCL";
    public string FileFilter => "*.tar";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new DtvDataSerializer(inputFile);
    }
  }
}
