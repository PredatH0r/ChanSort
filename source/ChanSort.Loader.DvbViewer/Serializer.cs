using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ChanSort.Api;

namespace ChanSort.Loader.DvbViewer
{
  /*
   * This serializer reads channel lists exported by DVBViewer as .ini file.
   * Each channel is stored in a [ChannelN] section with N being the 0-based position in the list. There is no separate channel number.
   * Additional audio tracks of a service are stored as separate [ChannelN] entries (flag 128) right after the main entry of the service.
   * The loader keeps all key=value lines untouched and only renumbers the sections and updates the Name when saving.
   */
  class Serializer : SerializerBase
  {
    private static readonly Regex SectionHeader = new Regex(@"^\[Channel(\d+)\]$");

    // bits of the "Encrypted" value
    private const int FlagEncrypted = 0x01;
    private const int FlagVideo = 0x08;
    private const int FlagAudio = 0x10;
    private const int FlagAdditionalAudioTrack = 0x80;

    private readonly ChannelList allChannels = new ChannelList(0, "All");

    private Encoding overrideEncoding;
    private string newLine = "\r\n";
    private readonly List<string> headerLines = new List<string>();

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.FavoritesMode = FavoritesMode.None;
      this.Features.CanSaveAs = true;
      this.Features.CanLockChannels = false;
      this.Features.CanSkipChannels = false;
      this.Features.CanHideChannels = false;

      this.DataRoot.AddChannelList(this.allChannels);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      // read file as binary and detect optional BOM and UTF-8 encoding
      var content = File.ReadAllBytes(this.FileName);
      if (Tools.HasUtf8Bom(content))
        overrideEncoding = Encoding.UTF8;
      else if (content.Length >= 2 && content[0] == 0xFF && content[1] == 0xFE)
        overrideEncoding = Encoding.Unicode;
      else if (Tools.IsUtf8(content))
        overrideEncoding = new UTF8Encoding(false);

      var text = (overrideEncoding ?? this.DefaultEncoding).GetString(content);
      this.newLine = text.Contains("\r\n") ? "\r\n" : "\n";

      var rdr = new StringReader(text);
      List<string> lines = null;
      int sectionIndex = 0;
      string line;
      while ((line = rdr.ReadLine()) != null)
      {
        if (sectionIndex == 0 && line.Length > 0 && line[0] == '﻿')
          line = line.Substring(1);

        var trimmed = line.Trim();
        if (trimmed.StartsWith("["))
        {
          var match = SectionHeader.Match(trimmed);
          if (!match.Success || int.Parse(match.Groups[1].Value) != sectionIndex)
            throw LoaderException.TryNext("Not a DVBViewer channel list: unexpected section " + trimmed);
          if (lines != null)
            this.ReadChannel(sectionIndex - 1, lines);
          lines = new List<string>();
          ++sectionIndex;
          continue;
        }

        if (lines == null)
        {
          if (trimmed != "" && !trimmed.StartsWith(";"))
            throw LoaderException.TryNext("Not a DVBViewer channel list: content before first [Channel0] section");
          this.headerLines.Add(line);
        }
        else if (trimmed != "")
          lines.Add(line);
      }

