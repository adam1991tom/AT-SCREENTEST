using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;
using Pen = System.Windows.Media.Pen;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;

namespace ScreenTest;

/// <summary>
/// Renders the currently selected test pattern directly via DrawingContext for full
/// control over pixel-level accuracy (important for dead-pixel / checkerboard / geometry
/// patterns on high-DPI 4K displays). Most patterns animate continuously off a shared
/// elapsed-time clock; geometry and sharpness patterns stay static since they need to
/// hold still to be measured against.
/// </summary>
public class PatternCanvas : FrameworkElement
{
    private static readonly int[] CheckerSizes = { 1, 2, 4, 8, 16, 32, 64 };
    private static readonly double[] MotionSpeeds = { 120, 240, 480, 960, 1920 }; // DIPs/sec

    private const double TargetFrameSeconds = 1.0 / 30.0; // cap animation redraws so no pattern can flood the UI thread

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _lastInvalidateSeconds = -1;
    private int _checkerSizeIndex = 1;
    private int _motionSpeedIndex = 2;
    private int _tileCols = 8;
    private int _tileRows = 4;

    public TestPattern Pattern { get; private set; } = TestPattern.SolidBlack;
    public bool ShowOverlay { get; set; } = true;
    public string OverlayText { get; set; } = string.Empty;

    public PatternCanvas()
    {
        CompositionTarget.Rendering += (_, _) =>
        {
            var now = Elapsed;
            if (now - _lastInvalidateSeconds < TargetFrameSeconds) return;
            _lastInvalidateSeconds = now;
            InvalidateVisual();
        };
    }

    public void SetPattern(TestPattern pattern)
    {
        Pattern = pattern;
        InvalidateVisual();
    }

    public void AdjustCheckerSize(int direction)
    {
        _checkerSizeIndex = Math.Clamp(_checkerSizeIndex + direction, 0, CheckerSizes.Length - 1);
    }

    public void AdjustMotionSpeed(int direction)
    {
        _motionSpeedIndex = Math.Clamp(_motionSpeedIndex + direction, 0, MotionSpeeds.Length - 1);
    }

    public void AdjustTileColumns(int direction)
    {
        _tileCols = Math.Clamp(_tileCols + direction, 1, 32);
    }

    public void AdjustTileRows(int direction)
    {
        _tileRows = Math.Clamp(_tileRows + direction, 1, 32);
    }

    public string AdjustmentSummary() => Pattern switch
    {
        TestPattern.Checkerboard => $"Cell size: {CheckerSizes[_checkerSizeIndex]}px  (+/- to change)",
        TestPattern.MotionLine => $"Speed: {MotionSpeeds[_motionSpeedIndex]:0} px/s  (+/- to change)",
        TestPattern.LedTileMap => $"Grid: {_tileCols} x {_tileRows} tiles  (+/- columns, [ / ] rows)",
        _ => string.Empty,
    };

    private double Elapsed => _clock.Elapsed.TotalSeconds;

    private double DpiScale
    {
        get
        {
            var source = PresentationSource.FromVisual(this);
            return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        }
    }

    /// <summary>Size, in DIPs, of one physical device pixel.</summary>
    private double DevicePixel => 1.0 / DpiScale;

    protected override void OnRender(DrawingContext dc)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var rect = new Rect(0, 0, w, h);

        switch (Pattern)
        {
            case TestPattern.SolidBlack: DrawPulsingSolid(dc, rect, Colors.Black); break;
            case TestPattern.SolidWhite: DrawPulsingSolid(dc, rect, Colors.White); break;
            case TestPattern.SolidRed: DrawPulsingSolid(dc, rect, Color.FromRgb(255, 0, 0)); break;
            case TestPattern.SolidGreen: DrawPulsingSolid(dc, rect, Color.FromRgb(0, 255, 0)); break;
            case TestPattern.SolidBlue: DrawPulsingSolid(dc, rect, Color.FromRgb(0, 0, 255)); break;
            case TestPattern.Gray50: DrawPulsingSolid(dc, rect, Color.FromRgb(128, 128, 128)); break;
            case TestPattern.ColorBars: DrawColorBars(dc, rect); break;
            case TestPattern.GrayscaleGradient: DrawGrayscaleGradient(dc, rect); break;
            case TestPattern.RgbGradient: DrawRgbGradient(dc, rect); break;
            case TestPattern.Checkerboard: DrawCheckerboard(dc, rect); break;
            case TestPattern.CrosshatchGeometry: DrawCrosshatch(dc, rect); break;
            case TestPattern.SharpnessText: DrawSharpnessText(dc, rect); break;
            case TestPattern.MotionLine: DrawMotionLine(dc, rect); break;
            case TestPattern.OverscanBorder: DrawOverscanBorder(dc, rect); break;
            case TestPattern.LedTileMap: DrawLedTileMap(dc, rect); break;
        }

