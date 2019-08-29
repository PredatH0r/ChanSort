using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.Hisense2017;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Hisense2017
{
  [TestClass]
  public class HisenseServicelistDbTest
  {
    private static readonly string filesDir;

    static HisenseServicelistDbTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestAstraChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAstraChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist.db", "ASTRA1 19.2°E", 1214, 1052, 162);
    }
    #endregion

    #region TestEutelsatChannelsAddedToCorrectLists
    [TestMethod]
    public void TesEutelsatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("servicelist.db", "Hot Bird 13°E", 1732, 1439, 293);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, string listCaption, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var plugin = new HisDbSerializerPlugin();
      var ser = plugin.CreateSerializer(filesDir + fileName);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.ChannelLists.FirstOrDefault(l => l.Caption.StartsWith(listCaption));
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // no data channels in Hisense/Loewe servicelist.db files
    }
    #endregion


  }
}
