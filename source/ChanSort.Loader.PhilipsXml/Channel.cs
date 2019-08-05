using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.PhilipsXml
{
  internal class Channel : ChannelInfo
  {
    public readonly XmlNode SetupNode;
    public string RawName;
    public string RawSatellite;

    internal Channel(SignalSource source, int order, int rowId, XmlNode setupNode)
    {
      this.SignalSource = source;
      this.RecordOrder = order;
      this.RecordIndex = rowId;
      this.SetupNode = setupNode;
    }
  }
}
