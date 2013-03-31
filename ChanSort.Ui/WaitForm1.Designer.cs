namespace ChanSort.Ui
{
  partial class WaitForm1
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitForm1));
      this.progressPanel1 = new DevExpress.XtraWaitForm.ProgressPanel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // progressPanel1
      // 
      resources.ApplyResources(this.progressPanel1, "progressPanel1");
      this.progressPanel1.Appearance.BackColor = ((System.Drawing.Color)(resources.GetObject("progressPanel1.Appearance.BackColor")));
      this.progressPanel1.Appearance.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("progressPanel1.Appearance.GradientMode")));
      this.progressPanel1.Appearance.Image = ((System.Drawing.Image)(resources.GetObject("progressPanel1.Appearance.Image")));
      this.progressPanel1.Appearance.Options.UseBackColor = true;
      this.progressPanel1.AppearanceCaption.Font = ((System.Drawing.Font)(resources.GetObject("resource.Font")));
      this.progressPanel1.AppearanceCaption.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("resource.GradientMode")));
      this.progressPanel1.AppearanceCaption.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image")));
      this.progressPanel1.AppearanceCaption.Options.UseFont = true;
      this.progressPanel1.AppearanceDescription.Font = ((System.Drawing.Font)(resources.GetObject("resource.Font1")));
      this.progressPanel1.AppearanceDescription.GradientMode = ((System.Drawing.Drawing2D.LinearGradientMode)(resources.GetObject("resource.GradientMode1")));
      this.progressPanel1.AppearanceDescription.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image1")));
      this.progressPanel1.AppearanceDescription.Options.UseFont = true;
      this.progressPanel1.ImageHorzOffset = 20;
      this.progressPanel1.Name = "progressPanel1";
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
      this.tableLayoutPanel1.Controls.Add(this.progressPanel1, 0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // WaitForm1
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.DoubleBuffered = true;
      this.Name = "WaitForm1";
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraWaitForm.ProgressPanel progressPanel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
