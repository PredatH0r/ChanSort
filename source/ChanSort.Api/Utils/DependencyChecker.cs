using System;
using System.IO;

namespace ChanSort.Api
{
  public static class DepencencyChecker
  {
    public static bool IsVc2010RedistPackageX86Installed()
    {
      object value = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x86",
                                        "Installed", null);
      return value != null && Convert.ToInt32(value) == 1;
    }

    public static void AssertVc2010RedistPackageX86Installed()
    {
      if (!IsVc2010RedistPackageX86Installed())
        throw new FileLoadException("Please download and install the Microsoft Visual C++ 2010 Redistributable Package (x86)");
    }
  }
}
