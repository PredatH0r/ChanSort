using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ChanSort.Api;
using ChanSort.Ui.Properties;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

namespace ChanSort.Ui
{
  // http://www.lg-forum.com/lg-led-plasma-lcd-fernseher/5098-channeleditor-40.html
  // http://www.lg-hack.info/cgi-bin/sn_forumr.cgi?fid=2677&cid=2674&tid=2690&pg=1&sc=20&x=0
  public partial class MainForm : XtraForm
  {
    #region enum EditMode
    private enum EditMode
    {
      InsertBefore = 0,
      InsertAfter = 1,
      Swap = 2
    }
    #endregion

    private readonly IList<ISerializerPlugin> plugins;
    private string currentTvFile;
    private string currentCsvFile;
    private ISerializerPlugin currentPlugin;
    private SerializerBase currentTvSerializer;
    private Editor editor;
    private readonly CsvFileSerializer csvSerializer = new CsvFileSerializer();
    private DataRoot dataRoot;
    private bool ignoreLanguageChange;
    private readonly string title;
    private Encoding defaultEncoding = Encoding.Default;
    private readonly List<string> isoEncodings = new List<string>();
    private ChannelList currentChannelList;
    private GridView lastFocusedGrid;
    private EditMode curEditMode = EditMode.InsertAfter;

    #region ctor()
    public MainForm()
    {
      if (!string.IsNullOrEmpty(Settings.Default.Language))
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
      this.LookAndFeel.SetSkinStyle("Office 2010 Blue");
      InitializeComponent();
      this.SetControlsEnabled(false);
      if (!Settings.Default.WindowSize.IsEmpty)
        this.Size = Settings.Default.WindowSize;
      this.title = this.Text;
      this.plugins = this.LoadSerializerPlugins();
      this.FillMenuWithIsoEncodings();

      this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("ChanSort.Ui.app.ico"));
      var bcLeft = new BindingContext();
      this.grpOutputList.BindingContext = bcLeft;
      this.lastFocusedGrid = this.gviewInput;
      this.comboEditMode.SelectedIndex = (int) this.curEditMode;
      this.comboUnsortedAction.SelectedIndex = (int)UnsortedChannelMode.Hide;
      this.ActiveControl = this.gridInput;
    }
    #endregion

    #region SetControlsEnabled()
    private void SetControlsEnabled(bool enabled)
    {
      foreach (Control control in this.grpTopPanel.Controls)
        control.Enabled = enabled;
      foreach (Control control in this.pnlEditControls.Controls)
      {
        if (control != this.btnClearLeftFilter && control != this.btnSyncFromLeft)
          control.Enabled = enabled;
      }

      this.miReload.Enabled = enabled;
      this.miSave.Enabled = enabled;
      this.miSaveAs.Enabled = enabled;
    }
    #endregion

    #region LoadSerializerPlugins()
    private IList<ISerializerPlugin> LoadSerializerPlugins()
    {
      var list = new List<ISerializerPlugin>();
      string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      foreach (var file in Directory.GetFiles(exeDir, "ChanSort.Loader.*.dll"))
      {
        try
        {
          var assembly = Assembly.LoadFrom(file);
          foreach(var type in assembly.GetTypes())
          {
            if (typeof(ISerializerPlugin).IsAssignableFrom(type) && !type.IsAbstract)
              list.Add((ISerializerPlugin) Activator.CreateInstance(type));
          }
        }
        catch(Exception ex) { HandleException(new IOException("Plugin " + file + "\n" + ex.Message, ex)); }
      }
      return list;
    }
    #endregion

