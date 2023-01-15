using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Unsupported
{
  // This Loader handles known unsupported file formats and informs users about the fact and that there is probably no need to ask for adding support

  public class UnsupportedPlugin : ISerializerPlugin
  {
    public string DllName { get; set; }
    public string PluginName => "Unsupported";
    public string FileFilter => "*";

    private string path;
    private string dir;
    private string name;
    private string ext;

    public SerializerBase CreateSerializer(string inputFile)
    {
      path = inputFile;
      dir = Path.GetDirectoryName(inputFile);
      name = Path.GetFileName(inputFile).ToLowerInvariant();
      ext = Path.GetExtension(name);

      CheckForMediaTekAndroidBin();
      CheckForVestelEncryptedDb();
      CheckForSdxEncrypted();
      
      throw LoaderException.TryNext("File is not on the list of explicitly unsupported formats");
    }

    #region CheckForMediaTekAndroidBin()
    /// <summary>
    /// Many different TV brands seem to use the same underlying hardware/firmware platform for their Android models.
    /// It is unclear how much the brands can customize the exported TV list, but many seem to use the default.
    /// In that configuration there is a .bin file, a .bin.crc, a .xml file and often a _sub.bin file on the USB stick.
    /// The .bin is a compressed archive with undocumented compression algorithm and file format.
    /// The .bin.crc does not use any of the standard CRC32 configurations.
    /// The .xml is contains only rudimentary and crippled data, that makes it useless.
    ///
    /// Known manufacturers using such lists: Grundig, Nokia, Panasonic, Philips, Sharp, Xiaomi
    /// </summary>
    private void CheckForMediaTekAndroidBin()
    {
      if (ext == ".bin" || ext == ".xml" || ext == ".crc")
      {
        var fname = Path.GetFileNameWithoutExtension(name);
        if (fname.EndsWith("_sub"))
          fname = fname.Substring(0, fname.Length - 4);
        var checkExt = new[] { ".bin", ".bin.crc", ".xml" };
        var hasAll = checkExt.All(ce => File.Exists(Path.Combine(dir, fname + ce)));
        if (hasAll)
          throw LoaderException.Fail(
            @"Encrypted MediaTek Android channel lists (used by many TV brands) can't be read/modified with ChanSort.");
      }
    }
    #endregion

    #region CheckForVestelEncryptedDb
    /// <summary>
    /// At some point Vestel firmware started to export encrypted settingsDB_enc.db instead of settingsDB.db.
    /// This seems to have affected many brands using the Vestel platform without their knowledge, including
    /// Panasonic, Nabo, Toshiba
    /// </summary>
    private void CheckForVestelEncryptedDb()
    {
      if (name.EndsWith("_enc") && ext == ".db")
        throw LoaderException.Fail(@"Encrypted Vestel channel lists (used by many TV brands) can't be read/modified with ChanSort.");
    }
    #endregion

    #region CheckForSdxEncrypted()
    /// <summary>
    /// Not sure if these .sdx files are also from SatcoDX, but they seem to have a 64 byte file header and then either encrypted or
    /// compressed data that can't be analyzed. One such file came from a Xoro 
    /// </summary>
    private void CheckForSdxEncrypted()
    {
      using var strm = new FileStream(this.path, FileMode.Open);
      var buffer = new byte[24];
      var len = strm.Read(buffer, 0, buffer.Length);
      if (len < buffer.Length)
        return;
      var str = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
      if (str.StartsWith("CDX\0MSTDatabaseV"))
        throw LoaderException.Fail("Encrypted SDX channel lists (used by Xoro and others) can't be read/modified with ChanSort.");
    }
    #endregion
  }
}
