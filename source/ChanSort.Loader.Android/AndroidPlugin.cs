using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Android
{
  public class AndroidPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Android (*.db)";
    public string FileFilter => "*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var file = Path.GetFileName(inputFile).ToLowerInvariant();
      
      // dvr_rtk_tv.db known from "Alden"
      if (file.StartsWith("dvr_rtk_tv") && file.EndsWith(".db"))
        return new AldenSerializer(inputFile);

      if (!file.EndsWith(".db"))
        return null;

      throw LoaderException.TryNext(SerializerBase.ERR_UnknownFormat);
    }
  }
}
