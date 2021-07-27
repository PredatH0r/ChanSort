using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChanSort.Api;
using ChanSort.Loader.Philips;

namespace Spike.PhilipsXml
{
  class PhilipsXmlStatsCollector
  {
    static void Main()
    {
      using var w = new StreamWriter(@"d:\sources\chansort\testfiles_philips\stats.txt");
      var dirs = new[] {"100.0", "105.0", "110.0"};
      foreach (var dir in dirs)
        ProcessDir(Path.Combine(@"d:\sources\chansort\testfiles_philips", dir), w);
    }

    private static void ProcessDir(string dir, StreamWriter w)
    {
      foreach (var bak in Directory.GetFiles(dir, "*.bak"))
        File.Copy(bak, bak.Replace(".bak", ""), true);

      foreach (var subdir in Directory.GetDirectories(dir))
        ProcessDir(subdir, w);

      var file = Path.Combine(dir, "chanLst.bin");
      if (File.Exists(file))
        ProcessFile(file, w);
    }

    private static void ProcessFile(string file, StreamWriter w)
    {
      var sb = new StringBuilder();
      sb.Append(file);
      try
      {
        var p = new PhilipsPlugin();
        var ser = p.CreateSerializer(file);
        ser.Load();
        int totalChans = 0;
        var conseq = true;
        var inOrder = true;
        var hasFav = false;
        var srcSum = new int[5];
        var typeSum = new int[4];
        foreach (var list in ser.DataRoot.ChannelLists)
        {
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
            lastNr = c.OldProgramNr;
            hasFav |= c.GetOldPosition(1) != -1;
            var s = c.SignalSource;
            var i0 = (s & SignalSource.Antenna) != 0 ? 1 : (s & SignalSource.Cable) != 0 ? 2 : (s & SignalSource.Sat) != 0 ? 3 : (s & SignalSource.IP) != 0 ? 4 : 0;
            var i1 = (s & SignalSource.Tv) != 0 ? 1 : (s & SignalSource.Radio) != 0 ? 2 : (s & SignalSource.Data) != 0 ? 3 : 0;
            ++chanCountBySrc[i0, i1];
            ++srcSum[i0];
            ++typeSum[i1];
          }
        }

        sb.Append($"\t{totalChans}");
        foreach(var n in srcSum)
          sb.Append("\t").Append(n);
        foreach (var n in typeSum)
          sb.Append("\t").Append(n);
        sb.Append($"\t{inOrder}\t{conseq}\t{hasFav}");
      }
      catch (Exception ex)
      {
        sb.Append("\t").Append(ex.Message);
      }
      w.WriteLine(sb.ToString());
    }
  }
}
