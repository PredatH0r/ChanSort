using System;
using System.IO.Hashing;

namespace ChanSort.Api;

/// <summary>
/// This class is a wrapper around System.IO.Hashing.Crc32 providing the same interface as the old custom implementation in ChanSort.Api.
/// </summary>
public class Crc32(bool msbFirst = true, uint poly = Crc32.NormalPoly, uint initialValue = Crc32.Init, uint finalXorValue = 0)
{
  public const uint NormalPoly = 0x04C11DB7;
  public const uint ReversedPoly = 0xEDB88320;
  public const uint Init = 0xFFFFFFFF;

  public static Crc32 Normal = new Crc32(true, NormalPoly, Init, 0);
  public static Crc32 Reversed = new Crc32(false, NormalPoly, Init, 0);

  private readonly Crc32ParameterSet parameterSet = Crc32ParameterSet.Create(poly, initialValue, finalXorValue, !msbFirst);

  public uint CalcCrc32(byte[] data, int start = 0, int length = -1)
  {
    if (length == -1)
      length = data.Length - start;
    var hash = new System.IO.Hashing.Crc32(this.parameterSet);
    hash.Append(data.AsSpan(start, length));
    return hash.GetCurrentHashAsUInt32();
  }

  public int Crack(byte[] data, int maxLength, uint crc)
  {
    var hash = new System.IO.Hashing.Crc32(this.parameterSet);
    for (var i = 0; i < maxLength; i++)
    {
      if (hash.GetCurrentHashAsUInt32() == crc)
        return i;
      hash.Append(data.AsSpan(i, 1));
    }

    return -1;
  }
}