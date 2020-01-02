using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ChanSort.Api;

namespace ChanSort.Loader.M3u
{
  class Serializer : SerializerBase
  {
    private readonly ChannelList allChannels = new ChannelList(SignalSource.IP, "All");

    private List<string> lines = new List<string>();

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
      var rdr = new StreamReader(this.FileName);
      var header = rdr.ReadLine()?.TrimEnd();
      if (header != "#EXTM3U")
        throw new FileLoadException("Unsupported .m3u file: " + this.FileName);

      int lineNr=0;
      string line1, line2;
      while ((line1 = rdr.ReadLine()) != null)
      {
        ++lineNr;
        if (line1.Trim() == "") 
          continue;

        var lineNr1 = lineNr;
        while ((line2 = rdr.ReadLine()) != null)
        {
          ++lineNr;
          if (line2.Trim() != "")
          {
            ReadChannel(lineNr1, line1, line2);
            break;
          }
        }
      }
    }
    #endregion

    #region ReadChannel()
    private static readonly Regex ExtInfRegex = new Regex(@"^#EXTINF:\d+,(?:(\d+)\. )?(.*)$");
    private void ReadChannel(int lineNr, string line1, string line2)
    {
      var match = ExtInfRegex.Match(line1);
      if (!match.Success)
        throw new FileLoadException($"Unsupported #EXTINF line #{lineNr}: {line1}");

      int progNr = string.IsNullOrEmpty(match.Groups[2].Value)
        ? this.allChannels.Count + 1
        : this.ParseInt(match.Groups[1].Value);

      Uri uri;
      try
      {
        uri = new Uri(line2);
      }
      catch
      {
         throw new FileLoadException($"Unsupported URI in line #{lineNr}: {line2}");
      }

      var chan = new Channel(lineNr, progNr, match.Groups[2].Value, line2);
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

      this.DataRoot.AddChannel(this.allChannels, chan);
    }
    #endregion

    #region DefaultEncoding
    public override Encoding DefaultEncoding
    {
      get => base.DefaultEncoding; // set to UTF-8 without BOM in constructor
      set { } // can't be changed
    }
    #endregion


    #region Save()
    public override void Save(string tvOutputFile)
    {
      using var file = new StreamWriter(new FileStream(tvOutputFile, FileMode.Create), this.DefaultEncoding);
      file.WriteLine("#EXTM3U");
      foreach (ChannelInfo channel in this.allChannels.GetChannelsByNewOrder())
      {
        // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
        if (channel is Channel chan && !channel.IsDeleted)
        {
          file.WriteLine($"#EXTINF:0,{chan.NewProgramNr}. {chan.Name}");
          file.WriteLine(chan.Uri);
        }
      }

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
