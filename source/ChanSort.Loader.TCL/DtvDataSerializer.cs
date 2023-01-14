//#define TestBuild

using System.Text;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.TCL
{
  /*
   * This class loads TCL / Thomson channel lists from a directory or a .tar file containing cloneCRC.bin, DtvData.db and satellite.db.
   *
   * None of the sample files contained more than a single input source (DVB-C/T/S), so for the time being this loader puts everything into a single list
   *
   * When a channel is added to favorites, it will: EditFlag |= 0x01, IsFavor=1, but will keep FavChannelNo=65535
   * When a channel is hidden through the TV's menu, it will result in: EditFlag |= 0x08, IsSkipped=1, leaving "VisibleFlag" unchanged (=1)
   * When a channel is deleted in the menu: EditFlag |= 0x10, IsDelete=1, but it will keep its unique ProgNum
   * When a channel is moved in the menu: EditFlag |= 0x02, but no change to IsMove(=0)
   */
  class DtvDataSerializer : SerializerBase
  {
    private const int CrcMaxDataLength = 0x4B000;

    [Flags]
    enum EditFlags
    {
      Favorite = 0x01,
      CustomProgNum = 0x02,
      Hidden = 0x08,
      Delete = 0x10,

      AllKnown = Favorite|CustomProgNum|Hidden|Delete
    }

    private readonly ChannelList dvbT = new(SignalSource.Antenna | SignalSource.MaskTvRadioData|SignalSource.Digital, "DVB-T");
    private readonly ChannelList dvbC = new(SignalSource.Cable | SignalSource.MaskTvRadioData | SignalSource.Digital, "DVB-C");
    private readonly ChannelList dvbS = new(SignalSource.Sat | SignalSource.MaskTvRadioData | SignalSource.Digital, "DVB-S");
    private string dbDir;
    private string dtvFile;
    private string satFile;
    private string crcFile;

    private readonly HashSet<string> tableNames = new();
    private readonly StringBuilder protocol = new();

    private GnuTar tar;

    #region ctor()
    public DtvDataSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
#if TestBuild
      this.Features.DeleteMode = DeleteMode.NotSupported;
#else
      this.Features.DeleteMode = DeleteMode.FlagWithoutPrNr;
#endif
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.FavoritesMode = FavoritesMode.Flags;
      this.Features.MaxFavoriteLists = 1;

      this.DataRoot.AddChannelList(this.dvbT);
      this.DataRoot.AddChannelList(this.dvbC);
      this.DataRoot.AddChannelList(this.dvbS);
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
        list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.ServiceType));
        list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Source));
      }
    }
    #endregion

    #region GetDataFilePaths()
    public override IEnumerable<string> GetDataFilePaths()
    {
      var list = new List<string>();
      list.Add(this.FileName);
      var backupFile = GetBackupFilePath();
      if (File.Exists(backupFile))
        list.Add(backupFile);
      return list;
    }

    private string GetBackupFilePath()
    {
      var dir = Path.GetDirectoryName(this.FileName) ?? ".";
      var name = Path.GetFileNameWithoutExtension(this.FileName);
      var ext = Path.GetExtension(this.FileName);
      var backupFile = Path.Combine(dir, name + "Backup" + ext);
      return backupFile;
    }

    #endregion

    #region Load()
    public override void Load()
    {
      PrepareWorkingDirectory();
      ValidateCrc();
      ReadSattelliteDb();
      ReadDtvDataDb();
    }
    #endregion
    
    #region PrepareWorkingDirectory()
    /// <summary>
    /// this.FileName might be
    /// - a .tar file containing database/cloneCRC.bin, database/userdata/DtvData.db, database/userdata/satellite.db
    /// - a .db file in a folder with DtvData.db and satellite.db and a cloneCRC.bin in either the same dir or the parent dir
    /// Other situations have already been handled in the <see cref="TclPlugin"/>
    /// </summary>
    private void PrepareWorkingDirectory()
    {
      var ext = Path.GetExtension(this.FileName).ToLowerInvariant();
      if (ext == ".tar")
      {
        UntarToTempDir();
        this.crcFile = Path.Combine(this.TempPath, "database", "cloneCRC.bin");
        this.dbDir = Path.Combine(this.TempPath, "database", "userdata");
      }
      else if (ext == ".db")
      {
        this.dbDir = Path.GetDirectoryName(this.FileName);
        this.crcFile = Path.Combine(this.dbDir, "cloneCRC.bin");
        if (!File.Exists(crcFile))
          this.crcFile = Path.Combine(Path.GetDirectoryName(this.dbDir), "cloneCRC.bin");
      }
      else
        throw LoaderException.TryNext("unrecognized TCL/Thomson directory structure");

      this.dtvFile = Path.Combine(dbDir, "DtvData.db");
      if (!File.Exists(dtvFile))
        throw LoaderException.TryNext("Missing DtvData.db file");

      this.satFile = Path.Combine(dbDir, "satellite.db");
      if (!File.Exists(satFile))
        satFile = null;

      if (!File.Exists(crcFile))
        crcFile = null;
    }
    #endregion

    #region UntarToTempDir()
    private void UntarToTempDir()
    {
      this.TempPath = Path.Combine(Path.GetTempPath(), "ChanSort_" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
      Directory.CreateDirectory(this.TempPath);

      this.tar = new GnuTar();
      tar.ExtractToDirectory(this.FileName, this.TempPath);
    }
    #endregion

    #region ValidateCrc()
    private void ValidateCrc()
    {
      if (!File.Exists(crcFile)) 
        return;

      var crcData = File.ReadAllBytes(crcFile);
      var crc = Crc16.CCITT;

      var data = File.ReadAllBytes(dtvFile);
      var actual = crc.Calc(data, 0, Math.Min(data.Length, CrcMaxDataLength));
      var expected = BitConverter.ToUInt16(crcData, 2);
      if (actual != expected)
      {
        var msg = $"Invalid CRC16-CCITT check sum for {dtvFile}. Expected {expected:X4} but calculated {actual:X4}";
        protocol.AppendLine(msg);
        throw LoaderException.Fail(msg);
      }

      if (satFile != null)
      {
        data = File.ReadAllBytes(satFile);
        actual = crc.Calc(data);
        expected = BitConverter.ToUInt16(crcData, 4);
        if (actual != expected)
        {
          var msg = $"Invalid CRC16-CCITT check sum for {satFile}. Expected {expected:X4} but calculated {actual:X4}";
          protocol.AppendLine(msg);
          throw LoaderException.Fail(msg);
        }
      }
    }

    #endregion

    #region ReadSattelliteDb()
    private void ReadSattelliteDb()
    {
      if (this.satFile == null)
        return;
      string satConnString = $"Data Source={satFile};Pooling=False";
      using var conn = new SqliteConnection(satConnString);
      conn.Open();
      using var cmd = conn.CreateCommand();

      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
          this.tableNames.Add(r.GetString(0).ToLowerInvariant());
      }

      if (!this.tableNames.Contains("sateliteinfotbl") || !this.tableNames.Contains("transponderinfotbl"))
        throw LoaderException.TryNext("File doesn't contain the expected tables");

      this.ReadSatellites(cmd);
    }

    #endregion

    #region ReadDtvDataDb()
    private void ReadDtvDataDb()
    {
      string dtvConnString = $"Data Source={dtvFile};Pooling=False";
      using var conn = new SqliteConnection(dtvConnString);
      conn.Open();
      using var cmd = conn.CreateCommand();

      this.ReadTransponders(cmd);
      this.ReadChannels(cmd);
    }
    #endregion

    #region ReadSatellites()
    private void ReadSatellites(SqliteCommand cmd)
    {
      cmd.CommandText = "select SateliteID, SateliteName, Longitude from SateliteInfoTbl";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        Satellite sat = new Satellite(r.GetInt32(0));
        string eastWest = "E";
        int pos = r.IsDBNull(2) ? 0 : r.GetInt32(2);
        if (pos != 0)
        {
          if (pos < 0)
          {
            pos = -pos;
            eastWest = "W";
          }

          sat.OrbitalPosition = $"{pos / 100}.{pos % 100}{eastWest}";
        }

        sat.Name = r.GetString(1);
        this.DataRoot.AddSatellite(sat);
      }
    }
    #endregion

    #region ReadTransponders()
    private void ReadTransponders(SqliteCommand cmd)
    {
      //cmd.CommandText = "select TransponderId, SateliteId, Freq, Polarisation, SymbolRate from TransponderInfoTbl";
      cmd.CommandText = "select u16MuxTblID, SatTblID, Freq, null, SymbolRate, TransportStreamId, OriginalNetworkId from MuxInfoTbl";
      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        int id = r.GetInt32(0);
        int satId = r.IsDBNull(1) ? -1 : r.GetInt32(1);
        int freq = r.GetInt32(2);
        
        if (this.DataRoot.Transponder.TryGet(id) != null)
          continue;
        Transponder tp = new Transponder(id);
        tp.FrequencyInMhz = (decimal)freq / 1000;
        //tp.Polarity = r.GetInt32(3) == 0 ? 'H' : 'V';
        tp.Satellite = this.DataRoot.Satellites.TryGet(satId);
        tp.SymbolRate = r.GetInt32(4);
        tp.TransportStreamId = r.GetInt32(5);
        tp.OriginalNetworkId = r.GetInt32(6);
        this.DataRoot.AddTransponder(tp.Satellite, tp);
      }
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels(SqliteCommand cmd)
    {
      cmd.CommandText = @"
select 
  p.u32Index, p.ProgNum, p.ServiceName, p.ShortServiceName, p.ServiceID, p.VideoType, p.PCRPID, p.VideoPID, p.unlockedFlag, p.LCN, p.LCNAssignmentType, p.EditFlag,
  m.OriginalNetworkId, m.TransportStreamId, m.Freq, m.SymbolRate,
  c.RouteName,
  a.RealServiceType, a.IsScramble, a.VisibleFlag, a.IsDelete, a.IsSkipped, a.IsLock, a.IsFavor, a.IsRename, a.IsMove, a.NumSelectFlag, a.FavChannelNo
from ProgramInfoTbl p 
left outer join AtrributeTbl a on a.u32index=p.u32index
left outer join MuxInfoTbl m on m.u16MuxTblID=p.u16MuxTblID
left outer join CurCIOPSerType c on c.u8DtvRoute=p.u8DtvRoute
";

      using var r = cmd.ExecuteReader();
      while (r.Read())
      {
        var handle = r.GetInt32(0);
        var oldProgNr = r.GetInt32(1);
        if (oldProgNr == 65535)
          oldProgNr = -1;

        var name = r.GetString(2)?.TrimEnd(' ', '\0');
        ChannelInfo channel = new ChannelInfo(SignalSource.Digital, handle, oldProgNr, name);
        channel.ShortName = r.GetString(3).TrimEnd(' ', '\0');
        channel.ServiceId = r.GetInt32(4);
        var vtype = r.GetInt32(5);
        channel.ServiceTypeName = vtype == 1 ? "SD-TV" : vtype == 4 ? "HD-TV" : vtype == 6 ? "UHD-TV" : null;
        channel.PcrPid = r.GetInt32(6);
        channel.VideoPid = r.GetInt32(7);
        var edit = (EditFlags)r.GetInt32(11);
        channel.Favorites = (edit & EditFlags.Favorite) != 0 ? Favorites.A : 0;
        channel.Hidden = (edit & EditFlags.Hidden) != 0;
        channel.AddDebug($"LCN={r.GetValue(9)}, edit={(int)edit:x4}");

        // DVB
        var ixD = 12;
        var ixC = ixD + 4;
        var ixA = ixC + 1;
        if (!r.IsDBNull(ixD))
        {
          channel.OriginalNetworkId = r.GetInt32(ixD + 0);
          channel.TransportStreamId = r.GetInt32(ixD + 1);
          channel.FreqInMhz = (decimal) r.GetInt32(ixD + 2) / 1000;
          channel.SymbolRate = r.GetInt32(ixD + 3);
          if (channel.FreqInMhz > 10000)
            channel.FreqInMhz = (int) channel.FreqInMhz;
        }

        // get signal source from CurCIOPSerType table
        if (r.IsDBNull(ixC))
          continue;
        channel.Source = r.GetString(ixC);
        if (channel.Source == "dvbc")
          channel.SignalSource |= SignalSource.Cable;
        else if (channel.Source == "dvbt")
          channel.SignalSource |= SignalSource.Antenna;
        else if (channel.Source == "dvbs")
          channel.SignalSource |= SignalSource.Sat;
        else
          continue;

        // AtrributeTbl (actual typo in the TV's table name!)
        if (!r.IsDBNull(ixA))
        {
          channel.ServiceType = r.GetInt32(ixA + 0);
          channel.ServiceTypeName = LookupData.Instance.GetServiceTypeDescription(channel.ServiceType);
          channel.SignalSource |= LookupData.Instance.IsRadioTvOrData(channel.ServiceType);
          channel.Encrypted = r.GetInt32(ixA + 1) != 0;
          channel.IsDeleted |= r.GetBoolean(ixA + 3);
          channel.Hidden |= r.GetBoolean(ixA + 4);
          channel.Lock = r.GetBoolean(ixA + 5);
          if (r.GetBoolean(ixA + 6))
            channel.Favorites |= Favorites.A;
          channel.IsNameModified = r.GetBoolean(ixA + 7);
          channel.AddDebug($", FavChannelNo={r.GetInt32(ixA + 10)}");
        }

        if (!channel.IsDeleted)
        {
          var list = this.DataRoot.GetChannelList(channel.SignalSource);
          this.DataRoot.AddChannel(list, channel);
        }
      }
    }
    #endregion


    #region Save()
    public override void Save()
    {
      string channelConnString = $"Data Source={dtvFile};Pooling=False";
      using (var conn = new SqliteConnection(channelConnString))
      {
        conn.Open();
        using var trans = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        using var cmd2 = conn.CreateCommand();

        this.WriteChannels(cmd, cmd2);
        trans.Commit();

        cmd.Transaction = null;
      }

      UpdateCrc();

      if (Path.GetExtension(this.FileName).ToLowerInvariant() == ".tar")
        WriteToTar();
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SqliteCommand cmd, SqliteCommand cmdAttrib)
    {
      // what the TV shows as "hide" in the menu is actually "skip" in the database

      cmd.CommandText = "update PrograminfoTbl set ProgNum=@nr"
#if !TestBuild      
        + ", ServiceName=@name, EditFlag=(EditFlag & " + ~(EditFlags.AllKnown) + ") | @editflag" // unlockedFlag=@hide,
#endif
        + " where u32Index=@handle";
      cmd.Parameters.Add("@handle", SqliteType.Integer);
      cmd.Parameters.Add("@nr", SqliteType.Integer);
#if !TestBuild
      cmd.Parameters.Add("@name", SqliteType.Blob, 64);
      cmd.Parameters.Add("@editflag", SqliteType.Integer);
#endif
      cmd.Prepare();

#if !TestBuild
      cmdAttrib.CommandText = @"update AtrributeTbl set IsDelete=@del, IsSkipped=@skip, IsLock=@lock, IsRename=@ren, IsFavor=@fav where u32Index=@handle;"; // IsMove=IsMove|@mov,
      cmdAttrib.Parameters.Add("@handle", SqliteType.Integer);
      cmdAttrib.Parameters.Add("@del", SqliteType.Integer);
      cmdAttrib.Parameters.Add("@skip", SqliteType.Integer);
      cmdAttrib.Parameters.Add("@lock", SqliteType.Integer);
      cmdAttrib.Parameters.Add("@ren", SqliteType.Integer);
      cmdAttrib.Parameters.Add("@fav", SqliteType.Integer);
      cmdAttrib.Prepare();
#endif

      foreach (var channelList in this.DataRoot.ChannelLists)
      {
        foreach (ChannelInfo channel in channelList.Channels)
        {
          if (channel.IsProxy) // ignore reference list proxy channels
            continue;

          channel.UpdateRawData();
          cmd.Parameters["@handle"].Value = channel.RecordIndex;
          cmd.Parameters["@nr"].Value = channel.IsDeleted ? 65535 : channel.NewProgramNr;
#if !TestBuild
          var bytes = Encoding.UTF8.GetBytes(channel.Name);
          var blob = new byte[64];
          Tools.MemCopy(bytes, 0, blob, 0, 64);
          cmd.Parameters["@name"].Value = blob;
          EditFlags flags = 0;
          if (channel.Favorites != 0)
            flags |= EditFlags.Favorite;
          if (channel.Hidden)
            flags |= EditFlags.Hidden;
          if (channel.IsDeleted)
            flags |= EditFlags.Delete;
          else
            flags |= EditFlags.CustomProgNum;
          cmd.Parameters["@editflag"].Value = (int)flags;

          cmdAttrib.Parameters["@handle"].Value = channel.RecordIndex;
          cmdAttrib.Parameters["@del"].Value = channel.IsDeleted ? 1 : 0;
          cmdAttrib.Parameters["@skip"].Value = channel.Hidden ? 1 : 0;
          cmdAttrib.Parameters["@lock"].Value = channel.Lock ? 1 : 0;
          cmdAttrib.Parameters["@ren"].Value = channel.IsNameModified ? 1 : 0;
          cmdAttrib.Parameters["@fav"].Value = channel.Favorites != 0 ? 1 : 0;
          cmdAttrib.ExecuteNonQuery();
#endif
          cmd.ExecuteNonQuery();
        }
      }
    }
    #endregion

    #region UpdateCrc
    /// <summary>
    /// update CRC in cloneCRC.bin
    /// </summary>
    private void UpdateCrc()
    {
      if (this.crcFile == null)
        return;

      var dtvData = File.ReadAllBytes(dtvFile);
      var crc = Crc16.CCITT.Calc(dtvData, 0, Math.Min(dtvData.Length, CrcMaxDataLength));
      var crcData = File.ReadAllBytes(this.crcFile);
      crcData[2] = (byte)(crc & 0xFF);
      crcData[3] = (byte)(crc >> 8);
      File.WriteAllBytes(crcFile, crcData);
    }
    #endregion

    #region WriteToTar()
    private void WriteToTar()
    {
      // delete old .tar file and create a new one from temp dir
      File.Delete(this.FileName);
      this.tar.UpdateFromDirectory(this.FileName);
    }
    #endregion
  }
}
