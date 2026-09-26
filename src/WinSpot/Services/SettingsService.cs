using System.IO;
using System.Text.Json;
using WinSpot.Models;

namespace WinSpot.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _settingsPath;

    public SettingsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinSpot");
        Directory.CreateDirectory(dir);
        _settingsPath = Path.Combine(dir, "settings.json");
    }

    public CaptureSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new CaptureSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<CaptureSettings>(json, JsonOptions) ?? new CaptureSettings();
        }
        catch
        {
            return new CaptureSettings();
        }
    }

    public void Save(CaptureSettings settings)
    {
        settings.JpegQuality = Math.Clamp(settings.JpegQuality, 1, 100);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
