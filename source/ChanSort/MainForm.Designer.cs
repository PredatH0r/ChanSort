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
      this.colOutLock = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutSkip = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutHide = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutDeleted = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutServiceType = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colOutSource = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colUid1 = new DevExpress.XtraGrid.Columns.GridColumn();
      this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
      this.lblHotkeyLeft = new DevExpress.XtraEditors.LabelControl();
      this.pnlEditControls = new DevExpress.XtraEditors.PanelControl();
      this.btnToggleFavH = new DevExpress.XtraEditors.SimpleButton();
      this.globalImageCollection1 = new ChanSort.Ui.GlobalImageCollection(this.components);
      this.btnToggleFavG = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavF = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleLock = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavE = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavD = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavC = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavB = new DevExpress.XtraEditors.SimpleButton();
      this.btnToggleFavA = new DevExpress.XtraEditors.SimpleButton();
      this.btnClearLeftFilter = new DevExpress.XtraEditors.SimpleButton();
      this.btnRenum = new DevExpress.XtraEditors.SimpleButton();
      this.btnDown = new DevExpress.XtraEditors.SimpleButton();
      this.btnUp = new DevExpress.XtraEditors.SimpleButton();
      this.btnRemoveLeft = new DevExpress.XtraEditors.SimpleButton();
      this.grpInputList = new DevExpress.XtraEditors.GroupControl();
      this.gridRight = new DevExpress.XtraGrid.GridControl();
      this.gviewRight = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colIndex = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSlotOld = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSlotNew = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSource = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colPrNr = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colShortName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFavorites = new DevExpress.XtraGrid.Columns.GridColumn();
      this.repositoryItemCheckedComboBoxEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
      this.colLock = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSkip = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colHidden = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDeleted = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colEncrypted = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colServiceType = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colServiceTypeName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colFreqInMhz = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colPolarity = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colChannelOrTransponder = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSatellite = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colNetworkId = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colTransportStreamId = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colServiceId = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colPcrPid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colVideoPid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colAudioPid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSymbolRate = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colNetworkName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colNetworkOperator = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colProvider = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colUid = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colLogicalIndex = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colSignalSource = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDebug = new DevExpress.XtraGrid.Columns.GridColumn();
      this.lblHotkeyRight = new DevExpress.XtraEditors.LabelControl();
      this.panelControl3 = new DevExpress.XtraEditors.PanelControl();
      this.lblPredefinedList = new DevExpress.XtraEditors.LabelControl();
      this.btnRemoveRight = new DevExpress.XtraEditors.SimpleButton();
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
      this.miAddFromRefList = new DevExpress.XtraBars.BarButtonItem();
      this.miSaveReferenceFile = new DevExpress.XtraBars.BarButtonItem();
      this.miExcelExport = new DevExpress.XtraBars.BarButtonItem();
      this.miPrint = new DevExpress.XtraBars.BarButtonItem();
      this.miQuit = new DevExpress.XtraBars.BarButtonItem();
      this.miRecentFiles = new DevExpress.XtraBars.BarListItem();
      this.miEdit = new DevExpress.XtraBars.BarSubItem();
      this.miAddChannel = new DevExpress.XtraBars.BarButtonItem();
      this.miRemove = new DevExpress.XtraBars.BarButtonItem();
      this.miRenameChannel = new DevExpress.XtraBars.BarButtonItem();
      this.mnuFavSet = new DevExpress.XtraBars.BarSubItem();
      this.mnuFavUnset = new DevExpress.XtraBars.BarSubItem();
      this.miLockOn = new DevExpress.XtraBars.BarButtonItem();
      this.miLockOff = new DevExpress.XtraBars.BarButtonItem();
      this.miSkipOn = new DevExpress.XtraBars.BarButtonItem();
      this.miSkipOff = new DevExpress.XtraBars.BarButtonItem();
      this.miHideOn = new DevExpress.XtraBars.BarButtonItem();
      this.miHideOff = new DevExpress.XtraBars.BarButtonItem();
      this.miRenum = new DevExpress.XtraBars.BarButtonItem();
      this.miSort = new DevExpress.XtraBars.BarButtonItem();
      this.miRenumFavByPrNr = new DevExpress.XtraBars.BarButtonItem();
      this.miCopyCsv = new DevExpress.XtraBars.BarButtonItem();
      this.barSubItem2 = new DevExpress.XtraBars.BarSubItem();
      this.miTvSettings = new DevExpress.XtraBars.BarButtonItem();
      this.miCleanupChannels = new DevExpress.XtraBars.BarButtonItem();
      this.mnuOptions = new DevExpress.XtraBars.BarSubItem();
      this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
      this.miEnglish = new DevExpress.XtraBars.BarButtonItem();
      this.miCzech = new DevExpress.XtraBars.BarButtonItem();
      this.miGerman = new DevExpress.XtraBars.BarButtonItem();
      this.miSpanish = new DevExpress.XtraBars.BarButtonItem();
      this.miPolski = new DevExpress.XtraBars.BarButtonItem();
      this.miPortuguese = new DevExpress.XtraBars.BarButtonItem();
      this.miRomanian = new DevExpress.XtraBars.BarButtonItem();
      this.miRussian = new DevExpress.XtraBars.BarButtonItem();
      this.miTurkish = new DevExpress.XtraBars.BarButtonItem();
      this.mnuCharset = new DevExpress.XtraBars.BarSubItem();
      this.miCharsetForm = new DevExpress.XtraBars.BarButtonItem();
      this.miUtf8Charset = new DevExpress.XtraBars.BarButtonItem();
      this.miIsoCharSets = new DevExpress.XtraBars.BarListItem();
      this.miShowWarningsAfterLoad = new DevExpress.XtraBars.BarCheckItem();
      this.miAllowEditPredefinedLists = new DevExpress.XtraBars.BarButtonItem();
      this.miExplorerIntegration = new DevExpress.XtraBars.BarButtonItem();
      this.miCheckUpdates = new DevExpress.XtraBars.BarButtonItem();
      this.mnuAccessibility = new DevExpress.XtraBars.BarSubItem();
      this.mnuGotoChannelList = new DevExpress.XtraBars.BarSubItem();
      this.mnuInputSource = new DevExpress.XtraBars.BarLinkContainerItem();
      this.mnuGotoFavList = new DevExpress.XtraBars.BarSubItem();
      this.mnuFavList = new DevExpress.XtraBars.BarLinkContainerItem();
      this.miSelectFavList0 = new DevExpress.XtraBars.BarButtonItem();
      this.miSelectFavListA = new DevExpress.XtraBars.BarButtonItem();
      this.miSelectFavListB = new DevExpress.XtraBars.BarButtonItem();
      this.miSelectFavListC = new DevExpress.XtraBars.BarButtonItem();
      this.miSelectFavListD = new DevExpress.XtraBars.BarButtonItem();
      this.miSelectFavListE = new DevExpress.XtraBars.BarButtonItem();
      this.miGotoLeftFilter = new DevExpress.XtraBars.BarButtonItem();
      this.miGotoLeftList = new DevExpress.XtraBars.BarButtonItem();
      this.miRightListFilter = new DevExpress.XtraBars.BarButtonItem();
      this.miGotoRightList = new DevExpress.XtraBars.BarButtonItem();
      this.miFontSmall = new DevExpress.XtraBars.BarButtonItem();
      this.miFontMedium = new DevExpress.XtraBars.BarButtonItem();
      this.miFontLarge = new DevExpress.XtraBars.BarButtonItem();
      this.miFontXLarge = new DevExpress.XtraBars.BarButtonItem();
      this.miFontXxLarge = new DevExpress.XtraBars.BarButtonItem();
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
      this.lblInsertMode = new DevExpress.XtraEditors.LabelControl();
      this.txtSetSlot = new DevExpress.XtraEditors.ButtonEdit();
      this.lblSetProgramNr = new DevExpress.XtraEditors.LabelControl();
      this.picDonate = new DevExpress.XtraEditors.PictureEdit();
      this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
      this.splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, typeof(global::ChanSort.Ui.WaitForm1), true, true);
      this.grpTopPanel = new DevExpress.XtraEditors.GroupControl();
      this.rbInsertSwap = new DevExpress.XtraEditors.CheckEdit();
      this.rbInsertAfter = new DevExpress.XtraEditors.CheckEdit();
      this.rbInsertBefore = new DevExpress.XtraEditors.CheckEdit();
      this.cbCloseGap = new DevExpress.XtraEditors.CheckEdit();
      this.tabChannelList = new DevExpress.XtraTab.XtraTabControl();
      this.pageEmpty = new DevExpress.XtraTab.XtraTabPage();
      this.popupContext = new DevExpress.XtraBars.PopupMenu(this.components);
      this.timerEditDelay = new System.Windows.Forms.Timer(this.components);
      this.grpSubList = new DevExpress.XtraEditors.GroupControl();
      this.tabSubList = new DevExpress.XtraTab.XtraTabControl();
      this.pageProgNr = new DevExpress.XtraTab.XtraTabPage();
      this.popupInputSource = new DevExpress.XtraBars.PopupMenu(this.components);
      this.popupFavList = new DevExpress.XtraBars.PopupMenu(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
      this.splitContainerControl1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grpOutputList)).BeginInit();
      this.grpOutputList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridLeft)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dsChannels)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewLeft)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pnlEditControls)).BeginInit();
      this.pnlEditControls.SuspendLayout();
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
      ((System.ComponentModel.ISupportInitialize)(this.tabChannelList)).BeginInit();
      this.tabChannelList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.popupContext)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpSubList)).BeginInit();
      this.grpSubList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tabSubList)).BeginInit();
      this.tabSubList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.popupInputSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.popupFavList)).BeginInit();
      this.SuspendLayout();
      // 
      // splitContainerControl1
      // 
      resources.ApplyResources(this.splitContainerControl1, "splitContainerControl1");
      this.splitContainerControl1.Name = "splitContainerControl1";
      this.splitContainerControl1.Panel1.Controls.Add(this.grpOutputList);
      resources.ApplyResources(this.splitContainerControl1.Panel1, "splitContainerControl1.Panel1");
      this.splitContainerControl1.Panel2.Controls.Add(this.grpInputList);
      resources.ApplyResources(this.splitContainerControl1.Panel2, "splitContainerControl1.Panel2");
      this.splitContainerControl1.SplitterPosition = 503;
      // 
      // grpOutputList
      // 
      this.grpOutputList.Controls.Add(this.gridLeft);
      this.grpOutputList.Controls.Add(this.lblHotkeyLeft);
      this.grpOutputList.Controls.Add(this.pnlEditControls);
      resources.ApplyResources(this.grpOutputList, "grpOutputList");
      this.grpOutputList.Name = "grpOutputList";
      this.grpOutputList.Enter += new System.EventHandler(this.grpOutputList_Enter);
      // 
      // gridLeft
      // 
      this.gridLeft.AllowDrop = true;
      this.gridLeft.DataSource = this.dsChannels;
      resources.ApplyResources(this.gridLeft, "gridLeft");
      this.gridLeft.MainView = this.gviewLeft;
      this.gridLeft.Name = "gridLeft";
      this.gridLeft.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckedComboBoxEdit1,
            this.repositoryItemTextEdit1});
      this.gridLeft.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gviewLeft});
      this.gridLeft.ProcessGridKey += new System.Windows.Forms.KeyEventHandler(this.gridLeft_ProcessGridKey);
      this.gridLeft.DragDrop += new System.Windows.Forms.DragEventHandler(this.gridLeft_DragDrop);
      this.gridLeft.DragOver += new System.Windows.Forms.DragEventHandler(this.gridLeft_DragOver);
      this.gridLeft.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.grid_GiveFeedback);
      // 
      // dsChannels
      // 
      this.dsChannels.DataSource = typeof(ChanSort.Api.ChannelInfo);
      // 
      // gviewLeft
      // 
      this.gviewLeft.Appearance.FocusedRow.Font = ((System.Drawing.Font)(resources.GetObject("gviewLeft.Appearance.FocusedRow.Font")));
      this.gviewLeft.Appearance.FocusedRow.Options.UseFont = true;
      this.gviewLeft.Appearance.HeaderPanel.Options.UseTextOptions = true;
      this.gviewLeft.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
      this.gviewLeft.Appearance.HideSelectionRow.Font = ((System.Drawing.Font)(resources.GetObject("gviewLeft.Appearance.HideSelectionRow.Font")));
      this.gviewLeft.Appearance.HideSelectionRow.Options.UseFont = true;
      this.gviewLeft.ColumnPanelRowHeight = 35;
      this.gviewLeft.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colIndex1,
            this.colOutSlot,
            this.colOutName,
            this.colOutFav,
            this.colOutLock,
            this.colOutSkip,
            this.colOutHide,
            this.colOutDeleted,
            this.colOutServiceType,
            this.colOutSource,
            this.colUid1});
      this.gviewLeft.GridControl = this.gridLeft;
      this.gviewLeft.Name = "gviewLeft";
      this.gviewLeft.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
      this.gviewLeft.OptionsCustomization.AllowGroup = false;
      this.gviewLeft.OptionsCustomization.AllowQuickHideColumns = false;
      this.gviewLeft.OptionsDetail.EnableMasterViewMode = false;
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
      this.gviewLeft.ShownEditor += new System.EventHandler(this.gview_ShownEditor);
      this.gviewLeft.EndSorting += new System.EventHandler(this.gviewLeft_EndSorting);
      this.gviewLeft.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gviewLeft_FocusedRowChanged);
      this.gviewLeft.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gviewLeft_CellValueChanged);
      this.gviewLeft.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gview_CustomUnboundColumnData);
      this.gviewLeft.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gviewLeft_CustomColumnDisplayText);
      this.gviewLeft.LayoutUpgrade += new DevExpress.Utils.LayoutUpgradeEventHandler(this.gviewLeft_LayoutUpgrade);
      this.gviewLeft.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.gview_KeyPress);
      this.gviewLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gview_MouseDown);
      this.gviewLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gview_MouseUp);
      this.gviewLeft.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gview_MouseMove);
      this.gviewLeft.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gviewLeft_ValidatingEditor);
      // 
      // colIndex1
      // 
      resources.ApplyResources(this.colIndex1, "colIndex1");
      this.colIndex1.FieldName = "RecordIndex";
      this.colIndex1.Name = "colIndex1";
      this.colIndex1.OptionsColumn.AllowEdit = false;
      // 
      // colOutSlot
      // 
      resources.ApplyResources(this.colOutSlot, "colOutSlot");
      this.colOutSlot.DisplayFormat.FormatString = "d";
      this.colOutSlot.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colOutSlot.FieldName = "Position";
      this.colOutSlot.Name = "colOutSlot";
      this.colOutSlot.OptionsFilter.AllowAutoFilter = false;
      this.colOutSlot.OptionsFilter.AllowFilterModeChanging = DevExpress.Utils.DefaultBoolean.False;
      this.colOutSlot.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
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
      this.repositoryItemCheckedComboBoxEdit1.Mask.EditMask = resources.GetString("repositoryItemCheckedComboBoxEdit1.Mask.EditMask");
      this.repositoryItemCheckedComboBoxEdit1.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("repositoryItemCheckedComboBoxEdit1.Mask.MaskType")));
      this.repositoryItemCheckedComboBoxEdit1.Name = "repositoryItemCheckedComboBoxEdit1";
      this.repositoryItemCheckedComboBoxEdit1.PopupSizeable = false;
      this.repositoryItemCheckedComboBoxEdit1.SelectAllItemVisible = false;
      this.repositoryItemCheckedComboBoxEdit1.ShowButtons = false;
      this.repositoryItemCheckedComboBoxEdit1.ShowPopupCloseButton = false;
      this.repositoryItemCheckedComboBoxEdit1.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
      // 
      // colOutLock
      // 
      resources.ApplyResources(this.colOutLock, "colOutLock");
      this.colOutLock.FieldName = "Lock";
      this.colOutLock.Name = "colOutLock";
      // 
      // colOutSkip
      // 
      resources.ApplyResources(this.colOutSkip, "colOutSkip");
      this.colOutSkip.FieldName = "Skip";
      this.colOutSkip.Name = "colOutSkip";
      // 
      // colOutHide
      // 
      resources.ApplyResources(this.colOutHide, "colOutHide");
      this.colOutHide.FieldName = "Hidden";
      this.colOutHide.Name = "colOutHide";
      // 
      // colOutDeleted
      // 
      resources.ApplyResources(this.colOutDeleted, "colOutDeleted");
      this.colOutDeleted.FieldName = "IsDeleted";
      this.colOutDeleted.Name = "colOutDeleted";
      // 
      // colOutServiceType
      // 
      resources.ApplyResources(this.colOutServiceType, "colOutServiceType");
      this.colOutServiceType.FieldName = "ServiceTypeName";
      this.colOutServiceType.Name = "colOutServiceType";
      this.colOutServiceType.OptionsColumn.AllowEdit = false;
      // 
      // colOutSource
      // 
      resources.ApplyResources(this.colOutSource, "colOutSource");
      this.colOutSource.FieldName = "Source";
      this.colOutSource.Name = "colOutSource";
      this.colOutSource.OptionsColumn.AllowEdit = false;
      // 
      // colUid1
      // 
      resources.ApplyResources(this.colUid1, "colUid1");
      this.colUid1.FieldName = "Uid";
      this.colUid1.Name = "colUid1";
      this.colUid1.OptionsColumn.AllowEdit = false;
      // 
      // repositoryItemTextEdit1
      // 
      resources.ApplyResources(this.repositoryItemTextEdit1, "repositoryItemTextEdit1");
      this.repositoryItemTextEdit1.DisplayFormat.FormatString = "d";
      this.repositoryItemTextEdit1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.repositoryItemTextEdit1.EditFormat.FormatString = "d";
      this.repositoryItemTextEdit1.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.repositoryItemTextEdit1.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("repositoryItemTextEdit1.Mask.MaskType")));
      this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
      // 
      // lblHotkeyLeft
      // 
      resources.ApplyResources(this.lblHotkeyLeft, "lblHotkeyLeft");
      this.lblHotkeyLeft.Name = "lblHotkeyLeft";
      // 
      // pnlEditControls
      // 
      this.pnlEditControls.Controls.Add(this.btnToggleFavH);
      this.pnlEditControls.Controls.Add(this.btnToggleFavG);
      this.pnlEditControls.Controls.Add(this.btnToggleFavF);
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
      this.pnlEditControls.Controls.Add(this.btnRemoveLeft);
      resources.ApplyResources(this.pnlEditControls, "pnlEditControls");
      this.pnlEditControls.Name = "pnlEditControls";
      // 
      // btnToggleFavH
      // 
      resources.ApplyResources(this.btnToggleFavH, "btnToggleFavH");
      this.btnToggleFavH.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavH.Name = "btnToggleFavH";
      this.btnToggleFavH.Tag = "";
      this.btnToggleFavH.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavG
      // 
      resources.ApplyResources(this.btnToggleFavG, "btnToggleFavG");
      this.btnToggleFavG.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavG.Name = "btnToggleFavG";
      this.btnToggleFavG.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavF
      // 
      resources.ApplyResources(this.btnToggleFavF, "btnToggleFavF");
      this.btnToggleFavF.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavF.Name = "btnToggleFavF";
      this.btnToggleFavF.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleLock
      // 
      this.btnToggleLock.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnToggleLock.ImageOptions.ImageIndex")));
      this.btnToggleLock.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnToggleLock, "btnToggleLock");
      this.btnToggleLock.Name = "btnToggleLock";
      this.btnToggleLock.Click += new System.EventHandler(this.btnToggleLock_Click);
      // 
      // btnToggleFavE
      // 
      resources.ApplyResources(this.btnToggleFavE, "btnToggleFavE");
      this.btnToggleFavE.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavE.Name = "btnToggleFavE";
      this.btnToggleFavE.Tag = "";
      this.btnToggleFavE.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavD
      // 
      resources.ApplyResources(this.btnToggleFavD, "btnToggleFavD");
      this.btnToggleFavD.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavD.Name = "btnToggleFavD";
      this.btnToggleFavD.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavC
      // 
      resources.ApplyResources(this.btnToggleFavC, "btnToggleFavC");
      this.btnToggleFavC.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavC.Name = "btnToggleFavC";
      this.btnToggleFavC.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavB
      // 
      resources.ApplyResources(this.btnToggleFavB, "btnToggleFavB");
      this.btnToggleFavB.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavB.Name = "btnToggleFavB";
      this.btnToggleFavB.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnToggleFavA
      // 
      resources.ApplyResources(this.btnToggleFavA, "btnToggleFavA");
      this.btnToggleFavA.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnToggleFavA.Name = "btnToggleFavA";
      this.btnToggleFavA.Click += new System.EventHandler(this.btnToggleFav_Click);
      // 
      // btnClearLeftFilter
      // 
      resources.ApplyResources(this.btnClearLeftFilter, "btnClearLeftFilter");
      this.btnClearLeftFilter.Appearance.FontStyleDelta = ((System.Drawing.FontStyle)(resources.GetObject("btnClearLeftFilter.Appearance.FontStyleDelta")));
      this.btnClearLeftFilter.Appearance.Options.UseFont = true;
      this.btnClearLeftFilter.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnClearLeftFilter.ImageOptions.ImageIndex")));
      this.btnClearLeftFilter.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnClearLeftFilter.Name = "btnClearLeftFilter";
      this.btnClearLeftFilter.Click += new System.EventHandler(this.btnClearLeftFilter_Click);
      // 
      // btnRenum
      // 
      this.btnRenum.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnRenum.ImageOptions.ImageIndex")));
      this.btnRenum.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnRenum, "btnRenum");
      this.btnRenum.Name = "btnRenum";
      this.btnRenum.Click += new System.EventHandler(this.btnRenum_Click);
      // 
      // btnDown
      // 
      this.btnDown.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnDown.ImageOptions.ImageIndex")));
      this.btnDown.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnDown, "btnDown");
      this.btnDown.Name = "btnDown";
      this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
      // 
      // btnUp
      // 
      this.btnUp.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnUp.ImageOptions.ImageIndex")));
      this.btnUp.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnUp, "btnUp");
      this.btnUp.Name = "btnUp";
      this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
      // 
      // btnRemoveLeft
      // 
      this.btnRemoveLeft.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnRemoveLeft.ImageOptions.ImageIndex")));
      this.btnRemoveLeft.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnRemoveLeft, "btnRemoveLeft");
      this.btnRemoveLeft.Name = "btnRemoveLeft";
      this.btnRemoveLeft.Click += new System.EventHandler(this.btnRemoveLeft_Click);
      // 
      // grpInputList
      // 
      this.grpInputList.Controls.Add(this.gridRight);
      this.grpInputList.Controls.Add(this.lblHotkeyRight);
      this.grpInputList.Controls.Add(this.panelControl3);
      resources.ApplyResources(this.grpInputList, "grpInputList");
      this.grpInputList.Name = "grpInputList";
      this.grpInputList.Enter += new System.EventHandler(this.grpInputList_Enter);
      // 
      // gridRight
      // 
      this.gridRight.AllowDrop = true;
      this.gridRight.DataSource = this.dsChannels;
      resources.ApplyResources(this.gridRight, "gridRight");
      this.gridRight.MainView = this.gviewRight;
      this.gridRight.Name = "gridRight";
      this.gridRight.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckedComboBoxEdit2});
      this.gridRight.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gviewRight});
      this.gridRight.ProcessGridKey += new System.Windows.Forms.KeyEventHandler(this.gridRight_ProcessGridKey);
      this.gridRight.DragDrop += new System.Windows.Forms.DragEventHandler(this.gridRight_DragDrop);
      this.gridRight.DragEnter += new System.Windows.Forms.DragEventHandler(this.gridRight_DragEnter);
      this.gridRight.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.grid_GiveFeedback);
      // 
      // gviewRight
      // 
      this.gviewRight.Appearance.FocusedRow.Font = ((System.Drawing.Font)(resources.GetObject("gviewRight.Appearance.FocusedRow.Font")));
      this.gviewRight.Appearance.FocusedRow.Options.UseFont = true;
      this.gviewRight.Appearance.HeaderPanel.Options.UseTextOptions = true;
      this.gviewRight.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
      this.gviewRight.Appearance.HideSelectionRow.Font = ((System.Drawing.Font)(resources.GetObject("gviewRight.Appearance.HideSelectionRow.Font")));
      this.gviewRight.Appearance.HideSelectionRow.Options.UseFont = true;
      this.gviewRight.ColumnPanelRowHeight = 35;
      this.gviewRight.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colIndex,
            this.colSlotOld,
            this.colSlotNew,
            this.colSource,
            this.colPrNr,
            this.colName,
            this.colShortName,
            this.colFavorites,
            this.colLock,
            this.colSkip,
            this.colHidden,
            this.colDeleted,
            this.colEncrypted,
            this.colServiceType,
            this.colServiceTypeName,
            this.colFreqInMhz,
            this.colPolarity,
            this.colChannelOrTransponder,
            this.colSatellite,
            this.colNetworkId,
            this.colTransportStreamId,
            this.colServiceId,
            this.colPcrPid,
            this.colVideoPid,
            this.colAudioPid,
            this.colSymbolRate,
            this.colNetworkName,
            this.colNetworkOperator,
            this.colProvider,
            this.colUid,
            this.colLogicalIndex,
            this.colSignalSource,
            this.colDebug});
      this.gviewRight.GridControl = this.gridRight;
      this.gviewRight.Name = "gviewRight";
      this.gviewRight.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
      this.gviewRight.OptionsCustomization.AllowGroup = false;
      this.gviewRight.OptionsDetail.EnableMasterViewMode = false;
      this.gviewRight.OptionsLayout.LayoutVersion = "5";
      this.gviewRight.OptionsSelection.MultiSelect = true;
      this.gviewRight.OptionsView.ColumnAutoWidth = false;
      this.gviewRight.OptionsView.ShowAutoFilterRow = true;
      this.gviewRight.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
      this.gviewRight.OptionsView.ShowGroupExpandCollapseButtons = false;
      this.gviewRight.OptionsView.ShowGroupPanel = false;
      this.gviewRight.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.colSlotOld, DevExpress.Data.ColumnSortOrder.Ascending)});
      this.gviewRight.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gviewRight_RowClick);
      this.gviewRight.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gviewRight_RowCellStyle);
      this.gviewRight.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gviewRight_PopupMenuShowing);
      this.gviewRight.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.gview_ShowingEditor);
      this.gviewRight.ShownEditor += new System.EventHandler(this.gview_ShownEditor);
      this.gviewRight.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gviewRight_FocusedRowChanged);
      this.gviewRight.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gviewRight_CellValueChanged);
      this.gviewRight.CustomColumnSort += new DevExpress.XtraGrid.Views.Base.CustomColumnSortEventHandler(this.gviewRight_CustomColumnSort);
      this.gviewRight.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gview_CustomUnboundColumnData);
      this.gviewRight.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gviewRight_CustomColumnDisplayText);
      this.gviewRight.LayoutUpgrade += new DevExpress.Utils.LayoutUpgradeEventHandler(this.gviewRight_LayoutUpgrade);
      this.gviewRight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.gview_KeyPress);
      this.gviewRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gview_MouseDown);
      this.gviewRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gview_MouseUp);
      this.gviewRight.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gview_MouseMove);
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
      this.colSlotOld.DisplayFormat.FormatString = "d";
      this.colSlotOld.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colSlotOld.FieldName = "OldPosition";
      this.colSlotOld.Name = "colSlotOld";
      this.colSlotOld.OptionsColumn.AllowEdit = false;
      this.colSlotOld.OptionsFilter.AllowAutoFilter = false;
      this.colSlotOld.SortMode = DevExpress.XtraGrid.ColumnSortMode.Custom;
      this.colSlotOld.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
      // 
      // colSlotNew
      // 
      resources.ApplyResources(this.colSlotNew, "colSlotNew");
      this.colSlotNew.DisplayFormat.FormatString = "d";
      this.colSlotNew.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colSlotNew.FieldName = "Position";
      this.colSlotNew.Name = "colSlotNew";
      this.colSlotNew.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
      // 
      // colSource
      // 
      resources.ApplyResources(this.colSource, "colSource");
      this.colSource.FieldName = "Source";
      this.colSource.Name = "colSource";
      // 
      // colPrNr
      // 
      resources.ApplyResources(this.colPrNr, "colPrNr");
      this.colPrNr.FieldName = "NewProgramNr";
      this.colPrNr.Name = "colPrNr";
      this.colPrNr.OptionsColumn.AllowEdit = false;
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
      this.repositoryItemCheckedComboBoxEdit2.Mask.EditMask = resources.GetString("repositoryItemCheckedComboBoxEdit2.Mask.EditMask");
      this.repositoryItemCheckedComboBoxEdit2.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("repositoryItemCheckedComboBoxEdit2.Mask.MaskType")));
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
      // colDeleted
      // 
      resources.ApplyResources(this.colDeleted, "colDeleted");
      this.colDeleted.FieldName = "IsDeleted";
      this.colDeleted.Name = "colDeleted";
      // 
      // colEncrypted
      // 
      resources.ApplyResources(this.colEncrypted, "colEncrypted");
      this.colEncrypted.FieldName = "Encrypted";
      this.colEncrypted.Name = "colEncrypted";
      this.colEncrypted.OptionsColumn.AllowEdit = false;
      this.colEncrypted.OptionsColumn.FixedWidth = true;
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
      // colFreqInMhz
      // 
      resources.ApplyResources(this.colFreqInMhz, "colFreqInMhz");
      this.colFreqInMhz.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colFreqInMhz.FieldName = "FreqInMhz";
      this.colFreqInMhz.Name = "colFreqInMhz";
      this.colFreqInMhz.OptionsColumn.AllowEdit = false;
      // 
      // colPolarity
      // 
      resources.ApplyResources(this.colPolarity, "colPolarity");
      this.colPolarity.FieldName = "Polarity";
      this.colPolarity.Name = "colPolarity";
      this.colPolarity.OptionsColumn.AllowEdit = false;
      // 
      // colChannelOrTransponder
      // 
      resources.ApplyResources(this.colChannelOrTransponder, "colChannelOrTransponder");
      this.colChannelOrTransponder.FieldName = "ChannelOrTransponder";
      this.colChannelOrTransponder.Name = "colChannelOrTransponder";
      this.colChannelOrTransponder.OptionsColumn.AllowEdit = false;
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
      // colServiceId
      // 
      resources.ApplyResources(this.colServiceId, "colServiceId");
      this.colServiceId.FieldName = "ServiceId";
      this.colServiceId.Name = "colServiceId";
      this.colServiceId.OptionsColumn.AllowEdit = false;
      // 
      // colPcrPid
      // 
      resources.ApplyResources(this.colPcrPid, "colPcrPid");
      this.colPcrPid.FieldName = "PcrPid";
      this.colPcrPid.Name = "colPcrPid";
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
      // colSymbolRate
      // 
      resources.ApplyResources(this.colSymbolRate, "colSymbolRate");
      this.colSymbolRate.FieldName = "SymbolRate";
      this.colSymbolRate.Name = "colSymbolRate";
      this.colSymbolRate.OptionsColumn.AllowEdit = false;
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
      // colProvider
      // 
      resources.ApplyResources(this.colProvider, "colProvider");
      this.colProvider.FieldName = "Provider";
      this.colProvider.Name = "colProvider";
      this.colProvider.OptionsColumn.AllowEdit = false;
      // 
      // colUid
      // 
      resources.ApplyResources(this.colUid, "colUid");
      this.colUid.FieldName = "Uid";
      this.colUid.Name = "colUid";
      this.colUid.OptionsColumn.AllowEdit = false;
      // 
      // colLogicalIndex
      // 
      resources.ApplyResources(this.colLogicalIndex, "colLogicalIndex");
      this.colLogicalIndex.FieldName = "RecordOrder";
      this.colLogicalIndex.Name = "colLogicalIndex";
      this.colLogicalIndex.OptionsColumn.AllowEdit = false;
      this.colLogicalIndex.OptionsColumn.ReadOnly = true;
      // 
      // colSignalSource
      // 
      resources.ApplyResources(this.colSignalSource, "colSignalSource");
      this.colSignalSource.FieldName = "SignalSource";
      this.colSignalSource.Name = "colSignalSource";
      this.colSignalSource.OptionsColumn.AllowEdit = false;
      // 
      // colDebug
      // 
      this.colDebug.FieldName = "Debug";
      this.colDebug.Name = "colDebug";
      this.colDebug.OptionsColumn.AllowEdit = false;
      // 
      // lblHotkeyRight
      // 
      resources.ApplyResources(this.lblHotkeyRight, "lblHotkeyRight");
      this.lblHotkeyRight.Name = "lblHotkeyRight";
      // 
      // panelControl3
      // 
      this.panelControl3.Controls.Add(this.lblPredefinedList);
      this.panelControl3.Controls.Add(this.btnRemoveRight);
      this.panelControl3.Controls.Add(this.btnAddAll);
      this.panelControl3.Controls.Add(this.btnClearRightFilter);
      this.panelControl3.Controls.Add(this.btnAdd);
      resources.ApplyResources(this.panelControl3, "panelControl3");
      this.panelControl3.Name = "panelControl3";
      // 
      // lblPredefinedList
      // 
      this.lblPredefinedList.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("lblPredefinedList.Appearance.Font")));
      this.lblPredefinedList.Appearance.ForeColor = System.Drawing.Color.Maroon;
      this.lblPredefinedList.Appearance.Options.UseFont = true;
      this.lblPredefinedList.Appearance.Options.UseForeColor = true;
      resources.ApplyResources(this.lblPredefinedList, "lblPredefinedList");
      this.lblPredefinedList.Name = "lblPredefinedList";
      // 
      // btnRemoveRight
      // 
      this.btnRemoveRight.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnRemoveRight.ImageOptions.ImageIndex")));
      this.btnRemoveRight.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnRemoveRight, "btnRemoveRight");
      this.btnRemoveRight.Name = "btnRemoveRight";
      this.btnRemoveRight.Click += new System.EventHandler(this.btnRemoveRight_Click);
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
      this.btnClearRightFilter.Appearance.FontStyleDelta = ((System.Drawing.FontStyle)(resources.GetObject("btnClearRightFilter.Appearance.FontStyleDelta")));
      this.btnClearRightFilter.Appearance.Options.UseFont = true;
      this.btnClearRightFilter.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnClearRightFilter.ImageOptions.ImageIndex")));
      this.btnClearRightFilter.ImageOptions.ImageList = this.globalImageCollection1;
      this.btnClearRightFilter.Name = "btnClearRightFilter";
      this.btnClearRightFilter.Click += new System.EventHandler(this.btnClearRightFilter_Click);
      // 
      // btnAdd
      // 
      this.btnAdd.ImageOptions.ImageIndex = ((int)(resources.GetObject("btnAdd.ImageOptions.ImageIndex")));
      this.btnAdd.ImageOptions.ImageList = this.globalImageCollection1;
      resources.ApplyResources(this.btnAdd, "btnAdd");
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
            ((DevExpress.XtraBars.BarManagerCategory)(resources.GetObject("barManager1.Categories3"))),
            ((DevExpress.XtraBars.BarManagerCategory)(resources.GetObject("barManager1.Categories4")))});
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
            this.miRenameChannel,
            this.miSort,
            this.miRenum,
            this.mnuFavSet,
            this.mnuFavUnset,
            this.miLockOn,
            this.miLockOff,
            this.miSkipOn,
            this.miSkipOff,
            this.miHideOn,
            this.miHideOff,
            this.barSubItem2,
            this.miTvSettings,
            this.miOpenWebsite,
            this.miWiki,
            this.miShowWarningsAfterLoad,
            this.miCleanupChannels,
            this.miSaveReferenceFile,
            this.miRecentFiles,
            this.miExcelExport,
            this.miPortuguese,
            this.miAddFromRefList,
            this.miPrint,
            this.miRenumFavByPrNr,
            this.mnuAccessibility,
            this.miGotoLeftFilter,
            this.miGotoLeftList,
            this.miRightListFilter,
            this.miGotoRightList,
            this.miSelectFavList0,
            this.miSelectFavListA,
            this.miSelectFavListB,
            this.miSelectFavListC,
            this.miSelectFavListD,
            this.miSelectFavListE,
            this.mnuGotoChannelList,
            this.mnuInputSource,
            this.mnuGotoFavList,
            this.mnuFavList,
            this.miRussian,
            this.miAllowEditPredefinedLists,
            this.miCzech,
            this.miRomanian,
            this.miExplorerIntegration,
            this.miCheckUpdates,
            this.miUtf8Charset,
            this.miCopyCsv,
            this.miSpanish,
            this.miPolski,
            this.miTurkish,
            this.miFontSmall,
            this.miFontMedium,
            this.miFontLarge,
            this.miFontXLarge,
            this.miFontXxLarge});
      this.barManager1.MainMenu = this.bar1;
      this.barManager1.MaxItemId = 109;
      this.barManager1.ShowFullMenus = true;
      // 
      // bar1
      // 
      this.bar1.BarName = "Tools";
      this.bar1.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Top;
      this.bar1.DockCol = 0;
      this.bar1.DockRow = 0;
      this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
      this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miFile),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpen),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpenReferenceFile),
            new DevExpress.XtraBars.LinkPersistInfo(this.miReload),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSave, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSaveAs),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSaveReferenceFile),
            new DevExpress.XtraBars.LinkPersistInfo(this.miPrint, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miEdit, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavSet),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem2, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miTvSettings),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuOptions, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuAccessibility, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuHelp),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAbout)});
      this.bar1.OptionsBar.AllowQuickCustomization = false;
      this.bar1.OptionsBar.DisableClose = true;
      this.bar1.OptionsBar.DisableCustomization = true;
      this.bar1.OptionsBar.DrawDragBorder = false;
      this.bar1.OptionsBar.MultiLine = true;
      this.bar1.OptionsBar.UseWholeRow = true;
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
            new DevExpress.XtraBars.LinkPersistInfo(this.miSave, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSaveAs),
            new DevExpress.XtraBars.LinkPersistInfo(this.miOpenReferenceFile, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAddFromRefList),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSaveReferenceFile),
            new DevExpress.XtraBars.LinkPersistInfo(this.miExcelExport),
            new DevExpress.XtraBars.LinkPersistInfo(this.miPrint),
            new DevExpress.XtraBars.LinkPersistInfo(this.miQuit, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRecentFiles, true)});
      this.miFile.Name = "miFile";
      // 
      // miOpen
      // 
      resources.ApplyResources(this.miOpen, "miOpen");
      this.miOpen.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miOpen.Id = 5;
      this.miOpen.ImageOptions.ImageIndex = ((int)(resources.GetObject("miOpen.ImageOptions.ImageIndex")));
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
      this.miReload.ImageOptions.ImageIndex = ((int)(resources.GetObject("miReload.ImageOptions.ImageIndex")));
      this.miReload.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R));
      this.miReload.Name = "miReload";
      this.miReload.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miReload_ItemClick);
      // 
      // miRestoreOriginal
      // 
      resources.ApplyResources(this.miRestoreOriginal, "miRestoreOriginal");
      this.miRestoreOriginal.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miRestoreOriginal.Id = 42;
      this.miRestoreOriginal.ImageOptions.ImageIndex = ((int)(resources.GetObject("miRestoreOriginal.ImageOptions.ImageIndex")));
      this.miRestoreOriginal.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.R));
      this.miRestoreOriginal.Name = "miRestoreOriginal";
      this.miRestoreOriginal.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRestoreOriginal_ItemClick);
      // 
      // miFileInformation
      // 
      resources.ApplyResources(this.miFileInformation, "miFileInformation");
      this.miFileInformation.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miFileInformation.Id = 43;
      this.miFileInformation.ImageOptions.ImageIndex = ((int)(resources.GetObject("miFileInformation.ImageOptions.ImageIndex")));
      this.miFileInformation.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I));
      this.miFileInformation.Name = "miFileInformation";
      this.miFileInformation.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miFileInformation_ItemClick);
      // 
      // miSave
      // 
      resources.ApplyResources(this.miSave, "miSave");
      this.miSave.CategoryGuid = new System.Guid("e6c9a329-010b-4d79-8d4d-215e5f197ed3");
      this.miSave.Enabled = false;
      this.miSave.Id = 6;
      this.miSave.ImageOptions.ImageIndex = ((int)(resources.GetObject("miSave.ImageOptions.ImageIndex")));
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
      this.miSaveAs.ImageOptions.ImageIndex = ((int)(resources.GetObject("miSaveAs.ImageOptions.ImageIndex")));
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
      this.miOpenReferenceFile.ImageOptions.ImageIndex = ((int)(resources.GetObject("miOpenReferenceFile.ImageOptions.ImageIndex")));
      this.miOpenReferenceFile.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.O));
      this.miOpenReferenceFile.Name = "miOpenReferenceFile";
      this.miOpenReferenceFile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miOpenReferenceFile_ItemClick);
      // 
      // miAddFromRefList
      // 
      resources.ApplyResources(this.miAddFromRefList, "miAddFromRefList");
      this.miAddFromRefList.Id = 61;
      this.miAddFromRefList.Name = "miAddFromRefList";
      this.miAddFromRefList.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miAddFromRefList_ItemClick);
      // 
      // miSaveReferenceFile
      // 
      resources.ApplyResources(this.miSaveReferenceFile, "miSaveReferenceFile");
      this.miSaveReferenceFile.Id = 57;
      this.miSaveReferenceFile.ImageOptions.ImageIndex = ((int)(resources.GetObject("miSaveReferenceFile.ImageOptions.ImageIndex")));
      this.miSaveReferenceFile.Name = "miSaveReferenceFile";
      this.miSaveReferenceFile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSaveReferenceFile_ItemClick);
      // 
      // miExcelExport
      // 
      resources.ApplyResources(this.miExcelExport, "miExcelExport");
      this.miExcelExport.Id = 59;
      this.miExcelExport.ImageOptions.ImageIndex = ((int)(resources.GetObject("miExcelExport.ImageOptions.ImageIndex")));
      this.miExcelExport.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.C));
      this.miExcelExport.Name = "miExcelExport";
      this.miExcelExport.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miExcelExport_ItemClick);
      // 
      // miPrint
      // 
      resources.ApplyResources(this.miPrint, "miPrint");
      this.miPrint.Id = 62;
      this.miPrint.ImageOptions.ImageIndex = ((int)(resources.GetObject("miPrint.ImageOptions.ImageIndex")));
      this.miPrint.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P));
      this.miPrint.Name = "miPrint";
      this.miPrint.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miPrint_ItemClick);
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
      // miRecentFiles
      // 
      resources.ApplyResources(this.miRecentFiles, "miRecentFiles");
      this.miRecentFiles.Id = 58;
      this.miRecentFiles.Name = "miRecentFiles";
      this.miRecentFiles.ListItemClick += new DevExpress.XtraBars.ListItemClickEventHandler(this.miRecentFiles_ListItemClick);
      // 
      // miEdit
      // 
      resources.ApplyResources(this.miEdit, "miEdit");
      this.miEdit.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miEdit.Id = 22;
      this.miEdit.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miAddChannel),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRemove),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenameChannel),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.mnuFavSet, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavUnset),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenum, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSort),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenumFavByPrNr),
            new DevExpress.XtraBars.LinkPersistInfo(this.miCopyCsv)});
      this.miEdit.Name = "miEdit";
      // 
      // miAddChannel
      // 
      resources.ApplyResources(this.miAddChannel, "miAddChannel");
      this.miAddChannel.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miAddChannel.Id = 39;
      this.miAddChannel.ImageOptions.ImageIndex = ((int)(resources.GetObject("miAddChannel.ImageOptions.ImageIndex")));
      this.miAddChannel.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Add));
      this.miAddChannel.Name = "miAddChannel";
      this.miAddChannel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miAddChannel_ItemClick);
      // 
      // miRemove
      // 
      resources.ApplyResources(this.miRemove, "miRemove");
      this.miRemove.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miRemove.Id = 25;
      this.miRemove.ImageOptions.ImageIndex = ((int)(resources.GetObject("miRemove.ImageOptions.ImageIndex")));
      this.miRemove.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X));
      this.miRemove.Name = "miRemove";
      this.miRemove.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRemove_ItemClick);
      // 
      // miRenameChannel
      // 
      resources.ApplyResources(this.miRenameChannel, "miRenameChannel");
      this.miRenameChannel.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miRenameChannel.Id = 52;
      this.miRenameChannel.ImageOptions.ImageIndex = ((int)(resources.GetObject("miRenameChannel.ImageOptions.ImageIndex")));
      this.miRenameChannel.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N));
      this.miRenameChannel.Name = "miRenameChannel";
      this.miRenameChannel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRenameChannel_ItemClick);
      // 
      // mnuFavSet
      // 
      resources.ApplyResources(this.mnuFavSet, "mnuFavSet");
      this.mnuFavSet.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.mnuFavSet.Id = 37;
      this.mnuFavSet.ImageOptions.ImageIndex = ((int)(resources.GetObject("mnuFavSet.ImageOptions.ImageIndex")));
      this.mnuFavSet.Name = "mnuFavSet";
      this.mnuFavSet.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu;
      // 
      // mnuFavUnset
      // 
      resources.ApplyResources(this.mnuFavUnset, "mnuFavUnset");
      this.mnuFavUnset.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.mnuFavUnset.Id = 38;
      this.mnuFavUnset.Name = "mnuFavUnset";
      // 
      // miLockOn
      // 
      resources.ApplyResources(this.miLockOn, "miLockOn");
      this.miLockOn.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miLockOn.Id = 27;
      this.miLockOn.ImageOptions.ImageIndex = ((int)(resources.GetObject("miLockOn.ImageOptions.ImageIndex")));
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
      this.miSkipOn.ImageOptions.ImageIndex = ((int)(resources.GetObject("miSkipOn.ImageOptions.ImageIndex")));
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
      this.miHideOn.ImageOptions.ImageIndex = ((int)(resources.GetObject("miHideOn.ImageOptions.ImageIndex")));
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
      // miRenum
      // 
      resources.ApplyResources(this.miRenum, "miRenum");
      this.miRenum.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miRenum.Id = 41;
      this.miRenum.ImageOptions.ImageIndex = ((int)(resources.GetObject("miRenum.ImageOptions.ImageIndex")));
      this.miRenum.Name = "miRenum";
      this.miRenum.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRenum_ItemClick);
      // 
      // miSort
      // 
      resources.ApplyResources(this.miSort, "miSort");
      this.miSort.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miSort.Id = 40;
      this.miSort.ImageOptions.ImageIndex = ((int)(resources.GetObject("miSort.ImageOptions.ImageIndex")));
      this.miSort.Name = "miSort";
      this.miSort.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSort_ItemClick);
      // 
      // miRenumFavByPrNr
      // 
      resources.ApplyResources(this.miRenumFavByPrNr, "miRenumFavByPrNr");
      this.miRenumFavByPrNr.Id = 63;
      this.miRenumFavByPrNr.Name = "miRenumFavByPrNr";
      this.miRenumFavByPrNr.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRenumFavByPrNr_ItemClick);
      // 
      // miCopyCsv
      // 
      resources.ApplyResources(this.miCopyCsv, "miCopyCsv");
      this.miCopyCsv.Id = 100;
      this.miCopyCsv.ImageOptions.ImageIndex = ((int)(resources.GetObject("miCopyCsv.ImageOptions.ImageIndex")));
      this.miCopyCsv.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.C));
      this.miCopyCsv.Name = "miCopyCsv";
      this.miCopyCsv.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miCopyCsv_ItemClick);
      // 
      // barSubItem2
      // 
      resources.ApplyResources(this.barSubItem2, "barSubItem2");
      this.barSubItem2.Id = 47;
      this.barSubItem2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miTvSettings),
            new DevExpress.XtraBars.LinkPersistInfo(this.miCleanupChannels)});
      this.barSubItem2.Name = "barSubItem2";
      // 
      // miTvSettings
      // 
      resources.ApplyResources(this.miTvSettings, "miTvSettings");
      this.miTvSettings.Id = 48;
      this.miTvSettings.ImageOptions.ImageIndex = ((int)(resources.GetObject("miTvSettings.ImageOptions.ImageIndex")));
      this.miTvSettings.Name = "miTvSettings";
      this.miTvSettings.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miTvCountrySetup_ItemClick);
      // 
      // miCleanupChannels
      // 
      resources.ApplyResources(this.miCleanupChannels, "miCleanupChannels");
      this.miCleanupChannels.Id = 56;
      this.miCleanupChannels.Name = "miCleanupChannels";
      this.miCleanupChannels.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miCleanupChannels_ItemClick);
      // 
      // mnuOptions
      // 
      resources.ApplyResources(this.mnuOptions, "mnuOptions");
      this.mnuOptions.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.mnuOptions.Id = 34;
      this.mnuOptions.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItem1, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.mnuCharset, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.miShowWarningsAfterLoad),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAllowEditPredefinedLists),
            new DevExpress.XtraBars.LinkPersistInfo(this.miExplorerIntegration),
            new DevExpress.XtraBars.LinkPersistInfo(this.miCheckUpdates)});
      this.mnuOptions.Name = "mnuOptions";
      // 
      // barSubItem1
      // 
      resources.ApplyResources(this.barSubItem1, "barSubItem1");
      this.barSubItem1.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.barSubItem1.Id = 0;
      this.barSubItem1.ImageOptions.ImageIndex = ((int)(resources.GetObject("barSubItem1.ImageOptions.ImageIndex")));
      this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miEnglish),
            new DevExpress.XtraBars.LinkPersistInfo(this.miCzech),
            new DevExpress.XtraBars.LinkPersistInfo(this.miGerman),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSpanish),
            new DevExpress.XtraBars.LinkPersistInfo(this.miPolski),
            new DevExpress.XtraBars.LinkPersistInfo(this.miPortuguese),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRomanian),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRussian),
            new DevExpress.XtraBars.LinkPersistInfo(this.miTurkish)});
      this.barSubItem1.Name = "barSubItem1";
      this.barSubItem1.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu;
      // 
      // miEnglish
      // 
      this.miEnglish.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miEnglish, "miEnglish");
      this.miEnglish.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miEnglish.Id = 2;
      this.miEnglish.ImageOptions.ImageIndex = ((int)(resources.GetObject("miEnglish.ImageOptions.ImageIndex")));
      this.miEnglish.Name = "miEnglish";
      this.miEnglish.Tag = "en";
      this.miEnglish.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miCzech
      // 
      this.miCzech.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miCzech, "miCzech");
      this.miCzech.Id = 95;
      this.miCzech.ImageOptions.ImageIndex = ((int)(resources.GetObject("miCzech.ImageOptions.ImageIndex")));
      this.miCzech.Name = "miCzech";
      this.miCzech.Tag = "cs-CZ";
      this.miCzech.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miGerman
      // 
      this.miGerman.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miGerman, "miGerman");
      this.miGerman.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miGerman.Id = 1;
      this.miGerman.ImageOptions.ImageIndex = ((int)(resources.GetObject("miGerman.ImageOptions.ImageIndex")));
      this.miGerman.Name = "miGerman";
      this.miGerman.Tag = "de-DE";
      this.miGerman.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miSpanish
      // 
      this.miSpanish.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miSpanish, "miSpanish");
      this.miSpanish.Id = 101;
      this.miSpanish.ImageOptions.ImageIndex = ((int)(resources.GetObject("miSpanish.ImageOptions.ImageIndex")));
      this.miSpanish.Name = "miSpanish";
      this.miSpanish.Tag = "es-ES";
      this.miSpanish.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miPolski
      // 
      this.miPolski.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miPolski, "miPolski");
      this.miPolski.Id = 102;
      this.miPolski.ImageOptions.ImageIndex = ((int)(resources.GetObject("miPolski.ImageOptions.ImageIndex")));
      this.miPolski.Name = "miPolski";
      this.miPolski.Tag = "pl-PL";
      this.miPolski.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miPortuguese
      // 
      this.miPortuguese.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miPortuguese, "miPortuguese");
      this.miPortuguese.Id = 60;
      this.miPortuguese.ImageOptions.ImageIndex = ((int)(resources.GetObject("miPortuguese.ImageOptions.ImageIndex")));
      this.miPortuguese.Name = "miPortuguese";
      this.miPortuguese.Tag = "pt-PT";
      this.miPortuguese.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miRomanian
      // 
      this.miRomanian.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miRomanian, "miRomanian");
      this.miRomanian.Id = 96;
      this.miRomanian.ImageOptions.ImageIndex = ((int)(resources.GetObject("miRomanian.ImageOptions.ImageIndex")));
      this.miRomanian.Name = "miRomanian";
      this.miRomanian.Tag = "ro-RO";
      this.miRomanian.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miRussian
      // 
      this.miRussian.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miRussian, "miRussian");
      this.miRussian.Id = 93;
      this.miRussian.ImageOptions.ImageIndex = ((int)(resources.GetObject("miRussian.ImageOptions.ImageIndex")));
      this.miRussian.Name = "miRussian";
      this.miRussian.Tag = "ru-RU";
      this.miRussian.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // miTurkish
      // 
      this.miTurkish.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miTurkish, "miTurkish");
      this.miTurkish.Id = 103;
      this.miTurkish.ImageOptions.ImageIndex = ((int)(resources.GetObject("miTurkish.ImageOptions.ImageIndex")));
      this.miTurkish.Name = "miTurkish";
      this.miTurkish.Tag = "tr-TR";
      this.miTurkish.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miLanguage_DownChanged);
      // 
      // mnuCharset
      // 
      resources.ApplyResources(this.mnuCharset, "mnuCharset");
      this.mnuCharset.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.mnuCharset.Id = 15;
      this.mnuCharset.ImageOptions.ImageIndex = ((int)(resources.GetObject("mnuCharset.ImageOptions.ImageIndex")));
      this.mnuCharset.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miCharsetForm),
            new DevExpress.XtraBars.LinkPersistInfo(this.miUtf8Charset, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miIsoCharSets)});
      this.mnuCharset.Name = "mnuCharset";
      this.mnuCharset.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu;
      // 
      // miCharsetForm
      // 
      resources.ApplyResources(this.miCharsetForm, "miCharsetForm");
      this.miCharsetForm.CategoryGuid = new System.Guid("870e935c-f3d9-4202-9c58-87966069155d");
      this.miCharsetForm.Id = 13;
      this.miCharsetForm.ImageOptions.ImageIndex = ((int)(resources.GetObject("miCharsetForm.ImageOptions.ImageIndex")));
      this.miCharsetForm.Name = "miCharsetForm";
      this.miCharsetForm.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miCharset_ItemClick);
      // 
      // miUtf8Charset
      // 
      resources.ApplyResources(this.miUtf8Charset, "miUtf8Charset");
      this.miUtf8Charset.Id = 99;
      this.miUtf8Charset.Name = "miUtf8Charset";
      this.miUtf8Charset.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.MiUtf8Charset_ItemClick);
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
      // miShowWarningsAfterLoad
      // 
      resources.ApplyResources(this.miShowWarningsAfterLoad, "miShowWarningsAfterLoad");
      this.miShowWarningsAfterLoad.Id = 54;
      this.miShowWarningsAfterLoad.Name = "miShowWarningsAfterLoad";
      // 
      // miAllowEditPredefinedLists
      // 
      this.miAllowEditPredefinedLists.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miAllowEditPredefinedLists, "miAllowEditPredefinedLists");
      this.miAllowEditPredefinedLists.Id = 94;
      this.miAllowEditPredefinedLists.ImageOptions.ImageIndex = ((int)(resources.GetObject("miAllowEditPredefinedLists.ImageOptions.ImageIndex")));
      this.miAllowEditPredefinedLists.Name = "miAllowEditPredefinedLists";
      this.miAllowEditPredefinedLists.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miAllowEditPredefinedLists_DownChanged);
      // 
      // miExplorerIntegration
      // 
      this.miExplorerIntegration.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miExplorerIntegration, "miExplorerIntegration");
      this.miExplorerIntegration.Id = 97;
      this.miExplorerIntegration.Name = "miExplorerIntegration";
      this.miExplorerIntegration.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miExplorerIntegration_ItemClick);
      // 
      // miCheckUpdates
      // 
      this.miCheckUpdates.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miCheckUpdates, "miCheckUpdates");
      this.miCheckUpdates.Id = 98;
      this.miCheckUpdates.Name = "miCheckUpdates";
      this.miCheckUpdates.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miCheckUpdates_ItemClick);
      // 
      // mnuAccessibility
      // 
      resources.ApplyResources(this.mnuAccessibility, "mnuAccessibility");
      this.mnuAccessibility.Id = 64;
      this.mnuAccessibility.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuGotoChannelList),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuGotoFavList),
            new DevExpress.XtraBars.LinkPersistInfo(this.miGotoLeftFilter),
            new DevExpress.XtraBars.LinkPersistInfo(this.miGotoLeftList),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRightListFilter),
            new DevExpress.XtraBars.LinkPersistInfo(this.miGotoRightList),
            new DevExpress.XtraBars.LinkPersistInfo(this.miFontSmall, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miFontMedium),
            new DevExpress.XtraBars.LinkPersistInfo(this.miFontLarge),
            new DevExpress.XtraBars.LinkPersistInfo(this.miFontXLarge),
            new DevExpress.XtraBars.LinkPersistInfo(this.miFontXxLarge)});
      this.mnuAccessibility.Name = "mnuAccessibility";
      // 
      // mnuGotoChannelList
      // 
      resources.ApplyResources(this.mnuGotoChannelList, "mnuGotoChannelList");
      this.mnuGotoChannelList.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.mnuGotoChannelList.Id = 89;
      this.mnuGotoChannelList.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuInputSource)});
      this.mnuGotoChannelList.Name = "mnuGotoChannelList";
      // 
      // mnuInputSource
      // 
      resources.ApplyResources(this.mnuInputSource, "mnuInputSource");
      this.mnuInputSource.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.mnuInputSource.Id = 90;
      this.mnuInputSource.Name = "mnuInputSource";
      // 
      // mnuGotoFavList
      // 
      resources.ApplyResources(this.mnuGotoFavList, "mnuGotoFavList");
      this.mnuGotoFavList.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.mnuGotoFavList.Id = 91;
      this.mnuGotoFavList.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavList)});
      this.mnuGotoFavList.Name = "mnuGotoFavList";
      // 
      // mnuFavList
      // 
      resources.ApplyResources(this.mnuFavList, "mnuFavList");
      this.mnuFavList.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.mnuFavList.Id = 92;
      this.mnuFavList.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miSelectFavList0),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSelectFavListA),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSelectFavListB),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSelectFavListC),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSelectFavListD),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSelectFavListE)});
      this.mnuFavList.Name = "mnuFavList";
      // 
      // miSelectFavList0
      // 
      resources.ApplyResources(this.miSelectFavList0, "miSelectFavList0");
      this.miSelectFavList0.Id = 83;
      this.miSelectFavList0.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
                | System.Windows.Forms.Keys.D0));
      this.miSelectFavList0.Name = "miSelectFavList0";
      this.miSelectFavList0.Tag = "0";
      this.miSelectFavList0.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSelectFavList_ItemClick);
      // 
      // miSelectFavListA
      // 
      resources.ApplyResources(this.miSelectFavListA, "miSelectFavListA");
      this.miSelectFavListA.Id = 84;
      this.miSelectFavListA.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
                | System.Windows.Forms.Keys.D1));
      this.miSelectFavListA.Name = "miSelectFavListA";
      this.miSelectFavListA.Tag = "1";
      this.miSelectFavListA.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSelectFavList_ItemClick);
      // 
      // miSelectFavListB
      // 
      resources.ApplyResources(this.miSelectFavListB, "miSelectFavListB");
      this.miSelectFavListB.Id = 85;
      this.miSelectFavListB.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
                | System.Windows.Forms.Keys.D2));
      this.miSelectFavListB.Name = "miSelectFavListB";
      this.miSelectFavListB.Tag = "2";
      this.miSelectFavListB.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSelectFavList_ItemClick);
      // 
      // miSelectFavListC
      // 
      resources.ApplyResources(this.miSelectFavListC, "miSelectFavListC");
      this.miSelectFavListC.Id = 86;
      this.miSelectFavListC.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
                | System.Windows.Forms.Keys.D3));
      this.miSelectFavListC.Name = "miSelectFavListC";
      this.miSelectFavListC.Tag = "3";
      this.miSelectFavListC.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSelectFavList_ItemClick);
      // 
      // miSelectFavListD
      // 
      resources.ApplyResources(this.miSelectFavListD, "miSelectFavListD");
      this.miSelectFavListD.Id = 87;
      this.miSelectFavListD.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
                | System.Windows.Forms.Keys.D4));
      this.miSelectFavListD.Name = "miSelectFavListD";
      this.miSelectFavListD.Tag = "4";
      this.miSelectFavListD.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSelectFavList_ItemClick);
      // 
      // miSelectFavListE
      // 
      resources.ApplyResources(this.miSelectFavListE, "miSelectFavListE");
      this.miSelectFavListE.Id = 88;
      this.miSelectFavListE.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
                | System.Windows.Forms.Keys.D5));
      this.miSelectFavListE.Name = "miSelectFavListE";
      this.miSelectFavListE.Tag = "5";
      this.miSelectFavListE.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miSelectFavList_ItemClick);
      // 
      // miGotoLeftFilter
      // 
      resources.ApplyResources(this.miGotoLeftFilter, "miGotoLeftFilter");
      this.miGotoLeftFilter.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.miGotoLeftFilter.Id = 67;
      this.miGotoLeftFilter.ItemShortcut = new DevExpress.XtraBars.BarShortcut(System.Windows.Forms.Keys.F3);
      this.miGotoLeftFilter.Name = "miGotoLeftFilter";
      this.miGotoLeftFilter.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miGotoLeftFilter_ItemClick);
      // 
      // miGotoLeftList
      // 
      resources.ApplyResources(this.miGotoLeftList, "miGotoLeftList");
      this.miGotoLeftList.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.miGotoLeftList.Id = 69;
      this.miGotoLeftList.ItemShortcut = new DevExpress.XtraBars.BarShortcut(System.Windows.Forms.Keys.F4);
      this.miGotoLeftList.Name = "miGotoLeftList";
      this.miGotoLeftList.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miGotoLeftList_ItemClick);
      // 
      // miRightListFilter
      // 
      resources.ApplyResources(this.miRightListFilter, "miRightListFilter");
      this.miRightListFilter.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.miRightListFilter.Id = 68;
      this.miRightListFilter.ItemShortcut = new DevExpress.XtraBars.BarShortcut(System.Windows.Forms.Keys.F5);
      this.miRightListFilter.Name = "miRightListFilter";
      this.miRightListFilter.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miRightListFilter_ItemClick);
      // 
      // miGotoRightList
      // 
      resources.ApplyResources(this.miGotoRightList, "miGotoRightList");
      this.miGotoRightList.CategoryGuid = new System.Guid("9cf38b78-167f-4d95-b848-8a3c913209c4");
      this.miGotoRightList.Id = 70;
      this.miGotoRightList.ItemShortcut = new DevExpress.XtraBars.BarShortcut(System.Windows.Forms.Keys.F6);
      this.miGotoRightList.Name = "miGotoRightList";
      this.miGotoRightList.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miGotoRightList_ItemClick);
      // 
      // miFontSmall
      // 
      this.miFontSmall.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miFontSmall, "miFontSmall");
      this.miFontSmall.Down = true;
      this.miFontSmall.Id = 104;
      this.miFontSmall.Name = "miFontSmall";
      this.miFontSmall.Tag = 0;
      this.miFontSmall.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miFont_DownChanged);
      // 
      // miFontMedium
      // 
      this.miFontMedium.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miFontMedium, "miFontMedium");
      this.miFontMedium.Id = 105;
      this.miFontMedium.Name = "miFontMedium";
      this.miFontMedium.Tag = 1;
      this.miFontMedium.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miFont_DownChanged);
      // 
      // miFontLarge
      // 
      this.miFontLarge.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miFontLarge, "miFontLarge");
      this.miFontLarge.Id = 106;
      this.miFontLarge.Name = "miFontLarge";
      this.miFontLarge.Tag = 2;
      this.miFontLarge.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miFont_DownChanged);
      // 
      // miFontXLarge
      // 
      this.miFontXLarge.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miFontXLarge, "miFontXLarge");
      this.miFontXLarge.Id = 107;
      this.miFontXLarge.Name = "miFontXLarge";
      this.miFontXLarge.Tag = 3;
      this.miFontXLarge.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miFont_DownChanged);
      // 
      // miFontXxLarge
      // 
      this.miFontXxLarge.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
      resources.ApplyResources(this.miFontXxLarge, "miFontXxLarge");
      this.miFontXxLarge.Id = 108;
      this.miFontXxLarge.Name = "miFontXxLarge";
      this.miFontXxLarge.Tag = 4;
      this.miFontXxLarge.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.miFont_DownChanged);
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
      this.miAbout.ImageOptions.ImageIndex = ((int)(resources.GetObject("miAbout.ImageOptions.ImageIndex")));
      this.miAbout.Name = "miAbout";
      this.miAbout.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miAbout_ItemClick);
      // 
      // barDockControlTop
      // 
      this.barDockControlTop.CausesValidation = false;
      resources.ApplyResources(this.barDockControlTop, "barDockControlTop");
      this.barDockControlTop.Manager = this.barManager1;
      // 
      // barDockControlBottom
      // 
      this.barDockControlBottom.CausesValidation = false;
      resources.ApplyResources(this.barDockControlBottom, "barDockControlBottom");
      this.barDockControlBottom.Manager = this.barManager1;
      // 
      // barDockControlLeft
      // 
      this.barDockControlLeft.CausesValidation = false;
      resources.ApplyResources(this.barDockControlLeft, "barDockControlLeft");
      this.barDockControlLeft.Manager = this.barManager1;
      // 
      // barDockControlRight
      // 
      this.barDockControlRight.CausesValidation = false;
      resources.ApplyResources(this.barDockControlRight, "barDockControlRight");
      this.barDockControlRight.Manager = this.barManager1;
      // 
      // miMoveUp
      // 
      resources.ApplyResources(this.miMoveUp, "miMoveUp");
      this.miMoveUp.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miMoveUp.Id = 45;
      this.miMoveUp.ImageOptions.ImageIndex = ((int)(resources.GetObject("miMoveUp.ImageOptions.ImageIndex")));
      this.miMoveUp.Name = "miMoveUp";
      this.miMoveUp.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miMoveUp_ItemClick);
      // 
      // miMoveDown
      // 
      resources.ApplyResources(this.miMoveDown, "miMoveDown");
      this.miMoveDown.CategoryGuid = new System.Guid("d7eec464-59c9-4f45-88aa-602e64c81cc0");
      this.miMoveDown.Id = 46;
      this.miMoveDown.ImageOptions.ImageIndex = ((int)(resources.GetObject("miMoveDown.ImageOptions.ImageIndex")));
      this.miMoveDown.Name = "miMoveDown";
      this.miMoveDown.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.miMoveDown_ItemClick);
      // 
      // lblInsertMode
      // 
      resources.ApplyResources(this.lblInsertMode, "lblInsertMode");
      this.lblInsertMode.Name = "lblInsertMode";
      // 
      // txtSetSlot
      // 
      resources.ApplyResources(this.txtSetSlot, "txtSetSlot");
      this.txtSetSlot.Name = "txtSetSlot";
      this.txtSetSlot.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("txtSetSlot.Properties.Buttons"))))});
      this.txtSetSlot.Properties.Mask.EditMask = resources.GetString("txtSetSlot.Properties.Mask.EditMask");
      this.txtSetSlot.Properties.Mask.MaskType = ((DevExpress.XtraEditors.Mask.MaskType)(resources.GetObject("txtSetSlot.Properties.Mask.MaskType")));
      this.txtSetSlot.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.txtSetSlot_ButtonClick);
      this.txtSetSlot.EditValueChanged += new System.EventHandler(this.txtSetSlot_EditValueChanged);
      this.txtSetSlot.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSetSlot_KeyDown);
      // 
      // lblSetProgramNr
      // 
      this.lblSetProgramNr.Appearance.Options.UseTextOptions = true;
      this.lblSetProgramNr.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
      resources.ApplyResources(this.lblSetProgramNr, "lblSetProgramNr");
      this.lblSetProgramNr.Name = "lblSetProgramNr";
      // 
      // picDonate
      // 
      resources.ApplyResources(this.picDonate, "picDonate");
      this.picDonate.Cursor = System.Windows.Forms.Cursors.Default;
      this.picDonate.EditValue = global::ChanSort.Ui.Properties.Resources.Donate;
      this.picDonate.MenuManager = this.barManager1;
      this.picDonate.Name = "picDonate";
      this.picDonate.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
      this.picDonate.Properties.Appearance.Options.UseBackColor = true;
      this.picDonate.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
      this.picDonate.Properties.PictureAlignment = System.Drawing.ContentAlignment.TopRight;
      this.picDonate.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
      this.picDonate.Click += new System.EventHandler(this.picDonate_Click);
      // 
      // defaultLookAndFeel1
      // 
      this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2010 Blue";
      // 
      // grpTopPanel
      // 
      this.grpTopPanel.Controls.Add(this.rbInsertSwap);
      this.grpTopPanel.Controls.Add(this.rbInsertAfter);
      this.grpTopPanel.Controls.Add(this.rbInsertBefore);
      this.grpTopPanel.Controls.Add(this.cbCloseGap);
      this.grpTopPanel.Controls.Add(this.lblInsertMode);
      this.grpTopPanel.Controls.Add(this.picDonate);
      this.grpTopPanel.Controls.Add(this.tabChannelList);
      this.grpTopPanel.Controls.Add(this.lblSetProgramNr);
      this.grpTopPanel.Controls.Add(this.txtSetSlot);
      resources.ApplyResources(this.grpTopPanel, "grpTopPanel");
      this.grpTopPanel.Name = "grpTopPanel";
      this.grpTopPanel.ShowCaption = false;
      // 
      // rbInsertSwap
      // 
      resources.ApplyResources(this.rbInsertSwap, "rbInsertSwap");
      this.rbInsertSwap.MenuManager = this.barManager1;
      this.rbInsertSwap.Name = "rbInsertSwap";
      this.rbInsertSwap.Properties.AutoWidth = true;
      this.rbInsertSwap.Properties.Caption = resources.GetString("rbInsertSwap.Properties.Caption");
      this.rbInsertSwap.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
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
      this.rbInsertAfter.Properties.AutoWidth = true;
      this.rbInsertAfter.Properties.Caption = resources.GetString("rbInsertAfter.Properties.Caption");
      this.rbInsertAfter.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbInsertAfter.Properties.RadioGroupIndex = 1;
      this.rbInsertAfter.CheckedChanged += new System.EventHandler(this.rbInsertMode_CheckedChanged);
      // 
      // rbInsertBefore
      // 
      resources.ApplyResources(this.rbInsertBefore, "rbInsertBefore");
      this.rbInsertBefore.MenuManager = this.barManager1;
      this.rbInsertBefore.Name = "rbInsertBefore";
      this.rbInsertBefore.Properties.AutoWidth = true;
      this.rbInsertBefore.Properties.Caption = resources.GetString("rbInsertBefore.Properties.Caption");
      this.rbInsertBefore.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbInsertBefore.Properties.RadioGroupIndex = 1;
      this.rbInsertBefore.TabStop = false;
      this.rbInsertBefore.CheckedChanged += new System.EventHandler(this.rbInsertMode_CheckedChanged);
      // 
      // cbCloseGap
      // 
      resources.ApplyResources(this.cbCloseGap, "cbCloseGap");
      this.cbCloseGap.MenuManager = this.barManager1;
      this.cbCloseGap.Name = "cbCloseGap";
      this.cbCloseGap.Properties.Caption = resources.GetString("cbCloseGap.Properties.Caption");
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
      this.pageEmpty.Name = "pageEmpty";
      resources.ApplyResources(this.pageEmpty, "pageEmpty");
      // 
      // popupContext
      // 
      this.popupContext.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.miMoveUp),
            new DevExpress.XtraBars.LinkPersistInfo(this.miMoveDown),
            new DevExpress.XtraBars.LinkPersistInfo(this.miAddChannel),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRemove),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenameChannel),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.mnuFavSet, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavUnset),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miLockOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSkipOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOn),
            new DevExpress.XtraBars.LinkPersistInfo(this.miHideOff),
            new DevExpress.XtraBars.LinkPersistInfo(this.miRenum, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.miSort),
            new DevExpress.XtraBars.LinkPersistInfo(this.miCopyCsv)});
      this.popupContext.Manager = this.barManager1;
      this.popupContext.Name = "popupContext";
      // 
      // timerEditDelay
      // 
      this.timerEditDelay.Interval = 500;
      this.timerEditDelay.Tick += new System.EventHandler(this.timerEditDelay_Tick);
      // 
      // grpSubList
      // 
      this.grpSubList.Controls.Add(this.tabSubList);
      resources.ApplyResources(this.grpSubList, "grpSubList");
      this.grpSubList.Name = "grpSubList";
      this.grpSubList.ShowCaption = false;
      // 
      // tabSubList
      // 
      resources.ApplyResources(this.tabSubList, "tabSubList");
      this.tabSubList.Name = "tabSubList";
      this.tabSubList.SelectedTabPage = this.pageProgNr;
      this.tabSubList.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.pageProgNr});
      this.tabSubList.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.tabSubList_SelectedPageChanged);
      this.tabSubList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabSubList_MouseUp);
      // 
      // pageProgNr
      // 
      this.pageProgNr.Name = "pageProgNr";
      resources.ApplyResources(this.pageProgNr, "pageProgNr");
      // 
      // popupInputSource
      // 
      this.popupInputSource.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuInputSource)});
      this.popupInputSource.Manager = this.barManager1;
      this.popupInputSource.MenuCaption = "Select input source";
      this.popupInputSource.Name = "popupInputSource";
      this.popupInputSource.ShowCaption = true;
      // 
      // popupFavList
      // 
      this.popupFavList.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.mnuFavList)});
      this.popupFavList.Manager = this.barManager1;
      this.popupFavList.MenuCaption = "Select program or favorite list";
      this.popupFavList.Name = "popupFavList";
      this.popupFavList.ShowCaption = true;
      // 
      // MainForm
      // 
      this.AllowDrop = true;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainerControl1);
      this.Controls.Add(this.grpSubList);
      this.Controls.Add(this.grpTopPanel);
      this.Controls.Add(this.barDockControlLeft);
      this.Controls.Add(this.barDockControlRight);
      this.Controls.Add(this.barDockControlBottom);
      this.Controls.Add(this.barDockControlTop);
      this.Name = "MainForm";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
      this.splitContainerControl1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grpOutputList)).EndInit();
      this.grpOutputList.ResumeLayout(false);
      this.grpOutputList.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridLeft)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dsChannels)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewLeft)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pnlEditControls)).EndInit();
      this.pnlEditControls.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grpInputList)).EndInit();
      this.grpInputList.ResumeLayout(false);
      this.grpInputList.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridRight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gviewRight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).EndInit();
      this.panelControl3.ResumeLayout(false);
      this.panelControl3.PerformLayout();
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
      ((System.ComponentModel.ISupportInitialize)(this.tabChannelList)).EndInit();
      this.tabChannelList.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.popupContext)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpSubList)).EndInit();
      this.grpSubList.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.tabSubList)).EndInit();
      this.tabSubList.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.popupInputSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.popupFavList)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

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
    private DevExpress.XtraEditors.SimpleButton btnRemoveLeft;
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
    private DevExpress.XtraEditors.LabelControl lblSetProgramNr;
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
    private DevExpress.XtraEditors.LabelControl lblInsertMode;
    private DevExpress.XtraGrid.Columns.GridColumn colOutFav;
    private DevExpress.XtraEditors.SimpleButton btnRenum;
    private DevExpress.XtraGrid.Columns.GridColumn colIndex1;
    private DevExpress.XtraGrid.Columns.GridColumn colUid1;
    private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit repositoryItemCheckedComboBoxEdit1;
    private DevExpress.XtraBars.PopupMenu popupContext;
    private DevExpress.XtraBars.BarSubItem miEdit;
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
    private DevExpress.XtraEditors.PictureEdit picDonate;
    private DevExpress.XtraBars.BarButtonItem miOpenWebsite;
    private DevExpress.XtraBars.BarButtonItem miWiki;
    private DevExpress.XtraTab.XtraTabControl tabChannelList;
    private DevExpress.XtraTab.XtraTabPage pageEmpty;
    private DevExpress.XtraEditors.CheckEdit cbCloseGap;
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
    private DevExpress.XtraGrid.Columns.GridColumn colOutServiceType;
    private DevExpress.XtraGrid.Columns.GridColumn colSignalSource;
    private DevExpress.XtraEditors.SimpleButton btnRemoveRight;
    private System.Windows.Forms.Timer timerEditDelay;
    private DevExpress.XtraBars.BarButtonItem miRenameChannel;
    private DevExpress.XtraBars.BarCheckItem miShowWarningsAfterLoad;
    private DevExpress.XtraBars.BarButtonItem miCleanupChannels;
    private DevExpress.XtraBars.BarButtonItem miSaveReferenceFile;
    private DevExpress.XtraBars.BarListItem miRecentFiles;
    private DevExpress.XtraBars.BarButtonItem miExcelExport;
    private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
    private DevExpress.XtraBars.BarButtonItem miPortuguese;
    private DevExpress.XtraEditors.GroupControl grpSubList;
    private DevExpress.XtraTab.XtraTabControl tabSubList;
    private DevExpress.XtraTab.XtraTabPage pageProgNr;
    private DevExpress.XtraBars.BarButtonItem miAddFromRefList;
    private DevExpress.XtraGrid.Columns.GridColumn colOutSkip;
    private DevExpress.XtraGrid.Columns.GridColumn colOutHide;
    private DevExpress.XtraBars.BarButtonItem miPrint;
    private DevExpress.XtraGrid.Columns.GridColumn colProvider;
    private DevExpress.XtraBars.BarButtonItem miRenumFavByPrNr;
    private DevExpress.XtraBars.BarSubItem mnuAccessibility;
    private DevExpress.XtraBars.BarButtonItem miGotoLeftFilter;
    private DevExpress.XtraBars.BarButtonItem miGotoLeftList;
    private DevExpress.XtraBars.BarButtonItem miRightListFilter;
    private DevExpress.XtraBars.BarButtonItem miGotoRightList;
    private DevExpress.XtraBars.PopupMenu popupInputSource;
    private DevExpress.XtraBars.PopupMenu popupFavList;
    private DevExpress.XtraBars.BarButtonItem miSelectFavList0;
    private DevExpress.XtraBars.BarButtonItem miSelectFavListA;
    private DevExpress.XtraBars.BarButtonItem miSelectFavListB;
    private DevExpress.XtraBars.BarButtonItem miSelectFavListC;
    private DevExpress.XtraBars.BarButtonItem miSelectFavListD;
    private DevExpress.XtraBars.BarButtonItem miSelectFavListE;
    private DevExpress.XtraBars.BarSubItem mnuGotoChannelList;
    private DevExpress.XtraBars.BarLinkContainerItem mnuInputSource;
    private DevExpress.XtraBars.BarSubItem mnuGotoFavList;
    private DevExpress.XtraBars.BarLinkContainerItem mnuFavList;
    private DevExpress.XtraBars.BarButtonItem miRussian;
    private DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1;
    private DevExpress.XtraEditors.LabelControl lblPredefinedList;
    private DevExpress.XtraBars.BarButtonItem miAllowEditPredefinedLists;
    private DevExpress.XtraGrid.Columns.GridColumn colPrNr;
    private DevExpress.XtraGrid.Columns.GridColumn colSource;
    private DevExpress.XtraBars.BarButtonItem miCzech;
    private DevExpress.XtraBars.BarButtonItem miRomanian;
    private DevExpress.XtraGrid.Columns.GridColumn colPcrPid;
    private DevExpress.XtraBars.BarButtonItem miExplorerIntegration;
    private DevExpress.XtraBars.BarButtonItem miCheckUpdates;
    private DevExpress.XtraBars.BarButtonItem miUtf8Charset;
    private DevExpress.XtraGrid.Columns.GridColumn colOutDeleted;
    private DevExpress.XtraGrid.Columns.GridColumn colDeleted;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavH;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavG;
    private DevExpress.XtraEditors.SimpleButton btnToggleFavF;
    private DevExpress.XtraBars.BarButtonItem miCopyCsv;
    private DevExpress.XtraGrid.Columns.GridColumn colOutSource;
        private DevExpress.XtraBars.BarButtonItem miSpanish;
        private DevExpress.XtraBars.BarButtonItem miPolski;
        private DevExpress.XtraBars.BarButtonItem miTurkish;
        private DevExpress.XtraBars.BarButtonItem miFontSmall;
        private DevExpress.XtraBars.BarButtonItem miFontMedium;
        private DevExpress.XtraBars.BarButtonItem miFontLarge;
        private DevExpress.XtraBars.BarButtonItem miFontXLarge;
        private DevExpress.XtraBars.BarButtonItem miFontXxLarge;
    }
}

