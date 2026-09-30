# Tasks: WinSpot Window Spot Tool

**Input**: Design documents from `/specs/001-window-spot-tool/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not requested in spec — omit formal test tasks; validate via quickstart.md

**Organization**: Tasks grouped by user story for incremental delivery.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1…US5 mapping to spec priorities
- Paths follow `src/WinSpot/` from plan.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution and project scaffolding

- [x] T001 Create `src/WinSpot/` WPF project structure per plan.md
- [x] T002 Initialize `WinSpot.csproj` as `net8.0-windows` WPF with single-file publish properties
- [x] T003 [P] Add `.gitignore` entries for `bin/`, `obj/`, `publish/`
- [x] T004 [P] Update root `README.md` with build/run/publish and Spec Kit pointers

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared models, native layer, settings, clipboard — blocks all stories

**⚠️ CRITICAL**: No user story UI/feature work until this phase completes

- [x] T005 [P] Create models in `src/WinSpot/Models/` (`BoundWindowInfo`, `ClientPointSample`, `ClientRegion`, `CaptureSettings`, `CaptureResult`)
- [x] T006 [P] Create `src/WinSpot/Native/NativeMethods.cs` P/Invoke surface (window/rect/DC/BitBlt/hotkey)
- [x] T007 [P] Implement `ClipboardHelper` and `FilenameTemplate` in `src/WinSpot/Helpers/`
- [x] T008 Implement `ISettingsService` / `SettingsService` JSON persistence under `%AppData%\WinSpot\`
- [x] T009 Wire service lifetimes in `App.xaml.cs` and empty `MainWindow` shell layout sections
- [x] T010 Add application manifest Per-Monitor DPI V2 awareness

**Checkpoint**: Foundation ready — user stories can begin

---

## Phase 3: User Story 1 - Bind Window & Inspect Info (Priority: P1) 🎯 MVP

**Goal**: Drag-bind a window and show copyable HWND/title/client size (+ related fields)

**Independent Test**: Bind Notepad; verify fields; copy works; clear/rebind works

### Implementation for User Story 1

- [x] T011 [P] [US1] Implement `IWindowBindService` / `WindowBindService` in `src/WinSpot/Services/`
- [x] T012 [US1] Add bind crosshair control + drag interaction in `MainWindow.xaml(.cs)`
- [x] T013 [US1] Bind info panel UI with copy buttons for handle/title/client size
- [x] T014 [US1] Support clear binding and refresh when target moves/resizes/title changes
- [x] T015 [US1] Handle invalid HWND with clear status message

**Checkpoint**: MVP — bind + inspect demoable

---

## Phase 4: User Story 2 - Client Click Pick (Priority: P2)

**Goal**: Pick mode reports relative client `(x,y)` and color, both copyable

**Independent Test**: Pick known points; compare coords/color to reference

### Implementation for User Story 2

- [x] T016 [P] [US2] Implement point sampling in `IClientProbeService` / `ClientProbeService`
- [x] T017 [US2] Add pick mode toggle + overlay/click capture path in UI
- [x] T018 [US2] Display and copy coordinate + HEX/RGB sample results
- [x] T019 [US2] Ignore or hint on clicks outside bound client area

**Checkpoint**: US1 + US2 independently usable

---

## Phase 5: User Story 3 - Client Region Marquee (Priority: P3)

**Goal**: Marquee relative region with clip + copyable description

**Independent Test**: Drag known region; verify clipped rect text

### Implementation for User Story 3

- [x] T020 [US3] Extend `ClientProbeService` with region clip helpers
- [x] T021 [US3] Implement marquee overlay drag UI (mutually exclusive with pick)
- [x] T022 [US3] Show/copy region as `x1,y1,x2,y2` format from contracts
- [x] T023 [US3] Enforce client-area clipping on mouse-up
- [x] T023b [US3] Show rubber-band outline while marquee dragging

**Checkpoint**: US1–US3 complete

---

## Phase 6: User Story 4 - Hotkey Capture & Settings (Priority: P4)

**Goal**: F11 client screenshot with persistent save/format/quality settings

**Independent Test**: Configure path/format; F11; verify file; restart persists settings; failure paths

### Implementation for User Story 4

- [x] T024 [P] [US4] Implement `ICaptureService` / `CaptureService` via DXGI Desktop Duplication (dxcam-equivalent)
- [x] T025 [P] [US4] Implement `IHotkeyService` / `HotkeyService` with F11 + conflict error UI
- [x] T026 [US4] Settings panel for directory/format/JPEG quality/filename template (defaults: Jpeg, `{yyyyMMdd_HHmmss}`)
- [x] T027 [US4] Connect F11 → capture → status/path feedback; no success on empty/corrupt file
- [x] T028 [US4] Validate minimized/closed window failure messaging

**Checkpoint**: Capture workflow complete

---

## Phase 7: User Story 5 - Single-File Delivery (Priority: P5)

**Goal**: Publish self-contained single-file `win-x64` exe

**Independent Test**: Run published exe without separate runtime install

### Implementation for User Story 5

- [x] T029 [US5] Finalize `WinSpot.csproj` publish properties and verify `dotnet publish` per quickstart.md
- [ ] T030 [US5] Smoke-test published `publish/WinSpot.exe` launch + bind smoke

**Checkpoint**: Distributable binary ready

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Docs and consistency

- [x] T031 [P] Align README with final UI labels and settings paths
- [ ] T032 Run full `quickstart.md` manual validation checklist
- [ ] T033 Mark completed tasks and prepare `/speckit-converge`

---

## Phase 9: UX Polish (FR-013–016, Session 2026-09-30)

**Purpose**: Apply clarified UX: no content banner, tabbed probe modes, copy feedback toast, “W” app icon

**Independent Test**: quickstart.md steps 1–5 (chrome, bind copy toast, pick/marquee tabs, idle tab)

### Implementation

- [x] T034 [US1] Remove in-content “WinSpot” title/intro banner from `src/WinSpot/MainWindow.xaml`; keep `Window.Title="WinSpot"` (FR-016)
- [x] T035 [US2] Replace probe-mode radio buttons with None/Pick/Marquee `TabControl` in `src/WinSpot/MainWindow.xaml(.cs)`; each tab shows only that mode’s results/hints (FR-013)
- [x] T036 [P] [US1] Add copy success UX: brief button success state (~1.5s) + top toast `已复制` (~2s auto-dismiss) for all copy actions in `MainWindow.xaml(.cs)` (FR-014)
- [x] T037 [P] [US5] Create abstract “W” letter `src/WinSpot/Assets/app.ico` and set `ApplicationIcon` in `src/WinSpot/WinSpot.csproj` (FR-015)
- [x] T038 Verify build (`dotnet build`) and that published icon wiring is present; update quickstart if UI labels changed

**Checkpoint**: Clarification UX complete — ready for manual quickstart re-run

---

## Phase 10: Probe Interaction (FR-017)

**Purpose**: WinSpot stays interactive under the pointer during Pick/Marquee; mode crosshair outside; samples keep the active tab; Esc or None tab exits

**Independent Test**: quickstart pick/marquee steps — operate WinSpot UI while mode active; sample twice without re-selecting tab; Esc → None

### Implementation

- [x] T039 [US2] Update `ProbeOverlayWindow` to exclude WinSpot from hit-testing (window region punch-out / click-through over owner) and use Cross cursor outside; keep Focus for Esc in `src/WinSpot/ProbeOverlayWindow.cs`
- [x] T040 [US2] After successful pick/marquee, keep overlay armed and do **not** auto-select None; Esc raises cancel → None; selecting None closes overlay in `MainWindow.xaml.cs` (FR-017)
- [x] T041 [US3] Ensure marquee rubber-band still works with exclusion region; stay on Marquee tab after release for repeat (FR-017)
- [x] T042 Build + smoke: mode cursor outside, WinSpot clicks work, Esc/None exit

**Checkpoint**: FR-017 probe interaction complete

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS stories
- **US1 → US5**: After Foundational; prefer priority order (US2–US4 need bound window from US1 in practice)
- **Polish (Phase 8)**: After desired stories
- **UX Polish (Phase 9)**: After US1–US3 UI exists; can run once MainWindow shell is present
- **Probe Interaction (Phase 10)**: After Phase 9 tab UI (T035)

### User Story Dependencies

- **US1**: After Phase 2 only
- **US2 / US3 / US4**: After Phase 2; practically require US1 bind UX
- **US5**: After core features compile; can parallelize publish config earlier (T002)
- **Phase 9**: Depends on MainWindow bind/probe UI (T012–T022)
- **Phase 10**: Depends on T035 overlay/tab wiring

### Parallel Opportunities

- T003/T004; T005/T006/T007; T024/T025 after contracts stable
- T036 / T037 can run in parallel after T034/T035 layout settles (T037 independent of T034/T035)
- T039–T041 are sequential on the same overlay/main-window files

---

## Parallel Example: Foundational

```text
Task: Create models in src/WinSpot/Models/
Task: Create NativeMethods.cs P/Invoke surface
Task: Implement ClipboardHelper and FilenameTemplate
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1 Setup
2. Phase 2 Foundational
3. Phase 3 US1
4. STOP — validate bind/inspect
5. Continue US2+

### Incremental Delivery

Setup → Foundation → Bind → Pick → Marquee → Capture/Settings → Publish → Polish
