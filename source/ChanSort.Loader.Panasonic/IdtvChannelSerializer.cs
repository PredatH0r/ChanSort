using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using ChanSort.Api;
using Microsoft.Data.Sqlite;

namespace ChanSort.Loader.Panasonic;

/*
 * Serializer for the 2022 Android based Panasonic LS 500, LX 700, ... series file format
 * The format uses a directory tree with
 * /hotel.bin (irrelevant content)
 * /mnt/vendor/tvdata/database/tv.db Sqlite database (probably for the menu, EPG, ...)
 * /mnt/vendor/tvdata/database/channel/idtvChannel.bin (probably for the tuner)
 */
internal class IdtvChannelSerializer : SerializerBase
{
  #region idtvChannel.bin file format

  /*
   The idtvChannel.bin seems to be related to the TV's DVB tuner. 
   It does not contain some streaming related channels that can be found in tv.db, but contains lots of DVB channels that are not includedin tv.bin (probably filtered out there by country settings)
   The data records in the .bin are shown in exactly that order in the TV's menu, so they must be physically ordered by the program number.

   The .bin file starts with:
   00 00 4b 09
   uint numRecords;
   fixed byte md5Chechsum[16];
   IdtvChannel channels[numRecords]

  */

  [Flags]
  enum Flags : ushort
  {
    Encrypted = 0x0002,
    IsFavorite = 0x0080,
    Deleted = 0x0100,
    Skip = 0x0400,
    CustomProgNr = 0x1000
  }

  [StructLayout(LayoutKind.Sequential,Pack=1)]
  unsafe struct IdtvChannel
  {
    public short U0; // always 1
    public short RecordLength; // 60 + length of channel name
    public short U4; // always 6
    public fixed byte U6[3]; // all 00
    public ushort U9; // 0 = sat, 18 = cable ?
    public fixed byte U11[5]; // all 00
    public uint Freq; // Hz for DVB-C/T, kHz for DVB-S
    public uint SymRate; // in Sym/s, like 22000000
    public short U24; // always 100
    public short U26; // always 0
    public short U28; // always 0
    public short ProgNr;
    public short Lcn; // maybe?
    public fixed byte U32[2]; // e.g. 0a 01 00 00
    public Flags Flags;
    public fixed byte U38[4]; // 12 07 01 02
    public short Tsid;
    public short Onid;
    public short Sid;
    public fixed byte U48[4];
    public uint InternalProviderFlag2; // seems like all sat channels are in the 20000-29999 range, dvb-c in 30000-39999
    public fixed byte U56[8];
    //public fixed byte ChannelName[RecordLength - 60]; // pseudo-C# description of variable length channel name UTF8 data at end of structure
  }

  #endregion

  private readonly string dbFile;
  private readonly string binFile;
  private Dictionary<int, ChannelInfo> channelDict = new(); // maps record-index of .bin file to Channel object created from .db file
  private byte[] binFileData;
  private List<int> binFileRecordOffsets = new();

  private readonly StringBuilder log = new();

  #region ctor()
  public IdtvChannelSerializer(string hotelBin) : base(hotelBin)
  {
    var dir = Path.Combine(Path.GetDirectoryName(hotelBin), "mnt/vendor/tvdata/database");
    dbFile = Path.Combine(dir, "tv.db");
    binFile = Path.Combine(dir, "channel", "idtvChannel.bin");

    this.Features.CanSaveAs = false;
    this.Features.FavoritesMode = FavoritesMode.Flags;
    this.Features.DeleteMode = DeleteMode.FlagWithPrNr;

    this.DataRoot.AddChannelList(new ChannelList(SignalSource.Antenna | SignalSource.MaskTvRadioData, "Antenna"));
    this.DataRoot.AddChannelList(new ChannelList(SignalSource.Cable | SignalSource.MaskTvRadioData, "Cable"));
    this.DataRoot.AddChannelList(new ChannelList(SignalSource.Sat | SignalSource.MaskTvRadioData, "Sat"));
    foreach (var list in this.DataRoot.ChannelLists)
    {
      var names = list.VisibleColumnFieldNames;
      names.Remove(nameof(ChannelInfo.Hidden)); // the TV's "hide" function actually works like "skip", only removing it from zapping, but allowing direct number input
      names.Remove(nameof(ChannelInfo.ShortName));
      names.Remove(nameof(ChannelInfo.Satellite));
      names.Remove(nameof(ChannelInfo.PcrPid));
      names.Remove(nameof(ChannelInfo.VideoPid));
      names.Remove(nameof(ChannelInfo.AudioPid));
      names.Remove(nameof(ChannelInfo.Provider));
      names.Add(nameof(ChannelInfo.Debug));
    }
  }
  #endregion

