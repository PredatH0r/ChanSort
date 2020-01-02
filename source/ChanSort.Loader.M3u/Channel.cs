using ChanSort.Api;

namespace ChanSort.Loader.M3u
{
   internal class Channel : ChannelInfo
   {
     public string Uri { get; }

     public Channel(int index, int progNr, string name, string uri) : base(SignalSource.IP, index, progNr, name)
     {
       this.Uri = uri;
     }
   }
}
