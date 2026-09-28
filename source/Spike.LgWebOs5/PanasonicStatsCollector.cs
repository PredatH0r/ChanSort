using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using ChanSort.Api;
using Newtonsoft.Json;

namespace Spike.Panasonic
{
  class PanasonicStatsCollector
  {
    private static string basedir;

    static void Main(string[] args)
    {
      CollectStats(args);
    }

    #region CollectStats()
    static void CollectStats(string[] args)
    {
      basedir = args.Length > 0 ? args[0] : @"c:\sources\chansort\testfiles\testfiles_panasonic";

      using var stream = new FileStream(Path.Combine(basedir, @"__panasonic.csv"), FileMode.Create);
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
      csv.Write("Path\tDVB-ind\tno DVB-in\tValidUTF\tnonASCII\tUmlaut\tDVB-T\tDVB-C\tDVB-S\tDVB-IP\tTV\tRadio\tData");
      detailed = true;
      foreach (var source in sources)
      {
        var fields = detailed ? ChanListStats.ColumnHeadersLong : ChanListStats.ColumnHeadersShort;
        foreach (var field in fields)
          csv.Write("\t" + field);
        detailed = false;
      }
      csv.WriteLine();

      ProcessFiles(basedir, csv);
    }
    #endregion

    #region ProcessFiles
    private static void ProcessFiles(string dir, StreamWriter csv)
    {
      var files = Directory.GetFiles(dir, "svl.db")
        .Concat(Directory.GetFiles(dir, "svl.bin"));
      
      foreach (var tll in files)
      {
        var bak = tll + ".bak";
        var file = File.Exists(bak) ? bak : tll;

        var line = file.Substring(basedir.Length + 1) + "\t" + ProcessFile(file);
        csv.WriteLine(line);
      }

      foreach (var subdir in Directory.GetDirectories(dir))
        ProcessFiles(subdir, csv);
    }
    #endregion

    #region ProcessFile()
    private static string ProcessFile(string file)
    {
      try
      {
        var loader = new ChanSort.Loader.Panasonic.PanasonicPlugin();
        var ser = loader.CreateSerializer(file);
        if (ser == null)
          return "not supported";
        ser.Load();

        var withIndicator = false;
        var withoutIndicator = false;
        var validUtf8 = true;
        var nonAscii = false;
        var umlaut = false;
        var antenna = 0;
        var cable = 0;
        var sat = 0;
        var ip = 0;
        var tv = 0;
        var radio = 0;
        var data = 0;
        foreach (var chList in ser.DataRoot.ChannelLists)
        {
          foreach (var ch in chList.Channels)
          {
            if (ch is not ChanSort.Loader.Panasonic.DbChannel c)
              continue;
            withIndicator |= c.RawName.Length > 0 && c.RawName[0] < 0x20;
            withoutIndicator |= c.RawName.Length > 0 && c.RawName[0] >= 0x20;
            if (c.RawName.Length > 0 && (c.RawName[0] == 0x15 || c.RawName[0] >= 20))
              validUtf8 &= c.ValidUtf8;
            nonAscii |= c.NonAscii;
            var name = c.Name.ToLower();
            umlaut |= name.Any("äöüáéíóúàèìòùß".Contains);

            if ((c.SignalSource & SignalSource.Antenna) != 0)
              ++antenna;
            if ((c.SignalSource & SignalSource.Cable) != 0)
              ++cable;
            if ((c.SignalSource & SignalSource.Sat) != 0)
              ++sat;
            if ((c.SignalSource & SignalSource.Ip) != 0)
              ++ip;
            if ((c.SignalSource & SignalSource.Tv) != 0)
              ++tv;
            if ((c.SignalSource & SignalSource.Radio) != 0)
              ++radio;
            if ((c.SignalSource & SignalSource.Data) != 0)
              ++data;
          }
        }

        return $"{withIndicator}\t{withoutIndicator}\t{validUtf8}\t{nonAscii}\t{umlaut}\t{antenna}\t{cable}\t{sat}\t{ip}\t{tv}\t{radio}\t{data}";
      }
      catch (Exception ex)
      {
        return ex.Message + "\t";
      }
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
