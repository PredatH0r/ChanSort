using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Hisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Hisense.ChannelDb
{
  [TestClass]
  public class HisenseChannelDbTest
  {
    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("channel.db", SignalSource.DvbS, 1278, 1118, 160, 8692, "DOWNLOAD G10 HUMAX");
    }
    #endregion


    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("channel.db", SignalSource.DvbC, 381, 310, 71, 4008, "Humax ESD 160C");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid, string dataProgramName)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ChannelDb\\TestFiles\\" + fileName);
      var plugin = new HisensePlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.GetChannelList(signalSource);
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // check that data channel is in the TV list
      var chan = list.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
      Assert.IsNotNull(chan);
      Assert.AreEqual(dataProgramName, chan.Name);
    }
    #endregion

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ChannelDb\\TestFiles\\channel.db");
      var plugin = new HisensePlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 130 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(130, orf2e.OldProgramNr);
      Assert.AreEqual(130, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.AppendInOrder);

      Assert.IsFalse(orf2e.IsDeleted);
      Assert.IsTrue(orf2e.NewProgramNr > 0);
      Assert.AreEqual(0, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was removed from database
      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.IsTrue(orf2e.NewProgramNr > 0);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense\\ChannelDb\\TestFiles\\" + "channel.db");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new HisensePlugin());
    }
    #endregion
  }
}
