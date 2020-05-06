#define LOCK_LCN_LISTS

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.Hisense2017
{
  public class HisDbSerializer : SerializerBase
  {
    /*
     * The 2017 Hisense / Loewe data model for channel lists is a bit different than all other supported models and need some workarounds to be supported.
     * It is based on a flat "Services" table which doesn't hold program numbers and a FavoritesList/FavoritesItem table to assign numbers
     * to physical tuner lists and user favorite lists alike.
     * 
     * Physical channel lists (e.g. for $av, Astra, Hot Bird) have their own ChannelList in the channelList dictionary and use 
     * ChannelInfo.NewProgramNr to hold the program number. This doesn't allow the user to add services from other lists.
     * 
     * The user favorite lists (FAV1-FAV4) use the separate favList ChannelList filled with all services from all physical lists.
     * ChannelInfo.FavIndex[0-3] holds the information for the program numbers in FAV1-4. The value -1 is used to indicate "not included".
     * 
     * The $all list is hidden from the user and automatically updated to match the contents of all other lists (except $av and FAV1-4).
     * 
     * The $av list is hidden from the user and not updated at all.
     * 
     * This loader poses the following restrictions on the database:
     * - a service must not appear in more than one physical channel list ($all and FAV1-4 are not part of this restriction)
     * - a service can't appear more than once in any list
     * 
     */

    /// <summary>
    /// list of all table names in the database
    /// </summary>
    private readonly List<string> tableNames = new List<string>();

    /// <summary>
    /// mapping of Service.Pid => ChannelInfo
    /// </summary>
    private readonly Dictionary<long, ChannelInfo> channelsById = new Dictionary<long, ChannelInfo>();

    /// <summary>
    /// mapping of FavoriteList.Pid => ChannelList. 
    /// This dict does not include real user favorite lists (FAV1-FAV4).
    /// </summary>
    private readonly Dictionary<int, ChannelList> channelLists = new Dictionary<int, ChannelList>();
    
    /// <summary>
    /// This list is filled with all channels/services and serves as a holder for favorite lists 1-4
    /// </summary>
    private readonly ChannelList userFavList = new ChannelList(0, "Favorites");

    /// <summary>
    /// mapping of FavoriteList.Pid for FAV1-4 => index of the internal favorite list within userFavList (0-3)
    /// Pids that don't belong to the FAV1-4 are not included in this dictionary.
    /// </summary>
    private readonly Dictionary<int,int> favListIdToFavIndex = new Dictionary<int, int>();

    /// <summary>
    /// FavoriteList.Pid of the $all list
    /// </summary>
    private int pidAll;

    /// <summary>
    /// FavoriteList.Pid of the $av list
    /// </summary>
    private int pidAv;

    /// <summary>
    /// Fields of the ChannelInfo that will be shown in the UI
    /// </summary>
    private static readonly List<string> ColumnNames = new List<string>
      {
        "OldPosition",
        "Position",
        "Source",
        "NewProgramNr",
        "Name",
        "ShortName",
        "Favorites",
        "Skip",
        "Lock",
        "Hidden",
        "Encrypted",
        "FreqInMhz",
        "OriginalNetworkId",
        "TransportStreamId",
        "ServiceId",
        //"ServiceType",
        "ServiceTypeName",
        "NetworkName",
        "Satellite"
//        "SymbolRate"
      };

    #region class HisTransponder
    /// <summary>
    /// This class holds information from the Tuner table
    /// </summary>
    public class HisTransponder : Transponder
    {
      public SignalSource SignalSource { get; set; }
      public string Source { get; set; }

      public HisTransponder(int id) : base(id)
      {
      }
    }
    #endregion

    #region ctor()

    public HisDbSerializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();

      Features.ChannelNameEdit = ChannelNameEditMode.All;
      Features.DeleteMode = DeleteMode.FlagWithPrNr;
      Features.CanSkipChannels = true;
      Features.CanLockChannels = true;
      Features.CanHideChannels = true;
      Features.CanHaveGaps = true;
      Features.MixedSourceFavorites = true;
      Features.SortedFavorites = true;
    }

    #endregion

    #region Load()

    public override void Load()
    {
      using (var conn = new SQLiteConnection("Data Source=" + FileName))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          RepairCorruptedDatabaseImage(cmd);
          LoadTableNames(cmd);

          // make sure this .db file contains the required tables
          if (!tableNames.Contains("service") || !tableNames.Contains("tuner") || !tableNames.Contains("favoriteitem"))
            throw new FileLoadException("File doesn't contain service/tuner/favoriteitem tables");

          LoadLists(cmd);
          LoadTunerData(cmd);
          LoadServiceData(cmd);
          LoadFavorites(cmd);
        }
      }

      if (channelsById.Count == 0)
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

          if (name == "$all")
            pidAll = listId;
          else if (name == "$av")
            pidAv = listId;
          else if (name.StartsWith("FAV"))
          {
            // all real user favorite lists are using the "userFavList"
            favListIdToFavIndex.Add(listId, int.Parse(name.Substring(3)) - 1);
            continue;
          }

          // lists for physical channel sources
          var list = new ChannelList(0, name);
          list.VisibleColumnFieldNames = ColumnNames;
          channelLists.Add(listId, list);
          if (name.StartsWith("$"))
            list.ReadOnly = true;
          else
            DataRoot.AddChannelList(list); // only lists in the DataRoot will be visible in the UI
        }
      }

      // add the special list for the user favorites 1-4
      userFavList.VisibleColumnFieldNames = ColumnNames;
      userFavList.IsMixedSourceFavoritesList = true;
      channelLists.Add(0, userFavList);
      DataRoot.AddChannelList(userFavList);
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
select s.pid, s.type, anls.Frequency, digs.TunerId, digs.Sid, Name, ShortName, Encrypted, Visible, Selectable, ParentalLock, MediaType
from service s
left outer join AnalogService anls on anls.ServiceId=s.Pid
left outer join DVBService digs on digs.ServiceId=s.Pid
";

      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          ChannelInfo ci = null;
          if (!r.IsDBNull(2)) // AnalogService
            ci = new ChannelInfo(SignalSource.Analog, r.GetInt32(0), -1, r.GetString(5));
          else if (!r.IsDBNull(3)) // DvbService
          {
            var trans = (HisTransponder) DataRoot.Transponder.TryGet(r.GetInt32(3));
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
            var mediaType = r.GetInt32(11);
            if (mediaType == 1)
            {
              ci.SignalSource |= SignalSource.Tv;
              ci.ServiceTypeName = "TV";
            }
            else if (mediaType == 2)
            {
              ci.SignalSource |= SignalSource.Radio;
              ci.ServiceTypeName = "Radio";
            }
            else
              ci.ServiceTypeName = mediaType.ToString();
          }
          else if (r.GetInt32(1) == 0) // A/V input
          {
            ci = new ChannelInfo(SignalSource.AvInput, r.GetInt32(0), -1, r.GetString(5));
            ci.ServiceTypeName = "A/V";
          }

          if (ci != null)
            channelsById.Add(ci.RecordIndex, ci);
        }
      }
    }
    #endregion

    #region LoadFavorites()

    private void LoadFavorites(SQLiteCommand cmd)
    {
      cmd.CommandText = @"
select fi.FavoriteId, fi.ServiceId, fi.ChannelNum, fi.Selectable, fi.Visible, fi.isDeleted, fi.Protected, l.Lcn 
from FavoriteItem fi 
left outer join Lcn l on l.ServiceId=fi.ServiceId and l.FavoriteId=fi.FavoriteId
";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          int favListId = r.GetInt32(0);
          var ci = channelsById.TryGet(r.GetInt32(1));
          if (ci == null)
            continue;
          
          int favListIdx = favListIdToFavIndex.TryGet(favListId, -1);
          if (favListIdx >= 0)
            ci.OldFavIndex[favListIdx] = r.GetInt32(2);

          ci.SetOldPosition(favListIdx + 1, r.GetInt32(2)); // 0=main nr, 1-4=fav 1-4
          if (favListIdx < 0)
          {
            // physical channel list (specific satellite, $av, ...)
            var list = channelLists.TryGet(favListId);

            if (!r.IsDBNull(7)) // LCN
            {
              ci.ProgramNrPreset = r.GetInt32(7);
#if LOCK_LCN_LISTS
              list.ReadOnly = true;
#endif
            }

            ci.Skip = r.GetInt32(3) == 0;
            ci.Lock = r.GetInt32(6) != 0;
            ci.Hidden = r.GetInt32(4) == 0;
            ci.IsDeleted = r.GetInt32(5) != 0;
            ci.Source = list.ShortCaption;
            if (ci.IsDeleted)
              ci.OldProgramNr = -1;
            if ((ci.SignalSource & (SignalSource.MaskAntennaCableSat | SignalSource.MaskAnalogDigital)) == SignalSource.DvbS)
              ci.Satellite = list.ShortCaption;

            DataRoot.AddChannel(list, ci);
          }
        }
      }

      foreach(var ci in channelsById.Values)
        DataRoot.AddChannel(userFavList, ci);
    }
    #endregion

    // Saving ====================================

    #region Save()

    public override void Save(string tvOutputFile)
    {
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
#if !LOCK_LCN_LISTS
            ResetLcn(cmd);
#endif
            UpdateServices(cmd);
            UpdatePhysicalChannelLists(cmd);
            UpdateUserFavoriteLists(cmd);

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

    #region ResetLcn()

    private void ResetLcn(SQLiteCommand cmd)
    {
      cmd.CommandText = "delete from Lcn where FavoriteId<>" + pidAv;
      cmd.ExecuteNonQuery();
    }

    #endregion

    #region UpdateServices()
    private void UpdateServices(SQLiteCommand cmd)
    {
      cmd.CommandText = "update Service set Name=@name, ShortName=@sname, ParentalLock=@lock, Visible=@vis, Selectable=@sel, FavTag=@fav1, FavTag2=@fav1, FavTag3=@fav3, FavTag4=@fav4 where Pid=@servId";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@servId", DbType.Int32);
      cmd.Parameters.Add("@name", DbType.String);
      cmd.Parameters.Add("@sname", DbType.String);
      cmd.Parameters.Add("@lock", DbType.Int32);
      cmd.Parameters.Add("@vis", DbType.Int32);
      cmd.Parameters.Add("@sel", DbType.Int32);
      cmd.Parameters.Add("@fav1", DbType.Int32);
      cmd.Parameters.Add("@fav2", DbType.Int32);
      cmd.Parameters.Add("@fav3", DbType.Int32);
      cmd.Parameters.Add("@fav4", DbType.Int32);
      cmd.Prepare();

      foreach (var ci in channelsById.Values)
      {
        cmd.Parameters["@servId"].Value = ci.RecordIndex;
        cmd.Parameters["@name"].Value = ci.Name;
        cmd.Parameters["@sname"].Value = ci.ShortName;
        cmd.Parameters["@lock"].Value = ci.Lock ? 1 : 0;
        cmd.Parameters["@vis"].Value = ci.Hidden ? 0 : 1;
        cmd.Parameters["@sel"].Value = ci.Skip ? 0 : 1;
        cmd.Parameters["@fav1"].Value = (ci.Favorites & Favorites.A) != 0 ? 1 : 0;
        cmd.Parameters["@fav2"].Value = (ci.Favorites & Favorites.B) != 0 ? 1 : 0;
        cmd.Parameters["@fav3"].Value = (ci.Favorites & Favorites.C) != 0 ? 1 : 0;
        cmd.Parameters["@fav4"].Value = (ci.Favorites & Favorites.D) != 0 ? 1 : 0;
        cmd.ExecuteNonQuery();
      }
    }
    #endregion

    #region UpdatePhysicalChannelLists()
    private void UpdatePhysicalChannelLists(SQLiteCommand cmd)
    {
      cmd.CommandText = "update FavoriteItem set ChannelNum=@ch, isDeleted=@del, Protected=@prot, Selectable=@sel, Visible=@vis where FavoriteId=@favId and ServiceId=@servId";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@favId", DbType.Int32);
      cmd.Parameters.Add("@servId", DbType.Int32);
      cmd.Parameters.Add("@ch", DbType.Int32);
      cmd.Parameters.Add("@del", DbType.Int32);
      cmd.Parameters.Add("@prot", DbType.Int32);
      cmd.Parameters.Add("@sel", DbType.Int32);
      cmd.Parameters.Add("@vis", DbType.Int32);
      cmd.Prepare();

      foreach (var entry in channelLists)
      {
        var list = entry.Value;
        if (list.ReadOnly) // don't update read-only lists (i.e. containing LCNs)
          continue;

        // don't update the $all list directly. It will be updated while iterating all other lists
        var favId = entry.Key;
        if (favId == pidAll)
          continue;

        foreach (var ci in list.Channels)
        {
          if (ci.IsProxy) // ignore proxies for missing channels that might have been added by applying a reference list
            continue;

          cmd.Parameters["@favId"].Value = favId;
          cmd.Parameters["@servId"].Value = ci.RecordIndex;
          cmd.Parameters["@ch"].Value = ci.NewProgramNr;
          cmd.Parameters["@del"].Value = ci.IsDeleted ? 1 : 0; // 1 or -1 ?
          // not sure if the following columns are used at all. they also exist in the Services table
          cmd.Parameters["@prot"].Value = ci.Lock ? -1 : 0;
          cmd.Parameters["@sel"].Value = ci.Skip ? 0 : -1;
          cmd.Parameters["@vis"].Value = ci.Hidden ? 0 : -1;
          cmd.ExecuteNonQuery();

          // update the $all list with the same values
          if (pidAll != 0 && favId != pidAv)
          {
            cmd.Parameters["@favId"].Value = pidAll;
            cmd.ExecuteNonQuery();
          }
        }
      }
    }
    #endregion

    #region UpdateUserFavoriteLists()
    private void UpdateUserFavoriteLists(SQLiteCommand cmd)
    {
      // delete all FavoriteItem records that belong to the FAV1-4 lists
      cmd.Parameters.Clear();
      cmd.CommandText = "delete from FavoriteItem where FavoriteId in (select Pid from FavoriteList where name like 'FAV_')";
      cmd.ExecuteNonQuery();

      // (re-)insert the user's new favorites
      cmd.CommandText = "insert into FavoriteItem (FavoriteId, ServiceId, ChannelNum) values (@favId, @servId, @ch)";
      cmd.Parameters.Add("@favId", DbType.Int32);
      cmd.Parameters.Add("@servId", DbType.Int32);
      cmd.Parameters.Add("@ch", DbType.Int32);
      foreach (var entry in favListIdToFavIndex)
      {
        var favIndex = entry.Value;
        cmd.Parameters["@favId"].Value = entry.Key;
        foreach (var ci in userFavList.Channels)
        {
          if (ci.IsProxy) // ignore proxies for missing channels that might have been added by applying a reference list
            continue;

          var num = ci.GetPosition(favIndex + 1);
          if (num > 0)
          {
            cmd.Parameters["@servId"].Value = ci.RecordIndex;
            cmd.Parameters["@ch"].Value = num;
            cmd.ExecuteNonQuery();
          }
        }
      }
    }

    #endregion

  }
}