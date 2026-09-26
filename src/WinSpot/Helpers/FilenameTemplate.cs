using System.Text.RegularExpressions;

namespace WinSpot.Helpers;

public static partial class FilenameTemplate
{
    public static string Render(string template, string title, DateTimeOffset timestamp)
    {
        var safeTitle = SanitizeFileToken(string.IsNullOrWhiteSpace(title) ? "window" : title);
        var result = template
            .Replace("{title}", safeTitle, StringComparison.OrdinalIgnoreCase)
            .Replace("{yyyyMMdd_HHmmss}", timestamp.ToString("yyyyMMdd_HHmmss"), StringComparison.OrdinalIgnoreCase)
            .Replace("{yyyyMMdd}", timestamp.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{HHmmss}", timestamp.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase);

        return InvalidFileChars().Replace(result, "_");
    }

    private static string SanitizeFileToken(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length > 64)
        {
            trimmed = trimmed[..64];
        }

        return InvalidFileChars().Replace(trimmed, "_");
    }

    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]")]
    private static partial Regex InvalidFileChars();
}
