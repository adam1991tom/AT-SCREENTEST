using System.Windows;
using System.Windows.Input;
using WinFormsScreen = System.Windows.Forms.Screen;

namespace ScreenTest;

public partial class MainWindow : Window
{
    private WinFormsScreen _currentScreen = WinFormsScreen.PrimaryScreen ?? WinFormsScreen.AllScreens[0];
    private int _patternIndex;
    private bool _fullscreen = true;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => MonitorHelper.CoverScreen(this, _currentScreen);
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
                Close();
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
                _currentScreen = MonitorHelper.NextScreen(_currentScreen);
                MonitorHelper.CoverScreen(this, _currentScreen);
                RefreshOverlay();
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

    private void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        if (_fullscreen)
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            MonitorHelper.CoverScreen(this, _currentScreen);
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
            Width = 1280;
            Height = 800;
            Left = _currentScreen.WorkingArea.Left + 80;
            Top = _currentScreen.WorkingArea.Top + 80;
        }
        RefreshOverlay();
    }

    private void RefreshOverlay()
    {
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
        lines.Add("<- / -> pattern   1-0 jump   I overlay   M monitor   F fullscreen   Esc quit");
        lines.Add("+/- adjust   [ / ] adjust rows (tile map)");

        PatternDisplay.OverlayText = string.Join("\n", lines);
        PatternDisplay.InvalidateVisual();
    }

    private double VisualTreeHelperDpi()
    {
        var source = PresentationSource.FromVisual(this);
        return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
    }
}
