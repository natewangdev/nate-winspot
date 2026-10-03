# Quickstart: Probe Range Copy-Idle

## Prerequisites

- Windows 10/11 x64
- .NET 8 SDK
- Bound-window workflow from `specs/001-window-spot-tool/quickstart.md`

## Build & Run

```powershell
dotnet restore WinSpot.sln
dotnet run --project src\WinSpot\WinSpot.csproj
```

## Manual validation

1. **Bind** Notepad (or similar) with a known client size.
2. **Pick copy-idle**: Select **拾取**, click client outside WinSpot, **复制坐标** → **无** selected, overlay gone; reselect **拾取** → last coord still shown. Repeat with **复制颜色**.
3. **Bind copy does not idle**: Select **拾取** again, copy **句柄** → still **拾取**.
4. **Marquee copy-idle**: **框选**, drag region, **复制区域** → **无**; reselect **框选** → last `x1,y1,x2,y2` remains.
5. **Range live**: **测距**. Hover WinSpot → normal cursor/clicks. Hover bound client outside WinSpot → live distance/angle update. Geometric center ≈ distance `0`. Straight up from center ≈ `0.0°`. Straight right ≈ `+90.0°`.
6. **Range commit**: Click on client → committed fields fill; tab stays **测距**; moving the pointer updates live only.
7. **Range copy-idle**: **复制** distance or angle → **无**; reselect **测距** → committed values remain.
8. **Esc**: From **测距**, Esc → **无**.
9. **Off-client click**: Click outside bound client (still outside WinSpot) → no overwrite of committed values.
10. **Unbound**: Clear bind, try **测距** → forced **无** + bind hint.

## Spec Kit continue

```text
/speckit-converge
```
