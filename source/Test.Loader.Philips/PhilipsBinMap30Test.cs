using System;
using System.Linq;
using ChanSort.Api;
using ChanSort.Loader.Philips;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader.Philips
{
  [TestClass]
  public class PhilipsBinMap30Test
  {
    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\ChannelMap_30") + "\\ChannelList\\chanLst.bin";
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new PhilipsPlugin());
    }
    #endregion

    #region TestSavingUpdatesCableChannelMapsDb
    [TestMethod]
    public void TestSavingUpdatesCableChannelMapsDb()
    {
      // format 30 keeps the program numbers redundantly in CableDb.bin and in the AnalogTable/DigSrvTable of CableChannelMaps.db
      var dir = TestUtils.DeploymentItem("Test.Loader.Philips\\TestFiles\\ChannelMap_30") + "\\ChannelList";
      var plugin = new PhilipsPlugin();
      var ser = plugin.CreateSerializer(dir + "\\chanLst.bin");
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var list = data.GetChannelList(SignalSource.DvbC);
      var ch1 = list.Channels.First(ch => ch.OldProgramNr == 1);
      var ch2 = list.Channels.First(ch => ch.OldProgramNr == 2);
      ch1.NewProgramNr = 2;
      ch2.NewProgramNr = 1;
      ser.Save();

      using var conn = new SqliteConnection("Data Source=" + dir + "\\channellib\\CableChannelMaps.db;Pooling=False");
      conn.Open();
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "select PresetNumber from DigSrvTable where OriginalNetworkId=@onid and Tsid=@tsid and ServiceId=@sid";
      cmd.Parameters.Add("@onid", SqliteType.Integer);
      cmd.Parameters.Add("@tsid", SqliteType.Integer);
      cmd.Parameters.Add("@sid", SqliteType.Integer);
      foreach (var ch in new[] { ch1, ch2 })
      {
        cmd.Parameters["@onid"].Value = ch.OriginalNetworkId;
        cmd.Parameters["@tsid"].Value = ch.TransportStreamId;
        cmd.Parameters["@sid"].Value = ch.ServiceId;
        Assert.AreEqual(ch.NewProgramNr, Convert.ToInt32(cmd.ExecuteScalar()));
      }
    }
    #endregion
  }
}
