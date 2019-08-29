using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.Panasonic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Panasonic
{
  [TestClass]
  public class PanasonicSvlTest
  {
    private static readonly string filesDir;

    static PanasonicSvlTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("svl-sat.db", SignalSource.DvbS, 1187, 1028, 159);
    }
    #endregion

    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("svl-cable.db", SignalSource.DvbC, 465, 304, 161);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var plugin = new SerializerPlugin();
      var ser = plugin.CreateSerializer(filesDir + fileName);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.GetChannelList(signalSource);
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // there are no data channels in Panasonic lists
    }
    #endregion


  }
}
