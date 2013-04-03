using System;
using System.Text;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;

namespace ChanSort.Ui
{
  public partial class CharsetForm : XtraForm
  {
    private readonly Encoding initialEncoding = Encoding.Default;
    private Encoding currentEncoding;

    public EventHandler<EncodingChangedEventArgs> EncodingChanged;

    public CharsetForm(Encoding encoding)
    {
      this.initialEncoding = encoding;
      InitializeComponent();
    }

    public Encoding Encoding 
    { 
      get { return this.currentEncoding; }
    }

    private void SelectEncoding(Encoding encoding)
    {
      var encodings = Encoding.GetEncodings();
      for (int i = 0; i < encodings.Length; i++)
      {
        if (encodings[i].Name == encoding.WebName)
        {
          this.gvCharset.FocusedRowHandle = i;
          this.gvCharset.MakeRowVisible(i);
        }
      }      
    }

    private void RaiseEncodingChanged()
    {
      if (this.EncodingChanged != null)
        this.EncodingChanged(this, new EncodingChangedEventArgs(this.currentEncoding));      
    }

    private void CharsetForm_Load(object sender, EventArgs e)
    {
      var encodings = Encoding.GetEncodings();
      this.gcCharset.DataSource = encodings;
    }

    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
      this.SelectEncoding(this.initialEncoding);
    }

    private void gvCharset_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
      var encodingInfo = (EncodingInfo) gvCharset.GetFocusedRow();
      if (encodingInfo == null)
        return;
      this.currentEncoding = Encoding.GetEncoding(encodingInfo.Name);
      this.RaiseEncodingChanged();
    }

    private void gvCharset_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
    {
      if (e.Clicks == 2 && this.gvCharset.IsDataRow(e.RowHandle))
        this.DialogResult = System.Windows.Forms.DialogResult.OK;
    }

    private void btnMyCountry_Click(object sender, EventArgs e)
    {
      this.SelectEncoding(Encoding.Default);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      if (this.DialogResult != System.Windows.Forms.DialogResult.OK)
      {
        // restore initial encoding
        this.currentEncoding = this.initialEncoding;
        this.RaiseEncodingChanged();
      }
    }
  }

  #region class EncodingChangedEventArgs
  public class EncodingChangedEventArgs : EventArgs
  {
    public readonly Encoding Encoding;

    public EncodingChangedEventArgs(Encoding encoding)
    {
      this.Encoding = encoding;
    }
  }
  #endregion
}