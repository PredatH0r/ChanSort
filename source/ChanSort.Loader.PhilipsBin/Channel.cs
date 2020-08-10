using ChanSort.Api;

namespace ChanSort.Loader.PhilipsBin
{
  class Channel : ChannelInfo
  {
    public Channel(SignalSource source, long index, int oldProgNr, string name) : base(source, index, oldProgNr, name)
    {
    }

    /// <summary>
    /// index of the record in the AntennaPresetTable / CablePresetTable file for the channel, matched by (onid + tsid + sid)
    /// </summary>
    public int PresetTableIndex { get; set; } = -1;
  }
}
