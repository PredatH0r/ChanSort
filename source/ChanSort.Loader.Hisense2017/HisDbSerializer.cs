//#define LOCK_LCN_LISTS

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ChanSort.Api;

namespace ChanSort.Loader.Hisense
{
  public class HisDbSerializer : SerializerBase
  {
    private readonly Dictionary<long, ChannelInfo> channelsById = new Dictionary<long, ChannelInfo>();
    private readonly Dictionary<int, ChannelList> channelLists = new Dictionary<int, ChannelList>();
    private ChannelList favlist;
    private readonly Dictionary<int,int> favListIdToFavIndex = new Dictionary<int, int>();
    private List<string> tableNames;

    private static readonly List<string> ColumnNames = new List<string>
      {
        "OldPosition",
        "Position",
        "Source",
        "NewProgramNr",
        "Name",
        "ShortName",
        "Favorites",
        "Lock",
        "Hidden",
        "Encrypted",
        "FreqInMhz",
        "OriginalNetworkId",
        "TransportStreamId",
        "ServiceId",
        "ServiceType",
        "ServiceTypeName",
        "NetworkName",
        "Satellite",
        "SymbolRate"
      };

    public class HisTransponder : Transponder
    {
      public SignalSource SignalSource { get; set; }
      public string Source { get; set; }

      public HisTransponder(int id) : base(id)
      {
      }
    }

    #region ctor()

    public HisDbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();

      Features.ChannelNameEdit = ChannelNameEditMode.All;
      Features.CanDeleteChannels = false;
      Features.CanSkipChannels = false;
      Features.CanHaveGaps = true;
      DataRoot.SortedFavorites = true;
    }

    #endregion

    public override string DisplayName => "Hisense servicelist.db Loader";

    #region Load()

