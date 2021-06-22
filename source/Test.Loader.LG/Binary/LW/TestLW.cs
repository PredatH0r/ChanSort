using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLW : TestBase
  {
    [TestMethod]
    public void TestLW4500_AC176TR()
    {
      // "Stabilo"
      //this.GenerateTestFiles("LW/xxLW4500-ZB00001");
      this.ExecuteTest("LW/xxLW4500-ZB00001");
    }

    [TestMethod]
    public void TestLW659S_AC184T_Sat68TR()
    {
      // "dangler"
      //this.GenerateTestFiles("LW/xxLW659S-ZC00001");
      this.ExecuteTest("LW/xxLW659S-ZC00001");
    }
  }
}
