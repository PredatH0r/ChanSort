using ChanSort.Api;

namespace ChanSort.Loader.Amdb
{
  public class AmdbPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "AMDB (*.db)";
    public string FileFilter => "amdb*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      return new AmdbSerializer(inputFile);
    }
  }
}