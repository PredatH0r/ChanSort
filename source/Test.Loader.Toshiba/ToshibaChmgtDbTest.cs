using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.Toshiba;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Toshiba
{
  [TestClass]
  public class ToshibaChmgtDbTest
  {
    private static readonly string filesDir;

    static ToshibaChmgtDbTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestSatChannelsAddedToCorrectLists

    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Toshiba-SL863G.zip", SignalSource.DvbS, 338, 172);
    }

    #endregion

    #region TestAnalogChannelsAddedToCorrectLists

    [TestMethod]
    public void TestAnalogChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Toshiba-SL863G.zip", SignalSource.AnalogCT, 1, 0);
    }

    #endregion


    #region TestChannelsAddedToCorrectList

    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var plugin = new DbSerializerPlugin();
      var ser = plugin.CreateSerializer(filesDir + fileName);
      ser.Load();

      var root = ser.DataRoot;

      var tv = root.GetChannelList(signalSource | SignalSource.Tv);
      if (expectedTv > 0)
      {
        Assert.IsNotNull(tv);
        Assert.AreEqual(expectedTv, tv?.Channels.Count ?? 0);
      }

      if (expectedRadio > 0)
      {
        var radio = root.GetChannelList(signalSource | SignalSource.Radio);
        Assert.IsNotNull(radio);
        Assert.AreEqual(expectedRadio, radio.Channels.Count);
      }

      // check that data channel is in the TV list
      if (dataProgramSid != 0)
      {
        var chan = tv.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
        Assert.IsNotNull(chan);
        Assert.AreEqual(dataProgramName, chan.Name);
      }
    }

    #endregion
  }
}
