using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ChanSort.Api
{
  #region class FileAssociation
  public class FileAssociation
  {
    public string Extension { get; }
    public string ProgId { get; }
    public string FileTypeDescription { get; }
    public string CommandLine { get; }
    public string IconPath { get; }

    public FileAssociation(string ext, string progId, string descr, string commandLine, string iconPath)
    {
      this.Extension = ext;
      this.ProgId = progId;
      this.FileTypeDescription = descr;
      this.CommandLine = commandLine;
      this.IconPath = iconPath;
    }
  }
  #endregion

  public static class FileAssociations
  {
    // needed so that Explorer windows get refreshed after the registry is updated
    [System.Runtime.InteropServices.DllImport("Shell32.dll")]
    private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    private const int SHCNE_ASSOCCHANGED = 0x8000000;
    private const int SHCNF_FLUSH = 0x1000;

    public static void CreateMissingAssociations(IEnumerable<string> fileExtensions)
    {
      var filePath = Process.GetCurrentProcess().MainModule.FileName;
      var cmdLine = "\"" + filePath + "\" \"%1\"";
      var icoPath = Path.Combine(Path.GetDirectoryName(filePath), "ChanSort.ico");

      var assocs = new List<FileAssociation>();
      foreach(var ext in fileExtensions)
        assocs.Add(new FileAssociation(ext, "ChanSort" + ext, "TV Channel List (" + ext + ")", cmdLine, icoPath));
      EnsureAssociationsSet(assocs);
    }

    public static void EnsureAssociationsSet(IEnumerable<FileAssociation> associations)
    {
      bool madeChanges = false;
      foreach (var assoc in associations)
        madeChanges |= SetAssociation(assoc.Extension, assoc.ProgId, assoc.FileTypeDescription, assoc.CommandLine, assoc.IconPath);

      if (madeChanges)
        RefreshDesktopIcons();
    }

    public static void RefreshDesktopIcons()
    {
      SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
    }

    public static bool SetAssociation(string extension, string progId, string fileTypeDescription, string commandLine, string iconPath)
    {
      bool madeChanges = false;
      //madeChanges |= SetValue($@"Software\Classes\{extension}", null, progId);
      madeChanges |= SetValue($@"Software\Classes\{extension}\OpenWithProgids", progId, "");
      madeChanges |= SetValue($@"Software\Classes\{progId}", null, fileTypeDescription);
      madeChanges |= SetValue($@"Software\Classes\{progId}\shell\open\command", null, commandLine, true);
      madeChanges |= SetValue($@"Software\Classes\{progId}\DefaultIcon", null, iconPath);
      madeChanges |= SetValue($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\OpenWithProgids", progId, "");

      //  if (madeChanges)
      //  {
      //    try
      //    {
      //      Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\UserChoice", false);
      //    }
      //    catch
      //    {
      //      // ignore
      //    }
      //  }

      return madeChanges;
    }

    private static bool SetValue(string keyPath, string name, string value, bool force = false)
    {
      using var key = Registry.CurrentUser.CreateSubKey(keyPath);
      var currentValue = key.GetValue(name) as string;
      var nameExists = currentValue != null;
      if (nameExists)
      {
        if (currentValue == value || !force)
          return false;
      }

      key.SetValue(name, value);
      return true;
    }

    public static void DeleteAssociations(IEnumerable<string> extensions)
    {
      foreach (var ext in extensions)
      {
        var progId = "ChanSort" + ext;
        DeleteValue($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\OpenWithProgids", progId);
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{progId}", false);
        DeleteValue($@"Software\Classes\{ext}\OpenWithProgids", progId);
      }

      void DeleteValue(string keyPath, string name)
      {
        using var key = Registry.CurrentUser.OpenSubKey(keyPath, true);
        if (key != null)
        {
          if (name == null)
            key.SetValue(null, "");
          else
            key.DeleteValue(name, false);
        }
      }
    }
  }
}