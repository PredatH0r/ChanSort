using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  class Serializer : SerializerBase
  {
    private const string ERR_FileFormatOrEncryption = "File uses an unknown format or encryption";
    private static readonly int[] headerCypherTable;
    private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT | SignalSource.Tv | SignalSource.Radio, "Analog Antenna");
    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC | SignalSource.Tv | SignalSource.Radio, "Analog Cable");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT | SignalSource.Tv | SignalSource.Radio, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC | SignalSource.Tv | SignalSource.Radio, "DVB-C");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv | SignalSource.Radio, "DVB-S");
    private readonly ChannelList freesatChannels = new ChannelList(SignalSource.DvbS | SignalSource.Freesat | SignalSource.Tv | SignalSource.Radio, "Freesat");

    private string workFile;
    private CypherMode cypherMode;
    private byte[] fileHeader = new byte[0];
    private int dbSizeOffset;
    private bool littleEndianByteOrder = false;
    private string charEncoding;

    enum CypherMode
    {
      None,
      HeaderAndChecksum,
      Encryption,
      Unknown
    }


    #region static ctor / headerCypherTable
    static Serializer()
    {
      headerCypherTable = new[]
        {
          0,
          79764919,
          159529838,
          222504665,
          319059676,
          398814059,
          445009330,
          507990021,
          638119352,
          583659535,
          797628118,
          726387553,
          890018660,
          835552979,
          1015980042,
          944750013,
          1276238704,
          1221641927,
          1167319070,
          1095957929,
          1595256236,
          1540665371,
          1452775106,
          1381403509,
          1780037320,
          1859660671,
          1671105958,
          1733955601,
          2031960084,
          2111593891,
          1889500026,
          1952343757,
          -1742489888,
          -1662866601,
          -1851683442,
          -1788833735,
          -1960329156,
          -1880695413,
          -2103051438,
          -2040207643,
          -1104454824,
          -1159051537,
          -1213636554,
          -1284997759,
          -1389417084,
          -1444007885,
          -1532160278,
          -1603531939,
          -734892656,
          -789352409,
          -575645954,
          -646886583,
          -952755380,
          -1007220997,
          -827056094,
          -898286187,
          -231047128,
          -151282273,
          -71779514,
          -8804623,
          -515967244,
          -436212925,
          -390279782,
          -327299027,
          881225847,
          809987520,
          1023691545,
          969234094,
          662832811,
          591600412,
          771767749,
          717299826,
          311336399,
          374308984,
          453813921,
          533576470,
          25881363,
          88864420,
          134795389,
          214552010,
          2023205639,
          2086057648,
          1897238633,
          1976864222,
          1804852699,
          1867694188,
          1645340341,
          1724971778,
          1587496639,
          1516133128,
          1461550545,
          1406951526,
          1302016099,
          1230646740,
          1142491917,
          1087903418,
          -1398421865,
          -1469785312,
          -1524105735,
          -1578704818,
          -1079922613,
          -1151291908,
          -1239184603,
          -1293773166,
          -1968362705,
          -1905510760,
          -2094067647,
          -2014441994,
          -1716953613,
          -1654112188,
          -1876203875,
          -1796572374,
          -525066777,
          -462094256,
          -382327159,
          -302564546,
          -206542021,
          -143559028,
          -97365931,
          -17609246,
          -960696225,
          -1031934488,
          -817968335,
          -872425850,
          -709327229,
          -780559564,
          -600130067,
          -654598054,
          1762451694,
          1842216281,
          1619975040,
          1682949687,
          2047383090,
          2127137669,
          1938468188,
          2001449195,
          1325665622,
          1271206113,
          1183200824,
          1111960463,
          1543535498,
          1489069629,
          1434599652,
          1363369299,
          622672798,
          568075817,
          748617968,
          677256519,
          907627842,
          853037301,
          1067152940,
          995781531,
          51762726,
          131386257,
          177728840,
          240578815,
          269590778,
          349224269,
          429104020,
          491947555,
          -248556018,
          -168932423,
          -122852000,
          -60002089,
          -500490030,
          -420856475,
          -341238852,
          -278395381,
          -685261898,
          -739858943,
          -559578920,
          -630940305,
          -1004286614,
          -1058877219,
          -845023740,
          -916395085,
          -1119974018,
          -1174433591,
          -1262701040,
          -1333941337,
          -1371866206,
          -1426332139,
          -1481064244,
          -1552294533,
          -1690935098,
          -1611170447,
          -1833673816,
          -1770699233,
          -2009983462,
          -1930228819,
          -2119160460,
          -2056179517,
          1569362073,
          1498123566,
          1409854455,
          1355396672,
          1317987909,
          1246755826,
          1192025387,
          1137557660,
          2072149281,
          2135122070,
          1912620623,
          1992383480,
          1753615357,
          1816598090,
          1627664531,
          1707420964,
          295390185,
          358241886,
          404320391,
          483945776,
          43990325,
          106832002,
          186451547,
          266083308,
          932423249,
          861060070,
          1041341759,
          986742920,
          613929101,
          542559546,
          756411363,
          701822548,
          -978770311,
          -1050133554,
          -869589737,
          -924188512,
          -693284699,
          -764654318,
          -550540341,
          -605129092,
          -475935807,
          -413084042,
          -366743377,
          -287118056,
          -257573603,
          -194731862,
          -114850189,
          -35218492,
          -1984365303,
          -1921392450,
          -2143631769,
          -2063868976,
          -1698919467,
          -1635936670,
          -1824608069,
          -1744851700,
          -1347415887,
          -1418654458,
          -1506661409,
          -1561119128,
          -1129027987,
          -1200260134,
          -1254728445,
          -1309196108
        };
    }
    #endregion

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = true;
      this.DataRoot.SortedFavorites = true;
      
      this.DataRoot.AddChannelList(this.avbtChannels);
      this.DataRoot.AddChannelList(this.avbcChannels);
      this.DataRoot.AddChannelList(this.dvbtChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.dvbsChannels);
      this.DataRoot.AddChannelList(this.freesatChannels);
    }
    #endregion

    public override string DisplayName { get { return "Panasonic .db/.bin Loader"; } }

    #region Load()
    public override void Load()
    {
      this.workFile = this.GetUncypheredWorkFile();

      this.CreateDummySatellites();

      string channelConnString = "Data Source=" + this.workFile;
      using (var conn = new SQLiteConnection(channelConnString))
      {
        conn.Open();
        InitCharacterEncoding(conn);
        using (var cmd = conn.CreateCommand())
        {
          this.ReadChannels(cmd);
        }
      }
    }
    #endregion

    #region GetUncypheredWorkFile()
    private string GetUncypheredWorkFile()
    {
      this.cypherMode = this.GetCypherMode(this.FileName);
      if (cypherMode == CypherMode.Unknown)
        throw new FileLoadException(ERR_FileFormatOrEncryption);
      if (cypherMode == CypherMode.None)
        return this.FileName;

      var tempFile = this.FileName + ".tmp";
      File.Delete(tempFile);
      Application.ApplicationExit += CleanTempFile;
      if (cypherMode == CypherMode.Encryption)
        this.CypherFile(this.FileName, tempFile, false);
      else
        this.RemoveHeader(this.FileName, tempFile);
      return tempFile;
    }
    #endregion

    #region GetCypherMode()
    private CypherMode GetCypherMode(string file)
    {
      using (var stream = File.OpenRead(file))
      using (var rdr = new BinaryReader(stream))
      {
        uint value = (uint)rdr.ReadInt32();
        if (value == 0x694C5153) return CypherMode.None; // "SQLi"
        if (value == 0x42445350) return CypherMode.HeaderAndChecksum; // "PSDB"
        if (value == 0xA07DCB50) return CypherMode.Encryption;
        return CypherMode.Unknown;
      }
    }
    #endregion

    #region CypherFile()
    /// <summary>
    /// XOR-based cypher which can be used to alternately crypt/decrypt data
    /// </summary>
    private void CypherFile(string input, string output, bool encrypt)
    {
      byte[] fileContent = File.ReadAllBytes(input);

      if (!encrypt && this.CalcChecksum(fileContent, fileContent.Length) != 0)
        throw new FileLoadException("Checksum validation failed");

      int chiffre = 0x0388;
      int step = 0;
      for (int i = 0; i < fileContent.Length - 4; i++)
      {
        byte b = fileContent[i];
        byte n = (byte) (b ^ (chiffre >> 8));
        fileContent[i] = n;
        if (++step < 256)
          chiffre += (encrypt ? n : b) + 0x96A3;
        else
        {
          chiffre = 0x0388;
          step = 0;
        }
      }

      if (encrypt)
        this.UpdateChecksum(fileContent);

      File.WriteAllBytes(output, fileContent);
    }
    #endregion

    #region RemoveHeader()
    private void RemoveHeader(string inputFile, string outputFile)
    {
      var data = File.ReadAllBytes(inputFile);
      if (this.CalcChecksum(data, data.Length) != 0)
        throw new FileLoadException("Checksum validation failed");

      int offset;
      if (!this.ValidateFileSize(data, false, out offset) 
        && !this.ValidateFileSize(data, true, out offset))
        throw new FileLoadException("File size validation failed");

      using (var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
        stream.Write(data, offset, data.Length - offset - 4);

      this.fileHeader = new byte[offset];
      Array.Copy(data, 0, this.fileHeader, 0, offset);
    }
    #endregion

    #region ValidateFileSize()
    private bool ValidateFileSize(byte[] data, bool littleEndian, out int offset)
    {
      this.littleEndianByteOrder = littleEndian;
      offset = 30 + Tools.GetInt16(data, 28, littleEndian);
      if (offset >= data.Length) return false;
      this.dbSizeOffset = offset;
      int dbSize = Tools.GetInt32(data, offset, littleEndian);
      offset += 4;
      return data.Length == offset + dbSize + 4;
    }
    #endregion

    #region CalcChecksum()
    private uint CalcChecksum(byte[] data, int length)
    {
      uint v = 0xffffffff;
      for (int i = 0; i < length; i++)
      {
        byte b = data[i];
        v = (v << 8) ^ (uint)headerCypherTable[((v >> 24) ^ b) & 0xFF];
      }
      return v;
    }
    #endregion

    #region CleanTempFile()
    private void CleanTempFile(object sender, EventArgs e)
    {
      try
      {
        if (this.workFile != null) 
          File.Delete(this.workFile);
      }
      catch { }
    }
    #endregion

    #region CreateDummySatellites()
    private void CreateDummySatellites()
    {
      for (int i = 1; i <= 4; i++)
      {
        var sat = new Satellite(i);
        sat.Name = "LNB "+i;
        sat.OrbitalPosition = i.ToString();
        this.DataRoot.Satellites.Add(i, sat);
      }
    }
    #endregion

    #region InitCharacterEncoding()
    private void InitCharacterEncoding(SQLiteConnection conn)
    {
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = "PRAGMA encoding";
        this.charEncoding = cmd.ExecuteScalar() as string;
      }
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels(SQLiteCommand cmd)
    {
      string[] fieldNames = { "rowid", "major_channel", "physical_ch","sname", "freq", "skip", "running_status","free_CA_mode","child_lock",
                            "profile1index","profile2index","profile3index","profile4index","stype", "onid", "tsid", "sid", "ntype", "ya_svcid", "delivery" };
      
      const string sql = @"
select s.rowid,s.major_channel,s.physical_ch,cast(s.sname as blob),t.freq,s.skip,s.running_status,s.free_CA_mode,s.child_lock,
  profile1index,profile2index,profile3index,profile4index,s.stype,s.onid,s.tsid,s.svcid,s.ntype,s.ya_svcid,delivery
from SVL s 
left outer join TSL t on s.ntype=t.ntype and s.physical_ch=t.physical_ch and s.tsid=t.tsid
order by s.ntype,major_channel
";
      
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          ChannelInfo channel = new DbChannel(r, fields, this.DataRoot, this.DefaultEncoding);
          if (!channel.IsDeleted)
          {
            var channelList = this.DataRoot.GetChannelList(channel.SignalSource);
            if (channelList != null)
              this.DataRoot.AddChannel(channelList, channel);
          }
        }
      }
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

    #region DefaultEncoding
    public override Encoding DefaultEncoding
    {
      get { return base.DefaultEncoding; }
      set
      {
        base.DefaultEncoding = value;
        foreach (var list in this.DataRoot.ChannelLists)
        {
          foreach(var channel in list.Channels)
            channel.ChangeEncoding(value);
        }
      }
    }
    #endregion


    #region Save()
    public override void Save(string tvOutputFile)
    {
      this.FileName = tvOutputFile;

      string channelConnString = "Data Source=" + this.workFile;
      using (var conn = new SQLiteConnection(channelConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          using (var trans = conn.BeginTransaction())
          {
            this.WriteChannels(cmd, this.avbtChannels);
            this.WriteChannels(cmd, this.avbcChannels);
            this.WriteChannels(cmd, this.dvbtChannels);
            this.WriteChannels(cmd, this.dvbcChannels);
            this.WriteChannels(cmd, this.dvbsChannels);
            this.WriteChannels(cmd, this.freesatChannels);
            trans.Commit();
          }
        }
      }

      this.WriteCypheredFile();
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SQLiteCommand cmd, ChannelList channelList)
    {
      cmd.CommandText = "update SVL set major_channel=@progNr, sname=@name, profile1index=@fav1, profile2index=@fav2, profile3index=@fav3, profile4index=@fav4, child_lock=@lock, skip=@skip where rowid=@rowid";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SQLiteParameter("@rowid", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@progNr", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@fav1", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@fav2", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@fav3", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@fav4", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@name", DbType.Binary));
      cmd.Parameters.Add(new SQLiteParameter("@lock", DbType.Int32));
      cmd.Parameters.Add(new SQLiteParameter("@skip", DbType.Int32));
      cmd.Prepare();
      foreach (DbChannel channel in channelList.Channels)
      {
        if (channel.NewProgramNr < 0 || channel.OldProgramNr < 0)
          continue;
        channel.UpdateRawData();
        cmd.Parameters["@rowid"].Value = channel.RecordIndex;
        cmd.Parameters["@progNr"].Value = channel.NewProgramNr;
        for (int fav = 0; fav < 4; fav++)
          cmd.Parameters["@fav" + (fav + 1)].Value = Math.Max(0, channel.FavIndex[fav]);
        cmd.Parameters["@name"].Value = channel.RawName;
        cmd.Parameters["@lock"].Value = channel.Lock;
        cmd.Parameters["@skip"].Value = channel.Skip;
        cmd.ExecuteNonQuery();
      }

      // delete unassigned channels
      cmd.CommandText = "delete from SVL where rowid=@rowid";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SQLiteParameter("@rowid", DbType.Int32));
      foreach (ChannelInfo channel in channelList.Channels)
      {
        if (channel.NewProgramNr == -1 && channel.OldProgramNr >= 0)
        {
          cmd.Parameters["@rowid"].Value = channel.RecordIndex;
          cmd.ExecuteNonQuery();
        }
      }
    }
    #endregion

    #region WriteCypheredFile()
    private void WriteCypheredFile()
    {
      switch (this.cypherMode)
      {
        case CypherMode.None:
          break;
        case CypherMode.Encryption:
          this.CypherFile(this.workFile, this.FileName, true);
          break;
        case CypherMode.HeaderAndChecksum:
          this.WriteFileWithHeaderAndChecksum();
          break;
      }
    }
    #endregion

    #region WriteFileWithHeaderAndChecksum()
    private void WriteFileWithHeaderAndChecksum()
    {
      long workFileSize = new FileInfo(this.workFile).Length;
      byte[] data = new byte[this.fileHeader.Length + workFileSize + 4];
      Array.Copy(fileHeader, data, fileHeader.Length);
      using (var stream = new FileStream(this.workFile, FileMode.Open, FileAccess.Read))
        stream.Read(data, fileHeader.Length, (int)workFileSize);

      Tools.SetInt32(data, this.dbSizeOffset, (int)workFileSize, this.littleEndianByteOrder);
      this.UpdateChecksum(data);

      using (var stream = new FileStream(this.FileName, FileMode.Create, FileAccess.Write))
        stream.Write(data, 0, data.Length);
    }
    #endregion

    #region UpdateChecksum()
    private void UpdateChecksum(byte[] data)
    {
      uint checksum = this.CalcChecksum(data, data.Length - 4);
      data[data.Length - 1] = (byte)(checksum & 0xFF);
      data[data.Length - 2] = (byte)((checksum >> 8) & 0xFF);
      data[data.Length - 3] = (byte)((checksum >> 16) & 0xFF);
      data[data.Length - 4] = (byte)((checksum >> 24) & 0xFF);
    }
    #endregion

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.GetFileInformation());

      sb.Append("Content type: ");
      switch (this.GetCypherMode(this.FileName))
      {
        case CypherMode.None: sb.AppendLine("unencrypted SQLite database"); break;
        case CypherMode.Encryption: sb.AppendLine("encrypted SQLite database"); break;
        case CypherMode.HeaderAndChecksum: 
          sb.AppendLine("embedded SQLite database");
          sb.Append("Byte order: ").AppendLine(this.littleEndianByteOrder ? 
            "little-endian (least significant byte first)" : "big-endian (most significant byte first)");
          break;
      }
      sb.Append("Character encoding: ").AppendLine(this.charEncoding);
      return sb.ToString();
    }
    #endregion
  }
}
