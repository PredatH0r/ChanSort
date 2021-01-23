namespace ChanSort.Loader.LG.Binary
{
  public class DvbsDataLayout
  {
    public readonly int satCount;
    public readonly int satLength;
    public readonly int sizeOfTransponderBlockHeader;
    public readonly int transponderCount;
    public readonly int transponderLength;
    public readonly int sizeOfChannelLinkedListEntry = 8;
    public readonly int linkedListExtraDataLength;
    public readonly int dvbsMaxChannelCount;
    public readonly int dvbsChannelLength;
    public readonly int lnbCount;
    public readonly int lnbLength;
    public readonly int[] dvbsSubblockLength;
    public readonly int dvbsBlockTotalLength;
    public readonly int satIndexFactor;

    public int LnbBlockHeaderSize = 12;

    public DvbsDataLayout(Api.IniFile.Section iniSection)
    {
      this.satCount = iniSection.GetInt("satCount");
      this.satLength = iniSection.GetInt("satLength");
      this.transponderCount = iniSection.GetInt("transponderCount");
      this.transponderLength = iniSection.GetInt("transponderLength");
      this.sizeOfTransponderBlockHeader = 14 + transponderCount/8 + transponderCount*6 + 2;
      this.linkedListExtraDataLength = iniSection.GetInt("linkedListExtraDataLength");
      this.dvbsMaxChannelCount = iniSection.GetInt("dvbsChannelCount");
      this.dvbsChannelLength = iniSection.GetInt("dvbsChannelLength");
      this.lnbCount = iniSection.GetInt("lnbCount");
      this.lnbLength = iniSection.GetInt("lnbLength");
      this.satIndexFactor = iniSection.GetInt("satIndexFactor");
      if (satIndexFactor == 0)
        satIndexFactor = 2;

      this.dvbsSubblockLength = new[]
                                  {
                                    12, // header
                                    14 + 2 + this.satCount + this.satCount*this.satLength, // satellites
                                    sizeOfTransponderBlockHeader - 4 + transponderCount * transponderLength, // transponder
                                    12 + dvbsMaxChannelCount/8 + dvbsMaxChannelCount*sizeOfChannelLinkedListEntry + linkedListExtraDataLength + dvbsMaxChannelCount * dvbsChannelLength, // channels
                                    LnbBlockHeaderSize - 4 + lnbCount * lnbLength // sat/LNB-Config
                                  };
      
      foreach (int len in this.dvbsSubblockLength)
        this.dvbsBlockTotalLength += len + 4;
    }

    /// <summary>
    /// relative to start of DVBS-Block (including the intial 4 length bytes)
    /// </summary>
    public int TransponderTableOffset
    {
      get { return 4 + 4 + dvbsSubblockLength[0] + 4 + dvbsSubblockLength[1] + sizeOfTransponderBlockHeader; }
    }

    /// <summary>
    /// relative to start of DVBS-Block (including the intial 4 length bytes)
    /// </summary>
    public int ChannelListHeaderOffset
    {
      get { return 4 + 4 + this.dvbsSubblockLength[0] + 4 + this.dvbsSubblockLength[1] + 4 + this.dvbsSubblockLength[2]; }
    }

    /// <summary>
    /// relative to start of DVBS-Block (including the intial 4 length bytes)
    /// </summary>
    public int AllocationBitmapOffset { get { return ChannelListHeaderOffset + 16; } }

    /// <summary>
    /// relative to start of DVBS-Block (including the intial 4 length bytes)
    /// </summary>
    public int SequenceTableOffset { get { return this.AllocationBitmapOffset + dvbsMaxChannelCount/8; } }

    /// <summary>
    /// relative to start of DVBS-Block (including the intial 4 length bytes)
    /// </summary>
    public int ChannelListOffset
    {
      get { return SequenceTableOffset + dvbsMaxChannelCount*sizeOfChannelLinkedListEntry + linkedListExtraDataLength; }
    }


  }
}