        if (ShowOverlay && !string.IsNullOrEmpty(OverlayText))
        {
            DrawOverlay(dc, rect);
        }
    }

    /// <summary>Modulates a solid color's channels with a slow sine "breathing" pulse.</summary>
    private static Color PulseColor(Color baseColor, double amplitude, double elapsed, double period)
    {
        var phase = Math.Sin(2 * Math.PI * elapsed / period);
        byte Pulse(byte channel) => (byte)Math.Clamp(channel + amplitude * phase, 0, 255);
        return Color.FromRgb(Pulse(baseColor.R), Pulse(baseColor.G), Pulse(baseColor.B));
    }

    private void DrawPulsingSolid(DrawingContext dc, Rect rect, Color baseColor)
    {
        var color = PulseColor(baseColor, 45, Elapsed, 2.4);
        dc.DrawRectangle(new SolidColorBrush(color), null, rect);
    }

    private void DrawColorBars(DrawingContext dc, Rect rect)
    {
        dc.DrawRectangle(Brushes.Black, null, rect);

        const int barCount = 8;
        var rotation = (Elapsed * 20) % 360; // slowly rotating spectrum, 18s per full cycle
        var barWidth = rect.Width / barCount;

        for (var i = 0; i < barCount; i++)
        {
            var hue = (i * (360.0 / barCount) + rotation) % 360;
            var barRect = new Rect(rect.X + i * barWidth, rect.Y, barWidth + 1, rect.Height);
            dc.DrawRectangle(new SolidColorBrush(HsvToColor(hue, 1.0, 1.0)), null, barRect);
        }
    }

    private static LinearGradientBrush ScrollingGradient(Color from, Color to, double phase)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(phase, 0),
            EndPoint = new Point(phase + 1, 0),
            SpreadMethod = GradientSpreadMethod.Repeat,
        };
        brush.GradientStops.Add(new GradientStop(from, 0));
        brush.GradientStops.Add(new GradientStop(to, 1));
        return brush;
    }

    private void DrawGrayscaleGradient(DrawingContext dc, Rect rect)
    {
        var phase = (Elapsed * 0.15) % 1.0;
        dc.DrawRectangle(ScrollingGradient(Colors.Black, Colors.White, phase), null, rect);
    }

    private void DrawRgbGradient(DrawingContext dc, Rect rect)
    {
        var phase = (Elapsed * 0.15) % 1.0;
        var third = rect.Height / 3.0;
        var redRect = new Rect(rect.X, rect.Y, rect.Width, third);
        var greenRect = new Rect(rect.X, rect.Y + third, rect.Width, third);
        var blueRect = new Rect(rect.X, rect.Y + 2 * third, rect.Width, rect.Height - 2 * third);

        dc.DrawRectangle(ScrollingGradient(Colors.Black, Colors.Red, phase), null, redRect);
        dc.DrawRectangle(ScrollingGradient(Colors.Black, Colors.Lime, phase), null, greenRect);
        dc.DrawRectangle(ScrollingGradient(Colors.Black, Colors.Blue, phase), null, blueRect);
    }

    private void DrawCheckerboard(DrawingContext dc, Rect rect)
    {
        // Rendered as a single GPU-tiled 2x2 brush rather than one shape per cell: at a 1px
        // cell size on a 4K screen that's millions of cells, and this pattern now redraws
        // every frame for the auto-invert animation, so a per-cell geometry would have to
        // rebuild millions of shapes 30 times a second and lock up the UI thread.
        var cellPixels = CheckerSizes[_checkerSizeIndex];
        var cellSize = cellPixels * DevicePixel;
        var invert = (long)(Elapsed / 1.0) % 2 == 1; // auto-inverts every second: classic pixel-refresh/burn-in pattern

        var tile = new DrawingGroup();
        using (var ctx = tile.Open())
        {
            var (bg, fg) = invert ? (Brushes.White, Brushes.Black) : ((Brush)Brushes.Black, (Brush)Brushes.White);
            ctx.DrawRectangle(bg, null, new Rect(0, 0, 2, 2));
            ctx.DrawRectangle(fg, null, new Rect(1, 0, 1, 1));
            ctx.DrawRectangle(fg, null, new Rect(0, 1, 1, 1));
        }
        tile.Freeze();

        var brush = new DrawingBrush(tile)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, cellSize * 2, cellSize * 2),
            ViewportUnits = BrushMappingMode.Absolute,
        };
        brush.Freeze();

        dc.DrawRectangle(brush, null, rect);
    }

    private void DrawCrosshatch(DrawingContext dc, Rect rect)
    {
        dc.DrawRectangle(Brushes.Black, null, rect);

        var pen = new Pen(Brushes.White, DevicePixel);
        pen.Freeze();
        var thickPen = new Pen(Brushes.Lime, DevicePixel * 2);
        thickPen.Freeze();

        const int divisions = 12;
        var stepX = rect.Width / divisions;
        var stepY = rect.Height / divisions;

        for (var i = 1; i < divisions; i++)
        {
            dc.DrawLine(pen, new Point(i * stepX, 0), new Point(i * stepX, rect.Height));
            dc.DrawLine(pen, new Point(0, i * stepY), new Point(rect.Width, i * stepY));
        }

        // Border frame
        dc.DrawRectangle(null, thickPen, new Rect(1, 1, rect.Width - 2, rect.Height - 2));

        // Center crosshair
        var cx = rect.Width / 2;
        var cy = rect.Height / 2;
        dc.DrawLine(thickPen, new Point(cx, 0), new Point(cx, rect.Height));
        dc.DrawLine(thickPen, new Point(0, cy), new Point(rect.Width, cy));

        // Inscribed circle for geometry/aspect-ratio distortion check
        var radius = Math.Min(rect.Width, rect.Height) / 2 - 4;
        dc.DrawEllipse(null, thickPen, new Point(cx, cy), radius, radius);

        // Corner markers
        var markerLen = Math.Min(rect.Width, rect.Height) * 0.04;
        DrawCornerMarker(dc, thickPen, new Point(0, 0), markerLen, 1, 1);
        DrawCornerMarker(dc, thickPen, new Point(rect.Width, 0), markerLen, -1, 1);
        DrawCornerMarker(dc, thickPen, new Point(0, rect.Height), markerLen, 1, -1);
        DrawCornerMarker(dc, thickPen, new Point(rect.Width, rect.Height), markerLen, -1, -1);
    }

    private static void DrawCornerMarker(DrawingContext dc, Pen pen, Point corner, double len, int dx, int dy)
    {
        dc.DrawLine(pen, corner, new Point(corner.X + len * dx, corner.Y));
        dc.DrawLine(pen, corner, new Point(corner.X, corner.Y + len * dy));
    }

    private void DrawSharpnessText(DrawingContext dc, Rect rect)
    {
        dc.DrawRectangle(Brushes.Black, null, rect);

        var typeface = new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        double[] sizes = { 48, 32, 24, 18, 14, 11, 9, 7 };
        var y = rect.Height * 0.06;

        foreach (var size in sizes)
        {
            var text = new FormattedText(
                $"{size:0}pt  AT ScreenTest — Sharpness Check 0123456789",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                size,
                Brushes.White,
                DpiScale);
            dc.DrawText(text, new Point(rect.Width * 0.04, y));
            y += size * 1.6;
        }

        // Fine hairline grid + diagonal lines in a corner box to reveal moire/scaling artifacts
        var boxSize = Math.Min(rect.Width, rect.Height) * 0.32;
        var box = new Rect(rect.Width - boxSize - 24, rect.Height - boxSize - 24, boxSize, boxSize);
        var pen = new Pen(Brushes.White, DevicePixel);
        pen.Freeze();
        dc.DrawRectangle(Brushes.Black, pen, box);

        var lineSpacing = DevicePixel * 2;
        for (var x = box.X; x < box.X + box.Width; x += lineSpacing)
        {
            dc.DrawLine(pen, new Point(x, box.Y), new Point(x, box.Y + box.Height));
        }
        dc.DrawLine(pen, box.TopLeft, box.BottomRight);
        dc.DrawLine(pen, box.TopRight, box.BottomLeft);
    }

    private void DrawMotionLine(DrawingContext dc, Rect rect)
    {
        dc.DrawRectangle(Brushes.Black, null, rect);
        var lineWidth = Math.Max(DevicePixel * 6, 4);
        var pen = new Pen(Brushes.White, lineWidth);
        var x = (Elapsed * MotionSpeeds[_motionSpeedIndex]) % Math.Max(rect.Width, 1);
        dc.DrawLine(pen, new Point(x, 0), new Point(x, rect.Height));
    }

    private void DrawOverscanBorder(DrawingContext dc, Rect rect)
    {
        dc.DrawRectangle(Brushes.Black, null, rect);

        var dashOffset = -(Elapsed * 8) % 8; // "marching ants" crawling along both borders

        var edgePen = new Pen(Brushes.Red, DevicePixel) { DashStyle = new DashStyle(new double[] { 4, 4 }, dashOffset) };
        dc.DrawRectangle(null, edgePen, new Rect(0.5, 0.5, rect.Width - 1, rect.Height - 1));

        // 5% inset "safe area" commonly used to check for TV overscan
        var insetPen = new Pen(Brushes.Lime, DevicePixel) { DashStyle = new DashStyle(new double[] { 4, 4 }, dashOffset) };
        var insetX = rect.Width * 0.05;
        var insetY = rect.Height * 0.05;
        dc.DrawRectangle(null, insetPen,
            new Rect(insetX, insetY, rect.Width - 2 * insetX, rect.Height - 2 * insetY));

        var typeface = new Typeface("Consolas");
        var label = new FormattedText(
            "RED = physical screen edge   GREEN = 5% overscan safe area",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Windows.FlowDirection.LeftToRight,
            typeface,
            16,
            Brushes.White,
            DpiScale);
        dc.DrawText(label, new Point(insetX + 12, insetY + 12));
    }

    private void DrawLedTileMap(DrawingContext dc, Rect rect)
    {
        dc.DrawRectangle(Brushes.Black, null, rect);

        var cellWidth = rect.Width / _tileCols;
        var cellHeight = rect.Height / _tileRows;
        var pixelCellWidth = (int)Math.Round(cellWidth * DpiScale);
        var pixelCellHeight = (int)Math.Round(cellHeight * DpiScale);

        var gridPen = new Pen(Brushes.Black, DevicePixel * 2);
        gridPen.Freeze();
        var highlightPen = new Pen(Brushes.Yellow, Math.Max(DevicePixel * 4, 3));
        highlightPen.Freeze();

        var fontSize = Math.Clamp(Math.Min(cellWidth, cellHeight) * 0.16, 7, 22);
        var typeface = new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var markerSize = Math.Min(cellWidth, cellHeight) * 0.18;

        // Scanner: a highlight sweeps tile-by-tile in row-major order, so an operator can
        // watch the physical wall and confirm panels light up in the expected sequence.
        var tileCount = _tileCols * _tileRows;
        var activeIndex = tileCount > 0 ? (int)(Elapsed / 0.15) % tileCount : -1;

        for (var row = 0; row < _tileRows; row++)
        {
            for (var col = 0; col < _tileCols; col++)
            {
                var cellRect = new Rect(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                var fill = new SolidColorBrush(TileColor(row, col));
                dc.DrawRectangle(fill, gridPen, cellRect);

                // Orientation marker: filled triangle in the top-left corner of every tile.
                // If a panel is physically rotated/flipped, its marker will no longer sit
                // top-left relative to its neighbors, making the fault obvious at a glance.
                var triangle = new StreamGeometry();
                using (var ctx = triangle.Open())
                {
                    ctx.BeginFigure(cellRect.TopLeft, true, true);
                    ctx.LineTo(new Point(cellRect.X + markerSize, cellRect.Y), false, false);
                    ctx.LineTo(new Point(cellRect.X, cellRect.Y + markerSize), false, false);
                }
                triangle.Freeze();
                dc.DrawGeometry(Brushes.White, null, triangle);

                var label = new FormattedText(
                    $"R{row} C{col}\n{pixelCellWidth}x{pixelCellHeight}\n@{(int)Math.Round(col * cellWidth * DpiScale)},{(int)Math.Round(row * cellHeight * DpiScale)}",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    Brushes.White,
                    DpiScale);

                var textOrigin = new Point(
                    cellRect.X + (cellRect.Width - label.Width) / 2,
                    cellRect.Y + (cellRect.Height - label.Height) / 2);
                dc.DrawText(label, textOrigin);

                if (row * _tileCols + col == activeIndex)
                {
                    dc.DrawRectangle(null, highlightPen, cellRect);
                }
            }
        }
    }

    private static Color TileColor(int row, int col)
    {
        var hue = (col / 1.0 % 8) * 45.0;
        var lightness = (row % 2 == 0) ? 0.42 : 0.30;
        return HsvToColor(hue, 0.65, lightness);
    }

    private static Color HsvToColor(double hue, double saturation, double value)
    {
        var c = value * saturation;
        var x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
        var m = value - c;
        var (r, g, b) = hue switch
        {
            < 60 => (c, x, 0.0),
            < 120 => (x, c, 0.0),
            < 180 => (0.0, c, x),
            < 240 => (0.0, x, c),
            < 300 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };
        return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }

    private void DrawOverlay(DrawingContext dc, Rect rect)
    {
        var typeface = new Typeface("Consolas");
        var text = new FormattedText(
            OverlayText,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Windows.FlowDirection.LeftToRight,
            typeface,
            16,
            Brushes.Lime,
            DpiScale);

        var padding = 10;
        var bgRect = new Rect(10, 10, text.Width + padding * 2, text.Height + padding * 2);
        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), null, bgRect);
        dc.DrawText(text, new Point(10 + padding, 10 + padding));
    }
}
