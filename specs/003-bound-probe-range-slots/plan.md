# Implementation Plan: Bound-Window Probe & Range Slots

**Branch**: `007-bound-probe-range-slots` | **Date**: 2026-10-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-bound-probe-range-slots/spec.md`

**Note**: Spec Kit setup-plan may report dir `003-bound-probe-range-slots`; git branch is `007-bound-probe-range-slots`.

## Summary

Amend WinSpot so (1) copying Pick/Marquee/Range results **does not** switch to None; (2) the probe overlay’s **hit-test region is only the bound client** (minus WinSpot if overlapping), so other apps get normal clicks and a normal cursor; (3) Range keeps live preview and stores **10 read-only** distance/angle slots with Clear and a full-set toast `测距已满（10/10）`.

## Technical Context

**Language/Version**: C# / .NET 8

**Primary Dependencies**: WPF; existing Win32 (`SetWindowRgn`, `GetClientRect`, `ClientToScreen`); no new packages

**Storage**: Session-only range slots (not settings.json)

**Testing**: Manual quickstart; `dotnet build` gate

**Target Platform**: Windows 10/11 x64, Per-Monitor DPI

**Project Type**: Existing desktop WPF app `src/WinSpot`

**Performance Goals**: Overlay region refresh ~30–60 Hz while armed so moving/resizing the target stays accurate; UI stays responsive

**Constraints**: No injection; no on-target drawing; public Win32 only

**Scale/Scope**: Overlay region change + Range tab 10-row UI + copy/toast tweaks + README

## Constitution Check

| Gate | Status | Notes |
|------|--------|-------|
| Spec-Driven Delivery | PASS | spec → plan → tasks → implement |
| Non-Invasive Introspection | PASS | Region uses documented window metrics only |
| Accuracy Over Cleverness | PASS | Client rect in screen pixels; live math unchanged from 002 |
| User-Copyable Insights | PASS | Each slot copyable; copy-idle removed |
| Simplicity & Single-Binary | PASS | Same project; GDI region already used |
| Bilingual Spec | PASS | spec + spec.zh-CN |
| Bilingual README | PASS at implement | Update both READMEs |

Post-design: PASS.

## Project Structure

### Documentation

```text
specs/003-bound-probe-range-slots/
├── plan.md, research.md, data-model.md, quickstart.md
├── contracts/probe-hit-region.md
└── tasks.md
```

### Source

```text
src/WinSpot/MainWindow.xaml(.cs)
src/WinSpot/ProbeOverlayWindow.cs
src/WinSpot/Native/NativeMethods.cs   # RGN_AND if needed
README.md, README.zh-CN.md
```

**Structure Decision**: Keep single WPF project; tighten overlay `SetWindowRgn` instead of a second window.

## Complexity Tracking

> No constitution violations.
