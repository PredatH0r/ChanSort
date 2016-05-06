using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChanSort.Api
{
  public class TxtRefListSerializer : SerializerBase
  {
    private static readonly char[] Separators = { ';' };

    private readonly ChannelList allChannels = new ChannelList(SignalSource.MaskAntennaCableSat | SignalSource.MaskAnalogDigital | SignalSource.MaskTvRadio, "All");

    #region ctor()

    public TxtRefListSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.CanSkipChannels = false;
      this.Features.CanDeleteChannels = true;
      this.Features.CanHaveGaps = true;
      this.Features.EncryptedFlagEdit = false;
      this.DataRoot.SortedFavorites = false;
      this.DataRoot.SupportedFavorites = 0;
      this.DataRoot.AddChannelList(this.allChannels);

      allChannels.VisibleColumnFieldNames = new List<string>
      {
        "OldPosition",
        "Position",
        "Name",
        "OriginalNetworkId",
        "TransportStreamId",
        "ServiceId"
      };
    }

    #endregion

    public override string DisplayName => ".txt Reference List Loader";

    #region Load()

    public override void Load()
    {
      this.ReadChannels();
    }

    #endregion

    #region ReadChannels()

    private void ReadChannels()
    {
      var lineNr = 0;

      using (var file = new StreamReader(this.FileName))
      {
        string line;
        while ((line = file.ReadLine()) != null)
        {
          ++lineNr;
          var parts = line.Split(Separators);
          if (parts.Length < 2)
            continue;
          int progNr;
          if (!int.TryParse(parts[0], out progNr))
            continue;

          var channel = new ChannelInfo(allChannels.SignalSource, lineNr, progNr, parts[1]);
          if (parts.Length >= 3)
          {
            var subParts = parts[2].Split('-');
            if (subParts.Length >= 3)
            {
              int val;
              if (int.TryParse(subParts[0], out val))
                channel.OriginalNetworkId = val;
              if (int.TryParse(subParts[1], out val))
                channel.TransportStreamId = val;
              if (int.TryParse(subParts[2], out val))
                channel.ServiceId = val;
            }
          }
          this.DataRoot.AddChannel(this.allChannels, channel);
          lineNr++;
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

    #region Save()
   
    public override void Save(string tvOutputFile)
    {
      Save(tvOutputFile, this.allChannels);
      this.FileName = tvOutputFile;
    }

    public static void Save(string fileName, ChannelList list)
    {
      var samToolBoxMode = (Path.GetExtension(fileName) ?? "").ToLower() == ".chl";

      using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
      {
        foreach (var channel in list.GetChannelsByNewOrder())
        {
          if (channel.NewProgramNr == -1) continue;

          writer.Write(channel.NewProgramNr);
          writer.Write(Separators[0]);
          writer.Write(channel.Name);
          if (!samToolBoxMode)
          {
            writer.Write(Separators[0]);
            writer.Write(channel.OriginalNetworkId);
            writer.Write("-");
            writer.Write(channel.TransportStreamId);
            writer.Write("-");
            writer.Write(channel.ServiceId);
          }
          writer.WriteLine();
        }
      }
    }
    #endregion
  }
}