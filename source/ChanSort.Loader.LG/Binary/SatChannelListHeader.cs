using System;
using ChanSort.Api;

namespace ChanSort.Loader.LG.Binary
{
  internal class SatChannelListHeader
  {
    private readonly byte[] data;
    private readonly int baseOffset;

    public SatChannelListHeader(byte[] data, int offset)
    {
      this.data = data; this.baseOffset = offset;
    }

    public uint Checksum { get { return BitConverter.ToUInt32(data, baseOffset + 0); } }

    public ushort LinkedListStartIndex
    {
      get { return BitConverter.ToUInt16(data, baseOffset + 8); }
      set { Tools.SetInt16(data, baseOffset + 8, value); }
    }
    public int LinkedListEndIndex1
    {
      get { return BitConverter.ToInt16(data, baseOffset + 10); }
      set { Tools.SetInt16(data, baseOffset + 10, value); }
    }
    public int LinkedListEndIndex2
    {
      get { return BitConverter.ToInt16(data, baseOffset + 12); }
      set { Tools.SetInt16(data, baseOffset + 12, value); }
    }
    public int ChannelCount
    {
      get { return BitConverter.ToInt16(data, baseOffset + 14); }
      set { Tools.SetInt16(data, baseOffset + 14, value); }
    }

    public int Size { get { return 16; } }
  }
}
