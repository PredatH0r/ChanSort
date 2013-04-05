using ChanSort.Api;

namespace ChanSort.Loader.TllFile
{
  public class TllFileSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "LG-Electronics *.tll"; } }
    public string FileFilter { get { return "*.TLL"; } }

    #region CreateSerializer()
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new TllFileSerializer(inputFile);
    }
    #endregion
  }
}
