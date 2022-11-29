using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Test.Loader
{
  public class LoaderTestBase
  {
    #region class ExpectedData
    public class ExpectedData
    {
      public readonly string File;
      public readonly int AnalogChannels;
      public readonly int DtvChannels;
      public readonly int SatChannels;
      public readonly int DtvRadio;
      public readonly int SatRadio;

      public ExpectedData(string file, int analog, int dtv, int sat, int dtvRadio, int satRadio)
      {
        this.File = file;
        this.AnalogChannels = analog;
        this.DtvChannels = dtv;
        this.SatChannels = sat;
        this.DtvRadio = dtvRadio;
        this.SatRadio = satRadio;
      }
    }
    #endregion

    #region Helper Methods

    public static IEnumerable<string> FindAllFiles(string baseDir, string mask)
    {
      string path = GetTestFileDirectory(baseDir);
      List<string> files = new List<string>();
      GetFilesRecursively(path, mask, files);
      return files;
    }

    private static string GetTestFileDirectory(string baseDir)
    {
      string exeDir = Assembly.GetExecutingAssembly().Location;
      while (!string.IsNullOrEmpty(exeDir))
      {
        string testFileDir = Path.Combine(exeDir, baseDir);
        if (Directory.Exists(testFileDir))
          return testFileDir;
        exeDir = Path.GetDirectoryName(exeDir);
      }
      throw new FileNotFoundException("No 'TestFiles' directory found");
    }

    private static void GetFilesRecursively(string path, string mask, List<string> files)
    {
      files.AddRange(Directory.GetFiles(path, mask));
      foreach (var dir in Directory.GetDirectories(path))
        GetFilesRecursively(dir, mask, files);
    }
    #endregion
  }
}
