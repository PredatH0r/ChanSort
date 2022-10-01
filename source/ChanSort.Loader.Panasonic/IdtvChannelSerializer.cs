using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ChanSort.Api;
using Microsoft.Data.Sqlite;

namespace ChanSort.Loader.Panasonic
{
  internal class IdtvChannelSerializer : SerializerBase
  {
    #region idtvChannel.bin file format

    /*
     The idtvChannel.bin seems to be related to the TV's DVB tuner. 
     It does not contain some streaming related channels that can be found in tv.db, but contains lots of DVB channels that are not includedin tv.bin (probably filtered out there by country settings)
     When changing program numbers through the TV's menu, the data records in the .bin file get physically reordered to match the logical order.
    */

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct IdtvChannel
    {
      public short U0; // always 1
      public short RecordLength; // 60 + length of channel name
      public short U4; // always 6
      public fixed byte U6[10]; // all 00
      public uint Freq; // Hz for DVB-C/T, kHz for DVB-S
      public uint SymRate; // in Sym/s, like 22000000
      public short U24; // always 100
      public short U26; // always 0
      public short U28; // always 0
      public short ProgNr;
      public fixed byte U32[10]; // probably 5 ushorts with unknown meaning
      public short Tsid;
      public short Onid;
      public short Sid;
      public fixed byte U48[16];
      //public byte[] ChannelName;
    }

    #endregion

    class ChannelDictEntry
    {
      public ChannelInfo Channel;
      public long FilePosition;
    }

    private readonly string dbFile;
    private readonly string binFile;
    private readonly Dictionary<long, ChannelDictEntry> channelDict = new();

    public IdtvChannelSerializer(string inputFile) : base(inputFile)
    {
      dbFile = inputFile;
      binFile = Path.Combine(Path.GetDirectoryName(dbFile), "channel", "idtvChannel.bin");

      this.Features.CanSaveAs = false;
      this.Features.FavoritesMode = FavoritesMode.Flags;

      this.DataRoot.AddChannelList(new ChannelList(SignalSource.Antenna | SignalSource.MaskTvRadioData, "Antenna"));
      this.DataRoot.AddChannelList(new ChannelList(SignalSource.Cable | SignalSource.MaskTvRadioData, "Cable"));
      this.DataRoot.AddChannelList(new ChannelList(SignalSource.Sat | SignalSource.MaskTvRadioData, "Sat"));
      foreach (var list in this.DataRoot.ChannelLists)
      {
        var names = list.VisibleColumnFieldNames;
        names.Remove(nameof(ChannelInfo.ShortName));
        names.Remove(nameof(ChannelInfo.Satellite));
        names.Remove(nameof(ChannelInfo.PcrPid));
        names.Remove(nameof(ChannelInfo.VideoPid));
        names.Remove(nameof(ChannelInfo.AudioPid));
        names.Remove(nameof(ChannelInfo.Provider));
      }
    }

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
      cmd.CommandText = "SELECT count(1) FROM sqlite_master WHERE type = 'table' and name in ('android_metadata', 'channels')";
      if (Convert.ToInt32(cmd.ExecuteScalar()) != 2)
        throw new FileLoadException("File doesn't contain the expected android_metadata/channels tables");

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

        ch.OriginalNetworkId = r.GetInt16(cols["original_network_id"]);
        ch.TransportStreamId = r.GetInt16(cols["transport_stream_id"]);
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

