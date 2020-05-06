using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Sony;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Sony
{
  [TestClass]
  public class SonyXmlTest
  {
    // Android OS seems to use the "FormateVer" XML element, KDL 2012 and 2014 use "FormatVer"

    #region TestAndroid ... ChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAndroidSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("android_sdb-sat.xml", SignalSource.DvbS, 1163, 1004, 159);
      this.TestChannelsAddedToCorrectLists("android_sdb-sat.xml", SignalSource.DvbS | SignalSource.Provider1, 397, 265, 132);
      this.TestChannelsAddedToCorrectLists("android_sdb-sat.xml", SignalSource.DvbS | SignalSource.Provider2, 0, 0, 0);
    }

    [TestMethod]
    public void TestAndroidCableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("android_sdb-cable.xml", SignalSource.DvbC | SignalSource.Tv, 314, 314, 0);
      this.TestChannelsAddedToCorrectLists("android_sdb-cable.xml", SignalSource.DvbC | SignalSource.Radio, 112, 0, 112);
    }

    [TestMethod]
    public void TestAndroidAntennaChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("android_sdb-antenna.xml", SignalSource.DvbT | SignalSource.Tv, 53, 53, 0);
      this.TestChannelsAddedToCorrectLists("android_sdb-antenna.xml", SignalSource.DvbT | SignalSource.Radio, 6, 0, 6);
    }
    #endregion

    #region TestKdl ... ChannelsAddedToCorrectLists
    [TestMethod]
    public void TestKdlSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("kdl_sdb-cable-sat.xml", SignalSource.DvbS, 1540, 1191, 173, 7216, "HUMAX DOWNLOAD SVC");
    }

    [TestMethod]
    public void TestKdlCableChannelsAddedToCorrectLists()
    {
      // there are 237 tv+radio channels in the list, but only a subset has assigned program numbers
      this.TestChannelsAddedToCorrectLists("kdl_sdb-cable-sat.xml", SignalSource.DvbC | SignalSource.Tv, 189, 189, 0);
      this.TestChannelsAddedToCorrectLists("kdl_sdb-cable-sat.xml", SignalSource.DvbC | SignalSource.Radio, 47, 0, 47);
      this.TestChannelsAddedToCorrectLists("kdl_sdb-cable-sat.xml", SignalSource.DvbC | SignalSource.Data, 1, 0, 0, 5024, "Zapp PDS");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\" + fileName);
      var plugin = new SerializerPlugin();
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
    }
    #endregion


    #region TestAndroidDeletingChannel

    [TestMethod]
    public void TestAndroidDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\android_sdb-sat.xml");
      var plugin = new SerializerPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 128 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(127, orf2e.OldProgramNr);
      Assert.AreEqual(127, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2e.IsDeleted);
      Assert.IsTrue(orf2e.NewProgramNr > 0);
      Assert.AreEqual(0, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save(tempFile);
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was marked deleted
      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.IsTrue(orf2e.IsDeleted);
      Assert.AreEqual(-1, orf2e.NewProgramNr);
    }
    #endregion

    #region TestKdlDeletingChannel

    [TestMethod]
    public void TestKdlDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\kdl_sdb-cable-sat.xml");
      var plugin = new SerializerPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 128 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(693, orf2e.OldProgramNr);
      Assert.AreEqual(693, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2e.IsDeleted);
      Assert.AreEqual(0, orf2e.NewProgramNr);


      // save and reload
      ser.Save(tempFile);
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channel was not assigned a number in the file
      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.IsTrue(orf2e.IsDeleted);
      Assert.AreEqual(-1, orf2e.NewProgramNr);
    }
    #endregion

  }
}
