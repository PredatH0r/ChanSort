using System.Collections.Generic;
using System.IO;
using ChanSort.Api;
using Microsoft.Data.Sqlite;

namespace ChanSort.Loader.Toshiba
{
  internal class ChmgtDbSerializer : SerializerBase
  {
    private const string FILE_chmgt_db = "\\chmgt_type001\\chmgt.db";
    private const string FILE_dvbSysData_db = "\\dvb_type001\\dvbSysData.db";
    private const string FILE_dvbMainData_db = "\\dvb_type001\\dvbMainData.db";

    private readonly ChannelList atvChannels = new(SignalSource.AnalogCT, "Analog");
    private readonly Dictionary<string, bool> channelInfoByUid = new();
    private readonly ChannelList dtvRadioChannels = new(SignalSource.DvbCT | SignalSource.Radio, "Radio");
    private readonly ChannelList dtvTvChannels = new(SignalSource.DvbCT | SignalSource.Tv, "DTV");
    private readonly ChannelList satRadioChannels = new(SignalSource.DvbS | SignalSource.Radio, "Sat-Radio");
    private readonly ChannelList satTvChannels = new(SignalSource.DvbS | SignalSource.Tv, "Sat-TV");

    private string workingDir;

    #region ctor()

    public ChmgtDbSerializer(string inputFile) : base(inputFile)
    {
      Features.ChannelNameEdit = ChannelNameEditMode.All;
      Features.DeleteMode = DeleteMode.Physically;
      Features.CanSkipChannels = false;
      Features.CanLockChannels = true;
      Features.CanHideChannels = false;
      Features.FavoritesMode = FavoritesMode.None;

      DataRoot.AddChannelList(atvChannels);
      DataRoot.AddChannelList(dtvTvChannels);
      DataRoot.AddChannelList(dtvRadioChannels);
      DataRoot.AddChannelList(satTvChannels);
      DataRoot.AddChannelList(satRadioChannels);
    }

    #endregion

    #region Load()

    public override void Load()
    {
      // this.FileName can be either hotelopt_type001.bin (as an anchor for the directory structure), or a .zip file containing that directory structure
      if (Path.GetExtension(this.FileName).ToLowerInvariant() == ".zip")
      {
        UnzipFileToTempFolder();
        workingDir = this.TempPath;
      }
      else
        workingDir = Path.GetDirectoryName(this.FileName);

      var sysDataConnString = $"Data Source={this.workingDir + FILE_dvbSysData_db};Pooling=false";
      using (var conn = new SqliteConnection(sysDataConnString))
      {
        conn.Open();
        using var cmd = conn.CreateCommand();
        RepairCorruptedDatabaseImage(cmd);
        ReadSatellites(cmd);
        ReadTransponders(cmd);
      }

      var mainDataConnString = $"Data Source={this.workingDir + FILE_dvbMainData_db};Pooling=False";
      using (var conn = new SqliteConnection(mainDataConnString))
      {
        conn.Open();
        using var cmd = conn.CreateCommand();
        ReadCryptInfo(cmd);
      }

      var channelConnString = $"Data Source={this.workingDir + FILE_chmgt_db};Pooling=False";
      using (var conn = new SqliteConnection(channelConnString))
      {
        conn.Open();
        using var cmd = conn.CreateCommand();
        ReadAnalogChannels(cmd);
        ReadDtvChannels(cmd);
        ReadSatChannels(cmd);
      }
    }

    #endregion

    #region RepairCorruptedDatabaseImage()

    private void RepairCorruptedDatabaseImage(SqliteCommand cmd)
    {
      cmd.CommandText = "REINDEX";
      cmd.ExecuteNonQuery();
    }

    #endregion

    #region ReadSatellites()

    private void ReadSatellites(SqliteCommand cmd)
    {
      cmd.CommandText = "select distinct satellite_id, satellite_name, orbital_position, west_east_flag from satellite";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var sat = new Satellite(r.GetInt32(0));
        var pos = r.GetInt32(2);
        sat.OrbitalPosition = $"{pos / 10}.{pos % 10}{(r.GetInt32(3) == 1 ? "E" : "W")}";
        sat.Name = r.GetString(1) + " " + sat.OrbitalPosition;
        DataRoot.AddSatellite(sat);
      }
    }

    #endregion

    #region ReadTransponders()

