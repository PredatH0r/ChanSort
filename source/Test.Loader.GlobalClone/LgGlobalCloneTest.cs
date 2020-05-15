using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
      Assert.AreEqual(0, orf2w.NewProgramNr);


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



    #region TestGlobalClone200JsonFormat

    [TestMethod]
    public void TestGlobalClone200JsonFormat()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.GlobalClone\\TestFiles\\GlobalClone00201.TLL");
      var orig = File.ReadAllText(tempFile, Encoding.UTF8);

      var plugin = new GcSerializerPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();
      var dvbs = data.GetChannelList(SignalSource.DvbS);

      // swap SRF 1 HD and SRF zwei HD
      var srf1 = dvbs.Channels.FirstOrDefault(ch => ch.Name == "SRF 1 HD");
      var srf2 = dvbs.Channels.FirstOrDefault(ch => ch.Name == "SRF zwei HD");
      Assert.AreEqual(1971, srf1.NewProgramNr);
      Assert.AreEqual(1972, srf2.NewProgramNr);
      srf1.NewProgramNr = 1972;
      srf2.NewProgramNr = 1971;

      // save and reload
      ser.Save(tempFile);
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      dvbs = data.GetChannelList(SignalSource.DvbS);
      srf1 = dvbs.Channels.FirstOrDefault(ch => ch.Name == "SRF 1 HD");
      srf2 = dvbs.Channels.FirstOrDefault(ch => ch.Name == "SRF zwei HD");
      Assert.AreEqual(1972, srf1.OldProgramNr);
      Assert.AreEqual(1971, srf2.OldProgramNr);

      // restore original program numbers and save
      srf1.NewProgramNr = 1971;
      srf2.NewProgramNr = 1972;
      ser.Save(tempFile);

      // undo expected changes to the file
      var changed = File.ReadAllText(tempFile, Encoding.UTF8);
      changed = changed.Replace("\"userEditChNumber\":true", "\"userEditChNumber\":false");
      changed = changed.Replace("\"userSelCHNo\":true", "\"userSelCHNo\":false");
      NUnit.Framework.Assert.AreEqual(orig, changed); // need NUnit.AreEqual to only show the actual difference and not 5MB + 5MB of data
    }
    #endregion



  }
}
