using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using ChanSort.Api;
using ChanSort.Api.Utils;

namespace ChanSort.Loader.Amdb
{
  /*
   * This class loads amdb*.db files from unknown Android set-top-boxes.
   * The srv_table contains two columns which somehow represent an order, but neither is the actual program number as shown in the menu.
   * chan_num: seems to be grouped by service type (TV, Radio, Other) across satellites, each group starting at 1
   * chan_order: is probably the order in which the channels where found during the scan. mostly ordered by satellite, but not strictly
   * major_chan_num: always 0 (for DVB-S)
   * Regardless of these columns, the receiver displays each combination of TV/Radio and satellite as a separate list starting at 1
   */
  class AmdbSerializer : SerializerBase
  {
    private readonly HashSet<string> tableNames = new();

    private readonly IDictionary<Tuple<SignalSource, int>, ChannelList> listBySatellite = new ListDictionary<Tuple<SignalSource, int>, ChannelList>();
    private static readonly SignalSource[] SignalSources = { SignalSource.Tv, SignalSource.Radio, SignalSource.Data };
    private static readonly string[] SignalSourceNames = { "TV", "Radio", "Other" };

    private readonly Dictionary<int, int> onidByNetId = new Dictionary<int, int>();

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
    }
    #endregion

    #region Load()
    public override void Load()
    {
      string connString = $"Data Source=\"{this.FileName}\";Pooling=False";
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
      this.ReadNetworks(cmd);
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

        int i = 0;
        foreach (var ss in SignalSources)
        {
          var list = new ChannelList(SignalSource.Sat | ss, SignalSourceNames[i++] + " " + sat.Name);
          this.listBySatellite[Tuple.Create(ss, sat.Id)] = list;
        }
      }

