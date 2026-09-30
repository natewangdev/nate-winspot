# Feature Specification: WinSpot Window Spot Tool

> Language: [English](spec.md) · [中文（仅供阅读）](spec.zh-CN.md)

**Feature Branch**: `001-window-spot-tool`

**Created**: 2026-09-27

**Status**: Draft

**Input**: User description: "Windows window tool: drag-bind selected window and show HWND/title/client size; click client area to show relative coordinates and color (copyable); marquee-select a client region with copyable relative coords; F11 client-area screenshot with configurable save path/format/quality; deliver as C# WPF single-file exe."

## Clarifications

### Session 2026-09-30

- Q: After switching probe modes to tabs, how should idle / pick / marquee and their result areas be organized? → A: Three tabs (None / Pick / Marquee); each tab shows only that mode’s results and hints.
- Q: After clicking Copy, how should button visual feedback and the top “Copied” toast behave? → A: Brief button success state (~1.5s) plus top “已复制” toast (~2s); both auto-reset/dismiss.
- Q: What visual theme should the app icon use to represent WinSpot? → A: Abstract “W” letter mark (weak functional metaphor).
- Q: When removing the top “WinSpot” title and intro, which chrome is in scope? → A: Remove only the in-content title+intro banner; keep system window title bar as `WinSpot`.
- Q: While Pick/Marquee is selected, how should pointer interaction and mode exit work? → A: Inside the WinSpot window, cursor and clicks stay normal; pick/marquee only apply outside WinSpot. Completing a pick/marquee MUST NOT switch the tab to None; Esc ends probe mode and selects None (selecting the None tab also idles).
- Q: Besides Esc, can the user exit probe mode by selecting the None tab? → A: Yes — Esc or selecting the None tab both end probe mode and return to None.
- Q: What cursor should appear outside WinSpot while Pick or Marquee is selected? → A: Outside WinSpot use a mode cursor (crosshair/cross for pick and marquee); restore the normal cursor over WinSpot.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Bind Window & Inspect Info (Priority: P1)

A power user opens WinSpot, drags a binding crosshair onto a visible desktop window, releases, and immediately sees that window’s handle, title, and client-area size (and related identity fields) in the tool. Values can be copied for use in other tools or notes. The main content area does not show a large in-app “WinSpot” title/intro banner; the system window title remains `WinSpot`.

**Why this priority**: Binding and identity are the foundation for every other measurement and capture action.

**Independent Test**: Bind Notepad (or File Explorer), verify displayed HWND/title/client size match a trusted inspector, copy each field successfully.

**Acceptance Scenarios**:

1. **Given** WinSpot is running with no bound window, **When** the user drags the bind control onto a visible window and releases, **Then** that window becomes the bound target and its handle, title, and client size appear in the tool.
2. **Given** a window is bound, **When** the user copies the handle or title, **Then** the clipboard contains the displayed text in a stable format (handle as decimal digits only, e.g. `265098`), the copy control briefly shows a success state for about 1.5 seconds, and a top-of-window toast “已复制” appears for about 2 seconds then dismisses.
3. **Given** a window is bound, **When** the user chooses clear/rebind, **Then** binding is cleared or replaced without crashing the tool.

---

### User Story 2 - Client Click Pick (Coordinate + Color) (Priority: P2)

With a bound window, the user selects the Pick tab. While that tab stays selected, the WinSpot window itself keeps a normal cursor and full UI clickability; pick sampling only happens when the pointer is outside WinSpot (on the bound client). After a successful pick, the Pick tab remains selected so the user can pick again. Esc (or selecting the None tab) ends pick mode and returns to None.

**Why this priority**: Coordinate/color pickup is the core daily inspection workflow after binding.

**Independent Test**: Bind a known window, pick a corner and a mid-point, verify relative coordinates and colors against a trusted reference (within stated tolerance); confirm WinSpot UI remains clickable during pick mode; confirm tab stays on Pick after a sample until Esc.

**Acceptance Scenarios**:

1. **Given** a bound window and the Pick tab selected, **When** the user moves the pointer over the WinSpot window, **Then** the cursor and clicks behave as normal WinSpot UI (not pick sampling). When the pointer is outside WinSpot, the cursor is a pick/crosshair-style cursor.
2. **Given** a bound window and the Pick tab selected, **When** the user clicks inside the bound client area outside WinSpot, **Then** WinSpot shows `(x, y)` relative to the client origin and the pixel color at that point inside the Pick tab, and the Pick tab remains selected.
3. **Given** pick results are shown, **When** the user copies coordinates or color, **Then** clipboard text matches the displayed format (e.g. `x,y` and `#RRGGBB`), with the same copy button success state and top “已复制” toast as other copy actions.
4. **Given** the Pick tab is selected, **When** the user clicks outside the bound client area (and outside WinSpot), **Then** WinSpot ignores the click or shows a non-destructive hint and does not overwrite valid last pick with wrong data.
5. **Given** the Pick tab is selected after a pick, **When** the user presses Esc **or** selects the None tab, **Then** the probe mode ends and the None tab becomes selected.

---

### User Story 3 - Client Region Marquee (Priority: P3)

