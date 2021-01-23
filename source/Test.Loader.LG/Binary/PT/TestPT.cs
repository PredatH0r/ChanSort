using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestPT : TestBase
  {
    [TestMethod]
    public void TestPT3553S_AC180TR()
    {
      // "MarioAntonioLiptaj"
      //this.GenerateTestFiles("PT/xxPT353-ZA00001");
      this.ExecuteTest("PT/xxPT353-ZA00001");
    }
  }
}
