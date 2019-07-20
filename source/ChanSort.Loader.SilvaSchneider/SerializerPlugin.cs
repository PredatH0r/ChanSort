using ChanSort.Api;

namespace ChanSort.Loader.SilvaSchneider
{
  public class SerializerPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName { get { return "ITT/Medion/Nabo/ok./Peaq/Schaub-Lorenz/Silva-Schneider/Telefunken"; } }
    public string FileFilter { get { return "*.sdx"; } }
    
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new Serializer(inputFile);
    }
  }
}
