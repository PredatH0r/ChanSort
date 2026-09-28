using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.MediaTek;

/// <summary>
/// Native MediaTek service list ("gfs_Svl_102" on BRAVIA 8 II, "ffs_Svl_101" in older Sony exports) inside the Java serialized &lt;internal&gt;&lt;service_database&gt;.
/// Sony BRAVIA 8 II ignores &lt;major_channel_number&gt; and only uses this binary table.
/// This class only modifies an existing TV export (renumber + re-sort), it cannot create records.
/// </summary>
internal class SvlTable
{
  /*
   * Layout (all values big endian):
   *
   * Java TC_ARRAY length (u32)
   * container: "ZI\x04\x32" u32 totalLen ... [+0x17] u24 (totalLen - 0x400) ... (0x400 bytes)
   *
   * outer object:     0D B0 AB CD  21  00 27                         ┐
   * outer meta:       00 "gfs_Svl_102" 00...  u32 cdbBlockLen        ┘ CRC Y
   * cdb block:        0C DB 0C DB  07  LL LL                         ┐
   * cdb meta:         LLLL bytes, last u32 = dataSectionLen          │ CRC X, continued over
   * data section:     "cl_Zip" + 10x00                               │ the deflate payloads only
   *                   n x [flag(1) compLen(4) rawLen(4) payload]     ┘ (not over "cl_Zip" and chunk headers)
   *                   X (u32)  F3 24 F3 24
   *                   Y (u32)  F2 4F 54 32
   *
   * CRC for X and Y: Crc32.Normal (MSB first, poly 04C11DB7, init FFFFFFFF, no final XOR) = CRC-32/MPEG-2
   *
   * chunks: raw deflate, flag 0x81 = records (65 x 504 bytes per chunk), flag 0x82 = name pool (single chunk)
   *
   * record (504 bytes):
   * +4  u16 record_id (same as in the XML record_id)
   * +6  u16 program number << 2 (lower 2 bits must be kept)
   * +9  u8  group: 0x81 = TV, 0x82 = radio, 0x83 = data
   * +22 u32 record hash (x31 hash, adjusted by delta(+6) * 31^6)
   * +26 u16 service id
   * +42 u16 1-based index into the name pool (0 = no name)
   *
   * name pool: u32 4*count, u32 stringsLen, u32 offset[count], strings (u16 len + bytes), in record order
   */

  public const int RecordSize = 504;
  private const int ChunkSize = 32768 / RecordSize * RecordSize;
  private const uint HashFactor = 887503681; // 31^6

  private static readonly byte[] SvlTableName = Encoding.ASCII.GetBytes("fs_Svl_10"); // gfs_Svl_102, ffs_Svl_101
  private static readonly byte[] CdbMagic = [0x0C, 0xDB, 0x0C, 0xDB];
  private static readonly byte[] ZipMagic = Encoding.ASCII.GetBytes("cl_Zip");
  private static readonly byte[] ContainerMagic = [0x5A, 0x49, 0x04, 0x32];

  private byte[] data;
  private int namePos;     // "gfs_Svl_102" / "ffs_Svl_101"
  private int cdbPos;      // 0C DB 0C DB
  private int zipPos;      // "cl_Zip"
  private int chainStart;  // first chunk header
  private int chainEnd;    // X
  private int containerPos;

  public readonly List<byte[]> Records = new();
  private readonly List<byte[]> names = new(); // raw name pool entry per record, null = no name

  #region TryLoad()
  public static SvlTable TryLoad(byte[] serviceDatabase)
  {
    var svl = new SvlTable();
    return svl.Load(serviceDatabase) ? svl : null;
  }
  #endregion

