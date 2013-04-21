using System;

namespace ChanSort.Loader.LG
{
  internal class SatChannelListHeader
  {
    private readonly byte[] data;
    private readonly int baseOffset;
    public SatChannelListHeader(byte[] data, int offset) { this.data = data; this.baseOffset = offset; }

    public uint Checksum { get { return BitConverter.ToUInt32(data, baseOffset + 0); } }        
    public int LinkedListStartIndex { get { return BitConverter.ToInt16(data, baseOffset + 8); }}
    public int LinkedListEndIndex1 { get { return BitConverter.ToInt16(data, baseOffset + 10); } }
    public int LinkedListEndIndex2 { get { return BitConverter.ToInt16(data, baseOffset + 12); } }
    public int ChannelCount { get { return BitConverter.ToInt16(data, baseOffset + 14); } }

    public int Size { get { return 16; } }
  }
}
