# Tasks: Probe Range Copy-Idle

**Input**: Design documents from `/specs/002-probe-range-copy/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/probe-range.md, quickstart.md

**Tests**: Not requested — no TDD tasks.

**Organization**: US1 copy-idle (P1 MVP), then US2 Range tab (P2).

## Phase 1: Setup

**Purpose**: Confirm existing WPF app is the implementation target (no new solution).

- [x] T001 Confirm `src/WinSpot/WinSpot.csproj` is the single app to change and no extra NuGet packages are required per `specs/002-probe-range-copy/plan.md`

---

## Phase 2: Foundational

**Purpose**: Shared copy-idle helper used by US1 and later Range copies.

**⚠️ CRITICAL**: Complete before US1 UI wiring

- [x] T002 Add `CopyText` success vs empty handling and a `SelectNoneAfterProbeResultCopy` path in `src/WinSpot/MainWindow.xaml.cs` so successful result copies can close overlay and select **无** without suppressing bind-field copies

**Checkpoint**: Foundation ready

---

## Phase 3: User Story 1 - Copy on Pick/Marquee Returns to Idle (Priority: P1) 🎯 MVP

**Goal**: Copy coordinate, copy color, or copy region successfully selects None; bind copies do not; last results remain.

**Independent Test**: Pick then 复制坐标 → 无; reselect 拾取 shows last values. Same for 复制颜色 and 框选 复制区域. Copy 句柄 while 拾取 stays on 拾取.

### Implementation for User Story 1

- [x] T003 [US1] Wire `CopyPickCoord_Click` and `CopyPickColor_Click` in `src/WinSpot/MainWindow.xaml.cs` to idle after successful copy
- [x] T004 [US1] Wire `CopyRegion_Click` in `src/WinSpot/MainWindow.xaml.cs` to idle after successful copy
- [x] T005 [US1] Update **无** / **拾取** / **框选** hint text in `src/WinSpot/MainWindow.xaml` so copy-to-idle is described alongside Esc

**Checkpoint**: US1 independently demoable

---

## Phase 4: User Story 2 - Range Mode Live Distance and Angle (Priority: P2)

**Goal**: 测距 tab with live preview, click-commit, copy-idle, Esc/None exit.

**Independent Test**: Follow `specs/002-probe-range-copy/quickstart.md` steps 5–10.

### Implementation for User Story 2

- [x] T006 [P] [US2] Add `ClientRangeSample` in `src/WinSpot/Models/ClientRangeSample.cs` with Pointer/Center doubles, Distance, AngleDegrees in [−180, 180], IsValid, and display/copy strings (distance integer or one decimal; angle at least one decimal)
- [x] T007 [P] [US2] Add `RangeMeasurement` in `src/WinSpot/Helpers/RangeMeasurement.cs`: center `(width/2, height/2)`, Euclidean distance, `Atan2(dx, -dy)` degrees wrapped to [−180, 180], angle 0 at coincidence
- [x] T008 [US2] Extend `IClientProbeService` in `src/WinSpot/Services/ServiceContracts.cs` with `TryMeasureRange` and implement it in `src/WinSpot/Services/CaptureAndProbeServices.cs` using ScreenToClient + GetClientRect + `RangeMeasurement`
- [x] T009 [US2] Extend `src/WinSpot/ProbeOverlayWindow.cs` with Range mode: `PointerMoved`, click `PointPicked` without GetPixel hide, owner exclusion unchanged
- [x] T010 [US2] Add **测距** tab (live + committed distance/angle + copy) in `src/WinSpot/MainWindow.xaml`
- [x] T011 [US2] Wire Range overlay, live vs committed, off-client ignore, Esc/None, and Range result copy-idle in `src/WinSpot/MainWindow.xaml.cs`
- [x] T012 [US2] Update None-tab hint in `src/WinSpot/MainWindow.xaml` to mention 测距

**Checkpoint**: US1 + US2 work together

---

## Phase 5: Polish & Cross-Cutting

- [x] T013 [P] Update Features in `README.md` for copy-idle and Range
- [x] T014 [P] Update 功能 in `README.zh-CN.md` equivalently
- [x] T015 Build `src/WinSpot/WinSpot.csproj` and fix compile errors

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 → US1 → US2 → Polish
- T006/T007 parallel; T008 after T006+T007; T009 after overlay design; T010/T011 sequential on MainWindow
- T013/T014 parallel after US2

### Parallel Example: User Story 2

```text
T006 ClientRangeSample.cs
T007 RangeMeasurement.cs
```

---

## Implementation Strategy

MVP: T001–T005 (copy-idle). Then Range T006–T012. Docs T013–T014. Compile T015.
