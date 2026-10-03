# Data Model: Probe Range Copy-Idle

## ProbeMode (session)

| Value | UI label | Overlay |
|-------|----------|---------|
| None | 无 | closed |
| Pick | 拾取 | pick |
| Marquee | 框选 | marquee |
| Range | 测距 | range (move + click) |

**Transitions**

- Tab select → corresponding mode (unbound → forced None + status).
- Esc or select None → None, overlay closed.
- Successful **result** copy on Pick / Marquee / Range → None, overlay closed; stored samples remain.
- Bind-field copy → no transition.

## ClientRangeSample

| Field | Type | Notes |
|-------|------|-------|
| PointerX / PointerY | double | client-relative pointer |
| CenterX / CenterY | double | `width/2`, `height/2` |
| Distance | double | Euclidean pixels; ≥ 0 |
| AngleDegrees | double | [−180, 180]; 0 at coincidence |
| IsValid | bool | false if hwnd invalid or pointer outside client |

**Live vs committed**: two instances (or two UI bindings). Move updates live only. Click copies live → committed when `IsValid`. Invalid click does not replace committed.

## Validation Rules

- Distance/angle numbers only when `IsValid`.
- Copy of empty committed text is not success (no idle).
- Angle wrap: values after computation MUST be in [−180, 180]. Prefer −180 vs +180 consistently (either is in-range; do not emit 180.1).

## Unchanged 001 entities

`BoundWindowInfo`, `ClientPointSample`, `ClientRegion`, `CaptureSettings` unchanged. Idle-after-copy MUST NOT clear Pick/Marquee/Range stored UI text.
