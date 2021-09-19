using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  public class PanasonicPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Panasonic (*.db, *.bin, *.xml)";
    public string FileFilter => "*.db;*.bin;*.xml";

    public SerializerBase CreateSerializer(string inputFile)
    {
      if (Path.GetExtension(inputFile).ToLowerInvariant() == ".xml")
      {
        var data = File.ReadAllBytes(inputFile);
        var header = Encoding.ASCII.GetBytes("<ChannelList>\n<ChannelInfo IsModified=");
        for (int i = 0; i < header.Length; i++)
        {
          if (data[i] != header[i])
            return null;
        }

        return new XmlSerializer(inputFile);
      }

      return new Serializer(inputFile);
    }
  }
}
