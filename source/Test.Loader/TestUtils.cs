using System;
using System.IO;
using System.Reflection;

namespace Test.Loader
{
  public class TestUtils
  {
    private static string solutionDir;
    private static string executableDir;

    #region DeploymentItem()

    /// <summary>
    /// Replacement for [DeploymentItemAttribute], which doesn't work with VS2010 + ReSharper
    /// </summary>
    public static string DeploymentItem(string file)
    {
      GetSolutionBaseDir();
      GetExecutableDir();


      var src = Path.Combine(solutionDir, file);
      var dest = Path.Combine(executableDir, Path.GetFileName(file));
      if (Directory.Exists(src))
        DeployRecursively(src, dest);
      else
        File.Copy(src, dest, true);
      return dest;
    }

    private static void DeployRecursively(string src, string dest)
    {
      Directory.CreateDirectory(dest);
      foreach(var file in Directory.GetFiles(src))
        File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
      foreach(var subdir in Directory.GetDirectories(src))
        DeployRecursively(subdir, Path.Combine(dest, Path.GetFileName(subdir)));
    }

    #endregion

    #region GetSolutionBaseDir()
    public static string GetSolutionBaseDir()
    {
      if (solutionDir != null)
        return solutionDir;

      var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      do
      {
        if (File.Exists(dir + "\\ChanSort.sln"))
          return solutionDir = dir;
        dir = Path.GetDirectoryName(dir);
      } while (!string.IsNullOrEmpty(dir));

      dir = Environment.CurrentDirectory;
      do
      {
        if (File.Exists(dir + "\\ChanSort.sln"))
          return solutionDir = dir;
        dir = Path.GetDirectoryName(dir);
      } while (!string.IsNullOrEmpty(dir));

      throw new InvalidOperationException("Cannot determine base directory of ChanSort solution");
    }
    #endregion

    #region GetExecutableDir()
    public static string GetExecutableDir()
    {
      if (executableDir == null)
        executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      return executableDir;
    }
    #endregion
  }
}
