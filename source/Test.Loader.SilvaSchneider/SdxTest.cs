using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.SilvaSchneider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.SilvaSchneider
{
  [TestClass]
  public class SdxTest
  {
    private static readonly string filesDir;

    static SdxTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("silva_schneider.sdx", SignalSource.DvbS, 1108, 948, 160);
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
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

      // check that data channel is in the TV list
      if (dataProgramSid != 0)
      {
        var chan = list.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
        Assert.IsNotNull(chan);
        Assert.AreEqual(dataProgramName, chan.Name);
      }
    }
    #endregion
  }
}
