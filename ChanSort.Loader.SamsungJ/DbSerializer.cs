using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using ChanSort.Api;
using ICSharpCode.SharpZipLib.Zip;

namespace ChanSort.Loader.Toshiba
{
  class DbSerializer : SerializerBase
  {
    private const string FILE_sat = "sat";
    private const string FILE_dvbc = "dvbc";
    //private const string FILE_dvbMainData_db = "dvbMainData.db";

    private readonly ChannelList atvChannels = new ChannelList(SignalSource.AnalogCT | SignalSource.TvAndRadio, "Analog");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbCT | SignalSource.TvAndRadio, "Cable");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Satellite");
    private readonly Dictionary<long, DbChannel> channelById = new Dictionary<long, DbChannel>();

    private string tempDir;

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = false;
      this.DataRoot.SortedFavorites = true;

      this.DataRoot.AddChannelList(this.atvChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.dvbsChannels);
    }
    #endregion

    #region DisplayName
    public override string DisplayName { get { return "Samsung J-Series .zip Loader"; } }
    #endregion


    #region Load()
    public override void Load()
    {
      this.UnzipDataFile();

      string channelConnString = "Data Source=" + tempDir + FILE_sat;
      using (var conn = new SQLiteConnection(channelConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.RepairCorruptedDatabaseImage(cmd);
          this.ReadSatellites(cmd);
          this.ReadTransponders(cmd);
        }
      }

      string sysDataConnString = "Data Source=" + tempDir + FILE_dvbc;
      using (var conn = new SQLiteConnection(sysDataConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.RepairCorruptedDatabaseImage(cmd);
          //this.ReadAnalogChannels(cmd);
          this.ReadDtvChannels(cmd);
          //this.ReadSatChannels(cmd);
          this.ReadFavorites(cmd);
        }
      }

#if false
      string mainDataConnString = "Data Source=" + tempDir + FILE_dvbMainData_db;
      using (var conn = new SQLiteConnection(mainDataConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.ReadCryptInfo(cmd);
        }
      }
#endif
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

    #region ReadCryptInfo()
    private void ReadCryptInfo(SQLiteCommand cmd)
    {
      cmd.CommandText =
        "select satellite_id, original_network_id, transport_stream_id, service_id, free_CA_mode from services";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          int satId = r.IsDBNull(0) ? 0 : r.GetInt32(0);
          var sat = this.DataRoot.Satellites.TryGet(satId);
          var satPos = sat != null ? sat.OrbitalPosition : "0.0";
          string format = sat != null ? "S{0}-{1}-{2}-{3}" : "C-{1}-{2}-{3}";
          string uid = string.Format(format, satPos, r.GetInt32(1), r.GetInt32(2), r.GetInt32(3));
        }
      }
    }
    #endregion


    #region ReadAnalogChannels()
    private void ReadAnalogChannels(SQLiteCommand cmd)
    {
      string[] fieldNames = {"channel_handle", "channel_number", "list_bits", "channel_label", "frequency"};
      var sql = this.GetQuery("EuroATVChanList", fieldNames);
      var fields = this.GetFieldMap(fieldNames);
      
      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          ChannelInfo channel = new DbChannel(SignalSource.Analog, r, fields, this.DataRoot);
          if (!channel.IsDeleted)
            this.DataRoot.AddChannel(this.atvChannels, channel);
        }
      }
    }
    #endregion

    #region ReadDtvChannels()
    private void ReadDtvChannels(SQLiteCommand cmd)
    {
      this.ReadDigitalChannels(cmd, "SRV_DVB", SignalSource.DvbCT, this.dvbcChannels);
    }
    #endregion

    #region ReadSatChannels()
    private void ReadSatChannels(SQLiteCommand cmd)
    {
      this.ReadDigitalChannels(cmd, "EuroSATChanList", SignalSource.DvbS, this.dvbsChannels);
    }
    #endregion

    #region ReadDigitalChannels()
    private void ReadDigitalChannels(SQLiteCommand cmd, string table, SignalSource signalSource, ChannelList channelList)
    {
      string[] fieldNames = { "SRV.srvId", "major", "cast(srvName as blob)", "cast(shrtSrvName as blob)", "CHNL.chId", 
                            "srvType", "onid", "tsid", "vidPid", "progNum", "freq", "hidden", "scrambled", "lockMode", "numSel" };
      var sql = this.GetQuery(table, fieldNames);
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var channel = new DbChannel(signalSource, r, fields, this.DataRoot);
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
        int fav = r.GetInt32(1);
        int pos = r.GetInt32(2);
        channel.FavIndex[fav] = channel.OriginalFavIndex[fav] = pos;
        if (pos > 0)
          channel.Favorites |= (Favorites) (1 << fav);
      }
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

      string channelConnString = "Data Source=" + this.tempDir + FILE_dvbc;
      using (var conn = new SQLiteConnection(channelConnString))
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
            this.WriteChannels(cmd, cmd2, cmd3, cmd4, this.dvbcChannels);
            trans.Commit();
          }
          this.RepairCorruptedDatabaseImage(cmd);
        }
      }

      this.ZipFiles();
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
            c.Parameters["@fav"].Value = i;
            c.Parameters["@pos"].Value = newPos;
            c.ExecuteNonQuery();
          }      
          else
          {
            cmd4.Parameters["@id"].Value = channel.RecordIndex;
            cmd4.Parameters["@fav"].Value = i;
            cmd4.ExecuteNonQuery();
          }
        }
      }
    }
    #endregion

    #region ZipFiles()
    private void ZipFiles()
    {
      const string entryName = FILE_dvbc;
      using (var zip = new ZipFile(this.FileName))
      {
        zip.BeginUpdate();
        zip.Delete(entryName);
        zip.Add(this.tempDir + FILE_dvbc, entryName);
        zip.CommitUpdate();
      }
    }
    #endregion
  }
}
