using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.DvbViewer
{
  internal class Channel : ChannelInfo
  {
    /// <summary>
    /// original "key=value" lines of the [ChannelN] section
    /// </summary>
    public List<string> Lines { get; }

    /// <summary>
    /// DVBViewer stores each additional audio track of a service as a separate [ChannelN] entry right after the main entry.
    /// These entries are kept together with the main channel so that they are moved and deleted along with it.
    /// </summary>
    public List<Channel> AudioTracks { get; } = new List<Channel>();

    public int Flags { get; set; }
    public int Frequency { get; set; }
    public int TunerType { get; set; }

    public Channel(SignalSource source, int index, int oldProgNr, string name, List<string> lines) : base(source, index, oldProgNr, name)
    {
      this.Lines = lines;
    }
  }
}
