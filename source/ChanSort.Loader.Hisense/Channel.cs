using ChanSort.Api;

namespace ChanSort.Loader.Hisense
{
  internal class Channel : ChannelInfo
  {
    public int ChannelId;
    public int NwMask;

    public Channel(SignalSource ssource, long id, int prNr, string name): base(ssource, id, prNr, name)
    {
    }
  }
}
