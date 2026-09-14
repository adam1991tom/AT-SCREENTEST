using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using WinFormsScreen = System.Windows.Forms.Screen;

namespace ScreenTest;

public partial class MainWindow : Window
{
    private WinFormsScreen _currentScreen = WinFormsScreen.PrimaryScreen ?? WinFormsScreen.AllScreens[0];
    private int _patternIndex;
    private bool _fullscreen;
    private Rect _restoreBounds;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            PatternDisplay.SetPattern(TestPatternInfo.Order[_patternIndex]);
            RefreshOverlay();
        };
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                if (_fullscreen) ToggleFullscreen();
                break;

            case Key.Right:
            case Key.Space:
            case Key.PageDown:
                ChangePattern(1);
                break;

            case Key.Left:
            case Key.Back:
            case Key.PageUp:
                ChangePattern(-1);
                break;

            case Key.I:
                PatternDisplay.ShowOverlay = !PatternDisplay.ShowOverlay;
                RefreshOverlay();
                break;

            case Key.M:
                MoveToNextScreen();
                break;

            case Key.F:
            case Key.F11:
                ToggleFullscreen();
                break;

            case Key.OemPlus:
            case Key.Add:
                Adjust(1);
                break;

            case Key.OemMinus:
            case Key.Subtract:
                Adjust(-1);
                break;

            case Key.OemCloseBrackets:
                AdjustRows(1);
                break;

            case Key.OemOpenBrackets:
                AdjustRows(-1);
                break;

            case Key.Q when Keyboard.Modifiers == ModifierKeys.Control:
                Close();
                break;

            default:
                if (e.Key >= Key.D1 && e.Key <= Key.D9)
                {
                    JumpToPattern(e.Key - Key.D1);
                }
                else if (e.Key == Key.D0)
                {
                    JumpToPattern(9);
                }
                break;
        }
    }

    private void ChangePattern(int direction)
    {
        var count = TestPatternInfo.Order.Length;
        _patternIndex = ((_patternIndex + direction) % count + count) % count;
        PatternDisplay.SetPattern(TestPatternInfo.Order[_patternIndex]);
        RefreshOverlay();
    }

    private void JumpToPattern(int index)
    {
        if (index < 0 || index >= TestPatternInfo.Order.Length) return;
        _patternIndex = index;
        PatternDisplay.SetPattern(TestPatternInfo.Order[_patternIndex]);
        RefreshOverlay();
    }

    private void Adjust(int direction)
    {
        var pattern = TestPatternInfo.Order[_patternIndex];
        if (pattern == TestPattern.Checkerboard) PatternDisplay.AdjustCheckerSize(direction);
        if (pattern == TestPattern.MotionLine) PatternDisplay.AdjustMotionSpeed(direction);
        if (pattern == TestPattern.LedTileMap) PatternDisplay.AdjustTileColumns(direction);
        RefreshOverlay();
    }

    private void AdjustRows(int direction)
    {
        if (TestPatternInfo.Order[_patternIndex] != TestPattern.LedTileMap) return;
        PatternDisplay.AdjustTileRows(direction);
        RefreshOverlay();
    }

    /// <summary>The monitor the window is currently sitting on, wherever it's been dragged to.</summary>
    private WinFormsScreen CurrentScreen()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        return hwnd == IntPtr.Zero ? _currentScreen : WinFormsScreen.FromHandle(hwnd);
    }

    private void MoveToNextScreen()
    {
        _currentScreen = MonitorHelper.NextScreen(CurrentScreen());
        if (_fullscreen)
        {
            MonitorHelper.CoverScreen(this, _currentScreen);
        }
        else
        {
            var area = _currentScreen.WorkingArea;
            Width = Math.Min(Width, area.Width - 80);
            Height = Math.Min(Height, area.Height - 80);
            Left = area.Left + (area.Width - Width) / 2;
            Top = area.Top + (area.Height - Height) / 2;
        }
        RefreshOverlay();
    }

    private void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        if (_fullscreen)
        {
            _restoreBounds = new Rect(Left, Top, Width, Height);
            _currentScreen = CurrentScreen();
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            MonitorHelper.CoverScreen(this, _currentScreen);
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
            Left = _restoreBounds.Left;
            Top = _restoreBounds.Top;
            Width = _restoreBounds.Width;
            Height = _restoreBounds.Height;
        }
        RefreshOverlay();
    }

    private void RefreshOverlay()
    {
        _currentScreen = CurrentScreen();
        var mode = DisplayInfo.GetCurrentMode(_currentScreen.DeviceName);
        var dpiScale = VisualTreeHelperDpi();
        var pattern = TestPatternInfo.Order[_patternIndex];

        var resLine = mode is { } m
            ? $"{m.Width} x {m.Height} @ {m.RefreshHz} Hz"
            : "resolution unavailable";

        var lines = new List<string>
        {
            $"AT ScreenTest — {_currentScreen.DeviceName}",
            resLine,
            $"DPI scale: {dpiScale:0.00}x   Monitor {Array.IndexOf(WinFormsScreen.AllScreens, _currentScreen) + 1}/{WinFormsScreen.AllScreens.Length}",
            $"Pattern {_patternIndex + 1}/{TestPatternInfo.Order.Length}: {pattern.Name()}",
        };

        var adjustment = PatternDisplay.AdjustmentSummary();
        if (!string.IsNullOrEmpty(adjustment)) lines.Add(adjustment);

        lines.Add("");
        lines.Add("<- / -> pattern   1-0 jump   I overlay   M next monitor   F fullscreen   Esc exit fullscreen");
        lines.Add("+/- adjust   [ / ] adjust rows (tile map)   drag window to move to another screen");

        PatternDisplay.OverlayText = string.Join("\n", lines);
        PatternDisplay.InvalidateVisual();
    }

    private double VisualTreeHelperDpi()
    {
        var source = PresentationSource.FromVisual(this);
        return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
    }
}
