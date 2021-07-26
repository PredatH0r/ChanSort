using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class Win32
{
  #region enum Msgs
  public enum Msgs
  {
    WM_SETREDRAW = 0x000B,
  }
  #endregion

  #region User32

  [DllImport("User32.dll", CharSet = CharSet.Auto)]
  public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

  #endregion

  #region SuspendRedraw() / ResumeRedraw()
  public static void SuspendRedraw(this Control control)
  {
    Win32.SendMessage(control.Handle, (int)Win32.Msgs.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
  }

  public static void ResumeRedraw(this Control control)
  {
    Win32.SendMessage(control.Handle, (int)Win32.Msgs.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
    control.Refresh();
  }
  #endregion

}