using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ChanSort.Api;
using ChanSort.Ui.Properties;
using DevExpress.Utils.Extensions;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace ChanSort.Ui
{
  public partial class ReferenceListForm : XtraForm
  {
    private readonly MainForm main;
    private SerializerBase serializer;
    private readonly string[] closeButtonText;

    class ListOption
    {
      public ChannelList ChannelList { get; }
      public int PosIndex { get; }

      public string Caption { get; }
      public ListOption(ChannelList list, int posIndex, string caption)
      {
        ChannelList = list;
        PosIndex = posIndex;
        Caption = caption;
      }

      public override string ToString() => Caption;
    }

    public ReferenceListForm(MainForm main)
    {
      this.main = main;
      InitializeComponent();

      this.closeButtonText = this.btnClose.Text.Split('/');
      this.UpdateButtons();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.components?.Dispose();
        this.serializer?.Dispose();
      }
      base.Dispose(disposing);
    }

    #region UpdateButtons()
    private void UpdateButtons()
    {
      this.btnOk.Visible = this.rbAuto.Checked;
      this.btnClose.Text = this.rbAuto.Checked ? closeButtonText[1] : closeButtonText[0];
    }
    #endregion

    #region OnLoad()
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      this.CreateHandle();
      this.Update();
      BeginInvoke((Action) (() =>
      {
        var ser = ShowOpenFileDialog(main);
        this.SetInput(ser);
      }));
    }
    #endregion

    #region ShowOpenFileDialog()

    private static SerializerBase ShowOpenFileDialog(MainForm main)
    {
      try
      {
        var filter = main.GetTvDataFileFilter(out var supportedExtensions, out var numberOfFilters);

        using var dlg = new OpenFileDialog();
        dlg.InitialDirectory = Config.Default.ReferenceListFolder ?? Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? ".", "ReferenceLists");
        dlg.AddExtension = true;
        dlg.Filter = filter + string.Format(Resources.MainForm_FileDialog_OpenFileFilter, supportedExtensions);
        dlg.FilterIndex = numberOfFilters + 1;
        dlg.CheckFileExists = true;
        dlg.RestoreDirectory = true;
        dlg.DereferenceLinks = true;
        dlg.Title = Resources.ReferenceListForm_ShowOpenFileDialog_Title;
        if (dlg.ShowDialog(main) != DialogResult.OK)
          return null;

        Config.Default.ReferenceListFolder = Path.GetDirectoryName(dlg.FileName);
        if (main.DetectCommonFileCorruptions(dlg.FileName))
          return null;

        ISerializerPlugin hint = dlg.FilterIndex <= main.Plugins.Count ? main.Plugins[dlg.FilterIndex - 1] : null;
        return main.GetSerializerForFile(dlg.FileName, ref hint);
      }
      catch
      {
        return null;
      }
    }

    #endregion

    #region SetInput()
    private void SetInput(SerializerBase ser)
    {
      this.serializer?.Dispose();

      this.tabPage2.PageEnabled = ser != null;
      if (ser == null)
        return;

      this.serializer = ser;
      this.edFile.Text = serializer.FileName;
      this.rbAuto.Enabled = this.rbManual.Enabled = true;
      ser.DataRoot.ApplyCurrentProgramNumbers();
      ser.DataRoot.ValidateAfterLoad();

      // fill source first, so that when a target is selected later, the event handler can pick the best source
      this.comboSource.EditValue = null;
      this.comboSource.Properties.Items.Clear();
      foreach (var list in serializer.DataRoot.ChannelLists)
      {
        if (list.Channels.Count == 0)
          continue;
        if (!list.IsMixedSourceFavoritesList)
          this.comboSource.Properties.Items.Add(new ListOption(list, 0, list.ShortCaption));

        if (!serializer.DataRoot.MixedSourceFavorites || list.IsMixedSourceFavoritesList)
        {
          for (int i = 1; i <= serializer.DataRoot.FavListCount; i++)
            this.comboSource.Properties.Items.Add(new ListOption(list, i, list.ShortCaption + " - " + list.GetFavListCaption(i - 1, true)));
        }
      }

      // fill target
      this.comboTarget.EditValue = null;
      this.comboTarget.Properties.Items.Clear();
      foreach (var list in main.DataRoot.ChannelLists)
      {
        if (list.Channels.Count == 0)
          continue;
        if (!list.IsMixedSourceFavoritesList)
          this.comboTarget.Properties.Items.Add(new ListOption(list, 0, list.ShortCaption));

        if (!main.DataRoot.MixedSourceFavorites || list.IsMixedSourceFavoritesList)
        {
          for (int i = 1; i <= main.DataRoot.FavListCount; i++)
            this.comboTarget.Properties.Items.Add(new ListOption(list, i, list.ShortCaption + (i == 0 ? "" : " - " + list.GetFavListCaption(i - 1, true))));
        }
      }

      // set current list/sublist from the main form as the target
      foreach (ListOption option in this.comboTarget.Properties.Items)
      {
        if (option.ChannelList == main.CurrentChannelList && option.PosIndex == main.SubListIndex)
        {
          this.comboTarget.SelectedItem = option;
          break;
        }
      }
      if (this.comboTarget.SelectedIndex < 0 && this.comboTarget.Properties.Items.Count > 0)
        this.comboTarget.SelectedIndex = 0;


      // detect whether auto-sorting is possible
      this.rbAuto.Enabled = true;
      foreach (var list in main.DataRoot.ChannelLists)
      {
        if (list.Channels.Count == 0 || list.IsMixedSourceFavoritesList)
          continue;
        this.rbAuto.Enabled &= (serializer.DataRoot.GetChannelList(list.SignalSource)?.SignalSource ?? 0) == list.SignalSource;
      }
      if (this.rbAuto.Enabled)
        this.rbAuto.Checked = true;
      else
        this.rbManual.Checked = true;

      this.tabControl.SelectedTabPage = tabPage2;
    }

    #endregion

    #region UpdateInfoTextAndOptions()

    private void UpdateInfoTextAndOptions()
    {
      foreach (var ctl in this.grpManual.Controls)
      {
        if (ctl == this.cbConsecutive)
          continue;
        var checkEdit = ctl as CheckEdit;
        if (checkEdit != null)
          checkEdit.Checked = checkEdit.Enabled = true;
      }

      var list = (ListOption) this.comboSource.EditValue;
      this.lblSourceInfo.Text = GetInfoText(list, true);
      list = (ListOption) this.comboTarget.EditValue;
      this.lblTargetInfo.Text = GetInfoText(list, false);

      var canApply = (cbAnalog.Checked || cbDigital.Checked) && (cbTv.Checked || cbRadio.Checked || cbData.Checked);
      this.btnApply.Enabled = canApply;
    }

    #endregion

    #region GetInfoText()

    private string GetInfoText(ListOption option, bool source)
    {
      var list = option?.ChannelList;
      var src = list?.SignalSource ?? 0;
      var sb = new StringBuilder();

      var sigSource = new[] {SignalSource.Antenna, SignalSource.Cable, SignalSource.Sat, SignalSource.Ip, SignalSource.Analog, SignalSource.Dvb, SignalSource.Tv, SignalSource.Radio, SignalSource.Data};
      var infoText = Resources.ReferenceListForm_AntennaCableSatIPAnalogDigitalTVRadio.Split(',').Select(txt => txt.Trim()).ToArray();
      var controls = new[] {cbAntenna, cbCable, cbSat, cbIp, cbAnalog, cbDigital, cbTv, cbRadio, cbData };

      for (int i = 0, c = sigSource.Length; i < c; i++)
      {
        if ((src & sigSource[i]) != 0)
          sb.Append(", ").Append(infoText[i].TrimEnd());
        else
        {
          controls[i].Checked = false;
          if (source || i >= 4)
            controls[i].Enabled = false;
        }      
      }

      if (sb.Length >= 2)
        sb.Remove(0, 2);
      return sb.ToString();
    }

    #endregion

    #region FilterChannel()
    private bool FilterChannel(ChannelInfo ch, bool source)
    {
      var ss = ch.SignalSource;
      if (source)
      {
        if ((ss & SignalSource.MaskBcastMedium) != 0 && 
            !(this.cbAntenna.Checked && (ss & SignalSource.Antenna) != 0 || this.cbCable.Checked && (ss & SignalSource.Cable) != 0 || this.cbSat.Checked && (ss & SignalSource.Sat) != 0))
          return false;
      }

      if ((ss & SignalSource.MaskBcastSystem) != 0 &&
        !(this.cbAnalog.Checked && (ss & SignalSource.Analog) != 0 || this.cbDigital.Checked && (ss & SignalSource.Dvb) != 0) || this.cbIp.Checked && (ss & SignalSource.Ip) != 0)
        return false;

      if ((ss & SignalSource.MaskTvRadioData) != 0 &&
        !(this.cbTv.Checked && (ss & SignalSource.Tv) != 0 || this.cbRadio.Checked && (ss & SignalSource.Radio) != 0 || this.cbData.Checked && (ss & SignalSource.Data) != 0))
        return false;
      return true;
    }
    #endregion


    #region edFile_ButtonClick
    private void edFile_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      var ser = ShowOpenFileDialog(this.main);
      SetInput(ser);
    }
    #endregion

    #region linkWiki_HyperlinkClick
    private void linkWiki_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
    {
      Process.Start("https://github.com/PredatH0r/ChanSort/wiki/Reference-Lists");
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

    #region comboTarget_EditValueChanged
    private void comboTarget_EditValueChanged(object sender, EventArgs e)
    {
      UpdateInfoTextAndOptions();

      // auto-select a compatible source list
      var list = ((ListOption)this.comboTarget.EditValue)?.ChannelList;
      if (list != null)
      {
        this.comboSource.SelectedIndex = -1;
        var src = list.SignalSource;
        foreach (ListOption sourceList in this.comboSource.Properties.Items)
        {
          if ((sourceList.ChannelList.SignalSource & src) == src)
          {
            this.comboSource.SelectedItem = sourceList;
            break;
          }
        }
      }
    }
    #endregion

    #region comboSource_EditValueChanged

    private void comboSource_EditValueChanged(object sender, EventArgs e)
    {
      UpdateInfoTextAndOptions();
      var list = ((ListOption)this.comboSource.EditValue)?.ChannelList;
      this.comboPrNr.Text = list == null || list.Count == 0 ? "1" : list.Channels.Min(ch => Math.Max(ch.OldProgramNr, 1)).ToString();
    }

    #endregion

    #region btnApply_Click

    private void btnApply_Click(object sender, EventArgs e)
    {
      var src = (ListOption) this.comboSource.EditValue;
      var target = (ListOption) this.comboTarget.EditValue;
      int offset;
      if (int.TryParse(this.comboPrNr.Text, out offset))
        offset -= src.ChannelList.Channels.Min(ch => Math.Max(ch.OldProgramNr, 1));

      bool overwrite = true;
      if (target.ChannelList.GetChannelsByNewOrder().Any(ch => ch.GetPosition(target.PosIndex) != -1))
      {
        using var dlg = new ActionBoxDialog(Resources.ReferenceListForm_btnApply_ConflictHandling);
        dlg.AddAction(Resources.ReferenceListForm_btnApply_Click_Clear, DialogResult.OK, dlg.EmptyList);
        dlg.AddAction(Resources.ReferenceListForm_btnApply_Click_Overwrite, DialogResult.Yes, dlg.Overwrite);
        dlg.AddAction(Resources.ReferenceListForm_btnApply_Click_Keep, DialogResult.No, dlg.CopyList);
        dlg.AddAction(closeButtonText[1], DialogResult.Cancel, dlg.Cancel);
        switch (dlg.ShowDialog(this))
        {
          case DialogResult.OK:
            target.ChannelList.Channels.ForEach(ch => ch.SetPosition(target.PosIndex, -1));
            break;
          case DialogResult.Yes:
            //overwrite = true;
            break;
          case DialogResult.No:
            overwrite = false;
            break;
          case DialogResult.Cancel:
            return;
        }
      }

      main.Editor.ApplyReferenceList(this.serializer.DataRoot, src.ChannelList, src.PosIndex, target.ChannelList, target.PosIndex, false, offset, FilterChannel, overwrite, this.cbConsecutive.Checked);
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
  }
}