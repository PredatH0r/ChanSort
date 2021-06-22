using ChanSort.Loader.Samsung.Scm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Samsung.Scm
{
  [TestClass]
  public class FileFormatDetectionTest
  {
    [TestMethod]
    public void LoadFileWithExcessiveHighFrequency_1()
    {
      // this seems to be a corrupt file caused by a buffer-overflow from analog channel names into the frequency data bytes
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\channel_list_UE55H6470_1201-Suchlauf-2015-04-26.scm");
      var s = new ScmSerializer(tempFile);
      s.Load();
    }

    [TestMethod]
    public void LoadFileWithExcessiveHighFrequency_2()
    {
      // this seems to be a corrupt file caused by a buffer-overflow from analog channel names into the frequency data bytes
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\channel_list_UE55H6470_1201.scm");
      var s = new ScmSerializer(tempFile);
      s.Load();
    }

    [TestMethod]
    public void LoadRenamedFile_HE40Cxxx_1201()
    {
      // This file uses the 1201 format (E,F,H,J), but has a "C" in its model name
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\E_format_with_C_model_name.scm");
      var s = new ScmSerializer(tempFile);
      s.Load();
      Assert.AreEqual("E", s.Series);
    }

    [TestMethod]
    public void LoadRenamedFile_LT24B_1201()
    {
      // This file uses the 1201 format (E,F,H,J), but has a "B" in its model name
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\E_format_with_B_model_name.scm");
      var s = new ScmSerializer(tempFile);
      s.Load();
      Assert.AreEqual("E", s.Series);
    }

    [TestMethod]
    public void LoadJSeriesWithScm1201Format()
    {
      // J-series model with E-J series SCM format
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Samsung\\Scm\\TestFiles\\channel_list_UE32J5170_1201_orig.scm");
      var s = new ScmSerializer(tempFile);
      s.Load();
      Assert.AreEqual("E", s.Series);
    }

  }
}
