using System.Windows;
using WinSpot.Models;

namespace WinSpot.Services;

public interface IWindowBindService
{
    BoundWindowInfo? TryResolveFromScreenPoint(Point screenPoint);
    BoundWindowInfo? Refresh(BoundWindowInfo current);
    bool IsAlive(nint hwnd);
}

public interface IClientProbeService
{
    bool TrySamplePoint(nint hwnd, Point screenPoint, out ClientPointSample? sample);
    ClientRegion ClipRegionToClient(nint hwnd, ClientRegion raw);
}

public interface ICaptureService
{
    CaptureResult CaptureClientArea(nint hwnd, CaptureSettings settings, string? windowTitle = null);
}

public interface IHotkeyService
{
    bool TryRegister(nint windowHandle, int id, uint modifiers, uint virtualKey, out string? error);
    void Unregister(nint windowHandle, int id);
}

public interface ISettingsService
{
    CaptureSettings Load();
    void Save(CaptureSettings settings);
}
