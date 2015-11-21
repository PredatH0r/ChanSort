using System;
using System.Data;
using ChanSort.Api;
using System.Data.SQLite;

namespace ChanSort.Loader.Hisense
{
  public class HisSerializer : SerializerBase
  {
    public override string DisplayName => "Hisense SQLite Loader";


    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC | SignalSource.Tv, "Analog-C");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC | SignalSource.Tv | SignalSource.Radio, "DVB-C");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT | SignalSource.Tv | SignalSource.Radio, "DVB-T");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv | SignalSource.Radio, "DVB-S");

    #region enums and bitmasks

    internal enum BroadcastType { Analog = 1, Dvb = 2 }
    internal enum BroadcastMedium { DigTer = 1, DigCab = 2, DigSat = 3, AnaTer = 4, AnaCab = 5, AnaSat = 6 }
    internal enum ServiceType { Tv = 1, Radio = 2, App = 3}
    [Flags]
    internal enum NwMask { Hide = 0, Skip = 1<<3, Fav1 = 1<<4, Fav2 = 1<<5, Fav3 = 1<<6, Fav4 = 1<<7, Lock = 1<<8 }
    [Flags]
    internal enum OptionMask { Rename = 1<<3, Move = 1<<10 }
    [Flags]
    internal enum HashCode { Name = 1<<0, ChannelId = 1<<1, BroadcastType = 1<<2, TsRecId = 1<<3, ProgNum = 1<<4, DvbShortName = 1<<5, Radio = 1<<10, Encrypted = 1<<11, Tv = 1<<13 }
    [Flags]
    internal enum DvbLinkageMask { Ts = 1<<2 }

    #endregion


    #region ctor()
    public HisSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.DataRoot.AddChannelList(avbcChannels);
      this.DataRoot.AddChannelList(dvbcChannels);
      this.DataRoot.AddChannelList(dvbtChannels);
      this.DataRoot.AddChannelList(dvbsChannels);
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

    #region LoadSatelliteData()
    private void LoadSatelliteData(SQLiteCommand cmd)
    {
      cmd.CommandText = "select ui2_satl_rec_id, ui4_mask, i2_orb_pos, ac_sat_name from satl_x";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var sat = new Satellite(r.GetInt32(0));
          var pos = r.GetInt16(2);
          sat.OrbitalPosition = $"{(decimal)Math.Abs(pos)/10:n1}{(pos < 0 ? 'W' : 'E')}";
          sat.Name = r.GetString(3);
          this.DataRoot.AddSatellite(sat);
        }
      }
    }
    #endregion

    #region LoadTslData()

    private void LoadTslData(SQLiteCommand cmd)
    {
      this.LoadTslData(cmd, "tsl_x_data_ter_dig", ", ui4_freq", (t, r, i0) => { t.FrequencyInMhz = r.GetInt32(i0 + 0); });

      this.LoadTslData(cmd, "tsl_x_data_cab_dig", ", ui4_freq, ui3_sym_rate", (t, r, i0) =>
      {
        t.FrequencyInMhz = r.GetInt32(i0 + 0);
        t.SymbolRate = r.GetInt32(i0 + 1);
      });

      this.LoadTslData(cmd, "tsl_x_data_sat_dig", ", ui4_freq, ui4_sym_rate", (t, r, i0) =>
      {
        t.FrequencyInMhz = r.GetInt32(i0 + 0);
        t.SymbolRate = r.GetInt32(i0 + 1);
      });
    }

    private void LoadTslData(SQLiteCommand cmd, string joinTable, string joinFields, Action<Transponder, SQLiteDataReader, int> enhanceTransponderInfo)
    {
      cmd.CommandText = "select tsl_x.ui2_tsl_rec_id, `t_desc.ui2_on_id`, `t_desc.ui2_ts_id`, `t_ref.ui2_satl_rec_id` " + joinFields
        + " from tsl_x inner join " + joinTable + " on " + joinTable + ".ui2_tsl_rec_id=tsl_x.ui2_tsl_rec_id"
        //+ $" where `t_desc.e_bcst_type` in ({(int)BroadcastType.Analog},{(int)BroadcastType.Dvb}) "
        //+ $" and `tdesc.e_bcst_medium` in ({(int)BroadcastMedium.DigTer},{(int)BroadcastMedium.DigCab},{(int)BroadcastMedium.DigSat},{(int)BroadcastMedium.AnaCab})"
        ;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var trans = new Transponder(r.GetInt16(0));
          trans.OriginalNetworkId = r.GetInt16(1);
          trans.TransportStreamId = r.GetInt16(2);
          trans.Satellite = this.DataRoot.Satellites.TryGet(r.GetInt32(3));

          enhanceTransponderInfo(trans, r, 4);

          this.DataRoot.AddTransponder(trans.Satellite, trans);
        }
      }
    }
    #endregion

    #region LoadSvlData()

    private void LoadSvlData(SQLiteCommand cmd)
    {
      this.LoadSvlData(cmd, "svl_x_data_analog", "", (ci, r, i0) => { });
      this.LoadSvlData(cmd, "svl_x_data_dvb", ", b_free_ca_mode", (ci, r, i0) => { ci.Encrypted = r.GetBoolean(i0 + 0); });
    }

    private void LoadSvlData(SQLiteCommand cmd, string joinTable, string joinFields, Action<ChannelInfo, SQLiteDataReader, int> enhanceChannelInfo)
    {
      cmd.CommandText = "select svl_x.ui2_svl_rec_id, ui4_channel_id, ui2_tsl_rec_id, e_brdcst_type, e_serv_type, ac_name, ui4_nw_mask, ui4_hashcode " + joinFields
        + " from svl_x inner join " + joinTable + " on " + joinTable + ".ui2_svl_rec_id=svl_x.ui2_svl_rec_id"
        //+ $" where `e_brdcst_type` in ({(int)BroadcastType.Analog},{(int)BroadcastType.Dvb}) "
        ;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var id = r.GetInt16(0);
          var hashcode = (HashCode)r.GetInt32(7);
          var progNr = (hashcode & HashCode.ProgNum) == 0 || r.IsDBNull(1) ? -1 : r.GetInt32(1) >> 2;
          var trans = this.DataRoot.Transponder.TryGet(r.GetInt16(2));
          var bcast = (BroadcastType)r.GetInt16(3);
          var stype = (ServiceType) r.GetInt16(4);
          var name = (hashcode & HashCode.Name) == 0 || r.IsDBNull(5) ? "" : r.GetString(5);

          var ssource = trans == null || bcast == BroadcastType.Analog ? SignalSource.Analog : SignalSource.Digital;
          ssource |= stype == ServiceType.Radio ? SignalSource.Radio : SignalSource.Tv;


          int idx = name.IndexOf("\0");
          if (idx >= 0)
            name = name.Substring(0, idx);

          var ci = new ChannelInfo(ssource, id, progNr, name);
          if (trans != null)
          {
            ci.Transponder = trans;
            ci.OriginalNetworkId = trans.OriginalNetworkId;
            ci.TransportStreamId = trans.TransportStreamId;
            ci.SymbolRate = trans.SymbolRate;
            ci.FreqInMhz = trans.FrequencyInMhz;
          }

          var nwMask = (NwMask)r.GetInt32(6);
          ci.Skip = (nwMask & NwMask.Skip) != 0;
          ci.Lock = (nwMask & NwMask.Lock) != 0;
          ci.Hidden = (nwMask & NwMask.Hide) != 0;
          ci.Favorites |= (Favorites) ((int)(nwMask & (NwMask.Fav1 | NwMask.Fav2 | NwMask.Fav3 | NwMask.Fav4)) >> 4);

          if (stype == ServiceType.App)
            ci.ServiceTypeName = "Data";

          enhanceChannelInfo(ci, r, 8);

          var list = this.DataRoot.GetChannelList(ssource);
          this.DataRoot.AddChannel(list, ci);
        }
      }
    }
    #endregion

