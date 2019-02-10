using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;
using ICSharpCode.SharpZipLib.Zip;

namespace ChanSort.Loader.SamsungJ
{
  /// <summary>
  /// Loader for Samsung J/K/M/Q/N/... series .zip files
  /// </summary>
  class DbSerializer : SerializerBase
  {
    private readonly Dictionary<long, DbChannel> channelById = new Dictionary<long, DbChannel>();
    private readonly Dictionary<ChannelList, string> dbPathByChannelList = new Dictionary<ChannelList, string>();
    private string tempDir;

    private enum FileType { Unknown, SatDb, ChannelDbDvb, ChannelDbAnalog }

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanDeleteChannels = true;
      this.DataRoot.SupportedFavorites = Favorites.A | Favorites.B | Favorites.C | Favorites.D | Favorites.E;
      this.DataRoot.SortedFavorites = true;
      this.DataRoot.AllowGapsInFavNumbers = false;
      this.DataRoot.ShowDeletedChannels = false;
    }
    #endregion

    #region DisplayName
    public override string DisplayName => "Samsung .zip Loader";

    #endregion


    #region Load()
    public override void Load()
    {
      this.UnzipDataFile();
      if (File.Exists(tempDir + "\\sat"))
      {
        try
        {
          using (var conn = new SQLiteConnection("Data Source=" + tempDir + "\\sat"))
          {
            conn.Open();
            this.ReadSatDatabase(conn);
          }
        }
        catch { }
      }

      var files = Directory.GetFiles(tempDir, "*.");
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
              case FileType.SatDb: break;
              case FileType.ChannelDbAnalog:
                ReadChannelDatabase(conn, filePath, false);
                break;
              case FileType.ChannelDbDvb:
                ReadChannelDatabase(conn, filePath, true);
                break;
            }
          }
        }
        catch
        {
        }
      }
    }
    #endregion

    #region UnzipDataFile()
    private void UnzipDataFile()
    {
      this.tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + "\\";
      Directory.CreateDirectory(tempDir);
      Application.ApplicationExit += this.CleanTempFolder;

      using (ZipFile zip = new ZipFile(this.FileName))
      {
        foreach(ZipEntry entry in zip)
          this.Expand(zip, entry.Name);
      }
    }
    #endregion

    #region CleanTempFolder()
    private void CleanTempFolder(object sender, EventArgs e)
    {
      try
      {
        foreach(var file in Directory.GetFiles(this.tempDir))
          File.Delete(file);
        Directory.Delete(this.tempDir);
      }
      catch { }
    }
    #endregion

    #region Expand()
    private void Expand(ZipFile zip, string path)
    {
      var entry = zip.GetEntry(path);
      if (entry == null)
        throw new FileLoadException("File not found inside .zip: " + path);

      byte[] buffer = new byte[65536];
      using (var input = zip.GetInputStream(entry))
      using (var output = new FileStream(this.tempDir + Path.GetFileName(path), FileMode.Create))
      {
        int len;
        while ((len = input.Read(buffer, 0, buffer.Length)) != 0)
          output.Write(buffer, 0, len);
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
      cmd.CommandText = "select count(1) from sqlite_master where type='table' and name in ('SAT','SAT_TP')";
      if ((long)cmd.ExecuteScalar() == 2)
        return FileType.SatDb;

      cmd.CommandText = "select count(1) from sqlite_master where type='table' and name in ('CHNL','SRV','SRV_DVB')";
      if ((long)cmd.ExecuteScalar() == 3)
        return FileType.ChannelDbDvb;

      cmd.CommandText = "select count(1) from sqlite_master where type='table' and name in ('CHNL','SRV','SRV_ANL')";
      if ((long)cmd.ExecuteScalar() == 3)
        return FileType.ChannelDbAnalog;

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
    private void ReadChannelDatabase(SQLiteConnection conn, string dbPath, bool digital)
    {
      this.channelById.Clear();
      using (var cmd = conn.CreateCommand())
      {
        var providers = digital ? this.ReadProviders(cmd) : null;
        var channelList = this.ReadChannels(cmd, dbPath, providers, digital);
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
    private ChannelList ReadChannels(SQLiteCommand cmd, string dbPath, Dictionary<long, string> providers, bool digital)
    {
      var signalSource = DetectSignalSource(cmd, digital);

      string name = Path.GetFileName(dbPath);
      ChannelList channelList = new ChannelList(signalSource, name);
      string table = digital ? "SRV_DVB" : "SRV_ANL";
      List<string> fieldNames = new List<string> { 
                            "chType", "chNum", "freq", // CHNL
                            "SRV.srvId", "major", "progNum", "cast(srvName as blob)", "srvType", "hidden", "scrambled", "lockMode", "numSel", // SRV
                            };
      if (digital)
        fieldNames.AddRange(new[] {"onid", "tsid", "vidPid", "provId", "cast(shrtSrvName as blob)", "lcn"}); // SRV_DVB

      var sql = this.BuildQuery(table, fieldNames);
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          // 171027 - ohuseyinoglu: With our change in transponder indexing, we can directly look it up by "chNum" now!
          var tp = this.DataRoot.Transponder.TryGet(r.GetInt32(1));
          // ... and get the satellite from that transponder - if set
          // Note that we can have channels from multiple satellites in the same list, so this is a loop variable now
          var sat = tp?.Satellite;
          var channel = new DbChannel(r, fields, this.DataRoot, providers, sat, tp);
          if (!channel.IsDeleted)
          {
            this.DataRoot.AddChannel(channelList, channel);
            this.channelById.Add(channel.RecordIndex, channel);
          }
        }
      }

      this.DataRoot.AddChannelList(channelList);
      return channelList;
    }
    #endregion

    #region DetectSignalSource()
    private static SignalSource DetectSignalSource(SQLiteCommand cmd, bool digital)
    {
      var signalSource = digital ? SignalSource.Digital : SignalSource.Analog;
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
      return signalSource | SignalSource.TvAndRadio;
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
      return sql;
    }
    #endregion

    #region GetFieldMap()
    private IDictionary<string, int> GetFieldMap(IList<string> fieldNames)
    {
      Dictionary<string, int> field = new Dictionary<string, int>();
      for (int i = 0; i < fieldNames.Count; i++)
        field[fieldNames[i]] = i;
      return field;
    }
    #endregion

    #region ReadFavorites()
    private void ReadFavorites(SQLiteCommand cmd)
    {
      cmd.CommandText = "select srvId, fav, pos from SRV_FAV";
      var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var channel = this.channelById.TryGet(r.GetInt64(0));
        if (channel == null) 
          continue;
        int fav = r.GetInt32(1) - 1; // fav values start with 1 in the table
        int pos = r.GetInt32(2);     // pos values start with 0
        if (pos >= 0)
        {
          channel.Favorites |= (Favorites) (1 << fav);
          channel.FavIndex[fav] = channel.OldFavIndex[fav] = pos + 1;
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
      if (tvOutputFile != this.FileName)
      {
        File.Copy(this.FileName, tvOutputFile);
        this.FileName = tvOutputFile;
      }

      using (var zip = new ZipFile(this.FileName))
      {
        zip.BeginUpdate();

        foreach (var channelList in this.DataRoot.ChannelLists)
        {
          var dbPath = this.dbPathByChannelList[channelList];
          SaveChannelList(channelList, dbPath);

          var entryName = Path.GetFileName(dbPath);
          zip.Delete(entryName);
          zip.Add(dbPath, entryName);
        }

        zip.CommitUpdate();
      }
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

      foreach (ChannelInfo channelInfo in channelList.Channels)
      {
        var channel = channelInfo as DbChannel;
        if (channel == null) // ignore reference list proxy channels
          continue;

#if false        
        // disabled, because channels should just be marked as deleted and not physically deleted

        if (channel.NewProgramNr < 0)
        {
          // delete channel from all tables that have a reference to srvId
          cmdDeleteSrv.Parameters["@id"].Value = channel.RecordIndex;
          cmdDeleteSrv.ExecuteNonQuery();
          continue;
        }
#endif

        // update channel record
        cmdUpdateSrv.Parameters["@id"].Value = channel.RecordIndex;
        cmdUpdateSrv.Parameters["@nr"].Value = channel.IsDeleted ? -1 : channel.NewProgramNr;
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

          channel.OldFavIndex[i] = channel.FavIndex[i] = newPos;
        }
      }
    }
    #endregion

  }
}
