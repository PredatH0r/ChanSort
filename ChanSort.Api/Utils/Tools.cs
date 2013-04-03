using System.Collections.Generic;
using System.Text;

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

    public static unsafe string GetString(this Encoding encoding, byte* str, int len)
    {
      byte[] copy = new byte[len];
      for (int i = 0; i < len; i++)
        copy[i] = *str++;
      string name = encoding.GetString(copy, 0, len);
      int idx = name.IndexOf('\0');
      if (idx >= 0)
        name = name.Substring(0, idx);
      return name;
    }

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
  }
}
