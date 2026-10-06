using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Api;

/*
 * This test class was added to verify that the new Crc32 wrapper class around System.IO.Hashing.Crc32
 * produces the same results as the old custom implementation in ChanSort.Api.
 */

[TestClass]
public class Crc32Test
{
  private static readonly byte[] data = [0x01, 0x02, 0x03, 0x04, 0x05, 0xF3, 0xF4, 0xDE, 0xAD, 0xBE, 0xEF];

  [TestMethod]
  public void TestCrc32Normal()
  {
    var crc = ChanSort.Api.Crc32.Normal.CalcCrc32(data, 2, 4);
    Assert.AreEqual(0xB3D7E6AC, crc);
  }

  [TestMethod]
  public void TestCrc32Reversed()
  {
    var crc = ChanSort.Api.Crc32.Reversed.CalcCrc32(data, 2, 4);
    Assert.AreEqual(0x92C47032, crc);
  }

  [TestMethod]
  public void TestCrc32Crack()
  {
    var crc = ChanSort.Api.Crc32.Normal.CalcCrc32(data, 0, 8);
    var i = ChanSort.Api.Crc32.Normal.Crack(data, data.Length, crc);
    Assert.AreEqual(8, i);
  }

}