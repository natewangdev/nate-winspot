# Quickstart: WinSpot

## Prerequisites

- Windows 10/11 x64
- .NET 8 SDK (for build)
- Spec Kit optional for continuing SDD workflow

## Build & Run

```powershell
dotnet restore src\WinSpot\WinSpot.csproj
dotnet run --project src\WinSpot\WinSpot.csproj
```

## Publish single-file exe

```powershell
dotnet publish src\WinSpot\WinSpot.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

Run `publish\WinSpot.exe`.

## Manual validation (maps to user stories)

1. **Chrome**: Launch → no in-content “WinSpot” title/intro banner; OS window title is `WinSpot`; exe/window/taskbar show “W” icon.
2. **Bind**: Drag crosshair to Notepad → verify HWND/title/client size; copy a field → button brief success + top toast `已复制` (~2s).
3. **Pick**: Select **拾取** tab → WinSpot UI still clickable under the pointer → click bound client outside WinSpot → verify `x,y` and `#RRGGBB` in that tab; tab stays on **拾取**; Esc → **无**.
4. **Marquee**: Select **框选** tab → drag on bound client outside WinSpot → rubber-band → verify `x1,y1,x2,y2`; tab stays on **框选** until Esc.
5. **Idle tab**: Select **无** (or Esc from pick/marquee) → probe inactive.
6. **Capture**: Default JPEG + time-only filename; press F11 → open image; content must match on-screen client (not gray); size = client size.
7. **Settings persist**: Change JPEG quality → restart → confirm value retained.
8. **Failure path**: Close bound window → F11 → clear error, no bogus success file.

## Spec Kit continue

```text
/speckit-implement
/speckit-converge
```
