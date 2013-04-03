using System;

namespace ChanSort.Loader.TllFile
{
  internal class SatTransponder
  {
    private readonly byte[] data;
    public int BaseOffset { get; set; }

    public SatTransponder(byte[] data)
    {
      this.data = data;
    }

    public int Frequency { get { return BitConverter.ToInt16(data, BaseOffset + 12); } }
    public int OriginalNetworkId { get { return BitConverter.ToInt16(data, BaseOffset + 18); } }
    public int TransportStreamId { get { return BitConverter.ToInt16(data, BaseOffset + 20); } }

    public int SymbolRate
    {
      get { return BitConverter.ToInt16(data, BaseOffset + 25); }
      set
      {
        data[BaseOffset + 25] = (byte)value;
        data[BaseOffset + 26] = (byte)(value >> 8);
      }
    }

    public int SatIndex { get { return data[BaseOffset + 36]; } }
  }
}
