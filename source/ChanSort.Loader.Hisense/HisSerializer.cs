using System;
using System.Collections.Generic;
using System.Data;
using ChanSort.Api;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace ChanSort.Loader.Hisense
{
  public class HisSerializer : SerializerBase
  {
    public override string DisplayName => "Hisense *.db Loader";

    #region enums and bitmasks

    internal enum BroadcastType { Analog = 1, Dvb = 2 }
    internal enum BroadcastMedium { DigTer = 1, DigCab = 2, DigSat = 3, AnaTer = 4, AnaCab = 5, AnaSat = 6 }
    internal enum ServiceType { Tv = 1, Radio = 2, App = 3}
    [Flags]
    internal enum NwMask { Active = 1<<1, Visible = 1<<3, Fav1 = 1<<4, Fav2 = 1<<5, Fav3 = 1<<6, Fav4 = 1<<7, Lock = 1<<8 }
    [Flags]
    internal enum OptionMask { NameEdited = 1<<3, ChNumEdited = 1<<10, DeletedByUser = 1<<13 }
    [Flags]
    internal enum HashCode { Name = 1<<0, ChannelId = 1<<1, BroadcastType = 1<<2, TsRecId = 1<<3, ProgNum = 1<<4, DvbShortName = 1<<5, Radio = 1<<10, Encrypted = 1<<11, Tv = 1<<13 }
    [Flags]
    internal enum DvbLinkageMask { Ts = 1<<2 }

    #endregion

    private readonly List<ChannelList> channelLists = new List<ChannelList>();
    private List<string> tableNames;


    #region ctor()
    public HisSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanDeleteChannels = false;
      channelLists.Add(new ChannelList(SignalSource.Antenna | SignalSource.Analog | SignalSource.Digital | SignalSource.Radio | SignalSource.Tv, "Antenna"));
      channelLists.Add(new ChannelList(SignalSource.Cable | SignalSource.Analog | SignalSource.Digital | SignalSource.Radio | SignalSource.Tv, "Cable"));
      channelLists.Add(new ChannelList(SignalSource.Sat | SignalSource.Analog | SignalSource.Digital | SignalSource.Radio | SignalSource.Tv, "Sat"));
      channelLists.Add(new ChannelList(SignalSource.Sat | SignalSource.Analog | SignalSource.Digital | SignalSource.Radio | SignalSource.Tv, "Prefered Sat"));
      channelLists.Add(new ChannelList(SignalSource.Antenna | SignalSource.Cable | SignalSource.Sat | SignalSource.Analog | SignalSource.Digital | SignalSource.Radio | SignalSource.Tv, "CI 1"));
      channelLists.Add(new ChannelList(SignalSource.Antenna | SignalSource.Cable | SignalSource.Sat | SignalSource.Analog | SignalSource.Digital | SignalSource.Radio | SignalSource.Tv, "CI 2"));

      foreach (var list in this.channelLists)
      {
        this.DataRoot.ChannelLists.Add(list);
        list.VisibleColumnFieldNames = new List<string> {"Position", "OldProgramNr", "Name", "ShortName", "Favorites", "Lock", "Hidden", "Encrypted",
          "FreqInMhz", "OriginalNetworkId", "TransportStreamId", "ServiceId", "ServiceTypeName", "NetworkName", "SymbolRate" };
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
          this.LoadSatelliteData(cmd);
          this.LoadTslData(cmd);
          this.LoadSvlData(cmd);
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
        cmd.CommandText = "select satl_rec_id, mask, i2_orb_pos, ac_sat_name from " + tableName;
        using (var r = cmd.ExecuteReader())
        {
          while (r.Read())
          {
            var sat = new Satellite(r.GetInt32(0));
            var pos = r.GetInt32(2);
            sat.OrbitalPosition = $"{(decimal) Math.Abs(pos)/10:n1}{(pos < 0 ? 'W' : 'E')}";
            sat.Name = r.GetString(3);
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

        this.LoadTslData(cmd, x, "tsl_#_data_ter_dig", ", freq", (t, r, i0) =>
        {
          t.FrequencyInMhz = (decimal)r.GetInt32(i0 + 0) / 1000000;
        });

        this.LoadTslData(cmd, x, "tsl_#_data_cab_dig", ", freq, sym_rate", (t, r, i0) =>
        {
          t.FrequencyInMhz = (decimal)r.GetInt32(i0 + 0) / 1000000;
          t.SymbolRate = r.GetInt32(i0 + 1);
        });

        this.LoadTslData(cmd, x, "tsl_#_data_sat_dig", ", freq, sym_rate", (t, r, i0) =>
        {
          t.FrequencyInMhz = r.GetInt32(i0 + 0);
          t.SymbolRate = r.GetInt32(i0 + 1);
        });
      }
    }

    private void LoadTslData(SQLiteCommand cmd, int tableNr, string joinTable, string joinFields, Action<Transponder, SQLiteDataReader, int> enhanceTransponderInfo)
    {
      if (!this.tableNames.Contains(joinTable.Replace("#", tableNr.ToString())))
        return;

      cmd.CommandText = $"select tsl_#.tsl_rec_id, `t_desc.on_id`, `t_desc.ts_id`, `t_ref.satl_rec_id`, `t_desc.e_bcst_medium` {joinFields} "
        + $" from tsl_# inner join {joinTable} on {joinTable}.tsl_rec_id=tsl_#.tsl_rec_id";
      cmd.CommandText = cmd.CommandText.Replace("#", tableNr.ToString());
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var trans = new Transponder(r.GetInt32(0));
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
        this.LoadSvlData(cmd, x, "svl_#_data_dvb", ", b_free_ca_mode, s_svc_name", (ci, r, i0) =>
        {
          ci.Encrypted = r.GetBoolean(i0 + 0);
          ci.ShortName = r.GetString(i0 + 1);
          if ((ci.SignalSource & SignalSource.DvbT) == SignalSource.DvbT)
            ci.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(ci.FreqInMhz).ToString();
        });
      }
    }

    private void LoadSvlData(SQLiteCommand cmd, int tableNr, string joinTable, string joinFields, Action<ChannelInfo, SQLiteDataReader, int> enhanceChannelInfo)
    {
      if (!this.tableNames.Contains(joinTable.Replace("#", tableNr.ToString())))
        return;

      cmd.CommandText = $"select svl_#.svl_rec_id, channel_id, svl_#.tsl_rec_id, e_serv_type, ac_name, nw_mask, prog_id, `t_desc.e_bcst_medium` {joinFields}"
        + $" from svl_# inner join {joinTable} on {joinTable}.svl_rec_id=svl_#.svl_rec_id inner join tsl_# on tsl_#.tsl_rec_id=svl_#.tsl_rec_id";
      cmd.CommandText = cmd.CommandText.Replace("#", tableNr.ToString());
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = ((long)tableNr << 32) | (uint)r.GetInt32(0);
          var prNr = (int)((uint)r.GetInt32(1)) >> 18;
          var trans = this.DataRoot.Transponder.TryGet(r.GetInt32(2));
          var stype = (ServiceType) r.GetInt32(3);
          var name = r.GetString(4);
          var nwMask = (NwMask)r.GetInt32(5);
          var sid = r.GetInt32(6);
          var bmedium = (BroadcastMedium)r.GetInt32(7);

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
          ci.Favorites |= (Favorites) ((int)(nwMask & (NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4)) >> 4);

          if (stype == ServiceType.Radio)
            ci.ServiceTypeName = "Radio";
          else if (stype == ServiceType.Tv)
            ci.ServiceTypeName = "TV";
          else if (stype == ServiceType.App)
            ci.ServiceTypeName = "Data";

          enhanceChannelInfo(ci, r, 8);

          var list = this.channelLists[tableNr - 1];
          this.DataRoot.AddChannel(list, ci);
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
      using (var conn = new SQLiteConnection("Data Source=" + this.FileName))
      {
        conn.Open();
        using (var trans = conn.BeginTransaction())
        using (var cmd = conn.CreateCommand())
        {
          cmd.Transaction = trans;
          try
          {
            foreach (var list in this.DataRoot.ChannelLists)
            {
              foreach (var ci in list.Channels)
                this.UpdateChannel(cmd, ci);
            }
            trans.Commit();
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

    #region UpdateChannel()
    private void UpdateChannel(SQLiteCommand cmd, ChannelInfo ci)
    {
      int x = (int)((ulong)ci.RecordIndex >> 32);   // the table number is kept in the higher 32 bits
      int id = (int)(ci.RecordIndex & 0xFFFFFFFF);  // the record id is kept in the lower 32 bits

      if (ci.NewProgramNr != ci.OldProgramNr)
      {
        if (ci.NewProgramNr >= 0)
        {
          cmd.CommandText = $"update svl_{x} set channel_id=(channel_id & {0xFFFC}) | @chnr, option_mask=option_mask | " + ((int) OptionMask.ChNumEdited) + " where svl_rec_id=@id";
          cmd.Parameters.Clear();
          cmd.Parameters.Add("@id", DbType.Int32);
          cmd.Parameters.Add("@chnr", DbType.Int32);
          cmd.Parameters["@id"].Value = id;
          cmd.Parameters["@chnr"].Value = ci.NewProgramNr << 18;
          cmd.ExecuteNonQuery();
        }
        else
        {
          cmd.CommandText = $"update svl_{x} set nw_mask=nw_mask | " + ((int)OptionMask.DeletedByUser) + " where svl_rec_id=@id";
          cmd.Parameters.Clear();
          cmd.Parameters.Add("@id", DbType.Int32);
          cmd.Parameters.Add("@fav", DbType.Int32);
          cmd.Parameters["@id"].Value = id;
          cmd.Parameters["@fav"].Value = ((int)ci.Favorites & 0x0F) << 4;
          cmd.ExecuteNonQuery();
        }
      }

      if (ci.IsNameModified)
      {
        cmd.CommandText = $"update svl_{x} set name=@name, option_mask=option_mask|" + ((int)OptionMask.NameEdited) + " where svl_rec_id=@id";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@id", DbType.Int32);
        cmd.Parameters.Add("@name", DbType.String);
        cmd.Parameters["@id"].Value = id;
        cmd.Parameters["@name"].Value = ci.Name;
        cmd.ExecuteNonQuery();
      }

      var resetFlags = NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4 | NwMask.Lock | NwMask.Visible;
      var setFlags = (NwMask)(((int)ci.Favorites & 0x0F) << 4);
      if (ci.Lock) setFlags |= NwMask.Lock;
      if (!ci.Hidden) setFlags |= NwMask.Visible;

      cmd.CommandText = $"update svl_{x} set nw_mask=(nw_mask & @resetFlags)|@setFlags where svl_rec_id=@id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", DbType.Int32);
      cmd.Parameters.Add("@resetFlags", DbType.Int32);
      cmd.Parameters.Add("@setFlags", DbType.Int32);
      cmd.Parameters["@id"].Value = id;
      cmd.Parameters["@resetFlags"].Value = ~(int)resetFlags;
      cmd.Parameters["@setFlags"].Value = (int)setFlags;
      cmd.ExecuteNonQuery();
    }

    #endregion
  }
}
