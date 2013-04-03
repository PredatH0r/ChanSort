#define SYMBOL_RATE_ROUNDING
#undef STORE_DVBS_CHANNELS_IN_DATABASE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;

namespace ChanSort.Loader.TllFile
{
  public partial class TllFileSerializer : SerializerBase
  {
    private const long MaxFileSize = 2000000;
    private readonly string ERR_fileTooBig = Resource.TllFileSerializerPlugin_ERR_fileTooBig;
    private readonly string ERR_modelUnknown = Resource.TllFileSerializerPlugin_ERR_modelUnknown;
    private readonly string ERR_wrongChecksum = Resource.TllFileSerializer_ERR_wrongChecksum;
    private readonly string ERR_dupeChannel = Resource.TllFileSerializer_ERR_dupeChannel;

    private readonly MappingPool<DataMapping> actMappings = new MappingPool<DataMapping>("Analog and DVB-C/T");
    private readonly MappingPool<DataMapping> dvbsMappings = new MappingPool<DataMapping>("DVB-S");
    private readonly MappingPool<FirmwareData> firmwareMappings = new MappingPool<FirmwareData>("Firmware");
    private readonly Dictionary<int, DvbsDataLayout> satConfigs = new Dictionary<int, DvbsDataLayout>();

    private byte[] fileContent;

    private int analogBlockOffset;
    private int firmwareBlockOffset;
    private int dvbctBlockOffset;
    private int dvbsBlockOffset;
    private int[] dvbsSubblockCrcOffset;
    private int settingsBlockOffset;

    private int actChannelSize;
    private bool reorderPhysically;

    private int analogChannelCount;
    private int dvbctChannelCount;
    private int dvbsChannelCount;

    private DvbsDataLayout satConfig;
    private bool isDvbsSymbolRateDiv2;

    private Dictionary<int, int> nextChannelIndex;
    private int firmwareBlockSize;
    private int dvbsBlockSize;
    private int settingsBlockSize;
    private string countryCode;

    private int duplicateChannels;
    private int deletedChannelsHard;
    private int deletedChannelsSoft;
    private int dvbsChannelsAtPr0;

    private bool removeDeletedActChannels = false;

    #region ctor()
    public TllFileSerializer(string inputFile) : base(inputFile)
    {

      this.Features.ChannelNameEdit = true;
      this.Features.EraseChannelData = true;
      this.Features.FileInformation = true;
      this.Features.DeviceSettings = true;
      this.SupportedTvCountryCodes = new List<string>
                                       {
                                         "___ (None)", "AUT (Austria)", "BEL (Belgium)", "CHE (Switzerland)", 
                                         "DEU (Germany)", "ESP (Spain)", "FRA (France)", "GBR (Great Britain)",
                                         "GRC (Greece)", "IRL (Ireland)", "ITA (Italy)", "LUX (Luxembourg)", 
                                         "NLD (Netherlands)", "PRT (Portugal)", "SVN (Slovenia)"
                                       };

      this.ReadConfigurationFromIniFile();
    }
    #endregion

    public IList<string> SupportedTvCountryCodes { get; private set; }

    #region ReadConfigurationFromIniFile()

    private void ReadConfigurationFromIniFile()
    {
      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      IniFile ini = new IniFile(iniFile);
      foreach (var section in ini.Sections)
      {
        int idx = section.Name.IndexOf(":");
        int recordLength = idx < 0 ? 0 : int.Parse(section.Name.Substring(idx + 1));
        if (section.Name.StartsWith("DvbsBlock"))
          this.satConfigs.Add(recordLength, new DvbsDataLayout(section));
        else if (section.Name.StartsWith("ACTChannelDataMapping"))
          actMappings.AddMapping(recordLength, new DataMapping(section));
        else if (section.Name.StartsWith("SatChannelDataMapping"))
          dvbsMappings.AddMapping(recordLength, new DataMapping(section));
        else if (section.Name.StartsWith("FirmwareData"))
          firmwareMappings.AddMapping(recordLength, new FirmwareData(section));
      }
    }
    #endregion


    #region DisplayName
    public override string DisplayName { get { return "TLL loader"; } }
    #endregion

    #region Load()

