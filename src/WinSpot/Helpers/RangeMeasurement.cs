using WinSpot.Models;

namespace WinSpot.Helpers;

public static class RangeMeasurement
{
    public static ClientRangeSample Measure(double clientX, double clientY, int clientWidth, int clientHeight)
    {
        var cx = clientWidth / 2.0;
        var cy = clientHeight / 2.0;
        var dx = clientX - cx;
        var dy = clientY - cy;
        var distance = Math.Sqrt((dx * dx) + (dy * dy));
        var angle = distance == 0
            ? 0
            : Wrap180(Math.Atan2(dx, -dy) * (180.0 / Math.PI));

        return new ClientRangeSample
        {
            PointerX = clientX,
            PointerY = clientY,
            CenterX = cx,
            CenterY = cy,
            Distance = distance,
            AngleDegrees = angle,
            IsValid = true
        };
    }

    public static double Wrap180(double degrees)
    {
        var a = degrees;
        while (a > 180)
        {
            a -= 360;
        }

        while (a < -180)
        {
            a += 360;
        }

        return a;
    }
}
