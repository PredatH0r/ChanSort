using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLV : TestBase
  {
    [TestMethod]
    public void TestLV375S_Sat68()
    {
      // "inselfan"
      //this.GenerateTestFiles("LV/xxLV375S-ZC00001");
      this.ExecuteTest("LV/xxLV375S-ZC00001");
    }

    [TestMethod]
    public void TestLV470S_AT184T_Sat68TR()
    {
      // "Sigi43"
      //this.GenerateTestFiles("LV/xxLV470S-ZC00001");
      this.ExecuteTest("LV/xxLV470S-ZC00001");
    }

    [TestMethod]
    public void TestLV579S_AC184TR_Sat68()
    {
      // "derartur"
      //this.GenerateTestFiles("LV/xxLV579S-ZB00001");
      this.ExecuteTest("LV/xxLV579S-ZB00001");
    }

  }
}
