using System;
using System.Drawing;
using ChanSort.Api;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;

namespace ChanSort.Ui.Printing
{
  public partial class ReportOptionsDialog : XtraForm
  {
    private readonly ChannelList channelList;
    private readonly int subListIndex;

    public ReportOptionsDialog(ChannelList channelList, int subListIndex)
    {
      InitializeComponent();
      this.channelList = channelList;
      this.subListIndex = subListIndex;
      if (Properties.Settings.Default.PrintSortByName)
        this.rbSortByName.Checked = true;
      this.fontEdit1.EditValue = Properties.Settings.Default.PrintFontName;
      this.spinFontSize.Value = Properties.Settings.Default.PrintFontSize;
    }

    private void btnPreview_Click(object sender, EventArgs e)
    {
      Properties.Settings.Default.PrintFontName = (string)this.fontEdit1.EditValue;
      Properties.Settings.Default.PrintFontSize = this.spinFontSize.Value;
      Properties.Settings.Default.PrintSortByName = this.rbSortByNumber.Checked;
      Properties.Settings.Default.Save();

      using (var font = new Font(this.fontEdit1.Text, (float)this.spinFontSize.Value))
      using (var report = new ChannelListReport(this.channelList, this.subListIndex, this.rbSortByName.Checked, font))
      using (ReportPrintTool printTool = new ReportPrintTool(report))
      {
        printTool.ShowPreviewDialog();
        printTool.ShowPreview(UserLookAndFeel.Default);
      }
    }
  }
}