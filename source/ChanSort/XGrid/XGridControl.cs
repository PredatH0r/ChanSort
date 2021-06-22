using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;

namespace ChanSort
{
  [ToolboxItem(true)]
  public class XGridControl : GridControl
  {
    private bool eventsInstalled;
    private readonly Timer timerEditDelay;

    #region ctor
    public XGridControl()
    {
      this.advancedMouseHandling = true;
      
      this.timerEditDelay = new Timer();
      this.timerEditDelay.Interval = 250;
      this.timerEditDelay.Tick += this.timerEditDelay_Tick;
    }
    #endregion

    #region AdvancedMouseHandling
    [DefaultValue(true)]
    public bool AdvancedMouseHandling
    {
      get { return advancedMouseHandling; }
      set
      {
        if (value == this.advancedMouseHandling) return;
        advancedMouseHandling = value;
        this.InstallEvents(this.advancedMouseHandling);
      }
    }
    #endregion

    [DefaultValue(null)]
    public string LayoutId { get; set; }

    [DefaultValue(true)]
    public bool AllowDefaultLoadingLayout { get; set; } = true;

    #region PrintHeader

    [DefaultValue(null)]
    public string PrintHeader { get; set; }

    [DefaultValue(null)]
    public string PrintHeader2 { get; set; }

    #endregion

    #region CreateDefaultView()
    protected override BaseView CreateDefaultView()
    {
      return CreateView("XGridView");
    }
    #endregion

    #region RegisterAvailableViewsCore()
    protected override void RegisterAvailableViewsCore(InfoCollection collection)
    {
      base.RegisterAvailableViewsCore(collection);
      collection.Add(new XGridViewInfoRegistrator());
    }
    #endregion

    #region OnEndInit()
    protected override void OnEndInit()
    {
      base.OnEndInit();
      if (this.AdvancedMouseHandling)
        this.InstallEvents(true);
    }
    #endregion

    #region InstallEvents()
    private void InstallEvents(bool install)
    {
      if (install == this.eventsInstalled) return;
      GridView view = (GridView) this.MainView;
      if (install)
      {
        view.MouseDown += gview_MouseDown;
        view.MouseUp += gview_MouseUp;
        view.ShowingEditor += gview_ShowingEditor;
      }
      else
      {
        view.MouseDown -= gview_MouseDown;
        view.MouseUp -= gview_MouseUp;
        view.ShowingEditor -= gview_ShowingEditor;        
      }
      this.eventsInstalled = install;
    }
    #endregion

    #region IsInputKey()
    protected override bool IsInputKey(Keys keyData)
    {
      if (keyData == (Keys.Control | Keys.C))
        return false;
      return base.IsInputKey(keyData);
    }
    #endregion

    #region OnHandleDestroyed()
    protected override void OnHandleDestroyed(EventArgs e)
    {
      base.OnHandleDestroyed(e);
      this.RaiseSaveLayout();
    }
    #endregion

