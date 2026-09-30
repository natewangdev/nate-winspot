using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using WinSpot.Native;

namespace WinSpot;

public partial class ProbeOverlayWindow : Window
{
    public event Action<Point>? PointPicked;
    public event Action<Point, Point>? RegionSelected;
    public event Action? Cancelled;

    private readonly bool _marquee;
    private readonly Window _ownerTool;
    private readonly Canvas _canvas;
    private readonly Rectangle _rubberBand;
    private bool _dragging;
    private Point _startScreen;
    private Point _startLocal;
    private bool _cancelRaised;

    public ProbeOverlayWindow(bool marquee, Window ownerTool)
    {
        _marquee = marquee;
        _ownerTool = ownerTool;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        Topmost = true;
        ShowInTaskbar = false;
        Focusable = true;
        // FR-017: mode crosshair outside WinSpot (owner is punched out of the hit region).
        Cursor = Cursors.Cross;

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
        KeyDown += OnKeyDown;
        SourceInitialized += (_, _) => UpdateOwnerExclusionRegion();
        _ownerTool.LocationChanged += OwnerGeometryChanged;
        _ownerTool.SizeChanged += OwnerGeometryChanged;
        _ownerTool.StateChanged += OwnerGeometryChanged;
        Closed += (_, _) =>
        {
            _ownerTool.LocationChanged -= OwnerGeometryChanged;
            _ownerTool.SizeChanged -= OwnerGeometryChanged;
            _ownerTool.StateChanged -= OwnerGeometryChanged;
        };
    }

    private void OwnerGeometryChanged(object? sender, EventArgs e) => UpdateOwnerExclusionRegion();

    public void UpdateOwnerExclusionRegion()
    {
        var helper = new WindowInteropHelper(this);
        if (helper.Handle == nint.Zero)
        {
            return;
        }

        var ownerHelper = new WindowInteropHelper(_ownerTool);
        if (ownerHelper.Handle == nint.Zero)
        {
            return;
        }

        // Full virtual-screen region in overlay client coords, minus owner tool frame.
        var full = NativeMethods.CreateRectRgn(0, 0, (int)Width, (int)Height);
        if (full == nint.Zero)
        {
            return;
        }

        try
        {
            if (!NativeMethods.GetWindowRect(helper.Handle, out var overlayRect))
            {
                NativeMethods.DeleteObject(full);
                full = nint.Zero;
                return;
            }

            // Rebuild full region in physical pixels matching the HWND.
            NativeMethods.DeleteObject(full);
            full = NativeMethods.CreateRectRgn(0, 0, overlayRect.Width, overlayRect.Height);
            if (full == nint.Zero)
            {
                return;
            }

            if (_ownerTool.WindowState == WindowState.Minimized ||
                !NativeMethods.GetWindowRect(ownerHelper.Handle, out var ownerRect))
            {
                NativeMethods.SetWindowRgn(helper.Handle, full, true);
                full = nint.Zero;
                return;
            }

            var left = ownerRect.Left - overlayRect.Left;
            var top = ownerRect.Top - overlayRect.Top;
            var right = ownerRect.Right - overlayRect.Left;
            var bottom = ownerRect.Bottom - overlayRect.Top;
            var hole = NativeMethods.CreateRectRgn(left, top, right, bottom);
            if (hole == nint.Zero)
            {
                NativeMethods.DeleteObject(full);
                full = nint.Zero;
                return;
            }

            NativeMethods.CombineRgn(full, full, hole, NativeMethods.RGN_DIFF);
            NativeMethods.DeleteObject(hole);
            NativeMethods.SetWindowRgn(helper.Handle, full, true);
            full = nint.Zero;
        }
        finally
        {
            if (full != nint.Zero)
            {
                NativeMethods.DeleteObject(full);
            }
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        RaiseCancelledAndClose();
    }

    public void RaiseCancelledAndClose()
    {
        if (_cancelRaised)
        {
            Close();
            return;
        }

        _cancelRaised = true;
        Cancelled?.Invoke();
        Close();
    }

    private void OnDown(object sender, MouseButtonEventArgs e)
    {
        _startLocal = e.GetPosition(this);
        _startScreen = PointToScreen(_startLocal);
        if (!_marquee)
        {
            // Hide briefly so GetPixel sees the target, not this overlay; keep mode armed.
            Hide();
            PointPicked?.Invoke(_startScreen);
            Show();
            Activate();
            Focus();
            UpdateOwnerExclusionRegion();
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
        Show();
        Activate();
        Focus();
        UpdateOwnerExclusionRegion();
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
