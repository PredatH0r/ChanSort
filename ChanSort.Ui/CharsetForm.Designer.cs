namespace ChanSort.Ui
{
  partial class CharsetForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharsetForm));
      this.gcCharset = new DevExpress.XtraGrid.GridControl();
      this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
      this.gvCharset = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colCodePage = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDisplayName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
      this.btnMyCountry = new DevExpress.XtraEditors.SimpleButton();
      this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
      this.btnOk = new DevExpress.XtraEditors.SimpleButton();
      ((System.ComponentModel.ISupportInitialize)(this.gcCharset)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvCharset)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
      this.panelControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // gcCharset
      // 
      resources.ApplyResources(this.gcCharset, "gcCharset");
      this.gcCharset.DataSource = this.bindingSource1;
      this.gcCharset.EmbeddedNavigator.AccessibleDescription = resources.GetString("gcCharset.EmbeddedNavigator.AccessibleDescription");
      this.gcCharset.EmbeddedNavigator.AccessibleName = resources.GetString("gcCharset.EmbeddedNavigator.AccessibleName");
      this.gcCharset.EmbeddedNavigator.AllowHtmlTextInToolTip = ((DevExpress.Utils.DefaultBoolean)(resources.GetObject("gcCharset.EmbeddedNavigator.AllowHtmlTextInToolTip")));
      this.gcCharset.EmbeddedNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gcCharset.EmbeddedNavigator.Anchor")));
      this.gcCharset.EmbeddedNavigator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gcCharset.EmbeddedNavigator.BackgroundImage")));
      this.gcCharset.EmbeddedNavigator.BackgroundImageLayout = ((System.Windows.Forms.ImageLayout)(resources.GetObject("gcCharset.EmbeddedNavigator.BackgroundImageLayout")));
      this.gcCharset.EmbeddedNavigator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gcCharset.EmbeddedNavigator.ImeMode")));
      this.gcCharset.EmbeddedNavigator.TextLocation = ((DevExpress.XtraEditors.NavigatorButtonsTextLocation)(resources.GetObject("gcCharset.EmbeddedNavigator.TextLocation")));
      this.gcCharset.EmbeddedNavigator.ToolTip = resources.GetString("gcCharset.EmbeddedNavigator.ToolTip");
      this.gcCharset.EmbeddedNavigator.ToolTipIconType = ((DevExpress.Utils.ToolTipIconType)(resources.GetObject("gcCharset.EmbeddedNavigator.ToolTipIconType")));
      this.gcCharset.EmbeddedNavigator.ToolTipTitle = resources.GetString("gcCharset.EmbeddedNavigator.ToolTipTitle");
      this.gcCharset.MainView = this.gvCharset;
      this.gcCharset.Name = "gcCharset";
      this.gcCharset.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvCharset});
      // 
      // bindingSource1
      // 
      this.bindingSource1.DataSource = typeof(System.Text.EncodingInfo);
      // 
      // gvCharset
      // 
      resources.ApplyResources(this.gvCharset, "gvCharset");
      this.gvCharset.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colCodePage,
            this.colName,
            this.colDisplayName});
      this.gvCharset.GridControl = this.gcCharset;
      this.gvCharset.Name = "gvCharset";
      this.gvCharset.OptionsBehavior.Editable = false;
      this.gvCharset.OptionsView.ShowAutoFilterRow = true;
      this.gvCharset.OptionsView.ShowGroupPanel = false;
      this.gvCharset.OptionsView.ShowIndicator = false;
      this.gvCharset.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gvCharset_RowClick);
      this.gvCharset.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvCharset_FocusedRowChanged);
      // 
      // colCodePage
      // 
      resources.ApplyResources(this.colCodePage, "colCodePage");
      this.colCodePage.FieldName = "CodePage";
      this.colCodePage.Name = "colCodePage";
      this.colCodePage.OptionsColumn.ReadOnly = true;
      // 
      // colName
      // 
      resources.ApplyResources(this.colName, "colName");
      this.colName.FieldName = "Name";
      this.colName.Name = "colName";
      this.colName.OptionsColumn.ReadOnly = true;
      // 
      // colDisplayName
      // 
      resources.ApplyResources(this.colDisplayName, "colDisplayName");
      this.colDisplayName.FieldName = "DisplayName";
      this.colDisplayName.Name = "colDisplayName";
      this.colDisplayName.OptionsColumn.ReadOnly = true;
      this.colDisplayName.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem()});
      // 
      // panelControl1
      // 
      resources.ApplyResources(this.panelControl1, "panelControl1");
      this.panelControl1.Controls.Add(this.btnMyCountry);
      this.panelControl1.Controls.Add(this.btnCancel);
      this.panelControl1.Controls.Add(this.btnOk);
      this.panelControl1.Name = "panelControl1";
      // 
      // btnMyCountry
      // 
      resources.ApplyResources(this.btnMyCountry, "btnMyCountry");
      this.btnMyCountry.Name = "btnMyCountry";
      this.btnMyCountry.Click += new System.EventHandler(this.btnMyCountry_Click);
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      // 
      // CharsetForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.gcCharset);
      this.Controls.Add(this.panelControl1);
      this.Name = "CharsetForm";
      this.Load += new System.EventHandler(this.CharsetForm_Load);
      ((System.ComponentModel.ISupportInitialize)(this.gcCharset)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gvCharset)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
      this.panelControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.XtraGrid.GridControl gcCharset;
    private DevExpress.XtraGrid.Views.Grid.GridView gvCharset;
    private DevExpress.XtraEditors.PanelControl panelControl1;
    private DevExpress.XtraEditors.SimpleButton btnMyCountry;
    private DevExpress.XtraEditors.SimpleButton btnCancel;
    private DevExpress.XtraEditors.SimpleButton btnOk;
    private System.Windows.Forms.BindingSource bindingSource1;
    private DevExpress.XtraGrid.Columns.GridColumn colCodePage;
    private DevExpress.XtraGrid.Columns.GridColumn colName;
    private DevExpress.XtraGrid.Columns.GridColumn colDisplayName;
  }
}