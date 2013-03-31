using System.Runtime.InteropServices;

namespace ChanSort.Plugin.TllFile
{
  /*
  TllFileHeader? ("ZZZZ" or nothing)
  ChannelBlock
     AnalogChannel[]
  FirmwareBlock
  ChannelBlock
     DvbCtChannel[]
  DvbSBlockHeader
     TllSatellite[64]
     TransponderBlockHeader
       TllTransponder[2400]
     SatChannelListHeader
       DvbSChannel[7520]
     LnbBlockHeader
       Lnb[40]
  SettingsBlock?
  */

  #region struct ChannelBlock
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct ChannelBlock
  {
    public uint BlockSize; // = 4 + ChannelCount * ChannelLength
    public uint ChannelCount;
    public uint ChannelLength { get { return ChannelCount == 0 ? 0 : (BlockSize - 4) / ChannelCount; } }
    public byte StartOfChannelList;
  }
  #endregion

  #region struct FirmwareBlock
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct FirmwareBlock
  {
    public uint BlockSize;
    public fixed byte Unknown_0x0000[35631];
    public fixed byte HotelMenu[29];
    public fixed byte Unknown_0x8B4C[1204]; // or more
  }
  #endregion


  #region struct DvbSBlockHeader
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct DvbSBlockHeader
  {
    public uint BlockSize;
    public uint Crc32ForSubblock1;
    public fixed byte DvbS_S2 [8]; // "DVBS-S2\0"
    public ushort Unknown_0x10; // 0x0007
    public ushort Unknown_0x12; // 0x0004 // 0x0000

    public uint Crc32ForSubblock2;
    public const int Unknown0x18_Length = 12;
    public fixed byte Unknown_0x18[Unknown0x18_Length];
    public ushort SatOrderLength;
    public fixed byte SatOrder[64];
    public fixed byte Unknown [2];
    //public fixed TllSatellite Satellites[64]
  }
  #endregion

  #region struct TllSatellite
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TllSatellite
  {
    public const int SatNameLength =32;
    
    public fixed byte Name [SatNameLength];
    public byte PosDeg;
    public byte PosCDeg;
    public byte Unknown_34; // typically 0
    public byte Unknown_35; // typically 2
    public ushort Unknown_36; // 0xFFFF if sat is not used
    public ushort Unknown_38; // 0xFFFF if sat is not used
    public ushort TransponderCount;
    public ushort Unknown_42; // typically 0
  }
  #endregion

  #region struct TransponderLinkedList
  public struct TransponderLinkedList
  {
    public ushort Prev;
    public ushort Next;
    public ushort Current;
  }
  #endregion

  #region struct TransponderBlockHeader
  public unsafe struct TransponderBlockHeader
  {
    public uint Crc32;
    public ushort Unknown1;
    public ushort HeadIndex;
    public ushort TailIndex1;
    public ushort TailIndex2;
    public ushort TransponderCount;
    public fixed byte AllocationBitmap [2400/8];
    public fixed ushort TransponderLinkedList [2400*3];
    public ushort Unknown3;
    // public fixed TllTransponder Transponders[2400]
  }
  #endregion

  #region struct TllTransponder
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TllTransponder
  {
    public fixed byte Unknown_0x00 [10];
    public ushort Index;
    public ushort Frequency;
    public fixed byte Unknown_0x0E [4];
    public ushort NetworkId;
    public ushort TransportStreamId;
    public fixed byte Unknown_0x16 [3];
    public ushort SymbolRateAndPolarity; 
    public fixed byte Unknown_0x1B [9];
    public byte SatIndex;
    public fixed byte Unknown_0x25 [3];

    //public int SymbolRate { get { return this.SymbolRateAndPolarity & 0x7FFFF; } }
    public ushort SymbolRate { get { return this.SymbolRateAndPolarity; } set { this.SymbolRateAndPolarity = value; } }
    public char Polarity { get { return '\0'; } }
  }
  #endregion

  #region struct SatChannelListHeader
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct SatChannelListHeader
  {
    public uint Checksum;
    public fixed byte Unknown_0x04[4];
    public ushort LinkedListStartIndex;
    public ushort LinkedListEndIndex1;
    public ushort LinkedListEndIndex2;
    public ushort ChannelCount;
  }
  #endregion

  #region struct LnbBlockHeader
  public unsafe struct LnbBlockHeader
  {
    public uint crc32;
    public ushort lastUsedIndex;
    public fixed byte lnbAllocationBitmap[6];
    // public Lnb lnbs[40];
  }
  #endregion

  #region struct Lnb
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct Lnb
  {
    public byte SettingsID;
    public fixed byte Unknown_0x0D[3];
    public byte SatelliteID; 
    public fixed byte Unknown_0x11[3];
    public fixed byte FrequencyName[12];
    public ushort LOF1;
    public fixed byte Unknown_0x22 [2];
    public ushort LOF2;
    public fixed byte Unknown_0x26 [18];
  }
  #endregion
}
