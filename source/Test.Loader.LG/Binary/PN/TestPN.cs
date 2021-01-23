using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestPN : TestBase
  {
    [TestMethod]
    public void TestPN6500_C212TR()
    {
      // "Backlit"
      //this.GenerateTestFiles("PN/xxPN6500-ZB00001");
      this.ExecuteTest("PN/xxPN6500-ZB00001");
    }
  }
}
