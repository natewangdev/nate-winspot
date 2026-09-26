# Implementation Plan: WinSpot Window Spot Tool

**Branch**: `001-window-spot-tool` | **Date**: 2026-09-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-window-spot-tool/spec.md`

## Summary

Build **WinSpot**, a Windows desktop assistant that binds a target window via drag, displays identity/geometry, picks client-relative coordinates and pixel color, marquees client regions, and captures the client area via F11 with persistent export settings. Implementation uses **C# / WPF on .NET 8**, Win32 interop for geometry/pixel/capture, and **single-file self-contained `win-x64` publish**.

## Technical Context

**Language/Version**: C# / .NET 8 (LTS)

**Primary Dependencies**: WPF; Win32 P/Invoke (user32/gdi32); Vortice.DXGI + Vortice.Direct3D11 (dxcam-equivalent Desktop Duplication); WIC/`BitmapEncoder` for encode; System.Text.Json for settings

**Storage**: Local JSON settings under `%AppData%\WinSpot\settings.json`; screenshot files under user-configured directory

**Testing**: Manual quickstart scenarios first; optional xUnit for pure helpers (coordinate math, filename template) if added later

**Target Platform**: Windows 10/11 x64 (Per-Monitor DPI aware)

**Project Type**: Desktop WPF application (single solution)

**Performance Goals**: UI remains responsive during bind hover; F11 client capture completes under ~2s for typical window sizes; pick/marquee feedback feels immediate (<100ms perception)

**Constraints**: No process injection; single bound window; global hotkey with conflict handling; accurate DPI/client transforms; single-file publish

**Scale/Scope**: One main window UI; ~5 user stories; no server/cloud

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| Spec-Driven Delivery | PASS | spec → plan → tasks before full feature code |
| Non-Invasive Introspection | PASS | Public Win32 only; no injection |
| Accuracy Over Cleverness | PASS | ScreenToClient / DXGI Desktop Duplication (dxcam-like); explicit errors |
| User-Copyable Insights | PASS | Clipboard helpers for all key fields |
| Simplicity & Single-Binary | PASS | One WPF project; PublishSingleFile self-contained |
| Tech Constraints (C# WPF .NET 8, win-x64) | PASS | Declared in Technical Context |

Post-design re-check: PASS — structure stays one app + services; no unjustified complexity.

## Project Structure

### Documentation (this feature)

```text
specs/001-window-spot-tool/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── win32-services.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
└── WinSpot/
    ├── WinSpot.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── Models/
    │   ├── BoundWindowInfo.cs
    │   ├── ClientPointSample.cs
    │   ├── ClientRegion.cs
    │   └── CaptureSettings.cs
    ├── Services/
    │   ├── IWindowBindService.cs
    │   ├── WindowBindService.cs
    │   ├── IClientProbeService.cs
    │   ├── ClientProbeService.cs
    │   ├── ICaptureService.cs
    │   ├── CaptureService.cs
    │   ├── IHotkeyService.cs
    │   ├── HotkeyService.cs
    │   ├── ISettingsService.cs
    │   └── SettingsService.cs
    ├── Native/
    │   └── NativeMethods.cs
    └── Helpers/
        ├── ClipboardHelper.cs
        └── FilenameTemplate.cs

publish/   # output of `dotnet publish` (gitignored)
```

**Structure Decision**: Single WPF project under `src/WinSpot` keeps the constitution’s simplicity gate. Win32 is isolated in `Native/` + service interfaces so UI stays thin.

## Complexity Tracking

> No constitution violations requiring justification.