    public override void Load()
    {
      using (var conn = new SQLiteConnection("Data Source=" + FileName))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          RepairCorruptedDatabaseImage(cmd);
          LoadLists(cmd);
          LoadTableNames(cmd);
          LoadSatelliteData(cmd);
          LoadTunerData(cmd);
          LoadServiceData(cmd);
          LoadFavorites(cmd);
        }
      }

      if (channelsById.Count == 0)
        MessageBox.Show(Resources.Load_NoChannelsMsg, Resources.Load_NoChannelsCaption, MessageBoxButtons.OK);
    }

    #endregion

    #region RepairCorruptedDatabaseImage()

    private void RepairCorruptedDatabaseImage(SQLiteCommand cmd)
    {
      cmd.CommandText = "REINDEX";
      cmd.ExecuteNonQuery();
    }

    #endregion

    #region LoadTableNames()

    private void LoadTableNames(SQLiteCommand cmd)
    {
      tableNames = new List<string>();
      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' order by name";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          tableNames.Add(r.GetString(0).ToLower());
      }
    }

    #endregion

    #region LoadLists()
    private void LoadLists(SQLiteCommand cmd)
    {
      cmd.CommandText = "select Pid, Name from FavoriteList";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          int listId = r.GetInt32(0);
          string name = r.GetString(1);
          if (name.StartsWith("FAV"))
          {
            favListIdToFavIndex.Add(listId, int.Parse(name.Substring(3)));
            continue;
          }

          var list = new ChannelList(SignalSource.Analog | SignalSource.AvInput | SignalSource.DvbCT | SignalSource.DvbS | SignalSource.TvAndRadio, name);
          list.VisibleColumnFieldNames = ColumnNames;
          list.IsMixedSourceFavoritesList = list.Caption.StartsWith("FAV");

          channelLists.Add(listId, list);
          DataRoot.AddChannelList(list);
        }


        favlist = new ChannelList(SignalSource.Analog | SignalSource.AvInput | SignalSource.DvbCT | SignalSource.DvbS | SignalSource.TvAndRadio, "Favorites");
        favlist.VisibleColumnFieldNames = ColumnNames;
        favlist.IsMixedSourceFavoritesList = true;
        channelLists.Add(0, favlist);
        DataRoot.AddChannelList(favlist);
      }
    }
    #endregion


    #region LoadSatelliteData()


    private void LoadSatelliteData(SQLiteCommand cmd)
    {
      // sample data file doesn't contain any satellite information
#if false
      var regex = new Regex(@"^satellite$");
      foreach (var tableName in this.tableNames)
      {
        if (!regex.IsMatch(tableName))
          continue;
        cmd.CommandText = "select satl_rec_id, i2_orb_pos, ac_sat_name from " + tableName;
        using (var r = cmd.ExecuteReader())
        {
          while (r.Read())
          {
            var sat = new Satellite(r.GetInt32(0));
            var pos = r.GetInt32(1);
            sat.OrbitalPosition = $"{(decimal) Math.Abs(pos)/10:n1}{(pos < 0 ? 'W' : 'E')}";
            sat.Name = r.GetString(2);
            this.DataRoot.AddSatellite(sat);
          }
        }
      }
#endif
    }

    #endregion


    #region LoadTunerData()

    private void LoadTunerData(SQLiteCommand cmd)
    {
      List<Tuple<string,SignalSource,string>> inputs = new List<Tuple<string, SignalSource, string>>
      {
        Tuple.Create("C", SignalSource.DvbC, "symbolrate"),
        Tuple.Create("C2", SignalSource.DvbC, "bandwidth"),
        Tuple.Create("S", SignalSource.DvbS, "symbolrate"),
        Tuple.Create("S2", SignalSource.DvbS, "symbolrate"),
        Tuple.Create("T", SignalSource.DvbT, "bandwidth"),
        Tuple.Create("T2", SignalSource.DvbT, "bandwidth"),
      };
      foreach (var input in inputs)
      {
        var table = input.Item1;
        var symrate = input.Item3;
        LoadTunerData(cmd, "DVB" + table + "Tuner", ", Frequency," + symrate, (t, r, i0) =>
        {
          t.Source = "DVB-" + input.Item1;
          t.SignalSource = input.Item2;
          t.FrequencyInMhz = (decimal) r.GetInt32(i0 + 0) / 1000;
          t.SymbolRate = r.GetInt32(i0 + 1);
        });
      }

#if false
      this.LoadTunerData(cmd, "tsl_#_data_sat_dig", ", freq, sym_rate, orb_pos", (t, r, i0) =>
      {
        t.FrequencyInMhz = r.GetInt32(i0 + 0);
        t.SymbolRate = r.GetInt32(i0 + 1);

        // satellite information may or may not be available in the database. if there is none, create a proxy sat records from the orbital position in the TSL data
        if (t.Satellite == null)
        {
          var opos = r.GetInt32(i0 + 2);
          var sat = this.DataRoot.Satellites.TryGet(opos);
          if (sat == null)
          {
            sat = new Satellite(opos);
            var pos = (decimal) opos / 10;
            sat.Name = pos < 0 ? (-pos).ToString("n1") + "W" : pos.ToString("n1") + "E";
          }
          t.Satellite = sat;
        }
      });
#endif
    }

    private void LoadTunerData(SQLiteCommand cmd, string joinTable, string joinFields, Action<HisTransponder, SQLiteDataReader, int> enhanceTransponderInfo)
    {
      if (!tableNames.Contains(joinTable.ToLower()))
        return;

      cmd.CommandText = $"select tuner.tunerid, oid, tid, satellite {joinFields} "
                        + $" from tuner inner join {joinTable} on {joinTable}.tunerid=tuner.tunerid";

      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = r.GetInt32(0);
          var trans = new HisTransponder(id);
          trans.OriginalNetworkId = r.GetInt32(1);
          trans.TransportStreamId = r.GetInt32(2);
          trans.Satellite = DataRoot.Satellites.TryGet(r.GetInt32(3));

          enhanceTransponderInfo(trans, r, 4);

          DataRoot.AddTransponder(trans.Satellite, trans);
        }
      }
    }

    #endregion

    #region LoadServiceData()

    private void LoadServiceData(SQLiteCommand cmd)
    {
      cmd.CommandText = @"
select s.pid, s.type, anls.Frequency, digs.TunerId, digs.Sid, Name, ShortName, Encrypted, Visible, Selectable, ParentalLock
from service s
left outer join AnalogService anls on anls.ServiceId=s.Pid
left outer join DVBService digs on digs.ServiceId=s.Pid
";

      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          ChannelInfo ci = null;
          if (!r.IsDBNull(2))
            ci = new ChannelInfo(SignalSource.Analog, r.GetInt32(0), -1, r.GetString(5));
          else if (!r.IsDBNull(3))
          {
            var trans = (HisTransponder)DataRoot.Transponder.TryGet(r.GetInt32(3));
            ci = new ChannelInfo(trans.SignalSource, r.GetInt32(0), -1, r.GetString(5));
            ci.Transponder = trans;
            ci.FreqInMhz = trans.FrequencyInMhz;
            ci.OriginalNetworkId = trans.OriginalNetworkId;
            ci.TransportStreamId = trans.TransportStreamId;
            ci.Source = trans.Source;
            ci.ServiceId = r.GetInt32(4);
            ci.ShortName = r.GetString(6);
            ci.Encrypted = r.GetInt32(7) != 0;
            ci.Hidden = r.GetInt32(8) == 0;
            ci.Skip = r.GetInt32(9) == 0;
            ci.Lock = r.GetInt32(10) != 0;
          }
          else if (r.GetInt32(1) == 0)
            ci = new ChannelInfo(SignalSource.AvInput, r.GetInt32(0), -1, r.GetString(5));

          if (ci != null)
            channelsById.Add(ci.RecordIndex, ci);
        }
      }
