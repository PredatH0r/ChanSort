using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  internal class GcChannel : ChannelInfo
  {
    internal int Index;
    internal XmlNode XmlNode;
    internal bool IsDisabled;

    #region ctor()
    internal GcChannel(SignalSource source, int index, XmlNode node)
    {
      this.SignalSource = source;
      this.Index = index;
      this.XmlNode = node;
    }
    #endregion

  }
}
