using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Api;
using ChanSort.Loader.Samsung;
using ChanSort.Loader.Samsung.Scm;

namespace Test.Loader.Samsung.Scm
{
  [TestClass]
  public class SamsungScmTest
  {
    #region InitExpectedSamsungData()
    private Dictionary<string, LoaderTestBase.ExpectedData> InitExpectedSamsungData()
    {
      var expected = new[]
                       {
                         new LoaderTestBase.ExpectedData(@"catmater_B\Clone.scm", 31, 272, 0, 0, 0) ,
                         new LoaderTestBase.ExpectedData(@"easy2003_B\easy2003_B.scm", 0, 0, 1225, 0, 0) ,
                         //new ExpectedData(@"_Manu_C\channel_list_LE40C650_1001.scm", 0, 9, 0, 0, 0) 
                       };

      var dict = new Dictionary<string, LoaderTestBase.ExpectedData>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var entry in expected)
        dict[entry.File] = entry;
      return dict;
    }

    #endregion

    #region TestSamsungScmLoader()
    [TestMethod]
    [DeploymentItem("ChanSort.Loader.Samsung\\ChanSort.Loader.Samsung.ini")]
    public void TestSamsungScmLoader()
    {
      var expectedData = this.InitExpectedSamsungData();
      SamsungPlugin plugin = new SamsungPlugin();

      StringBuilder errors = new StringBuilder();
      var list = LoaderTestBase.FindAllFiles("TestFiles_Samsung", "*.scm");
      var models = new Dictionary<string, string>();
      foreach (var file in list)
      {
        if (file.Contains("Apu TV") || file.Contains("__broken"))
          continue;

        Debug.Print("Testing " + file);
        try
        {
          var serializer = plugin.CreateSerializer(file) as ScmSerializer;
          Assert.IsNotNull(serializer, "No Serializer for " + file);

          serializer.Load();

          var fileName = Path.GetFileName(file) ?? "";
          var model = this.GetSamsungModel(file);
          var analogAirList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.AnalogT | ChanSort.Api.SignalSource.Tv);
          var analogCableList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.AnalogC | ChanSort.Api.SignalSource.Tv);
          var digitalAirList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbT | ChanSort.Api.SignalSource.Tv);
          var digitalCableList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbC | ChanSort.Api.SignalSource.Tv);
          var satChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbS | ChanSort.Api.SignalSource.Tv);
          var primeChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.CablePrimeD | ChanSort.Api.SignalSource.Tv);
          var hdplusChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.HdPlusD | ChanSort.Api.SignalSource.Tv);
          var freesatChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.FreesatD | ChanSort.Api.SignalSource.Tv);
          var tivusatChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.TivuSatD | ChanSort.Api.SignalSource.Tv);
          var iptvChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.IP | SignalSource.Tv);

          string key = serializer.Series + 
            "\t" + model +
            "\t" + serializer.AnalogChannelLength +
            "\t" + (analogAirList != null && analogAirList.Count > 0) +
            "\t" + (analogCableList != null && analogCableList.Count > 0) +
            "\t" + serializer.DigitalChannelLength +
            "\t" + (digitalAirList != null && digitalAirList.Count > 0) +
            "\t" + (digitalCableList != null && digitalCableList.Count > 0) +
            "\t" + (primeChannelList != null && primeChannelList.Count > 0) +
            "\t" + serializer.SatChannelLength +
            "\t" + (satChannelList != null && satChannelList.Count > 0) +
            "\t" + serializer.HdPlusChannelLength +
            "\t" + (hdplusChannelList != null && hdplusChannelList.Count > 0) +
            "\t" + (freesatChannelList != null && freesatChannelList.Count > 0) +
            "\t" + (tivusatChannelList != null && tivusatChannelList.Count > 0) +
            "\t" + (iptvChannelList != null && iptvChannelList.Count > 0) +
            "\t" + serializer.SatDatabaseVersion;
          string relPath = Path.GetFileName(Path.GetDirectoryName(file)) + "\\" + fileName;
          models[key] = serializer.Series + 
            "\t" + model +
            "\t" + serializer.AnalogChannelLength +
            "\t" + serializer.DigitalChannelLength +
            "\t" + serializer.SatChannelLength +
            "\t" + serializer.HdPlusChannelLength +
            "\t" + serializer.SatDatabaseVersion +
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
            "\t" + relPath;

          Assert.IsFalse(serializer.DataRoot.IsEmpty, "No channels loaded from " + file);

          LoaderTestBase.ExpectedData exp;
          key = Path.GetFileName(Path.GetDirectoryName(file)) + "\\" + Path.GetFileName(file);
          if (expectedData.TryGetValue(key, out exp))
          {
            var analogTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.AnalogC);
            var dtvTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbC);
            var satTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbS);
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

    #region TestDeletingSatChannel

    [TestMethod]
    public void TestDeletingSatChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\channel_list_T_J_ohne_smart_12.scm");
      var plugin = new SamsungPlugin();
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
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();


      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2w = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2W HD");
      
      // For .scm sat-channels, there is no known "IsDeleted" flag. Instead, the "IsUsed" flag is set to false when saving a channel with IsDeleted==true
      // When loading the file back, it can no longer be distinguished between a garbage record and a deleted record. The loader doesn't add IsUsed=false channels to the list
      Assert.IsNull(orf2w);
      //Assert.IsTrue(orf2w.IsDeleted);
      //Assert.AreEqual(-1, orf2w.OldProgramNr);
    }
    #endregion

    #region TestDeletingCableChannel

    [TestMethod]
    public void TestDeletingCableChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\Samsung_upcmini_EF_12.scm");
      var plugin = new SamsungPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 2 = ORF 2 Wien HD 

      var dvbc = data.GetChannelList(SignalSource.DvbC);
      var orf2w = dvbc.Channels.FirstOrDefault(ch => ch.Name == "ORF 2 Wien HD");
      Assert.IsNotNull(orf2w);
      Assert.AreEqual(2, orf2w.OldProgramNr);
      Assert.AreEqual(2, orf2w.NewProgramNr);
      Assert.IsFalse(orf2w.IsDeleted);

      orf2w.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2w.IsDeleted);
      Assert.AreNotEqual(-1, orf2w.NewProgramNr);
      Assert.AreEqual(0, dvbc.Channels.Count(ch => ch.NewProgramNr <= 0));


      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();


      dvbc = data.GetChannelList(SignalSource.DvbC);
      orf2w = dvbc.Channels.FirstOrDefault(ch => ch.Name == "ORF 2 Wien HD");

      // For .scm sat-channels, there is no known "IsDeleted" flag. Instead, the "IsUsed" flag is set to false when saving a channel with IsDeleted==true
      // When loading the file back, it can no longer be distinguished between a garbage record and a deleted record. The loader doesn't add IsUsed=false channels to the list
      Assert.IsNotNull(orf2w);
      Assert.IsTrue(orf2w.IsDeleted);
      Assert.AreEqual(-1, orf2w.OldProgramNr);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\Channel_list_T_J_ohne_smart_12.scm");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SamsungPlugin());
    }
    #endregion

  }
}
