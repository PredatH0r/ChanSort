using System.Collections.Generic;
using ChanSort.Api;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  public partial class AboutForm :XtraForm
  {
    public AboutForm(IList<ISerializerPlugin> plugins)
    {
      InitializeComponent();
      this.gcPlugins.DataSource = plugins;
      
      this.txtCredits.Text = 
@"TCr82:
Support for VDR's channels.conf file format

Vitor Martins Augusto:
Portuguese translation

PDA-User:
Analysis of TLL file structure and extensive testing of ChanSort

HellG:
For writing TLLsort which was the basis for the first versions of ChanSort

edank, JLevi, Oleg:
For writing TLLview, TLLedit and edankTLL and making the source available

Many more on lg-forum.com:
For providing example TLL files, error feedback and other helpful information
";
    }

    private void lnkLicense_OpenLink(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
    {
      BrowserHelper.OpenUrl("http://www.gnu.org/licenses/gpl.html");
    }

    private void lnkEmail_OpenLink(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
    {
      BrowserHelper.OpenMail("mailto:horst@beham.biz&subject=ChanSort%20" + MainForm.AppVersion);
    }

    private void lnkDownload_OpenLink(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
    {
      BrowserHelper.OpenUrl(this.lnkDownload.Text);
    }
  }
}