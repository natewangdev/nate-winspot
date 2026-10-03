# Contract: Range probe + copy-idle

Internal only (not HTTP). Extends `001` `contracts/win32-services.md`.

## IClientProbeService (add)

```
bool TryMeasureRange(nint hwnd, Point screenPoint, out ClientRangeSample? sample)
```

- Convert screen → client; read client size; reject if hwnd dead or point outside `[0,width)×[0,height)`.
- Center `(width/2.0, height/2.0)`.
- Distance = hypot(dx, dy).
- AngleDegrees = Atan2(dx, −dy) in degrees, wrapped to [−180, 180].
- If distance is 0: AngleDegrees = 0.

`TrySamplePoint` / `ClipRegionToClient` unchanged.

## ProbeOverlayWindow

| Mode | Move | Left click | Esc |
|------|------|------------|-----|
| Pick | none (except marquee drag N/A) | `PointPicked` (may hide briefly for GetPixel) | `Cancelled` |
| Marquee | rubber-band while drag | release → `RegionSelected` | `Cancelled` |
| Range | `PointerMoved(screen)` | `PointPicked` **without** hide-for-pixel | `Cancelled` |

Owner exclusion region unchanged (FR-017). Cursor remains Cross outside WinSpot.

## Copy idle (UI contract)

| Action | Clipboard | Then |
|--------|-----------|------|
| Copy pick coord / pick color | existing formats | select None + close overlay |
| Copy marquee region | `x1,y1,x2,y2` | select None + close overlay |
| Copy range distance | display string | select None + close overlay |
| Copy range angle | display string | select None + close overlay |
| Copy bind fields | existing | stay in current probe mode |
| Copy empty result | no clipboard change | stay; status “没有可复制的内容。” |

Display: distance integer without decimals when mathematically integer else one decimal; angle at least one decimal (e.g. `0.0`, `90.0`).

## Copy UX

Unchanged 001 FR-014: button flash ~1.5s + toast `已复制` ~2s on success.
