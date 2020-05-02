using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ChanSort.Api
{
  public static class Tools
  {
    public static V TryGet<K, V>(this IDictionary<K, V> dict, K key, V defaultValue = default(V))
    {
      V val;
      return dict.TryGetValue(key, out val) ? val : defaultValue;
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

    #region ReverseByteOrder()
    public static ushort ReverseByteOrder(ushort input)
    {
      return (ushort)(((input & 0x00FF) << 8) | (input >> 8));
    }

    public static uint ReverseByteOrder(uint input)
    {
      return ((input & 0x000000FF) << 24) | ((input & 0x0000FF00) << 8) | ((input & 0x00FF0000) >> 8) | ((input & 0xFF000000) >> 24);
    }
    #endregion

    #region HexDecode()
    public static byte[] HexDecode(string input)
    {
      var bytes = new byte[input.Length/2];
      for (int i = 0, c = input.Length/2; i < c; i++)
      {
        char ch = Char.ToUpper(input[i*2]);
        var high = Char.IsDigit(ch) ? ch - '0' : ch - 'A' + 10;
        ch = Char.ToUpper(input[i*2 + 1]);
        var low = Char.IsDigit(ch) ? ch - '0' : ch - 'A' + 10;
        bytes[i] = (byte)((high << 4) | low);
      }
      return bytes;
    }
    #endregion

    #region HexEncode()
    public static string HexEncode(byte[] bytes, bool uppercase = false)
    {
      const string HexDigitsLower = "0123456789abcdef";
      const string HexDigitsUpper = "0123456789ABCDEF";
      var hexDigits = uppercase ? HexDigitsUpper : HexDigitsLower;
      var sb = new StringBuilder(bytes.Length * 2);
      foreach (byte b in bytes)
        sb.Append(hexDigits[b >> 4]).Append(hexDigits[b & 0x0F]);
      return sb.ToString();
    }
    #endregion

    #region IsUtf8()
    /// <summary>
    /// This method tests whether the binary data can be interpreted as valid UTF-8. If not, it might be encoded with a locale specific encoding
    /// </summary>
    public static bool IsUtf8(byte[] buffer)
    {
      int followBytes = 0;
      foreach (byte b in buffer)
      {
        if (followBytes > 0)
        {
          if ((b & 0xC0) != 0x80) // follow-up bytes must be 10xx xxxx
            return false;
          --followBytes;
          continue;
        }

        if (b < 0x80) // standard ASCII characters
          continue;

        if (b < 0xC0) // [0x80-0xBF] is only allowed for UTF-8 follow-up bytes
          return false;

        if (b < 0xE0) // 110x xxxx
          followBytes = 1;
        else if (b < 0xF0) // 1110 xxxx
          followBytes = 2;
        else if (b < 0xF8) // 1111 0xxx
          followBytes = 3;
        else
          return false; // can't be more than 3 follow-up bytes
      }

      return followBytes == 0;
    }
    #endregion

    #region HasUtf8Bom()
    public static bool HasUtf8Bom(byte[] content)
    {
      if (content == null || content.Length < 3)
        return false;
      return content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF;
    }
    #endregion

    #region Scale()
    public static int Scale(this int dist, float factor)
    {
      return (int)Math.Round(dist * factor);
    }

    public static Size Scale(this Size size, SizeF factor)
    {
      return new Size((int)Math.Round(size.Width * factor.Width), (int)Math.Round(size.Height * factor.Height));
    }

    public static SizeF Scale(this SizeF absFactor, SizeF relFactor)
    {
      return new SizeF(absFactor.Width * relFactor.Width, absFactor.Height * relFactor.Height);
    }

    public static int Unscale(this int dist, float factor)
    {
      return (int)Math.Round(dist / factor);
    }

    public static Size Unscale(this Size size, SizeF factor)
    {
      return new Size((int)Math.Round(size.Width / factor.Width), (int)Math.Round(size.Height / factor.Height));
    }

    #endregion
  }
}
