﻿using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.Hisense2017;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Loader.Hisense2017
{
  [TestClass]
  public class HisenseServicelistDbTest
  {
    #region TestAstraChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAstraChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist.db", "ASTRA1 19.2°E", 1214, 1052, 162);
    }
    #endregion

    #region TestEutelsatChannelsAddedToCorrectLists
    [TestMethod]
    public void TesEutelsatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist.db", "Hot Bird 13°E", 1732, 1439, 293);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, string listCaption, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense2017\\TestFiles\\" + fileName);
      var plugin = new HisDbSerializerPlugin();
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
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Hisense2017\\TestFiles\\servicelist.db");
      var plugin = new HisDbSerializerPlugin();
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
      ser.Save(tempFile);
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

  }
}
