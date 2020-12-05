using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ChanSort.Api;
using Newtonsoft.Json;

namespace Spike.LgWebOs5
{
  class Program
  {
    private static string basedir;

    static void Main(string[] args)
    {
      basedir = args.Length > 0 ? args[0] : @"d:\sources\chansort\testfiles_lg";

      using var stream = new FileStream("c:\\temp\\lg.csv", FileMode.Create);
      using var csv = new StreamWriter(stream, Encoding.UTF8);

      var sources = new[] { "All", "Analog", "DVB-T", "DVB-C", "DVB-S", "Others" };
      var fields = ChanListStats.ColumnHeaders;

      csv.Write("\t\t\t\t\t\t\t\t");
      string skip = "";
      for (int i = 0; i < fields.Length - 1; i++)
        skip += "\t";
      foreach(var src in sources)
        csv.Write("\t" + src + skip);
      csv.WriteLine();
      csv.Write("Path\tStatus\tModelName\tVersion\tDTVInfo\tBroadcastCountrySettings\tcountry\tFW info\t#channels");
      foreach (var source in sources)
      {
        foreach(var field in fields)
          csv.Write("\t" + field);
      }
      csv.WriteLine();

      ProcessWebOs5Files(basedir, csv);
    }

    private static void ProcessWebOs5Files(string dir, StreamWriter csv)
    {
      var files = Directory.GetFiles(dir, "GlobalClone*.tll");
      
      foreach (var tll in files)
      {
        var bak = tll + ".bak";
        var file = File.Exists(bak) ? bak : tll;

        var line = file.Substring(basedir.Length + 1) + "\t" + ProcessFile(file);
        csv.WriteLine(line);
      }

      foreach (var subdir in Directory.GetDirectories(dir))
        ProcessWebOs5Files(subdir, csv);
    }

    #region ProcessFile()
    private static string ProcessFile(string file)
    {
      var data = File.ReadAllBytes(file);
      if (data.Length < 3)
        return "too small";
      if (data[0] != '<' || data[1] != '?')
        return "not XML";

      var xml = Encoding.UTF8.GetString(data);
      var ending = "";
      for (var i = xml.Length - 1; i >= 0; i--)
      {
        var ch = xml[i];
        if (ch == '\n')
          ending = "NL" + ending;
        else if (ch == '\r')
          ending = "CR" + ending;
        else if (ch == ' ')
          ending = "SP " + ending;
        else
          break;
      }

      var sb = new StringBuilder();
      var doc = new XmlDocument();
      try
      {
        doc.LoadXml(xml);
        var mi = doc["TLLDATA"]["ModelInfo"];
        sb.Append("\t" + mi["ModelName"]?.InnerText);
        var cv = mi["CloneVersion"];
        sb.Append("\t" + cv.Attributes["type"]?.InnerText + "." + cv["MajorVersion"]?.InnerText + "." + cv["MinorVersion"]?.InnerText + " / " + cv["SatelliteDBVersion"]?.InnerText);
        sb.Append("\t" + mi["DTVInfo"]?.InnerText);
        sb.Append("\t" + mi["BroadcastCountrySetting"]?.InnerText);
        sb.Append("\t" + mi["country"]?.InnerText);
        var legacy = doc["TLLDATA"]["CHANNEL"]["legacybroadcast"];
        if (legacy == null)
          sb.Append("\tnot webOS 5");
        else
        {
          sb.Append("\t" + GetFirmwareVersionFromInfoFile(Path.Combine(Path.GetDirectoryName(file), @"LG Smart TV\TN\INFO")));
          sb.Append(ProcessWebOs5JsonData(legacy.InnerText));
        }
      }
      catch (Exception ex)
      {
        return ex.Message + "\t" + sb;
      }

      return "ok ("+ending+")" + sb;
    }

    private static string GetFirmwareVersionFromInfoFile(string file)
    {
      // this info file contains an escaped JSON string as the value of the INFO json property
      if (!File.Exists(file))
        return "";

      var content = File.ReadAllText(file);
      content = content.Replace("\\\"", "\"");
      var key = "core_os_release\": \"";
      var start = content.IndexOf(key);
      if (start < 0)
        return "";
      var end = content.IndexOf("\"", start + key.Length + 1);
      start += key.Length;
      var value = content.Substring(start, end - start);
      return value;
    }

    #endregion

