using System.IO;
using ChanSort.Loader.LG;
using ChanSort.Loader.LG.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestDvbsCleanup : TestBase
  {
    [TestMethod]
    public void TestLM620S_WithSatChannels()
    {
      // "VitorMartinsAugusto"
      this.GenerateTestFiles("DvbsCleanup/xxLM620S-ZE00001", false);
      this.ExecuteTest("DvbsCleanup/xxLM620S-ZE00001");
    }

    [TestMethod]
    public void TestLM860V_WithoutSatChannels()
    {
      // "PDA-User"
      this.GenerateTestFiles("DvbsCleanup/xxLM860V-ZB99998", false);
      this.ExecuteTest("DvbsCleanup/xxLM860V-ZB99998");
    }

    [TestMethod]
    public void TestLM640T_WithBogusDvbsBlock()
    {
      // "OmarGadzhiev"
      this.GenerateTestFiles("DvbsCleanup/xxLM640T-ZA00000", false);
      this.ExecuteTest("DvbsCleanup/xxLM640T-ZA00000");
    }


    private void ExecuteTest(string modelAndBaseName, bool generateReferenceFile = false)
    {
      // copy required input and assertion files
      TestUtils.DeploymentItem("ChanSort.Loader.LG\\ChanSort.Loader.LG.ini");
      TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\" + modelAndBaseName + ".TLL.in");
      TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\" + modelAndBaseName + ".TLL.out");

      var baseName = Path.GetFileNameWithoutExtension(modelAndBaseName);

      // load the TLL file
      var plugin = new LgPlugin();
      var serializer = (TllFileSerializer)plugin.CreateSerializer(baseName + ".TLL.in");
      serializer.IsTesting = true;
      serializer.Load();
      serializer.DataRoot.ApplyCurrentProgramNumbers();     

      // save TLL file and compare to reference file
      serializer.CleanUpChannelData();
      serializer.Save();
      if (generateReferenceFile)
        File.Copy(serializer.FileName, TestUtils.GetSolutionBaseDir() + "\\Test.Loader.LG\\" + modelAndBaseName + ".TLL.out", true);
      else
        AssertBinaryFileContent(serializer.FileName, baseName + ".TLL.out");      
    }
  }
}
