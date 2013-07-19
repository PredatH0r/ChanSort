using System;
using System.Collections.Generic;

namespace ChanSort.Api
{
  public static class Tools
  {
    public static V TryGet<K, V>(this IDictionary<K, V> dict, K key)
    {
      V val;
      dict.TryGetValue(key, out val);
      return val;
    }

    #region GetAnalogChannelNumber()
    public static string GetAnalogChannelNumber(int freq)
    {
      if (freq < 41) return "";
      if (freq <= 68) return ((freq - 41)/7 + 1).ToString("d2"); // Band I (01-04)
      if (freq < 105) return "";
      if (freq <= 174) return "S" + ((freq - 105)/7 + 1).ToString("d2"); // Midband (S01-S10)
      if (freq <= 230) return ((freq - 175)/7 + 5).ToString("d2"); // Band III (05-12)
      if (freq <= 300) return "S" + ((freq - 231)/7 + 11); // Superband (S11-S20)
      if (freq <= 469) return "S" + ((freq - 303)/8 + 21); // Hyperband (S21-S41)
      if (freq <= 1000) return ((freq - 471)/8 + 21).ToString("d2"); // Band IV, V
      return "";
    }
    #endregion

    #region GetInt16/32()

    public static int GetInt16(byte[] data, int offset, bool littleEndian)
    {
      return littleEndian ? BitConverter.ToInt16(data, offset) : (data[offset] << 8) + data[offset + 1];
    }

    public static int GetInt32(byte[] data, int offset, bool littleEndian)
    {
      return littleEndian ? BitConverter.ToInt32(data, offset) :
        (data[offset] << 24) + (data[offset + 1] << 16) + (data[offset + 2] << 8) + data[offset + 3];
    }
    #endregion

    #region SetInt16/32()

    public static void SetInt16(byte[] data, int offset, int value, bool littleEndian = true)
    {
      if (littleEndian)
      {
        data[offset + 0] = (byte) value;
        data[offset + 1] = (byte) (value >> 8);
      }
      else
      {
        data[offset + 0] = (byte)(value >> 8);
        data[offset + 1] = (byte) value;
      }
    }

    public static void SetInt32(byte[] data, int offset, int value, bool littleEndian = true)
    {
      if (littleEndian)
      {
        data[offset + 0] = (byte) value;
        data[offset + 1] = (byte) (value >> 8);
        data[offset + 2] = (byte) (value >> 16);
        data[offset + 3] = (byte) (value >> 24);
      }
      else
      {
        data[offset + 0] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;        
      }
    }
    #endregion

    #region MemCopy(), MemSet()

    public static void MemCopy(byte[] source, int sourceIndex, byte[] dest, int destIndex, int count)
    {
      for (int i = 0; i < count; i++)
        dest[destIndex + i] = source[sourceIndex + i];
    }

    public static void MemSet(byte[] data, int offset, byte value, int count)
    {
      for (int i = 0; i < count; i++)
        data[offset++] = value;
    }
    #endregion
  }
}
