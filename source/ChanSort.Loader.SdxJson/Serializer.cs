using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChanSort.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChanSort.Loader.SdxJson
{
  /*
   * This serializer reads the JSON variant of .sdx channel lists, known from Anadol satellite receivers (e.g. Izybox 4K).
   * The file is a sequence of concatenated JSON objects without separators, each object having a single property with a unique name:
   * "satellite_object_N", "transponder_object_N", "program_tv_object_N", "program_radio_object_N", "box_object", "watching_prog_object",
   * "fav_list_object_N", "fav_list_info_in_box_object", "database_header_object" and "global_variable_object".
   * TV and radio programs are numbered separately by the N in their object name, which is also their order in the file.
   * Programs are referenced by favorites and the "watching" object through their "stProgNo" (transponder index + service ID), not by their number.
   * The "database_header_object" contains the number of programs and the size in bytes of all objects before the header.
   * Some objects contain duplicate property names, so the loader keeps the original JSON text of each object and only patches the changed values when saving.
   */
  class Serializer : SerializerBase
  {
    private const string HeaderKey = "database_header_object";
    private static readonly Regex ProgramKey = new Regex(@"^program_(tv|radio)_object_(\d+)$");
    private static readonly Regex ProgramKeyInJson = new Regex(@"^(\s*\{\s*"")program_(tv|radio)_object_\d+""");
    private static readonly Regex ServiceNameValue = new Regex(@"(""ServiceName""\s*:\s*)""(?:[^""\\]|\\.)*""");

    private readonly ChannelList tvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "TV");
    private readonly ChannelList radioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Radio");

    private readonly List<Segment> segments = new List<Segment>();
    private readonly Dictionary<int, JObject> satellites = new Dictionary<int, JObject>();
    private readonly Dictionary<int, JObject> transponders = new Dictionary<int, JObject>();
    private bool hasBom;
    private string preamble = "";
    private int programInsertPos = -1;
    private int maxNameBytes = 27;
    private bool headerHasOriginalSize;
    private bool headerHasFileLength;

    private class Segment
    {
      public string Key;
      public string Json;
      public JObject Value;
    }

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.FavoritesMode = FavoritesMode.None;
      this.Features.CanSaveAs = true;
      this.Features.CanHaveGaps = false;
      this.Features.CanLockChannels = false;
      this.Features.CanSkipChannels = false;
      this.Features.CanHideChannels = false;

      this.DataRoot.AddChannelList(this.tvChannels);
      this.DataRoot.AddChannelList(this.radioChannels);
    }
    #endregion

    #region Load()
    public override void Load()
    {
      var content = File.ReadAllBytes(this.FileName);
      this.hasBom = Tools.HasUtf8Bom(content);
      var text = this.hasBom ? Encoding.UTF8.GetString(content, 3, content.Length - 3) : Encoding.UTF8.GetString(content);

      this.SplitObjects(text);
      var header = this.segments.FirstOrDefault(s => s.Key == HeaderKey);
      if (header == null || (string)header.Value?["szMark"] != "CDX")
        throw LoaderException.TryNext("Not an .sdx JSON channel list: missing \"database_header_object\" with \"szMark\": \"CDX\"");

      int recordIndex = 0;
      foreach (var seg in this.segments)
      {
        var obj = seg.Value;
        if (obj == null)
          continue;
        var m = ProgramKey.Match(seg.Key);
        if (m.Success)
          this.ReadChannel(recordIndex++, m.Groups[1].Value == "radio", int.Parse(m.Groups[2].Value), seg.Json, obj);
        else if (seg.Key.StartsWith("satellite_object_"))
          this.satellites[int.Parse(seg.Key.Substring(17))] = obj;
        else if (seg.Key.StartsWith("transponder_object_"))
          this.transponders[(int?)obj["stFlag"]?["TPIndex"] ?? int.Parse(seg.Key.Substring(19))] = obj;
        else if (seg.Key.StartsWith("fav_list_object_") && this.IsFavListInUse(obj))
          this.Features.DeleteMode = DeleteMode.NotSupported; // favorite lists reference programs, which would become invalid when a program is deleted
        else if (seg.Key == "global_variable_object")
          this.maxNameBytes = (int?)obj["max_service_name_length"] ?? this.maxNameBytes;
        else if (seg.Key == HeaderKey)
          this.ReadHeader(obj);
      }

      foreach (var list in this.DataRoot.ChannelLists)
        list.MaxChannelNameLength = this.maxNameBytes;

      // programs are stored separately from the other objects to allow reordering them freely
      this.programInsertPos = this.segments.FindIndex(s => ProgramKey.IsMatch(s.Key));
      this.segments.RemoveAll(s => ProgramKey.IsMatch(s.Key));
    }
    #endregion

    #region SplitObjects()
    /// <summary>
    /// Splits the text into top-level JSON objects. Each segment includes the whitespace after the closing brace (if any).
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
          throw LoaderException.TryNext("Not an .sdx JSON channel list: unexpected character outside of JSON object");

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
          throw LoaderException.TryNext("Not an .sdx JSON channel list: incomplete JSON object at end of file");

        ++i;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
          ++i;

        var json = text.Substring(start, i - start);
        var prop = ParseObject(json).Properties().FirstOrDefault();
        if (prop == null)
          throw LoaderException.TryNext("Not an .sdx JSON channel list: empty JSON object");
        this.segments.Add(new Segment { Key = prop.Name, Json = json, Value = prop.Value as JObject });
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
        throw LoaderException.TryNext("Not an .sdx JSON channel list: " + ex.Message);
      }
    }
    #endregion

    #region IsFavListInUse()
    private bool IsFavListInUse(JObject favList)
    {
      if (((int?)favList["sNoOfTVFavor"] ?? 0) != 0 || ((int?)favList["sNoOfRadioFavor"] ?? 0) != 0)
        return true;
      return favList["stProgNo"] is JArray progNos && progNos.Any(p => ((long?)p["uiWord32"] ?? 0) != 0);
    }
    #endregion

    #region ReadHeader()
    private void ReadHeader(JObject header)
    {
      // "uiOriginalSize" and "uiFileLength" contain the number of bytes before the header object. They are only updated when saving if that is true for the loaded file.
      var size = Encoding.UTF8.GetByteCount(this.preamble) + this.segments.TakeWhile(s => s.Key != HeaderKey).Sum(s => Encoding.UTF8.GetByteCount(s.Json));
      this.headerHasOriginalSize = (long?)header["uiOriginalSize"] == size;
      this.headerHasFileLength = (long?)header["uiFileLength"] == size;
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(int recordIndex, bool isRadio, int progIndex, string json, JObject obj)
    {
      var list = isRadio ? this.radioChannels : this.tvChannels;

      var chan = new Channel(list.SignalSource, recordIndex, progIndex + 1, (string)obj["ServiceName"], json);
      chan.ServiceId = (int?)obj["stProgNo"]?["unShort"]?["sLo16"] ?? 0;
      chan.VideoPid = (int?)obj["VideoPID"] ?? 0;
      chan.PcrPid = (int?)obj["PCRPID"] ?? 0;
      var audio = obj["AudioArray"] as JArray;
      var audioIndex = (int?)obj["AudioSelected"] ?? 0;
      if (audio != null && audio.Count > 0)
        chan.AudioPid = (int?)audio[audioIndex >= 0 && audioIndex < audio.Count ? audioIndex : 0]["PID"] ?? 0;
      var flags = obj["uiSet"]?["uiBit"];
      chan.Lock = ((int?)flags?["Lock"] ?? 0) != 0;
      chan.Skip = ((int?)flags?["Skip"] ?? 0) != 0;
      chan.Hidden = ((int?)flags?["Hide"] ?? 0) != 0;
      chan.ServiceTypeName = isRadio ? "Radio" : "TV";

      var tpIndex = (int?)obj["stProgNo"]?["unShort"]?["sHi16"] ?? -1;
      if (this.transponders.TryGetValue(tpIndex, out var tp))
      {
        chan.FreqInMhz = (int?)tp["Freq"] ?? 0;
        chan.SymbolRate = (int?)tp["SR"] ?? 0;
        chan.Polarity = ((int?)tp["stFlag"]?["POL"] ?? 0) == 0 ? 'H' : 'V';
        if (this.satellites.TryGetValue((int?)tp["stFlag"]?["SatIndex"] ?? -1, out var sat))
        {
          // orbital position in 1/10 degree, "SatDir" 1 = west
          var pos = (int?)sat["SatAngle"] ?? 0;
          var west = ((int?)sat["uiSet"]?["uiBit"]?["SatDir"] ?? 0) != 0;
          chan.SatPosition = (pos / 10m).ToString("0.0", CultureInfo.InvariantCulture) + (west ? "W" : "E");
          chan.Satellite = (string)sat["SatName"];
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

      var sb = new StringBuilder(this.preamble);
      for (int i = 0; i < this.segments.Count; i++)
      {
        if (i == this.programInsertPos)
          this.AppendChannels(sb, tv, radio);

        var seg = this.segments[i];
        var json = seg.Json;
        if (seg.Key == HeaderKey)
        {
          var size = Encoding.UTF8.GetByteCount(sb.ToString());
          json = ReplaceIntValue(json, "sTVNumber", tv.Count);
          json = ReplaceIntValue(json, "sRadioNumber", radio.Count);
          if (this.headerHasOriginalSize)
            json = ReplaceIntValue(json, "uiOriginalSize", size);
          if (this.headerHasFileLength)
            json = ReplaceIntValue(json, "uiFileLength", size);
        }
        sb.Append(json);
      }
      if (this.programInsertPos < 0 || this.programInsertPos >= this.segments.Count)
        this.AppendChannels(sb, tv, radio);

      File.WriteAllText(this.FileName, sb.ToString(), new UTF8Encoding(this.hasBom));
    }

    private List<Channel> GetChannelsToSave(ChannelList list)
    {
      // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
      return list.GetChannelsByNewOrder().OfType<Channel>().Where(ch => !ch.IsDeleted && ch.NewProgramNr > 0).ToList();
    }

    private void AppendChannels(StringBuilder sb, List<Channel> tv, List<Channel> radio)
    {
      for (int i = 0; i < tv.Count; i++)
        sb.Append(this.UpdateChannelJson(tv[i], "tv", i));
      for (int i = 0; i < radio.Count; i++)
        sb.Append(this.UpdateChannelJson(radio[i], "radio", i));
    }

    private string UpdateChannelJson(Channel chan, string type, int index)
    {
      var json = ProgramKeyInJson.Replace(chan.Json, m => m.Groups[1].Value + "program_" + type + "_object_" + index + "\"", 1);
      if (chan.Name != chan.OriginalName)
      {
        var name = TruncateUtf8(chan.Name ?? "", this.maxNameBytes);
        json = ServiceNameValue.Replace(json, m => m.Groups[1].Value + JsonConvert.ToString(name), 1);
        json = ReplaceIntValue(json, "ucNameLen", Encoding.UTF8.GetByteCount(name));
      }
      return json;
    }
    #endregion

    #region ReplaceIntValue(), TruncateUtf8()
    private static string ReplaceIntValue(string json, string key, long value)
    {
      var regex = new Regex("(\"" + key + "\"\\s*:\\s*)-?\\d+");
      return regex.Replace(json, m => m.Groups[1].Value + value, 1);
    }

    /// <summary>
    /// "ucNameLen" is the length of the UTF-8 encoded name in bytes, which is limited by "max_service_name_length"
    /// </summary>
    private static string TruncateUtf8(string name, int maxBytes)
    {
      if (maxBytes <= 0 || Encoding.UTF8.GetByteCount(name) <= maxBytes)
        return name;
      var len = name.Length;
      while (len > 0 && Encoding.UTF8.GetByteCount(name.Substring(0, len)) > maxBytes)
        --len;
      if (len > 0 && char.IsHighSurrogate(name[len - 1]))
        --len;
      return name.Substring(0, len);
    }
    #endregion
  }
}
