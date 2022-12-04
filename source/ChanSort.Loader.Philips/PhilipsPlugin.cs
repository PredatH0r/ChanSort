using System;
using System.IO;
using System.Text.RegularExpressions;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
{
  public class PhilipsPlugin : ISerializerPlugin
  {
    /*
     * Philips has a whole lot of completely incompatible channel list file formats with different folder structures.
     * Most formats have a chanLst.bin file, which contains a minor and major version number in the header and CRC16 checksums for various other files.
     * (first word is the minor, second word the major)
     *
     * version -1 (not an official number):
     * Repair\mgr_chan_dvbt.db (binary file, not a SQLite database)
     * Repair\FLASH_*
     * no chanLst.bin
     * 
     * version 0 (not an official number):
     * Repair\CM_*_LA_CK.BIN (+ a hidden .xml file with the actual channel list)
     * e.g. 47PFL5008K
     * no chanLst.bin
     *
     * version 1.1
     * Repair\ChannelList\chanLst.bin
     * Repair\ChannelList\channellib\CableDigSrvTable
     * Repair\ChannelList\s2channellib\service.dat
     * e.g. 32PFL5806K/02, 42PFL7656K/02
     *
     * version 1.2
     * same as version 1.1 for most parts, but different tuneinfo.dat format
     * e.g. 32PFL5507K/12, 42PFL4317K/12, 32PFL5507K/12
     *
     * version 11.1
     * PhilipsChannelMaps\ChannelMap_11\ChannelList\chanLst.bin
     * PhilipsChannelMaps\ChannelMap_11\ChannelList\channelLib\*Table (as with 1.1)
     * PhilipsChannelMaps\ChannelMap_11\ChannelList\s2channellib\*.dat (as with 1.1)
     * PhilipsChannelMaps\ChannelMap_11\ChannelList\s2channellib\Satellite*Table (new here)
     * e.g. 55PFL8008S/12
     *
     * version 30.1
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\chanLst.bin
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\list.db (for each of the input sources Sat/Cable/Terr 4 separate fav lists)
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\tv.db (contains channels from all sources)
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\channellib\*ChannelMaps.db (separate files for Cable and Terrestrial)
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\channellib\*Db.bin (separate files for Cable and Terrestrial)
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\s2channellib\SatelliteChannelMaps.db
     * PhilipsChannelMaps\ChannelMap_30\ChannelList\s2channellib\SatelliteDb.bin
     * e.g. 40PUK6400/12
     *
     * version 45.1
     * PhilipsChannelMaps\ChannelMap_45\ChannelList\chanLst.bin
     * PhilipsChannelMaps\ChannelMap_45\ChannelList\list.db (favorite lists for all sources)
     * PhilipsChannelMaps\ChannelMap_45\ChannelList\tv.db (SQLite database including all channels - maybe just for EPG?)
     * PhilipsChannelMaps\ChannelMap_45\ChannelList\channelLib\*Db.bin
     * PhilipsChannelMaps\ChannelMap_45\ChannelList\s2channellib\*Db.bin
     * e.g. 65PUS7601/12, 55PUS6581/12, 43PUS6401/12, 55PUS8601/12
     *
     * version 100.0
     * PhilipsChannelMaps\ChannelMap_100\ChannelList\chanLst.bin
     * PhilipsChannelMaps\ChannelMap_100\ChannelList\channellib\DVB*.xml
     * PhilipsChannelMaps\ChannelMap_100\ChannelList\s2channellib\DVBS.xml
     * e.g. 65PUS6754/12, 24PFT4022/12
     *
     * version 105.0
     * PhilipsChannelMaps\ChannelMap_105\Favorite.xml
     * rest like 100.0
     * e.g. 43PUS7307/12, 49PUS8303/12, 65PUS8503/12, 55OLED803/12
     *
     * version 110.0
     * same as 105.0
     * e.g. 65PUS8535/12, 55PUS7334/12
     *
     * version 115.0
     * same as 110.0
     *
     * Version 0.1 and 100-115 are XML based and loaded through the XmlSerializer.
     * Version 1.1 and 1.2 are loaded through the BinSerializer.
     * Version 0.0, 11.1 and 45.1 are not supported yet.
     */

    public string DllName { get; set; }
    public string PluginName => "Philips";
    public string FileFilter => "*.bin;*.xml;*.db";

    public SerializerBase CreateSerializer(string inputFile)
    {
      int majorVersion = int.MinValue;
      var filename = Path.GetFileName(inputFile).ToLowerInvariant();
      if (Regex.IsMatch(filename, @"^CM_.*\.(?:bin|xml)$", RegexOptions.IgnoreCase))
        majorVersion = 0;
      else 
      {
        // allow the user to pick pretty much any file within a Repair\ChannelList or PhilipsChannelMaps\ChannelMap_xxx\ChannelList structure to find a chanLst.bin
        var dir = Path.GetDirectoryName(inputFile);
        while(true)
        {
          var path = Path.Combine(dir, "chanLst.bin");
          if (File.Exists(path))
          {
            inputFile = path;
            var data = File.ReadAllBytes(inputFile);
            majorVersion = BitConverter.ToInt16(data, 2);
            break;
          }

          path = Path.Combine(dir, "channel_db_ver.db");
          if (File.Exists(path))
          {
            inputFile = path;
            majorVersion = -1;
            break;
          }

          var dirName = Path.GetFileName(dir).ToLowerInvariant();
          if (dirName == "channellib" || dirName == "s2channellib")
            dir = Path.GetDirectoryName(dir);
          else if (Directory.Exists(Path.Combine(dir, "PhilipsChannelMaps")))
            dir = Path.Combine(dir, "PhilipsChannelMaps");
          else if (Directory.Exists(Path.Combine(dir, "ChannelList")))
            dir = Path.Combine(dir, "ChannelList");
          else
          {
            var maps = Directory.GetDirectories(dir, "ChannelMap_*");
            if (maps.Length > 0)
              dir = maps[0];
            else
              break;
          }
        }
      }

      if (majorVersion == 0 || majorVersion >= 100 && majorVersion <= 115)
        return new XmlSerializer(inputFile);
      if (majorVersion == 1 || majorVersion == 30 || majorVersion == 45) // || majorVersion == 11 // format version 11  is similar to 1.x, but not (yet) supported
        return new BinarySerializer(inputFile);
      if (majorVersion == -1)
        return new DbSerializer(inputFile);

      if (majorVersion != int.MinValue)
        throw LoaderException.Fail($"Philips ChannelMap format version {majorVersion} is not supported (yet).");
      throw LoaderException.TryNext(SerializerBase.ERR_UnknownFormat);
    }
  }
}
