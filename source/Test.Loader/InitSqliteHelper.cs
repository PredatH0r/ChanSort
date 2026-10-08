using System;
using System.IO;
using System.Threading;

namespace Test.Loader;

/// <summary>
/// Helper class for deploying the "runtimes" folder with native binaries, so that SQLite can be initialized.
/// 
/// </summary>
public class InitSqliteHelper
{
  private static readonly SemaphoreSlim _lock = new(1, 1);
  private static volatile bool initialized;

  public static void InitializeSqlite()
  {
    if (initialized)
      return;

    _lock.Wait();
    try
    {
      if (initialized)
        return;

      if (!Directory.Exists(Path.Combine(TestUtils.GetExecutableDir(), "runtimes")))
      {
        // copy runtimes folder with native binaries (couldn't find a clean way to determine the build output folder when running unit tests in parallel)
        var targetFramework = Environment.Version.Major == 4 ? "net48" : "net" + Environment.Version.Major;
        var buildOutputRuntimesDir = TestUtils.GetSolutionBaseDir() + "\\Debug\\" + targetFramework + "\\runtimes";
        if (Directory.Exists(buildOutputRuntimesDir))
        {
          TestUtils.DeploymentItem(buildOutputRuntimesDir);
          SQLitePCL.Batteries.Init();
        }
      }

      initialized = true;
    }
    finally
    {
      _lock.Release();
    }
  }
}