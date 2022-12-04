//#define STORE_DVBS_CHANNELS_IN_DATABASE
//#define LM640T_EXPERIMENT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChanSort.Api;
using View = ChanSort.Api.View;

namespace ChanSort.Loader.LG.Binary
{
  public partial class TllFileSerializer : SerializerBase, ITllFileSerializer
  {
    enum SpecialHandlingModels { Standard, LH3000, LH250, PN, LP, LT, LY };

    private const long DVBS_S2 = 0x0032532D53425644; // reverse of "DVBS-S2\0"
    private const long MaxFileSize = 2000000;
    private readonly string ERR_fileTooBig = Resource.TllFileSerializerPlugin_ERR_fileTooBig;
    private readonly string ERR_modelUnknown = Resource.TllFileSerializerPlugin_ERR_modelUnknown;
    private readonly string ERR_wrongChecksum = Resource.TllFileSerializer_ERR_wrongChecksum;
    private readonly string ERR_dupeChannel = Resource.TllFileSerializer_ERR_dupeChannel;

    private readonly Dictionary<int, DvbsDataLayout> satConfigs = new Dictionary<int, DvbsDataLayout>();
    private readonly MappingPool<DataMapping> actMappings = new MappingPool<DataMapping>("Analog and DVB-C/T");
    private readonly MappingPool<DataMapping> dvbsMappings = new MappingPool<DataMapping>("DVB-S Channel");
    private readonly MappingPool<DataMapping> dvbsTransponderMappings = new MappingPool<DataMapping>("DVB-S Transponder");
    private readonly MappingPool<FirmwareData> firmwareMappings = new MappingPool<FirmwareData>("Firmware");
    private readonly MappingPool<DataMapping> lnbMappings = new MappingPool<DataMapping>("LNB Config"); 

    private readonly ChannelList avbcTvChannels = new ChannelList(SignalSource.AnalogC | SignalSource.Tv, "Analog Cable");
    private readonly ChannelList avbtTvChannels = new ChannelList(SignalSource.AnalogT | SignalSource.Tv, "Analog Antenna");
    private readonly ChannelList dvbcTvChannels = new ChannelList(SignalSource.DvbC | SignalSource.Tv, "DVB-C TV");
    private readonly ChannelList dvbtTvChannels = new ChannelList(SignalSource.DvbT | SignalSource.Tv, "DVB-T TV");
    private readonly ChannelList dvbcRadioChannels = new ChannelList(SignalSource.DvbC | SignalSource.Radio, "DVB-C Radio");
    private readonly ChannelList dvbtRadioChannels = new ChannelList(SignalSource.DvbT | SignalSource.Radio, "DVB-T Radio");
    private readonly ChannelList satTvChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv, "Sat TV");
    private readonly ChannelList satRadioChannels = new ChannelList(SignalSource.DvbS | SignalSource.Radio, "Sat Radio");

    private byte[] fileContent;
    private SpecialHandlingModels specialModel = SpecialHandlingModels.Standard;

    private int analogBlockOffset;
    private int firmwareBlockOffset;
    private int hospitalityBlockOffset;
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

    private Dictionary<int, ushort> nextChannelIndex;
    private int firmwareBlockSize;
    private int hospitalityBlockSize;
    private int dvbsBlockSize;
    private int settingsBlockSize;
    private string countryCode;

    private int duplicateChannels;
    private int deletedChannelsHard;
    private int deletedChannelsSoft;
    private int dvbsChannelsAtPr0;

    private bool removeDeletedActChannels;
    private bool mustReorganizeDvbs;
    private decimal dvbsSymbolRateFactor;

    private ILgUserInterfaceFactory uiFactory;

    #region ctor()
    public TllFileSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.Analog;
      this.Features.DeleteMode = DeleteMode.FlagWithoutPrNr;
      this.Features.CanSkipChannels = true;
      this.Features.CanLockChannels = true;
      this.Features.CanHideChannels = true;
      this.Features.CanHaveGaps = true;
      this.Features.DeviceSettings = true;
      this.Features.CleanUpChannelData = true;
      this.Features.FavoritesMode = FavoritesMode.Flags;
      this.Features.MaxFavoriteLists = 4;
      this.SupportedTvCountryCodes = new List<string>
                                       {
                                         "___ (None)", "AUT (Austria)", "BEL (Belgium)", "CHE (Switzerland)", 
                                         "DEU (Germany)", "ESP (Spain)", "FRA (France)", "GBR (Great Britain)",
                                         "GRC (Greece)", "IRL (Ireland)", "ITA (Italy)", "LUX (Luxembourg)", 
                                         "NLD (Netherlands)", "PRT (Portugal)", "SVN (Slovenia)"
                                       };

      this.ReadConfigurationFromIniFile();

