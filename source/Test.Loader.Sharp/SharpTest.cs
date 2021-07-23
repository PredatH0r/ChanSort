using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Loader.Sharp;

namespace Test.Loader.Sharp
{
  [TestClass]
  public class SharpTest
  {
    #region TestChannelAndFavListEditing_3
    [TestMethod]
    public void TestChannelAndFavListEditing_3()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sharp\\TestFiles\\DVBS_Program3.csv");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SharpPlugin());
    }
    #endregion

    #region TestChannelAndFavListEditing_5
    [TestMethod]
    public void TestChannelAndFavListEditing_5()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sharp\\TestFiles\\DVBS_Program5.csv");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SharpPlugin());
    }
    #endregion

    #region TestChannelAndFavListEditing_6
    [TestMethod]
    public void TestChannelAndFavListEditing_6()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sharp\\TestFiles\\DVBS_Program6.csv");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SharpPlugin());
    }
    #endregion

    #region TestChannelAndFavListEditing_7
    [TestMethod]
    public void TestChannelAndFavListEditing_7()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sharp\\TestFiles\\MS6486_DVBS_CHANNEL_TABLE.csv");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SharpPlugin());
    }
    #endregion

    #region TestChannelAndFavListEditing_51
    [TestMethod]
    public void TestChannelAndFavListEditing_51()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sharp\\TestFiles\\DVBS_CHANNEL_TABLE.csv");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SharpPlugin());
    }
    #endregion

  }
}