    public override void Load()
    {
      long fileSize = new FileInfo(this.FileName).Length;
      if (fileSize > MaxFileSize)
        throw new InvalidOperationException(string.Format(ERR_fileTooBig, fileSize, MaxFileSize));

      this.fileContent = File.ReadAllBytes(this.FileName);
      int off = 0;

      this.ReadFileHeader(ref off);
      this.ReadAnalogChannelBlock(ref off);
      this.ReadFirmwareDataBlock(ref off);
      this.ReadDvbCtChannels(ref off);
      this.ReadDvbSBlock(ref off);
      this.ReadSettingsBlock(ref off);

#if STORE_DVBS_CHANNELS_IN_DATABASE
      this.StoreToDatabase();
#endif
    }

    #endregion

    #region ReadFileHeader()
    private void ReadFileHeader(ref int off)
    {
      if (fileContent.Length < 4)
        throw new InvalidOperationException(ERR_modelUnknown);
      if (BitConverter.ToUInt32(fileContent, off) == 0x5A5A5A5A)
        off += 4;
    }
    #endregion

    #region ReadAnalogChannelBlock()

    private void ReadAnalogChannelBlock(ref int off)
    {
      this.analogBlockOffset = off;
      this.ReadActChannelBlock(ref off, out analogChannelCount, out actChannelSize, 
        (slot, data) => new AnalogChannel(slot, data));
    }
    #endregion

    #region ReadFirmwareDataBlock()
    private void ReadFirmwareDataBlock(ref int off)
    {
      this.firmwareBlockOffset = off;
      this.firmwareBlockSize = this.GetBlockSize(off);
      off += 4 + this.firmwareBlockSize;
    }
    #endregion

    #region ReadDvbCtChannels()
    private void ReadDvbCtChannels(ref int off)
    {
      this.dvbctBlockOffset = off;
      this.ReadActChannelBlock(ref off, out dvbctChannelCount, out actChannelSize,
        (slot, data) => new DtvChannel(slot, data));
    }
    #endregion

    #region ReadDvbSBlock()
    private void ReadDvbSBlock(ref int off)
    {
      int blockSize;
      if (!IsDvbsBlock(off, out blockSize))
        return;

      this.dvbsBlockSize = blockSize;
      this.dvbsBlockOffset = off;
      off += 4;

      this.satConfig = this.satConfigs.TryGet(blockSize);
      if (satConfig != null)
        this.ReadDvbsSubblocks(ref off);
      else
      {
        this.DataRoot.Warnings.AppendFormat("DVB-S data format is not supported (size={0})\n", blockSize);
        off += blockSize;
      }
    }
    #endregion

    #region ReadSettingsBlock()
    private void ReadSettingsBlock(ref int off)
    {
      this.settingsBlockOffset = off;
      if (this.settingsBlockOffset >= fileContent.Length)
      {
        this.settingsBlockOffset = 0;
        return;
      }

      this.settingsBlockSize = this.GetBlockSize(off);
      off += 4;
      if (settingsBlockSize >= 8)
      {
        StringBuilder code = new StringBuilder();
        for (int i = 6; i >= 4; i--)
          code.Append((char)fileContent[off + i]);
        this.countryCode = code.ToString();
      }
      off += settingsBlockSize;
    }
    #endregion


    #region ReadActChannelBlock()
    private void ReadActChannelBlock(ref int off, out int channelCount, out int recordSize,
      Func<int, DataMapping, ChannelInfo> channelFactory)
    {
      recordSize = 0;
      int blockSize = this.GetBlockSize(off, minSize: 2);
      off += 4;
      channelCount = BitConverter.ToInt32(fileContent, off);
      off += 4;
      if (channelCount == 0) return;

      recordSize = GetActChannelRecordSize(off, blockSize, channelCount);
      var actMapping = this.actMappings.GetMapping(recordSize);
      this.reorderPhysically = actMapping.Settings.GetInt("reorderChannelData") != 0;

      for (int i = 0; i < channelCount; i++)
      {
        actMapping.SetDataPtr(fileContent, off);
        ChannelInfo ci = channelFactory(i, actMapping);

        var list = this.DataRoot.GetChannelList(ci.SignalSource, ci.SignalType, true);
        this.DataRoot.AddChannel(list, ci);

        off += recordSize;
      }
    }
    #endregion

    #region GetBlockSize()
    private int GetBlockSize(int off, int minSize = 0)
    {
      long len = BitConverter.ToUInt32(fileContent, off);
      if (len < minSize || off + 4 + len > fileContent.Length)
        throw new InvalidOperationException(ERR_modelUnknown);
      return (int)len;
    }
    #endregion

