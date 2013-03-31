namespace ChanSort.Plugin.TllFile
{
  public class ModelConstants
  {
    public readonly string series;
    public readonly byte[] magicBytes;

    public int actChannelLength; // auto-detect

    public readonly int satCount;
    public readonly int satLength;
    public readonly int sizeOfTransponderBlockHeader;
    public readonly int transponderCount;
    public readonly int transponderLength;
    public readonly int sizeOfZappingTableEntry = 8;
    public readonly int dvbsMaxChannelCount;
    public readonly int dvbsChannelLength;
    public readonly int lnbCount;
    public readonly int lnbLength;
    public readonly int[] dvbsSubblockLength;

    public bool hasDvbSBlock;
    public int firmwareBlockLength; // auto-detect

    public ModelConstants(Api.IniFile.Section iniSection)
    {
      this.series = iniSection.Name;
      this.magicBytes = iniSection.GetBytes("magicBytes");
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
                                    12, 
                                    14 + 2 + this.satCount + this.satCount*this.satLength, // 2896
                                    sizeOfTransponderBlockHeader - 4 + transponderCount * transponderLength, // 110712
                                    12 + dvbsMaxChannelCount/8 + dvbsMaxChannelCount*sizeOfZappingTableEntry + dvbsMaxChannelCount * dvbsChannelLength, // 602552
                                    8 + lnbCount * lnbLength // 1768
                                  };
    }
  }
}
