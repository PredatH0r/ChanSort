using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.DbFile
{
  class DbSerializer : SerializerBase
  {
    private const int BITS_Tv = 0x10000;
    private const int BITS_FavA = 0x100000;
    private const int BITS_FavB = 0x200000;
    private const int BITS_FavC = 0x400000;
    private const int BITS_FavD = 0x800000;
    private const int BITS_Locked = 0x20000000;

    private readonly ChannelList atvChannels = new ChannelList(SignalSource.AnalogCT | SignalSource.TvAndRadio, "Analog");
    private readonly ChannelList dtvTvChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Tv, "DTV");
    private readonly ChannelList dtvRadioChannels = new ChannelList(SignalSource.DvbCT | SignalSource.Radio, "Radio");
    private readonly ChannelList satTvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Sat-TV");
    private readonly ChannelList satRadioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat-Radio");
    private readonly Dictionary<string, bool> channelInfoByUid = new Dictionary<string, bool>();

    #region ctor()
    public DbSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = true;

      this.DataRoot.AddChannelList(this.atvChannels);
      this.DataRoot.AddChannelList(this.dtvTvChannels);
      this.DataRoot.AddChannelList(this.dtvRadioChannels);
      this.DataRoot.AddChannelList(this.satTvChannels);
      this.DataRoot.AddChannelList(this.satRadioChannels);
    }
    #endregion

    #region DisplayName
    public override string DisplayName { get { return "Toshiba *.db Loader"; } }
    #endregion


    #region Load()
    public override void Load()
    {
      string baseDir = Path.GetDirectoryName(Path.GetDirectoryName(this.FileName));
      
      string sysDataConnString = "Data Source=" + baseDir + "\\dvb_type001\\dvbSysData.db";
      using (var conn = new SQLiteConnection(sysDataConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.ReadSatellites(cmd);
          this.ReadTransponders(cmd);
        }
      }

      string mainDataConnString = "Data Source=" + baseDir + "\\dvb_type001\\dvbMainData.db";
      using (var conn = new SQLiteConnection(mainDataConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.ReadCryptInfo(cmd);
        }
      }

      string channelConnString = "Data Source=" + this.FileName;
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
          string format = satId != 0 ? "S{0}-{1}-{2}-{3}" : "C-{1}-{2}-{3}";
          string uid = string.Format(format, this.DataRoot.Satellites.TryGet(satId).OrbitalPosition,
                                r.GetInt32(1), r.GetInt32(2), r.GetInt32(3));
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
          ReadAnalogChannel(r, fields);
      }
    }

    private void ReadAnalogChannel(SQLiteDataReader r, IDictionary<string, int> field)
    {
      var bits = r.GetInt32(field["list_bits"]);
      if ((bits & BITS_Tv) == 0)
        return;

      ChannelInfo channel = new ChannelInfo(SignalSource.Analog|SignalSource.Tv,
                                            r.GetInt32(field["channel_handle"]),
                                            r.GetInt32(field["channel_number"]),
                                            r.GetString(field["channel_label"]));

      channel.FreqInMhz = (decimal) r.GetInt32(field["frequency"])/1000000;
      channel.Favorites = this.ParseFavorites(bits);

      this.DataRoot.AddChannel(this.atvChannels, channel);
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
          ReadDigitalChannel(r, fields, signalSource, tvChannels, radioChannels);
      }
    }
    #endregion

    #region ReadDigitalChannel()
    private void ReadDigitalChannel(SQLiteDataReader r, IDictionary<string, int> field, SignalSource signalSource, ChannelList tvChannels, ChannelList radioChannels)
    {
      var name = r.GetString(field["channel_label"]);
      string longName, shortName;
      this.GetChannelNames(name, out longName, out shortName);
      
      ChannelInfo channel = new ChannelInfo(signalSource,
                                            r.GetInt32(field["channel_handle"]),
                                            r.GetInt32(field["channel_number"]),
                                            longName);
      channel.ShortName = shortName;
      channel.RecordOrder = r.GetInt32(field["channel_order"]);
      channel.FreqInMhz = (decimal)r.GetInt32(field["frequency"]) / 1000;
      int serviceType = r.GetInt32(field["dvb_service_type"]);
      if (serviceType == 1 || serviceType == 25)
        channel.SignalSource |= SignalSource.Tv;
      else if (serviceType == 2)
        channel.SignalSource |= SignalSource.Radio;
      channel.ServiceType = serviceType;
      channel.OriginalNetworkId = r.GetInt32(field["onid"]);
      channel.TransportStreamId = r.GetInt32(field["tsid"]);
      channel.ServiceId = r.GetInt32(field["sid"]);
      int bits = r.GetInt32(field["list_bits"]);
      channel.Favorites = this.ParseFavorites(bits);
      channel.Lock = (bits & BITS_Locked) != 0;
      if ((signalSource & SignalSource.Sat) != 0)
      {
        int satId = r.GetInt32(field["sat_id"]);
        var sat = this.DataRoot.Satellites.TryGet(satId);
        if (sat != null)
        {
          channel.Satellite = sat.Name;
          channel.SatPosition = sat.OrbitalPosition;
          int tpId = satId*1000000 + (int) channel.FreqInMhz;
          var tp = this.DataRoot.Transponder.TryGet(tpId);
          if (tp != null)
          {
            channel.SymbolRate = tp.SymbolRate;
          }
        }
      }
      channel.Encrypted = this.channelInfoByUid.TryGet(channel.Uid);

      this.DataRoot.AddChannel(serviceType == 2 ? radioChannels : tvChannels, channel);
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

    #region GetChannelNames()
    private void GetChannelNames(string name, out string longName, out string shortName)
    {
      StringBuilder sbLong = new StringBuilder();
      StringBuilder sbShort = new StringBuilder();

      bool inShort = false;
      foreach (char c in name)
      {
        if (c == 0x86)
          inShort = true;
        else if (c == 0x87)
          inShort = false;
        if (c >= 0x80 && c <= 0x9F)
          continue;

        if (inShort)
          sbShort.Append(c);
        sbLong.Append(c);
      }

      longName = sbLong.ToString();
      shortName = sbShort.ToString();
    }
    #endregion

    #region ParseFavorites()
    private Favorites ParseFavorites(int bits)
    {
      Favorites fav = 0;
      if ((bits & BITS_FavA) != 0) fav |= Favorites.A;
      if ((bits & BITS_FavB) != 0) fav |= Favorites.B;
      if ((bits & BITS_FavC) != 0) fav |= Favorites.C;
      if ((bits & BITS_FavD) != 0) fav |= Favorites.D;
      return fav;
    }
    #endregion

    public override void Save(string tvOutputFile, string csvOutputFile)
    {
      throw new NotImplementedException();
    }

  }
}
