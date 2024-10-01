using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.MediaTek
{
  internal class Channel : ChannelInfo
  {
    public XmlElement Xml { get; }

    internal Channel(SignalSource source, int index, int oldProgNr, string name, XmlElement element) :base(source, index, oldProgNr, name)
    {
      this.Xml = element;
    }
  }
}
