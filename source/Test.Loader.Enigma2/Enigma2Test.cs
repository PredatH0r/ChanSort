using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Loader.Enigma2;

namespace Test.Loader.Enigma2
{
  [TestClass]
  public class Enigma2Test
  {
    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Enigma2\\TestFiles") + "\\bouquets.tv";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new Enigma2Plugin(), false);
    }
    #endregion
  }
}
