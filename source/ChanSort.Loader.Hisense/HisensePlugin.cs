using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Hisense
{
  public class HisensePlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Hisense (channel.db, servicelist.db)";
    public string FileFilter => "*.db;*.bin";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var name = Path.GetFileName(inputFile).ToLowerInvariant();

      if (name.Contains("channel")) // UHD models 2015-2016
        return new ChannelDb.ChannelDbSerializer(inputFile);

      if (name.Contains("servicelist")) // models 2017 and later
        return new ServicelistDb.ServicelistDbSerializer(inputFile);

      if (name.StartsWith("his_dvb") && name.EndsWith(".bin")) // HIS_DVB.BIN
        return new HisBin.HisDvbBinSerializer(inputFile);

      if (name.StartsWith("his_") && name.EndsWith(".bin")) // HIS_SVL.BIN, HIS_TSL.BIN, HIS_FAV.BIN
        return new HisBin.HisSvlBinSerializer(inputFile);
      return null;
    }
  }
}