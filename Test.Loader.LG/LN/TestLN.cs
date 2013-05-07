using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG
{
  [TestClass]
  public class TestLN : TestBase
  {
    [TestMethod]
    public void TestLX9500_T224T_Sat76TR()
    {
      // "ThomasOhmes"
      //this.GenerateTestFiles("LN/xxLN5406-ZA99999");
      this.ExecuteTest("LN/xxLN5406-ZA99999");
    }
  }
}
