﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.GlobalClone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.GlobalClone
{
  [TestClass]
  public class LgGlobalCloneTest
  {
    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.ChannelsAddedToCorrectLists("GlobalClone00001.TLL", SignalSource.DvbS, 1138, 160, 8692, "DOWNLOAD G10 HUMAX");
    }
    #endregion

    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      this.ChannelsAddedToCorrectLists("GlobalClone00002.TLL", SignalSource.DvbC, 405, 113, 11105, "ITV Content 01");
    }
    #endregion

    #region TestAntennaChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAntennaChannelsAddedToCorrectLists()
    {
      this.ChannelsAddedToCorrectLists("GlobalClone00003.TLL", SignalSource.DvbT, 67, 6, 14120, "SRT8505 OTA");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void ChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTv, int expectedRadio, int dataProgramSid, string dataProgramName)
    {
      var testFile = TestUtils.DeploymentItem("Test.Loader.GlobalClone\\TestFiles\\" + fileName);
      var plugin = new GcSerializerPlugin();
      var ser = plugin.CreateSerializer(testFile);
      ser.Load();

      var root = ser.DataRoot;

      var tv = root.GetChannelList(signalSource | SignalSource.Tv);
      Assert.IsNotNull(tv);
      Assert.AreEqual(expectedTv, tv.Channels.Count);

      var radio = root.GetChannelList(signalSource | SignalSource.Radio);
      Assert.IsNotNull(radio);
      Assert.AreEqual(expectedRadio, radio.Channels.Count);

      // check that data channel is in the TV list
      var chan = tv.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
      Assert.IsNotNull(chan);
      Assert.AreEqual(dataProgramName, chan.Name);
    }
    #endregion

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.GlobalClone\\TestFiles\\GlobalClone00001.TLL");
      var plugin = new GcSerializerPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 122 = ORF2W HD 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2w = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2W HD");
      Assert.AreEqual(122, orf2w.OldProgramNr);
      Assert.AreEqual(122, orf2w.NewProgramNr);
      Assert.IsFalse(orf2w.IsDeleted);

      orf2w.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2w.IsDeleted);
      Assert.AreNotEqual(-1, orf2w.NewProgramNr);
      Assert.AreEqual(0, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save(tempFile);
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();
    

      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2w = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2W HD");
      Assert.IsTrue(orf2w.IsDeleted);
      Assert.AreEqual(-1, orf2w.OldProgramNr);
    }
    #endregion

  }
}
