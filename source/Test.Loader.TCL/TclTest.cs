using System.Linq;
using ChanSort.Loader.TCL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.TCL
{
  [TestClass]
  public class TclTest
  {
    private const string SatFile = "Test.Loader.TCL\\TestFiles\\sat-astra28.tar";

    #region TestLoadingSatChannels
    [TestMethod]
    public void TestLoadingSatChannels()
    {
      var tempFile = TestUtils.DeploymentItem(SatFile);
      var ser = new TclPlugin().CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();

      var sat = data.ChannelLists.Single(l => l.ShortCaption == "DVB-S");
      Assert.AreEqual(869, sat.Count);

      var bbc2 = sat.Channels.Single(ch => ch.OldProgramNr == 12);
      Assert.AreEqual("BBC Two", bbc2.Name);
      Assert.AreEqual(6302, bbc2.ServiceId);
      Assert.AreEqual(10774m, bbc2.FreqInMhz);
      Assert.AreEqual("Astra 28.2E", bbc2.Satellite);
      Assert.AreEqual("28.2E", bbc2.SatPosition);
      Assert.AreEqual(10774m, bbc2.Transponder?.FrequencyInMhz);
    }
    #endregion
  }
}
