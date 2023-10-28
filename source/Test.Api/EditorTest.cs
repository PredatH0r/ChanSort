using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ChanSort.Api;

namespace Test.Api;

[TestClass]
public class EditorTest
{
  [TestMethod]
  public void TestEnforceTvBeforeRadioBeforeData()
  {
    var list = new ChannelList(SignalSource.All, "All");
    list.AddChannel(new ChannelInfo(SignalSource.Tv, 0, 1, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Tv, 1, 2, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Data, 2, 3, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Tv, 3, 4, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Radio, 4, 5, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Tv, 5, 6, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Radio, 6, 7, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Tv, 7, 8, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Data, 8, 9, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Data, 9, 10, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Radio, 10, 11, ""));
    list.AddChannel(new ChannelInfo(SignalSource.Tv, 11, 12, ""));

    var ser = new CsvRefListSerializer("foo.csv");
    var dataRoot = new DataRoot(ser);
    dataRoot.AddChannelList(list);
    dataRoot.ApplyCurrentProgramNumbers();
    
    var editor = new Editor();
    editor.DataRoot = dataRoot;
    Assert.IsTrue(editor.EnforceTvBeforeRadioBeforeData());

    var expected = new[] { 0, 1, 3, 5, 7, 11, 4, 6, 10, 2, 8, 9 };
    var newOrder = list.Channels.OrderBy(ch => ch.NewProgramNr).ToList();
    for (int i = 0; i<list.Channels.Count; i++)
      Assert.AreEqual(expected[i], newOrder[i].RecordIndex);

    // running it again should produce no changes
    Assert.IsFalse(editor.EnforceTvBeforeRadioBeforeData());
    for (int i = 0; i < list.Channels.Count; i++)
      Assert.AreEqual(expected[i], newOrder[i].RecordIndex);
  }
}

