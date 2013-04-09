using System.Net.Sockets;
using System.Text;
using System.Threading;
using ChanSort.Ui.Properties;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  class UpdateCheck
  {
    private const string SearchString = "ChanSort_";

    public static void CheckForNewVersion()
    {
      var check = new UpdateCheck();
      Thread thread = new Thread(check.Check);
      thread.Start();
    }

    private void Check()
    {
      try
      {
        var newVersion = this.GetLatestVersion();
        if (newVersion.CompareTo(MainForm.AppVersion.TrimStart('v')) > 0)
          this.NotifyAboutNewVersion(newVersion);
      }
      catch { }
    }

    private string GetLatestVersion()
    {
      // NOTE: tried using WebRequest class, but that causes massive timeout problems after program start (DLL loading/init?)
      byte[] buffer = new byte[100000];
      int len;
      using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
      {
        sock.ReceiveTimeout = 5000;
        sock.Connect("sourceforge.net", 80);
        var request = Encoding.UTF8.GetBytes("GET /projects/chansort/ HTTP/1.1\r\nHost: sourceforge.net\r\n\r\n");
        sock.Send(request);
        len = sock.Receive(buffer);
      }

      var response = Encoding.ASCII.GetString(buffer, 0, len);
      int start = response.IndexOf(SearchString);
      if (start >= 0)
      {
        int end = response.IndexOf(".zip", start);
        if (end == start + SearchString.Length + 10)
          return response.Substring(start + SearchString.Length, 10);
      }
      return string.Empty;
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
