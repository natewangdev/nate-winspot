# Research: Probe Range Copy-Idle

## Decision: Idle after result copy, not after sample

- **Decision**: After a successful clipboard copy of Pick/Marquee/Range **result** controls, select None and close the probe overlay. Sample completion (pick click, marquee release, range click-commit) keeps the current tab. Bind-field copies do not idle.
- **Rationale**: Matches clarified spec; users copy then move on; accidental overwrite is the pain, not needing a second sample.
- **Alternatives considered**: Idle on sample click (rejected — contradicts 001 FR-017(d) and ranging “click then copy”). Idle only on coordinate copy (rejected — Q1 A). Stay on Range after copy (rejected — Q2 A).

## Decision: Overlay third mode for live range (no GetPixel)

- **Decision**: Extend `ProbeOverlayWindow` with Pick / Marquee / Range. Range raises move events with screen coordinates and click-commit; do not hide-for-GetPixel. Live values compute from `ScreenToClient` + client rect only.
- **Rationale**: Pick already uses a virtual-screen overlay with owner exclusion; live tracking needs move events on that surface. Pixel sampling is irrelevant and would hitch the preview.
- **Alternatives considered**: Win32 mouse hook (more invasive, constitution prefers public APIs already in use). Polling `GetCursorPos` on a timer (works but overlay already has mouse). Drawing a ray on the target (out of spec).

## Decision: Angle formula (0° up, clockwise positive)

- **Decision**: With client origin top-left and Y down, vector from center `(cx,cy)=(w/2,h/2)` to pointer `(x,y)` is `dx=x-cx`, `dy=y-cy`. Angle degrees = `Atan2(dx, -dy) * 180/π`, then wrap to [−180, 180]. Center coincidence: distance 0, angle 0.
- **Rationale**: `Atan2(dx, -dy)` is 0 when pointing up, +90 right, −90 left, ±180 down — matches Q3 A without extra offsets.
- **Alternatives considered**: `Atan2(-dx, -dy)` (CCW). 0° down. Integer-pixel center before math (rejected — spec wants geometric center).

## Decision: Live vs committed UI

- **Decision**: Range tab shows live preview fields (read-only, not copyable as “result copy”) and committed fields with Copy. Copy of committed distance and/or angle idles. Empty committed copy fails without mode switch.
- **Rationale**: Spec requires hover updates without click, and click writes persistent copyable values not overwritten by the next move.
- **Alternatives considered**: Single field pair overwritten on move (cannot copy a frozen sample while hovering). Combined-only copy (allowed by spec but two buttons match Pick).

## Decision: Copy clipboard format

- **Decision**: Distance and angle as separate committed boxes; copy texts match display (distance: omit trailing `.0` when integer else one decimal; angle: at least one decimal). Optional combined copy `distance,angle` is not required if both fields have Copy; either copy idles.
- **Rationale**: Mirrors Pick coord/color; Q1/Q2 same idle rule.
- **Alternatives considered**: Single `123.4,45.0` string only.

## Decision: No new packages / no DXGI for range

- **Decision**: Geometry only; DXGI remains capture-only.
- **Rationale**: YAGNI and non-invasive principle.
- **Alternatives considered**: Capture a frame to measure (unnecessary, slower, wrong for hover).
