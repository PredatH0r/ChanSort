using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChanSort.Api;
using ChanSort.Loader.MediaTek;
using ChanSort.Loader.Sony;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Sony
{
  [TestClass]
  [DeploymentItem("ChanSort.Loader.Sony\\ChanSort.Loader.Sony.ini")]
  public class SonyXmlTest
  {
    // Android OS seems to use the "FormateVer" XML element, KDL 2012 and 2014 use "FormatVer"
    // Bravia 7 and 8 (2024, 2025) contain a Mediatek XML inside sdb.xml, just like Philips' "MtkChannelList.xml" with slight differences

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
      this.TestChannelsAddedToCorrectLists("kdl_sdb-cable-sat.xml", SignalSource.DvbS, 1540, 1225, 173, 7216, "HUMAX DOWNLOAD SVC");
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

    #region TestMediatek (Bravia 2024,2025)
    [TestMethod]
    public void TestMediatekCableChannelsAddedToCorrectLists()
    {
      // there are 237 tv+radio channels in the list, but only a subset has assigned program numbers
      this.TestChannelsAddedToCorrectLists("mediatek-sdb.xml", SignalSource.DvbC | SignalSource.Tv, 237, 237, 0);
      this.TestChannelsAddedToCorrectLists("mediatek-sdb.xml", SignalSource.DvbC | SignalSource.Radio, 138, 0, 138);
      this.TestChannelsAddedToCorrectLists("mediatek-sdb.xml", SignalSource.DvbC | SignalSource.Data, 0, 0, 0);
    }

    [TestMethod]
    public void TestMediatekHashcodeFollowsRenumbering()
    {
      // The hashcode at record offset +22 covers the program number, so it must follow a renumbering.
      // Moving channels to numbers above 64 changes the upper byte of the number field, which the
      // previous delta rule did not account for.
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\mediatek_sdb-cable.xml");
      var plugin = new SonyPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var validBefore = CountValidHashcodes(tempFile, out var recordsBefore);
      Assert.IsTrue(validBefore > 0, "no record with a computable hashcode");

      var tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      var nr = tv.Channels.Max(ch => ch.OldProgramNr) + 1;
      Assert.IsTrue(nr > 64, "the test file does not reach numbers above 64");
      foreach (var ch in tv.Channels.Where(ch => ch.OldProgramNr > 0).OrderBy(ch => ch.OldProgramNr))
        ch.NewProgramNr = nr++;
      ser.Save();

      var validAfter = CountValidHashcodes(tempFile, out var recordsAfter);
      Assert.AreEqual(recordsBefore, recordsAfter);
      Assert.AreEqual(validBefore, validAfter, "the hashcode of some records does not match after renumbering");
    }

    [TestMethod]
    public void TestMediatekHiddenIsReadFromBinaryTable()
    {
      // The TV takes the hidden state from byte +13 of the binary record. Files that were edited by older tools
      // can have a different visible_service in the XML, so make the XML claim that nothing is hidden.
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\mediatek-sdb.xml");
      var xml = File.ReadAllText(tempFile);
      Assert.IsTrue(xml.Contains("<visible_service>1</visible_service>"));
      File.WriteAllText(tempFile, xml.Replace("<visible_service>1</visible_service>", "<visible_service>3</visible_service>"));

      var plugin = new SonyPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      Assert.IsTrue(ser.Features.CanHideChannels);

      var tv = ser.DataRoot.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      var hidden = tv.Channels.Where(ch => ch.Hidden).Select(ch => ch.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();
      CollectionAssert.AreEqual(new[] { "Netflix", "Prime Sportsbar", "RTLSport 1", "RTLSport 2" }, hidden);
    }

    [TestMethod]
    public void TestMediatekHidingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\mediatek_sdb-cable.xml");
      var plugin = new SonyPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();
      var validHashcodes = CountValidHashcodes(tempFile, out _);

      var tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      var chan = tv.Channels.First(ch => !ch.Hidden && ch.OldProgramNr > 0);
      var recordId = (int)chan.RecordIndex;
      var nr = chan.OldProgramNr;
      Assert.AreEqual((byte)0x0F, GetSvlRecord(tempFile, recordId)[13]);

      chan.Hidden = true;
      ser.Save();

      // the binary flag is set, the XML mirrors it, and the hashcode does not depend on it
      Assert.AreEqual((byte)0x09, GetSvlRecord(tempFile, recordId)[13]);
      Assert.AreEqual("1", GetVisibleService(tempFile, recordId));
      Assert.AreEqual(validHashcodes, CountValidHashcodes(tempFile, out _));

      // a hidden channel keeps its number
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();
      tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      chan = tv.Channels.First(ch => (int)ch.RecordIndex == recordId);
      Assert.IsTrue(chan.Hidden);
      Assert.AreEqual(nr, chan.OldProgramNr);

      // and back
      chan.Hidden = false;
      ser.Save();
      Assert.AreEqual((byte)0x0F, GetSvlRecord(tempFile, recordId)[13]);
      Assert.AreEqual("3", GetVisibleService(tempFile, recordId));
    }

    [TestMethod]
    public void TestMediatekUnsortedChannelsAreAppendedAndHidden()
    {
      // Deleting is not supported for this format, so "append" is the only way the UI offers to handle a channel
      // that was removed from the list. ChanSort then gives it the next free number and sets Hidden.
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\mediatek_sdb-cable.xml");
      var plugin = new SonyPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      var chan = tv.Channels.First(ch => !ch.Hidden && ch.OldProgramNr > 0);
      var recordId = (int)chan.RecordIndex;
      var maxNr = tv.Channels.Max(ch => ch.OldProgramNr);

      chan.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.AppendAndHide);
      ser.Save();

      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      chan = tv.Channels.First(ch => (int)ch.RecordIndex == recordId);
      Assert.AreEqual(maxNr + 1, chan.OldProgramNr);
      Assert.IsTrue(chan.Hidden);
      Assert.AreEqual((byte)0x09, GetSvlRecord(tempFile, recordId)[13]);
    }

    [TestMethod]
    public void TestMediatekUnsortedChannelsCanBeAppendedWithoutHiding()
    {
      // The caller decides whether appended channels are hidden too, so that the user can be asked.
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\mediatek_sdb-cable.xml");
      var plugin = new SonyPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      var chan = tv.Channels.First(ch => !ch.Hidden && ch.OldProgramNr > 0);
      var recordId = (int)chan.RecordIndex;
      var maxNr = tv.Channels.Max(ch => ch.OldProgramNr);

      chan.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Append);
      ser.Save();

      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      tv = data.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
      chan = tv.Channels.First(ch => (int)ch.RecordIndex == recordId);
      Assert.AreEqual(maxNr + 1, chan.OldProgramNr);
      Assert.IsFalse(chan.Hidden);
      Assert.AreEqual((byte)0x0F, GetSvlRecord(tempFile, recordId)[13]);
    }

    private static byte[] GetSvlRecord(string sdbXmlPath, int recordId)
    {
      var xml = File.ReadAllText(sdbXmlPath);
      var base64 = Regex.Match(xml, "<service_database>(.*?)</service_database>", RegexOptions.Singleline).Groups[1].Value;
      return SvlTable.LoadAll(Convert.FromBase64String(base64)).SelectMany(t => t.Records).First(r => SvlTable.GetRecordId(r) == recordId);
    }

    private static string GetVisibleService(string sdbXmlPath, int recordId)
    {
      var xml = File.ReadAllText(sdbXmlPath);
      var serviceInfo = Regex.Matches(xml, "<service_info>.*?</service_info>", RegexOptions.Singleline).Cast<Match>()
        .First(m => Regex.IsMatch(m.Value, "<record_id>[^<]*/" + recordId + "</record_id>"));
      return Regex.Match(serviceInfo.Value, "<visible_service>([^<]*)</visible_service>").Groups[1].Value;
    }

    /// <summary>Number of Svl records whose stored hashcode is reproduced by SvlTable.CalcHashcode().</summary>
    private static int CountValidHashcodes(string sdbXmlPath, out int records)
    {
      var xml = File.ReadAllText(sdbXmlPath);
      var base64 = Regex.Match(xml, "<service_database>(.*?)</service_database>", RegexOptions.Singleline).Groups[1].Value;
      var tables = SvlTable.LoadAll(Convert.FromBase64String(base64));
      var valid = 0;
      records = 0;
      foreach (var table in tables)
      {
        for (int i = 0; i < table.Records.Count; i++)
        {
          records++;
          if (SvlTable.CalcHashcode(table.Records[i], table.Names[i]) == (uint)table.Records[i].GetInt32(22, false))
            valid++;
        }
      }
      return valid;
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\" + fileName);
      var plugin = new SonyPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.GetChannelList(signalSource);

      if (list == null)
      {
        if (expectedTotal == 0)
          return;
        Assert.IsNotNull(list);
      }
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
      var plugin = new SonyPlugin();
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
      ser.Save();
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
      var plugin = new SonyPlugin();
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
      ser.Save();
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


    #region TestChannelAndFavListEditing_Android
    [TestMethod]
    public void TestChannelAndFavListEditing_Android()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\android_sdb-sat.xml");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SonyPlugin());
    }
    #endregion

    #region TestChannelAndFavListEditing_KDL
    [TestMethod]
    public void TestChannelAndFavListEditing_KDL()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Sony\\TestFiles\\kdl_sdb-cable-sat.xml");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SonyPlugin());
    }
    #endregion
  }
}
