using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{

  public partial class ActionBoxDialog : XtraForm, Api.IActionBoxDialog
  {
    private const int ButtonSpacing = 10;
    private const int ButtonHeight = 50;

    public ActionBoxDialog(string message)
    {
      InitializeComponent();
      this.lblMessage.Text = message;
    }

    public Image EmptyList { get { return this.imageCollection1.Images[0]; } }
    public Image FullList { get { return this.imageCollection1.Images[1]; } }
    public Image CopyList { get { return this.imageCollection1.Images[2]; } }
    public Image Delete { get { return this.imageCollection1.Images[3]; } }
    public Image Cancel { get { return this.imageCollection1.Images[4]; } }
    public Image Save { get { return this.imageCollection1.Images[5]; } }
    public Image Overwrite { get { return this.imageCollection1.Images[6]; } }
    public Image Discard { get { return this.imageCollection1.Images[7]; } }

    #region Message
    public string Message
    {
      get => this.lblMessage.Text;
      set => this.lblMessage.Text = value;
    }
    #endregion

    #region AddAction()

    public void AddAction(string text, DialogResult result, Image image = null, bool isDefault = false)
    {
      this.AddAction(text, (object)result, image, isDefault);
    }

    public void AddAction(string text, int result, Image image = null, bool isDefault = false)
    {
      this.AddAction(text, (object)result, image, isDefault);
    }

    private void AddAction(string text, object result, Image image = null, bool isDefault = false)
    {
      int width = this.ClientSize.Width-20;
      var button = new SimpleButton();
      button.Text = text;
      button.Appearance.TextOptions.HAlignment = HorzAlignment.Near;
      button.Image = image;
      button.Width = width;
      button.Left = 10;
      button.Height = ButtonHeight;
      button.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
      button.Tag = result;
      button.Click += button_Click;
      
      this.Controls.Add(button);

      if (isDefault)
        this.AcceptButton = button;

      if (result is DialogResult)
      {
        button.DialogResult = (DialogResult) result;

        if ((DialogResult) result == DialogResult.Cancel)
        {
          this.CancelButton = button;
          this.ControlBox = true;
          this.SelectedAction = (int)(DialogResult)result;
        }
      }
    }
    #endregion

    #region SelectedAction

    /// <summary>
    /// Returns the action selected by the user
    /// </summary>
    public int SelectedAction { get; protected set; }
    #endregion

    #region OnCreateControl()
    protected override void OnCreateControl()
    {
      base.OnCreateControl();
      int top = this.lblMessage.Bottom + 20;
      foreach (Control c in this.Controls)
      {
        var button = c as SimpleButton;
        if (button != null)
        {
          button.Top = top;
          top += button.Height + ButtonSpacing;
        }
      }
      this.ClientSize = new Size(this.ClientSize.Width, top + 10);
      this.ActiveControl = this.lblMessage;
    }
    #endregion

    #region button_Click
    void button_Click(object sender, EventArgs e)
    {
      this.SelectedAction = (int)((Control)sender).Tag;
      var dlgResult = ((SimpleButton) sender).DialogResult;
      this.DialogResult = dlgResult != DialogResult.None ? dlgResult : DialogResult.OK;
    }
    #endregion

    #region IActionBoxDialog

    // Implementation of an interfact that can be used by the Loaders without a reference to Windows.Forms or DevExpress assemblies

    public void AddAction(string text, int result)
    {
      AddAction(text, result, null, false);
    }

    void Api.IActionBoxDialog.ShowDialog() => this.ShowDialog();

    #endregion
  }
}
