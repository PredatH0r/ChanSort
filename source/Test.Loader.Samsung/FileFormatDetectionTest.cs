using System;
using System.IO;
using ChanSort.Loader.Samsung;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Samsung
{
  [TestClass]
  public class FileFormatDetectionTest
  {
    private static readonly string RootPath;

    static FileFormatDetectionTest()
    {
      RootPath = GetSolutionBaseDir() + @"\Test.Loader.Samsung\TestFiles\";
    }

    #region GetSolutionBaseDir()
    protected static string GetSolutionBaseDir()
    {
      var dir = Path.GetDirectoryName(typeof(FileFormatDetectionTest).Assembly.Location);
      do
      {
        if (File.Exists(dir + "\\ChanSort.sln"))
          return dir;
        dir = Path.GetDirectoryName(dir);
      } while (!string.IsNullOrEmpty(dir));

      dir = Environment.CurrentDirectory;
      do
      {
        if (File.Exists(dir + "\\ChanSort.sln"))
          return dir;
        dir = Path.GetDirectoryName(dir);
      } while (!string.IsNullOrEmpty(dir));

      throw new InvalidOperationException("Cannot determine base directory of ChanSort solution");
    }
    #endregion

    [TestMethod]
    public void LoadFileWithExcessiveHighFrequency_1()
    {
      // this seems to be a corrupt file caused by a buffer-overflow from analog channel names into the frequency data bytes
      var s = new ScmSerializer(RootPath + @"channel_list_UE55H6470_1201-Suchlauf-2015-04-26.scm");
      s.Load();
    }

    [TestMethod]
    public void LoadFileWithExcessiveHighFrequency_2()
    {
      // this seems to be a corrupt file caused by a buffer-overflow from analog channel names into the frequency data bytes
      var s = new ScmSerializer(RootPath + @"channel_list_UE55H6470_1201.scm");
      s.Load();
    }

    [TestMethod]
    public void LoadRenamedFile_HE40Cxxx_1201()
    {
      // This file uses the 1201 format (E,F,H,J), but has a "C" in its model name
      var s = new ScmSerializer(RootPath + @"E_format_with_C_model_name.scm");
      s.Load();
      Assert.AreEqual("E", s.Series);
    }

    [TestMethod]
    public void LoadRenamedFile_LT24B_1201()
    {
      // This file uses the 1201 format (E,F,H,J), but has a "B" in its model name
      var s = new ScmSerializer(RootPath + @"E_format_with_C_model_name.scm");
      s.Load();
      Assert.AreEqual("E", s.Series);
    }

    [TestMethod]
    public void LoadJSeriesWithScm1201Format()
    {
      // J-series model with E-J series SCM format
      var s = new ScmSerializer(RootPath + @"channel_list_UE32J5170_1201_orig.scm");
      s.Load();
      Assert.AreEqual("E", s.Series);
    }

  }
}
