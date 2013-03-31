using System.Runtime.InteropServices;

namespace ChanSort.Plugin.ScmFile
{
  [StructLayout(LayoutKind.Sequential, Pack=1)]
  unsafe struct SatDataBase
  {
    public byte Magic0x55;
    public int SatelliteNr;
    public int TransponderCount;
    public fixed ushort Name[64];
    public int IsWest;
    public int LongitudeTimes10;
  };

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  struct TransponderDataBase
  {
    public byte Magic0x55;
    public int TransponderNr;
    public int SatelliteNr;
    public int Frequency;
    public int SymbolRate;
    public int IsVerticalPolarity;
    public int Modulation;
    public int CodeRate;
    public int Unknown1;
    public int Unknown2;
    public int Unknown3;
    public int Unknown4;
  };

#if false
  [StructLayout(LayoutKind.Sequential, Pack=1)]
  unsafe struct MapSateD
  {
    public ushort ChannelNumber;
    public ushort VideoPid;
    public ushort Pid;
    public byte Mpeg4;
    public byte Unknown8;
    public ushort Unknown9;
    public ushort Unknonw11;
    public ushort Unknown13;
    public byte ServiceType;
    public byte Unknown16;
    public ushort ServiceId;
    public ushort TransponderNr;
    public ushort SatelliteNr;
    public ushort Unknown23;
    public ushort TransportStreamId;
    public ushort Unknown27;
    public ushort OriginalNetworkId;
    public ushort Unknown31;
    public ushort HRes;
    public ushort VRes;
    public fixed ushort NameInBigEndianUtf16[51];
    public ushort Bouquet;
    public byte Unknown141;
    public byte Locked;
    public byte Favorites;
    public byte ChecksumCSeries;
    public fixed byte Padding [28];
  }
#endif
#if false
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  unsafe struct MapAirD
  {
    public ushort ChannelNumber;
    public ushort VideoPid;
    public ushort PcrPid;
    public ushort ServiceId;
    public ushort Status;
    public ushort Unknown11;
    public ushort Qam;
    public byte ServiceType;
    public byte Codec;
    public fixed byte Unknown17 [7];
    public byte Encrypted;
    public fixed byte Unknown25 [3];
    public ushort Frequency;
    public ushort LogicalChannelNumber;
    public fixed byte Unknown31 [2];
    public ushort SymbolRate;
    public ushort Bouquet;
    public ushort TransportStreamId;
    public fixed byte Unknown39 [5];
    public fixed ushort NameInBigEndianUtf16[100];
    public byte Unknown244;
    public ushort Locked;
    public byte FavoritesX79;
    public byte ChecksumBSeries;
    public fixed byte PaddingCSeries [292 - 248];
  }
#endif
}
