namespace WinSpot.Models;

public sealed class ClientPointSample
{
    public int X { get; init; }
    public int Y { get; init; }
    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }

    public string CoordinateText => $"{X},{Y}";
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";
    public string RgbText => $"RGB({R}, {G}, {B})";
}
