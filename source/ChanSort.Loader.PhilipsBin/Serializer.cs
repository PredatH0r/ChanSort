using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using ChanSort.Api;

namespace ChanSort.Loader.PhilipsBin
{
  
  class Serializer : SerializerBase
  {
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbCT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbCT, "DVB-C");
    private readonly ChannelList satChannels = new ChannelList(SignalSource.DvbS, "DVB-S");
    private readonly ChannelList favChannels = new ChannelList(SignalSource.All, "Favorites");


    private readonly IniFile ini;
    private byte[] dvbcData, dvbtData, dvbsData;

    private readonly List<string> dataFilePaths = new List<string>();

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = false;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanSaveAs = false;
      this.Features.SupportedFavorites = Favorites.A;
      this.Features.SortedFavorites = true;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = false;

      this.DataRoot.AddChannelList(this.dvbtChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.satChannels);
      //this.DataRoot.AddChannelList(this.favChannels);

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("ShortName");
      }

      var supportedColumns = new[] {"OldPosition", "Position", "Name", "Lock"};
      this.satChannels.VisibleColumnFieldNames.Remove("AudioPid");
      this.satChannels.VisibleColumnFieldNames.Remove("ServiceTypeName");
      this.satChannels.VisibleColumnFieldNames.Remove("Encrypted");
      this.satChannels.VisibleColumnFieldNames.Remove("Hidden");
      //this.satChannels.VisibleColumnFieldNames.Clear();
      //foreach(var supportedColumn in supportedColumns)
      //  this.satChannels.VisibleColumnFieldNames.Add(supportedColumn);

      this.favChannels.IsMixedSourceFavoritesList = true;
      this.ini = new IniFile("ChanSort.Loader.PhilipsBin.ini");
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
      dvbtData = LoadDvbCT(dvbtChannels, Path.Combine(dir, "channellib", "AntennaDigSrvTable"));
      dvbcData = LoadDvbCT(dvbcChannels, Path.Combine(dir, "channellib", "CableDigSrvTable"));

      LoadDvbsSatellites(Path.Combine(dir, "s2channellib", "satellite.dat"));
      LoadDvbsTransponders(Path.Combine(dir, "s2channellib", "tuneinfo.dat"));
      dvbsData = LoadDvbS(satChannels, Path.Combine(dir, "s2channellib", "service.dat"));
      LoadDvbsFavorites(Path.Combine(dir, "s2channellib", "favorite.dat"));
      var db_file_info = Path.Combine(dir, "s2channellib", "db_file_info.dat");
      if (File.Exists(db_file_info))
        this.dataFilePaths.Add(db_file_info);
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
    private byte[] LoadDvbS(ChannelList list, string path)
    {
      if (!File.Exists(path))
        return null;

      var data = File.ReadAllBytes(path);
      if (data.Length < 4)
        return null;

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
        var ch = new ChannelInfo(list.SignalSource, i, 0, null);

        var progNr = mapping.GetWord("offProgNr");
        var transponderId = mapping.GetWord("offTransponderIndex");
        if (progNr == 0xFFFF || transponderId == 0xFFFF)
        {
          ch.IsDeleted = true;
          ch.OldProgramNr = -1;
          DataRoot.AddChannel(list, ch);
          continue;
        }

        ch.PcrPid = mapping.GetWord("offPcrPid") & mapping.GetMask("maskPcrPid");
        ch.Lock = mapping.GetFlag("Locked");
        ch.OriginalNetworkId = mapping.GetWord("OffOnid"); // can be 0 in some lists
        ch.TransportStreamId = mapping.GetWord("offTsid");
        ch.ServiceId = mapping.GetWord("offSid");

        ch.VideoPid = mapping.GetWord("offVpid") & mapping.GetMask("maskVpid");
        //ch.Favorites = mapping.GetFlag("IsFav") ? Favorites.A : 0; // setting this here would mess up the proper order
        ch.OldProgramNr = progNr;

        dvbStringDecoder.GetChannelNames(data, mapping.BaseOffset + mapping.GetConst("offName",0), mapping.GetConst("lenName", 0), out var longName, out var shortName);
        ch.Name = longName.TrimEnd('\0');
        ch.ShortName = shortName.TrimEnd('\0');

        dvbStringDecoder.GetChannelNames(data, mapping.BaseOffset + mapping.GetConst("offProvider", 0), mapping.GetConst("lenProvider", 0), out var provider, out _);
        ch.Provider = provider.TrimEnd('\0');

        if (this.DataRoot.Transponder.TryGetValue(transponderId, out var t))
        {
          ch.Transponder = t;
          ch.FreqInMhz = t.FrequencyInMhz;
          ch.SymbolRate = t.SymbolRate;
          ch.SatPosition = t.Satellite?.OrbitalPosition;
          ch.Satellite = t.Satellite?.Name;
          if (ch.OriginalNetworkId == 0)
            ch.OriginalNetworkId = t.OriginalNetworkId;
          if (ch.TransportStreamId == 0)
            ch.TransportStreamId = t.TransportStreamId;
        }

        this.DataRoot.AddChannel(list, ch);
      }

      return data;
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

      int firstFavIndex = BitConverter.ToInt16(data, 4);
      int favCount = BitConverter.ToInt16(data, 6);
      if (favCount > recordCount || firstFavIndex < 0 || firstFavIndex >= recordCount)
        return;

      this.dataFilePaths.Add(path);

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
      byte[] deletedChannelData = { 
        0x00, 0x00, 0x00, 0x5c, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xc2, 0x3f, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00
      };

      var data = File.ReadAllBytes(path);
      int recordSize = BitConverter.ToInt32(data, 4);
      int recordCount = BitConverter.ToInt32(data, 8);

      var mapping = new DataMapping(this.ini.GetSection("service.dat_entry"));
      mapping.SetDataPtr(data, 12 + recordCount * 4);

      foreach (var ch in this.satChannels.Channels)
      {
        mapping.BaseOffset = 12 + recordCount * 4 + (int)ch.RecordIndex * recordSize;
        if (ch.IsDeleted)
        {
          Array.Copy(deletedChannelData, 0, data, mapping.BaseOffset, Math.Min(deletedChannelData.Length, recordSize));
          continue;
        }
        mapping.SetWord("offProgNr", ch.NewProgramNr);
        mapping.SetFlag("IsFav", ch.Favorites != 0);
        mapping.SetFlag("Locked", ch.Lock);
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
