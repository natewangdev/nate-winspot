using System.Diagnostics;
using System.Text;
using System.Windows;
using WinSpot.Models;
using WinSpot.Native;

namespace WinSpot.Services;

public sealed class WindowBindService : IWindowBindService
{
    public BoundWindowInfo? TryResolveFromScreenPoint(Point screenPoint)
    {
        var pt = new NativeMethods.POINT { X = (int)screenPoint.X, Y = (int)screenPoint.Y };
        var hwnd = NativeMethods.WindowFromPoint(pt);
        if (hwnd == nint.Zero || !NativeMethods.IsWindow(hwnd))
        {
            return null;
        }

        hwnd = GetRootWindow(hwnd);
        return BuildInfo(hwnd);
    }

    public BoundWindowInfo? Refresh(BoundWindowInfo current)
    {
        if (!IsAlive(current.Handle))
        {
            return new BoundWindowInfo
            {
                Handle = current.Handle,
                Title = current.Title,
                ClassName = current.ClassName,
                ProcessId = current.ProcessId,
                ProcessName = current.ProcessName,
                ClientWidth = current.ClientWidth,
                ClientHeight = current.ClientHeight,
                ClientScreenOriginX = current.ClientScreenOriginX,
                ClientScreenOriginY = current.ClientScreenOriginY,
                IsValid = false
            };
        }

        return BuildInfo(current.Handle);
    }

    public bool IsAlive(nint hwnd) => hwnd != nint.Zero && NativeMethods.IsWindow(hwnd);

    private static nint GetRootWindow(nint hwnd)
    {
        // Walk to top-level owner for stable binding in v1.
        var current = hwnd;
        while (true)
        {
            var parent = GetParent(current);
            if (parent == nint.Zero)
            {
                return current;
            }

            current = parent;
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern nint GetParent(nint hWnd);

    private static BoundWindowInfo BuildInfo(nint hwnd)
    {
        var title = new StringBuilder(512);
        NativeMethods.GetWindowText(hwnd, title, title.Capacity);

        var className = new StringBuilder(256);
        NativeMethods.GetClassName(hwnd, className, className.Capacity);

        NativeMethods.GetClientRect(hwnd, out var client);
        var origin = new NativeMethods.POINT { X = 0, Y = 0 };
        NativeMethods.ClientToScreen(hwnd, ref origin);

        NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
        var processName = string.Empty;
        try
        {
            processName = Process.GetProcessById((int)pid).ProcessName;
        }
        catch
        {
            // Process may have exited or be inaccessible.
        }

        return new BoundWindowInfo
        {
            Handle = hwnd,
            Title = title.ToString(),
            ClassName = className.ToString(),
            ProcessId = (int)pid,
            ProcessName = processName,
            ClientWidth = client.Width,
            ClientHeight = client.Height,
            ClientScreenOriginX = origin.X,
            ClientScreenOriginY = origin.Y,
            IsValid = true
        };
    }
}
