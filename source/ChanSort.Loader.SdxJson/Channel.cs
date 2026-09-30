using ChanSort.Api;

namespace ChanSort.Loader.SdxJson
{
  internal class Channel : ChannelInfo
  {
    /// <summary>
    /// original JSON text of the "program_tv_object_N" / "program_radio_object_N" object
    /// </summary>
    public string Json { get; }

    /// <summary>
    /// name as stored in the file, used to leave the JSON text untouched when the name was not edited
    /// </summary>
    public string OriginalName { get; }

    public Channel(SignalSource source, int index, int oldProgNr, string name, string json) : base(source, index, oldProgNr, name)
    {
      this.Json = json;
      this.OriginalName = name;
    }
  }
}
