using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLP : TestBase
  {
    [TestMethod]
    public void TestLP632H_Sat76TR()
    {
      // "KristianStolze"
      //this.GenerateTestFiles("LP/xxLP632H-ZA00001");
      this.ExecuteTest("LP/xxLP632H-ZA00001");
    }
  }
}
