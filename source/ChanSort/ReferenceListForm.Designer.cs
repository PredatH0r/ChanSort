namespace ChanSort.Ui
{
  partial class ReferenceListForm
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
      DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
      this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
      this.edFile = new DevExpress.XtraEditors.ButtonEdit();
      this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
      this.rbAuto = new DevExpress.XtraEditors.CheckEdit();
      this.rbManual = new DevExpress.XtraEditors.CheckEdit();
      this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
      this.comboSource = new DevExpress.XtraEditors.ComboBoxEdit();
      this.comboTarget = new DevExpress.XtraEditors.ComboBoxEdit();
      this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
      this.cbTv = new DevExpress.XtraEditors.CheckEdit();
      this.cbRadio = new DevExpress.XtraEditors.CheckEdit();
      this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
      this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
      this.comboPrNr = new DevExpress.XtraEditors.ComboBoxEdit();
      this.grpManual = new DevExpress.XtraEditors.GroupControl();
      this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
      this.cbAnalog = new DevExpress.XtraEditors.CheckEdit();
      this.cbDigital = new DevExpress.XtraEditors.CheckEdit();
      this.lblTargetInfo = new DevExpress.XtraEditors.LabelControl();
      this.lblSourceInfo = new DevExpress.XtraEditors.LabelControl();
      this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
      this.btnApply = new DevExpress.XtraEditors.SimpleButton();
      this.btnOk = new DevExpress.XtraEditors.SimpleButton();
      this.btnClose = new DevExpress.XtraEditors.SimpleButton();
      ((System.ComponentModel.ISupportInitialize)(this.edFile.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbAuto.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbManual.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comboSource.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comboTarget.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbTv.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbRadio.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comboPrNr.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpManual)).BeginInit();
      this.grpManual.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cbAnalog.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbDigital.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // labelControl1
      // 
      this.labelControl1.Location = new System.Drawing.Point(12, 16);
      this.labelControl1.Name = "labelControl1";
      this.labelControl1.Size = new System.Drawing.Size(92, 13);
      this.labelControl1.TabIndex = 0;
      this.labelControl1.Text = "Reference List File:";
      // 
      // edFile
      // 
      this.edFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edFile.EditValue = "← press button to select a file";
      this.edFile.Location = new System.Drawing.Point(133, 13);
      this.edFile.Name = "edFile";
      this.edFile.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Ellipsis, "", -1, true, true, true, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, "", null, null, true)});
      this.edFile.Properties.ReadOnly = true;
      this.edFile.Size = new System.Drawing.Size(547, 20);
      this.edFile.TabIndex = 1;
      this.edFile.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.edFile_ButtonClick);
      // 
      // labelControl2
      // 
      this.labelControl2.Location = new System.Drawing.Point(133, 39);
      this.labelControl2.Name = "labelControl2";
      this.labelControl2.Size = new System.Drawing.Size(318, 13);
      this.labelControl2.TabIndex = 2;
      this.labelControl2.Text = "(You can choose any supported channel list file as a reference list)";
      // 
      // rbAuto
      // 
      this.rbAuto.Enabled = false;
      this.rbAuto.Location = new System.Drawing.Point(13, 79);
      this.rbAuto.Name = "rbAuto";
      this.rbAuto.Properties.AutoWidth = true;
      this.rbAuto.Properties.Caption = "Automatically reorder all lists in the current file to match the reference file";
      this.rbAuto.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbAuto.Properties.RadioGroupIndex = 1;
      this.rbAuto.Size = new System.Drawing.Size(375, 19);
      this.rbAuto.TabIndex = 3;
      this.rbAuto.TabStop = false;
      this.rbAuto.CheckedChanged += new System.EventHandler(this.rbAuto_CheckedChanged);
      // 
      // rbManual
      // 
      this.rbManual.Enabled = false;
      this.rbManual.Location = new System.Drawing.Point(13, 104);
      this.rbManual.Name = "rbManual";
      this.rbManual.Properties.AutoWidth = true;
      this.rbManual.Properties.Caption = "Reorder only a particular list to match a selected reference list";
      this.rbManual.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
      this.rbManual.Properties.RadioGroupIndex = 1;
      this.rbManual.Size = new System.Drawing.Size(320, 19);
      this.rbManual.TabIndex = 4;
      this.rbManual.TabStop = false;
      this.rbManual.CheckedChanged += new System.EventHandler(this.rbAuto_CheckedChanged);
      // 
      // labelControl3
      // 
      this.labelControl3.Location = new System.Drawing.Point(5, 36);
      this.labelControl3.Name = "labelControl3";
      this.labelControl3.Size = new System.Drawing.Size(73, 13);
      this.labelControl3.TabIndex = 3;
      this.labelControl3.Text = "Reference List:";
      // 
      // comboSource
      // 
      this.comboSource.Location = new System.Drawing.Point(123, 33);
      this.comboSource.Name = "comboSource";
      this.comboSource.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.comboSource.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
      this.comboSource.Size = new System.Drawing.Size(178, 20);
      this.comboSource.TabIndex = 4;
      this.comboSource.EditValueChanged += new System.EventHandler(this.comboSource_EditValueChanged);
      // 
      // comboTarget
      // 
      this.comboTarget.Location = new System.Drawing.Point(123, 7);
      this.comboTarget.Name = "comboTarget";
      this.comboTarget.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.comboTarget.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
      this.comboTarget.Size = new System.Drawing.Size(178, 20);
      this.comboTarget.TabIndex = 1;
      this.comboTarget.EditValueChanged += new System.EventHandler(this.comboTarget_EditValueChanged);
      // 
      // labelControl4
      // 
      this.labelControl4.Location = new System.Drawing.Point(5, 10);
      this.labelControl4.Name = "labelControl4";
      this.labelControl4.Size = new System.Drawing.Size(55, 13);
      this.labelControl4.TabIndex = 0;
      this.labelControl4.Text = "Target List:";
      // 
      // cbTv
      // 
      this.cbTv.Location = new System.Drawing.Point(123, 84);
      this.cbTv.Name = "cbTv";
      this.cbTv.Properties.AutoWidth = true;
      this.cbTv.Properties.Caption = "TV";
      this.cbTv.Size = new System.Drawing.Size(34, 19);
      this.cbTv.TabIndex = 10;
      this.cbTv.TabStop = false;
      // 
      // cbRadio
      // 
      this.cbRadio.Location = new System.Drawing.Point(204, 84);
      this.cbRadio.Name = "cbRadio";
      this.cbRadio.Properties.AutoWidth = true;
      this.cbRadio.Properties.Caption = "Radio";
      this.cbRadio.Size = new System.Drawing.Size(49, 19);
      this.cbRadio.TabIndex = 11;
      this.cbRadio.TabStop = false;
      // 
      // labelControl5
      // 
      this.labelControl5.Location = new System.Drawing.Point(5, 112);
      this.labelControl5.Name = "labelControl5";
      this.labelControl5.Size = new System.Drawing.Size(62, 13);
      this.labelControl5.TabIndex = 12;
      this.labelControl5.Text = "Start at Pr#:";
      // 
      // labelControl6
      // 
      this.labelControl6.Location = new System.Drawing.Point(204, 112);
      this.labelControl6.Name = "labelControl6";
      this.labelControl6.Size = new System.Drawing.Size(177, 13);
      this.labelControl6.TabIndex = 14;
      this.labelControl6.Text = "(i.e. let radio channels start at 5000)";
      // 
      // comboPrNr
      // 
      this.comboPrNr.EditValue = "1";
      this.comboPrNr.Location = new System.Drawing.Point(123, 109);
      this.comboPrNr.Name = "comboPrNr";
      this.comboPrNr.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.comboPrNr.Properties.EditFormat.FormatString = "d";
      this.comboPrNr.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.comboPrNr.Properties.Items.AddRange(new object[] {
            "1",
            "100",
            "500",
            "1000",
            "5000",
            "9000"});
      this.comboPrNr.Size = new System.Drawing.Size(75, 20);
      this.comboPrNr.TabIndex = 13;
      // 
      // grpManual
      // 
      this.grpManual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpManual.Controls.Add(this.labelControl9);
      this.grpManual.Controls.Add(this.cbAnalog);
      this.grpManual.Controls.Add(this.cbDigital);
      this.grpManual.Controls.Add(this.lblTargetInfo);
      this.grpManual.Controls.Add(this.lblSourceInfo);
      this.grpManual.Controls.Add(this.labelControl7);
      this.grpManual.Controls.Add(this.btnApply);
      this.grpManual.Controls.Add(this.comboSource);
      this.grpManual.Controls.Add(this.comboPrNr);
      this.grpManual.Controls.Add(this.labelControl3);
      this.grpManual.Controls.Add(this.comboTarget);
      this.grpManual.Controls.Add(this.labelControl4);
      this.grpManual.Controls.Add(this.labelControl6);
      this.grpManual.Controls.Add(this.labelControl5);
      this.grpManual.Controls.Add(this.cbTv);
      this.grpManual.Controls.Add(this.cbRadio);
      this.grpManual.Enabled = false;
      this.grpManual.Location = new System.Drawing.Point(71, 129);
      this.grpManual.Name = "grpManual";
      this.grpManual.ShowCaption = false;
      this.grpManual.Size = new System.Drawing.Size(609, 177);
      this.grpManual.TabIndex = 5;
      this.grpManual.Text = "grpManual";
      // 
      // labelControl9
      // 
      this.labelControl9.Location = new System.Drawing.Point(5, 62);
      this.labelControl9.Name = "labelControl9";
      this.labelControl9.Size = new System.Drawing.Size(59, 13);
      this.labelControl9.TabIndex = 6;
      this.labelControl9.Text = "Signal Type:";
      // 
      // cbAnalog
      // 
      this.cbAnalog.Location = new System.Drawing.Point(123, 59);
      this.cbAnalog.Name = "cbAnalog";
      this.cbAnalog.Properties.AutoWidth = true;
      this.cbAnalog.Properties.Caption = "Analog";
      this.cbAnalog.Size = new System.Drawing.Size(55, 19);
      this.cbAnalog.TabIndex = 7;
      this.cbAnalog.TabStop = false;
      // 
      // cbDigital
      // 
      this.cbDigital.Location = new System.Drawing.Point(204, 59);
      this.cbDigital.Name = "cbDigital";
      this.cbDigital.Properties.AutoWidth = true;
      this.cbDigital.Properties.Caption = "Digital";
      this.cbDigital.Size = new System.Drawing.Size(51, 19);
      this.cbDigital.TabIndex = 8;
      this.cbDigital.TabStop = false;
      // 
      // lblTargetInfo
      // 
      this.lblTargetInfo.Location = new System.Drawing.Point(308, 10);
      this.lblTargetInfo.Name = "lblTargetInfo";
      this.lblTargetInfo.Size = new System.Drawing.Size(3, 13);
      this.lblTargetInfo.TabIndex = 2;
      this.lblTargetInfo.Text = " ";
      // 
      // lblSourceInfo
      // 
      this.lblSourceInfo.Location = new System.Drawing.Point(308, 36);
      this.lblSourceInfo.Name = "lblSourceInfo";
      this.lblSourceInfo.Size = new System.Drawing.Size(3, 13);
      this.lblSourceInfo.TabIndex = 5;
      this.lblSourceInfo.Text = " ";
      // 
      // labelControl7
      // 
      this.labelControl7.Location = new System.Drawing.Point(5, 87);
      this.labelControl7.Name = "labelControl7";
      this.labelControl7.Size = new System.Drawing.Size(70, 13);
      this.labelControl7.TabIndex = 9;
      this.labelControl7.Text = "Channel Type:";
      // 
      // btnApply
      // 
      this.btnApply.Enabled = false;
      this.btnApply.Location = new System.Drawing.Point(123, 144);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(103, 23);
      this.btnApply.TabIndex = 15;
      this.btnApply.Text = "Apply";
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(497, 321);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 23);
      this.btnOk.TabIndex = 6;
      this.btnOk.Text = "Ok";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Location = new System.Drawing.Point(592, 321);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(88, 23);
      this.btnClose.TabIndex = 7;
      this.btnClose.Text = "Close/Cancel";
      // 
      // ReferenceListForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(692, 356);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.grpManual);
      this.Controls.Add(this.rbManual);
      this.Controls.Add(this.rbAuto);
      this.Controls.Add(this.labelControl2);
      this.Controls.Add(this.edFile);
      this.Controls.Add(this.labelControl1);
      this.Name = "ReferenceListForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Apply Reference List";
      ((System.ComponentModel.ISupportInitialize)(this.edFile.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbAuto.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.rbManual.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comboSource.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comboTarget.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbTv.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbRadio.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comboPrNr.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.grpManual)).EndInit();
      this.grpManual.ResumeLayout(false);
      this.grpManual.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cbAnalog.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.cbDigital.Properties)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraEditors.LabelControl labelControl1;
    private DevExpress.XtraEditors.ButtonEdit edFile;
    private DevExpress.XtraEditors.LabelControl labelControl2;
    private DevExpress.XtraEditors.CheckEdit rbAuto;
    private DevExpress.XtraEditors.CheckEdit rbManual;
    private DevExpress.XtraEditors.LabelControl labelControl3;
    private DevExpress.XtraEditors.ComboBoxEdit comboSource;
    private DevExpress.XtraEditors.ComboBoxEdit comboTarget;
    private DevExpress.XtraEditors.LabelControl labelControl4;
    private DevExpress.XtraEditors.CheckEdit cbTv;
    private DevExpress.XtraEditors.CheckEdit cbRadio;
    private DevExpress.XtraEditors.LabelControl labelControl5;
    private DevExpress.XtraEditors.LabelControl labelControl6;
    private DevExpress.XtraEditors.ComboBoxEdit comboPrNr;
    private DevExpress.XtraEditors.GroupControl grpManual;
    private DevExpress.XtraEditors.SimpleButton btnApply;
    private DevExpress.XtraEditors.SimpleButton btnOk;
    private DevExpress.XtraEditors.SimpleButton btnClose;
    private DevExpress.XtraEditors.LabelControl labelControl7;
    private DevExpress.XtraEditors.LabelControl lblTargetInfo;
    private DevExpress.XtraEditors.LabelControl lblSourceInfo;
    private DevExpress.XtraEditors.LabelControl labelControl9;
    private DevExpress.XtraEditors.CheckEdit cbAnalog;
    private DevExpress.XtraEditors.CheckEdit cbDigital;
  }
}