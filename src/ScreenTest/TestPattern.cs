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
        TestPattern.SolidBlack => "Solid Black — dead/stuck pixel & light-leak check",
        TestPattern.SolidWhite => "Solid White — dead/stuck pixel & uniformity check",
        TestPattern.SolidRed => "Solid Red — sub-pixel check",
        TestPattern.SolidGreen => "Solid Green — sub-pixel check",
        TestPattern.SolidBlue => "Solid Blue — sub-pixel check",
        TestPattern.Gray50 => "50% Gray — backlight bleed & uniformity",
        TestPattern.ColorBars => "Color Bars — color/saturation reference",
        TestPattern.GrayscaleGradient => "Grayscale Gradient — banding check",
        TestPattern.RgbGradient => "RGB Gradient — color banding check",
        TestPattern.Checkerboard => "Pixel Checkerboard — pixel walk / artifacts",
        TestPattern.CrosshatchGeometry => "Crosshatch & Geometry — alignment/convergence",
        TestPattern.SharpnessText => "Sharpness & Text — focus/clarity check",
        TestPattern.MotionLine => "Motion Line — response time / ghosting",
        TestPattern.OverscanBorder => "Overscan Border — edge/safe-area check",
        TestPattern.LedTileMap => "LED Tile Map — panel numbering / coordinate / orientation check",
        _ => pattern.ToString(),
    };
}
