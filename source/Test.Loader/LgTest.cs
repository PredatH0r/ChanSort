using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Loader.LG;

namespace Test.Loader
{
  [TestClass]
  public class LgTest : LoaderTestBase
  {
    #region TestLgTllLoader()
    [TestMethod]
    [DeploymentItem("ChanSort.Loader.LG\\ChanSort.Loader.LG.ini")]
    public void TestLgTllLoader()
    {
      var expectedData = this.InitExpectedLgData();
      TllFileSerializerPlugin plugin = new TllFileSerializerPlugin();

      StringBuilder errors = new StringBuilder();
      var list = this.FindAllFiles("TestFiles_LG", "*.tll");
      var models = new Dictionary<string,string>();
      var firmwareSize = new Dictionary<int, string>();
      foreach(var file in list)
      {
        if (file.Contains("GlobalClone") || file.Contains("CountrySettings") || file.Contains("LV661H"))
          continue;
        //Debug.Print("Testing " + file);
        try
        {
          var serializer = plugin.CreateSerializer(file) as TllFileSerializer;
          Assert.IsNotNull(serializer, "No Serializer for " + file);
          serializer.IsTesting = true;
          serializer.Load();

          var fileName = Path.GetFileName(file) ?? "";
          var model = this.GetLgModel(file);
          var analogList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.AnalogCT | ChanSort.Api.SignalSource.Tv);
          var dvbcList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbC | ChanSort.Api.SignalSource.Tv);
          var dvbtList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbT | ChanSort.Api.SignalSource.Tv);
          var satChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbS | ChanSort.Api.SignalSource.Tv);
          string key = 
            model +
            "\t" + serializer.ACTChannelLength+
            "\t" + (analogList != null && analogList.Count > 0) +
            "\t" + (dvbtList != null && dvbtList.Count > 0) +
            "\t" + (dvbcList != null && dvbcList.Count > 0) +
            "\t" + serializer.SatChannelLength +
            "\t" + (satChannelList != null && satChannelList.Count > 0) +
            "\t" + (dvbtList != null && dvbtList.PresetProgramNrCount > 0) +
            "\t" + (dvbcList != null && dvbcList.PresetProgramNrCount > 0) +
            "\t" + (satChannelList != null && satChannelList.PresetProgramNrCount > 0) +
            "\t" + serializer.TvCountryCode;

          string relPath = Path.GetFileName(Path.GetDirectoryName(file))+"\\"+fileName;          
          models[key] = model + 
            "\t" + serializer.ACTChannelLength + 
            "\t" + serializer.SatChannelLength +
            "\t" + (analogList == null ? 0 : analogList.Count) +
            "\t" + (dvbtList == null ? 0 : dvbtList.Count) +
            "\t" + (dvbcList == null ? 0 : dvbcList.Count) +
            "\t" + (satChannelList == null ? 0 : satChannelList.Count) +
            "\t" + (dvbtList == null ? 0 : dvbtList.PresetProgramNrCount) +
            "\t" + (dvbcList == null ? 0 : dvbcList.PresetProgramNrCount) +
            "\t" + (satChannelList == null ? 0 : satChannelList.PresetProgramNrCount) +
            "\t" + serializer.TvCountryCode +
            "\t" + serializer.DvbsSymbolRateCorrectionFactor +
            "\t" + relPath;

          
          if (firmwareSize.ContainsKey(serializer.FirmwareBlockSize))
          {
            string x = firmwareSize[serializer.FirmwareBlockSize];
            if (!x.Contains(model))
              firmwareSize[serializer.FirmwareBlockSize] = x + ", " + model;
          }
          else
            firmwareSize[serializer.FirmwareBlockSize] = model;



          Assert.IsFalse(serializer.DataRoot.IsEmpty, "No channels loaded from " + file);

