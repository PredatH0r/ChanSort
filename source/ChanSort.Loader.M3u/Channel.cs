using System.Collections.Generic;
using ChanSort.Api;

namespace ChanSort.Loader.M3u
{
   internal class Channel : ChannelInfo
   {
     public List<string> Lines { get; }

     public Channel(int index, int progNr, string name, List<string> lines) : base(SignalSource.IP, index, progNr, name)
     {
       this.Lines = lines;
     }
   }
}
