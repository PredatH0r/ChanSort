using System.IO;
using System.IO.Compression;
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
      {
        // some Orsay TVs export a .zip file containing a Clone folder, which holds the same files as an .scm archive has in its root folder
        ZipArchiveEntry zipArchiveEntry = null;
        using (var stream = new FileStream(inputFile, FileMode.Open))
        {
          var zip = new ZipArchive(stream);
          zipArchiveEntry = zip.GetEntry("Clone/map-AirD");
        }
        if (zipArchiveEntry != null)
          return new ScmSerializer(inputFile, "Clone");

        return new DbSerializer(inputFile);
      }

      return null;
    }
  }
}
