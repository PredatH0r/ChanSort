using System.Collections.Generic;
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
    private const string LcnTestFile = "Test.Loader.DvbViewer\\TestFiles\\lublin_lcn.ini";

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

    #region TestSavingUnchangedListOnlyAddsLcn
    [TestMethod]
    public void TestSavingUnchangedListOnlyAddsLcn()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var original = File.ReadAllLines(tempFile);

      var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
      ser.Load();
      ser.DataRoot.ValidateAfterLoad();
      ser.DataRoot.ApplyCurrentProgramNumbers();
      ser.Save();

      // without LCN, DVBViewer doesn't assign channel numbers, so an LCN line is added after each Name line
      var lines = File.ReadAllLines(tempFile);
      CollectionAssert.AreEqual(original, lines.Where(l => !l.StartsWith("LCN=")).ToList());
      Assert.AreEqual(100, lines.Count(l => l.StartsWith("LCN=")));
      for (int i = 0; i < lines.Length; i++)
      {
        if (lines[i].StartsWith("LCN="))
          Assert.IsTrue(lines[i - 1].StartsWith("Name="));
      }

      // audio tracks get the number of their main channel
      var sections = ReadSections(lines);
      CollectionAssert.AreEqual(new[] { "1", "1", "2", "3", "3", "4" }, sections.Take(6).Select(s => s["LCN"]).ToList());
      Assert.AreEqual("60", sections.Last()["LCN"]);
    }
    #endregion

    #region TestLoadingChannelsWithLcn
    [TestMethod]
    public void TestLoadingChannelsWithLcn()
    {
      var tempFile = TestUtils.DeploymentItem(LcnTestFile);
      var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();

      // 94 entries in the file, 39 of them are additional audio tracks
      var list = data.ChannelLists.Single();
      Assert.AreEqual(55, list.Count);
      Assert.AreEqual(0, list.DuplicateProgNrCount);
      Assert.AreEqual("", data.Warnings.ToString());

      // LCN is used as channel number, including gaps and channels which are not sorted by LCN in the file
      Assert.AreEqual(1, list.Channels.Single(ch => ch.Name == "TVP1 (pol)").OldProgramNr);
      Assert.AreEqual(12, list.Channels.Single(ch => ch.Name == "ESKA TV (pol)").OldProgramNr);
      Assert.AreEqual(34, list.Channels.Single(ch => ch.Name == "TVP Info").OldProgramNr);
      Assert.AreEqual(215, list.Channels.Single(ch => ch.Name == "test 215").OldProgramNr);
      Assert.AreEqual(118, list.Channels.Single(ch => ch.Name == "Polsat News Polityka").OldProgramNr);
      Assert.AreEqual(333, list.Channels.Single(ch => ch.Name == "Polsat  HbbTV").OldProgramNr);
      Assert.AreEqual("+TVP1 (qaa) +TVP1 (aux) +TVP1 (MUL)", list.Channels.Single(ch => ch.Name == "TVP1 (pol)").Debug);
    }
    #endregion

    #region TestSavingUnchangedListWithLcn
    [TestMethod]
    public void TestSavingUnchangedListWithLcn()
    {
      var tempFile = TestUtils.DeploymentItem(LcnTestFile);
      var original = ReadSections(File.ReadAllLines(tempFile));

      var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
      ser.Load();
      ser.DataRoot.ValidateAfterLoad();
      ser.DataRoot.ApplyCurrentProgramNumbers();
      ser.Save();

      // all sections are kept unchanged, only sorted by LCN ("Polsat News Polityka" with LCN 118 was stored after LCN 215)
      var lines = File.ReadAllLines(tempFile);
      var saved = ReadSections(lines);
      var expected = original.OrderBy(s => int.Parse(s["LCN"])).ToList();
      Assert.AreEqual(expected.Count, saved.Count);
      for (int i = 0; i < saved.Count; i++)
        CollectionAssert.AreEqual(expected[i].ToList(), saved[i].ToList());
      Assert.AreEqual(94 * 27, lines.Length);
    }
    #endregion

    #region TestReorderChannelsWithLcn
    [TestMethod]
    public void TestReorderChannelsWithLcn()
    {
      var tempFile = TestUtils.DeploymentItem(LcnTestFile);
      var plugin = new DvbViewerPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var list = data.ChannelLists.Single();
      var editor = new Editor { DataRoot = data, ChannelList = list };

      // swap "TVP Info" (LCN 34) with "TVP1 (pol)" (LCN 1, with 3 additional audio tracks)
      var tvpInfo = list.Channels.Single(ch => ch.Name == "TVP Info");
      editor.SetSlotNumber(new[] { tvpInfo }, 1, true, false);
      ser.Save();

      var sections = ReadSections(File.ReadAllLines(tempFile));
      Assert.AreEqual(94, sections.Count);
      Assert.AreEqual("TVP Info", sections[0]["Name"]);
      Assert.AreEqual("1", sections[0]["LCN"]);
      var tvp1 = sections.Where(s => s["SID"] == "1").ToList();
      CollectionAssert.AreEqual(new[] { "TVP1 (pol)", "TVP1 (qaa)", "TVP1 (aux)", "TVP1 (MUL)" }, tvp1.Select(s => s["Name"]).ToList());
      CollectionAssert.AreEqual(new[] { "34", "34", "34", "34" }, tvp1.Select(s => s["LCN"]).ToList());

      // reload
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      list = ser.DataRoot.ChannelLists.Single();
      Assert.AreEqual(55, list.Count);
      Assert.AreEqual(1, list.Channels.Single(ch => ch.Name == "TVP Info").OldProgramNr);
      Assert.AreEqual(34, list.Channels.Single(ch => ch.Name == "TVP1 (pol)").OldProgramNr);
      Assert.AreEqual(2, list.Channels.Single(ch => ch.Name == "TVP2 (pol)").OldProgramNr);
    }
    #endregion

    #region TestChannelsWithoutLcnAreAppended
    [TestMethod]
    public void TestChannelsWithoutLcnAreAppended()
    {
      // remove the LCN of "TVP Info" (LCN 34) and "Republika" (LCN 51)
      var tempFile = TestUtils.DeploymentItem(LcnTestFile);
      var lines = File.ReadAllLines(tempFile).ToList();
      foreach (var name in new[] { "TVP Info", "Republika" })
        lines.RemoveAt(lines.IndexOf("Name=" + name) + 1);
      File.WriteAllLines(tempFile, lines);

      var ser = new DvbViewerPlugin().CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      // channels without LCN are numbered in file order after the highest LCN (333)
      var list = data.ChannelLists.Single();
      Assert.AreEqual(334, list.Channels.Single(ch => ch.Name == "TVP Info").OldProgramNr);
      Assert.AreEqual(335, list.Channels.Single(ch => ch.Name == "Republika").OldProgramNr);

      ser.Save();
      var sections = ReadSections(File.ReadAllLines(tempFile));
      CollectionAssert.AreEqual(new[] { "TVP Info", "Republika" }, sections.Skip(92).Select(s => s["Name"]).ToList());
      CollectionAssert.AreEqual(new[] { "334", "335" }, sections.Skip(92).Select(s => s["LCN"]).ToList());
    }
    #endregion

    #region TestChannelAndFavListEditingWithLcn
    [TestMethod]
    public void TestChannelAndFavListEditingWithLcn()
    {
      var tempFile = TestUtils.DeploymentItem(LcnTestFile);
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new DvbViewerPlugin());
    }
    #endregion

    #region ReadSections()
    /// <summary>
    /// returns the key=value pairs of each [ChannelN] section in file order
    /// </summary>
    private static List<Dictionary<string, string>> ReadSections(string[] lines)
    {
      var sections = new List<Dictionary<string, string>>();
      foreach (var line in lines)
      {
        if (line.StartsWith("[Channel"))
          sections.Add(new Dictionary<string, string>());
        else if (line.Contains("="))
          sections[sections.Count - 1][line.Substring(0, line.IndexOf('='))] = line.Substring(line.IndexOf('=') + 1);
      }
      return sections;
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
      Assert.AreEqual(98 * 27, lines.Length); // 98 sections with 24 values + added LCN + header + blank line
      var names = lines.Where(l => l.StartsWith("Name=")).Select(l => l.Substring(5)).ToList();
      Assert.AreEqual(98, names.Count);
      CollectionAssert.AreEqual(new[] { "TVP 1", "TVP1 (qaa)", "TVP1 (aux)", "ANTENA HD", "Belsat TV (bel)" }, names.Take(5).ToList());
      Assert.IsFalse(names.Contains("Alfa TVP (pol)"));
      Assert.IsFalse(names.Contains("Alfa TVP (aux)"));
      Assert.AreEqual("[Channel97]", lines[97 * 27]);
      var lcns = lines.Where(l => l.StartsWith("LCN=")).Select(l => l.Substring(4)).ToList();
      CollectionAssert.AreEqual(new[] { "1", "1", "1", "3", "4" }, lcns.Take(5).ToList()); // 2 was "Alfa TVP (pol)"

      // reload
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      list = data.ChannelLists.Single();
      Assert.AreEqual(59, list.Count);
      Assert.AreEqual(1, list.Channels.Single(ch => ch.Name == "TVP 1").OldProgramNr);
      Assert.AreEqual(3, list.Channels.Single(ch => ch.Name == "ANTENA HD").OldProgramNr);
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
