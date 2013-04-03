using System;
using System.Collections.Generic;

namespace ChanSort.Api
{
  public class MappingPool<T> where T : DataMapping
  {
    private const string ERR_unknownACTChannelDataLength = "Configuration doesn't contain a {0} data mapping for length {1}";
    private readonly Dictionary<int, T> mappings = new Dictionary<int, T>();
    private readonly string caption;

    public MappingPool(string caption)
    {
      this.caption = caption;
    }

    public void AddMapping(int dataLength, T mapping) 
    {
      mappings[dataLength] = mapping;
    }

    public T GetMapping(int dataLength, bool throwException = true)
    {
      if (dataLength == 0)
        return null;

      T mapping;
      if (!mappings.TryGetValue(dataLength, out mapping) && throwException)
        throw new Exception(string.Format(ERR_unknownACTChannelDataLength, this.caption, dataLength));
      return mapping;
    }
  }
}
