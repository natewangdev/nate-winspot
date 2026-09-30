# Data Model: WinSpot Window Spot Tool

## BoundWindowInfo

| Field | Type | Notes |
|-------|------|-------|
| Handle | nint / decimal display only | HWND shown/copied as decimal digits |
| Title | string | May be empty |
| ClassName | string | Optional display |
| ProcessId | int | Optional |
| ProcessName | string | Optional |
| ClientWidth | int | pixels |
| ClientHeight | int | pixels |
| ClientScreenOriginX/Y | int | optional screen position |
| IsValid | bool | false if destroyed |

**Relationships**: 0..1 current binding in app session.

## ClientPointSample

| Field | Type | Notes |
|-------|------|-------|
| X | int | relative to client origin |
| Y | int | relative to client origin |
| ColorR/G/B | byte | |
| Hex | string | `#RRGGBB` derived |

## ClientRegion

| Field | Type | Notes |
|-------|------|-------|
| X | int | left (relative) |
| Y | int | top (relative) |
| Width | int | |
| Height | int | |
| Right / Bottom | computed | copy format `{X},{Y},{Right},{Bottom}` → `x1,y1,x2,y2` |

## CaptureSettings

| Field | Type | Default |
|-------|------|---------|
| SaveDirectory | string | `%USERPROFILE%\Pictures\WinSpot` |
| ImageFormat | enum PNG/JPEG/BMP | **JPEG** |
| JpegQuality | int 1–100 | 90 |
| FilenameTemplate | string | `{yyyyMMdd_HHmmss}` (time only; no title) |
| Hotkey | display/fixed | F12 (v1; not persisted / not remappable in this version) |

## CaptureResult

| Field | Type | Notes |
|-------|------|-------|
| Success | bool | |
| FilePath | string? | |
| ErrorMessage | string? | user-facing |

## Validation Rules

- JpegQuality clamped to 1–100.
- SaveDirectory must be creatable/writable before treating capture as success.
- ClientRegion width/height MUST be > 0 after clip.
- BoundWindowInfo.IsValid MUST be rechecked before pick/marquee/capture.