#if LOCK_LCN_LISTS
// make the current list read-only if LCN is used
        if (r.GetInt32(i0 + 3) != 0)
        {
          this.channelLists[x - 1].ReadOnly = true;
        }
#endif

    }
#if false
    private void LoadServiceData(SQLiteCommand cmd, string joinTable, string joinFields, Action<ChannelInfo, SQLiteDataReader, int> enhanceChannelInfo)
    {
      if (!tableNames.Contains(joinTable))
        return;

      cmd.CommandText = $"select service.pid, -1,  {joinFields}"
                        + $" from service inner join {joinTable} on {joinTable}.ServiceId=";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = (uint)r.GetInt32(0);
          var prNr = (int)(uint)r.GetInt32(1) >> 18;
          var trans = DataRoot.Transponder.TryGet((r.GetInt32(2) << 16) | r.GetInt32(3));
          var stype = (ServiceType)r.GetInt32(4);
          var name = r.GetString(5);
          var nwMask = (NwMask)r.GetInt32(6);
          var sid = r.GetInt32(7);
          var bmedium = (BroadcastMedium)r.GetInt32(8);

          var ssource = DetermineSignalSource(bmedium, stype);
          var ci = new ChannelInfo(ssource, id, prNr, name);
          if (trans != null)
          {
            ci.Transponder = trans;
            ci.OriginalNetworkId = trans.OriginalNetworkId;
            ci.TransportStreamId = trans.TransportStreamId;
            ci.SymbolRate = trans.SymbolRate;
            ci.FreqInMhz = trans.FrequencyInMhz;
            ci.Satellite = trans.Satellite?.ToString();
          }

          ci.ServiceId = sid;

          //ci.Skip = (nwMask & NwMask.Active) == 0;
          ci.Lock = (nwMask & NwMask.Lock) != 0;
          ci.Hidden = (nwMask & NwMask.Visible) == 0;
          ci.Favorites |= (Favorites)((int)(nwMask & (NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4)) >> 4);

          if (stype == ServiceType.Radio)
            ci.ServiceTypeName = "Radio";
          else if (stype == ServiceType.Tv)
            ci.ServiceTypeName = "TV";
          else if (stype == ServiceType.App)
            ci.ServiceTypeName = "Data";

          enhanceChannelInfo(ci, r, 9);

          var list = channelLists[tableNr - 1];
          ci.Source = list.ShortCaption;
          DataRoot.AddChannel(list, ci);

          // add the channel to all favorites lists
          DataRoot.AddChannel(channelLists[6], ci);
          channelsById[ci.RecordIndex] = ci;
        }
      }
    }
