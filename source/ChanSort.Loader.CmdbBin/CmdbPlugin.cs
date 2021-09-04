using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.CmdbBin
{
  public class CmdbPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "cmdb .bin";
    public string FileFilter => "*.bin";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var dir = Path.GetDirectoryName(inputFile);
      
      // ignore Philips ChannelMap_100 channel lists which don't have atv_cmdb.bin and dtv_cmdb_2.bin in the same folder

      var anchorFile = Path.Combine(dir, "dtv_cmdb_2.bin");
      if (File.Exists(anchorFile) && File.Exists(Path.Combine(dir, "atv_cmdb.bin")))
        return new CmdbFileSerializer(anchorFile);

      return null;
    }
  }
}
