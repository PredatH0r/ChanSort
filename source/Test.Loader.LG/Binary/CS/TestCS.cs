using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestCS : TestBase
  {
    [TestMethod]
    public void TestCS460S_Sat68TR()
    {
      // "FranzSteinert"
      //this.GenerateTestFiles("CS/xxCS460S-ZA00001");
      this.ExecuteTest("CS/xxCS460S-ZA00001");
    }
  }
}
