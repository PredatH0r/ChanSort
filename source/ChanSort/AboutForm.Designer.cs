namespace ChanSort.Ui
{
  partial class AboutForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
      this.lblWebsite = new DevExpress.XtraEditors.LabelControl();
      this.lnkDownload = new DevExpress.XtraEditors.HyperLinkEdit();
      this.gcPlugins = new DevExpress.XtraGrid.GridControl();
      this.gvPlugins = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colPlugin = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDisplayText = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFileTypes = new DevExpress.XtraGrid.Columns.GridColumn();
      this.lnkEmail = new DevExpress.XtraEditors.HyperLinkEdit();
      this.lblAuthor = new DevExpress.XtraEditors.LabelControl();
      this.lblLicense = new DevExpress.XtraEditors.LabelControl();
      this.lnkLicense = new DevExpress.XtraEditors.HyperLinkEdit();
      this.lblCredits = new DevExpress.XtraEditors.LabelControl();
      this.txtCredits = new DevExpress.XtraEditors.MemoEdit();
      this.btnClose = new DevExpress.XtraEditors.SimpleButton();
      this.txtAuthor = new DevExpress.XtraEditors.LabelControl();
      ((System.ComponentModel.ISupportInitialize)(this.lnkDownload.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gcPlugins)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvPlugins)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lnkEmail.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lnkLicense.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtCredits.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // lblWebsite
      // 
      resources.ApplyResources(this.lblWebsite, "lblWebsite");
      this.lblWebsite.Name = "lblWebsite";
      // 
      // lnkDownload
      // 
      resources.ApplyResources(this.lnkDownload, "lnkDownload");
      this.lnkDownload.Name = "lnkDownload";
      this.lnkDownload.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
      this.lnkDownload.Properties.Appearance.Options.UseBackColor = true;
      this.lnkDownload.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.lnkDownload.OpenLink += new DevExpress.XtraEditors.Controls.OpenLinkEventHandler(this.lnkDownload_OpenLink);
      // 
      // gcPlugins
      // 
      resources.ApplyResources(this.gcPlugins, "gcPlugins");
      this.gcPlugins.MainView = this.gvPlugins;
      this.gcPlugins.Name = "gcPlugins";
      this.gcPlugins.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvPlugins});
      // 
      // gvPlugins
      // 
      this.gvPlugins.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colPlugin,
            this.colDisplayText,
            this.colFileTypes});
      this.gvPlugins.GridControl = this.gcPlugins;
      this.gvPlugins.Name = "gvPlugins";
      this.gvPlugins.OptionsBehavior.ReadOnly = true;
      this.gvPlugins.OptionsView.ShowGroupPanel = false;
      this.gvPlugins.OptionsView.ShowIndicator = false;
      // 
      // colPlugin
      // 
      this.colPlugin.FieldName = "PluginDll";
      this.colPlugin.Name = "colPlugin";
      this.colPlugin.UnboundType = DevExpress.Data.UnboundColumnType.String;
      // 
      // colDisplayText
      // 
      this.colDisplayText.FieldName = "PluginName";
      this.colDisplayText.Name = "colDisplayText";
      // 
      // colFileTypes
      // 
      this.colFileTypes.FieldName = "FileFilter";
      this.colFileTypes.Name = "colFileTypes";
      // 
      // lnkEmail
      // 
      resources.ApplyResources(this.lnkEmail, "lnkEmail");
      this.lnkEmail.Name = "lnkEmail";
      this.lnkEmail.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
      this.lnkEmail.Properties.Appearance.Options.UseBackColor = true;
      this.lnkEmail.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.lnkEmail.OpenLink += new DevExpress.XtraEditors.Controls.OpenLinkEventHandler(this.lnkEmail_OpenLink);
      // 
      // lblAuthor
      // 
      resources.ApplyResources(this.lblAuthor, "lblAuthor");
      this.lblAuthor.Name = "lblAuthor";
      // 
      // lblLicense
      // 
      resources.ApplyResources(this.lblLicense, "lblLicense");
      this.lblLicense.Name = "lblLicense";
      // 
      // lnkLicense
      // 
      resources.ApplyResources(this.lnkLicense, "lnkLicense");
      this.lnkLicense.Name = "lnkLicense";
      this.lnkLicense.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
      this.lnkLicense.Properties.Appearance.Options.UseBackColor = true;
      this.lnkLicense.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.lnkLicense.OpenLink += new DevExpress.XtraEditors.Controls.OpenLinkEventHandler(this.lnkLicense_OpenLink);
      // 
      // lblCredits
      // 
      resources.ApplyResources(this.lblCredits, "lblCredits");
      this.lblCredits.Name = "lblCredits";
      // 
      // txtCredits
      // 
      resources.ApplyResources(this.txtCredits, "txtCredits");
      this.txtCredits.Name = "txtCredits";
      // 
      // btnClose
      // 
      resources.ApplyResources(this.btnClose, "btnClose");
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnClose.Name = "btnClose";
      // 
      // txtAuthor
      // 
      resources.ApplyResources(this.txtAuthor, "txtAuthor");
      this.txtAuthor.Name = "txtAuthor";
      // 
      // AboutForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.txtAuthor);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.txtCredits);
      this.Controls.Add(this.lblCredits);
      this.Controls.Add(this.lnkLicense);
      this.Controls.Add(this.lblLicense);
      this.Controls.Add(this.lblAuthor);
      this.Controls.Add(this.lnkEmail);
      this.Controls.Add(this.lnkDownload);
      this.Controls.Add(this.lblWebsite);
      this.Name = "AboutForm";
      this.ShowInTaskbar = false;
      ((System.ComponentModel.ISupportInitialize)(this.lnkDownload.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gcPlugins)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvPlugins)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lnkEmail.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lnkLicense.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtCredits.Properties)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraEditors.LabelControl lblWebsite;
    private DevExpress.XtraEditors.HyperLinkEdit lnkDownload;
    private DevExpress.XtraGrid.GridControl gcPlugins;
    private DevExpress.XtraGrid.Views.Grid.GridView gvPlugins;
    private DevExpress.XtraGrid.Columns.GridColumn colPlugin;
    private DevExpress.XtraGrid.Columns.GridColumn colDisplayText;
    private DevExpress.XtraGrid.Columns.GridColumn colFileTypes;
    private DevExpress.XtraEditors.HyperLinkEdit lnkEmail;
    private DevExpress.XtraEditors.LabelControl lblAuthor;
    private DevExpress.XtraEditors.LabelControl lblLicense;
    private DevExpress.XtraEditors.HyperLinkEdit lnkLicense;
    private DevExpress.XtraEditors.LabelControl lblCredits;
    private DevExpress.XtraEditors.MemoEdit txtCredits;
    private DevExpress.XtraEditors.SimpleButton btnClose;
    private DevExpress.XtraEditors.LabelControl txtAuthor;
  }
}