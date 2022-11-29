using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Toshiba;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Toshiba
{
  [TestClass]
  public class ToshibaChmgtDbTest
  {
    #region TestSatChannelsAddedToCorrectLists

    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Toshiba-SL863G.zip", SignalSource.DvbS, 338, 172);
    }

    #endregion

    #region TestAnalogChannelsAddedToCorrectLists

    [TestMethod]
    public void TestAnalogChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Toshiba-SL863G.zip", SignalSource.AnalogCT, 1, 0);
    }

    #endregion


    #region TestChannelsAddedToCorrectList

    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Toshiba\\TestFiles\\" + fileName);
      var plugin = new ToshibaPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;

      var tv = root.GetChannelList(signalSource | SignalSource.Tv);
      if (expectedTv > 0)
      {
        Assert.IsNotNull(tv);
        Assert.AreEqual(expectedTv, tv?.Channels.Count ?? 0);
      }

      if (expectedRadio > 0)
      {
        var radio = root.GetChannelList(signalSource | SignalSource.Radio);
        Assert.IsNotNull(radio);
        Assert.AreEqual(expectedRadio, radio.Channels.Count);
      }

      // check that data channel is in the TV list
      if (dataProgramSid != 0)
      {
        var chan = tv.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
        Assert.IsNotNull(chan);
        Assert.AreEqual(dataProgramName, chan.Name);
      }
    }

    #endregion


    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Toshiba\\TestFiles\\Toshiba-SL863G.zip");
      var plugin = new ToshibaPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 128 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(336, orf2e.OldProgramNr);
      Assert.AreEqual(336, orf2e.NewProgramNr);
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

      // channel was deleted from database
      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNull(orf2e);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Toshiba\\TestFiles\\Toshiba-SL863G.zip");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new ToshibaPlugin());
    }
    #endregion

  }
}