  #region Load()
  private bool Load(byte[] serviceDatabase)
  {
    this.data = serviceDatabase;
    this.namePos = IndexOf(data, SvlTableName, 0) - 1;
    if (namePos < 8)
      return false;
    this.cdbPos = IndexOf(data, CdbMagic, namePos);
    this.zipPos = IndexOf(data, ZipMagic, cdbPos);
    this.containerPos = LastIndexOf(data, ContainerMagic, namePos - 16);
    if (cdbPos < 0 || zipPos < 0 || containerPos < 4)
      return false;

    // inflate all chunks
    var raw = new MemoryStream();
    int poolStart = -1;
    int off = zipPos + 16;
    this.chainStart = off;
    while (true)
    {
      var flag = data[off];
      var compLen = data.GetInt32(off + 1, false);
      var rawLen = data.GetInt32(off + 5, false);
      if (flag == 0x82)
        poolStart = (int)raw.Length;
      var chunk = Inflate(data, off + 9, compLen);
      if (chunk.Length != rawLen)
        throw LoaderException.Fail("Svl: invalid chunk length");
      raw.Write(chunk, 0, chunk.Length);
      off += 9 + compLen;
      if (flag != 0x81)
        break;
    }
    this.chainEnd = off;

    if (!VerifyChecksums())
      throw LoaderException.Fail("Svl: invalid checksum");

    var svl = raw.ToArray();
    if (poolStart < 0 || poolStart % RecordSize != 0)
      throw LoaderException.Fail("Svl: missing name pool");
    var pool = ParsePool(svl, poolStart);
    for (int i = 0; i < poolStart / RecordSize; i++)
    {
      var rec = new byte[RecordSize];
      Array.Copy(svl, i * RecordSize, rec, 0, RecordSize);
      this.Records.Add(rec);
      var nameIdx = rec.GetInt16(42, false);
      this.names.Add(nameIdx == 0 ? null : pool[nameIdx - 1]);
    }
    return true;
  }
  #endregion

  #region ParsePool()
  private static List<byte[]> ParsePool(byte[] svl, int start)
  {
    var count = svl.GetInt32(start, false) / 4;
    var stringsStart = start + 8 + count * 4;
    var list = new List<byte[]>();
    for (int i = 0; i < count; i++)
    {
      var from = stringsStart + svl.GetInt32(start + 8 + i * 4, false);
      var to = i + 1 < count ? stringsStart + svl.GetInt32(start + 12 + i * 4, false) : svl.Length;
      list.Add(svl.Skip(from).Take(to - from).ToArray());
    }
    return list;
  }
  #endregion

  #region record accessors
  // All accessors read big endian explicitly (Tools.GetInt16/32 with littleEndian=false), so the result does not
  // depend on the platform (.NET Framework / .NET, Windows / macOS, x86 / ARM).

  /// <summary>record_id at +4, same value as the last segment of the XML &lt;record_id&gt;</summary>
  public static int GetRecordId(byte[] rec) => rec.GetInt16(4, false) & 0xFFFF;

  /// <summary>
  /// Program number as shown on the TV (= XML &lt;major_channel_number&gt;): u16 at +6, shifted right by 2.
  /// Example: 00 7c -> 124 -> 31, 00 80 -> 128 -> 32. Range 0..16383 (14 bits); the lower 2 bits were 0 in all observed records.
  /// Numbers are unique per group (+9), not over the whole table: the BRAVIA 8 II DVB-S export uses separate ranges
  /// (TV from 1, radio from 7201, data from 7587), ChanSort's DVB-C test file numbers each group from 1.
  /// Records are stored sorted by (number, group).
  /// </summary>
  public static int GetProgramNr(byte[] rec) => (rec.GetInt16(6, false) & 0xFFFF) >> 2;

  /// <summary>group at +9: 0x81 = TV, 0x82 = radio, 0x83 = data (decides the list, not sdt_service_type)</summary>
  public static int GetGroup(byte[] rec) => rec[9];

  /// <summary>DVB service ID at +26 (not contained in the Sony XML)</summary>
  public static int GetServiceId(byte[] rec) => rec.GetInt16(26, false) & 0xFFFF;

