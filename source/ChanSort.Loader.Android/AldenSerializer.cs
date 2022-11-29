using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.Android
{
  /*

  Android TVs typically store a SQLite based data file containing an "android_metadata" table. The rest differs from brand to brand.
  
  Philips ChannelMap_30 and _45 use the android database format for the tv.db file. That's handled in the Philips loaders, not here.
  
  Alden uses a variant very similar to Philips, but instead of a single "channels" table containing channels from all sources,
  it has distinct (atv|dtv)_(cable|antena|satellite)_channels tables. And TV and Radio channels are also put in different lists, starting at 1 each
  
  */
  public class AldenSerializer : SerializerBase
  {
    private readonly ChannelList analChannels = new ChannelList(SignalSource.Analog, "Analog");
    private readonly ChannelList dvbtTvChannels = new ChannelList(SignalSource.DvbT|SignalSource.Tv, "DVB-T TV");
    private readonly ChannelList dvbtRadioChannels = new ChannelList(SignalSource.DvbT|SignalSource.Radio, "DVB-T Radio");
    private readonly ChannelList dvbtDataChannels = new ChannelList(SignalSource.DvbT | SignalSource.Data, "DVB-T Data");
    private readonly ChannelList dvbcTvChannels = new ChannelList(SignalSource.DvbC|SignalSource.Tv, "DVB-C TV");
    private readonly ChannelList dvbcRadioChannels = new ChannelList(SignalSource.DvbC|SignalSource.Radio, "DVB-C Radio");
    private readonly ChannelList dvbcDataChannels = new ChannelList(SignalSource.DvbC | SignalSource.Data, "DVB-C Data");
    private readonly ChannelList dvbsTvChannels = new ChannelList(SignalSource.DvbS|SignalSource.Tv, "DVB-S TV");
    private readonly ChannelList dvbsRadioChannels = new ChannelList(SignalSource.DvbS|SignalSource.Radio, "DVB-S Radio");
    private readonly ChannelList dvbsDataChannels = new ChannelList(SignalSource.DvbS | SignalSource.Data, "DVB-S Data");

    private readonly StringBuilder logMessages = new StringBuilder();

    private readonly Tuple<string, ChannelList, ChannelList, ChannelList>[] subLists;


    #region ctor()
    public AldenSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanHaveGaps = true; // at least the DVB-S Data list had gaps
      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      this.Features.MaxFavoriteLists = 1;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = false;

      this.subLists = new[]
      {
        Tuple.Create("atv_cable_channels", analChannels, (ChannelList)null, (ChannelList)null),
        Tuple.Create("dtv_antena_channels", dvbtTvChannels, dvbtRadioChannels, dvbtDataChannels),
        Tuple.Create("dtv_cable_channels", dvbcTvChannels, dvbcRadioChannels, dvbcDataChannels),
        Tuple.Create("dtv_satellite_channels", dvbsTvChannels, dvbsRadioChannels, dvbsDataChannels)
      };

      foreach (var subList in subLists)
      {
        this.DataRoot.AddChannelList(subList.Item2);
        if (subList.Item3 != null)
          this.DataRoot.AddChannelList(subList.Item3);
        if (subList.Item4 != null)
          this.DataRoot.AddChannelList(subList.Item4);
      }

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceType));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceTypeName));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Satellite));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.PcrPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.VideoPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ChannelOrTransponder));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
      }
    }
    #endregion

    // loading

    #region Load()
    public override void Load()
    {
      using var conn = new SqliteConnection($"Data Source={this.FileName};Pooling=False");
      conn.Open();
      using var cmd = conn.CreateCommand();

      foreach (var table in new[] { "dtv_satellite_channels" })
      {
        cmd.CommandText = $"select count(1) from sqlite_master where type='table' and name='{table}'";
        if ((long)cmd.ExecuteScalar() == 0)
          throw LoaderException.TryNext(ERR_UnknownFormat);
      }

      var columns = "_id, type, service_type, original_network_id, transport_stream_id, service_id, display_number, display_name, browsable, searchable, locked, "
                    + "internal_provider_flag1, internal_provider_flag4, favorite, scrambled, channel_index";
      var fields = columns.Split(',');
      var c = new Dictionary<string, int>();
      for (int i = 0; i < fields.Length; i++)
        c[fields[i].Trim()] = i;


      foreach (var subList in this.subLists)
      {
        cmd.CommandText = $"select count(1) from sqlite_master where type='table' and name='{subList.Item1}'";
        if ((long)cmd.ExecuteScalar() == 0)
          continue;
        
        cmd.CommandText = $"select {columns} from {subList.Item1}";
        using var r = cmd.ExecuteReader();

        while (r.Read())
        {
          var ch = new ChannelInfo(SignalSource.DvbS, r.GetInt32(c["_id"]), r.GetInt32(c["display_number"]), r.GetString(c["display_name"]));
          ch.OriginalNetworkId = r.GetInt16(c["original_network_id"]);
          ch.TransportStreamId = r.GetInt16(c["transport_stream_id"]);
          ch.ServiceId = r.GetInt16(c["service_id"]);
          ch.Hidden = r.GetInt16(c["browsable"]) == 0;
          ch.Skip = r.GetInt16(c["searchable"]) == 0;
          ch.Lock = r.GetInt16(c["locked"]) != 0;
          ch.FreqInMhz = (decimal)r.GetInt64(c["internal_provider_flag1"]) / 100000;
          ch.SymbolRate = r.GetInt32(c["internal_provider_flag4"]) / 1000;
          var f = r.GetInt32(c["favorite"]); // unknown if this is a flag or an ordered number for a single fav list. assuming the latter for now
          ch.SetOldPosition(1, f == 0 ? -1 : f);
          ch.Encrypted = r.GetBoolean(c["scrambled"]);
          ch.RecordOrder = r.GetInt32(c["channel_index"]);

          var source = r.GetString(c["service_type"]);
          ChannelList list;
          if (source == "SERVICE_TYPE_AUDIO_VIDEO")
          {
            list = subList.Item2;
            ch.SignalSource |= SignalSource.Tv;
          }
          else if (source == "SERVICE_TYPE_AUDIO")
          {
            list = subList.Item3;
            ch.SignalSource |= SignalSource.Radio;
          }
          else
          {
            list = subList.Item4;
            ch.SignalSource |= SignalSource.Data;
          }
          this.DataRoot.AddChannel(list, ch);
        }
      }

    }
    #endregion


    #region Save()
    /// <summary>
    /// The "tv.db" file was reported to exist as early as in ChannelMap_25 format and has been seen in formats 30 and 45 too
    /// </summary>
    public override void Save()
    {
      using var conn = new SqliteConnection($"Data Source={this.FileName};Pooling=False");
      conn.Open();
      using var trans = conn.BeginTransaction();
      using var cmd = conn.CreateCommand();
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@id", SqliteType.Integer);
      cmd.Parameters.Add("@prNum", SqliteType.Text);
      cmd.Parameters.Add("@name", SqliteType.Text);
      cmd.Parameters.Add("@browsable", SqliteType.Integer);
      cmd.Parameters.Add("@searchable", SqliteType.Integer);
      cmd.Parameters.Add("@locked", SqliteType.Integer);

      using var del = conn.CreateCommand();
      del.Parameters.Add("@id", SqliteType.Integer);

      foreach (var list in this.DataRoot.ChannelLists)
      {
        var table = this.subLists.First(sl => sl.Item2 == list || sl.Item3 == list || sl.Item4 == list).Item1;
        cmd.CommandText = $"update {table} set display_number=@prNum, display_name=@name, browsable=@browsable, searchable=@searchable, locked=@locked where _id=@id";
        cmd.Prepare();

        del.CommandText = $"delete from {table} where _id=@id";
        del.Prepare();

        foreach (var ch in list.Channels)
        {
          if (ch.IsProxy)
            continue;

          if (ch.IsDeleted)
          {
            del.Parameters["@id"].Value = ch.RecordOrder;
            del.ExecuteNonQuery();
          }
          else
          {
            cmd.Parameters["@id"].Value = ch.RecordIndex;
            cmd.Parameters["@prNum"].Value = ch.NewProgramNr.ToString();
            cmd.Parameters["@name"].Value = ch.Name;
            cmd.Parameters["@browsable"].Value = ch.Hidden ? 0 : 1; // TODO check which one is which (Skip/Hide)
            cmd.Parameters["@searchable"].Value = ch.Skip ? 0 : 1;
            cmd.Parameters["@locked"].Value = ch.Lock ? 1 : 0;
            cmd.ExecuteNonQuery();
          }
        }
      }
      trans.Commit();
      conn.Close();
    }
    #endregion

    // framework support methods

    #region GetFileInformation
    public override string GetFileInformation()
    {
      return base.GetFileInformation() + this.logMessages.Replace("\n", "\r\n");
    }
    #endregion
  }
}
