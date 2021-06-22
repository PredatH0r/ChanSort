using System.IO;
using ChanSort.Api;
using ChanSort.Loader.Samsung.Scm;
using ChanSort.Loader.Samsung.Zip;

namespace ChanSort.Loader.Samsung
{
  public class SamsungPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Samsung (*.scm, *.zip)";
    public string FileFilter => "*.scm;*.zip";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var ext = Path.GetExtension(inputFile).ToLowerInvariant();
      if (ext == ".scm")
        return new ScmSerializer(inputFile);
      if (ext == ".zip")
        return new DbSerializer(inputFile);
      return null;
    }
  }
}
