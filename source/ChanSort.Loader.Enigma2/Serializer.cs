using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChanSort.Api;

namespace ChanSort.Loader.Enigma2
{
  /*
   * This class loads "userbouquet.*" channel lists from Enigma2 Linux set-top-boxes (Dreambox, Vu+, ...)
   *
   * lamedb version 4 format: https://www.satsupreme.com/showthread.php/194074-Lamedb-format-explained
   * userbouqet.* format: https://www.opena.tv/english-section/43964-iptv-service-4097-service-1-syntax.html#post376271
   */
  internal class Serializer : SerializerBase
  {
    private static readonly Encoding utf8WithoutBom = new UTF8Encoding(false);

    private ChannelList channels = new ChannelList(SignalSource.Digital, "All Channels");

    private readonly List<string> favListFileNames = new();
    private readonly Dictionary<string, Transponder> transponderByLamedbId = new();
    private readonly Dictionary<string, Channel> channelsByBouquetId = new();
    private DvbStringDecoder decoder;

    private readonly StringBuilder log = new();

    #region ctor()

    public Serializer(string inputFile) : base(inputFile)
    {
      this.FileName = Path.Combine(Path.GetDirectoryName(inputFile), "lamedb");

      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = false;
      this.Features.CanHideChannels = false;
      this.Features.FavoritesMode = FavoritesMode.MixedSource;
      this.Features.MaxFavoriteLists = 0; // dynamically added

      this.channels.IsMixedSourceFavoritesList = true;
      this.DataRoot.AddChannelList(this.channels);

      // hide columns for fields that don't exist in Silva-Schneider channel list
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove("PcrPid");
        list.VisibleColumnFieldNames.Remove("VideoPid");
        list.VisibleColumnFieldNames.Remove("AudioPid");
        list.VisibleColumnFieldNames.Remove("Lock");
        list.VisibleColumnFieldNames.Remove("Skip");
        list.VisibleColumnFieldNames.Remove("Hidden");
        list.VisibleColumnFieldNames.Remove("Encrypted");
        list.VisibleColumnFieldNames.Remove("Favorites");
        list.VisibleColumnFieldNames.Remove("ServiceType");
        list.VisibleColumnFieldNames.Add("ServiceTypeName");
      }
    }

    #endregion

    #region Load()

    public override void Load()
    {
      this.decoder = new DvbStringDecoder(this.DefaultEncoding);
      this.LoadLamedb();

      int favIndex = 0;

      // load all userbouquet files listed in bouquets.tv
      var path = Path.Combine(Path.GetDirectoryName(this.FileName), "bouquets.tv");
      if (File.Exists(path))
        LoadBouquets(path, ref favIndex);

      // load all userbouquet files listed in bouquets.radio
      path = Path.Combine(Path.GetDirectoryName(this.FileName), "bouquets.radio");
      if (File.Exists(path))
        LoadBouquets(path, ref favIndex);

      // load all unlisted userbouquet files
      //foreach (var file in Directory.GetFiles(Path.GetDirectoryName(this.FileName), "userbouquet.*"))
      //{
      //  var ext = Path.GetExtension(file);
      //  if (ext != ".tv" && ext != ".radio") // ignore .del, .bak and other irrelevant files
      //    continue;
      //  if (!this.favListFileNames.Contains(file))
      //    this.LoadUserBouquet(file, ref favIndex);
      //}
    }
    #endregion

    #region LoadLamedb()
    private void LoadLamedb()
    {
      var path = Path.Combine(Path.GetDirectoryName(this.FileName), "lamedb");
      if (!File.Exists(path))
        throw LoaderException.Fail($"Could not find required file \"{path}\"");

      using var r = new StreamReader(File.OpenRead(path), utf8WithoutBom);
      var line = r.ReadLine();
      if (line != "eDVB services /4/")
        throw LoaderException.Fail($"lamedb version 4 is required");

      string mode = null;
      Transponder tp = null;
      int tpId = 0, chanId = 0;

      while ((line = r.ReadLine()) != null)
      {
        if (line.Trim() == "")
          continue;
        if (line == "transponders")
          mode = line;
        else if (line == "services")
          mode = "services";
        else if (line == "end")
          mode = null;
        else if (mode == "transponders")
          tp = ReadLamedbTransponderLine(line, tp, ref tpId);
        else if (mode == "services")
          ReadLamedbServiceLine(line, r, ref chanId);
      }
    }
    #endregion

