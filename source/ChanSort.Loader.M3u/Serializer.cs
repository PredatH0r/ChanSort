﻿using System;
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
    private static readonly Regex ExtInfRegex = new Regex(@"^#EXTINF:\d+,(?:(\d+)\. )?(.*)$");

    private readonly ChannelList allChannels = new ChannelList(SignalSource.IP, "All");

    private Encoding overrideEncoding;
    private string newLine = "\r\n";
    private string headerLine;
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
        "OldPosition", "Position", "Name", "FreqInMhz", "Polarity", "SymbolRate", "VideoPid", "AudioPid", "Satellite"
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
      this.headerLine = rdr.ReadLine()?.TrimEnd();
      if (this.headerLine == null || this.headerLine != "#EXTM3U")
        throw new FileLoadException("Unsupported .m3u file: " + this.FileName);

      // read lines until a non-comment non-empty line is found and then create a channel for the block
      int lineNr = 1;
      string line, extInfLine = null;
      var lines = new List<string>();
      while ((line = rdr.ReadLine()) != null)
      {
        ++lineNr;
        lines.Add(line);

        if (line.Trim() == "") 
          continue;

        if (line.StartsWith("#EXTINF:"))
          extInfLine = line;

        if (!line.StartsWith("#"))
        {
          ReadChannel(lineNr, line, extInfLine, lines);
          lines = new List<string>();
          extInfLine = null;
        }
      }

      this.trailingLines = lines;
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(int uriLineNr, string uriLine, string extInfLine, List<string> allLines)
    {
      int progNr = 0;
      string name = "";

      if (extInfLine != null)
      {
        var match = ExtInfRegex.Match(extInfLine);
        if (match.Success)
        {
          if (!string.IsNullOrEmpty(match.Groups[2].Value))
            progNr = this.ParseInt(match.Groups[1].Value);
          name = match.Groups[2].Value;
        }
      }

      if (progNr == 0)
        progNr = this.allChannels.Count + 1;

      var chan = new Channel(uriLineNr, progNr, name, allLines);

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

    #region Save()
    public override void Save(string tvOutputFile)
    {
      using var file = new StreamWriter(new FileStream(tvOutputFile, FileMode.Create), this.overrideEncoding ?? this.DefaultEncoding);
      file.NewLine = this.newLine;

      file.WriteLine(this.headerLine);

      foreach (ChannelInfo channel in this.allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel is Channel chan && !channel.IsDeleted)
        {
          foreach (var line in chan.Lines)
          {
            if (line.StartsWith("#EXTINF:"))
              file.WriteLine($"#EXTINF:0,{chan.NewProgramNr}. {chan.Name}");
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