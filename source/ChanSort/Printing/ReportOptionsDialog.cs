using System;
using System.Drawing;
using ChanSort.Api;
using ChanSort.Ui.Properties;
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
      if (Config.Default.PrintSortByName)
        this.rbSortByName.Checked = true;
      this.fontEdit1.EditValue = Config.Default.PrintFontName;
      this.spinFontSize.Value = Config.Default.PrintFontSize;
    }

    private void btnPreview_Click(object sender, EventArgs e)
    {
      Config.Default.PrintFontName = (string)this.fontEdit1.EditValue;
      Config.Default.PrintFontSize = this.spinFontSize.Value;
      Config.Default.PrintSortByName = this.rbSortByNumber.Checked;
      Config.Default.Save();

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