# Contract: Win32 / Application Services

Internal service contracts (not HTTP). Implemented by `src/WinSpot/Services`.

## IWindowBindService

- `BoundWindowInfo? TryResolveFromScreenPoint(Point screenPoint)`
- `BoundWindowInfo? Refresh(BoundWindowInfo current)`
- `bool IsAlive(nint hwnd)`

## IClientProbeService

- `bool TrySamplePoint(nint hwnd, Point screenPoint, out ClientPointSample sample)`
- `ClientRegion ClipRegionToClient(nint hwnd, ClientRegion raw)`
- Requires client-relative output; fails clearly if hwnd invalid.

## ICaptureService

- `CaptureResult CaptureClientArea(nint hwnd, CaptureSettings settings, string? windowTitle = null)`
- Backend: **DXGI Desktop Duplication** (dxcam-equivalent). Crop composed desktop to client screen rect.
- Output image dimensions MUST equal client width×height on success.
- MUST NOT use GDI BitBlt/GetDC as the screenshot backend.

## IHotkeyService

- `bool TryRegister(ModifierKeys mods, Key key, Action callback, out string? error)` / HWND-based register
- `void UnregisterAll()` / per-id unregister

## ISettingsService

- `CaptureSettings Load()`
- `void Save(CaptureSettings settings)`
- Defaults: `ImageFormat=Jpeg`, `FilenameTemplate={yyyyMMdd_HHmmss}`

## Copy Formats (UI contract)

| Value | Clipboard text |
|-------|----------------|
| HWND | decimal digits only, e.g. `265098` (no `0x` hex) |
| Coordinates | `{x},{y}` |
| Color | `#RRGGBB` |
| Region | `{x1},{y1},{x2},{y2}` (top-left, bottom-right) |
