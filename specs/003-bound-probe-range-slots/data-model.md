# Data Model: Bound-Window Probe & Range Slots

## ProbeArming vs ProbeActive

| State | Meaning |
|-------|---------|
| Armed | Pick/Marquee/Range tab selected, valid bind, overlay shown |
| Active | Pointer over bound client (overlay hit-test) |
| Idle | None tab, or overlay closed |

## RangeSlot[1..10]

| Field | Type | Rules |
|-------|------|-------|
| DistanceText | string | empty or formatted as 002 |
| AngleText | string | empty or one decimal |
| Filled | bool | both texts set together |

**NextIndex**: count of filled slots; write to `NextIndex` if `< 10`; else full.

**Clear**: all empty, NextIndex = 0. Live preview unchanged.

## ClientRangeSample

Unchanged from 002 (`RangeMeasurement`).

## Validation

- Slots read-only.
- Copy empty slot → not success.
- Full click → no slot mutation.
