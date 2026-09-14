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
    public PatternCanvas Canvas => PatternDisplay;

    public OutputWindow()
    {
        InitializeComponent();
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
