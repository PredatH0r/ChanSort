using System;
using System.IO;
using System.Text;

namespace Spike.Technisat
{
  internal class TechnisatDecrypt
  {
    static void Main()
    {
      var file = @"C:\Sources\ChanSort\TestFiles\TestFiles_Div\TechniSat\thenicnic\database.cdp";
      var original = File.ReadAllBytes(file);

      var decrypted = CdpDecrypt(original);
      File.WriteAllText($"{file}.txt", decrypted.Replace("\0", ""), encoding);

      var reencrypted = CdpEncrypt(decrypted);
      File.WriteAllBytes($"{file}.enc", reencrypted);

      // validate that decrypt + encrypt produces the original data
      if (reencrypted.Length != original.Length)
        throw new Exception("Incorrect file length");
      for (int i = 0; i < original.Length; i++)
      {
        if (reencrypted[i] != original[i])
          throw new Exception("Data corrupted at index " + i);
      }
    }

    static readonly Encoding encoding = Encoding.GetEncoding(1252);

    const uint InitSeed = 0xAC15FF4B;
    private const uint Polynomial = 0x80000062;

    private static string CdpDecrypt(byte[] data)
    {
      uint state = InitSeed;

      var sb = new StringBuilder(data.Length);
      foreach (var b in data)
      {
        var o = 0;
        for (int i = 0; i < 8; i++)
        {
          if ((state & 0x01) != 0)
          {
            state = (state ^ Polynomial) >> 1 | 0x80000000;
            o ^= 1 << i;
          }
          else
            state >>= 1;
        }

        o ^= b;
        if (o == 0)
          state = InitSeed;

        sb.Append((char)o);
      }

      return sb.ToString();
    }

    private static byte[] CdpEncrypt(string text)
    {
      uint state = InitSeed;

      var strm = new MemoryStream(text.Length);
      foreach (var b in encoding.GetBytes(text))
      {
        var o = 0;
        for (int i = 0; i < 8; i++)
        {
          if ((state & 0x01) != 0)
          {
            state = (state ^ Polynomial) >> 1 | 0x80000000;
            o ^= 1 << i;
          }
          else
            state >>= 1;
        }

        strm.WriteByte((byte)(o ^ b));

        if (b == 0)
          state = InitSeed;
      }

      var data = new byte[strm.Length];
      Array.Copy(strm.GetBuffer(), data, strm.Length);
      return data;
    }

#if false

    static readonly byte[] XorKey = { 255, 255, 255, 157, 27, 90, 156, 151, 214, 128, 125, 141, 0, 6, 245, 171 };

    private static int k = 0;

    private static byte XorDecode(byte b)
    {
      b ^= XorKey[k];
      k = (k + 1) & 0x0F;

      var o = 0;
      if ((b & 0x80) != 0) o |= 0x01;
      if ((b & 0x40) != 0) o |= 0x02;
      if ((b & 0x20) != 0) o |= 0x04;
      if ((b & 0x10) != 0) o |= 0x08;
      if ((b & 0x08) != 0) o |= 0x10;
      if ((b & 0x04) != 0) o |= 0x20;
      if ((b & 0x02) != 0) o |= 0x40;
      if ((b & 0x01) != 0) o |= 0x80;

      return (byte)o;
    }
#endif
  }
}
