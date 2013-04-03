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
      this.hyperLinkEdit1 = new DevExpress.XtraEditors.HyperLinkEdit();
      this.gcPlugins = new DevExpress.XtraGrid.GridControl();
      this.gvPlugins = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colPlugin = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDisplayText = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFileTypes = new DevExpress.XtraGrid.Columns.GridColumn();
      this.hyperLinkEdit2 = new DevExpress.XtraEditors.HyperLinkEdit();
      this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
      this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
      this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
      this.hyperLinkEdit3 = new DevExpress.XtraEditors.HyperLinkEdit();
      this.txtCredits = new DevExpress.XtraEditors.MemoEdit();
      this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
      this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
      this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
      this.picDonate = new DevExpress.XtraEditors.PictureEdit();
      this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
      ((System.ComponentModel.ISupportInitialize)(this.hyperLinkEdit1.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gcPlugins)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvPlugins)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.hyperLinkEdit2.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.hyperLinkEdit3.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtCredits.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
      this.xtraTabControl1.SuspendLayout();
      this.xtraTabPage1.SuspendLayout();
      this.xtraTabPage2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picDonate.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // labelControl1
      // 
      resources.ApplyResources(this.labelControl1, "labelControl1");
      this.labelControl1.Name = "labelControl1";
      // 
      // hyperLinkEdit1
      // 
      resources.ApplyResources(this.hyperLinkEdit1, "hyperLinkEdit1");
      this.hyperLinkEdit1.Name = "hyperLinkEdit1";
      this.hyperLinkEdit1.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("hyperLinkEdit1.Properties.Appearance.BackColor")));
      this.hyperLinkEdit1.Properties.Appearance.Options.UseBackColor = true;
      this.hyperLinkEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
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
      this.gvPlugins.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gvPlugins_CustomUnboundColumnData);
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
      // hyperLinkEdit2
      // 
      resources.ApplyResources(this.hyperLinkEdit2, "hyperLinkEdit2");
      this.hyperLinkEdit2.Name = "hyperLinkEdit2";
      this.hyperLinkEdit2.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("hyperLinkEdit2.Properties.Appearance.BackColor")));
      this.hyperLinkEdit2.Properties.Appearance.Options.UseBackColor = true;
      this.hyperLinkEdit2.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      // 
      // labelControl3
      // 
      resources.ApplyResources(this.labelControl3, "labelControl3");
      this.labelControl3.Name = "labelControl3";
      // 
      // labelControl4
      // 
      resources.ApplyResources(this.labelControl4, "labelControl4");
      this.labelControl4.Name = "labelControl4";
      // 
      // labelControl5
      // 
      resources.ApplyResources(this.labelControl5, "labelControl5");
      this.labelControl5.Name = "labelControl5";
      // 
      // hyperLinkEdit3
      // 
      resources.ApplyResources(this.hyperLinkEdit3, "hyperLinkEdit3");
      this.hyperLinkEdit3.Name = "hyperLinkEdit3";
      this.hyperLinkEdit3.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("hyperLinkEdit3.Properties.Appearance.BackColor")));
      this.hyperLinkEdit3.Properties.Appearance.Options.UseBackColor = true;
      this.hyperLinkEdit3.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      // 
      // txtCredits
      // 
      resources.ApplyResources(this.txtCredits, "txtCredits");
      this.txtCredits.Name = "txtCredits";
      this.txtCredits.Properties.ReadOnly = true;
      // 
      // xtraTabControl1
      // 
      resources.ApplyResources(this.xtraTabControl1, "xtraTabControl1");
      this.xtraTabControl1.Name = "xtraTabControl1";
      this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
      this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
      // 
      // xtraTabPage1
      // 
      this.xtraTabPage1.Controls.Add(this.txtCredits);
      this.xtraTabPage1.Name = "xtraTabPage1";
      resources.ApplyResources(this.xtraTabPage1, "xtraTabPage1");
      // 
      // xtraTabPage2
      // 
      this.xtraTabPage2.Controls.Add(this.gcPlugins);
      this.xtraTabPage2.Name = "xtraTabPage2";
      resources.ApplyResources(this.xtraTabPage2, "xtraTabPage2");
      // 
      // picDonate
      // 
      this.picDonate.Cursor = System.Windows.Forms.Cursors.Hand;
      resources.ApplyResources(this.picDonate, "picDonate");
      this.picDonate.Name = "picDonate";
      this.picDonate.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("picDonate.Properties.Appearance.BackColor")));
      this.picDonate.Properties.Appearance.Options.UseBackColor = true;
      this.picDonate.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.picDonate.Click += new System.EventHandler(this.picDonate_Click);
      // 
      // labelControl2
      // 
      resources.ApplyResources(this.labelControl2, "labelControl2");
      this.labelControl2.Name = "labelControl2";
      // 
      // AboutForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.labelControl2);
      this.Controls.Add(this.picDonate);
      this.Controls.Add(this.xtraTabControl1);
      this.Controls.Add(this.hyperLinkEdit3);
      this.Controls.Add(this.labelControl5);
      this.Controls.Add(this.labelControl4);
      this.Controls.Add(this.hyperLinkEdit2);
      this.Controls.Add(this.labelControl3);
      this.Controls.Add(this.hyperLinkEdit1);
      this.Controls.Add(this.labelControl1);
      this.Name = "AboutForm";
      ((System.ComponentModel.ISupportInitialize)(this.hyperLinkEdit1.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gcPlugins)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvPlugins)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.hyperLinkEdit2.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.hyperLinkEdit3.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtCredits.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
      this.xtraTabControl1.ResumeLayout(false);
      this.xtraTabPage1.ResumeLayout(false);
      this.xtraTabPage2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.picDonate.Properties)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraEditors.LabelControl labelControl1;
    private DevExpress.XtraEditors.HyperLinkEdit hyperLinkEdit1;
    private DevExpress.XtraGrid.GridControl gcPlugins;
    private DevExpress.XtraGrid.Views.Grid.GridView gvPlugins;
    private DevExpress.XtraGrid.Columns.GridColumn colPlugin;
    private DevExpress.XtraGrid.Columns.GridColumn colDisplayText;
    private DevExpress.XtraGrid.Columns.GridColumn colFileTypes;
    private DevExpress.XtraEditors.HyperLinkEdit hyperLinkEdit2;
    private DevExpress.XtraEditors.LabelControl labelControl3;
    private DevExpress.XtraEditors.LabelControl labelControl4;
    private DevExpress.XtraEditors.LabelControl labelControl5;
    private DevExpress.XtraEditors.HyperLinkEdit hyperLinkEdit3;
    private DevExpress.XtraEditors.MemoEdit txtCredits;
    private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
    private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
    private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
    private DevExpress.XtraEditors.PictureEdit picDonate;
    private DevExpress.XtraEditors.LabelControl labelControl2;
  }
}