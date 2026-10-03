# Contract: Probe hit-test region & range slots

## ProbeOverlayWindow

- Still covers virtual screen (rubber-band local coords).
- **Hit region**: bound window **client** in overlay physical pixels, minus WinSpot window rect if overlapping.
- Invalid/zero client → empty region.
- Refresh while shown so target move/resize is followed.
- Events unchanged: Pick `PointPicked`; Marquee drag; Range `PointerMoved` + `PointPicked`.
- Cursor on overlay remains Cross; OS default elsewhere.

## Copy

| Action | Idle to None? |
|--------|----------------|
| Any copy | No |

## Range UI

- Live distance/angle (not copy-idle, not a slot).
- 10 rows: distance, copy distance, angle, copy angle.
- Clear: empties slots only.
- Full click: toast text `测距已满（10/10）`.
