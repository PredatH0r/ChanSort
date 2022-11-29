using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  public class ToshibaPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Toshiba";
    public string FileFilter => "*.db;*.bin;*.zip";

    public SerializerBase CreateSerializer(string inputFile)
    {
      var dir = Path.GetDirectoryName(inputFile);
      var name = Path.GetFileName(inputFile).ToLowerInvariant();
      var ext = Path.GetExtension(name);
      const string FILE_hotelopt = "hotelopt_type001.bin";

      // CLONE00001\settingsDB.db
      if (name == "settingsdb.db")
        return new SettingsDbSerializer(inputFile);

      if (name == "settingsdb_enc.db")
        throw LoaderException.Fail("settingsDB_enc.db files can't be edited due to the encryption that was introduced via Firmware-Update.\n" +
                                    "This affects TV models based on the Vestel hard-/firmware from various brands like Panasonic, Toshiba, Nokia, Nabo, ...");

      // selecting a chmgt.db, dvbMainData.db or dvbSysData.db directly -> use hotelopt_type001.bin instead
      if (ext == ".db")
      {
        var hotelopt = Path.Combine(Path.GetDirectoryName(dir), FILE_hotelopt);
        if (File.Exists(hotelopt))
          return new ChmgtDbSerializer(hotelopt);
      }

      // hotelopt_type001.bin can belong to different formats
      if (name == FILE_hotelopt)
      {
        if (File.Exists(Path.Combine(dir, "chmgt_type001", "chmgt.db")))
          return new ChmgtDbSerializer(inputFile);

        // atv_cmdb.bin is handledby the CmdbBin loader
        if (File.Exists(Path.Combine(dir, "atv_cmdb.bin")))
          return null;

        // "Acropolis" format with chmgt_type001\*.txt is not supported
        throw LoaderException.Fail(string.Format(SerializerBase.ERR_UnsupportedFormat, "Euro*.txt"));
      }

      // chmgt.db folder structure in a .zip file (for backward-compatibility with older ChanSort versions)
      if (ext == ".zip")
        return new ChmgtDbSerializer(inputFile);

      // *.sdx is handled by the SatcoDX loader
      if (ext == ".sdx")
        return null;

      // atv_cmdb.bin is handled by the CmdbBin loader
      if ((name.StartsWith("atv_") || name.StartsWith("dtv_")) && ext == ".bin")
        return null;

      // Hotel\tcl_clone_user.bin
      if (name.StartsWith("tcl_clone_") && name.EndsWith(".bin"))
        throw LoaderException.Fail(string.Format(SerializerBase.ERR_UnsupportedFormat, "tcl_clone_user.bin"));

      // HOTEL_*.bin
      if (name.StartsWith("hotel_") && name.EndsWith(".bin"))
        throw LoaderException.Fail(string.Format(SerializerBase.ERR_UnsupportedFormat, "HOTEL_*.bin"));

      throw LoaderException.TryNext(SerializerBase.ERR_UnknownFormat);
    }
  }
}