    #region ProcessWebOs5JsonData()
    private static string ProcessWebOs5JsonData(string json)
    {
      dynamic doc = JsonConvert.DeserializeObject(json);
      var sb = new StringBuilder();
      var channels = (IList) doc.channelList;

      sb.Append("\t").Append(channels?.Count);
      var tvAndRadioPerSignalSource = new Dictionary<string, ChanListStats>();
      foreach (dynamic ch in channels)
      {
        var src = (string) ch.sourceIndex;
        if (!tvAndRadioPerSignalSource.TryGetValue(src, out var stats))
        {
          stats = new ChanListStats();
          tvAndRadioPerSignalSource[src] = stats;
        }
        stats.Add(ch);
      }

      var a = new ChanListStats();
      var t = new ChanListStats();
      var c = new ChanListStats();
      var s = new ChanListStats();
      var o = new ChanListStats();
      var all = new ChanListStats();
      foreach (var entry in tvAndRadioPerSignalSource)
      {
        switch (entry.Key)
        {
          case "ANTENNA ANALOG":
          case "CABLE ANALOG":
          case "SATELLITE ANALOG":
            a += entry.Value;
            break;
          case "ANTENNA DIGITAL":
            t += entry.Value;
            break;
          case "CABLE DIGITAL":
            c += entry.Value;
            break;
          case "SATELLITE DIGITAL":
            s += entry.Value;
            break;
          default:
            o += entry.Value;
            break;
        }

        all += entry.Value;
      }

      sb.Append(all.ToString() + a + t + c + s + o);
      return sb.ToString();
    }
    #endregion
  }

  #region class ChanListStats
  class ChanListStats
  {
    public int Tv;
    public int Radio;
    public int Radio0;
    public int Radio4k;
    public int RadioMaskServiceTypeMismatch;
    public int maxMajorTv;
    public int maxMajorRadio;
    public bool inMajorOrder = true;
    public bool hasGap = false;
    public bool deletedMajor0 = false;
    public bool deletedMajorNon0 = false;

    public static readonly string[] ColumnHeaders = { 
      "TV", "Radio", 
      // "Rad 0/4K", "BadSvcType",
      "InOrder",
      "Del0/!0"
    };

    public override string ToString()
    {
      return 
        "\t" + Tv + "\t" + Radio
        // + "\t" + Radio0 + "/" + Radio4k + "\t" + RadioMaskServiceTypeMismatch
        + "\t" + (inMajorOrder ? "J" : "N") + "/" + (hasGap ? "J" : "N")
        + "\t" + (deletedMajor0 ? "J" : "N") + "/" + (deletedMajorNon0 ? "J" : "N");
    }

    public void Add(dynamic ch)
    {
      var major = (int)ch.majorNumber;
      var nr = major & 0x3FFF;

      if ((major & 0x4000) != 0)
      {
        ++Radio;
        if (inMajorOrder && nr != 0 && nr <= maxMajorRadio)
          inMajorOrder = false;
        else
          maxMajorRadio = nr;

        hasGap |= nr != 0 && nr != maxMajorRadio;
      }
      else
      {
        ++Tv;
        if (inMajorOrder && major != 0 && major <= maxMajorTv)
          inMajorOrder = false;
        else
          maxMajorTv = major;

        hasGap |= nr != 0 && nr != maxMajorTv;
      }

      if (major == 0x4000)
        ++Radio4k;

      if (ch.serviceType != null)
      {
        var serviceIsRadio = LookupData.Instance.IsRadioTvOrData((int)ch.serviceType) == SignalSource.Radio;
        if (major == 0 && serviceIsRadio)
          ++Radio0;
        if (((major & 0x4000) != 0) != serviceIsRadio)
          ++RadioMaskServiceTypeMismatch;
      }

      if (ch.deleted != null && (bool)ch.deleted)
      {
        if (nr == 0)
          deletedMajor0 = true;
        else
          deletedMajorNon0 = true;
      }
    }

    public static ChanListStats operator +(ChanListStats a, ChanListStats b)
    {
      var stats = new ChanListStats();
      stats.Tv = a.Tv + b.Tv;
      stats.Radio = a.Radio + b.Radio;
      stats.RadioMaskServiceTypeMismatch = a.RadioMaskServiceTypeMismatch + b.RadioMaskServiceTypeMismatch;
      stats.Radio0 = a.Radio0 + b.Radio0;
      stats.Radio4k = a.Radio4k + b.Radio4k;
      stats.inMajorOrder = a.inMajorOrder && b.inMajorOrder;
      stats.hasGap = a.hasGap || b.hasGap;
      stats.deletedMajor0 = a.deletedMajor0 || b.deletedMajor0;
      stats.deletedMajorNon0 = a.deletedMajorNon0 || b.deletedMajorNon0;
      return stats;
    }
  }
  #endregion
}
