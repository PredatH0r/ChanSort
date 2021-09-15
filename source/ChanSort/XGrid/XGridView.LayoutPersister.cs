using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraGrid.Columns;

namespace ChanSort
{
  public partial class XGridView
  {
    private List<GridColumn> columnOrder = new();
    private int ignoreEvents;

    #region StoreDefaultColumnOrder()
    private void StoreDefaultColumnOrder()
    {
      // for this to work, the columns in absolute index order must represent the intended visible order
      this.columnOrder.Clear();
      this.columnOrder.AddRange(this.Columns.OrderBy(c => c.AbsoluteIndex));
    }
    #endregion

    #region SetColumnOrder()
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

    #region GetColumnOrder()
    public List<GridColumn> GetColumnOrder() => this.columnOrder.ToList();
    #endregion

    #region RaiseColumnPositionChanged()
    protected override void RaiseColumnPositionChanged(GridColumn column)
    {
      this.OnColumnPositionChanged(column);
      base.RaiseColumnPositionChanged(column);
    }
    #endregion

    #region OnColumnPositionChanged()
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
    #endregion

    #region SetColumnVisiblity()
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
    #endregion
  }
}