    #region GetActChannelRecordSize()
    private int GetActChannelRecordSize(int off, int blockSize, int channelCount)
    {
      if ((blockSize - 4) % channelCount != 0)
        throw new InvalidOperationException(ERR_modelUnknown);
      int recordSize = (blockSize - 4) / channelCount;
      if (off + channelCount * recordSize > fileContent.Length)
        throw new InvalidOperationException(ERR_modelUnknown);
      return recordSize;
    }
    #endregion


    #region IsDvbsBlock()
    private bool IsDvbsBlock(int off, out int blockSize)
    {
      blockSize = 0;
      if (off >= fileContent.Length)
        return false;
      blockSize = this.GetBlockSize(off);
      if (blockSize < 12)
        return false;
      ulong blockId = BitConverter.ToUInt64(fileContent, off + 8);
      if (blockId != 0x0032532D53425644) // reverse "DVBS-S2\0"
        return false;
      return true;
    }
    #endregion

    #region ReadDvbsSubblocks()
    private void ReadDvbsSubblocks(ref int off)
    {
      this.ScanDvbSSubBlockChecksums(off);

      // subblock 1 (DVBS header)
      off += 16;
      
      // subblock 2 (satellites)
      off += 84; // irrelevant data
      this.ReadSatellites(ref off);

      // subblock 3 (transponder)
      off += satConfig.sizeOfTransponderBlockHeader;
      this.ReadTransponderData(ref off);

      // subblock 4 (channels)
      SatChannelListHeader header = new SatChannelListHeader(fileContent, off);
      this.dvbsChannelCount = header.ChannelCount;
      off += header.Size;
      off += satConfig.dvbsMaxChannelCount/8; // skip allocation bitmap
      this.ReadDvbsChannelLinkedList(ref off);

      this.ReadDvbSChannels(ref off, header.LinkedListStartIndex);

      // subblock 5 (satellite/LNB config)
      off += satConfig.LnbBlockHeaderSize + satConfig.lnbCount*satConfig.lnbLength;
    }
    #endregion

    #region ScanDvbSSubBlockChecksums()
    private void ScanDvbSSubBlockChecksums(int off)
    {      
      this.dvbsSubblockCrcOffset = new int[satConfig.dvbsSubblockLength.Length];
      for (int i = 0; i < dvbsSubblockCrcOffset.Length; i++)
      {
        this.dvbsSubblockCrcOffset[i] = off;
        int subblockLength = satConfig.dvbsSubblockLength[i];
        uint fileCrc = BitConverter.ToUInt32(fileContent, off);
        uint calcCrc = Crc32.CalcCrc32(fileContent, off + 4, subblockLength);
        if (fileCrc != calcCrc)
          throw new IOException(string.Format(ERR_wrongChecksum, calcCrc, fileCrc));
        off += 4 + subblockLength;
      }
    }
    #endregion

    #region ReadSatellites()
    private void ReadSatellites(ref int off)
    {
      for (int i = 0; i < satConfig.satCount; i++)
      {
        Satellite sat = new Satellite(i);
        string satName = Encoding.ASCII.GetString(fileContent, off + 0, 32).TrimEnd('\0');
        sat.Name = satName;
        sat.OrbitalPosition = GetSatLocation(fileContent[off + 32], fileContent[off + 33]);
        this.DataRoot.AddSatellite(sat);
        off += satConfig.satLength;
      }
    }
    #endregion

    #region ReadTransponderData()
    private void ReadTransponderData(ref int off)
    {
      var data = new SatTransponder(fileContent);
      data.BaseOffset = off;
      for (int i=0; i<satConfig.transponderCount; i++)
      {
        if (data.SatIndex == 0xFF)
          continue;
#if SYMBOL_RATE_ROUNDING
        ushort sr = (ushort)(data.SymbolRate & 0x7FFF);
        if (sr % 100 >= 95)
          data.SymbolRate = (ushort)((data.SymbolRate & 0x8000) | ((sr / 100 + 1) * 100));
#endif

        Transponder transponder = new Transponder(i);
        transponder.FrequencyInMhz = data.Frequency;
        transponder.OriginalNetworkId = data.OriginalNetworkId;
        transponder.TransportStreamId = data.TransportStreamId;
        transponder.SymbolRate = data.SymbolRate & 0x7FFF;
        if (data.SymbolRate == 11000)
          this.isDvbsSymbolRateDiv2 = true;

        var sat = this.DataRoot.Satellites.TryGet(data.SatIndex/2);
        this.DataRoot.AddTransponder(sat, transponder);

        data.BaseOffset += satConfig.transponderLength;
      }

      if (this.isDvbsSymbolRateDiv2)
      {
        foreach (var transponder in this.DataRoot.Transponder.Values)
          transponder.SymbolRate *= 2;
      }

      off += this.satConfig.transponderCount * this.satConfig.transponderLength;
    }
    #endregion

