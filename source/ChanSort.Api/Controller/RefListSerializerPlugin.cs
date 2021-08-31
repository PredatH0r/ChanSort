using System.IO;

namespace ChanSort.Api
{
  public class RefSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }

    public string PluginName => "ChanSort Reference List";

    public string FileFilter => "*.txt;*.chl;*.csv";
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      var ext = (Path.GetExtension(inputFile) ?? "").ToLowerInvariant();
      if (ext == ".csv")
        return new CsvRefListSerializer(inputFile);
      else
        return new TxtRefListSerializer(inputFile);
    }
  }
}
