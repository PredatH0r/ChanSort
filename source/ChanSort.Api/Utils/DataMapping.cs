using System;
using System.Text;

namespace ChanSort.Api
{
  public class DataMapping
  {
    protected readonly IniFile.Section settings;
    private int baseOffset;
    private byte[] data { get; set; }
    public Encoding DefaultEncoding { get; set; }

    #region ctor()
    public DataMapping(IniFile.Section settings)
    {
      this.settings = settings;
      this.DefaultEncoding = Encoding.Default;
    }
    #endregion

    #region SetDataPtr(), Data, BaseOffset
    public void SetDataPtr(byte[] data, int baseOffset)
    {
      this.data = data;
      this.baseOffset = baseOffset;
    }

    public byte[] Data { get { return this.data; } }
    public int BaseOffset { get { return this.baseOffset; } set { this.baseOffset = value; } }
    #endregion

    
    #region GetOffsets()
    public int[] GetOffsets(string key)
    {
      return settings.GetIntList(key);
    }
    #endregion

    public IniFile.Section Settings { get { return this.settings; } }


    #region Byte
    public byte GetByte(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length==0) return 0;
      return this.data[baseOffset + offsets[0]];
    }

    public void SetByte(string key, int value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
        this.data[baseOffset + offset] = (byte)value;
    }
    #endregion

    #region Word
    public ushort GetWord(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return 0;
      return BitConverter.ToUInt16(this.data, baseOffset + offsets[0]);
    }

    public void SetWord(string key, int value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
      {
        this.data[baseOffset + offset + 0] = (byte)value;
        this.data[baseOffset + offset + 1] = (byte)(value>>8);
      }
    }
    #endregion

    #region DWord
    public long GetDword(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return 0;
      return BitConverter.ToUInt32(this.data, baseOffset + offsets[0]);
    }

    public void SetDword(string key, long value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
      {
        this.data[baseOffset + offset + 0] = (byte)value;
        this.data[baseOffset + offset + 1] = (byte)(value >> 8);
        this.data[baseOffset + offset + 2] = (byte)(value >> 16);
        this.data[baseOffset + offset + 3] = (byte)(value >> 24);
      }
    }
    #endregion

    #region Float
    public float GetFloat(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return 0;
      return BitConverter.ToSingle(this.data, baseOffset + offsets[0]);
    }

    public void SetFloat(string key, float value)
    {
      var offsets = settings.GetIntList(key);
      var bytes = BitConverter.GetBytes(value);
      foreach (int offset in offsets)
      {
        for (int i = 0; i < 4; i++)
          this.data[baseOffset + offset + i] = bytes[i];
      }
    }
    #endregion

    #region GetFlag

    public bool GetFlag(string key, bool defaultValue = false)
    {
      return GetFlag("off" + key, "mask" + key, defaultValue);
    }

    public bool GetFlag(string valueKey, string maskKey, bool defaultValue = false)
    {
      int mask = settings.GetInt(maskKey);
      return GetFlag(valueKey, mask, defaultValue);
    }

    public bool GetFlag(string valueKey, int mask, bool defaultValue = false)
    {
      if (mask == 0) return defaultValue;
      var offsets = settings.GetIntList(valueKey);
      if (offsets.Length == 0) return defaultValue;
      return (this.data[baseOffset + offsets[0]] & mask) == mask;
    }
    #endregion

    #region SetFlag()
    public void SetFlag(string key, bool value)
    {
      this.SetFlag("off" + key, "mask" + key, value);
    }

    public void SetFlag(string valueKey, string maskKey, bool value)
    {
      int mask = settings.GetInt(maskKey);
      SetFlag(valueKey, mask, value);
    }

    public void SetFlag(string valueKey, int mask, bool value)
    {
      if (mask == 0) return;
      var offsets = settings.GetIntList(valueKey);
      foreach (var offset in offsets)
      {
        if (value)
          this.data[baseOffset + offset] |= (byte)mask;
        else
          this.data[baseOffset + offset] &= (byte)~mask;
      }
    }
    #endregion


    #region GetString()
    public string GetString(string key, int maxLen)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return null;
      int length = this.GetByte(key + "Length");
      if (length == 0)
        length = maxLen;
      var encoding = this.DefaultEncoding;
      return encoding.GetString(this.data, baseOffset + offsets[0], length).TrimEnd('\0');
    }
    #endregion

    #region SetString()
    public int SetString(string key, string text, int maxLen)
    {
      var bytes = this.DefaultEncoding.GetBytes(text);
      int len = Math.Min(bytes.Length, maxLen);
      foreach (var offset in settings.GetIntList(key))
      {
        Array.Copy(bytes, 0, this.data, baseOffset + offset, len);
        for (int i = len; i < maxLen; i++)
          this.data[baseOffset + offset + i] = 0;
      }
      return len;
    }
    #endregion
  }

}
