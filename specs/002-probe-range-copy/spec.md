# Feature Specification: Probe Range Copy-Idle

> Language: [English](spec.md) · [中文（仅供阅读）](spec.zh-CN.md)

**Feature Branch**: `006-probe-range-copy`

**Created**: 2026-10-04

**Status**: Draft

**Input**: User description: "1) After Copy on the Pick tab, auto-switch probe mode to None; same for the Marquee tab. 2) Add probe mode Range: with that tab selected, moving the pointer in the bound window shows on the Range tab the distance from the pointer to the client-area center and the angle between the ray from the center to the current point and the vertical direction (−180~180); a click writes distance and angle onto the UI, copyable."

**Relationship to `001-window-spot-tool`**: This increment extends the existing probe-tab product. Binding, capture, copy toast, WinSpot-vs-outside pointer rules, Esc / None idle, and per-tab results still apply unless this spec explicitly amends them. It **amends** 001 FR-017(d) / SC-010 only for **successful Copy on Pick, Marquee, or Range result controls**: those copies MUST switch to None. Completing a pick/marquee/range sample (click or drag) without copying still keeps the current tab (001 unchanged for sample completion).

## Clarifications

### Session 2026-10-04

- Q: After Copy on Pick/Marquee, should probe mode go idle? → A: Yes — Copy on those tabs’ result copy actions MUST select None. Esc and selecting None remain valid exits.
- Q: Which Pick-tab Copy actions idle the probe? → A: Either **Copy coordinate** or **Copy color** on a successful copy selects None.
- Q: After a successful Range-result copy, should probe mode also switch to None? → A: Yes — same as Pick/Marquee. Click-commit without copy still stays on Range.
- Q: Angle 0° and sign? → A: **0° = straight up** (client Y decreasing). **Positive = clockwise**. Thus +90° is straight right, −90° is straight left, ±180° is straight down.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Copy on Pick/Marquee Returns to Idle (Priority: P1)

A user binds a window, selects Pick (or Marquee), samples a point (or region), then clicks Copy on that tab’s result. After a successful copy (same button success state and top “已复制” toast as today), WinSpot automatically selects the None tab so the pointer no longer samples the bound window. Last Pick or Marquee results remain stored on their tabs so the user can re-open the tab to review them without re-sampling.

**Why this priority**: Users currently stay in probe mode after copy and can accidentally overwrite samples; idle-after-copy is the daily-flow change.

**Independent Test**: Bind a window, Pick a point, copy coordinates; confirm None is selected. Repeat with copy color. Repeat with Marquee copy. Last results still visible if the tab is reselected.

**Acceptance Scenarios**:

1. **Given** the Pick tab is selected and a valid pick result is shown, **When** the user successfully copies **either** the pick coordinates **or** the pick color, **Then** the clipboard matches the displayed format, copy feedback (button ~1.5s + toast “已复制” ~2s) still occurs, and the None tab becomes selected so pick sampling stops.
2. **Given** the Marquee tab is selected and a valid region is shown, **When** the user successfully copies the region, **Then** copy feedback occurs and the None tab becomes selected so marquee input stops.
3. **Given** Pick, Marquee, or Range is selected, **When** the user copies a **binding** field (handle, title, etc.) that is not a probe-tab result control, **Then** probe mode MUST NOT switch to None solely because of that copy.
4. **Given** a successful pick or marquee with no copy yet, **When** the user does not copy, **Then** the Pick/Marquee tab remains selected (001 behavior).
5. **Given** probe mode returned to None after copy, **When** the user selects Pick or Marquee again, **Then** the last result for that tab is still shown (not cleared by the idle switch).

---

### User Story 2 - Range Mode Live Distance and Angle (Priority: P2)

With a bound window, the user selects the Range tab. WinSpot under the pointer stays a normal cursor and fully clickable. Outside WinSpot, a mode crosshair is used. While the pointer is over the bound client area, the Range tab **live-updates** Euclidean distance from the **client-area center** to the current client-relative pointer position, and the signed angle (−180 to 180 inclusive at the wrap) of the ray from that center through the pointer, measured from **straight up** with **positive clockwise**. Moving off the bound client (or over WinSpot) stops live updates without wiping a previously committed measurement. Clicking inside the bound client (outside WinSpot) **commits** the current distance and angle into persistent result fields on the Range tab (copyable). Completing a click MUST NOT auto-switch to None.

**Why this priority**: New measurement capability; depends on the same bind + probe-tab interaction model as Pick.

**Independent Test**: Bind a window of known size, select Range, hover the geometric client center (distance ~0), hover a point straight up from center (angle ~0°), hover straight right (~+90°), click to freeze, copy, confirm None is selected.

**Acceptance Scenarios**:

1. **Given** a bound window and Range selected, **When** the pointer is over WinSpot, **Then** cursor and clicks are normal WinSpot UI (no range sampling). Outside WinSpot the cursor is a crosshair/cross-style mode cursor.
2. **Given** Range selected and the pointer over the bound client outside WinSpot, **When** the pointer moves, **Then** the Range tab shows live distance (client pixels) and live angle updating without requiring a click.
3. **Given** live values are shown, **When** the user left-clicks inside the bound client outside WinSpot, **Then** those values are written into the Range tab’s committed result fields, the Range tab stays selected, and live preview may continue to follow the pointer afterward without immediately overwriting the committed fields until the next click.
4. **Given** committed Range results are shown, **When** the user successfully copies a Range **result** (distance, angle, or a combined copy that includes them), **Then** clipboard text matches the displayed committed format, copy success UX matches other copies, and the None tab becomes selected so range probing stops. Reselecting Range still shows the committed values.
5. **Given** Range selected, **When** the user clicks outside the bound client (and outside WinSpot), **Then** WinSpot ignores the click or shows a non-destructive hint and MUST NOT overwrite a valid committed range result with invalid data.
6. **Given** Range selected, **When** the user presses Esc **or** selects None, **Then** range probing ends and None is selected; committed Range results remain on the Range tab.

