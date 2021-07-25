namespace ChanSort.Ui
{
  partial class SkinPickerForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkinPickerForm));
      this.gallery = new DevExpress.XtraBars.Ribbon.GalleryControl();
      this.galleryControlClient1 = new DevExpress.XtraBars.Ribbon.GalleryControlClient();
      this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
      this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
      this.btnOk = new DevExpress.XtraEditors.SimpleButton();
      this.btnReset = new DevExpress.XtraEditors.SimpleButton();
      ((System.ComponentModel.ISupportInitialize)(this.gallery)).BeginInit();
      this.gallery.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
      this.panelControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // gallery
      // 
      resources.ApplyResources(this.gallery, "gallery");
      this.gallery.Controls.Add(this.galleryControlClient1);
      // 
      // 
      // 
      this.gallery.Gallery.AllowFilter = false;
      this.gallery.Gallery.ColumnCount = 8;
      this.gallery.Gallery.ImageSize = new System.Drawing.Size(48, 48);
      this.gallery.Gallery.ShowItemText = true;
      this.gallery.Gallery.ShowScrollBar = DevExpress.XtraBars.Ribbon.Gallery.ShowScrollBar.Auto;
      this.gallery.Name = "gallery";
      // 
      // galleryControlClient1
      // 
      resources.ApplyResources(this.galleryControlClient1, "galleryControlClient1");
      this.galleryControlClient1.GalleryControl = this.gallery;
      // 
      // panelControl1
      // 
      resources.ApplyResources(this.panelControl1, "panelControl1");
      this.panelControl1.Controls.Add(this.btnCancel);
      this.panelControl1.Controls.Add(this.btnOk);
      this.panelControl1.Controls.Add(this.btnReset);
      this.panelControl1.Name = "panelControl1";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnReset
      // 
      resources.ApplyResources(this.btnReset, "btnReset");
      this.btnReset.Name = "btnReset";
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // SkinPickerForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.gallery);
      this.Controls.Add(this.panelControl1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "SkinPickerForm";
      this.Load += new System.EventHandler(this.SkinPicker_Load);
      ((System.ComponentModel.ISupportInitialize)(this.gallery)).EndInit();
      this.gallery.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
      this.panelControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.XtraBars.Ribbon.GalleryControl gallery;
    private DevExpress.XtraBars.Ribbon.GalleryControlClient galleryControlClient1;
    private DevExpress.XtraEditors.PanelControl panelControl1;
    private DevExpress.XtraEditors.SimpleButton btnCancel;
    private DevExpress.XtraEditors.SimpleButton btnOk;
    private DevExpress.XtraEditors.SimpleButton btnReset;
  }
}