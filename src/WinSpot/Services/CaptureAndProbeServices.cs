using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WinSpot.Helpers;
using WinSpot.Models;
using WinSpot.Native;

namespace WinSpot.Services;

public sealed class ClientProbeService : IClientProbeService
{
    public bool TrySamplePoint(nint hwnd, Point screenPoint, out ClientPointSample? sample)
    {
        sample = null;
        if (hwnd == nint.Zero || !NativeMethods.IsWindow(hwnd))
        {
            return false;
        }

        var pt = new NativeMethods.POINT { X = (int)screenPoint.X, Y = (int)screenPoint.Y };
        if (!NativeMethods.ScreenToClient(hwnd, ref pt))
        {
            return false;
        }

        if (!NativeMethods.GetClientRect(hwnd, out var client))
        {
            return false;
        }

        if (pt.X < 0 || pt.Y < 0 || pt.X >= client.Width || pt.Y >= client.Height)
        {
            return false;
        }

        if (!TryGetPixel(hwnd, pt.X, pt.Y, out var r, out var g, out var b))
        {
            return false;
        }

        sample = new ClientPointSample
        {
            X = pt.X,
            Y = pt.Y,
            R = r,
            G = g,
            B = b
        };
        return true;
    }

    public bool TryMeasureRange(nint hwnd, Point screenPoint, out ClientRangeSample? sample)
    {
        sample = null;
        if (hwnd == nint.Zero || !NativeMethods.IsWindow(hwnd))
        {
            return false;
        }

        var pt = new NativeMethods.POINT { X = (int)screenPoint.X, Y = (int)screenPoint.Y };
        if (!NativeMethods.ScreenToClient(hwnd, ref pt))
        {
            return false;
        }

        if (!NativeMethods.GetClientRect(hwnd, out var client))
        {
            return false;
        }

        if (pt.X < 0 || pt.Y < 0 || pt.X >= client.Width || pt.Y >= client.Height)
        {
            return false;
        }

        sample = RangeMeasurement.Measure(pt.X, pt.Y, client.Width, client.Height);
        return true;
    }

    public ClientRegion ClipRegionToClient(nint hwnd, ClientRegion raw)
    {
        if (!NativeMethods.GetClientRect(hwnd, out var client))
        {
            return new ClientRegion { X = 0, Y = 0, Width = 0, Height = 0 };
        }

        var x1 = Math.Clamp(raw.X, 0, client.Width);
        var y1 = Math.Clamp(raw.Y, 0, client.Height);
        var x2 = Math.Clamp(raw.Right, 0, client.Width);
        var y2 = Math.Clamp(raw.Bottom, 0, client.Height);

        var left = Math.Min(x1, x2);
        var top = Math.Min(y1, y2);
        var width = Math.Abs(x2 - x1);
        var height = Math.Abs(y2 - y1);

        return new ClientRegion { X = left, Y = top, Width = width, Height = height };
    }

    private static bool TryGetPixel(nint hwnd, int x, int y, out byte r, out byte g, out byte b)
    {
        r = g = b = 0;
        var origin = new NativeMethods.POINT { X = x, Y = y };
        if (!NativeMethods.ClientToScreen(hwnd, ref origin))
        {
            return false;
        }

        // Sample composed desktop pixel (screen DC) — consistent with DXGI capture path.
        var hdc = NativeMethods.GetDC(nint.Zero);
        if (hdc == nint.Zero)
        {
            return false;
        }

        try
        {
            var colorRef = NativeMethods.GetPixel(hdc, origin.X, origin.Y);
            if (colorRef == 0xFFFFFFFF)
            {
                return false;
            }

            r = (byte)(colorRef & 0xFF);
            g = (byte)((colorRef >> 8) & 0xFF);
            b = (byte)((colorRef >> 16) & 0xFF);
            return true;
        }
        finally
        {
            NativeMethods.ReleaseDC(nint.Zero, hdc);
        }
    }
}

public sealed class CaptureService : ICaptureService
{
    public CaptureResult CaptureClientArea(nint hwnd, CaptureSettings settings, string? windowTitle = null)
    {
        if (hwnd == nint.Zero || !NativeMethods.IsWindow(hwnd))
        {
            return CaptureResult.Fail("绑定窗口无效或已关闭。");
        }

        if (!NativeMethods.GetClientRect(hwnd, out var client) || client.Width <= 0 || client.Height <= 0)
        {
            return CaptureResult.Fail("无法读取客户区尺寸（窗口可能已最小化）。");
        }

        try
        {
            Directory.CreateDirectory(settings.SaveDirectory);
        }
        catch (Exception ex)
        {
            return CaptureResult.Fail($"无法创建保存目录：{ex.Message}");
        }

        var origin = new NativeMethods.POINT { X = 0, Y = 0 };
        if (!NativeMethods.ClientToScreen(hwnd, ref origin))
        {
            return CaptureResult.Fail("无法换算客户区屏幕坐标。");
        }

        try
        {
            using var gdiBmp = DxgiDesktopCapture.CaptureScreenRect(
                origin.X, origin.Y, client.Width, client.Height);

            var fileName = FilenameTemplate.Render(
                settings.FilenameTemplate,
                windowTitle ?? "window",
                DateTimeOffset.Now);
            var extension = settings.ImageFormat switch
            {
                CaptureImageFormat.Jpeg => ".jpg",
                CaptureImageFormat.Bmp => ".bmp",
                _ => ".png"
            };
            var path = Path.Combine(settings.SaveDirectory, fileName + extension);

            SaveBitmap(gdiBmp, path, settings);
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                return CaptureResult.Fail("截图文件未正确写入。");
            }

            return CaptureResult.Ok(path);
        }
        catch (Exception ex)
        {
            return CaptureResult.Fail($"DXGI(dxcam) 截图失败：{ex.Message}");
        }
    }

    private static void SaveBitmap(System.Drawing.Image image, string path, CaptureSettings settings)
    {
        switch (settings.ImageFormat)
        {
            case CaptureImageFormat.Jpeg:
                var jpeg = new JpegBitmapEncoder { QualityLevel = Math.Clamp(settings.JpegQuality, 1, 100) };
                jpeg.Frames.Add(BitmapFrame.Create(ToBitmapSource(image)));
                using (var fs = File.Create(path))
                {
                    jpeg.Save(fs);
                }
                break;
            case CaptureImageFormat.Bmp:
                var bmp = new BmpBitmapEncoder();
                bmp.Frames.Add(BitmapFrame.Create(ToBitmapSource(image)));
                using (var fs = File.Create(path))
                {
                    bmp.Save(fs);
                }
                break;
            default:
                var png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(ToBitmapSource(image)));
                using (var fs = File.Create(path))
                {
                    png.Save(fs);
                }
                break;
        }
    }

    private static BitmapSource ToBitmapSource(System.Drawing.Image image)
    {
        using var ms = new MemoryStream();
        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Position = 0;
        var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return decoder.Frames[0];
    }
}

public sealed class HotkeyService : IHotkeyService
{
    public bool TryRegister(nint windowHandle, int id, uint modifiers, uint virtualKey, out string? error)
    {
        error = null;
        if (NativeMethods.RegisterHotKey(windowHandle, id, modifiers, virtualKey))
        {
            return true;
        }

        error = "全局热键注册失败（可能与其它程序冲突，或系统「用 PrtSc 打开截图」已占用）。";
        return false;
    }

    public void Unregister(nint windowHandle, int id)
    {
        NativeMethods.UnregisterHotKey(windowHandle, id);
    }
}
