using System.IO;
using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.LG;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.LG.Binary
{
  [TestClass]
  public class TestLM : TestBase
  {
    [TestMethod]
    public void TestLM340S_AC188TR_Sat68TR()
    {
      // "Tom2012"
      //this.GenerateTestFiles("LM/xxLM340S-ZA00001");
      this.ExecuteTest("LM/xxLM340S-ZA00001");
    }

    [TestMethod]
    public void TestLM611S_T188_Sat68TR()
    {
      // "wagnale"
      //this.GenerateTestFiles("LM/xxLM611S-ZA00001");
      this.ExecuteTest("LM/xxLM611S-ZA00001");
    }

    [TestMethod]
    public void TestLM620S_AT192T_Sat72TR()
    {
      // "VitorMartinsAugusto"
      //this.GenerateTestFiles("LM/xxLM620S-ZE00001");
      this.ExecuteTest("LM/xxLM620S-ZE00001");
    }

    [TestMethod]
    public void TestLM860V_C192TR_Sat72()
    {
      // "PDA-User"
      //this.GenerateTestFiles("LM/xxLM860V-ZB99998");
      this.ExecuteTest("LM/xxLM860V-ZB99998");
    }

    #region TestDeletingChannel

    [TestMethod]
    public void TestDeletingChannel()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\LM\\xxLM620s-ZE00001.TLL.in");
      var plugin = new LgPlugin() { IsTesting = true };
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // Pr# 127 = ORF2 HD 

      var dvbs = data.GetChannelList(SignalSource.DvbS);
      var orf2 = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2 HD");
      Assert.AreEqual(127, orf2.OldProgramNr);
      Assert.AreEqual(127, orf2.NewProgramNr);
      Assert.IsFalse(orf2.IsDeleted);

      orf2.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);

      Assert.IsTrue(orf2.IsDeleted);
      Assert.AreEqual(0, orf2.NewProgramNr);

      // save and reload
      ser.Save();
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      dvbs = data.GetChannelList(SignalSource.DvbS);
      orf2 = dvbs.Channels.FirstOrDefault(ch => ch.Name == "ORF2 HD");

      Assert.IsTrue(orf2.IsDeleted);
      Assert.AreEqual(-1, orf2.OldProgramNr);
      Assert.AreEqual(-1, orf2.NewProgramNr);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var inFile = TestUtils.DeploymentItem("Test.Loader.LG\\Binary\\LM\\xxLM620S-ZE00001.TLL.in");
      var tllFile = inFile.Replace(".in", ".tll");
      File.Delete(tllFile);
      File.Move(inFile, tllFile);
      RoundtripTest.TestChannelAndFavListEditing(tllFile, new LgPlugin() {IsTesting = true});
    }
    #endregion

  }
}
