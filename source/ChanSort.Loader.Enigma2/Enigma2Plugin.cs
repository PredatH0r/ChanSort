using ChanSort.Api;

namespace ChanSort.Loader.Enigma2
{
  public class Enigma2Plugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Enigma2 (Linux Receiver)";
    public string FileFilter => "lamedb";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