    #region ReadDvbsChannelLinkedList()
    private void ReadDvbsChannelLinkedList(ref int off)
    {
      this.nextChannelIndex = new Dictionary<int, int>();
      for (int i = 0; i < satConfig.dvbsMaxChannelCount; i++)
      {
        int offEntry = off + i*satConfig.sizeOfChannelLinkedListEntry;
        int cur = BitConverter.ToUInt16(fileContent, offEntry + 4);
        if (cur != i)
          break;
        this.nextChannelIndex.Add(cur, BitConverter.ToUInt16(fileContent, offEntry + 2));
      }
      off += satConfig.dvbsMaxChannelCount*satConfig.sizeOfChannelLinkedListEntry;
    }
    #endregion

    #region ReadDvbSChannels()
    private void ReadDvbSChannels(ref int off, int startIndex)
    {
      var mapping = this.dvbsMappings.GetMapping(satConfig.dvbsChannelLength);
      int index = startIndex;
      for (int i = 0; i < this.dvbsChannelCount; i++)
      {
        int recordOffset = off + index*satConfig.dvbsChannelLength;
        mapping.SetDataPtr(fileContent, recordOffset);
        SatChannel ci = new SatChannel(i, index, mapping, this.DataRoot);
        if (!ci.InUse)
          ++this.deletedChannelsHard;
        else if (ci.IsDeleted)
          ++this.deletedChannelsSoft;
        else
        {
          var list = this.DataRoot.GetChannelList(ci.SignalSource, ci.SignalType, true);
          var dupes = list.GetChannelByUid(ci.Uid);
          if (dupes.Count == 0)
          {
            if (ci.OldProgramNr == 0)
              ++this.dvbsChannelsAtPr0;
            this.DataRoot.AddChannel(list, ci);
          }
          else
          {
            // duplicate channels (ONID,TSID,SSID) cause the TV to randomly reorder channels and show wrong ones in the 
            // program list, so we erase all dupes here
            this.DataRoot.Warnings.AppendFormat(ERR_dupeChannel, ci.RecordIndex, ci.OldProgramNr, dupes[0].RecordIndex,
                                                dupes[0].OldProgramNr, dupes[0].Name).AppendLine();
            this.EraseDuplicateDvbsChannel(recordOffset, satConfig);
            ++this.duplicateChannels;
          }
        }

        if (!this.nextChannelIndex.TryGetValue(index, out index) || index == -1)
          break;
      }
      off += satConfig.dvbsMaxChannelCount * satConfig.dvbsChannelLength;
    }
    #endregion

    #region EraseDuplicateDvbsChannel()
    private void EraseDuplicateDvbsChannel(int off, DvbsDataLayout c)
    {
      for (int i = 0; i < c.dvbsChannelLength; i++)
        fileContent[off++] = 0xFF;
    }
    #endregion

    #region GetSatLocation()
    private string GetSatLocation(byte degree, byte fractionAndOrientation)
    {
      return string.Format("{0}.{1}{2}", degree, fractionAndOrientation & 0x0f, fractionAndOrientation < 16 ? "W" : "E");
    }
    #endregion


    
    #region Save()
    public override void Save(string tvOutputFile, string csvOutputFile)
    {
      int newAnalogChannelCount;
      int newDvbctChannelCount;
      this.UpdateRawChannelData(out newAnalogChannelCount, out newDvbctChannelCount);

      if (!removeDeletedActChannels)
      {
        newAnalogChannelCount = this.analogChannelCount;
        newDvbctChannelCount = this.dvbctChannelCount;
      }

      if (this.reorderPhysically || this.removeDeletedActChannels)
        this.ReorderActChannelsPhysically();

      if (satConfig != null)
        this.UpdateDvbsChecksums();

      using (var file = new BinaryWriter(new FileStream(tvOutputFile, FileMode.Create, FileAccess.Write)))
      {
        // header
        file.Write(this.fileContent, 0, this.analogBlockOffset);

        // analog
        file.Write(newAnalogChannelCount*this.actChannelSize + 4);
        file.Write(newAnalogChannelCount);
        file.Write(fileContent, this.analogBlockOffset + 8, newAnalogChannelCount*this.actChannelSize);

        // firmware
        file.Write(fileContent, this.firmwareBlockOffset, this.firmwareBlockSize + 4);

        // DVB-CT
        file.Write(newDvbctChannelCount*this.actChannelSize + 4);
        file.Write(newDvbctChannelCount);
        file.Write(fileContent, this.dvbctBlockOffset + 8, newDvbctChannelCount * this.actChannelSize);

        // DVB-S
        if (this.dvbsBlockOffset != 0)
          file.Write(fileContent, this.dvbsBlockOffset, this.dvbsBlockSize + 4);

        // rest (including settings)
        file.Write(fileContent, this.settingsBlockOffset, fileContent.Length - this.settingsBlockOffset);                
      }
    }
    #endregion

