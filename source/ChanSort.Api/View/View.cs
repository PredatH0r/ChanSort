using System;

namespace ChanSort.View
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
  }
}
