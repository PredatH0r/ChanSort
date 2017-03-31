#define HISENSE_ENABLED

/*
Support for the Hisense file format (Sep 2015) is currently disabled due to the risk of damaging the TV when 
users import files in an older/newer format than the currently installed firmware expects.
*/

using ChanSort.Api;

namespace ChanSort.Loader.Hisense
{
#if HISENSE_ENABLED
  public class HisDbSerializerPlugin : ISerializerPlugin
  {
    public string PluginName => "Hisense servicelist.db";
    public string FileFilter => "servicelist*.db";

#region CreateSerializer()
    public SerializerBase CreateSerializer(string inputFile)
    {
      return new HisDbSerializer(inputFile);
    }
#endregion
  }
#endif
}
