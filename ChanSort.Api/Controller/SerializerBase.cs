using System.Text;

namespace ChanSort.Api
{
  public abstract class SerializerBase
  {
    public class SupportedFeatures
    {
      public bool ChannelNameEdit { get; set; }
      public bool CleanUpChannelData { get; set; }
      public bool DeviceSettings { get; set; }
      public bool CanDeleteChannels { get; set; }

      public SupportedFeatures()
      {
        this.CanDeleteChannels = true;
      }
    }

    private Encoding defaultEncoding;

    public string FileName { get; set; }
    public DataRoot DataRoot { get; protected set; }
    public SupportedFeatures Features { get; private set; }

    protected SerializerBase(string inputFile)
    {
      this.Features = new SupportedFeatures();
      this.FileName = inputFile;
      this.DataRoot = new DataRoot();
      this.defaultEncoding = Encoding.GetEncoding("iso-8859-9");
    }

    public abstract string DisplayName { get; }
    public abstract void Load();
    public abstract void Save(string tvOutputFile);

    public virtual Encoding DefaultEncoding
    {
      get { return this.defaultEncoding; }
      set { this.defaultEncoding = value; }
    }

    public virtual void EraseChannelData() { }

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
        foreach (var channel in list.Channels)
        {
          if (channel.IsDeleted)
            ++deleted;
          if (channel.Hidden)
            ++hidden;
          if (channel.Skip)
            ++skipped;
        }
        sb.Append("number of deleted channels: ").AppendLine(deleted.ToString());
        sb.Append("number of hidden channels: ").AppendLine(hidden.ToString());
        sb.Append("number of skipped channels: ").AppendLine(skipped.ToString());
        sb.AppendLine();
      }
      return sb.ToString(); 
    }

    public virtual void ShowDeviceSettingsForm(object parentWindow) { }

    public virtual string CleanUpChannelData() { return ""; }
  }
}
