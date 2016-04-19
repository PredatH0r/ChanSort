using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ChanSort.Api;
using ChanSort.Ui.Properties;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  public partial class ReferenceListForm : DevExpress.XtraEditors.XtraForm
  {
    private readonly MainForm main;
    private SerializerBase ser;

    public ReferenceListForm(MainForm main)
    {
      this.main = main;
      InitializeComponent();
      this.UpdateButtons();
    }

    private void UpdateButtons()
    {
      this.btnOk.Visible = this.rbAuto.Checked;
      this.btnClose.Text = this.rbAuto.Checked ? "Cancel" : "Close";
    }

    #region ShowOpenFileDialog()
    private SerializerBase ShowOpenFileDialog()
    {
      try
      {
        string supportedExtensions;
        int numberOfFilters;
        var filter = main.GetTvDataFileFilter(out supportedExtensions, out numberOfFilters);

        using (OpenFileDialog dlg = new OpenFileDialog())
        {
          dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
          dlg.AddExtension = true;
          dlg.Filter = filter + string.Format(Resources.MainForm_FileDialog_OpenFileFilter, supportedExtensions);
          dlg.FilterIndex = numberOfFilters + 1;
          dlg.CheckFileExists = true;
          dlg.RestoreDirectory = true;
          if (dlg.ShowDialog() != DialogResult.OK)
            return null;

          if (main.DetectCommonFileCorruptions(dlg.FileName))
            return null;

          var plugin = dlg.FilterIndex <= main.Plugins.Count ? main.Plugins[dlg.FilterIndex - 1] : main.GetPluginForFile(dlg.FileName);
          var ser = plugin.CreateSerializer(dlg.FileName);
          ser.Load();
          return ser;
        }
      }
      catch
      {
        return null;
      }
    }
    #endregion

    #region UpdateInfoTextAndOptions()
    private void UpdateInfoTextAndOptions()
    {
      foreach (var ctl in this.grpManual.Controls)
      {
        var checkEdit = ctl as CheckEdit;
        if (checkEdit != null)
          checkEdit.Checked = checkEdit.Enabled = true;
      }

      var list = (ChannelList)this.comboSource.EditValue;
      this.lblSourceInfo.Text = GetInfoText(list);
      list = (ChannelList)this.comboTarget.EditValue;
      this.lblTargetInfo.Text = GetInfoText(list);

      bool canApply =
        (cbAntenna.Checked || cbCable.Checked || cbSatellite.Checked)
        && (cbAnalog.Checked || cbDigital.Checked)
        && (cbTv.Checked || cbRadio.Checked);
      this.btnApply.Enabled = canApply;
    }
    #endregion

    #region GetInfoText()
    private string GetInfoText(ChannelList list)
    {
      var src = list?.SignalSource ?? 0;
      var sb = new StringBuilder();

      if ((src & SignalSource.Antenna) != 0)
        sb.Append(", Antenna");
      else
        this.cbAntenna.Enabled = this.cbAntenna.Checked = false;

      if ((src & SignalSource.Cable) != 0)
        sb.Append(", Cable");
      else
        this.cbCable.Enabled = this.cbCable.Checked = false;

      if ((src & SignalSource.Sat) != 0)
        sb.Append(", Satellite");
      else
        this.cbSatellite.Enabled = this.cbSatellite.Checked = false;

      if ((src & SignalSource.IP) != 0)
        sb.Append(", IP");
      else
        this.cbIP.Enabled = this.cbIP.Checked = false;

      if ((src & SignalSource.Analog) != 0)
        sb.Append(", Analog");
      else
        this.cbAnalog.Enabled = this.cbAnalog.Checked = false;

      if ((src & SignalSource.Digital) != 0)
        sb.Append(", Digital");
      else
        this.cbDigital.Enabled = this.cbDigital.Checked = false;

      if ((src & SignalSource.Tv) != 0)
        sb.Append(", TV");
      else
        this.cbTv.Enabled = this.cbTv.Checked = false;

      if ((src & SignalSource.Radio) != 0)
        sb.Append(", Radio");
      else
        this.cbRadio.Enabled = this.cbRadio.Checked = false;

      if (sb.Length >= 2)
        sb.Remove(0, 2);
      return sb.ToString();
    }
    #endregion

    #region edFile_ButtonClick
    private void edFile_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
    {
      ser = ShowOpenFileDialog();
      if (ser == null)
        return;

      this.edFile.Text = ser.FileName;
      this.rbAuto.Enabled = this.rbManual.Enabled = true;

      this.comboSource.Properties.Items.Clear();
      foreach (var list in ser.DataRoot.ChannelLists)
      {
        if (!list.IsMixedSouceFavoritesList && list.Channels.Count > 0)
          this.comboSource.Properties.Items.Add(list);
      }

      this.comboTarget.Properties.Items.Clear();
      foreach (var list in main.DataRoot.ChannelLists)
      {
        if (!list.IsMixedSouceFavoritesList && list.Channels.Count > 0)
          this.comboTarget.Properties.Items.Add(list);
      }

      if (this.comboSource.Properties.Items.Count > 0)
        this.comboSource.SelectedIndex = 0;

      this.rbAuto.Enabled =
        ser.DataRoot.MixedSourceFavorites == main.DataRoot.MixedSourceFavorites &&
        ser.DataRoot.SortedFavorites == main.DataRoot.SortedFavorites;
      if (!this.rbAuto.Enabled)
        this.rbManual.Checked = true;
    }
    #endregion

    #region rbAuto_CheckedChanged
    private void rbAuto_CheckedChanged(object sender, EventArgs e)
    {
      var ed = (CheckEdit) sender;
      if (!ed.Checked) return;
      UpdateButtons();
      this.grpManual.Enabled = this.rbManual.Checked && this.rbManual.Enabled;
    }
    #endregion

    #region comboSource_EditValueChanged
    private void comboSource_EditValueChanged(object sender, EventArgs e)
    {
      UpdateInfoTextAndOptions();
      var list = (ChannelList) this.comboSource.EditValue;
      this.comboPrNr.Text = list == null || list.Count == 0 ? "1" : list.Channels.Min(ch => Math.Max(ch.OldProgramNr, 1)).ToString();

      // auto-select a compatible target list
      if (list != null)
      {
        this.comboTarget.SelectedIndex = -1;
        var src = list.SignalSource;
        foreach (ChannelList targetList in this.comboTarget.Properties.Items)
        {
          if ((targetList.SignalSource & src) == src)
          {
            this.comboTarget.SelectedItem = targetList;
            break;
          }
        }
      }
    }

    private void comboTarget_EditValueChanged(object sender, EventArgs e)
    {
      UpdateInfoTextAndOptions();
    }
    #endregion

    #region btnApply_Click
    private void btnApply_Click(object sender, EventArgs e)
    {

    }
    #endregion

    #region btnOk_Click
    private void btnOk_Click(object sender, EventArgs e)
    {
      main.Editor.ApplyReferenceList(ser.DataRoot);
    }
    #endregion

  }
}