using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChanSort.Api;
using ChanSort.Loader.LG;

namespace Test.Loader
{
  [TestClass]
  public class LH
  {
    [TestMethod]
    [DeploymentItem("ChanSort.Loader.LG\\ChanSort.Loader.LG.ini")]
    [DeploymentItem("Test.Loader\\LG\\xxLH3000-ZA00002.TLL")]
    public void TestLoading()
    {
      string filePath = @"xxLH3000-ZA00002.TLL";
      TllFileSerializerPlugin plugin = new TllFileSerializerPlugin();
      var serializer = (TllFileSerializer) plugin.CreateSerializer(filePath);
      serializer.Load();

      var data = serializer.DataRoot;
      Assert.AreEqual(53, data.GetChannelList(SignalSource.DvbCT).Count);
    }
  }
}
