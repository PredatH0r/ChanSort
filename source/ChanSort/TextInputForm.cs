using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  public partial class TextInputForm : XtraForm
  {
    public TextInputForm()
    {
      InitializeComponent();
    }

    private void textEdit_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
        this.DialogResult = DialogResult.Cancel;
      else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
        this.DialogResult = DialogResult.OK;
    }

    public override string Text
    {
      get => this.textEdit?.Text ?? "";
      set
      {
        if (this.textEdit != null)
          this.textEdit.Text = value;
      } 
    }
  }
}
