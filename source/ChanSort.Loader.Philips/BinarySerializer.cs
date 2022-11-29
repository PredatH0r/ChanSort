using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
{
  /*

  This loader handles the file format versions 1.x (*Table and *.dat files), version 25.x-45.x (*Db.bin files + tv.db and list.db)
  Version 30.x and 45.x were tested, version 25 is untested due to the lack of any sample files. Based on what I read online, the files are pretty much the 
  same as with Format 45.

  channellib\CableDigSrvTable:
  ===========================
  Channels in this file are not physically ordered by the program number and there is no linked list with prev/next indexes.
  When editing a channel with the Philips Channel Editor, it only updates the progNr field (and overwrites all trailing bytes of the channel name with 0x00).
  There is also the CablePresetTable file which is probably used for LCN. The Philips tool also updates the progNr in that file and uses it as is primary source
  for the progNr. I don't know if there is a direct reference from the channel to the preset, hence this code uses the combination of ONID+TSID+SID to link the two.

  s2channellib\service.dat:
  ========================
  All observed files have a perfectly linear next/prev table. The Philips Channel Editor also keeps that list linear and physically reorders the channel records.
  Also, all observed channel records have progNr either equal to the physical index + 1 or to 0xffff.
  Each channel record with progNr 0xFFFF causes a gap in the progNr sequence.
  It is unclear:
  - if the next/prev list MUST be kept linear 
  - channel records MUST be physically ordered to be in-sync with the next/prev list
  - channel records MUST be physically ordered by progNr (allowing 0xFFFF for gaps)
  To be on the safe side, this code keeps the list linear, physically reorders the records to match the progNr.
  Since we don't show deleted channels in the UI, we can't keep the gaps caused by them in the channel list. They will be appended at the end and the gaps closed.
  
  When swapping satellite channels 1 and 2 with the Philips Channel Editor 6.62, it only updates a few fields and leaves the rest stale.
  updated: SID, transponderIndex, channelName, providerName
  This code here copies the whole record before updating the fields.

  The favorite.dat file stores favorites as linked list which may support independent ordering from the main channel list.
  The Philips editor even saves non-linear lists, but not in any particular order.

  channellib\CableChannelMaps.db
  ==============================
  Used in format version 30 (not 45) as a 3rd file containing program numbers. SQLite database containing tables "AnalogTable" and "DigSrvTable".

  */
  public class BinarySerializer : SerializerBase
  {
    private readonly IniFile ini;
    private readonly List<string> dataFilePaths = new List<string>();
    // lists for old binary formats <= 11.x
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC, "DVB-C");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS, "DVB-S");
    // lists for binary format >= 25.x
    private readonly ChannelList antChannels = new ChannelList(SignalSource.Antenna, "Antenna");
    private readonly ChannelList cabChannels = new ChannelList(SignalSource.Cable, "Cable");
    private readonly ChannelList satChannels = new ChannelList(SignalSource.Sat, "Satellite");

    private ChanLstBin chanLstBin;
    private readonly StringBuilder logMessages = new StringBuilder();
    private readonly ChannelList favChannels = new ChannelList(SignalSource.All, "Favorites");
    private const int FavListCount = 8;
    private bool mustFixFavListIds;

    private readonly Dictionary<int, Channel> channelsById = new();


    #region ctor()
    public BinarySerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = false;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanHaveGaps = false;
      this.Features.FavoritesMode = FavoritesMode.Flags; // satellite favorites are stored in a separate file that may support independent sorting, but DVB C/T only have a flag
      this.Features.MaxFavoriteLists = 1; // Map45 format will change this
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = false;

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);
    }
    #endregion

    // loading

    #region Load
    public override void Load()
    {
      this.chanLstBin = new ChanLstBin();
      this.chanLstBin.Load(this.FileName, msg => this.logMessages.AppendLine(msg));
      this.TvModelName = this.chanLstBin.ModelName;
      this.FileFormatVersion = $"{chanLstBin.VersionMajor}.{chanLstBin.VersionMinor}";

      this.dataFilePaths.Add(this.FileName);

      var dir = Path.GetDirectoryName(this.FileName) ?? "";
      var channellib = Path.Combine(dir, "channellib");
      var s2channellib = Path.Combine(dir, "s2channellib");

      if (chanLstBin.VersionMajor <= 11)
      {
        this.DataRoot.AddChannelList(this.dvbtChannels);
        this.DataRoot.AddChannelList(this.dvbcChannels);
        this.DataRoot.AddChannelList(this.dvbsChannels);

        LoadDvbCT(dvbtChannels, Path.Combine(channellib, "AntennaDigSrvTable"), "CableDigSrvTable_entry");
        LoadDvbCTPresets(dvbtChannels, Path.Combine(channellib, "AntennaPresetTable"));
        LoadDvbCT(dvbcChannels, Path.Combine(channellib, "CableDigSrvTable"), "CableDigSrvTable_entry");
        LoadDvbCTPresets(dvbcChannels, Path.Combine(channellib, "CablePresetTable"));

        LoadDvbsSatellites(Path.Combine(s2channellib, "satellite.dat"));
        LoadDvbsTransponders(Path.Combine(s2channellib, "tuneinfo.dat"));
        LoadDvbS(dvbsChannels, Path.Combine(s2channellib, "service.dat"), "service.dat_entry");
        LoadDvbsFavorites(Path.Combine(s2channellib, "favorite.dat"));
        var db_file_info = Path.Combine(s2channellib, "db_file_info.dat");
        if (File.Exists(db_file_info))
          this.dataFilePaths.Add(db_file_info);
      }
      else if (chanLstBin.VersionMajor >= 25 && chanLstBin.VersionMajor <= 45)
      {
        // version 25-45
        this.Features.CanHaveGaps = true;

        this.DataRoot.AddChannelList(this.antChannels);
        this.DataRoot.AddChannelList(this.cabChannels);
        this.DataRoot.AddChannelList(this.satChannels);

        // version 45 supports mixed source favorites, version 30 (and probably 25) have separate fav lists per input source
        if (chanLstBin.VersionMajor == 45)
        {
          this.favChannels.IsMixedSourceFavoritesList = true;
          this.DataRoot.AddChannelList(this.favChannels);
        }

        LoadDvbCT(antChannels, Path.Combine(channellib, "TerrestrialDb.bin"), "Map45_CableDb.bin_entry");
        LoadDvbCT(cabChannels, Path.Combine(channellib, "CableDb.bin"), "Map45_CableDb.bin_entry");
        LoadDvbS(satChannels, Path.Combine(s2channellib, "SatelliteDb.bin"), "Map45_SatelliteDb.bin_entry");

        var tvDbFile = Path.Combine(dir, "tv.db");
        if (File.Exists(tvDbFile))
        {
          this.dataFilePaths.Add(tvDbFile);
          this.LoadTvDb(tvDbFile);
        }

        LoadMap30ChannelMapsDb(antChannels, Path.Combine(channellib, "TerrestrialChannelMaps.db"));
        LoadMap30ChannelMapsDb(cabChannels, Path.Combine(channellib, "CableChannelMaps.db"));
        LoadMap30ChannelMapsDb(satChannels, Path.Combine(s2channellib, "SatelliteChannelMaps.db"));

        // favorites in "list.db" have different schema depending on version
        var listDbFile = Path.Combine(dir, "list.db");
        if (File.Exists(listDbFile))
        {
          if (chanLstBin.VersionMajor == 30)
            this.LoadMap30Favorites(listDbFile);
          else if (chanLstBin.VersionMajor == 45)
            this.LoadMap45Favorites(listDbFile);
          this.dataFilePaths.Add(listDbFile);
        }

        satChannels.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Polarity));
      }
      else
      {
        throw LoaderException.Fail("Only Philips channel list format version 1.x and 25-45 are supported by this loader");
      }

      // for a proper ChanSort backup/restore with .bak files, the Philips _backup.dat files must also be included
      foreach (var file in this.dataFilePaths.ToList())
      {
        if (file.Contains(".dat"))
          this.dataFilePaths.Add(file.Replace(".dat", "_backup.dat"));
      }

      dvbsChannels.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Polarity));
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("ShortName");
        if (chanLstBin.VersionMajor <= 11)
          list.VisibleColumnFieldNames.Remove("ServiceTypeName");
        list.VisibleColumnFieldNames.Remove("Hidden");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        //list.VisibleColumnFieldNames.Remove("Encrypted");
      }

      foreach (var list in new[] { dvbcChannels, dvbtChannels, antChannels, cabChannels, satChannels })
      {
        list.VisibleColumnFieldNames.Remove("PcrPid");
        list.VisibleColumnFieldNames.Remove("VideoPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        list.VisibleColumnFieldNames.Remove("ChannelOrTransponder");
        list.VisibleColumnFieldNames.Remove("Provider");
      }

    }
    #endregion
    
    #region LoadDvbCT
    private void LoadDvbCT(ChannelList list, string path, string mappingName)
    {
      if (!ReadAndValidateChannellibFile(path, out var data, out var recordSize, out var recordCount)) 
        return;

      var mapping = new DataMapping(this.ini.GetSection(mappingName));
      mapping.SetDataPtr(data, chanLstBin.VersionMajor <= 11 ? 20 : 12);

      for (int i = 0; i < recordCount; i++, mapping.BaseOffset += recordSize)
      {
        var progNr = mapping.GetWord("offProgNr");
        
        var offChannelName = mapping.BaseOffset + mapping.GetConst("offName", 0);
        var lenName = mapping.GetConst("lenName", 0);
        for (int j = 0; j < lenName; j += 2)
        {
          if (data[offChannelName + j] == 0)
          {
            lenName = j;
            break;
          }
        }
        string channelName = Encoding.Unicode.GetString(data, offChannelName, lenName);

        if (chanLstBin.VersionMajor <= 11)
        {
          var checksum = mapping.GetDword("offChecksum");
          mapping.SetDword("offChecksum", 0);
          var crc = FaultyCrc32(data, mapping.BaseOffset + mapping.GetConst("offChecksum", 0), recordSize);
          if (crc != checksum)
            throw LoaderException.Fail($"Invalid CRC in record {i} in {path}");
        }

        var ch = new Channel(list.SignalSource & SignalSource.MaskAntennaCableSat, i, progNr, channelName);
        ch.Id = mapping.GetWord("offId"); // only relevant for ChannelMap45
        if (chanLstBin.VersionMajor <= 11)
          ch.FreqInMhz = (decimal) mapping.GetWord("offFreqTimes16") / 16;
        else
        {
          ch.FreqInMhz = mapping.GetDword("offFreq") / 1000;
          ch.Encrypted = mapping.GetDword("offEncrypted") != 0;
          ch.SignalSource |= mapping.GetDword("offIsDigital") == 0 ? SignalSource.Analog : SignalSource.Digital;
          if (mapping.GetDword("offServiceType") == 2)
          {
            ch.SignalSource |= SignalSource.Radio;
            ch.ServiceTypeName = "Radio";
          }
          else
          {
            ch.SignalSource |= SignalSource.Tv;
            ch.ServiceTypeName = "TV";
          }
        }

        ch.OriginalNetworkId = mapping.GetWord("offOnid");
        ch.TransportStreamId = mapping.GetWord("offTsid");
        ch.ServiceId = mapping.GetWord("offSid");
        ch.SymbolRate = (int)mapping.GetDword("offSymbolRate") / 1000;
        ch.Lock = mapping.GetByte("offLocked") != 0;
        ch.Favorites = mapping.GetByte("offIsFav") != 0 ? Favorites.A : 0;
        if (ch.Favorites != 0)
          ch.SetOldPosition(1, ch.OldProgramNr);

        this.DataRoot.AddChannel(list, ch);
      }
    }
    #endregion

    #region LoadDvbCTPresets
    private void LoadDvbCTPresets(ChannelList list, string path)
    {
      if (!ReadAndValidateChannellibFile(path, out var data, out var recordSize, out var recordCount))
        return;

      // build a mapping of (onid,tsid,sid) => channel
      var channelById = new Dictionary<ulong, Channel>();
      foreach(var chan in list.Channels)
      {
        var ch = (Channel)chan;
        var id = ((ulong)ch.OriginalNetworkId << 32) | ((ulong)ch.TransportStreamId << 16) | (uint)ch.ServiceId;
        channelById[id] = ch;
      }

      // apply preset progNr (LCN?) to the channel and remember the preset index for it
      var mapping = new DataMapping(this.ini.GetSection("CablePresetTable_entry"));
      mapping.SetDataPtr(data, 20);
      for (int i = 0; i < recordCount; i++, mapping.BaseOffset += recordSize)
      {
        var onid = mapping.GetWord("offOnid");
        var tsid = mapping.GetWord("offTsid");
        var sid = mapping.GetWord("offSid");
        var id = ((ulong)onid << 32) | ((ulong)tsid << 16) | sid;
        if (!channelById.TryGetValue(id, out var ch))
          continue;
        
        ch.PresetTableIndex = i;
        var progNr = mapping.GetWord("offProgNr");
        if (progNr != 0 && progNr != 0xFFFF)
          ch.OldProgramNr = progNr;
      }
    }
    #endregion

    #region ReadAndValidateChannellibFile
    private bool ReadAndValidateChannellibFile(string path, out byte[] data, out int recordSize, out int recordCount)
    {
      data = null;
      recordSize = 0;
      recordCount = 0;

      if (!File.Exists(path))
        return false;

      data = File.ReadAllBytes(path);
      if (chanLstBin.VersionMajor <= 11)
      {
        if (data.Length < 20)
          return false;

        recordSize = BitConverter.ToInt32(data, 8);
        recordCount = BitConverter.ToInt32(data, 12);
        if (data.Length != 20 + recordCount * recordSize)
          throw LoaderException.Fail("Unsupported file content: " + path);
      }
      else
      {
        if (data.Length < 12)
          return false;

        recordSize = 156; // Map45
        recordCount = BitConverter.ToInt32(data, 8);
        if (data.Length != 12 + recordCount * recordSize)
          throw LoaderException.Fail("Unsupported file content: " + path);
      }


      this.dataFilePaths.Add(path);
      return true;
    }

    #endregion

    #region LoadDvbsSatellites
    private void LoadDvbsSatellites(string path)
    {
      if (!File.Exists(path))
        return;
      var data = File.ReadAllBytes(path);
      if (data.Length < 4)
        return;
      var checksum = BitConverter.ToUInt32(data, data.Length - 4);
      var crc = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
      if (checksum != crc)
        return;
      int recordSize = BitConverter.ToInt32(data, 4);
      int recordCount = BitConverter.ToInt32(data, 8);

      // 12 byte header, table of (next, prev) transponder, records, crc32
      if (data.Length != 12 + recordCount * 4 + recordCount * recordSize + 4)
        return;

      var baseOffset = 12 + recordCount * 4;
      for (int i = 0; i < recordCount; i++, baseOffset += recordSize)
      {
        if (data[baseOffset + 0] == 0)
          continue;

        var s = new Satellite(i);
        var pos = (sbyte)data[baseOffset + 8];
        s.OrbitalPosition = pos < 0 ? -pos + "W" : pos + "E";
        s.Name = this.DefaultEncoding.GetString(data, baseOffset + 16, 16).TrimGarbage();

        this.DataRoot.AddSatellite(s);
      }
    }
    #endregion

    #region LoadDvbsTransponders
    private void LoadDvbsTransponders(string path)
    {
      if (!File.Exists(path))
        return;
      var data = File.ReadAllBytes(path);
      if (data.Length < 4)
        return;
      var checksum = BitConverter.ToUInt32(data, data.Length - 4);
      var crc = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
      if (checksum != crc)
        return;
      int recordSize = BitConverter.ToInt32(data, 4);
      int recordCount = BitConverter.ToInt32(data, 8);

      // 12 byte header, table of (next, prev) transponder, records, crc32
      if (data.Length != 12 + recordCount * 4 + recordCount * recordSize + 4)
        return;

      var baseOffset = 12 + recordCount * 4;
      for (int i = 0; i < recordCount; i++, baseOffset += recordSize)
      {
        var symRate = BitConverter.ToUInt16(data, baseOffset + 0);
        if (symRate == 0xFFFF)
          continue;

        var tsid = BitConverter.ToUInt16(data, baseOffset + 16);
        var onid = BitConverter.ToUInt16(data, baseOffset + 18);

        var t = new Transponder(i);
        t.SymbolRate = symRate;
        t.FrequencyInMhz = BitConverter.ToUInt16(data, baseOffset + 2) & 0x3FFF;
        t.Polarity = (BitConverter.ToUInt16(data, baseOffset + 2) & 0x4000) != 0 ? 'V' : 'H';
        var satIndex = data[baseOffset + 6] >> 4; // guesswork
        t.Satellite = DataRoot.Satellites.TryGet(satIndex);
        t.TransportStreamId = tsid;
        t.OriginalNetworkId = onid;
        this.DataRoot.AddTransponder(t.Satellite, t);
      }
    }
    #endregion

    #region LoadDvbS
    private void LoadDvbS(ChannelList list, string path, string mappingName)
    {
      if (!File.Exists(path))
        return;

      var data = File.ReadAllBytes(path);
      if (data.Length < 12)
        return;

      var version = chanLstBin.VersionMajor;

      if (version <= 11)
      {
        var checksum = BitConverter.ToUInt32(data, data.Length - 4);

        var crcObj = new Crc32(false, Crc32.NormalPoly);
        var crc = ~crcObj.CalcCrc32(data, 0, data.Length - 4);
        if (checksum != crc)
          throw LoaderException.Fail("Invalid CRC32 in " + path);
      }


      int recordSize = BitConverter.ToInt32(data, 4);
      int recordCount = BitConverter.ToInt32(data, 8);
      if (recordSize == 0 && version != 1)
        recordSize = recordCount == 0 ? 0 : (data.Length - 12) / recordCount;

      if (chanLstBin.VersionMajor <= 11)
      {
        // 12 bytes header, then a "next/prev" table, then the service records, then a CRC32
        // the "next/prev" table is a ring-list, every entry consists of 2 ushorts with the next and previous channel, wrapping around on the ends
        if (data.Length != 12 + recordCount * 4 + recordCount * recordSize + 4)
          throw LoaderException.Fail("Unsupported file content: " + path);
      }

      this.dataFilePaths.Add(path);

      var dvbStringDecoder = new DvbStringDecoder(this.DefaultEncoding);

      var mapping = new DataMapping(this.ini.GetSection(mappingName));
      mapping.SetDataPtr(data, 12 + (chanLstBin.VersionMajor <= 11 ? recordCount * 4 : 0));
      for (int i = 0; i < recordCount; i++, mapping.BaseOffset += recordSize)
      {
        var ch = LoadDvbsChannel(list, mapping, i, dvbStringDecoder);
        this.DataRoot.AddChannel(list, ch);
      }
    }
    #endregion

    #region LoadDvbsChannel
    private ChannelInfo LoadDvbsChannel(ChannelList list, DataMapping mapping, int recordIndex, DvbStringDecoder dvbStringDecoder)
    {
      var transponderId = mapping.GetWord("offTransponderIndex");
      var progNr = mapping.GetWord("offProgNr");
      var ch = new Channel(list.SignalSource & SignalSource.MaskAntennaCableSat, recordIndex, progNr, "");

      // deleted channels must be kept in the list because their records must also be physically reordered when saving the list
      if (progNr == 0xFFFF || transponderId == 0xFFFF)
      {
        ch.IsDeleted = true;
        ch.OldProgramNr = -1;
        return ch;
      }

      // onid, tsid, pcrpid and vpid can be 0 in some lists
      ch.PcrPid = mapping.GetWord("offPcrPid") & mapping.GetMask("maskPcrPid");
      ch.Lock = mapping.GetFlag("Locked", false);
      ch.Encrypted = mapping.GetFlag("Encrypted", false);
      ch.OriginalNetworkId = mapping.GetWord("offOnid");
      ch.TransportStreamId = mapping.GetWord("offTsid");
      ch.ServiceId = mapping.GetWord("offSid");
      ch.VideoPid = mapping.GetWord("offVpid") & mapping.GetMask("maskVpid");
      ch.Favorites = mapping.GetFlag("IsFav", false) ? Favorites.A : 0;
      ch.OldProgramNr = progNr;
      ch.Id = mapping.GetWord("offId"); // relevant for ChannelMap45

      if (chanLstBin.VersionMajor <= 11)
      {
        // the 0x1F as the first byte of the channel name is likely the DVB encoding indicator for UTF-8. So we use the DvbStringDecoder here
        dvbStringDecoder.GetChannelNames(mapping.Data, mapping.BaseOffset + mapping.GetConst("offName", 0), mapping.GetConst("lenName", 0), out var longName, out var shortName);
        ch.Name = longName;
        ch.ShortName = shortName;
      }
      else
      {
        ch.SignalSource |= mapping.GetWord("offIsDigital") == 0 ? SignalSource.Analog : SignalSource.Digital;
        ch.Name = Encoding.Unicode.GetString(mapping.Data, mapping.BaseOffset + mapping.GetConst("offName", 0), mapping.GetConst("lenName", 0)).TrimEnd('\0');
        ch.FreqInMhz = (decimal)mapping.GetDword("offFreq") / 1000;
        ch.Encrypted = mapping.GetDword("offEncrypted") != 0;
        ch.Polarity = mapping.GetDword("offPolarity") == 0 ? 'H' : 'V';
        ch.SymbolRate = (int)(mapping.GetDword("offSymbolRate") / 1000);
        if (mapping.GetDword("offServiceType") == 2)
        {
          ch.SignalSource |= SignalSource.Radio;
          ch.ServiceTypeName = "Radio";
        }
        else
        {
          ch.SignalSource |= SignalSource.Tv;
          ch.ServiceTypeName = "TV";
        }
        ch.AddDebug((byte)mapping.GetDword("offUnk1"));
        ch.AddDebug((byte)mapping.GetDword("offUnk2"));
      }

      dvbStringDecoder.GetChannelNames(mapping.Data, mapping.BaseOffset + mapping.GetConst("offProvider", 0), mapping.GetConst("lenProvider", 0), out var provider, out _);
      ch.Provider = provider;

      // copy values from the satellite/transponder tables to the channel
      if (this.DataRoot.Transponder.TryGetValue(transponderId, out var t))
      {
        ch.Transponder = t;
        ch.FreqInMhz = t.FrequencyInMhz;
        ch.Polarity = t.Polarity;
        ch.SymbolRate = t.SymbolRate;
        ch.SatPosition = t.Satellite?.OrbitalPosition;
        ch.Satellite = t.Satellite?.Name;
        if (t.OriginalNetworkId != 0)
          ch.OriginalNetworkId = t.OriginalNetworkId;
        if (t.TransportStreamId != 0)
          ch.TransportStreamId = t.TransportStreamId;
      }

      return ch;
    }

    #endregion

    #region LoadDvbsFavorites
    private void LoadDvbsFavorites(string path)
    {
      if (!File.Exists(path))
        return;
      var data = File.ReadAllBytes(path);
      if (data.Length < 4)
        return;
      var checksum = BitConverter.ToUInt32(data, data.Length - 4);
      var crc = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
      if (checksum != crc)
        return;

      int dataSize = BitConverter.ToInt32(data, 0);
      var recordSize = 4;
      var recordCount = (dataSize - 4) / recordSize;

      // 4 byte header, data, crc32
      if (data.Length != 4 + dataSize + 4)
        return;

      this.dataFilePaths.Add(path);

      int firstFavIndex = BitConverter.ToInt16(data, 4);
      int favCount = BitConverter.ToInt16(data, 6);
      if (favCount > recordCount || firstFavIndex < 0 || firstFavIndex >= recordCount)
        return;

      var baseOffset = 8;
      for (int i = 0, curFav = firstFavIndex; i < favCount; i++)
      {
        this.dvbsChannels.Channels[curFav].SetOldPosition(1, i + 1);
        curFav = BitConverter.ToInt16(data, baseOffset + curFav * 4 + 2);
      }
    }
    #endregion

    #region LoadMap30ChannelMapsDb
    private void LoadMap30ChannelMapsDb(ChannelList list, string dbPath)
    {
      // map30 format keeps channel numbers in 3 redundant locations: tv.db, a .bin file and a *ChannelMaps.db file
      // here we read the ChannelMaps.db file, compare the data with the .bin file and keep a reference to the records for later update
      if (!File.Exists(dbPath))
        return;
      this.dataFilePaths.Add(dbPath);
      
      using var conn = new SqliteConnection($"Data Source={dbPath};Pooling=False");
      conn.Open();
      using var cmd = conn.CreateCommand();

      var queries = new[]
      {
        new Tuple<SignalSource,string,string>(SignalSource.Analog | (list.SignalSource & SignalSource.MaskAntennaCableSat), "AnalogTable", 
          "select Dbindex, Frequency, 0, 0, 0, ChannelName, PresetNumber from AnalogTable order by Dbindex"),
        new Tuple<SignalSource,string,string>(SignalSource.Digital  | (list.SignalSource & SignalSource.MaskAntennaCableSat), "DigSrvTable", 
          "select Dbindex, Frequency, OriginalNetworkId, Tsid, ServiceId, ChannelName, PresetNumber from DigSrvTable order by Dbindex")
      };

      foreach (var entry in queries)
      {
        var source = entry.Item1;
        var table = entry.Item2;
        var query = entry.Item3;

        // not all files contain an AnalogTable table
        cmd.CommandText = $"select count(1) from sqlite_master where type='table' and name='{table}'";
        if ((long) cmd.ExecuteScalar() == 0)
          continue;

        cmd.CommandText = query;
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
          var idx = r.GetInt32(0);
          var freq = (decimal) r.GetInt32(1) / 1000;
          var onid = r.GetInt32(2);
          var tsid = r.GetInt32(3);
          var sid = r.GetInt32(4);
          var name = r.GetString(5);
          var prnr = r.GetInt32(6);

          var uid = ChannelInfo.GetUid(source, freq, onid, tsid, sid, null);
          var chans = list.GetChannelByUid(uid);
          if (chans == null || chans.Count == 0)
            this.logMessages.AppendLine($"{dbPath}: {table} entry with Dbindex={idx} has no corresponding channel in *.bin files");
          else
          {
            var ch = (Channel) chans[0];
            ch.Map30ChannelMapsDbindex = idx;
            if (ch.Name != name)
              this.logMessages.AppendLine($"{dbPath}: {table} entry with Dbindex={idx} has name {name}, in .bin file it is {ch.Name}");
            if (ch.OldProgramNr != prnr)
              this.logMessages.AppendLine($"{dbPath}: {table} entry with Dbindex={idx} has PresetNumber {prnr}, in .bin file it is {ch.OldProgramNr}");
          }
        }
      }
    }
    #endregion

    #region LoadTvDb
    private void LoadTvDb(string tvDb)
    {
      // the only purpose of this method is to validate if numbers in the the tv.db file are the same as in CableDb.bin/Terrestrial.bin/SatelliteDb.bin
      // differences are written to the log which can be viewed under File / File information
      // The tv.db file exists in formats 30 and 45 (and unconfirmed in 25 too)

      foreach (var chanList in this.DataRoot.ChannelLists)
      {
        if (chanList.IsMixedSourceFavoritesList)
          continue;
        foreach (var chan in chanList.Channels)
        {
          if (!(chan is Channel ch))
            continue;
          channelsById[ch.Id] = ch;
        }
      }

      using var conn = new SqliteConnection($"Data Source={tvDb};Pooling=False");
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "select _id, display_number, display_name, original_network_id, transport_stream_id, service_id, service_type from channels";
      var r = cmd.ExecuteReader();
      while (r.Read())
      {
        if (r.IsDBNull(1))
          continue;
        var prNr = r.GetString(1);
        if (!int.TryParse(prNr, out var nr))
          continue;

        var id = r.GetInt32(0);
        if (!channelsById.TryGetValue(id, out var ch))
        {
          this.logMessages.AppendLine($"Could not find channel with id {id} in tv.db");
          continue;
        }

        if (ch.OldProgramNr != nr)
          this.logMessages.AppendLine($"channel with id {id}: prNum {ch.OldProgramNr} in bin file and {r.GetString(1)} in tv.db");
        if (ch.Name != r.GetString(2))
          this.logMessages.AppendLine($"channel with id {id}: Name {ch.Name} in bin file and {r.GetString(2)} in tv.db");
        if (ch.OriginalNetworkId != r.GetInt32(3))
          this.logMessages.AppendLine($"channel with id {id}: ONID {ch.OriginalNetworkId} in bin file and {r.GetInt32(3)} in tv.db");
        if (ch.TransportStreamId != r.GetInt32(4))
          this.logMessages.AppendLine($"channel with id {id}: TSID {ch.TransportStreamId} in bin file and {r.GetInt32(4)} in tv.db");
        if (ch.ServiceId != r.GetInt32(5))
          this.logMessages.AppendLine($"channel with id {id}: SID {ch.ServiceId} in bin file and {r.GetInt32(5)} in tv.db");

        var stype = r.GetString(6);
        if (stype == "SERVICE_TYPE_AUDIO")
        {
          if ((ch.SignalSource & SignalSource.Radio) == 0)
            this.logMessages.AppendLine($"channel with id {id}: service type TV in bin file and RADIO in tv.db");
        }
        else // if (stype == "SERVICE_TYPE_AUDIO_VIDEO" || stype == "SERVICE_TYPE_OTHER") // Sky option channels are OTHER
        {
          if ((ch.SignalSource & SignalSource.Tv) == 0)
            this.logMessages.AppendLine($"channel with id {id}: service type RADIO in bin file and TV in tv.db");
        }

      }
    }
    #endregion

    #region LoadMap30Favorites
    private void LoadMap30Favorites(string listDb)
    {
      // The "list.db" file in format 30 contains tables TList1-4, CList1-4 and SList1-4 for 4 favorite favorite lists for cable, satellite and terrestrial
      // It also contains a table "List" with 12 entries holding names for the 3*4 favorite lists and SatFrequency and TCFrequency for satellite/terrestrial/cable frequencies
      // The list.db:xListN.channel_id field references the tv.db:channels._id field

      if (!File.Exists(listDb) || this.channelsById.Count == 0)
        return;

      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      this.Features.MaxFavoriteLists = 4;

      using var conn = new SqliteConnection($"Data Source={listDb};Pooling=False");
      conn.Open();
      using var cmd = conn.CreateCommand();

      // read favorite list names
      cmd.CommandText = "select list_id, list_name from List order by list_id";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = r.GetInt32(0);
          var name = r.GetString(1);
          var list = id <= 4 ? this.antChannels : id <= 8 ? this.cabChannels : this.satChannels;
          list.SetFavListCaption((id - 1)%4, name);
        }
      }

      // read favorite channels
      for (int listIdx=0; listIdx<12; listIdx++)
      {
        var table = "TCS"[listIdx / 4] + "List" + (listIdx % 4 + 1); // TList1-4, CList1-4, SList1-4
        cmd.CommandText = $"select count(1) from sqlite_master where type='table' and name='{table}'";
        if ((long) cmd.ExecuteScalar() == 0)
          continue;

        cmd.CommandText = $"select _id, channel_id from {table} order by rank";
        using var r = cmd.ExecuteReader();
        int order = 0;
        while (r.Read())
        {
          int channelId = r.GetInt32(1);
          if (this.channelsById.TryGetValue(channelId, out var ch))
            ch.SetOldPosition(1 + listIdx%4, ++order);
          else
            this.logMessages.AppendLine($"list.db: {table} _id {r.GetInt32(0)} references non-existing channel with channel_id {channelId}");
        }
      }
    }
    #endregion

    #region LoadMap45Favorites
    private void LoadMap45Favorites(string listDb)
    {
      foreach (var chanList in this.DataRoot.ChannelLists)
      {
        if (chanList.IsMixedSourceFavoritesList)
          continue;
        foreach (var chan in chanList.Channels)
        {
          chan.Source = chanList.ShortCaption;
          this.favChannels.AddChannel(chan);
        }
      }

      this.Features.FavoritesMode = FavoritesMode.MixedSource;
      this.Features.MaxFavoriteLists = 8;
      this.Features.AllowGapsInFavNumbers = false;

      using var conn = new SqliteConnection($"Data Source={listDb};Pooling=False");
      conn.Open();

      // older versions of ChanSort wrote invalid "list_id" values starting at 0 instead of 1 and going past 8.
      // if everything is in the range of 1-8, this code keeps the current ids. otherwise it remaps them to 1-8.
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "select min(list_id), max(list_id) from List";
      using (var r = cmd.ExecuteReader())
      {
        r.Read();
        mustFixFavListIds = !r.IsDBNull(0) && (r.GetInt16(0) < 1 || r.GetInt16(1) > 8);
        if (mustFixFavListIds)
          logMessages.AppendLine("invalid list_id values in list.db will be corrected");
      }

      cmd.CommandText = "select list_id, list_name from List order by list_id";
      var listIds = new List<int>();
      using (var r = cmd.ExecuteReader())
      {
        var listIndex = 0;
        while (r.Read())
        {
          var listId = r.GetInt16(0);
          listIds.Add(listId);
          if (!this.mustFixFavListIds)
            listIndex = listId - 1;
          this.favChannels.SetFavListCaption(listIndex, r.GetString(1));
          ++listIndex;
        }
      }

      for (int listIndex = 0; listIndex < listIds.Count; listIndex++)
      {
        cmd.CommandText = $"select channel_id from FavoriteChannels where fav_list_id={listIds[listIndex]} order by rank";
        using var r = cmd.ExecuteReader();
        int seq = 0;
        while (r.Read())
        { 
          var channelId = r.GetInt32(0);
          var chan = this.favChannels.Channels.FirstOrDefault(c => ((Channel)c).Id == channelId);
          if (chan == null)
          {
            this.logMessages.AppendLine($"Could not find favorite channel with id {channelId}");
            continue;
          }

          chan.SetOldPosition(listIndex + 1, ++seq);
        }
      }
    }
    #endregion

    // saving

    #region Save()
    public override void Save()
    {
      var dir = Path.GetDirectoryName(this.FileName) ?? "";
      var channellib = Path.Combine(dir, "channellib");
      var s2channellib = Path.Combine(dir, "s2channellib");

      if (chanLstBin.VersionMajor <= 11)
      {
        SaveDvbCTChannels(this.dvbtChannels, Path.Combine(channellib, "AntennaDigSrvTable"));
        SaveDvbCTPresets(this.dvbtChannels, Path.Combine(channellib, "AntennaPresetTable"));
        SaveDvbCTChannels(this.dvbcChannels, Path.Combine(channellib, "CableDigSrvTable"));
        SaveDvbCTPresets(this.dvbcChannels, Path.Combine(channellib, "CablePresetTable"));

        SaveDvbsChannels(this.dvbsChannels, Path.Combine(s2channellib, "service.dat"));
        SaveDvbsFavorites(Path.Combine(s2channellib, "favorite.dat"));
        SaveDvbsDbFileInfo(Path.Combine(s2channellib, "db_file_info.dat"));
      }
      else if (chanLstBin.VersionMajor >= 25 && chanLstBin.VersionMajor <= 45)
      {
        SaveDvbCTChannels(this.antChannels, Path.Combine(channellib, "TerrestrialDb.bin"));
        SaveDvbCTChannels(this.cabChannels, Path.Combine(channellib, "CableDb.bin"));
        SaveDvbsChannels(this.satChannels, Path.Combine(s2channellib, "SatelliteDb.bin"));

        SaveMap30ChannelMapsDb(this.antChannels, Path.Combine(channellib, "TerrestrialChannelMaps.db"));
        SaveMap30ChannelMapsDb(this.cabChannels, Path.Combine(channellib, "CableChannelMaps.db"));
        SaveMap30ChannelMapsDb(this.satChannels, Path.Combine(s2channellib, "SatelliteChannelMaps.db"));

        SaveTvDb();

        // favorite lists have different DB schema depending on version
        var listDb = Path.Combine(dir, "list.db");
        if (chanLstBin.VersionMajor == 30)
          SaveMap30Favorites(listDb);
        else if (chanLstBin.VersionMajor == 45)
          SaveMap45Favorites(listDb);
      }

      this.chanLstBin.Save(this.FileName);
    }

    #endregion

    #region SaveDvbCTChannels
    private void SaveDvbCTChannels(ChannelList list, string path)
    {
      if (!ReadAndValidateChannellibFile(path, out var data, out var recordSize, out _))
        return;

      int baseOffset;
      DataMapping mapping;

      if (chanLstBin.VersionMajor <= 11)
      {
        mapping = new DataMapping(this.ini.GetSection("CableDigSrvTable_entry"));
        baseOffset = 20;
      }
      else
      {
        mapping = new DataMapping(this.ini.GetSection("Map45_CableDb.bin_entry"));
        baseOffset = 12;
      }
      mapping.SetDataPtr(data, baseOffset);
      foreach (var ch in list.Channels)
      {
        if (ch.IsProxy) continue;
        mapping.BaseOffset = baseOffset + (int)ch.RecordIndex * recordSize;
        mapping.SetWord("offProgNr", ch.NewProgramNr);
        mapping.SetByte("offLocked", ch.Lock ? 1 : 0);
        mapping.SetByte("offIsFav", ch.Favorites == 0 ? 0 : 1);

        if (chanLstBin.VersionMajor <= 11)
        {
          mapping.SetDword("offChecksum", 0);
          var crc = FaultyCrc32(data, mapping.BaseOffset, recordSize);
          mapping.SetDword("offChecksum", crc);
        }
        else if (chanLstBin.VersionMajor >= 25 && chanLstBin.VersionMajor <= 45)
        {
          mapping.SetWord("offServiceEdit", 1);
        }
      }

      File.WriteAllBytes(path, data);
    }
    #endregion

    #region SaveDvbCTPresets
    private void SaveDvbCTPresets(ChannelList list, string path)
    {
      if (!ReadAndValidateChannellibFile(path, out var data, out var recordSize, out _))
        return;

      var mapping = new DataMapping(this.ini.GetSection("CablePresetTable_entry"));
      mapping.SetDataPtr(data, 20);


      // update the preset records with new channel numbers
      foreach (var chan in list.Channels)
      {
        if (!(chan is Channel ch) || ch.PresetTableIndex < 0)
          continue;
        mapping.BaseOffset = 20 + ch.PresetTableIndex * recordSize;
        mapping.SetWord("offProgNr", ch.NewProgramNr);

        mapping.SetDword("offChecksum", 0);
        var crc = FaultyCrc32(data, mapping.BaseOffset, recordSize);
        mapping.SetDword("offChecksum", crc);
      }

      File.WriteAllBytes(path, data);
    }
    #endregion

    #region SaveDvbsChannels
    private void SaveDvbsChannels(ChannelList list, string path)
    {
      var orig = File.ReadAllBytes(path);
      
      // create a new array for the modified data, copying the header and next/prev table
      var data = new byte[orig.Length];

      int recordCount = BitConverter.ToInt32(orig, 8);
      int recordSize;
      int baseOffset;
      DataMapping mapping;

      if (chanLstBin.VersionMajor <= 11)
      {
        recordSize = BitConverter.ToInt32(orig, 4);
        baseOffset = 12 + recordCount * 4;
        mapping = new DataMapping(this.ini.GetSection("service.dat_entry"));
      }
      else
      {
        recordSize = recordCount == 0 ? 0 : (orig.Length - 12) / recordCount;
        baseOffset = 12;
        mapping = new DataMapping(this.ini.GetSection("Map45_SatelliteDb.bin_entry"));
      }

      if (recordCount == 0)
        return;

      Array.Copy(orig, data, baseOffset);
      mapping.SetDataPtr(data, baseOffset);

      // copy physical records to bring them in the new order and update fields like progNr
      // this way the linked next/prev list remains in-sync with the channel order
      int i = 0;
      var channels = chanLstBin.VersionMajor < 25
        ? list.Channels.OrderBy(c => c.NewProgramNr <= 0 ? int.MaxValue : c.NewProgramNr).ThenBy(c => c.OldProgramNr)
        : list.Channels.OrderBy(c => c.RecordIndex);
      foreach (var ch in channels)
      {
        mapping.BaseOffset = baseOffset + i * recordSize;
        Array.Copy(orig, baseOffset + (int)ch.RecordIndex * recordSize, data, mapping.BaseOffset, recordSize);
        if (ch.IsDeleted)
        {
          mapping.SetWord("offSid", 0xFFFF);
          mapping.SetWord("offTransponderIndex", 0xFFFF);
          mapping.SetWord("offProgNr", 0xFFFF);
        }
        else
        {
          mapping.SetWord("offProgNr", ch.NewProgramNr);
          mapping.SetFlag("IsFav", ch.Favorites != 0);
          mapping.SetFlag("Locked", ch.Lock);
          if(mapping.GetWord("offWrongServiceEdit") == 1) // ChanSort versions before 2021-01-31 accidentally set a byte at the wrong offset
            mapping.SetWord("offWrongServiceEdit", 0);
          mapping.SetWord("offServiceEdit", 1);
        }

        ch.RecordIndex = i++; // required so that subsequent saves don't reshuffle the records
      }

      if (chanLstBin.VersionMajor <= 11)
      {
        var crc32 = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
        data.SetInt32(data.Length - 4, (int) crc32);

        var backupFile = path.Replace(".dat", "_backup.dat");
        File.WriteAllBytes(backupFile, data);
      }

      File.WriteAllBytes(path, data);
    }
    #endregion

    #region SaveDvbsFavorites
    private void SaveDvbsFavorites(string path)
    {
      var data = File.ReadAllBytes(path);

      int dataSize = BitConverter.ToInt32(data, 0);
      var recordSize = 4;
      var recordCount = (dataSize - 4) / recordSize;

      var favList = this.dvbsChannels.Channels.Where(c => c.GetPosition(1) != -1).OrderBy(c => c.GetPosition(1)).ToList();
      var favCount = favList.Count;
      var firstFavIndex = favCount == 0 ? -1 : (int)favList[0].RecordIndex;
      data.SetInt16(4, firstFavIndex);
      data.SetInt16(6, favCount);
      data.MemSet(8, 0xFF, recordCount * 4);
      if (favCount > 0)
      {
        var prevFav = (int) favList[favList.Count - 1].RecordIndex;
        var curFav = firstFavIndex;
        var nextFav = (int) favList[1 % favCount].RecordIndex;
        for (int i = 0; i < favCount; i++)
        {
          var ch = favList[i];
          var off = 8 + (int) ch.RecordIndex * 4;
          data.SetInt16(off + 0, prevFav);
          data.SetInt16(off + 2, nextFav);
          prevFav = curFav;
          curFav = nextFav;
          nextFav = (int) favList[(i + 2) % favCount].RecordIndex;
        }
      }

      var crc32 = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
      data.SetInt32(data.Length - 4, (int)crc32);
      File.WriteAllBytes(path, data);

      var backupFile = path.Replace(".dat", "_backup.dat");
      File.WriteAllBytes(backupFile, data);
    }
    #endregion

    #region SaveDvbsDbFileInfo
    private void SaveDvbsDbFileInfo(string path)
    {
      var data = File.ReadAllBytes(path);
      // the ushort at offset 10 is incremented by 4 every time a change is made to the list (maybe the lower 2 bits of that fields are used for something else)
      var offset = 10;
      data.SetInt16(offset,data.GetInt16(offset) + 4);

      var crc32 = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
      data.SetInt32(data.Length - 4, (int)crc32);
      File.WriteAllBytes(path, data);

      var backupFile = path.Replace(".dat", "_backup.dat");
      File.WriteAllBytes(backupFile, data);
    }
    #endregion

    #region SaveMap30ChannelMapsDb
    private void SaveMap30ChannelMapsDb(ChannelList list, string dbPath)
    {
      // map30 format keeps channel numbers in 3 redundant locations: tv.db, a .bin file and a *ChannelMaps.db file
      // here we save the ChannelMaps.db file
      if (!File.Exists(dbPath))
        return;

      using var conn = new SqliteConnection($"Data Source={dbPath};Pooling=False");
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();

      var tables = new[] {"AnalogTable", "DigSrvTable"};
      foreach (var table in tables)
      {
        // not all files contain an AnalogTable table
        cmd.CommandText = $"select count(1) from sqlite_master where type='table' and name='{table}'";
        if ((long)cmd.ExecuteScalar() == 0)
          continue;

        cmd.CommandText = $"update {table} set PresetNumber = @prNum where Dbindex = @dbindex";
        cmd.Parameters.Add("@prNum", SqliteType.Text);
        cmd.Parameters.Add("@dbindex", SqliteType.Integer);
        foreach(var channel in list.Channels)
        {
          if (!(channel is Channel ch) || ch.Map30ChannelMapsDbindex < 0)
            continue;
          cmd.Parameters["@dbindex"].Value = ch.Map30ChannelMapsDbindex;
          cmd.Parameters["@prNum"].Value = ch.NewProgramNr.ToString();
          cmd.ExecuteNonQuery();
        }
      }
      trans.Commit();
    }
    #endregion

    #region SaveTvDb
    /// <summary>
    /// The "tv.db" file was reported to exist as early as in ChannelMap_25 format and has been seen in formats 30 and 45 too
    /// </summary>
    private void SaveTvDb()
    {
      var tvDb = Path.Combine(Path.GetDirectoryName(this.FileName) ?? "", "tv.db");
      if (!File.Exists(tvDb))
        return;

      using var conn = new SqliteConnection($"Data Source={tvDb};Pooling=False");
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "update channels set display_number=@prNum, display_name=@name, browsable=@browsable, locked=@locked where _id=@id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", SqliteType.Integer);
      cmd.Parameters.Add("@prNum", SqliteType.Text);
      cmd.Parameters.Add("@name", SqliteType.Text);
      cmd.Parameters.Add("@browsable", SqliteType.Integer);
      cmd.Parameters.Add("@locked", SqliteType.Integer);
      cmd.Prepare();
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (var chan in list.Channels)
        {
          if (!(chan is Channel ch))
            continue;
          cmd.Parameters["@id"].Value = ch.Id;
          cmd.Parameters["@prNum"].Value = ch.NewProgramNr.ToString();
          cmd.Parameters["@name"].Value = ch.Name;
          cmd.Parameters["@browsable"].Value = ch.Skip ? 0 : 1;
          cmd.Parameters["@locked"].Value = ch.Lock ? 1 : 0;

          var res = cmd.ExecuteNonQuery();
          if (res == 0)
            this.logMessages.AppendFormat($"Could not update record with id {ch.Id} in tv.db service table");
        }
      }
      trans.Commit();
      conn.Close();
    }
    #endregion

    #region SaveMap30Favorites

    private void SaveMap30Favorites(string listDb)
    {
      if (!File.Exists(listDb) || this.channelsById.Count == 0)
        return;

      using var conn = new SqliteConnection($"Data Source={listDb};Pooling=False");
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();

      // save favorite channels
      for (int listIdx = 0; listIdx < 12; listIdx++)
      {
        var table = "TCS"[listIdx / 4] + "List" + (listIdx % 4 + 1); // TList1-4, CList1-4, SList1-4
        cmd.CommandText = $"select count(1) from sqlite_master where type='table' and name='{table}'";
        if ((long)cmd.ExecuteScalar() == 0)
          continue;

        var incFavList = (ini.GetSection("Map" + chanLstBin.VersionMajor)?.GetBool("incrementFavListVersion", true) ?? true)
          ? ", list_version=list_version+1"
          : "";

        var list = listIdx < 4 ? this.antChannels : listIdx < 8 ? this.cabChannels : this.satChannels;
        cmd.CommandText = "update List set list_name=@name" + incFavList + " where list_id=@listId";
        cmd.Parameters.Add("@listId", SqliteType.Integer);
        cmd.Parameters.Add("@name", SqliteType.Text);
        cmd.Parameters["@listId"].Value = listIdx + 1;
        cmd.Parameters["@name"].Value = list.GetFavListCaption(listIdx % 4, false);
        cmd.ExecuteNonQuery();


        cmd.CommandText = $"delete from {table}";
        cmd.Parameters.Clear();
        cmd.ExecuteNonQuery();

        cmd.CommandText = $"insert into {table} (_id, channel_id, rank) values (@id, @channelId, @rank)";
        cmd.Parameters.Add("@id", SqliteType.Integer);
        cmd.Parameters.Add("@channelId", SqliteType.Integer);
        cmd.Parameters.Add("@rank", SqliteType.Integer);

        int order = 0;
        foreach (var channel in list.Channels)
        {
          if (!(channel is Channel ch))
            continue;

          var favPos = ch.GetPosition(1 + listIdx % 4);
          if (favPos < 0)
            continue;

          ++order;
          cmd.Parameters["@id"].Value = order;
          cmd.Parameters["@channelId"].Value = ch.Id;
          cmd.Parameters["@rank"].Value = favPos;
          cmd.ExecuteNonQuery();
        }
      }

      trans.Commit();
    }
    #endregion

    #region SaveMap45Favorites
    private void SaveMap45Favorites(string listDb)
    {
      if (!File.Exists(listDb))
        return;

      using var conn = new SqliteConnection($"Data Source={listDb};Pooling=False");
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "delete from FavoriteChannels";
      cmd.ExecuteNonQuery();
      if (this.mustFixFavListIds)
      {
        cmd.CommandText = "delete from List";
        cmd.ExecuteNonQuery();
      }

      var incFavList = (ini.GetSection("Map" + chanLstBin.VersionMajor)?.GetBool("incrementFavListVersion", true) ?? true)
        ? ", list_version=list_version+1"
        : "";

      for (int favListIndex = 0; favListIndex < FavListCount; favListIndex++)
      {
        var favListId = favListIndex + 1;
        cmd.CommandText = $"select count(1) from List where list_id={favListId}";
        cmd.CommandText = (long) cmd.ExecuteScalar() == 0 ? 
          "insert into List (list_id, list_name, list_version) values (@id,@name,1)" : 
          "update List set list_name=@name" + incFavList + " where list_id=@id";

        cmd.Parameters.Add("@id", SqliteType.Integer);
        cmd.Parameters.Add("@name", SqliteType.Text);
        cmd.Parameters["@id"].Value = favListId;
        cmd.Parameters["@name"].Value = this.favChannels.GetFavListCaption(favListIndex) ?? "Fav " + (favListIndex + 1);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "insert into FavoriteChannels(fav_list_id, channel_id, rank) values (@listId,@channelId,@rank)";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@listId", SqliteType.Integer);
        cmd.Parameters.Add("@channelId", SqliteType.Integer);
        cmd.Parameters.Add("@rank", SqliteType.Real);
        cmd.Prepare();
        foreach (var chan in favChannels.Channels)
        {
          if (!(chan is Channel ch))
            continue;
          var rank = chan.GetPosition(favListIndex + 1);
          if (rank <= 0)
            continue;

          cmd.Parameters["@listId"].Value = favListId;
          cmd.Parameters["@channelId"].Value = ch.Id;
          cmd.Parameters["@rank"].Value = (double) rank;
          cmd.ExecuteNonQuery();
        }
      }

      // delete empty fav lists
      cmd.Parameters.Clear();
      cmd.CommandText = "delete from List where list_id not in (select fav_list_id from FavoriteChannels)";
      cmd.ExecuteNonQuery();

      // make sure the last_watched_channel_id is valid in the list
      cmd.CommandText = @"update List set last_watched_channel_id=(select channel_id from FavoriteChannels f where f.fav_list_id=List.list_id order by rank limit 1) where last_watched_channel_id not in (select channel_id from FavoriteChannels f where f.fav_list_id=List.list_id)";
      cmd.ExecuteNonQuery();

      trans.Commit();
      conn.Close();
    }
    #endregion

    // common

    #region FaultyCrc32
    /// <summary>
    /// Philips uses a broken CRC32 implementation, so we can't use the ChanSort.Api.Utils.Crc32 code
    /// </summary>
    public static uint FaultyCrc32(byte[] bytes, int start, int count)
    {
      var crc = 0xFFFFFFFF;
      var off = start;
      for (int i = 0; i < count; i++, off++)
      {
        var b = bytes[off];
        for (int j = 0; j < 8; j++)
        {
          crc <<= 1;
          var b1 = (uint)b >> 7;
          var b2 = crc >> 31;
          if (b1 != b2)
            crc ^= 0x04C11DB7;
          b <<= 1;
        }
      }

      return ~crc;
    }
    #endregion


    // framework support methods

    #region GetDataFilePaths

    /// <summary>
    /// List of files for backup/restore
    /// </summary>
    public override IEnumerable<string> GetDataFilePaths() => this.dataFilePaths;
    #endregion

    #region GetFileInformation
    public override string GetFileInformation()
    {
      return base.GetFileInformation() + this.logMessages.Replace("\n", "\r\n");
    }
    #endregion
  }
}
