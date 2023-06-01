using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Enigma2
{
  public class Enigma2Plugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Enigma2 (Linux Receiver)";
    public string FileFilter => "lamedb;*.tv;*.radio";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var ext = Path.GetExtension(inputFile).ToLowerInvariant();
      if (ext == ".tv" || ext == ".radio")
        inputFile = Path.Combine(Path.GetDirectoryName(inputFile) ?? "", "lamedb");
      return new Serializer(inputFile);
    }
  }
}
