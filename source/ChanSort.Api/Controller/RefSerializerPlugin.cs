using System.IO;

namespace ChanSort.Api
{
  public class RefSerializerPlugin : ISerializerPlugin
  {
    public string PluginName => "ChanSort Reference List";

    public string FileFilter => "*.txt;*.chl;*.csv";
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      var ext = (Path.GetExtension(inputFile) ?? "").ToLower();
      if (ext == ".csv")
        return new CsvFileSerializer(inputFile);
      else
        return new RefSerializer(inputFile);
    }
  }
}
