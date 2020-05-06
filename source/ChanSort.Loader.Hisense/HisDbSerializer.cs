//#define LOCK_LCN_LISTS

using System;
using System.Collections.Generic;
using System.Data;
using ChanSort.Api;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;

namespace ChanSort.Loader.Hisense
{
  public class HisDbSerializer : SerializerBase
  {
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

    private readonly List<ChannelList> channelLists = new List<ChannelList>();
    private readonly Dictionary<long, Channel> channelsById = new Dictionary<long, Channel>();
    private List<string> tableNames;

    // the fav_1 - fav_4 tables in channel.db of a H50B7700UW has different column names and a primary key/unique constraint which requires specific handling
    private bool hasCamelCaseFavSchema = false;

    #region ctor()

    public HisDbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();

      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.CanHaveGaps = true;
      this.Features.SortedFavorites = true;

      channelLists.Add(new ChannelList(SignalSource.Antenna, "Antenna"));
      channelLists.Add(new ChannelList(SignalSource.Cable, "Cable"));
      channelLists.Add(new ChannelList(SignalSource.Sat, "Sat"));
      channelLists.Add(new ChannelList(SignalSource.Sat, "Preferred Sat"));
      channelLists.Add(new ChannelList(0, "CI 1"));
      channelLists.Add(new ChannelList(0, "CI 2"));

      channelLists.Add(new ChannelList(0, "Favorites"));
      channelLists[channelLists.Count - 1].IsMixedSourceFavoritesList = true;

      foreach (var list in this.channelLists)
      {
        this.DataRoot.AddChannelList(list);
        list.VisibleColumnFieldNames = new List<string>
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
      }
    }

    #endregion


    #region Load()

