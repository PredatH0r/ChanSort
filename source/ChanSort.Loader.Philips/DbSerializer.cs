using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
{
  /*
   * This serializer is used for the channel list format with a Repair\ folder containing files like channel_db_ver.db, mgr_chan_s_fta.db, ...
   * The .db files are proprietary binary files, not SQLite databases.
   * Due to lack of sample lists, the analog and DVB-C files have not been reverse engineered yet.
   * The data offsets are defined in ChanSort.Loader.Philips.ini
   */
  class DbSerializer : SerializerBase
  {
    private readonly IniFile ini;

    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbT, "DVB-C");
    private readonly ChannelList dvbsFtaChannels = new ChannelList(SignalSource.DvbS | SignalSource.Provider0, "DVB-S FTA");
    private readonly ChannelList dvbsPkgChannels = new ChannelList(SignalSource.DvbS | SignalSource.Provider1, "DVB-S Preset");
    private readonly Dictionary<ChannelList, string> fileByList = new();


    public DbSerializer(string inputFile) : base(inputFile)
    {
      this.Features.MaxFavoriteLists = 1;
      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      this.Features.DeleteMode = DeleteMode.NotSupported;
      this.Features.CanHaveGaps = true; // the mgr_chan_s_pkg can have gaps

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);

      this.DataRoot.AddChannelList(dvbtChannels);
      this.DataRoot.AddChannelList(dvbcChannels);
      this.DataRoot.AddChannelList(dvbsFtaChannels);
      this.DataRoot.AddChannelList(dvbsPkgChannels);
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames = new List<string>
        {
          "Position", //nameof(Channel.NewProgramNr),
          "OldPosition", // nameof(Channel.OldProgramNr),
          nameof(Channel.Name),
          nameof(Channel.Favorites),
          nameof(Channel.FreqInMhz),
          nameof(Channel.SymbolRate),
          nameof(Channel.TransportStreamId),
          nameof(Channel.OriginalNetworkId),
          nameof(Channel.ServiceId)
        };
      }
    }

    #region Load()
    public override void Load()
    {
      bool validList = false;

      foreach (var file in Directory.GetFiles(Path.GetDirectoryName(this.FileName) ?? ""))
      {
        var lc = Path.GetFileName(file).ToLowerInvariant();
        switch (lc)
        {
          case "atv_channel_t.db":
            // TODO: no sample file yet that contains analog terrestrial channels
            break;
          case "atv_channel_c.db":
            // TODO: no sample file yet that contains analog cable channels
            break;
          case "channel_db_ver.db":
            LoadVersion(file);
            break;
          case "mgr_chan_dvbt.db":
            LoadDvb(file, lc, dvbtChannels);
            validList = true;
            break;
          case "mgr_chan_dvbc.db":
            // no sample file with DVB-C data yet, so this here is a guess based on DVB-T
            LoadDvb(file, lc, dvbcChannels);
            validList = true;
            break;
          case "mgr_chan_s_fta.db":
            LoadDvb(file, lc, dvbsFtaChannels);
            validList = true;
            break;
          case "mgr_chan_s_pkg.db":
            LoadDvb(file, lc, dvbsPkgChannels);
            validList = true;
            break;
        }
      }

      if (!validList)
        throw new FileLoadException(this.FileName + " is not a supported Philips Repair/channel_db_ver.db channel list");
    }
    #endregion

    #region LoadVersion()
    private void LoadVersion(string file)
    {
      var data = File.ReadAllBytes(file);
      this.FileFormatVersion = "FLASH/.db";
      if (data.Length >= 2)
        this.FileFormatVersion += " " + BitConverter.ToInt16(data, 0);
      if (data.Length >= 3)
        this.FileFormatVersion += $"-{data[2]:D2}";
      if (data.Length >= 4)
        this.FileFormatVersion += $"-{data[3]:D2}";
      if (data.Length >= 5)
        this.FileFormatVersion += $" {data[4]:D2}";
      if (data.Length >= 6)
        this.FileFormatVersion += $":{data[5]:D2}";
      if (data.Length >= 7)
        this.FileFormatVersion += $":{data[6]:D2}";

      // Philips doesn't export any information about the TV model in this format. For automated stats I manually place modelinfo.txt files in the folders
      for (var dir = Path.GetDirectoryName(file); dir != null; dir = Path.GetDirectoryName(dir))
      {
        var path = Path.Combine(dir, "modelinfo.txt");
        if (File.Exists(path))
        {
          this.TvModelName = File.ReadAllText(path);
          break;
        }
      }
    }
    #endregion

    #region LoadDvbs()
    private void LoadDvb(string path, string sectionName, ChannelList list)
    {
      var signalSource = list.SignalSource;
      var data = File.ReadAllBytes(path);

      var sec = ini.GetSection(sectionName);
      var lenHeader = sec.GetInt("lenHeader");
      var lenFooter = sec.GetInt("lenFooter");
      var lenEntry = sec.GetInt("lenEntry");
      var offFooterChecksum = sec.GetInt("offFooterChecksum");

      var records = (data.Length - lenHeader - lenFooter) / lenEntry;
      if (records <= 0)
        return;

      list.ReadOnly = !sec.GetBool("allowEdit", false);

      var mapping = new DataMapping(this.ini.GetSection(sectionName + "_entry"));
      sec = ini.GetSection("mgr_chan_s_fta.db_entry");
      var lenName = sec.GetInt("lenName");
      for (int i = 0; i < records; i++)
      {
        mapping.SetDataPtr(data, lenHeader + i * lenEntry);
        var oldProgNr = mapping.GetWord("offProgNr");

        // name can be either an 8-bit ASCII with unspecified encoding or big-endian 16-bit unicode
        var off = mapping.BaseOffset + mapping.GetOffsets("offName")[0];
        var name = data[off + 0] == 0 ? (data[off + 1] == 0 ? "" : Encoding.BigEndianUnicode.GetString(data, off, lenName)) : DefaultEncoding.GetString(data, off, lenName);
        name = name.TrimEnd('\0');

        var ch = new Channel(signalSource, i, oldProgNr, name);
        ch.RecordOrder = i;
        var favPos = mapping.GetWord("offFav");
        if (favPos > 0)
          ch.SetOldPosition(1, favPos);
        ch.SymbolRate = mapping.GetWord("offSymbolRate");
        ch.FreqInMhz = mapping.GetDword("offFreq");
        if (ch.FreqInMhz > 13000) // DVB-S stores value in MHz, DVB-T in Hz
          ch.FreqInMhz /= 1000;
        if (ch.FreqInMhz > 13000)
          ch.FreqInMhz /= 1000;
        ch.TransportStreamId = mapping.GetWord("offTsid");
        ch.OriginalNetworkId = mapping.GetWord("offOnid");
        ch.ServiceId = mapping.GetWord("offSid");
        this.DataRoot.AddChannel(list, ch);
      }

      var offChecksum = data.Length - lenFooter + offFooterChecksum;
      var expectedChecksum = BitConverter.ToUInt16(data, offChecksum);
      var actualChecksum = CalcChecksum(data, 0, offChecksum);
      if (actualChecksum != expectedChecksum)
        throw new FileLoadException($"File {path} contains invalid checksum. Expected {expectedChecksum:x4} but calculated {actualChecksum:x4}");

      this.fileByList[list] = path;
    }
    #endregion

    #region CalcChecksum()

    /// <summary>
    /// The checksum is the 16-bit sum over the byte-values in the file data from offset 0 to right before the checksum field
    /// </summary>
    private ushort CalcChecksum(byte[] data, int start, int len)
    {
      ushort checksum = 0;
      while (len > 0)
      {
        checksum += data[start++];
        --len;
      }

      return checksum;
    }
    #endregion

    public override IEnumerable<string> GetDataFilePaths() => this.fileByList.Values.ToList();

    #region Save()

    public override void Save(string tvOutputFile)
    {
      foreach (var listAndFile in this.fileByList)
      {
        var list = listAndFile.Key;
        var file = listAndFile.Value;
        var secName = Path.GetFileName(file).ToLowerInvariant();
        SaveDvb(file, secName, list);
      }
    }

    private void SaveDvb(string file, string secName, ChannelList list)
    {
      var data = File.ReadAllBytes(file);

      var sec = ini.GetSection(secName);
      var lenHeader = sec.GetInt("lenHeader");
      var lenFooter = sec.GetInt("lenFooter");
      var lenEntry = sec.GetInt("lenEntry");
      var offFooterChecksum = sec.GetInt("offFooterChecksum");

      var mapping = new DataMapping(ini.GetSection(secName + "_entry"));

      if (sec.GetBool("reorderRecordsByChannelNumber"))
      {
        // physically reorder channels
        var newData = new byte[data.Length];
        Array.Copy(data, newData, lenHeader);
        var off = lenHeader + lenEntry * list.Channels.Count;
        Array.Copy(data, off, newData, off, lenFooter);

        int i = 0;
        foreach (var ch in list.Channels.OrderBy(c => c.NewProgramNr))
        {
          off = lenHeader + i * lenEntry;
          Array.Copy(data, lenHeader + ch.RecordOrder * lenEntry, newData, off, lenEntry);
          mapping.SetDataPtr(newData, off);
          mapping.SetWord("offProgNr", ch.NewProgramNr);
          //mapping.SetWord("offPrevProgNr", ch.NewProgramNr - 1);
          mapping.SetWord("offFav", Math.Max(0, ch.GetPosition(1)));
          ch.RecordOrder = i;
          ++i;
        }

        data = newData;
      }
      else
      {
        // update channel data
        foreach (var ch in list.Channels)
        {
          mapping.SetDataPtr(data, lenHeader + ch.RecordOrder * lenEntry);
          mapping.SetWord("offProgNr", ch.NewProgramNr);
          //mapping.SetWord("offPrevProgNr", ch.NewProgramNr - 1);
          mapping.SetWord("offFav", Math.Max(0, ch.GetPosition(1)));
        }
      }

      // update checksum
      var offChecksum = data.Length - lenFooter + offFooterChecksum;
      var checksum = CalcChecksum(data, 0, offChecksum);
      data[offChecksum] = (byte)checksum;
      data[offChecksum + 1] = (byte)(checksum >> 8);

      File.WriteAllBytes(file, data);
    }
    #endregion
  }
}