    #region UpdateRawChannelData()
    private void UpdateRawChannelData(out int newAnalogChannelCount, out int newDvbctChannelCount)
    {
      newAnalogChannelCount = 0;
      newDvbctChannelCount = 0;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (TllChannelBase channel in list.Channels)
        {
          if (channel.NewProgramNr != 0)
          {
            if ((channel.SignalSource & SignalSource.Digital) == 0)
              ++newAnalogChannelCount;
            else if (channel.SignalSource != SignalSource.DvbS)
              ++newDvbctChannelCount;
          }
          channel.OldProgramNr = channel.NewProgramNr;
          channel.UpdateRawData();
        }
      }
    }
    #endregion

    #region ReorderActChannelsPhysically()
    private void ReorderActChannelsPhysically()
    {
      var analogTv = this.DataRoot.GetChannelList(SignalSource.AnalogCT, SignalType.Tv, false);
      var analogRadio = this.DataRoot.GetChannelList(SignalSource.AnalogCT, SignalType.Radio, false);
      var analog = analogTv.Channels.Union(analogRadio.Channels).ToList();
      this.ReorderChannelData(this.analogBlockOffset + 8, this.actChannelSize, this.analogChannelCount, analog);

      var dvbCtTv = this.DataRoot.GetChannelList(SignalSource.DvbCT, SignalType.Tv, false);
      var dvbCtRadio = this.DataRoot.GetChannelList(SignalSource.DvbCT, SignalType.Radio, false);
      var dvbCt = dvbCtTv.Channels.Union(dvbCtRadio.Channels).ToList();
      this.ReorderChannelData(this.dvbctBlockOffset + 8, this.actChannelSize, this.dvbctChannelCount, dvbCt);
    }
    #endregion

    
    #region ReorderChannelData()
    private void ReorderChannelData(int channelDataOffset, int channelDataLength, int recordCount, IList<ChannelInfo> sortedList)
    {
      if (sortedList.Count == 0) return;
      byte[] copy = new byte[recordCount * channelDataLength];
      Array.Copy(fileContent, channelDataOffset, copy, 0, copy.Length);

      int pTarget = channelDataOffset;
      int slot = 0;
      foreach (ChannelInfo appChannel in sortedList)
      {
        if (appChannel.NewProgramNr <= 0 && this.removeDeletedActChannels)
          continue;
        if (appChannel.RecordIndex != slot)
        {
          Array.Copy(copy, appChannel.RecordIndex*channelDataLength, fileContent, pTarget, channelDataLength);
          appChannel.RecordIndex = slot;
        }
        ++slot;
        pTarget += channelDataLength;
      }
    }
    #endregion

    #region UpdateDvbsChecksums()
    private void UpdateDvbsChecksums()
    {
      for (int i = 0; i < this.dvbsSubblockCrcOffset.Length; i++)
      {
        uint crc32 = Crc32.CalcCrc32(fileContent, this.dvbsSubblockCrcOffset[i] + 4, satConfig.dvbsSubblockLength[i]);
        var bytes = BitConverter.GetBytes(crc32);
        for (int j = 0; j < bytes.Length; j++)
          fileContent[this.dvbsSubblockCrcOffset[i] + j] = bytes[j];
      }
    }
    #endregion


    #region DefaultEncoding
    public override Encoding DefaultEncoding
    {
      get { return base.DefaultEncoding; }
      set 
      {
        if (Equals(value, this.DefaultEncoding))
          return;
        base.DefaultEncoding = value;
        if (this.DataRoot.IsEmpty)
          return;
        ChangeEncoding();
      }
    }
    #endregion

    #region ChangeEncoding()

