using System.IO;
using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Philips;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Philips
{
  [TestClass]
  public class PhilipsBinS2channellibTest
  {
    [TestMethod]
    public void TestFiles1()
    {
      var baseDir = Path.GetDirectoryName(this.GetType().Assembly.Location);
      var baseFile = Path.Combine(baseDir, "TestFiles1\\Repair\\ChannelList\\chanLst.bin");
      var plugin = new ChanSort.Loader.Philips.PhilipsPlugin();
      var loader = plugin.CreateSerializer(baseFile);
      loader.Load();

      var list = loader.DataRoot.GetChannelList(SignalSource.DvbS);
      Assert.AreEqual(5000, list.Channels.Count);
      Assert.AreEqual(4975, list.Channels.Count(ch => !ch.IsDeleted));

      var ch0 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 0);
      Assert.IsTrue(ch0.IsDeleted);

      var ch1 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 1);
      Assert.AreEqual(2, ch1.OldProgramNr);
      Assert.AreEqual("ZDF HD", ch1.Name);
      Assert.AreEqual(11361, ch1.FreqInMhz);
      Assert.AreEqual("Astra 1", ch1.Satellite);
      Assert.AreEqual(1, ch1.OriginalNetworkId);
      Assert.AreEqual(1011, ch1.TransportStreamId);
      Assert.AreEqual(11110, ch1.ServiceId);
      Assert.AreEqual(6110, ch1.PcrPid);
      Assert.AreEqual(6110, ch1.VideoPid);
      Assert.AreEqual(21999, ch1.SymbolRate);
      Assert.AreEqual("ZDFvision", ch1.Provider);
    }

    [TestMethod]
    public void TestFiles2()
    {
      var baseDir = Path.GetDirectoryName(this.GetType().Assembly.Location);
      var baseFile = Path.Combine(baseDir, "TestFiles2\\Repair\\ChannelList\\chanLst.bin");
      var plugin = new ChanSort.Loader.Philips.PhilipsPlugin();
      var loader = plugin.CreateSerializer(baseFile);
      loader.Load();

      var list = loader.DataRoot.GetChannelList(SignalSource.DvbS);
      Assert.AreEqual(5000, list.Channels.Count);
      Assert.AreEqual(1326, list.Channels.Count(ch => !ch.IsDeleted));

      var ch0 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 0);
      Assert.AreEqual(1, ch0.OldProgramNr);
      Assert.AreEqual("Das Erste HD", ch0.Name);
      Assert.AreEqual(11493, ch0.FreqInMhz);
      Assert.AreEqual("Astra 1", ch0.Satellite);
      //Assert.AreEqual(1, ch0.OriginalNetworkId);
      Assert.AreEqual(1019, ch0.TransportStreamId);
      Assert.AreEqual(10301, ch0.ServiceId);
      //Assert.AreEqual(6110, ch1.PcrPid);
      //Assert.AreEqual(6110, ch1.VideoPid);
      Assert.AreEqual(21999, ch0.SymbolRate);
      Assert.AreEqual("ARD", ch0.Provider);
      Assert.IsFalse(ch0.Lock);
      Assert.AreEqual((Favorites)0, ch0.Favorites);
      Assert.IsFalse(ch0.IsDeleted);

      var ch2 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 2);
      Assert.AreEqual("NDR FS HH", ch2.Name);
      Assert.IsTrue(ch2.Lock);
      Assert.AreEqual((Favorites)0, ch2.Favorites);

      var ch3 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 3);
      Assert.AreEqual("SAT.1", ch3.Name);
      Assert.AreEqual(Favorites.A, ch3.Favorites);

      var ch4 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 4);
      Assert.AreEqual("arte HD", ch4.Name);
      Assert.AreEqual(Favorites.A, ch4.Favorites);

      var ch7 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 7);
      Assert.AreEqual("RTL2", ch7.Name);
      Assert.AreEqual(Favorites.A, ch7.Favorites);

      var ch8 = list.Channels.FirstOrDefault(ch => ch.RecordIndex == 8);
      Assert.IsTrue(ch8.IsDeleted);

      Assert.AreEqual(1, ch4.GetOldPosition(1));
      Assert.AreEqual(2, ch7.GetOldPosition(1));
      Assert.AreEqual(3, ch3.GetOldPosition(1));
    }

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles2\\Repair\\ChannelList") + "\\chanLst.bin";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new PhilipsPlugin());
    }
    #endregion

  }
}
