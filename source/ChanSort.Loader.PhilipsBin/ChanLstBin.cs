using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChanSort.Loader.PhilipsBin
{
  class ChanLstBin
  {
    // the chanLst.bin file contains the model name and CRC16 values for all the files in the channellib and s2channellib subdirectories

    /*

    public struct Ph_chanLst_bin
    {
      word versionMinor;
      word versionMajor;
      byte unknown4[14];
      dword modelNameLen;
      char modelName[modelNameLen];
      Ph_chanLst_bin_FileEntry fileEntries[*];
    };

    struct Ph_chanLst_bin_FileEntry
    {
      dword fileNameLength;
      char fileName[fileNameLength];
      if (fileName[0] != '/')
        word crc16modbus;
    };

     */

    private byte[] content;
    private readonly Dictionary<string,int> crcOffsetByRelPath = new Dictionary<string, int>();
    private uint versionMinor;
    private uint versionMajor;
    private Action<string> log;

    public void Load(string path, Action<string> log)
    {
      this.log = log;
      this.content = File.ReadAllBytes(path);

      var off = 0;
      versionMinor = BitConverter.ToUInt16(content, off);
      off += 2;
      versionMajor = BitConverter.ToUInt16(content, off);
      off += 2;

      // skip unknown 14 bytes
      off += 14;

      var modelNameLen = BitConverter.ToInt32(content, off);
      off += 4 + modelNameLen;
      var relPath = "/channellib/";
      while (off < content.Length)
      {
        var fileNameLen = BitConverter.ToInt32(content, off);
        off += 4;
        var fileName = Encoding.ASCII.GetString(content, off, fileNameLen).TrimEnd('\0');
        off += fileNameLen;
        if (fileName[0] == '/')
          relPath = fileName;
        else
        {
          crcOffsetByRelPath[relPath + fileName] = off;
          off += 2;
        }
      }

      this.Validate(path);
    }

    private void Validate(string chanLstBinPath)
    {
      var baseDir = Path.GetDirectoryName(chanLstBinPath);
      string errors = "";
      foreach (var entry in crcOffsetByRelPath)
      {
        var crcOffset = entry.Value;
        var expectedCrc = BitConverter.ToUInt16(this.content, crcOffset);
        if (expectedCrc == 0)
          continue;

        var filePath = baseDir + entry.Key;
        if (!File.Exists(filePath))
          continue;
        var data = File.ReadAllBytes(filePath);
        var length = Math.Min(data.Length, versionMajor <= 12 ? 0x6000 : 0x145A00);
        var actualCrc = ModbusCrc16.Calc(data, 0, length);
        if (actualCrc != expectedCrc)
        {
          var msg = $"chanLst.bin: stored CRC for {entry.Key} is {expectedCrc:x4} but calculated {actualCrc:x4}";
          errors += "\n" + msg;
        }
      }

      if (errors != "")
      {
        this.log.Invoke(errors);
        //throw new FileLoadException(errors);
      }
    }

    public void Save(string chanLstBinPath)
    {
      var baseDir = Path.GetDirectoryName(chanLstBinPath);
      foreach (var entry in crcOffsetByRelPath)
      {
        var path = baseDir + entry.Key;
        var data = File.ReadAllBytes(path);
        var length = Math.Min(data.Length, versionMajor <= 12 ? 0x6000 : 0x145A00);
        var crc = ModbusCrc16.Calc(data, 0, length);
        var off = entry.Value;
        content[off] = (byte) crc;
        content[off + 1] = (byte) (crc >> 8);
      }
      File.WriteAllBytes(chanLstBinPath, content);
    }
  }
}
