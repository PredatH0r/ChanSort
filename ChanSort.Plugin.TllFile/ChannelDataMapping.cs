using System;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  internal static class ChannelDataMapping
  {
    public static unsafe void SetChannelName(ChannelMappingBase mapping, string channelName, Encoding defaultEncoding)
    {
      byte[] codePagePrefix = new byte[0]; // DvbStringDecoder.GetCodepageBytes(defaultEncoding);
      byte[] name = defaultEncoding.GetBytes(channelName);
      byte[] newName = new byte[codePagePrefix.Length + name.Length + 1];
      Array.Copy(codePagePrefix, 0, newName, 0, codePagePrefix.Length);
      Array.Copy(name, 0, newName, codePagePrefix.Length, name.Length);
      fixed (byte* ptrNewName = newName)
      {
        mapping.NamePtr = ptrNewName;
      }
      mapping.NameLength = newName.Length;      
    }
  }
}
