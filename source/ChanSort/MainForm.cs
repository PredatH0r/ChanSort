//#define ADD_CHANNELS_FROM_REF_LIST

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ChanSort.Api;
using ChanSort.Ui.Printing;
using ChanSort.Ui.Properties;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraTab;
using Timer = System.Windows.Forms.Timer;

namespace ChanSort.Ui
{
  public partial class MainForm : XtraForm
  {
    public static string AppVersion { get; private set; }

    private const int MaxMruEntries = 10;
    private readonly List<string> isoEncodings = new List<string>();
    private readonly List<string> mruFiles = new List<string>();

    private readonly string title;
    private EditMode curEditMode = EditMode.InsertAfter;
    private ISerializerPlugin currentPlugin;
    private string currentRefFile;
    private string currentTvFile;
    private SerializerBase currentTvSerializer;
    private Encoding defaultEncoding = Encoding.Default;
    private bool dontOpenEditor;
    private GridHitInfo downHit;
    private DragDropInfo dragDropInfo;
    private bool ignoreLanguageChange;
    private GridView lastFocusedGrid;
    private int subListIndex;
    private SizeF absScaleFactor = new SizeF(1,1);

    #region ctor()

    public MainForm()
    {
      if (!string.IsNullOrEmpty(Config.Default.Language))
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Config.Default.Language);
      this.LookAndFeel.SetSkinStyle("Office 2010 Blue");
      InitializeComponent();

      var version = this.GetType().Assembly.GetName().Version;
      AppVersion = new DateTime(2000, 1, 1).AddDays(version.Build).ToString("yyyy-MM-dd");

      // remember which columns should be visible by default
      foreach (GridColumn col in this.gviewLeft.Columns)
        col.Tag = col.Visible;
      foreach (GridColumn col in this.gviewRight.Columns)
        col.Tag = col.Visible;

      this.colOutSource.Caption = this.colSource.Caption; // copy translated caption

      if (!Config.Default.WindowSize.IsEmpty)
        this.ClientSize = Config.Default.WindowSize.Scale(absScaleFactor);
      this.title = string.Format(base.Text, AppVersion);
      base.Text = title;
      this.Plugins = this.LoadSerializerPlugins();
      this.FillMenuWithIsoEncodings();

      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ChanSort.Ui.app.ico"))
      {
        if (stream != null)
          this.Icon = new Icon(stream);
      }
      var bcLeft = new BindingContext();
      this.grpOutputList.BindingContext = bcLeft;
      this.lastFocusedGrid = this.gviewRight;
      if (this.curEditMode == EditMode.InsertAfter) this.rbInsertAfter.Checked = true;
      else if (this.curEditMode == EditMode.InsertBefore) this.rbInsertBefore.Checked = true;
      else this.rbInsertSwap.Checked = true;
      this.ActiveControl = this.gridRight;


#if !ADD_CHANNELS_FROM_REF_LIST
      this.miAddFromRefList.Visibility = BarItemVisibility.Never;
      this.miAddFromRefList.Enabled = false;
#endif

      // The Api.View.Default object gives loaders access to UI functions
      Api.View.Default = new Api.View();
      Api.View.Default.CreateActionBox = msg => new ActionBoxDialog(msg);
      Api.View.Default.MessageBoxImpl = (msg, caption, buttons, icon) => (int)XtraMessageBox.Show(this, msg, caption, (MessageBoxButtons) buttons, (MessageBoxIcon) icon);

      var defaultColumns = new List<string>();
      foreach (GridColumn col in this.gviewRight.Columns.OrderBy(c => c.VisibleIndex))
      {
        if (col.Visible)
          defaultColumns.Add(col.FieldName);
      }

      ChannelList.DefaultVisibleColumns = defaultColumns;
    }
    #endregion

    internal IList<ISerializerPlugin> Plugins { get; }

    internal DataRoot DataRoot { get; private set; }

    internal Editor Editor { get; private set; }

    internal ChannelList CurrentChannelList { get; private set; }

    #region IsLeftGridSortedByNewProgNr

    private bool IsLeftGridSortedByNewProgNr
    {
      get
      {
        return this.gviewLeft.SortedColumns.Count >= 1 &&
               this.gviewLeft.SortedColumns[0].FieldName == this.colOutSlot.FieldName;
      }
    }

    #endregion

    #region InitAppAfterMainWindowWasShown()

    private void InitAppAfterMainWindowWasShown()
    {
      if (!DepencencyChecker.IsVc2010RedistPackageX86Installed())
      {
        if (XtraMessageBox.Show(this,
              "Some channel list file formats can only be read when the\n" +
              "Microsoft Visual C++ 2010 Redistributable Package (x86) is installed.\n" +
              "\nDo you want to open the download page and quit ChanSort?",
              "ChanSort",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button1) == DialogResult.Yes)
        {
          System.Diagnostics.Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=5555");
          Application.Exit();
          return;
        }
      }

      if (Config.Default.CheckForUpdates)
        this.BeginInvoke((Action) UpdateCheck.CheckForNewVersion);
    }

    #endregion

    #region LoadSerializerPlugins()

    private IList<ISerializerPlugin> LoadSerializerPlugins()
    {
      var list = new List<ISerializerPlugin>();
      list.Add(new RefSerializerPlugin());
      var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
      foreach (var file in Directory.GetFiles(exeDir, "ChanSort.Loader.*.dll"))
      {
        try
        {
          var assembly = Assembly.UnsafeLoadFrom(file);
          foreach (var type in assembly.GetTypes())
          {
            if (typeof(ISerializerPlugin).IsAssignableFrom(type) && !type.IsAbstract)
            {
              var plugin = (ISerializerPlugin) Activator.CreateInstance(type);
              plugin.DllName = Path.GetFileName(file);
              list.Add(plugin);
            }
          }
        }
        catch (Exception ex)
        {
          HandleException(new IOException("Plugin " + file + "\n" + ex.Message, ex));
        }
      }
      list.Sort((a, b) => a.PluginName.CompareTo(b.PluginName));
      return list;
    }

    #endregion

    #region ShowOpenFileDialog()

    private void ShowOpenFileDialog()
    {
      string supportedExtensions;
      int numberOfFilters;
      var filter = GetTvDataFileFilter(out supportedExtensions, out numberOfFilters);

      using (var dlg = new OpenFileDialog())
      {
        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        dlg.AddExtension = true;
        dlg.Filter = filter + string.Format(Resources.MainForm_FileDialog_OpenFileFilter, supportedExtensions);
        dlg.FilterIndex = numberOfFilters + 1;
        dlg.CheckFileExists = true;
        dlg.RestoreDirectory = true;
        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        var plugin = dlg.FilterIndex <= this.Plugins.Count ? this.Plugins[dlg.FilterIndex - 1] : null;
        this.LoadFiles(plugin, dlg.FileName);
      }
    }

    #endregion

    #region GetTvDataFileFilter()

    internal string GetTvDataFileFilter(out string supportedExtensions, out int numberOfFilters)
    {
      numberOfFilters = 0;
      var filter = new StringBuilder();
      var extension = new StringBuilder();
      foreach (var plugin in this.Plugins)
      {
        filter.Append(plugin.PluginName).Append("|").Append(plugin.FileFilter);
        filter.Append("|");
        if (!(";" + extension + ";").Contains(";" + plugin.FileFilter + ";"))
        {
          extension.Append(plugin.FileFilter);
          extension.Append(";");
        }
        ++numberOfFilters;
      }
      if (extension.Length > 0)
        extension.Remove(extension.Length - 1, 1);
      supportedExtensions = extension.ToString();
      return filter.ToString();
    }

    #endregion

    #region SetFileName()

    private void SetFileName(string tvDataFile)
    {
      this.currentTvFile = tvDataFile;
      if (!string.IsNullOrEmpty(tvDataFile))
      {
        this.currentRefFile = Path.Combine(Path.GetDirectoryName(this.currentTvFile) ?? "",
          Path.GetFileNameWithoutExtension(this.currentTvFile) + ".txt");
      }
      this.Text = this.title + "  -  " + this.currentTvFile;
    }

    #endregion

    #region ReLoadFiles()

    private void ReLoadFiles(ISerializerPlugin plugin)
    {
      var listIndex = this.tabChannelList.SelectedTabPageIndex;
      this.LoadFiles(plugin, this.currentTvFile);
      this.tabChannelList.SelectedTabPageIndex = listIndex;
    }

    #endregion

    #region LoadFiles()

