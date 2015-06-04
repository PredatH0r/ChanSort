namespace ChanSort.Ui.Printing
{
  partial class ReportOptionsDialog
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportOptionsDialog));
      this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
      this.rbSortByNumber = new DevExpress.XtraEditors.CheckEdit();
      this.rbSortByName = new DevExpress.XtraEditors.CheckEdit();
      this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
      this.fontEdit1 = new DevExpress.XtraEditors.FontEdit();
      this.btnPreview = new DevExpress.XtraEditors.SimpleButton();
      this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
      this.spinFontSize = new DevExpress.XtraEditors.SpinEdit();
      ((System.ComponentModel.ISupportInitialize)(this.rbSortByNumber.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbSortByName.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fontEdit1.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.spinFontSize.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // labelControl1
      // 
      resources.ApplyResources(this.labelControl1, "labelControl1");
      this.labelControl1.Name = "labelControl1";
      // 
      // rbSortByNumber
      // 
      resources.ApplyResources(this.rbSortByNumber, "rbSortByNumber");
      this.rbSortByNumber.Name = "rbSortByNumber";
      this.rbSortByNumber.Properties.Caption = resources.GetString("rbSortByNumber.Properties.Caption");
      this.rbSortByNumber.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbSortByNumber.Properties.RadioGroupIndex = 1;
      // 
      // rbSortByName
      // 
      resources.ApplyResources(this.rbSortByName, "rbSortByName");
      this.rbSortByName.Name = "rbSortByName";
      this.rbSortByName.Properties.Caption = resources.GetString("rbSortByName.Properties.Caption");
      this.rbSortByName.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbSortByName.Properties.RadioGroupIndex = 1;
      this.rbSortByName.TabStop = false;
      // 
      // labelControl2
      // 
      resources.ApplyResources(this.labelControl2, "labelControl2");
      this.labelControl2.Name = "labelControl2";
      // 
      // fontEdit1
      // 
      resources.ApplyResources(this.fontEdit1, "fontEdit1");
      this.fontEdit1.Name = "fontEdit1";
      this.fontEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("fontEdit1.Properties.Buttons"))))});
      // 
      // btnPreview
      // 
      resources.ApplyResources(this.btnPreview, "btnPreview");
      this.btnPreview.Name = "btnPreview";
      this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      // 
      // spinFontSize
      // 
      resources.ApplyResources(this.spinFontSize, "spinFontSize");
      this.spinFontSize.Name = "spinFontSize";
      this.spinFontSize.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("spinFontSize.Properties.Buttons"))))});
      this.spinFontSize.Properties.DisplayFormat.FormatString = "0.#";
      this.spinFontSize.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.spinFontSize.Properties.Mask.ShowPlaceHolders = ((bool)(resources.GetObject("spinFontSize.Properties.Mask.ShowPlaceHolders")));
      // 
      // ReportOptionsDialog
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.spinFontSize);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnPreview);
      this.Controls.Add(this.fontEdit1);
      this.Controls.Add(this.labelControl2);
      this.Controls.Add(this.rbSortByName);
      this.Controls.Add(this.rbSortByNumber);
      this.Controls.Add(this.labelControl1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ReportOptionsDialog";
      ((System.ComponentModel.ISupportInitialize)(this.rbSortByNumber.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbSortByName.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fontEdit1.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.spinFontSize.Properties)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraEditors.LabelControl labelControl1;
    private DevExpress.XtraEditors.CheckEdit rbSortByNumber;
    private DevExpress.XtraEditors.CheckEdit rbSortByName;
    private DevExpress.XtraEditors.LabelControl labelControl2;
    private DevExpress.XtraEditors.FontEdit fontEdit1;
    private DevExpress.XtraEditors.SimpleButton btnPreview;
    private DevExpress.XtraEditors.SimpleButton btnCancel;
    private DevExpress.XtraEditors.SpinEdit spinFontSize;
  }
}