With a bound window, the user selects the Marquee tab. WinSpot UI under the pointer remains normal; marquee drag only applies outside WinSpot on the bound client. After a successful marquee, the Marquee tab remains selected for another selection. Esc (or selecting the None tab) ends marquee mode and returns to None.

**Why this priority**: Region measurement builds on binding; useful for cropping/automation planning but secondary to single-point pick.

**Independent Test**: Drag a known-size region inside a bound client area; confirm rubber-band is visible during drag; verify reported `x1,y1,x2,y2`; confirm tab stays on Marquee until Esc.

**Acceptance Scenarios**:

1. **Given** the Marquee tab selected and a bound window, **When** the user interacts with controls inside the WinSpot window, **Then** those clicks work normally and do not start a marquee; the cursor over WinSpot is normal. Outside WinSpot the cursor is a crosshair-style marquee cursor.
2. **Given** the Marquee tab selected and a bound window, **When** the user drags inside the bound client area outside WinSpot, **Then** a selection outline is visible during the drag, and on release WinSpot shows `x1,y1,x2,y2` (top-left / bottom-right, client-relative) inside the Marquee tab, and the Marquee tab remains selected.
3. **Given** a marquee result, **When** the user copies the region description, **Then** clipboard contains exactly that `x1,y1,x2,y2` text, with the same copy button success state and top “已复制” toast as other copy actions.
4. **Given** the drag extends past the client edge, **When** the user releases, **Then** the reported region is clipped to the client area.
5. **Given** the Marquee tab is selected after a marquee, **When** the user presses Esc **or** selects the None tab, **Then** the probe mode ends and the None tab becomes selected.

---

### User Story 4 - Hotkey Client Screenshot & Export Settings (Priority: P4)

With a bound window, the user presses F11 (or the configured capture hotkey) to capture only the client area and save it using configured directory, filename pattern, format, and quality. Settings persist across sessions.

**Why this priority**: Capture is high value but depends on reliable binding; settings are part of making captures usable day-to-day.

**Independent Test**: Bind a window, set save folder/format/quality, press F11, open the file and confirm client-only content and expected dimensions/format.

**Acceptance Scenarios**:

1. **Given** a visible bound window and valid save settings, **When** the user presses F11, **Then** a client-area image is saved to the configured location with the configured format, and the image content matches what is visibly composed on screen (not a blank/gray frame).
2. **Given** JPEG format is selected (the default), **When** the user changes quality and captures, **Then** the saved file reflects the new quality setting.
3. **Given** the bound window is minimized, closed, or capture is otherwise impossible, **When** the user presses F11, **Then** WinSpot shows a clear failure message and does not write a corrupt/empty image as success.
4. **Given** the user changes save directory/format/quality, **When** the app restarts, **Then** those settings are still applied.

---

### User Story 5 - Single-File Desktop Delivery (Priority: P5)

A user downloads/builds one executable and runs WinSpot on a clean Windows 10/11 x64 machine without installing a separate runtime.

**Why this priority**: Delivery requirement for distribution; does not block interactive MVP demos from a local build, but is required for release.

**Independent Test**: Publish single-file self-contained build; run on a machine/profile without preinstalled .NET and confirm the app launches.

**Acceptance Scenarios**:

1. **Given** a published single-file self-contained build, **When** the user double-clicks the exe on Windows 10/11 x64, **Then** WinSpot starts without a separate runtime install step.

---

### Edge Cases

