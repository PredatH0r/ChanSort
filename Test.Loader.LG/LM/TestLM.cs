using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG
{
  [TestClass]
  public class TestLM : TestBase
  {
    [TestMethod]
    public void TestLM340S_AC188TR_Sat68TR()
    {
      // "Tom2012"
      //this.GenerateTestFiles("LM/xxLM340S-ZA00001");
      this.ExecuteTest("LM/xxLM340S-ZA00001");
    }

    [TestMethod]
    public void TestLM611S_T188_Sat68TR()
    {
      // "wagnale"
      //this.GenerateTestFiles("LM/xxLM611S-ZA00001");
      this.ExecuteTest("LM/xxLM611S-ZA00001");
    }

    [TestMethod]
    public void TestLM620S_AT192T_Sat72TR()
    {
      // "VitorMartinsAugusto"
      //this.GenerateTestFiles("LM/xxLM620S-ZE00001");
      this.ExecuteTest("LM/xxLM620S-ZE00001");
    }

    [TestMethod]
    public void TestLM860V_C192TR_Sat72()
    {
      // "PDA-User"
      this.ExecuteTest("LM/xxLM860V-ZB99998");
    }

  }
}
