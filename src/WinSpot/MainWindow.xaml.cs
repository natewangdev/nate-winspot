using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using WinSpot.Helpers;
using WinSpot.Models;
using WinSpot.Native;
using WinSpot.Services;

namespace WinSpot;

public partial class MainWindow : Window
{
    private const int HotkeyId = 0x11;
    private const uint VkF11 = 0x7A;

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
    private ProbeOverlayWindow? _overlay;

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

        if (!_hotkeyService.TryRegister(helper.Handle, HotkeyId, 0, VkF11, out var error))
        {
            SetStatus(error ?? "F11 热键注册失败。");
        }
        else
        {
            SetStatus("就绪。拖动准星绑定窗口；F11 截取客户区。");
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

    private void Mode_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        CloseOverlay();

        if (_bound is null || !_bound.IsValid)
        {
            if (ModePick.IsChecked == true || ModeMarquee.IsChecked == true)
            {
                ModeNone.IsChecked = true;
                SetStatus("请先绑定窗口。");
            }

            return;
        }

        if (ModePick.IsChecked == true)
        {
            OpenOverlay(marquee: false);
            SetStatus("拾取模式：点击绑定窗口客户区（Esc 取消）。");
        }
        else if (ModeMarquee.IsChecked == true)
        {
            OpenOverlay(marquee: true);
            SetStatus("框选模式：在客户区内拖拽（Esc 取消）。");
        }
    }

    private void OpenOverlay(bool marquee)
    {
        CloseOverlay();
        _overlay = new ProbeOverlayWindow(marquee);
        if (marquee)
        {
            _overlay.RegionSelected += OnRegionSelected;
        }
        else
        {
            _overlay.PointPicked += OnPointPicked;
        }

        _overlay.Closed += (_, _) =>
        {
            _overlay = null;
            ModeNone.IsChecked = true;
        };
        _overlay.Show();
    }

    private void CloseOverlay()
    {
        if (_overlay is not null)
        {
            _overlay.Close();
            _overlay = null;
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
        ClipboardHelper.SetText(_bound?.HandleText ?? "");

    private void CopyTitle_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(TitleText.Text);

    private void CopyClass_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(ClassText.Text);

    private void CopyProcess_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(ProcessText.Text);

    private void CopyClientSize_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(ClientSizeText.Text);

    private void CopyClientOrigin_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(ClientOriginText.Text);

    private void CopyPickCoord_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(PickCoordText.Text);

    private void CopyPickColor_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(PickColorText.Text);

    private void CopyRegion_Click(object sender, RoutedEventArgs e) =>
        ClipboardHelper.SetText(RegionText.Text);
}
