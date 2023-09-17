using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.Amdb
{
  /*
   * This class loads amdb*.db files from unknown Android set-top-boxes.
   */
  class AmdbSerializer : SerializerBase
  {
    private readonly HashSet<string> tableNames = new();

    private readonly ChannelList tv = new (SignalSource.Tv, "TV");
    private readonly ChannelList radio = new(SignalSource.Radio, "Radio");
    private readonly ChannelList data = new(SignalSource.Data, "Data");

    #region ctor()
    public AmdbSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.CanHaveGaps = false;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.FavoritesMode = FavoritesMode.None;

      this.DataRoot.AddChannelList(tv);
      this.DataRoot.AddChannelList(radio);
      this.DataRoot.AddChannelList(data);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      string connString = $"Data Source={this.FileName};Pooling=False";
      using var conn = new SqliteConnection(connString);
      conn.Open();

      using var cmd = conn.CreateCommand();

      this.RepairCorruptedDatabaseImage(cmd);

      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          this.tableNames.Add(r.GetString(0).ToLowerInvariant());
      }

      if (new[] { "sat_para_table", "ts_table", "srv_table" }.Any(tbl => !tableNames.Contains(tbl)))
        throw LoaderException.TryNext("File doesn't contain the expected tables");

      this.ReadSatellites(cmd);
      this.ReadTransponders(cmd);
      this.ReadChannels(cmd);
      this.AdjustColumns();
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
      cmd.CommandText = "select db_id, sat_name, sat_longitude from sat_para_table order by db_id";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        Satellite sat = new Satellite(r.GetInt32(0));
        string eastWest = "E";
        int pos = r.GetInt32(2);
        // i haven't seen a file containing satellites on the west side. could be either negative or > 180°
        if (pos < 0)
        {
          pos = -pos;
          eastWest = "W";
        }
        else if (pos > 180)
        {
          pos = 360 - pos;
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
      cmd.CommandText = "select db_id, db_sat_para_id, freq, polar, symb from ts_table";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        int id = r.GetInt32(0);
        int satId = r.GetInt32(1);
        int freq = r.GetInt32(2);

        if (this.DataRoot.Transponder.TryGet(id) != null)
          continue;
        Transponder tp = new Transponder(id);
        tp.FrequencyInMhz = (int)(freq/1000);
        tp.Polarity = r.GetInt32(3) == 0 ? 'H' : 'V';
        tp.Satellite = this.DataRoot.Satellites.TryGet(satId);
        tp.SymbolRate = r.GetInt32(4) / 1000;
        this.DataRoot.AddTransponder(tp.Satellite, tp);
      }
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels(SqliteCommand cmd)
    {
      int ixP = 0;
      int ixST = ixP + 12;

      cmd.CommandText = @"
select 
  p.db_id, p.chan_num, p.name, p.service_id, p.vid_pid, p.pcr_pid, p.service_type, p.vid_fmt, p.free_ca_mode, p.lock, p.skip, p.hidden,
  t.db_sat_para_id, 0, t.ts_id, t.freq, t.polar, t.symb
from srv_table p
left outer join ts_table t on t.db_id=p.db_ts_id
order by p.chan_num";

      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var handle = r.GetInt32(ixP + 0);
        var oldProgNr = r.GetInt32(ixP + 1);
        var name = r.GetString(ixP + 2);
        if (name.StartsWith("xxx"))
          name = name.Substring(3);
        ChannelInfo channel = new ChannelInfo(0, handle, oldProgNr, name);
        channel.ServiceId = r.GetInt32(ixP + 3) & 0x7FFF;
        channel.VideoPid = r.GetInt32(ixP + 4);
        channel.PcrPid = r.IsDBNull(ixP + 5) ? 0 : r.GetInt32(ixP + 5);
        var serviceType = r.GetInt32(ixP + 6);
        var vidFmt = r.GetInt32(ixP + 7);
        channel.ServiceType = serviceType;
        if (serviceType == 1)
        {
          channel.ServiceTypeName = vidFmt == 2 ? "HD TV" : "TV";
          channel.SignalSource |= SignalSource.Tv;
        }
        else if (serviceType == 2)
        {
          channel.ServiceTypeName = "Radio";
          channel.SignalSource |= SignalSource.Radio;
        }
        else
        {
          channel.ServiceTypeName = "Data";
          channel.SignalSource |= SignalSource.Data;
        }
        channel.Encrypted = r.GetInt32(ixP + 8) != 0;
        channel.Lock = r.GetBoolean(ixP + 9);
        channel.Skip = r.GetBoolean(ixP + 10);
        channel.Hidden = r.GetBoolean(ixP + 11);

        // DVB-S
        if (!r.IsDBNull(ixST + 0))
        {
          var satId = r.GetInt32(ixST + 0);
          var sat = this.DataRoot.Satellites.TryGet(satId);
          channel.Satellite = sat?.Name;
          channel.SatPosition = sat?.OrbitalPosition;
          channel.OriginalNetworkId = r.GetInt32(ixST + 1) & 0x7FFF;
          channel.TransportStreamId = r.GetInt32(ixST + 2) & 0x7FFF;
          channel.FreqInMhz = r.GetInt32(ixST + 3);
          if (channel.FreqInMhz > 20000) // DVB-S is in MHz already, DVB-C/T in kHz
            channel.FreqInMhz /= 1000;
          channel.Polarity = r.GetInt32(ixST + 4) == 0 ? 'H' : 'V';
          channel.SymbolRate = r.GetInt32(ixST + 5)/1000;
        }

        var list = this.DataRoot.GetChannelList(channel.SignalSource);
        if (list != null)
          this.DataRoot.AddChannel(list, channel);
      }
    }
    #endregion

    #region AdjustColumns()
    private void AdjustColumns()
    {
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.NetworkName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.NetworkOperator));
      }
    }
    #endregion


    #region Save()
    public override void Save()
    {
      string channelConnString = $"Data Source={this.FileName};Pooling=False";
      using var conn = new SqliteConnection(channelConnString);
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();
      using var cmd2 = conn.CreateCommand();

      this.WriteChannels(cmd, cmd2);
      trans.Commit();

      cmd.Transaction = null;
      this.RepairCorruptedDatabaseImage(cmd);
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SqliteCommand cmd, SqliteCommand cmdDelete)
    {
      cmd.CommandText = "update srv_table set chan_num=@nr, chan_order=@nr, name=@name, skip=@skip, lock=@lock, hidden=@hide where db_id=@handle";
      cmd.Parameters.Add("@handle", SqliteType.Integer);
      cmd.Parameters.Add("@nr", SqliteType.Integer);
      cmd.Parameters.Add("@name", SqliteType.Text);
      cmd.Parameters.Add("@skip", SqliteType.Integer);
      cmd.Parameters.Add("@lock", SqliteType.Integer);
      cmd.Parameters.Add("@hide", SqliteType.Integer);
      cmd.Prepare();

      cmdDelete.CommandText = "delete from srv_table where db_id=@handle";
      cmdDelete.Parameters.Add("@handle", SqliteType.Integer);
      cmdDelete.Prepare();

      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (ChannelInfo channel in list.Channels.OrderBy(ch => (ch.SignalSource & SignalSource.Tv) != 0 ? 0 : 1).ThenBy(ch => ch.NewProgramNr))
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
            cmd.Parameters["@name"].Value = "xxx" + channel.Name;
            cmd.Parameters["@skip"].Value = channel.Skip ? 1 : 0;
            cmd.Parameters["@lock"].Value = channel.Lock ? 1 : 0;
            cmd.Parameters["@hide"].Value = channel.Hidden ? 1 : 0;
            cmd.ExecuteNonQuery();
          }
        }
      }
    }
    #endregion
  }
}
