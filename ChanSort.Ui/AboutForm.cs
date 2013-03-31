using System.Collections.Generic;
using System.IO;
using ChanSort.Api;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  public partial class AboutForm :XtraForm
  {
    public AboutForm(IList<ISerializerPlugin> plugins)
    {
      InitializeComponent();
      this.picDonate.Image = Properties.Resources.Donate;
      this.gcPlugins.DataSource = plugins;
      
      this.txtCredits.Text = @"PDA-User:
Analysis of TLL file structure and extensive testing of ChanSort

HellG:
For writing TLLsort which was the basis for the first versions of ChanSort

edank, JLevi, Oleg:
For writing TLLview, TLLedit and edankTLL and making the source available

Many more on lg-forum.com:
For providing example TLL files, error feedback and other helpful information";
    }

    private void gvPlugins_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {
      if (e.Column == this.colPlugin)
        e.Value = (Path.GetFileName(e.Row.GetType().Assembly.Location) ?? "").Replace("ChanSort.Plugin.", "");
    }

    private void picDonate_Click(object sender, System.EventArgs e)
    {
      try
      {
        string fileName = Path.GetTempFileName() + ".html";
        File.WriteAllText(fileName, Properties.Resources.paypal_button);
        System.Diagnostics.Process.Start(fileName);
      }
      catch
      {
      }
    }
  }
}