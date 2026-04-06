using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AlmostMaximize;

internal sealed partial class AlmostMaximizeCommand : InvokableCommand
{
    private readonly double _percentage;

    public AlmostMaximizeCommand(double percentage)
    {
        _percentage = Math.Max(0, percentage);
        Name = $"Almost maximize at {_percentage:0.#}% of maximized size";
    }

    public override ICommandResult Invoke()
    {
        return AlmostMaximizeRunner.ScheduleResize(_percentage);
    }
}

internal static class AlmostMaximizeRunner
{
    public static CommandResult ScheduleResize(double percentage)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(150).ConfigureAwait(false);

            var result = WindowResizer.AlmostMaximizeForegroundWindow(percentage);
            if (!result.Success)
            {
                new ToastStatusMessage(result.Message).Show();
            }
        });

        return CommandResult.Dismiss();
    }

    public static bool TryParsePercentage(string? input, out double percentage)
    {
        percentage = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out percentage)
            || double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out percentage);
    }
}

internal static class WindowResizer
{
    internal const int MaxPercentage = 100;
    internal const int MinPercentage = 1;
    private const int SwRestore = 9;
    private const uint MonitorDefaultToNearest = 2;
    private const uint GaRoot = 2;
    private static readonly IntPtr ShellWindow = NativeMethods.GetShellWindow();

    public static ResizeResult AlmostMaximizeForegroundWindow(double percentage)
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

        var rootHwnd = NativeMethods.GetAncestor(hwnd, GaRoot);
        if (rootHwnd != IntPtr.Zero)
        {
            hwnd = rootHwnd;
        }

        if (!IsSupportedWindow(hwnd, out var unsupportedMessage))
        {
            return new ResizeResult(false, unsupportedMessage);
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
        var boundedPercentage = Math.Max(MinPercentage, Math.Min(percentage, MaxPercentage));
        var targetWidth = Math.Max(1, (int)Math.Round(workArea.Width * (boundedPercentage / 100d)));
        var targetHeight = Math.Max(1, (int)Math.Round(workArea.Height * (boundedPercentage / 100d)));
        var targetLeft = workArea.Left + Math.Max(0, (workArea.Width - targetWidth) / 2);
        var targetTop = workArea.Top + Math.Max(0, (workArea.Height - targetHeight) / 2);

        NativeMethods.ShowWindow(hwnd, SwRestore);

        var moved = NativeMethods.MoveWindow(
            hwnd,
            targetLeft,
            targetTop,
            targetWidth,
            targetHeight,
            true);

        return moved
            ? new ResizeResult(true, $"Window resized to {boundedPercentage:0.#}% of the maximized size.")
            : new ResizeResult(false, "Windows could not resize the active window.");
    }

    private static bool IsSupportedWindow(IntPtr hwnd, out string message)
    {
        if (hwnd == ShellWindow)
        {
            message = "The Windows shell cannot be resized with this command.";
            return false;
        }

        var className = NativeMethods.GetWindowClassName(hwnd);
        if (string.Equals(className, "Shell_TrayWnd", StringComparison.Ordinal) ||
            string.Equals(className, "NotifyIconOverflowWindow", StringComparison.Ordinal) ||
            string.Equals(className, "TrayNotifyWnd", StringComparison.Ordinal) ||
            string.Equals(className, "Progman", StringComparison.Ordinal) ||
            string.Equals(className, "WorkerW", StringComparison.Ordinal))
        {
            message = "Taskbar, system tray, and desktop shell windows are intentionally ignored.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    internal readonly record struct ResizeResult(bool Success, string Message);

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

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

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        internal static MonitorInfo CreateMonitorInfo() => new()
        {
            cbSize = Marshal.SizeOf<MonitorInfo>(),
        };

        internal static string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder className = new(256);
            _ = GetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }
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
