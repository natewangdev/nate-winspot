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

## Decision 4: Marquee / pick overlay (amended 2026-09-30)

- **Decision**: Use a transparent overlay for pick/marquee outside WinSpot, but **hit-test through** (or exclude) the WinSpot main window region so the tool keeps normal cursor and clicks. Outside WinSpot show a mode crosshair/cross cursor. Completing a sample does **not** close the mode / switch to None; Esc or selecting None ends the mode. Overlay may stay armed for repeat picks/marquees.
- **Rationale**: FR-017 — tool must remain usable while probe mode is selected; continuous sampling without re-selecting the tab.
- **Alternatives considered**: Full-screen exclusive overlay that blocks WinSpot (rejected); auto-return to None after each sample (rejected).

## Decision 5: Hotkey

- **Decision**: `RegisterHotKey` on the WPF main window HWND for `VK_SNAPSHOT` (PrtSc / Print Screen); unregister on exit. On failure, show non-fatal banner. Do **not** use `VK_F12` — Windows reserves F12 for the debugger and registration fails.
- **Rationale**: Simple, no low-level hook required for a single hotkey. PrtSc may still conflict if Windows Settings enables “Use the Print screen key to open screen snipping”.
- **Alternatives considered**: `VK_F11` (works but user requested PrtSc); `VK_F12` (rejected — debugger-reserved); WH_KEYBOARD_LL (more power, more complexity/AV noise).

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

## Decision 9: Probe mode UI — tabs (2026-09-30)

- **Decision**: Replace radio buttons with a `TabControl` of three tabs: **None / Pick / Marquee**. Each tab hosts only that mode’s results and hints; selecting a tab activates the mode.
- **Rationale**: Clarification session; larger hit targets than radios; matches FR-013.
- **Alternatives considered**: Shared results panel under radios; two tabs without None (rejected — keep idle default).

## Decision 10: Copy feedback (2026-09-30)

- **Decision**: On successful clipboard copy, briefly restyle/relabel the clicked button (~1.5s success state) and show a top-of-window overlay toast with text `已复制` (~2s auto-dismiss). Non-blocking.
- **Rationale**: Clarification session; FR-014 / SC-007.
- **Alternatives considered**: Toast-only; persistent button label until next copy; manual toast dismiss.

## Decision 11: Application icon (2026-09-30)

- **Decision**: Ship `Assets/app.ico` as an abstract **“W”** letter mark; set `ApplicationIcon` in the csproj so exe, window, and taskbar share it.
- **Rationale**: Clarification chose letter mark over crosshair/eyedropper metaphors; FR-015 / SC-008.
- **Alternatives considered**: Crosshair+window; eyedropper; marquee rectangle.

## Decision 12: Remove in-content title banner (2026-09-30)

- **Decision**: Remove the dark content-area header showing “WinSpot” + feature intro. Keep WPF `Window.Title = "WinSpot"`.
- **Rationale**: Clarification session; FR-016 / SC-009.
- **Alternatives considered**: Blank OS title; keep large in-content title only.
