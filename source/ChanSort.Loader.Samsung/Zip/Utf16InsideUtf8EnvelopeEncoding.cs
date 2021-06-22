using System.IO;
using System.Text;

namespace ChanSort.Loader.Samsung.Zip
{
  // Samsung 1242 format does not store UTF16 characters directly, but instead wraps 16 data bits inside a UTF-8 lead + continuation byte sequence.
  // A 3 byte UTF-8 sequence is used to encode 16 bits of utf-16 big endian input: 1110aaaa 10bbbbcc 10ccdddd represents the 16bit big endian integer ccccddddaaaabbbb, i.e. 0xE4, 0x84, 0x80 => 0x00, 0x41 => "A" in UTF-16 BE
  // The Samsung encoder seems to create some illegal UTF-8 sequences at the end of the string as a result of padding and operating on 32bit inputs (2 characters) with big-endianness, which
  // this decoder has to take care of. 0xFFFD can appear both in the raw input bytes (0xFF, 0xFB) as well as already encoded into UTF-8 wrappings (0xEF,0xBF,0xBD)

  // This implementation here decodes the UTF-8 byte sequence into UTF-16 Little Endian for the sake of simplicity: aaaa=4, bbbb=1, cccc=0, dddd=0 => 0xE4, 0x84, 0x80 => 0x41, 0x00 => "A" in UTF-16 LE.
  // The encoder here operates on 16bit characters and not 32bit 2-characters, so there is no need for padding and no invalid UTF-8 sequences.

  public class Utf16InsideUtf8EnvelopeEncoding : Encoding
  {
    public override int GetMaxByteCount(int charCount)
    {
      return charCount * 3;
    }

    public override int GetByteCount(char[] chars, int index, int count)
    {
      return count * 3;
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
      var utf16Le = Unicode.GetBytes(chars, charIndex, charCount);
      int o = byteIndex;
      int c = utf16Le.Length;
      for (int i = 0; i < c; i += 2, o += 3)
      {
        var b0 = utf16Le[i + 0];
        var b1 = utf16Le[i + 1];
        bytes[o + 0] = (byte) (0xE0 + (b0 >> 4));
        bytes[o + 1] = (byte) (0x80 + ((b0 & 0x0F) << 2) + (b1 >> 6));
        bytes[o + 2] = (byte) (0x80 + (b1 & 0x3F));
      }

      return charCount * 3;
    }


    public override int GetMaxCharCount(int byteCount)
    {
      return (byteCount + 2) / 3;
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
      return (count + 2) / 3;
    }

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
      using MemoryStream ms = new MemoryStream(40);
      for (int i = byteIndex, c = byteIndex + byteCount; i < c; i++)
      {
        int b0 = bytes[i + 0];
        if (b0 == 0 && i == c - 1) // satellite names end with a single trailing 0x00 byte
          break;
        if (b0 > 0xF7) // invalid UTF-8 lead byte. (0xFF, 0xFD) = 0xFFFD in BigEndian can appear unencoded at the end of the byte stream, likely as a padding 
          continue;
        if (b0 >= 0xE0) // 3-byte UTF envelope for 2 input bytes
        {
          int b1 = bytes[i + 1];
          int b2 = bytes[i + 2];
          if ((b2 & 0xC0) != 0x80) // invalid 2nd UTF-8 continuation byte; only a single byte is encoded as 1110aaaa 10bbbbcc => aaaabbbb
          {
            b2 = 0;
            --i;
          }
          int ch1 = ((b0 & 0x0F) << 4) | ((b1 & 0x3C) >> 2);
          int ch2 = ((b1 & 0x03) << 6) | (b2 & 0x3F);
          if (ch1 != 0xFF || ch2 != 0xFD) // ignore UTF-16 "replacement character" U-0xFFFD
          {
            ms.WriteByte((byte) ch1);
            ms.WriteByte((byte) ch2);
          }
          i += 2;
        }
        else if (b0 >= 0xC0) // 2-byte UTF envelope for 1 input byte as 110xaaaa 10bbbbcc => aaaabbbb
        {
          int b1 = bytes[i + 1];
          int ch = ((b0 & 0x0F) << 4) | ((b1 & 0x3C)>>2);
          ms.WriteByte((byte)ch);
          ms.WriteByte(0);
          i++;
        }
        else if (b0 < 0x80) // 1-byte UTF envelope for 1 input byte < 0x80
        {
          ms.WriteByte(bytes[i]);
          ms.WriteByte(0);
        }
      }

      return Encoding.Unicode.GetChars(ms.GetBuffer(), 0, (int) ms.Length, chars, charIndex);
    }
  }
}
