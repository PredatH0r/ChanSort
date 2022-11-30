using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq;
using System.Text;
using ChanSort;
using ChanSort.Api;
using ChanSort.Loader.Samsung;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Samsung.Zip
{
  [TestClass]
  public class SamsungZipTest
  {
    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      TestChannelsAddedToCorrectLists("Channel_list_T-KTSUDEUC-1007.1.zip", SignalSource.DvbS, 1323, 878, 380, 4008, "Humax ESD 160C");
    }
    #endregion

    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      TestChannelsAddedToCorrectLists("Channel_list_T-KTMDEUC-1132.6.zip", SignalSource.DvbC, 146, 65, 75, 4008, "Humax 160C");
    }
    #endregion

    #region TestAntennaChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAntennaChannelsAddedToCorrectLists()
    {
      TestChannelsAddedToCorrectLists("Channel_list_T-KTSUDEUC-1007.2.zip", SignalSource.DvbT, 77, 71, 4, 3995, "Irdeto Code 4");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Zip\\TestFiles\\" + fileName);
      var plugin = new SamsungPlugin();
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

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Zip\\TestFiles\\Channel_list_T-KTSUDEUC-1007.3.zip");
      var plugin = new SamsungPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 418 = ORF2E 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      Assert.IsNotNull(dvbs);
      var orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNotNull(orf2e);
      Assert.AreEqual(418, orf2e.OldProgramNr);
      Assert.AreEqual(418, orf2e.NewProgramNr);
      Assert.IsFalse(orf2e.IsDeleted);

      orf2e.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2e.IsDeleted);
      Assert.AreNotEqual(-1, orf2e.NewProgramNr);
      Assert.AreEqual(1, dvbs.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();


      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2e = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2E");
      Assert.IsNull(orf2e);
    }
    #endregion


    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Zip\\TestFiles\\Channel_list_T-KTMDEUC-1132.6.zip");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SamsungPlugin());
    }
    #endregion


    // unstructured test crawling through all files in the sample directories

    #region InitExpectedSamsungData()
    private Dictionary<string, LoaderTestBase.ExpectedData> InitExpectedSamsungData()
    {
      var expected = new LoaderTestBase.ExpectedData[]
                       {
                         //new ExpectedData(@"catmater_B\Clone.scm", 31, 272, 0, 0, 0) ,
                         //new ExpectedData(@"easy2003_B\easy2003_B.scm", 0, 0, 1225, 0, 0) ,
                         //new ExpectedData(@"_Manu_C\channel_list_LE40C650_1001.scm", 0, 9, 0, 0, 0) 
                       };

      var dict = new Dictionary<string, LoaderTestBase.ExpectedData>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var entry in expected)
        dict[entry.File] = entry;
      return dict;
    }

    #endregion

    #region TestSamsungZipLoader()
    [TestMethod]
    public void TestSamsungZipLoader()
    {
      var expectedData = InitExpectedSamsungData();
      SamsungPlugin plugin = new SamsungPlugin();

      TestUtils.DeploymentItem("ChanSort.Loader.Samsung\\ChanSort.Loader.Samsung.ini");

      StringBuilder errors = new StringBuilder();
      var list = LoaderTestBase.FindAllFiles("TestFiles_Samsung", "*.zip");
      var models = new Dictionary<string, string>();
      foreach (var file in list)
      {
        var lower = file.ToLowerInvariant();
        if (lower.Contains("clone") || lower.Contains("__broken"))
          continue;

        Debug.Print("Testing " + file);
        try
        {
          var serializer = plugin.CreateSerializer(file) as ChanSort.Loader.Samsung.Zip.DbSerializer;
          Assert.IsNotNull(serializer, "No Serializer for " + file);

          serializer.Load();

          var fileName = Path.GetFileName(file) ?? "";
          var model = GetSamsungModel(file);
          var analogAirList = serializer.DataRoot.GetChannelList(SignalSource.AnalogT | SignalSource.Tv);
          var analogCableList = serializer.DataRoot.GetChannelList(SignalSource.AnalogC | SignalSource.Tv);
          var digitalAirList = serializer.DataRoot.GetChannelList(SignalSource.DvbT | SignalSource.Tv);
          var digitalCableList = serializer.DataRoot.GetChannelList(SignalSource.DvbC | SignalSource.Tv);
          var satChannelList = serializer.DataRoot.GetChannelList(SignalSource.DvbS | SignalSource.Tv);
          var primeChannelList = serializer.DataRoot.GetChannelList(SignalSource.CablePrimeD | SignalSource.Tv);
          var hdplusChannelList = serializer.DataRoot.GetChannelList(SignalSource.HdPlusD | SignalSource.Tv);
          var freesatChannelList = serializer.DataRoot.GetChannelList(SignalSource.FreesatD | SignalSource.Tv);
          var tivusatChannelList = serializer.DataRoot.GetChannelList(SignalSource.TivuSatD | SignalSource.Tv);
          var iptvChannelList = serializer.DataRoot.GetChannelList(SignalSource.IP | SignalSource.Tv);

          string key = serializer.FileFormatVersion +
            "\t" + model +
            "\t" + (analogAirList != null && analogAirList.Count > 0) +
            "\t" + (analogCableList != null && analogCableList.Count > 0) +
            "\t" + (digitalAirList != null && digitalAirList.Count > 0) +
            "\t" + (digitalCableList != null && digitalCableList.Count > 0) +
            "\t" + (primeChannelList != null && primeChannelList.Count > 0) +
            "\t" + (satChannelList != null && satChannelList.Count > 0) +
            "\t" + (hdplusChannelList != null && hdplusChannelList.Count > 0) +
            "\t" + (freesatChannelList != null && freesatChannelList.Count > 0) +
            "\t" + (tivusatChannelList != null && tivusatChannelList.Count > 0) +
            "\t" + (iptvChannelList != null && iptvChannelList.Count > 0);
          string relPath = Path.GetFileName(Path.GetDirectoryName(file)) + "\\" + fileName;
          models[key] = serializer.FileFormatVersion +
            "\t" + model +
            "\t" + (analogAirList == null ? 0 : analogAirList.Count) +
            "\t" + (analogCableList == null ? 0 : analogCableList.Count) +
            "\t" + (digitalAirList == null ? 0 : digitalAirList.Count) +
            "\t" + (digitalCableList == null ? 0 : digitalCableList.Count) +
            "\t" + (primeChannelList == null ? 0 : primeChannelList.Count) +
            "\t" + (satChannelList == null ? 0 : satChannelList.Count) +
            "\t" + (hdplusChannelList == null ? 0 : hdplusChannelList.Count) +
            "\t" + (freesatChannelList == null ? 0 : freesatChannelList.Count) +
            "\t" + (tivusatChannelList == null ? 0 : tivusatChannelList.Count) +
            "\t" + (iptvChannelList == null ? 0 : iptvChannelList.Count) +
            "\t" + serializer.EncodingInfo +
            "\t" + relPath;

          Assert.IsFalse(serializer.DataRoot.IsEmpty, "No channels loaded from " + file);

          LoaderTestBase.ExpectedData exp;
          key = Path.GetFileName(Path.GetDirectoryName(file)) + "\\" + Path.GetFileName(file);
          if (expectedData.TryGetValue(key, out exp))
          {
            var analogTv = serializer.DataRoot.GetChannelList(SignalSource.AnalogC);
            var dtvTv = serializer.DataRoot.GetChannelList(SignalSource.DvbC);
            var satTv = serializer.DataRoot.GetChannelList(SignalSource.DvbS);
            expectedData.Remove(key);
            if (exp.AnalogChannels != 0 || analogTv != null)
              Assert.AreEqual(exp.AnalogChannels, analogTv.Channels.Count, file + ": analog");
            if (exp.DtvChannels != 0 || dtvTv != null)
              Assert.AreEqual(exp.DtvChannels, dtvTv.Channels.Count, file + ": DTV");
            if (exp.SatChannels != 0 || satTv != null)
              Assert.AreEqual(exp.SatChannels, satTv.Channels.Count, file + ": Sat");
          }
        }
        catch (Exception ex)
        {
          if (ex is LoaderException lex && lex.Recovery == LoaderException.RecoveryMode.TryNext)
            continue;
          errors.AppendLine();
          errors.AppendLine();
          errors.AppendLine(file);
          errors.AppendLine(ex.ToString());
        }
      }

      foreach (var model in models.OrderBy(e => e.Key))
        Debug.WriteLine(model.Value);

      if (expectedData.Count > 0)
        Assert.Fail("Some files were not tested: " + expectedData.Keys.Aggregate((prev, cur) => prev + "," + cur));
      Assert.AreEqual("", errors.ToString());
    }

    private string GetSamsungModel(string filePath)
    {
      string fileName = Path.GetFileNameWithoutExtension(filePath) ?? "";
      return fileName.StartsWith("channel_list_") ? fileName.Substring(13, fileName.IndexOf('_', 14) - 13) : fileName;
    }
    #endregion

  }
}
