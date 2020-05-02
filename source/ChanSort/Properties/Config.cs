using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace ChanSort.Ui.Properties
{
  public class Config
  {
    private static readonly XmlSerializer Serializer;
    private static readonly string ConfigFilePath;

    #region static ctor()
    static Config()
    {
      Serializer = new XmlSerializer(typeof(Config));

      try
      {
        ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChanSort", "config.xml");
        if (File.Exists(ConfigFilePath))
        {
          using (var stream = new StreamReader(ConfigFilePath, System.Text.Encoding.UTF8))
            Default = (Config)Serializer.Deserialize(stream);
          return;
        }
      }
      catch
      {
        // ignore
      }

      Default = new Config();
    }
    #endregion

    public static Config Default { get; set; }

    public string OutputListLayout { get; set; } = "";
    public string Language { get; set; } = "";
    public string Encoding { get; set; } = "";
    public Size WindowSize { get; set; } = new Size(0,0);
    public string InputGridLayoutAnalog { get; set; } = "";
    public string InputGridLayoutDvbCT { get; set; } = "";
    public string InputGridLayoutDvbS { get; set; } = "";
    public int LeftPanelWidth { get; set; } = 0;
    public bool ShowWarningsAfterLoading { get; set; } = false;
    public bool CloseGaps { get; set; } = true;
    [XmlArray("MruFiles")]
    public List<string> MruFiles { get; set; } = new List<string>();
    public string PrintFontName { get; set; } = "Segoe UI";
    public decimal PrintFontSize { get; set; } = 12;
    public bool PrintSortByName { get; set; } = false;
    public int PrintColumnCount { get; set; } = 2;
    public bool ExplorerIntegration { get; set; } = false;
    public bool CheckForUpdates { get; set; } = true;
    public int FontSizeDelta { get; set; }

    public void Save()
    {
      var folder = Path.GetDirectoryName(ConfigFilePath);
      Directory.CreateDirectory(folder);

      using (var stream = new FileStream(ConfigFilePath, FileMode.Create))
      using (var writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
      {
        Serializer.Serialize(writer, this);
      }
    }
  }
}
