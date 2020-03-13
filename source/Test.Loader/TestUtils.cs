﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test.Loader
{
  public class TestUtils
  {
    private static string solutionDir;
    private static string executableDir;

    #region DeploymentItem()

    /// <summary>
    /// DeploymentItemAttribute doesn't work with the combination of VS2010, ReSharper 7.1.3, Target Framework 3.5
    /// </summary>
    public static string DeploymentItem(string file)
    {
      GetSolutionBaseDir();
      GetExecutableDir();


      var destFile = Path.Combine(executableDir, Path.GetFileName(file));
      File.Copy(Path.Combine(solutionDir, file), destFile, true);
      return destFile;
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
