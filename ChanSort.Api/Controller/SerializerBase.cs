using System.Text;

namespace ChanSort.Api
{
  public abstract class SerializerBase
  {
    private Encoding defaultEncoding;

    public string FileName { get; set; }
    public DataRoot DataRoot { get; protected set; }

    protected SerializerBase(string inputFile)
    {
      this.FileName = inputFile;
      this.DataRoot = new DataRoot();
      this.defaultEncoding = Encoding.GetEncoding("iso-8859-9");
    }

    public abstract string DisplayName { get; }
    public abstract void Load();
    public abstract void Save(string tvOutputFile, string csvOutputFile, UnsortedChannelMode unsortedChannelMode);

    public virtual Encoding DefaultEncoding
    {
      get { return this.defaultEncoding; }
      set { this.defaultEncoding = value; }
    }

    public bool SupportsEraseChannelData { get; protected set; }
    public virtual void EraseChannelData() { }

    public virtual string GetFileInformation() { return ""; }

    public virtual void ShowDeviceSettingsForm(object parentWindow) { }

    public bool SupportsChannelNameEdit { get; protected set; }
  }
}
