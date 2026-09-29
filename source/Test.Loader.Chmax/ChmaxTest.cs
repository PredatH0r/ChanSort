using System.IO;
using System.Linq;
using System.Text;
using ChanSort;
using ChanSort.Api;
using ChanSort.Loader.Chmax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Loader;

namespace Test.Loader.Chmax
{
  [TestClass]
  public class ChmaxTest
  {
    private const string TestFile = "Test.Loader.Chmax\\TestFiles\\musa.chl";

    #region TestLoadingChannels
    [TestMethod]
    public void TestLoadingChannels()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var ser = new ChmaxPlugin().CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();

      var tv = data.ChannelLists.Single(l => l.ShortCaption == "TV");
      var radio = data.ChannelLists.Single(l => l.ShortCaption == "Radio");
      Assert.AreEqual(60, tv.Count);
      Assert.AreEqual(20, radio.Count);

      var nova = tv.Channels.Single(ch => ch.Name == "Nova HR HD");
      Assert.AreEqual(1, nova.OldProgramNr);
      Assert.AreEqual(SignalSource.DvbS | SignalSource.Tv, nova.SignalSource);
      Assert.AreEqual(1662, nova.ServiceId);
      Assert.AreEqual(262, nova.VideoPid);
      Assert.AreEqual(3621, nova.AudioPid);
      Assert.AreEqual("Total TV", nova.Provider);
      Assert.AreEqual(11387m, nova.FreqInMhz);
      Assert.AreEqual(30000, nova.SymbolRate);
      Assert.AreEqual('H', nova.Polarity);
      Assert.AreEqual("Eutelsat 16A", nova.Satellite);
      Assert.AreEqual("16.0E", nova.SatPosition);
      Assert.AreEqual(true, nova.Encrypted);

      Assert.AreEqual(2, tv.Channels.Single(ch => ch.Name == "DMAX Italy HD").OldProgramNr);
      Assert.AreEqual(1, radio.Channels.Single(ch => ch.Name == "Dubai Radio").OldProgramNr);
    }
    #endregion

    #region TestSavingUnchangedListKeepsFileIdentical
    [TestMethod]
    public void TestSavingUnchangedListKeepsFileIdentical()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var original = File.ReadAllBytes(tempFile);

      var ser = new ChmaxPlugin().CreateSerializer(tempFile);
      ser.Load();
      ser.DataRoot.ValidateAfterLoad();
      ser.DataRoot.ApplyCurrentProgramNumbers();
      ser.Save();

      CollectionAssert.AreEqual(original, File.ReadAllBytes(tempFile));
    }
    #endregion

    #region TestReorderDeleteRenameAndFavorites
    [TestMethod]
    public void TestReorderDeleteRenameAndFavorites()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);

      // the sample file has only empty favorite lists, so put some channel indices into the first one
      var text = File.ReadAllText(tempFile);
      text = text.Replace("\"Name\" : \"News\",\n  \"TVChs\" : [ ]", "\"Name\" : \"News\",\n  \"TVChs\" : [ 2, 0, 1 ]");
      File.WriteAllText(tempFile, text, new UTF8Encoding(false));

      var plugin = new ChmaxPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var tv = data.ChannelLists.Single(l => l.ShortCaption == "TV");
      var editor = new Editor { DataRoot = data, ChannelList = tv };

      // move "MRT 3" (Index 2) to the top, delete "Nova HR HD" (Index 0), rename and lock "DMAX Italy HD" (Index 1)
      var mrt = tv.Channels.Single(ch => ch.Name == "MRT 3");
      editor.SetSlotNumber(new[] { mrt }, 1, false, false);
      tv.Channels.Single(ch => ch.Name == "Nova HR HD").NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);
      var dmax = tv.Channels.Single(ch => ch.Name == "DMAX Italy HD");
      dmax.Name = "DMAX \"IT\"";
      dmax.Lock = true;

      ser.Save();

      text = File.ReadAllText(tempFile);
      StringAssert.Contains(text, "\"ChTV\" : 59,");
      StringAssert.Contains(text, "\"CHRadio\" : 20,");
      StringAssert.Contains(text, "\"TVChs\" : [ 0, 1 ]"); // MRT 3 is now 0, DMAX is 1, Nova was deleted
      StringAssert.Contains(text, "\"Name\" : \"DMAX \\\"IT\\\"\",");
      Assert.IsFalse(text.Contains("Nova HR HD"));

      // reload
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      tv = data.ChannelLists.Single(l => l.ShortCaption == "TV");
      Assert.AreEqual(59, tv.Count);
      Assert.AreEqual(1, tv.Channels.Single(ch => ch.Name == "MRT 3").OldProgramNr);
      dmax = tv.Channels.Single(ch => ch.Name == "DMAX \"IT\"");
      Assert.AreEqual(2, dmax.OldProgramNr);
      Assert.IsTrue(dmax.Lock);
      Assert.AreEqual(20, data.ChannelLists.Single(l => l.ShortCaption == "Radio").Count);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new ChmaxPlugin());
    }
    #endregion

    #region TestSamToolBoxReferenceListIsRejected
    [TestMethod]
    public void TestSamToolBoxReferenceListIsRejected()
    {
      var tempFile = Path.Combine(Path.GetTempPath(), "ChanSort_ChmaxTest.chl");
      File.WriteAllText(tempFile, "1;Das Erste HD\r\n2;ZDF HD\r\n");
      try
      {
        var ser = new ChmaxPlugin().CreateSerializer(tempFile);
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
