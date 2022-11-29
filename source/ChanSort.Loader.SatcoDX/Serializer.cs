using System;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.SatcoDX
{
  internal class Serializer : SerializerBase
  {
    private readonly ChannelList allChannels = new ChannelList(0, "All");

    private byte[] content;
    private int trailingDataPos;

    #region ctor()

    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = false;
      this.Features.CanHideChannels = false;
      this.Features.FavoritesMode = FavoritesMode.None;

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
        list.VisibleColumnFieldNames.Remove("ServiceType");
        list.VisibleColumnFieldNames.Add("ServiceTypeName");
      }
    }

    #endregion

    #region Load()

    public override void Load()
    {
      var decoder = new DvbStringDecoder(this.DefaultEncoding);

      var pos = 0;
      content = File.ReadAllBytes(this.FileName);
      int prevPos = 0, nextPos;
      while (prevPos < content.Length && content[prevPos] != 0 && (nextPos = Array.FindIndex(content, prevPos, ch => ch == (byte)'\n')) >= 0)
      {
        if (nextPos - prevPos == 0)
          continue;
        string line = Encoding.ASCII.GetString(content, prevPos, nextPos-prevPos);
        ChannelInfo channel = new Channel(pos, line, content, prevPos, nextPos-prevPos, decoder);
        this.DataRoot.AddChannel(this.allChannels, channel);
        pos++;
        prevPos = nextPos + 1;
      }

      // SATCODX105 files contain a \0 character to mark the end, followed by an arbitrary number or spaces (or whatever data). We'll preserve it as-is.
      this.trailingDataPos = prevPos;
    }

    #endregion

    #region Save()

    public override void Save()
    {
      using var file = new FileStream(this.FileName, FileMode.Create);
      byte[] buffer = null;
      foreach (var channel in this.allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel.IsProxy || channel.IsDeleted)
          continue;
        if (channel is not Channel realChannel)
          continue;

        buffer ??= new byte[realChannel.Length + 1];
        realChannel.Export(buffer, this.DefaultEncoding);
        file.Write(buffer, 0, buffer.Length);
      }

      file.Write(this.content, this.trailingDataPos, this.content.Length - this.trailingDataPos);
    }

    #endregion


    #region DefaultEncoding

    /// SATCODX103 files can contain channel names with unspecified implicit encoding, so we support reparsing based on a user selected default code page

    public override Encoding DefaultEncoding
    {
      get => base.DefaultEncoding;
      set
      {
        if (ReferenceEquals(value, this.DefaultEncoding))
          return;
        base.DefaultEncoding = value;

        var decoder = new DvbStringDecoder(value);
        foreach (var chan in this.allChannels.Channels)
        {
          if (chan is Channel ch)
            ch.ParseName(decoder);
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