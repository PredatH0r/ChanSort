using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.Sony
{
  internal class Channel : ChannelInfo
  {
    internal int Index;
    internal XmlNode XmlNode;
    internal bool IsDisabled;

    #region ctor()
    internal Channel(SignalSource source, int index, XmlNode node)
    {
      this.SignalSource = source;
      this.Index = index;
      this.XmlNode = node;
    }
    #endregion

  }
}
