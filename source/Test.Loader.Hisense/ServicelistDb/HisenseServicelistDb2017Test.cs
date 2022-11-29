using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Hisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Loader.Hisense.ServicelistDb
{
  [TestClass]
  public class HisenseServicelistDb2017Test
  {
    #region TestAstraChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAstraChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist_2017.db", "ASTRA1 19.2°E", 1214, 1052, 162);
    }
    #endregion

    #region TestEutelsatChannelsAddedToCorrectLists
    [TestMethod]
    public void TesEutelsatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist_2017.db", "Hot Bird 13°E", 1732, 1439, 293);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, string listCaption, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ServicelistDb\\TestFiles\\" + fileName);
      var plugin = new HisensePlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.ChannelLists.FirstOrDefault(l => l.Caption.StartsWith(listCaption));
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // no data channels in Hisense/Loewe servicelist.db files
    }
    #endregion

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ServicelistDb\\TestFiles\\servicelist_2017.db");
      var plugin = new HisensePlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 910 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(910, orf2e.OldProgramNr);
      Assert.AreEqual(910, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2e.IsDeleted);
      Assert.IsTrue(orf2e.NewProgramNr > 0);
      Assert.AreEqual(0, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was marked deleted in database
      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.IsTrue(orf2e.IsDeleted);
      Assert.AreEqual(-1, orf2e.NewProgramNr);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ServicelistDb\\TestFiles\\" + "servicelist_2017.db");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new HisensePlugin());
    }
    #endregion

  }
}
