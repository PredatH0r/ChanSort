using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  public partial class InfoBox : XtraForm
  {
    public InfoBox()
    {
      InitializeComponent();
      this.ActiveControl = this.simpleButton1;
    }

    public static void Show(IWin32Window owner, string message, string caption)
    {
      if (string.IsNullOrEmpty(message))
        return;
      var box = new InfoBox();
      box.Text = caption;
      box.txtMessage.Text = message;
      box.txtMessage.Properties.ReadOnly = true;
      box.txtMessage.SelectionStart = 0;
      box.txtMessage.SelectionLength = 0;
      box.ShowDialog(owner);
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
      if (keyData == Keys.Escape)
      {
        this.DialogResult = DialogResult.Cancel;
        return true;
      }
      return base.ProcessDialogKey(keyData);
    }
  }
}