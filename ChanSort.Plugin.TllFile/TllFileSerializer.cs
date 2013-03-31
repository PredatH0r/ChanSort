#define FIX_SYMBOL_RATE

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public partial class TllFileSerializer : SerializerBase
  {
    private readonly string ERR_badFileHeader = Resource.TllFileSerializer_ERR_badFileHeader;
    private readonly string ERR_wrongChecksum = Resource.TllFileSerializer_ERR_wrongChecksum;
    private readonly string ERR_dupeChannel = Resource.TllFileSerializer_ERR_dupeChannel;
    private const ushort RadioMask = 0x4000;

    private readonly byte[] fileContent;
    private readonly ModelConstants c;
    private readonly ActChannelDataMapping actMapping;
    private readonly DvbsChannelDataMapping dvbsMapping;
    private readonly FirmwareDataMapping firmwareMapping;
    private int analogBlockOffset;
    private int firmwareBlockOffset;
    private int dvbctBlockOffset;
    private int dvbsBlockOffset;
    private int dvbsSatListOffset;
    private int dvbsTransponderSubblockOffset;
    private int dvbsChannelHeaderOffset;
    private int dvbsChannelCount;
    private int dvbsChannelListOffset;
    private int[] dvbsSubblockCrcOffset;
    private int dvbsLnbSubblockOffset;
    private int settingsBlockOffset;
    private int duplicateChannels;
    private int deletedChannelsHard;
    private int deletedChannelsSoft;

    private bool isDvbsSymbolRateDiv2;
    private UnsortedChannelMode unsortedChannelMode;
    private Dictionary<int, int> nextChannelIndex;
    private int settingsBlockLength;
    private int dvbsChannelLinkedListOffset;
    private string countryCode;
    public IList<string> SupportedTvCountryCodes { get; private set; }

    #region ctor()
    public TllFileSerializer(string inputFile, ModelConstants model, 
      ActChannelDataMapping actMapping,
      DvbsChannelDataMapping dvbsMapping,
      FirmwareDataMapping firmwareMapping,
      byte[] fileContent) : base(inputFile)
    {
      this.c = model;
      this.actMapping = actMapping;
      this.dvbsMapping = dvbsMapping;
      this.firmwareMapping = firmwareMapping;
      this.fileContent = fileContent;

      this.SupportsChannelNameEdit = true;
      this.SupportsEraseChannelData = true;
      this.SupportedTvCountryCodes = new List<string>
                                       {
                                         "___ (None)", "AUT (Austria)", "BEL (Belgium)", "CHE (Switzerland)", 
                                         "DEU (Germany)", "ESP (Spain)", "FRA (France)", "GBR (Great Britain)",
                                         "GRC (Greece)", "IRL (Ireland)", "ITA (Italy)", "LUX (Luxembourg)", 
                                         "NLD (Netherlands)", "PRT (Protugal)", "SVN (Slovenia)"
                                       };
    }
    #endregion

    #region DisplayName
    public override string DisplayName
    {
      get { return "TLL loader " + (this.c == null ? "" : this.c.series); }
    }
    #endregion

    #region Load()
    public unsafe override void Load()
    {
      if (this.dvbsMapping != null)
        this.dvbsMapping.Encoding = this.DefaultEncoding;
      if (this.actMapping != null)
        this.actMapping.Encoding = this.DefaultEncoding;

      fixed (byte* ptr = &fileContent[0])
      {
        // file header
        for (int i = 0; i < c.magicBytes.Length; i++)
        {
          if (ptr[i] != c.magicBytes[i])
            throw new IOException(ERR_badFileHeader);
        }
        byte* p = ptr + c.magicBytes.Length;

        // analog channels
        ChannelBlock* block = (ChannelBlock*)p;
        this.analogBlockOffset = (int)(p - ptr);
        this.ReadAnalogChannels(block);
        p += block->BlockSize + 4;
        
        // firmware data
        this.firmwareBlockOffset = (int) (p - ptr);
        this.FirmwareDataLength = *(uint*) p;
        p += this.FirmwareDataLength + 4;
        
        // dvb-c/t
        block = (ChannelBlock*)p;
        this.dvbctBlockOffset = (int)(p - ptr);
        this.ReadDvbCtChannels(block);
        p += block->BlockSize + 4;

        // models without DVB-S
        if (c.hasDvbSBlock)
        {
          this.dvbsBlockOffset = (int) (p - ptr);
          p = ReadDvbSBlock(ptr, p);
        }

        // settings
        this.settingsBlockOffset = (int) (p - ptr);
        if (this.settingsBlockOffset >= fileContent.Length)
          this.settingsBlockOffset = 0;
        else
// ReSharper disable RedundantAssignment
          p = this.ReadSettingsBlock(p);
// ReSharper restore RedundantAssignment
      }

      this.StoreToDatabase();
    }
    #endregion

    #region ReadAnalogChannels()
    private unsafe void ReadAnalogChannels(ChannelBlock* block)
    {
      if (block->ChannelCount == 0) return;
      this.actMapping.DataPtr = &block->StartOfChannelList;
      for (int i = 0; i < block->ChannelCount; i++)
      {
        string badFields = this.actMapping.Validate();
        if (badFields != null)
          DataRoot.Warnings.AppendFormat("Inconsistent data in analog dvbsMapping at index {0:d4}: {1}\r\n", i, badFields);

        ChannelInfo ci = new ChannelInfo(SignalSource.AnalogCT, GetSignalType(actMapping.ProgramNr), 
          i, actMapping.ProgramNr & 0x3FFF, actMapping.Name);
        
        ci.FreqInMhz = (decimal)(actMapping.AnalogFrequency)/20;
        ci.Favorites = actMapping.Favorites;
        ci.ChannelOrTransponder = (actMapping.AnalogChannelBand == 0 ? "E" : "S") + actMapping.AnalogChannelNr.ToString("d2");
        ci.Lock = actMapping.Lock;
        ci.Skip = actMapping.Skip;
        ci.Hidden = actMapping.Hide;

        var list = this.DataRoot.GetChannelList(ci.SignalSource, ci.SignalType, true);
        this.DataRoot.AddChannel(list, ci);

        actMapping.DataPtr += block->ChannelLength;
      }
    }

    #endregion

    #region ReadDvbCtChannels()
    private unsafe void ReadDvbCtChannels(ChannelBlock* block)
    {
      if (block->ChannelCount == 0) return;
      this.actMapping.DataPtr = &block->StartOfChannelList;
      for (int i = 0; i < block->ChannelCount; i++)
      {
        string badFields = this.actMapping.Validate();
        if (badFields != null)
          DataRoot.Warnings.AppendFormat("Inconsistent data in DVB-C/T dvbsMapping at index {0:d4}: {1}\r\n", i, badFields);

        ChannelInfo ci = new ChannelInfo(SignalSource.DvbCT, this.GetSignalType(actMapping.ProgramNr), i, actMapping.ProgramNr & 0x3FFF, actMapping.Name);
        ci.ShortName = actMapping.ShortName;
        ci.ChannelOrTransponder = actMapping.ChannelOrTransponder.ToString("d2");
        ci.FreqInMhz = (decimal)actMapping.FrequencyLong / 1000;
        ci.ServiceId = actMapping.ServiceId;
        ci.VideoPid = actMapping.VideoPid;
        ci.AudioPid = actMapping.AudioPid;
        ci.OriginalNetworkId = actMapping.OriginalNetworkId;
        ci.TransportStreamId = actMapping.TransportStreamId;
        ci.ServiceType = actMapping.ServiceType;
        ci.Lock = actMapping.Lock;
        ci.Skip = actMapping.Skip;
        ci.Hidden = actMapping.Hide;        
        ci.Favorites = actMapping.Favorites;

        var list = this.DataRoot.GetChannelList(ci.SignalSource, ci.SignalType, true);        
        this.DataRoot.AddChannel(list, ci);

        actMapping.DataPtr += block->ChannelLength;
      }
    }
    #endregion

    #region ReadDvbSBlock()
    private unsafe byte* ReadDvbSBlock(byte* rootPtr, byte* p)
    {
      var satBlock = (DvbSBlockHeader*)p;

      var endPtr = this.ScanDvbSSubBlockChecksums(rootPtr, (byte*)&satBlock->Crc32ForSubblock1);
      if ((int)(endPtr - p) - 4 != satBlock->BlockSize)
        throw new IOException("ERR_invalidDvbSBlockLen");

      // subblock 0 + 1 (header + satellites)
      p = satBlock->SatOrder + c.satCount + 2;
      this.dvbsSatListOffset = (int)(p - rootPtr);
      this.ReadSatellites((TllSatellite*)p);
      p += c.satCount * c.satLength;

      // subblock 2 (tllTransponder)
      this.dvbsTransponderSubblockOffset = (int)(p - rootPtr);
      p += c.sizeOfTransponderBlockHeader;
      this.ReadTransponderData((TllTransponder*)p);
      p += c.transponderCount * c.transponderLength;

      // subblock 3 (channels)
      this.dvbsChannelHeaderOffset = (int) (p - rootPtr);
      SatChannelListHeader* header = (SatChannelListHeader*)p;
      this.dvbsChannelCount = header->ChannelCount;
      p += sizeof(SatChannelListHeader);
      p += c.dvbsMaxChannelCount/8; // skip allocation bitmap
      this.dvbsChannelLinkedListOffset = (int) (p - rootPtr);
      this.ReadDvbsChannelLinkedList(p);
      p += c.dvbsMaxChannelCount * c.sizeOfZappingTableEntry; // linked dvbsMapping list
      this.dvbsChannelListOffset = (int)(p - rootPtr);
      this.ReadDvbSChannels(p, header->LinkedListStartIndex);
      p += c.dvbsMaxChannelCount * c.dvbsChannelLength;

      // subblock 4 (LNB)
      this.dvbsLnbSubblockOffset = (int) (p - rootPtr);

      return p + sizeof(LnbBlockHeader) + c.lnbCount*c.lnbLength;
    }

    #endregion

    #region ScanDvbSSubBlockChecksums()
    private unsafe byte* ScanDvbSSubBlockChecksums(byte* rootPtr, byte* p)
    {
      this.dvbsSubblockCrcOffset = new int[c.dvbsSubblockLength.Length];
      for (int i = 0; i < dvbsSubblockCrcOffset.Length; i++)
      {
        this.dvbsSubblockCrcOffset[i] = (int)(p - rootPtr);
        int subblockLength = c.dvbsSubblockLength[i];
        uint fileCrc = *(uint*)p;
        uint calcCrc = Crc32.CalcCrc32(p + 4, subblockLength);
        if (fileCrc != calcCrc)
          throw new IOException(string.Format(ERR_wrongChecksum, calcCrc, fileCrc));
        p += 4 + subblockLength;
      }
      return p;
    }
    #endregion

    #region ReadSatellites()
    private unsafe void ReadSatellites(TllSatellite* satellite)
    {
      for (int i = 0; i < c.satCount; i++)
      {
        char[] satName = new char[50];
        fixed (char* ptrSatName = satName)
        {
          Encoding.ASCII.GetDecoder().GetChars(satellite->Name, TllSatellite.SatNameLength, ptrSatName, satName.Length, true);
        }
        Satellite sat = new Satellite(i);
        sat.Name = new string(satName).TrimEnd('\0');
        sat.OrbitalPosition = GetSatLocation(*satellite);

        this.DataRoot.AddSatellite(sat);
        satellite = (TllSatellite*) ((byte*) satellite + c.satLength);
      }
    }
    #endregion

    #region ReadTransponderData()
    private unsafe void ReadTransponderData(TllTransponder* tllTransponder)
    {
      for (int i=0; i<c.transponderCount; i++)
      {
        if (tllTransponder->SatIndex == 0xFF)
          continue;
#if FIX_SYMBOL_RATE
        ushort sr = (ushort)(tllTransponder->SymbolRate & 0x7FFF);
        if (sr % 100 >= 95)
          tllTransponder->SymbolRate = (ushort)((tllTransponder->SymbolRate & 0x8000) | ((sr / 100 + 1) * 100));
#endif

        Transponder transponder = new Transponder(i);
        transponder.FrequencyInMhz = tllTransponder->Frequency;
        transponder.TransportStreamId = tllTransponder->TransportStreamId;
        transponder.SymbolRate = this.GetDvbsSymbolRate(tllTransponder);
        transponder.OriginalNetworkId = tllTransponder->NetworkId;
        if (tllTransponder->SymbolRate == 11000)
          this.isDvbsSymbolRateDiv2 = true;

        var sat = this.DataRoot.Satellites.TryGet(tllTransponder->SatIndex/2);
        this.DataRoot.AddTransponder(sat, transponder);

        tllTransponder = (TllTransponder*)((byte*)tllTransponder + c.transponderLength);
      }
    }
    #endregion

    #region ReadDvbsChannelLinkedList()
    private unsafe void ReadDvbsChannelLinkedList(byte* p)
    {
      this.nextChannelIndex = new Dictionary<int, int>();
      for (int i = 0; i < c.dvbsMaxChannelCount; i++)
      {
        short* node = (short*) (p + i*c.sizeOfZappingTableEntry);
        if (node[2] != i)
          break;
        this.nextChannelIndex.Add(node[2], node[1]);
      }
    }
    #endregion

    #region ReadDvbSChannels()
    private unsafe void ReadDvbSChannels(byte* channelRoot, int startIndex)
    {
      int index = startIndex;
      for (int i = 0; i < this.dvbsChannelCount; i++)
      {
        this.dvbsMapping.DataPtr = channelRoot + index * dvbsMapping.DataLength;
        string badFields = this.dvbsMapping.Validate();
        if (badFields != null)
          DataRoot.Warnings.AppendFormat("Inconsistent data in DVB-S dvbsMapping at index {0:d4}: {1}\r\n", i, badFields);

        if (dvbsMapping.ProgramNr == 0xFFFF)
          break;

        if (dvbsMapping.InUse)
        {
          if (dvbsMapping.IsDeleted)
            ++this.deletedChannelsSoft;
          else
            this.ReadDvbsChannel(index, i);
        }
        else
          ++this.deletedChannelsHard;

        if (!this.nextChannelIndex.TryGetValue(index, out index) || index == -1)
          break;
      }
    }
    #endregion

    #region ReadDvbsChannel()
    private void ReadDvbsChannel(int index, int order)
    {
      Transponder transponder = this.DataRoot.Transponder.TryGet(dvbsMapping.TransponderIndex);
      Satellite sat = transponder.Satellite;
      ChannelInfo ci = new ChannelInfo(SignalSource.DvbS, this.GetSignalType(dvbsMapping.ProgramNr),
                                       index, dvbsMapping.ProgramNr & 0x3FFF, dvbsMapping.Name);

      ci.Satellite = sat.Name;
      ci.SatPosition = sat.OrbitalPosition;
      ci.RecordOrder = order;
      ci.ShortName = dvbsMapping.ShortName;
      ci.ServiceId = dvbsMapping.ServiceId;
      ci.VideoPid = dvbsMapping.VideoPid & 0x3FFF;
      ci.AudioPid = dvbsMapping.AudioPid;
      ci.ServiceType = dvbsMapping.ServiceType;
      ci.Lock = dvbsMapping.Lock;
      ci.Skip = dvbsMapping.Skip;
      ci.Hidden = dvbsMapping.Hide;
      ci.Favorites = dvbsMapping.Favorites;
      ci.Encrypted = dvbsMapping.Encrypted;
      ci.TransportStreamId = transponder.TransportStreamId;
      ci.OriginalNetworkId = transponder.OriginalNetworkId;
      ci.SymbolRate = transponder.SymbolRate;
      ci.Polarity = transponder.Polarity;
      ci.FreqInMhz = transponder.FrequencyInMhz;
      ci.ChannelOrTransponder = this.GetTransponderChannelNumber((ushort)transponder.FrequencyInMhz);

      var list = this.DataRoot.GetChannelList(ci.SignalSource, ci.SignalType, true);
      var dupes = list.GetChannelByUid(ci.Uid);
      if (dupes.Count > 0)
      {
        // duplicate channels (ONID,TSID,SSID) cause the TV to randomly reorder channels and show wrong ones in the 
        // program list, so we erase all dupes here
        this.DataRoot.Warnings.AppendFormat(ERR_dupeChannel, ci.RecordIndex, ci.OldProgramNr, dupes[0].RecordIndex, dupes[0].OldProgramNr, dupes[0].Name).AppendLine();
        this.EraseDuplicateDvbsChannel();
        ++this.duplicateChannels;
      }
      else
        this.DataRoot.AddChannel(list, ci);
    }
    #endregion

    #region EraseDuplicateDvbsChannel()
    private unsafe void EraseDuplicateDvbsChannel()
    {
      byte* p = this.dvbsMapping.DataPtr;
      for (int c = this.dvbsMapping.DataLength - 1; c >= 0; c--)
        *p++ = 0xFF;
    }
    #endregion

    #region GetTransponderChannelNumber()
    private string GetTransponderChannelNumber(ushort frequency)
    {
      int nr = LookupData.Instance.GetTransponderNumber(frequency);
      return nr <= 0 ? "" : nr.ToString("d3");
    }
    #endregion

    #region GetSatLocation()
    private string GetSatLocation(TllSatellite sat)
    {
      return string.Format("{0}.{1}{2}", sat.PosDeg, sat.PosCDeg & 0x0f, sat.PosCDeg < 16 ? "W" : "E");
    }
    #endregion

    #region GetSignalType()
    private SignalType GetSignalType(uint programNr)
    {
      if ((programNr & RadioMask) != 0)
        return SignalType.Radio;
      return SignalType.Tv;
    }
    #endregion

    #region GetDvbsSymbolRate(), SetDvbsSymbolRate()
    private unsafe ushort GetDvbsSymbolRate(TllTransponder* data)
    {
      ushort value = (ushort)(data->SymbolRate & 0x7fff);
      return value <= 20000 ? (ushort)(value << 1) : value;
    }
    #endregion

    #region ReadSettingsBlock()
    private unsafe byte * ReadSettingsBlock(byte* ptr)
    {
      this.settingsBlockLength = *(int*)ptr;
      byte* p = ptr + 4;
      if (settingsBlockLength >= 8)
      {
        StringBuilder code = new StringBuilder();
        for (int i = 6; i >= 4; i--)
          code.Append((char) p[i]);
        this.countryCode = code.ToString();
      }
      return ptr + 4 + settingsBlockLength;
    }
    #endregion


    #region Save()
    public unsafe override void Save(string tvOutputFile, string csvOutputFile, UnsortedChannelMode unsortedChannelsMode)
    {
      this.unsortedChannelMode = unsortedChannelsMode;

      // update in-memory file content
      fixed (byte* ptrFileContent = this.fileContent)
      {
        WriteAnalogChannels(ptrFileContent);
        WriteDvbCtChannels(ptrFileContent);
        WriteDvbSChannels(ptrFileContent);
      }

      using (var file = new FileStream(tvOutputFile, FileMode.Create, FileAccess.Write))
      {
        fixed (byte* ptr = fileContent)
        {
          int[] blockOffsets =
            {
              this.analogBlockOffset, this.firmwareBlockOffset, this.dvbctBlockOffset,
              this.dvbsBlockOffset, this.settingsBlockOffset
            };

          // header
          file.Write(this.fileContent, 0, this.analogBlockOffset);

          // data blocks
          foreach (var blockOffset in blockOffsets)
          {
            if (blockOffset == 0) continue;
            ChannelBlock* block = (ChannelBlock*) (ptr + blockOffset);
            file.Write(this.fileContent, blockOffset, (int)block->BlockSize + 4);
          }
        }
      }
    }
    #endregion

    private unsafe delegate void ChannelUpdateFunc(ushort slot, ChannelInfo appChannel, byte* tllChannel);

    #region WriteAnalogChannels()
    private unsafe void WriteAnalogChannels(byte* ptrFileContent)
    {
      if (c.actChannelLength == 0) return;
      ChannelBlock* block = (ChannelBlock*) (ptrFileContent + this.analogBlockOffset);
      byte* ptr = &block->StartOfChannelList;

      this.WriteChannels(SignalSource.AnalogCT, ptr, (uint)c.actChannelLength, this.UpdateACTChannel);
    }
    #endregion

    #region WriteDvbCtChannels()
    private unsafe void WriteDvbCtChannels(byte* ptrFileContent)
    {
      ChannelBlock* block = (ChannelBlock*)(ptrFileContent + this.dvbctBlockOffset);
      byte* ptr = &block->StartOfChannelList;
      this.WriteChannels(SignalSource.DvbCT, ptr, (uint)c.actChannelLength, this.UpdateACTChannel);
    }
    #endregion

    #region UpdateACTChannel()
    private unsafe void UpdateACTChannel(ushort slot, ChannelInfo appChannel, byte* actChannel)
    {
      this.UpdateRawChannel(this.actMapping, actChannel, slot, appChannel);
    }
    #endregion
    
    #region WriteDvbSChannels()
    private unsafe void WriteDvbSChannels(byte* ptrFileContent)
    {
      if (c.dvbsChannelLength == 0 || !c.hasDvbSBlock) return;
      byte* ptr = ptrFileContent + this.dvbsChannelListOffset;     
      this.WriteChannels(SignalSource.DvbS, ptr, (uint)c.dvbsChannelLength, this.UpdateDvbSChannel);

      // update checksums
      for(int i=0; i<this.dvbsSubblockCrcOffset.Length; i++)
      {
        uint crc32 = Crc32.CalcCrc32(ptrFileContent + this.dvbsSubblockCrcOffset[i] + 4, c.dvbsSubblockLength[i]);
        *(uint*)(ptrFileContent + this.dvbsSubblockCrcOffset[i]) = crc32;
      }
    }
    #endregion

    #region UpdateDvbSChannel()
    private unsafe void UpdateDvbSChannel(ushort slot, ChannelInfo appChannel, byte* ptr)
    {
      this.UpdateRawChannel(this.dvbsMapping, ptr, slot, appChannel);
    }
    #endregion

    #region UpdateRawChannel()
    private unsafe void UpdateRawChannel(ChannelMappingBase mapping, byte* ptr, ushort progNr, ChannelInfo appChannel)
    {
      mapping.DataPtr = ptr;
      mapping.ProgramNr = progNr;
      mapping.Favorites = appChannel.Favorites;
      mapping.Lock = appChannel.Lock;
      mapping.Skip = appChannel.Skip;
      mapping.Hide = appChannel.Hidden;
      if (appChannel.IsNameModified)
      {
        mapping.Name = appChannel.Name;
        appChannel.IsNameModified = false;
      }
      bool deleted = (progNr & 0x3FFF) == 0;
      mapping.IsDeleted = deleted;
      if (deleted)
      {
        mapping.Lock = false;
        mapping.Skip = false;
        mapping.Hide = false;
        mapping.Favorites = 0;
      }
    }
    #endregion

    #region WriteChannels()
    private unsafe void WriteChannels(SignalSource source, byte* channelDataBase, uint channelDataLength,  ChannelUpdateFunc updateFunc)
    {
      int baseIndex = 0;
      foreach (var list in DataRoot.ChannelLists)
      {
        if (list.SignalSource != source)
          continue;
        var sortedChannels = list.Channels.OrderBy(ChanSortCriteria).ToList();
        ushort maxSlot = 0;

        foreach (var appChannel in sortedChannels)
        {
          if (appChannel.RecordIndex < 0)
            continue;
          
          var slot = GetNewProgramNr(appChannel, ref maxSlot);
          byte* tllChannel = channelDataBase + appChannel.RecordIndex * channelDataLength;
          updateFunc(slot, appChannel, tllChannel);

          foreach (var channel in appChannel.Duplicates)
          {
            byte* tllChannel2 = channelDataBase + channel.RecordIndex*channelDataLength;
            updateFunc(0, channel, tllChannel2);
          }
        }

        if (source != SignalSource.DvbS && this.actMapping != null && this.actMapping.ReorderChannelData)
          this.ReorderChannelData(channelDataBase, channelDataLength, sortedChannels, baseIndex);

        baseIndex += list.Count;
      }
    }

    #endregion

    #region ChanSortCriteria()
    private string ChanSortCriteria(ChannelInfo channel)
    {
      // explicitly sorted
      if (channel.NewProgramNr!=0)
        return channel.NewProgramNr.ToString("d4");

      // eventually hide unsorted channels
      if (this.unsortedChannelMode == UnsortedChannelMode.Hide)
      {
        return "Z";
      }

      // eventually append in old order
      if (this.unsortedChannelMode == UnsortedChannelMode.AppendInOrder)
        return "B" + channel.OldProgramNr.ToString("d4");

      // sort alphabetically, with "." and "" on the bottom
      if (channel.Name == ".")
        return "B";
      if (channel.Name == "")
      return "C";
      return "A" + channel.Name;
    }
    #endregion

    #region GetNewProgramNr()
    private ushort GetNewProgramNr(ChannelInfo appChannel, ref ushort maxSlot)
    {
      ushort slot = (ushort)appChannel.NewProgramNr;
      if (slot > maxSlot)
        maxSlot = slot;
      if (slot == 0)
      {
        if (appChannel.OldProgramNr != 0 && this.unsortedChannelMode != UnsortedChannelMode.Hide)
          slot = ++maxSlot;
      }
      appChannel.OldProgramNr = slot;
      if (appChannel.SignalType == SignalType.Radio)
        slot |= RadioMask;
      return slot;
    }
    #endregion

    #region ReorderChannelData()
    private unsafe void ReorderChannelData(byte* channelDataBase, uint channelDataLength, IList<ChannelInfo> sortedList, int baseIndex)
    {
      channelDataBase += baseIndex * channelDataLength;
      byte[] copy = new byte[sortedList.Count * channelDataLength];
      for (int i = 0; i < copy.Length; i++)
        copy[i] = channelDataBase[i];

      fixed (byte* pCopy = copy)
      {
        byte* pTarget = channelDataBase;
        for (int index = 0; index < sortedList.Count; index++, pTarget += channelDataLength)
        {
          ChannelInfo appChannel = sortedList[index];
          if (appChannel.RecordIndex == baseIndex + index)
            continue;

          byte* pSource = pCopy + (appChannel.RecordIndex - baseIndex)*channelDataLength;
          for (int i = 0; i < channelDataLength; i++)
            pTarget[i] = pSource[i];

          appChannel.RecordIndex = baseIndex + index;
        }
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
        if (this.dvbsMapping != null)
          this.dvbsMapping.Encoding = value;
        if (this.actMapping != null)
          this.actMapping.Encoding = value;
        if (this.DataRoot.IsEmpty)
          return;
        ChangeEncoding();
      }
    }
    #endregion

    #region ChangeEncoding()

    private unsafe void ChangeEncoding()
    {
      fixed (byte* ptr = this.fileContent)
      {
        byte* analogChannels = &((ChannelBlock*)(ptr + this.analogBlockOffset))->StartOfChannelList;
        byte* dvbctChannels = &((ChannelBlock*)(ptr + this.dvbctBlockOffset))->StartOfChannelList;
        byte* dvbsChannels = (ptr + this.dvbsChannelListOffset);

        foreach (var list in DataRoot.ChannelLists)
        {
          foreach (var channel in list.Channels)
          {
            switch (channel.SignalSource)
            {
              case SignalSource.AnalogCT:
                ChangeEncodingForChannels(channel, this.actMapping, analogChannels);
                break;
              case SignalSource.DvbCT:
                ChangeEncodingForChannels(channel, this.actMapping, dvbctChannels);
                break;
              case SignalSource.DvbS:
                ChangeEncodingForChannels(channel, this.dvbsMapping, dvbsChannels);
                break;
            }
          }
        }
      }
    }

    private unsafe void ChangeEncodingForChannels(ChannelInfo appChannel, ChannelMappingBase mapping, byte* channelList)
    {
      if (appChannel.RecordIndex >= c.dvbsMaxChannelCount)
        return;

      mapping.DataPtr = channelList + appChannel.RecordIndex*mapping.DataLength;
      appChannel.Name = mapping.Name;
      appChannel.ShortName = mapping.ShortName;
    }
    #endregion


    #region EraseChannelData()
    public unsafe override void EraseChannelData()
    {
      this.DataRoot = new DataRoot();
      fixed (byte* ptr = this.fileContent)
      {
        // analog channels
        ChannelBlock* block = (ChannelBlock*) (ptr + this.analogBlockOffset);
        block->BlockSize = 4;
        block->ChannelCount = 0;

        // DVB-C/T channels
        block = (ChannelBlock*)(ptr + this.dvbctBlockOffset);
        block->BlockSize = 4;
        block->ChannelCount = 0;

        if (this.dvbsChannelListOffset != 0)
        {
          EraseTransponders(ptr);
          EraseSatChannels(ptr);
          EraseLnbSettings(ptr);
        }

        this.Save(this.FileName, null, UnsortedChannelMode.Hide);
      }      
    }
    #endregion

    #region EraseTransponders()
    private unsafe void EraseTransponders(byte* ptr)
    {
      // clear DVB-S sat table tranponder counts
      byte* p = ptr + this.dvbsSatListOffset;
      for (int count = c.satCount; count > 0; count--)
      {
        TllSatellite* sat = (TllSatellite*) p;
        sat->Unknown_36 = 0xffff;
        sat->Unknown_38 = 0xffff;
        sat->TransponderCount = 0;
        sat->Unknown_42 = 0;
        p += c.satLength;
      }

      // clear DVB-S tllTransponder table
      p = ptr + this.dvbsTransponderSubblockOffset;
      TransponderBlockHeader* tpHeader = (TransponderBlockHeader*) p;
      tpHeader->Unknown1 = 0;
      tpHeader->HeadIndex = 0;
      tpHeader->TailIndex1 = 0;
      tpHeader->TailIndex2 = 0;
      tpHeader->TransponderCount = 0;
      for (int i = 0; i < c.transponderCount/8; i++)
        tpHeader->AllocationBitmap[i] = 0;
      tpHeader->TransponderLinkedList[0] = 0xFFFF;
      tpHeader->TransponderLinkedList[1] = 0xFFFF;
      tpHeader->TransponderLinkedList[3] = 0x0000;
      for (int i = 3, count = c.transponderCount*3; i < count; i++)
        tpHeader->TransponderLinkedList[i] = 0;
      tpHeader->Unknown3 = 0;
      p = (byte*) (&tpHeader->Unknown3 + 1);
      for (int i = 0, count = c.transponderCount*c.transponderLength; i < count; i++)
        p[i] = 0xff;
      for (int i = 0; i < c.transponderCount; i++)
      {
        int off = i*c.transponderLength;
        p[off + 4] = 0;
        p[off + 5] = 0;
        p[off + 8] = 0xFE;
      }
    }

    #endregion

    #region EraseSatChannels()
    private unsafe void EraseSatChannels(byte* ptr)
    {
      SatChannelListHeader* header = (SatChannelListHeader*)(ptr + this.dvbsChannelHeaderOffset);
      header->ChannelCount = 0;
      header->LinkedListEndIndex1 = 0;
      header->LinkedListEndIndex2 = 0;

      // clear DVB-S dvbsMapping allocation table
      byte* p = ptr + this.dvbsChannelHeaderOffset + sizeof(SatChannelListHeader);
      for (int count = c.dvbsMaxChannelCount / 8; count > 0; count--)
        *p++ = 0;

      // clear DVB-S dvbsMapping linked list
      for (int i = 0; i < 4; i++)
        *p++ = 0xFF;
      for (int count = c.dvbsMaxChannelCount * 8 - 4; count > 0; count--)
        *p++ = 0;

      // clear DVB-S dvbsMapping data
      p = ptr + this.dvbsChannelListOffset;
      for (int count = c.dvbsMaxChannelCount * c.dvbsChannelLength; count > 0; count--)
        *p++ = 0xFF;
    }
    #endregion

    #region EraseLnbSettings()
    private unsafe void EraseLnbSettings(byte* ptr)
    {
      // clear LNB data (except for first - otherwise TV crashes)
      byte* p = ptr + this.dvbsLnbSubblockOffset;
      LnbBlockHeader* lnbHeader = (LnbBlockHeader*)p;
      for (int i = 0; i < c.lnbCount / 8; i++)
        lnbHeader->lnbAllocationBitmap[i] = 0;
      lnbHeader->lnbAllocationBitmap[0] = 0x01;
      lnbHeader->lastUsedIndex = 0;
      p += sizeof(LnbBlockHeader);
      p += c.lnbLength;
      for (int count = (c.lnbCount - 1) * c.lnbLength; count > 0; count--)
        *p++ = 0;
    }
    #endregion

    // Testing

    #region GetHotelMenuOffset()
    public unsafe int GetHotelMenuOffset()
    {
      fixed (byte* ptr = this.fileContent)
      {
        byte* p = ptr + this.firmwareBlockOffset;
        for (int i = 6500; i < this.FirmwareDataLength - 3; i++)
        {
          if (*(uint*)(p+i) == 0x05000101) // 1,1,0,5
          {
            for (int j = 5; j < 20; j++) // backtrack to find Volume/MaxVolue pattern
            {
              if (*(p + i - j) == 101 && *(p + i - j - 6) == 100) // check for Volume/MaxVolue to be 101/100
                return this.firmwareBlockOffset + i - j - 15;
            }
            return -1;
          }
        }
        return -1;
      }
    }
    #endregion

    public uint ACTChannelLength { get { return (uint)c.actChannelLength; } }
    public bool HasDvbs { get { return c.hasDvbSBlock; } }
    public int SatChannelLength { get { return c.hasDvbSBlock ? c.dvbsChannelLength : 0; } }
    public bool SatSymbolRateDiv2 { get { return this.isDvbsSymbolRateDiv2; } }

    #region GetFileInformation()
    public unsafe override string GetFileInformation()
    {
      StringBuilder sb = new StringBuilder();

      fixed (byte* ptr = this.fileContent)
      {
        ChannelBlock* block = (ChannelBlock*)(ptr + this.analogBlockOffset);
        sb.AppendLine("ANALOG");
        sb.Append("Number of data records: ").Append(block->ChannelCount).AppendLine();
        sb.Append("Length of data record:  ").Append(block->ChannelLength).AppendLine();
        sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine("DVB-C/T");
        block = (ChannelBlock*)(ptr + this.dvbctBlockOffset);
        sb.Append("Number of data records: ").Append(block->ChannelCount).AppendLine();
        sb.Append("Length of data record:  ").Append(block->ChannelLength).AppendLine();
        sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine("DVB-S");
        if (c.hasDvbSBlock)
        {
          int numberOfDupePrNr;
          CountDuplicateRecords(out numberOfDupePrNr);
          SatChannelListHeader* satHeader = (SatChannelListHeader*) (ptr + dvbsChannelHeaderOffset);
          int numberOfDeletedChannels, numberOfChannelsAtPr0;
          this.CountDeletedDvbsChannels(ptr, satHeader->ChannelCount, out numberOfDeletedChannels, out numberOfChannelsAtPr0);
          sb.Append("Max number of data records:          ").Append(c.dvbsMaxChannelCount).AppendLine();
          sb.Append("Length of data record:               ").Append(c.dvbsChannelLength).AppendLine();
          sb.Append("Channel records in use:              ").Append(satHeader->ChannelCount).AppendLine();
          sb.Append("Channel records marked hard-deleted: ").Append(this.deletedChannelsHard).AppendLine();
          sb.Append("Channel records marked soft-deleted: ").Append(this.deletedChannelsSoft).AppendLine();
          sb.Append("Channel records erased (duplicates): ").Append(this.duplicateChannels).AppendLine();
          sb.Append("Channel records with Pr# 0:          ").Append(numberOfChannelsAtPr0).AppendLine();
          sb.Append("Channel records with duplicate Pr#:  ").Append(numberOfDupePrNr).AppendLine();
        }
        else
          sb.AppendLine("not present");
      }

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

    private unsafe void CountDeletedDvbsChannels(byte* ptr, ushort count, out int deletedChannels, out int channelsAtPr0)
    {
      deletedChannels = 0;
      channelsAtPr0 = 0;
      this.dvbsMapping.DataPtr = ptr + dvbsChannelListOffset;
      for (int i = 0; i < count; i++)
      {
        if (!this.dvbsMapping.InUse)
          ++deletedChannels;
        else if ((this.dvbsMapping.ProgramNr & 0x3FFF) == 0 && !this.dvbsMapping.IsDeleted)
          ++channelsAtPr0;
        this.dvbsMapping.Next();
      }
    }
    #endregion

    #region TvCountryCode
    public unsafe string TvCountryCode
    {
      get { return this.countryCode; }
      set
      {
        if (value.Length < 3 || this.settingsBlockOffset == 0 || this.settingsBlockLength < 8) return;
        value = value.ToUpper();
        fixed (byte* ptr = this.fileContent)
        {
          byte *p = ptr + this.settingsBlockOffset + 4 + 4 + 2;
          for (int i = 0; i < 3; i++)
            *p-- = (byte) value[i];
        }
        this.countryCode = value;
      }
    }
    #endregion

    #region ShowDeviceSettingsForm()
    public override void ShowDeviceSettingsForm(object parentWindow)
    {
      using (var dlg = new TvSettingsForm(this))
      {
        dlg.ShowDialog((Form) parentWindow);
      }
    }
    #endregion


    internal byte[] FileContent { get { return this.fileContent; } }

    internal unsafe FirmwareDataMapping GetFirmwareMapping(byte* ptrFileContent)
    {
      if (this.firmwareMapping == null) return null;
      this.firmwareMapping.DataPtr = ptrFileContent + this.firmwareBlockOffset;
      return this.firmwareMapping;
    }

    internal uint FirmwareDataLength { get; private set; }
    internal int FirmwareOffset { get { return this.firmwareBlockOffset; } }
  }
}
