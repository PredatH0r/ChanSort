using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLH : TestBase
  {
    [TestMethod]
    public void TestLH3000_T164TR()
    {
      // different file format than other LH-models

      // test file from "Egon"
      this.ExecuteTest("LH/xxLH3000-ZA00002");
    }

    [TestMethod]
    public void TestLH5000_AC164TR_Cyrillic()
    {
      // test file from "JLevi"
      this.ExecuteTest("LH/xxLH5000-ZB00002");
    }
  }
}
