using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG
{
  [TestClass]
  public class TestLH : TestBase
  {
    [TestMethod]
    public void TestLH3000_DvbCT_TvAndRadio()
    {
      // different file format than other LH-models

      // test file from "Egon"
      this.ExecuteTest("LH/xxLH3000-ZA00002");
    }

    [TestMethod]
    public void TestLH5000_AnalogAndDigital_Tv_CyrillicAlphabet()
    {
      // test file from "JLevi"
      this.ExecuteTest("LH/xxLH5000-ZB00002");
    }

    [TestMethod]
    public void TestLH7000_AnalogAndDigital_TvAndRadio()
    {
      // test file from "JLevi"
      this.ExecuteTest("LH/xxLH7000-ZA00001");
    }

  }
}
