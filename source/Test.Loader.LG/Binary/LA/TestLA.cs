using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLA : TestBase
  {
    [TestMethod]
    public void TestLA7408_C256TR_Sat92()
    {
      // "KaiKlau"
      //this.GenerateTestFiles("LA/xxLA7408-ZB00001");
      this.ExecuteTest("LA/xxLA7408-ZB00001");
    }

    [TestMethod]
    public void TestLA7408_Sat92TR()
    {
      // "martiko"
      //this.GenerateTestFiles("LA/xxLA7408-ZB00002");
      this.ExecuteTest("LA/xxLA7408-ZB00002");
    }

    [TestMethod]
    public void TestLA691S_Firmware04_20_29()
    {
      // "DanGmail"
      //this.GenerateTestFiles("LA/xxLA691S-ZA00004");
      this.ExecuteTest("LA/xxLA691S-ZA00004");
    }   
  }
}
