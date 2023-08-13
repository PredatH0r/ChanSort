using System;
using System.IO;
using System.Text;

namespace ChanSort.Loader.TechniSat;

internal class TechniSatCrypt
{
  internal static readonly Encoding Encoding = Encoding.GetEncoding(1252);

  private const uint InitSeed = 0xAC15FF4B;
  private const uint Polynomial = 0x80000062;

  #region Decrypt()
  public static string CdpDecrypt(byte[] data)
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
  #endregion

  #region Encrypt()
  public static byte[] CdpEncrypt(string text)
  {
    uint state = InitSeed;

    var strm = new MemoryStream(text.Length);
    foreach (var b in Encoding.GetBytes(text))
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
  #endregion

#if false
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
#endif

}