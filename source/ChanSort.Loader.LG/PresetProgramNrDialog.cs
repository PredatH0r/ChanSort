using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ChanSort.Loader.LG
{
  public partial class PresetProgramNrDialog : XtraForm
  {
    public PresetProgramNrDialog()
    {
      InitializeComponent();
    }

    private void linkDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        BrowserHelper.OpenUrl(
          "http://sourceforge.net/p/chansort/wiki/Channels%20disappear%20or%20change%20program%20numbers%20randomly/");
      }
      catch { }
    }

    private void btnOk_Click(object sender, System.EventArgs e)
    {
      if (ModifierKeys == (Keys.Alt|Keys.Control))
        this.DialogResult = DialogResult.Yes;
    }
  }
}
