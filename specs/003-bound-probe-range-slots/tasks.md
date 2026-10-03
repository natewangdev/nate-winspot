# Tasks: Bound-Window Probe & Range Slots

**Input**: `/specs/003-bound-probe-range-slots/`

**Tests**: Not requested.

## Phase 1: Setup

- [x] T001 Confirm changes stay in `src/WinSpot` per `specs/003-bound-probe-range-slots/plan.md` (no new packages)

---

## Phase 2: Foundational

- [x] T002 Add `RGN_AND` (if used) in `src/WinSpot/Native/NativeMethods.cs` only as needed for region combine

**Checkpoint**: Native constants ready

---

## Phase 3: User Story 1 - Copy No Longer Idles (P1) 🎯

**Independent Test**: Copy pick/marquee/range without leaving the tab.

- [x] T003 [US1] Remove idle-after-copy in `src/WinSpot/MainWindow.xaml.cs` (`CopyToClipboard` / `CopyProbeResult`)
- [x] T004 [US1] Update pick/marquee/none hint strings in `src/WinSpot/MainWindow.xaml` so copy no longer says return to 「无」

**Checkpoint**: US1 demoable

---

## Phase 4: User Story 2 - Probe Only Over Bound Client (P1)

**Independent Test**: Other apps receive clicks; bound client has crosshair.

- [x] T005 [US2] Change `src/WinSpot/ProbeOverlayWindow.cs` hit-test region to bound client minus WinSpot; refresh while armed; pass target HWND from `src/WinSpot/MainWindow.xaml.cs`
- [x] T006 [US2] Align remaining hints in `src/WinSpot/MainWindow.xaml` with “only on bound client”

**Checkpoint**: US2 demoable

---

## Phase 5: User Story 3 - Ten Range Slots (P2)

**Independent Test**: `specs/003-bound-probe-range-slots/quickstart.md` steps 4–5.

- [x] T007 [US3] Replace single commit pair with live preview + 10 read-only rows + Clear in `src/WinSpot/MainWindow.xaml`
- [x] T008 [US3] Fill slots in order, swallow full clicks with toast `测距已满（10/10）`, Clear, copy without idle in `src/WinSpot/MainWindow.xaml.cs`
- [x] T009 [US3] Parameterize toast text on the top banner in `src/WinSpot/MainWindow.xaml` and `src/WinSpot/MainWindow.xaml.cs`

**Checkpoint**: US1–US3 together

---

## Phase 6: Polish

- [x] T010 [P] Update `README.md` Features for bound-only probe and 10 range slots
- [x] T011 [P] Update `README.zh-CN.md` equivalently
- [x] T012 Build `src/WinSpot/WinSpot.csproj`

---

## Dependencies

T001 → T002 → US1 (T003–T004) → US2 (T005–T006) → US3 (T007–T009) → T010–T012

T010/T011 parallel after US3.

## MVP

T001–T004 (copy-idle revert). Then hit-region, then slots.
