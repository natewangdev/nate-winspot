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

Product versioning follows the constitution (Principle VIII):

1. **SemVer** — Git tags look like `vX.Y.Z` (e.g. `v0.1.0`).
2. **CI on `main`** — After merge to `main`, [`.github/workflows/release.yml`](.github/workflows/release.yml) computes the next version from [Conventional Commits](https://www.conventionalcommits.org/) since the last stable product tag.
3. **Skip empty releases** — No `feat` / `fix` / breaking change → no tag and no GitHub Release.
4. **Artifacts** — Release attaches published `WinSpot.exe`; binary version MUST match the tag (`v1.2.3` ↔ `1.2.3`).
5. **Pre-release** — Optional `vX.Y.Z-beta.N` is out of band; the default `main` workflow ships **stable** tags only.

Use Conventional Commit messages on PRs merged to `main` (squash titles count). Examples: `feat: …`, `fix: …`, `feat!: …` or a body line `BREAKING CHANGE: …`.

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
