using System.IO;

namespace WinSpot.Models;

public enum CaptureImageFormat
{
    Png,
    Jpeg,
    Bmp
}

public sealed class CaptureSettings
{
    public string SaveDirectory { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "WinSpot");

    public CaptureImageFormat ImageFormat { get; set; } = CaptureImageFormat.Jpeg;

    /// <summary>JPEG quality 1–100.</summary>
    public int JpegQuality { get; set; } = 90;

    public string FilenameTemplate { get; set; } = "{yyyyMMdd_HHmmss}";
}

public sealed class CaptureResult
{
    public bool Success { get; init; }
    public string? FilePath { get; init; }
    public string? ErrorMessage { get; init; }

    public static CaptureResult Ok(string path) => new() { Success = true, FilePath = path };
    public static CaptureResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
