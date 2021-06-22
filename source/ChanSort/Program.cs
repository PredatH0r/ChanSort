using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  static class Program
  {
    internal static bool ChangeLanguage;
    
    [STAThread]
    static void Main()
    {
      // if there is no valid locale set up in the Windows region settings, SharpZipLib will fail to open zip files.
      if (Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage <= 1)
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
      if (Thread.CurrentThread.CurrentUICulture.TextInfo.OEMCodePage <= 1)
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      Application.ThreadException += Application_ThreadException;
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
      DevExpress.Skins.SkinManager.EnableFormSkins();
      WindowsFormsSettings.AllowDpiScale = true;
      WindowsFormsSettings.AllowAutoScale = DefaultBoolean.True;
      BarAndDockingController.Default.PropertiesBar.ScaleIcons = false;
      do
      {
        ChangeLanguage = false;
        Application.Run(new MainForm());
      } while (ChangeLanguage);
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      HandleException(e.Exception);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      HandleException(e.ExceptionObject as Exception);
    }

    private static void HandleException(Exception ex)
    {
      MessageBox.Show(
        "Bei der Programmausführung trat folgender Fehler auf:\n" + (ex == null ? "(null)" : ex.ToString()),
        "Fehler bei Programmausführung", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
