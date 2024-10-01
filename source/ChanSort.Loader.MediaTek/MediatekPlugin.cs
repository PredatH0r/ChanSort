using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.MediaTek;

public class MediatekPlugin : ISerializerPlugin
{
  public string DllName { get; set; }
  public string PluginName => "MediaTek (MtkChannelList.xml)";
  public string FileFilter => "Mtk*.xml";

  public SerializerBase CreateSerializer(string inputFile)
  {
    var dir = Path.GetDirectoryName(inputFile);

    // if there is a chanLst.bin file, let the Philips module handle the channel list
    //if (File.Exists(Path.Combine(dir, "chanLst.bin")))
    //  return null;

    return new Serializer(inputFile);
  }
}