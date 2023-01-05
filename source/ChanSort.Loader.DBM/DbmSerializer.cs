using System;
using System.IO;
using System.Reflection;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.DBM
{
  /*


  */
  public class DbmSerializer : SerializerBase
  {
    private readonly IniFile ini;
    private IniFile.Section sec;
    private DataMapping mapping;
    private readonly ChannelList allChannels = new ChannelList(SignalSource.All, "All");

    private byte[] data;
    private int fileSize;

    private readonly StringBuilder logMessages = new StringBuilder();
   

    #region ctor()
    public DbmSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = false;
      this.Features.CanHideChannels = false;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanHaveGaps = false;
      this.Features.FavoritesMode = FavoritesMode.None;
      this.Features.MaxFavoriteLists = 0;
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
        list.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.ServiceTypeName));
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
      this.fileSize = (int)info.Length;

      this.sec = ini.GetSection("dbm:" + this.fileSize);
      if (sec == null)
        throw LoaderException.Fail($"No configuration for .DBM files with size {info.Length} in .ini file");

      if (!sec.GetBool("isDvbS"))
        allChannels.VisibleColumnFieldNames.Remove(nameof(ChannelInfo.Satellite));

      this.data = File.ReadAllBytes(this.FileName);
      this.mapping = new DataMapping(sec);
      this.mapping.SetDataPtr(data, 0);

      ValidateChecksum(data, sec);

      LoadTransponder(sec, data);
      LoadChannels(sec, data);
    }
    #endregion

    #region ValidateChecksum()
    private void ValidateChecksum(byte[] data, IniFile.Section sec)
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

    #region LoadTransponder()

    private void LoadTransponder(IniFile.Section sec, byte[] data)
    {
      var num = sec.GetInt("numTransponder");
      var offBitmap = sec.GetInt("offTransponderBitmap");

      var map = new DataMapping(ini.GetSection("transponder:" + this.fileSize));
      map.SetDataPtr(data, sec.GetInt("offTransponderData"));
      var recordSize = sec.GetInt("lenTransponderData");

      for (int i = 0; i < num; i++)
      {
        if ((data[offBitmap + i / 8] & (1 << (i & 0x07))) != 0)
        {
          var t = new Transponder(i);
          t.FrequencyInMhz = (decimal)map.GetDword("offFreq") / 1000;
          t.SymbolRate = (int)map.GetDword("offSymRate");
          this.DataRoot.AddTransponder(null, t);
        }

        map.BaseOffset += recordSize;
      }
    }
    #endregion

    #region LoadChannels()

    private void LoadChannels(IniFile.Section sec, byte[] data)
    {
      var num = sec.GetInt("numChannel");
      var offBitmap = sec.GetInt("offChannelBitmap");

      var sec2 = ini.GetSection("channel:" + this.fileSize);
      var map = new DataMapping(sec2);
      map.SetDataPtr(data, sec.GetInt("offChannelData"));
      var recordSize = sec.GetInt("lenChannelData");

      var dec = new DvbStringDecoder(this.DefaultEncoding);

      for (int i = 0; i < num; i++)
      {
        if ((data[offBitmap + i / 8] & (1 << (i & 0x07))) != 0)
        {
          var c = new ChannelInfo(SignalSource.Any, i, -1, null);
          dec.GetChannelNames(data, map.BaseOffset + sec2.GetInt("offName"), sec2.GetInt("lenName"), out var longName, out var shortName);
          c.Name = longName;
          c.ShortName = shortName;
          c.OldProgramNr = map.GetWord("offProgNr") + 1;
          c.OriginalNetworkId = map.GetWord("offOnid");
          c.TransportStreamId = map.GetWord("offTsid");
          c.ServiceId = map.GetWord("offSid");
          c.PcrPid = map.GetWord("offPcrPid");
          c.VideoPid = map.GetWord("offVideoPid");

          var transpIdx = map.GetByte("offTransponderIndex");
          var tp = this.DataRoot.Transponder.TryGet(transpIdx);
          if (tp != null)
          {
            c.FreqInMhz = tp.FrequencyInMhz;
            c.SymbolRate = tp.SymbolRate;
          }

          this.DataRoot.AddChannel(this.allChannels, c);
        }

        map.BaseOffset += recordSize;
      }
    }
    #endregion

    // saving

    #region Save()
    public override void Save()
    {
      var sec2 = ini.GetSection("channel:" + this.fileSize);
      var map = new DataMapping(sec2);
      var baseOffset = sec.GetInt("offChannelData");
      map.SetDataPtr(data, baseOffset);
      var recordSize = sec.GetInt("lenChannelData");

      foreach (var chan in this.allChannels.Channels)
      {
        if (chan.IsProxy) continue;
        map.BaseOffset = baseOffset + (int)chan.RecordIndex * recordSize;
        map.SetWord("offProgNr", chan.NewProgramNr - 1);
      }

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
