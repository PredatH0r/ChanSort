using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Medion;

public class MedionPlugin : ISerializerPlugin
{
  public string DllName { get; set; }
  public string PluginName => "Medion (senderliste.txt)";
  public string FileFilter => "*.txt";

  public SerializerBase CreateSerializer(string inputFile)
  {
    var content = File.ReadAllText(inputFile, Encoding.ASCII);
    if (content.StartsWith("{") && content.TrimEnd().EndsWith("}"))
      return new MedionSerializer(inputFile);

    throw LoaderException.TryNext("Unsupported file content");
  }
}

