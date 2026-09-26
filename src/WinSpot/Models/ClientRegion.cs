namespace WinSpot.Models;

public sealed class ClientRegion
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public int Right => X + Width;
    public int Bottom => Y + Height;

    /// <summary>Copy format: x1,y1,x2,y2 (top-left, bottom-right).</summary>
    public string CopyText => $"{X},{Y},{Right},{Bottom}";
}
