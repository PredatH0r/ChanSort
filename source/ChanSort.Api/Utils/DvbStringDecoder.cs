using System;
using System.Collections.Generic;
using System.Text;

namespace ChanSort.Api
{
  #region Documentation
  /*
  ETSI EN 300 468
   
  For one-byte character tables, the codes in the range 0x80 to 0x9F are assigned to control functions
  as shown in Table A.1: Single byte control codes

  Control code - Control code Description
  0x80 to 0x85 reserved for future use
  0x86 character emphasis on
  0x87 character emphasis off
  0x88 to 0x89 reserved for future use
  0x8A CR/LF
  0x8B to 0x9F user defined

  A.2 Selection of character table
  First byte value - Character code table - Table description - Reproduced in figure
  0x01 ISO/IEC 8859-5 [27] Latin/Cyrillic alphabet A.2
  0x02 ISO/IEC 8859-6 [28] Latin/Arabic alphabet A.3
  0x03 ISO/IEC 8859-7 [29] Latin/Greek alphabet A.4
  0x04 ISO/IEC 8859-8 [30] Latin/Hebrew alphabet A.5
  0x05 ISO/IEC 8859-9 [31] Latin alphabet No. 5 A.6
  0x06 ISO/IEC 8859-10 [32] Latin alphabet No. 6 A.7
  0x07 ISO/IEC 8859-11 [33] Latin/Thai (draft only) A.8
  0x08 reserved for future use (see note)
  0x09 ISO/IEC 8859-13 [34] Latin alphabet No. 7 A.9
  0x0A ISO/IEC 8859-14 [35] Latin alphabet No. 8 (Celtic) A.10
  0x0B ISO/IEC 8859-15 [36] Latin alphabet No. 9 A.11
  0x0C to 0x0F reserved for future use
  0x10 ISO/IEC 8859 See table A.4
  0x11 ISO/IEC 10646 [16] Basic Multilingual Plane (BMP)
  0x12 KSX1001-2004 [44] Korean Character Set
  0x13 GB-2312-1980 Simplified Chinese Character
  0x14 Big5 subset of ISO/IEC 10646 [16] Traditional Chinese
  0x15 UTF-8 encoding of ISO/IEC 10646 [16] Basic Multilingual Plane (BMP)
  0x16 to 0x1E reserved for future use
  0x1F Described by encoding_type_id Described by 8 bit

  Table A.4: Character Coding Tables for first byte 0x10
  First byte value - Second byte value - Third Byte Value - Selected character code - table - Table Description
  0x10 0x00 0x00 reserved for future use
  0x10 0x00 0x01 ISO/IEC 8859-1 [23] West European
  0x10 0x00 0x02 ISO/IEC 8859-2 [24] East European
  0x10 0x00 0x03 ISO/IEC 8859-3 [25] South European
  0x10 0x00 0x04 ISO/IEC 8859-4 [26] North and North-East European
  0x10 0x00 0x05 ISO/IEC 8859-5 [27] Latin/Cyrillic A.2
  0x10 0x00 0x06 ISO/IEC 8859-6 [28] Latin/Arabic A.3
  0x10 0x00 0x07 ISO/IEC 8859-7 [29] Latin/Greek A.4
  0x10 0x00 0x08 ISO/IEC 8859-8 [30] Latin/Hebrew A.5
  0x10 0x00 0x09 ISO/IEC 8859-9 [31] West European & Turkish A.6
  0x10 0x00 0x0A ISO/IEC 8859-10 [32] North European A.7
  0x10 0x00 0x0B ISO/IEC 8859-11 [33] Thai A.8
  0x10 0x00 0x0C Reserved for future use
  0x10 0x00 0x0D ISO/IEC 8859-13 [34] Baltic A.9
  0x10 0x00 0x0E ISO/IEC 8859-14 [35] Celtic A.10
  0x10 0x00 0x0F ISO/IEC 8859-15 [36] West European A.11
  */
  #endregion

  public class DvbStringDecoder
  {
    static readonly string[] codePages1 =
        {
          null, "iso-8859-5", "iso-8859-6", "iso-8859-7", "iso-8859-8", "iso-8859-9", "iso-8859-10", "iso-8859-11", 
          null, "iso-8859-13", "iso-8859-14", "iso-8859-15", null, null, null, null,
          null, // codePages2 prefix
          "utf-16", "x-cp20949", "x-cp20936", "utf-16", "utf-8", null, null, null,
          "utf-8", null, null, null, "utf-8"
        };

