using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.SatcoDX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.SatcoDX
{
  [TestClass]
  public class SdxTest
  {
    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      var list = this.TestChannelsAddedToCorrectLists("silva_schneider.sdx", SignalSource.DvbS, 1108, 948, 160);
      
      // Test encoding as CP1252
      Assert.AreEqual("SAT.1 Gold Österreic", list.Channels[9].Name);
    }
    #endregion

    #region TestFormatVersion105UtfEncoding
    [TestMethod]
    public void TestFormatVersion105UtfEncoding()
    {
      var list = this.TestChannelsAddedToCorrectLists("telefunken105.sdx", SignalSource.DvbS, 737, 650, 87);

      // Test encoding as UTF-8
      Assert.AreEqual("WDR HD Köln", list.Channels[5].Name);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private ChannelList TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.SatcoDX\\TestFiles\\" + fileName);
      var plugin = new SatcoDxPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.GetChannelList(signalSource);
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // check that data channel is in the TV list
      if (dataProgramSid != 0)
      {
        var chan = list.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
        Assert.IsNotNull(chan);
        Assert.AreEqual(dataProgramName, chan.Name);
      }

      return list;
    }
    #endregion

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.SatcoDX\\TestFiles\\silva_schneider.sdx");
      var plugin = new SatcoDxPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 128 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(128, orf2e.OldProgramNr);
      Assert.AreEqual(128, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2e.IsDeleted);
      Assert.IsTrue(orf2e.NewProgramNr == 0);
      Assert.AreEqual(1, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


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
      var tempFile = TestUtils.DeploymentItem("Test.Loader.SatcoDX\\TestFiles\\silva_schneider.sdx");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SatcoDxPlugin());
    }
    #endregion
  }
}
