namespace ChanSort.Plugin.TllFile
{
  public class DvbsDataLayout
  {
    public readonly int satCount;
    public readonly int satLength;
    public readonly int sizeOfTransponderBlockHeader;
    public readonly int transponderCount;
    public readonly int transponderLength;
    public readonly int sizeOfChannelIndexTableEntry = 8;
    public readonly int dvbsMaxChannelCount;
    public readonly int dvbsChannelLength;
    public readonly int lnbCount;
    public readonly int lnbLength;
    public readonly int[] dvbsSubblockLength;

    public int LnbBlockHeaderSize = 12;

    public DvbsDataLayout(Api.IniFile.Section iniSection)
    {
      this.satCount = iniSection.GetInt("satCount");
      this.satLength = iniSection.GetInt("satLength");
      this.transponderCount = iniSection.GetInt("transponderCount");
      this.transponderLength = iniSection.GetInt("transponderLength");
      this.sizeOfTransponderBlockHeader = 14 + transponderCount/8 + transponderCount*6 + 2;
      this.dvbsMaxChannelCount = iniSection.GetInt("dvbsChannelCount");
      this.dvbsChannelLength = iniSection.GetInt("dvbsChannelLength");
      this.lnbCount = iniSection.GetInt("lnbCount");
      this.lnbLength = iniSection.GetInt("lnbLength");

      this.dvbsSubblockLength = new[]
                                  {
                                    12, // header
                                    14 + 2 + this.satCount + this.satCount*this.satLength, // satellites
                                    sizeOfTransponderBlockHeader - 4 + transponderCount * transponderLength, // transponder
                                    12 + dvbsMaxChannelCount/8 + dvbsMaxChannelCount*sizeOfChannelIndexTableEntry + dvbsMaxChannelCount * dvbsChannelLength, // channels
                                    LnbBlockHeaderSize - 4 + lnbCount * lnbLength // sat/LNB-Config
                                  };
    }
  }
}
