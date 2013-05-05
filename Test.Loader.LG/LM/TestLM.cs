using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG
{
  [TestClass]
  public class TestLM : TestBase
  {
    [TestMethod]
    public void TestLM611S_T188_Sat68TR()
    {
      // test file from "wagnale" - 611S and 340S have different file format than rest of LM series
      this.ExecuteTest("LM/xxLM611S-ZA00001");
    }

    [TestMethod]
    public void TestLM620S_T192T_Sat72TR()
    {
      // test file from "Pred"
      this.ExecuteTest("LM/xxLM620S-ZE00000");
    }

    [TestMethod]
    public void TestLM760S()
    {
      // "PDA-User"
      this.GenerateTestFiles("LM/xxLM760S-ZB00001");
      this.ExecuteTest("LM/xxLM760S-ZB00001");
    }

    [TestMethod]
    public void TestLM860V_C192TR()
    {
      // "PDA-User"
      this.ExecuteTest("LM/xxLM860V-ZB99998");
    }

  }
}
