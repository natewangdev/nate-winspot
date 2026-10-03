# Feature Specification: Bound-Window Probe & Range Slots

> Language: [English](spec.md) · [中文（仅供阅读）](spec.zh-CN.md)

**Feature Branch**: `007-bound-probe-range-slots`

**Created**: 2026-10-04

**Status**: Draft

**Input**: User description: "1) Remove switching to None after Copy on Pick / Marquee / Range. 2) After selecting Pick, Marquee, or Range, the pointer only enters probe mode (with probe styling) when it enters the bound window; outside the bound window the pointer is normal. 3) Range tab: 10 distance+angle field groups; each click inside the bound window writes the next group in order; when all 10 are full, further clicks do not write and WinSpot shows a prompt; add a Clear button that empties all written distance/angle values."

**Relationship to prior specs**: Extends `001-window-spot-tool` and **amends** `002-probe-range-copy`. Copy-idle (002 FR-001/002/010 idle-after-copy) is **withdrawn**. Probe styling and sampling are **no longer** “anywhere outside WinSpot”; they apply **only over the bound window’s client area**. Range click-commit becomes **up to 10 sequential slots** instead of a single committed pair. Angle convention from 002 (0° = up, clockwise positive) is unchanged. Esc / selecting None still ends probing. Binding, capture, and copy toast (001 FR-014) still apply.

## Clarifications

### Session 2026-10-04

- Q: When all 10 slots are full, what prompt and click behavior? → A: Top toast (same family as “已复制”, auto-dismiss) **and the click is still swallowed** so the bound target does not receive it. Slots unchanged.
- Q: Are the 10 groups editable? → A: **Read-only** measurement displays; filled only by bound-client clicks; each group is copyable.
- Q: Keep live hover preview? → A: **Yes** — a separate live distance/angle readout above the 10 slots; click writes the next empty slot only.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Copy No Longer Idles Probe (Priority: P1)

A user stays on Pick, Marquee, or Range after copying a result. They can copy repeatedly and keep sampling without re-selecting the tab. Esc or selecting None still ends probe mode.

**Why this priority**: Reverts a workflow that now conflicts with collecting multiple range samples and with staying in pick/marquee.

**Independent Test**: Pick a point, copy coord and color; tab stays Pick; another click still samples. Same for Marquee region copy and Range slot copy.

**Acceptance Scenarios**:

1. **Given** Pick is selected with a result, **When** the user copies coordinates or color, **Then** copy UX still runs (button ~1.5s + toast “已复制” ~2s) and the Pick tab **remains** selected; overlay/probing stays armed per Story 2.
2. **Given** Marquee is selected with a region, **When** the user copies the region, **Then** copy UX runs and Marquee stays selected.
3. **Given** Range has at least one filled slot, **When** the user copies a slot value, **Then** copy UX runs and Range stays selected.
4. **Given** Pick/Marquee/Range is selected, **When** the user presses Esc or selects None, **Then** probing ends and None is selected (unchanged).

---

### User Story 2 - Probe Style Only Over Bound Window (Priority: P1)

The user selects Pick, Marquee, or Range (tab stays selected). While the pointer is over **WinSpot**, or over the desktop / any window that is **not** the bound target, the cursor is **normal** and clicks behave as the OS/app would (WinSpot UI remains fully usable). When the pointer **enters the bound window’s client area**, the cursor switches to the **probe style** (crosshair/cross) and clicks/drags are consumed for pick / marquee / range. Leaving the bound client restores a normal cursor.

**Why this priority**: Stops the full-screen overlay from stealing clicks on other apps; matches “enter bound window → probe, elsewhere → normal.”

**Independent Test**: Bind Notepad, select Pick. Hover WinSpot → normal, UI clickable. Hover desktop/another app → normal, that app receives clicks. Hover Notepad client → crosshair; click samples. Repeat for Marquee and Range.

**Acceptance Scenarios**:

1. **Given** Pick, Marquee, or Range is selected and a window is bound, **When** the pointer is over the WinSpot window, **Then** the cursor is normal and WinSpot controls work.
2. **Given** the same, **When** the pointer is outside the bound client (including other windows and empty desktop), **Then** the cursor is normal and those surfaces receive pointer input; WinSpot MUST NOT pick/marquee/range from those clicks.
3. **Given** the same, **When** the pointer enters the bound client, **Then** the cursor becomes the probe crosshair/cross for the active tab and sampling/marquee/range clicks apply.
4. **Given** no bound window (or invalid bind), **When** the user selects Pick/Marquee/Range, **Then** WinSpot refuses or returns to None with a bind hint (same as today); there is no probe cursor on arbitrary windows.
5. **Given** Marquee, **When** a drag starts inside the bound client and is released, **Then** the region is clipped to the client as in 001. A drag that does not start on the bound client MUST NOT start a marquee.

---

### User Story 3 - Ten Range Slots, Full Prompt, Clear (Priority: P2)

On the Range tab the user sees a **live** distance/angle readout (hover over bound client, same math as 002) **above** **10 ordered read-only groups**, each with distance, angle, and copy. Each successful click on the bound client writes the current live values into the **next empty slot** (1 then 2 … then 10). After slot 10 is filled, further clicks on the bound client **do not overwrite** any slot, **are still consumed** (target app does not get the click), and WinSpot shows a **top toast** (auto-dismiss, distinct from “已复制”). A **Clear** control on the Range tab empties all 10 groups so the next click fills slot 1 again. Live preview continues after Clear and after the set is full.

