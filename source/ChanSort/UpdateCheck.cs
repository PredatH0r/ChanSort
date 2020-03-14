using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using ChanSort.Ui.Properties;
using DevExpress.XtraEditors;

namespace ChanSort.Ui
{
  class UpdateCheck
  {
    private const string UpdateUrl = "https://github.com/PredatH0r/ChanSort/releases";
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
      string response;

      var oldProtocol = ServicePointManager.SecurityProtocol;
      try
      {
        //Change SSL checks so that all checks pass
        //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        using (WebClient client = new WebClient())
        {
          client.Proxy = null; // prevent a 1min wait/timeout by a .NET bug
          response = client.DownloadString(UpdateUrl);
        }
      }
      finally
      {
        ServicePointManager.SecurityProtocol = oldProtocol;
      }

      int start = response.IndexOf(SearchString);
      if (start >= 0)
      {
        int end = response.IndexOf(".zip", start);
        int len = end - start - SearchString.Length;
        if (len >= 10) // YYYY-MM-DD plus optional suffix for a revision
          return response.Substring(start + SearchString.Length, len);
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
      BrowserHelper.OpenUrl(UpdateUrl);
    }
  }
}
