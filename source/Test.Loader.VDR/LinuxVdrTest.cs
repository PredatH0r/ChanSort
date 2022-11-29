using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.VDR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.VDR
{
  [TestClass]
  public class LinuxVdrTest
  {
    #region TestAstraChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAstraChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("channels.conf", 3380, 2649, 492);
    }
    #endregion

    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.VDR\\TestFiles\\channels.conf");
      var plugin = new VdrPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.ChannelLists.FirstOrDefault();
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // no data channels in channels.conf files
    }
    #endregion


    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.VDR\\TestFiles\\channels.conf");
      var plugin = new VdrPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 421 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(421, orf2e.OldProgramNr);
      Assert.AreEqual(421, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2e.IsDeleted);
      Assert.IsTrue(orf2e.NewProgramNr == 0);


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was deleted from file
      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNull(orf2e);
    }
    #endregion


    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.VDR\\TestFiles\\channels.conf");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new VdrPlugin());
    }
    #endregion
  }
}
