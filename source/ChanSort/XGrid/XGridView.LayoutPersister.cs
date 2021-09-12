using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ChanSort.Api;
using DevExpress.Data;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace ChanSort
{
  public partial class XGridView
  {
    public void StoreVisibleOrder()
    {
      // place invisible column based on the absolute order
      this.columnOrder.Clear();
      int visIndex = 0;
      var comparer = new DelegateComparer<GridColumn>((a, b) => Tools.FirstNotDefault(a.VisibleIndex.CompareTo(b.VisibleIndex), a.AbsoluteIndex.CompareTo(b.AbsoluteIndex)));
      var cleanVisList = this.Columns.Where(c => c.VisibleIndex >= 0).OrderBy(c => c, comparer).ToList();
      foreach (GridColumn col in this.Columns)
      {
        if (!col.Visible)
          this.columnOrder.Add(col);
        else
        {
          this.columnOrder.Add(cleanVisList[visIndex]);
          visIndex++;
        }
      }
    }

  }
}
