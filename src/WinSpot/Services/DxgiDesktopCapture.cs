using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using D3D11Device = Vortice.Direct3D11.ID3D11Device;
using MapFlags = Vortice.Direct3D11.MapFlags;

namespace WinSpot.Services;

/// <summary>
/// DXGI Desktop Duplication capture (same approach as Python dxcam).
/// Captures composed desktop pixels and crops to a screen rectangle.
/// </summary>
internal static class DxgiDesktopCapture
{
    private static readonly FeatureLevel[] FeatureLevels =
    [
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0
    ];

    public static Bitmap CaptureScreenRect(int screenLeft, int screenTop, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Capture size must be positive.");
        }

        using var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
        Exception? lastError = null;

        for (uint adapterIndex = 0; ; adapterIndex++)
        {
            var enumAdapterHr = factory.EnumAdapters1(adapterIndex, out var adapter);
            if (enumAdapterHr.Failure || adapter is null)
            {
                break;
            }

            using (adapter)
            {
                for (uint outputIndex = 0; ; outputIndex++)
                {
                    var enumOutputHr = adapter.EnumOutputs(outputIndex, out var output);
                    if (enumOutputHr.Failure || output is null)
                    {
                        break;
                    }

                    using (output)
                    using (var output1 = output.QueryInterface<IDXGIOutput1>())
                    {
                        var bounds = output1.Description.DesktopCoordinates;

                        if (screenLeft < bounds.Left || screenTop < bounds.Top ||
                            screenLeft + width > bounds.Right || screenTop + height > bounds.Bottom)
                        {
                            continue;
                        }

                        try
                        {
                            return CaptureFromOutput(
                                adapter,
                                output1,
                                bounds.Left,
                                bounds.Top,
                                bounds.Right,
                                bounds.Bottom,
                                screenLeft,
                                screenTop,
                                width,
                                height);
                        }
                        catch (Exception ex)
                        {
                            lastError = ex;
                        }
                    }
                }
            }
        }

        throw lastError ?? new InvalidOperationException(
            "未找到覆盖目标客户区的显示器，或 DXGI 桌面复制失败（窗口可能跨屏）。");
    }

    private static Bitmap CaptureFromOutput(
        IDXGIAdapter1 adapter,
        IDXGIOutput1 output1,
        int boundsLeft,
        int boundsTop,
        int boundsRight,
        int boundsBottom,
        int screenLeft,
        int screenTop,
        int width,
        int height)
    {
        var createHr = D3D11.D3D11CreateDevice(
            adapter,
            DriverType.Unknown,
            DeviceCreationFlags.BgraSupport,
            FeatureLevels,
            out D3D11Device? device);
        createHr.CheckError();
        if (device is null)
        {
            throw new InvalidOperationException("D3D11CreateDevice 返回空设备。");
        }

        using (device)
        {
            using var context = device.ImmediateContext;
            var outputWidth = (uint)(boundsRight - boundsLeft);
            var outputHeight = (uint)(boundsBottom - boundsTop);

            var stagingDesc = new Texture2DDescription
            {
                CPUAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = outputWidth,
                Height = outputHeight,
                MiscFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging
            };

            using var staging = device.CreateTexture2D(stagingDesc);
            using var duplication = output1.DuplicateOutput(device);

            // Give DWM a moment so the first acquire is not empty (dxcam-style).
            Thread.Sleep(50);

            IDXGIResource? desktopResource = null;
            var acquired = false;
            try
            {
                for (var attempt = 0; attempt < 10; attempt++)
                {
                    var hr = duplication.AcquireNextFrame(500, out var frameInfo, out desktopResource);
                    if (hr.Failure)
                    {
                        desktopResource?.Dispose();
                        desktopResource = null;
                        Thread.Sleep(30);
                        continue;
                    }

                    acquired = true;
                    if (desktopResource is not null &&
                        (frameInfo.LastPresentTime != 0 || frameInfo.AccumulatedFrames > 0 || attempt >= 2))
                    {
                        break;
                    }

                    duplication.ReleaseFrame();
                    acquired = false;
                    desktopResource?.Dispose();
                    desktopResource = null;
                    Thread.Sleep(30);
                }

                if (desktopResource is null)
                {
                    throw new InvalidOperationException("DXGI 在超时内未获得可用桌面帧。");
                }

                using var frameTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
                context.CopyResource(staging, frameTexture);

                var mapped = context.Map(staging, 0, MapMode.Read, MapFlags.None);
                try
                {
                    var srcX = screenLeft - boundsLeft;
                    var srcY = screenTop - boundsTop;
                    var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    var rect = new Rectangle(0, 0, width, height);
                    var bits = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    try
                    {
                        var dstStride = bits.Stride;
                        var srcPitch = mapped.RowPitch;
                        for (var y = 0; y < height; y++)
                        {
                            var srcOffset = checked((int)((srcY + y) * srcPitch + srcX * 4L));
                            var dstOffset = checked((int)(y * (long)dstStride));
                            var srcPtr = nint.Add(mapped.DataPointer, srcOffset);
                            var dstPtr = nint.Add(bits.Scan0, dstOffset);
                            CopyMemory(dstPtr, srcPtr, (nuint)(width * 4));
                        }
                    }
                    finally
                    {
                        bitmap.UnlockBits(bits);
                    }

                    return bitmap;
                }
                finally
                {
                    context.Unmap(staging, 0);
                }
            }
            finally
            {
                desktopResource?.Dispose();
                if (acquired)
                {
                    try { duplication.ReleaseFrame(); } catch { /* ignore */ }
                }
            }
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory", SetLastError = false)]
    private static extern void CopyMemory(nint dest, nint src, nuint count);
}