- Target window is closed or handle becomes invalid after binding.
- Target window moves, resizes, or changes title while bound.
- Target runs at higher integrity level than WinSpot (access/capture may fail).
- High-DPI and multi-monitor layouts affect screen↔client conversion.
- Global hotkey F11 is already registered by another application.
- Minimized, cloaked, or fully occluded windows.
- UWP / exclusive fullscreen / hardware-protected content may not yield accurate pixels (best-effort with clear failure).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow the user to bind a visible desktop window via drag-and-drop (or equivalent drag-from-tool) interaction.
- **FR-002**: System MUST display at least: window handle, window title, and client-area width/height for the bound window; values MUST be copyable. The handle MUST be shown and copied as decimal digits only (no `0x` hex prefix).
- **FR-003**: System MUST allow clearing and rebinding the target window.
- **FR-004**: System MUST provide a pick mode that, on click inside the bound client area, reports coordinates relative to the client top-left origin.
- **FR-005**: System MUST report the color of the picked client pixel in a copyable RGB and/or HEX form.
- **FR-006**: System MUST provide a marquee mode to select a rectangular region inside the bound client area, show a visible rubber-band outline while dragging, and report the region as copyable `x1,y1,x2,y2` (top-left and bottom-right, client-relative).
- **FR-007**: System MUST clip marquee results to the client-area bounds.
- **FR-008**: System MUST register a capture hotkey defaulting to F11 that captures only the bound window’s client area.
- **FR-009**: System MUST allow configuring screenshot save directory, image format (at least PNG and JPEG), JPEG quality, and a filename pattern; settings MUST persist locally. Defaults MUST be: JPEG format; filename template time-only (e.g. `{yyyyMMdd_HHmmss}`), without window title.
- **FR-010**: System MUST present clear errors when bind, pick, marquee, or capture cannot complete accurately.
- **FR-011**: System MUST ship as a single-file self-contained Windows executable for the supported architecture.
- **FR-012**: Client-area screenshots MUST use DXGI Desktop Duplication (same approach as Python dxcam) so composed desktop pixels are captured; GDI `BitBlt`/`GetDC` MUST NOT be the capture backend.
- **FR-013**: System MUST present probe modes (None / Pick / Marquee) as mutually exclusive tabs (not radio buttons). Each tab MUST show only that mode’s results and guidance; selecting a tab activates the corresponding mode.
- **FR-014**: On every successful copy-to-clipboard action, the system MUST (a) briefly change the clicked copy control to a perceptible success state for approximately 1.5 seconds then restore it, and (b) show a top-of-window toast with the text `已复制` for approximately 2 seconds that auto-dismisses.
- **FR-015**: The shipping executable and main window MUST use a dedicated application icon: an abstract “W” letter mark (not a crosshair/eyedropper/marquee metaphor). The icon MUST appear at least on the `.exe` file and in the window/taskbar chrome.
- **FR-016**: The main window content MUST NOT show a top promotional/title banner with the app name “WinSpot” and feature intro text. The system window title bar MUST still display `WinSpot`.
- **FR-017**: While Pick or Marquee is selected: (a) pointer interaction over the WinSpot main window MUST keep the normal cursor and MUST allow normal WinSpot UI clicks (bind, copy, settings, tab switching, etc.); (b) pick/marquee sampling MUST only occur for pointer actions outside the WinSpot window; (c) outside WinSpot the cursor MUST be a mode-appropriate crosshair/cross style for the active tab; (d) completing a successful pick or marquee MUST NOT auto-switch the tab to None—the current Pick/Marquee tab MUST remain selected so the user can repeat; (e) pressing Esc MUST end the active probe mode and select the None tab; selecting the None tab MUST also end probe mode.

### Key Entities

- **BoundWindow**: The currently selected target; identity (handle, title, class/process as available) and client geometry.
- **ClientPointSample**: A relative `(x,y)` plus color sample from the bound client area.
- **ClientRegion**: A relative rectangle (origin + size) within the bound client area.
- **CaptureSettings**: Save directory, format, quality, filename pattern, hotkey.
- **CaptureResult**: Success path or failure reason for a client-area screenshot attempt.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can bind a window and read handle/title/client size within 30 seconds of first launch.
- **SC-002**: For standard Win32 windows (e.g. Notepad), relative pick coordinates match a trusted reference with zero pixel error; sampled color matches within ±1 per RGB channel under normal desktop composition.
- **SC-003**: Marquee regions reported for an on-screen known rectangle match expected relative bounds after clipping.
- **SC-004**: F11 capture produces a file whose pixel dimensions equal the bound client size (for a visible, capturable window) in under 2 seconds on a typical developer PC.
- **SC-005**: After changing save settings and restarting, the next capture uses the saved settings without reconfiguration.
- **SC-006**: Published single-file build launches on a clean Windows 10/11 x64 environment without a separate runtime installer.
- **SC-007**: After a successful copy, users can perceive both button success feedback and a top “已复制” toast within the stated durations without blocking further interaction.
- **SC-008**: On a published build, the executable file icon and the running window/taskbar icon both show the WinSpot “W” mark (not the default blank/generic icon).
- **SC-009**: At first glance after launch, the main content area has no top “WinSpot” title/intro banner, while the OS window title still reads `WinSpot`.
- **SC-010**: With Pick or Marquee selected, the user can still operate WinSpot UI under the pointer with a normal cursor; outside WinSpot a mode crosshair/cross cursor appears; after a successful sample the same tab stays selected until Esc or selecting None.

## Assumptions

- Primary users are developers/QA/automation authors on Windows 10/11 x64.
- First release binds one window at a time (no multi-bind).
- Pick and marquee are mutually exclusive modes selected via tabs (None / Pick / Marquee); each tab shows only that mode’s results and hints.
- While Pick/Marquee is active, WinSpot remains fully interactive under the pointer with a normal cursor; outside WinSpot a mode crosshair/cross cursor is used and probe input applies. Completing a sample does not return to None; Esc or selecting None does.
- Default capture hotkey is F11; remapping may be deferred if not required for MVP beyond documenting conflict handling.
- Default image format is JPEG; default filename template is `{yyyyMMdd_HHmmss}` (no `{title}`).
- Capture backend is DXGI Desktop Duplication (dxcam-equivalent); client area is obtained by capturing the composed desktop region of the client screen rect.
- OCR, process injection, scripted clicking, and guaranteed exclusive-fullscreen/protected capture are out of scope for this feature.
- Best-effort behavior is acceptable for protected/fullscreen content when failures are visible to the user.
- “Related info” beyond handle/title/client size may include class name, process name/PID, and client screen position when obtainable without elevating privileges.
- Marquee `x2,y2` is the bottom-right corner using `x1+width` / `y1+height` after clipping (same corner as a normalized drag endpoint).
