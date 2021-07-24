using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.LG
{
  public class LgPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "LG (*.tll, xx*.xml)";
    public string FileFilter => "*.tll;*.xml"; // can be GlobalClone00001.tll, xx*.tll with GlobalClone XML content, xx*.dll with binary data content, xx*.xml with GlobalClone XML content

    public SerializerBase CreateSerializer(string inputFile)
    {

      // webOS 5 files with <TLLDATA><ModelInfo><CloneVersion><MajorVersion>200</MajorVersion> .... contain all the actual channel data in JSON format inside a <legacybroadcast> element
      var content = File.ReadAllText(inputFile, Encoding.UTF8);
      if (content.Contains("<legacybroadcast>"))
        return new GlobalClone.GcJsonSerializer(inputFile, content);

      if (content.Contains("<TLLDATA>"))
        return new GlobalClone.GcXmlSerializer(inputFile);

      return new Binary.TllFileSerializer(inputFile) { IsTesting = this.IsTesting };
    }

    internal bool IsTesting { get; set; }
  }
}