    public override void Load()
    {
      using (var conn = new SQLiteConnection("Data Source=" + this.FileName))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.RepairCorruptedDatabaseImage(cmd);
          this.LoadTableNames(cmd);

          if (!tableNames.Contains("svl_1") && !tableNames.Contains("svl_2") && !tableNames.Contains("svl_3"))
            throw new FileLoadException("File doesn't contain svl_* tables");

          this.LoadSatelliteData(cmd);
          this.LoadTslData(cmd);
          this.LoadSvlData(cmd);
          this.LoadFavorites(cmd);
        }
      }

      int totalCount = 0;
      foreach (var list in this.channelLists)
        totalCount += list.Count;
      if (totalCount == 0)
        Api.View.Default.MessageBox(Resources.Load_NoChannelsMsg, Resources.Load_NoChannelsCaption);
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
      this.tableNames = new List<string>();
      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' order by name";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          tableNames.Add(r.GetString(0));
      }
    }

    #endregion

    #region LoadSatelliteData()

    private void LoadSatelliteData(SQLiteCommand cmd)
    {
      var regex = new Regex(@"^satl_\d$");
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
            sat.OrbitalPosition = $"{(decimal) Math.Abs(pos) / 10:n1}{(pos < 0 ? 'W' : 'E')}";
            sat.Name = r.GetString(2);
            this.DataRoot.AddSatellite(sat);
          }
        }
      }
    }

    #endregion

    #region LoadTslData()

    private void LoadTslData(SQLiteCommand cmd)
    {
      var regex = new Regex(@"^tsl_(\d)$");
      foreach (var table in this.tableNames)
      {
        var match = regex.Match(table);
        if (!match.Success)
          continue;
        int x = int.Parse(match.Groups[1].Value);

        this.LoadTslData(cmd, x, "tsl_#_data_ter_dig", ", freq",
          (t, r, i0) => { t.FrequencyInMhz = (decimal) r.GetInt32(i0 + 0) / 1000000; });

        this.LoadTslData(cmd, x, "tsl_#_data_ter_ana", ", freq",
          (t, r, i0) => { t.FrequencyInMhz = (decimal) r.GetInt32(i0 + 0) / 1000000; });

        this.LoadTslData(cmd, x, "tsl_#_data_cab_dig", ", freq, sym_rate", (t, r, i0) =>
        {
          t.FrequencyInMhz = (decimal) r.GetInt32(i0 + 0) / 1000000;
          t.SymbolRate = r.GetInt32(i0 + 1);
        });

        this.LoadTslData(cmd, x, "tsl_#_data_cab_ana", ", freq",
          (t, r, i0) => { t.FrequencyInMhz = (decimal) r.GetInt32(i0 + 0) / 1000000; });

        this.LoadTslData(cmd, x, "tsl_#_data_sat_dig", ", freq, sym_rate, orb_pos", (t, r, i0) =>
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
      }
    }

    private void LoadTslData(SQLiteCommand cmd, int tableNr, string joinTable, string joinFields,
      Action<Transponder, SQLiteDataReader, int> enhanceTransponderInfo)
    {
      if (!this.tableNames.Contains(joinTable.Replace("#", tableNr.ToString())))
        return;

      cmd.CommandText =
        $"select tsl_#.tsl_rec_id, `t_desc.on_id`, `t_desc.ts_id`, `t_ref.satl_rec_id`, `t_desc.e_bcst_medium` {joinFields} "
        + $" from tsl_# inner join {joinTable} on {joinTable}.tsl_rec_id=tsl_#.tsl_rec_id";
      cmd.CommandText = cmd.CommandText.Replace("#", tableNr.ToString());
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          int id = (tableNr << 16) | r.GetInt32(0);
          var trans = new Transponder(id);
          trans.OriginalNetworkId = r.GetInt32(1);
          trans.TransportStreamId = r.GetInt32(2);
          trans.Satellite = this.DataRoot.Satellites.TryGet(r.GetInt32(3));

          enhanceTransponderInfo(trans, r, 5);

          this.DataRoot.AddTransponder(trans.Satellite, trans);
        }
      }
    }

    #endregion

    #region LoadSvlData()

    private void LoadSvlData(SQLiteCommand cmd)
    {
      var regex = new Regex(@"^svl_(\d)$");
      foreach (var table in this.tableNames)
      {
        var match = regex.Match(table);
        if (!match.Success)
          continue;
        int x = int.Parse(match.Groups[1].Value);
        if (x < 1 || x > 6)
        {
          this.DataRoot.Warnings.AppendLine("Skipping unknown channel list with number " + x);
          return;
        }

        this.LoadSvlData(cmd, x, "svl_#_data_analog", "", (ci, r, i0) => { });
        this.LoadSvlData(cmd, x, "svl_#_data_dvb", ", b_free_ca_mode, s_svc_name, sdt_service_type, cur_lcn",
          (ci, r, i0) =>
          {
            ci.Encrypted = r.GetBoolean(i0 + 0);
            ci.ShortName = r.GetString(i0 + 1);
            ci.ServiceType = r.GetInt32(i0 + 2);
            if (ci.ServiceType != 0)
              ci.ServiceTypeName = LookupData.Instance.GetServiceTypeDescription(ci.ServiceType);

            if ((ci.SignalSource & SignalSource.DvbT) == SignalSource.DvbT)
              ci.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(ci.FreqInMhz).ToString();
            else if ((ci.SignalSource & SignalSource.DvbC) == SignalSource.DvbC)
              ci.ChannelOrTransponder = LookupData.Instance.GetDvbcTransponder(ci.FreqInMhz).ToString();

#if LOCK_LCN_LISTS
          // make the current list read-only if LCN is used
          if (r.GetInt32(i0 + 3) != 0)
          {
            this.channelLists[x - 1].ReadOnly = true;
          }
#endif
          });
      }
    }

    private void LoadSvlData(SQLiteCommand cmd, int tableNr, string joinTable, string joinFields,
      Action<ChannelInfo, SQLiteDataReader, int> enhanceChannelInfo)
    {
      if (!this.tableNames.Contains(joinTable.Replace("#", tableNr.ToString())))
        return;

      cmd.CommandText =
        $"select svl_#.svl_rec_id, channel_id, svl_#.tsl_id, svl_#.tsl_rec_id, e_serv_type, ac_name, nw_mask, prog_id, `t_desc.e_bcst_medium` {joinFields}"
        + $" from svl_# inner join {joinTable} on {joinTable}.svl_rec_id=svl_#.svl_rec_id inner join tsl_# on tsl_#.tsl_rec_id=svl_#.tsl_rec_id";
      cmd.CommandText = cmd.CommandText.Replace("#", tableNr.ToString());
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = ((long) tableNr << 32) | (uint) r.GetInt32(0);
          var prNr = (int) ((uint) r.GetInt32(1)) >> 18;
          var trans = this.DataRoot.Transponder.TryGet((r.GetInt32(2) << 16) | r.GetInt32(3));
          var stype = (ServiceType) r.GetInt32(4);
          var name = r.GetString(5);
          var nwMask = (NwMask) r.GetInt32(6);
          var sid = r.GetInt32(7);
          var bmedium = (BroadcastMedium) r.GetInt32(8);

          var ssource = DetermineSignalSource(bmedium, stype);
          var ci = new Channel(ssource, id, prNr, name);
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
          ci.ChannelId = r.GetInt32(1);
          ci.NwMask = (int)nwMask;

          //ci.Skip = (nwMask & NwMask.Active) == 0;
          ci.Lock = (nwMask & NwMask.Lock) != 0;
          ci.Hidden = (nwMask & NwMask.Visible) == 0;
          ci.Favorites |= (Favorites) ((int) (nwMask & (NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4)) >> 4);

          if (stype == ServiceType.Radio)
            ci.ServiceTypeName = "Radio";
          else if (stype == ServiceType.Tv)
            ci.ServiceTypeName = "TV";
          else if (stype == ServiceType.App)
            ci.ServiceTypeName = "Data";

          enhanceChannelInfo(ci, r, 9);

          var list = this.channelLists[tableNr - 1];
          ci.Source = list.ShortCaption;
          this.DataRoot.AddChannel(list, ci);

          // add the channel to all favorites lists
          this.DataRoot.AddChannel(this.channelLists[6], ci);
          this.channelsById[ci.RecordIndex] = ci;
        }
      }
    }

    #endregion

    #region LoadFavorites()

    private void LoadFavorites(SQLiteCommand cmd)
    {
      // detect schema used by fav_x tables
      if (tableNames.Contains("fav_1"))
      {
        cmd.CommandText = "pragma table_info('fav_1')";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
          if (r.GetString(1) == "sortId")
            this.hasCamelCaseFavSchema = true;
        }
      }

      // load the actual favorites data
      for (int i = 1; i <= 4; i++)
      {
        if (!this.tableNames.Contains($"fav_{i}"))
          continue;
        cmd.CommandText = hasCamelCaseFavSchema 
          ? $"select svlId, svlRecId, sortId from fav_{i}"
          : $"select ui2_svc_id, ui2_svc_rec_id, cast(user_defined_ch_num as integer) from fav_{i}";

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
          var id = ((long) r.GetInt32(0) << 32) | (uint) r.GetInt32(1);
          var ci = this.channelsById.TryGet(id);
          if (ci != null)
            ci.OldFavIndex[i - 1] = r.GetInt32(2);
        }
      }
    }

    #endregion

    #region DetermineSignalSource()

    private static SignalSource DetermineSignalSource(BroadcastMedium bmedium, ServiceType stype)
    {
      SignalSource ssource = 0;
      if (bmedium == BroadcastMedium.AnaCab)
        ssource = SignalSource.AnalogC;
      else if (bmedium == BroadcastMedium.AnaSat)
        ssource = SignalSource.Analog | SignalSource.Sat;
      else if (bmedium == BroadcastMedium.AnaTer)
        ssource = SignalSource.AnalogT;
      else if (bmedium == BroadcastMedium.DigCab)
        ssource = SignalSource.DvbC;
      else if (bmedium == BroadcastMedium.DigSat)
        ssource = SignalSource.DvbS;
      else if (bmedium == BroadcastMedium.DigTer)
        ssource = SignalSource.DvbT;
      ssource |= stype == ServiceType.Radio ? SignalSource.Radio : SignalSource.Tv;
      return ssource;
    }

    #endregion

    // Saving ====================================

    #region Save()

    public override void Save(string tvOutputFile)
    {
      Editor.SequentializeFavPos(this.channelLists[6], 4);

      if (tvOutputFile != this.FileName)
        File.Copy(this.FileName, tvOutputFile, true);

      using var conn = new SQLiteConnection("Data Source=" + tvOutputFile);
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();
      cmd.Transaction = trans;
      try
      {
        this.CreateFavTables(cmd);
        
        // must truncate and re-fill this table because there is a unique primary key constraint on a data column that needs to be edited
        if (this.hasCamelCaseFavSchema)
        {
          for (int i = 1; i <= 4; i++)
          {
            cmd.CommandText = $"delete from fav_{i}";
            cmd.ExecuteNonQuery();
          }
        }

#if !LOCK_LCN_LISTS
        this.ResetLcn(cmd);
#endif
        foreach (var list in this.DataRoot.ChannelLists)
        {
          if (list.ReadOnly || list.IsMixedSourceFavoritesList)
            continue;
          foreach (var ci in list.Channels)
            this.UpdateChannel(cmd, ci as Channel);
        }

        trans.Commit();
        this.FileName = tvOutputFile;
      }
      catch
      {
        trans.Rollback();
        throw;
      }
    }

    #endregion

    #region CreateFavTables()

    private void CreateFavTables(SQLiteCommand cmd)
    {
      for (int i = 1; i <= 4; i++)
      {
        if (!this.tableNames.Contains("fav_" + i))
        {
          cmd.CommandText = hasCamelCaseFavSchema
            ? $"CREATE TABLE fav_{i} (sortId INTEGER, channelId INTEGER, svlId INTEGER, channelName VARCHAR(), svlRecId INTEGER, nwMask INTEGER, PREIMARY KEY (sortId)"
            : $"CREATE TABLE fav_{i} (ui2_svc_id INTEGER, ui2_svc_rec_id INTEGER, user_defined_ch_num VARCHAR, user_defined_ch_name VARCHAR)";
          cmd.ExecuteNonQuery();
          this.tableNames.Add($"fav_{i}");
        }
      }
    }

    #endregion

    #region ResetLcn()

    private void ResetLcn(SQLiteCommand cmd)
    {
      var regex = new Regex(@"^svl_\d_data_dvb$");
      foreach (var table in this.tableNames)
      {
        if (!regex.IsMatch(table))
          continue;
        cmd.CommandText = "update " + table + " set cur_lcn=0, original_lcn=0, lcn_idx=0";
        cmd.ExecuteNonQuery();
      }
    }

    #endregion

    #region UpdateChannel()

    private void UpdateChannel(SQLiteCommand cmd, Channel ci)
    {
      if (ci == null || ci.IsProxy)
        return;

      int x = (int) ((ulong) ci.RecordIndex >> 32); // the table number is kept in the higher 32 bits
      int id = (int) (ci.RecordIndex & 0xFFFFFFFF); // the record id is kept in the lower 32 bits

      var resetFlags = NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4 | NwMask.Lock | NwMask.Visible;
      var setFlags = (NwMask) (((int) ci.Favorites & 0x0F) << 4);
      if (ci.Lock) setFlags |= NwMask.Lock;
      if (!ci.Hidden && ci.NewProgramNr >= 0) setFlags |= NwMask.Visible;
      var nwMask = (int)(((NwMask)ci.NwMask & ~resetFlags) | setFlags);

      cmd.CommandText = $"update svl_{x} set channel_id=(channel_id&{0x3FFFF})|(@chnr << 18)" +
                        $", ch_id_txt=@chnr || '   0'" +
                        $", ac_name=@name" +
                        $", option_mask=option_mask|{(int) (OptionMask.ChNumEdited | OptionMask.NameEdited)}" +
                        $", nw_mask=@nwMask" +
                        $" where svl_rec_id=@id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", DbType.Int32);
      cmd.Parameters.Add("@chnr", DbType.Int32);
      cmd.Parameters.Add("@name", DbType.String);
      cmd.Parameters.Add("@nwMask", DbType.Int32);
      cmd.Parameters["@id"].Value = id;
      cmd.Parameters["@chnr"].Value = ci.NewProgramNr;
      cmd.Parameters["@name"].Value = ci.Name;
      cmd.Parameters["@nwMask"].Value = nwMask;
      cmd.ExecuteNonQuery();

      ci.NwMask = nwMask;

      if (this.hasCamelCaseFavSchema)
        this.UpdateFavoritesWithCamelCaseColumnNames(cmd, ci);
      else
        this.UpdateFavoritesWithUnderlinedColumnNames(cmd, ci);
    }

    #endregion

    #region UpdateFavoritesWithUnderlinedColumnNames()
    private void UpdateFavoritesWithUnderlinedColumnNames(SQLiteCommand cmd, ChannelInfo ci)
    {
      for (int i = 0; i < 4; i++)
      {
        if (ci.FavIndex[i] <= 0)
        {
          cmd.CommandText = $"delete from fav_{i + 1} where ui2_svc_id={ci.RecordIndex >> 32} and ui2_svc_rec_id={ci.RecordIndex & 0xFFFFFFFF}";
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
    }
    #endregion

    #region UpdateFavoritesWithCamelCaseColumnNames()
    private void UpdateFavoritesWithCamelCaseColumnNames(SQLiteCommand cmd, Channel ci)
    {
      for (int i = 0; i < 4; i++)
      {
        if (ci.FavIndex[i] <= 0)
          continue;

        cmd.CommandText = $"insert into fav_{i + 1} (sortId, channelId, svlId, channelName, svlRecId, nwMask) values (@chnr,@chanid,@svcid,@name,@recid,@nwmask)";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@chnr", DbType.Int32);
        cmd.Parameters.Add("@chanid", DbType.Int32);
        cmd.Parameters.Add("@svcid", DbType.Int32);
        cmd.Parameters.Add("@name", DbType.String);
        cmd.Parameters.Add("@recid", DbType.Int32);
        cmd.Parameters.Add("@nwmask", DbType.Int32);
        cmd.Parameters["@chnr"].Value = ci.FavIndex[i];
        cmd.Parameters["@chanid"].Value = ci.ChannelId;
        cmd.Parameters["@name"].Value = ci.Name;
        cmd.Parameters["@svcid"].Value = ci.RecordIndex >> 32;
        cmd.Parameters["@recid"].Value = ci.RecordIndex & 0xFFFF;
        cmd.Parameters["@nwmask"].Value = ci.NwMask;
        cmd.ExecuteNonQuery();
      }
    }
    #endregion

  }
}