      this.DataRoot.AddChannelList(avbcTvChannels);
      this.DataRoot.AddChannelList(avbtTvChannels);
      this.DataRoot.AddChannelList(dvbcTvChannels);
      this.DataRoot.AddChannelList(dvbcRadioChannels);
      this.DataRoot.AddChannelList(dvbtTvChannels);
      this.DataRoot.AddChannelList(dvbtRadioChannels);
    }
    #endregion

    public IList<string> SupportedTvCountryCodes { get; private set; }

    #region ReadConfigurationFromIniFile()

    private void ReadConfigurationFromIniFile()
    {
      string iniFile = this.GetType().Assembly.Location.ToLowerInvariant().Replace(".dll", ".ini");
      IniFile ini = new IniFile(iniFile);
      foreach (var section in ini.Sections)
      {
        int idx = section.Name.IndexOf(":");
        string recordLength = idx < 0 ? "" : section.Name.Substring(idx + 1);
        if (section.Name.StartsWith("DvbsBlock"))
          this.satConfigs.Add(int.Parse(recordLength), new DvbsDataLayout(section));
        else if (section.Name.StartsWith("ACTChannelDataMapping"))
          actMappings.AddMapping(recordLength, new DataMapping(section));
        else if (section.Name.StartsWith("SatChannelDataMapping"))
          dvbsMappings.AddMapping(recordLength, new DataMapping(section));
        else if (section.Name.StartsWith("TransponderDataMapping"))
          dvbsTransponderMappings.AddMapping(recordLength, new DataMapping(section));
        else if (section.Name.StartsWith("LnbMapping"))
          lnbMappings.AddMapping(recordLength, new DataMapping(section));
        else if (section.Name.StartsWith("FirmwareData"))
          firmwareMappings.AddMapping(recordLength, new FirmwareData(section));
      }
    }
    #endregion


    #region Load()

    public override void Load()
    {
      this.InitUiFactory();

      string basename = (Path.GetFileNameWithoutExtension(this.FileName) ?? "").ToUpperInvariant();
      if (basename.StartsWith("XXLH250"))
        this.specialModel = SpecialHandlingModels.LH250;
      else if (basename.StartsWith("XXLH3000"))
        this.specialModel = SpecialHandlingModels.LH3000;
      else if (basename.StartsWith("XXPN"))
        this.specialModel = SpecialHandlingModels.PN;
      else if (basename.StartsWith("XXLP"))
        this.specialModel = SpecialHandlingModels.LP;
      else if (basename.StartsWith("XXLT"))
        this.specialModel = SpecialHandlingModels.LT;
      else if (basename.StartsWith("XXLY"))
        this.specialModel = SpecialHandlingModels.LY;
      else
        this.specialModel = SpecialHandlingModels.Standard;

      long fileSize = new FileInfo(this.FileName).Length;
      if (fileSize > MaxFileSize)
        throw LoaderException.Fail(string.Format(ERR_fileTooBig, fileSize, MaxFileSize));

      this.fileContent = File.ReadAllBytes(this.FileName);
      if (this.fileContent[0] == '<')
        throw LoaderException.TryNext("Invalid binary TLL file format. Maybe a GlobalClone/XML file?");

      int off = 0;

      this.ReadFileHeader(ref off);
      this.ReadAnalogChannelBlock(ref off);
      this.ReadFirmwareDataBlock(ref off);
      this.ReadHotelModelExtraBlock(ref off);
      this.ReadDvbCtChannels(ref off);
      this.ReadDvbSBlock(ref off);
      this.ReadSettingsBlock(ref off);

      if (this.satTvChannels.PresetProgramNrCount > 0 || this.satRadioChannels.PresetProgramNrCount > 0)
      {
        foreach (var channel in this.satTvChannels.Channels)
          channel.NewProgramNr = channel.OldProgramNr;
        foreach (var channel in this.satRadioChannels.Channels)
          channel.NewProgramNr = channel.OldProgramNr;
        if (IsTesting || !PromptForEditingPresetList())
        {
          this.satTvChannels.ReadOnly = true;
          this.satRadioChannels.ReadOnly = true;          
        }
      }

#if STORE_DVBS_CHANNELS_IN_DATABASE
      this.StoreToDatabase();
#endif

      foreach (var list in this.DataRoot.ChannelLists)
        list.MaxChannelNameLength = 40;
    }

    private void InitUiFactory()
    {
      if (this.uiFactory != null || this.IsTesting)
        return;

      // run-time-binding to the UI factory so that the loader module can be edited + compiled without DevExpress dependencies
      var ass = Assembly.Load(new AssemblyName("ChanSort.Loader.LG.UI"));
      var type = ass.GetType("ChanSort.Loader.LG.UI.LgUserInterfaceFactory");
      this.uiFactory = (ILgUserInterfaceFactory)Activator.CreateInstance(type);
    }

    private bool PromptForEditingPresetList()
    {
#if false
      return uiFactory.ShowPresetProgramNrDialog();
#else
      View.Default.ShowHtmlBox(
@"<b>Editing of the satellite channel list is disabled!</b>

This file contains preset program numbers for satellite channels. 
Due to issues with most recent LG firmwares such lists can no longer be modified reliably.

<b>To enable editing you must first run a clean full channel search:</b>
- Keep a copy of the current TLL file if you want to use it as a reference list later
- Execute a Factory Reset on your TV
- Execute an automatic channel search with options 'Full', 'None' and 'Blind search'
- Save the new list to USB and open it with ChanSort


<a href='http://sourceforge.net/p/chansort/wiki/Channels%20disappear%20or%20change%20program%20numbers%20randomly/'>See the ChanSort Wiki for details</a>",
"!!! Attention !!!", 450, 220, url => Process.Start(url));
      return false; // TODO
#endif
    }

    #endregion

    #region ReadFileHeader()
    private void ReadFileHeader(ref int off)
    {
      if (fileContent.Length < 4)
        throw LoaderException.Fail(ERR_modelUnknown);
      var magic = BitConverter.ToUInt32(fileContent, off);
      if (magic == 0x5A5A5A5A || magic == 0xA5A5A5A5)
        off += 4;
    }
    #endregion

    #region ReadAnalogChannelBlock()

    private void ReadAnalogChannelBlock(ref int off)
    {
      this.analogBlockOffset = off;
      this.ReadActChannelBlock(ref off, out analogChannelCount, ref actChannelSize, 
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

    #region ReadHotelModelExtraBlock()
    private void ReadHotelModelExtraBlock(ref int off)
    {
      if (!(this.specialModel == SpecialHandlingModels.LT || this.specialModel == SpecialHandlingModels.LP || this.specialModel == SpecialHandlingModels.LY))
        return;

      int size = BitConverter.ToInt32(this.fileContent, off);
      this.hospitalityBlockOffset = off;
      this.hospitalityBlockSize = size;
      off += 4 + size;
    }
    #endregion

    #region ReadDvbCtChannels()
    private void ReadDvbCtChannels(ref int off)
    {
      this.dvbctBlockOffset = off;
      this.ReadActChannelBlock(ref off, out dvbctChannelCount, ref actChannelSize,
        (slot, data) => new DtvChannel(slot, data));
    }
    #endregion

    #region ReadDvbSBlock()
    private void ReadDvbSBlock(ref int off)
    {
      int blockSize;
      if (!IsDvbsBlock(ref off, out blockSize))
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
    private void ReadActChannelBlock(ref int off, out int channelCount, ref int recordSize,
      Func<int, DataMapping, ChannelInfo> channelFactory)
    {
      int blockSize = this.GetBlockSize(off, 2);
      off += 4;

      channelCount = BitConverter.ToInt32(fileContent, off);
      off += 4;
      if (channelCount == 0) return;

      recordSize = GetActChannelRecordSize(off, blockSize, channelCount);
      var actMapping = GetActChannelMapping(recordSize);
      this.reorderPhysically = actMapping.Settings.GetInt("reorderChannelData") != 0;
      this.removeDeletedActChannels |= this.reorderPhysically;

      for (int i = 0; i < channelCount; i++)
      {
        actMapping.SetDataPtr(fileContent, off);
        ChannelInfo ci = channelFactory(i, actMapping);

        var list = this.DataRoot.GetChannelList(ci.SignalSource);
        this.DataRoot.AddChannel(list, ci);

        off += recordSize;
      }
    }

    #endregion

    #region GetActChannelMapping()
    private DataMapping GetActChannelMapping(int recordSize)
    {
      var key = recordSize.ToString();
      if (this.specialModel == SpecialHandlingModels.LH250)
        key += "LH250"; 
      else if (this.specialModel == SpecialHandlingModels.LH3000)
        key += "LH3000";
      else if (this.specialModel == SpecialHandlingModels.PN)
        key += "PN";
      else if (this.specialModel == SpecialHandlingModels.LY)
        key += "LY";
      var actMapping = this.actMappings.GetMapping(key);
      return actMapping;
    }
    #endregion

    #region GetBlockSize()
    private int GetBlockSize(int off, int minSize = 0)
    {
      long len = BitConverter.ToUInt32(fileContent, off);
      if (len < minSize || off + 4 + len > fileContent.Length)
        throw LoaderException.Fail(ERR_modelUnknown);
      return (int)len;
    }
    #endregion

    #region GetActChannelRecordSize()
    private int GetActChannelRecordSize(int off, int blockSize, int channelCount)
    {
      if ((blockSize - 4) % channelCount != 0)
        throw LoaderException.Fail(ERR_modelUnknown);
      int recordSize = (blockSize - 4) / channelCount;
      if (off + channelCount * recordSize > fileContent.Length)
        throw LoaderException.Fail(ERR_modelUnknown);
      return recordSize;
    }
    #endregion


    #region IsDvbsBlock()
    private bool IsDvbsBlock(ref int off, out int blockSize)
    {
      blockSize = 0;
      if (off >= fileContent.Length)
        return false;
      blockSize = this.GetBlockSize(off);
      if (blockSize < 12)
        return false;
      long blockId = BitConverter.ToInt64(fileContent, off + 8);
      if (blockId == DVBS_S2)
        return true;

      // LW4500, LW5400, LMxxxT and maybe other models with bogus DVB-S block
      if (blockId == -1)
      {
        this.satConfig = satConfigs.TryGet(blockSize);
        if (this.satConfig != null)
        {
          this.dvbsBlockSize = blockSize;
          this.dvbsBlockOffset = off;
          off += 4 + blockSize;
        }
      }

      return false;
    }
    #endregion

    #region ReadDvbsSubblocks()
    private void ReadDvbsSubblocks(ref int off)
    {
      this.DataRoot.AddChannelList(satTvChannels);
      this.DataRoot.AddChannelList(satRadioChannels);

      this.VerifyDvbsSubblockChecksums(off);

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
      this.ReadDvbsChannelLinkedList(header, ref off);
      off += satConfig.linkedListExtraDataLength;
      this.ReadDvbsChannels(ref off, header.LinkedListStartIndex);

      // subblock 5 (satellite/LNB config)
      this.ReadLnbConfig(ref off);
    }
    #endregion

    #region VerifyDvbsSubblockChecksums()
    private void VerifyDvbsSubblockChecksums(int off)
    {      
      this.dvbsSubblockCrcOffset = new int[satConfig.dvbsSubblockLength.Length];
      for (int i = 0; i < dvbsSubblockCrcOffset.Length; i++)
      {
        this.dvbsSubblockCrcOffset[i] = off;
        int subblockLength = satConfig.dvbsSubblockLength[i];
        uint fileCrc = BitConverter.ToUInt32(fileContent, off);
        uint calcCrc = Crc32.Reversed.CalcCrc32(fileContent, off + 4, subblockLength);
        if (fileCrc != calcCrc)
          throw LoaderException.Fail(string.Format(ERR_wrongChecksum, fileCrc, calcCrc));
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
      dvbsSymbolRateFactor = 1;
      var dvbsSymbolRateMask = 0x7FFFF;
      var mapping = GetTransponderMapping();
      for (int i=0; i<satConfig.transponderCount; i++)
      {
        mapping.SetDataPtr(this.fileContent, off + i*satConfig.transponderLength);
        SatTransponder transponder = new SatTransponder(i, mapping, this.DataRoot, this.satConfig.satIndexFactor);
        if (transponder.Satellite == null)
          continue;
        
        if ((transponder.SymbolRate & 0x7FFF) == 11000)
          dvbsSymbolRateFactor = 2;
        else if (transponder.SymbolRate == 44000)
        {
          dvbsSymbolRateMask = 0xFFFF;
          dvbsSymbolRateFactor = 0.5m;
        }

        var sat = transponder.Satellite;
        this.DataRoot.AddTransponder(sat, transponder);
      }

      if (dvbsSymbolRateFactor != 1)
      {
        foreach (var transponder in this.DataRoot.Transponder.Values)
        {
          int symbolRate = transponder.SymbolRate & dvbsSymbolRateMask;
          transponder.SymbolRate = (int) (symbolRate*dvbsSymbolRateFactor);
        }
      }

      off += this.satConfig.transponderCount * this.satConfig.transponderLength;
    }

    private DataMapping GetTransponderMapping()
    {
      string key = this.satConfig.transponderLength.ToString();
      if (this.specialModel == SpecialHandlingModels.LP)
        key += "LP";
      var mapping = this.dvbsTransponderMappings.GetMapping(key);
      return mapping;
    }

    #endregion

    #region ReadDvbsChannelLinkedList()
    private void ReadDvbsChannelLinkedList(SatChannelListHeader header, ref int off)
    {
      this.nextChannelIndex = new Dictionary<int, ushort>();
      int index = header.LinkedListStartIndex;
      while (index != 0xFFFF)
      {
        int offEntry = off + index*satConfig.sizeOfChannelLinkedListEntry;
        ushort nextIndex = BitConverter.ToUInt16(fileContent, offEntry + 2);
        if (this.nextChannelIndex.ContainsKey(index)) // prevent infinite loop (exists in some test files)
          break;
        this.nextChannelIndex.Add(index, nextIndex);
        index = nextIndex;
      }
      off += satConfig.dvbsMaxChannelCount*satConfig.sizeOfChannelLinkedListEntry;
    }
    #endregion

    #region ReadDvbsChannels()
    private void ReadDvbsChannels(ref int off, ushort startIndex)
    {
      var mapping = GetDvbsChannelMapping();
      ushort index = startIndex;
      for (int i = 0; i < this.dvbsChannelCount; i++)
      {
        int recordOffset = off + index*satConfig.dvbsChannelLength;
        mapping.SetDataPtr(fileContent, recordOffset);
        SatChannel ci = new SatChannel(i, index, mapping, this.DataRoot);
        if (!ci.InUse)
          ++this.deletedChannelsHard;
        else
        {
          if (ci.IsDeleted)
            ++this.deletedChannelsSoft;

          var list = this.DataRoot.GetChannelList(ci.SignalSource);
          var dupes = list.GetChannelByUid(ci.Uid);
          if (dupes.Count == 0)
          {
            if (ci.OldProgramNr == 0 && !ci.IsDeleted)
              ++this.dvbsChannelsAtPr0;
          }
          else
          {
            this.DataRoot.Warnings.AppendFormat(ERR_dupeChannel, ci.RecordIndex, ci.OldProgramNr, dupes[0].RecordIndex,
                                                dupes[0].OldProgramNr, dupes[0].Name).AppendLine();
            ++this.duplicateChannels;
          }
          this.DataRoot.AddChannel(list, ci);
        }

        if (!this.nextChannelIndex.TryGetValue(index, out index) || index == 0xFFFF)
          break;
      }
      off += satConfig.dvbsMaxChannelCount * satConfig.dvbsChannelLength;
    }

    #endregion

    #region GetDvbsChannelMapping()
    private DataMapping GetDvbsChannelMapping()
    {
      var key = satConfig.dvbsChannelLength.ToString();
      if (this.specialModel == SpecialHandlingModels.LP)
        key += "LP";
      var mapping = this.dvbsMappings.GetMapping(key);
      return mapping;
    }
    #endregion

    #region GetSatLocation()
    private string GetSatLocation(byte degree, byte fractionAndOrientation)
    {
      return string.Format("{0}.{1}{2}", degree, fractionAndOrientation & 0x0f, fractionAndOrientation < 16 ? "W" : "E");
    }
    #endregion

    #region ReadLnbConfig()
    private void ReadLnbConfig(ref int off)
    {
      off += satConfig.LnbBlockHeaderSize;
      var mapping = this.lnbMappings.GetMapping(satConfig.lnbLength);
      for (int i = 0; i < satConfig.lnbCount; i++)
      {
        mapping.SetDataPtr(this.fileContent, off);
        var lnb = new LnbConfig(mapping, this.DataRoot);
        if (lnb.Id != 0)
        {
          lnb.Satellite.LnbConfig = lnb;
          this.DataRoot.AddLnbConfig(lnb);
        }
        off += satConfig.lnbLength;
      }
    }
    #endregion

    // Test code for fixing broken DVB-S block of xxLM640T ==========

    #region EraseDvbsBlock()
#if LM640T_EXPERIMENT
    /// <summary>
    /// The model LM640T has the whole DVB-S2 block filled with 0xFF bytes, including the checksums.
    /// When a file (even the originally saved one) is loaded back, the TV crashes and performs a factory reset.
    /// </summary>
    private void EraseDvbsBlock(int off)
    {
      this.dvbsBlockOffset = off;
      this.dvbsBlockSize = satConfig.dvbsBlockTotalLength;
      this.dvbsSubblockCrcOffset = new int[satConfig.dvbsSubblockLength.Length];

      int p = this.dvbsBlockOffset + 4;
      for (int i = 0; i < this.dvbsBlockSize; i++)
        this.fileContent[p++] = 0;

      using (MemoryStream stream = new MemoryStream(this.fileContent))
      using (BinaryWriter wrt = new BinaryWriter(stream))
      {
        stream.Seek(this.dvbsBlockOffset+4, SeekOrigin.Begin); // skip length

        // header
        this.dvbsSubblockCrcOffset[0] = (int)stream.Position;
        stream.Seek(4, SeekOrigin.Current); // skip CRC32
        stream.Write(Encoding.ASCII.GetBytes("DVBS-S2\0"), 0, 8);
        wrt.Write((ushort) 7);
        wrt.Write((ushort) 4);

        // satellite
        this.dvbsSubblockCrcOffset[1] = (int)stream.Position;
        stream.Seek(4, SeekOrigin.Current); // skip CRC32
        stream.Seek(2 + satConfig.satCount/8 + 2 + 2 + satConfig.satCount + 2, SeekOrigin.Current);
        for (int i = 0; i < satConfig.satCount; i++)
        {
          stream.Seek(36, SeekOrigin.Current);
          wrt.Write((short) -1);
          wrt.Write((short) -1);
          stream.Seek(2 + 2, SeekOrigin.Current);
        }

        // transponders
        this.dvbsSubblockCrcOffset[2] = (int)stream.Position;
        stream.Seek(4, SeekOrigin.Current); // skip CRC32
        stream.Seek(5*2 + satConfig.transponderCount/8, SeekOrigin.Current);
        wrt.Write((short) -1);
        wrt.Write((short) -1);
        stream.Seek(2 + (satConfig.transponderCount - 1)*6 + 2, SeekOrigin.Current);
        for (int i = 0; i < satConfig.transponderCount; i++)
        {
          wrt.Write(-1);
          wrt.Write((ushort)0);
          wrt.Write((short)-1);
          wrt.Write((byte)0xfe);
          for (int j = 9; j < satConfig.transponderLength; j++)
            wrt.Write((byte)0xFF);
        }

        // channels
        this.dvbsSubblockCrcOffset[3] = (int)stream.Position;
        stream.Seek(4, SeekOrigin.Current); // skip CRC32
        stream.Seek(12 + satConfig.dvbsMaxChannelCount/8, SeekOrigin.Current);
        wrt.Write((short) -1);
        wrt.Write((short) -1);
        stream.Seek(4 + (satConfig.dvbsMaxChannelCount - 1)*8 + satConfig.linkedListExtraDataLength, SeekOrigin.Current);
        for (int i = 0; i < satConfig.dvbsMaxChannelCount*satConfig.dvbsChannelLength; i++)
          wrt.Write((byte) 0xFF);

        // sat/LNB-config
        this.dvbsSubblockCrcOffset[4] = (int)stream.Position;
        stream.Seek(4, SeekOrigin.Current); // skip CRC32
        stream.Seek(2, SeekOrigin.Current);
        wrt.Write((byte) 1);
        stream.Seek(satConfig.lnbCount/8 - 1, SeekOrigin.Current);
      }

      this.UpdateDvbsChecksums();
    }
#endif
    #endregion

    // Sat channel list cleanup ==================

    #region CleanUpChannelData()
    public override string CleanUpChannelData()
    {
      if (this.satConfig == null || this.dvbsSubblockCrcOffset == null)
        return "";

      this.DataRoot.NeedsSaving = true;
      this.ResetChannelInformationInTransponderData();

      byte[] sortedChannels = new byte[this.satConfig.dvbsMaxChannelCount*this.satConfig.dvbsChannelLength];

      var channelsByTransponder =
        this.satTvChannels.Channels.Union(this.satRadioChannels.Channels).OrderBy(PhysicalChannelOrder).ToList();
      int originalChannelCount = this.dvbsChannelCount;
      int prevChannelOrderId = -1;
      int prevTransponderIndex = -1;
      int channelCounter = 0;
      int removedCounter = 0;
      SatTransponder currentTransponder = null;
      foreach (var channel in channelsByTransponder)
      {
        SatChannel satChannel = channel as SatChannel;
        if (satChannel == null) // ignore proxy channels created by a reference list
          continue;
        RelocateChannelData(satChannel, ref prevChannelOrderId, sortedChannels, ref removedCounter,
                            ref prevTransponderIndex, ref channelCounter, ref currentTransponder);
      }
      if (currentTransponder != null)
      {
        currentTransponder.LastChannelIndex = channelCounter - 1;
        currentTransponder.ChannelCount = channelCounter - currentTransponder.FirstChannelIndex;
      }

      // copy temp data back to fileContent and clear remainder
      if (originalChannelCount == 0)
      {
        // even if there's 0 channels, channel[0] must contain valid data
        Tools.MemSet(this.fileContent,
                     this.dvbsBlockOffset + satConfig.ChannelListOffset + 1*satConfig.dvbsChannelLength,
                     0xFF,
                     (satConfig.dvbsMaxChannelCount - 1)*satConfig.dvbsChannelLength);
      }
      else
      {
        Tools.MemCopy(sortedChannels, 0,
                      this.fileContent, this.dvbsBlockOffset + satConfig.ChannelListOffset,
                      channelCounter*satConfig.dvbsChannelLength);
        Tools.MemSet(this.fileContent,
                     this.dvbsBlockOffset + satConfig.ChannelListOffset + channelCounter*satConfig.dvbsChannelLength,
                     0xFF,
                     (satConfig.dvbsMaxChannelCount - channelCounter)*satConfig.dvbsChannelLength);
      }

      UpdateChannelAllocationBitmap(channelCounter);
      UpdateChannelLinkedList(channelCounter);      
      UpdateDvbsChecksums();

      return string.Format("{0} duplicate channels were detected and removed", removedCounter);
    }
    #endregion

    #region ResetChannelInformationInTransponderData()
    private void ResetChannelInformationInTransponderData()
    {
      foreach (SatTransponder transponder in this.DataRoot.Transponder.Values)
      {
        transponder.FirstChannelIndex = 0xFFFF;
        transponder.LastChannelIndex = 0xFFFF;
        transponder.ChannelCount = 0;
      }
    }
    #endregion

    #region PhysicalChannelOrder()
    private int PhysicalChannelOrder(ChannelInfo channel)
    {
      return (channel.Transponder.Id << 16) + channel.ServiceId;
    }
    #endregion

    #region RelocateChannelData()
    private void RelocateChannelData(SatChannel channel, ref int prevChannelOrderId,
                                     byte[] sortedChannels, ref int removed, ref int prevTransponderIndex, ref int counter,
                                     ref SatTransponder currentTransponder)
    {
      if (RemoveChannelIfDupe(channel, ref prevChannelOrderId, ref removed))
        return;

      UpdateChannelIndexInTransponderData(channel, ref prevTransponderIndex, counter, ref currentTransponder);

      Tools.MemCopy(
        channel.RawDataBuffer,
        channel.RawDataOffset,
        sortedChannels,
        counter*satConfig.dvbsChannelLength,
        satConfig.dvbsChannelLength);

      channel.RecordIndex = counter++;
      channel.baseOffset = this.dvbsBlockOffset + satConfig.ChannelListOffset + (int)channel.RecordIndex*satConfig.dvbsChannelLength;
    }
    #endregion

    #region RemoveChannelIfDupe()
    private bool RemoveChannelIfDupe(SatChannel channel, ref int prevOrder, ref int removed)
    {
      int order = this.PhysicalChannelOrder(channel);
      if (order == prevOrder)
      {
        var list = this.DataRoot.GetChannelList(channel.SignalSource);
        list.RemoveChannel(channel);
        ++removed;
        channel.NewProgramNr = -1;
        channel.OldProgramNr = -1;
        return true;
      }
      prevOrder = order;
      return false;
    }
    #endregion

    #region UpdateChannelIndexInTransponderData()
    private void UpdateChannelIndexInTransponderData(SatChannel channel, ref int prevTransponderIndex, int counter,
                                                    ref SatTransponder transponder)
    {
      if (channel.Transponder.Id == prevTransponderIndex) 
        return;

      if (transponder != null)
      {
        transponder.LastChannelIndex = counter - 1;
        transponder.ChannelCount = counter - transponder.FirstChannelIndex;
      }

      transponder = (SatTransponder)channel.Transponder;
      transponder.FirstChannelIndex = counter;
      prevTransponderIndex = channel.Transponder.Id;
    }
    #endregion

    #region UpdateChannelAllocationBitmap()
    private void UpdateChannelAllocationBitmap(int counter)
    {
      Tools.MemSet(fileContent, this.dvbsBlockOffset + satConfig.AllocationBitmapOffset, 0, satConfig.dvbsMaxChannelCount / 8);
      Tools.MemSet(fileContent, this.dvbsBlockOffset + satConfig.AllocationBitmapOffset, 0xFF, counter / 8);
      if (counter % 8 != 0)
        fileContent[this.dvbsBlockOffset + satConfig.AllocationBitmapOffset + counter / 8] = (byte)(0xFF >> (8 - counter % 8));
    }
    #endregion

    #region UpdateChannelLinkedList()
    private void UpdateChannelLinkedList(int counter)
    {
      var header = new SatChannelListHeader(this.fileContent, this.dvbsBlockOffset + satConfig.ChannelListHeaderOffset);
      header.ChannelCount = counter;
      header.LinkedListStartIndex = 0;
      if (counter == 0)
        counter = 1;
      header.LinkedListEndIndex1 = counter - 1;
      header.LinkedListEndIndex2 = counter - 1;

      // update linked list
      var off = this.dvbsBlockOffset + satConfig.SequenceTableOffset;
      for (int i = 0; i < counter; i++)
      {
        Tools.SetInt16(this.fileContent, off + 0, i - 1);
        Tools.SetInt16(this.fileContent, off + 2, i + 1);
        Tools.SetInt16(this.fileContent, off + 4, i);
        off += satConfig.sizeOfChannelLinkedListEntry;
      }
      Tools.SetInt16(this.fileContent, off - satConfig.sizeOfChannelLinkedListEntry + 2, 0xFFFF);
      Tools.MemSet(fileContent, off, 0, (satConfig.dvbsMaxChannelCount - counter) * satConfig.sizeOfChannelLinkedListEntry);
    }
    #endregion


    // Saving ====================================

    #region Save()
    public override void Save()
    {
      int newAnalogChannelCount;
      int newDvbctChannelCount;
      this.UpdateRawChannelData(out newAnalogChannelCount, out newDvbctChannelCount);

      if (!removeDeletedActChannels)
      {
        newAnalogChannelCount = this.analogChannelCount;
        newDvbctChannelCount = this.dvbctChannelCount;
      }

      if (this.reorderPhysically || removeDeletedActChannels)
        this.ReorderActChannelsPhysically();

      if (this.mustReorganizeDvbs)
      {
        this.CleanUpChannelData();
        this.mustReorganizeDvbs = false;
      }

      if (this.dvbsSubblockCrcOffset != null)
        this.UpdateDvbsChecksums();

      using var file = new BinaryWriter(new FileStream(this.FileName, FileMode.Create, FileAccess.Write));
      // header
      file.Write(this.fileContent, 0, this.analogBlockOffset);

      // analog
      file.Write(newAnalogChannelCount*this.actChannelSize + 4);
      file.Write(newAnalogChannelCount);
      file.Write(fileContent, this.analogBlockOffset + 8, newAnalogChannelCount*this.actChannelSize);

      // firmware
      file.Write(fileContent, this.firmwareBlockOffset, this.firmwareBlockSize + 4);

      // hospitality models extra block
      if (hospitalityBlockOffset != 0)
        file.Write(fileContent, this.hospitalityBlockOffset, this.hospitalityBlockSize + 4);

      // DVB-CT
      file.Write(newDvbctChannelCount*this.actChannelSize + 4);
      file.Write(newDvbctChannelCount);
      file.Write(fileContent, this.dvbctBlockOffset + 8, newDvbctChannelCount * this.actChannelSize);

      // DVB-S
      if (this.dvbsBlockOffset != 0)
        file.Write(fileContent, this.dvbsBlockOffset, this.dvbsBlockSize + 4);

      // rest (including settings)
      if (this.settingsBlockOffset != 0)
        file.Write(fileContent, this.settingsBlockOffset, fileContent.Length - this.settingsBlockOffset);
    }
    #endregion

    #region UpdateRawChannelData()
    private void UpdateRawChannelData(out int newAnalogChannelCount, out int newDvbctChannelCount)
    {
      newAnalogChannelCount = 0;
      newDvbctChannelCount = 0;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach(var channel in list.Channels)
        {
          if (channel.IsProxy)
            continue;

          if (channel.NewProgramNr != -1)
          {
            if ((channel.SignalSource & SignalSource.Analog) != 0)
              ++newAnalogChannelCount;
            else if ((channel.SignalSource & SignalSource.DvbCT) != 0)
              ++newDvbctChannelCount;
          }

          channel.UpdateRawData();
        }
      }
    }

    #endregion

    #region ReorderActChannelsPhysically()
    private void ReorderActChannelsPhysically()
    {
      var avbt = this.avbtTvChannels.Channels.OrderBy(c => c.NewProgramNr);
      var avbc = this.avbcTvChannels.Channels.OrderBy(c => c.NewProgramNr);
      var avb = avbt.Union(avbc).ToList();
      this.ReorderChannelData(this.analogBlockOffset + 8, this.actChannelSize, this.analogChannelCount, avb);

      var dvbcTv = this.dvbcTvChannels.Channels.OrderBy(c => c.NewProgramNr);
      var dvbcRadio = this.dvbcRadioChannels.Channels.OrderBy(c => c.NewProgramNr);
      var dvbtTv = this.dvbtTvChannels.Channels.OrderBy(c => c.NewProgramNr);
      var dvbtRadio = this.dvbtRadioChannels.Channels.OrderBy(c => c.NewProgramNr);

      var dvb = dvbtTv.Union(dvbtRadio).Union(dvbcTv).Union(dvbcRadio).ToList();
      this.ReorderChannelData(this.dvbctBlockOffset + 8, this.actChannelSize, this.dvbctChannelCount, dvb);
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
        if (appChannel.RecordIndex < 0 || appChannel.NewProgramNr <= 0 && removeDeletedActChannels)
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
        uint crc32 = Crc32.Reversed.CalcCrc32(fileContent, this.dvbsSubblockCrcOffset[i] + 4, satConfig.dvbsSubblockLength[i]);
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
        this.actMappings.DefaultEncoding = value;
        this.firmwareMappings.DefaultEncoding = value;
        this.dvbsMappings.DefaultEncoding = value;
        this.dvbsTransponderMappings.DefaultEncoding = value;
        this.lnbMappings.DefaultEncoding = value;
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
      sb.AppendLine(base.GetFileInformation());

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
        value = value.ToUpperInvariant();
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
      uiFactory.ShowTvSettingsForm(this, parentWindow);
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

    internal bool IsTesting { get; set; }
    internal int ACTChannelLength { get { return this.actChannelSize; } }
    internal bool HasDvbs { get { return satConfig != null; } }
    internal int SatChannelLength { get { return satConfig != null ? satConfig.dvbsChannelLength : 0; } }
    internal decimal DvbsSymbolRateCorrectionFactor { get { return this.dvbsSymbolRateFactor; } }
    internal int FirmwareBlockSize { get { return this.firmwareBlockSize; } }
  }
}
