using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.GlobalClone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.GlobalClone
{
  [TestClass]
  public class LgGlobalCloneTest
  {
    private static readonly string filesDir;

    static LgGlobalCloneTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("GlobalClone00001.TLL", SignalSource.DvbS, 1138, 160, 8692, "DOWNLOAD G10 HUMAX");
    }
    #endregion

    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("GlobalClone00002.TLL", SignalSource.DvbC, 405, 113, 11105, "ITV Content 01");
    }
    #endregion

    #region TestAntennaChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAntennaChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("GlobalClone00003.TLL", SignalSource.DvbT, 67, 6, 14120, "SRT8505 OTA");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTv, int expectedRadio, int dataProgramSid, string dataProgramName)
    {
      var plugin = new GcSerializerPlugin();
      var ser = plugin.CreateSerializer(filesDir + fileName);
      ser.Load();

      var root = ser.DataRoot;

      var tv = root.GetChannelList(signalSource | SignalSource.Tv);
      Assert.IsNotNull(tv);
      Assert.AreEqual(expectedTv, tv.Channels.Count);

      var radio = root.GetChannelList(signalSource | SignalSource.Radio);
      Assert.IsNotNull(radio);
      Assert.AreEqual(expectedRadio, radio.Channels.Count);

      // check that data channel is in the TV list
      var chan = tv.Channels.FirstOrDefault(ch => ch.ServiceId == dataProgramSid);
      Assert.IsNotNull(chan);
      Assert.AreEqual(dataProgramName, chan.Name);
    }
    #endregion
  }
}
