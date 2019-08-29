using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.Hisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Hisense
{
  [TestClass]
  public class HisenseChannelDbTest
  {
    private static readonly string filesDir;

    static HisenseChannelDbTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("channel.db", SignalSource.DvbS, 1278, 1118, 160, 8692, "DOWNLOAD G10 HUMAX");
    }
    #endregion


    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("channel.db", SignalSource.DvbC, 381, 310, 71, 4008, "Humax ESD 160C");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid, string dataProgramName)
    {
      var plugin = new HisDbSerializerPlugin();
      var ser = plugin.CreateSerializer(filesDir + fileName);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.GetChannelList(signalSource);
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // check that data channel is in the TV list
      var chan = list.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
      Assert.IsNotNull(chan);
      Assert.AreEqual(dataProgramName, chan.Name);
    }
    #endregion


  }
}
