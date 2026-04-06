using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AlmostMaximize;

internal sealed partial class AlmostMaximizeCommand : InvokableCommand
{
    private readonly int _margin;

    public AlmostMaximizeCommand(int margin)
    {
        _margin = Math.Max(0, margin);
        Name = $"Almost maximize with {_margin} px margin";
    }

    public override ICommandResult Invoke()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(150).ConfigureAwait(false);

            var result = WindowResizer.AlmostMaximizeForegroundWindow(_margin);
            if (!result.Success)
            {
                new ToastStatusMessage(result.Message).Show();
            }
        });

        return CommandResult.Dismiss();
    }
}

internal static class WindowResizer
{
    private const int SwRestore = 9;
    private const uint MonitorDefaultToNearest = 2;

    public static ResizeResult AlmostMaximizeForegroundWindow(int margin)
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
        {
            return new ResizeResult(false, "No active window was found.");
        }

        if (!NativeMethods.IsWindow(hwnd))
        {
            return new ResizeResult(false, "The active window is no longer available.");
        }

        var monitor = NativeMethods.MonitorFromWindow(hwnd, MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero)
        {
            return new ResizeResult(false, "Unable to detect the monitor for the active window.");
        }

        var monitorInfo = NativeMethods.CreateMonitorInfo();
        if (!NativeMethods.GetMonitorInfo(monitor, ref monitorInfo))
        {
            return new ResizeResult(false, "Unable to read the monitor work area.");
        }

        var workArea = monitorInfo.rcWork;
        var boundedMargin = Math.Max(0, Math.Min(margin, Math.Min(workArea.Width, workArea.Height) / 2));

        NativeMethods.ShowWindow(hwnd, SwRestore);

        var moved = NativeMethods.MoveWindow(
            hwnd,
            workArea.Left + boundedMargin,
            workArea.Top + boundedMargin,
            Math.Max(1, workArea.Width - (boundedMargin * 2)),
            Math.Max(1, workArea.Height - (boundedMargin * 2)),
            true);

        return moved
            ? new ResizeResult(true, $"Window resized with a {boundedMargin} px margin.")
            : new ResizeResult(false, "Windows could not resize the active window.");
    }

    internal readonly record struct ResizeResult(bool Success, string Message);

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool repaint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        internal static MonitorInfo CreateMonitorInfo() => new()
        {
            cbSize = Marshal.SizeOf<MonitorInfo>(),
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;

        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }
}
