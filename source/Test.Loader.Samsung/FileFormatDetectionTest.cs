using System;
using System.IO;
using System.Text;
using ChanSort.Loader.Samsung;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Samsung
{
  [TestClass]
  public class FileFormatDetectionTest
  {
    private readonly StringBuilder errors = new StringBuilder();
    private const string RootPath = @"d:\sources\ChanSort\TestFiles_Samsung\";

    #region LoadAllFilesAndLogDetecedFileFormat()

    [TestMethod]
    public void LoadAllFilesAndLogDetecedFileFormat()
    {
      
      RecurseInto(RootPath);

      Assert.AreEqual("", errors.ToString());
    }

    private void RecurseInto(string dir)
    {
      foreach (var path in Directory.GetFiles(dir, "*.scm"))
      {
        if (path.Contains("_null"))
          continue;
        try
        {
          var s = new ScmSerializer(path);
          s.Load();
          var series = s.Series;
          System.Diagnostics.Debug.WriteLine($"{path.Substring(RootPath.Length)}: {series}");
        }
        catch (Exception ex)
        {
          errors.AppendLine($"{path.Substring(RootPath.Length)} : {ex.Message}");
        }
      }

      foreach (var path in Directory.GetDirectories(dir))
        RecurseInto(path);
    }
    #endregion

    [TestMethod]
    public void LoadFileWithExcessiveHighFrequency_1()
    {
      // this seems to be a corrupt file caused by a buffer-overflow from analog channel names into the frequency data bytes
      var s = new ScmSerializer(RootPath + @"ThomasSaur_DH\channel_list_UE55H6470_1201-Suchlauf-2015-04-26.scm");
      s.Load();
    }

    [TestMethod]
    public void LoadFileWithExcessiveHighFrequency_2()
    {
      // this seems to be a corrupt file caused by a buffer-overflow from analog channel names into the frequency data bytes
      var s = new ScmSerializer(RootPath + @"ThomasSaur_DH\channel_list_UE55H6470_1201.scm");
      s.Load();
    }

    [TestMethod]
    public void LoadRenamedFile_HE40Cxxx_1201()
    {
      // This file uses the 1201 format (E,F,H,J), but has a "C" in its model name
      var s = new ScmSerializer(RootPath + @"__C=F\Kinig\Reier Monika.scm");
      s.Load();
      Assert.AreEqual("F", s.Series);
    }

    [TestMethod]
    public void LoadRenamedFile_LT24B_1201()
    {
      // This file uses the 1201 format (E,F,H,J), but has a "B" in its model name
      var s = new ScmSerializer(RootPath + @"__B=F\DieterHerzberg_B\renamed.scm");
      s.Load();
      Assert.AreEqual("F", s.Series);
    }

    [TestMethod]
    public void LoadJSeriesWithScm1201Format()
    {
      // J-series model with SCM format
      var s = new ScmSerializer(RootPath + @"__J\HenryLoenwind_SCM\channel_list_UE32J5170_1201_orig.scm");
      s.Load();
      Assert.AreEqual("F", s.Series);
    }

  }
}
