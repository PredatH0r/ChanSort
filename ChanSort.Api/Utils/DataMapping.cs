using System.Text;

namespace ChanSort.Api
{
  public unsafe class DataMapping
  {
    protected readonly Encoding stringEncoding;
    protected readonly IniFile.Section settings;
    protected readonly int length;

    #region ctor()
    public DataMapping(IniFile.Section settings, int structureLength, Encoding stringEncoding)
    {
      this.settings = settings;
      this.length = structureLength;
      this.stringEncoding = stringEncoding;
    }
    #endregion

    #region DataPtr
    public byte* DataPtr { get; set; }
    #endregion

    #region DataLength
    public int DataLength { get { return this.length; } }
    #endregion

    #region Next()
    public void Next()
    {
      this.DataPtr += this.length;
    }
    #endregion

    #region GetOffsets()
    protected int[] GetOffsets(string key)
    {
      return settings.GetIntList(key);
    }
    #endregion


    #region GetByte()
    public byte GetByte(int off)
    {
      return off < 0 ? (byte)0 : this.DataPtr[off];
    }

    public byte GetByte(string key)
    {
      var offsets = settings.GetIntList(key);
      return offsets.Length > 0 ? this.GetByte(offsets[0]) : (byte)0;
    }
    #endregion

    #region GetWord()
    public ushort GetWord(int off)
    {
      return off < 0 ? (ushort)0 : *(ushort*) (this.DataPtr + off);
    }

    public ushort GetWord(string key)
    {
      var offsets = settings.GetIntList(key);
      return offsets.Length > 0 ? this.GetWord(offsets[0]) : (ushort)0;
    }
    #endregion

    #region GetDword()
    public uint GetDword(int off)
    {
      return off < 0 ? 0 : *(uint*) (this.DataPtr + off);
    }

    public uint GetDword(string key)
    {
      var offsets = settings.GetIntList(key);
      return offsets.Length > 0 ? this.GetDword(offsets[0]) : 0;
    }
    #endregion

    #region GetFloat()
    public float GetFloat(int off)
    {
      return off < 0 ? 0 : *(float*) (this.DataPtr + off);
    }

    public float GetFloat(string key)
    {
      var offsets = settings.GetIntList(key);
      return offsets.Length > 0 ? this.GetFloat(offsets[0]) : 0;
    }
    #endregion

    #region GetFlag()
    public bool GetFlag(int offset, byte mask)
    {
      return offset >= 0 && (this.GetByte(offset) & mask) != 0;
    }

    public bool GetFlag(string valueKey, string maskKey)
    {
      byte mask = (byte)settings.GetInt(maskKey);
      var offsets = settings.GetIntList(valueKey);
      return offsets.Length > 0 && this.GetFlag(offsets[0], mask);
    }
    #endregion

    #region GetString()
    public string GetString(int offset, int maxByteLen)
    {
      if (offset < 0) return null;
      byte[] buffer = new byte[maxByteLen];
      for (int i = 0; i < maxByteLen; i++)
        buffer[i] = this.DataPtr[offset + i];
      return stringEncoding.GetString(buffer).TrimEnd('\0');
    }

    public string GetString(string key, int maxLen)
    {
      var offsets = settings.GetIntList(key);
      return offsets.Length == 0 ? null : GetString(offsets[0], maxLen);
    }
    #endregion


    #region SetByte()
    public void SetByte(int off, byte value)
    {
      if (off >= 0)
        this.DataPtr[off] = value;
    }

    public void SetByte(string key, byte value)
    {
      var offsets = settings.GetIntList(key);
      foreach(int offset in offsets)
        this.SetByte(offset, value);
    }
    #endregion

    #region SetWord()
    public void SetWord(int off, int value)
    {
      if (off >= 0)
        *(ushort*) (this.DataPtr + off) = (ushort)value;
    }

    public void SetWord(string key, int value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
        this.SetWord(offset, value);
    }
    #endregion

    #region SetDword()
    public void SetDword(int off, uint value)
    {
      if (off >= 0)
        *(uint*) (this.DataPtr + off) = value;
    }

    public void SetDword(string key, uint value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
        this.SetDword(offset, value);
    }
    #endregion

    #region SetFloat()
    public void SetFloat(int off, float value)
    {
      if (off >= 0)
        *(float*)(this.DataPtr + off) = value;
    }

    public void SetFloat(string key, float value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
        this.SetFloat(offset, value);
    }
    #endregion

    #region SetFlag()
    public void SetFlag(int offset, byte mask, bool set)
    {
      byte val = this.GetByte(offset);
      this.SetByte(offset, (byte)(set ? val | mask : val & ~mask));
    }

    public void SetFlag(string valueKey, string maskKey, bool set)
    {
      byte mask = (byte)settings.GetInt(maskKey);
      var offsets = settings.GetIntList(valueKey);
      foreach (int offset in offsets)
        this.SetFlag(offset, mask, set);
    }
    #endregion
  }

}