  /// <summary>
  /// Sets the program number (keeping the lower 2 bits) and adjusts the record hash at +22 by delta(raw) * 31^6.
  /// Only valid numbers 1..16383 are accepted; ChanSort's -1 for "no new number" must be resolved by the caller.
  /// </summary>
  public static void SetProgramNr(byte[] rec, int nr)
  {
    if (nr < 1 || nr > 0x3FFF)
      throw LoaderException.Fail($"Svl: invalid program number {nr} for record_id {GetRecordId(rec)}");
    var oldRaw = rec.GetInt16(6, false) & 0xFFFF;
    var newRaw = (nr << 2) | (oldRaw & 3);
    rec.SetInt16(6, newRaw, false);
    var hash = (uint)rec.GetInt32(22, false);
    hash = unchecked(hash + (uint)(newRaw - oldRaw) * HashFactor);
    rec.SetInt32(22, (int)hash, false);
  }
  #endregion

  #region Save()
  /// <summary>
  /// Sorts the records by (program number, group) like the TV does, rebuilds the name pool, recompresses the table,
  /// updates all length fields and checksums. Returns the record_ids in their new order so that the caller can
  /// reorder the XML &lt;service_info&gt; elements accordingly (they must match 1:1).
  /// </summary>
  public List<int> Save(out byte[] serviceDatabase)
  {
    var order = Enumerable.Range(0, Records.Count).OrderBy(i => GetProgramNr(Records[i])).ThenBy(i => GetGroup(Records[i])).ToList();
    // numbers are unique per group only: e.g. a DVB-C export numbers TV, radio and data each from 1
    var dup = order.GroupBy(i => (GetGroup(Records[i]), GetProgramNr(Records[i]))).FirstOrDefault(g => g.Count() > 1);
    if (dup != null)
      throw LoaderException.Fail($"Svl: program number {dup.Key.Item2} is used by record_ids {string.Join(", ", dup.Select(i => GetRecordId(Records[i])))} in group 0x{dup.Key.Item1:x2}");

    // records and name pool in new order
    var recData = new MemoryStream();
    var poolEntries = new List<byte[]>();
    foreach (var i in order)
    {
      var rec = Records[i];
      if (names[i] == null)
        rec.SetInt16(42, 0, false);
      else
      {
        poolEntries.Add(names[i]);
        rec.SetInt16(42, poolEntries.Count, false);
      }
      recData.Write(rec, 0, RecordSize);
    }

    // compressed chunk chain
    var chain = new MemoryStream();
    var recBytes = recData.ToArray();
    for (int off = 0; off < recBytes.Length; off += ChunkSize)
      WriteChunk(chain, 0x81, recBytes, off, Math.Min(ChunkSize, recBytes.Length - off));
    var pool = BuildPool(poolEntries);
    WriteChunk(chain, 0x82, pool, 0, pool.Length);

    // splice the new chain into the blob
    var newChain = chain.ToArray();
    var delta = newChain.Length - (chainEnd - chainStart);
    var buf = new byte[data.Length + delta];
    Array.Copy(data, 0, buf, 0, chainStart);
    Array.Copy(newChain, 0, buf, chainStart, newChain.Length);
    Array.Copy(data, chainEnd, buf, chainStart + newChain.Length, data.Length - chainEnd);

    // length fields outside of the chain
    AddInt32(buf, containerPos - 4, delta);         // Java TC_ARRAY length
    AddInt32(buf, containerPos + 4, delta);         // container total length
    AddInt24(buf, containerPos + 0x17, delta);      // container total length - 0x400
    AddInt32(buf, namePos + 0x22, delta);           // outer meta: cdb block length (in Y)
    AddInt32(buf, zipPos - 4, delta);               // cdb meta: data section length (in X)

    this.data = buf;
    this.chainEnd = chainStart + newChain.Length;
    buf.SetInt32(chainEnd, (int)CalcX(), false);
    buf.SetInt32(chainEnd + 8, (int)CalcY(), false);

    serviceDatabase = buf;
    return order.Select(i => GetRecordId(Records[i])).ToList();
  }
  #endregion

