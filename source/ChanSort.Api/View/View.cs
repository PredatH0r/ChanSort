using System;

namespace ChanSort.Api
{

  public interface IActionBoxDialog : IDisposable
  {
    string Message { get; set; }
    void AddAction(string text, int result);
    int SelectedAction { get; }
    void ShowDialog();
  }

  public class View
  {
    #region enums MessageBoxButtons, MessageBoxIcon, DialogResult

    /// <summary>
    /// copy of System.Windows.Forms.MessageBoxButtons so that no reference to a UI assembly is required for dependent projects
    /// </summary>
    public enum MessageBoxButtons
    {
      OK,
      OKCancel,
      AbortRetryIgnore,
      YesNoCancel,
      YesNo,
      RetryCancel,
    }

    /// <summary>
    /// copy of System.Windows.Forms.MessageBoxIcon so that no reference to a UI assembly is required for dependent projects
    /// </summary>
    public enum MessageBoxIcon
    {
      None = 0,
      Error = 16, // 0x00000010
      Hand = 16, // 0x00000010
      Stop = 16, // 0x00000010
      Question = 32, // 0x00000020
      Exclamation = 48, // 0x00000030
      Warning = 48, // 0x00000030
      Asterisk = 64, // 0x00000040
      Information = 64, // 0x00000040
    }

    /// <summary>
    /// copy of System.Windows.Forms.DialogResult so that no reference to a UI assembly is required for dependent projects
    /// </summary>
    public enum DialogResult
    {
      None,
      OK,
      Cancel,
      Abort,
      Retry,
      Ignore,
      Yes,
      No,
    }
    #endregion

    public static View Default { get; set; }

    public Func<string, IActionBoxDialog> CreateActionBox { get; set; }

    public Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> MessageBoxImpl { get; set; }

    public Action<string, string, int, int, Action<string>> ShowHtmlBoxImpl { get; set; }



    public DialogResult MessageBox(string msg, string caption = "", MessageBoxButtons buttons = 0, MessageBoxIcon icon = 0)
    {
      return MessageBoxImpl(msg, caption, buttons, icon);
    }

    public void ShowHtmlBox(string html, string title = null, int width = 450, int height = 250, Action<string> onUrlClick = null)
    {
      ShowHtmlBoxImpl.Invoke(html, title, width, height, onUrlClick);
    }
  }
}
