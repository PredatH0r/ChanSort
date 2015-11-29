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
  class DbSerializer : SerializerBase
  {
    private readonly Dictionary<long, DbChannel> channelById = new Dictionary<long, DbChannel>();
    private readonly Dictionary<ChannelList, string> dbPathByChannelList = new Dictionary<ChannelList, string>();
    private string tempDir;
    private Dictionary<int, Transponder> transponderByFreq;

    private enum FileType { Unknown, SatDb, ChannelDbDvb, ChannelDbAnalog }

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanDeleteChannels = true;
      this.DataRoot.SupportedFavorites = Favorites.A | Favorites.B | Favorites.C | Favorites.D | Favorites.E;
      this.DataRoot.SortedFavorites = true;
    }
    #endregion

    #region DisplayName
    public override string DisplayName => "Samsung J-Series .zip Loader";

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

      foreach (var filePath in Directory.GetFiles(tempDir, "*."))
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
          sat.OrbitalPosition = $"{pos/10}.{pos%10}{(r.GetInt32(3) == 1 ? "E" : "W")}";
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
          int satId = r.GetInt32(0);
          int freq = r.GetInt32(1);
          int id = satId * 1000000 + freq / 1000;
          if (this.DataRoot.Transponder.TryGet(id) != null)
            continue;
          Transponder tp = new Transponder(id);
          tp.FrequencyInMhz = (decimal)freq / 1000;
          tp.Number = r.GetInt32(4);
          tp.Polarity = r.GetInt32(2) == 0 ? 'H' : 'V';
          tp.Satellite = this.DataRoot.Satellites.TryGet(satId);
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
        this.RepairCorruptedDatabaseImage(cmd);
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
      var sat = (signalSource & SignalSource.Sat) == 0 ? null : this.DetectSatellite(cmd);

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
          var tp = this.transponderByFreq?.TryGet(r.GetInt32(2));
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

    #region DetectSatellite()

    /// <summary>
    /// I haven't found a direct way to link a dvbs database file or its channels to a satId.
    /// This workaround compares the transponder frequencies in the channel list with the transponder frequencies of each satellite to find a match.
    /// </summary>
    private Satellite DetectSatellite(SQLiteCommand cmd)
    {
      List<int> tpFreq = new List<int>();
      cmd.CommandText = "select freq from CHNL where chType=7";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          tpFreq.Add(r.GetInt32(0));
      }

      this.transponderByFreq = null;
      foreach (var sat in DataRoot.Satellites.Values)
      {
        Dictionary<int, Transponder> satFreq = new Dictionary<int, Transponder>();
        foreach (var tp in sat.Transponder.Values)
          satFreq.Add((int) (tp.FrequencyInMhz*1000), tp);

        int mismatch = 0;
        foreach (int freq in tpFreq)
        {
          if (satFreq.ContainsKey(freq) || satFreq.ContainsKey(freq - 1000) || satFreq.ContainsKey(freq + 1000))
            continue;

          ++mismatch;
        }

        if (mismatch < 10)
        {
          this.transponderByFreq = satFreq;
          return sat;
        }
      }
      return null;
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
          channel.OriginalFavs = channel.Favorites;
          channel.FavIndex[fav] = channel.OriginalFavIndex[fav] = pos + 1;
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
      return Encoding.BigEndianUnicode.GetString(nameBytes, 0, nameLen);
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
        
        if (channel.NewProgramNr < 0)
        {
          // delete channel from all tables that have a reference to srvId
          cmdDeleteSrv.Parameters["@id"].Value = channel.RecordIndex;
          cmdDeleteSrv.ExecuteNonQuery();
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
          int oldPos = channel.OriginalFavIndex[i];
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

          channel.OriginalFavIndex[i] = channel.FavIndex[i] = newPos;
        }
        channel.OriginalFavs = channel.Favorites;
      }
    }
    #endregion

  }
}
