namespace ScreenTest;

public enum TestPattern
{
    SolidBlack,
    SolidWhite,
    SolidRed,
    SolidGreen,
    SolidBlue,
    Gray50,
    ColorBars,
    GrayscaleGradient,
    RgbGradient,
    Checkerboard,
    CrosshatchGeometry,
    SharpnessText,
    MotionLine,
    OverscanBorder,
    LedTileMap,
}

public static class TestPatternInfo
{
    public static readonly TestPattern[] Order =
    {
        TestPattern.SolidBlack,
        TestPattern.SolidWhite,
        TestPattern.SolidRed,
        TestPattern.SolidGreen,
        TestPattern.SolidBlue,
        TestPattern.Gray50,
        TestPattern.ColorBars,
        TestPattern.GrayscaleGradient,
        TestPattern.RgbGradient,
        TestPattern.Checkerboard,
        TestPattern.CrosshatchGeometry,
        TestPattern.SharpnessText,
        TestPattern.MotionLine,
        TestPattern.OverscanBorder,
        TestPattern.LedTileMap,
    };

    public static string Name(this TestPattern pattern) => pattern switch
    {
        TestPattern.SolidBlack => "Solid Black (pulsing) — dead/stuck pixel & light-leak check",
        TestPattern.SolidWhite => "Solid White (pulsing) — dead/stuck pixel & uniformity check",
        TestPattern.SolidRed => "Solid Red (pulsing) — sub-pixel check",
        TestPattern.SolidGreen => "Solid Green (pulsing) — sub-pixel check",
        TestPattern.SolidBlue => "Solid Blue (pulsing) — sub-pixel check",
        TestPattern.Gray50 => "50% Gray (pulsing) — backlight bleed & uniformity",
        TestPattern.ColorBars => "Color Bars (rotating spectrum) — color/saturation reference",
        TestPattern.GrayscaleGradient => "Grayscale Gradient (scrolling) — banding check",
        TestPattern.RgbGradient => "RGB Gradient (scrolling) — color banding check",
        TestPattern.Checkerboard => "Pixel Checkerboard (auto-inverting) — pixel walk / artifacts",
        TestPattern.CrosshatchGeometry => "Crosshatch & Geometry — alignment/convergence",
        TestPattern.SharpnessText => "Sharpness & Text — focus/clarity check",
        TestPattern.MotionLine => "Motion Line — response time / ghosting",
        TestPattern.OverscanBorder => "Overscan Border (marching ants) — edge/safe-area check",
        TestPattern.LedTileMap => "LED Tile Map (scanning) — panel numbering / coordinate / orientation check",
        _ => pattern.ToString(),
    };
}
