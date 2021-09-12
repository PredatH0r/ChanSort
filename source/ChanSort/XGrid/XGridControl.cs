using System;
using System.ComponentModel;
using System.Windows.Forms;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace ChanSort
{
  [ToolboxItem(true)]
  public class XGridControl : GridControl
  {
    private bool eventsInstalled;
    private readonly Timer timerEditDelay;
    private bool dontOpenEditor;
    private bool advancedMouseHandling;

    #region ctor
    public XGridControl()
    {
      this.advancedMouseHandling = true;
      
      this.timerEditDelay = new Timer();
      this.timerEditDelay.Interval = 250;
      this.timerEditDelay.Tick += this.timerEditDelay_Tick;
    }
    #endregion

    #region Dispose()
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.timerEditDelay.Dispose();
      }
      base.Dispose(disposing);
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
  }
}
