# Implementation Plan: Probe Range Copy-Idle

**Branch**: `006-probe-range-copy` | **Date**: 2026-10-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-probe-range-copy/spec.md`

**Note**: Spec Kit `setup-plan` reports feature dir `002-probe-range-copy`; git branch is `006-probe-range-copy`.

## Summary

Extend WinSpot probe tabs: (1) successful **result** copies on Pick, Marquee, and Range select **None** and tear down the probe overlay, without clearing last results; (2) add **测距 / Range** tab with live Euclidean distance from client geometric center and signed angle from straight-up, clockwise-positive, click-commit, then copy-idle.

Reuse existing WPF overlay (owner punched out), `IClientProbeService`, and copy toast. Add a small pure geometry helper for distance/angle; extend overlay with a range mode that reports pointer move + click without color sampling.

## Technical Context

**Language/Version**: C# / .NET 8 (LTS)

**Primary Dependencies**: WPF; existing Win32 P/Invoke (`ScreenToClient`, `GetClientRect`); no new NuGet packages

**Storage**: N/A (range samples are session-only; capture settings unchanged)

**Testing**: Manual quickstart; optional `dotnet` compile as gate. No new test project unless a tiny helper is isolated enough to unit-test later.

**Target Platform**: Windows 10/11 x64 (Per-Monitor DPI aware)

**Project Type**: Desktop WPF application (existing `src/WinSpot`)

**Performance Goals**: Live range preview updates on pointer move without UI freeze (same overlay path as pick; skip pixel read)

**Constraints**: No process injection; no on-target ray overlay; WinSpot-under-pointer remains interactive; single bound window

**Scale/Scope**: One extra tab + overlay mode + copy-idle wiring; README bilingual update

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| Spec-Driven Delivery | PASS | spec → this plan → tasks → implement |
| Non-Invasive Introspection | PASS | Public Win32 geometry only; no injection; no on-target drawing |
| Accuracy Over Cleverness | PASS | Client center `(w/2,h/2)`; Euclidean distance; documented angle convention; errors when unbound / off-client |
| User-Copyable Insights | PASS | Committed distance and angle copyable; FR-014 toast unchanged |
| Simplicity & Single-Binary | PASS | No new projects/deps; stay in `src/WinSpot` |
| Tech Constraints | PASS | C# WPF .NET 8 win-x64 unchanged |
| Bilingual Spec Companion | PASS | spec.md + spec.zh-CN.md already synced |
| Bilingual README | PASS (design) | README.md + README.zh-CN.md updated in implement polish |

Post-design re-check: PASS — Range is overlay + helper + tab UI; idle-after-copy is a few call sites.

## Project Structure

### Documentation (this feature)

```text
specs/002-probe-range-copy/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── probe-range.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
src/WinSpot/
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── ProbeOverlayWindow.cs
├── Models/ClientRangeSample.cs   # new
├── Helpers/RangeMeasurement.cs   # new (pure math)
├── Services/ServiceContracts.cs  # IClientProbeService.TryMeasureRange
└── Services/CaptureAndProbeServices.cs
```

**Structure Decision**: Keep the single WPF project. Overlay gains a third mode rather than a second window type.

## Complexity Tracking

> No constitution violations requiring justification.
