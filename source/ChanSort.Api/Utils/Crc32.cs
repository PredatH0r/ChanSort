namespace ChanSort.Api
{
  public class Crc32
  {
    // This implementation is MSB-first based, using left-shift operators and a polynomial of 0x04C11DB7
    // To get the same CRC32 values that an LSB-first implementation would produce,
    // all bits in the input bytes, the polynomial (=> 0xEDB88320) and the resulting crc need to be reversed (msb to lsb)

    public const uint NormalPoly = 0x04C11DB7;
    public const uint ReversedPoly = 0xEDB88320;
    private const uint CrcMask = 0xFFFFFFFF;

    public static Crc32 Normal = new Crc32(true, NormalPoly);
    public static Crc32 Reversed = new Crc32(false, NormalPoly);
    internal static readonly byte[] BitReversedBytes = new byte[256];

    private readonly uint[] crc32Table;
    private readonly bool msbFirst;

    #region static initializers

    static Crc32()
    {
      InitReversedBitOrderTable();
    }

    private static void InitReversedBitOrderTable()
    {
      for (int i = 0; i < 256; i++)
      {
        byte v = 0;
        var m = i;
        for (int j = 0; j < 8; j++)
        {
          v <<= 1;
          if ((m & 1) != 0)
            v |= 0x01;
          m >>= 1;
        }

        BitReversedBytes[i] = v;
      }
    }

    #endregion

    /// <param name="msbFirst">true for using the "left shift" most-significant-bit-first algorithm</param>
    /// <param name="poly"></param>
    public Crc32(bool msbFirst, uint poly)
    {
      this.msbFirst = msbFirst;
      this.crc32Table = InitCrc32Table(poly);
    }

    #region InitCrc32Table()
    private uint[] InitCrc32Table(uint poly)
    {
      var crcTable = new uint[256];
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
