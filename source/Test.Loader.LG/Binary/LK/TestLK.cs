using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLK : TestBase
  {
    [TestMethod]
    public void TestLK450_AC176TR()
    {
      // "chlabnet"
      //this.GenerateTestFiles("LK/xxLK450-ZB00001");
      this.ExecuteTest("LK/xxLK450-ZB00001");
    }

    [TestMethod]
    public void TestLK950S_AC184TR_Sat68TR()
    {
      // "Klausi1"
      //this.GenerateTestFiles("LK/xxLK950S-ZA00001");
      this.ExecuteTest("LK/xxLK950S-ZA00001");
    }

  }
}
