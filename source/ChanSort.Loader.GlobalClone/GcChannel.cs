using System.Xml;
using ChanSort.Api;

namespace ChanSort.Loader.GlobalClone
{
  internal class GcChannel<TNode> : ChannelInfo
  {
    internal int Index;
    internal TNode Node;
    internal bool IsDisabled;

    #region ctor()
    internal GcChannel(SignalSource source, int index, TNode node)
    {
      this.SignalSource = source;
      this.Index = index;
      this.Node = node;
    }
    #endregion

  }
}
