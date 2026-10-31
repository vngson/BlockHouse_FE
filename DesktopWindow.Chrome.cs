using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Automation;
using System.Windows.Media;

namespace BlockHouse;

public partial class DesktopWindow
{
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(ConstrainMaximizedWindow);
    }

    // A frameless window must use the monitor work area, otherwise WebView extends below the taskbar.
    private IntPtr ConstrainMaximizedWindow(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != 0x0024) return IntPtr.Zero; // WM_GETMINMAXINFO
        var monitor = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(MonitorFromWindow(hwnd, 2), ref monitor)) return IntPtr.Zero;
        var bounds = GetMaximizedBounds(monitor.Monitor.ToRect(), monitor.Work.ToRect());
        var limits = Marshal.PtrToStructure<MinMaxInfo>(lParam);
        limits.MaxPosition = new NativePoint { X = bounds.X, Y = bounds.Y };
        limits.MaxSize = new NativePoint { X = bounds.Width, Y = bounds.Height };
        Marshal.StructureToPtr(limits, lParam, false);
        handled = true;
        return IntPtr.Zero;
    }

    internal static Int32Rect GetMaximizedBounds(Int32Rect monitor, Int32Rect work) =>
        new(work.X - monitor.X, work.Y - monitor.Y, work.Width, work.Height);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left, Top, Right, Bottom;
        public readonly Int32Rect ToRect() => new(Left, Top, Right - Left, Bottom - Top);
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo { public NativePoint Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo { public int Size; public NativeRect Monitor, Work; public int Flags; }
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    private void MinimizeDesktop(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
    private void ToggleDesktopSize(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
        else SystemCommands.MaximizeWindow(this);
    }
    private void CloseDesktop(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

    private void CaptionStateChanged(object? sender, EventArgs e)
    {
        if (MaximizeButton is null || MaximizeGlyph is null) return;
        var maximized = WindowState == WindowState.Maximized;
        MaximizeGlyph.Data = Geometry.Parse(maximized ? "M5,4 L5,1 L13,1 L13,9 L10,9 M1,4 L10,4 L10,13 L1,13 Z" : "M2,2 L12,2 L12,12 L2,12 Z");
        MaximizeButton.ToolTip = maximized ? "Thu về" : "Phóng to";
        AutomationProperties.SetName(MaximizeButton, maximized ? "Thu về kích thước trước" : "Phóng to cửa sổ");
    }

    private void ApplyCaptionTheme(bool dark)
    {
        Resources["CaptionSurface"] = new SolidColorBrush(dark ? Color.FromRgb(28,28,28) : Colors.White);
        Resources["CaptionInk"] = new SolidColorBrush(dark ? Color.FromRgb(238,238,238) : Color.FromRgb(23,23,23));
        Resources["CaptionLine"] = new SolidColorBrush(dark ? Color.FromRgb(153,153,153) : Color.FromRgb(32,32,32));
        Resources["CaptionShadow"] = new SolidColorBrush(dark ? Colors.Black : Color.FromRgb(32,32,32));
        Resources["DesktopGround"] = new SolidColorBrush(dark ? Color.FromRgb(16,16,16) : Color.FromRgb(237,237,237));
    }
}
