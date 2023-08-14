using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ChanSort.Api;

namespace ChanSort.Loader.Samsung.Zip
{
  internal class DbChannel : ChannelInfo
  {
    private dynamic jsonMeta;
    private Encoding encoding;
    public bool JsonModified { get; private set; }

    #region ctor()
    internal DbChannel(IDataReader r, IDictionary<string, int> field, DbSerializer loader, Dictionary<long, string> providers, Satellite sat, Transponder tp)
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
      this.Name = loader.ReadUtf16(r, field["srvName"]);
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

      if ((this.SignalSource & SignalSource.Dvb) != 0)
        this.ReadDvbData(r, field, loader, providers);
      else if ((this.SignalSource & SignalSource.Analog) != 0)
        this.ReadAnalogData(r, field);
      if (field.ContainsKey("jsonMeta"))
        this.ReadIpData(r, field, loader);
      base.IsDeleted = this.OldProgramNr == -1;
    }

    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(IDataReader r, IDictionary<string, int> field)
    {
      
    }
    #endregion

    #region ReadDvbData()
    protected void ReadDvbData(IDataReader r, IDictionary<string, int> field, DbSerializer loader, Dictionary<long, string> providers)
    {
      this.ShortName = loader.ReadUtf16(r, field["srvName"]);
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

    #region ReadIpData()

    private void ReadIpData(IDataReader r, IDictionary<string, int> field, DbSerializer loader)
    {
      var json = loader.ReadUtf16(r, field["jsonMeta"]);
      if (json != null)
      {
        var s = JsonSerializer.Create();
        dynamic obj = s.Deserialize(new JsonTextReader(new StringReader(json)));
        this.encoding = loader.DefaultEncoding;
        this.jsonMeta = obj;
        this.JsonDefaultUrl = obj?.default_url;
        this.JsonLogoUrl = obj?.logo_url;
      }
    }
    #endregion

    #region JsonDefaultUrl
    public string JsonDefaultUrl
    {
      get => jsonMeta?.default_url;
      set
      {
        if (jsonMeta == null || value == (string)jsonMeta.default_url.Value)
          return;
        jsonMeta.default_url = value;
        JsonModified = true;
      }
    }
    #endregion

    #region JsonLogoUrl
    public string JsonLogoUrl
    {
      get => jsonMeta?.logo_url;
      set
      {
        if (jsonMeta == null || value == (string)jsonMeta.logo_url.Value)
          return;
        jsonMeta.logo_url = value;
        JsonModified = true;
      }
    }
    #endregion

    #region GetRawJson()
    public byte[] GetRawJson()
    {
      var s = JsonSerializer.Create();
      using var w = new StringWriter();
      s.Serialize(new JsonTextWriter(w), this.jsonMeta);
      w.Flush();
      var rawJson = Encoding.BigEndianUnicode.GetBytes(w.ToString());
      return rawJson;
    }
    #endregion
  }
}
