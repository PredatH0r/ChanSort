using System.IO;
using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Philips;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Philips
{
  [TestClass]
  [DeploymentItem("ChanSort.Loader.Philips\\ChanSort.Loader.Philips.ini")]
  public class PhilipsBinChannellibTest
  {
    [TestMethod]
    public void TestFiles1()
    {
      var baseFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles1\\Repair\\ChannelList") + "\\chanLst.bin";
      var plugin = new ChanSort.Loader.Philips.PhilipsPlugin();
      var loader = plugin.CreateSerializer(baseFile);
      loader.Load();

      var list = loader.DataRoot.GetChannelList(SignalSource.DvbC);
      Assert.AreEqual(179, list.Channels.Count);
      Assert.AreEqual(179, list.Channels.Count(ch => !ch.IsDeleted));

      var ch0 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 0);
      Assert.AreEqual(41, ch0.OldProgramNr);
      Assert.AreEqual("Passion HD", ch0.Name);
      Assert.IsFalse(ch0.Lock);
      Assert.AreEqual((Favorites)0, ch0.Favorites);
      Assert.AreEqual(810, ch0.FreqInMhz);
      Assert.AreEqual(9999, ch0.OriginalNetworkId);
      Assert.AreEqual(461, ch0.TransportStreamId);
      Assert.AreEqual(46102, ch0.ServiceId);
      Assert.AreEqual(6900, ch0.SymbolRate);
    }

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles1\\Repair\\ChannelList") + "\\chanLst.bin";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new PhilipsPlugin(), true, 3);
    }
    #endregion

  }
}
