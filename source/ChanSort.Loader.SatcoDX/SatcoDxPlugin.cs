using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.SatcoDX
{
  public class SatcoDxPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "SatcoDX (ITT, Medion, Nabo, ok., Peaq, Schaub-Lorenz, Silva-Schneider, Telefunken)";
    public string FileFilter => "*.sdx";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var buffer = new byte[7];
      using (var strm = new FileStream(inputFile, FileMode.Open))
      {
        var len = strm.Read(buffer, 0, buffer.Length);
        if (len != buffer.Length || Encoding.ASCII.GetString(buffer, 0, len) != "SATCODX")
          return null;
      }

      return new Serializer(inputFile);
    }
  }
}
