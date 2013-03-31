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
  }
}
