using System.Collections.Generic;
using System.Data.SQLite;
using ChanSort.Api;

namespace ChanSort.Loader.Toshiba
{
  internal class DbChannel : ChannelInfo
  {
    internal Dictionary<int,int> OriginalFavIndex = new Dictionary<int, int>();

    #region ctor()
    internal DbChannel(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, Dictionary<long, string> providers)
    {
      var chType = r.GetInt32(field["chType"]);
      if (chType == 7)       
        this.SignalSource = SignalSource.DvbS;
      else if (chType == 4)
        this.SignalSource = SignalSource.DvbC;

      this.RecordIndex = r.GetInt64(field["SRV.srvId"]);
      this.OldProgramNr = r.GetInt32(field["major"]);
      this.FreqInMhz = (decimal)r.GetInt32(field["freq"]) / 1000;
      this.ChannelOrTransponder = 
        (this.SignalSource & SignalSource.Cable) != 0 ? LookupData.Instance.GetDvbtTransponder(this.FreqInMhz).ToString() :
        (this.SignalSource & SignalSource.Sat) != 0 ? LookupData.Instance.GetTransponderNumber((int)this.FreqInMhz).ToString() :
        "";
      this.Name = DbSerializer.ReadUtf16(r, 2);
      this.Hidden = r.GetBoolean(field["hidden"]);
      this.Encrypted = r.GetBoolean(field["scrambled"]);
      this.Lock = r.GetBoolean(field["lockMode"]);
      this.Skip = !r.GetBoolean(field["numSel"]);
      
      //if (isAnalog)
      //  this.ReadAnalogData(r, field);
      //else
        this.ReadDvbData(r, field, dataRoot, providers);
    }
    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(SQLiteDataReader r, IDictionary<string, int> field)
    {
      this.Name = r.GetString(field["channel_label"]);
      this.FreqInMhz = (decimal)r.GetInt32(field["frequency"]) / 1000000;
    }
    #endregion

    #region ReadDvbData()
    protected void ReadDvbData(SQLiteDataReader r, IDictionary<string, int> field, DataRoot dataRoot, Dictionary<long, string> providers)
    {
      this.ShortName = DbSerializer.ReadUtf16(r, 3);
      this.RecordOrder = r.GetInt32(field["major"]);
      int serviceType = r.GetInt32(field["srvType"]);
      this.ServiceType = serviceType;
      this.SignalSource |= LookupData.Instance.IsRadioOrTv(serviceType);
      this.OriginalNetworkId = r.GetInt32(field["onid"]);
      this.TransportStreamId = r.GetInt32(field["tsid"]);
      this.ServiceId = r.GetInt32(field["progNum"]);
      this.VideoPid = r.GetInt32(field["vidPid"]);
      if (!r.IsDBNull(field["provId"]))
        this.Provider = providers.TryGet(r.GetInt64(field["provId"]));
      if ((this.SignalSource & SignalSource.Sat) != 0)
      {
        //int satId = r.GetInt32(field["sat_id"]);
        //var sat = dataRoot.Satellites.TryGet(satId);
        //if (sat != null)
        //{
        //  this.Satellite = sat.Name;
        //  this.SatPosition = sat.OrbitalPosition;
        //  int tpId = satId * 1000000 + (int)this.FreqInMhz;
        //  var tp = dataRoot.Transponder.TryGet(tpId);
        //  if (tp != null)
        //  {
        //    this.SymbolRate = tp.SymbolRate;
        //  }
        //}
      }
      //this.Encrypted = encryptionInfo.TryGet(this.Uid);      
    }
    #endregion


    #region UpdateRawData()
    public override void UpdateRawData()
    {
    }
    #endregion
  }
}
