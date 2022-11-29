using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Philips;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Philips
{
  [TestClass]
  public class PhilipsXmlTest
  {
    #region TestRepairFormatCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestRepairFormatCableChannelsAddedToCorrectLists()
    {
      // this file format doesn't provide any information whether a channel is TV/radio/data or analog/digital. It only contains the "medium" for antenna/cable/sat
      var file = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles") + "\\Repair\\CM_TPM1013E_LA_CK.xml";
      this.TestChannelsAddedToCorrectLists(file, SignalSource.DvbC, 483, 0, 0);
    }
    #endregion

    #region TestChannelMapFormatSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestChannelMapFormatSatChannelsAddedToCorrectLists()
    {
      var file = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles") + "\\ChannelMap_100\\ChannelList\\chanLst.bin";
      this.TestChannelsAddedToCorrectLists(file, SignalSource.DvbS, 502, 350, 152);
    }
    #endregion

    #region TestChannelMapFormatCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestChannelMapFormatCableChannelsAddedToCorrectLists()
    {
      var file = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles") + "\\ChannelMap_100\\ChannelList\\chanLst.bin";
      this.TestChannelsAddedToCorrectLists(file, SignalSource.DvbC, 459, 358, 101);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string filePath, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var plugin = new PhilipsPlugin();
      var ser = plugin.CreateSerializer(filePath);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.GetChannelList(signalSource);
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // no data channels found in any of the Philips channel lists available to me
    }
    #endregion

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles") + "\\ChannelMap_100\\ChannelList\\chanLst.bin";
      var plugin = new PhilipsPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 42 = NTV HD

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var ntvHd = dvbs.Channels.FirstOrDefault(ch => ch.Name == "NTV HD");
      Assert.IsNotNull(ntvHd);
      Assert.AreEqual(42, ntvHd.OldProgramNr);
      Assert.AreEqual(42, ntvHd.NewProgramNr);
      Assert.IsFalse(ntvHd.IsDeleted);

      ntvHd.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(ntvHd.IsDeleted);
      Assert.IsTrue(ntvHd.NewProgramNr == 0);
      Assert.AreEqual(1, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was deleted from database
      dvbs = data.GetChannelList(SignalSource.DvbS);
      ntvHd = dvbs.Channels.FirstOrDefault(ch => ch.Name == "NTV HD");
      Assert.IsNull(ntvHd);
    }
    #endregion


    #region TestChannelAndFavListEditing_100
    [TestMethod]
    public void TestChannelAndFavListEditing_100()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\ChannelMap_100\\ChannelList") + "\\chanLst.bin";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new PhilipsPlugin());
    }
    #endregion

    #region TestChannelAndFavListEditing_Legacy
    [TestMethod]
    public void TestChannelAndFavListEditing_Legacy()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\Repair") + "\\CM_TPM1013E_LA_CK.xml";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new PhilipsPlugin());
    }
    #endregion

  }
}
