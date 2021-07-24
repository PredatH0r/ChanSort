using System;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace ChanSort.Loader.LG.Binary
{
  public partial class TvSettingsForm : XtraForm
  {
    private readonly ITllFileSerializer tvSerializer;

    public TvSettingsForm(ITllFileSerializer tvSerializer)
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
        this.grpInformation.Visible = false;
        this.Height -= this.grpInformation.Height;
      }

      if (mapping == null || !mapping.SupportsAutoChannelUpdate)
      {
        this.grpSetup.Visible = false;
        this.Height -= this.grpSetup.Height;
      }
      else
      {
        this.cbAutoChannelUpdate.Checked = mapping.SettingsAutomaticChannelUpdate;        
      }

      if (mapping == null || !mapping.SupportsHbbTv)
        this.cbHbbTv.Enabled = false;
      else
        this.cbHbbTv.Checked = mapping.HbbTvEnabled;

      if (mapping == null || !mapping.SupportsHotelMenu)
      {
        this.grpHotelMode.Visible = false;
        this.Height -= this.grpHotelMode.Height;
      }
      else
      {
        this.cbHotelMode.Checked = mapping.HotelModeEnabled;
        this.cbDtvUpdate.Checked = mapping.HotelModeDtvUpdate;        
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

    private void lblHotelMenuAutoDetect_Click(object sender, EventArgs e)
    {
      this.lblHotelMenuAutoDetect.Text = this.tvSerializer.GetHotelMenuOffset().ToString();
    }
  }
}
