using System;
using System.Threading;
using System.Windows.Forms;

namespace ChanSort.Ui
{
  static class Program
  {
    internal static bool ChangeLanguage;
    
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      Application.ThreadException += Application_ThreadException;
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
      DevExpress.Skins.SkinManager.EnableFormSkins();
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
