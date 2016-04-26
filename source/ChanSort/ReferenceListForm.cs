using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;
using ChanSort.Ui.Properties;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace ChanSort.Ui
{
  public partial class ReferenceListForm : XtraForm
  {
    private readonly MainForm main;
    private SerializerBase serializer;

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

        using (var dlg = new OpenFileDialog())
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

      var list = (ChannelList) this.comboSource.EditValue;
      this.lblSourceInfo.Text = GetInfoText(list);
      list = (ChannelList) this.comboTarget.EditValue;
      this.lblTargetInfo.Text = GetInfoText(list);

      var canApply = (cbAnalog.Checked || cbDigital.Checked) && (cbTv.Checked || cbRadio.Checked);
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
      if ((src & SignalSource.Cable) != 0)
        sb.Append(", Cable");
      if ((src & SignalSource.Sat) != 0)
        sb.Append(", Satellite");
      if ((src & SignalSource.IP) != 0)
        sb.Append(", IP");

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

    #region FilterChannel()
    private bool FilterChannel(ChannelInfo ch)
    {
      var ss = ch.SignalSource;
      if (!(this.cbAnalog.Checked && (ss & SignalSource.Analog) != 0 || this.cbDigital.Checked && (ss & SignalSource.Digital) != 0))
        return false;
      if (!(this.cbTv.Checked && (ss & SignalSource.Tv) != 0 || this.cbRadio.Checked && (ss & SignalSource.Radio) != 0))
        return false;
      return true;
    }
    #endregion

    #region edFile_ButtonClick

    private void edFile_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      serializer = ShowOpenFileDialog();
      if (serializer == null)
        return;

      this.edFile.Text = serializer.FileName;
      this.rbAuto.Enabled = this.rbManual.Enabled = true;

      this.comboSource.EditValue = null;
      this.comboSource.Properties.Items.Clear();
      foreach (var list in serializer.DataRoot.ChannelLists)
      {
        if (!list.IsMixedSouceFavoritesList && list.Channels.Count > 0)
          this.comboSource.Properties.Items.Add(list);
      }

      this.comboTarget.EditValue = null;
      this.comboTarget.Properties.Items.Clear();
      foreach (var list in main.DataRoot.ChannelLists)
      {
        if (!list.IsMixedSouceFavoritesList && list.Channels.Count > 0)
        {
          this.comboTarget.Properties.Items.Add(list);
          if (main.CurrentChannelList == list)
            this.comboTarget.SelectedItem = list;
        }
      }

      if (this.comboTarget.SelectedIndex < 0 && this.comboTarget.Properties.Items.Count > 0)
        this.comboTarget.SelectedIndex = 0;

      this.rbAuto.Enabled = true;
      foreach (var list in main.DataRoot.ChannelLists)
        this.rbAuto.Enabled &= (serializer.DataRoot.GetChannelList(list.SignalSource)?.SignalSource ?? 0) == list.SignalSource;
        //serializer.DataRoot.MixedSourceFavorites == main.DataRoot.MixedSourceFavorites &&
        //serializer.DataRoot.SortedFavorites == main.DataRoot.SortedFavorites;
      if (this.rbAuto.Enabled)
        this.rbAuto.Checked = true;
      else
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

    #region btnApply_Click

    private void btnApply_Click(object sender, EventArgs e)
    {
      var src = (ChannelList) this.comboSource.EditValue;
      var target = (ChannelList) this.comboTarget.EditValue;
      int offset;
      if (int.TryParse(this.comboPrNr.Text, out offset))
        offset -= src.Channels.Min(ch => Math.Max(ch.OldProgramNr, 1));
      main.Editor.ApplyReferenceList(this.serializer.DataRoot, src, target, false, offset, FilterChannel);
      main.RefreshGrids();
    }

    #endregion

    #region btnOk_Click

    private void btnOk_Click(object sender, EventArgs e)
    {
      main.Editor.ApplyReferenceList(serializer.DataRoot);
      main.RefreshGrids();
    }

    #endregion

    private void comboTarget_EditValueChanged(object sender, EventArgs e)
    {
      UpdateInfoTextAndOptions();

      // auto-select a compatible source list
      var list = (ChannelList)this.comboTarget.EditValue;
      if (list != null)
      {
        this.comboSource.SelectedIndex = -1;
        var src = list.SignalSource;
        foreach (ChannelList sourceList in this.comboSource.Properties.Items)
        {
          if ((sourceList.SignalSource & src) == src)
          {
            this.comboSource.SelectedItem = sourceList;
            break;
          }
        }
      }
    }

    #region comboSource_EditValueChanged

    private void comboSource_EditValueChanged(object sender, EventArgs e)
    {
      UpdateInfoTextAndOptions();
      var list = (ChannelList)this.comboSource.EditValue;
      this.comboPrNr.Text = list == null || list.Count == 0 ? "1" : list.Channels.Min(ch => Math.Max(ch.OldProgramNr, 1)).ToString();
    }

    #endregion
  }
}