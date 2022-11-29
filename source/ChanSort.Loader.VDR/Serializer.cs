using System.IO;
using ChanSort.Api;

namespace ChanSort.Loader.VDR
{
  internal class Serializer : SerializerBase
  {
    private readonly ChannelList allChannels = new ChannelList(0, "All");

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      Features.ChannelNameEdit = ChannelNameEditMode.None;
      Features.DeleteMode = DeleteMode.Physically;
      Features.FavoritesMode = FavoritesMode.None;

      DataRoot.AddChannelList(allChannels);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      ReadChannels();
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels()
    {
      var pos = 0;

      using var file = new StreamReader(FileName);
      string line;
      while ((line = file.ReadLine()) != null)
      {
        ChannelInfo channel = new Channels(pos, line, DataRoot);
        DataRoot.AddChannel(allChannels, channel);
        pos++;
      }
    }
    #endregion

    #region Save()
    public override void Save()
    {
      using var file = new StreamWriter(this.FileName);
      foreach (var channel in allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel is Channels vdrChannel && !channel.IsDeleted)
          file.WriteLine(vdrChannel.confLine);
      }
    }
    #endregion
  }
}