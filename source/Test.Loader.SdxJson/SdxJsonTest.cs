using System.IO;
using System.Linq;
using System.Text;
using ChanSort;
using ChanSort.Api;
using ChanSort.Loader.SatcoDX;
using ChanSort.Loader.SdxJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Test.Loader;

namespace Test.Loader.SdxJson
{
  [TestClass]
  public class SdxJsonTest
  {
    private const string TestFile = "Test.Loader.SdxJson\\TestFiles\\izybox.sdx";
    private const string HeaderStart = "{\n\t\"database_header_object\"";

    #region TestLoadingChannels
    [TestMethod]
    public void TestLoadingChannels()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var ser = new SdxJsonPlugin().CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();

      var tv = data.ChannelLists.Single(l => l.ShortCaption == "TV");
      var radio = data.ChannelLists.Single(l => l.ShortCaption == "Radio");
      Assert.AreEqual(19, tv.Count);
      Assert.AreEqual(8, radio.Count);
      Assert.AreEqual(27, tv.MaxChannelNameLength);
      Assert.AreEqual(SerializerBase.DeleteMode.Physically, ser.Features.DeleteMode);

      var laUne = tv.Channels.Single(ch => ch.Name == "La Une HD");
      Assert.AreEqual(1, laUne.OldProgramNr);
      Assert.AreEqual(SignalSource.DvbS | SignalSource.Tv, laUne.SignalSource);
      Assert.AreEqual(4823, laUne.ServiceId);
      Assert.AreEqual(3162, laUne.VideoPid);
      Assert.AreEqual(3162, laUne.PcrPid);
      Assert.AreEqual(3089, laUne.AudioPid);
      Assert.AreEqual(10892m, laUne.FreqInMhz);
      Assert.AreEqual(27500, laUne.SymbolRate);
      Assert.AreEqual('H', laUne.Polarity);
      Assert.AreEqual("Hotbird", laUne.Satellite);
      Assert.AreEqual("13.0E", laUne.SatPosition);

      var rbb = tv.Channels.Single(ch => ch.Name == "RBB Berlin");
      Assert.AreEqual(17, rbb.OldProgramNr);
      Assert.AreEqual(28206, rbb.ServiceId);
      Assert.AreEqual(12109m, rbb.FreqInMhz);
      Assert.AreEqual("Astra1", rbb.Satellite);
      Assert.AreEqual("19.2E", rbb.SatPosition);

      Assert.AreEqual(16, tv.Channels.Single(ch => ch.Name == "Comédie+ HD").OldProgramNr);
      Assert.AreEqual("28.2E", tv.Channels.Single(ch => ch.Name == "BBC One HD (28)").SatPosition);

      var premiere = radio.Channels.Single(ch => ch.Name == "La prem1ère");
      Assert.AreEqual(1, premiere.OldProgramNr);
      Assert.AreEqual(SignalSource.DvbS | SignalSource.Radio, premiere.SignalSource);
    }
    #endregion

    #region TestSavingUnchangedListKeepsFileIdentical
    [TestMethod]
    public void TestSavingUnchangedListKeepsFileIdentical()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      var original = File.ReadAllBytes(tempFile);

      var ser = new SdxJsonPlugin().CreateSerializer(tempFile);
      ser.Load();
      ser.DataRoot.ValidateAfterLoad();
      ser.DataRoot.ApplyCurrentProgramNumbers();
      ser.Save();

