using System;
using System.Linq;
using ChanSort.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader
{
  public static class RoundtripTest
  {
    #region TestChannelAndFavListEditing
    public static void TestChannelAndFavListEditing(string tempFile, ISerializerPlugin plugin, bool swapChans = true, int firstProgNr = 1, int firstProgIndex = 0)
    {
      var ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      var root = ser.DataRoot;
      root.ValidateAfterLoad();
      foreach (var l in root.ChannelLists)
      {
        foreach (var ch in l.Channels)
        {
          ch.NewProgramNr = ch.OldProgramNr;
          ch.ResetFavorites();
        }
      }

      int maxFav = Math.Min(4, ser.DataRoot.FavListCount);
      int chanCount = Math.Max(2, maxFav);

      var editor = new Editor();
      editor.DataRoot = root;
      var list = root.ChannelLists.FirstOrDefault(l => l.Channels.Count(ch => !ch.IsDeleted) >= chanCount);
      Assert.IsNotNull(list);
      editor.ChannelList = list;

      // swap channels 1 and 2
      if (swapChans)
      {
        var chans1 = list.GetChannelByNewProgNr(firstProgNr + 0);
        var chans2 = list.GetChannelByNewProgNr(firstProgNr + 1);
        Assert.AreEqual(1, chans1.Count);
        Assert.AreEqual(1, chans2.Count);
        Assert.AreEqual(firstProgNr + 0, chans1[0].NewProgramNr);
        Assert.AreEqual(firstProgNr + 1, chans2[0].NewProgramNr);
        editor.SetSlotNumber(chans2, firstProgNr, false, false);
        Assert.AreEqual(firstProgNr + 0, chans2[0].NewProgramNr);
        Assert.AreEqual(firstProgNr + 1, chans1[0].NewProgramNr);
      }

      // set channels 4-1 as favorites 1-4 (using reverse order to catch bugs with equal progNr and favNr)
      var orderedFav = root.SortedFavorites;
      if (root.FavoritesMode != FavoritesMode.None)
      {
        for (int i = 1; i <= maxFav; i++)
        {
          var progNr = maxFav + 1 - i;
          var chans = list.GetChannelByNewProgNr(firstProgNr + progNr - 1);
          Assert.AreEqual(1, chans.Count);
          editor.SetFavorites(chans, i - 1, true);
          Assert.AreEqual(orderedFav ? 1 : firstProgNr + progNr - 1, chans[0].GetPosition(i));
        }
      }

      foreach (var l in root.ChannelLists)
        l.ReadOnly = false;

      ser.Save();


      ser = plugin.CreateSerializer(tempFile);
      ser.Load();

      root = ser.DataRoot;
      root.ValidateAfterLoad();
      root.ApplyCurrentProgramNumbers();
      list = root.ChannelLists.FirstOrDefault(l => l.Count >= chanCount);
      Assert.IsNotNull(list);

      // validate program number and fav numbers
      for (int i = 1; i <= chanCount; i++)
      {
        var chans = list.GetChannelByNewProgNr(firstProgNr + i - 1);
        Assert.AreEqual(1, chans.Count);
        if (i <= maxFav) // ignore favs for lists that don't support favs
          Assert.AreEqual(orderedFav ? 1 : chans[0].NewProgramNr, chans[0].GetPosition(maxFav + 1 - i));
      }
    }
    #endregion

  }
}
