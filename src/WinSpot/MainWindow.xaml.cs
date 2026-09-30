using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using WinSpot.Helpers;
using WinSpot.Models;
using WinSpot.Native;
using WinSpot.Services;

namespace WinSpot;

public partial class MainWindow : Window
{
    private const int HotkeyId = 0x11;
    private const uint VkF12 = 0x7B;
    private static readonly TimeSpan CopyButtonSuccessDuration = TimeSpan.FromMilliseconds(1500);
    private static readonly TimeSpan CopyToastDuration = TimeSpan.FromSeconds(2);

    private readonly IWindowBindService _bindService = new WindowBindService();
    private readonly IClientProbeService _probeService = new ClientProbeService();
    private readonly ICaptureService _captureService = new CaptureService();
    private readonly IHotkeyService _hotkeyService = new HotkeyService();
    private readonly ISettingsService _settingsService = new SettingsService();

    private BoundWindowInfo? _bound;
    private CaptureSettings _settings = new();
    private bool _bindingDrag;
    private HwndSource? _hwndSource;
    private bool _suppressSettingsEvent;
    private bool _suppressModeChange;
    private ProbeOverlayWindow? _overlay;
    private DispatcherTimer? _copyToastTimer;
    private readonly Dictionary<Button, DispatcherTimer> _copyButtonTimers = new();

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();
        LoadSettingsIntoUi();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(WndProc);

        if (!_hotkeyService.TryRegister(helper.Handle, HotkeyId, 0, VkF12, out var error))
        {
            SetStatus(error ?? "F12 热键注册失败。");
        }
        else
        {
            SetStatus("就绪。拖动准星绑定窗口；F12 截取客户区。");
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        CloseOverlay();
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != nint.Zero)
        {
            _hotkeyService.Unregister(hwnd, HotkeyId);
        }

