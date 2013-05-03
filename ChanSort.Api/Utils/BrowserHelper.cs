using System.IO;

namespace ChanSort
{
  public static class BrowserHelper
  {
    public static void OpenUrl(string url)
    {
      OpenHtml(@"
<html>
<head>
  <meta http-equiv='Refresh' content='0;" + url + @"'/>
</head>
</html>");
    }

    public static void OpenMail(string url)
    {
      OpenHtml(@"
<html>
<head>
  <script language=""javascript"">
    window.open(""" + url + @""");
    window.close();
  </script>
</head>
</html>");
    }

    public static void OpenHtml(string html)
    {
      try
      {
        string fileName = Path.GetTempFileName() + ".html";
        File.WriteAllText(fileName, html);
        System.Diagnostics.Process.Start(fileName);
      }
      catch
      {
      }
    }
  }
}
