# Quickstart: Bound-Window Probe & Range Slots

## Build

```powershell
dotnet restore WinSpot.sln
dotnet run --project src\WinSpot\WinSpot.csproj
```

## Manual checks

1. **Copy stays armed**: 拾取 → sample → 复制坐标 / 复制颜色 → still 拾取. 框选 copy region → still 框选.
2. **Other apps**: 拾取 selected; click another app (not bound) — that app gets the click; cursor normal. Hover bound client — crosshair; click samples.
3. **WinSpot**: With 拾取 selected, WinSpot buttons/tabs still work.
4. **Range slots**: 测距, hover live numbers; click 10 points → slots 1–10; 11th → toast `测距已满（10/10）`, slots unchanged, target not clicked.
5. **Clear**: 清空 → all empty; next click fills slot 1 only.
6. **Esc**: still returns to 无.
