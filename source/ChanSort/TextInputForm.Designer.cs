namespace ChanSort.Ui
{
  partial class TextInputForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.textEdit = new DevExpress.XtraEditors.TextEdit();
      ((System.ComponentModel.ISupportInitialize)(this.textEdit.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // textEdit
      // 
      this.textEdit.Location = new System.Drawing.Point(0, 0);
      this.textEdit.Name = "textEdit";
      this.textEdit.Size = new System.Drawing.Size(240, 20);
      this.textEdit.TabIndex = 0;
      this.textEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textEdit_KeyDown);
      // 
      // TextInputForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(240, 20);
      this.ControlBox = false;
      this.Controls.Add(this.textEdit);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "TextInputForm";
      this.Text = "TextInputForm";
      ((System.ComponentModel.ISupportInitialize)(this.textEdit.Properties)).EndInit();
      this.ResumeLayout(false);

    }

        #endregion

        private DevExpress.XtraEditors.TextEdit textEdit;
    }
}