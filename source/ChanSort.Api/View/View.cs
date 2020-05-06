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
    public static View Default { get; set; }

    public Func<string, IActionBoxDialog> CreateActionBox { get; set; }

    public Func<string, string, int, int, int> MessageBoxImpl { get; set; }

    public int MessageBox(string msg, string caption = "", int buttons = 0, int icon = 0)
    {
      return MessageBoxImpl(msg, caption, buttons, icon);
    }
  }
}
