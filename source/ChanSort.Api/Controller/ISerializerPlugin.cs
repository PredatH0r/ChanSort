namespace ChanSort.Api
{
  public interface ISerializerPlugin
  {
    string DllName { get; set; }

    /// <summary>
    /// Name of the plugin, as displayed in the OpenFileDialog file-type selection combo box
    /// </summary>
    string PluginName { get; }

    /// <summary>
    /// Semicolon separated list of supported file types (e.g. "xxLM*.TTL;xxLV*.TTL")
    /// </summary>
    string FileFilter { get; }

    /// <summary>
    /// Create an object that can read/write the file
    /// </summary>
    /// <exception cref="System.IO.IOException">file is not of any supported type</exception>
    SerializerBase CreateSerializer(string inputFile);
  }
}
