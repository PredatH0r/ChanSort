using System;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.SilvaSchneider
{
  internal class Serializer : SerializerBase
  {
    private readonly ChannelList allChannels = new ChannelList(SignalSource.DvbT | SignalSource.DvbC | SignalSource.DvbS | SignalSource.AnalogC | SignalSource.AnalogT | SignalSource.Tv | SignalSource.Radio, "All");

    private byte[] content;

    #region ctor()

    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.DataRoot.SortedFavorites = false;
      this.DataRoot.SupportedFavorites = 0;

      this.DataRoot.AddChannelList(this.allChannels);

      // hide columns for fields that don't exist in Silva-Schneider channel list
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("PcrPid");
        list.VisibleColumnFieldNames.Remove("VideoPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        list.VisibleColumnFieldNames.Remove("Lock");
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("Hidden");
        list.VisibleColumnFieldNames.Remove("Encrypted");
        list.VisibleColumnFieldNames.Remove("Favorites");
      }
    }

    #endregion

    public override string DisplayName => "Silva Schneider *.sdx Loader";

    #region Load()

    public override void Load()
    {
      var decoder = new DvbStringDecoder(this.DefaultEncoding);

      var pos = 0;
      content = File.ReadAllBytes(this.FileName);
      int prevPos = 0, nextPos;
      while (prevPos < content.Length && (nextPos = Array.FindIndex(content, prevPos, ch => ch == (byte)'\n')) >= 0)
      {
        if (nextPos - prevPos == 0)
          continue;
        string line = Encoding.ASCII.GetString(content, prevPos, nextPos-prevPos);
        ChannelInfo channel = new Channels(pos, line, content, prevPos, nextPos-prevPos, decoder);
        this.DataRoot.AddChannel(this.allChannels, channel);
        pos++;
        prevPos = nextPos + 1;
      }
    }

    #endregion

    #region Save()

    public override void Save(string tvOutputFile)
    {
      if (tvOutputFile != this.FileName)
      {
        File.Copy(this.FileName, tvOutputFile);
        this.FileName = tvOutputFile;
      }

      using (var file = new FileStream(tvOutputFile, FileMode.Create))
      {
        foreach (var channel in this.allChannels.GetChannelsByNewOrder())
        {
          // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
          if (channel is Channels realChannel && channel.NewProgramNr >= 0)
            file.Write(this.content, realChannel.FileOffset, realChannel.Length + 1);
        }
      }
    }

    #endregion

    #region GetFileInformation()

    public override string GetFileInformation()
    {
      var sb = new StringBuilder();
      sb.Append(base.GetFileInformation());
      return sb.ToString();
    }

    #endregion
  }
}