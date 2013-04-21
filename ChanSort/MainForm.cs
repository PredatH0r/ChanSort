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
using DevExpress.XtraTab;

namespace ChanSort.Ui
{
  public partial class MainForm : XtraForm
  {
    public const string AppVersion = "v2013-04-21";

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
    private DataRoot dataRoot;
    private bool ignoreLanguageChange;
    private readonly string title;
    private Encoding defaultEncoding = Encoding.Default;
    private readonly List<string> isoEncodings = new List<string>();
    private ChannelList currentChannelList;
    private GridView lastFocusedGrid;
    private EditMode curEditMode = EditMode.InsertAfter;
    private bool dontOpenEditor;

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
      this.title = string.Format(this.Text, AppVersion);
      this.Text = title;
      this.plugins = this.LoadSerializerPlugins();
      this.FillMenuWithIsoEncodings();

      this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("ChanSort.Ui.app.ico"));
      var bcLeft = new BindingContext();
      this.grpOutputList.BindingContext = bcLeft;
      this.lastFocusedGrid = this.gviewRight;
      if (this.curEditMode == EditMode.InsertAfter) this.rbInsertAfter.Checked = true;
      else if (this.curEditMode == EditMode.InsertBefore) this.rbInsertBefore.Checked = true;
      else this.rbInsertSwap.Checked = true;
      this.cbAppendUnsortedChannels.Checked = true;
      this.ActiveControl = this.gridRight;
    }
    #endregion

    #region InitAppAfterMainWindowWasShown()
    private void InitAppAfterMainWindowWasShown()
    {
      this.BeginInvoke((Action)UpdateCheck.CheckForNewVersion);
    }
    #endregion

    #region SetControlsEnabled()
    private void SetControlsEnabled(bool enabled)
    {
      foreach (Control control in this.grpTopPanel.Controls)
        control.Enabled = enabled;
      foreach (Control control in this.pnlEditControls.Controls)
      {
        if (control != this.btnClearLeftFilter)
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

    #region ReLoadFiles()
    private void ReLoadFiles(ISerializerPlugin plugin)
    {
      int listIndex = this.tabChannelList.SelectedTabPageIndex;
      this.LoadFiles(plugin);
      this.tabChannelList.SelectedTabPageIndex = listIndex;
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
        this.gviewRight.BeginDataUpdate();
        this.gviewLeft.BeginDataUpdate();

        this.currentPlugin = plugin;
        this.currentTvSerializer = newSerializer;
        this.currentTvSerializer.DefaultEncoding = this.defaultEncoding;
        this.miEraseChannelData.Enabled = newSerializer.Features.EraseChannelData;
        if (!this.LoadTvDataFile())
          return;
        this.LoadCsvFile();

        this.editor = new Editor();
        this.editor.DataRoot = this.dataRoot;

        this.currentChannelList = null;
        this.editor.ChannelList = null;
        this.gridRight.DataSource = null;
        this.gridLeft.DataSource = null;
        this.FillChannelListCombo();

        this.SetControlsEnabled(!this.dataRoot.IsEmpty);
        this.UpdateFavoritesEditor(this.dataRoot.SupportedFavorites);
        this.colName.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.ChannelNameEdit;
        this.colOutName.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.ChannelNameEdit;
      }
      catch (Exception ex)
      {
        if (!(ex is IOException))
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
          this.gviewRight.EndDataUpdate();
          this.gviewLeft.EndDataUpdate();
        }
      }
    }

    private void FillChannelListCombo()
    {
      this.tabChannelList.TabPages.Clear();
      XtraTabPage firstNonEmpty = null;
      foreach (var list in this.dataRoot.ChannelLists)
      {
        var tab = this.tabChannelList.TabPages.Add(list.Caption);
        tab.Tag = list;
        if (firstNonEmpty == null && list.Count > 0)
          firstNonEmpty = tab;
      }

      if (firstNonEmpty == null)
        firstNonEmpty = tabChannelList.TabPages[0];
      if (firstNonEmpty == this.tabChannelList.SelectedTabPage)
        this.ShowChannelList((ChannelList)firstNonEmpty.Tag);
      else
        this.tabChannelList.SelectedTabPage = firstNonEmpty;
    }

    #endregion

    #region UpdateFavoritesEditor()
    private void UpdateFavoritesEditor(Favorites favorites)
    {
      this.miFavSet.Strings.Clear();
      this.miFavUnset.Strings.Clear();
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

          string str = c.ToString();
          this.miFavSet.Strings.Add(str);
          this.miFavUnset.Strings.Add(str);
        }
      }
      regex += "]*";
      this.btnToggleFavE.Enabled = (favorites & Favorites.E) != 0;
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
      string upperFileName = Path.GetFileName(inputFileName).ToUpper();
      foreach (var plugin in this.plugins)
      {
        if ((plugin.FileFilter.ToUpper()+"|").Contains("*"+extension) || plugin.FileFilter.ToUpper() == upperFileName)
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
      {
        var csvSerializer = new CsvFileSerializer(this.currentCsvFile, this.dataRoot);
        csvSerializer.Load();
      }
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
          this.gviewRight.BeginDataUpdate();
          this.gviewLeft.BeginDataUpdate();

          string ext = (Path.GetExtension(dlg.FileName) ?? "").ToLower();
          if (ext == ".csv")
          {
            var csvSerializer = new CsvFileSerializer(dlg.FileName, this.dataRoot);
            csvSerializer.Load();
          }
          else if (ext == ".chl")
          {
            ChlFileSerializer loader = new ChlFileSerializer();
            string warnings = loader.Load(dlg.FileName, this.dataRoot, this.currentChannelList);
            InfoBox.Show(this, warnings, Path.GetFileName(dlg.FileName));
          }

          this.gviewRight.EndDataUpdate();
          this.gviewLeft.EndDataUpdate();
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
        this.gridRight.DataSource = channelList.Channels;
        this.gridLeft.DataSource = channelList.Channels;
      }
      else
      {
        this.gridRight.DataSource = null;
        this.gridLeft.DataSource = null;        
      }
      this.currentChannelList = channelList;
      this.editor.ChannelList = channelList;

      if (gviewRight.IsValidRowHandle(0))
        this.SelectFocusedRow(this.gviewRight, 0);

      if (gviewLeft.IsValidRowHandle(0))
        this.SelectFocusedRow(this.gviewLeft, 0);

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
      this.gviewRight.PostEditor();
      this.gviewLeft.PostEditor();
      try
      {
        this.gviewRight.BeginDataUpdate();
        this.gviewLeft.BeginDataUpdate();
        this.editor.AutoNumberingForUnassignedChannels(
          this.cbAppendUnsortedChannels.Checked ? UnsortedChannelMode.AppendInOrder : UnsortedChannelMode.MarkDeleted);
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
        this.gviewRight.EndDataUpdate();
        this.gviewLeft.EndDataUpdate();
      }
    }

    #endregion

    #region SaveReferenceFile()

    private void SaveReferenceFile()
    {
      var csvSerializer = new CsvFileSerializer(this.currentCsvFile, this.dataRoot);
      csvSerializer.Save();
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
        this.currentTvSerializer.Save(this.currentTvFile);
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
      var selectedChannels = this.GetSelectedChannels(gviewRight);
      if (selectedChannels.Count == 0) return;

      if (this.rbInsertSwap.Checked)
        this.RemoveChannels(this.gviewLeft, this.cbCloseGap.Checked);

      ChannelInfo lastInsertedChannel;
      this.gviewLeft.BeginDataUpdate();
      this.gviewRight.BeginDataUpdate();
      try
      {
        lastInsertedChannel = this.editor.AddChannels(selectedChannels);
        this.UpdateInsertSlotTextBox();
      }
      finally
      {
        this.gviewRight.EndDataUpdate();
        this.gviewLeft.EndDataUpdate();
      }

      if (lastInsertedChannel == null)
      {
        this.NavigateToChannel(selectedChannels[0], this.gviewLeft);
        return;
      }

      int index = this.currentChannelList.Channels.IndexOf(lastInsertedChannel);
      int rowHandle = this.gviewLeft.GetRowHandle(index);
      if (this.rbInsertBefore.Checked)
        ++rowHandle;
      this.SelectFocusedRow(this.gviewLeft, rowHandle);
    }
    #endregion

    #region RemoveChannels()

    private void RemoveChannels(GridView grid, bool closeGap)
    {
      var selectedChannels = this.GetSelectedChannels(grid);
      if (selectedChannels.Count == 0) return;

      int focusedRow = Math.Max(0, this.gviewLeft.FocusedRowHandle - selectedChannels.Count);
      if (!gviewLeft.IsLastRow)
        ++focusedRow;
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      try
      {
        this.editor.RemoveChannels(selectedChannels, closeGap);
      }
      finally
      {
        this.gviewLeft.EndDataUpdate();
        this.gviewRight.EndDataUpdate();
      }
      this.SelectFocusedRow(this.gviewLeft, focusedRow);
      this.UpdateInsertSlotTextBox();
    }

    #endregion

    #region SelectFocusedRow()
    private void SelectFocusedRow(GridView grid, int rowHandle)
    {
      grid.BeginSelection();
      grid.ClearSelection();
      grid.FocusedRowHandle = rowHandle;
      grid.SelectRow(rowHandle);
      grid.EndSelection();      
    }
    #endregion

    #region MoveChannels()

    private void MoveChannels(bool up)
    {
      var selectedChannels = this.GetSelectedChannels(this.gviewLeft);
      if (selectedChannels.Count == 0) return;

      this.gviewLeft.BeginDataUpdate();
      try
      {
        this.editor.MoveChannels(selectedChannels, up);
      }
      finally
      {
        this.gviewLeft.EndDataUpdate();
      }
      this.UpdateInsertSlotTextBox();
    }

    #endregion

    #region SetSlotNumber()
    private bool SetSlotNumber(string progNr)
    {
      int prog;
      if (!int.TryParse(progNr, out prog) || prog < 0)
        return false;
      var selectedChannels = this.GetSelectedChannels(this.lastFocusedGrid);
      if (selectedChannels.Count == 0) return true;
      this.gviewLeft.BeginDataUpdate();
      this.gviewRight.BeginDataUpdate();
      try
      {
        this.editor.SetSlotNumber(selectedChannels, prog, this.rbInsertSwap.Checked, this.cbCloseGap.Checked);
      }
      finally
      {
        this.gviewRight.EndDataUpdate();
        this.gviewLeft.EndDataUpdate();
      }
      return true;
    }
    #endregion

    #region SortSelectedChannels()
    private void SortSelectedChannels()
    {
      var selectedChannels = this.GetSelectedChannels(this.gviewLeft);
      if (selectedChannels.Count == 0) return;
      this.gviewLeft.BeginDataUpdate();
      this.gviewRight.BeginDataUpdate();
      try
      {
        this.editor.SortSelectedChannels(selectedChannels);
      }
      finally
      {
        this.gviewRight.EndDataUpdate();
        this.gviewLeft.EndDataUpdate();
      }
    }
    #endregion

    #region AddAllUnsortedChannels()
    private void AddAllUnsortedChannels()
    {
      if (this.currentChannelList == null) return;
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      int maxNr = this.currentChannelList.InsertProgramNumber;
      foreach (var channel in this.currentChannelList.Channels)
        maxNr = Math.Max(maxNr, channel.NewProgramNr);
      foreach (var channel in this.currentChannelList.Channels)
      {
        if (channel.NewProgramNr == -1 && !channel.IsDeleted)
          channel.NewProgramNr = maxNr++;
      }
      this.gviewRight.EndDataUpdate();
      this.gviewLeft.EndDataUpdate();
    }
    #endregion

    #region RenumberSelectedChannels()
    private void RenumberSelectedChannels()
    {
      var list = this.GetSelectedChannels(this.gviewLeft);
      if (list.Count == 0) return;
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      this.editor.RenumberChannels(list);
      this.gviewLeft.EndDataUpdate();
      this.gviewRight.EndDataUpdate();
    }
    #endregion

    #region GetSelectedChannels()
    private List<ChannelInfo> GetSelectedChannels(GridView gview)
    {
      var channels = new List<ChannelInfo>();
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

      this.SetGridLayout(this.gviewLeft, Settings.Default.OutputListLayout);

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
        OptionsLayoutGrid options = new OptionsLayoutGrid();
        options.StoreDataSettings = true;
        options.StoreAppearance = false;
        options.StoreVisualOptions = false;
        grid.RestoreLayoutFromStream(stream, options);
      }

      // put the filter text back into the auto-filter-row
      foreach (GridColumn col in grid.Columns)
      {
        string[] parts = (col.FilterInfo.FilterString ?? "").Split('\'');
        if (parts.Length >= 2)
          this.gviewRight.SetRowCellValue(GridControl.AutoFilterRowHandle, col, parts[1]);
      }
    }
    #endregion

    #region SaveSettings(), GetGridLayout()
    private void SaveSettings()
    {
      this.gviewRight.PostEditor();
      this.gviewLeft.PostEditor();

      Settings.Default.WindowSize = this.WindowState == FormWindowState.Normal ? this.Size : this.RestoreBounds.Size;
      Settings.Default.Encoding = this.defaultEncoding.WebName;
      Settings.Default.Language = Thread.CurrentThread.CurrentUICulture.Name;
      Settings.Default.InputTLL = this.currentTvFile;
      Settings.Default.LeftPanelWidth = this.splitContainerControl1.SplitterPosition;
      Settings.Default.OutputListLayout = GetGridLayout(this.gviewLeft);
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
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      this.currentTvSerializer.DefaultEncoding = encoding;
      this.gviewRight.EndDataUpdate();
      this.gviewLeft.EndDataUpdate();
    }
    #endregion

    #region ClearLeftFilter(), ClearRightFilter()
    private void ClearLeftFilter()
    {
      this.gviewLeft.BeginSort();
      this.gviewLeft.ClearColumnsFilter();
      this.colOutSlot.FilterInfo = new ColumnFilterInfo("[NewProgramNr]<>-1");
      this.gviewLeft.EndSort();
    }

    private void ClearRightFilter()
    {
      this.gviewRight.BeginSort();
      this.gviewRight.ClearColumnsFilter();
      this.colSlotOld.FilterInfo = new ColumnFilterInfo("[OldProgramNr]<>-1");
      this.gviewRight.EndSort();
    }
    #endregion

    #region LoadInputGridLayout()
    private void LoadInputGridLayout(SignalSource newSource)
    {
      string newLayout;
      if ((newSource & SignalSource.Analog) != 0)
        newLayout = Settings.Default.InputGridLayoutAnalog;
      else if ((newSource & SignalSource.DvbS) != 0)
        newLayout = Settings.Default.InputGridLayoutDvbS; 
      else
        newLayout = Settings.Default.InputGridLayoutDvbCT;
      if (!string.IsNullOrEmpty(newLayout))
        this.SetGridLayout(this.gviewRight, newLayout);
      
      foreach (GridColumn col in this.gviewRight.Columns)
        col.Visible = GetGridColumnVisibility(col, newSource);

      this.ClearRightFilter();
    }
    #endregion

    #region SaveInputGridLayout()
    private void SaveInputGridLayout(SignalSource signalSource)
    {
      string currentLayout = GetGridLayout(this.gviewRight);
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
      if (col == this.colSignalSource) return col.Visible;
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

      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      Favorites mask = (Favorites)(1 << (ch - 'A'));
      foreach(var channel in list)
      {
        if (set)
          channel.Favorites |= mask;
        else
          channel.Favorites &= ~mask;
      }
      this.gviewRight.EndDataUpdate();
      this.gviewLeft.EndDataUpdate();
    }
    #endregion

    #region SetChannelFlag()
    private void SetChannelFlag(Action<ChannelInfo> setFlag)
    {
      var list = this.GetSelectedChannels(this.lastFocusedGrid);
      if (list.Count == 0) return;

      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      foreach (var channel in list)
        setFlag(channel);
      this.gviewRight.EndDataUpdate();
      this.gviewLeft.EndDataUpdate();
      this.dataRoot.NeedsSaving = true;
    }
    #endregion

    #region NavigateToChannel
    private void NavigateToChannel(ChannelInfo channel, GridView view)
    {
      if (channel == null) return;
      int rowHandle = view.GetRowHandle(this.currentChannelList.Channels.IndexOf(channel));
      if (view.IsValidRowHandle(rowHandle))
      {
        this.SelectFocusedRow(view, rowHandle);
        view.MakeRowVisible(rowHandle);
      }
    }
    #endregion

    #region SetActiveGrid()
    private void SetActiveGrid(GridView grid)
    {
      if (grid == this.gviewLeft)
      {
        this.grpOutputList.AppearanceCaption.ForeColor = Color.DodgerBlue;
        this.grpOutputList.AppearanceCaption.Options.UseForeColor = true;
        this.grpInputList.AppearanceCaption.Options.UseForeColor = false;
        this.lastFocusedGrid = this.gviewLeft;
      }
      else
      {
        this.grpInputList.AppearanceCaption.ForeColor = Color.DodgerBlue;
        this.grpInputList.AppearanceCaption.Options.UseForeColor = true;
        this.grpOutputList.AppearanceCaption.Options.UseForeColor = false;
        this.lastFocusedGrid = this.gviewRight;
      }
      this.UpdateMenu();
    }
    #endregion

    #region UpdateMenu
    private void UpdateMenu()
    {
      bool isRight = this.lastFocusedGrid == this.gviewRight;
      this.miAddChannel.Enabled = isRight;

      var visRight = isRight ? BarItemVisibility.Always : BarItemVisibility.Never;
      var visLeft = isRight ? BarItemVisibility.Never : BarItemVisibility.Always;

      this.miSort.Visibility = visLeft;
      this.miRenum.Visibility = visLeft;
      this.miMoveUp.Visibility = visLeft;
      this.miMoveDown.Visibility = visLeft;
      this.miAddChannel.Visibility = visRight;

      var sel = this.gviewLeft.GetSelectedRows();
      var channel = sel.Length == 0 ? null : (ChannelInfo) this.gviewRight.GetRow(sel[0]);
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

      if (this.dataRoot.Warnings.Length > 0)
      {
        var lines = this.dataRoot.Warnings.ToString().Split('\n');
        Array.Sort(lines);
        var sortedWarnings = String.Join("\n", lines);
        info += Resources.MainForm_LoadFiles_ValidationWarningMsg + "\r\n\r\n" + sortedWarnings;
      }

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

    #region RefreshGrid()
    private void RefreshGrid(params GridView[] grids)
    {
      foreach (var grid in grids)
      {
        grid.BeginDataUpdate();
        grid.EndDataUpdate();
      }
    }
    #endregion

    #region ShowTvCountrySettings()
    private void ShowTvCountrySettings()
    {
      if (this.currentTvSerializer != null)
        this.currentTvSerializer.ShowDeviceSettingsForm(this);
    }
    #endregion

    #region ToggleFavorite()
    private void ToggleFavorite(string fav)
    {
      var list = this.GetSelectedChannels(this.gviewLeft);
      if (list.Count == 0) return;
      var value = (Favorites)Enum.Parse(typeof (Favorites), fav);
      this.SetFavorite(fav, (list[0].Favorites&value) == 0);
      this.RefreshGrid(gviewLeft, gviewRight);
    }
    #endregion

    #region ToggleLock()
    private void ToggleLock()
    {
      var list = this.GetSelectedChannels(this.gviewLeft);
      if (list.Count == 0) return;
      bool setLock = !list[0].Lock;
      foreach (var channel in list)
        channel.Lock = setLock;
      this.RefreshGrid(gviewLeft, gviewRight);
    }
    #endregion

    #region RenameChannel()
    private void RenameChannel()
    {
      if (this.lastFocusedGrid == null) return;

      if (this.lastFocusedGrid == this.gviewLeft)
        this.gviewLeft.FocusedColumn = this.colOutName;
      else
        this.gviewRight.FocusedColumn = this.colName;
      this.dontOpenEditor = false;
      this.lastFocusedGrid.ShowEditor();
    }
    #endregion

    // UI events

    #region MainForm_Load
    private void MainForm_Load(object sender, EventArgs e)
    {
      this.TryExecute(this.LoadSettings);
      this.TryExecute(this.InitAppAfterMainWindowWasShown);
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
      TryExecute(() => this.ReLoadFiles(this.currentPlugin));
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
      this.TryExecute(() => this.RemoveChannels(this.lastFocusedGrid, this.cbCloseGap.Checked));
    }

    private void miRenameChannel_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.RenameChannel);
    }

    private void miSort_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.SortSelectedChannels);
    }

    private void miRenum_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.RenumberSelectedChannels);
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
    
    private void miEraseChannelData_ItemClick(object sender, ItemClickEventArgs e)
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

    #region Help menu

    private void miWiki_ItemClick(object sender, ItemClickEventArgs e)
    {
      BrowserHelper.OpenUrl("http://sourceforge.net/p/chansort/wiki/Home/");
    }

    private void miOpenWebsite_ItemClick(object sender, ItemClickEventArgs e)
    {
      BrowserHelper.OpenUrl("http://sourceforge.net/p/chansort/");
    }

    private void miAbout_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(() => new AboutForm(this.plugins).ShowDialog());
    }
    #endregion

    // -- controls

    #region picDonate_Click
    private void picDonate_Click(object sender, EventArgs e)
    {
      BrowserHelper.OpenHtml(Resources.paypal_button);
    }
    #endregion

    #region tabChannelList_SelectedPageChanged
    private void tabChannelList_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
    {
      this.TryExecute(() => ShowChannelList(e.Page == null ? null : (ChannelList)e.Page.Tag));
    }
    #endregion

    #region gview_MouseDown, gview_MouseUp, timerEditDelay_Tick, gview_ShowingEditor

    // these 4 event handler in combination override the default row-selection and editor-opening 
    // behavior of the grid control.

    private void gview_MouseDown(object sender, MouseEventArgs e)
    {
      GridView view = (GridView)sender;
      var hit = view.CalcHitInfo(e.Location);
      if (!view.IsDataRow(hit.RowHandle))
        return;
      if (e.Button == MouseButtons.Left)
      {
        if (ModifierKeys == Keys.None)
        {
          if (hit.RowHandle != view.FocusedRowHandle)
            SelectFocusedRow(view, hit.RowHandle);
          this.timerEditDelay.Start();
        }
        else
        {
          if (ModifierKeys == Keys.Control && !view.IsRowSelected(hit.RowHandle))
            this.BeginInvoke((Action) (() => view.SelectRow(hit.RowHandle)));
        }
      }    
      else if (e.Button == MouseButtons.Right)
      {
        if (!view.IsRowSelected(hit.RowHandle))
          SelectFocusedRow(view, hit.RowHandle);
      }

      this.dontOpenEditor = true;
    }

    private void gview_MouseUp(object sender, MouseEventArgs e)
    {
      this.timerEditDelay.Stop();
    }

    private void timerEditDelay_Tick(object sender, EventArgs e)
    {
      this.timerEditDelay.Stop();
      this.dontOpenEditor = false;
      if (this.lastFocusedGrid != null)
        this.lastFocusedGrid.ShowEditor();
    }

    private void gview_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
    {
      var field = ((GridView) sender).FocusedColumn.FieldName;
      if (this.dontOpenEditor && (field == this.colSlotNew.FieldName || field == this.colName.FieldName))
        e.Cancel = true;
    }
    #endregion

    #region gviewLeft_LayoutUpgrade, gviewRight_LayoutUpgrade
    private void gviewLeft_LayoutUpgrade(object sender, LayoutUpgadeEventArgs e)
    {
      this.gviewLeft.ClearGrouping();
      this.gviewLeft.OptionsCustomization.AllowGroup = false;
    }

    private void gviewRight_LayoutUpgrade(object sender, LayoutUpgadeEventArgs e)
    {
      this.gviewRight.ClearGrouping();
      this.gviewRight.OptionsCustomization.AllowGroup = false;
    }
    #endregion

    #region gviewRight_FocusedRowChanged
    private void gviewRight_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
      this.gviewRight.SelectRow(e.FocusedRowHandle);
    }
    #endregion

    #region gviewRight_CustomColumnDisplayText
    private void gviewRight_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column == this.colSlotNew)
      {
        if (!(e.Value is int)) return;
        if ((int) e.Value == -1)
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

    #region gviewRight_RowCellStyle
    private void gviewRight_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      ChannelInfo channel = (ChannelInfo)this.gviewRight.GetRow(e.RowHandle);
      if (channel == null) return;
      if (channel.OldProgramNr == -1)
      {
        e.Appearance.ForeColor = Color.Red;
        e.Appearance.Options.UseForeColor = true;
      }
      else if (channel.NewProgramNr != -1)
      {
        e.Appearance.ForeColor = Color.Gray;
        e.Appearance.Options.UseForeColor = true;
      }
    }
    #endregion

    #region gviewRight_RowClick
    private void gviewRight_RowClick(object sender, RowClickEventArgs e)
    {
      if (e.Clicks == 2 && e.Button == MouseButtons.Left && this.gviewRight.IsDataRow(e.RowHandle))
        TryExecute(this.AddChannels);
    }
    #endregion

    #region gridRight_ProcessGridKey
    private void gridRight_ProcessGridKey(object sender, KeyEventArgs e)
    {
      if (this.gviewRight.ActiveEditor != null)
        return;
      if (e.KeyCode == Keys.Enter)
      {
        TryExecute(this.AddChannels);
        e.Handled = true;
      }
    }

    #endregion

    #region gviewRight_ValidatingEditor
    private void gviewRight_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
    {
      try
      {
        if (gviewRight.FocusedRowHandle == GridControl.AutoFilterRowHandle)
          return;
        if (this.gviewRight.FocusedColumn == this.colSlotNew && e.Value is string)
          e.Valid = this.SetSlotNumber((string)e.Value);
        else if (this.gviewRight.FocusedColumn == this.colFavorites && e.Value is string)
          e.Value = ChannelInfo.ParseFavString((string)e.Value);
        else if (gviewRight.FocusedColumn == this.colName)
        {
          var ci = this.gviewRight.GetFocusedRow() as ChannelInfo;
          this.VerifyChannelNameModified(ci, e.Value as string);
          //this.BeginInvoke((Action) (() => RefreshGrid(this.gviewLeft)));
        }
        dataRoot.NeedsSaving = true;
      } catch(Exception ex) { HandleException(ex); }
    }
    #endregion

    #region gviewRight_CellValueChanged
    private void gviewRight_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {
      TryExecute(() => RefreshGrid(this.gviewLeft));
    }
    #endregion

    #region gviewRight_PopupMenuShowing
    private void gviewRight_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      this.lastFocusedGrid = this.gviewRight;
      this.UpdateMenu();
      if (e.MenuType == GridMenuType.Row)
        this.mnuContext.ShowPopup(this.gridRight.PointToScreen(e.Point));
    }
    #endregion


    #region gviewLeft_FocusedRowChanged

    private void gviewLeft_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
      var channel = (ChannelInfo)this.gviewLeft.GetRow(e.FocusedRowHandle);
      if (channel == null)
        return;
      int programNr = channel.NewProgramNr;
      if (this.rbInsertAfter.Checked)
        ++programNr;
      if (this.currentChannelList != null)
        this.currentChannelList.InsertProgramNumber = programNr;
      this.UpdateInsertSlotTextBox();
      this.gviewLeft.SelectRow(e.FocusedRowHandle);
    }

    #endregion

    #region gviewLeft_SelectionChanged
    private void gviewLeft_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
    {
      this.UpdateMenu();
    }
    #endregion

    #region gviewLeft_CustomColumnDisplayText
    private void gviewLeft_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column == this.colOutFav)
      {
        if (!(e.Value is Favorites)) return;
        if ((Favorites)e.Value == 0)
          e.DisplayText = string.Empty;
      }
    }
    #endregion

    #region gviewLeft_RowCellStyle
    private void gviewLeft_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      var channel = (ChannelInfo)this.gviewLeft.GetRow(e.RowHandle);
      if (channel == null) return;
      if (channel.OldProgramNr == -1)
      {
        e.Appearance.ForeColor = Color.Red;
        e.Appearance.Options.UseForeColor = true;
      }
    }
    #endregion

    #region gviewLeft_ValidatingEditor
    private void gviewLeft_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
    {
      try
      {
        if (gviewLeft.FocusedRowHandle == GridControl.AutoFilterRowHandle)
          return;
        if (this.gviewLeft.FocusedColumn == this.colOutSlot && e.Value is string)
          e.Valid = this.SetSlotNumber((string)e.Value);
        else if (this.gviewLeft.FocusedColumn == this.colOutFav && e.Value is string)
          e.Value = ChannelInfo.ParseFavString((string) e.Value);
        else if (gviewLeft.FocusedColumn == this.colOutName)
        {
          this.VerifyChannelNameModified(this.gviewLeft.GetFocusedRow() as ChannelInfo, e.Value as string);
          this.BeginInvoke((Action) (() => RefreshGrid(this.gviewLeft)));
        }
        dataRoot.NeedsSaving = true;
      }
      catch (Exception ex) { HandleException(ex); }
    }
    #endregion

    #region gviewLeft_CellValueChanged
    private void gviewLeft_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {
      this.gviewRight.BeginDataUpdate();
      this.gviewRight.EndDataUpdate();
    }
    #endregion

    #region gviewLeft_PopupMenuShowing
    private void gviewLeft_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      this.lastFocusedGrid = this.gviewLeft;
      this.UpdateMenu();
      if (e.MenuType == GridMenuType.Row && e.HitInfo.InRow && this.gviewLeft.IsDataRow(e.HitInfo.RowHandle))
        this.mnuContext.ShowPopup(this.gridLeft.PointToScreen(e.Point));
    }
    #endregion

    #region gviewLeft_RowClick
    private void gviewLeft_RowClick(object sender, RowClickEventArgs e)
    {
      if (e.Clicks == 2 && e.Button == MouseButtons.Left && this.gviewLeft.IsDataRow(e.RowHandle))
      {
        ChannelInfo channel = (ChannelInfo) this.gviewLeft.GetRow(e.RowHandle);
        this.NavigateToChannel(channel, this.gviewRight);
      }
    }
    #endregion

    #region gridLeft_ProcessGridKey
    private void gridLeft_ProcessGridKey(object sender, KeyEventArgs e)
    {
      if (gviewLeft.ActiveEditor != null)
        return;
      if (e.KeyCode == Keys.Delete)
        TryExecute(() => this.RemoveChannels(this.gviewLeft, this.cbCloseGap.Checked));
      else if (e.KeyCode == Keys.Add)
        TryExecute(() => this.MoveChannels(false));
      else if (e.KeyCode == Keys.Subtract)
        TryExecute(() => this.MoveChannels(true));
      else
        return;
      e.Handled = true;
    }
    #endregion

    #region rbInsertMode_CheckedChanged
    private void rbInsertMode_CheckedChanged(object sender, EventArgs e)
    {
      if (!((CheckEdit)sender).Checked)
        return;
      try
      {
        if (this.currentChannelList == null)
          return;
        int delta = this.curEditMode == EditMode.InsertAfter ? -1 :
          this.rbInsertAfter.Checked ? +1 : 0;
        this.currentChannelList.InsertProgramNumber += delta;
        this.UpdateInsertSlotTextBox();
        this.curEditMode = this.rbInsertBefore.Checked ? EditMode.InsertBefore 
          : this.rbInsertAfter.Checked ? EditMode.InsertAfter 
          : EditMode.Swap;
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

    #region btnAdd_Click

    private void btnAdd_Click(object sender, EventArgs e)
    {
      TryExecute(this.AddChannels);
    }
    #endregion

    #region btnRemoveLeft_Click, btnRemoveRight_Click

    private void btnRemoveLeft_Click(object sender, EventArgs e)
    {
      TryExecute(() => this.RemoveChannels(this.gviewLeft, this.cbCloseGap.Checked));
    }

    private void btnRemoveRight_Click(object sender, EventArgs e)
    {
      this.TryExecute(() => this.RemoveChannels(this.gviewRight, this.cbCloseGap.Checked));
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
          this.gviewLeft.FocusedRowHandle = GridControl.AutoFilterRowHandle;
          this.gviewLeft.FocusedColumn = this.colOutName;
          this.gridLeft.Focus();
          return true;
        case Keys.F4:
          if (this.gviewLeft.SelectedRowsCount > 0)
          {
            this.gviewLeft.FocusedRowHandle = this.gviewLeft.GetSelectedRows()[0];
            this.gridLeft.Focus();
          }
          return true;
        case Keys.F5:
          this.gviewRight.FocusedRowHandle = GridControl.AutoFilterRowHandle;
          this.gviewRight.FocusedColumn = this.colName;
          this.gridRight.Focus();
          return true;
        case Keys.F6:
          if (this.gviewRight.SelectedRowsCount > 0)
          {
            this.gviewRight.FocusedRowHandle = this.gviewRight.GetSelectedRows()[0];
            this.gridRight.Focus();
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

    #region btnAddAll_Click
    private void btnAddAll_Click(object sender, EventArgs e)
    {
      this.TryExecute(this.AddAllUnsortedChannels);
    }
    #endregion    

    #region btnToggleFav_Click, btnToggleLock_Click
    private void btnToggleFav_Click(object sender, EventArgs e)
    {
      string fav = ((Control) sender).Text.Substring(1);
      this.TryExecute(() => this.ToggleFavorite(fav));
    }

    private void btnToggleLock_Click(object sender, EventArgs e)
    {
      this.TryExecute(this.ToggleLock);
    }
    #endregion

    #region grpOutputList_Enter, grpInputList_Enter
    private void grpOutputList_Enter(object sender, EventArgs e)
    {
      this.SetActiveGrid(this.gviewLeft);
    }

    private void grpInputList_Enter(object sender, EventArgs e)
    {
      this.SetActiveGrid(this.gviewRight);
    }
    #endregion

  }
}
