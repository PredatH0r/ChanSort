//#define DUMP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ChanSort.Api;
using Microsoft.Data.Sqlite;

namespace ChanSort.Loader.Panasonic;

/*
 * Serializer for the 2022/2023 Android based Panasonic LS 500, LX 700 series file format
 *
 * The format uses a directory tree with
 * /hotel.bin (irrelevant content)
 * /mnt/vendor/tvdata/database/tv.db Sqlite database
 * /mnt/vendor/tvdata/database/channel/idtvChannel.bin
 *
 * All statements here are based on observation without confirmation from official sources.
 *
 * The .bin file contains all DVB-S, DVB-T and DVB-C channels that were found in a scan. Channels are sorted by source (DVB-S, -T, -C), TV/radio/data and then by display_number (or channel_index).
 * The .db file may contain a subset of DVB channels, particularly omitting data channels, but also contains additional non-DVB channels.
 * The link between DVB channel records in the .db and .bin file is via a common internal_provider_flag2 value.
 * In the .bin file the ipf2 is a unique value, but multiple .db channels may reference the same .bin channel.
 *
 * In Menu / Channels / Channel Management the list is based on the records from the .db file ordered by display_number.
 * Entering a channel's number on the remote control also seems to use the .db file records and offers selection between channels that start with the same display_number digits (which includes possible duplicates).
 *
 * The TV's EPG list is not ordered by the display_number. It somehow depends on the channel_index in the .db file and the physical order of records in the .bin file.
 * There is some nontransparent regrouping/reordering going on that may result in completely random looking EPG order when the physical records in the .bin are not in the same sequence as the channel_index
 * in the .db file.
 *
 * For zapping the TV seems uses the EPG channel order. Zapping fails when TV and radio channels are mixed. It works for the first part but after alternating TV/radio several times, it will zap back
 * to the first TV channel even if there are further channels of the same type as the currently tuned in channel in the list. Therefore it is highly recommended to have all TV channels first, then all radio channels.
 *
 * At least the initial firmware of these models has a quirks with inconsistent handling of internal_provider_flag2 as int16, uint16 and int32 with wrong sign-extension, causing lookup-failures
 * and duplicate channel records in list. In the .bin file the int32 value 55984 can either be -9552 or 55984 in the .db file (some rows have int16, others uint16 values!). For this reason
 * this code here casts the values down to uint16 to ensure lookups work fine. It's unknown if there can be values > 65535 in the .bin or .db file.
 *
 * The value in the tv.db channel_index is ambiguous because when searching for DVB-C and then DVB-S, both input sources start with channel_index=0, but when searching DVB-S first and then DVB-C,
 * the channel_index sequence is continuous and doesn't reset to 0. There's also duplicate values when the TV puts several channels on the same display_number.
 *
 * In this case of multiple .db channels referencing the same .bin channel the lowest display_number will be used and stored in the .bin channel.
 * When saving a new list, the channel_index will be set to a consecutive sequence following the ordering by display_number.
 *
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
    Radio = 0x04,
    Data = 0x10,
    IsFavorite = 0x0080,
    Deleted = 0x0100, // if really by "user" is uncertain
    Skip = 0x0400,
    CustomProgNr = 0x1000
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  unsafe struct IdtvChannel
  {
    public short U0; // always 1
    public ushort RecordLength; // 60 + length of channel name
    public short U4; // always 6
    public fixed byte U6[3]; // all 00
    public ushort U9; // 0 = sat, 18 = cable ?
    public fixed byte U11[5]; // all 00
    public uint Freq; // Hz for DVB-C/T, kHz for DVB-S
    public uint SymRate; // in Sym/s, like 22000000
    public short U24; // always 100
    public short U26; // always 0
    public short U28; // always 0
    public ushort ProgNr;
    public short Lcn; // maybe?
    public fixed byte U32[2]; // e.g. 0a 01 00 00
    public Flags Flags;
    public fixed byte U38[4]; // 12 07 01 02
    public ushort Tsid;
    public ushort Onid;
    public ushort Sid;
    public fixed byte U48[4];

    public int InternalProviderFlag2; // this is a unique .bin record identifier, used in the .db file's "internal_provider_flag2" to reference the .bin record
                                      
    public fixed byte U56[8];
    //public fixed byte ChannelName[RecordLength - 60]; // pseudo-C# description of variable length channel name UTF8 data at end of structure
  }
  #endregion

  #region tv.db channels table
  /*
   * type: TYPE_PREVIEW, TYPE_OTHER, TYPE_DVB_S, TYPE_DVB_C, TYPE_DVB_T, TYPE_DVB_T2
   * service_type: SERVICE_TYPE_AUDIO_VIDEO, SERVICE_TYPE_AUDIO, SERVICE_TYPE_DATA
   * display_number: program number (entered on remote control)
   * internal_provider_flag1: DVB-C/T: frequency in Hz, DVB-S: freq in kHz
   * internal_provider_flag2: id to link from .db to .bin data record
   * internal_provider_flag3: maybe DVB-S satellite index (17 = Sat #18 in the TV's UI = Astra 19.2E), but also at times 0
   * internal_provider_flag4: symbol rate in sym/sec
   * input_type: 0=cable, 2=sat, ...
   */
  #endregion

  #region BinChannelEntry
  private class BinChannelEntry
  {
    public readonly int Index;
    public readonly IdtvChannel Channel;
    public readonly string Name;
    public readonly int StartOffset;

    public BinChannelEntry(int index, IdtvChannel channel, string name, int startOffset)
    {
      this.Index = index;
      this.Channel = channel;
      this.Name = name;
      this.StartOffset = startOffset;
    }
  }
  #endregion

  private readonly string dbFile;
  private readonly string binFile;

  private byte[] binFileData; // will keep the originally loaded record order as-is, even after saving the file with a different physical record order
  private readonly Dictionary<ushort, BinChannelEntry> binChannelByInternalProviderFlag2 = new();

  private readonly StringBuilder log = new();

  #region ctor()
  public IdtvChannelSerializer(string hotelBin) : base(hotelBin)
  {
    var dir = Path.Combine(Path.GetDirectoryName(hotelBin), "mnt/vendor/tvdata/database");
    dbFile = Path.Combine(dir, "tv.db");
    binFile = Path.Combine(dir, "channel", "idtvChannel.bin");

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
      throw LoaderException.Fail("expected file not found: " + dbFile);
    if (!File.Exists(binFile))
      throw LoaderException.Fail("expected file not found: " + binFile);

    string connString = $"Data Source={this.dbFile};Pooling=False";
    using var db = new SqliteConnection(connString);
    db.Open();
    using var cmd = db.CreateCommand();

    try
    {
      cmd.CommandText = "SELECT count(1) FROM sqlite_master WHERE type = 'table' and name in ('android_metadata', 'channels')";
      var result = Convert.ToInt32(cmd.ExecuteScalar()); // if the database file is corrupted, the execption will be thrown here and not when opening it
      if (result != 2)
        throw LoaderException.Fail("File doesn't contain the expected android_metadata/channels tables");
    }
    catch (SqliteException)
    {
      // when the USB stick is removed without properly ejecting it, the .db file is often corrupted, causing an exception when running the first query
      View.Default.MessageBox(
        "The Panasonic tv.db file in this channel list is corrupted and can't be loaded.\n\n" +
        "After using the Hotel Menu's \"TV to USB\", press HOME / Notifications / your USB stick / Eject.\n" +
        "This will properly finish all write operations so the stick can be unplugged safely without data loss.");
      return;
    }

    this.ReadIdtvChannelsBin();
    this.ReadChannelsFromDatabase(cmd);
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
        throw LoaderException.Fail("Invalid MD5 checksum in " + binFile);
    }

    using var strm = new MemoryStream(binFileData);
    using var r = new BinaryReader(strm);

    r.ReadBytes(2 + 2); // 00 00, 4b 09
    var numRecords = r.ReadUInt16();
    r.ReadBytes(2); // 00 00
    r.ReadBytes(16); // md5
    i = 0;

