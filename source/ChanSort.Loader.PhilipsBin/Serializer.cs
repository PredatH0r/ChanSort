using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.PhilipsBin
{
  /*
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
  This code here copyies the whole record before updating the fields
  */
  class Serializer : SerializerBase
  {
    private readonly IniFile ini;
    private readonly List<string> dataFilePaths = new List<string>();
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbCT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbCT, "DVB-C");
    private readonly ChannelList satChannels = new ChannelList(SignalSource.DvbS, "DVB-S");


    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = false;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanSaveAs = false;
      this.Features.CanHaveGaps = false;
      this.Features.SupportedFavorites = Favorites.A;
      this.Features.SortedFavorites = true;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = false;

      this.DataRoot.AddChannelList(this.dvbtChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.satChannels);

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("ShortName");
      }

      var supportedColumns = new[] {"OldPosition", "Position", "Name", "Lock"};
      //this.satChannels.VisibleColumnFieldNames.Clear();
      //foreach(var supportedColumn in supportedColumns)
      //  this.satChannels.VisibleColumnFieldNames.Add(supportedColumn);

      this.satChannels.VisibleColumnFieldNames.Remove("AudioPid");
      this.satChannels.VisibleColumnFieldNames.Remove("ServiceTypeName");
      this.satChannels.VisibleColumnFieldNames.Remove("Encrypted");
      this.satChannels.VisibleColumnFieldNames.Remove("Hidden");

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      if (!SetFileNameToChanLstBin())
        throw new FileLoadException("Unsupported folder structure. Required files are:\n"
                                    + "ChannelList\\chanLst.bin\n"
                                    + "ChannelList\\channellib\\CableDigSrvTable\n"
                                    + "ChannelList\\s2channellib\\service.dat");

      var dir = Path.GetDirectoryName(this.FileName);
      LoadDvbCT(dvbtChannels, Path.Combine(dir, "channellib", "AntennaDigSrvTable"));
      LoadDvbCT(dvbcChannels, Path.Combine(dir, "channellib", "CableDigSrvTable"));

      LoadDvbsSatellites(Path.Combine(dir, "s2channellib", "satellite.dat"));
      LoadDvbsTransponders(Path.Combine(dir, "s2channellib", "tuneinfo.dat"));
      LoadDvbS(satChannels, Path.Combine(dir, "s2channellib", "service.dat"));
      LoadDvbsFavorites(Path.Combine(dir, "s2channellib", "favorite.dat"));
      var db_file_info = Path.Combine(dir, "s2channellib", "db_file_info.dat");
      if (File.Exists(db_file_info))
        this.dataFilePaths.Add(db_file_info);

      // for a proper ChanSort backup/restore with .bak files, the Philips _backup.dat files must also be included
      foreach(var file in this.dataFilePaths.ToList())
        this.dataFilePaths.Add(file.Replace(".dat", "_backup.dat"));
    }
    #endregion

    #region SetFileNameToChanLstBin()
    private bool SetFileNameToChanLstBin()
    {
      var dir = Path.GetDirectoryName(this.FileName);
      var dirName = Path.GetFileName(dir);
      if (StringComparer.InvariantCultureIgnoreCase.Compare(dirName, "channellib") == 0 || StringComparer.InvariantCultureIgnoreCase.Compare(dirName, "s2channellib") == 0)
      {
        dir = Path.GetDirectoryName(dir);
        dirName = Path.GetFileName(dir);
      }

      if (StringComparer.InvariantCultureIgnoreCase.Compare(dirName, "ChannelList") != 0)
        return false;

      var chanLstBin = Path.Combine(dir, "chanLst.bin");
      if (!File.Exists(chanLstBin))
        return false;

      if (!File.Exists(Path.Combine(dir, "channellib", "CableDigSrvTable")))
        return false;
      if (!File.Exists(Path.Combine(dir, "s2channellib", "service.dat")))
        return false;

      this.FileName = chanLstBin; // this file is used as a fixed reference point for the whole directory structure
      return true;
    }
    #endregion

    #region LoadDvbCT
    private byte[] LoadDvbCT(ChannelList list, string path)
    {
      if (!File.Exists(path))
        return null;

      var data = File.ReadAllBytes(path);
      if (data.Length < 20)
        return null;

      var recordSize = BitConverter.ToInt32(data, 8);
      var recordCount = BitConverter.ToInt32(data, 12);
      if (data.Length != 20 + recordCount * recordSize)
        throw new FileLoadException("Unsupported file content: " + path);

      this.dataFilePaths.Add(path);

      int baseOffset = 20;
      for (int i = 0; i < recordCount; i++, baseOffset += recordSize)
      {
        uint checksum = BitConverter.ToUInt32(data, baseOffset + 0);
        ushort progNr = BitConverter.ToUInt16(data, baseOffset + 122);
        byte locked = data[baseOffset + 140];
        int nameLen;
        for (nameLen=0; nameLen<64; nameLen+=2)
          if (data[baseOffset + 216 + nameLen] == 0)
            break;
        string channelName = Encoding.Unicode.GetString(data, baseOffset + 216, nameLen);

        data[baseOffset + 0] = 0;
        data[baseOffset + 1] = 0;
        data[baseOffset + 2] = 0;
        data[baseOffset + 3] = 0;
        var crc = FaultyCrc32(data, baseOffset, recordSize);

        if (crc != checksum)
          throw new FileLoadException($"Invalid CRC in record {i} in {path}");

        var ch = new ChannelInfo(list.SignalSource, i, progNr, channelName);
        ch.Lock = locked != 0;
        this.DataRoot.AddChannel(list, ch);
      }

      return data;
    }
    #endregion


    #region LoadDvbsSatellites()
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
        s.Name = this.DefaultEncoding.GetString(data, baseOffset + 16, 16).TrimEnd('\0');
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
        t.FrequencyInMhz = BitConverter.ToUInt16(data, baseOffset + 2);
        var satIndex = data[baseOffset + 6] >> 4; // guesswork
        t.Satellite = DataRoot.Satellites.TryGet(satIndex);
        t.TransportStreamId = tsid;
        t.OriginalNetworkId = onid;
        this.DataRoot.AddTransponder(t.Satellite, t);
      }
    }
    #endregion

    #region LoadDvbS
    private void LoadDvbS(ChannelList list, string path)
    {
      if (!File.Exists(path))
        return;

      var data = File.ReadAllBytes(path);
      if (data.Length < 4)
        return;

      var checksum = BitConverter.ToUInt32(data, data.Length - 4);

      var crcObj = new Crc32(false, Crc32.NormalPoly);
      var crc = ~crcObj.CalcCrc32(data, 0, data.Length - 4);
      if (checksum != crc)
        throw new FileLoadException("Invalid CRC32 in " + path);

      int recordSize = BitConverter.ToInt32(data, 4);
      int recordCount = BitConverter.ToInt32(data, 8);

      // 12 bytes header, then a "next/prev" table, then the service records, then a CRC32
      // the "next/prev" table is a ring-list, every entry consists of 2 ushorts with the next and previous channel, wrapping around on the ends
      if (data.Length != 12 + recordCount * 4 + recordCount * recordSize + 4)
        throw new FileLoadException("Unsupported file content: " + path);

      this.dataFilePaths.Add(path);

      var dvbStringDecoder = new DvbStringDecoder(this.DefaultEncoding);

      var mapping = new DataMapping(this.ini.GetSection("service.dat_entry"));
      mapping.SetDataPtr(data, 12 + recordCount * 4);
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
      var ch = new ChannelInfo(list.SignalSource, recordIndex, progNr, null);

      // deleted channels must be kept in the list because their records must also be physically reordered when saving the list
      if (progNr == 0xFFFF || transponderId == 0xFFFF)
      {
        ch.IsDeleted = true;
        ch.OldProgramNr = -1;
        return ch;
      }

      // onid, tsid, pcrpid and vpid can be 0 in some lists
      ch.PcrPid = mapping.GetWord("offPcrPid") & mapping.GetMask("maskPcrPid");
      ch.Lock = mapping.GetFlag("Locked");
      ch.OriginalNetworkId = mapping.GetWord("OffOnid");
      ch.TransportStreamId = mapping.GetWord("offTsid");
      ch.ServiceId = mapping.GetWord("offSid");
      ch.VideoPid = mapping.GetWord("offVpid") & mapping.GetMask("maskVpid");
      ch.Favorites = mapping.GetFlag("IsFav") ? Favorites.A : 0;
      ch.OldProgramNr = progNr;

      // the 0x1F as the first byte of the channel name is likely the DVB encoding indicator for UTF-8. So we use the DvbStringDecoder here
      dvbStringDecoder.GetChannelNames(mapping.Data, mapping.BaseOffset + mapping.GetConst("offName", 0), mapping.GetConst("lenName", 0), out var longName, out var shortName);
      ch.Name = longName.TrimEnd('\0');
      ch.ShortName = shortName.TrimEnd('\0');

      dvbStringDecoder.GetChannelNames(mapping.Data, mapping.BaseOffset + mapping.GetConst("offProvider", 0), mapping.GetConst("lenProvider", 0), out var provider, out _);
      ch.Provider = provider.TrimEnd('\0');

      // copy values from the satellite/transponder tables to the channel
      if (this.DataRoot.Transponder.TryGetValue(transponderId, out var t))
      {
        ch.Transponder = t;
        ch.FreqInMhz = t.FrequencyInMhz;
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
        this.satChannels.Channels[curFav].SetOldPosition(1, i + 1);
        curFav = BitConverter.ToInt16(data, baseOffset + curFav * 4 + 2);
      }
    }
    #endregion


    #region GetDataFilePaths

    /// <summary>
    /// List of files for backup/restore
    /// </summary>
    public override IEnumerable<string> GetDataFilePaths() => this.dataFilePaths;
    #endregion


    #region Save()
    public override void Save(string tvOutputFile)
    {
      var dir = Path.GetDirectoryName(this.FileName);

      // TODO: save cable and antenna channels

      SaveDvbsChannels(Path.Combine(dir, "s2channellib", "service.dat"));
      SaveDvbsFavorites(Path.Combine(dir, "s2channellib", "favorite.dat"));
      SaveDvbsDbFileInfo(Path.Combine(dir, "s2channellib", "db_file_info.dat"));
    }
    #endregion

    #region SaveDvbsChannels
    private void SaveDvbsChannels(string path)
    {
      var orig = File.ReadAllBytes(path);
      int recordSize = BitConverter.ToInt32(orig, 4);
      int recordCount = BitConverter.ToInt32(orig, 8);

      // create a new array for the modified data, copying the header and next/prev table
      var data = new byte[orig.Length];
      Array.Copy(orig, data, 12 + recordCount * 4);

      var baseOffset = 12 + recordCount * 4;

      var mapping = new DataMapping(this.ini.GetSection("service.dat_entry"));
      mapping.SetDataPtr(data, baseOffset);

      // copy physical records to bring them in the new order and update fields like progNr
      // this way the linked next/prev list remains in-sync with the channel order
      int i = 0;
      foreach (var ch in this.satChannels.Channels.OrderBy(c => c.NewProgramNr <= 0 ? int.MaxValue : c.NewProgramNr).ThenBy(c => c.OldProgramNr))
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
        }

        ch.RecordIndex = i++; // required so that subsequent saves don't reshuffle the records
      }


      var crc32 = ~Crc32.Reversed.CalcCrc32(data, 0, data.Length - 4);
      data.SetInt32(data.Length-4, (int)crc32);
      File.WriteAllBytes(path, data);

      var backupFile = path.Replace(".dat", "_backup.dat");
      File.WriteAllBytes(backupFile, data);
    }
    #endregion

    #region SaveDvbsFavorites
    private void SaveDvbsFavorites(string path)
    {
      var data = File.ReadAllBytes(path);

      int dataSize = BitConverter.ToInt32(data, 0);
      var recordSize = 4;
      var recordCount = (dataSize - 4) / recordSize;

      var favList = this.satChannels.Channels.Where(c => c.FavIndex[0] != -1).OrderBy(c => c.FavIndex[0]).ToList();
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


    #region FaultyCrc32
    public uint FaultyCrc32(byte[] bytes, int start, int count)
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
  }
}
