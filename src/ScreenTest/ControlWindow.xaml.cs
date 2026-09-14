using System.Windows;
using System.Windows.Controls;
using WinFormsScreen = System.Windows.Forms.Screen;

namespace ScreenTest;

/// <summary>
/// The control panel: pick which monitor to output the test pattern to and which
/// pattern to show, all with the mouse. Meant to sit on a different monitor than the
/// one being tested, so you never need keyboard/mouse access to the screen under test.
/// </summary>
public partial class ControlWindow : Window
{
    private sealed record MonitorItem(WinFormsScreen Screen, string Label);
    private sealed record PatternItem(TestPattern Pattern, string Label);

    private readonly OutputWindow _output = new();

    public ControlWindow()
    {
        InitializeComponent();

        PatternList.ItemsSource = TestPatternInfo.Order
            .Select(p => new PatternItem(p, p.Name()))
            .ToList();

        RefreshMonitorList();

        Loaded += (_, _) =>
        {
            PatternList.SelectedIndex = 0;
            var monitors = (List<MonitorItem>)MonitorCombo.ItemsSource;
            MonitorCombo.SelectedItem = monitors.Count > 1 ? monitors[1] : monitors[0];
        };

        Closing += (_, _) =>
        {
            _output.AllowClose = true;
            _output.Close();
        };
    }

    private void RefreshMonitorList()
    {
        var items = WinFormsScreen.AllScreens.Select((s, i) =>
        {
            var mode = DisplayInfo.GetCurrentMode(s.DeviceName);
            var res = mode is { } m ? $"{m.Width}x{m.Height} @ {m.RefreshHz}Hz" : "unknown resolution";
            var primary = s.Primary ? " (Primary)" : "";
            return new MonitorItem(s, $"Monitor {i + 1}{primary} — {res}");
        }).ToList();

        MonitorCombo.ItemsSource = items;
    }

    private void OnMonitorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MonitorCombo.SelectedItem is MonitorItem item)
        {
            _output.ShowOnScreen(item.Screen);
            UpdateOutputOverlay();
        }
    }

    private void OnPatternChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PatternList.SelectedItem is PatternItem item)
        {
            _output.Canvas.SetPattern(item.Pattern);
            UpdateOutputOverlay();
        }
    }

    private void OnAdjustDown(object sender, RoutedEventArgs e) => Adjust(-1);
    private void OnAdjustUp(object sender, RoutedEventArgs e) => Adjust(1);

    private void OnRowsDown(object sender, RoutedEventArgs e)
    {
        _output.Canvas.AdjustTileRows(-1);
        UpdateOutputOverlay();
    }

    private void OnRowsUp(object sender, RoutedEventArgs e)
    {
        _output.Canvas.AdjustTileRows(1);
        UpdateOutputOverlay();
    }

    private void Adjust(int direction)
    {
        var pattern = (PatternList.SelectedItem as PatternItem)?.Pattern;
        if (pattern == TestPattern.Checkerboard) _output.Canvas.AdjustCheckerSize(direction);
        if (pattern == TestPattern.MotionLine) _output.Canvas.AdjustMotionSpeed(direction);
        if (pattern == TestPattern.LedTileMap) _output.Canvas.AdjustTileColumns(direction);
        UpdateOutputOverlay();
    }

    private void OnOverlayToggled(object sender, RoutedEventArgs e)
    {
        _output.Canvas.ShowOverlay = OverlayCheck.IsChecked == true;
    }

    private void UpdateOutputOverlay()
    {
        if (MonitorCombo.SelectedItem is not MonitorItem item) return;

        var mode = DisplayInfo.GetCurrentMode(item.Screen.DeviceName);
        var resLine = mode is { } m ? $"{m.Width} x {m.Height} @ {m.RefreshHz} Hz" : "resolution unavailable";
        var dpi = _output.GetDpiScale();
        var patternLabel = (PatternList.SelectedItem as PatternItem)?.Label ?? "";

        var lines = new List<string>
        {
            $"AT ScreenTest — {item.Label}",
            resLine,
            $"DPI scale: {dpi:0.00}x",
            patternLabel,
        };

        var adjustment = _output.Canvas.AdjustmentSummary();
        AdjustLabel.Text = string.IsNullOrEmpty(adjustment) ? "No adjustable settings for this pattern" : adjustment;
        if (!string.IsNullOrEmpty(adjustment)) lines.Add(adjustment);

        _output.Canvas.OverlayText = string.Join("\n", lines);
        StatusText.Text = $"Output: {item.Label}";
    }
}
