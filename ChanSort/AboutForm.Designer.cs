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
      this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
      this.lnkDownload = new DevExpress.XtraEditors.HyperLinkEdit();
      this.gcPlugins = new DevExpress.XtraGrid.GridControl();
      this.gvPlugins = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colPlugin = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDisplayText = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFileTypes = new DevExpress.XtraGrid.Columns.GridColumn();
      this.lnkEmail = new DevExpress.XtraEditors.HyperLinkEdit();
      this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
      this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
      this.lnkLicense = new DevExpress.XtraEditors.HyperLinkEdit();
      this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
      this.txtCredits = new DevExpress.XtraEditors.MemoEdit();
      this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
      this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
      ((System.ComponentModel.ISupportInitialize)(this.lnkDownload.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gcPlugins)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvPlugins)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lnkEmail.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lnkLicense.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtCredits.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // labelControl1
      // 
      resources.ApplyResources(this.labelControl1, "labelControl1");
      this.labelControl1.Name = "labelControl1";
      // 
      // lnkDownload
      // 
      resources.ApplyResources(this.lnkDownload, "lnkDownload");
      this.lnkDownload.Name = "lnkDownload";
      this.lnkDownload.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("lnkDownload.Properties.Appearance.BackColor")));
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
      resources.ApplyResources(this.colPlugin, "colPlugin");
      this.colPlugin.FieldName = "PluginDll";
      this.colPlugin.Name = "colPlugin";
      this.colPlugin.UnboundType = DevExpress.Data.UnboundColumnType.String;
      // 
      // colDisplayText
      // 
      resources.ApplyResources(this.colDisplayText, "colDisplayText");
      this.colDisplayText.FieldName = "PluginName";
      this.colDisplayText.Name = "colDisplayText";
      // 
      // colFileTypes
      // 
      resources.ApplyResources(this.colFileTypes, "colFileTypes");
      this.colFileTypes.FieldName = "FileFilter";
      this.colFileTypes.Name = "colFileTypes";
      // 
      // lnkEmail
      // 
      resources.ApplyResources(this.lnkEmail, "lnkEmail");
      this.lnkEmail.Name = "lnkEmail";
      this.lnkEmail.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("lnkEmail.Properties.Appearance.BackColor")));
      this.lnkEmail.Properties.Appearance.Options.UseBackColor = true;
      this.lnkEmail.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.lnkEmail.OpenLink += new DevExpress.XtraEditors.Controls.OpenLinkEventHandler(this.lnkEmail_OpenLink);
      // 
      // labelControl2
      // 
      resources.ApplyResources(this.labelControl2, "labelControl2");
      this.labelControl2.Name = "labelControl2";
      // 
      // labelControl4
      // 
      resources.ApplyResources(this.labelControl4, "labelControl4");
      this.labelControl4.Name = "labelControl4";
      // 
      // lnkLicense
      // 
      resources.ApplyResources(this.lnkLicense, "lnkLicense");
      this.lnkLicense.Name = "lnkLicense";
      this.lnkLicense.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("lnkLicense.Properties.Appearance.BackColor")));
      this.lnkLicense.Properties.Appearance.Options.UseBackColor = true;
      this.lnkLicense.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.lnkLicense.OpenLink += new DevExpress.XtraEditors.Controls.OpenLinkEventHandler(this.lnkLicense_OpenLink);
      // 
      // labelControl5
      // 
      resources.ApplyResources(this.labelControl5, "labelControl5");
      this.labelControl5.Name = "labelControl5";
      // 
      // txtCredits
      // 
      resources.ApplyResources(this.txtCredits, "txtCredits");
      this.txtCredits.Name = "txtCredits";
      // 
      // simpleButton1
      // 
      resources.ApplyResources(this.simpleButton1, "simpleButton1");
      this.simpleButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.simpleButton1.Name = "simpleButton1";
      // 
      // labelControl3
      // 
      resources.ApplyResources(this.labelControl3, "labelControl3");
      this.labelControl3.Name = "labelControl3";
      // 
      // AboutForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.labelControl3);
      this.Controls.Add(this.simpleButton1);
      this.Controls.Add(this.txtCredits);
      this.Controls.Add(this.labelControl5);
      this.Controls.Add(this.lnkLicense);
      this.Controls.Add(this.labelControl4);
      this.Controls.Add(this.labelControl2);
      this.Controls.Add(this.lnkEmail);
      this.Controls.Add(this.lnkDownload);
      this.Controls.Add(this.labelControl1);
      this.Name = "AboutForm";
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

    private DevExpress.XtraEditors.LabelControl labelControl1;
    private DevExpress.XtraEditors.HyperLinkEdit lnkDownload;
    private DevExpress.XtraGrid.GridControl gcPlugins;
    private DevExpress.XtraGrid.Views.Grid.GridView gvPlugins;
    private DevExpress.XtraGrid.Columns.GridColumn colPlugin;
    private DevExpress.XtraGrid.Columns.GridColumn colDisplayText;
    private DevExpress.XtraGrid.Columns.GridColumn colFileTypes;
    private DevExpress.XtraEditors.HyperLinkEdit lnkEmail;
    private DevExpress.XtraEditors.LabelControl labelControl2;
    private DevExpress.XtraEditors.LabelControl labelControl4;
    private DevExpress.XtraEditors.HyperLinkEdit lnkLicense;
    private DevExpress.XtraEditors.LabelControl labelControl5;
    private DevExpress.XtraEditors.MemoEdit txtCredits;
    private DevExpress.XtraEditors.SimpleButton simpleButton1;
    private DevExpress.XtraEditors.LabelControl labelControl3;
  }
}