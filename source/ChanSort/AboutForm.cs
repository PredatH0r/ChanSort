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
@"
Ciprian Leca: Romanian translation
Istvan Krisko: Hungarian translation
Jakub Driver: Polish translation
Marco Sánchez: Spanish Translation
Pavel Mizera: Czech translation
Vitor Martins Augusto: Portuguese translation
Yaşar Tuna Zorlu: Turkish Translation

TCr82 (Github): Support for VDR's channels.conf file format
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