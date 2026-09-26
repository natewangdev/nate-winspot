# Research: WinSpot Window Spot Tool

**Date**: 2026-09-27 | **Feature**: 001-window-spot-tool

## Decision 1: UI stack — WPF on .NET 8

- **Decision**: WPF desktop app targeting `net8.0-windows`.
- **Rationale**: Matches product requirement; mature HWND interop; sufficient for a single-tool window UI.
- **Alternatives considered**: WinUI 3 (heavier packaging story for single-file); WinForms (weaker layout for multi-panel inspector).

## Decision 2: Window binding via drag crosshair

- **Decision**: Toolbar “crosshair” capture: on mouse move while dragging, `WindowFromPoint` (or equivalent) resolves HWND under cursor; optional highlight via `DrawFrameControl`/`FrameRect` or temporary layered outline; on mouse up, bind HWND.
- **Rationale**: Familiar Spy++/inspector UX; no need for window list enumeration as primary path.
- **Alternatives considered**: Dropdown of top-level windows only (less precise for child targets — v1 binds top-level unless research during implement shows child bind is trivial).

## Decision 3: Client coordinates & color

- **Decision**: Convert screen point with `ScreenToClient`; sample color via `GetDC` + `GetPixel` on the target HWND (or capture 1×1 via `BitBlt` if GetPixel is unreliable under DWM for some apps — prefer BitBlt 1×1 from client DC as primary for consistency with screenshot path).
- **Rationale**: Aligns pick color with capture pipeline; better consistency under composition.
- **Alternatives considered**: Desktop DC GetPixel only (fails more often with DPI/DWM).

## Decision 4: Marquee overlay

- **Decision**: While in marquee mode, use a full-screen (or target-covering) transparent overlay window that captures mouse drag, maps to client coords, clips to client rect, draws rubber-band.
- **Rationale**: Reliable mouse capture without injecting into the target.
- **Alternatives considered**: Subclassing target window (invasive — rejected by constitution).

## Decision 5: Hotkey

- **Decision**: `RegisterHotKey` on the WPF main window HWND for `VK_F11`; unregister on exit. On failure, show non-fatal banner.
- **Rationale**: Simple, no low-level hook required for a single hotkey.
- **Alternatives considered**: WH_KEYBOARD_LL (more power, more complexity/AV noise).

## Decision 6: Screenshot backend & settings (amended)

- **Decision**: Client capture via **DXGI Desktop Duplication** (same technique as Python **dxcam**): duplicate the output covering the bound client screen rect, acquire a composed frame, crop to the client rectangle, then encode with WPF `BitmapEncoder` (default **JPEG**, quality configurable). Filename default `{yyyyMMdd_HHmmss}`. Settings JSON in `%AppData%\WinSpot\settings.json`.
- **Rationale**: GDI `BitBlt`/`GetDC` often yields blank/gray frames under DWM; DXGI reads the composed desktop like dxcam.
- **Alternatives considered**: GDI BitBlt (rejected — gray frames); PrintWindow (inconsistent); embedding Python dxcam (rejected — breaks single-file C# delivery).
- **Implementation note**: Use Vortice.DXGI + Vortice.Direct3D11 bindings; hide/minimize WinSpot briefly during capture so the tool UI is not composited over the target.

## Decision 7: DPI awareness

- **Decision**: Declare Per-Monitor V2 DPI awareness in app manifest; perform all geometry in physical pixels consistent with Win32 client rects shown to the user.
- **Rationale**: Spec requires accurate coordinates on modern Windows.
- **Alternatives considered**: System DPI awareness only (incorrect on mixed-DPI setups).

## Decision 8: Publish

- **Decision**: `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true`
- **Rationale**: Meets FR-011 / SC-006.
- **Alternatives considered**: Framework-dependent single-file (fails clean-machine requirement).
