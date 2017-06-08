using ChanSort.Api;

namespace ChanSort.Loader.LG
{
  public class TllFileSerializerPlugin : ISerializerPlugin
  {
    public string PluginName { get { return "LG model specific (xx*.tll)"; } }
    public string FileFilter { get { return "xx*.TLL"; } }

    #region CreateSerializer()
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new TllFileSerializer(inputFile);
    }
    #endregion
  }
}
