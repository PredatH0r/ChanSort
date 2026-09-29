using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChanSort.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChanSort.Loader.Chmax
{
  /*
   * This serializer reads channel lists of Android based satellite receivers using the "chmax" .chl format.
   * The file is a sequence of concatenated JSON objects, each with a "Type" property:
   * "index" (counters), "fav" (favorite lists), "sat" (satellites), "tp" (transponders) and "ch" (TV and radio channels).
   * TV and radio channels are numbered separately by their 0-based "Index" value, which is also their order in the file.
   * The loader keeps the original JSON text of each object and only patches the changed values when saving.
   */
  class Serializer : SerializerBase
  {
    private static readonly Regex IndexValue = new Regex(@"(""Index""\s*:\s*)-?\d+");
    private static readonly Regex NameValue = new Regex(@"(""Name""\s*:\s*)""(?:[^""\\]|\\.)*""");

    private readonly ChannelList tvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "TV");
    private readonly ChannelList radioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Radio");

    private Encoding encoding;
    private string preamble = "";
    private readonly List<Segment> segments = new List<Segment>();
    private readonly Dictionary<int, JObject> satellites = new Dictionary<int, JObject>();
    private readonly Dictionary<int, JObject> transponders = new Dictionary<int, JObject>();

    private class Segment
    {
      public string Type;
      public string Json;
    }

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.FavoritesMode = FavoritesMode.None;
      this.Features.CanSaveAs = true;
      this.Features.CanHaveGaps = false;
      this.Features.CanLockChannels = true;
      this.Features.CanSkipChannels = true;
      this.Features.CanHideChannels = true;

      this.DataRoot.AddChannelList(this.tvChannels);
      this.DataRoot.AddChannelList(this.radioChannels);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      var content = File.ReadAllBytes(this.FileName);
      this.encoding = Tools.HasUtf8Bom(content) ? new UTF8Encoding(true) : new UTF8Encoding(false);
      var text = this.encoding.GetString(content);
      if (text.Length > 0 && text[0] == '﻿')
        text = text.Substring(1);

      this.SplitObjects(text);
      if (this.segments.Count == 0 || this.segments[0].Type != "index")
        throw LoaderException.TryNext("Not a chmax channel list: first JSON object is not of type \"index\"");

      int recordIndex = 0;
      foreach (var seg in this.segments)
      {
        var obj = ParseObject(seg.Json);
        switch (seg.Type)
        {
          case "sat":
            this.satellites[(int)obj["Index"]] = obj;
            break;
          case "tp":
            this.transponders[(int)obj["Index"]] = obj;
            break;
          case "ch":
            this.ReadChannel(recordIndex, seg.Json, obj);
            break;
        }
        ++recordIndex;
      }

      // channels are stored separately from the other objects to allow reordering them freely
      this.segments.RemoveAll(seg => seg.Type == "ch");
    }
    #endregion

    #region SplitObjects()
    /// <summary>
    /// Splits the text into top-level JSON objects. Each segment includes the whitespace/line break after the closing brace.
    /// </summary>
    private void SplitObjects(string text)
    {
      int i = 0;
      while (i < text.Length && char.IsWhiteSpace(text[i]))
        ++i;
      this.preamble = text.Substring(0, i);

      while (i < text.Length)
      {
        if (text[i] != '{')
          throw LoaderException.TryNext("Not a chmax channel list: unexpected character outside of JSON object");

        int start = i;
        int depth = 0;
        bool inString = false;
        for (; i < text.Length; i++)
        {
          var c = text[i];
          if (inString)
          {
            if (c == '\\')
              ++i;
            else if (c == '"')
              inString = false;
          }
          else if (c == '"')
            inString = true;
          else if (c == '{' || c == '[')
            ++depth;
          else if (c == '}' || c == ']')
          {
            if (--depth == 0)
              break;
          }
        }
        if (i >= text.Length)
          throw LoaderException.TryNext("Not a chmax channel list: incomplete JSON object at end of file");

        ++i;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
          ++i;

        var json = text.Substring(start, i - start);
        var type = (string)ParseObject(json)["Type"];
        if (type == null)
          throw LoaderException.TryNext("Not a chmax channel list: JSON object without \"Type\"");
        this.segments.Add(new Segment { Type = type, Json = json });
      }
    }
    #endregion

    #region ParseObject()
    private static JObject ParseObject(string json)
    {
      try
      {
        return JObject.Parse(json);
      }
      catch (JsonException ex)
      {
        throw LoaderException.TryNext("Not a chmax channel list: " + ex.Message);
      }
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(int recordIndex, string json, JObject obj)
    {
      var isRadio = (string)obj["TVType"] == "Radio";
      var list = isRadio ? this.radioChannels : this.tvChannels;
      var index = (int)obj["Index"];

      var chan = new Channel(list.SignalSource, recordIndex, index + 1, (string)obj["Name"], json);
      chan.ServiceId = (int?)obj["SID"] ?? 0;
      chan.VideoPid = (int?)obj["VideoPID"] ?? 0;
      chan.PcrPid = (int?)obj["PcrPID"] ?? 0;
      chan.AudioPid = (int?)obj["Audio"]?.FirstOrDefault()?["PID"] ?? 0;
      chan.Provider = (string)obj["Provider"];
      chan.Encrypted = ((int?)obj["CA"] ?? 0) != 0;
      chan.Lock = ((int?)obj["Lock"] ?? 0) != 0;
      chan.Skip = ((int?)obj["Skip"] ?? 0) != 0;
      chan.Hidden = ((int?)obj["Hide"] ?? 0) != 0;
      chan.ServiceTypeName = isRadio ? "Radio" : (string)obj["VideoType"];

      if (this.transponders.TryGetValue((int?)obj["TPIndex"] ?? -1, out var tp))
      {
        chan.FreqInMhz = (int?)tp["Freq"] ?? 0;
        chan.SymbolRate = (int?)tp["SR"] ?? 0;
        var pol = (string)tp["Pol"];
        chan.Polarity = string.IsNullOrEmpty(pol) ? '\0' : char.ToUpperInvariant(pol[0]);
        if (this.satellites.TryGetValue((int?)tp["SatIndex"] ?? -1, out var sat))
        {
          // orbital position in 1/10 degree east, values > 1800 are west
          var pos = (int?)sat["Angle"] ?? 0;
          chan.SatPosition = pos > 1800
            ? ((3600 - pos) / 10m).ToString("0.0", CultureInfo.InvariantCulture) + "W"
            : (pos / 10m).ToString("0.0", CultureInfo.InvariantCulture) + "E";
          chan.Satellite = (string)sat["Name"];
        }
      }

      this.DataRoot.AddChannel(list, chan);
    }
    #endregion

    #region Save()
    public override void Save()
    {
      if (!string.IsNullOrEmpty(this.SaveAsFileName))
        this.FileName = this.SaveAsFileName;

      var tv = this.GetChannelsToSave(this.tvChannels);
      var radio = this.GetChannelsToSave(this.radioChannels);

      // map old to new index so that favorite lists keep pointing to the same channels
      var tvIndexMap = new Dictionary<int, int>();
      var radioIndexMap = new Dictionary<int, int>();
      for (int i = 0; i < tv.Count; i++)
        tvIndexMap[tv[i].OldProgramNr - 1] = i;
      for (int i = 0; i < radio.Count; i++)
        radioIndexMap[radio[i].OldProgramNr - 1] = i;

      // channels are written after the transponders, where they were located in the original file
      var lastTp = this.segments.LastOrDefault(s => s.Type == "tp");
      var sb = new StringBuilder(this.preamble);
      foreach (var seg in this.segments)
      {
        var json = seg.Json;
        if (seg.Type == "index")
        {
          json = ReplaceIntValue(json, "ChTV", tv.Count);
          json = ReplaceIntValue(json, "CHRadio", radio.Count);
        }
        else if (seg.Type == "fav")
        {
          json = RemapIntArray(json, "TVChs", tvIndexMap);
          json = RemapIntArray(json, "RadioChs", radioIndexMap);
        }
        sb.Append(json);
        if (seg == lastTp)
          this.AppendChannels(sb, tv, radio);
      }
      if (lastTp == null)
        this.AppendChannels(sb, tv, radio);

      File.WriteAllText(this.FileName, sb.ToString(), this.encoding);
    }

    private List<Channel> GetChannelsToSave(ChannelList list)
    {
      // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
      return list.GetChannelsByNewOrder().OfType<Channel>().Where(ch => !ch.IsDeleted && ch.NewProgramNr > 0).ToList();
    }

    private void AppendChannels(StringBuilder sb, List<Channel> tv, List<Channel> radio)
    {
      foreach (var list in new[] { tv, radio })
      {
        for (int i = 0; i < list.Count; i++)
          sb.Append(this.UpdateChannelJson(list[i], i));
      }
    }

    private string UpdateChannelJson(Channel chan, int index)
    {
      var json = IndexValue.Replace(chan.Json, m => m.Groups[1].Value + index, 1);
      if (chan.Name != chan.OriginalName)
        json = NameValue.Replace(json, m => m.Groups[1].Value + JsonConvert.ToString(chan.Name ?? ""), 1);
      json = ReplaceIntValue(json, "Lock", chan.Lock ? 1 : 0);
      json = ReplaceIntValue(json, "Skip", chan.Skip ? 1 : 0);
      json = ReplaceIntValue(json, "Hide", chan.Hidden ? 1 : 0);
      return json;
    }
    #endregion

    #region ReplaceIntValue(), RemapIntArray()
    private static string ReplaceIntValue(string json, string key, int value)
    {
      var regex = new Regex("(\"" + key + "\"\\s*:\\s*)-?\\d+");
      return regex.Replace(json, m => m.Groups[1].Value + value, 1);
    }

    /// <summary>
    /// The favorite lists contain the Index values of the channels. They are updated to the new Index values and deleted channels are removed.
    /// </summary>
    private static string RemapIntArray(string json, string key, Dictionary<int, int> indexMap)
    {
      var regex = new Regex("(\"" + key + "\"\\s*:\\s*)\\[([^\\]]*)\\]");
      return regex.Replace(json, m =>
      {
        var items = m.Groups[2].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        if (items.Count == 0 || !items.All(s => int.TryParse(s, out _)))
          return m.Value;
        var remapped = items.Select(int.Parse).Where(indexMap.ContainsKey).Select(i => indexMap[i]).ToList();
        return m.Groups[1].Value + (remapped.Count == 0 ? "[ ]" : "[ " + string.Join(", ", remapped) + " ]");
      }, 1);
    }
    #endregion
  }
}
