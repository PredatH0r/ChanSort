using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;
using ICSharpCode.SharpZipLib.Zip;

namespace ChanSort.Loader.Toshiba
{
  class DbSerializer : SerializerBase
  {
    private readonly Dictionary<long, DbChannel> channelById = new Dictionary<long, DbChannel>();
    private readonly Dictionary<ChannelList, string> dbPathByChannelList = new Dictionary<ChannelList, string>();
    private string tempDir;

    private enum FileType { Unknown, SatDb, ChannelDb }

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = false;
      this.DataRoot.SortedFavorites = true;
    }
    #endregion

    #region DisplayName
    public override string DisplayName { get { return "Samsung J-Series .zip Loader"; } }
    #endregion


    #region Load()
    public override void Load()
    {
      this.UnzipDataFile();
      foreach (var filePath in Directory.GetFiles(tempDir, "*."))
      {
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
                //ReadSatDatabase(conn);
                break;
              case FileType.ChannelDb:
                ReadChannelDatabase(conn, filePath);
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
      if ((long) cmd.ExecuteScalar() == 3)
        return FileType.ChannelDb;

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
      cmd.CommandText = "select distinct satId, satName, satPos, satDir from SAT";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          Satellite sat = new Satellite(r.GetInt32(0));
          int pos = Math.Abs(r.GetInt32(2));
          sat.OrbitalPosition = string.Format("{0}.{1}{2}", pos / 10, pos % 10, r.GetInt32(3) == 1 ? "E" : "W");
          sat.Name = r.GetString(1) + " " + sat.OrbitalPosition;
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
          tp.SymbolRate = r.GetInt32(3) / 1000;
          this.DataRoot.AddTransponder(tp.Satellite, tp);
        }
      }
    }
    #endregion


    #region ReadChannelDatabase()
    private void ReadChannelDatabase(SQLiteConnection conn, string dbPath)
    {
      this.channelById.Clear();
      using (var cmd = conn.CreateCommand())
      {
        this.RepairCorruptedDatabaseImage(cmd);
        var providers = this.ReadProviders(cmd);
        //this.ReadAnalogChannels(cmd);
        var channelList = this.ReadDtvChannels(cmd, dbPath, providers);
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

    #region ReadAnalogChannels()
    private void ReadAnalogChannels(SQLiteCommand cmd)
    {
      //string[] fieldNames = {"channel_handle", "channel_number", "list_bits", "channel_label", "frequency"};
      //var sql = this.GetQuery("EuroATVChanList", fieldNames);
      //var fields = this.GetFieldMap(fieldNames);
      
      //cmd.CommandText = sql;
      //using (var r = cmd.ExecuteReader())
      //{
      //  while (r.Read())
      //  {
      //    ChannelInfo channel = new DbChannel(SignalSource.Analog, r, fields, this.DataRoot);
      //    if (!channel.IsDeleted)
      //      this.DataRoot.AddChannel(this.atvChannels, channel);
      //  }
      //}
    }
    #endregion

    #region ReadDtvChannels()
    private ChannelList ReadDtvChannels(SQLiteCommand cmd, string dbPath, Dictionary<long, string> providers)
    {
      string name = Path.GetFileName(dbPath);
      ChannelList channelList = new ChannelList(SignalSource.Digital, name);
      this.ReadDigitalChannels(cmd, "SRV_DVB", channelList, providers);
      this.DataRoot.AddChannelList(channelList);
      return channelList;
    }
    #endregion

    #region ReadDigitalChannels()
    private void ReadDigitalChannels(SQLiteCommand cmd, string table, ChannelList channelList, Dictionary<long, string> providers)
    {
      string[] fieldNames = { "SRV.srvId", "major", "cast(srvName as blob)", "cast(shrtSrvName as blob)", "chType", "chNum", 
                            "srvType", "onid", "tsid", "vidPid", "progNum", "freq", "hidden", "scrambled", "lockMode", "numSel", "provId" };
      var sql = this.GetQuery(table, fieldNames);
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var channel = new DbChannel(r, fields, this.DataRoot, providers);
          if (!channel.IsDeleted)
          {
            this.DataRoot.AddChannel(channelList, channel);
            this.channelById.Add(channel.RecordIndex, channel);
          }
        }
      }
    }
    #endregion

    #region GetQuery()
    private string GetQuery(string table, string[] fieldNames)
    {
      string sql = "select ";
      for (int i = 0; i < fieldNames.Length; i++)
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
    private IDictionary<string, int> GetFieldMap(string[] fieldNames)
    {
      Dictionary<string, int> field = new Dictionary<string, int>();
      for (int i = 0; i < fieldNames.Length; i++)
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
        int fav = r.GetInt32(1) - 1;
        int pos = r.GetInt32(2);
        channel.FavIndex[fav] = channel.OriginalFavIndex[fav] = pos;
        if (pos >= 0)
          channel.Favorites |= (Favorites) (1 << fav);
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
        using (var cmd = conn.CreateCommand())
        using (var cmd2 = conn.CreateCommand())
        using (var cmd3 = conn.CreateCommand())
        using (var cmd4 = conn.CreateCommand())
        {
          using (var trans = conn.BeginTransaction())
          {
            this.PrepareCommands(cmd, cmd2, cmd3, cmd4);
            this.WriteChannels(cmd, cmd2, cmd3, cmd4, channelList);
            trans.Commit();
          }
          this.RepairCorruptedDatabaseImage(cmd);
        }
      }
    }

    #endregion

    #region PrepareCommands()
    private void PrepareCommands(SQLiteCommand cmd, SQLiteCommand cmd2, SQLiteCommand cmd3, SQLiteCommand cmd4)
    {
      cmd.CommandText = "update SRV set major=@nr, lockMode=@lock, hidden=@hidden, numSel=@numsel where srvId=@id";
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd.Parameters.Add(new SQLiteParameter("@nr", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@lock", DbType.Boolean));
      cmd.Parameters.Add(new SQLiteParameter("@hidden", DbType.Boolean));
      cmd.Parameters.Add(new SQLiteParameter("@numsel", DbType.Boolean));
      cmd.Prepare();

      cmd2.CommandText = "insert into SRV_FAV (srvId, fav, pos) values (@id, @fav, @pos)";
      cmd2.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd2.Parameters.Add(new SQLiteParameter("@fav", DbType.Int32));
      cmd2.Parameters.Add(new SQLiteParameter("@pos", DbType.Int32));
      cmd2.Prepare();

      cmd3.CommandText = "update SRV_FAV set pos=@pos where srvId=@id and fav=@fav";
      cmd3.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd3.Parameters.Add(new SQLiteParameter("@fav", DbType.Int32));
      cmd3.Parameters.Add(new SQLiteParameter("@pos", DbType.Int32));
      cmd3.Prepare();

      cmd4.CommandText = "delete from SRV_FAV where srvId=@id and fav=@fav";
      cmd4.Parameters.Add(new SQLiteParameter("@id", DbType.Int64));
      cmd4.Parameters.Add(new SQLiteParameter("@fav", DbType.Int32));
      cmd4.Prepare();      
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SQLiteCommand cmd, SQLiteCommand cmd2, SQLiteCommand cmd3, SQLiteCommand cmd4, ChannelList channelList, bool analog = false)
    {      
      foreach (ChannelInfo channelInfo in channelList.Channels)
      {
        var channel = channelInfo as DbChannel;
        if (channel == null) // ignore reference list proxy channels
          continue;
        channel.UpdateRawData();
        cmd.Parameters["@id"].Value = channel.RecordIndex;
        cmd.Parameters["@nr"].Value = channel.NewProgramNr;
        cmd.Parameters["@lock"].Value = channel.Lock;
        cmd.Parameters["@hidden"].Value = channel.Hidden;
        cmd.Parameters["@numsel"].Value = !channel.Skip;
        cmd.ExecuteNonQuery();

        for (int i=0; i<channel.FavIndex.Count; i++)
        {
          int oldPos;
          if (!channel.OriginalFavIndex.TryGetValue(i, out oldPos))
            oldPos = -1;
          int newPos = channel.FavIndex[i];
          if (newPos == oldPos)
            continue;
          if (newPos > 0)
          {
            var c = oldPos < 0 ? cmd2 : cmd3;
            c.Parameters["@id"].Value = channel.RecordIndex;
            c.Parameters["@fav"].Value = i + 1;
            c.Parameters["@pos"].Value = newPos;
            c.ExecuteNonQuery();
          }      
          else
          {
            cmd4.Parameters["@id"].Value = channel.RecordIndex;
            cmd4.Parameters["@fav"].Value = i + 1;
            cmd4.ExecuteNonQuery();
          }
        }
      }
    }
    #endregion
  }
}
