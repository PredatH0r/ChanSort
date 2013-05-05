using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG
{
  [TestClass]
  public class TestLA : TestBase
  {
    [TestMethod]
    public void TestLA7408_C256TR()
    {
      // "KaiKlau"
      //this.GenerateTestFiles("LA/xxLA7408-ZB00001");
      this.ExecuteTest("LA/xxLA7408-ZB00001");
    }

    [TestMethod]
    public void TestLA7408_Sat92()
    {
      // "martiko"
      //this.GenerateTestFiles("LA/xxLA7408-ZB00002");
      this.ExecuteTest("LA/xxLA7408-ZB00002");
    }
  }
}
