using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.Sony
{
  internal class Channel : ChannelInfo
  {
    public readonly Dictionary<string,string> ServiceData = new Dictionary<string, string>();
    public readonly Dictionary<string,string> ProgrammeData = new Dictionary<string, string>();

    internal Channel(SignalSource source, int order, int recId)
    {
      this.SignalSource = source;
      this.RecordOrder = order;
      this.RecordIndex = recId;
    }
  }
}
