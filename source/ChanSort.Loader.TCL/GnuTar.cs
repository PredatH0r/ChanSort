using System.Text;

namespace ChanSort.Loader.TCL;

/// <summary>
/// Minimal implementation to support the contents of TCL's .tar.
/// Reading supports all "ustar" formats including old-GNU and POSIX 1990 .tar flavors.
/// Saving uses the "old-GNU" format, that's used by TCL.
///
/// Unlike all tools and libraries available under Windows, this implementation preserves unix metadata like:
/// file mode, user-id, group-id, user name, group name and device major/minor number
///
/// Information about GNU tar can be found on https://www.gnu.org/software/tar/manual/html_node/Standard.html 
/// </summary>
internal class GnuTar
{
  private static readonly DateTime Epoc = new (1970, 1, 1);
  private static readonly byte[] Padding = new byte[512];

  public Encoding Encoding { get; set; } = Encoding.UTF8;
  public List<TarEntry> Entries { get; } = new();

  #region ExtractToDirectory()
  public void ExtractToDirectory(string tarPath, string targetDir)
  {
    var data = File.ReadAllBytes(tarPath);
    this.Read(data);

    foreach (var entry in this.Entries)
    {
      if (entry.TypeFlag == TarEntryTypes.Directory)
      {
        entry.Path = Path.Combine(targetDir, entry.Name.TrimEnd('/', '\\'));
        Directory.CreateDirectory(entry.Path);
      }
      else if (entry.TypeFlag == TarEntryTypes.File || entry.TypeFlag == TarEntryTypes.File0)
      {
        var path = Path.Combine(targetDir, entry.Name);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var outStream = new FileStream(path, FileMode.Create))
          outStream.Write(data, entry.Position, entry.Size);
        entry.Path = path;
        File.SetLastWriteTimeUtc(path, entry.LastModified);
      }
      else
        throw new NotImplementedException($"unsupported tar entry type {(char)entry.TypeFlag} for {entry.Name}");
    }
  }
  #endregion

  #region Read()
  public void Read(string path)
  {
    var stream = new FileStream(path, FileMode.Open);
    Read(stream, false);
  }

  public void Read(byte[] data)
  {
    using var stream = new MemoryStream(data);
    Read(stream);
  }

  public void Read(Stream stream, bool keepOpen = true)
  {
    try
    {
      Entries.Clear();
      var header = new byte[512];
      var memStream = stream as MemoryStream;
      while (true)
      {
        var len = stream.Read(header, 0, 512);
        if (len < 512)
          break;

        if (header.All(b => b == 0)) // end block is all-zero
          break;

        var position = stream.Position;
        var entry = ReadEntryHeader(header);
        entry.Data = memStream?.ToArray();
        entry.Position = (int)position;
        Entries.Add(entry);

        var extra = entry.Size % 512;
        if (extra != 0)
          extra = 512 - extra;
        stream.Seek(entry.Size + extra, SeekOrigin.Current);
      }
    }
    finally
    {
      if (!keepOpen)
        stream.Dispose();
    }
  }
  #endregion

  #region ReadEntryHeader()
  private TarEntry ReadEntryHeader(byte[] header)
  {
    var e = new TarEntry();
    e.Name = ReadString(header, 0, 100, this.Encoding);
    e.Mode = (ushort)ReadNumber(header, 100, 8);
    e.UserId = ReadNumber(header, 108, 8);
    e.GroupId = ReadNumber(header, 116, 8);
    e.Size = ReadNumber(header, 124, 12);
    var mtime = (uint)ReadNumber(header, 136, 12);
    e.LastModified = Epoc.AddSeconds(mtime);
    e.Checksum = (uint)ReadNumber(header, 148, 8);
    e.TypeFlag = (TarEntryTypes)header[156];
    e.LinkName = ReadString(header, 157, 100, this.Encoding);

    e.Magic = ReadString(header, 257, 6);
    e.Version = ReadString(header, 263, 2);
    if (e.Magic != "ustar" && !(e.Magic == "ustar " && e.Version == " "))
      throw new InvalidOperationException("not a POSIX/GNU or old-GNU tar");
    e.Username = ReadString(header, 265, 32, this.Encoding);
    e.Groupname = ReadString(header, 297, 32, this.Encoding);
    e.DeviceMajor = ReadOptionalNumber(header, 329, 8);
    e.DeviceMinor = ReadOptionalNumber(header, 337, 8);
    e.Prefix = ReadString(header, 345, 155, this.Encoding);

    return e;
  }
  #endregion

  #region ReadString(), ReadNumber()
  private string ReadString(byte[] data, int offset, int length, Encoding encoding = null)
  {
    encoding ??= Encoding.ASCII;
    int idx = Array.IndexOf(data, (byte)0, offset);
    if (idx == 0)
      idx = data.Length;
    if (idx > 0)
      length = Math.Min(length, idx - offset);
    return encoding.GetString(data, offset, length);
  }
  
  private int ReadNumber(byte[] data, int offset, int length)
  {
    var val = ReadOptionalNumber(data, offset, length);
    return val ?? 0;
  }

  private int? ReadOptionalNumber(byte[] data, int offset, int length)
  {
    var nr = ReadString(data, offset, length).TrimEnd(' ');
    return nr.Length == 0 ? null : Convert.ToInt32(nr, 8);
  }

  #endregion


  #region UpdateFromDirectory()
  public void UpdateFromDirectory(string tarPath)
  {
    using var outStream = new FileStream(tarPath, FileMode.Create);
    using var memStream = new MemoryStream(512);
    foreach (var entry in this.Entries)
    {
      byte[] data = null;
      if (entry.TypeFlag == TarEntryTypes.Directory)
      {
        if (!Directory.Exists(entry.Path))
          continue;
      }
      else
      {
        var info = new FileInfo(entry.Path);
        if (!info.Exists)
          continue;

        data = File.ReadAllBytes(entry.Path);
        entry.Size = data.Length;
        entry.LastModified = info.LastWriteTimeUtc;
      }

      // prepare header in a memory stream and patch checksum into it
      memStream.Seek(0, SeekOrigin.Begin);
      WriteEntryHeader(entry, memStream);
      entry.Checksum = CalcChecksum(memStream.GetBuffer());
      memStream.Seek(148, SeekOrigin.Begin);
      WriteNumber(memStream, (ushort)entry.Checksum, 7);
      outStream.Write(memStream.GetBuffer(), 0, 512);

      // write file data
      if (data != null)
        outStream.Write(data, 0, data.Length);
      
      // padding zeros to 512
      var padlen = entry.Size % 512;
      if (padlen != 0)
        outStream.Write(Padding, 0, 512 - padlen);
    }

    // end-of-file marker: 2x 512 byte blocks with all zeros
    outStream.Write(Padding, 0, Padding.Length);
    outStream.Write(Padding, 0, Padding.Length);
  }
  #endregion

  #region CalcChecksum()
  private uint CalcChecksum(byte[] data)
  {
    uint sum = 0;
    int count = data.Length;
    for (int i=0; count>0; i++, count--)
      sum += data[i];
    return sum;
  }
  #endregion

  #region WriteEntryHeader()
  private void WriteEntryHeader(TarEntry e, Stream strm)
  {
    WriteString(strm, e.Name, 100, this.Encoding);
    WriteNumber(strm, e.Mode, 8);
    WriteNumber(strm, e.UserId, 8);
    WriteNumber(strm, e.GroupId, 8);
    WriteNumber(strm, e.Size, 12);
    WriteNumber(strm, (int)(e.LastModified - Epoc).TotalSeconds, 12);
    strm.Write("        "u8.ToArray(), 0, 8); // placeholder for checksum
    strm.WriteByte((byte)e.TypeFlag);
    WriteString(strm, e.LinkName, 100, this.Encoding);
    var bytes = Encoding.ASCII.GetBytes(e.Magic);
    strm.Write(bytes, 0, Math.Min(bytes.Length, 6));
    strm.Write(Padding, 0, 6 - bytes.Length);
    WriteString(strm, e.Version, 2);
    WriteString(strm, e.Username, 32);
    WriteString(strm, e.Groupname, 32);
    WriteNumber(strm, e.DeviceMajor, 8);
    WriteNumber(strm, e.DeviceMinor, 8);
    WriteString(strm, e.Prefix, 155, this.Encoding);
    strm.Write(Padding, 0, 512 - 500);
  }
  #endregion

  #region WriteString()
  private void WriteString(Stream strm, string str, int length, Encoding enc = null)
  {
    enc ??= Encoding.UTF8;
    var bytes = enc.GetBytes(str);
    if (bytes.Length >= length)
    {
      strm.Write(bytes, 0, length-1);
      strm.WriteByte(0);
      return;
    }
    
    strm.Write(bytes, 0, bytes.Length);
    strm.Write(Padding, 0, length - bytes.Length);
  }
  #endregion

  #region WriteNumber()
  private void WriteNumber(Stream strm, int number, int length)
  {
    var str = Convert.ToString((uint)number, 8);
    if (str.Length >= length)
      throw new ArgumentException($"{number} is too long for {length} octal digits");
    for (int i=length - str.Length - 1; i>0; i--)
      strm.WriteByte((byte)'0');
    var bytes = Encoding.ASCII.GetBytes(str);
    strm.Write(bytes, 0, bytes.Length);
    strm.WriteByte(0);
  }

  private void WriteNumber(Stream strm, int? number, int length)
  {
    if (number.HasValue)
      WriteNumber(strm, number.Value, length);
    else
      WriteString(strm, "", length);
  }
  #endregion
}

