using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.M3u;

namespace Test.Loader.M3u
{
  [TestClass]
  public class M3uTest
  {
    #region TestReading()
    [TestMethod]
    public void TestReading()
    {
      var m3uFile = TestUtils.DeploymentItem("Test.Loader.M3u\\TestFiles\\example.m3u");
      var refFile = TestUtils.DeploymentItem("Test.Loader.M3u\\TestFiles\\example-ref.txt");

      var loader = new M3uPlugin();
      var ser = loader.CreateSerializer(m3uFile);
      ser.Load();
      Assert.IsNotNull(ser);
      var root = ser.DataRoot;
      Assert.IsNotNull(root);

      root.ApplyCurrentProgramNumbers();
      var lists = root.ChannelLists.ToList();
      Assert.AreEqual(1, lists.Count);
      var chans = lists[0].Channels;
      Assert.AreEqual(6, chans.Count);
      Assert.AreEqual("Russia Today", chans[0].Name);
      Assert.AreEqual(1, chans[0].NewProgramNr);
      Assert.AreEqual("MP4", chans[5].Name);
      Assert.AreEqual(6, chans[5].NewProgramNr);


      var refLoader = new RefSerializerPlugin();
      var refSer = refLoader.CreateSerializer(refFile);
      refSer.Load();
      var ed = new Editor();
      ed.DataRoot = ser.DataRoot;
      ed.ChannelList = lists[0];

      // reset the order so we can apply a reference list
      foreach (var chan in lists[0].Channels)
        chan.NewProgramNr = -1;

      //ed.ApplyReferenceList(refSer.DataRoot);
      ed.ApplyReferenceList(refSer.DataRoot, refSer.DataRoot.ChannelLists.First(), 0, lists[0], 0,false, 0, null, true, false);

      Assert.AreEqual(1, chans[5].NewProgramNr);
      Assert.AreEqual(2, chans[4].NewProgramNr);
    }
    #endregion

    #region TestSavingKeepsExtinfTags()
    [TestMethod]
    public void TestSavingKeepsExtinfTags()
    {
      var m3uFile = TestUtils.DeploymentItem("Test.Loader.M3u\\TestFiles\\extinftags.m3u");

      var orig = File.ReadAllText(m3uFile);

      var loader = new M3uPlugin();
      var ser = loader.CreateSerializer(m3uFile);
      ser.Load();
      ser.Save();
      
      var text = File.ReadAllText(m3uFile);

      orig = orig.Replace("\r", "").TrimEnd();
      text = text.Replace("\r", "").TrimEnd();
      NUnit.Framework.Assert.AreEqual(orig, text);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.M3u\\TestFiles\\example.m3u");
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new M3uPlugin());
    }
    #endregion
  }
}