      if (lines == null)
        throw LoaderException.TryNext("Not a DVBViewer channel list: no [Channel0] section found");
      this.ReadChannel(sectionIndex - 1, lines);
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(int index, List<string> lines)
    {
      var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      foreach (var line in lines)
      {
        var idx = line.IndexOf('=');
        if (idx > 0)
          values[line.Substring(0, idx).Trim()] = line.Substring(idx + 1);
      }

      if (!values.ContainsKey("TunerType") || !values.ContainsKey("Name"))
        throw LoaderException.TryNext("Not a DVBViewer channel list: missing TunerType or Name in [Channel" + index + "]");

      string Get(string key) => values.TryGetValue(key, out var val) ? val : null;

      var tunerType = this.ParseInt(Get("TunerType"));
      var flags = this.ParseInt(Get("Encrypted"));

      var signalSource = tunerType switch
      {
        0 => SignalSource.DvbC,
        1 => SignalSource.DvbS,
        2 => SignalSource.DvbT,
        3 => SignalSource.Antenna,
        4 => SignalSource.Ip,
        _ => (SignalSource)0
      };
      if ((flags & FlagVideo) != 0)
        signalSource |= SignalSource.Tv;
      else if ((flags & FlagAudio) != 0)
        signalSource |= SignalSource.Radio;
      else
        signalSource |= SignalSource.Data;

      var chan = new Channel(signalSource, index, 0, Get("Name"), lines);
      chan.TunerType = tunerType;
      chan.Flags = flags;
      chan.Source = tunerType switch { 0 => "DVB-C", 1 => "DVB-S", 2 => "DVB-T", 3 => "ATSC", 4 => "IPTV", _ => "" };
      chan.Encrypted = (flags & FlagEncrypted) != 0;
      chan.Provider = Get("Category");
      chan.OriginalNetworkId = this.ParseInt(Get("NetworkID"));
      chan.TransportStreamId = this.ParseInt(Get("StreamID"));
      chan.ServiceId = this.ParseInt(Get("SID"));
      chan.VideoPid = this.ParseInt(Get("VPID"));
      chan.AudioPid = this.ParseInt(Get("APID"));
      chan.PcrPid = this.ParseInt(Get("PCRPID"));
      chan.SymbolRate = this.ParseInt(Get("Symbolrate"));
      chan.Frequency = this.ParseInt(Get("Frequency"));

      if (tunerType == 1)
      {
        // satellite frequencies are stored in MHz, the orbital position in 1/10 degree east (values > 1800 are west)
        chan.FreqInMhz = chan.Frequency;
        var pos = this.ParseInt(Get("OrbitalPos"));
        chan.SatPosition = pos > 1800
          ? ((3600 - pos) / 10m).ToString("0.0", CultureInfo.InvariantCulture) + "W"
          : (pos / 10m).ToString("0.0", CultureInfo.InvariantCulture) + "E";
        chan.Satellite = chan.SatPosition;
        var pol = Get("Polarity") ?? "";
        chan.Polarity = pol switch { "0" => 'H', "1" => 'V', "2" => 'L', "3" => 'R', _ => pol.Length == 1 ? char.ToUpperInvariant(pol[0]) : '\0' };
      }
      else
      {
        // cable and terrestrial frequencies are stored in kHz
        chan.FreqInMhz = chan.Frequency / 1000m;
        chan.Satellite = chan.Source;
      }

      // attach additional audio tracks to the main entry of the service
      if ((flags & FlagAdditionalAudioTrack) != 0 && this.allChannels.Channels.Count > 0)
      {
        var main = (Channel)this.allChannels.Channels[this.allChannels.Channels.Count - 1];
        if (main.TunerType == chan.TunerType && main.Frequency == chan.Frequency && main.OriginalNetworkId == chan.OriginalNetworkId
            && main.TransportStreamId == chan.TransportStreamId && main.ServiceId == chan.ServiceId)
        {
          main.AudioTracks.Add(chan);
          main.AddDebug("+" + chan.Name);
          return;
        }
      }

      chan.OldProgramNr = this.allChannels.Channels.Count + 1;
      this.DataRoot.AddChannel(this.allChannels, chan);
    }
    #endregion

    #region Save()
    public override void Save()
    {
      if (!string.IsNullOrEmpty(this.SaveAsFileName))
        this.FileName = this.SaveAsFileName;

      using var file = new StreamWriter(new FileStream(this.FileName, FileMode.Create), this.overrideEncoding ?? this.DefaultEncoding);
      file.NewLine = this.newLine;

      foreach (var line in this.headerLines)
        file.WriteLine(line);

      int index = 0;
      foreach (ChannelInfo channel in this.allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel is not Channel chan || channel.IsDeleted)
          continue;

        WriteSection(file, index++, chan);
        foreach (var track in chan.AudioTracks)
          WriteSection(file, index++, track);
      }
    }

    private void WriteSection(StreamWriter file, int index, Channel chan)
    {
      file.WriteLine("[Channel" + index + "]");
      foreach (var line in chan.Lines)
      {
        if (line.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
          file.WriteLine("Name=" + chan.Name);
        else
          file.WriteLine(line);
      }
      file.WriteLine();
    }
    #endregion
  }
}
