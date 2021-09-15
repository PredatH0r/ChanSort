using System;
using System.Text;

namespace ChanSort.Api
{
  public class DataMapping
  {
    protected readonly IniFile.Section settings;
    public Encoding DefaultEncoding { get; set; }
    public bool ThrowWhenAccessingUnknownSetting { get; set; } = false;

    #region ctor()
    public DataMapping(IniFile.Section settings, byte[] data = null)
    {
      this.settings = settings;
      this.Data = data;
      this.DefaultEncoding = Encoding.Default;
    }
    #endregion

    #region SetDataPtr(), Data, BaseOffset
    public void SetDataPtr(byte[] data, int baseOffset)
    {
      this.Data = data;
      this.BaseOffset = baseOffset;
    }

    public byte[] Data { get; set; }

    public int BaseOffset { get; set; }
    #endregion

    
    #region GetOffsets()
    public int[] GetOffsets(string key)
    {
      var list= settings.GetIntList(key);
      if (list == null) 
        return DefaultValue(key, Array.Empty<int>());
      return list;
    }
    #endregion

    #region GetMask()
    public int GetMask(string key)
    {
      var list = settings.GetIntList(key);
      if (list != null && list.Length > 0)
        return list[0];
      list = settings.GetIntList("mask" + key);
      if (list != null && list.Length > 0)
        return list[0];
      return DefaultValue(key, -1);
    }
    #endregion

    #region GetConst()
    public int GetConst(string key, int defaultValue)
    {
      var list = settings.GetIntList(key);
      if (list != null && list.Length > 0)
        return list[0];
      return defaultValue;
    }
    #endregion


    public IniFile.Section Settings => this.settings;


    #region Byte
    public byte GetByte(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length==0) return DefaultValue(key, (byte)0);
      return this.Data[BaseOffset + offsets[0]];
    }

    public void SetByte(string key, int value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
        this.Data[BaseOffset + offset] = (byte)value;
    }
    #endregion

    #region Word
    public ushort GetWord(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return DefaultValue(key, (ushort)0);
      return BitConverter.ToUInt16(this.Data, BaseOffset + offsets[0]);
    }

    public void SetWord(string key, int value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
      {
        this.Data[BaseOffset + offset + 0] = (byte)value;
        this.Data[BaseOffset + offset + 1] = (byte)(value>>8);
      }
    }
    #endregion

    #region DWord
    public long GetDword(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return DefaultValue(key, (uint)0);
      return BitConverter.ToUInt32(this.Data, BaseOffset + offsets[0]);
    }

    public void SetDword(string key, long value)
    {
      var offsets = settings.GetIntList(key);
      foreach (int offset in offsets)
      {
        this.Data[BaseOffset + offset + 0] = (byte)value;
        this.Data[BaseOffset + offset + 1] = (byte)(value >> 8);
        this.Data[BaseOffset + offset + 2] = (byte)(value >> 16);
        this.Data[BaseOffset + offset + 3] = (byte)(value >> 24);
      }
    }
    #endregion

    #region Float
    public float GetFloat(string key)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return DefaultValue(key, 0f);
      return BitConverter.ToSingle(this.Data, BaseOffset + offsets[0]);
    }

    public void SetFloat(string key, float value)
    {
      var offsets = settings.GetIntList(key);
      var bytes = BitConverter.GetBytes(value);
      foreach (int offset in offsets)
      {
        for (int i = 0; i < 4; i++)
          this.Data[BaseOffset + offset + i] = bytes[i];
      }
    }
    #endregion

    #region GetFlag

    public bool GetFlag(string key, bool defaultValue)
    {
      return GetFlag("off" + key, "mask" + key, defaultValue);
    }

    public bool GetFlag(string valueKey, string maskKey, bool defaultValue)
    {
      int mask = settings.GetInt(maskKey);
      return GetFlag(valueKey, mask, defaultValue);
    }

    public bool GetFlag(string valueKey, int mask, bool defaultValue)
    {
      if (mask == 0) return defaultValue;

      bool reverseLogic = false;
      if (mask < 0)
      {
        reverseLogic = true;
        mask = -mask;
      }
      var offsets = settings.GetIntList(valueKey);
      if (offsets.Length == 0) return defaultValue;
      bool isSet = (this.Data[BaseOffset + offsets[0]] & mask) == mask;
      return isSet != reverseLogic;
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
      bool reverseLogic = false;
      if (mask < 0)
      {
        reverseLogic = true;
        mask = -mask;
      }
      var offsets = settings.GetIntList(valueKey);
      foreach (var offset in offsets)
      {
        if (value != reverseLogic)
          this.Data[BaseOffset + offset] |= (byte)mask;
        else
          this.Data[BaseOffset + offset] &= (byte)~mask;
      }
    }
    #endregion


    #region GetString()
    public string GetString(string key, int maxLen)
    {
      var offsets = settings.GetIntList(key);
      if (offsets.Length == 0) return DefaultValue(key, (string)null);
      int length = this.GetByte(key + "Length");
      if (length == 0)
        length = maxLen;
      var encoding = this.DefaultEncoding;
      return encoding.GetString(this.Data, BaseOffset + offsets[0], length).TrimEnd('\0');
    }
    #endregion

    #region SetString()
    public int SetString(string key, string text, int maxLen)
    {
      var bytes = this.DefaultEncoding.GetBytes(text);
      int len = Math.Min(bytes.Length, maxLen);
      foreach (var offset in settings.GetIntList(key))
      {
        Array.Copy(bytes, 0, this.Data, BaseOffset + offset, len);
        for (int i = len; i < maxLen; i++)
          this.Data[BaseOffset + offset + i] = 0;
      }
      return len;
    }
    #endregion

    #region DefaultValue()
    private T DefaultValue<T>(string key, T value)
    {
      if (this.ThrowWhenAccessingUnknownSetting)
        throw new ArgumentException($"undefined setting \"{key}\" in {this.Settings}");
      return value;
    }
    #endregion

    #region HasValue()
    public bool HasValue(string key)
    {
      var list = this.settings.GetIntList(key);
      return list != null && list.Length > 0;
    }
    #endregion
  }

}
