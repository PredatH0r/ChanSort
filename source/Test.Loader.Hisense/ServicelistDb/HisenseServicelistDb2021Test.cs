using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Hisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Loader.Hisense.ServicelistDb
{
  [TestClass]
  public class HisenseServicelistDb2021Test
  {
    #region TestAntennaChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAntennaChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist_2021.db", "Antenna", 33, 24, 9);
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
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ServicelistDb\\TestFiles\\servicelist_2021.db");
      var plugin = new HisensePlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 804 = Das Erste HD

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var chan = dvbs.Channels.FirstOrDefault(ch => ch.Name == "Das Erste HD");
      Assert.IsNotNull(chan);
      Assert.AreEqual(804, chan.OldProgramNr);
      Assert.AreEqual(804, chan.NewProgramNr);
      Assert.IsFalse(chan.IsDeleted);

      chan.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(chan.IsDeleted);
      Assert.IsTrue(chan.NewProgramNr > 0);
      Assert.AreEqual(0, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));

      foreach (var list in data.ChannelLists)
        list.ReadOnly = false;


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was marked deleted in database
      dvbs = data.GetChannelList(SignalSource.DvbS);
      chan = dvbs.Channels.FirstOrDefault(ch => ch.Name == "Das Erste HD");
      Assert.IsNotNull(chan);
      Assert.IsTrue(chan.IsDeleted);
      Assert.AreEqual(-1, chan.NewProgramNr);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ServicelistDb\\TestFiles\\" + "servicelist_2021.db");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new HisensePlugin(), true, 271, 7);
    }
    #endregion

  }
}
