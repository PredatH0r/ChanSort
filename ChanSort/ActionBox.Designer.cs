namespace ChanSort.Ui
{
  partial class ActionBoxDialog
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActionBoxDialog));
      this.lblMessage = new DevExpress.XtraEditors.LabelControl();
      this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
      this.SuspendLayout();
      // 
      // lblMessage
      // 
      resources.ApplyResources(this.lblMessage, "lblMessage");
      this.lblMessage.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("lblMessage.Appearance.Font")));
      this.lblMessage.Name = "lblMessage";
      // 
      // imageCollection1
      // 
      resources.ApplyResources(this.imageCollection1, "imageCollection1");
      this.imageCollection1.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection1.ImageStream")));
      this.imageCollection1.Images.SetKeyName(0, "0000.png");
      this.imageCollection1.Images.SetKeyName(1, "0001.png");
      this.imageCollection1.Images.SetKeyName(2, "0002.png");
      this.imageCollection1.Images.SetKeyName(3, "0003.png");
      this.imageCollection1.Images.SetKeyName(4, "0004.png");
      this.imageCollection1.Images.SetKeyName(5, "0005.png");
      this.imageCollection1.Images.SetKeyName(6, "0006.png");
      this.imageCollection1.Images.SetKeyName(7, "0007.png");
      // 
      // ActionBoxDialog
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ControlBox = false;
      this.Controls.Add(this.lblMessage);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ActionBoxDialog";
      this.ShowInTaskbar = false;
      ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.XtraEditors.LabelControl lblMessage;
    private DevExpress.Utils.ImageCollection imageCollection1;
  }
}