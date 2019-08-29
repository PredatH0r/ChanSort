using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ChanSort.Api;
using DevExpress.XtraReports.UI;

namespace ChanSort.Ui
{
  public partial class ChannelListReport : XtraReport
  {
    private readonly int subListIndex;
    private readonly bool orderByName;

    public ChannelListReport(ChannelList list, int subListIndex, bool orderByName, Font font)
    {
      this.subListIndex = subListIndex;
      this.orderByName = orderByName;

      InitializeComponent();
      this.DataSource = GenerateDataSource(list);

      this.txtHeading.Text = list.ShortCaption + (subListIndex <= 0 ? "" : " - Fav " + (char)('A' + subListIndex - 1));
     
      this.txtHeading.Font = new Font(font.Name, font.Size+4, FontStyle.Bold);
      this.txtNumber.Font = font;
      this.txtChannelName.Font = font;
    }

    private List<ChannelList> GenerateDataSource(ChannelList list)
    {
      ChannelList sortedList = new ChannelList(list.SignalSource, list.Caption);
      foreach (var channel in list.Channels.OrderBy(this.SortCriteria))
      {
        int pos = channel.GetPosition(this.subListIndex);
        if (orderByName && (channel.Name == "" || channel.Name == "."))
          continue;
        if (pos >= 0)
          sortedList.Channels.Add(channel);
      }

      List<ChannelList> lists = new List<ChannelList>();
      lists.Add(sortedList);
      return lists;
    }

    private IComparable SortCriteria(ChannelInfo a)
    {
      return this.orderByName ? (IComparable) a.Name : a.GetPosition(this.subListIndex);
    }

    private void bandChannelDetail_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
    {
      var channel = (ChannelInfo) this.repChannels.GetCurrentRow();
      if (channel == null) // happens if there are no data records
        return;
      
      this.txtNumber.Text = channel.GetPosition(this.subListIndex).ToString();
      this.txtChannelName.Text = channel.Name;
    }
  }
}
