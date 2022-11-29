using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
{
  /*
  This serializer is used for the channel list format with a Repair\ folder containing files like channel_db_ver.db, mgr_chan_s_fta.db, FLASH_DTVINFO_S_FTA, ...
  The .db files are proprietary binary files, not SQLite databases.
  
  There are several variations of this file format using different record sizes, record counts and data offsets. Currently 2 different formats are known.
  They are identified by their channel record size in the mgr_chan_s_fta.db file, currently supporting 476 and 480. This decides the config that will be used for other files too.
  
  Lots of data is duplicated between FLASH_* and *.db files and must be updated in both.
  It seems that the .db file contains valid channels and the index of a channel record in this file is used as an index in the ChannelIdMappingTable of the FLASH file
  where IDs for for the channel and transponder are stored, which are used to find channels and transponders in the FLASH file. 
  The data records in the FLASH file are then looked up by their IDs, not by index.

  A full satellite scan usually populates the mgr_chan_s_fta.db + FLASH_DTVINFO_S_FTA files. A preset list fills mgr_chan_s_pkg.db and FLASH_DTVINFO_S_PKG.
  However there is also an example where a preset list uses mgr_chan_s_pkg.db + FLASH_DTVINFO_S_FTA.

  A preset list has a .db file where records are ordered by the desired channel order. The corresponding FLASH file however has a different channel record order
  and the ID-mapping table is used to resolve references.
     
  Due to lack of sample lists, the analog and DVB-T files have not been reverse engineered yet, but DVB-T is supported experimentally assuming it is identical to DVB-C
   
  The data offsets are defined in ChanSort.Loader.Philips.ini
  */
  class DbSerializer : SerializerBase
  {
    private readonly IniFile ini;

    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbT, "DVB-C");
    private readonly ChannelList dvbsFtaChannels = new ChannelList(SignalSource.DvbS | SignalSource.Provider0, "DVB-S FTA");
    private readonly ChannelList dvbsPkgChannels = new ChannelList(SignalSource.DvbS | SignalSource.Provider1, "DVB-S Preset");
    private readonly Dictionary<ChannelList, Tuple<string, int>> dbFileByList = new();
    private readonly Dictionary<ChannelList, Tuple<string, int>> flashFileByList = new();
    private int dvbtChannelRecordLength;
    private int dvbcChannelRecordLength;
    private int ftaChannelRecordLength;
    private int pkgChannelRecordLength;
    private readonly bool reorderPhysically;


    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      this.Features.MaxFavoriteLists = 1;
      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource; // doesn't work yet, must be hidden somewhere inside the FLASH files too
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanHaveGaps = true; // the mgr_chan_s_pkg can have gaps

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);

      var sec = ini.GetSection("flash_db");
      this.reorderPhysically = sec.GetBool("reorderRecordsByChannelNumber", true);
      var allowEdit = sec.GetBool("allowEdit", false);

      this.DataRoot.AddChannelList(dvbtChannels);
      this.DataRoot.AddChannelList(dvbcChannels);
      this.DataRoot.AddChannelList(dvbsFtaChannels);
      this.DataRoot.AddChannelList(dvbsPkgChannels);
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames = new List<string>
        {
          // data in *.db files
          "Position", // new progNr in main or fav list
          "OldPosition", // old progNr in main or fav list,
          nameof(Channel.Name),
          nameof(Channel.Favorites),
          nameof(Channel.FreqInMhz),
          nameof(Channel.SymbolRate),
          nameof(Channel.TransportStreamId),
          nameof(Channel.OriginalNetworkId),
          nameof(Channel.ServiceId),

          // additional data in FLASH_ files only
          nameof(ChannelInfo.PcrPid),
          nameof(ChannelInfo.VideoPid),
          nameof(ChannelInfo.AudioPid),
          nameof(ChannelInfo.ServiceTypeName)
        };
        list.ReadOnly = !allowEdit;
      }
    }
    #endregion

    #region Load()
    public override void Load()
    {
      bool validList = false;

      var dir = Path.GetDirectoryName(this.FileName) ?? ".";
      // must process *.db files first, then the FLASH_ files
      var files = Directory.GetFiles(dir, "*.db").Union(Directory.GetFiles(dir, "FLASH_*"));
      foreach (var file in files)
      {
        var lowercaseFileName = Path.GetFileName(file).ToLowerInvariant();
        switch (lowercaseFileName)
        {
          case "atv_channel_t.db":
            // TODO: no sample file yet that contains analog terrestrial channels
            break;
          case "atv_channel_c.db":
            // TODO: no sample file yet that contains analog cable channels
            break;
          case "channel_db_ver.db":
            LoadVersion(file);
            break;
          case "mgr_chan_dvbt.db":
            LoadDvb(file, lowercaseFileName, dvbtChannels, ref dvbtChannelRecordLength);
            validList = true;
            break;
          case "mgr_chan_dvbc.db":
            // no sample file with DVB-C data yet, so this here is a guess based on DVB-T
            LoadDvb(file, lowercaseFileName, dvbcChannels, ref dvbcChannelRecordLength);
            validList = true;
            break;
          case "mgr_chan_s_fta.db":
            LoadDvb(file, lowercaseFileName, dvbsFtaChannels, ref ftaChannelRecordLength);
            validList = true;
            break;
          case "mgr_chan_s_pkg.db":
            LoadDvb(file, lowercaseFileName, dvbsPkgChannels, ref pkgChannelRecordLength);
            validList = true;
            break;
          case "flash_dtvinfo_s_fta":
            if (dvbsFtaChannels.Count == 0 && dvbsPkgChannels.Count > 0)
              LoadFlash(file, lowercaseFileName, dvbsPkgChannels, pkgChannelRecordLength); // weird case where _pkg.db must be combined with FLASH_FTA
            else
              LoadFlash(file, lowercaseFileName, dvbsFtaChannels, ftaChannelRecordLength);
            break;
          case "flash_dtvinfo_s_pkg":
            if (dvbsPkgChannels.Count > 0)
              LoadFlash(file, lowercaseFileName, dvbsPkgChannels, pkgChannelRecordLength);
            break;
        }
      }

      if (!validList)
        throw LoaderException.TryNext(this.FileName + " is not a supported Philips Repair/channel_db_ver.db channel list");

      foreach (var channelList in this.DataRoot.ChannelLists)
      {
        foreach (var channelInfo in channelList.Channels)
        {
          var ch = (Channel)channelInfo;
          if (ch.FlashFileOffset == 0)
            this.DataRoot.Warnings.AppendLine($"Channel with index {ch.RecordIndex:d4} in .db file ({ch.OldProgramNr} - {ch.Name}) has no entry in FLASH files");
        }
      }
    }
    #endregion

    #region LoadVersion()
    private void LoadVersion(string file)
    {
      var data = File.ReadAllBytes(file);
      this.FileFormatVersion = "FLASH/.db";
      if (data.Length >= 2)
        this.FileFormatVersion += " " + BitConverter.ToInt16(data, 0);
      if (data.Length >= 3)
        this.FileFormatVersion += $"-{data[2]:D2}";
      if (data.Length >= 4)
        this.FileFormatVersion += $"-{data[3]:D2}";
      if (data.Length >= 5)
        this.FileFormatVersion += $" {data[4]:D2}";
      if (data.Length >= 6)
        this.FileFormatVersion += $":{data[5]:D2}";
      if (data.Length >= 7)
        this.FileFormatVersion += $":{data[6]:D2}";

      // Philips doesn't export any information about the TV model in this format. For automated stats I manually place modelinfo.txt files in the folders
      for (var dir = Path.GetDirectoryName(file); dir != null; dir = Path.GetDirectoryName(dir))
      {
        var path = Path.Combine(dir, "modelinfo.txt");
        if (File.Exists(path))
        {
          this.TvModelName = File.ReadAllText(path);
          break;
        }
      }
    }
    #endregion

    #region LoadDvbs()
    private void LoadDvb(string path, string sectionName, ChannelList list, ref int channelRecordLength)
    {
      var signalSource = list.SignalSource;
      var data = File.ReadAllBytes(path);
      var sec = ini.GetSection(sectionName);

      if (!GetValuesFromDvbFileHeader(sec, data, out var lenHeader, out var lenEntry, out var records, out var offChecksum))
      {
        this.DataRoot.Warnings.AppendLine($"{sectionName} was not loaded because data record size could not be determined");
        return;
      }

      var expectedChecksum = BitConverter.ToUInt16(data, offChecksum);
      var actualChecksum = (UInt16)CalcChecksum(data, 0, offChecksum);
      if (actualChecksum != expectedChecksum)
        throw LoaderException.Fail($"File {path} contains invalid checksum. Expected {expectedChecksum:x4} but calculated {actualChecksum:x4}");

      channelRecordLength = lenEntry;
      
      sec = this.ini.GetSection("mgr.db_entry:" + channelRecordLength);
      var mapping = new DataMapping(sec);
      var lenName = sec.GetInt("lenName");
      for (int i = 0; i < records; i++)
      {
        mapping.SetDataPtr(data, lenHeader + i * lenEntry);
        var oldProgNr = mapping.GetWord("offProgNr");

        // name is normally in 8-bit ASCII with unspecified encoding, but there has been an instance where some names in the file were in 16 bit big-endian unicode
        var off = mapping.BaseOffset + mapping.GetOffsets("offName")[0];
        var name = data[off + 0] == 0 ? (data[off + 1] == 0 ? "" : Encoding.BigEndianUnicode.GetString(data, off, lenName)) : DefaultEncoding.GetString(data, off, lenName);
        name = name.TrimEnd('\0');

        var ch = new Channel(signalSource, i, oldProgNr, name);
        ch.DbFileOffset = mapping.BaseOffset;
        var favPos = mapping.GetWord("offFav");
        if (favPos > 0)
          ch.SetOldPosition(1, favPos);
        ch.SymbolRate = mapping.GetWord("offSymbolRate");
        ch.FreqInMhz = mapping.GetDword("offFreq");
        if (ch.FreqInMhz > 13000) // DVB-S stores value in MHz, DVB-T in Hz
          ch.FreqInMhz /= 1000;
        if (ch.FreqInMhz > 13000)
          ch.FreqInMhz /= 1000;
        ch.TransportStreamId = mapping.GetWord("offTsid");
        ch.OriginalNetworkId = mapping.GetWord("offOnid");
        ch.ServiceId = mapping.GetWord("offSid");

        ch.AddDebug(mapping.GetByte("offFavFlags1"));
        ch.AddDebug(mapping.GetByte("offFavFlags2"));

        this.DataRoot.AddChannel(list, ch);
      }

      this.dbFileByList[list] = Tuple.Create(path, channelRecordLength);
    }
    #endregion

    #region GetValuesFromDvbFileHeader()
    private bool GetValuesFromDvbFileHeader(IniFile.Section sec, byte[] data, out int lenHeader, out int lenEntry, out int numEntries, out int checksumOffset)
    {
      lenHeader = 0;
      lenEntry = 0;
      numEntries = 0;
      checksumOffset = -1;

      // _fta.db or _pkg.db may contain only a tiny header without the required header fields or actual data
      if (data.Length < sec.GetInt("channelBlockSize") + 4)
        return false;

      lenHeader = sec.GetInt("lenHeader");
      var lenFooter = sec.GetInt("lenFooter");

      // the size of the channel entry can vary, so it must be calculated (seen 476 and 480 so far in mgr_chan_s_pkg.db and 472 in mgr_chan_dvbt.db)
      var mapping = new DataMapping(sec, data);
      var blockSize = mapping.GetDword("channelBlockSize");
      numEntries = mapping.GetWord("numTvChannels") + mapping.GetWord("numRadioChannels");
      if (numEntries == 0)
      {
        lenEntry = sec.GetInt("lenEntry");
        if (lenEntry == 0)
          return false;
        numEntries = (int)(blockSize / lenEntry);
      }
      else
      {
        lenEntry = (int)(blockSize / numEntries);
      }

      if (blockSize % numEntries != 0)
        return false;
      

      var offFooterChecksum = sec.GetInt("offFooterChecksum");
      checksumOffset = data.Length - lenFooter + offFooterChecksum;
      return true;
    }

    #endregion

    #region LoadFlash()
    private void LoadFlash(string path, string sectionName, ChannelList channelList, int dbChannelRecordLength)
    {
      if (dbChannelRecordLength == 0) // if the .db file wasn't read, ignore the FLASH file
        return;
      var data = File.ReadAllBytes(path);
      if (data.Length < 4)
        return;

      var expectedChecksum = BitConverter.ToUInt32(data, data.Length - 4);
      var actualChecksum = CalcChecksum(data, 0, data.Length - 4);
      if (actualChecksum != expectedChecksum)
        throw LoaderException.Fail($"File {path} contains invalid checksum. Expected {expectedChecksum:x8} but calculated {actualChecksum:x8}");

      var settings = this.ini.GetSection(sectionName + ":" + dbChannelRecordLength, true);
      var mapping = new DataMapping(settings, data);

      ReadTransponderFromFlash(settings, mapping);
      var idMapping = ReadChannelIdMappingFromFlash(settings, data);
      var filename = Path.GetFileName(path);
      ReadChannelBlocksFromFlash(filename, channelList, settings, data, mapping, idMapping);

      this.flashFileByList[channelList] = Tuple.Create(path, dbChannelRecordLength);
    }
    #endregion

    #region ReadTransponderFromFlash()
    private void ReadTransponderFromFlash(IniFile.Section settings, DataMapping mapping)
    {
      // read transponders (mostly for validating data that was already read from .db file)
      var off = settings.GetInt("offTransponderTable");
      var num = settings.GetInt("numTransponderTable");
      var len = settings.GetInt("lenTransponder");
      mapping.BaseOffset = off;
      for (int i = 0; i < num; i++)
      {
        var id = mapping.GetWord("transponderId");
        if (id != 0xFFFF)
        {
          var tp = new Transponder(id);
          tp.FrequencyInMhz = mapping.GetWord("freq");
          tp.SymbolRate = mapping.GetWord("symbolRate");
          tp.OriginalNetworkId = mapping.GetWord("onid");
          tp.TransportStreamId = mapping.GetWord("tsid");
          this.DataRoot.AddTransponder(null, tp);
        }

        mapping.BaseOffset += len;
      }
    }
    #endregion

    #region ReadChannelIdMappingFromFlash()
    private static Dictionary<int, IndexToIdMapping> ReadChannelIdMappingFromFlash(IniFile.Section settings, byte[] data)
    {
      // read a table that maps the channel index from the .db file to a channel ID and transponder ID in the FLASH file
      // and convert this into a reverse-lookup that allows to map a channel ID to a channel index and transponder ID
      var idMapping = new Dictionary<int, IndexToIdMapping>();
      var off = settings.GetInt("offChannelTransponderTable");
      var num = settings.GetInt("numChannelTransponderTable");
      for (int i = 0; i < num; i++)
      {
        var chanId = BitConverter.ToUInt16(data, off + i * 4 + 2);
        if (chanId == 0xFFFF) continue;
        var transpId = BitConverter.ToUInt16(data, off + i * 4 + 0);
        var flags = transpId & 0x1F;
        transpId >>= 5;
        idMapping[chanId] = new IndexToIdMapping(i, transpId, flags);
      }
      return idMapping;
    }
    #endregion

    #region ReadChannelBlocksFromFlash()
    private void ReadChannelBlocksFromFlash(string filename, ChannelList channelList, IniFile.Section settings, byte[] data, DataMapping mapping, Dictionary<int,IndexToIdMapping> idMapping)
    {
      // channel data is spread across multiple 64KB data blocks which all have a small header, then 734 channel records and a footer

      var off = settings.GetInt("offChannelBlock");
      var len = settings.GetInt("lenChannelBlock");
      for (int block = 0; off + (block + 1) * len <= data.Length; block++)
      {
        mapping.BaseOffset = off + block * len + settings.GetInt("offChannel");
        ReadChannelsFromFlashChannelBlock(filename, channelList, mapping, idMapping, block);
      }
    }
    #endregion

    #region ReadChannelsFromFlashChannelBlock
    private void ReadChannelsFromFlashChannelBlock(string filename, ChannelList channelList, DataMapping mapping, Dictionary<int,IndexToIdMapping> channelIdMapping, int block)
    {
      var settings = mapping.Settings;
      var numChannelsInBlock = settings.GetInt("numChannel");
      var lenChannel = settings.GetInt("lenChannel");

      mapping.BaseOffset -= lenChannel;
      for (int i = 0; i < numChannelsInBlock; i++)
      {
        mapping.BaseOffset += lenChannel;

        var id = mapping.GetWord("channelId");
        var flags = mapping.GetByte("flags"); // in the sample files this is always 63 except for "dead" records, where it is 0
        if (id == 0xFFFF || flags == 0)
          continue;
        if (!channelIdMapping.TryGetValue(id, out var idMapping))
        {
          this.DataRoot.Warnings.AppendLine($"Channel record in {filename}, block {block}, index {i:d4} has no entry in the ID mapping table");
          continue;
        }

        var index = idMapping.ChannelIndex;

        if (idMapping.ChannelIndex >= channelList.Channels.Count)
        {
          this.DataRoot.Warnings.AppendLine($"Channel record in {filename}, block {block}, index {i:d4} refers to non-existing channel index {index} in the .db file");
          continue;
        }

        var ch = (Channel)channelList.Channels[idMapping.ChannelIndex];
        ch.FlashFileOffset = mapping.BaseOffset;
        ch.AddDebug($"{ch.FlashFileOffset:x5}:{block}.{i:d3}");
        var hasDiff = false;
        var sid = mapping.GetWord("sid");
        var progNr = (mapping.GetWord("progNr") & 0x3FFF);
        hasDiff |= ch.ServiceId != sid;
        ch.PcrPid = mapping.GetWord("pcrPid");
        ch.VideoPid = mapping.GetWord("vpid");
        ch.AudioPid = mapping.GetWord("apid");
        hasDiff |= ch.OldProgramNr != progNr;

        var isRadio = (idMapping.Flags & 0x08) != 0;
        if (isRadio)
        {
          ch.SignalSource |= SignalSource.Radio;
          ch.ServiceTypeName = "Radio";
        }
        else
        {
          ch.SignalSource |= SignalSource.Tv;
          ch.ServiceTypeName = "TV";
        }

        if (!this.DataRoot.Transponder.TryGetValue(idMapping.TransponderId, out var tp))
          this.DataRoot.Warnings.AppendLine($"Channel record in {filename}, block {block}, index {i:d4}: could not find transponder record with id {idMapping.TransponderId}");
        else
        {
          hasDiff |= ch.OriginalNetworkId != tp.OriginalNetworkId;
          hasDiff |= ch.TransportStreamId != tp.TransportStreamId;
          hasDiff |= ch.FreqInMhz != tp.FrequencyInMhz;
        }

        if (hasDiff)
          this.DataRoot.Warnings.AppendLine($"Channel record in {filename}, block {block}, index {i:d4} does not match data in .db file: " +
                                            $"ProgNr={progNr}|{ch.OldProgramNr}, onid={tp?.OriginalNetworkId}|{ch.OriginalNetworkId}, tsid={tp?.TransportStreamId}|{ch.TransportStreamId}, " +
                                            $"sid={sid}|{ch.ServiceId}, freq={tp?.FrequencyInMhz}|{ch.FreqInMhz}");
      }
    }
    #endregion


    #region CalcChecksum()

    /// <summary>
    /// The checksum is the 32-bit sum over the byte-values in the file data from offset 0 to right before the checksum field
    /// </summary>
    private uint CalcChecksum(byte[] data, int start, int len)
    {
      uint checksum = 0;
      while (len > 0)
      {
        checksum += data[start++];
        --len;
      }

      return checksum;
    }
    #endregion

    public override IEnumerable<string> GetDataFilePaths() => this.dbFileByList.Values.Union(this.flashFileByList.Values).Select(tup => tup.Item1);

    #region Save()
    public override void Save()
    {
      // update *.db files
      foreach (var listAndFile in this.dbFileByList)
      {
        var list = listAndFile.Key;
        var file = listAndFile.Value.Item1;
        var lenEntry = listAndFile.Value.Item2;
        var secName = Path.GetFileName(file).ToLowerInvariant();
        SaveDvb(file, secName, list, lenEntry);
      }

      // update FLASH_* files
      foreach (var listAndFile in this.flashFileByList)
      {
        var list = listAndFile.Key;
        var file = listAndFile.Value.Item1;
        var dbChannelRecordSize = listAndFile.Value.Item2;
        var secName = Path.GetFileName(file).ToLowerInvariant();
        SaveFlash(file, secName, list, dbChannelRecordSize);
      }
    }
    #endregion

    #region SaveDvb()
    private void SaveDvb(string file, string secName, ChannelList list, int channelRecordLength)
    {
      var oldData = File.ReadAllBytes(file);

      var sec = ini.GetSection(secName);
      if (!GetValuesFromDvbFileHeader(sec, oldData, out var lenHeader, out var lenEntry, out _, out var offChecksum))
        return;

      var newData = new byte[oldData.Length];
      Array.Copy(oldData, newData, oldData.Length);

      var mapping = new DataMapping(ini.GetSection("mgr.db_entry:" + channelRecordLength));

      if (this.reorderPhysically)
      {
        int newIndex = 0;
        foreach (var chan in list.Channels.OrderBy(c => c.NewProgramNr).ThenBy(c => c.RecordIndex))
        {
          if (chan is not Channel ch)
            continue;
          var newOff = lenHeader + newIndex * lenEntry;
          Array.Copy(oldData, lenHeader + (int)ch.RecordIndex * lenEntry, newData, newOff, lenEntry);
          var favPos = Math.Max(0, ch.GetPosition(1));
          mapping.SetDataPtr(newData, newOff);
          mapping.SetWord("offProgNr", ch.NewProgramNr);
          mapping.SetWord("offFav", favPos);
          mapping.SetWord("offOldProgNr", ch.NewProgramNr);
          mapping.SetWord("offRecordIndex", newIndex);
          mapping.SetFlag("FavFlags1", favPos > 0);
          mapping.SetFlag("FavFlags2", favPos > 0);
          //ch.RecordIndex = newIndex; // will be updated when saving the FLASH file
          ++newIndex;
        }
      }
      else
      {
        foreach (var chan in list.Channels.OrderBy(c => c.NewProgramNr).ThenBy(c => c.RecordIndex))
        {
          if (chan is not Channel ch)
            continue;
          var newOff = lenHeader + (int)ch.RecordIndex * lenEntry;
          mapping.SetDataPtr(newData, newOff);
          mapping.SetWord("offProgNr", ch.NewProgramNr);
          mapping.SetWord("offFav", Math.Max(0, ch.GetPosition(1)));
        }
      }

      // update checksum (only 16 bits are stored)
      var checksum = CalcChecksum(newData, 0, offChecksum);
      newData[offChecksum + 0] = (byte)checksum;
      newData[offChecksum + 1] = (byte)(checksum >> 8);

      File.WriteAllBytes(file, newData);
    }
    #endregion

    #region SaveFlash()
    private void SaveFlash(string file, string secName, ChannelList list, int dbChannelRecordLength)
    {
      var data = File.ReadAllBytes(file);
      if (data.Length == 0)
        return;

      var sec = ini.GetSection(secName + ":" + dbChannelRecordLength, true);
      var mapping = new DataMapping(sec, data);

      // update channel index->id mapping table to match the indices in the new .db file, which is in order by the new ProgNr
      if (this.reorderPhysically)
      {
        var off = sec.GetInt("offChannelTransponderTable");
        var num = sec.GetInt("numChannelTransponderTable");
        var oldTable = new byte[num * 4];
        Array.Copy(data, off, oldTable, 0, oldTable.Length);
        int i = 0;
        foreach (var chan in list.Channels.OrderBy(c => c.NewProgramNr).ThenBy(c => c.RecordIndex))
        {
          if (chan is not Channel ch)
            continue;
          Array.Copy(oldTable, (int)ch.RecordIndex * 4, data, off + i * 4, 4);
          ch.RecordIndex = i++;
        }
      }

      // in-place update of channel data
      foreach (var chan in list.Channels)
      {
        if (chan is not Channel ch) 
          continue; // skip proxy channels
        if (ch.FlashFileOffset == 0)
          continue;
        mapping.BaseOffset = ch.FlashFileOffset;
        mapping.SetWord("progNr", ch.NewProgramNr | (mapping.GetWord("progNr") & ~0x3FFF));
        //mapping.SetWord("fav", Math.Max(0, ch.GetPosition(1)));
      }

      // update checksum (full 32 bit)
      var offChecksum = data.Length - 4;
      var checksum = CalcChecksum(data, 0, offChecksum);
      data[offChecksum + 0] = (byte)checksum;
      data[offChecksum + 1] = (byte)(checksum >> 8);
      data[offChecksum + 2] = (byte)(checksum >> 16);
      data[offChecksum + 3] = (byte)(checksum >> 24);

      File.WriteAllBytes(file, data);
    }
    #endregion


    #region IndexToIdMapping
    class IndexToIdMapping
    {
      public readonly int ChannelIndex;
      public readonly int TransponderId;
      public readonly int Flags;

      public IndexToIdMapping(int channelIndex, int transponderId, int flags)
      {
        this.ChannelIndex = channelIndex;
        this.TransponderId = transponderId;
        this.Flags = flags;
      }
    }
    #endregion
  }
}