---

### Edge Cases

- No window bound, or bound handle invalid: Range MUST NOT invent distances; show a clear idle/error hint instead of live numbers.
- Pointer over bound client but window moved/resized: center and coordinates MUST follow current client geometry.
- Pointer exactly at client center: distance is 0; angle SHOULD show a stable defined value (e.g. 0) rather than NaN.
- Angle wrap at ±180: display MUST stay in [−180, 180]; do not show values outside that range.
- High-DPI / multi-monitor: same client-relative conversion accuracy as Pick.
- Odd vs even client width/height: center is the geometric center of the client rectangle in client coordinates (may be on a half-pixel).
- Copy with empty committed Range fields: MUST NOT claim success or switch modes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST treat a successful copy from **either** Pick-tab result control (**Copy coordinate** or **Copy color**) as ending probe mode and selecting None.
- **FR-002**: System MUST treat a successful copy from the Marquee tab’s region copy control as ending probe mode and selecting None.
- **FR-003**: Copy actions that are not Pick, Marquee, or Range **result** copies (window bind fields, settings, etc.) MUST NOT by themselves switch probe mode to None.
- **FR-004**: Switching to None after a Pick, Marquee, or Range result copy MUST NOT clear the last stored results on those tabs.
- **FR-005**: Completing a pick click or marquee drag without copying MUST still keep Pick/Marquee selected (001 FR-017(d) remains for sample completion).
- **FR-006**: System MUST add a mutually exclusive probe tab **Range** (Chinese UI label **测距**) alongside None / Pick / Marquee. Selecting Range activates range probing; the tab shows only range guidance, live preview, and committed results.
- **FR-007**: While Range is selected, WinSpot-under-pointer remains normal cursor and UI; range live-tracking and click-commit MUST only apply outside WinSpot; outside WinSpot the cursor MUST be a mode crosshair/cross.
- **FR-008**: While Range is active and the pointer is over the bound client, the Range tab MUST live-display (a) Euclidean distance in client pixels from the client-area geometric center to the current client-relative pointer position, and (b) the signed angle in degrees in [−180, 180] of the ray from that center through the current point, measured from **straight up** (decreasing client Y = 0°) with **positive clockwise** (straight right ≈ +90°, straight left ≈ −90°, straight down ≈ ±180°).
- **FR-009**: A left-click inside the bound client (outside WinSpot) while Range is active MUST copy the current live distance and angle into committed, copyable result fields on the Range tab without switching to None.
- **FR-010**: Committed Range results MUST be one-click copyable in a stable text format that matches what is shown (distance and angle both included, or separate copy controls whose labels match the fields). Copy UX MUST follow 001 FR-014. A successful copy of any Range **result** control MUST end probe mode and select None (same as Pick/Marquee).
- **FR-011**: Esc and selecting None MUST end Range probing the same way as Pick/Marquee.
- **FR-012**: Distance MUST use the same client-relative pixel space as Pick coordinates. Display at least one decimal place when the center or distance is not an integer; integer-valued distances MAY omit trailing decimals. Angle MUST be shown in degrees with enough precision that a 1° change at typical window sizes is visible (at least one decimal).
- **FR-013**: When range probing cannot produce a valid measurement (no bind, pointer not on client, conversion failure), the UI MUST show a clear non-numeric or prior-committed state rather than a silent wrong value.

### Key Entities

- **ProbeMode**: Mutually exclusive None / Pick / Marquee / Range.
- **ClientRangeSample**: Live or committed measurement: client-relative pointer `(x,y)`, client center `(cx,cy)`, Euclidean `distance`, signed `angleDegrees` in [−180, 180].
- **ClientPointSample** / **ClientRegion**: Unchanged from 001; idle-after-copy does not delete them.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After copying a Pick, Marquee, or Range **result**, a typical user is idle (None) without an extra Esc or tab click; accidental further samples do not occur until they select a probe tab again.
- **SC-002**: With Range selected, hovering the bound client updates distance and angle on the Range tab in a continuously readable way (users perceive updates while moving, not only after click).
- **SC-003**: For a standard Win32 window of known even size, hovering/clicking the geometric client center reports distance 0 (within 0.5 px). A point straight up from center reports angle 0° within 0.5°; a point straight right reports about +90° within 0.5°.
- **SC-004**: Users can copy a committed Range distance+angle in one or two clicks and paste stable text elsewhere, with the same perceptible copy confirmation as other fields.
- **SC-005**: First-time users who already know Pick can discover and complete one Range commit (hover + click + optional copy) in under 60 seconds after selecting the Range tab.

## Assumptions

- Primary users remain developers/QA/automation authors on Windows 10/11 x64 (same as 001).
- Range is a fourth exclusive probe tab; no overlay drawing on the target window is required in this version (numbers live in WinSpot only).
- Client-area center is `(width / 2, height / 2)` in client coordinates (floating), not rounded to a Pick-style integer pixel before the distance is computed.
- Distance is Euclidean, not Manhattan or “vertical-only”.
- Live preview and committed fields are distinct: click freezes committed values; subsequent motion updates live preview only until the next click.
- Range click-commit does **not** auto-switch to None; a successful Range **result** copy **does**. If distance and angle are copied by separate controls, either successful copy idles the probe (same rule as Pick coordinate vs color).
- Bind-area copies never idle the probe.
- No README change until this feature is implemented; current README still describes shipped 001 behavior.
- Out of scope: on-target rubber-band/ray overlay, keyboard nudging, persisting last range across app restarts, multi-window bind.
