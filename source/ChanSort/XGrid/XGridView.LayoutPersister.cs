using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ChanSort.Api;
using DevExpress.Data;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace ChanSort
{
  public partial class XGridView
  {
    private List<GridColumn> columnOrder = new();
    private int ignoreEvents;

    private void StoreDefaultColumnOrder()
    {
      // for this to work, the columns in absolute index order must represent the intended visible order
      this.columnOrder.Clear();
      this.columnOrder.AddRange(this.Columns.OrderBy(c => c.AbsoluteIndex));
    }

    #region SetColumnOrder
    public void SetColumnOrder(IEnumerable<string> names, bool updateVisibleIndex = true)
    {
      // must handle situations where new columns were added to the program, which are not included in the config
      // These columns should be kept at their relative position in the default visible order

      var fieldNames = names.ToList();

      // build a dictionary with FieldName => "desired visible order"
      var colsInConfig = new Dictionary<string, int>();
      int visIndex = 0;
      foreach (var name in fieldNames)
      {
        if (this.Columns[name] != null)
          colsInConfig.Add(name, visIndex++);
      }

      ++this.ignoreEvents;
      var oldList = new List<GridColumn>(this.columnOrder);
      var newList = new List<GridColumn>();
      foreach (var name in fieldNames)
      {
        var col = this.Columns[name];

        // ignore columns from config that don't exist in the program
        if (col == null)
          continue;

        // prepend columns that don't exist in the config and come before the current column in the default order
        while (oldList.Count > 0)
        {
          var oldCol = oldList[0];
          if (oldCol == col)
            break;
          if (colsInConfig.ContainsKey(oldCol.FieldName))
            break;
          newList.Add(oldCol);
          oldList.Remove(oldCol);
        }

        newList.Add(col);
        oldList.Remove(col);
      }

      newList.AddRange(oldList);
      this.columnOrder = newList;


      if (updateVisibleIndex)
      {
        visIndex = 0;
        foreach (var col in newList)
          col.VisibleIndex = col.Visible ? visIndex++ : -1;
      }

      --this.ignoreEvents;
    }
    #endregion

    public List<GridColumn> GetColumnOrder() => this.columnOrder.ToList();

    protected override void RaiseColumnPositionChanged(GridColumn column)
    {
      this.OnColumnPositionChanged(column);
      base.RaiseColumnPositionChanged(column);
    }

    private void OnColumnPositionChanged(GridColumn col)
    {
      // internal reordering should be ignored
      if (this.ignoreEvents > 0)
        return;

      // columnOrderLeft and columnOrderRight are kept in desired column order including hidden columns
      // when a column is moved to a new visible position, it is put behind all columns (including invisible ones) that have a lower position

      var visIdx = col.VisibleIndex;
      if (visIdx < 0)
        return;

      var list = this.columnOrder;
      var listIdx = list.IndexOf(col);
      list.RemoveAt(listIdx);
      int i;
      for (i = 0; i < list.Count; i++)
      {
        if (list[i].VisibleIndex >= 0 && list[i].VisibleIndex >= visIdx)
          break;
      }
      list.Insert(i, col);
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

    //protected override void SetColumnVisibleIndex(GridColumn column, int newValue)
    //{
    //  int oldVisIndex = column.VisibleIndex;
    //  int newIdx = newValue > column.VisibleIndex ? newValue - 1 : newValue;
    //  base.SetColumnVisibleIndex(column, newValue);

    //  if (newIdx < 0 || oldVisIndex == newIdx)
    //  {
    //    // hide or no change: keep as-is
    //  }
    //  else if (newIdx >= 0)
    //  {
    //    // move
    //    columnOrder.Remove(column);
    //    if (newIdx == 0)
    //      columnOrder.Insert(0, column);
    //    else
    //    {
    //      var afterCol = this.VisibleColumns[column.VisibleIndex - 1];
    //      columnOrder.Insert(columnOrder.IndexOf(afterCol) + 1, column);
    //    }
    //  }
    //}
  }
}
