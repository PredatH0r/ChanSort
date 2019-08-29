using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.PhilipsXml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.PhilipsXml
{
  [TestClass]
  public class PhilipsXmlTest
  {
    private static readonly string filesDir;

    static PhilipsXmlTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestFormat1SatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestFormat1SatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("DVBS.xml", SignalSource.DvbS, 502, 350, 152);
    }
    #endregion


    #region TestFormat1CableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestFormat1CableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("DVBC.xml", SignalSource.DvbC, 459, 358, 101);
    }
    #endregion

    #region TestFormat2CableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestFormat2CableChannelsAddedToCorrectLists()
    {
      // this file format doesn't provide any information whether a channel is TV/radio/data or analog/digital. It only contains the "medium" for antenna/cable/sat
      this.TestChannelsAddedToCorrectLists("CM_TPM1013E_LA_CK.xml", SignalSource.DvbC, 483, 0, 0);
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

      // no data channels found in any of the Philips channel lists available to me
    }
    #endregion

  }
}
