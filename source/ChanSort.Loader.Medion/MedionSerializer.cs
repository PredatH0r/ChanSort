using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;
using Newtonsoft.Json;

namespace ChanSort.Loader.Medion;

/// <summary>
/// This class loads Medion Android channel lists containing JSON data lines like
/// {"svlId":4,"svlRecId":1,"channelNumber":1,"channelId":262273,"serviceName":"Das Erste HD","brdcstType":2,"nwMask":1057311,"optionMask":32,"serviceType":1,"brdcstMedium":3,"progId":10301,"sd":0,"scrambled":1059359}
///
/// The nwMask 0x0010 bit indicates if the channel is a favorite, but the file does not contain the independent position of the channel within the channel list.
/// It maintains its favorites even after importing a new data file. Therefore it's not possible to edit favorites for this type of TV.
///
/// nwMask 0x0800 is the correct indicator for encrypted channels. The value of "scrambled" always has that bit set and is otherwise the same as nwMask, hence useless.
///
/// optionMask 0x0020 seems to indicate LCN
/// </summary>
public class MedionSerializer : SerializerBase
{
  private readonly Dictionary<int, ChannelList> serviceLists = new();

  #region ctor()
  public MedionSerializer(string inputFile) : base(inputFile)
  {
    this.Features.CanSaveAs = true;
    this.Features.CanSkipChannels = false;
    this.Features.CanHideChannels = false;
    this.Features.CanLockChannels = false;
    this.Features.FavoritesMode = FavoritesMode.None;
    this.Features.DeleteMode = DeleteMode.Physically;
  }
  #endregion

  #region Load()
  public override void Load()
  {
    var lines = File.ReadAllLines(this.FileName);
    foreach (var line in lines)
    {
      var data = JsonConvert.DeserializeObject<dynamic>(line);
      
      var channel = ReadChannel(data);
      
      if (channel != null)
      {
        var list = this.DataRoot.GetChannelList(channel.SignalSource);
        if (list != null)
          list.AddChannel(channel);
      }
    }

    AdjustVisibleColumns();
  }
  #endregion

  #region AdjustVisibleColumns()
  private void AdjustVisibleColumns()
  {
    foreach (var list in this.serviceLists.Values)
    {
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ShortName));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Lock));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Skip));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Hidden));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.FreqInMhz));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Satellite));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.SymbolRate));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.OriginalNetworkId));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.TransportStreamId));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.PcrPid));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.VideoPid));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.NetworkName));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.NetworkOperator));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
      list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ChannelOrTransponder));
    }
  }
  #endregion

  #region ParseChannel()
  private ChannelInfo ReadChannel(dynamic data)
  {
    var ch = new ChannelInfo<dynamic>();
    ch.ExtraData = data;
    ch.RecordOrder = (int)data.svlRecId;
    ch.RecordIndex = (int)data.channelId;
    ch.OldProgramNr = (int)data.channelNumber;
    ch.Name = Convert.ToString(data.serviceName); // (string)data.serviceName may throw a RuntimeBinderException if serviceName is null

    var svlId = (int)data.svlId;

    // brdcastType - this here is just a guess, all observed channels had value 2
    var bcType = (int)data.brdcstType;
    switch (bcType)
    {
      case 1:
        ch.SignalSource |= SignalSource.Analog;
        break;
      case 2:
        ch.SignalSource |= SignalSource.Digital;
        break;
    }

    // brdcstMedium - all digital satellite channels (TV and radio) have value 3. Rest is a guess
    var medium = (int)data.brdcstMedium;
    string name;
    switch (medium)
    {
      case 1:
        ch.SignalSource |= SignalSource.Analog;
        name = "Antenna";
        break;
      case 2:
        ch.SignalSource |= SignalSource.Cable;
        name = "Cable";
        break;
      case 3:
        ch.SignalSource |= SignalSource.Sat;
        name = "Satellite";
        break;
      default:
        name = $"Input {medium}";
        break;
    }

    if (!this.serviceLists.ContainsKey(svlId))
    {
      var list = new ChannelList(ch.SignalSource | SignalSource.MaskTvRadioData, $"{svlId} - {name}");
      this.serviceLists.Add(svlId, list);
      this.DataRoot.AddChannelList(list);
    }

    // nwMask - 0x0010 indicates a favorite, but the data lacks the channel's favorite indexes for list 1-4
    uint nwMask = (uint)data.nwMask;
    if ((nwMask & 0x0010) != 0)
      ch.AddDebug("FAV");
    ch.Encrypted = (nwMask & 0x0800) != 0;

    // optionMask - 0x0020 seems to indicates LCN assigned channel number

    int svcType = (int)data.serviceType;
    switch (svcType)
    {
      case 1:
        ch.SignalSource |= SignalSource.Tv;
        break;
      case 2:
        ch.SignalSource |= SignalSource.Radio;
        break;
      default:
        ch.SignalSource |= SignalSource.Data;
        break;
    }

    ch.ServiceId = (int)data.progId;

    ch.ServiceTypeName = svcType == 1 ? ((int)data.sd == 0 ? "HD TV" : "SD TV") : svcType == 2 ? "Radio" : "Data";

    // scrambled is useless. It contains the same as nwMask, but with bit 0x0800 always set, while in nwMask it is only set for encrypted channels

    return ch;
  }
  #endregion

  #region Save()
  public override void Save()
  {
    var sb = new StringBuilder(500000);
    foreach (var list in this.DataRoot.ChannelLists)
    {
      foreach (var channel in list.Channels.OrderBy(ch => ch.NewProgramNr))
      {
        if (channel is not ChannelInfo<dynamic> ch)
          continue;

        if (ch.NewProgramNr < 0)
          continue;

        var data = ch.ExtraData;
        data.channelNumber = ch.NewProgramNr;
        var line = JsonConvert.SerializeObject(data, Formatting.None);
        sb.AppendLine(line);
      }
    }

    var fn = this.SaveAsFileName ?? this.FileName;
    File.WriteAllText(fn, sb.ToString());
    this.FileName = fn;
  }
  #endregion
}