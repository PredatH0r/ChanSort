﻿namespace ChanSort.Api
{
  public class Crc32
  {
    // This implementation is MSB-first based, using left-shift operators and a polynomial of 0x04C11DB7
    // To get the same CRC32 values that an LSB-first implementation with polynomial 0xEDB88320 would produce,
    // all bits in the input bytes and the resulting crc need to be reversed (msb to lsb)

    private const uint CrcMask = 0xFFFFFFFF;
    private const uint CrcPoly = 0x04C11DB7;

    public static Crc32 Normal = new Crc32(true);
    public static Crc32 Reversed = new Crc32(false);
    private static readonly byte[] BitReversedBytes = new byte[256];

    private readonly uint[] crc32Table;
    private readonly bool msbFirst;

    #region static initializers

    static Crc32()
    {
      InitCrc32Table();
      InitReversedBitOrderTable();
    }

    private static void InitReversedBitOrderTable()
    {
      for (int i = 0; i < 256; i++)
      {
        byte v = 0;
        for (int j = 0, m = i; j < 8; j++, m >>= 1)
        {
          v <<= 1;
          if ((m & 1) != 0)
            v |= 0x01;
        }

        BitReversedBytes[i] = v;
      }
    }


    private static uint[] InitCrc32Table()
    {
      var crcTable = new uint[256];
      var poly = CrcPoly;
      for (uint i = 0; i < 256; i++)
      {
        uint r = i << 24;
        for (uint j = 0; j < 8; j++)
        {
          if ((r & 0x80000000) != 0)
            r = (r << 1) ^ poly;
          else
            r <<= 1;
        }

        crcTable[i] = r;
      }

      return crcTable;
    }
    #endregion

    /// <param name="msbFirst">true for using the "left shift" MSB-first algorithm with polynomial 0x04C11Db7. false to use "right shift" with polynomial 0xEDB883320</param>
    public Crc32(bool msbFirst = true)
    {
      this.msbFirst = msbFirst;
      crc32Table = InitCrc32Table();
    }

    #region CalcCrc32()
    public uint CalcCrc32(byte[] data, int start, int length)
    {
      uint crc32 = CrcMask;
      for (int i = 0; i < length; i++)
      {
        var b = data[start + i];
        if (!this.msbFirst)
          b = BitReversedBytes[b];
        crc32 = (crc32 << 8) ^ crc32Table[((crc32 >> 24) ^ b) & 0xFF];
      }

      if (this.msbFirst)
        return crc32;

      // reverse all bits to make MSB <-> LSB
      return (uint) BitReversedBytes[crc32 >> 24] | ((uint) BitReversedBytes[(crc32 >> 16) & 0xFF] << 8) | ((uint) BitReversedBytes[(crc32 >> 8) & 0xFF] << 16) | ((uint) BitReversedBytes[crc32 & 0xFF] << 24);
    }
    #endregion

    #region Crack()
#if false
    public static unsafe int Crack(byte* block, int maxLen, uint checksum)
    {
      uint crc32 = CrcMask;
      for (int i = 0; i < maxLen; i++)
      {
        if (crc32 == checksum)
          return i;
        crc32 = (crc32 >> 8) ^ crc32Table[(crc32 >> 24) ^ block[i]];
      }
      return 0;
    }
#endif
    #endregion
  }
}