    private void ReadTransponders(SqliteCommand cmd)
    {
      cmd.CommandText = "select satellite_id, frequency, polarization, symbol_rate, transponder_number from satellite";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var satId = r.GetInt32(0);
        var freq = r.GetInt32(1);
        var id = satId * 1000000 + freq / 1000;
        if (DataRoot.Transponder.TryGet(id) != null)
          continue;
        var tp = new Transponder(id)
        {
          FrequencyInMhz = (decimal) freq / 1000,
          Number = r.GetInt32(4),
          Polarity = r.GetInt32(2) == 0 ? 'H' : 'V',
          Satellite = DataRoot.Satellites.TryGet(satId),
          SymbolRate = r.GetInt32(3) / 1000
        };
        DataRoot.AddTransponder(tp.Satellite, tp);
      }
    }

    #endregion

    #region ReadCryptInfo()

    private void ReadCryptInfo(SqliteCommand cmd)
    {
      cmd.CommandText =
        "select satellite_id, original_network_id, transport_stream_id, service_id, free_CA_mode from services";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var satId = r.IsDBNull(0) ? 0 : r.GetInt32(0);
        var sat = DataRoot.Satellites.TryGet(satId);
        var satPos = sat != null ? sat.OrbitalPosition : "0.0";
        var format = sat != null ? "S{0}-{1}-{2}-{3}" : "C-{1}-{2}-{3}";
        var uid = string.Format(format, satPos, r.GetInt32(1), r.GetInt32(2), r.GetInt32(3));
        channelInfoByUid[uid] = r.GetInt32(4) != 0;
      }
    }

    #endregion


    #region ReadAnalogChannels()

    private void ReadAnalogChannels(SqliteCommand cmd)
    {
      string[] fieldNames = {"channel_handle", "channel_number", "list_bits", "channel_label", "frequency"};
      var sql = GetQuery("EuroATVChanList", fieldNames);
      var fields = GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        ChannelInfo channel = new DbChannel(SignalSource.Analog, r, fields, DataRoot, channelInfoByUid);
        if (!channel.IsDeleted)
          DataRoot.AddChannel(atvChannels, channel);
      }
    }

    #endregion

    #region ReadDtvChannels()

    private void ReadDtvChannels(SqliteCommand cmd)
    {
      ReadDigitalChannels(cmd, "EuroDTVChanList", SignalSource.DvbCT, dtvTvChannels, dtvRadioChannels);
    }

    #endregion

    #region ReadSatChannels()

    private void ReadSatChannels(SqliteCommand cmd)
    {
      ReadDigitalChannels(cmd, "EuroSATChanList", SignalSource.DvbS, satTvChannels, satRadioChannels);
    }

    #endregion

    #region ReadDigitalChannels()

    private void ReadDigitalChannels(SqliteCommand cmd, string table, SignalSource signalSource, ChannelList tvChannels, ChannelList radioChannels)
    {
      string[] fieldNames =
      {
        "channel_handle", "channel_number", "channel_label", "frequency", "list_bits",
        "dvb_service_type", "onid", "tsid", "sid", "sat_id", "channel_order"
      };
      var sql = GetQuery(table, fieldNames);
      var fields = GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        ChannelInfo channel = new DbChannel(signalSource, r, fields, DataRoot, channelInfoByUid);
        if (!channel.IsDeleted)
        {
          var channelList = (channel.SignalSource & SignalSource.Radio) != 0 ? radioChannels : tvChannels;
          DataRoot.AddChannel(channelList, channel);
        }
      }
    }

    #endregion

    #region GetQuery()

    private string GetQuery(string table, string[] fieldNames)
    {
      var sql = "select ";
      for (var i = 0; i < fieldNames.Length; i++)
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
      var field = new Dictionary<string, int>();
      for (var i = 0; i < fieldNames.Length; i++)
        field[fieldNames[i]] = i;
      return field;
    }

    #endregion


    #region Save()

    public override void Save()
    {
      var channelConnString = $"Data Source={this.workingDir + FILE_chmgt_db};Pooling=False";
      using (var conn = new SqliteConnection(channelConnString))
      {
        conn.Open();
        using var trans = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        using var cmd2 = conn.CreateCommand();
        WriteChannels(cmd, cmd2, "EuroATVChanList", atvChannels, true);
        WriteChannels(cmd, cmd2, "EuroDTVChanList", dtvTvChannels);
        WriteChannels(cmd, cmd2, "EuroDTVChanList", dtvRadioChannels);
        WriteChannels(cmd, cmd2, "EuroSATChanList", satTvChannels);
        WriteChannels(cmd, cmd2, "EuroSATChanList", satRadioChannels);
        trans.Commit();

        cmd.Transaction = null;
        RepairCorruptedDatabaseImage(cmd);
      }

      if (Path.GetExtension(this.FileName).ToLowerInvariant() == ".zip")
        ZipToOutputFile();
    }

    #endregion

    #region WriteChannels()

    private void WriteChannels(SqliteCommand cmd, SqliteCommand cmdDelete, string table, ChannelList channelList, bool analog = false)
    {
      var sql = "update " + table + " set channel_number=@nr ";
      if (!analog)
        sql += ", channel_order=@nr";
      sql += ", list_bits=@Bits where channel_handle=@id";
      cmd.CommandText = sql;
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", SqliteType.Integer);
      cmd.Parameters.Add("@nr", SqliteType.Integer);
      cmd.Parameters.Add("@Bits", SqliteType.Integer);
      cmd.Prepare();

      cmdDelete.CommandText = "delete from " + table + " where channel_handle=@id";
      cmdDelete.Parameters.Clear();
      cmdDelete.Parameters.Add("@id", SqliteType.Integer);
      cmdDelete.Prepare();

      foreach (var channelInfo in channelList.Channels)
      {
        if (channelInfo is not DbChannel channel) // ignore reference list proxy channels
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