    #region Dispose()
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.timerEditDelay.Dispose();
        this.RaiseSaveLayout();
      }
      base.Dispose(disposing);
    }
    #endregion

    #region Layout handling - RaiseLoadLayout(), RaiseSaveLayout()

    /// <summary>
    /// Raises the LoadLayout event. Requires that PopulateGrid() has been called before
    /// </summary>
    /// <returns>true if an eventhandler is installed </returns>
    public bool RaiseLoadLayout()
    {
      GridLayoutEventArgs args = new GridLayoutEventArgs(this, this.LayoutId);
      try
      {
        XGridView view = this.MainView as XGridView;
        view?.SaveInitialLayout();
        this.LoadingLayout?.Invoke(this, args);
        if (!args.Handled && this.AllowDefaultLoadingLayout)
          DefaultLoadingLayout?.Invoke(this, args);
      }
      catch { }
      return args.Handled;
    }

    /// <summary>
    /// Raises the SaveLayout event handler to save the grid's column width and ordering.
    /// Requires that PopulateGrid() has been called before
    /// </summary>
    /// <returns>true if an event handler is installed</returns>
    public bool RaiseSaveLayout()
    {
      GridLayoutEventArgs args = new GridLayoutEventArgs(this, this.LayoutId);
      try
      {
        this.SavingLayout?.Invoke(this, args);
        if (!args.Handled)
          DefaultSavingLayout?.Invoke(this, args);
      }
      catch { }
      return args.Handled;
    }
    #endregion

    #region events DefaultSavingLayout, DefaultLoadingLayout, SavingLayout, LoadingLayout

    public delegate void GridLayoutEventHandler(object sender, GridLayoutEventArgs e);

    /// <summary>
    /// This delegate gets called, when no SavingLayout handler is installed on the GridAdapter instance
    /// </summary>
    public static GridLayoutEventHandler DefaultSavingLayout;

    /// <summary>
    /// Gets called when the grid should load its layout
    /// </summary>
    public event GridLayoutEventHandler SavingLayout;

    /// <summary>
    /// This delegate gets called, when no LoadingLayout handler is installed on the GridAdapter instance
    /// </summary>
    public static GridLayoutEventHandler DefaultLoadingLayout;

    /// <summary>
    /// Gets called when the grid should save its layout
    /// </summary>
    public event GridLayoutEventHandler LoadingLayout;
    private bool dontOpenEditor;
    private bool advancedMouseHandling;
    internal bool isExporting;

    #endregion

    #region gview_MouseDown, gview_MouseUp, timerEditDelay_Tick, gview_ShowingEditor

    // these 4 event handler in combination override the default row-selection and editor-opening 
    // behavior of the grid control.

    protected override void OnEnter(EventArgs e)
    {
      this.dontOpenEditor = MouseButtons != 0 && this.RectangleToScreen(this.Bounds).Contains(MousePosition);
      base.OnEnter(e);
    }

    private void gview_MouseDown(object sender, MouseEventArgs e)
    {
      XGridView view = (XGridView)sender;
      int focusedRowHandle = view.FocusedRowHandle;
      var hit = view.CalcHitInfo(e.Location);
      if (!view.IsDataRow(hit.RowHandle) && hit.RowHandle != NewItemRowHandle)
        return;
      if (e.Button == MouseButtons.Left)
      {
        if (ModifierKeys == Keys.None)
        {
          if (hit.RowHandle != focusedRowHandle)
          {
            // diese Zeile hat das Problem, dass wenn eine teilweise am unteren Rand sichtbare Zeile gewählt wird, die TopRow 2x geändert wird und man dann eine andere Zeile markiert
            //SelectFocusedRow(view, hit.RowHandle);
            this.BeginInvoke((Action)(() => view.SelectRow(hit.RowHandle)));
          }
          else if (e.Clicks == 1 && AllowOpenEditorOnMouseDown(view, hit, focusedRowHandle))
          {
            this.dontOpenEditor = false;
            return;
          }
          this.timerEditDelay.Start();
        }
        else
        {
          if (ModifierKeys == Keys.Control && !view.IsRowSelected(hit.RowHandle))
            this.BeginInvoke((Action)(() => view.SelectRow(hit.RowHandle)));
        }
      }
      else if (e.Button == MouseButtons.Right)
      {
        if (!view.IsRowSelected(hit.RowHandle))
          SelectFocusedRow(view, hit.RowHandle);
      }

      this.dontOpenEditor = true;
    }

    private bool AllowOpenEditorOnMouseDown(XGridView view, GridHitInfo gridHit, int focusedRowHandle)
    {
      if (gridHit == null || gridHit.Column == null || !(gridHit.Column.RealColumnEdit is RepositoryItemTextEdit))
        return true;

      // grid focused and cell focused
      if (!this.dontOpenEditor && gridHit.RowHandle == focusedRowHandle && gridHit.Column == view.FocusedColumn)
        return true; // !this.dontOpenEditor;

      bool buttonVisible;
      if (view.OptionsView.ShowButtonMode == ShowButtonModeEnum.ShowAlways)
        buttonVisible = true;
      else if (view.OptionsView.ShowButtonMode == ShowButtonModeEnum.ShowForFocusedRow)
        buttonVisible = gridHit.RowHandle == focusedRowHandle;
      else if (view.OptionsView.ShowButtonMode == ShowButtonModeEnum.ShowForFocusedCell)
        buttonVisible = gridHit.RowHandle == focusedRowHandle && gridHit.Column == view.FocusedColumn;
      else
        buttonVisible = false;

      if (!buttonVisible)
        return false;

      var gridViewInfo = view.ViewInfo;
      var cellInfo = gridViewInfo.GetGridCellInfo(view.FocusedRowHandle, view.FocusedColumn);
      ButtonEditViewInfo editViewInfo = cellInfo.ViewInfo as ButtonEditViewInfo;
      if (editViewInfo == null)
        return !this.dontOpenEditor; // standard text edit (w/o buttons)

      var point = gridHit.HitPoint;
      point.Offset(-cellInfo.Bounds.Left, -cellInfo.Bounds.Top);
      var hit = editViewInfo.CalcHitInfo(point);
      return hit.HitTest == EditHitTest.Button || hit.HitTest == EditHitTest.Button2;
    }

    private void gview_MouseUp(object sender, MouseEventArgs e)
    {
      this.timerEditDelay.Stop();
      this.BeginInvoke((Action)(() => { this.dontOpenEditor = false; }));
    }

    private void timerEditDelay_Tick(object sender, EventArgs e)
    {
      this.timerEditDelay.Stop();
      this.dontOpenEditor = false;
      if (this.MainView != null)
        this.MainView.ShowEditor();
    }

    private void gview_ShowingEditor(object sender, CancelEventArgs e)
    {
      if (this.dontOpenEditor)
        e.Cancel = true;
    }
    #endregion

    #region SelectFocusedRow()
    private void SelectFocusedRow(GridView grid, int rowHandle)
    {
      try
      {
        grid.BeginSelection();
        grid.ClearSelection();
        grid.FocusedRowHandle = rowHandle;
      }
      catch (DevExpress.Utils.HideException) { }
      finally
      {
        grid.SelectRow(grid.FocusedRowHandle);
        grid.EndSelection();
      }
    }
    #endregion

    #region ShowExportDialog()
    public void ShowExportDialog()
    {
      using (var dlg = new SaveFileDialog())
      {
        dlg.Title = "Tabelle exportieren";
        dlg.RestoreDirectory = true;
        dlg.AddExtension = true;
        dlg.DefaultExt = ".xlsx";
        dlg.Filter = "Excel Datei|*.xlsx|PDF-Dokument|*.pdf|CSV-Tabelle|*.csv|Rich-Text|*.rtf";
        dlg.FilterIndex = 1;
        dlg.AutoUpgradeEnabled = true;
        dlg.CheckPathExists = true;
        dlg.ValidateNames = true;
        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        try
        {
          this.isExporting = true;
          switch ((Path.GetExtension(dlg.FileName) ?? "").ToLower())
          {
            case ".xlsx":
              this.ExportToXlsx(dlg.FileName);
              break;
            case ".csv":
              this.ExportToCsv(dlg.FileName);
              break;
            case ".pdf":
              this.ExportToPdf(dlg.FileName);
              break;
            case ".rtf":
              this.ExportToRtf(dlg.FileName);
              break;
            default:
              TextExportOptions opt = new TextExportOptions();
              opt.Separator = "\t";
              this.ExportToText(dlg.FileName, opt);
              break;
          }
        }
        finally
        {
          this.isExporting = false;
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.UseShellExecute = true;
        psi.FileName = dlg.FileName;
        Process.Start(psi);
      }
    }
    #endregion

    #region OnKeyDown()
    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.KeyData == (Keys.Add | Keys.Control))
        e.Handled = ((XGridView) this.MainView).SmartExpand();
      else if (e.KeyData == (Keys.Subtract | Keys.Control))
        e.Handled = ((XGridView) this.MainView).SmartCollapse();
      base.OnKeyDown(e);
    }
    #endregion

    #region OnSizeChanged() / OnLayout()
    protected override void OnSizeChanged(EventArgs e)
    {
      // attempt to work around a windows bug/limitation with kernel stack overflow when controls are nested too deeply:
      // http://blogs.msdn.com/b/alejacma/archive/2008/11/20/controls-won-t-get-resized-once-the-nesting-hierarchy-of-windows-exceeds-a-certain-depth-x64.aspx?Redirected=true

      if (this.IsHandleCreated)
        this.BeginInvoke((Action)(() => base.OnSizeChanged(e)));
      else
        base.OnSizeChanged(e);
    }

    #endregion
  }
}
