using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Sony
{
  public class SonyPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Sony (sdb.xml)";
    public string FileFilter => "*.xml";

    public SerializerBase CreateSerializer(string inputFile)
    {
      // Bravia 7 (and 8?) from 2024 onward use the same sdb.xml filename, but it contains Mediatek's XML
      using (var rdr = new StreamReader(inputFile))
      {
        var line1 = rdr.ReadLine() ?? "";
        var line2 = rdr.ReadLine() ?? "";
        if (line1.Contains("<service_list_transfer>") || line2.Contains("<service_list_transfer>"))
          return new MediaTek.Serializer(inputFile);
      }

      // older versions use Sony's proprietary XML
      return new Serializer(inputFile);
    }
  }
}
