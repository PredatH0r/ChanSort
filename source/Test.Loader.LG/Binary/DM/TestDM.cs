using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestDM : TestBase
  {
    [TestMethod]
    public void Test2350D_AC164TR()
    {
      // "opel"
      this.ExecuteTest("DM/xx2350D-PZM00001");
    }
  }
}
