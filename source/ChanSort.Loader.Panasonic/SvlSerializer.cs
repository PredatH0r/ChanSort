using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  internal enum ByteOrder { BigEndian, LittleEndian }

  class SvlSerializer : SerializerBase
  {
    private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT, "Analog Antenna");
    private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC, "Analog Cable");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC, "DVB-C");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS, "DVB-S");
    private readonly ChannelList satipChannels = new ChannelList(SignalSource.SatIP, "SAT>IP");
    private readonly ChannelList freesatChannels = new ChannelList(SignalSource.DvbS | SignalSource.Freesat, "Freesat");

    private string workFile;
    private CypherMode cypherMode;
    private byte[] fileHeader = Array.Empty<byte>();
    private int dbSizeOffset;
    private ByteOrder headerByteOrder;
    private string charEncoding;
    private bool explicitUtf8;
    private bool implicitUtf8;

    enum CypherMode
    {
      None,
      HeaderAndChecksum,
      Encryption,
      Unknown
    }

    #region ctor()
    public SvlSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None; // due to the chaos with binary data inside the "sname" string column, writing back a name has undesired side effects
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = false;
      this.Features.CanHaveGaps = false;
      this.Features.EncryptedFlagEdit = true;
      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      
      this.DataRoot.AddChannelList(this.avbtChannels);
      this.DataRoot.AddChannelList(this.avbcChannels);
      this.DataRoot.AddChannelList(this.dvbtChannels);
      this.DataRoot.AddChannelList(this.dvbcChannels);
      this.DataRoot.AddChannelList(this.dvbsChannels);
      this.DataRoot.AddChannelList(this.satipChannels);
      this.DataRoot.AddChannelList(this.freesatChannels);

      // hide columns for fields that don't exist in Panasonic channel list
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("PcrPid");
        list.VisibleColumnFieldNames.Remove("VideoPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
      }
    }
    #endregion

    #region Load()
    public override void Load()
    {
      this.workFile = this.GetUncypheredWorkFile();

      this.CreateDummySatellites();

      string channelConnString = $"Data Source={this.workFile};Pooling=False";
      using var conn = new SqliteConnection(channelConnString);
      conn.Open();
      using var cmd = conn.CreateCommand();
      RepairCorruptedDatabaseImage(cmd);
      InitCharacterEncoding(cmd);

      cmd.CommandText = "SELECT count(1) FROM sqlite_master WHERE type = 'table' and name in ('SVL', 'TSL')";
      if (Convert.ToInt32(cmd.ExecuteScalar()) != 2)
        throw LoaderException.TryNext("File doesn't contain the expected TSL/SVL tables");

      this.ReadChannels(cmd);
    }
    #endregion

    #region GetUncypheredWorkFile()
    private string GetUncypheredWorkFile()
    {
      this.cypherMode = this.GetCypherMode(this.FileName);
      if (cypherMode == CypherMode.Unknown)
        throw LoaderException.TryNext(ERR_UnknownFormat);
      if (cypherMode == CypherMode.None)
        return this.FileName;

      this.TempPath = Path.GetTempFileName();
      this.DeleteTempPath();

      if (cypherMode == CypherMode.Encryption)
        this.CypherFile(this.FileName, this.TempPath, false);
      else
        this.RemoveHeader(this.FileName, this.TempPath);
      return this.TempPath;
    }
    #endregion

    #region GetCypherMode()
    private CypherMode GetCypherMode(string file)
    {
      using var stream = File.OpenRead(file);
      using var rdr = new BinaryReader(stream);
      uint value = (uint)rdr.ReadInt32();
      if (value == 0x694C5153) return CypherMode.None; // "SQLi"
      if (value == 0x42445350) return CypherMode.HeaderAndChecksum; // "PSDB"
      if (value == 0xA07DCB50) return CypherMode.Encryption;
      return CypherMode.Unknown;
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
        throw LoaderException.Fail("Checksum validation failed");

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
        throw LoaderException.Fail("Checksum validation failed");

      int offset;
      if (!this.ValidateFileSize(data, ByteOrder.BigEndian, out offset) && 
          !this.ValidateFileSize(data, ByteOrder.LittleEndian, out offset))
        throw LoaderException.Fail("File size validation failed");

      using (var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
        stream.Write(data, offset, data.Length - offset - 4);

      this.fileHeader = new byte[offset];
      Array.Copy(data, 0, this.fileHeader, 0, offset);
    }
    #endregion

    #region ValidateFileSize()
    private bool ValidateFileSize(byte[] data, ByteOrder byteOrder, out int offset)
    {
      this.headerByteOrder = byteOrder;
      offset = 30 + Tools.GetInt16(data, 28, byteOrder == ByteOrder.LittleEndian);
      if (offset >= data.Length) return false;
      this.dbSizeOffset = offset;
      int dbSize = Tools.GetInt32(data, offset, byteOrder == ByteOrder.LittleEndian);
      offset += 4;
      return data.Length == offset + dbSize + 4;
    }
    #endregion

    #region CalcChecksum()

    private uint CalcChecksum(byte[] data, int length)
    {
      return Crc32.Normal.CalcCrc32(data, 0, length);
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
    private void InitCharacterEncoding(SqliteCommand cmd)
    {
      cmd.CommandText = "PRAGMA encoding";
      this.charEncoding = cmd.ExecuteScalar() as string;
    }
    #endregion

    #region RepairCorruptedDatabaseImage()
    private void RepairCorruptedDatabaseImage(SqliteCommand cmd)
    {
      cmd.CommandText = "REINDEX";
      cmd.ExecuteNonQuery();
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels(SqliteCommand cmd)
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
      this.implicitUtf8 = true;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          var channel = new DbChannel(r, fields, this.DataRoot, this.DefaultEncoding);
          if (!channel.IsDeleted)
          {
            if (channel.RawName.Length > 0 && channel.RawName[0] == 0x15) // if there is a channel with a 0x15 encoding ID (UTF-8), we can allow editing channels
              this.explicitUtf8 = true;
            if (channel.NonAscii) // if all channels with non-ascii characters are valid UTF-8 strings, we can allow editing too
              this.implicitUtf8 &= channel.ValidUtf8;
            var channelList = this.DataRoot.GetChannelList(channel.SignalSource);
            this.DataRoot.AddChannel(channelList, channel);
          }
        }
      }

      if (this.explicitUtf8 || this.implicitUtf8)
        this.Features.ChannelNameEdit = ChannelNameEditMode.All;
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
    public override void Save()
    {
      string channelConnString = $"Data Source={this.workFile};Pooling=False";
      using (var conn = new SqliteConnection(channelConnString))
      {
        conn.Open();
        using var trans = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = trans;
        this.WriteChannels(cmd, this.avbtChannels);
        this.WriteChannels(cmd, this.avbcChannels);
        this.WriteChannels(cmd, this.dvbtChannels);
        this.WriteChannels(cmd, this.dvbcChannels);
        this.WriteChannels(cmd, this.dvbsChannels);
        this.WriteChannels(cmd, this.satipChannels);
        this.WriteChannels(cmd, this.freesatChannels);
        trans.Commit();

        cmd.Transaction = null;
        this.RepairCorruptedDatabaseImage(cmd);
      }

      this.WriteCypheredFile();
    }
    #endregion

    #region WriteChannels()
    private void WriteChannels(SqliteCommand cmd, ChannelList channelList)
    {
      if (channelList.Channels.Count == 0)
        return;

      cmd.CommandText = "update SVL set major_channel=@progNr, sname=@sname, profile1index=@fav1, profile2index=@fav2, profile3index=@fav3, profile4index=@fav4, child_lock=@lock, skip=@skip where rowid=@rowid";
      cmd.Parameters.Clear();
      cmd.Parameters.Add("@rowid", SqliteType.Integer);
      cmd.Parameters.Add("@progNr", SqliteType.Integer);
      cmd.Parameters.Add("@sname", this.implicitUtf8 ? SqliteType.Text : SqliteType.Blob); // must use "TEXT" when possible to preserve collation / case-insensitive sorting for the TV
      cmd.Parameters.Add("@fav1", SqliteType.Integer);
      cmd.Parameters.Add("@fav2", SqliteType.Integer);
      cmd.Parameters.Add("@fav3", SqliteType.Integer);
      cmd.Parameters.Add("@fav4", SqliteType.Integer);
      cmd.Parameters.Add("@lock", SqliteType.Integer);
      cmd.Parameters.Add("@skip", SqliteType.Integer);
      cmd.Prepare();
      foreach (ChannelInfo channelInfo in channelList.Channels)
      {
        var channel = channelInfo as DbChannel;
        if (channel == null) // skip reference list proxy channels
          continue;
        if (channel.IsDeleted && channel.OldProgramNr >= 0)
          continue;
        channel.UpdateRawData(this.explicitUtf8, this.implicitUtf8);
        cmd.Parameters["@rowid"].Value = channel.RecordIndex;
        cmd.Parameters["@progNr"].Value = channel.NewProgramNr;
        cmd.Parameters["@sname"].Value = this.implicitUtf8 ? channel.Name : channel.RawName; // must use a string when possible to preserve collation / case-insensitive sorting for the TV
        for (int fav = 0; fav < 4; fav++)
          cmd.Parameters["@fav" + (fav + 1)].Value = Math.Max(0, channel.GetPosition(fav+1));
        cmd.Parameters["@lock"].Value = channel.Lock;
        cmd.Parameters["@skip"].Value = channel.Skip;
        cmd.ExecuteNonQuery();
      }

      // delete unassigned channels
      cmd.CommandText = "delete from SVL where rowid=@rowid";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SqliteParameter("@rowid", DbType.Int32));
      foreach (ChannelInfo channel in channelList.Channels)
      {
        if (channel.IsDeleted && channel.OldProgramNr >= 0)
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

      Tools.SetInt32(data, this.dbSizeOffset, (int)workFileSize, this.headerByteOrder == ByteOrder.LittleEndian);
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
          sb.Append("Header byte order: ").AppendLine(this.headerByteOrder.ToString());
          break;
      }
      sb.Append("Character encoding: ").AppendLine(this.charEncoding);
      return sb.ToString();
    }
    #endregion
  }
}
