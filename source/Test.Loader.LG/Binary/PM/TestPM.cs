using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestPM : TestBase
  {
    [TestMethod]
    public void TestPM670S_Sat72TR()
    {
      // "FvN"
      //this.GenerateTestFiles("PM/xxPM670S-ZA00001");
      this.ExecuteTest("PM/xxPM670S-ZA00001");
    }

    [TestMethod]
    public void TestPM970S_C192TR_Sat72TR()
    {
      // "DavidePalmaghini"
      //this.GenerateTestFiles("PM/xxPM970S-ZA99999");
      this.ExecuteTest("PM/xxPM970S-ZA99999");
    }
  }
}