    #region ReadLamedbTransponderLine
    private Transponder ReadLamedbTransponderLine(string line, Transponder tp, ref int tpId)
    {
      if (line == "/")
        return tp;
      if (!line.StartsWith("\t"))
      {
        tp = new Transponder(++tpId);
        this.transponderByLamedbId[line] = tp;
        var parts = line.Split(':');
        tp.Number = FromHex(parts[0]);
        tp.TransportStreamId = FromHex(parts[1]);
        tp.OriginalNetworkId = FromHex(parts[2]);
      }
      else
      {
        if (line[1] == 's')
        {
          var parts = line.Substring(3).Split(':');
          tp.FrequencyInMhz = int.Parse(parts[0]) / 1000;
          tp.SymbolRate = int.Parse(parts[1]) / 1000;
          tp.Polarity = "HVLR"[int.Parse(parts[2])];
          tp.Satellite = GetOrCreateSatellite(int.Parse(parts[4]));
        }
      }

      return tp;
    }
    #endregion

    #region GetOrCreateSatellite
    private Satellite GetOrCreateSatellite(int orbitalPos)
    {
      if (this.DataRoot.Satellites.TryGetValue(orbitalPos, out var sat))
        return sat;
      sat = new Satellite(orbitalPos);
      sat.OrbitalPosition = $"{(float) Math.Abs(orbitalPos) / 10:0.0}{(orbitalPos<0?'W':'E')}";
      sat.Name = sat.OrbitalPosition;
      this.DataRoot.Satellites.Add(orbitalPos, sat);
      return sat;
    }
    #endregion

    #region ReadLamedbServiceLine
    private void ReadLamedbServiceLine(string line, StreamReader r, ref int chanId)
    {
      var line2 = r.ReadLine();
      var line3 = r.ReadLine();
      if (line2 == null || line3 == null)
        return;

      var ch = new Channel();
      ch.SignalSource = SignalSource.Digital;

      // line 1: SID:DvbNamespace:TSID:ONID:ServiceType:ServiceNumber
      var parts = line.Split(':');
      ch.RecordIndex = chanId;
      ch.RecordOrder = chanId;
      ch.OldProgramNr = ++chanId;
      ch.NewProgramNr = ch.OldProgramNr; // won't be set automatically because this is a mixed-source favorites list
      ch.IsDeleted = false;
      ch.ServiceId = FromHex(parts[0]);
      ch.DvbNamespace = FromHex(parts[1]);
      ch.TransportStreamId = FromHex(parts[2]);
      ch.OriginalNetworkId = FromHex(parts[3]);
      ch.ServiceType = int.Parse(parts[4]);
      ch.ServiceNumber = int.Parse(parts[5]);
      ch.SignalSource |= LookupData.Instance.IsRadioTvOrData(ch.ServiceType);

      var tpId = parts[1] + ":" + parts[2] + ":" + parts[3];
      if (this.transponderByLamedbId.TryGetValue(tpId, out var tp))
      {
        ch.Satellite = tp.Satellite?.Name;
        ch.SymbolRate = tp.SymbolRate;
        ch.FreqInMhz = tp.FrequencyInMhz;
        ch.Polarity = tp.Polarity;
      }

      // line 2: channel name (in raw DVB encoding)
      var rawName = new byte[line2.Length];
      for (int i = 0, c = rawName.Length; i < c; i++)
        rawName[i] = (byte)line2[i];
      this.decoder.GetChannelNames(rawName, 0, rawName.Length, out var longName, out var shortName);
      ch.Name = longName;
      ch.ShortName = shortName;

      // line 3: provider and other info
      parts = line3.Split(',');
      foreach (var part in parts)
      {
        var keyVal = part.Split(new char[] {':'}, 2);
        switch (keyVal[0])
        {
          case "p":
            ch.Provider = keyVal[1];
            break;
        }
      }

      //var list = (ch.SignalSource & SignalSource.Radio) != 0 ? this.radio : this.tv;
      var list = this.channels;
      this.DataRoot.AddChannel(list, ch);
      this.channelsByBouquetId[$"{ch.DvbNamespace}:{ch.OriginalNetworkId}:{ch.TransportStreamId}:{ch.ServiceId}"] = ch;
    }
    #endregion

    #region LoadBouquets
    private void LoadBouquets(string file, ref int favIndex)
    {
      var regex = new Regex(".*FROM BOUQUET \"(.*?)\".*");
      foreach (var line in File.ReadAllLines(file))
      {
        var match = regex.Match(line);
        if (match.Success)
        {
          var path = Path.Combine(Path.GetDirectoryName(file), match.Groups[1].Value);
          LoadUserBouquet(path, ref favIndex);
        }
      }
    }
    #endregion

