#define WITH_FAVORITES

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.VisionEdge4K
{
  /*
   * This class loads the database.db file from Vision EDGE 4K set-top-boxes.
   * Currently only "satellite_transponder_table" is supported, but there are databases with additional "cable_transponder_table" and "terrestrial_transponder_table" tables.
   * All these transponder tables have tp_id values starting at 0, which makes it unclear how to reference them from within program_table.
   * The guess is that program_table.tp_type=0 means satellite.
   *
   * The disp_order stored in the database is not used directly by the STB. Instead it only orders the DB results by it and creates internal lists for each satellite, all starting at 1.
   */
  class VisionEdge4KSerializer : SerializerBase
  {
    private readonly HashSet<string> tableNames = new();
    private readonly List<int> favListIds = new();
    private readonly Dictionary<int, int> favListIndexById = new();

    private readonly Dictionary<int, ChannelList> channels = new();
    private readonly ChannelList favs = new(SignalSource.All, "Fav") { IsMixedSourceFavoritesList = true };


    #region ctor()
    public VisionEdge4KSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.CanHaveGaps = false;
      this.Features.AllowGapsInFavNumbers = false;
#if WITH_FAVORITES
      this.Features.FavoritesMode = FavoritesMode.MixedSource;
#else
      this.Features.FavoritesMode = FavoritesMode.None;
#endif
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

      if (new[] { "satellite_table", "satellite_transponder_table", "program_table", "fav_prog_table" }.Any(tbl => !tableNames.Contains(tbl)))
        throw LoaderException.TryNext("File doesn't contain the expected tables");

      this.ReadSatellites(cmd);
      this.ReadTransponders(cmd);
      this.ReadChannels(cmd);
#if WITH_FAVORITES
      this.ReadFavorites(cmd);
#endif
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
      cmd.CommandText = "select id, name, angle from satellite_table order by disp_order";
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

        var list = new ChannelList(SignalSource.Sat | SignalSource.Digital | SignalSource.MaskTvRadioData, sat.Name);
        this.channels.Add(sat.Id, list);
        this.DataRoot.AddChannelList(list);
      }
    }
    #endregion

    #region ReadTransponders()
    private void ReadTransponders(SqliteCommand cmd)
    {
      cmd.CommandText = "select id, sat_id, freq, pol, sym_rate from satellite_transponder_table";
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
      int ixP = 0;
      int ixST = ixP + 12;

      cmd.CommandText = @"
select 
  p.id, p.disp_order, p.name, p.service_id, p.vid_pid, p.pcr_pid, p.vid_type, p.tv_type, p.ca_type, p.lock, p.skip, p.hide,
  st.sat_id, st.on_id, st.ts_id, st.freq, st.pol, st.sym_rate
from program_table p
left outer join satellite_transponder_table st on p.tp_type=0 and st.id=p.tp_id
order by p.tv_type,p.disp_order";

      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var handle = r.GetInt32(ixP + 0);
        var oldProgNr = r.GetInt32(ixP + 1);
        var name = r.GetString(ixP + 2);
        ChannelInfo channel = new ChannelInfo(0, handle, oldProgNr, name);
        channel.ServiceId = r.GetInt32(ixP + 3) & 0x7FFF;
        channel.VideoPid = r.GetInt32(ixP + 4);
        channel.PcrPid = r.GetInt32(ixP + 5);
        var vidType = r.GetInt32(ixP + 6);
        var tvType = r.GetInt32(ixP + 7);
        if (tvType == 0)
        {
          channel.ServiceType = vidType;
          channel.ServiceTypeName = "TV";
          channel.SignalSource |= SignalSource.Tv;
        }
        else
        {
          channel.ServiceType = 0;
          channel.ServiceTypeName = "Radio/Data";
          channel.SignalSource |= SignalSource.Radio | SignalSource.Data;
        }
        channel.Encrypted = r.GetInt32(ixP + 8) != 0;
        channel.Lock = r.GetBoolean(ixP + 9);
        channel.Skip = r.GetBoolean(ixP + 10);
        channel.Hidden = r.GetBoolean(ixP + 11);

        // DVB-S
        int satId = 0;
        if (!r.IsDBNull(ixST + 0))
        {
          satId = r.GetInt32(ixST + 0);
          var sat = this.DataRoot.Satellites.TryGet(satId);
          channel.Satellite = sat?.Name;
          channel.SatPosition = sat?.OrbitalPosition;
          channel.OriginalNetworkId = r.GetInt32(ixST + 1) & 0x7FFF;
          channel.TransportStreamId = r.GetInt32(ixST + 2) & 0x7FFF;
          channel.FreqInMhz = r.GetInt32(ixST + 3);
          if (channel.FreqInMhz > 20000) // DVB-S is in MHz already, DVB-C/T in kHz
            channel.FreqInMhz /= 1000;
          channel.Polarity = r.GetInt32(ixST + 4) == 0 ? 'H' : 'V';
          channel.SymbolRate = r.GetInt32(ixST + 5);
        }

        var list = this.channels.TryGet(satId);
        if (list != null)
        {
          channel.OldProgramNr = list.Channels.Count + 1;
          this.DataRoot.AddChannel(list, channel);
          this.DataRoot.AddChannel(this.favs, channel);
        }
      }
    }
    #endregion

    #region ReadFavorites

    private void ReadFavorites(SqliteCommand cmd)
    {
      this.Features.MaxFavoriteLists = 0;
      cmd.CommandText = "select id, fav_name from fav_name_table order by id";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = r.GetInt32(0);
          favListIds.Add(id);
          favListIndexById[id] = this.Features.MaxFavoriteLists;
          this.favs.SetFavListCaption(this.Features.MaxFavoriteLists, r.GetString(1));
          ++this.Features.MaxFavoriteLists;
        }
      }

      cmd.CommandText = "select fav_group_id, prog_id, disp_order, tv_type from fav_prog_table order by disp_order";
      var lastProgNr = new Dictionary<int, int>();
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var chan = this.favs.GetChannelById(r.GetInt32(1));
          if (chan == null)
            continue;
          if (!this.favListIndexById.TryGetValue(r.GetInt32(0), out var idx))
            continue;

          lastProgNr.TryGetValue(idx, out var nr);
          lastProgNr[idx] = ++nr;
          chan.SetOldPosition(idx + 1, nr);
        }
      }
    }
    #endregion

    #region AdjustColumns()
    private void AdjustColumns()
    {
      this.DataRoot.AddChannelList(this.favs);
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
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
#if WITH_FAVORITES
      this.WriteFavorites(cmd);
#endif
      trans.Commit();

      cmd.Transaction = null;
      this.RepairCorruptedDatabaseImage(cmd);
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SqliteCommand cmd, SqliteCommand cmdDelete)
    {
      cmd.CommandText = "update program_table set disp_order=@nr, name=@name, skip=@skip, lock=@lock, hide=@hide, fav=@fav where id=@handle";
      cmd.Parameters.Add("@handle", SqliteType.Integer);
      cmd.Parameters.Add("@nr", SqliteType.Integer);
      cmd.Parameters.Add("@name", SqliteType.Text);
      cmd.Parameters.Add("@skip", SqliteType.Integer);
      cmd.Parameters.Add("@lock", SqliteType.Integer);
      cmd.Parameters.Add("@hide", SqliteType.Integer);
      cmd.Parameters.Add("@fav", SqliteType.Integer);
      cmd.Prepare();

      cmdDelete.CommandText = "delete from program_table where id=@handle; delete from fav_prog_table where prog_id=@handle;";
      cmdDelete.Parameters.Add("@handle", SqliteType.Integer);
      cmdDelete.Prepare();

      int dispOrder = 1;
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
            cmd.Parameters["@nr"].Value = dispOrder++;
            cmd.Parameters["@name"].Value = channel.Name;
            cmd.Parameters["@skip"].Value = channel.Skip ? 1 : 0;
            cmd.Parameters["@lock"].Value = channel.Lock ? 1 : 0;
            cmd.Parameters["@hide"].Value = channel.Hidden ? 1 : 0;
            cmd.Parameters["@fav"].Value = channel.Favorites == 0 ? 0 : 1;
            cmd.ExecuteNonQuery();
          }
        }
      }
    }
    #endregion

    #region WriteFavorites()
    private void WriteFavorites(SqliteCommand cmd)
    {
      cmd.Parameters.Clear();
      cmd.CommandText = "delete from fav_prog_table";
      cmd.ExecuteNonQuery();

      cmd.CommandText = "insert into fav_prog_table(prog_id, fav_group_id, disp_order, tv_type) values (@progid, @groupid, @order, @type)";
      cmd.Parameters.Add("@progid", SqliteType.Integer);
      cmd.Parameters.Add("@groupid", SqliteType.Integer);
      cmd.Parameters.Add("@order", SqliteType.Integer);
      cmd.Parameters.Add("@type", SqliteType.Integer);

      int favProgNr = 1; // the TV maintains continuous numbers over all favorite lists, not per-list

      int max = this.Features.MaxFavoriteLists;
      for (int i = 0; i < max; i++)
      {
        // the particular priorities of sorting by fav-list, tv/radio, satellite is unclear
        // the sample file only shows that grouping by fav comes before sat, but the role of tv/radio is ambiguous
        foreach (var chan in favs.Channels.OrderBy(ch => (ch.SignalSource & SignalSource.Tv) != 0 ? 0 : 1).ThenBy(ch => ch.GetPosition(i + 1)))
        {
          if (chan.IsProxy)
            continue;
          var num = chan.GetPosition(i + 1);
          if (num <= 0)
            continue;
          cmd.Parameters["@progid"].Value = chan.RecordIndex;
          cmd.Parameters["@groupid"].Value = this.favListIds[i];
          cmd.Parameters["@order"].Value = favProgNr++;
          cmd.Parameters["@type"].Value = (chan.SignalSource & SignalSource.Tv) != 0 ? 0 : 1;
          cmd.ExecuteNonQuery();
        }
      }
    }
    #endregion
  }
}
