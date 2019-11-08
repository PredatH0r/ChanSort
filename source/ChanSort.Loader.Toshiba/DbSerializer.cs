using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  class DbSerializer : SerializerBase
  {
    private const string FILE_chmgt_db = "chmgt_type001\\chmgt.db";
    private const string FILE_dvbSysData_db = "dvb_type001\\dvbSysData.db";
    private const string FILE_dvbMainData_db = "dvb_type001\\dvbMainData.db";

    private readonly ChannelList atvChannels = new ChannelList(SignalSource.AnalogCT, "Analog");
    private readonly ChannelList dtvTvChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Tv, "DTV");
    private readonly ChannelList dtvRadioChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Radio, "Radio");
    private readonly ChannelList satTvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Sat-TV");
    private readonly ChannelList satRadioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat-Radio");
    private readonly Dictionary<string, bool> channelInfoByUid = new Dictionary<string, bool>();

    private string tempDir;

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;

      this.DataRoot.AddChannelList(this.atvChannels);
      this.DataRoot.AddChannelList(this.dtvTvChannels);
      this.DataRoot.AddChannelList(this.dtvRadioChannels);
      this.DataRoot.AddChannelList(this.satTvChannels);
      this.DataRoot.AddChannelList(this.satRadioChannels);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      this.tempDir = this.UnzipFileToTempFolder() + "\\"; 

      string sysDataConnString = "Data Source=" + tempDir + FILE_dvbSysData_db;
      using (var conn = new SQLiteConnection(sysDataConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.RepairCorruptedDatabaseImage(cmd);
          this.ReadSatellites(cmd);
          this.ReadTransponders(cmd);
        }
      }

      string mainDataConnString = "Data Source=" + tempDir + FILE_dvbMainData_db;
      using (var conn = new SQLiteConnection(mainDataConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.ReadCryptInfo(cmd);
        }
      }

      string channelConnString = "Data Source=" + tempDir + FILE_chmgt_db;
      using (var conn = new SQLiteConnection(channelConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.ReadAnalogChannels(cmd);
          this.ReadDtvChannels(cmd);
          this.ReadSatChannels(cmd);
        }
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
      cmd.CommandText = "select distinct satellite_id, satellite_name, orbital_position, west_east_flag from satellite";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          Satellite sat = new Satellite(r.GetInt32(0));
          int pos = r.GetInt32(2);
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
      cmd.CommandText = "select satellite_id, frequency, polarization, symbol_rate, transponder_number from satellite";
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
          this.channelInfoByUid[uid] = r.GetInt32(4) != 0;
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
          ChannelInfo channel = new DbChannel(SignalSource.Analog, r, fields, this.DataRoot, this.channelInfoByUid);
          if (!channel.IsDeleted)
            this.DataRoot.AddChannel(this.atvChannels, channel);
        }
      }
    }
    #endregion

    #region ReadDtvChannels()
    private void ReadDtvChannels(SQLiteCommand cmd)
    {
      this.ReadDigitalChannels(cmd, "EuroDTVChanList", SignalSource.DvbCT, this.dtvTvChannels, this.dtvRadioChannels);
    }
    #endregion

    #region ReadSatChannels()
    private void ReadSatChannels(SQLiteCommand cmd)
    {
      this.ReadDigitalChannels(cmd, "EuroSATChanList", SignalSource.DvbS, this.satTvChannels, this.satRadioChannels);
    }
    #endregion

    #region ReadDigitalChannels()
    private void ReadDigitalChannels(SQLiteCommand cmd, string table, SignalSource signalSource, ChannelList tvChannels, ChannelList radioChannels)
    {
      string[] fieldNames = { "channel_handle", "channel_number", "channel_label", "frequency", "list_bits",
                            "dvb_service_type", "onid", "tsid", "sid", "sat_id", "channel_order" };
      var sql = this.GetQuery(table, fieldNames);
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          ChannelInfo channel = new DbChannel(signalSource, r, fields, this.DataRoot, this.channelInfoByUid);
          if (!channel.IsDeleted)
          {
            var channelList = (channel.SignalSource & SignalSource.Radio) != 0 ? radioChannels : tvChannels;
            this.DataRoot.AddChannel(channelList, channel);
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
      sql += " from " + table;
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


    #region Save()
    public override void Save(string tvOutputFile)
    {
      string channelConnString = "Data Source=" + this.tempDir + FILE_chmgt_db;
      using (var conn = new SQLiteConnection(channelConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        using (var cmd2 = conn.CreateCommand())
        {
          using (var trans = conn.BeginTransaction())
          {
            this.WriteChannels(cmd, cmd2, "EuroATVChanList", this.atvChannels, true);
            this.WriteChannels(cmd, cmd2, "EuroDTVChanList", this.dtvTvChannels);
            this.WriteChannels(cmd, cmd2, "EuroDTVChanList", this.dtvRadioChannels);
            this.WriteChannels(cmd, cmd2, "EuroSATChanList", this.satTvChannels);
            this.WriteChannels(cmd, cmd2, "EuroSATChanList", this.satRadioChannels);
            trans.Commit();
          }
          this.RepairCorruptedDatabaseImage(cmd);
        }        
      }

      this.ZipToOutputFile(tvOutputFile);
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SQLiteCommand cmd, SQLiteCommand cmdDelete, string table, ChannelList channelList, bool analog=false)
    {
      string sql = "update " + table + " set channel_number=@nr ";
      if (!analog)
        sql += ", channel_order=@nr";
      sql += ", list_bits=@Bits where channel_handle=@id";
      cmd.CommandText = sql;
      cmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@nr", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@Bits", DbType.Int32));
      cmd.Prepare();

      cmdDelete.CommandText = "delete from " + table + " where channel_handle=@id";
      cmdDelete.Parameters.Add(new SQLiteParameter("@id", DbType.Int32));

      foreach (ChannelInfo channelInfo in channelList.Channels)
      {
        var channel = channelInfo as DbChannel;
        if (channel == null) // ignore reference list proxy channels
          continue;

        if (channel.IsDeleted)
        {
          cmdDelete.Parameters["@id"].Value = channel.RecordIndex;
          cmdDelete.ExecuteNonQuery();
        }
        else
        {
          channel.UpdateRawData();
          cmd.Parameters["@id"].Value = channel.RecordIndex;
          cmd.Parameters["@nr"].Value = channel.NewProgramNr;
          cmd.Parameters["@Bits"].Value = channel.Bits;
          cmd.ExecuteNonQuery();
        }
      }
    }
    #endregion
  }
}