    #region LoadUserBoquet
    private void LoadUserBouquet(string file, ref int favIndex)
    {
      if (!File.Exists(file) || this.favListFileNames.Contains(file))
        return;

      using var r = new StreamReader(File.OpenRead(file), utf8WithoutBom);
      var line = r.ReadLine();
      if (line == null || !line.StartsWith("#NAME "))
      {
        log.AppendLine($"{file} does not start with #NAME");
        return;
      }
      
      this.channels.SetFavListCaption(favIndex, line.Substring(6));

      int lineNr = 0;
      int progNr = 0;
      Channel ch = null;
      while ((line = r.ReadLine()) != null)
      {
        ++lineNr;
        if (line.Trim() == "")
          continue;
        if (line.Contains(":FROM") && line.Contains("ORDER BY")) // ignore the root-level bouquet that only references other bouquet files
          return;
        if (line.StartsWith("#DESCRIPTION "))
        {
          if (ch != null)
            ch.Description = line.Substring(13);
          continue;
        }
        if (!line.StartsWith("#SERVICE "))
          continue;
        
        var parts = line.Substring(9).Split(':');
        if (parts[0] != "1") // ignore non-DVB
          continue;
        if (parts[1] != "0") // ignore special-purpose rows
          continue;

        var prefix = parts[0] + ":" + parts[1];

        // parts[2] = DVB service type
        var sid = FromHex(parts[3]);
        var tsid = FromHex(parts[4]);
        var onid = FromHex(parts[5]);
        var dvbNamespace = FromHex(parts[6]);

        var suffix = "";
        for (int i = 7; i < parts.Length; i++)
          suffix += ":" + parts[i];

        var key = $"{dvbNamespace}:{onid}:{tsid}:{sid}";
        if (!this.channelsByBouquetId.TryGetValue(key, out ch))
        {
          log.AppendLine($"{file} line {lineNr}: service not found in lamedb");
          continue;
        }

        ch.Prefix = prefix;
        ch.Suffix = suffix;
        ch.SetOldPosition(1+favIndex, ++progNr);
      }


      this.favListFileNames.Add(file);
      ++this.Features.MaxFavoriteLists;
      ++favIndex;
    }
    #endregion

    #region FromHex()
    private int FromHex(string str)
    {
      int result = 0;
      foreach (var ch in str)
      {
        if (Char.IsWhiteSpace(ch))
          continue;

        result <<= 4;
        if (ch >= '0' && ch <= '9')
          result += ch - '0';
        else if (ch >= 'A' && ch <= 'F')
          result += ch - 'A' + 10;
        else if (ch >= 'a' && ch <= 'f')
          result += ch - 'a' + 10;
        else
          throw new ArgumentException(str + " contains invalid hex characters");
      }

      return result;
    }
    #endregion

    #region GetDataFilePaths()
    public override IEnumerable<string> GetDataFilePaths()
    {
      var list = new List<string>(this.favListFileNames.Count + 1);
      list.Add(this.FileName); // lamedb
      list.AddRange(this.favListFileNames); // userbouquet*
      return list;
    }
    #endregion


    #region Save()

    public override void Save()
    {
      for (int favIndex = 0; favIndex < this.favListFileNames.Count; favIndex++)
      {
        var file = this.favListFileNames[favIndex];
        using var w = new StreamWriter(File.Create(file), utf8WithoutBom);
        w.WriteLine($"#NAME {this.channels.GetFavListCaption(favIndex)}");
        foreach (var ch in this.channels.Channels.OrderBy(c => c.GetPosition(favIndex+1)))
        {
          if (!(ch is Channel c) || c.GetPosition(favIndex + 1) < 0)
            continue;

          w.WriteLine($"#SERVICE {c.Prefix}:{c.ServiceType:X}:{c.ServiceId:X}:{c.TransportStreamId:X}:{c.OriginalNetworkId:X}:{c.DvbNamespace:X}{c.Suffix}");
          if (c.Description != null)
            w.WriteLine($"#DESCRIPTION {c.Description}");
        }
      }
    }

    #endregion


    #region GetFileInformation()

    public override string GetFileInformation()
    {
      var sb = new StringBuilder();
      sb.Append(base.GetFileInformation());
      sb.AppendLine();
      sb.Append(this.log);
      return sb.ToString();
    }

    #endregion
  }
}