using System.IO;
using System.Linq;
using ChanSort;
using ChanSort.Api;
using ChanSort.Loader.DvbViewer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Loader;

namespace Test.Loader.DvbViewer
{
  [TestClass]
  public class DvbViewerTest
  {
    private const string TestFile = "Test.Loader.DvbViewer\\TestFiles\\emitel.ini";

    #region TestLoadingChannels
    [TestMethod]
    public void TestLoadingChannels()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();

      // 100 entries in the file, 40 of them are additional audio tracks which are attached to their main channel
      var list = data.ChannelLists.Single();
      Assert.AreEqual(60, list.Count);
      Assert.AreEqual(60, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));

      var tvp1 = list.Channels.Single(ch => ch.Name == "TVP1 (pol)");
      Assert.AreEqual(SignalSource.DvbT | SignalSource.Tv, tvp1.SignalSource);
      Assert.AreEqual("DVB-T", tvp1.Source);
      Assert.AreEqual(490m, tvp1.FreqInMhz);
      Assert.AreEqual(8808, tvp1.OriginalNetworkId);
      Assert.AreEqual(3, tvp1.TransportStreamId);
      Assert.AreEqual(1, tvp1.ServiceId);
      Assert.AreEqual(102, tvp1.VideoPid);
      Assert.AreEqual(103, tvp1.AudioPid);
      Assert.AreEqual(false, tvp1.Encrypted);
      Assert.AreEqual("Emitel", tvp1.Provider);
      Assert.AreEqual("+TVP1 (qaa) +TVP1 (aux)", tvp1.Debug);

      Assert.AreEqual(1, list.Channels.Single(ch => ch.Name == "Alfa TVP (pol)").OldProgramNr);
      Assert.AreEqual(2, list.Channels.Single(ch => ch.Name == "ANTENA HD").OldProgramNr);
      Assert.IsNull(list.Channels.FirstOrDefault(ch => ch.Name == "TVP1 (aux)"));
    }
    #endregion

    #region TestSavingUnchangedListKeepsFileIdentical
    [TestMethod]
    public void TestSavingUnchangedListKeepsFileIdentical()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var original = File.ReadAllBytes(tempFile);

      var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
      ser.Load();
      ser.DataRoot.ValidateAfterLoad();
      ser.DataRoot.ApplyCurrentProgramNumbers();
      ser.Save();

      CollectionAssert.AreEqual(original, File.ReadAllBytes(tempFile));
    }
    #endregion

    #region TestReorderAndDeleteMovesAudioTracks
    [TestMethod]
    public void TestReorderAndDeleteMovesAudioTracks()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var plugin = new DvbViewerPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var list = data.ChannelLists.Single();
      var editor = new Editor { DataRoot = data, ChannelList = list };

      // move TVP1 (with its 2 additional audio tracks) to the top and delete "Alfa TVP (pol)" (with its audio track)
      var tvp1 = list.Channels.Single(ch => ch.Name == "TVP1 (pol)");
      editor.SetSlotNumber(new[] { tvp1 }, 1, false, false);
      var alfa = list.Channels.Single(ch => ch.Name == "Alfa TVP (pol)");
      alfa.NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);
      tvp1.Name = "TVP 1";

      ser.Save();

      var lines = File.ReadAllLines(tempFile);
      Assert.AreEqual("[Channel0]", lines[0]);
      Assert.AreEqual(98 * 26, lines.Length); // 98 sections with 24 values + header + blank line
      var names = lines.Where(l => l.StartsWith("Name=")).Select(l => l.Substring(5)).ToList();
      Assert.AreEqual(98, names.Count);
      CollectionAssert.AreEqual(new[] { "TVP 1", "TVP1 (qaa)", "TVP1 (aux)", "ANTENA HD", "Belsat TV (bel)" }, names.Take(5).ToList());
      Assert.IsFalse(names.Contains("Alfa TVP (pol)"));
      Assert.IsFalse(names.Contains("Alfa TVP (aux)"));
      Assert.AreEqual("[Channel97]", lines[97 * 26]);

      // reload
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      list = data.ChannelLists.Single();
      Assert.AreEqual(59, list.Count);
      Assert.AreEqual(1, list.Channels.Single(ch => ch.Name == "TVP 1").OldProgramNr);
      Assert.AreEqual(2, list.Channels.Single(ch => ch.Name == "ANTENA HD").OldProgramNr);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new DvbViewerPlugin());
    }
    #endregion

    #region TestOtherIniFileIsRejected
    [TestMethod]
    public void TestOtherIniFileIsRejected()
    {
      var tempFile = Path.Combine(Path.GetTempPath(), "ChanSort_DvbViewerTest.ini");
      File.WriteAllText(tempFile, "[Settings]\r\nFoo=Bar\r\n");
      try
      {
        var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
        var ex = Assert.ThrowsExactly<LoaderException>(() => ser.Load());
        Assert.AreEqual(LoaderException.RecoveryMode.TryNext, ex.Recovery);
      }
      finally
      {
        File.Delete(tempFile);
      }
    }
    #endregion
  }
}
