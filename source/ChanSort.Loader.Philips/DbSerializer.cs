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
   * So far only the mgr_chan_s_fta.db file holing DVB-S channels is reverse engineered, the offsets are defined in PChanSort.Loader.Philips.ini
   */
  class DbSerializer : SerializerBase
  {
    private readonly IniFile ini;
    private readonly List<string> dataFilePaths = new List<string>();

    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS, "DVB-S");


    public DbSerializer(string inputFile) : base(inputFile)
    {
      this.Features.MaxFavoriteLists = 1;
      this.Features.FavoritesMode = FavoritesMode.OrderedPerSource;
      this.Features.DeleteMode = DeleteMode.NotSupported;

      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      this.ini = new IniFile(iniFile);

      this.DataRoot.AddChannelList(dvbsChannels);
      dvbsChannels.VisibleColumnFieldNames = new List<string>
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

    #region Load()
    public override void Load()
    {
      bool validList = false;

      foreach (var file in Directory.GetFiles(Path.GetDirectoryName(this.FileName)))
      {
        var lc = Path.GetFileName(file).ToLowerInvariant();
        switch (lc)
        {
          case "mgr_chan_s_fta.db":
            LoadDvbs(file);
            validList = true;
            break;
        }
      }

      if (!validList)
        throw new FileLoadException(this.FileName + " is not a supported Philips Repair/mgr_chan_s_fta.db channel list");
    }

    private void LoadDvbs(string file)
    {
      var data = File.ReadAllBytes(file);

      var sec = ini.GetSection("mgr_chan_s_fta.db");
      var lenHeader = sec.GetInt("lenHeader");
      var lenFooter = sec.GetInt("lenFooter");
      var lenEntry = sec.GetInt("lenEntry");
      var offFooterChecksum = sec.GetInt("offFooterChecksum");

      var records = (data.Length - lenHeader - lenFooter) / lenEntry;
      if (records <= 0)
        throw new FileLoadException("Currently only DVB-S lists are supported and mgr_chan_s_fta.db contains no channels.");

      var mapping = new DataMapping(this.ini.GetSection("mgr_chan_s_fta.db_entry"));
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

        var ch = new Channel(SignalSource.DvbS, i, oldProgNr, name);
        ch.RecordOrder = i;
        var favPos = mapping.GetWord("offFav");
        if (favPos > 0)
          ch.SetOldPosition(1, favPos);
        ch.SymbolRate = mapping.GetWord("offSymbolRate");
        ch.FreqInMhz = mapping.GetWord("offFreq");
        ch.TransportStreamId = mapping.GetWord("offTsid");
        ch.OriginalNetworkId = mapping.GetWord("offOnid");
        ch.ServiceId = mapping.GetWord("offSid");
        this.DataRoot.AddChannel(dvbsChannels, ch);
      }

      var offChecksum = data.Length - lenFooter + offFooterChecksum;
      var expectedChecksum = BitConverter.ToUInt16(data, offChecksum);
      var actualChecksum = CalcChecksum(data, 0, offChecksum);
      if (actualChecksum != expectedChecksum)
        throw new FileLoadException($"File {file} contains invalid checksum. Expected {expectedChecksum:x4} but calculated {actualChecksum:x4}");

      this.dataFilePaths.Add(file);
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

    public override IEnumerable<string> GetDataFilePaths() => this.dataFilePaths.ToList();

    #region Save()

    public override void Save(string tvOutputFile)
    {
      foreach (var file in this.dataFilePaths)
      {
        var lc = Path.GetFileName(file).ToLowerInvariant();
        switch (lc)
        {
          case "mgr_chan_s_fta.db":
            SaveDvbs(file);
            break;
        }
      }
    }

    private void SaveDvbs(string file)
    {
      var data = File.ReadAllBytes(file);

      var sec = ini.GetSection("mgr_chan_s_fta.db");
      var lenHeader = sec.GetInt("lenHeader");
      var lenFooter = sec.GetInt("lenFooter");
      var lenEntry = sec.GetInt("lenEntry");
      var offFooterChecksum = sec.GetInt("offFooterChecksum");

      var mapping = new DataMapping(ini.GetSection("mgr_chan_s_fta.db_entry"));

      // update channel data
      foreach (var ch in dvbsChannels.Channels)
      {
        mapping.SetDataPtr(data, lenHeader + ch.RecordOrder * lenEntry);
        mapping.SetWord("offProgNr", ch.NewProgramNr);
        mapping.SetWord("offPrevProgNr", ch.NewProgramNr - 1);
        mapping.SetWord("offFav", Math.Max(0, ch.GetPosition(1)));
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
