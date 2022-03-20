using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.Loewe
{
  internal class Channel : ChannelInfo
  {
    public XmlElement XmlElement { get; set; }
    public int PhysicalListId { get; set; }

    public Channel(int id) : base(0, id, -1, "")
    {
    }
  }
}