#region enum TarEntryTypes
public enum TarEntryTypes : byte
{
  File0 = (byte)'\0',
  File = (byte)'0',
  Link = (byte)'1',
  Sym = (byte)'2',
  CharDevice = (byte)'3',
  BlockDevice = (byte)'4',
  Directory = (byte)'5',
  FiFo = (byte)'6',
  Cont = (byte)'7',
  ExtendedFileHeader = (byte)'x',
  GlobalHeader = (byte)'g'
}
#endregion

#region class TarEntry
class TarEntry
{
  // original tar V7
  public string Name { get; set; }
  public ushort Mode { get; set; }
  public int UserId { get; set; }
  public int GroupId { get; set; }
  public int Size { get; set; }
  public DateTime LastModified { get; set; }
  public uint Checksum { get; set; }
  public TarEntryTypes TypeFlag { get; set; }
  public string LinkName { get; set; }
    
  // UStar (POSIX 1003.1-1990) / GNU / old-GNU
  public string Magic { get; set; }
  public string Version { get; set; }
  public string Username { get; set; }
  public string Groupname { get; set; }
  public int? DeviceMajor { get; set; }
  public int? DeviceMinor { get; set; }
  public string Prefix { get; set; }

  // internal
  public byte[] Data { get; set; }
  public int Position { get; set; }
  public string Path { get; set; }
}
#endregion