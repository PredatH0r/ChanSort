using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.SamsungJ;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.SamsungJ
{
  [TestClass]
  public class SamsungZipTest
  {
    private static readonly string filesDir;

    static SamsungZipTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestSatChannelsAddedToCorrectLists
    [TestMethod]
    public void TestSatChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Channel_list_T-KTSUDEUC-1007.1.zip", SignalSource.DvbS, 1323, 878, 380, 4008, "Humax ESD 160C");
    }
    #endregion

    #region TestCableChannelsAddedToCorrectLists
    [TestMethod]
    public void TestCableChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Channel_list_T-KTMDEUC-1132.6.zip", SignalSource.DvbC, 146, 65, 75, 4008, "Humax 160C");
    }
    #endregion

    #region TestAntennaChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAntennaChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("Channel_list_T-KTSUDEUC-1007.1.zip", SignalSource.DvbT, 77, 71, 4, 3995, "Irdeto Code 4");
    }
    #endregion


    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, SignalSource signalSource, int expectedTotal, int expectedTv, int expectedRadio, int dataProgramSid = 0, string dataProgramName = null)
    {
      var plugin = new DbSerializerPlugin();
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
