using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ChanSort.Api;

namespace ChanSort.Loader.M3u
{
  /*
   * This serializer reads .m3u files that are used for SAT>IP lists. Some hardware SAT>IP servers use this format as well as VNC.
   * There is no official standard for .m3u and files may have a UTF-8 BOM or not, may be encoded in UTF-8 or a locale specific encoding and my have different new-line sequences.
   * This loader attempts to maintain the original file as much as possible, including comment lines that are not directly understood by ChanSort.
   */
  class Serializer : SerializerBase
  {
    private static readonly Regex ExtInfTrackName = new Regex(@"^(?:(\d+). )?(.*)$");
    private readonly ChannelList allChannels = new ChannelList(SignalSource.IP, "All");

    private Encoding overrideEncoding;
    private string newLine = "\r\n";
    private List<string> headerLines = new List<string>();
    private List<string> trailingLines = new List<string>(); // comment and blank lines after the last URI line

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.SortedFavorites = false;
      this.Features.SupportedFavorites = 0;
      this.Features.CanLockChannels = false;
      this.Features.CanSkipChannels = false;
      this.Features.CanHideChannels = false;

      this.DataRoot.AddChannelList(this.allChannels);

      base.DefaultEncoding = new UTF8Encoding(false);
      this.allChannels.VisibleColumnFieldNames = new List<string>()
      {
        "OldPosition", "Position", "Name", "FreqInMhz", "Polarity", "SymbolRate", "VideoPid", "AudioPid", "Satellite", "Provider"
      };
    }
    #endregion

    #region Load()
    public override void Load()
    {
      // read file as binary and detect optional BOM and UTF-8 encoding
      var content = File.ReadAllBytes(this.FileName);
      if (Tools.HasUtf8Bom(content))
        overrideEncoding = Encoding.UTF8;
      else if (Tools.IsUtf8(content))
        overrideEncoding = new UTF8Encoding(false);

      // detect line separator
      int idx = Array.IndexOf(content, '\n');
      this.newLine = idx >= 1 && content[idx] - 1 == '\r' ? "\r\n" : "\n";

      var rdr = new StreamReader(new MemoryStream(content), overrideEncoding ?? this.DefaultEncoding);
      string line = rdr.ReadLine()?.TrimEnd();
      if (line == null || line != "#EXTM3U")
        throw new FileLoadException("Unsupported .m3u file: " + this.FileName);

      this.headerLines.Add(line);

      // read lines until a non-comment non-empty line is found and then create a channel for the block
      int lineNr = 1;
      string extInfLine = null;
      string extGrp = null;
      var lines = new List<string>();
      while ((line = rdr.ReadLine()) != null)
      {
        ++lineNr;

        // text encoding (non-standard)
        if (line.StartsWith("#EXTENC:"))
        {
          this.headerLines.Add(line);
          continue;
        }

        // playlist display title
        if (line.StartsWith("#PLAYLIST:"))
        {
          this.headerLines.Add(line);
          this.allChannels.ShortCaption = line.Substring(10);
          continue;
        }

        // begins a named group
        if (line.StartsWith("#EXTGRP:"))
        {
          extGrp = line.Substring(8);
          continue;
        }

        // everything else is assumed to belong to the next resource/URI that will follow
        lines.Add(line);

        if (line.Trim() == "") 
          continue;

        if (line.StartsWith("#EXTINF:"))
          extInfLine = line;

        if (!line.StartsWith("#"))
        {
          ReadChannel(lineNr, line, extInfLine, extGrp, lines);
          lines = new List<string>();
          extInfLine = null;
        }
      }

      this.trailingLines = lines;
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(int uriLineNr, string uriLine, string extInfLine, string group, List<string> allLines)
    {
      int progNr = 0;
      string name = "";
      int extInfTrackNameIndex = -1;

      if (extInfLine != null)
      {
        extInfTrackNameIndex = FindExtInfTrackName(extInfLine);
        if (extInfTrackNameIndex >= 0)
        {
          name = extInfLine.Substring(extInfTrackNameIndex);
          var match = ExtInfTrackName.Match(name);
          if (!string.IsNullOrEmpty(match.Groups[1].Value))
          {
            progNr = this.ParseInt(match.Groups[1].Value);
            name = match.Groups[2].Value;
          }
        }
      }

      if (progNr == 0)
        progNr = this.allChannels.Count + 1;

      var chan = new Channel(uriLineNr, progNr, name, allLines);
      chan.ExtInfTrackNameIndex = extInfTrackNameIndex;
      chan.Provider = group;

      try
      {
        var uri = new Uri(uriLine);
        chan.Satellite = uri.GetLeftPart(UriPartial.Path);
        var parms = HttpUtility.ParseQueryString(uri.Query);
        foreach (var key in parms.AllKeys)
        {
          var val = parms.Get(key);
          switch (key)
          {
            case "freq":
              chan.FreqInMhz = this.ParseInt(val);
              break;
            case "pol":
              if (val.Length == 1)
                chan.Polarity = Char.ToUpper(val[0]);
              break;
            case "sr":
              chan.SymbolRate = this.ParseInt(val);
              break;
            case "pids":
              var pids = val.Split(',');
              //if (pids.Length > 3)
              //  chan.PcrPid = this.ParseInt(pids[3]);
              if (pids.Length > 4)
                chan.VideoPid = this.ParseInt(pids[4]);
              if (pids.Length > 5)
                chan.AudioPid = this.ParseInt(pids[5]);
              break;
          }
        }

        if (name == "")
          chan.Name = chan.FreqInMhz + chan.Polarity + " " + chan.VideoPid;
      }
      catch
      {
        if (name == "")
          chan.Name = uriLine;
      }

      this.DataRoot.AddChannel(this.allChannels, chan);
    }
    #endregion

    #region FindExtInfTrackName()
    /// <summary>
    /// parse track name from lines that may look like:
    /// #EXTINF:&lt;length&gt;[ key="value" ...],&lt;TrackName&gt;
    /// </summary>
    private int FindExtInfTrackName(string extInfLine)
    {
      bool inQuote = false;
      for (int i = 0, c = extInfLine.Length; i < c; i++)
      {
        var ch = extInfLine[i];
        if (ch == ',' && !inQuote)
          return i + 1;
        if (ch == '"')
          inQuote = !inQuote;
      }

      return -1;
    }

    #endregion

    #region Save()
    public override void Save(string tvOutputFile)
    {
      using var file = new StreamWriter(new FileStream(tvOutputFile, FileMode.Create), this.overrideEncoding ?? this.DefaultEncoding);
      file.NewLine = this.newLine;

      foreach(var line in this.headerLines)
        file.WriteLine(line);

      foreach (ChannelInfo channel in this.allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel is Channel chan && !channel.IsDeleted)
        {
          foreach (var line in chan.Lines)
          {
            if (line.StartsWith("#EXTINF:"))
              file.WriteLine($"{line.Substring(0, chan.ExtInfTrackNameIndex)}{chan.NewProgramNr}. {chan.Name}");
            else
              file.WriteLine(line);
          }
        }
      }

      foreach(var line in this.trailingLines)
        file.WriteLine(line);

      this.FileName = tvOutputFile;
    }
    #endregion

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.GetFileInformation());
      return sb.ToString();
    }
    #endregion
  }
}
