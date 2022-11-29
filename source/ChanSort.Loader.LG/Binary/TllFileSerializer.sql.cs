using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  /// <summary>
  /// For research purposes this class writes DVB-S channel information into a database
  /// It is not used for production.
  /// </summary>
  public partial class TllFileSerializer
  {
    #region SQL (create table)

    /*
     
create table list (
listid int not null,
filename varchar(100),
created datetime,
primary key (listid))

create table channel (
listid int not null,
slot int not null,
isdel bit not null,
seq int,
progmask int not null,
prognr int not null,
name varchar(40) not null,
tpnr int not null,
satnr int,
onid int not null,
tsid int not null,
ssid int not null,
uid varchar(25),
favcrypt int,
lockskiphide int,
progfix int,
primary key (listid, slot))

create table chanseq(
listid int not null,
seq int not null,
slot int not null,
primary key (listid, seq))


update channel
set seq=s.seq
from channel c inner join chanseq s on s.listid=c.listid and s.slot=c.slot
     

*/
    #endregion

    #region StoreToDatabase()
    private void StoreToDatabase()
    {
      if (this.dvbsBlockSize == 0)
        return;

      var list = this.DataRoot.GetChannelList(SignalSource.DvbS|SignalSource.Tv);
      if (list == null || list.Count == 0)
        return;

      using var conn = SqlClientFactory.Instance.CreateConnection();
      conn.ConnectionString = "server=(local);database=ChanSort;Integrated Security=true";
      conn.Open();

      using var cmd = conn.CreateCommand();
      var listId = InsertListData(cmd);

      InsertChannelLinkedList(cmd, listId);
      InsertChannelData(cmd, listId);
    }
    #endregion

    #region InsertListData()
    private int InsertListData(DbCommand cmd)
    {
      cmd.CommandText = "select max(listid) from list";
      var maxObj = cmd.ExecuteScalar();
      int listId = maxObj == DBNull.Value ? 1 : (int) maxObj + 1;

      cmd.CommandText = "insert into list(listid, filename, created) values (" + listId + ", @filename, getdate())";
      var parm = cmd.CreateParameter();
      parm.ParameterName = "@filename";
      parm.DbType = DbType.String;
      parm.Value = this.FileName;
      cmd.Parameters.Add(parm);
      cmd.ExecuteNonQuery();
      return listId;
    }
    #endregion

    #region InsertChannelLinkedList()
    private void InsertChannelLinkedList(DbCommand cmd, int listId)
    {
      cmd.Parameters.Clear();
      cmd.CommandText = "insert into chanseq(listid,seq,slot) values (" + listId + ",@seq,@slot)";
      var pSeq = cmd.CreateParameter();
      pSeq.ParameterName = "@seq";
      pSeq.DbType = DbType.Int32;
      cmd.Parameters.Add(pSeq);
      var pSlot = cmd.CreateParameter();
      pSlot.ParameterName = "@slot";
      pSlot.DbType = DbType.Int32;
      cmd.Parameters.Add(pSlot);

      SatChannelListHeader header = new SatChannelListHeader(this.fileContent,
                                                             this.dvbsBlockOffset + this.satConfig.ChannelListHeaderOffset);
      int seq = 0;
      int tableIndex = header.LinkedListStartIndex;
      int linkedListOffset = this.satConfig.SequenceTableOffset;
      while (tableIndex != 0xFFFF)
      {
        int entryOffset = linkedListOffset + tableIndex * satConfig.sizeOfChannelLinkedListEntry;
        pSeq.Value = seq;
        if (BitConverter.ToInt16(this.fileContent, entryOffset + 4) != tableIndex)
          break;
        pSlot.Value = tableIndex;
        cmd.ExecuteNonQuery();

        tableIndex = BitConverter.ToInt16(this.fileContent, entryOffset + 2);
        ++seq;
      }
    }
    #endregion

    #region InsertChannelData()
    private void InsertChannelData(DbCommand cmd, int listId)
    {
      PrepareChannelInsert(cmd);

      DvbStringDecoder decoder = new DvbStringDecoder(this.DefaultEncoding);
      DataMapping dvbsMapping = this.dvbsMappings.GetMapping(this.dvbsBlockSize);
      dvbsMapping.SetDataPtr(this.fileContent, this.dvbsBlockOffset + this.satConfig.ChannelListOffset);
      for (int slot = 0; slot < this.dvbsChannelCount; slot++)
      {
        cmd.Parameters["@listid"].Value = listId;
        cmd.Parameters["@slot"].Value = slot;
        cmd.Parameters["@seq"].Value = DBNull.Value;
        cmd.Parameters["@isdel"].Value = dvbsMapping.GetFlag("InUse", false) ? 0 : 1;
        cmd.Parameters["@progmask"].Value = dvbsMapping.GetWord("offProgramNr");
        cmd.Parameters["@prognr"].Value = dvbsMapping.GetWord("offProgramNr") & 0x3FFF;
        cmd.Parameters["@progfix"].Value = dvbsMapping.GetWord("offProgramNrPreset");
        int absNameOffset = dvbsMapping.BaseOffset + dvbsMapping.GetOffsets("offName")[0];
        string longName, shortName;
        decoder.GetChannelNames(fileContent, absNameOffset, dvbsMapping.GetByte("offNameLength"), out longName, out shortName);
        cmd.Parameters["@name"].Value = longName;
        cmd.Parameters["@tpnr"].Value = dvbsMapping.GetWord("offTransponderIndex");
        var transp = this.DataRoot.Transponder.TryGet(dvbsMapping.GetWord("offTransponderIndex"));
        cmd.Parameters["@satnr"].Value = transp == null ? (object)DBNull.Value : transp.Satellite.Id;
        cmd.Parameters["@onid"].Value = transp == null ? (object)DBNull.Value : transp.OriginalNetworkId;
        cmd.Parameters["@tsid"].Value = transp == null ? (object)DBNull.Value : transp.TransportStreamId;
        cmd.Parameters["@ssid"].Value = (int)dvbsMapping.GetWord("offServiceId");
        cmd.Parameters["@uid"].Value = transp == null
                                         ? (object) DBNull.Value
                                         : transp.TransportStreamId + "-" + transp.OriginalNetworkId + "-" +
                                           dvbsMapping.GetWord("offServiceId");
        cmd.Parameters["@favcrypt"].Value = (int)dvbsMapping.GetByte("offFavorites");
        cmd.Parameters["@lockskiphide"].Value = (int)dvbsMapping.GetByte("offLock");
        cmd.ExecuteNonQuery();
        dvbsMapping.BaseOffset += this.satConfig.dvbsChannelLength;
      }
    }
    #endregion

    #region PrepareChannelInsert()
    private static void PrepareChannelInsert(DbCommand cmd)
    {
      var cols = new[] { "listid", "slot", "seq", "isdel", "progmask", "prognr", "progfix", "name", "tpnr", "satnr", "onid", "tsid", "ssid", "uid", "favcrypt", "lockskiphide" };

      cmd.Parameters.Clear();

      var sb = new StringBuilder();
      sb.Append("insert into channel (");
      var comma = "";
      foreach (var col in cols)
      {
        sb.Append(comma).Append(col);
        comma = ",";
      }
      sb.Append(") values (");
      comma = "";
      foreach (var col in cols)
      {
        sb.Append(comma).Append('@').Append(col);
        comma = ",";
      }
      sb.Append(")");
      cmd.CommandText = sb.ToString();

      foreach (var col in cols)
      {
        DbParameter parm = cmd.CreateParameter();
        parm.ParameterName = "@" + col;
        if (col == "name" || col == "uid")
        {
          parm.DbType = DbType.String;
          parm.Size = 40;
        }
        else
          parm.DbType = DbType.Int32;
        cmd.Parameters.Add(parm);
      }

      cmd.Prepare();
    }
    #endregion

  }
}