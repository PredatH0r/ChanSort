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
        this.ModelName = Encoding.ASCII.GetString(content, off, modelNameLen-1);
      off += modelNameLen;

      log?.Invoke($"Philips TV model: {this.ModelName}\nFile format version: {VersionMajor}.{VersionMinor}\n\n");

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
        content[off + 1] = (byte) (crc >> 8);
      }
      File.WriteAllBytes(chanLstBinPath, content);
    }
  }
}
