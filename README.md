# WinSpot

[中文](README.zh-CN.md)

Windows desktop tool for window binding, client-area coordinate/color pickup, region marquee, and hotkey client capture.

English is the primary documentation language. Spec-Driven Development via [GitHub Spec Kit](https://github.com/github/spec-kit). Feature requirements also provide a Chinese viewing copy (`spec.zh-CN.md`) that does not feed Spec Kit.

## Features

- Drag-bind a window and inspect HWND, title, class, process, client size
- Pick client-relative coordinates and pixel color (copyable)
- Marquee a client region (copyable `x1,y1,x2,y2`) with visible rubber-band
- F11 client-area screenshot via DXGI Desktop Duplication (dxcam-equivalent)
- Single-file self-contained `win-x64` publish

## Tech stack

- C# / WPF / .NET 8
- Win32 interop (user32 / gdi32)
- Vortice.DXGI / Vortice.Direct3D11 (Desktop Duplication)
- Spec Kit (`specify` CLI) + Cursor Agent skills under `.cursor/skills`

## Prerequisites

- Windows 10/11 x64
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Optional: [uv](https://docs.astral.sh/uv/) + `specify-cli` for Spec Kit workflows

## Quick start

```powershell
dotnet restore WinSpot.sln
dotnet run --project src\WinSpot\WinSpot.csproj
```

### Publish single-file exe

```powershell
dotnet publish src\WinSpot\WinSpot.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

Output: `publish\WinSpot.exe`

## Configuration

Settings persist at `%AppData%\WinSpot\settings.json`:

| Key | Description | Default |
|-----|-------------|---------|
| `saveDirectory` | Screenshot folder | `%USERPROFILE%\Pictures\WinSpot` |
| `imageFormat` | `Png` / `Jpeg` / `Bmp` | `Jpeg` |
| `jpegQuality` | 1–100 | `90` |
| `filenameTemplate` | Tokens `{yyyyMMdd_HHmmss}`, `{yyyyMMdd}`, `{HHmmss}` (optional `{title}`) | `{yyyyMMdd_HHmmss}` |

Hotkey: **F11** (global). Conflicts show a status message.

## Versioning & releases

This repo uses **tag-driven releases** (same approach as nate-game-engine): the
git tag is the source of truth. **Merging to `main` does not publish.** Pushing
a `v*` tag builds `WinSpot.exe`, creates a GitHub Release, and attaches the
binary ([`.github/workflows/release.yml`](.github/workflows/release.yml)).

```powershell
# After merge to main (or on the commit you want to ship):
git tag v0.1.0
git push origin v0.1.0
```

Rules (constitution Principle VIII):

1. **SemVer tags** — `vX.Y.Z` (e.g. `v0.1.0`). Optional pre-release: `vX.Y.Z-beta.N`.
2. **Tag → CI** — Only `v*` tag pushes run the release workflow.
3. **Artifacts** — Release attaches published `WinSpot.exe`; binary version matches the tag (`v1.2.3` ↔ `1.2.3`).
4. **Do not reuse** a tag / version that already exists as a GitHub Release.

Prefer Conventional Commits on PRs for readable history and generated notes.
Constitution version (governance doc) is separate from product SemVer.

## Spec Kit

Active feature: `specs/001-window-spot-tool/` (see `.specify/feature.json`).

- English (Spec Kit authoritative): `spec.md`
- Chinese (viewing only): `spec.zh-CN.md`

```text
/speckit-constitution
/speckit-specify
/speckit-plan
/speckit-tasks
/speckit-implement
/speckit-converge
```

Constitution: `.specify/memory/constitution.md`

## Project layout

```text
src/WinSpot/          # WPF application
specs/001-window-spot-tool/  # Feature spec (EN + ZH viewing copy), plan, tasks
.specify/             # Spec Kit infrastructure
.cursor/skills/       # Cursor Agent Spec Kit skills
README.md / README.zh-CN.md
```

## License

See repository license if present.
