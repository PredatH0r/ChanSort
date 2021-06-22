using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Loader.Grundig;

namespace Test.Loader.Grundig
{
  [TestClass]
  public class GrundigTest
  {
    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Grundig\\TestFiles") + "\\dvbs_config.xml";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new GrundigPlugin());
    }
    #endregion
  }
}
