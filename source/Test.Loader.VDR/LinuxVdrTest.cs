using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ChanSort.Api;
using ChanSort.Loader.VDR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.VDR
{
  [TestClass]
  public class LinuxVdrTest
  {
    private static readonly string filesDir;

    static LinuxVdrTest()
    {
      filesDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TestFiles\\";
    }

    #region TestAstraChannelsAddedToCorrectLists
    [TestMethod]
    public void TestAstraChannelsAddedToCorrectLists()
    {
      this.TestChannelsAddedToCorrectLists("channels.conf", 3380, 2649, 492);
    }
    #endregion

    #region TestChannelsAddedToCorrectList
    private void TestChannelsAddedToCorrectLists(string fileName, int expectedTotal, int expectedTv, int expectedRadio)
    {
      var plugin = new SerializerPlugin();
      var ser = plugin.CreateSerializer(filesDir + fileName);
      ser.Load();

      var root = ser.DataRoot;

      var list = root.ChannelLists.FirstOrDefault();
      Assert.IsNotNull(list);
      Assert.AreEqual(expectedTotal, list.Channels.Count);
      Assert.AreEqual(expectedTv, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Tv) != 0));
      Assert.AreEqual(expectedRadio, list.Channels.Count(ch => (ch.SignalSource & SignalSource.Radio) != 0));

      // no data channels in channels.conf files
    }
    #endregion


  }
}
