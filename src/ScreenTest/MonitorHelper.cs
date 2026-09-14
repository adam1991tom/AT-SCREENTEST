using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WinFormsScreen = System.Windows.Forms.Screen;

namespace ScreenTest;

/// <summary>
/// Positions a WPF window to exactly cover a monitor's bounds in physical pixels.
/// WPF's Window.Left/Top/Width/Height are DPI-scaled logical units, which don't map
/// cleanly across monitors with different DPI, so we set the window's HWND rect directly.
/// </summary>
public static class MonitorHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public static void CoverScreen(Window window, WinFormsScreen screen)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var bounds = screen.Bounds;
        SetWindowPos(hwnd, IntPtr.Zero, bounds.X, bounds.Y, bounds.Width, bounds.Height,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    public static WinFormsScreen NextScreen(WinFormsScreen current)
    {
        var screens = WinFormsScreen.AllScreens;
        var index = Array.IndexOf(screens, current);
        var nextIndex = (index + 1) % screens.Length;
        return screens[nextIndex];
    }
}
