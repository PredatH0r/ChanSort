using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLD : TestBase
  {
    [TestMethod]
    public void TestLD750_AC176TR()
    {
      // "karlv"
      //this.GenerateTestFiles("LD/xxLD750-ZA00001");
      this.ExecuteTest("LD/xxLD750-ZA00001");
    }
  }
}
