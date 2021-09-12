using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ChanSort.Api;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.Handler;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.Handler;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace ChanSort
{
  public partial class XGridView : GridView
  {
    private readonly List<GridColumn> columnOrder = new List<GridColumn>();
    protected override string ViewName => "XGridView";

    #region ctor

    public XGridView() : this(null) { }
    public XGridView(GridControl grid) : base(grid)
    {
    }
    #endregion

    public override void EndInit()
    {
      base.EndInit();
      this.StoreVisibleOrder();
    }

    protected override void SetColumnVisibleIndex(GridColumn column, int newValue)
    {
      int oldVisIndex = column.VisibleIndex;
      int newIdx = newValue > column.VisibleIndex ? newValue - 1 : newValue;
      base.SetColumnVisibleIndex(column, newValue);

      if (newIdx < 0 || oldVisIndex == newIdx)
      {
        // hide or no change: keep as-is
      }
      else if (newIdx >= 0)
      {
        // move
        columnOrder.Remove(column);
        if (newIdx == 0)
          columnOrder.Insert(0, column);
        else
        {
          var afterCol = this.VisibleColumns[column.VisibleIndex - 1];
          columnOrder.Insert(columnOrder.IndexOf(afterCol) + 1, column);
        }
      }
    }

    protected override void OnColumnDeleted(GridColumn column)
    {
      this.columnOrder.Remove(column);
      base.OnColumnDeleted(column);
    }

    protected override void OnColumnAdded(GridColumn column)
    {
      base.OnColumnAdded(column);
      if (this.IsInitialized)
        this.StoreVisibleOrder();
    }

    public void SetColumnVisibility(GridColumn column, bool visible)
    {
      if (column.Visible == visible)
        return;
      if (!visible)
      {
        column.Visible = false;
        return;
      }

      int idx = 0;
      foreach (var col in this.columnOrder)
      {
        if (col == column)
        {
          col.VisibleIndex = idx;
          return;
        }

        if (col.Visible)
          ++idx;
      }

      // fallback
      column.VisibleIndex = this.VisibleColumns.Count;
    }

    internal new GridViewInfo ViewInfo => base.ViewInfo;


    #region ShowEditor()
    public override void ShowEditor()
    {
      base.ShowEditor();
      if (this.ActiveEditor == null)
        return;

      var textEdit = this.ActiveEditor as TextEdit;
      if (textEdit != null)
        textEdit.KeyDown += textEdit_KeyDown;

      if (!this.IsNewItemRow(this.FocusedRowHandle))
        return;

      bool cleared = false;
      if (this.SortedColumns.Count > 0)
      {
        this.ClearSorting();
        cleared = true;
      }
      if (this.GroupCount > 0)
      {
        this.ClearGrouping();
        cleared = true;
      }
      if (cleared)
        this.GridControl.BeginInvoke((Action)this.EditFocusedRow);
    }
    #endregion

    #region HideEditor()
    public override void HideEditor()
    {
      var textEdit = this.ActiveEditor as TextEdit;
      if (textEdit != null)
        textEdit.KeyDown -= textEdit_KeyDown;

      base.HideEditor();
    }
    #endregion

    #region textEdit_KeyDown
    private void textEdit_KeyDown(object sender, KeyEventArgs e)
    {
      var textEdit = (TextEdit)sender;
      if (textEdit.SelectionStart == 0 && textEdit.SelectionLength == textEdit.Text.Length)
      {
        if (e.KeyData == Keys.Left)
          textEdit.SelectionStart += textEdit.SelectionLength;
        else if (e.KeyData == Keys.Right)
          textEdit.SelectionLength = 0;
      }
    }
    #endregion

    #region EditFocusedRow()
    private void EditFocusedRow()
    {
      this.FocusedRowHandle = GridControl.NewItemRowHandle;
      this.ShowEditor();
    }
    #endregion
  }

  #region class XGridViewInfoRegistrator
  internal class XGridViewInfoRegistrator : GridInfoRegistrator
  {
    public override string ViewName => "XGridView";
    public override BaseView CreateView(GridControl grid) => new XGridView(grid);
    public override BaseViewInfo CreateViewInfo(BaseView view) => new GridViewInfo(view as XGridView);
    public override BaseViewHandler CreateHandler(BaseView view) => new GridHandler(view as XGridView);
  }
  #endregion

}