# WinSpot

[English](README.md)

Windows 桌面工具：窗口绑定、客户区坐标/取色、框选区域、快捷键客户区截图。

文档以英文为主；本文件为简体中文对照。规格驱动开发使用 [GitHub Spec Kit](https://github.com/github/spec-kit)。功能需求另有中文阅读版 `spec.zh-CN.md`，**不参与** Spec Kit 流程。

## 功能

- 拖拽准星绑定窗口，查看句柄、标题、类名、进程、客户区大小
- 拾取客户区相对坐标与像素颜色（可复制）
- 框选客户区区域（可复制 `x1,y1,x2,y2`），拖拽时显示橡皮筋
- PrtSc 客户区截图（DXGI Desktop Duplication，与 dxcam 同技术路径）
- 发布为自包含单文件 `win-x64` exe

## 技术栈

- C# / WPF / .NET 8
- Win32 互操作（user32 / gdi32）
- Vortice.DXGI / Vortice.Direct3D11（桌面复制）
- Spec Kit（`specify` CLI）+ `.cursor/skills` 下的 Cursor Agent 技能

## 前置条件

- Windows 10/11 x64
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- 可选：[uv](https://docs.astral.sh/uv/) + `specify-cli`（Spec Kit 工作流）

## 快速开始

```powershell
dotnet restore WinSpot.sln
dotnet run --project src\WinSpot\WinSpot.csproj
```

### 发布单文件 exe

```powershell
dotnet publish src\WinSpot\WinSpot.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

产物：`publish\WinSpot.exe`

## 配置

设置保存在 `%AppData%\WinSpot\settings.json`：

| 键 | 说明 | 默认值 |
|-----|------|--------|
| `saveDirectory` | 截图目录 | `%USERPROFILE%\Pictures\WinSpot` |
| `imageFormat` | `Png` / `Jpeg` / `Bmp` | `Jpeg` |
| `jpegQuality` | 1–100 | `90` |
| `filenameTemplate` | 可用 `{yyyyMMdd_HHmmss}`、`{yyyyMMdd}`、`{HHmmss}`（可选 `{title}`） | `{yyyyMMdd_HHmmss}` |

热键：**PrtSc**（Print Screen，全局，v1 固定）。冲突时在状态栏提示。若注册失败，可在「设置 → 辅助功能 → 键盘」关闭「使用 Print Screen 键打开屏幕截图」后重试。

## 版本与发布

本仓库采用 **tag 驱动发版**（与 nate-game-engine 相同）：**git tag 即为版本号唯一来源**。
**合并到 `main` 不会发布。** 推送 `v*` tag 后会构建 `WinSpot.exe`、创建 GitHub Release 并挂上产物（[`.github/workflows/release.yml`](.github/workflows/release.yml)）。

```powershell
# 合并到 main（或选定要发布的 commit）之后：
git tag v0.1.0
git push origin v0.1.0
```

规则（宪章原则 VIII）：

1. **SemVer 标签** — `vX.Y.Z`（例如 `v0.1.0`）。可选预发布：`vX.Y.Z-beta.N`。
2. **Tag → CI** — 仅推送 `v*` tag 会触发发版工作流。
3. **产物** — Release 附件为 `WinSpot.exe`；二进制内嵌版本与 tag 一致（`v1.2.3` ↔ `1.2.3`）。
4. **勿复用** 已存在的 GitHub Release / tag / 版本号。

PR 建议使用 Conventional Commits，便于历史阅读与自动生成说明。
宪章自身的版本号与产品 SemVer 相互独立，请勿混用。

## Spec Kit

当前功能：`specs/001-window-spot-tool/`（见 `.specify/feature.json`）。

- 英文（Spec Kit 权威输入）：`spec.md`
- 中文（仅供阅读）：`spec.zh-CN.md`

```text
/speckit-constitution
/speckit-specify
/speckit-plan
/speckit-tasks
/speckit-implement
/speckit-converge
```

宪章：`.specify/memory/constitution.md`

## 目录结构

```text
src/WinSpot/          # WPF 应用
specs/001-window-spot-tool/  # 功能规格（英 + 中文阅读版）、计划、任务
.specify/             # Spec Kit 基础设施
.cursor/skills/       # Cursor Agent Spec Kit 技能
README.md / README.zh-CN.md
```

## 许可证

若仓库含 LICENSE，以该文件为准。