#if DUMP
    log.AppendLine($"#\tname\tprogNr\tonid-tsid-sid\tflags\tlcn\tipf2");
#endif

    // load data records and store them in the binChannelByInternalProviderFlag2 dictionary
    var structSize = Marshal.SizeOf<IdtvChannel>();
    while (strm.Position + structSize <= binFileData.Length)
    {
      var off = (int)strm.Position;

      // C# trickery to read binary data into a structure
      var bytes = r.ReadBytes(structSize);
      GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
      try
      {
        var chan = Marshal.PtrToStructure<IdtvChannel>(handle.AddrOfPinnedObject());
        var name = Encoding.UTF8.GetString(r.ReadBytes(chan.RecordLength - 60));

        var key = (ushort)chan.InternalProviderFlag2;

        if (this.binChannelByInternalProviderFlag2.TryGetValue(key, out var ch))
          throw LoaderException.Fail($"{binFile} channel records {ch.Index} and {i} have duplicate internal_provider_flag2 value {key}.");

        this.binChannelByInternalProviderFlag2.Add(key, new BinChannelEntry(i, chan, name, off));

#if DUMP
        var progNr = chan.ProgNr;
        log.AppendLine($"{i}\t{name}\t{progNr}\t{chan.Onid}-{chan.Tsid}-{chan.Sid}\t0x{(ushort)chan.Flags:X4}\t{chan.Lcn}\t{chan.InternalProviderFlag2}");
#endif
      }
      finally
      {
        handle.Free();
      }

      ++i;
    }

    if (i < numRecords)
      throw LoaderException.Fail($"idtvChannel contains only {i} data records, but expected {numRecords}");
  }
  #endregion

  #region ReadChannelsFromDatabase()
  private void ReadChannelsFromDatabase(SqliteCommand cmd)
  {
    cmd.CommandText = "select * from channels where type in ('TYPE_DVB_S','TYPE_DVB_C','TYPE_DVB_T','TYPE_DVB_T2') order by _id";
    using var r = cmd.ExecuteReader();

    var cols = new Dictionary<string, int>();
    for (int i = 0, c = r.FieldCount; i < c; i++)
      cols[r.GetName(i)] = i;

    var channelDict = new Dictionary<ushort, ChannelInfo>(); // maps InternalProviderFlag2 of .bin file to Channel object created from .db file

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
        case "TYPE_DVB_C":
          signalSource |= SignalSource.Cable;
          break;
        case "TYPE_DVB_S":
          signalSource |= SignalSource.Sat;
          break;
        case "TYPE_DVB_T":
          signalSource |= SignalSource.Antenna;
          break;
        case "TYPE_DVB_T2":
          signalSource |= SignalSource.Antenna;
          break;
      }

      switch (svcType)
      {
        case "SERVICE_TYPE_AUDIO":
          signalSource |= SignalSource.Radio;
          break;
        case "SERVICE_TYPE_AUDIO_VIDEO":
          signalSource |= SignalSource.Tv;
          break;
        default:
          signalSource |= SignalSource.Data;
          break;
      }

      var ch = new DbChannel(signalSource, id, progNr, name);
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
      ch.InternalProviderFlag2 = (ushort)r.GetInt32(cols["internal_provider_flag2"]); // reference between idtvChannel.bin and tv.db records.
      ch.RecordOrder = ch.InternalProviderFlag2; // r.GetInt32(cols["channel_index"]); // only read for debugging purpose
      ch.Favorites = (Favorites)r.GetByte(cols["favorite"]);

      var list = this.DataRoot.GetChannelList(signalSource);
      if (channelDict.TryGetValue((ushort)ch.InternalProviderFlag2, out var olderChannel))
      {
        log.AppendLine(
          //$"tv.db channel _id {olderChannel.RecordIndex} ({olderChannel.Name}) is overridden by _id {ch.RecordIndex} ({ch.Name}) with same internal_provider_flag2 {ch.InternalProviderFlag2}");
          $"tv.db channel _id {ch.RecordIndex} (#{ch.OldProgramNr} {ch.Name}) is a duplicate of _id {olderChannel.RecordIndex} (#{olderChannel.OldProgramNr} {olderChannel.Name}) with same internal_provider_flag2 {ch.InternalProviderFlag2}");
        //list.RemoveChannel(olderChannel);
      }
      else
        channelDict[(ushort)ch.InternalProviderFlag2] = ch;

      this.DataRoot.AddChannel(list, ch);

      // validate consistency between .db and .bin (multiple .db rows can reference the same .bin record)
      if (!this.binChannelByInternalProviderFlag2.TryGetValue((ushort)ch.InternalProviderFlag2, out var idtvEntry))
        throw LoaderException.Fail($"{list.ShortCaption} channel with _id {ch.RecordIndex} refers to non-existing idtvChannel.bin record with internal_provider_flag2 {ch.InternalProviderFlag2}");
      ValidateChannelData(ch, idtvEntry);
    }
  }
  #endregion

  #region ValidateChannelData()
  private void ValidateChannelData(DbChannel ch, BinChannelEntry entry)
  {
    var chan = entry.Channel;
    var name = entry.Name;
    var i = entry.Index;

    var freq = chan.Freq / 1000;
    if (freq >= 13000)
      freq /= 1000;
    var symRate = chan.SymRate / 1000;

    //var progNr = chan.ProgNr;
    //if (ch.OldProgramNr != progNr) // multiple .db rows with different display_number can reference the same .db row, so skip this check
    //  throw new LoaderException.Fail($"mismatching display_number between tv.db _id {ch.RecordIndex} ({ch.OldProgramNr}) and idtvChannel.bin record {i} ({progNr})");
    if (ch.Name != name)
      throw LoaderException.Fail($"mismatching name between tv.db _id {ch.RecordIndex} ({ch.Name}) and idtvChannel.bin record {i} ({name})");
    if (Math.Abs(ch.FreqInMhz - freq) > 2)
      throw LoaderException.Fail($"mismatching frequency between tv.db _id {ch.RecordIndex} ({ch.FreqInMhz}) and idtvChannel.bin record {i} ({freq})");
    if (Math.Abs(ch.SymbolRate - symRate) > 2)
      throw LoaderException.Fail($"mismatching symbol rate between tv.db _id {ch.RecordIndex} ({ch.SymbolRate}) and idtvChannel.bin record {i} ({symRate})");

    if (ch.Encrypted != ((chan.Flags & Flags.Encrypted) != 0))
      log.AppendLine($"mismatching crypt-flag between tv.db _id {ch.RecordIndex} ({ch.Encrypted}) and idtvChannel.bin record {i}");
    if (ch.Skip != ((chan.Flags & Flags.Skip) != 0)) // it seems running a DVB-C search will alter the "browsable" flag of already existing DVB-S channels
      log.AppendLine($"mismatching browsable-flag between tv.db _id {ch.RecordIndex} ({ch.Skip}) and idtvChannel.bin record {i}");
    if ((ch.Favorites == 0) != ((chan.Flags & Flags.IsFavorite) == 0))
      log.AppendLine($"mismatching favorites-info between tv.db _id {ch.RecordIndex} ({ch.Favorites}) and idtvChannel.bin record {i}");

    ch.AddDebug((ushort)chan.Flags);
  }
  #endregion



  #region Save()
  public override void Save()
  {
    // saving the list requires to:
    // - update fields inside the .bin file data records and physically reorder the records
    // - updating records in the .db file

    GetNewIdtvChannelBinRecordOrder(out var newToOld,  out var newChannelIndexMap, out var channelDict);

    SaveIdtvChannelBin(newToOld, channelDict);
    SaveTvDb(newChannelIndexMap);
  }
  #endregion

  #region GetNewBinFileRecordOrder()
  private void GetNewIdtvChannelBinRecordOrder(out List<ushort> newToOld, out IDictionary<ushort, int> newChannelIndexMap, out Dictionary<ushort,DbChannel> channelMap)
  {
    // detect the smallest new program number (from possibly multiple .db channels) for each specific .bin channel
    var channelDict = new Dictionary<ushort, DbChannel>();
    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var ch in list.Channels)
      {
        if (ch is not DbChannel dbc)
          continue;
        if (!channelDict.TryGetValue((ushort)dbc.InternalProviderFlag2, out var cur) || ch.NewProgramNr >= 0 && ch.NewProgramNr <= cur.NewProgramNr)
          channelDict[(ushort)dbc.InternalProviderFlag2] = dbc;
      }
    }


    // sort list of ipf2 values to get the desired channel order
    newToOld = this.binChannelByInternalProviderFlag2.Keys.ToList();
    newToOld.Sort((a, b) =>
    {
      var entry1 = this.binChannelByInternalProviderFlag2[a];
      var entry2 = this.binChannelByInternalProviderFlag2[b];

      // all sat channels must come first before cable/antenna channels
      var freq1 = entry1.Channel.Freq;
      var freq2 = entry2.Channel.Freq;
      var c = (freq1 < 14000000 ? 0 : 1).CompareTo(freq2 < 14000000 ? 0 : 1); // hack: Sat has values below 14 000 000 (in kHz), Cable/antenna above (in Hz)
      if (c != 0)
        return c;

      channelDict.TryGetValue(a, out var ch1);
      channelDict.TryGetValue(b, out var ch2);

      // existing channels first (TV, radio), non-existing ones last (data)
      if (ch1 == null && ch2 == null)
        return a.CompareTo(b);
      if (ch2 == null)
        return -1;
      if (ch1 == null)
        return +1;

      // group TV/Radio/Data
      var ss1 = GetSignalSource(ch1, a);
      var ss2 = GetSignalSource(ch2, b);
      c = ((int)ss1).CompareTo((int)ss2);
      if (c != 0)
        return c;

      // lower display number first
      c = ch1.NewProgramNr.CompareTo(ch2.NewProgramNr);
      if (c != 0)
        return c;

      // keep previous order
      return a.CompareTo(b); 
    });

    // create reverse mapping
    newChannelIndexMap = new Dictionary<ushort, int>();
    for (int i = 0, c = newToOld.Count; i < c; i++)
      newChannelIndexMap[newToOld[i]] = i;

    channelMap = channelDict;

    SignalSource GetSignalSource(DbChannel channel, ushort internalProviderFlag2)
    {
      if (channel != null)
        return channel.SignalSource;
      var binEntry = this.binChannelByInternalProviderFlag2[internalProviderFlag2];
     
      var flags = binEntry.Channel.Flags;
      if ((flags & Flags.Radio) != 0)
        return SignalSource.Radio;
      if ((flags & Flags.Data) != 0)
        return SignalSource.Data;
      return SignalSource.Tv;
    }
  }
  #endregion

  #region SaveIdtvChannelBin()
  private void SaveIdtvChannelBin(IList<ushort> newToOld, IDictionary<ushort, DbChannel> channelMap)
  {
    UpdateIdtvChannelBinRecords(channelMap);
    var newBin = ReorderBinFileRecords(newToOld);

    // update MD5 checksum
    var md5 = MD5.Create();
    var checksum = md5.ComputeHash(newBin, 8 + 16, newBin.Length - 8 - 16);
    Array.Copy(checksum, 0, newBin, 8, 16);

    File.WriteAllBytes(this.binFile, newBin);
  }
  #endregion

  #region UpdateIdtvChannelBinRecords()
  private void UpdateIdtvChannelBinRecords(IDictionary<ushort, DbChannel> channelMap)
  {
    // in-place update of channel data in the initially loaded binFileData

    var offProgNr = (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.ProgNr));
    var offFlags = (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.Flags));

    var w = new BinaryWriter(new MemoryStream(this.binFileData));

    foreach(var entry in channelMap)
    {
      var dbc = entry.Value;

      if (!this.binChannelByInternalProviderFlag2.TryGetValue(entry.Key, out var binEntry))
        continue;

      // update display_number
      var filePosition = binEntry.StartOffset;
      w.Seek(filePosition + offProgNr, SeekOrigin.Begin);
      //w.Write(ch.NewProgramNr > 0 ? (ushort)ch.NewProgramNr : (ushort)0xFFFE); // deleted channels have -2 / 0xFFFE
      w.Write(dbc.NewProgramNr);


      // update flags
      var off = filePosition + offFlags;
      var flags = BitConverter.ToUInt16(this.binFileData, off);
      if (dbc.Favorites == 0)
        flags = (ushort)(flags & ~(ushort)Flags.IsFavorite);
      else
        flags = (ushort)(flags | (ushort)Flags.IsFavorite);

      if (dbc.Skip)
        flags = (ushort)(flags | (ushort)Flags.Skip);
      else
        flags = (ushort)(flags & ~(ushort)Flags.Skip);

      flags = (ushort)(flags & ~(ushort)Flags.Data); // Sky option channels can transformed from Data to TV and might otherwise be out-of-order in EPG and zapping

      if (dbc.IsDeleted)
        flags |= (ushort)Flags.Deleted;

      flags |= (ushort)Flags.CustomProgNr;

      w.Seek(filePosition + offFlags, SeekOrigin.Begin);
      w.Write(flags);
    }
  
    w.Flush();
  }
  #endregion

  #region ReorderBinFileRecords()
  private byte[] ReorderBinFileRecords(IList<ushort> newToOld)
  {
    using var mem = new MemoryStream(this.binFileData.Length);
    mem.Write(this.binFileData, 0, 8 + 16); // copy header

    foreach (var ipf2 in newToOld)
    {
      // TODO: this only works as long as channel name editing is not supported
      var entry = this.binChannelByInternalProviderFlag2[ipf2];
      var off = entry.StartOffset;
      var recordLen = entry.Channel.RecordLength + 4;
      mem.Write(this.binFileData, off, recordLen);
    }

    mem.Flush();
    return mem.GetBuffer();
  }
  #endregion

  #region SaveTvDb()
  private void SaveTvDb(IDictionary<ushort, int> newChannelIndexMap)
  {
    string connString = $"Data Source={this.dbFile};Pooling=False";
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
    //upd.Parameters.Add("@ipf2", SqliteType.Integer);
    upd.Prepare();

    using var del = db.CreateCommand();
    del.CommandText = "delete from channels where _id=@id";
    del.Parameters.Add("@id", SqliteType.Integer);
    del.Prepare();

    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var ch in list.Channels)
      {
        if (ch is not DbChannel dbc)
          continue;
        if (ch.NewProgramNr < 0 || ch.IsDeleted)
        {
          del.Parameters["@id"].Value = ch.RecordIndex;
          del.ExecuteNonQuery();
        }
        else
        {
          upd.Parameters["@id"].Value = ch.RecordIndex;
          upd.Parameters["@progNr"].Value = ch.NewProgramNr;
          upd.Parameters["@browseable"].Value = !ch.Skip;
          //upd.Parameters["@searchable"].Value = !ch.Hidden;
          upd.Parameters["@locked"].Value = ch.Lock;
          upd.Parameters["@fav"].Value = (int)ch.Favorites;
          upd.Parameters["@recIdx"].Value = newChannelIndexMap[(ushort)dbc.InternalProviderFlag2];
          //upd.Parameters["@ipf2"].Value = (int)(ushort)dbc.InternalProviderFlag2; // fix broken short/ushort/int sign extension
          upd.ExecuteNonQuery();
        }
      }
    }

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