**Why this priority**: Multi-sample ranging is the new measurement workflow; depends on Stories 1–2 so the user can stay in Range and click only on the bound window.

**Independent Test**: Click 10 distinct client points; slots 1–10 fill in order; 11th click shows prompt and leaves slots unchanged; Clear empties all; next click fills slot 1.

**Acceptance Scenarios**:

1. **Given** Range selected, bound window, slots empty, **When** the user clicks the bound client, **Then** slot 1 shows that click’s distance and angle (formats unchanged from 002: Euclidean px; 0° up, clockwise positive, [−180, 180]).
2. **Given** k slots filled (k < 10), **When** the user clicks the bound client again, **Then** slot k+1 is written and earlier slots are unchanged.
3. **Given** all 10 slots filled, **When** the user clicks the bound client again, **Then** no slot changes, the click does **not** reach the bound app, and a top toast appears within about 1–2 seconds then dismisses (message states the ten slots are full).
4. **Given** any number of filled slots, **When** the user clicks Clear on the Range tab, **Then** all 10 distance and angle fields are empty and the next bound-client click writes slot 1.
5. **Given** a click outside the bound client, **When** Range is selected, **Then** slots are not written (Story 2).
6. **Given** filled slots, **When** the user switches to None and back to Range, **Then** the 10 groups still show the same read-only values (until Clear or app exit). Typing into those fields is not available.
7. **Given** Range selected and the pointer over the bound client, **When** the pointer moves, **Then** the live readout updates continuously; the 10 slots change only on click into an empty slot (or not at all if full).

---

### Edge Cases

- Bound window moves/resizes: probe region follows current client; live math uses current center/size.
- Clear while hovering: live preview continues; slots empty.
- Copy of an empty slot: not success (status as today); does not Clear.
- Partial last slot: a slot is written as a pair (distance and angle together); never distance-only.
- App restart: slots need not persist (session-only), unless later specified.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Successful copy of Pick, Marquee, or Range results MUST NOT select None or otherwise end probe mode. Copy UX (001 FR-014) MUST still apply.
- **FR-002**: Esc and selecting the None tab MUST still end probe mode and select None.
- **FR-003**: While Pick, Marquee, or Range is the selected tab and a valid window is bound: probe cursor and probe input MUST apply **only** when the pointer is over that window’s **client area**. Over WinSpot and over any other screen region the pointer MUST be a normal (non-probe) cursor and MUST NOT perform pick/marquee/range.
- **FR-004**: Clicks/drags that are not on the bound client MUST be delivered to the underlying UI (other apps / desktop), not swallowed by a full-desktop probe capture.
- **FR-005**: The Range tab MUST present a live distance/angle readout, **10** ordered **read-only** distance+angle groups with copy, and a **Clear** action that empties all groups (not the live readout).
- **FR-006**: Each valid Range click on the bound client MUST write the current live distance and angle into the next empty group in order (1…10). When all 10 are filled, further bound-client clicks MUST NOT change slots, MUST still be swallowed (bound app does not receive them), and MUST show a top auto-dismiss toast stating the slots are full (not the “已复制” copy toast).
- **FR-007**: Distance/angle math MUST remain 002: Euclidean from client geometric center `(width/2, height/2)`; 0° straight up; positive clockwise; range [−180, 180]; coincidence → distance 0, angle 0.
- **FR-008**: Each filled Range slot’s distance and/or angle MUST be copyable in a stable format matching the displayed text, without idling probe mode. Empty slots MUST NOT copy as success.
- **FR-009**: Unbound / invalid target MUST not invent range or pick data; selecting a probe tab without a bind MUST hint and not arm probe on arbitrary windows.
- **FR-010**: While Range is armed and the pointer is over the bound client, the live readout MUST update without writing slots; leaving the bound client MUST stop live updates without clearing slots.

### Key Entities

- **ProbeArming**: Tab selected (Pick/Marquee/Range) vs **ProbeActive** (pointer over bound client).
- **RangeSlot[1..10]**: Ordered pairs `(distance, angleDegrees)`; empty or filled; next-write index 1–11 (11 = full).
- **ClientRangeSample**: Unchanged math from 002; used for live preview and for filling a slot.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After copying a Pick/Marquee/Range result, the same tab stays selected 100% of the time in manual checks (no extra re-select).
- **SC-002**: With Pick selected, a tester can click another application’s title/client (not the bound window) and that click reaches that application; hovering the bound client still samples in WinSpot.
- **SC-003**: Ten sequential bound-client clicks fill ten slots in order; the eleventh does not change stored numbers and produces a visible prompt within 1 second.
- **SC-004**: Clear then one click fills only slot 1; slots 2–10 stay empty.
- **SC-005**: A user who knows Range from 002 can fill three slots and copy one pair in under 60 seconds after selecting 测距.

## Assumptions

- Primary users remain Windows 10/11 x64 developers/QA (001).
- Session-only slots (no persist across restart).
- No on-target ray overlay (002).
- Probe “style” is at least the mode crosshair over the bound client; no extra target-window chrome required.
- Marquee rubber-band remains a WinSpot overlay visual, visible while dragging on the bound client.
- README updated when this feature is implemented, not in this specify step.
- Full-set toast copy is user-visible Chinese, e.g. `测距已满（10/10）` (exact string may match this or equivalent).
- Out of scope: more than 10 slots, reordering slots, undo-single-slot, typing into slots.
