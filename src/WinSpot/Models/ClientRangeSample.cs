namespace WinSpot.Models;

public sealed class ClientRangeSample
{
    public double PointerX { get; init; }
    public double PointerY { get; init; }
    public double CenterX { get; init; }
    public double CenterY { get; init; }
    public double Distance { get; init; }
    public double AngleDegrees { get; init; }
    public bool IsValid { get; init; }

    public string DistanceText => FormatDistance(Distance);
    public string AngleText => AngleDegrees.ToString("0.0");

    public static string FormatDistance(double distance)
    {
        if (double.IsNaN(distance) || double.IsInfinity(distance))
        {
            return "";
        }

        var rounded = Math.Round(distance);
        if (Math.Abs(distance - rounded) < 1e-6)
        {
            return ((int)rounded).ToString();
        }

        return distance.ToString("0.0");
    }
}
