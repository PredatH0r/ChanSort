using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DevExpress.Data;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace ChanSort
{
  #region class GridLayoutEventArgs
  public class GridLayoutEventArgs : EventArgs
  {
    public readonly XGridControl GridControl;
    public readonly XGridView GridView;
    public readonly string ControlId;
    public bool Handled = false;

    public GridLayoutEventArgs(XGridControl gc, string controlId)
    {
      this.GridControl = gc;
      this.GridView = (XGridView)gc.MainView;
      this.ControlId = controlId;
    }
  }
  #endregion

  public partial class XGridView
  {
    #region enum ColumnOptions

    [Flags]
    public enum ColumnOptions
    {
      None = 0x00,
      OrderAndVisibility = 0x01,
      SortingAndGrouping = 0x02,
      All = 0x03,
    }

    #endregion

    #region class GridColumnInfo

    internal class GridColumnInfo
    {
      public readonly string FieldName;
      public int Width;
      public int VisibleIndex;
      public int InvisibleIndex;
      public int SortIndex;
      public ColumnSortOrder SortOrder;
      public int GroupIndex;

      public GridColumnInfo(string fieldName)
      {
        this.FieldName = fieldName;
        this.SortIndex = -1;
        this.GroupIndex = -1;
        this.InvisibleIndex = -1;
      }
    }

    #endregion

    public static float ColumnWidthDpiScale = 1f; // used to work around the autoscaling of DevExpress 18.1 grid column width

    #region SaveLayoutToXml()

    /// <summary>
    /// Save column widths and visible indices to a XmlWriter
    /// </summary>
    public string SaveLayoutToXml()
    {
      var gridView = this;
      var sb = new StringBuilder();
      var settings = new XmlWriterSettings();
      settings.OmitXmlDeclaration = true;
      using var xmlWriter = XmlWriter.Create(sb, settings);
      xmlWriter.WriteStartElement("GridColumns");
      xmlWriter.WriteAttributeString("widthScale", ColumnWidthDpiScale.ToString(NumberFormatInfo.InvariantInfo));
      int count = gridView.Columns.Count;
      for (int i = 0; i < count; i++)
      {
        GridColumn col = gridView.Columns[i];
        xmlWriter.WriteRaw("\r\n");
        xmlWriter.WriteStartElement("Column");
        xmlWriter.WriteAttributeString("FieldName", col.FieldName);
        xmlWriter.WriteElementString("GroupIndex", col.GroupIndex.ToString());
        xmlWriter.WriteElementString("VisibleIndex", col.VisibleIndex.ToString());
        xmlWriter.WriteElementString("InvisibleIndex", this.columnOrder.IndexOf(col).ToString());
        xmlWriter.WriteElementString("Width", col.Width.ToString());
        xmlWriter.WriteElementString("SortIndex", col.SortIndex.ToString());
        xmlWriter.WriteElementString("SortOrder", col.SortOrder.ToString());
        xmlWriter.WriteEndElement();
      }

      xmlWriter.WriteRaw("\r\n");
      xmlWriter.WriteEndElement();
      xmlWriter.Flush();
      return sb.ToString();
    }

    #endregion

    #region LoadLayoutFromXml()

    /// <summary>
    /// Load column widths and visible indices from a XmlReader
    /// </summary>
    public void LoadLayoutFromXml(string xml, ColumnOptions options = ColumnOptions.All)
    {
      if (string.IsNullOrEmpty(xml))
        return;

      var gridView = this;

      XmlDocument doc = new XmlDocument();
      doc.Load(new StringReader(xml));
      XmlNode root = doc.FirstChild;
      List<GridColumnInfo> cols = new List<GridColumnInfo>();

      // gültiges Root-Element prüfen
      if (!root.Name.Equals("GridColumns"))
        return;

      var ws = root.Attributes?["widthScale"]?.Value;
      var storedScale = ws != null ? float.Parse(ws, CultureInfo.InvariantCulture) : ColumnWidthDpiScale;
      float scaleCorrection = gridView.GridControl.ScaleFactor.Width / storedScale;

      for (XmlNode node = root.FirstChild; node != null; node = node.NextSibling)
      {
        var ci = ReadColumnInfoAndSetWidth(node, gridView, scaleCorrection);
        if (ci != null)
          cols.Add(ci);
      }

      //gridView.BeginUpdate();
      if ((options & ColumnOptions.OrderAndVisibility) != 0)
        RestoreVisibleOrder(cols);
      if ((options & ColumnOptions.SortingAndGrouping) != 0)
        RestoreGroupingAndSorting(gridView, cols);
      //gridView.EndUpdate();
    }

    #endregion

    #region ReadColumnInfoAndSetWidth()

    private static GridColumnInfo ReadColumnInfoAndSetWidth(XmlNode node, GridView gridView, float scaleCorrection)
    {
      if (!node.Name.Equals("Column"))
        return null;
      var fieldName = node.Attributes["FieldName"].Value;
      var col = gridView.Columns[fieldName];
      var ci = new GridColumnInfo(fieldName);
      var conv = new EnumConverter(typeof(ColumnSortOrder));
      for (XmlNode item = node.FirstChild; item != null; item = item.NextSibling)
      {
        if (item.Name.Equals("Width"))
        {
          ci.Width = Convert.ToInt32(item.InnerText);
          if (col != null)
            col.Width = (int) Math.Round(ci.Width * scaleCorrection, 0);
        }
        else if (item.Name.Equals("VisibleIndex"))
          ci.VisibleIndex = Convert.ToInt32(item.InnerText);
        else if (item.Name == "InvisibleIndex")
          ci.InvisibleIndex = Convert.ToInt32(item.InnerText);
        else if (item.Name.Equals("SortIndex"))
          ci.SortIndex = Convert.ToInt32(item.InnerText);
        else if (item.Name.Equals("SortOrder"))
          ci.SortOrder = (ColumnSortOrder) conv.ConvertFromString(item.InnerText);
        else if (item.Name.Equals("GroupIndex"))
          ci.GroupIndex = int.Parse(item.InnerText);
      }

      return ci;
    }

    #endregion

    #region RestoreVisibleOrder()
    internal void RestoreVisibleOrder(List<GridColumnInfo> cols)
    {
      var gridView = this;
      var frow = gridView.FocusedRowHandle;
      var fcol = gridView.FocusedColumn;
      gridView.BeginUpdate();
      gridView.ClearGrouping();

      var oldVisibleColumns = this.VisibleColumns.ToList();
      foreach (GridColumn col in gridView.Columns)
        col.VisibleIndex = -1;


      // place new column based on the original order
      int visIndex = 0;
      var newVisList = new List<GridColumn>();
      bool hasInvisOrder = cols.Any(ci => ci.InvisibleIndex >= 0);
      var visibleColumns = new List<GridColumn>();
      foreach (GridColumn col in this.columnOrder)
      {
        if (!cols.Any(c => c.FieldName == col.FieldName && (c.InvisibleIndex >= 0 || c.VisibleIndex >= 0)))
        {
          newVisList.Add(col);
          if (oldVisibleColumns.Contains(col))
            visibleColumns.Add(col);
        }
        else
        {
          while (visIndex < cols.Count)
          {
            var ci = cols.FirstOrDefault(c => hasInvisOrder ? c.InvisibleIndex == visIndex : c.VisibleIndex == visIndex);
            visIndex++;
            var visCol = ci != null ? this.Columns[ci.FieldName] : null;
            if (visCol != null)
            {
              newVisList.Add(visCol);
              if (ci.VisibleIndex >= 0)
                visibleColumns.Add(visCol);
              break;
            }
          }
        }
      }

      visIndex = 0;
      foreach (var col in newVisList)
        col.VisibleIndex = visibleColumns.Contains(col) ? visIndex++ : -1;

      this.columnOrder.Clear();
      this.columnOrder.AddRange(newVisList);

      gridView.FocusedRowHandle = frow;
      if (fcol != null && fcol.Visible)
        gridView.FocusedColumn = fcol;
      gridView.EndUpdate();
    }

    #endregion

    #region RestoreGroupingAndSorting()

    private static void RestoreGroupingAndSorting(GridView gridView, List<GridColumnInfo> cols)
    {
      bool mayGroup = gridView.OptionsCustomization.AllowGroup;
      int groupCount = 0;
      List<GridColumnInfo> sortColumns = new List<GridColumnInfo>();
      foreach (GridColumnInfo ci in cols.Where(ci => ci.SortIndex >= 0 || ci.GroupIndex >= 0).OrderBy(ci => ci.GroupIndex + (ci.SortIndex + 1) * 1000))
      {
        sortColumns.Add(ci);
        if (ci.GroupIndex >= 0)
          ++groupCount;
      }

      GridColumnSortInfo[] sortInfo = new GridColumnSortInfo[sortColumns.Count];
      int i = 0;
      foreach (var ci in sortColumns)
        sortInfo[i++] = new GridColumnSortInfo(gridView.Columns[ci.FieldName], ci.SortOrder);

      gridView.SortInfo.ClearAndAddRange(sortInfo, mayGroup ? groupCount : 0);
      gridView.OptionsView.ShowGroupPanel |= mayGroup && groupCount > 0;
    }

    #endregion
  }
}
