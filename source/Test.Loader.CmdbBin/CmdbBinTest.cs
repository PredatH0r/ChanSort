using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.CmdbBin;

namespace Test.Loader.CmdbBin
{
  [TestClass]
  public class CmdbBinTest
  {
    #region TestGrundigAnalogCable()
    [TestMethod]
    public void TestGrundigAnalogCable()
    {
      // load existing file and make assertions
      var path = TestUtils.DeploymentItem(@"Test.Loader.CmdbBin\TestFiles\GrundigAnalogCable\atv_cmdb_cable.bin");
      var ser = new CmdbFileSerializer(path);
      ser.Load();
      ser.DataRoot.ApplyCurrentProgramNumbers();

      var list = ser.DataRoot.GetChannelList(SignalSource.AnalogC);
      Assert.IsNotNull(list);
      Assert.AreEqual(SignalSource.AnalogC|SignalSource.Tv, list.SignalSource);
      Assert.AreEqual(20, list.Channels.Count);
      Assert.AreEqual("HR", list.Channels[0].Name);
      Assert.AreEqual("S-12", list.Channels.FirstOrDefault(ch => ch.OldProgramNr == 1)?.Name);

      // modify and save file
      var ard = list.Channels[7];
      Assert.AreEqual("ARD", ard.Name);
      var zdf = list.Channels[1];
      zdf.Name = "ZDF";
      zdf.IsNameModified = true;

      ser.DataRoot.ApplyCurrentProgramNumbers();

      var ed = new Editor();
      ed.DataRoot = ser.DataRoot;
      ed.ChannelList = list;
      ed.SetSlotNumber(new[] { ard }, 1, false, true );
      ser.Save();

      // load modified file again and verify changes
      ser = new CmdbFileSerializer(path);
      ser.Load();
      list = ser.DataRoot.GetChannelList(SignalSource.AnalogC);
      Assert.IsNotNull(list);
      Assert.AreEqual(SignalSource.AnalogC | SignalSource.Tv, list.SignalSource);
      Assert.AreEqual(20, list.Channels.Count);
      Assert.AreEqual("HR", list.Channels[0].Name);
      Assert.AreEqual("ZDF", list.Channels[1].Name);
      Assert.AreEqual("ARD", list.Channels.FirstOrDefault(ch => ch.OldProgramNr == 1)?.Name);
      Assert.AreEqual("ZDF", list.Channels.FirstOrDefault(ch => ch.OldProgramNr == 2)?.Name);
    }
    #endregion

    #region TestChangHongDvbS()
    [TestMethod]
    public void TestChangHongDvbS()
    {
      // load existing file and make assertions
      TestUtils.DeploymentItem(@"Test.Loader.CmdbBin\TestFiles\ChangHongDvbS\atv_cmdb_cable.bin");
      var path = TestUtils.DeploymentItem(@"Test.Loader.CmdbBin\TestFiles\ChangHongDvbS\dtv_cmdb_2.bin");
      var ser = new CmdbFileSerializer(path);
      ser.Load();
      ser.DataRoot.ApplyCurrentProgramNumbers();

      // TODO implement the rest down here

      //var list = ser.DataRoot.GetChannelList(SignalSource.AnalogC);
      //Assert.IsNotNull(list);
      //Assert.AreEqual(SignalSource.AnalogC | SignalSource.Tv, list.SignalSource);
      //Assert.AreEqual(20, list.Channels.Count);
      //Assert.AreEqual("HR", list.Channels[0].Name);
      //Assert.AreEqual("S-12", list.Channels.FirstOrDefault(ch => ch.OldProgramNr == 1)?.Name);

      //// modify and save file
      //var ard = list.Channels[7];
      //Assert.AreEqual("ARD", ard.Name);
      //var zdf = list.Channels[1];
      //zdf.Name = "ZDF";
      //zdf.IsNameModified = true;

      //var ed = new Editor();
      //ed.DataRoot = ser.DataRoot;
      //ed.ChannelList = list;
      //ed.SetSlotNumber(new[] { ard }, 1, false, true);
      //ser.Save(null);

      //// load modified file again and verify changes
      //ser = new CmdbFileSerializer(path);
      //ser.Load();
      //list = ser.DataRoot.GetChannelList(SignalSource.AnalogC);
      //Assert.IsNotNull(list);
      //Assert.AreEqual(SignalSource.AnalogC | SignalSource.Tv, list.SignalSource);
      //Assert.AreEqual(20, list.Channels.Count);
      //Assert.AreEqual("HR", list.Channels[0].Name);
      //Assert.AreEqual("ZDF", list.Channels[1].Name);
      //Assert.AreEqual("ARD", list.Channels.FirstOrDefault(ch => ch.OldProgramNr == 1)?.Name);
      //Assert.AreEqual("ZDF", list.Channels.FirstOrDefault(ch => ch.OldProgramNr == 2)?.Name);
    }
    #endregion

  }
}
