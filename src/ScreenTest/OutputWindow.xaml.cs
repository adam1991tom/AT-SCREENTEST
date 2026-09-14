using System.Windows;
using WinFormsScreen = System.Windows.Forms.Screen;

namespace ScreenTest;

/// <summary>
/// The borderless window that actually displays the test pattern. Owned and driven by
/// a <see cref="ControlWindow"/>, which is normally sitting on a different monitor so
/// the person testing a screen doesn't need keyboard/mouse access to it directly.
/// </summary>
public partial class OutputWindow : Window
{
    /// <summary>Set by ControlWindow right before it intentionally shuts this window down.</summary>
    public bool AllowClose { get; set; }

    public PatternCanvas Canvas => PatternDisplay;

    public OutputWindow()
    {
        InitializeComponent();
        Closing += OnClosingAttempt;
    }

    private void OnClosingAttempt(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // This window has no title bar, so there's no visible way to close it - but
        // Alt+F4 still closes whatever window has focus regardless of its chrome. Once a
        // WPF Window is closed it can never be shown again, which would leave the Control
        // window unable to redisplay output. Hide instead, unless we're actually quitting.
        if (AllowClose) return;
        e.Cancel = true;
        Hide();
    }

    public void ShowOnScreen(WinFormsScreen screen)
    {
        if (!IsVisible) Show();
        MonitorHelper.CoverScreen(this, screen);
    }

    public double GetDpiScale()
    {
        var source = PresentationSource.FromVisual(this);
        return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
    }
}