    private void ChangeEncoding()
    {
      foreach (var list in DataRoot.ChannelLists)
      {
        foreach (var channel in list.Channels)
          channel.ChangeEncoding(this.DefaultEncoding);
      }
    }
    #endregion

    // TvSettingsForm

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("ANALOG");
      sb.Append("Number of data records: ").Append(this.analogChannelCount).AppendLine();
      sb.Append("Length of data record:  ").Append(this.actChannelSize).AppendLine();
      sb.AppendLine();

      sb.AppendLine();
      sb.AppendLine("DVB-C/T");
      sb.Append("Number of data records: ").Append(this.dvbctChannelCount).AppendLine();
      sb.Append("Length of data record:  ").Append(this.actChannelSize).AppendLine();
      sb.AppendLine();

      sb.AppendLine();
      sb.AppendLine("DVB-S");
      if (satConfig != null)
      {
        int numberOfDupePrNr;
        CountDuplicateRecords(out numberOfDupePrNr);
        sb.Append("Max number of data records:          ").Append(satConfig.dvbsMaxChannelCount).AppendLine();
        sb.Append("Length of data record:               ").Append(satConfig.dvbsChannelLength).AppendLine();
        sb.Append("Channel records in use:              ").Append(dvbsChannelCount).AppendLine();
        sb.Append("Channel records marked hard-deleted: ").Append(this.deletedChannelsHard).AppendLine();
        sb.Append("Channel records marked soft-deleted: ").Append(this.deletedChannelsSoft).AppendLine();
        sb.Append("Channel records erased (duplicates): ").Append(this.duplicateChannels).AppendLine();
        sb.Append("Channel records with Pr# 0:          ").Append(dvbsChannelsAtPr0).AppendLine();
        sb.Append("Channel records with duplicate Pr#:  ").Append(numberOfDupePrNr).AppendLine();
      }
      else
        sb.AppendLine("not present");

      return sb.ToString();
    }

    private void CountDuplicateRecords(out int numberOfDupePrNr)
    {
      numberOfDupePrNr = 0;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        if ((list.SignalSource & SignalSource.Sat) != 0)
          numberOfDupePrNr += list.DuplicateProgNrCount;
      }
    }
    #endregion

    #region TvCountryCode
    public string TvCountryCode
    {
      get { return this.countryCode; }
      set
      {
        if (value.Length < 3 || this.settingsBlockOffset == 0 || this.settingsBlockSize < 8) return;
        value = value.ToUpper();
        int off = this.settingsBlockOffset + 4 + 4 + 2;
        for (int i = 0; i < 3; i++)
          this.fileContent[off--] = (byte)value[i];
        this.countryCode = value;
      }
    }
    #endregion

    #region ShowDeviceSettingsForm()
    public override void ShowDeviceSettingsForm(object parentWindow)
    {
      using (var dlg = new TvSettingsForm(this))
      {
        dlg.ShowDialog((Form)parentWindow);
      }
    }
    #endregion

    #region GetFirmwareMapping()
    public FirmwareData GetFirmwareMapping()
    {
      var mapping = this.firmwareMappings.GetMapping(this.firmwareBlockSize, false);
      if (mapping == null) return null;
      mapping.SetDataPtr(this.fileContent, this.firmwareBlockOffset);
      return mapping;
    }
    #endregion

    // Testing

    #region GetHotelMenuOffset()
    public int GetHotelMenuOffset()
    {
      int off = this.firmwareBlockOffset;
      for (int i = 6500; i < this.FirmwareBlockSize - 3; i++)
      {
        if (BitConverter.ToUInt32(this.fileContent, off + i) == 0x05000101) // 1,1,0,5
        {
          for (int j = 5; j < 20; j++) // backtrack to find Volume/MaxVolue pattern
          {
            if (fileContent[off + i - j] == 101 && fileContent[off + i - j - 6] == 100)
              // check for Volume/MaxVolue to be 101/100
              return i - j - 15;
          }
          return 0;
        }
      }
      return 0;
    }
    #endregion

    internal int ACTChannelLength { get { return this.actChannelSize; } }
    internal bool HasDvbs { get { return dvbsBlockOffset != 0; } }
    internal int SatChannelLength { get { return satConfig != null ? satConfig.dvbsChannelLength : 0; } }
    internal bool SatSymbolRateDiv2 { get { return this.isDvbsSymbolRateDiv2; } }
    internal int FirmwareBlockSize { get { return this.firmwareBlockSize; } }
  }
}