    private void LoadFiles(ISerializerPlugin plugin, string tvDataFile)
    {
      var dataUpdated = false;
      try
      {
        if (DetectCommonFileCorruptions(tvDataFile))
          return;

        if (!this.LoadTvDataFile(plugin, tvDataFile))
          return;

        dataUpdated = true;
        this.gviewRight.BeginDataUpdate();
        this.gviewLeft.BeginDataUpdate();

        this.Editor = new Editor();
        this.Editor.DataRoot = this.DataRoot;

        this.CurrentChannelList = null;
        this.Editor.ChannelList = null;
        this.gridRight.DataSource = null;
        this.gridLeft.DataSource = null;
        this.FillChannelListTabs();

        //this.SetControlsEnabled(!this.dataRoot.IsEmpty);
        this.UpdateFavoritesEditor(this.DataRoot.SupportedFavorites);
        this.colEncrypted.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.EncryptedFlagEdit;
        this.UpdateMenu(true);

        if (this.DataRoot.Warnings.Length > 0 && this.miShowWarningsAfterLoad.Checked)
          this.BeginInvoke((Action) this.ShowFileInformation);

        this.BeginInvoke((Action) this.InitInitialChannelOrder);
      }
      catch (Exception ex)
      {
        if (!(ex is IOException))
          throw;
        var name = plugin != null ? plugin.PluginName : "Loader";
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

    #endregion

    #region DetectCommonFileCorruptions()

    internal bool DetectCommonFileCorruptions(string tvDataFile)
    {
      if (!File.Exists(tvDataFile)) // a loader (like Philips) may use internal file names that don't match the one in the UI, i.e. tvDataFile might be a directory path
        return true;

      var content = File.ReadAllBytes(tvDataFile);
      var isAllSame = true;
      var val = content.Length > 0 ? content[0] : 0;
      for (int i = 0, c = content.Length; i < c; i++)
      {
        if (content[i] != val)
        {
          isAllSame = false;
          break;
        }
      }

      if (isAllSame)
      {
        XtraMessageBox.Show(this,
          Resources.MainForm_LoadFiles_AllZero,
          "ChanSort", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return true;
      }
      return false;
    }

    #endregion

    #region FillChannelListTabs()

    private void FillChannelListTabs()
    {
      this.tabChannelList.TabPages.Clear();

      var itemList = new List<BarItem>();
      foreach (BarItemLink link in this.mnuInputSource.ItemLinks)
        itemList.Add(link.Item);
      foreach (var item in itemList)
      {
        this.barManager1.Items.Remove(item);
        item.Dispose();
      }
      this.mnuInputSource.ClearLinks();

      XtraTabPage mostChannels = null;
      int mostChannelsCount = 0;
      var i = 0;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        if (list.Channels.Count == 0)
          continue;
        var tab = this.tabChannelList.TabPages.Add(list.Caption);
        tab.Tag = list;
        if (!list.IsMixedSourceFavoritesList && (mostChannels == null || list.Count > mostChannelsCount))
        {
          mostChannels = tab;
          mostChannelsCount = list.Count;
        }

        var item = new BarButtonItem(this.barManager1, list.Caption);
        item.ItemShortcut = new BarShortcut((Keys) ((int) (Keys.Alt | Keys.D1) + i));
        item.Tag = i;
        item.ItemClick += this.miInputSource_ItemClick;
        this.mnuInputSource.AddItem(item);
        ++i;
      }

      if (tabChannelList.TabPages.Count > 0)
      {
        if (mostChannels == null)
          mostChannels = tabChannelList.TabPages[0];
        if (mostChannels == this.tabChannelList.SelectedTabPage)
          this.ShowChannelList((ChannelList)mostChannels.Tag);
        else
          this.tabChannelList.SelectedTabPage = mostChannels;
      }
      else
      {
        this.tabChannelList.TabPages.Add(this.pageEmpty);
        this.CurrentChannelList = null;
      }
    }

    #endregion

    #region UpdateFavoritesEditor()

    private void UpdateFavoritesEditor(Favorites favorites)
    {
      foreach(var link in this.mnuFavSet.ItemLinks.ToList())
        link.Item?.Dispose();
      foreach (var link in this.mnuFavUnset.ItemLinks.ToList())
        link.Item?.Dispose();

      this.repositoryItemCheckedComboBoxEdit1.Items.Clear();
      this.repositoryItemCheckedComboBoxEdit2.Items.Clear();
      var regex = "[";
      var favCount = 0;
      for (var favMask = (uint)favorites; (favMask & 1) != 0; favMask >>= 1)
      {
        var c = (char) ('A' + favCount);
        ++favCount;
        this.repositoryItemCheckedComboBoxEdit1.Items.Add(c);
        this.repositoryItemCheckedComboBoxEdit2.Items.Add(c);

        var miSet = new BarButtonItem(this.barManager1, "&" + c);
        miSet.Tag = c.ToString();
        miSet.ItemShortcut = new BarShortcut(Keys.Control | (Keys)((int)Keys.D0 + favCount%10));
        miSet.ItemClick += this.miFavSet_ItemClick;
        this.mnuFavSet.AddItem(miSet);

        var miUnset = new BarButtonItem(this.barManager1, "&" + c);
        miUnset.Tag = c.ToString();
        miUnset.ItemShortcut = new BarShortcut(Keys.Control | Keys.Shift | (Keys)((int)Keys.D0 + favCount%10));
        miUnset.ItemClick += this.miFavUnset_ItemClick;
        this.mnuFavUnset.AddItem(miUnset);

        regex += c;
      }
      regex += "]*";
      this.repositoryItemCheckedComboBoxEdit1.Mask.EditMask = regex;
      this.repositoryItemCheckedComboBoxEdit2.Mask.EditMask = regex;

      while (this.tabSubList.TabPages.Count > favCount + 1)
        this.tabSubList.TabPages.RemoveAt(this.tabSubList.TabPages.Count - 1);
      while (this.tabSubList.TabPages.Count < favCount + 1)
        this.tabSubList.TabPages.Add();
      for (int i = 1; i < this.tabSubList.TabPages.Count; i++)
        this.tabSubList.TabPages[i].Text = this.DataRoot.GetFavListCaption(i - 1, true);

      if (!this.DataRoot.SortedFavorites || this.subListIndex >= favCount)
      {
        this.tabSubList.SelectedTabPageIndex = 0;
        this.subListIndex = 0;
      }
      this.colOutFav.OptionsColumn.AllowEdit = !this.DataRoot.SortedFavorites;
      this.colFavorites.OptionsColumn.AllowEdit = !this.DataRoot.SortedFavorites;
    }

    #endregion

    #region GetSerializerForFile()

    internal SerializerBase GetSerializerForFile(string inputFileName, ref ISerializerPlugin hint)
    {
      if (!File.Exists(inputFileName))
      {
        XtraMessageBox.Show(this, string.Format(Resources.MainForm_LoadTll_SourceTllNotFound, inputFileName));
        return null;
      }

      List<ISerializerPlugin> candidates = new List<ISerializerPlugin>();
      if (hint != null)
        candidates.Add(hint);
      else
      {
        var upperFileName = (Path.GetFileName(inputFileName) ?? "").ToUpper();
        foreach (var plugin in this.Plugins)
        {
          foreach (var filter in plugin.FileFilter.ToUpper().Split(';'))
          {
            var regex = filter.Replace(".", "\\.").Replace("*", ".*").Replace("?", ".");
            if (Regex.IsMatch(upperFileName, regex))
            {
              candidates.Add(plugin);
              break;
            }
          }
        }
      }

      var errorMsgs = new StringBuilder();
      foreach (var plugin in candidates)
      {
        SerializerBase serializer = null;
        try
        {
          serializer = plugin.CreateSerializer(inputFileName);
          if (serializer != null)
          {
            serializer.DefaultEncoding = this.defaultEncoding;
            serializer.Load();
            hint = plugin;
            return serializer;
          }
        }
        catch (Exception ex)
        {
          serializer?.Dispose();
          errorMsgs.AppendLine($"{plugin.DllName} ({plugin.PluginName}): {ex}\n\n");
          if (ex is ArgumentException)
          {
            var msg = ex.ToString();
            if (msg.Contains("ZipFile..ctor()"))
            {
              XtraMessageBox.Show(this, string.Format(Resources.MainForm_LoadTll_InvalidZip, inputFileName));
              return null;
            }
          }
        }
      }


      XtraMessageBox.Show(this, string.Format(Resources.MainForm_LoadTll_SerializerNotFound, inputFileName) + "\n\n" + errorMsgs);
      return null;
    }

    #endregion

    #region LoadTvDataFile()

    private bool LoadTvDataFile(ISerializerPlugin plugin, string tvDataFile)
    {
      if (!File.Exists(tvDataFile))
      {
        XtraMessageBox.Show(this, Resources.MainForm_LoadTvDataFile_FileNotFound_Caption,
          string.Format(Resources.MainForm_LoadTvDataFile_FileNotFound_Message, tvDataFile),
          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      // abort action if there is no currentTvSerializer for the input file
      SerializerBase serializer = this.GetSerializerForFile(tvDataFile, ref plugin);
      if (serializer == null)
        return false;

      if (!this.PromptSaveAndContinue())
        return false;

      this.currentTvSerializer?.Dispose();

      serializer.DataRoot.ValidateAfterLoad();
      this.SetFileName(serializer.FileName);
      this.currentPlugin = plugin;
      this.currentTvSerializer = serializer;
      this.DataRoot = serializer.DataRoot;
      this.AddFileToMruList(tvDataFile);
      this.UpdateMruMenu();

      return true;
    }

    #endregion

    #region AddFileToMruList()

    private void AddFileToMruList(string file)
    {
      if (string.IsNullOrEmpty(file))
        return;
      this.mruFiles.Remove(file);
      if (this.mruFiles.Count >= MaxMruEntries)
        this.mruFiles.RemoveAt(this.mruFiles.Count - 1);
      this.mruFiles.Insert(0, file);
    }

    #endregion

    #region PromptSaveAndContinue()

    private bool PromptSaveAndContinue()
    {
      if (this.DataRoot == null || !this.DataRoot.NeedsSaving)
        return true;

      using (var dlg = new ActionBoxDialog(Resources.MainForm_PromptSaveAndContinue_Question))
      {
        dlg.AddAction(Resources.MainForm_PromptSaveAndContinue_Save, DialogResult.Yes, dlg.Save);
        dlg.AddAction(Resources.MainForm_PromptSaveAndContinue_Discard, DialogResult.No, dlg.Discard);
        dlg.AddAction(Resources.MainForm_Cancel, DialogResult.Cancel, dlg.Cancel);
        switch (dlg.ShowDialog(this))
        {
          case DialogResult.Yes:
            this.SaveFiles();
            break;
          case DialogResult.No:
            break;
          case DialogResult.Cancel:
            return false;
        }
      }
      return true;
    }

    #endregion

    #region InitInitialChannelOrder()

    private void InitInitialChannelOrder()
    {
      DialogResult res;
      var msg = Resources.MainForm_InitInitialChannelOrder_Question;
      using (var dlg = new ActionBoxDialog(msg))
      {
        dlg.AddAction(Resources.MainForm_InitInitialChannelOrder_ReferenceList, DialogResult.Yes, dlg.CopyList, true);
        dlg.AddAction(Resources.MainForm_InitInitialChannelOrder_CurrentList, DialogResult.No, dlg.FullList);
        dlg.AddAction(Resources.MainForm_InitInitialChannelOrder_EmptyList, DialogResult.Cancel, dlg.EmptyList);
        res = dlg.ShowDialog(this);
      }

      if (res == DialogResult.Yes)
        this.BeginInvoke((Action) (() => this.ShowOpenReferenceFileDialog(false)));
      else if (res == DialogResult.No)
      {
        this.DataRoot.ApplyCurrentProgramNumbers();
        this.RefreshGrid(this.gviewLeft, this.gviewRight);
      }
    }

    #endregion

    #region ShowOpenReferenceFileDialog()

    private void ShowOpenReferenceFileDialog(bool addChannels)
    {
      new ReferenceListForm(this).ShowDialog(this);
    }

    #endregion

    #region ShowChannelList()

    private void ShowChannelList(ChannelList channelList)
    {
      if (this.CurrentChannelList != null)
        this.SaveInputGridLayout(this.CurrentChannelList.SignalSource);

      this.CurrentChannelList = channelList;
      this.Editor.ChannelList = channelList;

      if (channelList != null)
      {
        this.LoadInputGridLayout();
        this.gridRight.DataSource = channelList.Channels;
        this.gridLeft.DataSource = channelList.Channels;

        SignalSource src = 0;
        if ((this.currentTvSerializer.Features.ChannelNameEdit & ChannelNameEditMode.Analog) != 0)
          src |= SignalSource.Analog;
        if ((this.currentTvSerializer.Features.ChannelNameEdit & ChannelNameEditMode.Digital) != 0)
          src |= SignalSource.Digital;
        this.colName.OptionsColumn.AllowEdit = this.colOutName.OptionsColumn.AllowEdit = (channelList.SignalSource & src) != 0;

        if (this.DataRoot.MixedSourceFavorites)
        {
          if (channelList.IsMixedSourceFavoritesList)
          {
            this.tabSubList.SelectedTabPageIndex = 1;
            this.pageProgNr.PageVisible = false;
            this.grpSubList.Visible = true;
          }
          else
          {
            this.grpSubList.Visible = false;
            this.pageProgNr.PageVisible = true;
            this.tabSubList.SelectedTabPageIndex = 0;
          }
        }
        else
        {
          this.pageProgNr.PageVisible = true;
          this.grpSubList.Visible = DataRoot.SortedFavorites;
        }
      }
      else
      {
        this.gridRight.DataSource = null;
        this.gridLeft.DataSource = null;
        this.grpSubList.Visible = false;
      }

      if (gviewRight.IsValidRowHandle(0))
        this.SelectFocusedRow(this.gviewRight, 0);

      if (gviewLeft.IsValidRowHandle(0))
        this.SelectFocusedRow(this.gviewLeft, 0);

      UpdateGridReadOnly();


      this.UpdateInsertSlotNumber();
      this.UpdateMenu();

      this.mnuFavList.Enabled = this.grpSubList.Visible;
      if (!this.grpSubList.Visible)
        this.tabSubList.SelectedTabPageIndex = 0;
    }

    #endregion

    #region UpdateGridReadOnly

    private void UpdateGridReadOnly()
    {
      var allowEdit = !this.CurrentChannelList?.ReadOnly ?? true;
      var forceEdit = this.miAllowEditPredefinedLists.Down;

      this.gviewLeft.OptionsBehavior.Editable = allowEdit || forceEdit;
      this.gviewRight.OptionsBehavior.Editable = allowEdit || forceEdit;
      this.gviewLeft.Appearance.Row.BackColor = this.gviewRight.Appearance.Row.BackColor = Color.MistyRose;
      this.gviewLeft.Appearance.Empty.BackColor = this.gviewRight.Appearance.Empty.BackColor = Color.MistyRose;
      this.gviewLeft.Appearance.Row.Options.UseBackColor = this.gviewRight.Appearance.Row.Options.UseBackColor = !allowEdit;
      this.gviewLeft.Appearance.Empty.Options.UseBackColor = this.gviewRight.Appearance.Empty.Options.UseBackColor = !allowEdit;
      this.lblPredefinedList.Visible = !(allowEdit || forceEdit);
    }

    #endregion

    #region ShowSaveFileDialog()

    private void ShowSaveFileDialog()
    {
      var extension = Path.GetExtension(this.currentTvFile) ?? ".";
      using (var dlg = new SaveFileDialog())
      {
        dlg.InitialDirectory = Path.GetDirectoryName(this.currentTvFile);
        dlg.FileName = Path.GetFileName(this.currentTvFile);
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
        if (!this.HandleChannelNumberGaps())
          return;
        if (!this.PromptHandlingOfUnsortedChannels())
          return;
        this.SaveTvDataFile();
        this.DataRoot.NeedsSaving = false;
        this.RefreshGrid(this.gviewLeft, this.gviewRight);
        this.UpdateMenu(true);
      }
      catch (IOException ex)
      {
        XtraMessageBox.Show(this,
          Resources.MainForm_SaveFiles_ErrorMsg +
          ex.Message,
          Resources.MainForm_SaveFiles_ErrorTitle,
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region PromptHandlingOfUnsortedChannels()

    private bool PromptHandlingOfUnsortedChannels()
    {
      var hasUnsorted = false;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (var channel in list.Channels)
        {
          if (channel.NewProgramNr < 0 && !channel.IsDeleted)
          {
            hasUnsorted = true;
            break;
          }
        }
      }

      UnsortedChannelMode mode = UnsortedChannelMode.Delete;

      if (hasUnsorted)
      {
        var msg = Resources.MainForm_PromptHandlingOfUnsortedChannels_Question;
        DialogResult res;
        using (var dlg = new ActionBoxDialog(msg))
        {
          dlg.AddAction(Resources.MainForm_PromptHandlingOfUnsortedChannels_Append, DialogResult.Yes, dlg.FullList);
          if (this.currentTvSerializer.Features.DeleteMode != SerializerBase.DeleteMode.NotSupported)
            dlg.AddAction(Resources.MainForm_PromptHandlingOfUnsortedChannels_Delete, DialogResult.No, dlg.Delete);
          dlg.AddAction(Resources.MainForm_Cancel, DialogResult.Cancel, dlg.Cancel);
          res = dlg.ShowDialog(this);
        }

        if (res == DialogResult.Cancel)
          return false;
        if (res == DialogResult.Yes)
          mode = UnsortedChannelMode.AppendInOrder;
      }

      // ensure unsorted and deleted channels have a valid program number
      this.DataRoot.AssignNumbersToUnsortedAndDeletedChannels(mode);
      return true;
    }

    #endregion

    #region HandleChannelNumberGaps()

    private bool HandleChannelNumberGaps()
    {
      if (this.currentTvSerializer.Features.CanHaveGaps)
        return true;

      var hasGaps = this.ProcessChannelNumberGaps(true);
      if (hasGaps)
      {
        var action = XtraMessageBox.Show(this,
          Resources.MainForm_HandleChannelNumberGaps,
          "ChanSort", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (action == DialogResult.Cancel)
          return false;
        if (action == DialogResult.Yes)
          this.ProcessChannelNumberGaps(false);
      }
      return true;
    }

    #endregion

    #region ProcessChannelNumberGaps()

    private bool ProcessChannelNumberGaps(bool testOnly)
    {
      var wasRenumbered = false;
      foreach (var list in this.DataRoot.ChannelLists)
      {
        var chNr = 1;
        foreach (var channel in list.Channels.OrderBy(c => c.NewProgramNr))
        {
          if (channel.IsDeleted || channel.NewProgramNr < 0)
            continue;
          if (channel.NewProgramNr == 0 && chNr == 1)
            chNr = 0;
          if (channel.NewProgramNr != chNr)
          {
            if (testOnly)
              return true;
            wasRenumbered = true;
            channel.NewProgramNr = chNr;
          }
          ++chNr;
        }
      }
      return wasRenumbered;
    }

    #endregion

    #region SaveReferenceFile()

    private void SaveReferenceFile()
    {
      string fileName;
      using (var dlg = new SaveFileDialog())
      {
        dlg.RestoreDirectory = true;
        dlg.InitialDirectory = Path.GetDirectoryName(this.currentRefFile);
        dlg.FileName = Path.GetFileName(this.currentRefFile);
        dlg.DefaultExt = ".txt";
        dlg.Filter = "ChanSort Single-List|*.txt|ChanSort Multi-List|*.csv|SamToolBox|*.chl|All files|*";
        dlg.FilterIndex = 1;
        dlg.CheckPathExists = true;
        dlg.CheckFileExists = false;
        dlg.AddExtension = true;
        if (dlg.ShowDialog(this) != DialogResult.OK)
          return;
        fileName = dlg.FileName;
      }

      var ext = (Path.GetExtension(fileName) ?? "").ToLower();
      if (ext == ".csv")
        CsvRefListSerializer.Save(fileName, this.DataRoot);
      else if (ext == ".chl" || ext == ".txt")
        TxtRefListSerializer.Save(fileName, this.CurrentChannelList);
    }

    #endregion

    #region SaveTvDataFile()

    private void SaveTvDataFile()
    {
      this.splashScreenManager1.ShowWaitForm();
      try
      {
        foreach (var filePath in this.currentTvSerializer.GetDataFilePaths())
        {
          if (File.Exists(filePath))
          {
            var bakFile = filePath + ".bak";
            if (!File.Exists(bakFile))
              File.Copy(filePath, bakFile);
          }
        }

        this.currentTvSerializer.Save(this.currentTvFile);
        this.DataRoot.ValidateAfterSave();
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
      if (this.rbInsertSwap.Checked)
      {
        this.SwapChannels();
        return;
      }

      var selectedChannels = this.GetSelectedChannels(gviewRight);
      if (selectedChannels.Count == 0) return;      

      ChannelInfo lastInsertedChannel;
      this.gviewLeft.BeginDataUpdate();
      this.gviewRight.BeginDataUpdate();

      // remove all the selected channels which are about to be added. 
      // This may require an adjustment of the insert position when channels are removed in front of it and gaps are closed.
      var insertSlot = this.CurrentChannelList.InsertProgramNumber;
      if (insertSlot == 1 && this.rbInsertAfter.Checked && this.gviewLeft.RowCount == 0)
        insertSlot = 0;     
      var contextRow = (ChannelInfo)this.gviewLeft.GetFocusedRow();
      if (contextRow != null)
      {
        if (!(this.rbInsertBefore.Checked && insertSlot == contextRow.NewProgramNr || this.rbInsertAfter.Checked && insertSlot == contextRow.NewProgramNr + 1))
          contextRow = null;
      }
      this.RemoveChannels(gviewRight, this.cbCloseGap.Checked);
      if (contextRow != null)
        this.CurrentChannelList.InsertProgramNumber = this.rbInsertBefore.Checked ? contextRow.NewProgramNr : contextRow.NewProgramNr + 1;
      else
        this.CurrentChannelList.InsertProgramNumber = insertSlot;


      try
      {
        lastInsertedChannel = this.Editor.AddChannels(selectedChannels);
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

      var index = this.CurrentChannelList.Channels.IndexOf(lastInsertedChannel);
      var rowHandle = this.gviewLeft.GetRowHandle(index);
      if (this.rbInsertBefore.Checked)
        ++rowHandle;
      this.SelectFocusedRow(this.gviewLeft, rowHandle);
    }

    #endregion

    #region SwapChannels()
    private void SwapChannels()
    {
      if (this.gviewRight.SelectedRowsCount == 0)
        return;

      if (this.gviewLeft.SelectedRowsCount != this.gviewRight.SelectedRowsCount)
      {
        XtraMessageBox.Show(this, Resources.MainForm_SwapChannels_RowCountMsg, Resources.MainForm_SwapChannels_RowCountTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      // get selected channel objects from left and right grid before we start modifying the data
      var leftChannels = this.GetSelectedChannels(gviewLeft);
      var rightChannels = this.GetSelectedChannels(gviewRight);

      // swap channel numbers
      ChannelInfo ch1 = null, ch2 = null;
      for (int i = 0, c = leftChannels.Count; i < c; i++)
      {
        ch1 = leftChannels[i];
        ch2 = rightChannels[i];
        int p = ch1.NewProgramNr;
        ch1.NewProgramNr = ch2.NewProgramNr;
        ch2.NewProgramNr = p;
      }

      // resort the grids
      this.RefreshGrids();

      // in the left grid, select the last swapped channel from the right grid
      this.gviewLeft.ClearSelection();
      var h = this.gviewLeft.GetRowHandle(this.CurrentChannelList.Channels.IndexOf(ch2));
      if (h >= 0)
      {
        this.gviewLeft.SelectRow(h);
        this.gviewLeft.FocusedRowHandle = h;
        this.gviewLeft.MakeRowVisible(h);
      }

      // in the right grid, select the last swapped channel from the left grid
      this.gviewRight.ClearSelection();
      h = this.gviewRight.GetRowHandle(this.CurrentChannelList.Channels.IndexOf(ch1));
      if (h >= 0)
      {
        this.gviewRight.SelectRow(h);
        this.gviewRight.FocusedRowHandle = h;
        this.gviewRight.MakeRowVisible(h);
      }
    }
    #endregion

    #region RemoveChannels()

    private void RemoveChannels(GridView grid, bool closeGap)
    {
      var selectedChannels = this.GetSelectedChannels(grid);
      if (selectedChannels.Count == 0) return;

      var focusedRow = this.gviewLeft.FocusedRowHandle - selectedChannels.Count;
      if (!gviewLeft.IsLastRow)
        ++focusedRow;
      if (focusedRow < 0) focusedRow = 0;
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      try
      {
        this.Editor.RemoveChannels(selectedChannels, closeGap);
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
      if (!this.IsLeftGridSortedByNewProgNr) return;
      var selectedChannels = this.GetSelectedChannels(this.gviewLeft);
      if (selectedChannels.Count == 0) return;

      this.gviewLeft.BeginDataUpdate();
      try
      {
        this.Editor.MoveChannels(selectedChannels, up);
      }
      finally
      {
        this.gviewLeft.EndDataUpdate();
      }
      this.UpdateInsertSlotNumber();
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
        this.Editor.SetSlotNumber(selectedChannels, prog, this.rbInsertSwap.Checked, this.cbCloseGap.Checked);
        this.txtSetSlot.Text = (prog + selectedChannels.Count).ToString();
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
      var selectedChannels = this.GetSelectedChannels(this.gviewLeft, true);
      if (selectedChannels.Count == 0) return;
      this.gviewLeft.BeginDataUpdate();
      this.gviewRight.BeginDataUpdate();
      try
      {
        this.Editor.SortSelectedChannels(selectedChannels);
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
      if (this.CurrentChannelList == null) return;
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      var maxNr = this.CurrentChannelList.InsertProgramNumber;
      foreach (var channel in this.CurrentChannelList.Channels)
        maxNr = Math.Max(maxNr, channel.GetPosition(this.subListIndex));

      var max = this.gviewRight.RowCount;
      for (var handle = 0; handle < max; handle++)
      {
        var channel = (ChannelInfo) this.gviewRight.GetRow(handle);
        if (channel != null && channel.GetPosition(this.subListIndex) == -1 && !channel.IsDeleted)
          channel.SetPosition(this.subListIndex, maxNr++);
      }

      this.gviewRight.EndDataUpdate();
      this.gviewLeft.EndDataUpdate();
    }

    #endregion

    #region RenumberSelectedChannels()

    private void RenumberSelectedChannels()
    {
      var list = this.GetSelectedChannels(this.gviewLeft, true);
      if (list.Count == 0) return;
      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      this.Editor.RenumberChannels(list);
      this.gviewLeft.EndDataUpdate();
      this.gviewRight.EndDataUpdate();
    }

    #endregion

    #region GetSelectedChannels()

    private List<ChannelInfo> GetSelectedChannels(GridView gview, bool selectAllIfOnlyOneIsSelected = false)
    {
      var channels = new List<ChannelInfo>();
      if (gview.SelectedRowsCount <= 1 && selectAllIfOnlyOneIsSelected)
      {
        for (int rowHandle=0; rowHandle<gview.RowCount; rowHandle++)
          channels.Add((ChannelInfo)gview.GetRow(rowHandle));
      }
      else
      {
        foreach (var rowHandle in gview.GetSelectedRows())
        {
          if (gview.IsDataRow(rowHandle))
            channels.Add((ChannelInfo) gview.GetRow(rowHandle));
        }
      }

      return channels;
    }

    #endregion

    #region TryExecute()

    private void TryExecute(Action action)
    {
      try
      {
        action();
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
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
      if (!string.IsNullOrEmpty(Config.Default.Encoding))
        this.defaultEncoding = Encoding.GetEncoding(Config.Default.Encoding);

      var width = Config.Default.LeftPanelWidth;
      if (width > 0)
        this.splitContainerControl1.SplitterPosition = width; // set unscaled value because the whole Form will be scaled later
      this.SelectLanguageMenuItem();

      //this.SetGridLayout(this.gviewLeft, Config.Default.OutputListLayout);

      this.miShowWarningsAfterLoad.Checked = Config.Default.ShowWarningsAfterLoading;
      this.cbCloseGap.Checked = Config.Default.CloseGaps;
      this.ClearLeftFilter();
      this.ClearRightFilter();
      this.mruFiles.Clear();
      this.mruFiles.AddRange(Config.Default.MruFiles);
      this.UpdateMruMenu();

      this.miExplorerIntegration.Down = Config.Default.ExplorerIntegration;
      this.miCheckUpdates.Down = Config.Default.CheckForUpdates;

      foreach (var mi in new[] { miFontSmall, miFontMedium, miFontLarge, miFontXLarge, miFontXxLarge })
      {
        if ((int)mi.Tag == Config.Default.FontSizeDelta)
        {
          mi.Down = true;
          break;
        }
      }
    }

    #endregion

    #region SelectLanguageMenuItem()

    private void SelectLanguageMenuItem()
    {
      this.barManager1.ForceLinkCreate();
      foreach (BarItemLink itemLink in this.barSubItem1.ItemLinks)
      {
        if (Config.Default.Language.StartsWith((string) itemLink.Item.Tag))
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
      var stream = new MemoryStream();
      using (var wrt = new StreamWriter(stream))
      {
        wrt.Write(layout);
        wrt.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        var options = new OptionsLayoutGrid();
        options.StoreDataSettings = true;
        options.StoreAppearance = false;
        options.StoreVisualOptions = false;
        grid.RestoreLayoutFromStream(stream, options);
      }

      // put the filter text back into the auto-filter-row
      foreach (GridColumn col in grid.Columns)
      {
        var parts = (col.FilterInfo.FilterString ?? "").Split('\'');
        if (parts.Length >= 2)
          this.gviewRight.SetRowCellValue(GridControl.AutoFilterRowHandle, col, parts[1]);
      }
    }

    #endregion

    #region UpdateInsertSlotNumber()

    private void UpdateInsertSlotNumber()
    {
      if (this.subListIndex < 0)
        return;
      var channel = (ChannelInfo) this.gviewLeft.GetFocusedRow();
      int programNr;
      if (channel == null)
        programNr = this.CurrentChannelList == null ? 1 : this.CurrentChannelList.FirstProgramNumber;
      else
      {
        programNr = channel.GetPosition(this.subListIndex);
        if (this.rbInsertAfter.Checked)
          ++programNr;
      }
      if (this.CurrentChannelList != null)
        this.CurrentChannelList.InsertProgramNumber = programNr;
      this.UpdateInsertSlotTextBox();
      this.gviewLeft.SelectRow(this.gviewLeft.FocusedRowHandle);
    }

    #endregion

    #region UpdateInsertSlotTextBox()

    private void UpdateInsertSlotTextBox()
    {
      var programNr = this.CurrentChannelList == null ? 0 : this.CurrentChannelList.InsertProgramNumber;
      this.txtSetSlot.Text = programNr.ToString();
    }

    #endregion

    #region FillMenuWithIsoEncodings()

    private void FillMenuWithIsoEncodings()
    {
      this.miIsoCharSets.Strings.Clear();
      this.isoEncodings.Clear();
      foreach (var encoding in Encoding.GetEncodings())
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

    #region LoadInputGridLayout()

    private void LoadInputGridLayout()
    {
#if false
  // code disabled because it causes unpredictable column order when working with different file formats which may of may not show columns

      string newLayout;
      var newSource = list.SignalSource;
      if ((newSource & SignalSource.Analog) != 0)
        newLayout = Config.Default.InputGridLayoutAnalog;
      else if ((newSource & SignalSource.DvbS) != 0)
        newLayout = Config.Default.InputGridLayoutDvbS; 
      else
        newLayout = Config.Default.InputGridLayoutDvbCT;
      if (!string.IsNullOrEmpty(newLayout))
        this.SetGridLayout(this.gviewRight, newLayout);
#endif
      this.ShowGridColumns(this.gviewLeft);
      this.ShowGridColumns(this.gviewRight);
      this.ClearRightFilter();
    }

    #endregion

    #region ShowGridColumns()

    private void ShowGridColumns(GridView gview)
    {
      var visIndex = 0;
      foreach (GridColumn col in gview.Columns)
        col.VisibleIndex = GetGridColumnVisibility(col) ? visIndex++ : -1;
    }

    #endregion

    #region SaveInputGridLayout()

    private void SaveInputGridLayout(SignalSource signalSource)
    {
      var currentLayout = GetGridLayout(this.gviewRight);
      if ((signalSource & SignalSource.Analog) != 0)
        Config.Default.InputGridLayoutAnalog = currentLayout;
      else if ((signalSource & SignalSource.DvbS) != 0)
        Config.Default.InputGridLayoutDvbS = currentLayout;
      else //if ((signalSource & SignalSource.DvbCT) != 0)
        Config.Default.InputGridLayoutDvbCT = currentLayout;
    }

    #endregion

    #region GetGridColumnVisibility()

    private bool GetGridColumnVisibility(GridColumn col)
    {
      var list = this.CurrentChannelList;
      if (list == null)
        return false;

      if (list.IsMixedSourceFavoritesList)
      {
        if (col == this.colSource || col == this.colOutSource) return true;
        if (col == this.colOutHide || col == this.colOutLock || col == this.colOutSkip) return false;
      }
      
      var filter = list.VisibleColumnFieldNames;
      if (filter != null)
      {
        if (filter.Contains("+" + col.FieldName)) // force-show without further checks
          return true;
        if (filter.Contains("-" + col.FieldName) || !filter.Contains(col.FieldName)) // force-hide without further checks
          return false;
      }
      else if (col.Tag is bool originalVisible && !originalVisible)
        return false;

      var source = list.SignalSource;
      if (col == this.colPrNr) return this.subListIndex > 0;
      if (col == this.colChannelOrTransponder) return (source & SignalSource.Sat) == 0;
      if (col == this.colShortName) return (source & SignalSource.Digital) != 0;
      if (col == this.colEncrypted) return (source & SignalSource.Digital) != 0;
      if (col == this.colServiceId) return (source & SignalSource.Digital) != 0;
      if (col == this.colPcrPid) return (source & SignalSource.Digital) != 0;
      if (col == this.colVideoPid) return (source & SignalSource.Digital) != 0;
      if (col == this.colAudioPid) return (source & SignalSource.Digital) != 0;
      //if (col == this.colServiceType) return (source & SignalSource.Digital) != 0;
      if (col == this.colServiceTypeName) return (source & SignalSource.Digital) != 0;
      if (col == this.colTransportStreamId) return (source & SignalSource.Digital) != 0;
      if (col == this.colNetworkName) return (source & SignalSource.Digital) != 0;
      if (col == this.colNetworkOperator) return (source & SignalSource.Digital) != 0;
      if (col == this.colProvider) return (source & SignalSource.Digital) != 0;
      if (col == this.colSatellite) return (source & SignalSource.Sat) != 0;
      if (col == this.colNetworkId) return (source & SignalSource.Digital) != 0;
      if (col == this.colSymbolRate) return (source & SignalSource.Digital) != 0;
      if (col == this.colSkip) return (source & SignalSource.Digital) != 0 && this.DataRoot.CanSkip;
      if (col == this.colLock) return (source & SignalSource.Digital) != 0 && this.DataRoot.CanLock;
      if (col == this.colHidden) return (source & SignalSource.Digital) != 0 && this.DataRoot.CanHide;
      if (col == this.colIndex) return col.Visible;
      if (col == this.colUid) return col.Visible;
      if (col == this.colDebug) return col.Visible;
      if (col == this.colSignalSource) return col.Visible;
      if (col == this.colLogicalIndex) return col.Visible;
      if (col == this.colPolarity) return (source & SignalSource.Sat) != 0 || (source & SignalSource.IP) != 0;

      return true;
    }

    #endregion

    #region SetFavorite()

    private void SetFavorite(string fav, bool set)
    {
      if (string.IsNullOrEmpty(fav)) return;
      int idx = char.ToUpper(fav[0]) - 'A';
      if (idx < 0 || idx >= this.mnuFavSet.ItemLinks.Count || this.subListIndex == idx + 1) return;
      var list = this.GetSelectedChannels(this.lastFocusedGrid);
      if (list.Count == 0) return;

      this.gviewRight.BeginDataUpdate();
      this.gviewLeft.BeginDataUpdate();
      this.Editor.SetFavorites(list, idx, set);
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
      this.DataRoot.NeedsSaving = true;
    }

    #endregion

    #region NavigateToChannel

    private void NavigateToChannel(ChannelInfo channel, GridView view)
    {
      if (channel == null) return;
      var rowHandle = view.GetRowHandle(this.CurrentChannelList.Channels.IndexOf(channel));
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

    private void UpdateMenu(bool afterFileLoad = false)
    {
      var fileLoaded = this.DataRoot != null;
      var isRight = this.lastFocusedGrid == this.gviewRight;
      var mayEdit = fileLoaded && this.CurrentChannelList != null && (!this.CurrentChannelList.ReadOnly || this.miAllowEditPredefinedLists.Down);

      foreach (BarItemLink link in this.miEdit.ItemLinks)
        link.Item.Enabled = mayEdit;

      this.btnAdd.Enabled = mayEdit;
      this.btnAddAll.Enabled = mayEdit;
      this.btnRemoveLeft.Enabled = mayEdit;
      this.btnRemoveRight.Enabled = mayEdit;
      this.btnRenum.Enabled = mayEdit;
      this.btnToggleFavA.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.A) != 0 && this.subListIndex != 1;
      this.btnToggleFavB.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.B) != 0 && this.subListIndex != 2;
      this.btnToggleFavC.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.C) != 0 && this.subListIndex != 3;
      this.btnToggleFavD.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.D) != 0 && this.subListIndex != 4;
      this.btnToggleFavE.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.E) != 0 && this.subListIndex != 5;
      this.btnToggleFavF.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.F) != 0 && this.subListIndex != 6;
      this.btnToggleFavG.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.G) != 0 && this.subListIndex != 7;
      this.btnToggleFavH.Enabled = mayEdit && (this.DataRoot.SupportedFavorites & Favorites.H) != 0 && this.subListIndex != 8;
      this.btnToggleLock.Enabled = mayEdit && this.DataRoot.CanLock;

      if (afterFileLoad)
      {
        // this block may contain some time-expensive checks that only need to be done after loading a file
        this.miReload.Enabled = fileLoaded;
        this.miFileInformation.Enabled = fileLoaded;
        this.miRestoreOriginal.Enabled = fileLoaded && this.GetPathOfMissingBackupFile() == null;
        this.miSave.Enabled = fileLoaded;
        this.miSaveAs.Enabled = fileLoaded && this.currentTvSerializer.Features.CanSaveAs;
        this.miOpenReferenceFile.Enabled = fileLoaded;
        this.miSaveReferenceFile.Enabled = fileLoaded;
        this.miExcelExport.Enabled = fileLoaded;
        this.miPrint.Enabled = fileLoaded;
      }

      this.miAddChannel.Enabled = fileLoaded && isRight;

      var visRight = isRight ? BarItemVisibility.Always : BarItemVisibility.Never;
      var visLeft = isRight ? BarItemVisibility.Never : BarItemVisibility.Always;

      this.miSort.Visibility = visLeft;
      this.miRenum.Visibility = visLeft;
      this.miMoveUp.Visibility = visLeft;
      this.miMoveDown.Visibility = visLeft;
      this.miAddChannel.Visibility = visRight;
      this.miSkipOn.Enabled = this.miSkipOff.Enabled = this.currentTvSerializer?.Features.CanSkipChannels ?? false;
      this.miLockOn.Enabled = this.miLockOff.Enabled = this.currentTvSerializer?.Features.CanLockChannels ?? false;
      this.miHideOn.Enabled = this.miHideOff.Enabled = this.currentTvSerializer?.Features.CanHideChannels ?? false;

      var isLeftGridSortedByNewProgNr = this.IsLeftGridSortedByNewProgNr;
      var sel = this.gviewLeft.GetSelectedRows();
      var channel = sel.Length == 0 ? null : (ChannelInfo) this.gviewLeft.GetRow(sel[0]);
      this.miMoveUp.Enabled = this.btnUp.Enabled = mayEdit && this.subListIndex >= 0 && isLeftGridSortedByNewProgNr && channel != null
                                                   && channel.GetPosition(this.subListIndex) > this.CurrentChannelList.FirstProgramNumber;
      this.miMoveDown.Enabled = this.btnDown.Enabled = mayEdit && isLeftGridSortedByNewProgNr;

      this.miTvSettings.Enabled = this.currentTvSerializer != null && this.currentTvSerializer.Features.DeviceSettings;
      this.miCleanupChannels.Enabled = this.currentTvSerializer != null && this.currentTvSerializer.Features.CleanUpChannelData;

      this.mnuFavList.Enabled = this.grpSubList.Visible;

      this.txtSetSlot.Enabled = mayEdit;
    }

    #endregion

    #region UpdateMruMenu()

    private void UpdateMruMenu()
    {
      this.miRecentFiles.Strings.Clear();
      foreach (var file in this.mruFiles)
      {
        var key = Path.GetFileName(Path.GetDirectoryName(file)) + "\\" + Path.GetFileName(file);
        if (key != file)
          key = "...\\" + key;
        this.miRecentFiles.Strings.Add(key);
      }
    }

    #endregion

    #region GetPathOfMissingBackupFile()
    /// <summary>
    /// If any backup file exists, return NULL. Otherwise the name of any expected .bak file (in case the loader has multiple data files)
    /// </summary>
    /// <returns></returns>
    private string GetPathOfMissingBackupFile()
    {
      var files = this.currentTvSerializer.GetDataFilePaths().ToList();
      string bakFile = null;
      foreach (var dataFilePath in files)
      {
        bakFile = dataFilePath + ".bak";
        if (File.Exists(bakFile))
          return null;
      }

      return bakFile;
    }
    #endregion

    #region RestoreBackupFile()

    private void RestoreBackupFile()
    {
      var bakFile = this.GetPathOfMissingBackupFile();
      if (bakFile != null)
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
        foreach (var dataFilePath in this.currentTvSerializer.GetDataFilePaths())
        {
          bakFile = dataFilePath + ".bak";
          File.Copy(bakFile, dataFilePath, true);
          var attr = File.GetAttributes(dataFilePath);
          File.SetAttributes(dataFilePath, attr & ~FileAttributes.ReadOnly);
        }

        this.currentTvSerializer.DataRoot.NeedsSaving = false;
        if (this.currentPlugin != null)
          this.LoadFiles(this.currentPlugin, this.currentTvFile);
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

      if (this.DataRoot.Warnings.Length > 0)
      {
        var lines = this.DataRoot.Warnings.ToString().Split('\n');
        Array.Sort(lines);
        var sortedWarnings = string.Join("\n", lines);
        info += "\r\n\r\n\r\n" + Resources.MainForm_LoadFiles_ValidationWarningMsg + "\r\n\r\n" + sortedWarnings;
      }

      InfoBox.Show(this, info, this.miFileInformation.Caption.Replace("...", "").Replace("&", ""));
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

    #region ToggleFavorite()

    private void ToggleFavorite(string fav)
    {
      var list = this.GetSelectedChannels(this.gviewLeft);
      if (list.Count == 0) return;
      var value = (Favorites) Enum.Parse(typeof (Favorites), fav);
      this.SetFavorite(fav, (list[0].Favorites & value) == 0);
      this.RefreshGrid(gviewLeft, gviewRight);
    }

    #endregion

    #region ToggleLock()

    private void ToggleLock()
    {
      var list = this.GetSelectedChannels(this.gviewLeft);
      if (list.Count == 0) return;
      var setLock = !list[0].Lock;
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

    #region CleanupChannelData()

    private void CleanupChannelData()
    {
      if (this.currentTvSerializer != null && this.currentTvSerializer.Features.CleanUpChannelData)
      {
        var msg = this.currentTvSerializer.CleanUpChannelData();
        this.FillChannelListTabs();
        InfoBox.Show(this, msg, this.miCleanupChannels.Caption);
        this.RefreshGrid(gviewLeft, gviewRight);
      }
    }

    #endregion

    #region ExportExcelList()

    private void ExportExcelList()
    {
      const string header = "List;Pr#;Channel Name;Favorites;Lock;Skip;Hide;Encrypted;Satellite;Ch/Tp;Freq;ONID;TSID;SymRate;SID;VPID;APID";
      const char sep = '\t';
      var sb = new StringBuilder();
      sb.AppendLine(header.Replace(';', sep));
      foreach (var list in this.DataRoot.ChannelLists)
      {
        foreach (var channel in list.Channels.OrderBy(c => c.NewProgramNr))
        {
          if (channel.IsDeleted || channel.NewProgramNr == -1)
            continue;
          sb.Append(list.ShortCaption).Append(sep);
          sb.Append(channel.NewProgramNr).Append(sep);
          sb.Append('"').Append(channel.Name).Append('"').Append(sep);
          sb.Append(channel.Favorites).Append(sep);
          sb.Append(channel.Lock ? "L" : "").Append(sep);
          sb.Append(channel.Skip ? "S" : "").Append(sep);
          sb.Append(channel.Hidden ? "H" : "").Append(sep);
          sb.Append(channel.Encrypted == null ? "?" : channel.Encrypted.Value ? "C" : "").Append(sep);
          sb.Append('"').Append(channel.Satellite).Append('"').Append(sep);
          sb.Append(channel.ChannelOrTransponder).Append(sep);
          sb.Append(channel.FreqInMhz).Append(sep);
          sb.Append(channel.OriginalNetworkId).Append(sep);
          sb.Append(channel.TransportStreamId).Append(sep);
          sb.Append(channel.SymbolRate).Append(sep);
          sb.Append(channel.ServiceId).Append(sep);
          sb.Append(channel.VideoPid).Append(sep);
          sb.Append(channel.AudioPid);

          sb.AppendLine();
        }
      }

      Clipboard.Clear();
      Clipboard.SetData(DataFormats.Text, sb.ToString());
      XtraMessageBox.Show(this,
        Resources.MainForm_ExportExcelList_Message,
        this.miExcelExport.Caption,
        MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #endregion

    #region Print()

    private void Print()
    {
      using (var dlg = new ReportOptionsDialog(this.CurrentChannelList, this.subListIndex))
        dlg.ShowDialog(this);
    }

    #endregion

    #region GetFavString()
    private string GetFavString(Favorites fav)
    {
      if (fav == 0)
        return string.Empty;

      var sb = new StringBuilder();
      int i = 0;
      for (var mask = (int)fav; mask != 0; mask >>= 1)
      {
        if ((mask & 1) != 0)
          sb.Append((char)('A' + i));
        ++i;
      }
      return sb.ToString();
    }
    #endregion

    // UI events

    #region MainForm_Load

    private void MainForm_Load(object sender, EventArgs e)
    {
      this.TryExecute(this.LoadSettings);
      this.TryExecute(this.InitAppAfterMainWindowWasShown);

      var args = Environment.GetCommandLineArgs();
      if (args.Length > 1)
        this.TryExecute(() => this.LoadFiles(null, args[args.Length - 1]));
    }

    #endregion

    #region MainForm_DragEnter, DragDrop

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = DragDropEffects.None;
      if (e.Data.GetDataPresent("FileNameW"))
      {
        if (e.Data.GetData("FileNameW") is string[] files && files.Length == 1)
          e.Effect = DragDropEffects.Copy;
      }
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        if (e.Data.GetDataPresent("FileNameW"))
        {
          if (e.Data.GetData("FileNameW") is string[] files && files.Length == 1)
            this.LoadFiles(null, files[0]);
        }
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }
    #endregion

    #region ProcessCmdKey()

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == Keys.F1)
      {
        this.popupInputSource.ShowPopup(this.tabChannelList.PointToScreen(new Point(0, this.tabChannelList.Height)));
        return true;
      }

      if (keyData == (Keys.F1 | Keys.Shift))
      {
        this.popupFavList.ShowPopup(this.tabSubList.PointToScreen(new Point(0, this.tabSubList.Height)));
        return true;
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }

    #endregion

    #region OnDpiChanged, OnScaleControl
    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
      GlobalImageCollection.Scale((float)e.DeviceDpiNew / e.DeviceDpiOld, true);
      base.OnDpiChanged(e);
    }

    protected override void OnScaleControl()
    {
      this.absScaleFactor = absScaleFactor.Scale(this.AutoScaleFactor);
      GlobalImageCollection.Scale(this.AutoScaleFactor.Height, true);
      base.OnScaleControl();
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

    private void tabChannelList_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
    {
      this.TryExecute(() => ShowChannelList(e.Page == null ? null : (ChannelList) e.Page.Tag));
    }

    #endregion

    #region tabSubList_SelectedPageChanged

    private void tabSubList_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
    {
      this.subListIndex = this.tabSubList.SelectedTabPageIndex;
      this.ShowGridColumns(this.gviewRight);

      this.Editor.SubListIndex = this.subListIndex;
      this.gviewLeft.BeginSort();
      this.gviewLeft.EndSort();
      this.gviewRight.BeginSort();
      if (this.subListIndex > 0 && !this.CurrentChannelList.IsMixedSourceFavoritesList)
        this.colPrNr.FilterInfo = new ColumnFilterInfo("[NewProgramNr]<>-1");
      else
        this.colPrNr.ClearFilter();
      this.gviewRight.EndSort();

      this.UpdateInsertSlotNumber();
    }

    #endregion

    #region gview_CustomUnboundColumnData

    private void gview_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
    {
      var channel = (ChannelInfo) e.Row;
      if (e.Column.FieldName == "Position")
        e.Value = channel.GetPosition(this.subListIndex);
      else if (e.Column.FieldName == "OldPosition")
        e.Value = channel.GetOldPosition(this.subListIndex);
    }

    #endregion

    #region gview_MouseMove

    private void gview_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        var view = (GridView) sender;
        if (this.downHit == null || downHit.RowHandle < 0 || e.Button != MouseButtons.Left || view.ActiveEditor != null || ModifierKeys != Keys.None)
          return;
        if (this.CurrentChannelList == null || this.CurrentChannelList.ReadOnly)
          return;
        // drag/drop only allowed when left grid is sorted by NewSlotNr
        if (!this.IsLeftGridSortedByNewProgNr)
          return;
        if (Math.Abs(e.Y - downHit.HitPoint.Y) < SystemInformation.DragSize.Height &&
            Math.Abs(e.X - downHit.HitPoint.X) < SystemInformation.DragSize.Width)
          return;

        // start drag operation
        var channel = (ChannelInfo) view.GetRow(downHit.RowHandle);
        this.dragDropInfo = new DragDropInfo(view, channel.GetPosition(this.subListIndex));
        view.GridControl.DoDragDrop(this.dragDropInfo, DragDropEffects.Move);
        this.downHit = null;
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    #endregion

    #region grid_GiveFeedback

    private void grid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
      // this event is called on the source of the drag operation
      e.UseDefaultCursors = false;
      if (e.Effect == DragDropEffects.Move)
      {
        if (this.dragDropInfo.EditMode == EditMode.InsertBefore)
          Cursor.Current = Cursors.PanNE;
        else if (this.dragDropInfo.EditMode == EditMode.InsertAfter)
          Cursor.Current = Cursors.PanSE;
        else
          Cursor.Current = Cursors.HSplit;
      }
      else if (sender == this.gridRight)
        Cursor.Current = Cursors.PanWest;
      else
        Cursor.Current = Cursors.No;
    }

    #endregion

    #region gridLeft_DragOver

    private void gridLeft_DragOver(object sender, DragEventArgs e)
    {
      if (this.dragDropInfo == null) // drag operation from outside ChanSort
      {
        MainForm_DragEnter(sender, e);
        return;
      }

      // this event is called on the current target of the drag operation
      var point = this.gridLeft.PointToClient(MousePosition);
      var hit = this.gviewLeft.CalcHitInfo(point);
      if (hit.RowHandle >= 0)
      {
        var vi = (GridViewInfo) this.gviewLeft.GetViewInfo();
        var rowInfo = vi.GetGridRowInfo(hit.RowHandle);
        var dropChannel = (ChannelInfo) this.gviewLeft.GetRow(hit.RowHandle);
        var moveUp = this.dragDropInfo.SourcePosition < 0 || dropChannel.GetPosition(this.subListIndex) <= this.dragDropInfo.SourcePosition;
        if (moveUp && point.Y < rowInfo.Bounds.Top + rowInfo.Bounds.Height/2)
          this.dragDropInfo.EditMode = EditMode.InsertBefore;
        else if (!moveUp && point.Y > rowInfo.Bounds.Top + rowInfo.Bounds.Height/2)
          this.dragDropInfo.EditMode = EditMode.InsertAfter;
        else if (this.dragDropInfo.SourceView == this.gviewLeft)
          this.dragDropInfo.EditMode = EditMode.Swap;
        else if (moveUp)
          this.dragDropInfo.EditMode = EditMode.InsertAfter;
        else
          this.dragDropInfo.EditMode = EditMode.InsertBefore;

        this.dragDropInfo.DropRowHandle = hit.RowHandle;
        e.Effect = DragDropEffects.Move;
        return;
      }

      e.Effect = DragDropEffects.None;
      this.dragDropInfo.DropRowHandle = GridControl.InvalidRowHandle;
    }

    #endregion

    #region gridLeft_DragDrop

    private void gridLeft_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        if (this.dragDropInfo == null)
        {
          MainForm_DragDrop(sender, e);
          return;
        }

        if (this.dragDropInfo.DropRowHandle < 0) return;
        this.curEditMode = this.dragDropInfo.EditMode;
        var dropChannel = (ChannelInfo) this.gviewLeft.GetRow(this.dragDropInfo.DropRowHandle);

        var selectedChannels = this.GetSelectedChannels(this.dragDropInfo.SourceView);
        int newProgNr;
        var dropPos = dropChannel.GetPosition(this.subListIndex);
        if (this.dragDropInfo.EditMode != EditMode.InsertAfter || !this.cbCloseGap.Checked)
          newProgNr = dropPos;
        else
        {
          var numberOfChannelsToMoveDown = 0;
          foreach (var channel in selectedChannels)
          {
            var curPos = channel.GetPosition(this.subListIndex);
            if (curPos != -1 && curPos <= dropPos)
              ++numberOfChannelsToMoveDown;
          }
          newProgNr = dropPos + 1 - numberOfChannelsToMoveDown;
        }

        this.Editor.SetSlotNumber(selectedChannels, newProgNr, this.dragDropInfo.EditMode == EditMode.Swap, this.cbCloseGap.Checked);
        this.RefreshGrid(this.gviewLeft, this.gviewRight);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    #endregion

    #region gridLeft_ProcessGridKey

    private void gridLeft_ProcessGridKey(object sender, KeyEventArgs e)
    {
      if (this.CurrentChannelList != null && this.CurrentChannelList.ReadOnly)
        return;
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
      e.SuppressKeyPress = true;
    }

    #endregion

    #region gviewLeft_FocusedRowChanged

    private void gviewLeft_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
      TryExecute(UpdateInsertSlotNumber);
    }

    #endregion

    #region gviewLeft_SelectionChanged

    private void gviewLeft_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateMenu();
    }

    #endregion

    #region gviewLeft_CustomColumnDisplayText

    private void gviewLeft_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column == this.colOutFav)
      {
        if (e.Value is Favorites fav)
          e.DisplayText = GetFavString(fav);
      }
    }

    #endregion

    #region gviewLeft_RowCellStyle

    private void gviewLeft_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      var channel = (ChannelInfo) this.gviewLeft.GetRow(e.RowHandle);
      if (channel == null) return;
      if (channel.IsProxy || channel.IsDeleted)
      {
        e.Appearance.ForeColor = Color.Red;
        e.Appearance.Options.UseForeColor = true;
      }
      else if (channel.Hidden)
      {
        e.Appearance.ForeColor = Color.LightGray;
        e.Appearance.Options.UseForeColor = true;
      }
      else if (channel.Skip)
      {
        e.Appearance.ForeColor = Color.Blue;
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
          e.Valid = this.SetSlotNumber((string) e.Value);
        else if (this.gviewLeft.FocusedColumn == this.colOutFav && e.Value is string)
          e.Value = ChannelInfo.ParseFavString((string) e.Value);
        else if (gviewLeft.FocusedColumn == this.colOutName)
        {
          this.VerifyChannelNameModified(this.gviewLeft.GetFocusedRow() as ChannelInfo, e.Value as string);
          this.BeginInvoke((Action) (() => RefreshGrid(this.gviewLeft)));
        }
        DataRoot.NeedsSaving = true;
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
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
        this.popupContext.ShowPopup(this.gridLeft.PointToScreen(e.Point));
    }

    #endregion

    #region gviewLeft_RowClick

    private void gviewLeft_RowClick(object sender, RowClickEventArgs e)
    {
      if (e.Clicks == 2 && e.Button == MouseButtons.Left && this.gviewLeft.IsDataRow(e.RowHandle))
      {
        var channel = (ChannelInfo) this.gviewLeft.GetRow(e.RowHandle);
        this.NavigateToChannel(channel, this.gviewRight);
      }
    }

    #endregion

    #region gviewLeft_EndSorting

    private void gviewLeft_EndSorting(object sender, EventArgs e)
    {
      TryExecute(() => this.UpdateMenu());
    }

    #endregion

    #region gridRight_ProcessGridKey

    private void gridRight_ProcessGridKey(object sender, KeyEventArgs e)
    {
      if (this.gviewRight.ActiveEditor != null)
        return;
      if (e.KeyCode == Keys.Enter && this.CurrentChannelList != null && !this.CurrentChannelList.ReadOnly)
      {
        TryExecute(this.AddChannels);
        e.Handled = true;
      }
    }

    #endregion

    #region gridRight_DragEnter, DragDrop
    private void gridRight_DragEnter(object sender, DragEventArgs e)
    {
      MainForm_DragEnter(sender, e);
    }

    private void gridRight_DragDrop(object sender, DragEventArgs e)
    {
      MainForm_DragDrop(sender, e);
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
      if (e.Column == this.colSlotNew || e.Column == this.colSlotOld || e.Column == this.colPrNr)
      {
        if (!(e.Value is int)) return;
        if ((int) e.Value == -1)
          e.DisplayText = string.Empty;
      }
      else if (e.Column == this.colFavorites)
      {
        if (e.Value is Favorites fav)
          e.DisplayText = GetFavString(fav);
      }
    }

    #endregion

    #region gviewRight_RowCellStyle

    private void gviewRight_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      var channel = (ChannelInfo) this.gviewRight.GetRow(e.RowHandle);
      if (channel == null) return;
      if (channel.IsProxy)
      {
        e.Appearance.ForeColor = Color.Red;
        e.Appearance.Options.UseForeColor = true;
      }
      else if (channel.GetPosition(this.subListIndex) != -1)
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
      {
        if (this.rbInsertSwap.Checked)
        {
          TryExecute(this.SwapChannels);          
        }
        else
          TryExecute(this.AddChannels);

        // rows were re-arranged and the pending MouseDown event handler would focus+select the wrong row again
        this.dontFocusClickedRow = true;
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
          e.Valid = this.SetSlotNumber((string) e.Value);
        else if (this.gviewRight.FocusedColumn == this.colFavorites && e.Value is string)
          e.Value = ChannelInfo.ParseFavString((string) e.Value);
        else if (gviewRight.FocusedColumn == this.colName)
        {
          var ci = this.gviewRight.GetFocusedRow() as ChannelInfo;
          this.VerifyChannelNameModified(ci, e.Value as string);
          //this.BeginInvoke((Action) (() => RefreshGrid(this.gviewLeft)));
        }
        DataRoot.NeedsSaving = true;
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    #endregion

    #region gviewRight_CellValueChanged

    private void gviewRight_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {
      TryExecute(() => RefreshGrid(this.gviewLeft));
    }

    #endregion

    #region gviewRight_CustomColumnSort
    private void gviewRight_CustomColumnSort(object sender, CustomColumnSortEventArgs e)
    {
      if (e.Column == this.colSlotOld)
      {
        // sort unassigned channels (PrNr = -1) to the bottom of the list
        var ch1 = (int)this.gviewRight.GetListSourceRowCellValue(e.ListSourceRowIndex1, e.Column);
        var ch2 = (int)this.gviewRight.GetListSourceRowCellValue(e.ListSourceRowIndex2, e.Column);
        if (ch1 < 0) ch1 = int.MaxValue;
        if (ch2 < 0) ch2 = int.MaxValue;
        e.Result = System.Collections.Comparer.Default.Compare(ch1, ch2);
        e.Handled = true;
      }
    }
    #endregion

    #region gviewRight_PopupMenuShowing

    private void gviewRight_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      this.lastFocusedGrid = this.gviewRight;
      this.UpdateMenu();
      if (e.MenuType == GridMenuType.Row)
        this.popupContext.ShowPopup(this.gridRight.PointToScreen(e.Point));
    }

    #endregion

    #region rbInsertMode_CheckedChanged

    private void rbInsertMode_CheckedChanged(object sender, EventArgs e)
    {
      if (!((CheckEdit) sender).Checked)
        return;
      try
      {
        this.btnAdd.ImageIndex = this.rbInsertSwap.Checked ? 38 : this.rbInsertAfter.Checked ? 39 : 40;
        this.miAddChannel.ImageIndex = this.btnAdd.ImageIndex;

        if (this.CurrentChannelList == null)
          return;

        if (this.gviewLeft.RowCount == 0)
          this.CurrentChannelList.InsertProgramNumber = 1;
        else
        {
          var delta = this.curEditMode == EditMode.InsertAfter
            ? -1
            : this.rbInsertAfter.Checked ? +1 : 0;
          this.CurrentChannelList.InsertProgramNumber += delta;
        }

        this.UpdateInsertSlotTextBox();
        this.curEditMode = this.rbInsertBefore.Checked
          ? EditMode.InsertBefore
          : this.rbInsertAfter.Checked
            ? EditMode.InsertAfter
            : EditMode.Swap;
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    #endregion

    #region btnAdd_Click

    private void btnAdd_Click(object sender, EventArgs e)
    {
      TryExecute(this.AddChannels);
    }

    #endregion

    #region txtSetSlot_EditValueChanged

    private void txtSetSlot_EditValueChanged(object sender, EventArgs e)
    {
      TryExecute(() =>
      {
        int nr;
        int.TryParse(this.txtSetSlot.Text, out nr);
        if (this.CurrentChannelList != null)
          this.CurrentChannelList.InsertProgramNumber = nr;
      });
    }

    #endregion

    #region btnRenum_Click

    private void btnRenum_Click(object sender, EventArgs e)
    {
      TryExecute(this.RenumberSelectedChannels);
    }

    #endregion

    #region MainForm_FormClosing

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        if (this.PromptSaveAndContinue())
        {
          this.SaveSettings();
          this.currentTvSerializer?.Dispose();
        }
        else
          e.Cancel = true;
      }
      catch
      {
        // ignore - always allow to exit
      }
    }

    #endregion

    #region btnAddAll_Click

    private void btnAddAll_Click(object sender, EventArgs e)
    {
      this.TryExecute(this.AddAllUnsortedChannels);
    }

    #endregion

    #region miRenumFavByPrNr_ItemClick

    private void miRenumFavByPrNr_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.Editor.ApplyPrNrToFavLists);
      this.RefreshGrid(this.gviewLeft, this.gviewRight);
    }

    #endregion

    #region miAllowEditPredefinedLists_DownChanged

    private void miAllowEditPredefinedLists_DownChanged(object sender, ItemClickEventArgs e)
    {
      TryExecute(() =>
      {
        this.UpdateGridReadOnly();
        this.UpdateMenu();
      });
    }

    #endregion

    #region enum EditMode

    private enum EditMode
    {
      InsertBefore = 0,
      InsertAfter = 1,
      Swap = 2
    }

    #endregion

    #region class DragDropInfo

    private class DragDropInfo
    {
      public readonly int SourcePosition;
      public readonly GridView SourceView;
      public int DropRowHandle = -1;
      public EditMode EditMode;

      public DragDropInfo(GridView source, int sourcePosition)
      {
        this.SourceView = source;
        this.SourcePosition = sourcePosition;
      }
    }

    #endregion

    #region SaveSettings(), GetGridLayout()

    private void SaveSettings()
    {
      this.gviewRight.PostEditor();
      this.gviewLeft.PostEditor();

      Config.Default.WindowSize = Tools.Unscale(this.WindowState == FormWindowState.Normal ? this.ClientSize : this.RestoreBounds.Size, this.absScaleFactor);
      Config.Default.Encoding = this.defaultEncoding.WebName;
      Config.Default.Language = Thread.CurrentThread.CurrentUICulture.Name;
      Config.Default.LeftPanelWidth = this.splitContainerControl1.SplitterPosition.Unscale(this.absScaleFactor.Width);
      Config.Default.OutputListLayout = GetGridLayout(this.gviewLeft);
      if (this.CurrentChannelList != null)
        SaveInputGridLayout(this.CurrentChannelList.SignalSource);
      Config.Default.ShowWarningsAfterLoading = this.miShowWarningsAfterLoad.Checked;
      Config.Default.CloseGaps = this.cbCloseGap.Checked;
      Config.Default.MruFiles.Clear();
      Config.Default.MruFiles.AddRange(this.mruFiles);
      Config.Default.ExplorerIntegration = this.miExplorerIntegration.Down;
      Config.Default.CheckForUpdates = this.miCheckUpdates.Down;

      Config.Default.Save();
    }

    private string GetGridLayout(GridView grid)
    {
      var stream = new MemoryStream();
      grid.SaveLayoutToStream(stream, OptionsLayoutBase.FullLayout);
      stream.Seek(0, SeekOrigin.Begin);
      using (var rdr = new StreamReader(stream, Encoding.UTF8))
        return rdr.ReadToEnd();
    }

    #endregion

    #region ClearLeftFilter(), ClearRightFilter()

    private void ClearLeftFilter()
    {
      this.gviewLeft.BeginSort();
      this.gviewLeft.ClearColumnsFilter();
      this.colOutSlot.FilterInfo = new ColumnFilterInfo("[Position]<>-1");
      this.gviewLeft.EndSort();
    }

    private void ClearRightFilter()
    {
      this.gviewRight.BeginSort();
      this.gviewRight.ClearColumnsFilter();
      this.colSlotOld.FilterInfo = new ColumnFilterInfo("[OldProgramNr]<>-1");
      if (this.subListIndex > 0 && !this.CurrentChannelList.IsMixedSourceFavoritesList)
        this.colPrNr.FilterInfo = new ColumnFilterInfo("[NewProgramNr]<>-1");
      this.gviewRight.EndSort();
    }

    #endregion

    #region RefreshGrid()

    internal void RefreshGrids()
    {
      RefreshGrid(this.gviewLeft, this.gviewRight);
    }

    private void RefreshGrid(params GridView[] grids)
    {
      foreach (var grid in grids)
      {
        grid.BeginDataUpdate();
        grid.EndDataUpdate();
      }
    }

    #endregion

    #region Accessibility

    private void FocusRightList()
    {
      if (this.gviewRight.SelectedRowsCount > 0)
      {
        this.gviewRight.FocusedRowHandle = this.gviewRight.GetSelectedRows()[0];
        this.gridRight.Focus();
      }
    }

    private void FocusRightListFilter()
    {
      this.gviewRight.FocusedRowHandle = GridControl.AutoFilterRowHandle;
      this.gviewRight.FocusedColumn = this.colName;
      this.gridRight.Focus();
    }

    private void FocusLeftList()
    {
      if (this.gviewLeft.SelectedRowsCount > 0)
      {
        this.gviewLeft.FocusedRowHandle = this.gviewLeft.GetSelectedRows()[0];
        this.gridLeft.Focus();
      }
    }

    private void FocusLeftListFilter()
    {
      this.gviewLeft.FocusedRowHandle = GridControl.AutoFilterRowHandle;
      this.gviewLeft.FocusedColumn = this.colOutName;
      this.gridLeft.Focus();
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
      this.TryExecute(() => this.ShowOpenReferenceFileDialog(false));
    }

    private void miAddFromRefList_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(() => this.ShowOpenReferenceFileDialog(true));
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

    private void miSaveReferenceFile_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.SaveReferenceFile);
    }

    private void miExcelExport_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.ExportExcelList);
    }

    private void miPrint_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.Print);
    }

    private void miQuit_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.Close();
    }

    private void miRecentFiles_ListItemClick(object sender, ListItemClickEventArgs e)
    {
      TryExecute(() => this.LoadFiles(null, this.mruFiles[e.Index]));
    }

    #endregion

    #region Edit menu

    private void miCopyCsv_ItemClick(object sender, ItemClickEventArgs e)
    {
      var gview = this.gridRight.ContainsFocus ? this.gviewRight : this.gviewLeft;
      var cols = gview.VisibleColumns;

      var sb = new StringBuilder();
      foreach (GridColumn col in cols)
        sb.Append(col.Caption).Append('\t');
      sb[sb.Length - 1] = '\n';
      for (int i = 0, c = gview.RowCount; i < c; i++)
      {
        foreach (GridColumn col in cols)
        {
          if (col.ColumnType == typeof(bool))
          {
            var val = gview.GetRowCellValue(i, col);
            if (val is bool b && b)
              sb.Append('x');
            sb.Append('\t');
          }
          else
          {
            var val = gview.GetRowCellDisplayText(i, col);
            sb.Append(val).Append('\t');
          }
        }
        sb[sb.Length - 1] = '\n';
      }

      Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
    }

    private void miAddChannel_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.AddChannels);
    }

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

    private void miFavSet_ItemClick(object sender, ItemClickEventArgs e)
    {
      var fav = e.Item.Tag as string;
      this.SetFavorite(fav, true);
    }

    private void miFavUnset_ItemClick(object sender, ItemClickEventArgs e)
    {
      var fav = e.Item.Tag as string;
      this.SetFavorite(fav, false);
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

    #region TV-Set menu

    private void miTvCountrySetup_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.ShowTvCountrySettings);
    }

    private void miCleanupChannels_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.TryExecute(this.CleanupChannelData);
    }

    #endregion

    #region Character set menu

    private void MiUtf8Charset_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(() => this.SetDefaultEncoding(Encoding.UTF8));
    }

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

    #region Language menu

    private void miLanguage_DownChanged(object sender, ItemClickEventArgs e)
    {
      try
      {
        if (this.ignoreLanguageChange)
          return;
        var menuItem = (BarButtonItem)sender;
        if (!menuItem.Down)
          return;
        if (!this.PromptSaveAndContinue())
          return;
        var locale = (string)menuItem.Tag;
        Program.ChangeLanguage = true;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
        this.Close();
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    #endregion

    #region Help menu

    private void miWiki_ItemClick(object sender, ItemClickEventArgs e)
    {
      BrowserHelper.OpenUrl("https://github.com/PredatH0r/ChanSort/wiki");
    }

    private void miOpenWebsite_ItemClick(object sender, ItemClickEventArgs e)
    {
      BrowserHelper.OpenUrl("https://github.com/PredatH0r/ChanSort");
    }

    private void miAbout_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(() => new AboutForm(this.Plugins).ShowDialog());
    }

    #endregion

    #region Accessibility menu

    private void miInputSource_ItemClick(object sender, ItemClickEventArgs e)
    {
      this.tabChannelList.SelectedTabPageIndex = (int) e.Item.Tag;
    }

    private void miSelectFavList_ItemClick(object sender, ItemClickEventArgs e)
    {
      try
      {
        var idx = Convert.ToInt32(e.Item.Tag);
        if (this.grpSubList.Visible && idx < this.tabSubList.TabPages.Count)
          this.tabSubList.SelectedTabPageIndex = idx;
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    private void miGotoLeftFilter_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.FocusLeftListFilter);
    }

    private void miGotoLeftList_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.FocusLeftList);
    }

    private void miRightListFilter_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.FocusRightListFilter);
    }

    private void miGotoRightList_ItemClick(object sender, ItemClickEventArgs e)
    {
      TryExecute(this.FocusRightList);
    }

    private void miFont_DownChanged(object sender, ItemClickEventArgs e)
    {
      TryExecute(() =>
      {
        var deltaSize = (int)e.Item.Tag;
        if (!((BarButtonItem) e.Item).Down)
        {
          // reselect the current font size
          if (deltaSize == Config.Default.FontSizeDelta)
            ((BarButtonItem) e.Item).Down = true;
          return;
        }

        if (deltaSize == Config.Default.FontSizeDelta) // no change => early exit
          return;

        var font = new Font(deltaSize == 0 ? "Tahoma" : "Segoe UI", 8.25f + deltaSize);
        WindowsFormsSettings.DefaultFont = font;
        font = new Font("Segoe UI", 9 + deltaSize);
        WindowsFormsSettings.DefaultMenuFont = font;

        Config.Default.FontSizeDelta = deltaSize;
        foreach (var mi in new[] {miFontSmall, miFontMedium, miFontLarge, miFontXLarge, miFontXxLarge})
          mi.Down = e.Item == mi;
      });
    }

    #endregion

    #region miExplorerIntegration_ItemClick
    private void miExplorerIntegration_ItemClick(object sender, ItemClickEventArgs e)
    {
      try
      {
        if (this.miExplorerIntegration.Down == Config.Default.ExplorerIntegration)
          return;

        // get all file extensions from loader plugins
        var ext = new HashSet<string>();
        foreach (var loader in this.Plugins)
        {
          var filters = loader.FileFilter.Split(';');
          foreach (var filter in filters)
          {
            int i = filter.LastIndexOf('.');
            if (i >= 0 && i < filter.Length - 1)
              ext.Add(filter.Substring(i).ToLowerInvariant());
          }
        }

        if (this.miExplorerIntegration.Down)
          FileAssociations.CreateMissingAssociations(ext);
        else
          FileAssociations.DeleteAssociations(ext);

        this.SaveSettings();
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }
    #endregion

    #region miCheckUpdates_ItemClick
    private void miCheckUpdates_ItemClick(object sender, ItemClickEventArgs e)
    {
      try
      {
        if (this.miCheckUpdates.Down == Config.Default.CheckForUpdates)
          return;

        if (this.miCheckUpdates.Down)
          UpdateCheck.CheckForNewVersion();
        this.SaveSettings();
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }
    #endregion



    #region gview_MouseDown, gview_MouseUp, timerEditDelay_Tick, gview_ShowingEditor

    // these 4 event handler in combination override the default row-selection and editor-opening 
    // behavior of the grid control.

    private bool dontFocusClickedRow;

    private void gview_MouseDown(object sender, MouseEventArgs e)
    {
      var view = (GridView) sender;
      this.downHit = view.CalcHitInfo(e.Location);
      this.dragDropInfo = null;
      if (!view.IsDataRow(downHit.RowHandle))
        return;
      if (e.Button == MouseButtons.Left)
      {
        if (ModifierKeys == Keys.None)
        {
          if (downHit.RowHandle != view.FocusedRowHandle && !dontFocusClickedRow)
            SelectFocusedRow(view, downHit.RowHandle);
          this.timerEditDelay.Start();
        }
        else
        {
          if (ModifierKeys == Keys.Control && !view.IsRowSelected(downHit.RowHandle))
            this.BeginInvoke((Action) (() => view.SelectRow(downHit.RowHandle)));
        }
      }
      else if (e.Button == MouseButtons.Right)
      {
        if (!view.IsRowSelected(downHit.RowHandle))
          SelectFocusedRow(view, downHit.RowHandle);
      }

      this.dontOpenEditor = true;
      this.dontFocusClickedRow = false;
    }

    private void gview_MouseUp(object sender, MouseEventArgs e)
    {
      this.timerEditDelay.Stop();
      this.BeginInvoke((Action) (() => { this.dontOpenEditor = false; }));
    }

    private void timerEditDelay_Tick(object sender, EventArgs e)
    {
      this.timerEditDelay.Stop();
      this.dontOpenEditor = false;
      if (this.lastFocusedGrid != null)
      {
        var hit = this.lastFocusedGrid.CalcHitInfo(this.lastFocusedGrid.GridControl.PointToClient(MousePosition));
        if (hit.Column == this.lastFocusedGrid.FocusedColumn && hit.RowHandle == this.lastFocusedGrid.FocusedRowHandle)
          this.lastFocusedGrid.ShowEditor();
      }
    }

    private void gview_ShowingEditor(object sender, CancelEventArgs e)
    {
      var field = ((GridView) sender).FocusedColumn.FieldName;
      if (this.dontOpenEditor && (field == this.colSlotNew.FieldName || field == this.colName.FieldName))
        e.Cancel = true;
    }

    #endregion

    #region gview_ShownEditor, gview_KeyPress

    private void gview_ShownEditor(object sender, EventArgs e)
    {
      var view = (GridView) sender;
      if (view.FocusedRowHandle < 0)
        return;
      var edit = view.ActiveEditor as TextEdit;
      if (edit == null) return;
      edit.Properties.MaxLength = view.FocusedColumn.FieldName == "Name" ? this.CurrentChannelList.MaxChannelNameLength : 0;
    }

    private void gview_KeyPress(object sender, KeyPressEventArgs e)
    {
      var view = (GridView) sender;
      if (view.FocusedColumn.DisplayFormat.FormatType == FormatType.Numeric && (e.KeyChar < '0' || e.KeyChar > '9'))
        e.Handled = true;
    }

    #endregion

    #region gviewLeft_LayoutUpgrade, gviewRight_LayoutUpgrade

    private void gviewLeft_LayoutUpgrade(object sender, LayoutUpgradeEventArgs e)
    {
      this.gviewLeft.ClearGrouping();
      this.gviewLeft.OptionsCustomization.AllowGroup = false;
    }

    private void gviewRight_LayoutUpgrade(object sender, LayoutUpgradeEventArgs e)
    {
      this.gviewRight.ClearGrouping();
      this.gviewRight.OptionsCustomization.AllowGroup = false;
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

    #region btnToggleFav_Click, btnToggleLock_Click

    private void btnToggleFav_Click(object sender, EventArgs e)
    {
      var fav = ((Control) sender).Text.Substring(1);
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

    #region tabSubList_MouseUp
    private void tabSubList_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        var hit = this.tabSubList.CalcHitInfo(e.Location);
        if (hit.IsValid && hit.Page != null)
        {
          using var dlg = new TextInputForm();
          dlg.StartPosition = FormStartPosition.Manual;
          dlg.Location = this.tabSubList.PointToScreen(e.Location);
          var favIndex = this.tabSubList.TabPages.IndexOf(hit.Page) - 1;
          dlg.Text = this.DataRoot.GetFavListCaption(favIndex);
          if (dlg.ShowDialog(this) == DialogResult.OK)
          {
            this.DataRoot.SetFavListCaption(favIndex, dlg.Text);
            hit.Page.Text = this.DataRoot.GetFavListCaption(favIndex, true);
          }
        }
      }
    }
    #endregion
  }
}