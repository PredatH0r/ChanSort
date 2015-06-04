using System.IO;
using System.Linq;
using System.Text;

namespace ChanSort.Api
{
  /// <summary>
  /// Reader for SamToolBox reference lists (*.chl)
  /// The file has no header, each line represents a channel and fields are separated by semi-colon:
  /// Number;Channel Name[;Transponder Index]
  /// </summary>
  public class ChlFileSerializer
  {
    private static readonly char[] Separators = new[] { ';' };
    private readonly StringBuilder warnings = new StringBuilder();
    private int lineNumber;
    private DataRoot dataRoot;
    private ChannelList channelList;

    #region Load()
    public string Load(string fileName, DataRoot root, ChannelList list)
    {
      if (list.ReadOnly)
        return "The current channel list is read-only";

      this.lineNumber = 0;
      this.dataRoot = root;
      this.channelList = list;
      this.warnings.Remove(0, this.warnings.Length);

      foreach (var channel in this.channelList.Channels)
        channel.NewProgramNr = -1;

      using (var stream = new StreamReader(fileName, Encoding.Default))
      {
        ReadChannelsFromStream(stream);
      }
      return this.warnings.ToString();
    }
    #endregion

    #region ReadChannelsFromStream()

    private void ReadChannelsFromStream(TextReader stream)
    {
      string line;
      while ((line = stream.ReadLine()) != null)
      {
        ++lineNumber;
        ParseChannel(line);
      }
    }

    #endregion

    #region ParseChannel()

    private void ParseChannel(string line)
    {
      var parts = line.Split(Separators);
      if (parts.Length < 2) return;
      int progNr;
      Transponder transponder = null;
      if (!int.TryParse(parts[0], out progNr)) return;
      if (parts.Length >= 3)
      {
        int transponderIndex;
        if (int.TryParse(parts[2], out transponderIndex))
        {
          transponder = this.dataRoot.Transponder.TryGet(transponderIndex);
          if (transponder == null)
            warnings.AppendFormat("Line #{0,4}: invalid transponder index {1}\r\n", this.lineNumber, transponderIndex);
        }
      }

      string name = parts[1].Replace("\"", "");
      if (name.Trim().Length == 0)
        return;

      int found = 0;
      var channels = channelList.GetChannelByName(name);
      if (transponder != null)
        channels = channels.Where(chan => chan.Transponder == transponder);

      foreach(var channel in channels)
      {
        if (channel.NewProgramNr != -1)
          continue;
        ++found;
        if (found > 1)
          break;
        channel.NewProgramNr = progNr;
      }

      if (found == 0)
        this.warnings.AppendFormat("Line {0,4}: Pr# {1,4}, channel '{2}' could not be found\r\n", this.lineNumber, progNr, name);
      if (found > 1)
        this.warnings.AppendFormat("Line {0,4}: Pr# {1,4}, channel '{2}' found multiple times\r\n", this.lineNumber, progNr, name);
    }
    #endregion

    #region Save()
    public void Save(string fileName, ChannelList list)
    {
      using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
      {
        foreach (var channel in list.Channels.OrderBy(c => c.NewProgramNr))
        {
          if (channel.NewProgramNr == -1) continue;
          
          writer.Write(channel.NewProgramNr);
          writer.Write(';');
          writer.Write(channel.Name);
          writer.WriteLine();
        }
      }
    }
    #endregion
  }
}