    static readonly string[] codePages2 =
        {
          null, "iso-8859-1", "iso-8859-2", "iso-8859-3", "iso-8859-4", "iso-8859-5", "iso-8859-6", "iso-8859-7", 
          "iso-8859-8", "iso-8859-9", "iso-8859-10", "iso-8859-11", null, "iso-8859-13", "iso-8859-14", "iso-8859-15"
        };

    private readonly Dictionary<string, Decoder> decoderCache = new Dictionary<string, Decoder>();

    public DvbStringDecoder(Encoding defaultEncoding)
    {
      this.DefaultEncoding = defaultEncoding;
    }

    public Encoding DefaultEncoding { get; set; }

    #region GetChannelNames()

    public void GetChannelNames(byte[] name, int off, int len, out string longName, out string shortName)
    {
      this.GetChannelNamesCore(name, off, len, out longName, out shortName);
      longName = longName.TrimGarbage();
      shortName = shortName.TrimGarbage();
    }

    private void GetChannelNamesCore(byte[] name, int off, int len, out string longName, out string shortName)
    {
      longName = "";
      shortName = "";
      if (len == 0)
        return;
      byte b = name[off];
      if (b == 0)
        return;

      Decoder decoder = this.DefaultEncoding.GetDecoder();
      bool singleByteChar = true;
      if (b < 0x20)
      {
        if (b == 0x10) // prefix for 2-byte code page
        {
          int cpIndex = name[off + 1] * 256 + name[off + 2];
          off += 2;
          len -= 2;
          SetDecoder(codePages2, cpIndex, ref decoder);
        }
        if (b <= 0x1F)
          SetDecoder(codePages1, b, ref decoder);
        singleByteChar = b < 0x10;
        ++off;
        --len;
      }
      if (!singleByteChar)
      {
        char[] buffer = new char[100];
        int l= decoder.GetChars(name, off, len, buffer, 0, false);
        longName = new string(buffer, 0, l);
        return;
      }

      StringBuilder sbLong = new StringBuilder();
      StringBuilder sbShort = new StringBuilder();
      bool inShortMode = false;
      for (int c = 0; c < len; c++)
      {
        int i = off + c;
        b = name[i];
        if (b == 0x00)
          break;

        char ch = '\0';
        switch (b)
        {
          case 0x86: inShortMode = true; continue;
          case 0x87: inShortMode = false; continue;
          case 0x8a: ch = '\n'; break;
          default:
            if (b >= 0x80 && b <= 0x9f) // DVB-S control characters
              continue;
            break;
        }
        if (ch == '\0')
        {
          // read as many bytes as necessary to get a character. Note that the decoder keeps internal state of all previously unprocessed bytes
          char[] charArray = new char[5];
          while (c < len && decoder.GetChars(name, i++, 1, charArray, 0) == 0)
            c++;
          if (c >= len)
            break;
          ch = charArray[0];
        }
        if (ch == '\0')
          continue;

        sbLong.Append(ch);
        if (inShortMode)
          sbShort.Append(ch);
      }
      longName = sbLong.ToString();
      shortName = sbShort.ToString();
    }
    #endregion

    #region SetDecoder()
    private void SetDecoder(string[] codePages, int cpIndex, ref Decoder defaultDecoder)
    {
      if (cpIndex >= codePages.Length)
        return;
      Decoder decoder;

      string cp = codePages[cpIndex];
      if (cp == null)
        return;

      if (this.decoderCache.TryGetValue(cp, out decoder))
      {
        defaultDecoder = decoder;
        return;
      }

      try
      {
        var encoding = Encoding.GetEncoding(cp);
        defaultDecoder = encoding.GetDecoder();
      }
      catch (ArgumentException)
      {
      }
      decoderCache[cp] = defaultDecoder;
    }
    #endregion

    #region GetCodepageBytes()
    public static byte[] GetCodepageBytes(Encoding encoding)
    {
      var encName = encoding.WebName;
      for (int i = 0; i < codePages1.Length; i++)
      {
        if (codePages1[i] == encName)
          return new [] {(byte)i};
      }

      for (int i = 0; i < codePages2.Length; i++)
      {
        if (codePages2[i] == encName)
          return new[] { (byte)0x10, (byte)i };
      }

      return new byte[0];
    }
    #endregion

    #region GetEncoding()
    /// <summary>
    /// Pass in either a value &lt;=0x1F excluding 0x10 or 0x10xxyy
    /// </summary>
    public static Encoding GetEncoding(int encodingMarker)
    {
      string enc = null;
      if (encodingMarker < 0x20)
        enc = codePages1[encodingMarker];
      else
      {
        encodingMarker &= 0xFFFF;
        if (encodingMarker < codePages2.Length)
          enc = codePages2[encodingMarker];
      }
      return enc == null ? null : Encoding.GetEncoding(enc);
    }
    #endregion
  }
}
