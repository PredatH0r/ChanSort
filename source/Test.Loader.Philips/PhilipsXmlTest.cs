using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChanSort.Api;
using ChanSort.Loader.Philips;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Philips
{
  [TestClass]
  [DeploymentItem("ChanSort.Loader.Philips\\ChanSort.Loader.Philips.ini")]
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

    #region TestChannelMap125FormatCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestChannelMap125FormatCableChannelsAddedToCorrectLists()
    {
      // format 125 also contains a MtkChannelList.xml with MultiBank=COMMON, which must be loaded as one list per source
      var file = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\ChannelMap_125") + "\\ChannelList\\chanLst.bin";
      this.TestChannelsAddedToCorrectLists(file, SignalSource.DvbC, 382, 236, 143);
    }
    #endregion


    #region TestChannelMap125HiddenFlagInMediaTekTable
    [TestMethod]
    public void TestChannelMap125HiddenFlagInMediaTekTable()
    {
      // Hiding a channel has always been written to Philips' own XML (UserHidden). Writing it into the binary table of
      // MtkChannelList.xml as well has not been tested on a Philips TV and is therefore limited to developer mode.
      foreach (var developerMode in new[] { false, true })
      {
        var file = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\ChannelMap_125") + "\\ChannelList\\chanLst.bin";
        var mtkFile = Path.Combine(Path.GetDirectoryName(file), "MtkChannelList.xml");
        var hiddenBefore = CountHiddenSvlRecords(mtkFile);
        Assert.AreEqual(7, hiddenBefore);

        var plugin = new PhilipsPlugin();
        var ser = plugin.CreateSerializer(file);
        ser.DeveloperMode = developerMode;
        ser.Load();
        var data = ser.DataRoot;
        data.ValidateAfterLoad();
        data.ApplyCurrentProgramNumbers();

        // Philips' UserHidden and the flag in the binary table agree in the exported file
        var list = data.GetChannelList(SignalSource.DvbC);
        Assert.AreEqual(hiddenBefore, list.Channels.Count(ch => ch.Hidden));

        var chan = list.Channels.First(ch => !ch.Hidden && ch.OldProgramNr > 0);
        chan.Hidden = true;
        ser.Save();

        Assert.AreEqual(hiddenBefore + (developerMode ? 1 : 0), CountHiddenSvlRecords(mtkFile), "developer mode: " + developerMode);
      }
    }

    private static int CountHiddenSvlRecords(string mtkChannelListPath)
    {
      var xml = File.ReadAllText(mtkChannelListPath);
      var base64 = Regex.Match(xml, "<service_database>(.*?)</service_database>", RegexOptions.Singleline).Groups[1].Value;
      return ChanSort.Loader.MediaTek.SvlTable.LoadAll(Convert.FromBase64String(base64))
        .SelectMany(t => t.Records).Count(ChanSort.Loader.MediaTek.SvlTable.GetHidden);
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

    #region TestChannelAndFavListEditing_125
    [TestMethod]
    public void TestChannelAndFavListEditing_125()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\ChannelMap_125") + "\\ChannelList\\chanLst.bin";
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
