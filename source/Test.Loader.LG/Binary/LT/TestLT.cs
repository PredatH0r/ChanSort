using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLT : TestBase
  {
    [TestMethod]
    public void TestLT380H_AC212TR()
    {
      // SJahre
      //this.GenerateTestFiles("LT/xxLT380H-ZA00001");
      this.ExecuteTest("LT/xxLT380H-ZA00001");
    }
  }
}
