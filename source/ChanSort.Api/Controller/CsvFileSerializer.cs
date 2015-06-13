using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChanSort.Api
{
  /// <summary>
  /// Reads a reference list from a .csv file with the format 
  /// [dummy1],ProgramNr,[dummy2],UID,ChannelName[,SignalSource,FavAndFlags]
  /// </summary>
  public class CsvFileSerializer
  {
    private readonly HashSet<ChannelList> clearedLists = new HashSet<ChannelList>();
    private readonly DataRoot dataRoot;
    private readonly string fileName;
    private readonly bool addChannels;

    #region ctor()
    public CsvFileSerializer(string fileName, DataRoot dataRoot, bool addChannels)
    {
      this.fileName = fileName;
      this.dataRoot = dataRoot;
      this.addChannels = addChannels;
    }
    #endregion

    #region Load()
    public void Load()
    {
      this.clearedLists.Clear();
      using (var stream = new StreamReader(fileName))
        this.ReadChannelsFromStream(stream);
    }
    #endregion

    #region ReadChannelsFromStream()

    public void ReadChannelsFromStream(TextReader stream)
    {
      int lineNr = 0;
      string line = "";
      try
      {
        while ((line = stream.ReadLine()) != null)
        {
          ++lineNr;
          this.ReadChannel(line);
        }
      }
      catch (Exception ex)
      {
        throw new FileLoadException(string.Format("Error in reference file line #{0}: {1}", lineNr, line), ex);
      }
    }

    #endregion

    #region ReadChannel()

    private void ReadChannel(string line)
    {
      var parts = CsvFile.Parse(line, ',');
      if (parts.Count < 5) return;
      int programNr;
      if (!int.TryParse(parts[1], out programNr)) return;
      string uid = parts[3];
      if (uid.StartsWith("S")) // remove satellite orbital position from UID ... not all TV models provide this information
        uid = "S" + uid.Substring(uid.IndexOf('-'));
      SignalSource signalSource = GetSignalSource(ref programNr, uid, parts);
      if (signalSource == 0)
        return;

      string name = parts[4];
      ChannelList channelList = this.GetInitiallyClearedChannelList(signalSource);
      if (channelList == null)
        return;

      IEnumerable<ChannelInfo> channels = FindChannels(channelList, name, uid);
      var channel = channels == null ? null : channels.FirstOrDefault(c => c.NewProgramNr == -1);
      if (channel != null)
      {
        if (!this.addChannels)
        {
          channel.NewProgramNr = programNr;
          if ((channel.SignalSource & SignalSource.Analog) != 0)
          {
            channel.Name = name;
            channel.IsNameModified = true;
          }
          if (parts.Count >= 7)
            ApplyFlags(channel, parts[6]);
        }
      }
      else if (parts.Count >= 6) // create proxy channel when using the new ref-list format
      {        
        channel = new ChannelInfo(signalSource, uid, programNr, name);
        if (addChannels)
        {
          channel.NewProgramNr = -1;
          channel.OldProgramNr = programNr;
        }
        channelList.AddChannel(channel);
      }
    }
    #endregion

    #region GetSignalSource()
    private static SignalSource GetSignalSource(ref int slot, string uid, IList<string> parts)
    {
      // new lists store a bitmask which defines the type of channel and list it came from
      if (parts.Count >= 6 && parts[5].Length >= 4)
      {
        SignalSource s = 0;
        string code = parts[5];
        if (code[0] == 'A') s |= SignalSource.Analog;
        else if (code[0] == 'D') s |= SignalSource.Digital;

        if (code[1] == 'A') s |= SignalSource.Antenna;
        else if (code[1] == 'C') s |= SignalSource.Cable;
        else if (code[1] == 'S') s |= SignalSource.Sat;

        if (code[2] == 'T') s |= SignalSource.Tv;
        else if (code[2] == 'R') s |= SignalSource.Radio;

        s |= (SignalSource) (int.Parse(code.Substring(3)) << 12);
        return s;
      }

      // compatibility for older lists
      bool isTv = slot < 0x4000;
      slot &= 0x3FFFF;
      SignalSource signalSource;
      switch (uid[0])
      {
        case 'S': signalSource = SignalSource.DvbS; break;
        case 'C': signalSource = SignalSource.DvbCT; break;
        case 'A': signalSource = SignalSource.AnalogCT; break;
        case 'H': signalSource = SignalSource.HdPlusD; break;
        default: return 0;
      }
      signalSource |= isTv ? SignalSource.Tv : SignalSource.Radio;
      return signalSource;
    }

    #endregion

    #region GetInitiallyClearedChannelList()
    private ChannelList GetInitiallyClearedChannelList(SignalSource signalSource)
    {
      var channelList = dataRoot.GetChannelList(signalSource);
      if (channelList == null || channelList.ReadOnly)
        return null;
      if (!this.addChannels && !this.clearedLists.Contains(channelList))
      {
        foreach (var channel in channelList.Channels)
          channel.NewProgramNr = -1;
        this.clearedLists.Add(channelList);
      }
      return channelList;
    }
    #endregion

    #region FindChannels()
    private IEnumerable<ChannelInfo> FindChannels(ChannelList channelList, string name, string uid)
    {
      // if there's only a single channel with the given name, use it regardless of UID (allows for a changed freq/tranpsonder)
      IList<ChannelInfo> list = channelList.GetChannelByName(name).ToList();
      if (list.Count == 1)
        return list;

      string[] uidParts;
      if (uid.StartsWith("C") && (uidParts = uid.Split('-')).Length <= 4)
      {
        // older CSV files didn't use the Transponder as part of the UID, which is necessary
        // to distinguish between DVB-T channels with identical (onid,tsid,sid), which may be received 
        // from multiple regional transmitters on different transponders
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

    #region ApplyFlags()
    private void ApplyFlags(ChannelInfo channel, string flags)
    {
      channel.Lock = false;
      channel.Skip = false;
      channel.Hidden = false;

      foreach (char c in flags)
      {
        switch (c)
        {
          case '1': channel.Favorites |= Favorites.A; break;
          case '2': channel.Favorites |= Favorites.B; break;
          case '3': channel.Favorites |= Favorites.C; break;
          case '4': channel.Favorites |= Favorites.D; break;
          case '5': channel.Favorites |= Favorites.E; break;
          case 'L': channel.Lock = true; break;
          case 'S': channel.Skip = true; break;
          case 'H': channel.Hidden = true; break;
          case 'D': channel.IsDeleted = true; break;
        }
      }
    }
    #endregion

    #region Save()
    public void Save()
    {
      using (StreamWriter stream = new StreamWriter(fileName))
      {
        Save(stream);
      }
    }

    public void Save(StreamWriter stream)
    {
      foreach (var channelList in dataRoot.ChannelLists)
      {
        foreach (var channel in channelList.Channels.Where(ch => ch.NewProgramNr != -1).OrderBy(ch => ch.NewProgramNr))
        {
          string line = string.Format("{0},{1},{2},{3},\"{4}\",{5},{6}",
                                      "", // past: channel.RecordIndex,
                                      channel.NewProgramNr,
                                      "", // past: channel.TransportStreamId,
                                      channel.Uid,
                                      channel.Name,
                                      this.EncodeSignalSource(channel.SignalSource),
                                      this.EncodeFavoritesAndFlags(channel));
          stream.WriteLine(line);
        }
      }
    }

    #endregion

    #region EncodeSignalSource()
    private object EncodeSignalSource(SignalSource signalSource)
    {
      StringBuilder sb = new StringBuilder();
      if ((signalSource & SignalSource.Analog) != 0) sb.Append('A');
      else sb.Append('D');

      if ((signalSource & SignalSource.Antenna) != 0) sb.Append('A');
      else if ((signalSource & SignalSource.Cable) != 0) sb.Append('C');
      else sb.Append('S');

      if ((signalSource & SignalSource.Radio) != 0) sb.Append('R');
      else sb.Append('T');

      sb.Append((int)signalSource >> 12);
      return sb.ToString();
    }
    #endregion

    #region EncodeFavoritesAndFlags()
    private string EncodeFavoritesAndFlags(ChannelInfo channel)
    {
      StringBuilder sb = new StringBuilder();
      if ((channel.Favorites & Favorites.A) != 0) sb.Append('1');
      if ((channel.Favorites & Favorites.B) != 0) sb.Append('2');
      if ((channel.Favorites & Favorites.C) != 0) sb.Append('3');
      if ((channel.Favorites & Favorites.D) != 0) sb.Append('4');
      if ((channel.Favorites & Favorites.E) != 0) sb.Append('5');
      if (channel.Lock) sb.Append('L');
      if (channel.Skip) sb.Append('S');
      if (channel.Hidden) sb.Append('H');
      if (channel.IsDeleted) sb.Append('D');
      return sb.ToString();
    }

    #endregion
  }
}
