namespace ChanSort.Ui
{
  partial class ChannelListReport
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.bandListDetail = new DevExpress.XtraReports.UI.DetailBand();
      this.txtHeading = new DevExpress.XtraReports.UI.XRLabel();
      this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
      this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
      this.repChannels = new DevExpress.XtraReports.UI.DetailReportBand();
      this.bandChannelDetail = new DevExpress.XtraReports.UI.DetailBand();
      this.txtNumber = new DevExpress.XtraReports.UI.XRLabel();
      this.txtChannelName = new DevExpress.XtraReports.UI.XRLabel();
      this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
      // 
      // bandListDetail
      // 
      this.bandListDetail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.txtHeading});
      this.bandListDetail.Dpi = 254F;
      this.bandListDetail.HeightF = 40F;
      this.bandListDetail.Name = "bandListDetail";
      this.bandListDetail.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 254F);
      this.bandListDetail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
      // 
      // txtHeading
      // 
      this.txtHeading.Dpi = 254F;
      this.txtHeading.Font = new System.Drawing.Font("Times New Roman", 10F);
      this.txtHeading.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
      this.txtHeading.Name = "txtHeading";
      this.txtHeading.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 0, 0, 254F);
      this.txtHeading.SizeF = new System.Drawing.SizeF(1801F, 40F);
      this.txtHeading.StylePriority.UseFont = false;
      this.txtHeading.Text = "Heading";
      // 
      // TopMargin
      // 
      this.TopMargin.Dpi = 254F;
      this.TopMargin.HeightF = 83.1875F;
      this.TopMargin.Name = "TopMargin";
      this.TopMargin.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 254F);
      this.TopMargin.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
      // 
      // BottomMargin
      // 
      this.BottomMargin.Dpi = 254F;
      this.BottomMargin.Font = new System.Drawing.Font("Arial", 9.75F);
      this.BottomMargin.HeightF = 119.0625F;
      this.BottomMargin.Name = "BottomMargin";
      this.BottomMargin.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 254F);
      this.BottomMargin.StylePriority.UseFont = false;
      this.BottomMargin.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
      // 
      // repChannels
      // 
      this.repChannels.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.bandChannelDetail});
      this.repChannels.DataMember = "Channels";
      this.repChannels.DataSource = this.bindingSource1;
      this.repChannels.Dpi = 254F;
      this.repChannels.Level = 0;
      this.repChannels.Name = "repChannels";
      // 
      // bandChannelDetail
      // 
      this.bandChannelDetail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.txtNumber,
            this.txtChannelName});
      this.bandChannelDetail.Dpi = 254F;
      this.bandChannelDetail.HeightF = 35F;
      this.bandChannelDetail.KeepTogether = true;
      this.bandChannelDetail.MultiColumn.ColumnCount = 3;
      this.bandChannelDetail.MultiColumn.ColumnSpacing = 50F;
      this.bandChannelDetail.MultiColumn.Mode = DevExpress.XtraReports.UI.MultiColumnMode.UseColumnCount;
      this.bandChannelDetail.Name = "bandChannelDetail";
      this.bandChannelDetail.BeforePrint += new System.Drawing.Printing.PrintEventHandler(this.bandChannelDetail_BeforePrint);
      // 
      // txtNumber
      // 
      this.txtNumber.Dpi = 254F;
      this.txtNumber.Font = new System.Drawing.Font("Times New Roman", 8F);
      this.txtNumber.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
      this.txtNumber.Name = "txtNumber";
      this.txtNumber.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 0, 0, 254F);
      this.txtNumber.SizeF = new System.Drawing.SizeF(89.95834F, 35F);
      this.txtNumber.StylePriority.UseFont = false;
      this.txtNumber.Text = "0000";
      // 
      // txtChannelName
      // 
      this.txtChannelName.Dpi = 254F;
      this.txtChannelName.Font = new System.Drawing.Font("Times New Roman", 8F);
      this.txtChannelName.LocationFloat = new DevExpress.Utils.PointFloat(89.95834F, 0F);
      this.txtChannelName.Name = "txtChannelName";
      this.txtChannelName.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 0, 0, 254F);
      this.txtChannelName.SizeF = new System.Drawing.SizeF(489.4792F, 35F);
      this.txtChannelName.StylePriority.UseFont = false;
      this.txtChannelName.Text = "Empty channel list";
      // 
      // bindingSource1
      // 
      this.bindingSource1.DataSource = typeof(ChanSort.Api.ChannelList);
      // 
      // ChannelListReport
      // 
      this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.bandListDetail,
            this.TopMargin,
            this.BottomMargin,
            this.repChannels});
      this.DataSource = this.bindingSource1;
      this.Dpi = 254F;
      this.Margins = new System.Drawing.Printing.Margins(148, 111, 83, 119);
      this.PageHeight = 2970;
      this.PageWidth = 2100;
      this.PaperKind = System.Drawing.Printing.PaperKind.A4;
      this.ReportUnit = DevExpress.XtraReports.UI.ReportUnit.TenthsOfAMillimeter;
      this.SnapGridSize = 25F;
      this.Version = "19.2";
      ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

    }

    #endregion

    private DevExpress.XtraReports.UI.DetailBand bandListDetail;
    private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
    private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
    private DevExpress.XtraReports.UI.XRLabel txtHeading;
    private DevExpress.XtraReports.UI.DetailReportBand repChannels;
    private DevExpress.XtraReports.UI.DetailBand bandChannelDetail;
    private DevExpress.XtraReports.UI.XRLabel txtNumber;
    private DevExpress.XtraReports.UI.XRLabel txtChannelName;
    private System.Windows.Forms.BindingSource bindingSource1;
  }
}
