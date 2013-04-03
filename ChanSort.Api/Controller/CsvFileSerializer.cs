using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChanSort.Api
{
  public class CsvFileSerializer
  {
    private static readonly char[] Separators = new[] { ',', ';', '\t' };

    #region Load()
    public void Load(string fileName, DataRoot dataRoot)
    {
      foreach (var list in dataRoot.ChannelLists)
      {
        foreach (var channel in list.Channels)
          channel.NewProgramNr = 0;
      }

      using (var stream = new StreamReader(fileName))
      {
        ReadChannelsFromStream(stream, dataRoot);
      }
    }
    #endregion

    #region ReadChannelsFromStream()

    public static void ReadChannelsFromStream(TextReader stream, DataRoot dataRoot)
    {
      string line;
      while ((line = stream.ReadLine()) != null)
      {
        ParseChannel(line, dataRoot);
      }
    }

    #endregion

    #region ParseChannel()

    private static void ParseChannel(string line, DataRoot dataRoot)
    {
      var parts = line.Split(Separators);
      if (parts.Length < 5) return;
      int index, slot, transportStreamId;
      if (!int.TryParse(parts[0], out index)) return;
      if (!int.TryParse(parts[1], out slot)) return;
      if (!int.TryParse(parts[2], out transportStreamId)) return;
      string uid = parts[3].Replace("\"", "");
      SignalSource signalSource;
      SignalType signalType;
      if (!GetSignalSourceAndType(ref slot, uid, parts, out signalSource, out signalType)) 
        return;

      string name = parts[4].Replace("\"", "");
      ChannelList channelList = dataRoot.GetChannelList(signalSource, signalType, true);
      

      IEnumerable<ChannelInfo> channels = FindChannels(channelList, name, uid);
      var channel = channels == null ? null : channels.FirstOrDefault(c => c.NewProgramNr == 0);
      if (channel != null)
        channel.NewProgramNr = slot;
      else
      {
        channel = new ChannelInfo(signalSource, signalType, uid, slot, name);
        channel.TransportStreamId = transportStreamId;
        channelList.AddChannel(channel);
      }
    }

    private static bool GetSignalSourceAndType(ref int slot, string uid, string[] parts, out SignalSource signalSource, out SignalType signalType)
    {
      // new lists store a bitmask which defines the type of channel and list it came from
      if (parts.Length >= 6)
      {
        signalSource = (SignalSource)int.Parse(parts[5]);
        signalType = (SignalType)((int)signalSource & (int)SignalType.Mixed);
        return true;
      }

      // compatibility for older lists
      signalSource = 0;
      signalType = 0;
      switch (uid[0])
      {
        case 'S': signalSource = SignalSource.DvbS; break;
        case 'C': signalSource = SignalSource.DvbCT; break;
        case 'A': signalSource = SignalSource.AnalogCT; break;
        case 'H': signalSource = SignalSource.HdPlusD; break;
        default: return false;
      }
      signalType = slot < 0x4000 ? SignalType.Tv : SignalType.Radio;
      signalSource |= (SignalSource)signalType;
      slot &= 0x3FFFF;
      return true;
    }

    #endregion

    #region FindChannels()
    private static IEnumerable<ChannelInfo> FindChannels(ChannelList channelList, string name, string uid)
    {
      IList<ChannelInfo> list = channelList.GetChannelByName(name).ToList();
      if (list.Count == 1)
        return list;

      string[] uidParts;
      if (uid.StartsWith("C") && (uidParts = uid.Split('-')).Length <= 4)
      {
        // older CSV files didn't use the Transponder as part of the UID, which is necessary
        // to distinguish between identical channels (onid,tsid,sid) received on multiple transponders
        // (e.g. from differnt regional DVB-T transmitters)
        int onid = int.Parse(uidParts[1]);
        int tsid = int.Parse(uidParts[2]);
        int sid = int.Parse(uidParts[3]);
        return channelList.Channels.Where(c => 
          c.OriginalNetworkId == onid && 
          c.TransportStreamId == tsid &&
          c.ServiceId == sid
          ).ToList();
      }

      var byUidList = channelList.GetChannelByUid(uid);
      return byUidList;
    }
    #endregion

    #region Save()
    public void Save(string fileName, DataRoot dataRoot, UnsortedChannelMode unsortedChannelMode)
    {
      using (StreamWriter stream = new StreamWriter(fileName))
      {
        foreach (var channelList in dataRoot.ChannelLists)
        {
          int radioMask = channelList.SignalType == SignalType.Radio ? 0x4000 : 0;
          foreach (var channel in channelList.Channels.Where(ch => ch.NewProgramNr != 0).OrderBy(ch => ch.NewProgramNr))
          {
            string line = string.Format("{0},{1},{2},{3},\"{4}\"",
                                        channel.RecordIndex,
                                        channel.NewProgramNr | radioMask,
                                        channel.TransportStreamId,
                                        channel.Uid,
                                        channel.Name);
            stream.WriteLine(line);
          }
        }
      }
    }
    #endregion
  }
}