#endif
    #endregion

    #region LoadFavorites()

    private void LoadFavorites(SQLiteCommand cmd)
    {
      cmd.CommandText = "select FavoriteId, ServiceId, ChannelNum from FavoriteItem fi";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          int favListId = r.GetInt32(0);
          var ci = channelsById.TryGet(r.GetInt32(1));
          int favListIdx = favListIdToFavIndex.TryGet(favListId);
          if (favListIdx != 0)
          {
            ci?.SetOldPosition(favListIdx, r.GetInt32(1));
          }
          else
          {
            var list = channelLists.TryGet(favListId);
            // TODO create copy of channel for each channel list so that it can have an independant number
            ci?.SetOldPosition(0, r.GetInt32(1));
            DataRoot.AddChannel(list, ci);
          }
        }
      }

      foreach(var ci in channelsById.Values)
        DataRoot.AddChannel(favlist, ci);
    }
    #endregion

    // Saving ====================================

    #region Save()

    public override void Save(string tvOutputFile)
    {
      //Editor.SequentializeFavPos(channelLists[6], 4);

      if (tvOutputFile != FileName)
        File.Copy(FileName, tvOutputFile, true);

      using (var conn = new SQLiteConnection("Data Source=" + tvOutputFile))
      {
        conn.Open();
        using (var trans = conn.BeginTransaction())
        using (var cmd = conn.CreateCommand())
        {
          cmd.Transaction = trans;
          try
          {
            CreateFavTables(cmd);
#if !LOCK_LCN_LISTS
            ResetLcn(cmd);
#endif
            foreach (var list in DataRoot.ChannelLists)
            {
              if (list.ReadOnly)
                continue;
              foreach (var ci in list.Channels)
                UpdateChannel(cmd, ci);
            }
            trans.Commit();
            FileName = tvOutputFile;
          }
          catch
          {
            trans.Rollback();
            throw;
          }
        }
      }
    }

    #endregion

    #region CreateFavTables()

    private void CreateFavTables(SQLiteCommand cmd)
    {
      for (var i = 1; i <= 4; i++)
        if (!tableNames.Contains("fav_" + i))
        {
          cmd.CommandText = $"CREATE TABLE fav_{i} (ui2_svc_id INTEGER, ui2_svc_rec_id INTEGER, user_defined_ch_num VARCHAR, user_defined_ch_name VARCHAR)";
          cmd.ExecuteNonQuery();
          tableNames.Add($"fav_{i}");
        }
    }

    #endregion

    #region ResetLcn()

    private void ResetLcn(SQLiteCommand cmd)
    {
      var regex = new Regex(@"^svl_\d_data_dvb$");
      foreach (var table in tableNames)
      {
        if (!regex.IsMatch(table))
          continue;
        cmd.CommandText = "update " + table + " set cur_lcn=0, original_lcn=0, lcn_idx=0";
        cmd.ExecuteNonQuery();
      }
    }

    #endregion

    #region UpdateChannel()

    private void UpdateChannel(SQLiteCommand cmd, ChannelInfo ci)
    {
      if (ci.RecordIndex < 0) // skip reference list proxy channels
        return;

      var x = (int) ((ulong) ci.RecordIndex >> 32); // the table number is kept in the higher 32 bits
      var id = (int) (ci.RecordIndex & 0xFFFFFFFF); // the record id is kept in the lower 32 bits

      var resetFlags = NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4 | NwMask.Lock | NwMask.Visible;
      var setFlags = (NwMask) (((int) ci.Favorites & 0x0F) << 4);
      if (ci.Lock) setFlags |= NwMask.Lock;
      if (!ci.Hidden && ci.NewProgramNr >= 0) setFlags |= NwMask.Visible;

      cmd.CommandText = $"update svl_{x} set channel_id=(channel_id&{0x3FFFF})|(@chnr << 18)" +
                        $", ch_id_txt=@chnr || '   0'" +
                        $", ac_name=@name" +
                        $", option_mask=option_mask|{(int) (OptionMask.ChNumEdited | OptionMask.NameEdited)}" +
                        $", nw_mask=(nw_mask&@resetFlags)|@setFlags" +
                        $" where svl_rec_id=@id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", DbType.Int32);
      cmd.Parameters.Add("@chnr", DbType.Int32);
      cmd.Parameters.Add("@name", DbType.String);
      cmd.Parameters.Add("@resetFlags", DbType.Int32);
      cmd.Parameters.Add("@setFlags", DbType.Int32);
      cmd.Parameters["@id"].Value = id;
      cmd.Parameters["@chnr"].Value = ci.NewProgramNr;
      cmd.Parameters["@name"].Value = ci.Name;
      cmd.Parameters["@resetFlags"].Value = ~(int) resetFlags;
      cmd.Parameters["@setFlags"].Value = (int) setFlags;
      cmd.ExecuteNonQuery();

      for (var i = 0; i < 4; i++)
        if (ci.FavIndex[i] <= 0)
        {
          cmd.CommandText = $"delete from fav_{i + 1} where ui2_svc_id={ci.RecordIndex >> 32} and ui2_svc_rec_id={ci.RecordIndex & 0xFFFF}";
          cmd.ExecuteNonQuery();
        }
        else
        {
          cmd.CommandText = $"update fav_{i + 1} set user_defined_ch_num=@chnr, user_defined_ch_name=@name where ui2_svc_id=@svcid and ui2_svc_rec_id=@recid";
          cmd.Parameters.Clear();
          cmd.Parameters.Add("@chnr", DbType.String); // for some reason this is a VARCHAR in the database
          cmd.Parameters.Add("@name", DbType.String);
          cmd.Parameters.Add("@svcid", DbType.Int32);
          cmd.Parameters.Add("@recid", DbType.Int32);
          cmd.Parameters["@chnr"].Value = ci.FavIndex[i].ToString();
          cmd.Parameters["@name"].Value = ci.Name;
          cmd.Parameters["@svcid"].Value = ci.RecordIndex >> 32;
          cmd.Parameters["@recid"].Value = ci.RecordIndex & 0xFFFF;
          if (cmd.ExecuteNonQuery() == 0)
          {
            cmd.CommandText = $"insert into fav_{i + 1} (ui2_svc_id, ui2_svc_rec_id, user_defined_ch_num, user_defined_ch_name) values (@svcid,@recid,@chnr,@name)";
            cmd.ExecuteNonQuery();
          }
        }
    }

    #endregion

    #region enums and bitmasks

    internal enum BroadcastType
    {
      Analog = 1,
      Dvb = 2
    }

    internal enum BroadcastMedium
    {
      DigTer = 1,
      DigCab = 2,
      DigSat = 3,
      AnaTer = 4,
      AnaCab = 5,
      AnaSat = 6
    }

    internal enum ServiceType
    {
      Tv = 1,
      Radio = 2,
      App = 3
    }

    [Flags]
    internal enum NwMask
    {
      Active = 1 << 1,
      Visible = 1 << 3,
      Fav1 = 1 << 4,
      Fav2 = 1 << 5,
      Fav3 = 1 << 6,
      Fav4 = 1 << 7,
      Lock = 1 << 8
    }

    [Flags]
    internal enum OptionMask
    {
      NameEdited = 1 << 3,
      ChNumEdited = 1 << 10,
      DeletedByUser = 1 << 13
    }

    [Flags]
    internal enum HashCode
    {
      Name = 1 << 0,
      ChannelId = 1 << 1,
      BroadcastType = 1 << 2,
      TsRecId = 1 << 3,
      ProgNum = 1 << 4,
      DvbShortName = 1 << 5,
      Radio = 1 << 10,
      Encrypted = 1 << 11,
      Tv = 1 << 13
    }

    [Flags]
    internal enum DvbLinkageMask
    {
      Ts = 1 << 2
    }

    #endregion

  }
}