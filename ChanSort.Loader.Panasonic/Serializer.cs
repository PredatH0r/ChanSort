using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
  class Serializer : SerializerBase
  {
    private readonly ChannelList atvChannels = new ChannelList(SignalSource.AnalogCT | SignalSource.Tv, "Analog");
    private readonly ChannelList dtvTvChannels = new ChannelList(SignalSource.DvbCT | SignalSource.DvbS | SignalSource.Tv, "DTV");
    private readonly ChannelList dtvRadioChannels = new ChannelList(SignalSource.DvbCT | SignalSource.DvbS | SignalSource.Radio, "Radio");

    private string tempFile;

    public Serializer(string inputFile) : base(inputFile)
    {
      this.DataRoot.AddChannelList(this.atvChannels);
      this.DataRoot.AddChannelList(this.dtvTvChannels);
      this.DataRoot.AddChannelList(this.dtvRadioChannels);
    }

    public override string DisplayName { get { return "Panasonic .db Loader"; } }

    #region Load()
    public override void Load()
    {
      this.tempFile = Path.GetTempFileName();
      Application.ApplicationExit += CleanTempFile;
      this.CypherFile(this.FileName, this.tempFile);

      this.CreateDummySatellites();

      string channelConnString = "Data Source=" + this.tempFile;
      using (var conn = new SQLiteConnection(channelConnString))
      {
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
          this.ReadChannels(cmd);
        }
      }
    }
    #endregion

    #region CypherFile()
    /// <summary>
    /// XOR-based cypher which can be used to alternately crypt/decrypt data
    /// </summary>
    private void CypherFile(string input, string output)
    {
      byte[] fileContent = File.ReadAllBytes(input);
      int chiffre = 0x0388;
      int step = 0;
      for (int i = 0; i < fileContent.Length /*- 41*/; i++)
      {
        byte b = fileContent[i];
        fileContent[i] = (byte)(b ^ (chiffre >> 8));
        if (++step < 256)
          chiffre += b + 0x96A3;
        else
        {
          chiffre = 0x0388;
          step = 0;
        }
      }
      File.WriteAllBytes(output, fileContent);
    }
    #endregion

    #region CleanTempFile()
    private void CleanTempFile(object sender, EventArgs e)
    {
      try { File.Delete(this.tempFile);}
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

    #region ReadChannels()
    private void ReadChannels(SQLiteCommand cmd)
    {
      string[] fieldNames = { "rowid", "major_channel", "physical_ch","sname", "freq", "skip", "running_status","free_CA_mode","child_lock",
                            "profile1index","profile2index","profile3index","profile4index","stype", "onid", "tsid", "sid", "ntype", "delivery" };
      
      const string sql = "select s.rowid,s.major_channel,s.physical_ch,s.sname,t.freq,s.skip,s.running_status,s.free_CA_mode,s.child_lock, " +
                         "profile1index,profile2index,profile3index,profile4index,s.stype,s.onid,s.tsid,s.svcid,s.ntype,delivery" +
                         " from SVL s left outer join TSL t on s.ntype=t.ntype and s.physical_ch=t.physical_ch and s.tsid=t.tsid"+
                         " order by s.ntype,major_channel";
      
      var fields = this.GetFieldMap(fieldNames);

      cmd.CommandText = sql;
      using (var r = cmd.ExecuteReader())
      {
        while (r.Read())
        {
          ChannelInfo channel = new DbChannel(r, fields, this.DataRoot);
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


    public override void Save(string tvOutputFile)
    {
    }
  }
}
