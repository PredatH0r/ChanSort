namespace ChanSort.Api
{
  public static class Crc32
  {
    private const uint CrcMask = 0xFFFFFFFF;
    private const uint CrcPoly = 0xEDB88320;

    private static readonly uint[] crc32Table;

    static Crc32()
    {
      crc32Table = InitCrc32Table();
    }

    #region InitCrc32Table()

    private static uint[] InitCrc32Table()
    {
      var crcTable = new uint[256];
      for (uint i = 0; i < 256; i++)
      {
        uint r = i;
        for (uint j = 8; j > 0; j--)
        {
          if ((r & 1) == 1)
            r = ((r >> 1) ^ CrcPoly);
          else
            r >>= 1;
        }
        crcTable[i] = r;
      }
      return crcTable;
    }
    #endregion

    #region CalcCrc32()
    public static uint CalcCrc32(byte[] block, int start, int length)
    {
      uint crc32 = CrcMask;
      for (int i = 0; i < length; i++)
        crc32 = crc32Table[(crc32 & 0xff) ^ block[start + i]] ^ (crc32 >> 8);
      return crc32;
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
        crc32 = crc32Table[(crc32 & 0xff) ^ block[i]] ^ (crc32 >> 8);
      }
      return 0;
    }
#endif
    #endregion
  }
}
