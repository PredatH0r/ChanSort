using System;
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
      do
      {
        ChangeLanguage = false;
        Application.Run(new MainForm());
      } while (ChangeLanguage);
    }
  }
}
