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
  class WebOs5StatsCollector
  {
    private static string basedir;

    static void Main(string[] args)
    {
      CollectLgWebOs5Stats(args);
    }

    #region CollectLgWebOs5Stats()
    static void CollectLgWebOs5Stats(string[] args)
    {
      basedir = args.Length > 0 ? args[0] : @"d:\sources\chansort\testfiles_lg";

      using var stream = new FileStream(@"d:\sources\chansort\testfiles_lg\__webOS5\lg.csv", FileMode.Create);
      using var csv = new StreamWriter(stream, Encoding.UTF8);

      var sources = new[] { "All", "Analog", "DVB-T", "DVB-C", "DVB-S", "Others" };

      csv.Write("\t\t\t\t\t\t\t\t");
      bool detailed = true;
      foreach (var src in sources)
      {
        csv.Write("\t" + src + Dup("\t", (detailed ? ChanListStats.ColumnHeadersLong.Length : ChanListStats.ColumnHeadersShort.Length) - 1));
        detailed = false;
      }

      csv.WriteLine();
      csv.Write("Path\tStatus\tModelName\tVersion\tDTVInfo\tBroadcastCountrySettings\tcountry\tFW info\t#channels");
      detailed = true;
      foreach (var source in sources)
      {
        var fields = detailed ? ChanListStats.ColumnHeadersLong : ChanListStats.ColumnHeadersShort;
        foreach (var field in fields)
          csv.Write("\t" + field);
        detailed = false;
      }
      csv.WriteLine();

      ProcessWebOs5Files(basedir, csv);
    }
    #endregion

    #region ProcessWebOs5Files
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
    #endregion

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

      sb.Append(all.ToString(true) + a + t + c + s + o);
      return sb.ToString();
    }
    #endregion

    #region Dup()
    private static string Dup(string str, int count)
    {
      var sb = new StringBuilder(str.Length * count);
      for (int i = 0; i < count; i++)
        sb.Append(str);
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

    public int UserEditChNumber;
    public int UserSelChNo;
    public int FactoryDefault;
    public int Disabled;
    public int Skipped;
    public int Invisible;
    public int Locked;
    public int Deleted;
    public int Discarded;
    public int UserCustomize;
    public int NumUnSel;
    public int Lcn;

    public static readonly string[] ColumnHeadersLong = { 
      "TV", "Radio", 
      // "Rad 0/4K", "BadSvcType",
      "InOrder/Gaps",
      "Del0/!0",
      "UserEdit",
      "FactDef",
      "LCN",
      "DelDisbDisc",
      "SLHU"
    };

    public static readonly string[] ColumnHeadersShort = {
      "TV", "Radio", 
      // "Rad 0/4K", "BadSvcType",
      "InOrder/Gaps",
      "Del0/!0"
    };

    public override string ToString() => ToString(false);
    public string ToString(bool full)
    {
      var part1 =  
        "\t" + Tv + "\t" + Radio
        // + "\t" + Radio0 + "/" + Radio4k + "\t" + RadioMaskServiceTypeMismatch
        + "\t" + (inMajorOrder ? "J" : "N") + "/" + (hasGap ? "J" : "N")
        + "\t" + (deletedMajor0 ? "J" : "N") + "/" + (deletedMajorNon0 ? "J" : "N");
      if (!full)
        return part1;

      return
        part1
        + "\t" + UserEditChNumber + "/" + UserSelChNo
        + "\t" + FactoryDefault
        + "\t" + Lcn
        + "\t" + Deleted + "/" + Disabled + "/" + Discarded
        + "\t" + Skipped + "/" + Locked + "/" + Invisible + "/" + NumUnSel;
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
        ++Deleted;
        if (nr == 0)
          deletedMajor0 = true;
        else
          deletedMajorNon0 = true;
      }
      if (ch.diabled != null && (bool)ch.disabled)
        ++Disabled;
      if (ch.discarded != null && (bool)ch.discarded)
        ++Discarded;

      if (ch.userEditChNumber != null && (bool) ch.userEditChNumber)
        ++UserEditChNumber;
      if (ch.userSelCHNo != null && (bool) ch.userSelCHNo)
        ++UserSelChNo;

      if (ch.factoryDefault != null && (bool) ch.factoryDefault)
        ++FactoryDefault;
      
      if (ch.skipped != null && (bool) ch.skipped)
        ++Skipped;
      if (ch.locked != null && (bool) ch.locked)
        ++Locked;
      if (ch.Invisible != null && (bool) ch.Invisible)
        ++Invisible;
      if (ch.NumUnSel != null && (bool) ch.NumUnSel)
        ++NumUnSel;

      if (ch.validLCN != null && (bool) ch.validLCN)
        ++Lcn;
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

      stats.UserEditChNumber = a.UserEditChNumber + b.UserEditChNumber;
      stats.UserSelChNo = a.UserSelChNo + b.UserSelChNo;
      stats.FactoryDefault = a.FactoryDefault + b.FactoryDefault;
      stats.Disabled = a.Disabled + b.Disabled;
      stats.Skipped = a.Skipped + b.Skipped;
      stats.Invisible = a.Invisible + b.Invisible;
      stats.Deleted = a.Deleted + b.Deleted;
      stats.Discarded = a.Discarded + b.Discarded;
      stats.UserCustomize = a.UserCustomize + b.UserCustomize;
      stats.NumUnSel = a.NumUnSel + b.NumUnSel;
      stats.Lcn = a.Lcn + b.Lcn;
      return stats;
    }
  }
  #endregion
}
