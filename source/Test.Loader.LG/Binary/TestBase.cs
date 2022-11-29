using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Api;
using ChanSort.Loader.LG;
using ChanSort.Loader.LG.Binary;
using Assert = NUnit.Framework.Assert;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestBase
  {
    protected string tempFile;

    #region ExecuteTest()
    protected void ExecuteTest(string modelAndBaseName)
    {
      // copy required input and assertion files
      TestUtils.DeploymentItem("ChanSort.Loader.LG\\ChanSort.Loader.LG.ini");
      TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\" + modelAndBaseName + ".TLL.in");
      TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\" + modelAndBaseName + ".csv.in");
      TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\" + modelAndBaseName + ".TLL.out");

      var baseName = Path.GetFileNameWithoutExtension(modelAndBaseName);

      // load the TLL file
      tempFile = baseName + ".TLL.in";
      var plugin = new LgPlugin();
      var serializer = (TllFileSerializer)plugin.CreateSerializer(tempFile);
      serializer.IsTesting = true;
      serializer.Load();

      // verify channel name, number, favorites, ... against a reference list
      var data = serializer.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();
      AssertRefListContent(data, baseName + ".csv.in");

      // modify channel lists
      Editor editor = new Editor();
      editor.DataRoot = data;
      foreach (var list in data.ChannelLists)
      {
        var channels = this.Get2ndProgramNumber(list);
        if (channels != null)
        {
          editor.ChannelList = list;
          editor.MoveChannels(channels, true);
        }
      }

      // save TLL file and compare to reference file
      serializer.Save();
      AssertBinaryFileContent(tempFile, baseName + ".TLL.out");
    }
    #endregion

    #region Get2ndProgramNumber
    private IList<ChannelInfo> Get2ndProgramNumber(ChannelList list)
    {
      var channels = list.GetChannelByNewProgNr(2);
      if (channels.Count == 1)
        return channels;

      var ch = list.Channels.OrderBy(c => c.NewProgramNr <= 0 ? 10000 : c.NewProgramNr).FirstOrDefault();
      if (ch == null)
        return null;
      channels = new List<ChannelInfo>();
      channels.Add(ch);
      return channels;
    }
    #endregion

    #region AssertRefListContent()
    private void AssertRefListContent(DataRoot dataRoot, string refListFile)
    {
      MemoryStream mem = new MemoryStream();
      var writer = new StreamWriter(mem);
      CsvRefListSerializer.Save(writer, dataRoot, false); // don't include deleted channels to maintain compatibility between new code and old test files
      writer.Flush();
      mem.Seek(0, SeekOrigin.Begin);
      var actual = new StreamReader(mem).ReadToEnd();
      var expected = File.ReadAllText(refListFile);

      // satellite orbital position is no longer part of the UID (Samsung J series doesn't provide this information)
      var regex = new Regex(@",S\d+.\d+[EW](-\d+-\d+-\d+,)");
      expected = regex.Replace(expected, @",S$1"); 

      Assert.AreEqual(expected, actual);
    }
    #endregion

    #region AssertBinaryFileContent()
    protected void AssertBinaryFileContent(string actualFile, string expectedFile)
    {
      if (StringComparer.InvariantCultureIgnoreCase.Equals(actualFile, expectedFile)) 
        throw new ArgumentException("input and verification file must not be the same");

      var actual = File.ReadAllBytes(actualFile);
      var expected = File.ReadAllBytes(expectedFile);
      Assert.AreEqual(expected.Length, actual.Length);
      for (int i = 0; i < actual.Length; i++)
      {
        if (actual[i] != expected[i])
        {
          StringBuilder sb1 = new StringBuilder();
          StringBuilder sb2 = new StringBuilder();
          for (int j = 0; j < 8 && i + j < expected.Length; j++)
          {
            sb1.AppendFormat("{0:X2} ", expected[i + j]);
            sb2.AppendFormat("{0:X2} ", actual[i + j]);
          }
          Assert.Fail("Files differ at index {0}: expected <{1}> but found <{2}>", i, sb1, sb2);
        }
      }
    }
    #endregion

    
    #region TearDown()
    [TestCleanup]
    public void TearDown()
    {
      if (File.Exists(this.tempFile))
        File.Delete(this.tempFile);
    }
    #endregion

    #region GenerateTestFiles()
    protected void GenerateTestFiles(string modelAndBaseName, bool moveChannels = true)
    {
      TestUtils.DeploymentItem("ChanSort.Loader.LG\\ChanSort.Loader.LG.ini");
      var testDataDir = TestUtils.GetSolutionBaseDir() + "\\Test.Loader.LG\\Binary\\" + Path.GetDirectoryName(modelAndBaseName);
      var basename = Path.GetFileNameWithoutExtension(modelAndBaseName);

      // copy .TLL.in
      string destFileName = testDataDir + "\\" + basename + ".TLL.in";
      if (!File.Exists(testDataDir + "\\" + basename + ".TLL.in"))
      {
        if (File.Exists(testDataDir + "\\" + basename + ".TLL.bak"))
          File.Move(testDataDir + "\\" + basename + ".TLL.bak", destFileName);
        else
          File.Move(testDataDir + "\\" + basename + ".TLL", destFileName);
      }

      var outPath = testDataDir + "\\" + basename + ".TLL.out";
      File.Copy(destFileName, outPath, true);

      TllFileSerializer tll = new TllFileSerializer(outPath);
      tll.IsTesting = true;
      tll.Load();
      tll.DataRoot.ApplyCurrentProgramNumbers();
      if (moveChannels)
      {
        // save .csv.in file (with ref list of original .TLL.in)
        using (var writer = new StringWriter())
        {
          CsvRefListSerializer.Save(writer, tll.DataRoot);
          File.WriteAllText(testDataDir + "\\" + basename + ".csv.in", writer.ToString());
        }

        // save modified list as .TLL.out
        Console.WriteLine();
        Console.WriteLine(basename);
        Console.WriteLine("   a/c/t={0}, sat={1}", tll.ACTChannelLength, tll.SatChannelLength);
        Editor editor = new Editor();
        editor.DataRoot = tll.DataRoot;
        foreach (var list in tll.DataRoot.ChannelLists)
        {
          editor.ChannelList = list;
          var channels = this.Get2ndProgramNumber(list);
          if (channels != null)
          {
            editor.MoveChannels(channels, true);
            Console.WriteLine("   {0}={1}", list.ShortCaption, list.Count);
          }
        }
      }
      else
      {
        tll.CleanUpChannelData();
      }
      tll.Save();
    }
    #endregion
  }
}