        _hwndSource?.RemoveHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            CaptureBoundClient();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void BindCrosshair_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _bindingDrag = true;
        BindCrosshair.CaptureMouse();
        SetStatus("拖到目标窗口后松开…");
    }

    private void BindCrosshair_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_bindingDrag)
        {
            return;
        }

        var screen = BindCrosshair.PointToScreen(e.GetPosition(BindCrosshair));
        var info = _bindService.TryResolveFromScreenPoint(screen);
        if (info is not null)
        {
            SetStatus($"悬停: {info.Title} ({info.HandleText})");
        }
    }

    private void BindCrosshair_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_bindingDrag)
        {
            return;
        }

        _bindingDrag = false;
        BindCrosshair.ReleaseMouseCapture();
        var screen = BindCrosshair.PointToScreen(e.GetPosition(BindCrosshair));
        var info = _bindService.TryResolveFromScreenPoint(screen);
        if (info is null)
        {
            SetStatus("未解析到窗口。");
            return;
        }

        var self = new WindowInteropHelper(this).Handle;
        if (info.Handle == self || IsDescendantOf(info.Handle, self))
        {
            SetStatus("不能绑定 WinSpot 自身窗口。");
            return;
        }

        _bound = info;
        ApplyBoundToUi(info);
        SetStatus($"已绑定: {info.Title}");
    }

    private static bool IsDescendantOf(nint hwnd, nint root)
    {
        var current = hwnd;
        while (current != nint.Zero)
        {
            if (current == root)
            {
                return true;
            }

            current = GetParent(current);
        }

        return false;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern nint GetParent(nint hWnd);

    private void ClearBind_Click(object sender, RoutedEventArgs e)
    {
        _bound = null;
        ApplyBoundToUi(null);
        SetStatus("已清除绑定。");
    }

    private void RefreshBind_Click(object sender, RoutedEventArgs e)
    {
        if (_bound is null)
        {
            SetStatus("当前没有绑定窗口。");
            return;
        }

        _bound = _bindService.Refresh(_bound);
        ApplyBoundToUi(_bound);
        SetStatus(_bound?.IsValid == true ? "已刷新。" : "绑定窗口已失效。");
    }

    private void ApplyBoundToUi(BoundWindowInfo? info)
    {
        HandleText.Text = info?.HandleText ?? "";
        TitleText.Text = info?.Title ?? "";
        ClassText.Text = info?.ClassName ?? "";
        ProcessText.Text = info is null ? "" : $"{info.ProcessName} ({info.ProcessId})";
        ClientSizeText.Text = info?.ClientSizeText ?? "";
        ClientOriginText.Text = info is null ? "" : $"{info.ClientScreenOriginX},{info.ClientScreenOriginY}";
    }

    private void ProbeModeTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _suppressModeChange)
        {
            return;
        }

        // TabControl raises SelectionChanged for nested selectors too.
        if (!ReferenceEquals(e.Source, ProbeModeTabs))
        {
            return;
        }

        CloseOverlay();

        if (ProbeModeTabs.SelectedItem == TabNone)
        {
            SetStatus("探测已关闭。");
            return;
        }

        if (_bound is null || !_bound.IsValid)
        {
            SelectNoneTabQuietly();
            SetStatus("请先绑定窗口。");
            return;
        }

        if (ProbeModeTabs.SelectedItem == TabPick)
        {
            OpenOverlay(marquee: false);
            SetStatus("拾取模式：在 WinSpot 外点击绑定窗口客户区；Esc 或「无」结束。");
        }
        else if (ProbeModeTabs.SelectedItem == TabMarquee)
        {
            OpenOverlay(marquee: true);
            SetStatus("框选模式：在 WinSpot 外拖拽客户区；Esc 或「无」结束。");
        }
    }

    private void SelectNoneTabQuietly()
    {
        _suppressModeChange = true;
        ProbeModeTabs.SelectedItem = TabNone;
        _suppressModeChange = false;
    }

    private void OpenOverlay(bool marquee)
    {
        CloseOverlay();
        _overlay = new ProbeOverlayWindow(marquee, this);
        if (marquee)
        {
            _overlay.RegionSelected += OnRegionSelected;
        }
        else
        {
            _overlay.PointPicked += OnPointPicked;
        }

        _overlay.Cancelled += () => SelectNoneTabQuietly();
        _overlay.Closed += (_, _) => _overlay = null;
        _overlay.Show();
        _overlay.Activate();
        _overlay.Focus();
        _overlay.UpdateOwnerExclusionRegion();
    }

    private void CloseOverlay()
    {
        if (_overlay is not null)
        {
            var closing = _overlay;
            _overlay = null;
            closing.Close();
        }
    }

    private void OnPointPicked(Point screen)
    {
        if (_bound is null)
        {
            return;
        }

        if (_probeService.TrySamplePoint(_bound.Handle, screen, out var sample) && sample is not null)
        {
            PickCoordText.Text = sample.CoordinateText;
            PickColorText.Text = sample.Hex;
            SetStatus($"已拾取 {sample.CoordinateText} {sample.Hex}");
        }
        else
        {
            SetStatus("点击不在绑定窗口客户区内，或采样失败。");
        }
    }

    private void OnRegionSelected(Point screenStart, Point screenEnd)
    {
        if (_bound is null)
        {
            return;
        }

        var a = new NativeMethods.POINT { X = (int)screenStart.X, Y = (int)screenStart.Y };
        var b = new NativeMethods.POINT { X = (int)screenEnd.X, Y = (int)screenEnd.Y };
        NativeMethods.ScreenToClient(_bound.Handle, ref a);
        NativeMethods.ScreenToClient(_bound.Handle, ref b);

        var raw = new ClientRegion
        {
            X = Math.Min(a.X, b.X),
            Y = Math.Min(a.Y, b.Y),
            Width = Math.Abs(b.X - a.X),
            Height = Math.Abs(b.Y - a.Y)
        };
        var clipped = _probeService.ClipRegionToClient(_bound.Handle, raw);
        if (clipped.Width <= 0 || clipped.Height <= 0)
        {
            SetStatus("框选无效或未落在客户区内。");
            return;
        }

        RegionText.Text = clipped.CopyText;
        SetStatus($"已框选 {clipped.CopyText}");
    }

    private void LoadSettingsIntoUi()
    {
        _suppressSettingsEvent = true;
        SaveDirText.Text = _settings.SaveDirectory;
        FormatCombo.ItemsSource = Enum.GetValues<CaptureImageFormat>();
        FormatCombo.SelectedItem = _settings.ImageFormat;
        JpegQualitySlider.Value = _settings.JpegQuality;
        JpegQualityLabel.Text = ((int)JpegQualitySlider.Value).ToString();
        FilenameTemplateText.Text = _settings.FilenameTemplate;
        _suppressSettingsEvent = false;
    }

    private void ReadSettingsFromUi()
    {
        _settings.SaveDirectory = SaveDirText.Text.Trim();
        if (FormatCombo.SelectedItem is CaptureImageFormat fmt)
        {
            _settings.ImageFormat = fmt;
        }

        _settings.JpegQuality = (int)JpegQualitySlider.Value;
        _settings.FilenameTemplate = string.IsNullOrWhiteSpace(FilenameTemplateText.Text)
            ? "{yyyyMMdd_HHmmss}"
            : FilenameTemplateText.Text.Trim();
    }

    private void Settings_Changed(object sender, RoutedEventArgs e)
    {
        if (_suppressSettingsEvent || !IsLoaded)
        {
            return;
        }

        JpegQualityLabel.Text = ((int)JpegQualitySlider.Value).ToString();
    }

    private void BrowseSaveDir_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择截图保存目录"
        };
        if (dialog.ShowDialog() == true)
        {
            SaveDirText.Text = dialog.FolderName;
        }
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        ReadSettingsFromUi();
        _settingsService.Save(_settings);
        SetStatus("设置已保存。");
    }

    private void CaptureNow_Click(object sender, RoutedEventArgs e) => CaptureBoundClient();

    private void CaptureBoundClient()
    {
        if (_bound is null || !_bound.IsValid)
        {
            SetStatus("请先绑定有效窗口再截图。");
            return;
        }

        ReadSettingsFromUi();
        var refreshed = _bindService.Refresh(_bound);
        if (refreshed is null || !refreshed.IsValid)
        {
            _bound = refreshed;
            ApplyBoundToUi(_bound);
            SetStatus("绑定窗口已失效，无法截图。");
            return;
        }

        _bound = refreshed;
        ApplyBoundToUi(_bound);

        // Hide tool UI so DXGI composed capture is not covered by WinSpot.
        var previousState = WindowState;
        try
        {
            WindowState = WindowState.Minimized;
            // Allow DWM to present without this window.
            System.Threading.Thread.Sleep(80);
            var result = _captureService.CaptureClientArea(_bound.Handle, _settings, _bound.Title);
            SetStatus(result.Success ? $"已保存: {result.FilePath}" : result.ErrorMessage ?? "截图失败。");
        }
        finally
        {
            WindowState = previousState == WindowState.Minimized ? WindowState.Normal : previousState;
            Activate();
        }
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void CopyHandle_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, _bound?.HandleText ?? "");

    private void CopyTitle_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, TitleText.Text);

    private void CopyClass_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, ClassText.Text);

    private void CopyProcess_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, ProcessText.Text);

    private void CopyClientSize_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, ClientSizeText.Text);

    private void CopyClientOrigin_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, ClientOriginText.Text);

    private void CopyPickCoord_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, PickCoordText.Text);

    private void CopyPickColor_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, PickColorText.Text);

    private void CopyRegion_Click(object sender, RoutedEventArgs e) =>
        CopyText(sender, RegionText.Text);

    private void CopyText(object sender, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            SetStatus("没有可复制的内容。");
            return;
        }

        ClipboardHelper.SetText(text);
        if (sender is Button button)
        {
            FlashCopyButton(button);
        }

        ShowCopyToast();
    }

    private void FlashCopyButton(Button button)
    {
        if (_copyButtonTimers.TryGetValue(button, out var existing))
        {
            existing.Stop();
            _copyButtonTimers.Remove(button);
        }

        var original = button.Content;
        button.Content = "已复制";
        var timer = new DispatcherTimer { Interval = CopyButtonSuccessDuration };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            button.Content = original;
            _copyButtonTimers.Remove(button);
        };
        _copyButtonTimers[button] = timer;
        timer.Start();
    }

    private void ShowCopyToast()
    {
        CopyToast.Visibility = Visibility.Visible;
        _copyToastTimer?.Stop();
        _copyToastTimer = new DispatcherTimer { Interval = CopyToastDuration };
        _copyToastTimer.Tick += (_, _) =>
        {
            _copyToastTimer.Stop();
            CopyToast.Visibility = Visibility.Collapsed;
        };
        _copyToastTimer.Start();
    }
}
