namespace ChanSort.Ui
{
  partial class MainForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
      this.grpOutputList = new DevExpress.XtraEditors.GroupControl();
      this.gridLeft = new DevExpress.XtraGrid.GridControl();
      this.dsChannels = new System.Windows.Forms.BindingSource(this.components);
      this.gviewLeft = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colIndex1 = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutSlot = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutFav = new DevExpress.XtraGrid.Columns.GridColumn();
      this.repositoryItemCheckedComboBoxEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
      this.colUid1 = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutLock = new DevExpress.XtraGrid.Columns.GridColumn();
      this.lblHotkeyLeft = new DevExpress.XtraEditors.LabelControl();
      this.pnlEditControls = new DevExpress.XtraEditors.PanelControl();
      this.btnToggleLock = new DevExpress.XtraEditors.SimpleButton();
      this.globalImageCollection1 = new GlobalImageCollection(this.components);
      this.btnToggleFavE = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavD = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavC = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavB = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavA = new DevExpress.XtraEditors.SimpleButton();
      this.btnClearLeftFilter = new DevExpress.XtraEditors.SimpleButton();
      this.btnRenum = new DevExpress.XtraEditors.SimpleButton();
      this.btnDown = new DevExpress.XtraEditors.SimpleButton();
      this.btnUp = new DevExpress.XtraEditors.SimpleButton();
      this.btnRemove = new DevExpress.XtraEditors.SimpleButton();
      this.grpInputList = new DevExpress.XtraEditors.GroupControl();
      this.gridRight = new DevExpress.XtraGrid.GridControl();
      this.gviewRight = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colIndex = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSlotOld = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSlotNew = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colShortName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFavorites = new DevExpress.XtraGrid.Columns.GridColumn();
      this.repositoryItemCheckedComboBoxEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
      this.colLock = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSkip = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colHidden = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colEncrypted = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colChannelOrTransponder = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFreqInMhz = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colServiceId = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colVideoPid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colAudioPid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colServiceType = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colServiceTypeName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSatellite = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colNetworkId = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colTransportStreamId = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSymbolRate = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colPolarity = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colUid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colNetworkName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colNetworkOperator = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDebug = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colLogicalIndex = new DevExpress.XtraGrid.Columns.GridColumn();
      this.lblHotkeyRight = new DevExpress.XtraEditors.LabelControl();
      this.panelControl3 = new DevExpress.XtraEditors.PanelControl();
      this.btnAddAll = new DevExpress.XtraEditors.SimpleButton();
      this.btnClearRightFilter = new DevExpress.XtraEditors.SimpleButton();
      this.btnAdd = new DevExpress.XtraEditors.SimpleButton();
      this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
      this.bar1 = new DevExpress.XtraBars.Bar();
      this.miFile = new DevExpress.XtraBars.BarSubItem();
      this.miOpen = new DevExpress.XtraBars.BarButtonItem();
      this.miReload = new DevExpress.XtraBars.BarButtonItem();
      this.miRestoreOriginal = new DevExpress.XtraBars.BarButtonItem();
      this.miFileInformation = new DevExpress.XtraBars.BarButtonItem();
      this.miSave = new DevExpress.XtraBars.BarButtonItem();
      this.miSaveAs = new DevExpress.XtraBars.BarButtonItem();
      this.miOpenReferenceFile = new DevExpress.XtraBars.BarButtonItem();
      this.miQuit = new DevExpress.XtraBars.BarButtonItem();
      this.miEdit = new DevExpress.XtraBars.BarSubItem();
      this.miAddChannel = new DevExpress.XtraBars.BarButtonItem();
      this.miRemove = new DevExpress.XtraBars.BarButtonItem();
      this.miSort = new DevExpress.XtraBars.BarButtonItem();
      this.miRenum = new DevExpress.XtraBars.BarButtonItem();
      this.mnuFavSet = new DevExpress.XtraBars.BarSubItem();
      this.miFavSet = new DevExpress.XtraBars.BarListItem();
      this.mnuFavUnset = new DevExpress.XtraBars.BarSubItem();
      this.miFavUnset = new DevExpress.XtraBars.BarListItem();
      this.miLockOn = new DevExpress.XtraBars.BarButtonItem();
      this.miLockOff = new DevExpress.XtraBars.BarButtonItem();
      this.miSkipOn = new DevExpress.XtraBars.BarButtonItem();
      this.miSkipOff = new DevExpress.XtraBars.BarButtonItem();
      this.miHideOn = new DevExpress.XtraBars.BarButtonItem();
      this.miHideOff = new DevExpress.XtraBars.BarButtonItem();
      this.barSubItem2 = new DevExpress.XtraBars.BarSubItem();
      this.miTvSettings = new DevExpress.XtraBars.BarButtonItem();
      this.miEraseChannelData = new DevExpress.XtraBars.BarButtonItem();
      this.mnuOptions = new DevExpress.XtraBars.BarSubItem();
      this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
      this.miEnglish = new DevExpress.XtraBars.BarButtonItem();
      this.miGerman = new DevExpress.XtraBars.BarButtonItem();
      this.mnuCharset = new DevExpress.XtraBars.BarSubItem();
      this.miCharsetForm = new DevExpress.XtraBars.BarButtonItem();
      this.miIsoCharSets = new DevExpress.XtraBars.BarListItem();
      this.mnuHelp = new DevExpress.XtraBars.BarSubItem();
      this.miWiki = new DevExpress.XtraBars.BarButtonItem();
      this.miOpenWebsite = new DevExpress.XtraBars.BarButtonItem();
      this.miAbout = new DevExpress.XtraBars.BarButtonItem();
      this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
      this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
      this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
      this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
      this.miMoveUp = new DevExpress.XtraBars.BarButtonItem();
      this.miMoveDown = new DevExpress.XtraBars.BarButtonItem();
      this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
      this.txtSetSlot = new DevExpress.XtraEditors.ButtonEdit();
      this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
      this.picDonate = new DevExpress.XtraEditors.PictureEdit();
      this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
      this.splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, typeof(global::ChanSort.Ui.WaitForm1), true, true);
      this.grpTopPanel = new DevExpress.XtraEditors.GroupControl();
      this.rbInsertSwap = new DevExpress.XtraEditors.CheckEdit();
      this.rbInsertAfter = new DevExpress.XtraEditors.CheckEdit();
      this.rbInsertBefore = new DevExpress.XtraEditors.CheckEdit();
      this.cbCloseGap = new DevExpress.XtraEditors.CheckEdit();
      this.cbAppendUnsortedChannels = new DevExpress.XtraEditors.CheckEdit();
      this.tabChannelList = new DevExpress.XtraTab.XtraTabControl();
      this.pageEmpty = new DevExpress.XtraTab.XtraTabPage();
      this.mnuContext = new DevExpress.XtraBars.PopupMenu(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
      this.splitContainerControl1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grpOutputList)).BeginInit();
      this.grpOutputList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridLeft)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dsChannels)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewLeft)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pnlEditControls)).BeginInit();
      this.pnlEditControls.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.globalImageCollection1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpInputList)).BeginInit();
      this.grpInputList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridRight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewRight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).BeginInit();
      this.panelControl3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtSetSlot.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.picDonate.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpTopPanel)).BeginInit();
      this.grpTopPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.rbInsertSwap.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbInsertAfter.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbInsertBefore.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbCloseGap.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbAppendUnsortedChannels.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tabChannelList)).BeginInit();
      this.tabChannelList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.mnuContext)).BeginInit();
      this.SuspendLayout();
      // 
      // splitContainerControl1
      // 
      resources.ApplyResources(this.splitContainerControl1, "splitContainerControl1");
      this.splitContainerControl1.Name = "splitContainerControl1";
      resources.ApplyResources(this.splitContainerControl1.Panel1, "splitContainerControl1.Panel1");
      this.splitContainerControl1.Panel1.Controls.Add(this.grpOutputList);
      resources.ApplyResources(this.splitContainerControl1.Panel2, "splitContainerControl1.Panel2");
      this.splitContainerControl1.Panel2.Controls.Add(this.grpInputList);
      this.splitContainerControl1.SplitterPosition = 343;
      // 
      // grpOutputList
      // 
      resources.ApplyResources(this.grpOutputList, "grpOutputList");
      this.grpOutputList.Controls.Add(this.gridLeft);
      this.grpOutputList.Controls.Add(this.lblHotkeyLeft);
      this.grpOutputList.Controls.Add(this.pnlEditControls);
      this.grpOutputList.Name = "grpOutputList";
      this.grpOutputList.Enter += new System.EventHandler(this.grpOutputList_Enter);
      // 
      // gridLeft
      // 
      resources.ApplyResources(this.gridLeft, "gridLeft");
      this.gridLeft.DataSource = this.dsChannels;
      this.gridLeft.EmbeddedNavigator.AccessibleDescription = resources.GetString("gridLeft.EmbeddedNavigator.AccessibleDescription");
      this.gridLeft.EmbeddedNavigator.AccessibleName = resources.GetString("gridLeft.EmbeddedNavigator.AccessibleName");
      this.gridLeft.EmbeddedNavigator.AllowHtmlTextInToolTip = ((DevExpress.Utils.DefaultBoolean)(resources.GetObject("gridLeft.EmbeddedNavigator.AllowHtmlTextInToolTip")));
      this.gridLeft.EmbeddedNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gridLeft.EmbeddedNavigator.Anchor")));
      this.gridLeft.EmbeddedNavigator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gridLeft.EmbeddedNavigator.BackgroundImage")));
      this.gridLeft.EmbeddedNavigator.BackgroundImageLayout = ((System.Windows.Forms.ImageLayout)(resources.GetObject("gridLeft.EmbeddedNavigator.BackgroundImageLayout")));
      this.gridLeft.EmbeddedNavigator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gridLeft.EmbeddedNavigator.ImeMode")));
      this.gridLeft.EmbeddedNavigator.TextLocation = ((DevExpress.XtraEditors.NavigatorButtonsTextLocation)(resources.GetObject("gridLeft.EmbeddedNavigator.TextLocation")));
      this.gridLeft.EmbeddedNavigator.ToolTip = resources.GetString("gridLeft.EmbeddedNavigator.ToolTip");
      this.gridLeft.EmbeddedNavigator.ToolTipIconType = ((DevExpress.Utils.ToolTipIconType)(resources.GetObject("gridLeft.EmbeddedNavigator.ToolTipIconType")));
      this.gridLeft.EmbeddedNavigator.ToolTipTitle = resources.GetString("gridLeft.EmbeddedNavigator.ToolTipTitle");
      this.gridLeft.MainView = this.gviewLeft;
      this.gridLeft.Name = "gridLeft";
      this.gridLeft.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckedComboBoxEdit1});
      this.gridLeft.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gviewLeft});
      this.gridLeft.ProcessGridKey += new System.Windows.Forms.KeyEventHandler(this.gridLeft_ProcessGridKey);
      // 
      // dsChannels
      // 
      this.dsChannels.DataSource = typeof(ChanSort.Api.ChannelInfo);
      // 
      // gviewLeft
      // 
      this.gviewLeft.Appearance.HeaderPanel.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("gviewLeft.Appearance.HeaderPanel.GradientMode")));
      this.gviewLeft.Appearance.HeaderPanel.Image = ((System.Drawing.Image)(resources.GetObject("gviewLeft.Appearance.HeaderPanel.Image")));
      this.gviewLeft.Appearance.HeaderPanel.Options.UseTextOptions = true;
      this.gviewLeft.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
      resources.ApplyResources(this.gviewLeft, "gviewLeft");
      this.gviewLeft.ColumnPanelRowHeight = 35;
      this.gviewLeft.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colIndex1,
            this.colOutSlot,
            this.colOutName,
            this.colOutFav,
            this.colUid1,
            this.colOutLock});
      this.gviewLeft.GridControl = this.gridLeft;
      this.gviewLeft.Name = "gviewLeft";
      this.gviewLeft.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
      this.gviewLeft.OptionsCustomization.AllowGroup = false;
      this.gviewLeft.OptionsLayout.LayoutVersion = "2";
      this.gviewLeft.OptionsSelection.MultiSelect = true;
      this.gviewLeft.OptionsView.ColumnAutoWidth = false;
      this.gviewLeft.OptionsView.ShowAutoFilterRow = true;
      this.gviewLeft.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
      this.gviewLeft.OptionsView.ShowGroupPanel = false;
      this.gviewLeft.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.colOutSlot, DevExpress.Data.ColumnSortOrder.Ascending)});
      this.gviewLeft.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gviewLeft_RowClick);
      this.gviewLeft.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gviewLeft_RowCellStyle);
      this.gviewLeft.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gviewLeft_PopupMenuShowing);
      this.gviewLeft.SelectionChanged += new DevExpress.Data.SelectionChangedEventHandler(this.gviewLeft_SelectionChanged);
      this.gviewLeft.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.gview_ShowingEditor);
      this.gviewLeft.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gviewLeft_FocusedRowChanged);
      this.gviewLeft.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gviewLeft_CellValueChanged);
      this.gviewLeft.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gviewLeft_CustomColumnDisplayText);
      this.gviewLeft.LayoutUpgrade += new DevExpress.Utils.LayoutUpgadeEventHandler(this.gviewLeft_LayoutUpgrade);
      this.gviewLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gview_MouseDown);
      this.gviewLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gview_MouseUp);
      this.gviewLeft.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gviewLeft_ValidatingEditor);
      // 
      // colIndex1
      // 
      resources.ApplyResources(this.colIndex1, "colIndex1");
      this.colIndex1.FieldName = "RecordIndex";
      this.colIndex1.Name = "colIndex1";
      // 
      // colOutSlot
      // 
      resources.ApplyResources(this.colOutSlot, "colOutSlot");
      this.colOutSlot.FieldName = "NewProgramNr";
      this.colOutSlot.Name = "colOutSlot";
      this.colOutSlot.OptionsFilter.AllowAutoFilter = false;
      this.colOutSlot.OptionsFilter.AllowFilterModeChanging = DevExpress.Utils.DefaultBoolean.False;
      // 
      // colOutName
      // 
      resources.ApplyResources(this.colOutName, "colOutName");
      this.colOutName.FieldName = "Name";
      this.colOutName.Name = "colOutName";
      this.colOutName.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Contains;
      // 
      // colOutFav
      // 
      resources.ApplyResources(this.colOutFav, "colOutFav");
      this.colOutFav.ColumnEdit = this.repositoryItemCheckedComboBoxEdit1;
      this.colOutFav.FieldName = "Favorites";
      this.colOutFav.Name = "colOutFav";
      this.colOutFav.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Contains;
      // 
      // repositoryItemCheckedComboBoxEdit1
      // 
      resources.ApplyResources(this.repositoryItemCheckedComboBoxEdit1, "repositoryItemCheckedComboBoxEdit1");
      this.repositoryItemCheckedComboBoxEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Buttons"))))});
      this.repositoryItemCheckedComboBoxEdit1.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F2);
      this.repositoryItemCheckedComboBoxEdit1.ForceUpdateEditValue = DevExpress.Utils.DefaultBoolean.True;
      this.repositoryItemCheckedComboBoxEdit1.Mask.AutoComplete = ((DevExpress.XtraEditors.Mask.AutoCompleteType)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.AutoComplete")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.BeepOnError = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.BeepOnError")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.EditMask = resources.GetString("repositoryItemCheckedComboBoxEdit1.Mask.EditMask");
      this.repositoryItemCheckedComboBoxEdit1.Mask.IgnoreMaskBlank = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.IgnoreMaskBlank")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.MaskType")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.PlaceHolder = ((char)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.PlaceHolder")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.SaveLiteral = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.SaveLiteral")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.ShowPlaceHolders = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.ShowPlaceHolders")));
      this.repositoryItemCheckedComboBoxEdit1.Mask.UseMaskAsDisplayFormat = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.UseMaskAsDisplayFormat")));
      this.repositoryItemCheckedComboBoxEdit1.Name = "repositoryItemCheckedComboBoxEdit1";
      this.repositoryItemCheckedComboBoxEdit1.PopupSizeable = false;
      this.repositoryItemCheckedComboBoxEdit1.SelectAllItemVisible = false;
      this.repositoryItemCheckedComboBoxEdit1.ShowButtons = false;
      this.repositoryItemCheckedComboBoxEdit1.ShowPopupCloseButton = false;
      this.repositoryItemCheckedComboBoxEdit1.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
      // 
      // colUid1
      // 
      resources.ApplyResources(this.colUid1, "colUid1");
      this.colUid1.FieldName = "Uid";
      this.colUid1.Name = "colUid1";
      this.colUid1.OptionsColumn.AllowEdit = false;
      // 
      // colOutLock
      // 
      resources.ApplyResources(this.colOutLock, "colOutLock");
      this.colOutLock.FieldName = "Lock";
      this.colOutLock.Name = "colOutLock";
      // 
      // lblHotkeyLeft
      // 
      resources.ApplyResources(this.lblHotkeyLeft, "lblHotkeyLeft");
      this.lblHotkeyLeft.Name = "lblHotkeyLeft";
      // 
      // pnlEditControls
      // 
      resources.ApplyResources(this.pnlEditControls, "pnlEditControls");
      this.pnlEditControls.Controls.Add(this.btnToggleLock);
      this.pnlEditControls.Controls.Add(this.btnToggleFavE);
      this.pnlEditControls.Controls.Add(this.btnToggleFavD);
      this.pnlEditControls.Controls.Add(this.btnToggleFavC);
      this.pnlEditControls.Controls.Add(this.btnToggleFavB);
      this.pnlEditControls.Controls.Add(this.btnToggleFavA);
      this.pnlEditControls.Controls.Add(this.btnClearLeftFilter);
      this.pnlEditControls.Controls.Add(this.btnRenum);
      this.pnlEditControls.Controls.Add(this.btnDown);
      this.pnlEditControls.Controls.Add(this.btnUp);
      this.pnlEditControls.Controls.Add(this.btnRemove);
      this.pnlEditControls.Name = "pnlEditControls";
      // 
      // btnToggleLock
      // 
      resources.ApplyResources(this.btnToggleLock, "btnToggleLock");
      this.btnToggleLock.ImageIndex = 15;
      this.btnToggleLock.ImageList = this.globalImageCollection1;
      this.btnToggleLock.Name = "btnToggleLock";
      this.btnToggleLock.Click += new System.EventHandler(this.btnToggleLock_Click);
      // 
      // globalImageCollection1
      // 
      this.globalImageCollection1.ParentControl = this;
      // 
      // btnToggleFavE
      // 
      resources.ApplyResources(this.btnToggleFavE, "btnToggleFavE");
      this.btnToggleFavE.ImageList = this.globalImageCollection1;
      this.btnToggleFavE.Name = "btnToggleFavE";
      this.btnToggleFavE.Tag = "";
      this.btnToggleFavE.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavD
      // 
      resources.ApplyResources(this.btnToggleFavD, "btnToggleFavD");
      this.btnToggleFavD.ImageList = this.globalImageCollection1;
      this.btnToggleFavD.Name = "btnToggleFavD";
      this.btnToggleFavD.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavC
      // 
      resources.ApplyResources(this.btnToggleFavC, "btnToggleFavC");
      this.btnToggleFavC.ImageList = this.globalImageCollection1;
      this.btnToggleFavC.Name = "btnToggleFavC";
      this.btnToggleFavC.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavB
      // 
      resources.ApplyResources(this.btnToggleFavB, "btnToggleFavB");
      this.btnToggleFavB.ImageList = this.globalImageCollection1;
      this.btnToggleFavB.Name = "btnToggleFavB";
      this.btnToggleFavB.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavA
      // 
      resources.ApplyResources(this.btnToggleFavA, "btnToggleFavA");
      this.btnToggleFavA.ImageList = this.globalImageCollection1;
      this.btnToggleFavA.Name = "btnToggleFavA";
      this.btnToggleFavA.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnClearLeftFilter
      // 
      resources.ApplyResources(this.btnClearLeftFilter, "btnClearLeftFilter");
      this.btnClearLeftFilter.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnClearLeftFilter.Appearance.Font")));
      this.btnClearLeftFilter.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("btnClearLeftFilter.Appearance.GradientMode")));
      this.btnClearLeftFilter.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("btnClearLeftFilter.Appearance.Image")));
      this.btnClearLeftFilter.Appearance.Options.UseFont = true;
      this.btnClearLeftFilter.ImageIndex = 28;
      this.btnClearLeftFilter.ImageList = this.globalImageCollection1;
      this.btnClearLeftFilter.Name = "btnClearLeftFilter";
      this.btnClearLeftFilter.Click += new System.EventHandler(this.btnClearLeftFilter_Click);
      // 
      // btnRenum
      // 
      resources.ApplyResources(this.btnRenum, "btnRenum");
      this.btnRenum.ImageIndex = 22;
      this.btnRenum.ImageList = this.globalImageCollection1;
      this.btnRenum.Name = "btnRenum";
      this.btnRenum.Click += new System.EventHandler(this.btnRenum_Click);
      // 
      // btnDown
      // 
      resources.ApplyResources(this.btnDown, "btnDown");
      this.btnDown.ImageIndex = 25;
      this.btnDown.ImageList = this.globalImageCollection1;
      this.btnDown.Name = "btnDown";
      this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
      // 
      // btnUp
      // 
      resources.ApplyResources(this.btnUp, "btnUp");
      this.btnUp.ImageIndex = 24;
      this.btnUp.ImageList = this.globalImageCollection1;
      this.btnUp.Name = "btnUp";
      this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
      // 
      // btnRemove
      // 
      resources.ApplyResources(this.btnRemove, "btnRemove");
      this.btnRemove.ImageIndex = 11;
      this.btnRemove.ImageList = this.globalImageCollection1;
      this.btnRemove.Name = "btnRemove";
      this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
      // 
      // grpInputList
      // 
      resources.ApplyResources(this.grpInputList, "grpInputList");
      this.grpInputList.Controls.Add(this.gridRight);
      this.grpInputList.Controls.Add(this.lblHotkeyRight);
      this.grpInputList.Controls.Add(this.panelControl3);
      this.grpInputList.Name = "grpInputList";
      this.grpInputList.Enter += new System.EventHandler(this.grpInputList_Enter);
      // 
      // gridRight
      // 
      resources.ApplyResources(this.gridRight, "gridRight");
      this.gridRight.DataSource = this.dsChannels;
      this.gridRight.EmbeddedNavigator.AccessibleDescription = resources.GetString("gridRight.EmbeddedNavigator.AccessibleDescription");
      this.gridRight.EmbeddedNavigator.AccessibleName = resources.GetString("gridRight.EmbeddedNavigator.AccessibleName");
      this.gridRight.EmbeddedNavigator.AllowHtmlTextInToolTip = ((DevExpress.Utils.DefaultBoolean)(resources.GetObject("gridRight.EmbeddedNavigator.AllowHtmlTextInToolTip")));
      this.gridRight.EmbeddedNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gridRight.EmbeddedNavigator.Anchor")));
      this.gridRight.EmbeddedNavigator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gridRight.EmbeddedNavigator.BackgroundImage")));
      this.gridRight.EmbeddedNavigator.BackgroundImageLayout = ((System.Windows.Forms.ImageLayout)(resources.GetObject("gridRight.EmbeddedNavigator.BackgroundImageLayout")));
      this.gridRight.EmbeddedNavigator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gridRight.EmbeddedNavigator.ImeMode")));
      this.gridRight.EmbeddedNavigator.TextLocation = ((DevExpress.XtraEditors.NavigatorButtonsTextLocation)(resources.GetObject("gridRight.EmbeddedNavigator.TextLocation")));
      this.gridRight.EmbeddedNavigator.ToolTip = resources.GetString("gridRight.EmbeddedNavigator.ToolTip");
      this.gridRight.EmbeddedNavigator.ToolTipIconType = ((DevExpress.Utils.ToolTipIconType)(resources.GetObject("gridRight.EmbeddedNavigator.ToolTipIconType")));
      this.gridRight.EmbeddedNavigator.ToolTipTitle = resources.GetString("gridRight.EmbeddedNavigator.ToolTipTitle");
      this.gridRight.MainView = this.gviewRight;
      this.gridRight.Name = "gridRight";
      this.gridRight.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckedComboBoxEdit2});
      this.gridRight.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gviewRight});
      this.gridRight.ProcessGridKey += new System.Windows.Forms.KeyEventHandler(this.gridRight_ProcessGridKey);
      // 
      // gviewRight
      // 
      this.gviewRight.Appearance.HeaderPanel.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("gviewRight.Appearance.HeaderPanel.GradientMode")));
      this.gviewRight.Appearance.HeaderPanel.Image = ((System.Drawing.Image)(resources.GetObject("gviewRight.Appearance.HeaderPanel.Image")));
      this.gviewRight.Appearance.HeaderPanel.Options.UseTextOptions = true;
      this.gviewRight.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
      resources.ApplyResources(this.gviewRight, "gviewRight");
      this.gviewRight.ColumnPanelRowHeight = 35;
      this.gviewRight.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colIndex,
            this.colSlotOld,
            this.colSlotNew,
            this.colName,
            this.colShortName,
            this.colFavorites,
            this.colLock,
            this.colSkip,
            this.colHidden,
            this.colEncrypted,
            this.colChannelOrTransponder,
            this.colFreqInMhz,
            this.colServiceId,
            this.colVideoPid,
            this.colAudioPid,
            this.colServiceType,
            this.colServiceTypeName,
            this.colSatellite,
            this.colNetworkId,
            this.colTransportStreamId,
            this.colSymbolRate,
            this.colPolarity,
            this.colUid,
            this.colNetworkName,
            this.colNetworkOperator,
            this.colDebug,
            this.colLogicalIndex});
      this.gviewRight.GridControl = this.gridRight;
      this.gviewRight.Name = "gviewRight";
      this.gviewRight.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
      this.gviewRight.OptionsCustomization.AllowGroup = false;
      this.gviewRight.OptionsLayout.LayoutVersion = "3";
      this.gviewRight.OptionsSelection.MultiSelect = true;
      this.gviewRight.OptionsView.ColumnAutoWidth = false;
      this.gviewRight.OptionsView.ShowAutoFilterRow = true;
      this.gviewRight.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
      this.gviewRight.OptionsView.ShowGroupPanel = false;
      this.gviewRight.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.colSlotOld, DevExpress.Data.ColumnSortOrder.Ascending)});
      this.gviewRight.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gviewRight_RowClick);
      this.gviewRight.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gviewRight_RowCellStyle);
      this.gviewRight.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gviewRight_PopupMenuShowing);
      this.gviewRight.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.gview_ShowingEditor);
      this.gviewRight.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gviewRight_FocusedRowChanged);
      this.gviewRight.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gviewRight_CellValueChanged);
      this.gviewRight.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gviewRight_CustomColumnDisplayText);
      this.gviewRight.LayoutUpgrade += new DevExpress.Utils.LayoutUpgadeEventHandler(this.gviewRight_LayoutUpgrade);
      this.gviewRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gview_MouseDown);
      this.gviewRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gview_MouseUp);
      this.gviewRight.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gviewRight_ValidatingEditor);
      // 
      // colIndex
      // 
      resources.ApplyResources(this.colIndex, "colIndex");
      this.colIndex.FieldName = "RecordIndex";
      this.colIndex.Name = "colIndex";
      this.colIndex.OptionsColumn.AllowEdit = false;
      // 
      // colSlotOld
      // 
      resources.ApplyResources(this.colSlotOld, "colSlotOld");
      this.colSlotOld.FieldName = "OldProgramNr";
      this.colSlotOld.Name = "colSlotOld";
      this.colSlotOld.OptionsColumn.AllowEdit = false;
      this.colSlotOld.OptionsFilter.AllowAutoFilter = false;
      // 
      // colSlotNew
      // 
      resources.ApplyResources(this.colSlotNew, "colSlotNew");
      this.colSlotNew.FieldName = "NewProgramNr";
      this.colSlotNew.Name = "colSlotNew";
      // 
      // colName
      // 
      resources.ApplyResources(this.colName, "colName");
      this.colName.FieldName = "Name";
      this.colName.Name = "colName";
      this.colName.OptionsColumn.AllowEdit = false;
      this.colName.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Contains;
      // 
      // colShortName
      // 
      resources.ApplyResources(this.colShortName, "colShortName");
      this.colShortName.FieldName = "ShortName";
      this.colShortName.Name = "colShortName";
      this.colShortName.OptionsColumn.AllowEdit = false;
      // 
      // colFavorites
      // 
      resources.ApplyResources(this.colFavorites, "colFavorites");
      this.colFavorites.ColumnEdit = this.repositoryItemCheckedComboBoxEdit2;
      this.colFavorites.FieldName = "Favorites";
      this.colFavorites.Name = "colFavorites";
      // 
      // repositoryItemCheckedComboBoxEdit2
      // 
      resources.ApplyResources(this.repositoryItemCheckedComboBoxEdit2, "repositoryItemCheckedComboBoxEdit2");
      this.repositoryItemCheckedComboBoxEdit2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Buttons"))))});
      this.repositoryItemCheckedComboBoxEdit2.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F2);
      this.repositoryItemCheckedComboBoxEdit2.ForceUpdateEditValue = DevExpress.Utils.DefaultBoolean.True;
      this.repositoryItemCheckedComboBoxEdit2.Mask.AutoComplete = ((DevExpress.XtraEditors.Mask.AutoCompleteType)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.AutoComplete")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.BeepOnError = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.BeepOnError")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.EditMask = resources.GetString("repositoryItemCheckedComboBoxEdit2.Mask.EditMask");
      this.repositoryItemCheckedComboBoxEdit2.Mask.IgnoreMaskBlank = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.IgnoreMaskBlank")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.MaskType")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.PlaceHolder = ((char)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.PlaceHolder")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.SaveLiteral = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.SaveLiteral")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.ShowPlaceHolders = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.ShowPlaceHolders")));
      this.repositoryItemCheckedComboBoxEdit2.Mask.UseMaskAsDisplayFormat = ((bool)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.UseMaskAsDisplayFormat")));
      this.repositoryItemCheckedComboBoxEdit2.Name = "repositoryItemCheckedComboBoxEdit2";
      this.repositoryItemCheckedComboBoxEdit2.PopupSizeable = false;
      this.repositoryItemCheckedComboBoxEdit2.SelectAllItemVisible = false;
      this.repositoryItemCheckedComboBoxEdit2.ShowButtons = false;
      this.repositoryItemCheckedComboBoxEdit2.ShowPopupCloseButton = false;
      this.repositoryItemCheckedComboBoxEdit2.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
      // 
      // colLock
      // 
      resources.ApplyResources(this.colLock, "colLock");
      this.colLock.FieldName = "Lock";
      this.colLock.Name = "colLock";
      // 
      // colSkip
      // 
      resources.ApplyResources(this.colSkip, "colSkip");
      this.colSkip.FieldName = "Skip";
      this.colSkip.Name = "colSkip";
      // 
      // colHidden
      // 
      resources.ApplyResources(this.colHidden, "colHidden");
      this.colHidden.FieldName = "Hidden";
      this.colHidden.Name = "colHidden";
      // 
      // colEncrypted
      // 
      resources.ApplyResources(this.colEncrypted, "colEncrypted");
      this.colEncrypted.FieldName = "Encrypted";
      this.colEncrypted.Name = "colEncrypted";
      this.colEncrypted.OptionsColumn.AllowEdit = false;
      this.colEncrypted.OptionsColumn.FixedWidth = true;
      // 
      // colChannelOrTransponder
      // 
      resources.ApplyResources(this.colChannelOrTransponder, "colChannelOrTransponder");
      this.colChannelOrTransponder.FieldName = "ChannelOrTransponder";
      this.colChannelOrTransponder.Name = "colChannelOrTransponder";
      this.colChannelOrTransponder.OptionsColumn.AllowEdit = false;
      // 
      // colFreqInMhz
      // 
      resources.ApplyResources(this.colFreqInMhz, "colFreqInMhz");
      this.colFreqInMhz.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colFreqInMhz.FieldName = "FreqInMhz";
      this.colFreqInMhz.Name = "colFreqInMhz";
      this.colFreqInMhz.OptionsColumn.AllowEdit = false;
      // 
      // colServiceId
      // 
      resources.ApplyResources(this.colServiceId, "colServiceId");
      this.colServiceId.FieldName = "ServiceId";
      this.colServiceId.Name = "colServiceId";
      this.colServiceId.OptionsColumn.AllowEdit = false;
      // 
      // colVideoPid
      // 
      resources.ApplyResources(this.colVideoPid, "colVideoPid");
      this.colVideoPid.FieldName = "VideoPid";
      this.colVideoPid.Name = "colVideoPid";
      this.colVideoPid.OptionsColumn.AllowEdit = false;
      // 
      // colAudioPid
      // 
      resources.ApplyResources(this.colAudioPid, "colAudioPid");
      this.colAudioPid.FieldName = "AudioPid";
      this.colAudioPid.Name = "colAudioPid";
      this.colAudioPid.OptionsColumn.AllowEdit = false;
      // 
      // colServiceType
      // 
      resources.ApplyResources(this.colServiceType, "colServiceType");
      this.colServiceType.FieldName = "ServiceType";
      this.colServiceType.Name = "colServiceType";
      this.colServiceType.OptionsColumn.AllowEdit = false;
      // 
      // colServiceTypeName
      // 
      resources.ApplyResources(this.colServiceTypeName, "colServiceTypeName");
      this.colServiceTypeName.FieldName = "ServiceTypeName";
      this.colServiceTypeName.Name = "colServiceTypeName";
      this.colServiceTypeName.OptionsColumn.AllowEdit = false;
      // 
      // colSatellite
      // 
      resources.ApplyResources(this.colSatellite, "colSatellite");
      this.colSatellite.FieldName = "Satellite";
      this.colSatellite.Name = "colSatellite";
      this.colSatellite.OptionsColumn.AllowEdit = false;
      // 
      // colNetworkId
      // 
      resources.ApplyResources(this.colNetworkId, "colNetworkId");
      this.colNetworkId.FieldName = "OriginalNetworkId";
      this.colNetworkId.Name = "colNetworkId";
      this.colNetworkId.OptionsColumn.AllowEdit = false;
      // 
      // colTransportStreamId
      // 
      resources.ApplyResources(this.colTransportStreamId, "colTransportStreamId");
      this.colTransportStreamId.FieldName = "TransportStreamId";
      this.colTransportStreamId.Name = "colTransportStreamId";
      this.colTransportStreamId.OptionsColumn.AllowEdit = false;
      // 
      // colSymbolRate
      // 
      resources.ApplyResources(this.colSymbolRate, "colSymbolRate");
      this.colSymbolRate.FieldName = "SymbolRate";
      this.colSymbolRate.Name = "colSymbolRate";
      this.colSymbolRate.OptionsColumn.AllowEdit = false;
      // 
      // colPolarity
      // 
      resources.ApplyResources(this.colPolarity, "colPolarity");
      this.colPolarity.FieldName = "Polarity";
      this.colPolarity.Name = "colPolarity";
      this.colPolarity.OptionsColumn.AllowEdit = false;
      // 
      // colUid
      // 
      resources.ApplyResources(this.colUid, "colUid");
      this.colUid.FieldName = "Uid";
      this.colUid.Name = "colUid";
      this.colUid.OptionsColumn.AllowEdit = false;
      // 
      // colNetworkName
      // 
      resources.ApplyResources(this.colNetworkName, "colNetworkName");
      this.colNetworkName.FieldName = "NetworkName";
      this.colNetworkName.Name = "colNetworkName";
      this.colNetworkName.OptionsColumn.AllowEdit = false;
      // 
      // colNetworkOperator
      // 
      resources.ApplyResources(this.colNetworkOperator, "colNetworkOperator");
      this.colNetworkOperator.FieldName = "NetworkOperator";
      this.colNetworkOperator.Name = "colNetworkOperator";
      this.colNetworkOperator.OptionsColumn.AllowEdit = false;
      // 
      // colDebug
      // 
      resources.ApplyResources(this.colDebug, "colDebug");
      this.colDebug.FieldName = "Debug";
      this.colDebug.Name = "colDebug";
      this.colDebug.OptionsColumn.AllowEdit = false;
      // 
      // colLogicalIndex
      // 
      resources.ApplyResources(this.colLogicalIndex, "colLogicalIndex");
      this.colLogicalIndex.FieldName = "RecordOrder";
      this.colLogicalIndex.Name = "colLogicalIndex";
      this.colLogicalIndex.OptionsColumn.AllowEdit = false;
      this.colLogicalIndex.OptionsColumn.ReadOnly = true;
      // 
      // lblHotkeyRight
      // 
      resources.ApplyResources(this.lblHotkeyRight, "lblHotkeyRight");
      this.lblHotkeyRight.Name = "lblHotkeyRight";
      // 
      // panelControl3
      // 
      resources.ApplyResources(this.panelControl3, "panelControl3");
      this.panelControl3.Controls.Add(this.btnAddAll);
      this.panelControl3.Controls.Add(this.btnClearRightFilter);
      this.panelControl3.Controls.Add(this.btnAdd);
      this.panelControl3.Name = "panelControl3";
      // 
      // btnAddAll
      // 
      resources.ApplyResources(this.btnAddAll, "btnAddAll");
      this.btnAddAll.Name = "btnAddAll";
      this.btnAddAll.Click += new System.EventHandler(this.btnAddAll_Click);
      // 
      // btnClearRightFilter
      // 
      resources.ApplyResources(this.btnClearRightFilter, "btnClearRightFilter");
      this.btnClearRightFilter.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnClearRightFilter.Appearance.Font")));
      this.btnClearRightFilter.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("btnClearRightFilter.Appearance.GradientMode")));
      this.btnClearRightFilter.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("btnClearRightFilter.Appearance.Image")));
      this.btnClearRightFilter.Appearance.Options.UseFont = true;
      this.btnClearRightFilter.ImageIndex = 28;
      this.btnClearRightFilter.ImageList = this.globalImageCollection1;
      this.btnClearRightFilter.Name = "btnClearRightFilter";
      this.btnClearRightFilter.Click += new System.EventHandler(this.btnClearRightFilter_Click);
      // 
      // btnAdd
      // 
      resources.ApplyResources(this.btnAdd, "btnAdd");
      this.btnAdd.ImageIndex = 26;
      this.btnAdd.ImageList = this.globalImageCollection1;
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // barManager1
      // 
      this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1});
      this.barManager1.Categories.AddRange(new DevExpress.XtraBars.BarManagerCategory[] {
            ((DevExpress.XtraBars.BarManagerCategory)(resources.GetObject("barManager1.Categories"))),
            ((DevExpress.XtraBars.BarManagerCategory)(resources.GetObject("barManager1.Categories1"))),
            ((DevExpress.XtraBars.BarManagerCategory)(resources.GetObject("barManager1.Categories2"))),
            ((DevExpress.XtraBars.BarManagerCategory)(resources.GetObject("barManager1.Categories3")))});
      this.barManager1.DockControls.Add(this.barDockControlTop);
      this.barManager1.DockControls.Add(this.barDockControlBottom);
      this.barManager1.DockControls.Add(this.barDockControlLeft);
      this.barManager1.DockControls.Add(this.barDockControlRight);
      this.barManager1.Form = this;
      this.barManager1.Images = this.globalImageCollection1;
      this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.mnuOptions,
            this.barSubItem1,
            this.miEnglish,
            this.miGerman,
            this.miFile,
            this.miOpen,
            this.miOpenReferenceFile,
            this.miReload,
            this.miRestoreOriginal,
            this.miFileInformation,
            this.miSave,
            this.miSaveAs,
            this.miQuit,
            this.mnuHelp,
            this.miAbout,
            this.mnuCharset,
            this.miIsoCharSets,
            this.miCharsetForm,
            this.miEdit,
            this.miMoveUp,
            this.miMoveDown,
            this.miAddChannel,
            this.miRemove,
            this.miSort,
            this.miRenum,
            this.mnuFavSet,
            this.miFavSet,
            this.mnuFavUnset,
            this.miFavUnset,
            this.miLockOn,
            this.miLockOff,
            this.miSkipOn,
            this.miSkipOff,
            this.miHideOn,
            this.miHideOff,
            this.barSubItem2,
            this.miTvSettings,
            this.miEraseChannelData,
            this.miOpenWebsite,
            this.miWiki});
      this.barManager1.MaxItemId = 52;
      // 
      // bar1
      // 
      this.bar1.BarName = "Tools";
      this.bar1.DockCol = 0;
      this.bar1.DockRow = 0;
      this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
      this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miFile),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpen),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpenReferenceFile),
            new DevExpress.XtraBars.LinkPersistInfo(this.miReload),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSave),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSaveAs),
            new DevExpress.XtraBars.LinkPersistInfo(this.miEdit, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavSet),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem2, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miTvSettings),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuOptions, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuHelp, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAbout)});
      resources.ApplyResources(this.bar1, "bar1");
      // 
      // miFile
      // 
      resources.ApplyResources(this.miFile, "miFile");
      this.miFile.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miFile.Id = 4;
      this.miFile.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpen),
            new DevExpress.XtraBars.LinkPersistInfo(this.miReload),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRestoreOriginal),
            new DevExpress.XtraBars.LinkPersistInfo(this.miFileInformation),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSave),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSaveAs),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpenReferenceFile, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miQuit, true)});
      this.miFile.Name = "miFile";
      // 
      // miOpen
      // 
      resources.ApplyResources(this.miOpen, "miOpen");
      this.miOpen.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miOpen.Id = 5;
      this.miOpen.ImageIndex = 3;
      this.miOpen.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O));
      this.miOpen.Name = "miOpen";
      this.miOpen.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miOpen_ItemClick);
      // 
      // miReload
      // 
      resources.ApplyResources(this.miReload, "miReload");
      this.miReload.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miReload.Enabled = false;
      this.miReload.Id = 7;
      this.miReload.ImageIndex = 5;
      this.miReload.Name = "miReload";
      this.miReload.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miReload_ItemClick);
      // 
      // miRestoreOriginal
      // 
      resources.ApplyResources(this.miRestoreOriginal, "miRestoreOriginal");
      this.miRestoreOriginal.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miRestoreOriginal.Id = 42;
      this.miRestoreOriginal.Name = "miRestoreOriginal";
      this.miRestoreOriginal.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRestoreOriginal_ItemClick);
      // 
      // miFileInformation
      // 
      resources.ApplyResources(this.miFileInformation, "miFileInformation");
      this.miFileInformation.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miFileInformation.Id = 43;
      this.miFileInformation.Name = "miFileInformation";
      this.miFileInformation.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miFileInformation_ItemClick);
      // 
      // miSave
      // 
      resources.ApplyResources(this.miSave, "miSave");
      this.miSave.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miSave.Enabled = false;
      this.miSave.Id = 6;
      this.miSave.ImageIndex = 4;
      this.miSave.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S));
      this.miSave.Name = "miSave";
      this.miSave.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSave_ItemClick);
      // 
      // miSaveAs
      // 
      resources.ApplyResources(this.miSaveAs, "miSaveAs");
      this.miSaveAs.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miSaveAs.Enabled = false;
      this.miSaveAs.Id = 8;
      this.miSaveAs.ImageIndex = 6;
      this.miSaveAs.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.S));
      this.miSaveAs.Name = "miSaveAs";
      this.miSaveAs.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSaveAs_ItemClick);
      // 
      // miOpenReferenceFile
      // 
      resources.ApplyResources(this.miOpenReferenceFile, "miOpenReferenceFile");
      this.miOpenReferenceFile.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miOpenReferenceFile.Id = 44;
      this.miOpenReferenceFile.ImageIndex = 23;
      this.miOpenReferenceFile.Name = "miOpenReferenceFile";
      this.miOpenReferenceFile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miOpenReferenceFile_ItemClick);
      // 
      // miQuit
      // 
      resources.ApplyResources(this.miQuit, "miQuit");
      this.miQuit.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miQuit.Id = 9;
      this.miQuit.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4));
      this.miQuit.Name = "miQuit";
      this.miQuit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miQuit_ItemClick);
      // 
      // miEdit
      // 
      resources.ApplyResources(this.miEdit, "miEdit");
      this.miEdit.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miEdit.Id = 22;
      this.miEdit.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miAddChannel),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRemove),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSort),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenum),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.mnuFavSet, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavUnset),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOn, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOn, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOn, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOff)});
      this.miEdit.Name = "miEdit";
      // 
      // miAddChannel
      // 
      resources.ApplyResources(this.miAddChannel, "miAddChannel");
      this.miAddChannel.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miAddChannel.Id = 39;
      this.miAddChannel.ImageIndex = 26;
      this.miAddChannel.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Add));
      this.miAddChannel.Name = "miAddChannel";
      // 
      // miRemove
      // 
      resources.ApplyResources(this.miRemove, "miRemove");
      this.miRemove.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miRemove.Id = 25;
      this.miRemove.ImageIndex = 11;
      this.miRemove.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X));
      this.miRemove.Name = "miRemove";
      this.miRemove.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRemove_ItemClick);
      // 
      // miSort
      // 
      resources.ApplyResources(this.miSort, "miSort");
      this.miSort.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miSort.Id = 40;
      this.miSort.ImageIndex = 21;
      this.miSort.Name = "miSort";
      this.miSort.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSort_ItemClick);
      // 
      // miRenum
      // 
      resources.ApplyResources(this.miRenum, "miRenum");
      this.miRenum.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miRenum.Id = 41;
      this.miRenum.ImageIndex = 22;
      this.miRenum.Name = "miRenum";
      this.miRenum.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRenum_ItemClick);
      // 
      // mnuFavSet
      // 
      resources.ApplyResources(this.mnuFavSet, "mnuFavSet");
      this.mnuFavSet.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.mnuFavSet.Id = 37;
      this.mnuFavSet.ImageIndex = 13;
      this.mnuFavSet.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.F));
      this.mnuFavSet.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miFavSet)});
      this.mnuFavSet.Name = "mnuFavSet";
      this.mnuFavSet.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu;
      // 
      // miFavSet
      // 
      resources.ApplyResources(this.miFavSet, "miFavSet");
      this.miFavSet.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miFavSet.Id = 20;
      this.miFavSet.Name = "miFavSet";
      this.miFavSet.ShowNumbers = true;
      this.miFavSet.Strings.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D"});
      this.miFavSet.ListItemClick += new DevExpress.XtraBars.ListItemClickEventHandler(this.miFavSet_ListItemClick);
      // 
      // mnuFavUnset
      // 
      resources.ApplyResources(this.mnuFavUnset, "mnuFavUnset");
      this.mnuFavUnset.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.mnuFavUnset.Id = 38;
      this.mnuFavUnset.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miFavUnset)});
      this.mnuFavUnset.Name = "mnuFavUnset";
      // 
      // miFavUnset
      // 
      resources.ApplyResources(this.miFavUnset, "miFavUnset");
      this.miFavUnset.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miFavUnset.Id = 21;
      this.miFavUnset.Name = "miFavUnset";
      this.miFavUnset.Strings.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D"});
      this.miFavUnset.ListItemClick += new DevExpress.XtraBars.ListItemClickEventHandler(this.miFavUnset_ListItemClick);
      // 
      // miLockOn
      // 
      resources.ApplyResources(this.miLockOn, "miLockOn");
      this.miLockOn.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miLockOn.Id = 27;
      this.miLockOn.ImageIndex = 15;
      this.miLockOn.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L));
      this.miLockOn.Name = "miLockOn";
      this.miLockOn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miLockOn_ItemClick);
      // 
      // miLockOff
      // 
      resources.ApplyResources(this.miLockOff, "miLockOff");
      this.miLockOff.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miLockOff.Id = 28;
      this.miLockOff.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.L));
      this.miLockOff.Name = "miLockOff";
      this.miLockOff.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miLockOff_ItemClick);
      // 
      // miSkipOn
      // 
      resources.ApplyResources(this.miSkipOn, "miSkipOn");
      this.miSkipOn.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miSkipOn.Id = 29;
      this.miSkipOn.ImageIndex = 16;
      this.miSkipOn.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K));
      this.miSkipOn.Name = "miSkipOn";
      this.miSkipOn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSkipOn_ItemClick);
      // 
      // miSkipOff
      // 
      resources.ApplyResources(this.miSkipOff, "miSkipOff");
      this.miSkipOff.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miSkipOff.Id = 30;
      this.miSkipOff.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.K));
      this.miSkipOff.Name = "miSkipOff";
      this.miSkipOff.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSkipOff_ItemClick);
      // 
      // miHideOn
      // 
      resources.ApplyResources(this.miHideOn, "miHideOn");
      this.miHideOn.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miHideOn.Id = 31;
      this.miHideOn.ImageIndex = 17;
      this.miHideOn.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H));
      this.miHideOn.Name = "miHideOn";
      this.miHideOn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miHideOn_ItemClick);
      // 
      // miHideOff
      // 
      resources.ApplyResources(this.miHideOff, "miHideOff");
      this.miHideOff.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miHideOff.Id = 32;
      this.miHideOff.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.H));
      this.miHideOff.Name = "miHideOff";
      this.miHideOff.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miHideOff_ItemClick);
      // 
      // barSubItem2
      // 
      resources.ApplyResources(this.barSubItem2, "barSubItem2");
      this.barSubItem2.Id = 47;
      this.barSubItem2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miTvSettings),
            new DevExpress.XtraBars.LinkPersistInfo(this.miEraseChannelData)});
      this.barSubItem2.Name = "barSubItem2";
      // 
      // miTvSettings
      // 
      resources.ApplyResources(this.miTvSettings, "miTvSettings");
      this.miTvSettings.Id = 48;
      this.miTvSettings.ImageIndex = 27;
      this.miTvSettings.Name = "miTvSettings";
      this.miTvSettings.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miTvCountrySetup_ItemClick);
      // 
      // miEraseChannelData
      // 
      resources.ApplyResources(this.miEraseChannelData, "miEraseChannelData");
      this.miEraseChannelData.Id = 49;
      this.miEraseChannelData.Name = "miEraseChannelData";
      this.miEraseChannelData.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miEraseChannelData_ItemClick);
      // 
      // mnuOptions
      // 
      resources.ApplyResources(this.mnuOptions, "mnuOptions");
      this.mnuOptions.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.mnuOptions.Id = 34;
      this.mnuOptions.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItem1, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.mnuCharset, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
      this.mnuOptions.Name = "mnuOptions";
      // 
      // barSubItem1
      // 
      resources.ApplyResources(this.barSubItem1, "barSubItem1");
      this.barSubItem1.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.barSubItem1.Id = 0;
      this.barSubItem1.ImageIndex = 14;
      this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miEnglish),
            new DevExpress.XtraBars.LinkPersistInfo(this.miGerman)});
      this.barSubItem1.Name = "barSubItem1";
      this.barSubItem1.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu;
      // 
      // miEnglish
      // 
      resources.ApplyResources(this.miEnglish, "miEnglish");
      this.miEnglish.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      this.miEnglish.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miEnglish.Id = 2;
      this.miEnglish.ImageIndex = 0;
      this.miEnglish.Name = "miEnglish";
      this.miEnglish.Tag = "en";
      this.miEnglish.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miGerman
      // 
      resources.ApplyResources(this.miGerman, "miGerman");
      this.miGerman.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      this.miGerman.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miGerman.Id = 1;
      this.miGerman.ImageIndex = 1;
      this.miGerman.Name = "miGerman";
      this.miGerman.Tag = "de-DE";
      this.miGerman.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // mnuCharset
      // 
      resources.ApplyResources(this.mnuCharset, "mnuCharset");
      this.mnuCharset.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.mnuCharset.Id = 15;
      this.mnuCharset.ImageIndex = 9;
      this.mnuCharset.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miCharsetForm),
            new DevExpress.XtraBars.LinkPersistInfo(this.miIsoCharSets, true)});
      this.mnuCharset.Name = "mnuCharset";
      this.mnuCharset.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu;
      // 
      // miCharsetForm
      // 
      resources.ApplyResources(this.miCharsetForm, "miCharsetForm");
      this.miCharsetForm.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miCharsetForm.Id = 13;
      this.miCharsetForm.ImageIndex = 9;
      this.miCharsetForm.Name = "miCharsetForm";
      this.miCharsetForm.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miCharset_ItemClick);
      // 
      // miIsoCharSets
      // 
      resources.ApplyResources(this.miIsoCharSets, "miIsoCharSets");
      this.miIsoCharSets.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miIsoCharSets.Id = 14;
      this.miIsoCharSets.Name = "miIsoCharSets";
      this.miIsoCharSets.ShowNumbers = true;
      this.miIsoCharSets.ListItemClick += new DevExpress.XtraBars.ListItemClickEventHandler(this.miIsoCharSets_ListItemClick);
      // 
      // mnuHelp
      // 
      resources.ApplyResources(this.mnuHelp, "mnuHelp");
      this.mnuHelp.CategoryGuid = new System.Guid("0d554574-30e8-4d31-9a70-da702a984260");
      this.mnuHelp.Id = 10;
      this.mnuHelp.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miWiki),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpenWebsite),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAbout)});
      this.mnuHelp.Name = "mnuHelp";
      // 
      // miWiki
      // 
      resources.ApplyResources(this.miWiki, "miWiki");
      this.miWiki.Id = 51;
      this.miWiki.Name = "miWiki";
      this.miWiki.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miWiki_ItemClick);
      // 
      // miOpenWebsite
      // 
      resources.ApplyResources(this.miOpenWebsite, "miOpenWebsite");
      this.miOpenWebsite.Id = 50;
      this.miOpenWebsite.Name = "miOpenWebsite";
      this.miOpenWebsite.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miOpenWebsite_ItemClick);
      // 
      // miAbout
      // 
      resources.ApplyResources(this.miAbout, "miAbout");
      this.miAbout.CategoryGuid = new System.Guid("0d554574-30e8-4d31-9a70-da702a984260");
      this.miAbout.Id = 11;
      this.miAbout.ImageIndex = 7;
      this.miAbout.Name = "miAbout";
      this.miAbout.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miAbout_ItemClick);
      // 
      // barDockControlTop
      // 
      resources.ApplyResources(this.barDockControlTop, "barDockControlTop");
      this.barDockControlTop.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("barDockControlTop.Appearance.GradientMode")));
      this.barDockControlTop.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("barDockControlTop.Appearance.Image")));
      this.barDockControlTop.CausesValidation = false;
      // 
      // barDockControlBottom
      // 
      resources.ApplyResources(this.barDockControlBottom, "barDockControlBottom");
      this.barDockControlBottom.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("barDockControlBottom.Appearance.GradientMode")));
      this.barDockControlBottom.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("barDockControlBottom.Appearance.Image")));
      this.barDockControlBottom.CausesValidation = false;
      // 
      // barDockControlLeft
      // 
      resources.ApplyResources(this.barDockControlLeft, "barDockControlLeft");
      this.barDockControlLeft.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("barDockControlLeft.Appearance.GradientMode")));
      this.barDockControlLeft.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("barDockControlLeft.Appearance.Image")));
      this.barDockControlLeft.CausesValidation = false;
      // 
      // barDockControlRight
      // 
      resources.ApplyResources(this.barDockControlRight, "barDockControlRight");
      this.barDockControlRight.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("barDockControlRight.Appearance.GradientMode")));
      this.barDockControlRight.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("barDockControlRight.Appearance.Image")));
      this.barDockControlRight.CausesValidation = false;
      // 
      // miMoveUp
      // 
      resources.ApplyResources(this.miMoveUp, "miMoveUp");
      this.miMoveUp.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miMoveUp.Id = 45;
      this.miMoveUp.ImageIndex = 24;
      this.miMoveUp.Name = "miMoveUp";
      this.miMoveUp.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miMoveUp_ItemClick);
      // 
      // miMoveDown
      // 
      resources.ApplyResources(this.miMoveDown, "miMoveDown");
      this.miMoveDown.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miMoveDown.Id = 46;
      this.miMoveDown.ImageIndex = 25;
      this.miMoveDown.Name = "miMoveDown";
      this.miMoveDown.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miMoveDown_ItemClick);
      // 
      // labelControl2
      // 
      resources.ApplyResources(this.labelControl2, "labelControl2");
      this.labelControl2.Name = "labelControl2";
      // 
      // txtSetSlot
      // 
      resources.ApplyResources(this.txtSetSlot, "txtSetSlot");
      this.txtSetSlot.Name = "txtSetSlot";
      this.txtSetSlot.Properties.AccessibleDescription = resources.GetString("txtSetSlot.Properties.AccessibleDescription");
      this.txtSetSlot.Properties.AccessibleName = resources.GetString("txtSetSlot.Properties.AccessibleName");
      this.txtSetSlot.Properties.AutoHeight = ((bool)(resources.GetObject("txtSetSlot.Properties.AutoHeight")));
      this.txtSetSlot.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("txtSetSlot.Properties.Buttons"))))});
      this.txtSetSlot.Properties.Mask.AutoComplete = ((DevExpress.XtraEditors.Mask.AutoCompleteType)(resources.GetObject("txtSetSlot.Properties.Mask.AutoComplete")));
      this.txtSetSlot.Properties.Mask.BeepOnError = ((bool)(resources.GetObject("txtSetSlot.Properties.Mask.BeepOnError")));
      this.txtSetSlot.Properties.Mask.EditMask = resources.GetString("txtSetSlot.Properties.Mask.EditMask");
      this.txtSetSlot.Properties.Mask.IgnoreMaskBlank = ((bool)(resources.GetObject("txtSetSlot.Properties.Mask.IgnoreMaskBlank")));
      this.txtSetSlot.Properties.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("txtSetSlot.Properties.Mask.MaskType")));
      this.txtSetSlot.Properties.Mask.PlaceHolder = ((char)(resources.GetObject("txtSetSlot.Properties.Mask.PlaceHolder")));
      this.txtSetSlot.Properties.Mask.SaveLiteral = ((bool)(resources.GetObject("txtSetSlot.Properties.Mask.SaveLiteral")));
      this.txtSetSlot.Properties.Mask.ShowPlaceHolders = ((bool)(resources.GetObject("txtSetSlot.Properties.Mask.ShowPlaceHolders")));
      this.txtSetSlot.Properties.Mask.UseMaskAsDisplayFormat = ((bool)(resources.GetObject("txtSetSlot.Properties.Mask.UseMaskAsDisplayFormat")));
      this.txtSetSlot.Properties.NullValuePrompt = resources.GetString("txtSetSlot.Properties.NullValuePrompt");
      this.txtSetSlot.Properties.NullValuePromptShowForEmptyValue = ((bool)(resources.GetObject("txtSetSlot.Properties.NullValuePromptShowForEmptyValue")));
      this.txtSetSlot.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.txtSetSlot_ButtonClick);
      this.txtSetSlot.EditValueChanged += new System.EventHandler(this.txtSetSlot_EditValueChanged);
      this.txtSetSlot.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSetSlot_KeyDown);
      // 
      // labelControl11
      // 
      resources.ApplyResources(this.labelControl11, "labelControl11");
      this.labelControl11.Name = "labelControl11";
      // 
      // picDonate
      // 
      resources.ApplyResources(this.picDonate, "picDonate");
      this.picDonate.EditValue = global::ChanSort.Ui.Properties.Resources.Donate;
      this.picDonate.MenuManager = this.barManager1;
      this.picDonate.Name = "picDonate";
      this.picDonate.Properties.AccessibleDescription = resources.GetString("picDonate.Properties.AccessibleDescription");
      this.picDonate.Properties.AccessibleName = resources.GetString("picDonate.Properties.AccessibleName");
      this.picDonate.Properties.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("picDonate.Properties.Appearance.BackColor")));
      this.picDonate.Properties.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("picDonate.Properties.Appearance.GradientMode")));
      this.picDonate.Properties.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("picDonate.Properties.Appearance.Image")));
      this.picDonate.Properties.Appearance.Options.UseBackColor = true;
      this.picDonate.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.picDonate.Properties.PictureAlignment = System.Drawing.ContentAlignment.TopRight;
      this.picDonate.Click += new System.EventHandler(this.picDonate_Click);
      // 
      // defaultLookAndFeel1
      // 
      this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2007 Blue";
      // 
      // grpTopPanel
      // 
      resources.ApplyResources(this.grpTopPanel, "grpTopPanel");
      this.grpTopPanel.Controls.Add(this.rbInsertSwap);
      this.grpTopPanel.Controls.Add(this.rbInsertAfter);
      this.grpTopPanel.Controls.Add(this.rbInsertBefore);
      this.grpTopPanel.Controls.Add(this.cbCloseGap);
      this.grpTopPanel.Controls.Add(this.cbAppendUnsortedChannels);
      this.grpTopPanel.Controls.Add(this.labelControl2);
      this.grpTopPanel.Controls.Add(this.picDonate);
      this.grpTopPanel.Controls.Add(this.tabChannelList);
      this.grpTopPanel.Controls.Add(this.labelControl11);
      this.grpTopPanel.Controls.Add(this.txtSetSlot);
      this.grpTopPanel.Name = "grpTopPanel";
      this.grpTopPanel.ShowCaption = false;
      // 
      // rbInsertSwap
      // 
      resources.ApplyResources(this.rbInsertSwap, "rbInsertSwap");
      this.rbInsertSwap.MenuManager = this.barManager1;
      this.rbInsertSwap.Name = "rbInsertSwap";
      this.rbInsertSwap.Properties.AccessibleDescription = resources.GetString("rbInsertSwap.Properties.AccessibleDescription");
      this.rbInsertSwap.Properties.AccessibleName = resources.GetString("rbInsertSwap.Properties.AccessibleName");
      this.rbInsertSwap.Properties.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("rbInsertSwap.Properties.Appearance.GradientMode")));
      this.rbInsertSwap.Properties.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("rbInsertSwap.Properties.Appearance.Image")));
      this.rbInsertSwap.Properties.Appearance.Options.UseTextOptions = true;
      this.rbInsertSwap.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
      this.rbInsertSwap.Properties.AutoHeight = ((bool)(resources.GetObject("rbInsertSwap.Properties.AutoHeight")));
      this.rbInsertSwap.Properties.Caption = resources.GetString("rbInsertSwap.Properties.Caption");
      this.rbInsertSwap.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbInsertSwap.Properties.DisplayValueChecked = resources.GetString("rbInsertSwap.Properties.DisplayValueChecked");
      this.rbInsertSwap.Properties.DisplayValueGrayed = resources.GetString("rbInsertSwap.Properties.DisplayValueGrayed");
      this.rbInsertSwap.Properties.DisplayValueUnchecked = resources.GetString("rbInsertSwap.Properties.DisplayValueUnchecked");
      this.rbInsertSwap.Properties.GlyphAlignment = ((DevExpress.Utils.HorzAlignment)(resources.GetObject("rbInsertSwap.Properties.GlyphAlignment")));
      this.rbInsertSwap.Properties.RadioGroupIndex = 1;
      this.rbInsertSwap.TabStop = false;
      this.rbInsertSwap.CheckedChanged += new System.EventHandler(this.rbInsertMode_CheckedChanged);
      // 
      // rbInsertAfter
      // 
      resources.ApplyResources(this.rbInsertAfter, "rbInsertAfter");
      this.rbInsertAfter.MenuManager = this.barManager1;
      this.rbInsertAfter.Name = "rbInsertAfter";
      this.rbInsertAfter.Properties.AccessibleDescription = resources.GetString("rbInsertAfter.Properties.AccessibleDescription");
      this.rbInsertAfter.Properties.AccessibleName = resources.GetString("rbInsertAfter.Properties.AccessibleName");
      this.rbInsertAfter.Properties.AutoHeight = ((bool)(resources.GetObject("rbInsertAfter.Properties.AutoHeight")));
      this.rbInsertAfter.Properties.Caption = resources.GetString("rbInsertAfter.Properties.Caption");
      this.rbInsertAfter.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbInsertAfter.Properties.DisplayValueChecked = resources.GetString("rbInsertAfter.Properties.DisplayValueChecked");
      this.rbInsertAfter.Properties.DisplayValueGrayed = resources.GetString("rbInsertAfter.Properties.DisplayValueGrayed");
      this.rbInsertAfter.Properties.DisplayValueUnchecked = resources.GetString("rbInsertAfter.Properties.DisplayValueUnchecked");
      this.rbInsertAfter.Properties.RadioGroupIndex = 1;
      this.rbInsertAfter.CheckedChanged += new System.EventHandler(this.rbInsertMode_CheckedChanged);
      // 
      // rbInsertBefore
      // 
      resources.ApplyResources(this.rbInsertBefore, "rbInsertBefore");
      this.rbInsertBefore.MenuManager = this.barManager1;
      this.rbInsertBefore.Name = "rbInsertBefore";
      this.rbInsertBefore.Properties.AccessibleDescription = resources.GetString("rbInsertBefore.Properties.AccessibleDescription");
      this.rbInsertBefore.Properties.AccessibleName = resources.GetString("rbInsertBefore.Properties.AccessibleName");
      this.rbInsertBefore.Properties.AutoHeight = ((bool)(resources.GetObject("rbInsertBefore.Properties.AutoHeight")));
      this.rbInsertBefore.Properties.Caption = resources.GetString("rbInsertBefore.Properties.Caption");
      this.rbInsertBefore.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbInsertBefore.Properties.DisplayValueChecked = resources.GetString("rbInsertBefore.Properties.DisplayValueChecked");
      this.rbInsertBefore.Properties.DisplayValueGrayed = resources.GetString("rbInsertBefore.Properties.DisplayValueGrayed");
      this.rbInsertBefore.Properties.DisplayValueUnchecked = resources.GetString("rbInsertBefore.Properties.DisplayValueUnchecked");
      this.rbInsertBefore.Properties.RadioGroupIndex = 1;
      this.rbInsertBefore.TabStop = false;
      this.rbInsertBefore.CheckedChanged += new System.EventHandler(this.rbInsertMode_CheckedChanged);
      // 
      // cbCloseGap
      // 
      resources.ApplyResources(this.cbCloseGap, "cbCloseGap");
      this.cbCloseGap.MenuManager = this.barManager1;
      this.cbCloseGap.Name = "cbCloseGap";
      this.cbCloseGap.Properties.AccessibleDescription = resources.GetString("cbCloseGap.Properties.AccessibleDescription");
      this.cbCloseGap.Properties.AccessibleName = resources.GetString("cbCloseGap.Properties.AccessibleName");
      this.cbCloseGap.Properties.AutoHeight = ((bool)(resources.GetObject("cbCloseGap.Properties.AutoHeight")));
      this.cbCloseGap.Properties.Caption = resources.GetString("cbCloseGap.Properties.Caption");
      this.cbCloseGap.Properties.DisplayValueChecked = resources.GetString("cbCloseGap.Properties.DisplayValueChecked");
      this.cbCloseGap.Properties.DisplayValueGrayed = resources.GetString("cbCloseGap.Properties.DisplayValueGrayed");
      this.cbCloseGap.Properties.DisplayValueUnchecked = resources.GetString("cbCloseGap.Properties.DisplayValueUnchecked");
      // 
      // cbAppendUnsortedChannels
      // 
      resources.ApplyResources(this.cbAppendUnsortedChannels, "cbAppendUnsortedChannels");
      this.cbAppendUnsortedChannels.MenuManager = this.barManager1;
      this.cbAppendUnsortedChannels.Name = "cbAppendUnsortedChannels";
      this.cbAppendUnsortedChannels.Properties.AccessibleDescription = resources.GetString("cbAppendUnsortedChannels.Properties.AccessibleDescription");
      this.cbAppendUnsortedChannels.Properties.AccessibleName = resources.GetString("cbAppendUnsortedChannels.Properties.AccessibleName");
      this.cbAppendUnsortedChannels.Properties.AutoHeight = ((bool)(resources.GetObject("cbAppendUnsortedChannels.Properties.AutoHeight")));
      this.cbAppendUnsortedChannels.Properties.Caption = resources.GetString("cbAppendUnsortedChannels.Properties.Caption");
      this.cbAppendUnsortedChannels.Properties.DisplayValueChecked = resources.GetString("cbAppendUnsortedChannels.Properties.DisplayValueChecked");
      this.cbAppendUnsortedChannels.Properties.DisplayValueGrayed = resources.GetString("cbAppendUnsortedChannels.Properties.DisplayValueGrayed");
      this.cbAppendUnsortedChannels.Properties.DisplayValueUnchecked = resources.GetString("cbAppendUnsortedChannels.Properties.DisplayValueUnchecked");
      // 
      // tabChannelList
      // 
      resources.ApplyResources(this.tabChannelList, "tabChannelList");
      this.tabChannelList.Name = "tabChannelList";
      this.tabChannelList.SelectedTabPage = this.pageEmpty;
      this.tabChannelList.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.pageEmpty});
      this.tabChannelList.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.tabChannelList_SelectedPageChanged);
      // 
      // pageEmpty
      // 
      resources.ApplyResources(this.pageEmpty, "pageEmpty");
      this.pageEmpty.Name = "pageEmpty";
      // 
      // mnuContext
      // 
      this.mnuContext.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miMoveUp),
            new DevExpress.XtraBars.LinkPersistInfo(this.miMoveDown),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAddChannel),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRemove),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSort),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenum),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.mnuFavSet, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavUnset),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOn, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOn, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOn, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOff)});
      this.mnuContext.Manager = this.barManager1;
      this.mnuContext.Name = "mnuContext";
      // 
      // MainForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainerControl1);
      this.Controls.Add(this.grpTopPanel);
      this.Controls.Add(this.barDockControlLeft);
      this.Controls.Add(this.barDockControlRight);
      this.Controls.Add(this.barDockControlBottom);
      this.Controls.Add(this.barDockControlTop);
      this.Name = "MainForm";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.Shown += new System.EventHandler(this.MainForm_Shown);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
      this.splitContainerControl1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grpOutputList)).EndInit();
      this.grpOutputList.ResumeLayout(false);
      this.grpOutputList.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridLeft)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dsChannels)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewLeft)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pnlEditControls)).EndInit();
      this.pnlEditControls.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.globalImageCollection1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpInputList)).EndInit();
      this.grpInputList.ResumeLayout(false);
      this.grpInputList.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridRight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewRight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).EndInit();
      this.panelControl3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtSetSlot.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.picDonate.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpTopPanel)).EndInit();
      this.grpTopPanel.ResumeLayout(false);
      this.grpTopPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.rbInsertSwap.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbInsertAfter.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbInsertBefore.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbCloseGap.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbAppendUnsortedChannels.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tabChannelList)).EndInit();
      this.tabChannelList.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.mnuContext)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
    private DevExpress.XtraGrid.GridControl gridRight;
    private DevExpress.XtraGrid.Views.Grid.GridView gviewRight;
    private System.Windows.Forms.BindingSource dsChannels;
    private DevExpress.XtraGrid.Columns.GridColumn colIndex;
    private DevExpress.XtraGrid.Columns.GridColumn colSlotOld;
    private DevExpress.XtraGrid.Columns.GridColumn colSlotNew;
    private DevExpress.XtraGrid.Columns.GridColumn colUid;
    private DevExpress.XtraGrid.Columns.GridColumn colName;
    private DevExpress.XtraEditors.GroupControl grpOutputList;
    private DevExpress.XtraEditors.SimpleButton btnRemove;
    private DevExpress.XtraEditors.GroupControl grpInputList;
    private DevExpress.XtraEditors.SimpleButton btnAdd;
    private DevExpress.XtraGrid.GridControl gridLeft;
    private DevExpress.XtraGrid.Views.Grid.GridView gviewLeft;
    private DevExpress.XtraGrid.Columns.GridColumn colOutSlot;
    private DevExpress.XtraGrid.Columns.GridColumn colOutName;
    private DevExpress.XtraEditors.SimpleButton btnDown;
    private DevExpress.XtraEditors.SimpleButton btnUp;
    private DevExpress.XtraEditors.LabelControl lblHotkeyLeft;
    private DevExpress.XtraEditors.LabelControl lblHotkeyRight;
    private DevExpress.XtraEditors.ButtonEdit txtSetSlot;
    private DevExpress.XtraEditors.LabelControl labelControl11;
    private DevExpress.XtraGrid.Columns.GridColumn colEncrypted;
    private DevExpress.XtraBars.BarManager barManager1;
    private DevExpress.XtraBars.Bar bar1;
    private DevExpress.XtraBars.BarSubItem barSubItem1;
    private DevExpress.XtraBars.BarButtonItem miGerman;
    private DevExpress.XtraBars.BarButtonItem miEnglish;
    private DevExpress.XtraBars.BarDockControl barDockControlTop;
    private DevExpress.XtraBars.BarDockControl barDockControlBottom;
    private DevExpress.XtraBars.BarDockControl barDockControlLeft;
    private DevExpress.XtraBars.BarDockControl barDockControlRight;
    private GlobalImageCollection globalImageCollection1;
    private DevExpress.XtraBars.BarSubItem miFile;
    private DevExpress.XtraBars.BarButtonItem miOpen;
    private DevExpress.XtraBars.BarButtonItem miSave;
    private DevExpress.XtraBars.BarButtonItem miReload;
    private DevExpress.XtraBars.BarButtonItem miSaveAs;
    private DevExpress.XtraBars.BarButtonItem miQuit;
    private DevExpress.XtraBars.BarSubItem mnuHelp;
    private DevExpress.XtraBars.BarButtonItem miAbout;
    private DevExpress.XtraBars.BarButtonItem miCharsetForm;
    private DevExpress.XtraBars.BarListItem miIsoCharSets;
    private DevExpress.XtraBars.BarSubItem mnuCharset;
    private DevExpress.XtraEditors.SimpleButton btnClearRightFilter;
    private DevExpress.XtraEditors.SimpleButton btnClearLeftFilter;
    private DevExpress.XtraGrid.Columns.GridColumn colAudioPid;
    private DevExpress.XtraGrid.Columns.GridColumn colVideoPid;
    private DevExpress.XtraGrid.Columns.GridColumn colNetworkId;
    private DevExpress.XtraGrid.Columns.GridColumn colFreqInMhz;
    private DevExpress.XtraGrid.Columns.GridColumn colServiceId;
    private DevExpress.XtraGrid.Columns.GridColumn colServiceType;
    private DevExpress.XtraGrid.Columns.GridColumn colShortName;
    private DevExpress.XtraGrid.Columns.GridColumn colFavorites;
    private DevExpress.XtraGrid.Columns.GridColumn colSkip;
    private DevExpress.XtraGrid.Columns.GridColumn colLock;
    private DevExpress.XtraGrid.Columns.GridColumn colSatellite;
    private DevExpress.XtraGrid.Columns.GridColumn colHidden;
    private DevExpress.XtraGrid.Columns.GridColumn colSymbolRate;
    private DevExpress.XtraGrid.Columns.GridColumn colPolarity;
    private DevExpress.XtraGrid.Columns.GridColumn colTransportStreamId;
    private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
    private DevExpress.XtraEditors.PanelControl pnlEditControls;
    private DevExpress.XtraEditors.GroupControl grpTopPanel;
    private DevExpress.XtraEditors.PanelControl panelControl3;
    private DevExpress.XtraEditors.LabelControl labelControl2;
    private DevExpress.XtraGrid.Columns.GridColumn colOutFav;
    private DevExpress.XtraEditors.SimpleButton btnRenum;
    private DevExpress.XtraGrid.Columns.GridColumn colIndex1;
    private DevExpress.XtraGrid.Columns.GridColumn colUid1;
    private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit repositoryItemCheckedComboBoxEdit1;
    private DevExpress.XtraBars.PopupMenu mnuContext;
    private DevExpress.XtraBars.BarSubItem miEdit;
    private DevExpress.XtraBars.BarListItem miFavSet;
    private DevExpress.XtraBars.BarListItem miFavUnset;
    private DevExpress.XtraBars.BarButtonItem miRemove;
    private DevExpress.XtraBars.BarButtonItem miLockOn;
    private DevExpress.XtraBars.BarButtonItem miLockOff;
    private DevExpress.XtraBars.BarButtonItem miSkipOn;
    private DevExpress.XtraBars.BarButtonItem miSkipOff;
    private DevExpress.XtraBars.BarButtonItem miHideOn;
    private DevExpress.XtraBars.BarButtonItem miHideOff;
    private DevExpress.XtraBars.BarSubItem mnuOptions;
    private DevExpress.XtraBars.BarSubItem mnuFavSet;
    private DevExpress.XtraBars.BarSubItem mnuFavUnset;
    private DevExpress.XtraBars.BarButtonItem miAddChannel;
    private DevExpress.XtraBars.BarButtonItem miSort;
    private DevExpress.XtraBars.BarButtonItem miRenum;
    private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit repositoryItemCheckedComboBoxEdit2;
    private DevExpress.XtraGrid.Columns.GridColumn colChannelOrTransponder;
    private DevExpress.XtraGrid.Columns.GridColumn colNetworkName;
    private DevExpress.XtraGrid.Columns.GridColumn colNetworkOperator;
    private DevExpress.XtraGrid.Columns.GridColumn colDebug;
    private DevExpress.XtraGrid.Columns.GridColumn colServiceTypeName;
    private DevExpress.XtraBars.BarButtonItem miRestoreOriginal;
    private DevExpress.XtraGrid.Columns.GridColumn colLogicalIndex;
    private DevExpress.XtraBars.BarButtonItem miFileInformation;
    private DevExpress.XtraBars.BarButtonItem miOpenReferenceFile;
    private DevExpress.XtraEditors.SimpleButton btnAddAll;
    private DevExpress.XtraBars.BarButtonItem miMoveUp;
    private DevExpress.XtraBars.BarButtonItem miMoveDown;
    private DevExpress.XtraBars.BarSubItem barSubItem2;
    private DevExpress.XtraBars.BarButtonItem miTvSettings;
    private DevExpress.XtraBars.BarButtonItem miEraseChannelData;
    private DevExpress.XtraEditors.PictureEdit picDonate;
    private DevExpress.XtraBars.BarButtonItem miOpenWebsite;
    private DevExpress.XtraBars.BarButtonItem miWiki;
    private DevExpress.XtraTab.XtraTabControl tabChannelList;
    private DevExpress.XtraTab.XtraTabPage pageEmpty;
    private DevExpress.XtraEditors.CheckEdit cbCloseGap;
    private DevExpress.XtraEditors.CheckEdit cbAppendUnsortedChannels;
    private DevExpress.XtraEditors.CheckEdit rbInsertSwap;
    private DevExpress.XtraEditors.CheckEdit rbInsertAfter;
    private DevExpress.XtraEditors.CheckEdit rbInsertBefore;
    private DevExpress.XtraEditors.SimpleButton btnToggleLock;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavE;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavD;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavC;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavB;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavA;
    private DevExpress.XtraGrid.Columns.GridColumn colOutLock;
    private DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1;
  }
}

