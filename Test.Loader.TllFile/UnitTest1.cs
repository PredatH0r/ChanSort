using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChanSort.Loader.TllFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.TllFile
{
  [TestClass]
  public class UnitTest1
  {
    #region class ExpectedData
    public class ExpectedData
    {
      public readonly string File;
      public readonly int AnalogChannels;
      public readonly int DtvChannels;
      public readonly int SatChannels;
      public readonly int DtvRadio;
      public readonly int SatRadio;

      public ExpectedData(string file, int analog, int dtv, int sat, int dtvRadio, int satRadio)
      {
        this.File = file;
        this.AnalogChannels = analog;
        this.DtvChannels = dtv;
        this.SatChannels = sat;
        this.DtvRadio = dtvRadio;
        this.SatRadio = satRadio;
      }
    }
    #endregion

    private readonly Dictionary<string, ExpectedData> expectedData = new Dictionary<string, ExpectedData>();

    #region ctor()
    public UnitTest1()
    {
      var expected = new [] {
        new ExpectedData(@"opel\xx2350D-PZM00001.TLL", 31, 345, 0, 89, 0), // 164/-
        new ExpectedData(@"defycgn\xxLH4010-ZD99970.TLL", 34, 280, 0, 129, 0), // 164/-
        new ExpectedData(@"karlv\xxLD750-ZA00001.TLL", 30, 248, 0, 104, 0), // 176/-
        new ExpectedData(@"JLevi\xxLE5500-ZA00002.TLL", 36, 0, 0,0, 0), // 176/-
        new ExpectedData(@"chlabnet\xxLK450-ZB00001.TLL", 34, 310, 0, 128, 0), // 176/-
        new ExpectedData(@"Stabilo\xxLW4500-ZB00001.TLL", 31 ,338, 0, 34, 0), // 176/-
        new ExpectedData(@"MarioAntonioLiptaj\xxPT353-ZA00001.TLL", 50, 123, 0, 13, 0), // 180/-
        new ExpectedData(@"Muffix\xxLW5500-ZE00001.TLL", 34, 290, 0, 125, 0), // 184/-
        new ExpectedData(@"FranzSteinert\xxCS460S-ZA00001.TLL", 0, 0, 1261, 0, 200), // ?/68
        new ExpectedData(@"Klausi1\xxLK950S-ZA00001.TLL", 37, 390, 2695, 150, 491), // 184/68
        new ExpectedData(@"MP3Chris2712\xxLV570S-ZB00001.TLL", 0, 12, 2895, 0, 669), // 184/68
        new ExpectedData(@"decklen\xxLW570S-ZD00001.TLL", 0, 30, 1598, 0, 339), // 184/68
        new ExpectedData(@"NeuerScan\xxLM340S-ZA00001.TLL", 34, 317, 1698, 129, 264), // 188/68
        new ExpectedData(@"wagnale\xxLM611S-ZA00001.TLL", 0, 13, 1094, 0, 191), // 188/68
        new ExpectedData(@"_Pred\xxLM620S-ZE00021.TLL", 0, 11, 1303, 0, 191) // 192/72
      };

      foreach (var entry in expected)
        this.expectedData[entry.File] = entry;
    }
    #endregion

    [TestMethod]
    public void MetaTest()
    {      
    }

    #region TestLoadingAllTllFilesInTestFilesDirectory()
    [TestMethod]
    [DeploymentItem("ChanSort.Loader.TllFile\\ChanSort.Loader.TllFile.ini")]
    public void TestLoadingAllTllFilesInTestFilesDirectory()
    {
      TllFileSerializerPlugin plugin = new TllFileSerializerPlugin();

      StringBuilder errors = new StringBuilder();
      var list = this.FindAllTllFiles();
      var models = new Dictionary<string,string>();
      var firmwareSize = new Dictionary<int, string>();
      foreach(var file in list)
      {
        if (file.Contains("GlobalClone") || file.Contains("CountrySettings"))
          continue;
        Debug.Print("Testing " + file);
        try
        {
          var serializer = plugin.CreateSerializer(file) as TllFileSerializer;
          Assert.IsNotNull(serializer, "No Serializer for " + file);
          
          serializer.Load();

          var fileName = Path.GetFileName(file) ?? "";
          int idx = fileName.IndexOf("-");
          string key = idx < 0 ? fileName : fileName.Substring(0, idx);
          var satChannelList = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbS|ChanSort.Api.SignalSource.Tv);
          key += "\t" + serializer.ACTChannelLength+
            "\t"+serializer.HasDvbs+
            "\t"+serializer.SatChannelLength+
            "\t" + (satChannelList == null ? "n/a" : satChannelList.Count.ToString());
          string relPath = Path.GetFileName(Path.GetDirectoryName(file))+"\\"+fileName;          
          models[key] = relPath;

          var model = this.GetModel(file);
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
          this.expectedData.TryGetValue(key, out exp);
          var analogTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.AnalogCT|ChanSort.Api.SignalSource.Tv);
          var dtvTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbCT|ChanSort.Api.SignalSource.Tv);
          var satTv = serializer.DataRoot.GetChannelList(ChanSort.Api.SignalSource.DvbS | ChanSort.Api.SignalSource.Tv);
          if (exp != null)
          {
            this.expectedData.Remove(key);
            Assert.AreEqual(exp.AnalogChannels, analogTv.Channels.Count, file + ": analog");
            Assert.AreEqual(exp.DtvChannels, dtvTv.Channels.Count, file + ": DTV");
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
        Debug.WriteLine(model.Key + "\t"+model.Value);

      foreach (var size in firmwareSize.OrderBy(e => e.Key))
        Debug.WriteLine(size.Key + "\t" + size.Value);

      if (this.expectedData.Count > 0)
        Assert.Fail("Some files were not tested: " + this.expectedData.Keys.Aggregate((prev,cur) => prev+","+cur));
      Assert.AreEqual("", errors.ToString());
    }
    #endregion

    private string GetModel(string filePath)
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

    #region Helper Methods

    private IEnumerable<string> FindAllTllFiles()
    {
      string path = this.GetTestFileDirectory();
      List<string> files = new List<string>();
      this.FindAllTllFilesRecursively(path, files);
      return files;
    }

    private string GetTestFileDirectory()
    {
      string exeDir = Assembly.GetExecutingAssembly().Location;
      while (!string.IsNullOrEmpty(exeDir))
      {
        string testFileDir = Path.Combine(exeDir, "TestFiles");
        if (Directory.Exists(testFileDir))
          return testFileDir;
        exeDir = Path.GetDirectoryName(exeDir);
      }
      throw new FileNotFoundException("No 'TestFiles' directory found");
    }

    private void FindAllTllFilesRecursively(string path, List<string> files)
    {
      files.AddRange(Directory.GetFiles(path, "*.TLL"));
      foreach (var dir in Directory.GetDirectories(path))
        this.FindAllTllFilesRecursively(dir, files);
    }
    #endregion
  }
}
