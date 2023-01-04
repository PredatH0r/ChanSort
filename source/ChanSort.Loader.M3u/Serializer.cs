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
   * This serializer reads .m3u files that are used for SAT>IP lists. Some hardware SAT>IP servers use this format as well as VLC.
   * There is no official standard for .m3u and files may have a UTF-8 BOM or not, may be encoded in UTF-8 or a locale specific and my have different new-line sequences.
   * This loader attempts to maintain the original file as much as possible, including comment lines that are not directly understood by ChanSort.
   */
  class Serializer : SerializerBase
  {
    private static readonly Regex ExtInfTrackName = new Regex(@"^(?:(\d+). )?(.*)$");
    private readonly ChannelList allChannels = new ChannelList(SignalSource.IP, "All");

    private Encoding overrideEncoding;
    private string newLine = "\r\n";
    private bool allChannelsPrefixedWithProgNr = true;
    private List<string> headerLines = new List<string>();
    private List<string> trailingLines = new List<string>(); // comment and blank lines after the last URI line

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
      this.Features.AllowShortNameEdit = true;

      this.DataRoot.AddChannelList(this.allChannels);

      base.DefaultEncoding = new UTF8Encoding(false);
      this.allChannels.VisibleColumnFieldNames = new List<string>()
      {
        "+OldPosition", "+Position", "+Name", "+SatPosition", "+Source", "+FreqInMhz", "+Polarity", "+SymbolRate", "+Satellite", "+Provider", "+Debug", "+ShortName"
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
      int idx = Array.IndexOf(content, (byte)'\n');
      this.newLine = idx >= 1 && content[idx-1] == '\r' ? "\r\n" : "\n";

      var rdr = new StreamReader(new MemoryStream(content), overrideEncoding ?? this.DefaultEncoding);
      string line = rdr.ReadLine()?.TrimEnd();
      if (line == null || !(line == "#EXTM3U" || line.StartsWith("#EXTM3U ")))
        throw LoaderException.Fail("Unsupported .m3u file: " + this.FileName);

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
      string paramStr = null;
      int extInfParamIndex = -1;

      int extInfTrackNameIndex = -1;
      if (extInfLine != null)
      {
        bool extInfContainsProgNr = false;
        ParseExtInf(extInfLine, out name, out extInfTrackNameIndex, out paramStr, out extInfParamIndex, out var param);
        if (name != "")
        {
          var match = ExtInfTrackName.Match(name);
          if (!string.IsNullOrEmpty(match.Groups[1].Value))
          {
            progNr = this.ParseInt(match.Groups[1].Value);
            name = match.Groups[2].Value;
            extInfContainsProgNr = true;
          }
        }

        if (string.IsNullOrEmpty(group))
          param.TryGetValue("group-title", out group);

        this.allChannelsPrefixedWithProgNr &= extInfContainsProgNr;
      }


      if (progNr == 0)
        progNr = this.allChannels.Count + 1;

      var chan = new Channel(uriLineNr, progNr, name, allLines);
      chan.Uid = uriLine;
      chan.ExtInfTrackNameIndex = extInfTrackNameIndex;
      chan.Provider = group;
      chan.ShortName = paramStr;
      chan.ExtInfParamIndex = extInfParamIndex;

      try
      {
        var uri = new Uri(uriLine);
        chan.Satellite = uri.GetLeftPart(UriPartial.Path);
        var parms = HttpUtility.ParseQueryString(uri.Query);
        chan.AddDebug(uri.Query);
        foreach (var key in parms.AllKeys)
        {
          var val = parms.Get(key);
          switch (key)
          {
            case "src":
              chan.SatPosition = "src=" + val;
              break;
            case "freq":
              chan.FreqInMhz = this.ParseInt(val);
              break;
            case "pol":
              if (val.Length == 1)
                chan.Polarity = Char.ToUpperInvariant(val[0]);
              break;
            case "msys":
              chan.Source = val;
              break;
            case "sr":
              chan.SymbolRate = this.ParseInt(val);
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

    #region ParseExtInf()

    enum ExtInfParsePhase
    {
      Header,
      Length,
      Key,
      Value,
      Name
    }

    /// <summary>
    /// parse track name from lines that may look like:
    /// #EXTINF:&lt;length&gt;[ key="value" ...],&lt;TrackName&gt;
    /// </summary>
    private void ParseExtInf(string extInfLine, out string name, out int nameIndex, out string paramString, out int paramIndex, out Dictionary<string,string> param)
    {
      name = "";
      nameIndex = -1;
      paramString = "";
      paramIndex = -1;
      param = new Dictionary<string, string>();
      bool inQuote = false;
      var key = "";
      var value = "";
      ExtInfParsePhase phase = ExtInfParsePhase.Header;
      for (int i = 0, c = extInfLine.Length; i < c; i++)
      {
        var ch = extInfLine[i];
        switch (phase)
        {
          case ExtInfParsePhase.Header:
            if (ch == ':')
              phase = ExtInfParsePhase.Length;
            break;
          case ExtInfParsePhase.Length:
            if (ch == ' ')
            {
              phase = ExtInfParsePhase.Key;
              paramIndex = i;
            }
            else if (ch == ',')
            {
              phase = ExtInfParsePhase.Name;
              nameIndex = i + 1;
            }
            break;
          case ExtInfParsePhase.Key:
            if (ch == '=')
              phase = ExtInfParsePhase.Value;
            else
              key += ch;
            break;
          case ExtInfParsePhase.Value:
            if (ch == '"')
              inQuote = !inQuote;
            else if (ch == ' ' && !inQuote)
            {
              param[key] = value;
              key = "";
              value = "";
            }
            else if (ch == ',' && !inQuote)
            {
              param[key] = value;
              phase = ExtInfParsePhase.Name;
              nameIndex = i + 1;
            }
            else
              value += ch;
            break;
          case ExtInfParsePhase.Name:
            if (ch != ' ' || name.Length > 0)
              name += ch;
            break;
        }
      }

      if (paramIndex >= 0 && nameIndex >= 0)
        paramString = extInfLine.Substring(paramIndex + 1, nameIndex - paramIndex - 2);
    }

    #endregion

    #region Save()
    public override void Save()
    {
      if (!string.IsNullOrEmpty(this.SaveAsFileName))
        this.FileName = this.SaveAsFileName;

      using var file = new StreamWriter(new FileStream(this.FileName, FileMode.Create), this.overrideEncoding ?? this.DefaultEncoding);
      file.NewLine = this.newLine;

      foreach(var line in this.headerLines)
        file.WriteLine(line);

      foreach (ChannelInfo channel in this.allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel is not Channel chan || channel.IsDeleted) 
          continue;

        foreach (var line in chan.Lines)
        {
          if (line.StartsWith("#EXTINF:"))
          {
            var progNrPrefix = this.allChannelsPrefixedWithProgNr ? chan.NewProgramNr + ". " : "";
            string linePrefix;
            if (chan.ExtInfParamIndex >= 0)
              linePrefix = line.Substring(0, chan.ExtInfParamIndex) + " " + chan.ShortName + ",";
            else if (chan.ExtInfTrackNameIndex >= 0)
              linePrefix = line.Substring(0, chan.ExtInfTrackNameIndex);
            else 
              linePrefix = "#EXTINF:-1,";
            file.WriteLine($"{linePrefix}{progNrPrefix}{chan.Name}");
          }
          else
            file.WriteLine(line);
        }
      }

      foreach(var line in this.trailingLines)
        file.WriteLine(line);
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
