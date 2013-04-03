using System.Text;

namespace ChanSort.Api
{
  public abstract class SerializerBase
  {
    public class SupportedFeatures
    {
      public bool EraseChannelData { get; set; }
      public bool ChannelNameEdit { get; set; }
      public bool FileInformation { get; set; }

      public bool DeviceSettings { get; set; }
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
    public abstract void Save(string tvOutputFile, string csvOutputFile);

    public virtual Encoding DefaultEncoding
    {
      get { return this.defaultEncoding; }
      set { this.defaultEncoding = value; }
    }

    public virtual void EraseChannelData() { }

    public virtual string GetFileInformation() { return ""; }

    public virtual void ShowDeviceSettingsForm(object parentWindow) { }
  }
}
