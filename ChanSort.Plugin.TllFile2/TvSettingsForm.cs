using System;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace ChanSort.Plugin.TllFile
{
  public partial class TvSettingsForm : XtraForm
  {
    private readonly TllFileSerializer tvSerializer;

    public TvSettingsForm(TllFileSerializer tvSerializer)
    {
      this.tvSerializer = tvSerializer;
      InitializeComponent();
    }

    private void TvSettingsForm_Load(object sender, EventArgs e)
    {
      var items = tvSerializer.SupportedTvCountryCodes;
      foreach(var item in items)
        this.comboBoxEdit1.Properties.Items.Add(item);
      this.comboBoxEdit1.Text = this.tvSerializer.TvCountryCode;

      var mapping = this.tvSerializer.GetFirmwareMapping();
      if (mapping != null)
      {
        this.cbAutoChannelUpdate.Checked = mapping.SettingsAutomaticChannelUpdate;
        this.cbHbbTv.Checked = mapping.HbbTvEnabled;
        this.cbHotelMode.Checked = mapping.HotelModeEnabled;
        this.cbDtvUpdate.Checked = mapping.HotelModeDtvUpdate;

        this.grpFirmwareNote.Visible = false;
        this.Height -= this.grpFirmwareNote.Height;
      }
      else
      {
        this.cbAutoChannelUpdate.Enabled = false;
        this.cbHbbTv.Enabled = false;
        this.cbHotelMode.Enabled = false;
        this.cbDtvUpdate.Enabled = false;
      }
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      this.tvSerializer.TvCountryCode = this.comboBoxEdit1.Text;

      var mapping = this.tvSerializer.GetFirmwareMapping();
      if (mapping != null)
      {
        mapping.SettingsAutomaticChannelUpdate = this.cbAutoChannelUpdate.Checked;
        mapping.HbbTvEnabled = this.cbHbbTv.Checked;
        mapping.HotelModeEnabled = this.cbHotelMode.Checked;
        mapping.HotelModeDtvUpdate = this.cbDtvUpdate.Checked;
      }
    }

    private void cbCustomCountry_CheckedChanged(object sender, EventArgs e)
    {
      this.comboBoxEdit1.Properties.TextEditStyle = this.cbCustomCountry.Checked
                                                      ? TextEditStyles.Standard
                                                      : TextEditStyles.DisableTextEditor;
    }
  }
}
