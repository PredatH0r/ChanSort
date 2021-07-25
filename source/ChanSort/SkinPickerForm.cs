using System;
using ChanSort.Ui.Properties;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  public partial class SkinPickerForm : XtraForm
  {
    string initialSkinName;

    public SkinPickerForm()
    {
      InitializeComponent();
    }

    private void SkinPicker_Load(object sender, EventArgs e)
    {
      this.initialSkinName = UserLookAndFeel.Default.SkinName;

      DevExpress.XtraBars.Helpers.SkinHelper.InitSkinGallery(gallery);
      gallery.Gallery.ColumnCount = 8;
      gallery.Gallery.AllowFilter = false;
      gallery.Gallery.FixedImageSize = true;
      gallery.Gallery.ImageSize = new System.Drawing.Size(48, 48);
      gallery.Gallery.FixedHoverImageSize = true;
      gallery.Gallery.HoverImageSize = new System.Drawing.Size(48, 48);
      gallery.Gallery.ShowItemText = true;
      gallery.Gallery.ShowScrollBar = DevExpress.XtraBars.Ribbon.Gallery.ShowScrollBar.Auto;
      foreach (var galItem in gallery.Gallery.GetAllItems())
      {
        galItem.Caption = galItem.Caption.Replace("DevExpress", "DX");
        galItem.Image = galItem.HoverImage;
      }
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      foreach (var galItem in gallery.Gallery.GetAllItems())
      {
        if (galItem.Caption == UserLookAndFeel.Default.SkinName)
        {
          galItem.Checked = true;
          break;
        }
      }
      UserLookAndFeel.Default.SkinName = "The Bezier";
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      Config.Default.SkinName = UserLookAndFeel.Default.SkinName;
      Config.Default.Save();
      this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      UserLookAndFeel.Default.SkinName = this.initialSkinName;
      this.Close();
    }
  }
}
