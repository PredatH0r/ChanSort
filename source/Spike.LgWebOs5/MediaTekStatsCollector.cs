using System;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace Spike.MediatekXml
{
  class MediatekXmlStatsCollector
  {
    private static readonly string[] ChannelListFileNames = ["sdb.xml", "MtkChannelList.xml"];  // Sony, Philips
    static void Main()
    {
      using var w = new StreamWriter(@"c:\sources\chansort\testfiles\testfiles_mediatek\stats.txt");
      w.WriteLine(
        "File\tNumChan\tDVB-?\tDVB-T\tDVB-C\tDVB-S\tDVB-IP\tOther\tTV\tRadio\tData\tInOrder\tConseq\tHasFav\t" + 
        "LcnType\tOperator\tMultiBank\tScanType\tServiceType");

      var dirs = new[] { "testfiles_philips\\120.0", "testfiles_philips\\125.0", "testfiles_sony\\mediatek" };
      foreach (var dir in dirs)
        ProcessDir(Path.Combine(@"c:\sources\chansort\testfiles", dir), w);
    }

    private static void ProcessDir(string dir, StreamWriter w)
    {
      foreach (var bak in Directory.GetFiles(dir, "*.bak"))
        File.Copy(bak, bak.Replace(".bak", ""), true);

      foreach (var subdir in Directory.GetDirectories(dir))
        ProcessDir(subdir, w);

      foreach (var name in ChannelListFileNames)
      {
        var file = Path.Combine(dir, name);
        if (File.Exists(file))
          ProcessFile(file, w);
      }
    }

    private static void ProcessFile(string file, StreamWriter w)
    {
      var sb = new StringBuilder();
      sb.Append(file);
      try
      {
        var ser = new ChanSort.Loader.MediaTek.Serializer(file);
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
            var i0 = (s & SignalSource.Antenna) != 0 ? 1 : (s & SignalSource.Cable) != 0 ? 2 : (s & SignalSource.Sat) != 0 ? 3 : (s & SignalSource.Ip) != 0 ? 4 : 0;
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

        var lcnType = ser.ScanParameters.TryGet("LcnType");
        var operat = ser.ScanParameters.TryGet("Operator");
        var multiBank = ser.ScanParameters.TryGet("MultiBank");
        var scanType = ser.ScanParameters.TryGet("ScanType");
        var serviceType = ser.ScanParameters.TryGet("ServiceType"); // n/a in Sony lists

        sb.Append($"\t{lcnType}\t{operat}\t{multiBank}\t{scanType}\t{serviceType}");
      }
      catch (Exception ex)
      {
        sb.Append("\t").Append(ex.Message);
      }
      w.WriteLine(sb.ToString());
    }
  }
}
