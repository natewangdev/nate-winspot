using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WinSpot;

public partial class ProbeOverlayWindow : Window
{
    public event Action<Point>? PointPicked;
    public event Action<Point, Point>? RegionSelected;

    private readonly bool _marquee;
    private readonly Canvas _canvas;
    private readonly Rectangle _rubberBand;
    private bool _dragging;
    private Point _startScreen;
    private Point _startLocal;

    public ProbeOverlayWindow(bool marquee)
    {
        _marquee = marquee;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        Topmost = true;
        ShowInTaskbar = false;
        Cursor = marquee ? Cursors.Cross : Cursors.Pen;

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        _canvas = new Canvas();
        Content = _canvas;

        _rubberBand = new Rectangle
        {
            Stroke = new SolidColorBrush(Color.FromRgb(0, 200, 83)),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(40, 0, 200, 83)),
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false
        };
        _canvas.Children.Add(_rubberBand);

        MouseLeftButtonDown += OnDown;
        MouseMove += OnMove;
        MouseLeftButtonUp += OnUp;
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    private void OnDown(object sender, MouseButtonEventArgs e)
    {
        _startLocal = e.GetPosition(this);
        _startScreen = PointToScreen(_startLocal);
        if (!_marquee)
        {
            // Hide before sampling so screen GetPixel / DXGI sees the target, not this overlay.
            Hide();
            PointPicked?.Invoke(_startScreen);
            Close();
            return;
        }

        _dragging = true;
        CaptureMouse();
        UpdateRubberBand(_startLocal, _startLocal);
        _rubberBand.Visibility = Visibility.Visible;
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
        if (!_marquee || !_dragging)
        {
            return;
        }

        UpdateRubberBand(_startLocal, e.GetPosition(this));
    }

    private void OnUp(object sender, MouseButtonEventArgs e)
    {
        if (!_marquee || !_dragging)
        {
            return;
        }

        _dragging = false;
        ReleaseMouseCapture();
        _rubberBand.Visibility = Visibility.Collapsed;
        Hide();
        var end = PointToScreen(e.GetPosition(this));
        RegionSelected?.Invoke(_startScreen, end);
        Close();
    }

    private void UpdateRubberBand(Point a, Point b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var w = Math.Abs(b.X - a.X);
        var h = Math.Abs(b.Y - a.Y);
        Canvas.SetLeft(_rubberBand, x);
        Canvas.SetTop(_rubberBand, y);
        _rubberBand.Width = w;
        _rubberBand.Height = h;
    }
}
