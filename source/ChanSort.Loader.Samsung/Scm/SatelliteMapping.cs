using System;
using System.Text;

namespace ChanSort.Loader.Samsung.Scm
{
  internal class SatelliteMapping
  {
    private static readonly Encoding utf16Encoding = new UnicodeEncoding(false, false);

    private readonly byte[] data;
    public int BaseOffset;

    public SatelliteMapping(byte[] data, int offset)
    {
      this.data = data;
      this.BaseOffset = offset;
    }

    public byte MagicMarker { get { return data[BaseOffset]; } }
    public int SatelliteNr { get { return BitConverter.ToInt32(data, BaseOffset + 1); } }
    public string Name { get { return utf16Encoding.GetString(data, BaseOffset + 9, 128).TrimEnd('\0'); } }
    public bool IsEast { get { return BitConverter.ToInt32(data, BaseOffset + 137) != 0; } }
    public int Longitude { get { return BitConverter.ToInt32(data, BaseOffset + 141); } }
  }
}
