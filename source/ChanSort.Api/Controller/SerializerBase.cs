using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ChanSort.Api
{
  public abstract class SerializerBase : IDisposable
  {
    #region class SupportedFeatures

    public enum DeleteMode
    {
      NotSupported = 0,
      Physically = 1,
      FlagWithoutPrNr = 2,
      FlagWithPrNr = 3
    }

    public class SupportedFeatures
    {
      public ChannelNameEditMode ChannelNameEdit { get; set; }
      public bool CleanUpChannelData { get; set; }
      public bool DeviceSettings { get; set; }
      public bool CanSkipChannels { get; set; } = true;
      public bool CanLockChannels { get; set; } = true;
      public bool CanHideChannels { get; set; } = true;
      public bool CanHaveGaps { get; set; } = true;
      public bool EncryptedFlagEdit { get; set; }
      public DeleteMode DeleteMode { get; set; } = DeleteMode.NotSupported;
      public bool CanSaveAs { get; set; } = true;


      public Favorites SupportedFavorites { get; set; } = Favorites.A | Favorites.B | Favorites.C | Favorites.D;
      public bool SortedFavorites { get; set; }
      public bool MixedSourceFavorites { get; set; }
      public bool AllowGapsInFavNumbers { get; set; }
      public bool CanEditFavListNames { get; set; }
    }
    #endregion

    private Encoding defaultEncoding;

    public string FileName { get; protected set; }
    public DataRoot DataRoot { get; protected set; }
    public SupportedFeatures Features { get; } = new SupportedFeatures();

    protected SerializerBase(string inputFile)
    {
      this.FileName = inputFile;
      this.defaultEncoding = Encoding.GetEncoding("iso-8859-9");
      this.DataRoot = new DataRoot(this);
    }

    public abstract void Load();
    public abstract void Save(string tvOutputFile);

    public virtual Encoding DefaultEncoding
    {
      get { return this.defaultEncoding; }
      set { this.defaultEncoding = value; }
    }

    #region GetDataFilePaths
    /// <summary>
    /// returns the list of all data files that need to be copied for backup/restore
    /// </summary>
    public virtual IEnumerable<string> GetDataFilePaths()
    {
      return new List<string> { this.FileName };
    }
    #endregion

    #region GetFileInformation()
    public virtual string GetFileInformation() 
    { 
      StringBuilder sb = new StringBuilder();
      sb.Append("File name: ").AppendLine(this.FileName);
      sb.AppendLine();
      foreach (var list in this.DataRoot.ChannelLists)
      {
        sb.Append(list.ShortCaption).AppendLine("-----");
        sb.Append("number of channels: ").AppendLine(list.Count.ToString());
        sb.Append("number of predefined channel numbers: ").AppendLine(list.PresetProgramNrCount.ToString());
        sb.Append("number of duplicate program numbers: ").AppendLine(list.DuplicateProgNrCount.ToString());
        sb.Append("number of duplicate channel identifiers: ").AppendLine(list.DuplicateUidCount.ToString());
        int deleted = 0;
        int hidden = 0;
        int skipped = 0;
        int locked = 0;
        foreach (var channel in list.Channels)
        {
          if (channel.IsDeleted)
            ++deleted;
          if (channel.Hidden)
            ++hidden;
          if (channel.Skip)
            ++skipped;
          if (channel.Lock)
            ++locked;
        }
        sb.Append("number of deleted channels: ").AppendLine(deleted.ToString());
        sb.Append("number of hidden channels: ").AppendLine(hidden.ToString());
        sb.Append("number of skipped channels: ").AppendLine(skipped.ToString());
        sb.Append("number of locked channels: ").AppendLine(locked.ToString());
        sb.AppendLine();
      }
      return sb.ToString(); 
    }
    #endregion

    public virtual void ShowDeviceSettingsForm(object parentWindow) { }

    public virtual string CleanUpChannelData() { return ""; }


    // common implementation helper methods

    protected string TempPath { get; set; }

    #region UnzipToTempFolder(), ZipToOutputFile()

    protected void UnzipFileToTempFolder()
    {
      this.DeleteTempPath();
      this.TempPath = Path.Combine(Path.GetTempPath(), "ChanSort_" + Path.GetRandomFileName());

      if (Directory.Exists(this.TempPath))
        Directory.Delete(this.TempPath, true);
      Directory.CreateDirectory(this.TempPath);
      ZipFile.ExtractToDirectory(this.FileName, this.TempPath);
    }

    protected void ZipToOutputFile(string tvOutputFile)
    {
      File.Delete(tvOutputFile);
      ZipFile.CreateFromDirectory(this.TempPath, tvOutputFile);
      this.FileName = tvOutputFile;
    }
    #endregion

    #region IDisposable

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~SerializerBase()
    {
      this.Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      this.DeleteTempPath();
    }

    #endregion

    #region DeleteTempPath()
    protected void DeleteTempPath()
    {
      var path = this.TempPath;
      if (string.IsNullOrEmpty(path))
        return;
      try
      {
        if (Directory.Exists(path))
          Directory.Delete(path, true);
        else if (File.Exists(path))
          File.Delete(path);
      }
      catch
      {
        // ignore
      }
    }
    #endregion

    #region ParseInt()
    protected int ParseInt(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return 0;
      if (input.Length > 2 && input[0] == '0' && char.ToLower(input[1]) == 'x')
        return int.Parse(input.Substring(2), NumberStyles.HexNumber);
      if (int.TryParse(input, out var value))
        return value;
      return 0;
    }
    #endregion

    #region ParseInt()
    protected long ParseLong(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return 0;
      if (input.Length > 2 && input[0] == '0' && char.ToLower(input[1]) == 'x')
        return long.Parse(input.Substring(2), NumberStyles.HexNumber);
      if (long.TryParse(input, out var value))
        return value;
      return 0;
    }
    #endregion
  }
}