  #region checksums
  private uint CalcX()
  {
    // cdb header + cdb meta, followed by the deflate payloads of all chunks (without chunk headers)
    var metaLen = data.GetInt16(cdbPos + 5, false);
    var ms = new MemoryStream();
    ms.Write(data, cdbPos, 7 + metaLen);
    for (int off = chainStart; off < chainEnd;)
    {
      var compLen = data.GetInt32(off + 1, false);
      ms.Write(data, off + 9, compLen);
      off += 9 + compLen;
    }
    var bytes = ms.ToArray();
    return Crc32.Normal.CalcCrc32(bytes, 0, bytes.Length);
  }

  private uint CalcY()
  {
    var start = namePos - 8;
    var metaLen = data.GetInt16(start + 5, false);
    return Crc32.Normal.CalcCrc32(data, start, 7 + metaLen);
  }

  public bool VerifyChecksums()
  {
    return (uint)data.GetInt32(chainEnd, false) == CalcX() && (uint)data.GetInt32(chainEnd + 8, false) == CalcY();
  }
  #endregion

  #region helpers
  private static byte[] Inflate(byte[] buf, int off, int len)
  {
    using var input = new MemoryStream(buf, off, len);
    using var deflate = new DeflateStream(input, CompressionMode.Decompress);
    var output = new MemoryStream();
    deflate.CopyTo(output);
    return output.ToArray();
  }

  private static void WriteChunk(Stream chain, byte flag, byte[] raw, int off, int len)
  {
    var comp = new MemoryStream();
    // any valid raw deflate stream works because X covers the written bytes; SmallestSize (zlib level 9) reproduces the TV's output
#if NETFRAMEWORK
    using (var deflate = new DeflateStream(comp, CompressionLevel.Optimal, true))
#else
    using (var deflate = new DeflateStream(comp, CompressionLevel.SmallestSize, true))
#endif
      deflate.Write(raw, off, len);
    var header = new byte[9];
    header[0] = flag;
    header.SetInt32(1, (int)comp.Length, false);
    header.SetInt32(5, len, false);
    chain.Write(header, 0, header.Length);
    comp.Position = 0;
    comp.CopyTo(chain);
  }

  private static byte[] BuildPool(List<byte[]> entries)
  {
    var ms = new MemoryStream();
    var head = new byte[8 + 4 * entries.Count];
    head.SetInt32(0, 4 * entries.Count, false);
    head.SetInt32(4, entries.Sum(e => e.Length), false);
    int strOff = 0;
    for (int i = 0; i < entries.Count; i++)
    {
      head.SetInt32(8 + 4 * i, strOff, false);
      strOff += entries[i].Length;
    }
    ms.Write(head, 0, head.Length);
    foreach (var e in entries)
      ms.Write(e, 0, e.Length);
    return ms.ToArray();
  }

  private static void AddInt32(byte[] buf, int off, int delta) => buf.SetInt32(off, buf.GetInt32(off, false) + delta, false);

  private static void AddInt24(byte[] buf, int off, int delta)
  {
    var v = (buf[off] << 16) + (buf[off + 1] << 8) + buf[off + 2] + delta;
    buf[off] = (byte)(v >> 16);
    buf[off + 1] = (byte)(v >> 8);
    buf[off + 2] = (byte)v;
  }

  private static int IndexOf(byte[] buf, byte[] pattern, int start)
  {
    for (int i = Math.Max(0, start); i <= buf.Length - pattern.Length; i++)
    {
      if (Tools.MemComp(buf, i, pattern) == 0)
        return i;
    }
    return -1;
  }

  private static int LastIndexOf(byte[] buf, byte[] pattern, int before)
  {
    for (int i = Math.Min(before, buf.Length - pattern.Length); i >= 0; i--)
    {
      if (Tools.MemComp(buf, i, pattern) == 0)
        return i;
    }
    return -1;
  }
  #endregion
}
