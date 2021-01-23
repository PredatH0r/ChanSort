using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  public class ToshibaPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Toshiba (*.zip, settingsDB.db)";
    public string FileFilter => "*.zip;*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      if (Path.GetExtension(inputFile).ToLower() == ".db")
        return new SettingsDbSerializer(inputFile);
      else
        return new ChmgtDbSerializer(inputFile);
    }
  }
}
