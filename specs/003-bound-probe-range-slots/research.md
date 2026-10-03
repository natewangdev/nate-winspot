# Research: Bound-Window Probe & Range Slots

## Decision: Hit-test region = bound client minus WinSpot

- **Decision**: Keep a virtual-screen overlay for rubber-band coordinates, but `SetWindowRgn` to the bound **client** rectangle in overlay pixels, then `RGN_DIFF` the WinSpot frame if it overlaps. Refresh the region on a short dispatcher timer while armed (target can move/resize without events). Empty/invalid bind → empty region (all clicks pass through).
- **Rationale**: Clicks outside the region go to underlying windows (FR-004). Crosshair only appears over the hittable overlay (bound client). WinSpot stays clickable when overlapping the target.
- **Alternatives considered**: Full-screen overlay with owner hole only (002 — steals other apps). Per-monitor layered window sized to client only (must move/resize window constantly; similar result). Low-level mouse hook (heavier, more invasive).

## Decision: Remove copy-idle

- **Decision**: All successful copies use the same clipboard + FR-014 path; never `SelectNone` / `CloseOverlay` as a side effect of copy.
- **Rationale**: Spec FR-001; needed for ten range samples.
- **Alternatives considered**: Keep idle on Pick only (rejected).

## Decision: Ten slots in code-generated rows

- **Decision**: Build 10 read-only distance/angle + copy rows in code (or equivalent ItemsControl) under live preview; `_rangeFilled` 0..10; click writes `_rangeFilled` then increments; at 10, toast `测距已满（10/10）` and swallow click; Clear sets texts empty and count 0.
- **Rationale**: Avoids 10× duplicated XAML; copy handlers share one method with slot index.
- **Alternatives considered**: Single committed pair (002 — superseded). User-editable boxes (rejected Q2 A).

## Decision: Shared toast with variable text

- **Decision**: Reuse the top toast border; set message to `已复制` or `测距已满（10/10）`; ~2s auto-dismiss.
- **Rationale**: Q1 A; do not reuse copy wording for full slots.
- **Alternatives considered**: Modal dialog (rejected).
