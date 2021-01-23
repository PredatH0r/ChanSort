using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.Grundig
{
  internal class Channel : ChannelInfo
  {
    public readonly XmlNode Node;

    internal Channel(SignalSource source, int order, int rowId, XmlNode node)
    {
      this.SignalSource = source;
      this.RecordOrder = order;
      this.RecordIndex = rowId;
      this.Node = node;
    }
  }
}
