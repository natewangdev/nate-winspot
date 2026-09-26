namespace WinSpot.Models;

public sealed class BoundWindowInfo
{
    public nint Handle { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public int ClientWidth { get; init; }
    public int ClientHeight { get; init; }
    public int ClientScreenOriginX { get; init; }
    public int ClientScreenOriginY { get; init; }
    public bool IsValid { get; init; } = true;

    /// <summary>HWND as decimal digits only (copy/display format).</summary>
    public string HandleText => Handle.ToInt64().ToString();
    public string ClientSizeText => $"{ClientWidth} × {ClientHeight}";
}
