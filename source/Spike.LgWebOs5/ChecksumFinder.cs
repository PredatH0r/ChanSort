using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ChanSort.Api;

namespace Spike.LgWebOs5;

public class ChecksumFinder
{
  public static void Main()
  {
    //var data = File.ReadAllBytes(@"C:\Sources\ChanSort\TestFiles\TestFiles_Philips\120.0\marko-arn\PhilipsChannelMaps\ChannelMap_120\ChannelList\channellib\DVBC.xml"); // 0x00 0x00 0xFB
    //int xor = 0xa4;
    //int sum = 0xce;

    //var data = File.ReadAllBytes(@"C:\Sources\ChanSort\TestFiles\TestFiles_Philips\120.0\marko-arn\PhilipsChannelMaps\ChannelMap_120\ChannelList\s2channellib\DVBSall.xml"); // 0xA6
    var data = File.ReadAllBytes(@"C:\Sources\ChanSort\TestFiles\TestFiles_Philips\120.0\marko-arn\PhilipsChannelMaps\ChannelMap_120\ChannelList\Favorite.xml"); // 0x00 0x9a

    int xor = 0;
    int sum = 0;

    foreach (var b in data)
    {
      xor ^= b;
      sum += b;
    }

    var crc8m = new Crc8CalculatorMsb();
    var crc8l = new Crc8CalculatorLsb();
    var crcm = crc8m.CalculateCrc8(data);
    var crcl = crc8l.CalculateCrc8(data);
    var mb = Crc16.Modbus(data);

    Console.WriteLine($"SUM: {sum:x2}  ~{~sum & 0xff:x2}");
    Console.WriteLine($"XOR: {xor:x2}  ~{~xor & 0xff:x2}");
    Console.WriteLine($"CRCM: {crcm:x2}  ~{~crcm & 0xff:x2}");
    Console.WriteLine($"CRCL: {crcl:x2}  ~{~crcl & 0xff:x2}");

    Console.WriteLine($"MB: {mb:x2}  ~{~mb & 0xff:x2}");
  }
}

class Crc8CalculatorMsb
{
  private readonly byte[] crcTable;

  public Crc8CalculatorMsb(byte poly = 0x07)
  {
    crcTable = new byte[256];
    for (int i = 0; i < 256; ++i)
    {
      byte crc = (byte)i;
      for (int j = 0; j < 8; ++j)
      {
        crc = (byte)((crc << 1) ^ ((crc & 0x80) != 0 ? poly : 0));
      }
      crcTable[i] = crc;
    }
  }

  public byte CalculateCrc8(byte[] data, byte init = 0)
  {
    byte crc = init;

    foreach (byte b in data)
      crc = crcTable[crc ^ b];

    return crc;
  }
}

class Crc8CalculatorLsb
{
  private readonly byte[] crcTable;

  public Crc8CalculatorLsb(byte poly = 0xE0)
  {
    crcTable = new byte[256];
    for (int i = 0; i < 256; ++i)
    {
      byte crc = (byte)i;
      for (int j = 0; j < 8; ++j)
      {
        crc = (byte)((crc >> 1) ^ ((crc & 0x01) != 0 ? poly : 0));
      }
      crcTable[i] = crc;
    }
  }

  public byte CalculateCrc8(byte[] data, byte init = 0)
  {
    byte crc = init;

    foreach (byte b in data)
      crc = crcTable[crc ^ b];

    return crc;
  }
}