      CollectionAssert.AreEqual(original, File.ReadAllBytes(tempFile));
    }
    #endregion

    #region TestReorderDeleteRename
    [TestMethod]
    public void TestReorderDeleteRename()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);

      var plugin = new SdxJsonPlugin();
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      var data = ser.DataRoot;
      data.ValidateAfterLoad();
      data.ApplyCurrentProgramNumbers();

      var tv = data.ChannelLists.Single(l => l.ShortCaption == "TV");
      var editor = new Editor { DataRoot = data, ChannelList = tv };

      // move "RBB Berlin" to the top, delete "Tipik" and give "La Une HD" a name that exceeds the 27 bytes limit
      var rbb = tv.Channels.Single(ch => ch.Name == "RBB Berlin");
      editor.SetSlotNumber(new[] { rbb }, 1, false, false);
      tv.Channels.Single(ch => ch.Name == "Tipik").NewProgramNr = -1;
      data.AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode.Delete);
      tv.Channels.Single(ch => ch.Name == "La Une HD").Name = "ÄÖÜ äöü 0123456789 abcdefghij";

      ser.Save();

      var text = File.ReadAllText(tempFile, Encoding.UTF8);
      Assert.IsFalse(text.Contains("\"Tipik\""));
      StringAssert.Contains(text, "\"ucNameLen\":\t27,");
      StringAssert.Contains(text, "\"ServiceName\":\t\"ÄÖÜ äöü 0123456789 ab\",");

      // header must contain the new number of programs and the size of all objects before the header
      var headerPos = text.IndexOf(HeaderStart);
      var header = JObject.Parse(text.Substring(headerPos, text.IndexOf('}', headerPos) - headerPos + 3))["database_header_object"];
      Assert.AreEqual(18, (int)header["sTVNumber"]);
      Assert.AreEqual(8, (int)header["sRadioNumber"]);
      var size = new UTF8Encoding(false).GetByteCount(text.Substring(0, headerPos));
      Assert.AreEqual(size, (int)header["uiOriginalSize"]);
      Assert.AreEqual(size, (int)header["uiFileLength"]);

      // reload
      ser = plugin.CreateSerializer(tempFile);
      ser.Load();
      data = ser.DataRoot;
      data.ValidateAfterLoad();
      tv = data.ChannelLists.Single(l => l.ShortCaption == "TV");
      Assert.AreEqual(18, tv.Count);
      Assert.AreEqual(1, tv.Channels.Single(ch => ch.Name == "RBB Berlin").OldProgramNr);
      Assert.AreEqual(2, tv.Channels.Single(ch => ch.Name == "ÄÖÜ äöü 0123456789 ab").OldProgramNr);
      Assert.AreEqual(3, tv.Channels.Single(ch => ch.Name == "RTL-TVI HD").OldProgramNr);
      Assert.AreEqual(8, data.ChannelLists.Single(l => l.ShortCaption == "Radio").Count);
    }
    #endregion

    #region TestChannelAndFavListEditing
    [TestMethod]
    public void TestChannelAndFavListEditing()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);
      RoundtripTest.TestChannelAndFavListEditing(tempFile, new SdxJsonPlugin());
    }
    #endregion

    #region TestDeletingIsDisabledWhenFavoritesAreUsed
    [TestMethod]
    public void TestDeletingIsDisabledWhenFavoritesAreUsed()
    {
      var tempFile = TestUtils.DeploymentItem(TestFile);

      // the sample file has only empty favorite lists, so pretend the first list contains a TV program
      var text = File.ReadAllText(tempFile, Encoding.UTF8);
      var pos = text.IndexOf("\"sNoOfTVFavor\":\t0");
      text = text.Substring(0, pos) + "\"sNoOfTVFavor\":\t1" + text.Substring(pos + 17);
      File.WriteAllText(tempFile, text, new UTF8Encoding(false));

      var ser = new SdxJsonPlugin().CreateSerializer(tempFile);
      ser.Load();
      Assert.AreEqual(SerializerBase.DeleteMode.NotSupported, ser.Features.DeleteMode);
    }
    #endregion

    #region TestSatcoDxFilesAreLeftToSatcoDxLoader
    [TestMethod]
    public void TestSatcoDxFilesAreLeftToSatcoDxLoader()
    {
      var jsonFile = TestUtils.DeploymentItem(TestFile);
      Assert.IsNull(new SatcoDxPlugin().CreateSerializer(jsonFile));

      var textFile = Path.Combine(Path.GetTempPath(), "ChanSort_SdxJsonTest.sdx");
      File.WriteAllText(textFile, "SATCODX103 ...\n");
      try
      {
        Assert.IsNull(new SdxJsonPlugin().CreateSerializer(textFile));
      }
      finally
      {
        File.Delete(textFile);
      }
    }
    #endregion
  }
}