          ExpectedData exp;
          key = Path.GetFileName(Path.GetDirectoryName(file)) + "\\" + Path.GetFileName(file);
          if (expectedData.TryGetValue(key, out exp))
          {
            var analogTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.AnalogCT|ChanSort.Api.SignalSource.Tv);
            var dvbcTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbC|ChanSort.Api.SignalSource.Tv);
            var dvbtTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbT | ChanSort.Api.SignalSource.Tv);
            var dtvTv = dvbcTv.Channels.Count > 0 ? dvbcTv : dvbtTv;
            var satTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbS | ChanSort.Api.SignalSource.Tv);
            expectedData.Remove(key);
            Assert.AreEqual(exp.AnalogChannels, analogTv.Channels.Count, file + ": analog");
            Assert.AreEqual(exp.DtvChannels, dtvTv.Channels.Count, file + ": DTV");
            if (exp.SatChannels != 0)
              Assert.AreEqual(exp.SatChannels, satTv.Channels.Count, file + ": Sat");
          }
        }
        catch(Exception ex)
        {
          errors.AppendLine();
          errors.AppendLine();
          errors.AppendLine(file);
          errors.AppendLine(ex.ToString());
        }
      }

      foreach(var model in models.OrderBy(e => e.Key))
        Debug.WriteLine(model.Value);

      foreach (var size in firmwareSize.OrderBy(e => e.Key))
        Debug.WriteLine(size.Key + "\t" + size.Value);

      if (expectedData.Count > 0)
        Assert.Fail("Some files were not tested: " + expectedData.Keys.Aggregate((prev,cur) => prev+","+cur));
      Assert.AreEqual("", errors.ToString());
    }
    #endregion

    #region InitExpectedLgData()
    private Dictionary<string,ExpectedData> InitExpectedLgData()
    {
      var expected = new[]
                       {
                         new ExpectedData(@"opel\xx2350D-PZM00001.TLL", 31, 345, 0, 89, 0), // 164/-
                         new ExpectedData(@"defycgn\xxLH4010-ZD99970.TLL", 34, 280, 0, 129, 0), // 164/-
                         new ExpectedData(@"karlv\xxLD750-ZA00001.TLL", 30, 248, 0, 104, 0), // 176/-
                         new ExpectedData(@"JLevi\xxLE5500-ZA00002.TLL", 36, 0, 0, 0, 0), // 176/-
                         new ExpectedData(@"chlabnet\xxLK450-ZB00001.TLL", 34, 310, 0, 128, 0), // 176/-
                         new ExpectedData(@"Stabilo\xxLW4500-ZB00001.TLL", 31, 338, 0, 34, 0), // 176/-
                         new ExpectedData(@"MarioAntonioLiptaj\xxPT353-ZA00001.TLL", 50, 123, 0, 13, 0), // 180/-
                         new ExpectedData(@"Muffix\xxLW5500-ZE00001.TLL", 34, 290, 0, 125, 0), // 184/-
                         new ExpectedData(@"FranzSteinert\xxCS460S-ZA00001.TLL", 0, 0, 1261 + 118, 0, 200), // ?/68
                         new ExpectedData(@"Klausi1\xxLK950S-ZA00001.TLL", 37, 390, 2695, 150, 491), // 184/68
                         new ExpectedData(@"MP3Chris2712\xxLV570S-ZB00001.TLL", 0, 12, 2895, 0, 669), // 184/68
                         new ExpectedData(@"decklen\xxLW570S-ZD00001.TLL", 0, 30, 1598, 0, 339), // 184/68
                         new ExpectedData(@"NeuerScan\xxLM340S-ZA00001.TLL", 34, 317, 1698, 129, 264), // 188/68
                         new ExpectedData(@"wagnale\xxLM611S-ZA00001.TLL", 0, 13, 1094 + 70, 0, 191), // 188/68
                         new ExpectedData(@"!Pred\xxLM620S-ZE00021.TLL", 0, 11, 1303, 0, 191) // 192/72
                       };

      var dict = new Dictionary<string, ExpectedData>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var entry in expected)
        dict[entry.File] = entry;
      return dict;
    }

    #endregion

    private string GetLgModel(string filePath)
    {
      string name = Path.GetFileName(filePath)??"";
      if (name.StartsWith("xx"))
      {
        int idx = name.IndexOf("-");
        if (idx > 0)
          return name.Substring(2, idx - 2);
      }
      return filePath;
    }
  }
}
