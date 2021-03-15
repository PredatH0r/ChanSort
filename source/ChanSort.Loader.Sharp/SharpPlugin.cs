using ChanSort.Api;

namespace ChanSort.Loader.Sharp
{
  public class SharpPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Sharp";
    public string FileFilter => "*DVBS*.csv";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new SharpSerializer(inputFile);
    }
  }
}
