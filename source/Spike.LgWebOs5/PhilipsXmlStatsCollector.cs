using System;
using System.IO;
using System.Linq;
using System.Text;
using ChanSort.Api;
using ChanSort.Loader.Philips;

namespace Spike.PhilipsXml
{
  class PhilipsXmlStatsCollector
  {
    private const string BaseDir = @"c:\sources\chansort\testfiles\testfiles_philips";

    static void Main()
    {
      using var w = new StreamWriter(Path.Combine(BaseDir, "stats.txt"));
      
      w.WriteLine("File\t#Channels\t#Lists\tAntenna\tCable\tSat\tIp\tOtherSrc\tTv\tRadio\tData\tOtherType\tIn Order\tConsecutive\tOverlap\tHasFav\tSkip\tLock\tHide");
      
      var dirs = new[] {"100.0", "105.0", "110.0", "115.0", "120.0", "125.0" };
      foreach (var dir in dirs)
        ProcessDir(Path.Combine(BaseDir, dir), w);
    }

    private static void ProcessDir(string dir, StreamWriter w)
    {
      // restore .bak file to avoid noise from badly modified files
      foreach (var bak in Directory.GetFiles(dir, "*.bak"))
        File.Copy(bak, bak.Replace(".bak", ""), true);

      // process subdirectories first
      foreach (var subdir in Directory.GetDirectories(dir))
        ProcessDir(subdir, w);

      // process files in currrect directory
      var file = Path.Combine(dir, "chanLst.bin");
      if (File.Exists(file))
        ProcessFile(file, w);
    }

    private static void ProcessFile(string file, StreamWriter w)
    {
      var sb = new StringBuilder();
      sb.Append(file.Replace(BaseDir + "\\", ""));
      try
      {
        var p = new PhilipsPlugin();
        var ser = p.CreateSerializer(file);
        if (ser is ChanSort.Loader.Philips.XmlSerializer mtk) 
          mtk.DecodeMediaTekSvl = false;// for now ignore errors due to XML/SVL data mismatch

        ser.Load();
        int totalChans = 0;
        var conseq = true;
        var inOrder = true;
        var hasFav = false;
        var srcSum = new int[5];
        var typeSum = new int[4];
        var hasOverlap = false;
        var minNrAllLists = int.MaxValue;
        var maxNrAllLists = int.MinValue;
        var hasSkip = false;
        var hasLock = false;
        var hasHide = false;
        foreach (var list in ser.DataRoot.ChannelLists)
        {
          var minNrThisList = int.MaxValue;
          var maxNrThisList = int.MinValue;
          if (list.IsMixedSourceFavoritesList)
            continue;
          totalChans += list.Channels.Count;
          var lastNr = 0;
          var chanCountBySrc = new int[5,4];
          foreach (var c in list.Channels)
          {
            inOrder &= c.OldProgramNr >= lastNr;
            if (!inOrder)
            {
            }
            conseq &= c.OldProgramNr == lastNr + 1;
            if (!conseq)
            {
            }

            if (!hasOverlap)
            {
              minNrThisList = Math.Min(minNrThisList, c.OldProgramNr);
              maxNrThisList = Math.Max(maxNrThisList, c.OldProgramNr);
            }
            lastNr = c.OldProgramNr;
            hasFav |= c.GetOldPosition(1) != -1;
            var s = c.SignalSource;
            var i0 = (s & SignalSource.Antenna) != 0 ? 0 : (s & SignalSource.Cable) != 0 ? 1 : (s & SignalSource.Sat) != 0 ? 2 : (s & SignalSource.Ip) != 0 ? 3 : 4;
            var i1 = (s & SignalSource.Tv) != 0 ? 0 : (s & SignalSource.Radio) != 0 ? 1 : (s & SignalSource.Data) != 0 ? 2 : 3;
            ++chanCountBySrc[i0, i1];
            ++srcSum[i0];
            ++typeSum[i1];
            hasSkip |= c.Skip;
            hasLock |= c.Lock;
            hasHide |= c.Hidden;
          }

          // check if this list has an overlapping ProgNr range with any other lists
          if (minNrThisList <= maxNrAllLists || maxNrThisList >= minNrAllLists)
            hasOverlap = true;
          minNrAllLists = Math.Min(minNrAllLists, minNrThisList);
          maxNrAllLists = Math.Max(maxNrAllLists, maxNrThisList);
        }

        sb.Append($"\t{totalChans}");
        sb.Append($"\t{ser.DataRoot.ChannelLists.Count(l => !l.IsMixedSourceFavoritesList && l.Channels.Count > 0)}");
        foreach (var n in srcSum)
          sb.Append("\t").Append(n);
        foreach (var n in typeSum)
          sb.Append("\t").Append(n);
        sb.Append($"\t{inOrder}\t{conseq}\t{hasOverlap}\t{hasFav}\t{hasSkip}\t{hasLock}\t{hasHide}");
      }
      catch (Exception ex)
      {
        sb.Append("\t").Append(ex.Message);
      }
      w.WriteLine(sb.ToString());
    }
  }
}
