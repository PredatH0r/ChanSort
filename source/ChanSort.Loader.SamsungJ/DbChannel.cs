using System.Collections.Generic;
using System.Data.SQLite;
using ChanSort.Api;

namespace ChanSort.Loader.SamsungJ
{
  internal class DbChannel : ChannelInfo
  {
    #region ctor()
    internal DbChannel(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, Dictionary<long, string> providers, Satellite sat, Transponder tp)
    {
      var chType = r.GetInt32(field["chType"]);
      this.SignalSource = DbSerializer.ChTypeToSignalSource(chType);

      this.RecordIndex = r.GetInt64(field["SRV.srvId"]);
      this.OldProgramNr = r.GetInt32(field["major"]);
      this.FreqInMhz = (decimal)r.GetInt32(field["freq"]) / 1000;
      this.ChannelOrTransponder = 
        (this.SignalSource & SignalSource.DvbT) == SignalSource.DvbT ? LookupData.Instance.GetDvbtTransponder(this.FreqInMhz).ToString() :
        (this.SignalSource & SignalSource.DvbC) == SignalSource.DvbC ? LookupData.Instance.GetDvbcTransponder(this.FreqInMhz).ToString() :
        (this.SignalSource & SignalSource.DvbS) == SignalSource.DvbS ? LookupData.Instance.GetAstraTransponder((int)this.FreqInMhz).ToString() :
        "";
      this.Name = DbSerializer.ReadUtf16(r, field["srvName"]);
      this.Hidden = r.GetBoolean(field["hidden"]);
      this.Encrypted = r.GetBoolean(field["scrambled"]);
      this.Lock = r.GetBoolean(field["lockMode"]);
      this.Skip = !r.GetBoolean(field["numSel"]);

      if (sat != null)
      {
        this.Satellite = sat.Name;
        this.SatPosition = sat.OrbitalPosition;
      }
      if (tp != null)
      {
        this.Transponder = tp;
        this.SymbolRate = tp.SymbolRate;
        this.Polarity = tp.Polarity;
      }

      if ((this.SignalSource & SignalSource.Digital) != 0)
        this.ReadDvbData(r, field, dataRoot, providers);
      else
        this.ReadAnalogData(r, field);

      base.IsDeleted = this.OldProgramNr == -1;
    }

    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(SQLiteDataReader r, IDictionary<string, int> field)
    {
      
    }
    #endregion

    #region ReadDvbData()
    protected void ReadDvbData(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, Dictionary<long, string> providers)
    {
      this.ShortName = DbSerializer.ReadUtf16(r, field["srvName"]);
      this.RecordOrder = r.GetInt32(field["major"]);
      int serviceType = r.GetInt32(field["srvType"]);
      this.ServiceType = serviceType;
      this.SignalSource |= LookupData.Instance.IsRadioTvOrData(serviceType);
      this.OriginalNetworkId = r.GetInt32(field["onid"]);
      this.TransportStreamId = r.GetInt32(field["tsid"]);
      this.ServiceId = r.GetInt32(field["progNum"]);
      this.VideoPid = r.GetInt32(field["vidPid"]);
      if (!r.IsDBNull(field["provId"]))
        this.Provider = providers.TryGet(r.GetInt64(field["provId"]));
      this.AddDebug(r.GetInt32(field["lcn"]).ToString());
    }
    #endregion
  }
}
