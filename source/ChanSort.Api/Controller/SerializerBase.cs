using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace ChanSort.Api
{
  public abstract class SerializerBase
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
      public bool CanHaveGaps { get; set; } = true;
      public bool EncryptedFlagEdit { get; set; }
      public DeleteMode DeleteMode { get; set; } = DeleteMode.NotSupported;


      public Favorites SupportedFavorites { get; set; } = Favorites.A | Favorites.B | Favorites.C | Favorites.D;
      public bool SortedFavorites { get; set; }
      public bool MixedSourceFavorites { get; set; }
      public bool AllowGapsInFavNumbers { get; set; }

      public bool CanDeleteChannelsWithFlag => this.DeleteMode == DeleteMode.FlagWithPrNr || this.DeleteMode == DeleteMode.FlagWithoutPrNr;
      public bool CanDeleteChannelsFromFile => this.DeleteMode == DeleteMode.Physically;
      public bool DeletedChannelsNeedNumbers => this.DeleteMode == DeleteMode.FlagWithPrNr;

    }
    #endregion

    private Encoding defaultEncoding;

    public string FileName { get; set; }
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

    protected string UnzipFileToTempFolder()
    {
      var tempDir = this.FileName + ".tmp";

      if (Directory.Exists(tempDir))
        Directory.Delete(tempDir, true);
      Directory.CreateDirectory(tempDir);
      ZipFile.ExtractToDirectory(this.FileName, tempDir);
      this.DeleteOnExit(tempDir);
      return tempDir;
    }

    protected void ZipToOutputFile(string tvOutputFile)
    {
      var tempDir = this.FileName + ".tmp";
      File.Delete(tvOutputFile);
      ZipFile.CreateFromDirectory(tempDir, tvOutputFile);
      this.FileName = tvOutputFile;
    }

    // TODO: replace this with a SerializerBase implementing IDisposable
    protected virtual void DeleteOnExit(string fileOrFolder)
    {
      Application.ApplicationExit += (sender, args) =>
      {
        try
        {
          if (Directory.Exists(fileOrFolder))
            Directory.Delete(fileOrFolder, true);
          else if (File.Exists(fileOrFolder))
            File.Delete(fileOrFolder);
        }
        catch
        {
          // ignore
        }
      };
    }
  }
}
