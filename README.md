# AT ScreenTest

A lightweight Windows desktop app for testing 4K (and other) displays — plus an LED
video-wall tile map for verifying panel numbering, coordinates and orientation.

Built with WPF on .NET 8, per-monitor DPI aware for pixel-accurate patterns on scaled
4K displays.

## Test patterns

- Solid Black / White / Red / Green / Blue — dead/stuck pixel & sub-pixel checks
- 50% Gray — backlight bleed & uniformity
- Color Bars — color/saturation reference
- Grayscale Gradient — banding check
- RGB Gradient — per-channel banding check
- Pixel Checkerboard — pixel walk / artifacts (adjustable cell size)
- Crosshatch & Geometry — alignment, convergence, aspect-ratio distortion
- Sharpness & Text — focus/clarity check at multiple sizes
- Motion Line — response time / ghosting (adjustable speed)
- Overscan Border — physical edge vs. 5% safe-area
- **LED Tile Map** — configurable rows x columns grid; each tile is labeled with its
  row/column index, pixel size and pixel coordinate, colored distinctly per column,
  and marked with a corner triangle so a rotated or mis-mapped panel is obvious at a
  glance. Handy for commissioning/checking LED video walls built from multiple panels.

## Controls

| Key | Action |
|---|---|
| `←` / `→` / `Space` | Previous / next pattern |
| `1`–`9`, `0` | Jump directly to pattern 1–10 |
| `I` | Toggle info overlay |
| `M` | Move to next monitor |
| `F` / `F11` | Toggle fullscreen / windowed |
| `+` / `-` | Adjust current pattern (checker cell size, motion speed, tile columns) |
| `[` / `]` | Adjust LED tile map rows |
| `Esc` | Quit |

## Building

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows).

```powershell
dotnet build ScreenTest.sln
dotnet run --project src/ScreenTest
```

## Publishing a standalone .exe

```powershell
dotnet publish src/ScreenTest -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

The resulting `publish\ScreenTest.exe` runs on any Windows 10/11 x64 machine without
requiring .NET to be installed separately.

Prebuilt artifacts are also produced automatically by the GitHub Actions workflow on
every push — see the **Actions** tab.
