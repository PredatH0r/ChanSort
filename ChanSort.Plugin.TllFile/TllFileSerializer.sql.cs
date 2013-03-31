using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public partial class TllFileSerializer
  {
    #region SQL

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

    private unsafe void StoreToDatabase()
    {
      return;
      var list = this.DataRoot.GetChannelList(SignalSource.DvbS, SignalType.Tv, false);
      if (list == null)
        return;

      using (var conn = SqlClientFactory.Instance.CreateConnection())
      {
        conn.ConnectionString = "server=(local);database=ChanSort;Integrated Security=true";
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
          var listId = InsertListData(cmd);

          fixed (byte* ptr = this.fileContent)
          {
            InsertSequenceData(ptr, cmd, listId);
            InsertChannelData(ptr, cmd, listId);
          }
        }
      }
    }

    private unsafe void InsertSequenceData(byte* ptr, DbCommand cmd, int listId)
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

      SatChannelListHeader* header = (SatChannelListHeader*) (ptr + this.dvbsChannelHeaderOffset);

      int seq = 0;
      ushort tableIndex = header->LinkedListStartIndex;
      while(tableIndex != 0xFFFF)
      {
        ushort* entry = (ushort*)(ptr + this.dvbsChannelLinkedListOffset + tableIndex*c.sizeOfZappingTableEntry);
        pSeq.Value = seq;
        if (entry[2] != tableIndex)
          break;
        pSlot.Value = (int)tableIndex;
        cmd.ExecuteNonQuery();

        tableIndex = entry[1];
        ++seq;
      }
    }

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

    #region InsertChannelData()
    private unsafe void InsertChannelData(byte* ptr, DbCommand cmd, int listId)
    {
      PrepareChannelInsert(cmd);

      dvbsMapping.DataPtr = ptr + this.dvbsChannelListOffset;
      for (int slot = 0; slot < this.dvbsChannelCount; slot++)
      {
        cmd.Parameters["@listid"].Value = listId;
        cmd.Parameters["@slot"].Value = slot;
        cmd.Parameters["@seq"].Value = DBNull.Value;
        cmd.Parameters["@isdel"].Value = dvbsMapping.InUse ? 0 : 1;
        cmd.Parameters["@progmask"].Value = dvbsMapping.ProgramNr;
        cmd.Parameters["@prognr"].Value = (dvbsMapping.ProgramNr & 0x3FFF);
        cmd.Parameters["@progfix"].Value = dvbsMapping.ProgramNrPreset;
        cmd.Parameters["@name"].Value = dvbsMapping.Name;
        cmd.Parameters["@tpnr"].Value = dvbsMapping.TransponderIndex;
        var transp = this.DataRoot.Transponder.TryGet(dvbsMapping.TransponderIndex);
        cmd.Parameters["@satnr"].Value = transp == null ? (object)DBNull.Value : transp.Satellite.Id;
        cmd.Parameters["@onid"].Value = transp == null ? (object)DBNull.Value : transp.OriginalNetworkId;
        cmd.Parameters["@tsid"].Value = transp == null ? (object)DBNull.Value : transp.TransportStreamId;
        cmd.Parameters["@ssid"].Value = (int)dvbsMapping.ServiceId;
        cmd.Parameters["@uid"].Value = transp == null
                                         ? (object) DBNull.Value
                                         : transp.TransportStreamId + "-" + transp.OriginalNetworkId + "-" +
                                           dvbsMapping.ServiceId;
        cmd.Parameters["@favcrypt"].Value = (int)dvbsMapping.GetByte("offFavorites");
        cmd.Parameters["@lockskiphide"].Value = (int)dvbsMapping.GetByte("offLock");
        cmd.ExecuteNonQuery();
        dvbsMapping.Next();
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