//using Ble.Linux;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Automatically close the WCL nag dialog
/// </summary>
public class AntiNag
{
  // Delegate for the EnumWindows method
  private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  // Importing necessary functions from user32.dll
  [DllImport("user32.dll")]
  private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

  [DllImport("user32.dll")]
  private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

  // Constant for the SendMessage function to close a window
  private const uint WM_CLOSE = 0x0010;

  public void Run()
  {
    _ = Task.Run(() =>
    {
      while (true)
      {
        EnumWindowsProc enumProc = new(EnumWindow);
        if (!EnumWindows(enumProc, IntPtr.Zero))
        {
          break;
        }
        Thread.Sleep(10);
      }
    });
  }

  private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
  {
    const string searchKeyword = "DEMO";
    var title = GetWindowTitle(hWnd);

    if (title.Contains(searchKeyword))
    {
      Close(hWnd);
      return false; // Stop enumerating windows
    }

    return true; // Continue enumerating windows
  }

  private static string GetWindowTitle(IntPtr hWnd)
  {
    StringBuilder sb = new(256);
    GetWindowText(hWnd, sb, sb.Capacity);
    return sb.ToString();
  }

  private static void Close(IntPtr hWnd) => SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
}