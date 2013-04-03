using System.IO;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public class TllFileSerializerPlugin : ISerializerPlugin
  {
    private const int MAX_FILE_SIZE = 16*1000*1000;
    private readonly string ERR_fileTooBig = Resource.TllFileSerializerPlugin_ERR_fileTooBig;


    public string PluginName { get { return Resource.TllFileSerializerPlugin_PluginName; } }
    public string FileFilter { get { return "*.TLL"; } }

    #region CreateSerializer()
    public SerializerBase CreateSerializer(string inputFile)
    {
      long fileSize = new FileInfo(inputFile).Length;
      if (fileSize > MAX_FILE_SIZE)
        throw new IOException(string.Format(ERR_fileTooBig, fileSize, MAX_FILE_SIZE));

      return new TllFileSerializer(inputFile);
    }
    #endregion

  }
}