  #region Load()
  public override void Load()
  {
    if (!File.Exists(dbFile))
      throw new FileLoadException("expected file not found: " + dbFile);
    if (!File.Exists(binFile))
      throw new FileLoadException("expected file not found: " + binFile);

    string connString = "Data Source=" + this.dbFile;
    using var db = new SqliteConnection(connString);
    db.Open();
    using var cmd = db.CreateCommand();

    try
    {
      cmd.CommandText = "SELECT count(1) FROM sqlite_master WHERE type = 'table' and name in ('android_metadata', 'channels')";
      var result = Convert.ToInt32(cmd.ExecuteScalar()); // if the database file is corrupted, the execption will be thrown here and not when opening it
      if (result != 2)
        throw new FileLoadException("File doesn't contain the expected android_metadata/channels tables");
    }
    catch (SqliteException)
    {
      // when the USB stick is removed without going through the menu and "safely remove USB", the DB is often corrupted and can't be opened
      View.Default.MessageBox(
        "The Panasonic tv.db file in this channel list is corrupted and can't be loaded.\n\n"+
        "This very often happens when the USB stick is unplugged from the TV without using the TV menu to \"safely remove USB device\" before.\n" +
        "Please export the list again and use this menu before unplugging the stick from the TV.");
      //throw new FileLoadException("Corrupt database file");
      return;
    }

    this.ReadChannelsFromDatabase(cmd);
    this.ReadIdtvChannelsBin();
  }
  #endregion

  #region ReadChannelsFromDatabase()
  private void ReadChannelsFromDatabase(SqliteCommand cmd)
  {
    cmd.CommandText = "select * from channels where type in ('TYPE_DVB_S','TYPE_DVB_C','TYPE_DVB_T','TYPE_DVB_T2')";
    using var r = cmd.ExecuteReader();
      
    var cols = new Dictionary<string, int>();
    for (int i = 0, c = r.FieldCount; i < c; i++)
      cols[r.GetName(i)] = i;

    while (r.Read())
    {
      var id = r.GetInt64(cols["_id"]);
      var type = r.GetString(cols["type"]);
      var svcType = r.GetString(cols["service_type"]);
      var name = r.IsDBNull(cols["display_name"]) ? "" : r.GetString(cols["display_name"]);
      var progNrStr = r.GetString(cols["display_number"]);
      if (!int.TryParse(progNrStr, out var progNr))
        continue;

      SignalSource signalSource = 0;
      switch (type)
      {
        case "TYPE_DVB_C": signalSource |= SignalSource.Cable; break;
        case "TYPE_DVB_S": signalSource |= SignalSource.Sat; break;
        case "TYPE_DVB_T": signalSource |= SignalSource.Antenna; break;
        case "TYPE_DVB_T2": signalSource |= SignalSource.Antenna; break;
      }

      switch (svcType)
      {
        case "SERVICE_TYPE_AUDIO": signalSource |= SignalSource.Radio; break;
        case "SERVICE_TYPE_AUDIO_VIDEO": signalSource |= SignalSource.Tv; break;
        default: signalSource |= SignalSource.Data; break;
      }

      var ch = new ChannelInfo(signalSource, id, progNr, name);
      ch.Lock = r.GetBoolean(cols["locked"]);
      ch.Skip = !r.GetBoolean(cols["browsable"]);
      ch.Hidden = !r.GetBoolean(cols["searchable"]);
      ch.Encrypted = r.GetBoolean(cols["scrambled"]);

      ch.OriginalNetworkId = r.GetInt32(cols["original_network_id"]);
      ch.TransportStreamId = r.GetInt32(cols["transport_stream_id"]);
      ch.ServiceId = r.GetInt32(cols["service_id"]);
      ch.FreqInMhz = r.GetInt64(cols["internal_provider_flag1"]) / 1000; // for DVB-S it is in MHz, for DVB-C/T it is in kHz
      if (ch.FreqInMhz >= 13000)
        ch.FreqInMhz /= 1000;
      ch.SymbolRate = r.GetInt32(cols["internal_provider_flag4"]) / 1000;
      if ((signalSource & SignalSource.Radio) != 0)
        ch.ServiceTypeName = "Radio";
      else if ((signalSource & SignalSource.Tv) != 0)
        ch.ServiceTypeName = r.GetBoolean(cols["is_hd"]) ? "HD-TV" : "SD-TV";
      else
        ch.ServiceTypeName = "Data";
      ch.RecordOrder = r.GetInt32(cols["channel_index"]); // record index in the idtvChannel.bin file
      ch.Favorites = (Favorites)r.GetByte(cols["favorite"]);

      var list = this.DataRoot.GetChannelList(signalSource);
      this.DataRoot.AddChannel(list, ch);

      if (channelDict.TryGetValue(ch.RecordOrder, out var otherChannel))
        throw new FileLoadException($"tv.db channel _ids {otherChannel.RecordIndex} ({otherChannel.Name}) and {ch.RecordIndex} ({ch.Name}) both reference same idtvChannel.bin record {ch.RecordOrder}\n"+ 
                                    "Please make sure to search satellite channels first and cable/antenna afterwards.");
      channelDict.Add(ch.RecordOrder, ch);
    }
  }
  #endregion

