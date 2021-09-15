using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
{
  internal class Channel : ChannelInfo
  {
    public Channel(SignalSource source, long index, int oldProgNr, string name) : base(source, index, oldProgNr, name)
    {
      this.RecordOrder = (int)index;
    }

    internal Channel(SignalSource source, int order, int rowId, XmlNode setupNode)
    {
      this.SignalSource = source;
      this.RecordOrder = order;
      this.RecordIndex = rowId;
      this.SetupNode = setupNode;
    }

    /// <summary>
    /// index of the record in the AntennaPresetTable / CablePresetTable file for the channel, matched by (onid + tsid + sid)
    /// </summary>
    public int PresetTableIndex { get; set; } = -1;

    public int Map30ChannelMapsDbindex { get; set; } = -1;

    // fields relevant for ChannelMap_100 and later (XML nodes)
    public readonly XmlNode SetupNode;
    public string RawName;
    public string RawSatellite;
    public int Format;

    /// <summary>
    /// _id in tv.db:channels; referenced by ChannelMap30 list.db:xListN.channel_id and ChannelMap45 *DB.bin records
    /// </summary>
    public int Id; // links entries in the ChannelMap45/*Db.bin files with the entries in the tv.db channels table

    /// <summary>
    /// used in the mgr_db / FLASH format to hold the absolute offset of the channel in the FLASH file
    /// </summary>
    public int FlashFileOffset;

    public int DbFileOffset;
  }
}
