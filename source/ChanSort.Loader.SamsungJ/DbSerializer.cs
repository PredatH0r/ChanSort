using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.SamsungJ
{
  /// <summary>
  /// Loader for Samsung J/K/M/N/R/Q series .zip files (2015 - 2019+)
  /// </summary>
  class DbSerializer : SerializerBase
  {
    private readonly Dictionary<long, DbChannel> channelById = new Dictionary<long, DbChannel>();
    private readonly Dictionary<ChannelList, string> dbPathByChannelList = new Dictionary<ChannelList, string>();
    private readonly List<string> tableNames = new List<string>();

    private enum FileType { Unknown, SatDb, ChannelDbDvb, ChannelDbAnalog, ChannelDbIp }

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.SupportedFavorites = Favorites.A | Favorites.B | Favorites.C | Favorites.D | Favorites.E;
      this.Features.SortedFavorites = true;
      this.Features.AllowGapsInFavNumbers = true;
    }
    #endregion


    #region Load()
    public override void Load()
    {
      try
      {
        this.UnzipFileToTempFolder();
        if (File.Exists(this.TempPath + "\\sat"))
        {
          try
          {
            using (var conn = new SQLiteConnection("Data Source=" + this.TempPath + "\\sat"))
            {
              conn.Open();
              this.ReadSatDatabase(conn);
            }
          }
          catch
          {
          }
        }

        var files = Directory.GetFiles(this.TempPath, "*.");
        if (files.Length == 0)
          throw new FileLoadException("The Samsung .zip channel list archive does not contain any supported files.");

        foreach (var filePath in files)
        {
          var filename = Path.GetFileName(filePath) ?? "";
          if (filename.StartsWith("vconf_"))
            continue;
          try
          {
            using (var conn = new SQLiteConnection("Data Source=" + filePath))
            {
              FileType type;
              conn.Open();
              using (var cmd = conn.CreateCommand())
              {
                this.RepairCorruptedDatabaseImage(cmd);
                type = this.DetectFileType(cmd);
              }

              switch (type)
              {
                case FileType.SatDb: 
                  break;
                case FileType.ChannelDbAnalog:
                case FileType.ChannelDbDvb:
                case FileType.ChannelDbIp:
                  ReadChannelDatabase(conn, filePath, type);
                  break;
              }
            }
          }
          catch
          {
          }
        }
      }
      finally
      {
        // force closing the file and releasing the locks
        SQLiteConnection.ClearAllPools(); 
        GC.Collect();
        GC.WaitForPendingFinalizers();
      }
    }
    #endregion

    #region RepairCorruptedDatabaseImage()
    private void RepairCorruptedDatabaseImage(SQLiteCommand cmd)
    {
      cmd.CommandText = "REINDEX";
      cmd.ExecuteNonQuery();
    }
    #endregion

    #region DetectFileType()
    private FileType DetectFileType(SQLiteCommand cmd)
    {
      this.tableNames.Clear();
      cmd.CommandText = "select name from sqlite_master where type='table'";
      using var r = cmd.ExecuteReader();
      while (r.Read())
        this.tableNames.Add(r.GetString(0).ToUpper());

      if (tableNames.Contains("SAT") && tableNames.Contains("SAT_TP"))
        return FileType.SatDb;

      if (tableNames.Contains("CHNL") && tableNames.Contains("SRV") && tableNames.Contains("SRV_DVB"))
        return FileType.ChannelDbDvb;

      if (tableNames.Contains("CHNL") && tableNames.Contains("SRV") && tableNames.Contains("SRV_ANL"))
        return FileType.ChannelDbAnalog;

      if (tableNames.Contains("CHNL") && tableNames.Contains("SRV") && tableNames.Contains("SRV_IP"))
        return FileType.ChannelDbIp;

      return FileType.Unknown;
    }
    #endregion

    #region ReadSatDatabase()
    private void ReadSatDatabase(SQLiteConnection conn)
    {
      using (var cmd = conn.CreateCommand())
      {
        this.RepairCorruptedDatabaseImage(cmd);
        this.ReadSatellites(cmd);
        this.ReadTransponders(cmd);
      }
    }
    #endregion

    #region ReadSatellites()
    private void ReadSatellites(SQLiteCommand cmd)
    {
      cmd.CommandText = "select distinct satId, cast(satName as blob), satPos, satDir from SAT";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          Satellite sat = new Satellite(r.GetInt32(0));
          int pos = Math.Abs(r.GetInt32(2));
          // 171027 - ohuseyinoglu: For user-defined satellites, the direction may be -1
          // (and not just 1 for "E", 0 for "W")
          int dir = r.GetInt32(3);
          sat.OrbitalPosition = $"{pos / 10}.{pos % 10}{(dir == 1 ? "E" : dir == 0 ? "W" : "")}";
          sat.Name = ReadUtf16(r, 1);
          this.DataRoot.AddSatellite(sat);
        }
      }
    }
    #endregion

    #region ReadTransponders()
    private void ReadTransponders(SQLiteCommand cmd)
    {
      cmd.CommandText = "select satId, tpFreq, tpPol, tpSr, tpId from SAT_TP";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          // 171027 - ohuseyinoglu: tpId is the primary key of this table, we should be able to use it as "id/dict. index"
          // It will also be our lookup value for the CHNL table
          int id = r.GetInt32(4);
          Transponder tp = new Transponder(id);
          tp.FrequencyInMhz = (decimal)r.GetInt32(1) / 1000;
          tp.Number = id;
          tp.Polarity = r.GetInt32(2) == 0 ? 'H' : 'V';
          tp.Satellite = this.DataRoot.Satellites.TryGet(r.GetInt32(0));
          tp.SymbolRate = r.GetInt32(3);
          this.DataRoot.AddTransponder(tp.Satellite, tp);
        }
      }
    }
    #endregion


    #region ReadChannelDatabase()
    private void ReadChannelDatabase(SQLiteConnection conn, string dbPath, FileType fileType)
    {
      this.channelById.Clear();
      using (var cmd = conn.CreateCommand())
      {
        var providers = fileType == FileType.ChannelDbDvb ? this.ReadProviders(cmd) : null;
        var channelList = this.ReadChannels(cmd, dbPath, providers, fileType);
        this.ReadFavorites(cmd);
        this.dbPathByChannelList.Add(channelList, dbPath);
      }
    }
    #endregion

    #region ReadProviders()
    private Dictionary<long, string> ReadProviders(SQLiteCommand cmd)
    {
      var dict = new Dictionary<long, string>();
      try
      {
        cmd.CommandText = "select provId, cast(provName as blob) from PROV";
        using (var r = cmd.ExecuteReader())
        {
          while (r.Read())
            dict.Add(r.GetInt64(0), ReadUtf16(r, 1));
        }
      }
      catch
      {
      }
      return dict;
    }
    #endregion

    #region ReadChannels()
    private ChannelList ReadChannels(SQLiteCommand cmd, string dbPath, Dictionary<long, string> providers, FileType fileType)
    {
      var signalSource = DetectSignalSource(cmd, fileType);

      string name = Path.GetFileName(dbPath);
      ChannelList channelList = this.CreateChannelList(signalSource, name);
      string table = fileType == FileType.ChannelDbDvb ? "SRV_DVB" : fileType == FileType.ChannelDbAnalog ? "SRV_ANL" : "SRV_IP";
      List<string> fieldNames = new List<string> { 
                            "chType", "chNum", "freq", // CHNL
                            "SRV.srvId", "major", "progNum", "cast(srvName as blob) srvName", "srvType", "hidden", "scrambled", "lockMode", "numSel", "elim" // SRV
                            };
      if (fileType == FileType.ChannelDbDvb)
        fieldNames.AddRange(new[] {"onid", "tsid", "vidPid", "provId", "cast(shrtSrvName as blob) shrtSrvName", "lcn"}); // SRV_DVB

      var sql = this.BuildQuery(table, fieldNames);
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        int prevNr = 0;
        while (r.Read())
        {
          if (r.GetInt32(fields["elim"]) != 0)
            continue;

          // 171027 - ohuseyinoglu: With our change in transponder indexing, we can directly look it up by "chNum" now!
          var tp = this.DataRoot.Transponder.TryGet(r.GetInt32(1));
          // ... and get the satellite from that transponder - if set
          // Note that we can have channels from multiple satellites in the same list, so this is a loop variable now
          var sat = tp?.Satellite;
          var channel = new DbChannel(r, fields, this.DataRoot, providers, sat, tp);

          if (channel.OldProgramNr == prevNr) // when there is a SRV_EXT_APP table in the database, the service with the highest ext_app "recState" takes priority
            continue;

          this.DataRoot.AddChannel(channelList, channel);
          this.channelById.Add(channel.RecordIndex, channel);
          prevNr = channel.OldProgramNr;
        }
      }

      this.DataRoot.AddChannelList(channelList);
      return channelList;
    }
    #endregion

    private ChannelList CreateChannelList(SignalSource signalSource, string name)
    {
      var list = new ChannelList(signalSource, name);
      if ((list.SignalSource & SignalSource.IP) != 0)
      {
        list.VisibleColumnFieldNames = new List<string>
        {
          "OldPosition", "Position", "PrNr", "Name", "Favorites", "SymbolRate"
        };
      }
      return list;
    }

    #region DetectSignalSource()
    private static SignalSource DetectSignalSource(SQLiteCommand cmd, FileType fileType)
    {
      if (fileType == FileType.ChannelDbIp)
        return SignalSource.IP|SignalSource.Digital;
      var signalSource = fileType == FileType.ChannelDbAnalog ? SignalSource.Analog : SignalSource.Digital;
      cmd.CommandText = "select distinct chType from CHNL";
      using (var r = cmd.ExecuteReader())
      {
        if (r.Read())
        {
          var ss = ChTypeToSignalSource(r.GetInt32(0));
          if (ss != 0)
            signalSource = ss;
        }
      }
      return signalSource;
    }

    #endregion

    #region ChTypeToSignalSource()
    internal static SignalSource ChTypeToSignalSource(int chType)
    {
      switch (chType)
      {
        case 1: return SignalSource.AnalogT;
        case 2: return SignalSource.DvbT;
        case 3: return SignalSource.AnalogC;
        case 4: return SignalSource.DvbC;
        case 7: return SignalSource.DvbS;
        default: return 0;
      }      
    }
    #endregion

    #region BuildQuery()
    private string BuildQuery(string table, IList<string> fieldNames)
    {
      string sql = "select ";
      for (int i = 0; i < fieldNames.Count; i++)
      {
        if (i > 0)
          sql += ",";
        sql += fieldNames[i];
      }
      sql += " from " + table + " inner join SRV on SRV.srvId="+table+".srvId inner join CHNL on CHNL.chId=SRV.chId";

      if (this.tableNames.Contains("SRV_EXT_APP")) // in format 1352.0 there are duplicate "major" values in SRV and this recState seems to be the only difference
        sql += " inner join SRV_EXT_APP on SRV_EXT_APP.srvId=SRV.srvId order by SRV.major, ifnull(SRV_EXT_APP.recState,0) desc";

      return sql;
    }
    #endregion

    #region GetFieldMap()
    private IDictionary<string, int> GetFieldMap(IList<string> fieldNames)
    {
      Dictionary<string, int> field = new Dictionary<string, int>();
      for (int i = 0; i < fieldNames.Count; i++)
      {
        var idx = fieldNames[i].LastIndexOf(' ') + 1;
        field[fieldNames[i].Substring(idx)] = i;
      }

      return field;
    }
    #endregion

    #region ReadFavorites()
    private void ReadFavorites(SQLiteCommand cmd)
    {
      cmd.CommandText = "select srvId, fav, pos from SRV_FAV";
      var r = cmd.ExecuteReader();
      int favPosAdjust = tableNames.Contains("SRV_EXT_APP") ? 0 : 1;
      while (r.Read())
      {
        var channel = this.channelById.TryGet(r.GetInt64(0));
        if (channel == null) 
          continue;
        int fav = r.GetInt32(1) - 1; // fav values start with 1 in the table
        int pos = r.GetInt32(2) + favPosAdjust;     // pos values start with 0 or 1
        if (pos >= 0)
        {
          channel.Favorites |= (Favorites) (1 << fav);
          channel.OldFavIndex[fav] = pos;
        }
      }
    }
    #endregion

    #region ReadUtf16()
    internal static string ReadUtf16(SQLiteDataReader r, int fieldIndex)
    {
      if (r.IsDBNull(fieldIndex))
        return null;
      byte[] nameBytes = new byte[200];
      int nameLen = (int)r.GetBytes(fieldIndex, 0, nameBytes, 0, nameBytes.Length);
      return Encoding.BigEndianUnicode.GetString(nameBytes, 0, nameLen).Replace("\0", ""); // remove trailing \0 characters found in Samsung "_T_..." channel list
    }
    #endregion


    #region Save()
    public override void Save(string tvOutputFile)
    {
      foreach (var channelList in this.DataRoot.ChannelLists)
      {
        var dbPath = this.dbPathByChannelList[channelList];
        SaveChannelList(channelList, dbPath);
      }

      // force closing the file and releasing the locks
      SQLiteConnection.ClearAllPools();
      GC.Collect();
      GC.WaitForPendingFinalizers();

      this.ZipToOutputFile(tvOutputFile);
    }
    #endregion

    #region SaveChannelList()
    private void SaveChannelList(ChannelList channelList, string dbPath)
    {
      using (var conn = new SQLiteConnection("Data Source=" + dbPath))
      {
        conn.Open();
        using (var cmdUpdateSrv = PrepareUpdateCommand(conn))
        using (var cmdDeleteSrv = PrepareDeleteCommand(conn, (channelList.SignalSource & SignalSource.Digital) != 0))
        using (var cmdInsertFav = PrepareInsertFavCommand(conn))
        using (var cmdUpdateFav = PrepareUpdateFavCommand(conn))
        using (var cmdDeleteFav = PrepareDeleteFavCommand(conn))
        {
          using (var trans = conn.BeginTransaction())
          {
            Editor.SequentializeFavPos(channelList, 5);
            this.WriteChannels(cmdUpdateSrv, cmdDeleteSrv, cmdInsertFav, cmdUpdateFav, cmdDeleteFav, channelList);
            trans.Commit();
          }
          this.RepairCorruptedDatabaseImage(cmdUpdateSrv);
        }
      }
    }

    #endregion

    #region Prepare*Command()

    private static SQLiteCommand PrepareUpdateCommand(SQLiteConnection conn)
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "update SRV set major=@nr, lockMode=@lock, hideGuide=@hidden, hidden=@hidden, numSel=@numsel, srvName=cast(@srvname as varchar) where srvId=@id";
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd.Parameters.Add(new SQLiteParameter("@nr", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@lock", DbType.Boolean));
      cmd.Parameters.Add(new SQLiteParameter("@hidden", DbType.Boolean));
      cmd.Parameters.Add(new SQLiteParameter("@numsel", DbType.Boolean));
      cmd.Parameters.Add(new SQLiteParameter("@srvname", DbType.Binary));
      cmd.Prepare();
      return cmd;
    }

    private static SQLiteCommand PrepareDeleteCommand(SQLiteConnection conn, bool digital)
    {
      var cmd = conn.CreateCommand();
      var sql = new StringBuilder();
      cmd.CommandText = "select name from sqlite_master where sql like '%srvId integer%' order by name desc";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          sql.AppendLine($"; delete from {r.GetString(0)} where srvId=@id");
      }
      cmd.CommandText = sql.ToString();
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd.Prepare();
      return cmd;
    }

    private static SQLiteCommand PrepareInsertFavCommand(SQLiteConnection conn)
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "insert into SRV_FAV (srvId, fav, pos) values (@id, @fav, @pos)";
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd.Parameters.Add(new SQLiteParameter("@fav", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@pos", DbType.Int32));
      cmd.Prepare();
      return cmd;
    }

    private static SQLiteCommand PrepareUpdateFavCommand(SQLiteConnection conn)
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "update SRV_FAV set pos=@pos where srvId=@id and fav=@fav";
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd.Parameters.Add(new SQLiteParameter("@fav", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@pos", DbType.Int32));
      cmd.Prepare();
      return cmd;
    }
    private static SQLiteCommand PrepareDeleteFavCommand(SQLiteConnection conn)
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "delete from SRV_FAV where srvId=@id and fav=@fav";
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd.Parameters.Add(new SQLiteParameter("@fav", DbType.Int32));
      cmd.Prepare();
      return cmd;
    }

    #endregion

    #region WriteChannels()
    private void WriteChannels(SQLiteCommand cmdUpdateSrv, SQLiteCommand cmdDeleteSrv, SQLiteCommand cmdInsertFav, SQLiteCommand cmdUpdateFav, SQLiteCommand cmdDeleteFav, 
      ChannelList channelList, bool analog = false)
    {

      foreach (ChannelInfo channelInfo in channelList.Channels.ToList())
      {
        var channel = channelInfo as DbChannel;
        if (channel == null) // ignore reference list proxy channels
          continue;

        if (channel.IsDeleted)
        {
          // delete channel from all tables that have a reference to srvId
          cmdDeleteSrv.Parameters["@id"].Value = channel.RecordIndex;
          cmdDeleteSrv.ExecuteNonQuery();
          channelList.Channels.Remove(channelInfo);
          continue;
        }

        // update channel record
        cmdUpdateSrv.Parameters["@id"].Value = channel.RecordIndex;
        cmdUpdateSrv.Parameters["@nr"].Value = channel.NewProgramNr;
        cmdUpdateSrv.Parameters["@lock"].Value = channel.Lock;
        cmdUpdateSrv.Parameters["@hidden"].Value = channel.Hidden;
        cmdUpdateSrv.Parameters["@numsel"].Value = !channel.Skip;
        cmdUpdateSrv.Parameters["@srvname"].Value = channel.Name == null ? (object)DBNull.Value : Encoding.BigEndianUnicode.GetBytes(channel.Name);
        cmdUpdateSrv.ExecuteNonQuery();

        // update favorites
        for (int i=0, mask=1; i<5; i++, mask <<= 1)
        {
          int oldPos = channel.OldFavIndex[i];
          int newPos = ((int)channel.Favorites & mask) != 0 ? channel.FavIndex[i] : -1;

          if (newPos >= 0)
          {
            var c = oldPos < 0 ? cmdInsertFav : cmdUpdateFav;
            c.Parameters["@id"].Value = channel.RecordIndex;
            c.Parameters["@fav"].Value = i + 1;
            c.Parameters["@pos"].Value = newPos - 1;
            c.ExecuteNonQuery();
          }      
          else
          {
            cmdDeleteFav.Parameters["@id"].Value = channel.RecordIndex;
            cmdDeleteFav.Parameters["@fav"].Value = i + 1;
            cmdDeleteFav.ExecuteNonQuery();
          }

          channel.FavIndex[i] = newPos;
        }
      }
    }
    #endregion

  }
}
