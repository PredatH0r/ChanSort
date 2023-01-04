using ChanSort.Api;

namespace ChanSort.Loader.TCL
{
  public class TclPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "TCL";
    public string FileFilter => "*.tar;*.bin;*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var ext = Path.GetExtension(inputFile).ToLowerInvariant();
      if (ext == ".tar")
        return new DtvDataSerializer(inputFile);

      var name = Path.GetFileName(inputFile).ToLowerInvariant();
      var dir = Path.GetDirectoryName(inputFile);

      if (name == "dtvdata.db" || name == "satellite.db")
        return new DtvDataSerializer(Path.Combine(dir, "DtvData.db"));

      if (name == "clonecrc.bin")
      {
        // cloneCRC.bin normally is in the parent folder of userdata/DtvData.db, but might also be in the same folder
        var file1 = Path.Combine(dir, "userdata", "DtvData.db");
        var file2 = Path.Combine(dir, "DtvData.db");
        foreach (var file in new[] { file1, file2 })
        {
          if (File.Exists(file))
            return new DtvDataSerializer(file);
        }
      }

      throw LoaderException.TryNext("No DtvData.db file found");
    }
  }
}
