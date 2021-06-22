using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLX : TestBase
  {
    [TestMethod]
    public void TestLX9500_AC176TR()
    {
      // "JeanPaul"
      //this.GenerateTestFiles("LX/xxLX9500-ZA00001");
      this.ExecuteTest("LX/xxLX9500-ZA00001");
    }
  }
}