  #region ReadIdtvChannelsBin()
  private void ReadIdtvChannelsBin()
  {
    this.binFileData = File.ReadAllBytes(this.binFile);

    // verify MD5 checksum
    var md5 = MD5.Create();
    var hash = md5.ComputeHash(binFileData, 24, binFileData.Length - 24);
    int i;
    for (i = 0; i < 16; i++)
    {
      if (binFileData[8 + i] != hash[i])
        throw new FileLoadException("Invalid MD5 checksum in " + binFile);
    }

    using var strm = new MemoryStream(binFileData);
    using var r = new BinaryReader(strm);

    r.ReadBytes(2 + 2); // 00 00, 4b 09
    var numRecords = r.ReadUInt16();
    r.ReadBytes(2); // 00 00
    r.ReadBytes(16); // md5
    i = 0;

    log.AppendLine($"#\tname\tprogNr\tonid-tsid-sid\tflags\tlcn\tipf2");

    var structSize = Marshal.SizeOf<IdtvChannel>();
    while (strm.Position + structSize <= binFileData.Length)
    {
      var off = strm.Position;
      binFileRecordOffsets.Add((int)off);
        
      // C# trickery to read binary data into a structure
      var bytes = r.ReadBytes(structSize);
      GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
      var chan = Marshal.PtrToStructure<IdtvChannel>(handle.AddrOfPinnedObject());
      handle.Free();

      var freq = chan.Freq / 1000;
      if (freq >= 13000)
        freq /= 1000;
      var symRate = chan.SymRate / 1000;
      var progNr = chan.ProgNr;
      var name = Encoding.UTF8.GetString(r.ReadBytes(chan.RecordLength - 60));

      log.AppendLine($"{i}\t{name}\t{progNr}\t{chan.Onid}-{chan.Tsid}-{chan.Sid}\t{(ushort)chan.Flags:X4}\t{chan.Lcn}\t{chan.InternalProviderFlag2}");

      if (channelDict.TryGetValue(i, out var ch))
      {
        if (ch.OldProgramNr != progNr)
          throw new FileLoadException($"mismatching program_number between tv.db _id {ch.RecordIndex} ({ch.OldProgramNr}) and idtvChannel.bin record {i} ({progNr})");
        if (ch.Name != name)
          throw new FileLoadException($"mismatching name between tv.db _id {ch.RecordIndex} ({ch.Name}) and idtvChannel.bin record {i} ({name})");
        if (Math.Abs(ch.FreqInMhz - freq) > 2)
          throw new FileLoadException($"mismatching frequency between tv.db _id {ch.RecordIndex} ({ch.FreqInMhz}) and idtvChannel.bin record {i} ({freq})");
        if (Math.Abs(ch.SymbolRate - symRate) > 2)
          throw new FileLoadException($"mismatching symbol rate between tv.db _id {ch.RecordIndex} ({ch.SymbolRate}) and idtvChannel.bin record {i} ({symRate})");

        if (ch.Encrypted != ((chan.Flags & Flags.Encrypted) != 0))
          throw new FileLoadException($"mismatching crypt-flag between tv.db _id {ch.RecordIndex} ({ch.Encrypted}) and idtvChannel.bin record {i}");
        if (ch.Skip != ((chan.Flags & Flags.Skip) != 0))
          throw new FileLoadException($"mismatching browsable-flag between tv.db _id {ch.RecordIndex} ({ch.Skip}) and idtvChannel.bin record {i}");
        if ((ch.Favorites == 0) != ((chan.Flags & Flags.IsFavorite) == 0))
          throw new FileLoadException($"mismatching favorites-info between tv.db _id {ch.RecordIndex} ({ch.Favorites}) and idtvChannel.bin record {i}");

        ch.AddDebug((ushort)chan.Flags);
      }

      ++i;
    }

    if (i < numRecords)
      throw new FileLoadException($"idtvChannel contains only {i} data records, but expected {numRecords}");

    // make sure no channel from tv.db refers to a record_index that does not exist in idtvChannel.bin
    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var ch in list.Channels)
      {
        if (ch.RecordOrder < 0 || ch.RecordOrder >= numRecords)
          throw new FileLoadException($"{list.ShortCaption} channel with _id {ch.RecordIndex} refers to non-existing index {ch.RecordOrder} in idtvChannel.bin");
      }
    }
  }
  #endregion



  #region Save()
  public override void Save(string tvOutputFile)
  {
    // saving the list requires to:
    // - update fields inside the data records and physically reorder the records
    // - updating records in the .db file
    // - updating all channel record indexes in this loader's internal data in channelDict, binFileRecordOffsets and channelInfo.RecordOrder for consecutive save operations to work

    GetNewIdtvChannelBinRecordOrder(out var newToOld, out var oldToNew);

    SaveIdtvChannelBin(newToOld);
    SaveTvDb(oldToNew);
  }
  #endregion

  #region GetNewBinFileRecordOrder()
  private void GetNewIdtvChannelBinRecordOrder(out List<int> newToOld, out IDictionary<int, int> oldToNew)
  {
    // create forward mapping by sorting a list that contains old record numbers. The index of the list is the new record number.
    newToOld = new List<int>(binFileRecordOffsets.Count);
    for (int i = 0, c=this.binFileRecordOffsets.Count; i<c; i++)
      newToOld.Add(i);

    var offFreq = (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.Freq));
    newToOld.Sort((a, b) =>
    {
      // all sat channels must come first before cable/antenna channels
      var freq1 = BitConverter.ToUInt32(this.binFileData, binFileRecordOffsets[a] + offFreq);
      var freq2 = BitConverter.ToUInt32(this.binFileData, binFileRecordOffsets[b] + offFreq);
      var c = (freq1 < 14000000 ? 0 : 1).CompareTo(freq2 < 14000000 ? 0 : 1); // hack: Sat has values below 14 000 000 (in kHz), Cable/antenna above (in Hz)
      if (c != 0)
        return c;

      this.channelDict.TryGetValue(a, out var ch1);
      this.channelDict.TryGetValue(b, out var ch2);
      if (ch1 == null && ch2 == null)
        return a.CompareTo(b);
      if (ch2 == null)
        return -1;
      if (ch1 == null)
        return +1;

      c = ((int)ch1.SignalSource).CompareTo((int)ch2.SignalSource); // group TV/Radio/Data
      if (c != 0)
        return c;
      c = ch1.NewProgramNr.CompareTo(ch2.NewProgramNr);
      if (c != 0)
        return c;

      return a.CompareTo(b); // default keeps old order
    });

    // create reverse mapping
    oldToNew = new Dictionary<int, int>(newToOld.Count);
    for (int i = 0; i < newToOld.Count; i++)
      oldToNew[newToOld[i]] = i;
  }
  #endregion

  #region SaveIdtvChannelBin()
  private void SaveIdtvChannelBin(IList<int> newToOld)
  {
    UpdateIdtvChannelBinRecords();
    ReorderBinFileRecords(newToOld);

    // update MD5 checksum
    var md5 = MD5.Create();
    var checksum = md5.ComputeHash(binFileData, 8 + 16, binFileData.Length - 8 - 16);
    Array.Copy(checksum, 0, binFileData, 8, 16);

    File.WriteAllBytes(binFile, binFileData);
  }
  #endregion
  
  #region UpdateIdtvChannelBinRecords()
  private void UpdateIdtvChannelBinRecords()
  {
    // in-place update of the old .bin file data

    var offProgNr = (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.ProgNr));
    var offFlags = (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.Flags));

    var w = new BinaryWriter(new MemoryStream(this.binFileData));

    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var ch in list.Channels)
      {
        if (ch.IsProxy)
          continue;
        var filePosition = this.binFileRecordOffsets[ch.RecordOrder];
        w.Seek(filePosition + offProgNr, SeekOrigin.Begin);
        //w.Write(ch.NewProgramNr > 0 ? (ushort)ch.NewProgramNr : (ushort)0xFFFE); // deleted channels have -2 / 0xFFFE
        w.Write(ch.NewProgramNr);


        // update flags
        var off = filePosition + offFlags;
        var flags = BitConverter.ToUInt16(this.binFileData, off);
        if (ch.Favorites == 0)
          flags = (ushort)(flags & ~(ushort)Flags.IsFavorite);
        else
          flags = (ushort)(flags | (ushort)Flags.IsFavorite);
        
        if (ch.Skip)
          flags = (ushort)(flags | (ushort)Flags.Skip);
        else
          flags = (ushort)(flags & ~(ushort)Flags.Skip);

        if (ch.IsDeleted)
          flags |= (ushort)Flags.Deleted;

        flags |= (ushort)Flags.CustomProgNr;
        w.Seek(filePosition + offFlags, SeekOrigin.Begin);
        w.Write(flags);
      }
    }

    w.Flush();
  }
  #endregion

  #region ReorderBinFileRecords()
  private void ReorderBinFileRecords(IList<int> newToOld)
  {
    using var mem = new MemoryStream(this.binFileData.Length);
    mem.Write(binFileData, 0, 8 + 16); // copy header

    var newOffsets = new List<int>(newToOld.Count);
    foreach (var oldIndex in newToOld)
    {
      newOffsets.Add((int)mem.Position);

      // TODO: this only works as long as channel name editing is not supported
      var off = binFileRecordOffsets[oldIndex];
      var recordLen = BitConverter.ToInt16(binFileData, off + (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.RecordLength))) + 4;
      mem.Write(binFileData, off, recordLen);
    }
    mem.Flush();
    binFileRecordOffsets = newOffsets;

    binFileData = new byte[mem.Length];
    Array.Copy(mem.GetBuffer(), 0, binFileData, 0, mem.Length);
  }
  #endregion

  #region SaveTvDb()
  private void SaveTvDb(IDictionary<int, int> oldToNew)
  {
    string connString = "Data Source=" + this.dbFile;
    using var db = new SqliteConnection(connString);
    db.Open();

    using var trans = db.BeginTransaction();

    using var upd = db.CreateCommand();
    upd.CommandText = "update channels set display_number=@progNr, browsable=@browseable, locked=@locked, favorite=@fav, channel_index=@recIdx where _id=@id"; // searchable=@searchable, 
    upd.Parameters.Add("@id", SqliteType.Integer);
    upd.Parameters.Add("@progNr", SqliteType.Text);
    upd.Parameters.Add("@browseable", SqliteType.Integer);
    //upd.Parameters.Add("@searchable", SqliteType.Integer);
    upd.Parameters.Add("@locked", SqliteType.Integer);
    upd.Parameters.Add("@fav", SqliteType.Integer);
    upd.Parameters.Add("@recIdx", SqliteType.Integer);
    upd.Prepare();

    using var del = db.CreateCommand();
    del.CommandText = "delete from channels where _id=@id";
    del.Parameters.Add("@id", SqliteType.Integer);
    del.Prepare();

    var newChannelDict = new Dictionary<int, ChannelInfo>();
    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var ch in list.Channels)
      {
        if (ch.IsProxy)
          continue;
        if (ch.NewProgramNr < 0 || ch.IsDeleted)
        {
          del.Parameters["@id"].Value = ch.RecordIndex;
          del.ExecuteNonQuery();
        }
        else
        {
          var newRecordIndex = oldToNew[ch.RecordOrder];

          upd.Parameters["@id"].Value = ch.RecordIndex;
          upd.Parameters["@progNr"].Value = ch.NewProgramNr;
          upd.Parameters["@browseable"].Value = !ch.Skip;
          //upd.Parameters["@searchable"].Value = !ch.Hidden;
          upd.Parameters["@locked"].Value = ch.Lock;
          upd.Parameters["@fav"].Value = (int)ch.Favorites;
          upd.Parameters["@recIdx"].Value = newRecordIndex;
          upd.ExecuteNonQuery();

          ch.RecordOrder = newRecordIndex;

          newChannelDict.Add(newRecordIndex, ch);
        }
      }
    }
    this.channelDict = newChannelDict;

    trans.Commit();
  }
  #endregion



  #region GetDataFilePaths()
  public override IEnumerable<string> GetDataFilePaths()
  {
    // return the list of files where ChanSort will create a .bak copy
    return new[] { dbFile, dbFile + "-shm", dbFile + "-wal", binFile };
  }
  #endregion

  #region GetFileInformation()
  public override string GetFileInformation()
  {
    return base.GetFileInformation() + "\n\n\n" + this.log;
  }
  #endregion
}