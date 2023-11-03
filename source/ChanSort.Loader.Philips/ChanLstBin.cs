using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Philips
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
    public uint VersionMinor { get; private set; }
    public uint VersionMajor { get; private set; }
    public string ModelName { get; private set; }

    private Action<string> log;

    public void Load(string path, Action<string> log)
    {
      this.log = log;
      this.content = File.ReadAllBytes(path);

      var off = 0;
      VersionMinor = BitConverter.ToUInt16(content, off);
      off += 2;
      VersionMajor = BitConverter.ToUInt16(content, off);
      off += 2;

      // skip unknown 14 bytes
      off += 14;

      var modelNameLen = BitConverter.ToInt32(content, off);
      off += 4;

      if (modelNameLen >= 1)
        this.ModelName = Encoding.ASCII.GetString(content, off, modelNameLen).TrimEnd('\0');
      off += modelNameLen;

      log?.Invoke($"Philips TV model: {this.ModelName}\nFile format version: {VersionMajor}.{VersionMinor}\n\n");

      if (VersionMajor >= 120)
      {
        LoadVersion120OrLater(path);
        return;
      }

      var baseDir = Path.GetDirectoryName(path) ?? "";
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
          // normally all files after the /s2channellib/ entry are inside that folder, but "Favorite.xml" is in the main folder
          // in ChannelMap45 there is also tv.db and list.db in the main folder
          var newPath = relPath + fileName;
          if (!File.Exists(Path.Combine(baseDir, newPath)) && File.Exists(Path.Combine(baseDir, fileName)))
            newPath = "/" + fileName;

          crcOffsetByRelPath[newPath] = off;
          off += 2;
        }
      }

      this.Validate(path, false);
    }

    private class V120Info
    {
      public readonly int Offset;
      public readonly string Filename;
      public readonly int BytesBeforeChecksum;
      public readonly string SubDir;

      public V120Info(int offset, string filename, int bytesBeforeChecksum, string subDir)
      {
        this.Offset = offset;
        this.Filename = filename;
        this.BytesBeforeChecksum = bytesBeforeChecksum;
        this.SubDir = subDir;
      }
    }

    private void LoadVersion120OrLater(string path)
    {
      // starting with version 120, the chanLst.bin
      // - no longer includes a 0x00 terminating character as part of the file name
      // - has a random number of 0x00 bytes following the file name (0-3) (2 after DVBT, 3 after DVBC, 0 after DVBSall, 1 after Favorite)
      // - only stores 1 byte for the length of "/s2channellib/" instead of 4
      // - only stores the lower 8 bits of the CRC16-Modbus
      // This format required a tailor-made implementation with the exact byte layout

      if (content.Length != 118)
        throw LoaderException.Fail($"chanLst.bin has an unsupported size");

      var entries = new List<V120Info>
      {
        new V120Info(0x29, "DVBT.xml", 2, "/channellib/"),
        new V120Info(0x38, "DVBC.xml", 3, "/channellib/"),
        new V120Info(0x58, "DVBSall.xml", 0, "/s2channellib/"),
        new V120Info(0x68, "Favorite.xml", 1, ""),
      };


      foreach(var entry in entries )
      {
        var off = entry.Offset;
        var fileName = Encoding.ASCII.GetString(content, off, entry.Filename.Length);
        if (fileName != entry.Filename)
          throw LoaderException.Fail("Entry in chanLst.bin doesn't match expected data");
        
        var newPath = entry.SubDir + fileName;
        crcOffsetByRelPath[newPath] = off + entry.Filename.Length + entry.BytesBeforeChecksum;
      }

      this.Validate(path, true);
    }

    private void Validate(string chanLstBinPath, bool onlyLower8Bits)
    {
      var baseDir = Path.GetDirectoryName(chanLstBinPath);
      string errors = "";
      foreach (var entry in crcOffsetByRelPath)
      {
        var crcOffset = entry.Value;
        int expectedCrc;
        if (onlyLower8Bits)
          expectedCrc = this.content[crcOffset];
        else
        {
          expectedCrc = BitConverter.ToUInt16(this.content, crcOffset);
          if (expectedCrc == 0)
            continue;
        }

        var filePath = baseDir + entry.Key;
        if (!File.Exists(filePath))
        {
          this.log?.Invoke($"\nchanLst.bin: file not found in directory structure: {entry.Key}");
          continue;
        }

        var data = File.ReadAllBytes(filePath);
        var length = data.Length;
        if (VersionMajor < 12 && length > 0x6000)
          length = 0x6000; // there might be another cap at 0x013FA000 + 0x6000 in some versions
        //if (length > 0x0140000)
        //  length = 0x0140000;

        var actualCrc = Crc16.Modbus(data, 0, length);
        if (onlyLower8Bits)
          actualCrc &= 0x00FF;
        if (actualCrc != expectedCrc)
        {
          var msg = $"chanLst.bin: stored CRC for {entry.Key} is {expectedCrc:X4} but calculated {actualCrc:X4}";
          errors += "\n" + msg;
        }
      }

      if (errors != "")
      {
        this.log?.Invoke(errors);

        if (View.Default != null) // can't show dialog while unit-testing
        {

          var dlg = View.Default.CreateActionBox(Resources.WarningChecksumErrorMsg);
          dlg.AddAction(Resources.WarningChechsumErrorIgnore, 1);
          dlg.AddAction(Resources.Cancel, 0);
          dlg.ShowDialog();
          switch (dlg.SelectedAction)
          {
            case 0:
              throw LoaderException.Fail("Aborted due to checksum errors");
          }
        }
      }

      var info = Resources.InfoRestartAfterImport;
      if (this.VersionMajor >= 25 && this.VersionMajor <= 45)
        info += "\n" + Resources.InfoIgnoreImportError;

      View.Default?.MessageBox(info, "Philips");
    }

    public void Save(string chanLstBinPath)
    {
      var baseDir = Path.GetDirectoryName(chanLstBinPath);
      foreach (var entry in crcOffsetByRelPath)
      {
        var path = baseDir + entry.Key;
        if (!File.Exists(path))
          continue; 
        var data = File.ReadAllBytes(path);
        var length = data.Length;
        if (VersionMajor < 12 && length > 0x6000)
          length = 0x6000; // there might be another cap at 0x013FA000 + 0x6000 in some versions
        var crc = Crc16.Modbus(data, 0, length);
        var off = entry.Value;
        content[off] = (byte) crc;
        if (VersionMajor < 120)
          content[off + 1] = (byte) (crc >> 8);
      }
      File.WriteAllBytes(chanLstBinPath, content);
    }
  }
}