    #region ShowOpenFileDialog()
    private void ShowOpenFileDialog()
    {
      StringBuilder filter = new StringBuilder();
      StringBuilder extension = new StringBuilder();
      foreach (var plugin in this.plugins)
      {
        filter.Append(plugin.PluginName).Append("|").Append(plugin.FileFilter);
        extension.Append(plugin.FileFilter);
        filter.Append("|");
        extension.Append(";");
      }
      if (extension.Length > 0)
        extension.Remove(extension.Length - 1, 1);

      using (OpenFileDialog dlg = new OpenFileDialog())
      {
        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        dlg.AddExtension = true;
        dlg.Filter = filter + string.Format(Resources.MainForm_FileDialog_OpenFileFilter, extension);
        dlg.FilterIndex = this.plugins.Count + 1;
        dlg.CheckFileExists = true;
        dlg.RestoreDirectory = true;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          this.SetFileName(dlg.FileName);
          var plugin = dlg.FilterIndex <= this.plugins.Count ? this.plugins[dlg.FilterIndex - 1] : null;
          this.LoadFiles(plugin);
        }
      }
    }
    #endregion

    #region SetFileName()
    private void SetFileName(string tvDataFile)
    {
      this.currentTvFile = tvDataFile;
      if (!string.IsNullOrEmpty(tvDataFile))
      {
        this.currentCsvFile = Path.Combine(Path.GetDirectoryName(this.currentTvFile),
          Path.GetFileNameWithoutExtension(this.currentTvFile) + ".csv");
      }
      this.Text = this.title + "  -  " + this.currentTvFile;
    }
    #endregion

    #region LoadFiles()

    private void LoadFiles(ISerializerPlugin plugin)
    {
      SerializerBase newSerializer = null;
      bool dataUpdated = false;
      try
      {
        if (plugin == null)
          plugin = this.GetPluginForFile(this.currentTvFile);
        // abort action if there is no currentTvSerializer for the input file
        newSerializer = plugin == null ? null : plugin.CreateSerializer(this.currentTvFile);
        if (newSerializer == null)
        {
          this.currentTvFile = this.currentTvSerializer == null ? string.Empty : this.currentTvSerializer.FileName;
          return;
        }

        if (!this.PromptSaveAndContinue())
          return;

        dataUpdated = true;
        this.gviewInput.BeginDataUpdate();
        this.gviewOutput.BeginDataUpdate();

        this.currentPlugin = plugin;
        this.currentTvSerializer = newSerializer;
        this.currentTvSerializer.DefaultEncoding = this.defaultEncoding;
        this.btnResetChannelData.Visible = newSerializer.Features.EraseChannelData;
        if (!this.LoadTvDataFile())
          return;
        this.LoadCsvFile();

        this.editor = new Editor();
        this.editor.DataRoot = this.dataRoot;

        this.currentChannelList = null;
        this.editor.ChannelList = null;
        this.gridInput.DataSource = null;
        this.gridOutput.DataSource = null;
        this.comboChannelList.Properties.Items.Clear();
        foreach (var list in this.dataRoot.ChannelLists)
          this.comboChannelList.Properties.Items.Add(list);
        this.comboChannelList.SelectedIndex = this.dataRoot.IsEmpty ? -1 : 0;

        this.SetControlsEnabled(!this.dataRoot.IsEmpty);
        this.UpdateFavoritesEditor(this.dataRoot.SupportedFavorites);
        this.colName.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.ChannelNameEdit;
        this.colOutName.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.ChannelNameEdit;

        if (this.dataRoot.Warnings.Length > 0)
        {
          var lines = this.dataRoot.Warnings.ToString().Split('\n');
          Array.Sort(lines);
          var sortedWarnings = String.Join("\n", lines);
          InfoBox.Show(this,
                       Resources.MainForm_LoadFiles_ValidationWarningMsg + "\r\n\r\n" + sortedWarnings,
                       Resources.MainForm_LoadFiles_ValidationWarningCap);
        }
      }
      catch (Exception ex)
      {
        if (ex.GetType() != typeof(Exception) && !(ex is IOException))
          throw;
        string name = newSerializer != null ? newSerializer.DisplayName : plugin.PluginName;
        XtraMessageBox.Show(this, name + "\n\n" + ex.Message, Resources.MainForm_LoadFiles_IOException,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        this.currentPlugin = null;
        this.currentTvFile = null;
        this.currentTvSerializer = null;
        this.Text = this.title;
      }
      finally
      {
        if (dataUpdated)
        {
          this.gviewInput.EndDataUpdate();
          this.gviewOutput.EndDataUpdate();
        }
      }
    }
    #endregion

    #region UpdateFavoritesEditor()
    private void UpdateFavoritesEditor(Favorites favorites)
    {
      this.repositoryItemCheckedComboBoxEdit1.Items.Clear();
      this.repositoryItemCheckedComboBoxEdit2.Items.Clear();
      byte mask = 0x01;
      string regex = "[";
      for (int bit=0; bit<8; bit++, mask<<=1)
      {
        if (((int) favorites & mask) != 0)
        {
          char c = (char) ('A' + bit);
          this.repositoryItemCheckedComboBoxEdit1.Items.Add(c);
          this.repositoryItemCheckedComboBoxEdit2.Items.Add(c);
          regex += c;
        }
      }
      regex += "]*";
      this.repositoryItemCheckedComboBoxEdit1.Mask.EditMask = regex;
      this.repositoryItemCheckedComboBoxEdit2.Mask.EditMask = regex;
    }

    #endregion

    #region GetTvFileSerializer()
    private ISerializerPlugin GetPluginForFile(string inputFileName)
    {
      if (!File.Exists(inputFileName))
      {
        XtraMessageBox.Show(this, String.Format(Resources.MainForm_LoadTll_SourceTllNotFound, inputFileName));
        return null;
      }
      string extension = (Path.GetExtension(inputFileName) ?? "").ToUpper();
      foreach (var plugin in this.plugins)
      {
        if ((plugin.FileFilter.ToUpper()+"|").Contains("*"+extension))
          return plugin;
      }

      XtraMessageBox.Show(this, String.Format(Resources.MainForm_LoadTll_SerializerNotFound, inputFileName));
      return null;
    }
    #endregion

    #region LoadTvDataFile()
    private bool LoadTvDataFile()
    {
      this.currentTvSerializer.Load();
      this.dataRoot = this.currentTvSerializer.DataRoot;
      return true;
    }
    #endregion

    #region PromptSaveAndContinue()
    private bool PromptSaveAndContinue()
    {
      if (this.dataRoot == null || !this.dataRoot.NeedsSaving)
        return true;

      switch (XtraMessageBox.Show(this,
                                  Resources.MainForm_PromptSaveAndContinue_Message,
                                  Resources.MainForm_PromptSaveAndContinue_Caption,
                                  MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button3))
      {
        case DialogResult.Yes: this.SaveFiles(); break;
        case DialogResult.No: break;
        case DialogResult.Cancel: return false;
      }
      return true;
    }
    #endregion

    #region LoadCsvFile()
    private void LoadCsvFile()
    {
      if (File.Exists(this.currentCsvFile))
        this.csvSerializer.Load(this.currentCsvFile, this.dataRoot);
      else
      {
        foreach (var list in this.dataRoot.ChannelLists)
        {
          foreach (var channel in list.Channels)
            channel.NewProgramNr = channel.OldProgramNr;
        }
      }
    }
    #endregion

    #region ShowOpenReferenceFileDialog()
    private void ShowOpenReferenceFileDialog()
    {
      using (OpenFileDialog dlg = new OpenFileDialog())
      {
        dlg.Title = Resources.MainForm_ShowOpenReferenceFileDialog_Title;
        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        dlg.AddExtension = true;
        dlg.Filter = Resources.MainForm_ShowOpenReferenceFileDialog_Filter;
        dlg.FilterIndex = 3;
        dlg.CheckFileExists = true;
        dlg.RestoreDirectory = true;
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
          this.gviewInput.BeginDataUpdate();
          this.gviewOutput.BeginDataUpdate();

          string ext = (Path.GetExtension(dlg.FileName) ?? "").ToLower();
          if (ext == ".csv")
            this.csvSerializer.Load(dlg.FileName, this.dataRoot);
          else if (ext == ".chl")
          {
            ChlFileSerializer loader = new ChlFileSerializer();
            string warnings = loader.Load(dlg.FileName, this.dataRoot, this.currentChannelList);
            InfoBox.Show(this, warnings, Path.GetFileName(dlg.FileName));
          }

          this.gviewInput.EndDataUpdate();
          this.gviewOutput.EndDataUpdate();
        }
      }
    }
    #endregion


    #region ShowChannelList()
    private void ShowChannelList(ChannelList channelList)
    {
      if (this.currentChannelList != null)
        this.SaveInputGridLayout(this.currentChannelList.SignalSource);
      if (channelList != null)
      {
        this.LoadInputGridLayout(channelList.SignalSource);
        this.gridInput.DataSource = channelList.Channels;
        this.gridOutput.DataSource = channelList.Channels;
      }
      else
      {
        this.gridInput.DataSource = null;
        this.gridOutput.DataSource = null;        
      }
      this.currentChannelList = channelList;
      this.editor.ChannelList = channelList;

      if (gviewInput.IsValidRowHandle(0))
      {
        this.gviewInput.FocusedRowHandle = 0;
        this.gviewInput.SelectRow(0);
      }
      if (gviewOutput.IsValidRowHandle(0))
      {
        this.gviewOutput.FocusedRowHandle = 0;
        this.gviewOutput.SelectRow(0);
      }
      this.UpdateInsertSlotTextBox();
    }
    #endregion

    #region ShowSaveFileDialog()

    private void ShowSaveFileDialog()
    {
      string extension = Path.GetExtension(this.currentTvSerializer.FileName) ?? ".";
      using (SaveFileDialog dlg = new SaveFileDialog())
      {
        dlg.FileName = this.currentTvFile;
        dlg.AddExtension = true;
        dlg.DefaultExt = extension;
        dlg.Filter = string.Format(Resources.MainForm_FileDialog_SaveFileFilter, extension);
        dlg.FilterIndex = 0;
        dlg.ValidateNames = true;
        dlg.RestoreDirectory = true;
        dlg.OverwritePrompt = true;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          this.SetFileName(dlg.FileName);
          this.SaveFiles();
        }
      }
    }

    #endregion

    #region SaveFiles()

    private void SaveFiles()
    {
      this.gviewInput.PostEditor();
      this.gviewOutput.PostEditor();
      try
      {
        this.gviewInput.BeginDataUpdate();
        this.gviewOutput.BeginDataUpdate();
        this.editor.AutoNumberingForUnassignedChannels((UnsortedChannelMode)this.comboUnsortedAction.SelectedIndex);
        this.SaveReferenceFile();
        this.SaveTvDataFile();
        this.dataRoot.NeedsSaving = false;
      }
      catch (IOException ex)
      {
        XtraMessageBox.Show(this, 
          Resources.MainForm_SaveFiles_ErrorMsg +
          ex.Message, 
          Resources.MainForm_SaveFiles_ErrorTitle, 
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        this.gviewInput.EndDataUpdate();
        this.gviewOutput.EndDataUpdate();
      }
    }

    #endregion

    #region SaveReferenceFile()

    private void SaveReferenceFile()
    {
      this.csvSerializer.Save(this.currentCsvFile, this.dataRoot, UnsortedChannelMode.Hide);
    }

    #endregion

    #region SaveTvDataFile()

    private void SaveTvDataFile()
    {
      this.splashScreenManager1.ShowWaitForm();
      try
      {
        // create backup file if none exists
        if (File.Exists(currentTvFile))
        {
          string bakFile = currentTvFile + ".bak";
          if (!File.Exists(bakFile))
            File.Copy(currentTvFile, bakFile);
        }
        this.currentTvSerializer.Save(this.currentTvFile, this.currentCsvFile);
      }
      finally
      {
        this.splashScreenManager1.CloseWaitForm();
      }
    }
    #endregion

    #region AddChannels()
    private void AddChannels()
    {
      var selectedChannels = this.GetSelectedChannels(gviewInput);
      if (selectedChannels.Count == 0) return;

      ChannelInfo lastInsertedChannel;
      this.gviewOutput.BeginDataUpdate();
      this.gviewInput.BeginDataUpdate();
      try
      {
        lastInsertedChannel = this.editor.AddChannels(selectedChannels);
        this.UpdateInsertSlotTextBox();
      }
      finally
      {
        this.gviewInput.EndDataUpdate();
        this.gviewOutput.EndDataUpdate();
      }

      if (lastInsertedChannel == null)
      {
        this.NavigateToChannel(selectedChannels[0], this.gviewOutput);
        return;
      }

      this.gviewOutput.BeginSelection();
      try
      {
        this.gviewOutput.ClearSelection();
        int index = this.currentChannelList.Channels.IndexOf(lastInsertedChannel);
        int rowHandle = this.gviewOutput.GetRowHandle(index);
        if (this.comboEditMode.SelectedIndex == (int)EditMode.InsertBefore)
          ++rowHandle;
        this.gviewOutput.FocusedRowHandle = rowHandle;
        this.gviewOutput.SelectRow(rowHandle);
      }
      finally
      {
        this.gviewOutput.EndSelection();
      }
    }
    #endregion

    #region RemoveChannels()

    private void RemoveChannels()
    {
      var selectedChannels = this.GetSelectedChannels(this.lastFocusedGrid);
      if (selectedChannels.Count == 0) return;

      int focusedRow = Math.Max(0, this.gviewOutput.FocusedRowHandle - selectedChannels.Count);
      if (!gviewOutput.IsLastRow)
        ++focusedRow;
      this.gviewInput.BeginDataUpdate();
      this.gviewOutput.BeginDataUpdate();
      try
      {
        this.editor.RemoveChannels(selectedChannels);
      }
      finally
      {
        this.gviewOutput.EndDataUpdate();
        this.gviewInput.EndDataUpdate();
      }
      this.gviewOutput.FocusedRowHandle = focusedRow;
      this.gviewOutput.SelectRow(focusedRow);
      this.UpdateInsertSlotTextBox();
    }

    #endregion

    #region MoveChannels()

    private void MoveChannels(bool up)
    {
      var selectedChannels = this.GetSelectedChannels(this.gviewOutput);
      if (selectedChannels.Count == 0) return;

      this.gviewOutput.BeginDataUpdate();
      try
      {
        this.editor.MoveChannels(selectedChannels, up);
      }
      finally
      {
        this.gviewOutput.EndDataUpdate();
      }
      this.UpdateInsertSlotTextBox();
    }

    #endregion

    #region SetSlotNumber()
    private bool SetSlotNumber(string slotNr)
    {
      int slot;
      if (!int.TryParse(slotNr, out slot) || slot < 0)
        return false;
      var selectedChannels = this.GetSelectedChannels(this.lastFocusedGrid);
      if (selectedChannels.Count == 0) return true;
      this.gviewOutput.BeginDataUpdate();
      this.gviewInput.BeginDataUpdate();
      try
      {
        this.editor.SetSlotNumber(selectedChannels, slot, this.comboEditMode.SelectedIndex == (int)EditMode.Swap);
      }
      finally
      {
        this.gviewInput.EndDataUpdate();
        this.gviewOutput.EndDataUpdate();
      }
      return true;
    }
    #endregion

    #region SortSelectedChannels()
    private void SortSelectedChannels()
    {
      var selectedChannels = this.GetSelectedChannels(this.gviewOutput);
      if (selectedChannels.Count == 0) return;
      this.gviewOutput.BeginDataUpdate();
      this.gviewInput.BeginDataUpdate();
      try
      {
        this.editor.SortSelectedChannels(selectedChannels);
      }
      finally
      {
        this.gviewInput.EndDataUpdate();
        this.gviewOutput.EndDataUpdate();
      }
    }
    #endregion

    #region CloneChannelList()
    private void CloneChannelList()
    {
      if (this.currentChannelList == null) return;
      this.gviewInput.BeginDataUpdate();
      this.gviewOutput.BeginDataUpdate();
      foreach (var channel in this.currentChannelList.Channels)
        channel.NewProgramNr = channel.OldProgramNr;
      this.gviewInput.EndDataUpdate();
      this.gviewOutput.EndDataUpdate();
    }
    #endregion

    #region RenumberSelectedChannels()
    private void RenumberSelectedChannels()
    {
      var list = this.GetSelectedChannels(this.gviewOutput);
      if (list.Count == 0) return;
      this.gviewInput.BeginDataUpdate();
      this.gviewOutput.BeginDataUpdate();
      this.editor.RenumberChannels(list);
      this.gviewOutput.EndDataUpdate();
      this.gviewInput.EndDataUpdate();
    }
    #endregion

    #region SwapChannels()
    private void SwapChannels()
    {
      int oldMode = this.comboEditMode.SelectedIndex;
      this.comboEditMode.SelectedIndex = (int)EditMode.Swap;
      this.RemoveChannels();
      this.AddChannels();
      this.comboEditMode.SelectedIndex = oldMode;
    }
    #endregion

    #region GetSelectedChannels()
    private List<ChannelInfo> GetSelectedChannels(GridView gview)
    {
      var channels = new List<ChannelInfo>();
      if (gview.IsDataRow(gview.FocusedRowHandle))
        gview.SelectRow(gview.FocusedRowHandle);
      foreach (int rowHandle in gview.GetSelectedRows())
      {
        if (gview.IsDataRow(rowHandle))
          channels.Add((ChannelInfo)gview.GetRow(rowHandle));
      }
      return channels;
    }
    #endregion

    #region TryExecute()

    private void TryExecute(Action action)
    {
      try { action(); }
      catch (Exception ex) { HandleException(ex); }
    }

    #endregion

    #region HandleException()
    public static void HandleException(Exception ex)
    {
      XtraMessageBox.Show(string.Format(Resources.MainForm_TryExecute_Exception, ex));
    }
    #endregion

    #region LoadSettings()
    private void LoadSettings()
    {
      // note: WindowSize must be restored in ctor in order to make WindowStartPosition.CenterScreen work
      if (!string.IsNullOrEmpty(Settings.Default.Encoding))
        this.defaultEncoding = Encoding.GetEncoding(Settings.Default.Encoding);
      this.SetFileName(Settings.Default.InputTLL);
      int width = Settings.Default.LeftPanelWidth;
      if (width > 0)
        this.splitContainerControl1.SplitterPosition = width;
      this.SelectLanguageMenuItem();

      this.SetGridLayout(this.gviewOutput, Settings.Default.OutputListLayout);
      this.gviewOutput.ClearSelection();
      if (this.gviewOutput.IsValidRowHandle(this.gviewOutput.FocusedRowHandle))
        this.gviewOutput.SelectRow(Settings.Default.OutputListRowHandle);

      this.ClearLeftFilter();
    }
    #endregion

    #region SelectLanguageMenuItem()
    private void SelectLanguageMenuItem()
    {
      this.barManager1.ForceLinkCreate();
      foreach (BarItemLink itemLink in this.barSubItem1.ItemLinks)
      {
        if (Settings.Default.Language.StartsWith((string) itemLink.Item.Tag))
        {
          this.ignoreLanguageChange = true;
          ((BarButtonItem) itemLink.Item).Down = true;
          this.ignoreLanguageChange = false;
          break;
        }
      }
    }
    #endregion

    #region SetGridLayout()
    private void SetGridLayout(GridView grid, string layout)
    {
      if (string.IsNullOrEmpty(layout)) return;
      MemoryStream stream = new MemoryStream();
      using (StreamWriter wrt = new StreamWriter(stream))
      {
        wrt.Write(layout);
        wrt.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        grid.RestoreLayoutFromStream(stream);
      }

      // put the filter text back into the auto-filter-row
      foreach (GridColumn col in grid.Columns)
      {
        string[] parts = (col.FilterInfo.FilterString ?? "").Split('\'');
        if (parts.Length >= 2)
          this.gviewInput.SetRowCellValue(GridControl.AutoFilterRowHandle, col, parts[1]);
      }
    }
    #endregion

    #region SaveSettings(), GetGridLayout()
    private void SaveSettings()
    {
      this.gviewInput.PostEditor();
      this.gviewOutput.PostEditor();

      Settings.Default.WindowSize = this.WindowState == FormWindowState.Normal ? this.Size : this.RestoreBounds.Size;
      Settings.Default.Encoding = this.defaultEncoding.WebName;
      Settings.Default.Language = Thread.CurrentThread.CurrentUICulture.Name;
      Settings.Default.InputTLL = this.currentTvFile;
      Settings.Default.LeftPanelWidth = this.splitContainerControl1.SplitterPosition;
      Settings.Default.OutputListLayout = GetGridLayout(this.gviewOutput);
      if (this.currentChannelList != null)
        SaveInputGridLayout(this.currentChannelList.SignalSource);
      Settings.Default.Save();
    }

    private string GetGridLayout(GridView grid)
    {
      MemoryStream stream = new MemoryStream();
      grid.SaveLayoutToStream(stream, OptionsLayoutBase.FullLayout);
      stream.Seek(0, SeekOrigin.Begin);
      using (StreamReader rdr = new StreamReader(stream, Encoding.UTF8))
        return rdr.ReadToEnd();
    }

    #endregion

    #region UpdateInsertSlotTextBox()
    private void UpdateInsertSlotTextBox()
    {
      int programNr = this.currentChannelList == null ? 0 : this.currentChannelList.InsertProgramNumber;
      this.txtSetSlot.Text = programNr.ToString();
    }
    #endregion

    #region FillMenuWithIsoEncodings()
    private void FillMenuWithIsoEncodings()
    {
      this.miIsoCharSets.Strings.Clear();
      this.isoEncodings.Clear();
      foreach(var encoding in Encoding.GetEncodings())
      {
        if (!encoding.Name.StartsWith("iso"))
          continue;
        this.miIsoCharSets.Strings.Add(encoding.DisplayName);
        this.isoEncodings.Add(encoding.Name);
      }
    }
    #endregion

    #region ShowCharsetForm()
    private void ShowCharsetForm()
    {
      using (var form = new CharsetForm(this.defaultEncoding))
      {
        form.Location = new Point(this.Location.X + 30, this.Location.Y + 70);
        form.EncodingChanged += this.charsetForm_EncodingChanged;
        form.ShowDialog(this);
      }
    }
    #endregion

    #region SetDefaultEncoding()
    private void SetDefaultEncoding(Encoding encoding)
    {
      this.defaultEncoding = encoding;
      if (this.currentTvSerializer == null)
        return;
      this.gviewInput.BeginDataUpdate();
      this.gviewOutput.BeginDataUpdate();
      this.currentTvSerializer.DefaultEncoding = encoding;
      this.gviewInput.EndDataUpdate();
      this.gviewOutput.EndDataUpdate();
    }
    #endregion

    #region ClearLeftFilter(), ClearRightFilter()
    private void ClearLeftFilter()
    {
      this.gviewOutput.BeginSort();
      this.gviewOutput.ClearColumnsFilter();
      this.colOutSlot.FilterInfo = new ColumnFilterInfo("[NewProgramNr]<>0");
      this.gviewOutput.EndSort();
    }

    private void ClearRightFilter()
    {
      this.gviewInput.BeginSort();
      this.gviewInput.ClearColumnsFilter();
      this.colSlotOld.FilterInfo = new ColumnFilterInfo("[OldProgramNr]<>0");
      this.gviewInput.EndSort();
    }
    #endregion

    #region SyncLists()
    private void SyncLists(GridView source, GridView target)
    {
      var channel = (ChannelInfo)source.GetFocusedRow();
      if (channel == null)
        return;
      target.SetRowCellValue(GridControl.AutoFilterRowHandle, colName, channel.Name);
      target.BeginSelection();
      target.ClearSelection();
      int rowIndex = ((IList<ChannelInfo>) target.DataSource).IndexOf(channel);
      int rowHandle = target.GetRowHandle(rowIndex);
      target.FocusedRowHandle = rowHandle;
      target.SelectRow(rowHandle);
      target.EndSelection();
    }
    #endregion

    #region LoadInputGridLayout()
    private void LoadInputGridLayout(SignalSource newSource)
    {
      string newLayout;
      if ((newSource & SignalSource.Digital) == 0)
        newLayout = Settings.Default.InputGridLayoutAnalog;
      else if (newSource == SignalSource.DvbS)
        newLayout = Settings.Default.InputGridLayoutDvbS; 
      else
        newLayout = Settings.Default.InputGridLayoutDvbCT;
      if (!string.IsNullOrEmpty(newLayout))
        this.SetGridLayout(this.gviewInput, newLayout);
      
      foreach (GridColumn col in this.gviewInput.Columns)
        col.Visible = GetGridColumnVisibility(col, newSource);

      this.gviewInput.ClearSelection();
      if (this.gviewInput.IsValidRowHandle(this.gviewInput.FocusedRowHandle))
        this.gviewInput.SelectRow(Settings.Default.InputListRowHandle);

      this.ClearRightFilter();
    }
    #endregion

    #region SaveInputGridLayout()
    private void SaveInputGridLayout(SignalSource signalSource)
    {
      string currentLayout = GetGridLayout(this.gviewInput);
      switch (signalSource)
      {
        case SignalSource.Analog: Settings.Default.InputGridLayoutAnalog = currentLayout; break;
        case SignalSource.DvbCT: Settings.Default.InputGridLayoutDvbCT = currentLayout; break;
        case SignalSource.DvbS: Settings.Default.InputGridLayoutDvbS = currentLayout; break;
      }
    }
    #endregion

    #region GetGridColumnVisibility()

    private bool GetGridColumnVisibility(GridColumn col, SignalSource source)
    {
      if (col == this.colShortName) return (source & SignalSource.Digital) != 0;
      if (col == this.colEncrypted) return (source & SignalSource.Digital) != 0;
      if (col == this.colServiceId) return (source & SignalSource.Digital) != 0;
      if (col == this.colVideoPid) return (source & SignalSource.Digital) != 0;
      if (col == this.colAudioPid) return (source & SignalSource.Digital) != 0;
      if (col == this.colServiceType) return (source & SignalSource.Digital) != 0;
      if (col == this.colServiceTypeName) return (source & SignalSource.Digital) != 0;
      if (col == this.colEncrypted) return (source & SignalSource.Digital) != 0;
      if (col == this.colTransportStreamId) return (source & SignalSource.Digital) != 0;
      if (col == this.colNetworkName) return (source & SignalSource.Digital) != 0;
      if (col == this.colNetworkOperator) return (source & SignalSource.Digital) != 0;
      if (col == this.colSatellite) return (source & SignalSource.Sat) != 0;
      if (col == this.colNetworkId) return (source & SignalSource.Digital) != 0;
      if (col == this.colSymbolRate) return (source & SignalSource.Sat) != 0;
      if (col == this.colIndex) return col.Visible;
      if (col == this.colUid) return col.Visible;
      if (col == this.colDebug) return colDebug.Visible;
      if (col == this.colLogicalIndex) return colLogicalIndex.Visible;
      if (col == this.colPolarity) return false;

      return true;
    }

    #endregion

    #region SetFavorite()
    private void SetFavorite(string fav, bool set)
    {
      if (string.IsNullOrEmpty(fav)) return;
      char ch = Char.ToUpper(fav[0]);
      if (ch<'A' || ch>'D') return;
      var list = this.GetSelectedChannels(this.lastFocusedGrid);
      if (list.Count == 0) return;

      this.gviewInput.BeginDataUpdate();
      this.gviewOutput.BeginDataUpdate();
      Favorites mask = (Favorites)(1 << (ch - 'A'));
      foreach(var channel in list)
      {
        if (set)
          channel.Favorites |= mask;
        else
          channel.Favorites &= ~mask;
      }
      this.gviewInput.EndDataUpdate();
      this.gviewOutput.EndDataUpdate();
    }
    #endregion

    #region SetChannelFlag()
    private void SetChannelFlag(Action<ChannelInfo> setFlag)
    {
      var list = this.GetSelectedChannels(this.lastFocusedGrid);
      if (list.Count == 0) return;

      this.gviewInput.BeginDataUpdate();
      this.gviewOutput.BeginDataUpdate();
      foreach (var channel in list)
        setFlag(channel);
      this.gviewInput.EndDataUpdate();
      this.gviewOutput.EndDataUpdate();
      this.dataRoot.NeedsSaving = true;
    }
    #endregion

    #region NavigateToChannel
    private void NavigateToChannel(ChannelInfo channel, GridView view)
    {
      if (channel == null) return;
      int rowHandle = this.gviewInput.GetRowHandle(this.currentChannelList.Channels.IndexOf(channel));
      if (view.IsValidRowHandle(rowHandle))
      {
        view.ClearSelection();
        view.FocusedRowHandle = rowHandle;
        view.SelectRow(rowHandle);
        view.MakeRowVisible(rowHandle);
      }
    }
    #endregion

    #region UpdateMenu
    private void UpdateMenu()
    {
      bool isRight = this.lastFocusedGrid == this.gviewInput;
      this.miAddChannel.Enabled = isRight;

      var visRight = isRight ? BarItemVisibility.Always : BarItemVisibility.Never;
      var visLeft = isRight ? BarItemVisibility.Never : BarItemVisibility.Always;

      this.miSort.Visibility = visLeft;
      this.miRenum.Visibility = visLeft;
      this.miMoveUp.Visibility = visLeft;
      this.miMoveDown.Visibility = visLeft;
      this.miRemove.Visibility = visLeft;
      this.miAddChannel.Visibility = visRight;

      var sel = this.gviewOutput.GetSelectedRows();
      var channel = sel.Length == 0 ? null : (ChannelInfo) this.gviewInput.GetRow(sel[0]);
      this.miMoveUp.Enabled = channel != null && channel.NewProgramNr > 1;

      this.miTvSettings.Enabled = this.currentTvSerializer != null;
    }
    #endregion

    #region RestoreBackupFile()
    private void RestoreBackupFile()
    {
      var bakFile = this.currentTvFile + ".bak";
      if (!File.Exists(bakFile))
      {
        XtraMessageBox.Show(this, string.Format(Resources.MainForm_miRestoreOriginal_ItemClick_NoBackup, bakFile),
          this.miRestoreOriginal.Caption,
          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }

      if (XtraMessageBox.Show(this,
                              Resources.MainForm_miRestoreOriginal_ItemClick_Confirm,
                              this.miRestoreOriginal.Caption,
                              MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) !=
          DialogResult.Yes)
      {
        return;
      }

      try
      {
        File.Copy(bakFile, this.currentTvFile, true);
        if (this.currentPlugin != null)
          this.LoadFiles(this.currentPlugin);
      }
      catch (Exception)
      {
        XtraMessageBox.Show(this, string.Format(Resources.MainForm_miRestoreOriginal_Message, this.currentTvFile),
          this.miRestoreOriginal.Caption,
          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }      
    }
    #endregion

    #region ShowFileInformation()
    private void ShowFileInformation()
    {
      if (this.currentTvSerializer == null)
        return;
      var info = this.currentTvSerializer.GetFileInformation();
      InfoBox.Show(this, info, this.miFileInformation.Caption.Replace("...", "").Replace("&",""));
    }
    #endregion

    #region VerifyChannelNameModified()
    private void VerifyChannelNameModified(ChannelInfo info, string newName)
    {
      if (newName != info.Name)
        info.IsNameModified = true;
    }
    #endregion

    #region ShowTvCountrySettings()
    private void ShowTvCountrySettings()
    {
      if (this.currentTvSerializer != null)
        this.currentTvSerializer.ShowDeviceSettingsForm(this);
    }
    #endregion

    // UI events

    #region MainForm_Load
    private void MainForm_Load(object sender, EventArgs e)
    {
      TryExecute(this.LoadSettings);
    }
    #endregion

    #region MainForm_Shown
    private void MainForm_Shown(object sender, EventArgs e)
    {
      try
      {
        if (this.currentTvFile != "" && File.Exists(this.currentTvFile))
          this.LoadFiles(null);
      }
      catch (Exception ex) { HandleException(ex); }
    }
    #endregion

    // -- menus

    #region File menu

    private void miOpen_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.ShowOpenFileDialog);
    }

    private void miOpenReferenceFile_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.ShowOpenReferenceFileDialog);
    }

    private void miReload_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(() => this.LoadFiles(this.currentPlugin));
    }

    private void miRestoreOriginal_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.RestoreBackupFile);
    }

    private void miFileInformation_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.ShowFileInformation);
    }

    private void miSave_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.SaveFiles);
    }

    private void miSaveAs_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.ShowSaveFileDialog);
    }

    private void miQuit_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.Close();
    }

    #endregion

    #region Edit menu

    private void miMoveDown_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.MoveChannels(false));
    }

    private void miMoveUp_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.MoveChannels(true));
    }

    private void miRemove_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.RemoveChannels);
    }

    private void miReplace_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.SwapChannels);
    }

    private void miFavSet_ListItemClick(object sender, ListItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetFavorite(this.miFavSet.Strings[e.Index], true));
    }

    private void miFavUnset_ListItemClick(object sender, ListItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetFavorite(this.miFavSet.Strings[e.Index], false));
    }

    private void miLockOn_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetChannelFlag(ch => ch.Lock = true));
    }

    private void miLockOff_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetChannelFlag(ch => ch.Lock = false));
    }

    private void miSkipOn_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetChannelFlag(ch => ch.Skip = true));
    }

    private void miSkipOff_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetChannelFlag(ch => ch.Skip = false));
    }

    private void miHideOn_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetChannelFlag(ch => ch.Hidden = true));
    }

    private void miHideOff_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.SetChannelFlag(ch => ch.Hidden = false));
    }
    #endregion

    #region Language menu
    private void miLanguage_DownChanged(object sender, ItemClickEventArgs e)
    {
      try
      {
        if (this.ignoreLanguageChange)
          return;
        BarButtonItem menuItem = (BarButtonItem)sender;
        if (!menuItem.Down)
          return;
        if (!this.PromptSaveAndContinue())
          return;
        string locale = (string)menuItem.Tag;
        Program.ChangeLanguage = true;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
        this.Close();
      }
      catch (Exception ex) { HandleException(ex); }
    }
    #endregion

    #region TV-Set menu
    private void miTvCountrySetup_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.ShowTvCountrySettings);
    }
    #endregion

    #region Character set menu

    private void miIsoCharSets_ListItemClick(object sender, ListItemClickEventArgs e)
    {
      TryExecute(() => this.SetDefaultEncoding(Encoding.GetEncoding(this.isoEncodings[e.Index])));
    }

    private void miCharset_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(ShowCharsetForm);
    }

    private void charsetForm_EncodingChanged(object sender, EncodingChangedEventArgs e)
    {
      SetDefaultEncoding(e.Encoding);
    }
    #endregion

    #region miAbout_ItemClick
    private void miAbout_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(() => new AboutForm(this.plugins).ShowDialog());
    }
    #endregion

    // -- controls

    #region comboChannelList_SelectedIndexChanged
    private void comboChannelList_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.TryExecute(() => ShowChannelList((ChannelList) this.comboChannelList.SelectedItem));
    }
    #endregion

    #region gview_MouseDown
    private void gview_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        GridView view = (GridView)sender;
        var hit = view.CalcHitInfo(e.Location);
        if (view.IsDataRow(hit.RowHandle) && !view.IsRowSelected(hit.RowHandle))
        {
          view.ClearSelection();
          view.SelectRow(hit.RowHandle);
        }
      }
    }
    #endregion

    #region gviewOutput_LayoutUpgrade, gviewInput_LayoutUpgrade
    private void gviewOutput_LayoutUpgrade(object sender, LayoutUpgadeEventArgs e)
    {
      this.gviewOutput.ClearGrouping();
      this.gviewOutput.OptionsCustomization.AllowGroup = false;
    }

    private void gviewInput_LayoutUpgrade(object sender, LayoutUpgadeEventArgs e)
    {
      this.gviewInput.ClearGrouping();
      this.gviewInput.OptionsCustomization.AllowGroup = false;
    }
    #endregion

    #region gridInput_Enter, gridOutput_Enter
    private void gridInput_Enter(object sender, EventArgs e)
    {
      this.lastFocusedGrid = this.gviewInput;
      this.UpdateMenu();
    }

    private void gridOutput_Enter(object sender, EventArgs e)
    {
      this.lastFocusedGrid = this.gviewOutput;
      this.UpdateMenu();
    }
    #endregion

    #region gviewInput_FocusedRowChanged
    private void gviewInput_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
      if (!this.gviewInput.IsRowSelected(e.FocusedRowHandle))
        this.gviewInput.SelectRow(e.FocusedRowHandle);
    }
    #endregion

    #region gviewInput_CustomColumnDisplayText
    private void gviewInput_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column == this.colSlotNew)
      {
        if (!(e.Value is int)) return;
        if ((int) e.Value == 0)
          e.DisplayText = string.Empty;
      }
      else if (e.Column == this.colFavorites)
      {
        if (!(e.Value is Favorites)) return;
        if ((Favorites) e.Value == 0)
          e.DisplayText = string.Empty;
      }
    }
    #endregion

    #region gviewInput_RowCellStyle
    private void gviewInput_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      ChannelInfo channel = (ChannelInfo)this.gviewInput.GetRow(e.RowHandle);
      if (channel == null) return;
      if (channel.OldProgramNr == 0)
      {
        e.Appearance.ForeColor = Color.Red;
        e.Appearance.Options.UseForeColor = true;
      }
      else if (channel.NewProgramNr != 0)
      {
        e.Appearance.ForeColor = Color.Gray;
        e.Appearance.Options.UseForeColor = true;
      }
    }
    #endregion

    #region gviewInput_RowClick
    private void gviewInput_RowClick(object sender, RowClickEventArgs e)
    {
      if (e.Clicks == 2 && e.Button == MouseButtons.Left && this.gviewInput.IsDataRow(e.RowHandle))
        TryExecute(this.AddChannels);
    }
    #endregion

    #region gridInput_ProcessGridKey
    private void gridInput_ProcessGridKey(object sender, KeyEventArgs e)
    {
      if (this.gviewInput.ActiveEditor != null)
        return;
      if (e.KeyCode == Keys.Enter)
      {
        TryExecute(this.AddChannels);
        e.Handled = true;
      }
    }

    #endregion

    #region gviewInput_ValidatingEditor
    private void gviewInput_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
    {
      try
      {
        if (gviewInput.FocusedRowHandle == GridControl.AutoFilterRowHandle)
          return;
        if (this.gviewInput.FocusedColumn == this.colSlotNew && e.Value is string)
          e.Valid = this.SetSlotNumber((string)e.Value);
        else if (this.gviewInput.FocusedColumn == this.colFavorites && e.Value is string)
          e.Value = ChannelInfo.ParseFavString((string)e.Value);
        else if (gviewInput.FocusedColumn == this.colName)
          this.VerifyChannelNameModified(this.gviewInput.GetFocusedRow() as ChannelInfo, e.Value as string);
        dataRoot.NeedsSaving = true;
      } catch(Exception ex) { HandleException(ex); }
    }
    #endregion

    #region gviewInput_PopupMenuShowing
    private void gviewInput_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      this.lastFocusedGrid = this.gviewInput;
      this.UpdateMenu();
      if (e.MenuType == GridMenuType.Row)
        this.mnuContext.ShowPopup(this.gridInput.PointToScreen(e.Point));
    }
    #endregion

    #region gviewOutput_FocusedRowChanged

    private void gviewOutput_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
      var channel = (ChannelInfo)this.gviewOutput.GetRow(e.FocusedRowHandle);
      if (channel == null)
        return;
      int programNr = channel.NewProgramNr;
      if (this.comboEditMode.SelectedIndex == (int)EditMode.InsertAfter)
        ++programNr;
      if (this.currentChannelList != null)
        this.currentChannelList.InsertProgramNumber = programNr;
      this.UpdateInsertSlotTextBox();
      if (!this.gviewOutput.IsRowSelected(e.FocusedRowHandle))
        this.gviewOutput.SelectRow(e.FocusedRowHandle);
    }

    #endregion

    #region gviewOutput_CustomColumnDisplayText
    private void gviewOutput_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column == this.colOutFav)
      {
        if (!(e.Value is Favorites)) return;
        if ((Favorites)e.Value == 0)
          e.DisplayText = string.Empty;
      }
    }
    #endregion

    #region gviewOutput_RowCellStyle
    private void gviewOutput_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      var channel = (ChannelInfo)this.gviewOutput.GetRow(e.RowHandle);
      if (channel == null) return;
      if (channel.OldProgramNr == 0)
      {
        e.Appearance.ForeColor = Color.Red;
        e.Appearance.Options.UseForeColor = true;
      }
    }
    #endregion

    #region gviewOutput_ValidatingEditor
    private void gviewOutput_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
    {
      try
      {
        if (gviewOutput.FocusedRowHandle == GridControl.AutoFilterRowHandle)
          return;
        if (this.gviewOutput.FocusedColumn == this.colOutSlot && e.Value is string)
          e.Valid = this.SetSlotNumber((string)e.Value);
        else if (this.gviewOutput.FocusedColumn == this.colOutFav && e.Value is string)
          e.Value = ChannelInfo.ParseFavString((string) e.Value);
        else if (gviewOutput.FocusedColumn == this.colOutName)
          this.VerifyChannelNameModified(this.gviewOutput.GetFocusedRow() as ChannelInfo, e.Value as string);
        dataRoot.NeedsSaving = true;
      }
      catch (Exception ex) { HandleException(ex); }
    }
    #endregion

    #region gviewOutput_CellValueChanged
    private void gviewOutput_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {
      this.gviewInput.BeginDataUpdate();
      this.gviewInput.EndDataUpdate();
    }
    #endregion

    #region gviewOutput_PopupMenuShowing
    private void gviewOutput_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      this.lastFocusedGrid = this.gviewOutput;
      this.UpdateMenu();
      if (e.MenuType == GridMenuType.Row)
        this.mnuContext.ShowPopup(this.gridOutput.PointToScreen(e.Point));
    }
    #endregion

    #region gviewOutput_RowClick
    private void gviewOutput_RowClick(object sender, RowClickEventArgs e)
    {
      if (e.Clicks == 2 && e.Button == MouseButtons.Left && this.gviewOutput.IsDataRow(e.RowHandle))
      {
        ChannelInfo channel = (ChannelInfo) this.gviewOutput.GetRow(e.RowHandle);
        this.NavigateToChannel(channel, this.gviewInput);
      }
    }
    #endregion

    #region gridOutput_ProcessGridKey
    private void gridOutput_ProcessGridKey(object sender, KeyEventArgs e)
    {
      if (gviewOutput.ActiveEditor != null)
        return;
      if (e.KeyCode == Keys.Delete)
        TryExecute(this.RemoveChannels);
      else if (e.KeyCode == Keys.Add)
        TryExecute(() => this.MoveChannels(false));
      else if (e.KeyCode == Keys.Subtract)
        TryExecute(() => this.MoveChannels(true));
      else
        return;
      e.Handled = true;
    }
    #endregion

    #region comboEditMode_SelectedIndexChanged
    private void comboEditMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        if (this.currentChannelList == null)
          return;
        int delta = this.curEditMode == EditMode.InsertAfter ? -1 :
          (EditMode)this.comboEditMode.SelectedIndex == EditMode.InsertAfter ? +1 : 0;
        this.currentChannelList.InsertProgramNumber += delta;
        this.UpdateInsertSlotTextBox();
        this.curEditMode = (EditMode) this.comboEditMode.SelectedIndex;
      } catch(Exception ex) { HandleException(ex); }
    }
    #endregion

    #region btnClearLeftFilter_Click, btnClearRightFilter_Click
    private void btnClearLeftFilter_Click(object sender, EventArgs e)
    {
      TryExecute(this.ClearLeftFilter);
    }

    private void btnClearRightFilter_Click(object sender, EventArgs e)
    {
      TryExecute(this.ClearRightFilter);
    }
    #endregion

    #region btnSyncFromLeft_Click, btnSyncFromRight_Click
    private void btnSyncFromLeft_Click(object sender, EventArgs e)
    {
      TryExecute(() => this.SyncLists(this.gviewOutput, this.gviewInput));
    }

    private void btnSyncFromRight_Click(object sender, EventArgs e)
    {
      TryExecute(() => this.SyncLists(this.gviewInput, this.gviewOutput));
    }
    #endregion

    #region btnAdd_Click

    private void btnAdd_Click(object sender, EventArgs e)
    {
      TryExecute(this.AddChannels);
    }
    #endregion

    #region btnRemove_Click

    private void btnRemove_Click(object sender, EventArgs e)
    {
      TryExecute(this.RemoveChannels);
    }
    #endregion

    #region btnUp_Click, btnDown_Click

    private void btnUp_Click(object sender, EventArgs e)
    {
      TryExecute(() => MoveChannels(true));
    }

    private void btnDown_Click(object sender, EventArgs e)
    {
      TryExecute(() => MoveChannels(false));
    }

    #endregion

    #region txtSetSlot_EditValueChanged
    private void txtSetSlot_EditValueChanged(object sender, EventArgs e)
    {
      TryExecute(() =>
                   {
                     int nr;
                     int.TryParse(this.txtSetSlot.Text, out nr);
                     if (this.currentChannelList != null)
                       this.currentChannelList.InsertProgramNumber = nr;
                   });
    }
    #endregion

    #region txtSetSlot_ButtonClick, txtSetSlot_KeyDown
    private void txtSetSlot_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      TryExecute(() => this.SetSlotNumber(this.txtSetSlot.Text));
    }

    private void txtSetSlot_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Enter)
      {
        TryExecute(() => this.SetSlotNumber(this.txtSetSlot.Text));
        e.Handled = true;
      }
    }
    #endregion

    #region btnSort_Click
    private void btnSort_Click(object sender, EventArgs e)
    {
      TryExecute(this.SortSelectedChannels);
    }
    #endregion

    #region btnRenum_Click
    private void btnRenum_Click(object sender, EventArgs e)
    {
      TryExecute(this.RenumberSelectedChannels);
    }
    #endregion

    #region ProcessCmdKey

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      switch (keyData)
      {
        case Keys.F3:
          this.gviewOutput.FocusedRowHandle = GridControl.AutoFilterRowHandle;
          this.gviewOutput.FocusedColumn = this.colOutName;
          this.gridOutput.Focus();
          return true;
        case Keys.F4:
          if (this.gviewOutput.SelectedRowsCount > 0)
          {
            this.gviewOutput.FocusedRowHandle = this.gviewOutput.GetSelectedRows()[0];
            this.gridOutput.Focus();
          }
          return true;
        case Keys.F5:
          this.gviewInput.FocusedRowHandle = GridControl.AutoFilterRowHandle;
          this.gviewInput.FocusedColumn = this.colName;
          this.gridInput.Focus();
          return true;
        case Keys.F6:
          if (this.gviewInput.SelectedRowsCount > 0)
          {
            this.gviewInput.FocusedRowHandle = this.gviewInput.GetSelectedRows()[0];
            this.gridInput.Focus();
          }
          return true;
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }

    #endregion

    #region MainForm_FormClosing
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (this.PromptSaveAndContinue())
        this.SaveSettings();
      else
        e.Cancel = true;
    }
    #endregion

    #region btnResetChannelData_Click
    private void btnResetChannelData_Click(object sender, EventArgs e)
    {
      if (this.currentTvSerializer == null)
        return;

      if (XtraMessageBox.Show(this,
                              Resources.MainForm_btnResetChannelData_Click_Message,
                              Resources.MainForm_btnResetChannelData_Click_Caption, 
                              MessageBoxButtons.YesNo, 
                              MessageBoxIcon.Warning,
                              MessageBoxDefaultButton.Button2) != DialogResult.Yes)
      {
        return;
      }

      TryExecute(() =>
                   {
                     this.currentTvSerializer.EraseChannelData();
                     this.LoadFiles(this.currentPlugin);
                   });
    }
    #endregion

    #region btnCloneChannelList_Click
    private void btnCloneChannelList_Click(object sender, EventArgs e)
    {
      this.TryExecute(this.CloneChannelList);
    }
    #endregion

    private void gviewOutput_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
    {
      this.UpdateMenu();
    }

    private void gviewInput_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {

    }

  }
}
