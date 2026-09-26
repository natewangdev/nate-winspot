# Feature Specification: WinSpot Window Spot Tool

> Language: [English](spec.md) · [中文（仅供阅读）](spec.zh-CN.md)

**Feature Branch**: `001-window-spot-tool`

**Created**: 2026-09-27

**Status**: Draft

**Input**: User description: "Windows window tool: drag-bind selected window and show HWND/title/client size; click client area to show relative coordinates and color (copyable); marquee-select a client region with copyable relative coords; F11 client-area screenshot with configurable save path/format/quality; deliver as C# WPF single-file exe."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Bind Window & Inspect Info (Priority: P1)

A power user opens WinSpot, drags a binding crosshair onto a visible desktop window, releases, and immediately sees that window’s handle, title, and client-area size (and related identity fields) in the tool. Values can be copied for use in other tools or notes.

**Why this priority**: Binding and identity are the foundation for every other measurement and capture action.

**Independent Test**: Bind Notepad (or File Explorer), verify displayed HWND/title/client size match a trusted inspector, copy each field successfully.

**Acceptance Scenarios**:

1. **Given** WinSpot is running with no bound window, **When** the user drags the bind control onto a visible window and releases, **Then** that window becomes the bound target and its handle, title, and client size appear in the tool.
2. **Given** a window is bound, **When** the user copies the handle or title, **Then** the clipboard contains the displayed text in a stable format (handle as decimal digits only, e.g. `265098`).
3. **Given** a window is bound, **When** the user chooses clear/rebind, **Then** binding is cleared or replaced without crashing the tool.

---

### User Story 2 - Client Click Pick (Coordinate + Color) (Priority: P2)

With a bound window, the user enters pick mode and clicks a point inside that window’s client area. WinSpot shows the point’s coordinates relative to the client top-left and the color at that point; both are copyable.

**Why this priority**: Coordinate/color pickup is the core daily inspection workflow after binding.

**Independent Test**: Bind a known window, pick a corner and a mid-point, verify relative coordinates and colors against a trusted reference (within stated tolerance).

**Acceptance Scenarios**:

1. **Given** a bound window and pick mode enabled, **When** the user clicks inside the bound client area, **Then** WinSpot shows `(x, y)` relative to the client origin and the pixel color at that point.
2. **Given** pick results are shown, **When** the user copies coordinates or color, **Then** clipboard text matches the displayed format (e.g. `x,y` and `#RRGGBB`).
3. **Given** pick mode is enabled, **When** the user clicks outside the bound client area, **Then** WinSpot ignores the click or shows a non-destructive hint and does not overwrite valid last pick with wrong data.

---

### User Story 3 - Client Region Marquee (Priority: P3)

With a bound window, the user enters marquee mode and drags a rectangle inside the client area. While dragging, a visible selection rectangle (rubber-band) is shown. WinSpot reports the region as copyable `x1,y1,x2,y2` (top-left and bottom-right, client-relative).

**Why this priority**: Region measurement builds on binding; useful for cropping/automation planning but secondary to single-point pick.

**Independent Test**: Drag a known-size region inside a bound client area; confirm rubber-band is visible during drag; verify reported `x1,y1,x2,y2`.

**Acceptance Scenarios**:

1. **Given** marquee mode and a bound window, **When** the user drags inside the client area, **Then** a selection outline is visible during the drag, and on release WinSpot shows `x1,y1,x2,y2` (top-left / bottom-right, client-relative).
2. **Given** a marquee result, **When** the user copies the region description, **Then** clipboard contains exactly that `x1,y1,x2,y2` text.
3. **Given** the drag extends past the client edge, **When** the user releases, **Then** the reported region is clipped to the client area.

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

## Assumptions

- Primary users are developers/QA/automation authors on Windows 10/11 x64.
- First release binds one window at a time (no multi-bind).
- Pick and marquee are mutually exclusive modes selected in the tool UI.
- Default capture hotkey is F11; remapping may be deferred if not required for MVP beyond documenting conflict handling.
- Default image format is JPEG; default filename template is `{yyyyMMdd_HHmmss}` (no `{title}`).
- Capture backend is DXGI Desktop Duplication (dxcam-equivalent); client area is obtained by capturing the composed desktop region of the client screen rect.
- OCR, process injection, scripted clicking, and guaranteed exclusive-fullscreen/protected capture are out of scope for this feature.
- Best-effort behavior is acceptable for protected/fullscreen content when failures are visible to the user.
- “Related info” beyond handle/title/client size may include class name, process name/PID, and client screen position when obtainable without elevating privileges.
- Marquee `x2,y2` is the bottom-right corner using `x1+width` / `y1+height` after clipping (same corner as a normalized drag endpoint).