        channelDict.Add(ch.RecordOrder, new ChannelDictEntry() { Channel = ch });
      }
    }
    #endregion

    #region ReadIdtvChannelsBin()
    private void ReadIdtvChannelsBin()
    {
      // verify MD5 checksum
      var data = File.ReadAllBytes(this.binFile);
      var md5 = MD5.Create();
      var hash = md5.ComputeHash(data, 24, data.Length - 24);
      int i;
      for (i = 0; i < 16; i++)
      {
        if (data[8 + i] != hash[i])
          throw new FileLoadException("Invalid MD5 checksum in " + binFile);
      }


      var strm = new MemoryStream(data);
      using var r = new BinaryReader(strm);

      r.ReadBytes(2 + 2); // 00 00, 4b 09
      var numRecords = r.ReadUInt16();
      r.ReadBytes(2); // 00 00
      r.ReadBytes(16); // md5
      i = 0;
      while (strm.Position + 4 <= data.Length)
      {
        var off = strm.Position;

        r.ReadBytes(2);
        var len = r.ReadUInt16();
        r.ReadBytes(2 + 10);
        var freq = r.ReadUInt32() / 1000;
        if (freq >= 13000)
          freq /= 1000;
        var symRate = r.ReadUInt32() / 1000;
        r.ReadBytes(2 + 2 + 2);
        var progNr = r.ReadUInt16();
        r.ReadBytes(4 + 2 + 2 + 2 + 2 + 2 + 2 + 2 + 2 + 4 + 8);
        var name = Encoding.UTF8.GetString(r.ReadBytes(len - 60));

        //if (progNr != i + 1)
        //  throw new FileLoadException($"progNr {progNr} inside idtvChannel.bin data record #{i}");

        if (!channelDict.TryGetValue(i, out var entry))
        {
          //throw new FileLoadException($"no data record in tv.db with record_index {i}");
         
        }
        else
        {
          entry.FilePosition = off;

          var ch = entry.Channel;
          if (ch.OldProgramNr != progNr)
            throw new FileLoadException($"mismatching program_number between tv.db _id {ch.RecordIndex} ({ch.OldProgramNr}) and idtvChannel.bin record {i} ({progNr})");
          if (ch.Name != name)
            throw new FileLoadException($"mismatching name between tv.db _id {ch.RecordIndex} ({ch.Name}) and idtvChannel.bin record {i} ({name})");
          if (Math.Abs(ch.FreqInMhz - freq) > 2)
            throw new FileLoadException($"mismatching frequency between tv.db _id {ch.RecordIndex} ({ch.FreqInMhz}) and idtvChannel.bin record {i} ({freq})");
          if (Math.Abs(ch.SymbolRate - symRate) > 2)
            throw new FileLoadException($"mismatching symbol rate between tv.db _id {ch.RecordIndex} ({ch.SymbolRate}) and idtvChannel.bin record {i} ({symRate})");
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


    public override IEnumerable<string> GetDataFilePaths()
    {
      return new[] { dbFile, binFile };
    }


    public override void Save(string tvOutputFile)
    {
      string connString = "Data Source=" + this.dbFile;
      using var db = new SqliteConnection(connString);
      db.Open();

      var data = File.ReadAllBytes(binFile);
      var w = new BinaryWriter(new MemoryStream(data));

      using var trans = db.BeginTransaction();
      
      using var upd = db.CreateCommand();
      upd.CommandText = "update channels set display_number=@progNr, browsable=@browseable, searchable=@searchable, locked=@locked, favorite=@fav where _id=@id";
      upd.Parameters.Add("@id", SqliteType.Integer);
      upd.Parameters.Add("@progNr", SqliteType.Text);
      upd.Parameters.Add("@browseable", SqliteType.Integer);
      upd.Parameters.Add("@searchable", SqliteType.Integer);
      upd.Parameters.Add("@locked", SqliteType.Integer);
      upd.Parameters.Add("@fav", SqliteType.Integer);
      upd.Prepare();

      using var del = db.CreateCommand();
      del.CommandText = "delete from channels where _id=@id";
      del.Parameters.Add("@id", SqliteType.Integer);
      del.Prepare();

      var offProgNr = (int)Marshal.OffsetOf<IdtvChannel>(nameof(IdtvChannel.ProgNr));
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
            upd.Parameters["@id"].Value = ch.RecordIndex;
            upd.Parameters["@progNr"].Value = ch.NewProgramNr;
            upd.Parameters["@browseable"].Value = !ch.Skip;
            upd.Parameters["@searchable"].Value = !ch.Hidden;
            upd.Parameters["@locked"].Value = ch.Lock;
            upd.Parameters["@fav"].Value = (int)ch.Favorites;
            upd.ExecuteNonQuery();

            var entry = channelDict[ch.RecordOrder];
            w.Seek((int)entry.FilePosition + offProgNr, SeekOrigin.Begin);
            w.Write((ushort)ch.NewProgramNr);
          }
        }
      }
      trans.Commit();

      w.Flush();

      // TODO reorder data records in .bin file based on progNr

      // update MD5 checksum
      var md5 = MD5.Create();
      var checksum = md5.ComputeHash(data, 8 + 16, data.Length - 8 - 16);
      Array.Copy(checksum, 0, data, 8, 16);

      File.WriteAllBytes(binFile, data);
    }
  }
}
