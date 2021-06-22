using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLE : TestBase
  {
    [TestMethod]
    public void TestLE5500_A176T()
    {
      // "JLevi"
      //this.GenerateTestFiles("LE/xxLE5500-ZA00002");
      this.ExecuteTest("LE/xxLE5500-ZA00002");
    }
  }
}
