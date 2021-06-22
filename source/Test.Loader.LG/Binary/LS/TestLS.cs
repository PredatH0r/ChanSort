using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLS : TestBase
  {
    [TestMethod]
    public void TestLS560_AC188TR_Sat68TR()
    {
      // "posti3571"
      //this.GenerateTestFiles("LS/xxLS560S-ZC00010");
      this.ExecuteTest("LS/xxLS560S-ZC00010");
    }

    [TestMethod]
    public void TestLS570S_AC192TR_Sat72()
    {
      // "Wolfpack"
      //this.GenerateTestFiles("LS/xxLS570S-ZB00001");
      this.ExecuteTest("LS/xxLS570S-ZB00001");
    }

  }
}
