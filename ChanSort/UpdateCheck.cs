using System.Net.Sockets;
using System.Text;
using ChanSort.Ui.Properties;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  class UpdateCheck
  {
    private const string SearchString = "ChanSort_";

    public static void CheckForNewVersion()
    {
      try
      {
        var check = new UpdateCheck();
        check.Check();
      }
      catch
      {
      }
    }

    private void Check()
    {
      // NOTE: tried using WebRequest class, but that causes massive timeout problems after program start (DLL loading/init?)
      using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
      {
        sock.ReceiveTimeout = 1000;
        sock.Connect("sourceforge.net", 80);
        var request = Encoding.UTF8.GetBytes("GET /projects/chansort/ HTTP/1.1\r\nHost: sourceforge.net\r\n\r\n");
        sock.Send(request);
        byte[] buffer = new byte[100000];
        int len = sock.Receive(buffer);
        var response = Encoding.ASCII.GetString(buffer, 0, len);
        int start = response.IndexOf(SearchString);
        if (start >= 0)
        {
          int end = response.IndexOf(".zip", start);
          if (end == start + SearchString.Length + 10)
          {
            string newVersion = response.Substring(start + SearchString.Length, 10);
            if (newVersion.CompareTo(MainForm.AppVersion.TrimStart('v')) > 0)
              this.NotifyAboutNewVersion(newVersion);
          }
        }
      }      
    }

    private void NotifyAboutNewVersion(string newVersion)
    {
      if (XtraMessageBox.Show(
        string.Format(Resources.UpdateCheck_NotifyAboutNewVersion_Message, newVersion),
        Resources.UpdateCheck_NotifyAboutNewVersion_Caption,
        System.Windows.Forms.MessageBoxButtons.YesNo,
        System.Windows.Forms.MessageBoxIcon.Question,
        System.Windows.Forms.MessageBoxDefaultButton.Button1) != System.Windows.Forms.DialogResult.Yes)
        return;
      BrowserHelper.OpenUrl("http://sourceforge.net/p/chansort/files/");
    }
  }
}
