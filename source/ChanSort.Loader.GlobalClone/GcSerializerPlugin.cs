using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  public class GcSerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "LG GlobalClone";
    public string FileFilter => "*Clone*.tll;xx*.xml;xx*.tll";

    public SerializerBase CreateSerializer(string inputFile)
    {
      // files with <TLLDATA><ModelInfo><CloneVersion><MajorVersion>200</MajorVersion> .... contain all the actual channel data in JSON format inside a <legacybroadcast> element
      var content = File.ReadAllText(inputFile, Encoding.UTF8);
      var startTag = "<legacybroadcast>";
      var start = content.IndexOf(startTag);
      if (start >= 0)
      {
        var end = content.IndexOf("</legacybroadcast>", start);
        var json = content.Substring(start + startTag.Length, end - start - startTag.Length);
        return new GcJsonSerializer(inputFile, content);
      }

      return new GcXmlSerializer(inputFile);
    }
  }
}
