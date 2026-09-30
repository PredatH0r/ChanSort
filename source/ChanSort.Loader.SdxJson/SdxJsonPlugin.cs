using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.SdxJson
{
  public class SdxJsonPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "SDX JSON (Anadol)";
    public string FileFilter => "*.sdx";

    public SerializerBase CreateSerializer(string inputFile)
    {
      // .sdx is also used by SatcoDX text files (handled by the SatcoDX loader) and by an encrypted format. This loader only handles JSON content.
      using (var strm = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
      {
        var buffer = new byte[64];
        var len = strm.Read(buffer, 0, buffer.Length);
        var i = Tools.HasUtf8Bom(buffer) ? 3 : 0;
        while (i < len && (buffer[i] == ' ' || buffer[i] == '\t' || buffer[i] == '\r' || buffer[i] == '\n'))
          ++i;
        if (i >= len || buffer[i] != '{')
          return null;
      }

      return new Serializer(inputFile);
    }
  }
}
