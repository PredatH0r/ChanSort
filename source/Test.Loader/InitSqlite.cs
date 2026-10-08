using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Loader;

/// <summary>
/// Add a link to this class in each test project that needs to use SQLite.
/// </summary>
[TestClass]
public class InitSqlite
{
  [AssemblyInitialize]
  public static void InitializeSqlite(TestContext context)
  {
    InitSqliteHelper.InitializeSqlite();
  }
}