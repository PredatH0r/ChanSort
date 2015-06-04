namespace ChanSort.Ui
{
  partial class InfoBox
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
      this.txtMessage = new DevExpress.XtraEditors.MemoEdit();
      this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
      this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
      ((System.ComponentModel.ISupportInitialize)(this.txtMessage.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
      this.panelControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // txtMessage
      // 
      this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtMessage.Location = new System.Drawing.Point(0, 0);
      this.txtMessage.Name = "txtMessage";
      this.txtMessage.Properties.Appearance.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtMessage.Properties.Appearance.Options.UseFont = true;
      this.txtMessage.Size = new System.Drawing.Size(953, 486);
      this.txtMessage.TabIndex = 0;
      // 
      // panelControl1
      // 
      this.panelControl1.Controls.Add(this.simpleButton1);
      this.panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panelControl1.Location = new System.Drawing.Point(0, 486);
      this.panelControl1.Name = "panelControl1";
      this.panelControl1.Size = new System.Drawing.Size(953, 45);
      this.panelControl1.TabIndex = 1;
      // 
      // simpleButton1
      // 
      this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.simpleButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.simpleButton1.Location = new System.Drawing.Point(866, 10);
      this.simpleButton1.Name = "simpleButton1";
      this.simpleButton1.Size = new System.Drawing.Size(75, 23);
      this.simpleButton1.TabIndex = 0;
      this.simpleButton1.Text = "Ok";
      // 
      // InfoBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(953, 531);
      this.Controls.Add(this.txtMessage);
      this.Controls.Add(this.panelControl1);
      this.Name = "InfoBox";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "InfoBox";
      ((System.ComponentModel.ISupportInitialize)(this.txtMessage.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
      this.panelControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.XtraEditors.MemoEdit txtMessage;
    private DevExpress.XtraEditors.PanelControl panelControl1;
    private DevExpress.XtraEditors.SimpleButton simpleButton1;
  }
}