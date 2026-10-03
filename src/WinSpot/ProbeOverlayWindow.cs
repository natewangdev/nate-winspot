using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinSpot.Native;

namespace WinSpot;

public enum ProbeOverlayKind
{
    Pick,
    Marquee,
    Range
}

public partial class ProbeOverlayWindow : Window
{
    public event Action<Point>? PointPicked;
    public event Action<Point>? PointerMoved;
    public event Action<Point, Point>? RegionSelected;
    public event Action? Cancelled;

    private readonly ProbeOverlayKind _kind;
    private readonly Window _ownerTool;
    private readonly Func<nint> _getTargetHwnd;
    private readonly Canvas _canvas;
    private readonly Rectangle _rubberBand;
    private readonly DispatcherTimer _hitRegionTimer;
    private bool _dragging;
    private Point _startScreen;
    private Point _startLocal;
    private bool _cancelRaised;

    public ProbeOverlayWindow(ProbeOverlayKind kind, Window ownerTool, Func<nint> getTargetHwnd)
    {
        _kind = kind;
        _ownerTool = ownerTool;
        _getTargetHwnd = getTargetHwnd;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        Topmost = true;
        ShowInTaskbar = false;
        Focusable = true;
        // Probe cursor only where the hit region is (bound client).
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
        _hitRegionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _hitRegionTimer.Tick += (_, _) => UpdateHitRegion();
        SourceInitialized += (_, _) =>
        {
            UpdateHitRegion();
            _hitRegionTimer.Start();
        };
        _ownerTool.LocationChanged += OwnerGeometryChanged;
        _ownerTool.SizeChanged += OwnerGeometryChanged;
        _ownerTool.StateChanged += OwnerGeometryChanged;
        Closed += (_, _) =>
        {
            _hitRegionTimer.Stop();
            _ownerTool.LocationChanged -= OwnerGeometryChanged;
            _ownerTool.SizeChanged -= OwnerGeometryChanged;
            _ownerTool.StateChanged -= OwnerGeometryChanged;
        };
    }

    private void OwnerGeometryChanged(object? sender, EventArgs e) => UpdateHitRegion();

    public void UpdateHitRegion()
    {
        var helper = new WindowInteropHelper(this);
        if (helper.Handle == nint.Zero)
        {
            return;
        }

        if (!NativeMethods.GetWindowRect(helper.Handle, out var overlayRect))
        {
            return;
        }

        var target = _getTargetHwnd();
        if (target == nint.Zero || !NativeMethods.IsWindow(target) ||
            !TryGetClientScreenRect(target, out var client))
        {
            ApplyRegion(helper.Handle, NativeMethods.CreateRectRgn(0, 0, 0, 0));
            return;
        }

        var hit = NativeMethods.CreateRectRgn(
            client.Left - overlayRect.Left,
            client.Top - overlayRect.Top,
            client.Right - overlayRect.Left,
            client.Bottom - overlayRect.Top);
        if (hit == nint.Zero)
        {
            return;
        }

        var ownerHelper = new WindowInteropHelper(_ownerTool);
        if (_ownerTool.WindowState != WindowState.Minimized &&
            ownerHelper.Handle != nint.Zero &&
            NativeMethods.GetWindowRect(ownerHelper.Handle, out var ownerRect))
        {
            var hole = NativeMethods.CreateRectRgn(
                ownerRect.Left - overlayRect.Left,
                ownerRect.Top - overlayRect.Top,
                ownerRect.Right - overlayRect.Left,
                ownerRect.Bottom - overlayRect.Top);
            if (hole != nint.Zero)
            {
                NativeMethods.CombineRgn(hit, hit, hole, NativeMethods.RGN_DIFF);
                NativeMethods.DeleteObject(hole);
            }
        }

        ApplyRegion(helper.Handle, hit);
    }

    private static bool TryGetClientScreenRect(nint hwnd, out NativeMethods.RECT clientScreen)
    {
        clientScreen = default;
        if (!NativeMethods.GetClientRect(hwnd, out var client) || client.Width <= 0 || client.Height <= 0)
        {
            return false;
        }

        var origin = new NativeMethods.POINT { X = 0, Y = 0 };
        if (!NativeMethods.ClientToScreen(hwnd, ref origin))
        {
            return false;
        }

        clientScreen = new NativeMethods.RECT
        {
            Left = origin.X,
            Top = origin.Y,
            Right = origin.X + client.Width,
            Bottom = origin.Y + client.Height
        };
        return true;
    }

    private static void ApplyRegion(nint overlayHwnd, nint region)
    {
        if (region == nint.Zero)
        {
            return;
        }

        NativeMethods.SetWindowRgn(overlayHwnd, region, true);
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
        if (_kind == ProbeOverlayKind.Range)
        {
            PointPicked?.Invoke(_startScreen);
            return;
        }

        if (_kind == ProbeOverlayKind.Pick)
        {
            // Hide briefly so GetPixel sees the target, not this overlay; keep mode armed.
            Hide();
            PointPicked?.Invoke(_startScreen);
            Show();
            Activate();
            Focus();
            UpdateHitRegion();
            return;
        }

        _dragging = true;
        CaptureMouse();
        UpdateRubberBand(_startLocal, _startLocal);
        _rubberBand.Visibility = Visibility.Visible;
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
        if (_kind == ProbeOverlayKind.Range)
        {
            PointerMoved?.Invoke(PointToScreen(e.GetPosition(this)));
            return;
        }

        if (_kind != ProbeOverlayKind.Marquee || !_dragging)
        {
            return;
        }

        UpdateRubberBand(_startLocal, e.GetPosition(this));
    }

    private void OnUp(object sender, MouseButtonEventArgs e)
    {
        if (_kind != ProbeOverlayKind.Marquee || !_dragging)
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
        UpdateHitRegion();
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