#if false

    #region ReadHeader()
    private int ReadHeader(byte[] data, ref int off)
    {
      if (off + 40 > data.Length)
        throw new FileLoadException(ERR_badFileFormat);
      var blockSize = BitConverter.ToInt32(data, off + 36);
      if (off + blockSize > data.Length)
        throw new FileLoadException(ERR_badFileFormat);

      off += 40;
      return blockSize;
    }
    #endregion

    #region ReadChannelList()
    private void ReadChannelList(ref int off, int size, int table, ChannelList channels)
    {
      int recordSize = svlMapping.Settings.GetInt("RecordSize");
      if (size % recordSize != 0)
        throw new FileLoadException(ERR_badFileFormat);
      int channelCount = size/recordSize;
      if (channelCount == 0)
        return;

      var broadcastDataOffset = svlMapping.Settings.GetInt("BroadcastSystemData");
      var nameLength = svlMapping.Settings.GetInt("NameLength");
      var source = channels.SignalSource & (SignalSource.MaskAnalogDigital | SignalSource.MaskAntennaCableSat);
      for (int i = 0; i < channelCount; i++)
      {
        svlMapping.SetDataPtr(svlFileContent, off);
        dvbMapping.SetDataPtr(svlFileContent, off + broadcastDataOffset);
        var ci = ReadChannel(source, i, nameLength);
        if (ci != null)
          this.DataRoot.AddChannel(channels, ci);        
        off += recordSize;
      }
    }
    #endregion

    #region ReadChannel()
    private ChannelInfo ReadChannel(SignalSource source, int index, int nameLength)
    {
      ChannelInfo ci = new ChannelInfo(source, index, 0, "");
      //ci.RecordIndex = svlMapping.GetWord("RecordId");
      ci.OldProgramNr = svlMapping.GetWord("ChannelId") >> 2;

      var nwMask = svlMapping.GetDword("NwMask");
      ci.Skip = (nwMask & svlMapping.Settings.GetInt("NwMask_Skip")) != 0;
      ci.Lock = (nwMask & svlMapping.Settings.GetInt("NwMask_Lock")) != 0;
      ci.Hidden = (nwMask & svlMapping.Settings.GetInt("NwMask_Hide")) != 0;
      for (int i = 0; i < 3; i++)
      {
        bool isFav = (nwMask & svlMapping.Settings.GetInt("NwMask_Fav" + (i + 1))) != 0;
        if (isFav)
          ci.Favorites |= (Favorites) (1 << i);
      }

      var fieldMask = svlMapping.GetDword("HashcodeFieldMask");

      if ((fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_Name")) != 0)
      {
        ci.Name = svlMapping.GetString("Name", nameLength);
        int term = ci.Name.IndexOf('\0');
        if (term >= 0)
          ci.Name = ci.Name.Substring(0, term);
      }

      var serviceType = svlMapping.GetByte("ServiceType");
      if (serviceType == 1)
      {
        ci.SignalSource |= SignalSource.Tv;
        ci.ServiceTypeName = "TV";
      }
      else if (serviceType == 2)
      {
        ci.SignalSource |= SignalSource.Radio;
        ci.ServiceTypeName = "Radio";
      }
      else
      {
        ci.ServiceTypeName = "Data";
      }

      if ((fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_TslRecId")) != 0)
      {
        int transpTableId = svlMapping.GetByte("TslTableId");
        int transpRecordId = svlMapping.GetByte("TslRecordId");
        var transpId = (transpTableId << 16) + transpRecordId;
        var transp = this.transponder.TryGet(transpId);
        if (transp != null)
        {
          ci.Transponder = transp;
          ci.FreqInMhz = transp.FrequencyInMhz;
          ci.SymbolRate = transp.SymbolRate;
        }
      }

      if ((fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_BroadcastType")) != 0)
      {
        var bcastType = svlMapping.GetByte("BroadcastType");
        if (bcastType == 1)
          ReadAnalogData(ci);
        else if (bcastType == 2)
          ReadDvbData(ci);
      }

      ci.Encrypted = (fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_Encrypted")) != 0;

      //ci.AddDebug("u1=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 2, 2);
      //ci.AddDebug("u2=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 4, 2);
      //ci.AddDebug(", hash=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 8, 2);
      //ci.AddDebug(", nw=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 12, 4);
      //ci.AddDebug(", o1=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 16, 4);
      ci.AddDebug(", o2=");
      ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 20, 4);
      ci.AddDebug("progId=");
      ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 24, 4);

      return ci;
    }
    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(ChannelInfo ci)
    {
      
    }
    #endregion

    #region ReadDvbData()
    private void ReadDvbData(ChannelInfo ci)
    {
      var mask = dvbMapping.GetDword("LinkageMask");
      var tsFlag = dvbMapping.Settings.GetInt("LinkageMask_Ts");

      if ((mask & tsFlag) != 0)
      {
        ci.OriginalNetworkId = dvbMapping.GetWord("Onid");
        ci.TransportStreamId = dvbMapping.GetWord("Tsid");
        ci.ServiceId = dvbMapping.GetWord("Ssid");
      }
      //ci.Encrypted = dvbMapping.GetByte("Encrypted") != 0;

      if ((ci.SignalSource & SignalSource.DvbT) == SignalSource.DvbT)
        ci.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(ci.FreqInMhz).ToString();

      ci.ShortName = dvbMapping.GetString("ShortName", dvbMapping.Settings.GetInt("ShortName_Size"));
    }
    #endregion

#endif

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
      if (ci.NewProgramNr != ci.OldProgramNr)
      {
        if (ci.NewProgramNr >= 0)
        {
          cmd.CommandText = "update svl_x set channel_id=@chnr, ui4_option_mask=ui4_option_mask|" + ((int) OptionMask.Move) + " where ui2_svl_rec_id=@id";
          cmd.Parameters.Clear();
          cmd.Parameters.Add("@id", DbType.Int16);
          cmd.Parameters.Add("@chnr", DbType.Int32);
          cmd.Parameters["@id"].Value = ci.RecordIndex;
          cmd.Parameters["@chnr"].Value = ci.NewProgramNr;
          cmd.ExecuteNonQuery();
        }
        else
        {
          // TODO: delete channel .. HOW?!?
        }
      }

      if (ci.IsNameModified)
      {
        cmd.CommandText = "update svl_x set name=@name, ui4_option_mask=ui4_option_mask|" + ((int)OptionMask.Rename) + " where ui2_svl_rec_id=@id";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@id", DbType.Int16);
        cmd.Parameters.Add("@name", DbType.String);
        cmd.Parameters["@id"].Value = ci.RecordIndex;
        cmd.Parameters["@name"].Value = ci.Name;
        cmd.ExecuteNonQuery();
      }

      cmd.CommandText = "update svl_x set ui4_nw_mask=(ui4_nw_mask & 0xFFFFF0F)|@fav where ui2_svl_rec_id=@id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", DbType.Int16);
      cmd.Parameters.Add("@fav", DbType.Int32);
      cmd.Parameters["@id"].Value = ci.RecordIndex;
      cmd.Parameters["@fav"].Value = ((int)ci.Favorites & 0x0F) << 4;
      cmd.ExecuteNonQuery();
    }

    #endregion
  }
}