      foreach (var entry in this.listBySatellite.OrderBy(e => e.Key.Item1))
      {
        if ((entry.Key.Item1 & SignalSource.Data) != 0)
          continue;
        this.DataRoot.AddChannelList(entry.Value);
      }
    }
    #endregion

    #region ReadNetworks()
    private void ReadNetworks(SqliteCommand cmd)
    {
      cmd.CommandText = "select db_id, network_id from net_table";
      using var r = cmd.ExecuteReader();
      while (r.Read())
        onidByNetId[r.GetInt32(0)] = r.GetInt32(1);
    }
    #endregion

    #region ReadTransponders()
    private void ReadTransponders(SqliteCommand cmd)
    {
      cmd.CommandText = "select db_id, db_sat_para_id, db_net_id, ts_id, freq, polar, symb from ts_table";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        int id = r.GetInt32(0);
        int satId = r.GetInt32(1);
        var netId = r.GetInt32(2);
        var tsid = r.GetInt32(3);
        int freq = r.GetInt32(4);

        if (this.DataRoot.Transponder.TryGet(id) != null)
          continue;
        Transponder tp = new Transponder(id);
        tp.TransportStreamId = tsid;
        tp.FrequencyInMhz = (int)(freq/1000);
        tp.Polarity = r.GetInt32(5) == 0 ? 'H' : 'V';
        tp.Satellite = this.DataRoot.Satellites.TryGet(satId);
        this.onidByNetId.TryGetValue(netId, out var onid);
        tp.OriginalNetworkId = onid;
        tp.SymbolRate = r.GetInt32(6) / 1000;
        this.DataRoot.AddTransponder(tp.Satellite, tp);
      }
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels(SqliteCommand cmd)
    {
      int ixP = 0;
      cmd.CommandText = @"
select 
  p.db_id, p.chan_num, p.name, p.service_id, p.vid_pid, p.pcr_pid, p.service_type, p.vid_fmt, p.free_ca_mode, p.lock, p.skip, p.hidden, p.db_net_id, p.db_ts_id, p.db_sat_para_id
from srv_table p
left outer join ts_table t on t.db_id=p.db_ts_id
order by t.db_sat_para_id, case p.service_type when 0 then 3 when 1 then 0 when 2 then 1 else p.service_type end, p.chan_num";

      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var handle = r.GetInt32(ixP + 0);
        var oldProgNr = r.GetInt32(ixP + 1);
        var name = r.GetString(ixP + 2);
        if (name.StartsWith("xxx"))
          name = name.Substring(3).Trim();

        ChannelInfo channel = new ChannelInfo(0, handle, 0, name);
        channel.RecordOrder = oldProgNr;
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

        var netDbId = r.GetInt32(ixP + 12);
        this.onidByNetId.TryGetValue(netDbId, out var onid);
        channel.OriginalNetworkId = onid;

        var tsDbId = r.GetInt32(ixP + 13);
        DataRoot.Transponder.TryGetValue(tsDbId, out var transponder);
        if (transponder != null)
        {
          channel.Transponder = transponder;
          channel.TransportStreamId = transponder.TransportStreamId;
          channel.Satellite = transponder.Satellite?.Name;
          channel.SatPosition = transponder.Satellite?.OrbitalPosition;
          channel.FreqInMhz = transponder.FrequencyInMhz;
          if (channel.FreqInMhz > 20000) // DVB-S is in MHz already, DVB-C/T in kHz
            channel.FreqInMhz /= 1000;
          channel.Polarity = transponder.Polarity;
          channel.SymbolRate = transponder.SymbolRate;
        }

        var satId = r.GetInt32(ixP + 14);
        var key = Tuple.Create(channel.SignalSource & SignalSource.MaskTvRadioData, satId);
        if (this.listBySatellite.TryGetValue(key, out var list))
        {
          channel.OldProgramNr = list.Count + 1;
          this.DataRoot.AddChannel(list, channel);
        }
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
      string channelConnString = $"Data Source=\"{this.FileName}\";Pooling=False";
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
      cmd.CommandText = "update srv_table set chan_num=@num, chan_order=@order, name=@name, skip=@skip, lock=@lock, hidden=@hide where db_id=@handle";
      cmd.Parameters.Add("@handle", SqliteType.Integer);
      cmd.Parameters.Add("@num", SqliteType.Integer);
      cmd.Parameters.Add("@order", SqliteType.Integer);
      cmd.Parameters.Add("@name", SqliteType.Text);
      cmd.Parameters.Add("@skip", SqliteType.Integer);
      cmd.Parameters.Add("@lock", SqliteType.Integer);
      cmd.Parameters.Add("@hide", SqliteType.Integer);
      cmd.Prepare();

      cmdDelete.CommandText = "delete from srv_table where db_id=@handle";
      cmdDelete.Parameters.Add("@handle", SqliteType.Integer);
      cmdDelete.Prepare();

      // combine all lists (including "others") into a single one
      IEnumerable<ChannelInfo> union = new List<ChannelInfo>();
      foreach (var list in this.listBySatellite.Values)
        union = union.Concat(list.Channels);

      var chanOrder = 1;
      var allChannels = union.OrderBy(SignalSourceOrder).ThenBy(ch => ch.NewProgramNr).ToList();
      foreach (ChannelInfo channel in allChannels)
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
          cmd.Parameters["@num"].Value = channel.NewProgramNr;
          cmd.Parameters["@order"].Value = chanOrder++;
          cmd.Parameters["@name"].Value = "xxx" + channel.Name;
          cmd.Parameters["@skip"].Value = channel.Skip ? 1 : 0;
          cmd.Parameters["@lock"].Value = channel.Lock ? 1 : 0;
          cmd.Parameters["@hide"].Value = channel.Hidden ? 1 : 0;
          cmd.ExecuteNonQuery();
        }
      }

      int SignalSourceOrder(ChannelInfo ch)
      {
        var ss = ch.SignalSource;
        if ((ss & SignalSource.Tv) != 0)
          return 0;
        if ((ss & SignalSource.Radio) != 0)
          return 1;
        return 2;
      }
    }
    #endregion
  }
}
