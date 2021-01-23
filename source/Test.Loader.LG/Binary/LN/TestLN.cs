using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLN : TestBase
  {
    [TestMethod]
    public void TestLN5406_T224T_Sat76TR()
    {
      // "ThomasOhmes"
      //this.GenerateTestFiles("LN/xxLN5406-ZA99999");
      this.ExecuteTest("LN/xxLN5406-ZA99999");
    }

    [TestMethod]
    public void TestLN5758_FW04_20_29__C260TR_Sat92TR()
    {
      // "MarkusLenz"
      //this.GenerateTestFiles("LN/xxLN5758-ZE99999");
      this.ExecuteTest("LN/xxLN5758-ZE99999");
    }

  }
}
