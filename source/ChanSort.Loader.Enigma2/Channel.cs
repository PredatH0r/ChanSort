using ChanSort.Api;

namespace ChanSort.Loader.Enigma2
{
  internal class Channel : ChannelInfo
  {
    /// <summary>
    /// first two fields of the lamedb entry
    /// </summary>
    public string Prefix { get; set; } = "1:0";

    /// <summary>
    /// For DVB-S it is the orbital position * 10 (e.g. 192 for Astra 19.2E) * 65536
    /// </summary>
    public int DvbNamespace { get; set; }
    
    public int ServiceNumber { get; set; }

    /// <summary>
    /// all fields after the DVB-namespace in the lamedb entry
    /// </summary>
    public string Suffix { get; set; } = ":0:0:0:";
    
    /// <summary>
    /// #DESCRIPTION of the userbouquet entry
    /// </summary>
    public string Description { get; set; }
  }
}