using System.Collections.Generic;

namespace ChanSort.Api
{
  public class MappingPool<T> where T : DataMapping
  {
    private const string ERR_unknownACTChannelDataLength = "Configuration doesn't contain a {0} data mapping for length {1}";
    private readonly Dictionary<string, T> mappings = new Dictionary<string, T>();
    private readonly string caption;
    public System.Text.Encoding DefaultEncoding { get; set; }

    public MappingPool(string caption)
    {
      this.caption = caption;
    }

    public void AddMapping(int dataLength, T mapping)
    {
      this.AddMapping(dataLength.ToString(), mapping);
    }

    public void AddMapping(string id, T mapping)
    {
      this.mappings.Add(id, mapping);
    }

    public T GetMapping(int dataLength, bool throwException = true)
    {
      return this.GetMapping(dataLength.ToString(), throwException);
    }

    public T GetMapping(string id, bool throwException = true)
    {
      if (id == "0" || string.IsNullOrEmpty(id))
        return null;

      T mapping;
      if (!mappings.TryGetValue(id, out mapping) && throwException)
        throw LoaderException.Fail(string.Format(ERR_unknownACTChannelDataLength, this.caption, id));

      if (mapping != null && this.DefaultEncoding != null)
        mapping.DefaultEncoding = this.DefaultEncoding;
      return mapping;
    }
  }
}
