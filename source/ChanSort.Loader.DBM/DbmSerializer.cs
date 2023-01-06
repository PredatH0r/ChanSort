using System;
using System.IO;
using System.Reflection;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.DBM
{
  /*
  Loads .DBM binary channel lists from Xoro, TechniSat, ...
  There are different variants for DVB-S, DVB-C, ... which require specific configuration entries in ChanSort.Loader.DBM.ini
  */
  public class DbmSerializer : SerializerBase
  {
    private byte[] data;

    private readonly IniFile ini;
    private IniFile.Section sec;
    private DataMapping mapping;
    private bool isDvbS;
    
    private readonly ChannelList allChannels = new(SignalSource.All, "All");
    private readonly StringBuilder logMessages = new();

    #region ctor()
    public DbmSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanHaveGaps = false;
      this.Features.FavoritesMode = FavoritesMode.Flags;
      this.Features.MaxFavoriteLists = 4;
      this.Features.AllowGapsInFavNumbers = false;
      this.Features.CanEditFavListNames = false;

      this.DataRoot.AddChannelList(this.allChannels);

      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.AudioPid));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ChannelOrTransponder));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Provider));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Encrypted));
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceType));
        list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.ServiceTypeName));
      }

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);
    }
    #endregion

    // loading

    #region Load
    public override void Load()
    {
      var info = new FileInfo(this.FileName);
      this.sec = ini.GetSection("dbm:" + info.Length);
      if (sec == null)
        throw LoaderException.Fail($"No configuration for .DBM files with size {info.Length} in .ini file");

      this.isDvbS = sec.GetBool("isDvbS");
      if (isDvbS)
      {
        allChannels.ShortCaption = "DVB-S";
        allChannels.SignalSource &= ~SignalSource.MaskAntennaCableSat;
        allChannels.SignalSource |= SignalSource.Sat;
      }
      else
      {
        allChannels.ShortCaption = "DVB-C";
        allChannels.SignalSource &= ~SignalSource.MaskAntennaCableSat;
        allChannels.SignalSource |= SignalSource.Cable;
        allChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Satellite));
      }

      this.data = File.ReadAllBytes(this.FileName);
      this.mapping = new DataMapping(sec);
      this.mapping.SetDataPtr(data, 0);

      ValidateChecksum();
      LoadSatellites();
      LoadTransponder();
      LoadChannels();
    }
    #endregion

    #region ValidateChecksum()
    private void ValidateChecksum()
    {
      var expectedChecksum = BitConverter.ToUInt16(data, 0);
      var calculatedChecksum = CalcChecksum(data, sec.GetInt("offData"), (int)mapping.GetDword("offDataLength"));
      if (expectedChecksum != calculatedChecksum)
      {
        var msg = $"Invalid checksum. Expected {expectedChecksum:x4} but calculated {calculatedChecksum:x4}";
        throw LoaderException.Fail(msg);
      }
    }
    #endregion

    #region LoadSatellites()
    private void LoadSatellites()
    {
      if (!this.isDvbS)
        return;

      var num = sec.GetInt("numSatellite");
      var offBitmap = sec.GetInt("offSatelliteBitmap");

      mapping.SetDataPtr(data, sec.GetInt("offSatelliteData"));
      var recordSize = sec.GetInt("lenSatelliteData");

      for (int i = 0; i < num; i++)
      {
        if ((data[offBitmap + i / 8] & (1 << (i & 0x07))) != 0)
        {
          var s = new Satellite(i);
          s.Name = mapping.GetString("offSatName", sec.GetInt("lenSatName"));
          var pos = mapping.GetWord("offOrbitalPos");
          var suffix = pos <= 180 ? "E" : "W";
          if (pos >= 180 && pos <= 360)
            pos = (ushort)(360 - pos);
          s.OrbitalPosition = $"{pos / 10}.{pos % 10}{suffix}";
          this.DataRoot.AddSatellite(s);
        }

        mapping.BaseOffset += recordSize;
      }
    }
    #endregion

    #region LoadTransponder()

    private void LoadTransponder()
    {
      var num = sec.GetInt("numTransponder");
      var offBitmap = sec.GetInt("offTransponderBitmap");

      mapping.SetDataPtr(data, sec.GetInt("offTransponderData"));
      var recordSize = sec.GetInt("lenTransponderData");

      for (int i = 0; i < num; i++)
      {
        if ((data[offBitmap + i / 8] & (1 << (i & 0x07))) != 0)
        {
          var t = new Transponder(i);
          t.FrequencyInMhz = (decimal)mapping.GetDword("offFreq");
          if (!isDvbS)
            t.FrequencyInMhz /= 1000;
          t.SymbolRate = (int)mapping.GetWord("offSymRate");
          this.DataRoot.AddTransponder(null, t);
        }

        mapping.BaseOffset += recordSize;
      }
    }
    #endregion

    #region LoadChannels()

    private void LoadChannels()
    {
      var num = sec.GetInt("numChannel");
      var offBitmap = sec.GetInt("offChannelBitmap");

      mapping.SetDataPtr(data, sec.GetInt("offChannelData"));
      var recordSize = sec.GetInt("lenChannelData");

      var dec = new DvbStringDecoder(this.DefaultEncoding);

      for (int i = 0; i < num; i++)
      {
        if ((data[offBitmap + i / 8] & (1 << (i & 0x07))) != 0)
        {
          var serviceType = mapping.GetByte("offServiceType");
          var src = serviceType == 1 ? SignalSource.Tv : serviceType == 2 ? SignalSource.Radio : SignalSource.Data;
          src |= SignalSource.Digital;
          src |= isDvbS ? SignalSource.Sat : SignalSource.Cable;
          var c = new ChannelInfo(src, i, -1, null);
          dec.GetChannelNames(data, mapping.BaseOffset + sec.GetInt("offName"), sec.GetInt("lenName"), out var longName, out var shortName);
          c.Name = longName;
          c.ShortName = shortName;
          c.OldProgramNr = mapping.GetWord("offProgNr") + 1;
          c.OriginalNetworkId = mapping.GetWord("offOnid");
          c.TransportStreamId = mapping.GetWord("offTsid");
          c.ServiceId = mapping.GetWord("offSid");
          c.PcrPid = mapping.GetWord("offPcrPid");
          c.VideoPid = mapping.GetWord("offVideoPid");
          c.Hidden = mapping.GetFlag("Hide", false);
          c.Skip = mapping.GetFlag("Skip", false);
          c.Lock = mapping.GetFlag("Lock", false);
          c.ServiceTypeName = serviceType == 1 ? "TV" : serviceType == 2 ? "Radio" : "";

          var fav = mapping.GetByte("offFavorites");
          fav = (byte)(((fav >> 1) & 0x0E) | (fav & 0x01)); // A=1, B=4, C=8, D=16
          c.Favorites = (Favorites)fav;

          var transpIdx = isDvbS ? mapping.GetWord("offTransponderIndex") : mapping.GetByte("offTransponderIndex");
          var tp = this.DataRoot.Transponder.TryGet(transpIdx);
          if (tp != null)
          {
            c.FreqInMhz = tp.FrequencyInMhz;
            c.SymbolRate = tp.SymbolRate;
          }

          if (isDvbS && this.DataRoot.Satellites.TryGetValue(mapping.GetWord("offSatelliteIndex"), out var sat))
          {
            c.Satellite = sat.Name;
            c.SatPosition = sat.OrbitalPosition;
          }

          this.DataRoot.AddChannel(this.allChannels, c);
        }

        mapping.BaseOffset += recordSize;
      }
    }
    #endregion

    // saving

    #region Save()
    public override void Save()
    {
      var baseOffset = sec.GetInt("offChannelData");
      var recordSize = sec.GetInt("lenChannelData");

      foreach (var chan in this.allChannels.Channels)
      {
        if (chan.IsProxy) 
          continue;
        mapping.BaseOffset = baseOffset + (int)chan.RecordIndex * recordSize;
        mapping.SetWord("offProgNr", chan.NewProgramNr - 1);
        mapping.SetFlag("Skip", chan.Skip);
        mapping.SetFlag("Lock", chan.Lock);
        mapping.SetFlag("Hide", chan.Hidden);
        var fav = (int)mapping.GetByte("offFavorites");
        fav &= ~0x1D;
        var newFav = (int)chan.Favorites;
        fav |= (byte)((newFav & 0x01) | ((newFav & 0xFE) << 1)); // A=1, B=4, C=8, D=16
        mapping.SetByte("offFavorites", fav);
      }

      mapping.BaseOffset = 0;
      var calculatedChecksum = CalcChecksum(data, sec.GetInt("offData"), (int)mapping.GetDword("offDataLength"));
      mapping.SetWord("offChecksum", calculatedChecksum);
      File.WriteAllBytes(this.FileName, this.data);
    }
    #endregion

    // common

    #region CalcChecksum
    /// <summary>
    /// The checksum is the byte sum over the data bytes (offset 6 to end-2) plus 0x55 added to it
    /// </summary>
    public ushort CalcChecksum(byte[] bytes, int start, int count)
    {
      var sum = 0x55u;
      while (count-- > 0)
        sum += bytes[start++];
      return (ushort)sum;
    }
    #endregion

    // framework support methods

    #region GetFileInformation
    public override string GetFileInformation()
    {
      return base.GetFileInformation() + this.logMessages.Replace("\n", "\r\n");
    }
    #endregion
  }
}
