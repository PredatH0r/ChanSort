using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  /*
   * This class loads Toshiba files stores as CLONE00001\settingsDB.db along with settingsDBBackup.db
   * Currently only channel renaming, reordering and deletion is supported.
   * We don't know yet how/where information about favorites and skip/lock/hide is stored.
   *
   * Also, there are SatTable and SatTxTable for satellites and transponders in the file, but it's unknown
   * how these tables are linked with EASISerTable / DVBSerTable / ...
   */
  class SettingsDbSerializer : SerializerBase
  {
    private readonly ChannelList channels = new ChannelList(SignalSource.All, "All");

    private readonly HashSet<string> tableNames = new();

    #region ctor()
    public SettingsDbSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = false;
      this.Features.CanHideChannels = false;
      this.Features.FavoritesMode = FavoritesMode.None;

      this.DataRoot.AddChannelList(this.channels);
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Lock));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Skip));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Hidden));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Encrypted));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.PcrPid));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.VideoPid));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Satellite));
      channels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
    }
    #endregion

    #region GetDataFilePaths()
    public override IEnumerable<string> GetDataFilePaths()
    {
      var list = new List<string>();
      list.Add(this.FileName);
      var backupFile = GetBackupFilePath();
      if (File.Exists(backupFile))
        list.Add(backupFile);
      return list;
    }

    private string GetBackupFilePath()
    {
      var dir = Path.GetDirectoryName(this.FileName) ?? ".";
      var name = Path.GetFileNameWithoutExtension(this.FileName);
      var ext = Path.GetExtension(this.FileName);
      var backupFile = Path.Combine(dir, name + "Backup" + ext);
      return backupFile;
    }

    #endregion

    #region Load()
    public override void Load()
    {
      string sysDataConnString = $"Data Source={this.FileName};Pooling=False";
      using var conn = new SqliteConnection(sysDataConnString);
      conn.Open();
      
      using var cmd = conn.CreateCommand();

      this.RepairCorruptedDatabaseImage(cmd);

      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          this.tableNames.Add(r.GetString(0).ToLowerInvariant());
      }

      if (!this.tableNames.Contains("easisertable"))
        throw LoaderException.TryNext("File doesn't contain the expected tables");

      this.ReadSatellites(cmd);
      this.ReadTransponders(cmd);
      this.ReadChannels(cmd);
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
      cmd.CommandText = "select m_id, m_name_serialized, m_orbital_position from SatTable";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        Satellite sat = new Satellite(r.GetInt32(0));
        string eastWest = "E";
        int pos = r.GetInt32(2);
        if (pos < 0)
        {
          pos = -pos;
          eastWest = "W";
        }
        sat.OrbitalPosition = $"{pos / 10}.{pos % 10}{eastWest}";
        sat.Name = r.GetString(1);
        this.DataRoot.AddSatellite(sat);
      }
    }
    #endregion

    #region ReadTransponders()
    private void ReadTransponders(SqliteCommand cmd)
    {
      cmd.CommandText = "select m_id, m_satellite_id, m_frequency, m_polarisation, m_symbol_rate from SatTxTable";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        int id = r.GetInt32(0);
        int satId = r.GetInt32(1);
        int freq = r.GetInt32(2);
        
        if (this.DataRoot.Transponder.TryGet(id) != null)
          continue;
        Transponder tp = new Transponder(id);
        tp.FrequencyInMhz = freq;
        tp.Polarity = r.GetInt32(3) == 0 ? 'H' : 'V';
        tp.Satellite = this.DataRoot.Satellites.TryGet(satId);
        tp.SymbolRate = r.GetInt32(4);
        this.DataRoot.AddTransponder(tp.Satellite, tp);
      }
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels(SqliteCommand cmd)
    {
      int ixE = 0;
      int ixD = 3;
      int ixA = 9;
      int ixDC = 10;
      int ixT = 12;

      var hasTADTunerDataTable = this.tableNames.Contains("tadtunerdatatable");
      string analogTunerFields = hasTADTunerDataTable ? ",t.frequency_in_multiples_of_10Hz" : "";
      string analogTunerTable = hasTADTunerDataTable ? " left outer join TADTunerDataTable t on t.channel=ac.channel_no" : "";
      cmd.CommandText = $@"
select 
  e.m_handle, e.m_rsn, e.m_name_serialized, 
  d.m_onid, d.m_tsid, d.m_id, d.m_type, d.m_name_serialized, d.m_provider_serialized,
  a.m_name_serialized,
  dc.frequency, dc.symbol_rate
  {analogTunerFields}
from EASISerTable e 
left outer join DVBSerTable d on d.m_handle=e.m_handle
left outer join AnalogSerTable a on a.m_handle=e.m_handle
left outer join ChanDataTable dc on dc.handle=d.m_channel_no
left outer join ChanDataTable ac on ac.handle=a.m_channel_no
{analogTunerTable}";

      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var handle = r.GetInt32(ixE + 0);
        var oldProgNr = r.GetInt32(ixE + 1);
        var name = r.GetString(ixE + 2);
        ChannelInfo channel = new ChannelInfo(0, handle, oldProgNr, name);

        // DVB
        if (!r.IsDBNull(ixD + 0))
        {
          channel.OriginalNetworkId = r.GetInt32(ixD + 0) & 0x7FFF;
          channel.TransportStreamId = r.GetInt32(ixD + 1) & 0x7FFF;
          channel.ServiceId = r.GetInt32(ixD + 2) & 0x1FFF;
          channel.ServiceType = r.GetInt32(ixD + 3) & 0x1FFF;
          channel.Provider = r.GetString(ixD + 5);
          channel.FreqInMhz = (decimal) r.GetInt32(ixDC + 0) / 1000;
          channel.SymbolRate = r.GetInt32(ixDC + 1);
          if (channel.FreqInMhz > 10000)
            channel.FreqInMhz = (int) channel.FreqInMhz;
        }

        // analog
        if (!r.IsDBNull(ixA + 0) && hasTADTunerDataTable)
        {
          channel.FreqInMhz = (decimal) r.GetInt32(ixT + 0) / 100000;
        }

        if (!channel.IsDeleted)
          this.DataRoot.AddChannel(this.channels, channel);
      }
    }
    #endregion


    #region Save()
    public override void Save()
    {
      string channelConnString = $"Data Source={this.FileName};Pooling=False";
      using (var conn = new SqliteConnection(channelConnString))
      {
        conn.Open();
        using var trans = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        using var cmd2 = conn.CreateCommand();

        this.WriteChannels(cmd, cmd2, this.channels);
        trans.Commit();

        cmd.Transaction = null;
        this.RepairCorruptedDatabaseImage(cmd);
      }

      // copy settingsDB.db to settingsDBBackup.db
      var backupFile = GetBackupFilePath();
      File.Copy(this.FileName, backupFile, true);
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SqliteCommand cmd, SqliteCommand cmdDelete, ChannelList channelList)
    {
      cmd.CommandText = "update EASISerTable set m_rsn=@nr, m_name_serialized=@name where m_handle=@handle";
      cmd.Parameters.Add("@handle", SqliteType.Integer);
      cmd.Parameters.Add("@nr", SqliteType.Integer);
      cmd.Parameters.Add("@name", SqliteType.Text);
      cmd.Prepare();

      cmdDelete.CommandText = @"
delete from EASISerTable where m_handle=@handle; 
delete from DVBSerTable where m_handle=@handle; 
delete from AnalogSerTable where m_handle=@handle;
";
      cmdDelete.Parameters.Add("@handle", SqliteType.Integer);
      cmdDelete.Prepare();

      foreach (ChannelInfo channel in channelList.Channels)
      {
        if (channel.IsProxy) // ignore reference list proxy channels
          continue;

        if (channel.IsDeleted)
        {
          cmdDelete.Parameters["@handle"].Value = channel.RecordIndex;
          cmdDelete.ExecuteNonQuery();
        }
        else
        {
          channel.UpdateRawData();
          cmd.Parameters["@handle"].Value = channel.RecordIndex;
          cmd.Parameters["@nr"].Value = channel.NewProgramNr;
          cmd.Parameters["@name"].Value = channel.Name;
          cmd.ExecuteNonQuery();
        }
      }
    }
    #endregion
  }
}
