# AT ScreenTest

A lightweight Windows desktop app for testing 4K (and other) displays — plus an LED
video-wall tile map for verifying panel numbering, coordinates and orientation.

Built with WPF on .NET 8, per-monitor DPI aware for pixel-accurate patterns on scaled
4K displays.

## Two windows: Control + Output

Launching the app opens two windows:

- **Control** — a normal small window with a monitor picker, a clickable list of test
  patterns, and adjustment buttons. Keep this on your own screen (e.g. your laptop).
- **Output** — a borderless fullscreen window that shows the actual test pattern.
  Point it at whichever monitor you're testing, using the "Output monitor" dropdown
  in the Control window.

This means you never need keyboard/mouse access to the screen under test — everything
is driven from the Control window, which can sit on a completely different monitor.

## Test patterns

Every pattern animates continuously except Crosshatch/Geometry and Sharpness/Text,
which hold still on purpose since they're meant to be measured against.

- Solid Black / White / Red / Green / Blue, 50% Gray — slow brightness "breathing"
  pulse; dead/stuck pixel, sub-pixel & backlight bleed checks
- Color Bars — rotating hue spectrum; color/saturation reference
- Grayscale Gradient / RGB Gradient — continuously scrolling; banding check
- Pixel Checkerboard — auto-inverts every second (classic pixel-refresh/burn-in
  pattern); pixel walk / artifacts (adjustable cell size)
- Crosshatch & Geometry (static) — alignment, convergence, aspect-ratio distortion
- Sharpness & Text (static) — focus/clarity check at multiple sizes
- Motion Line — response time / ghosting (adjustable speed)
- Overscan Border — marching-ants dashed edges; physical edge vs. 5% safe-area
- **LED Tile Map** — configurable rows x columns grid; each tile is labeled with its
  row/column index, pixel size and pixel coordinate, colored distinctly per column,
  and marked with a corner triangle so a rotated or mis-mapped panel is obvious at a
  glance. A highlight scans tile-by-tile in sequence so you can confirm panels light
  up in the expected order on an LED wall.

## Controls

Everything is mouse-driven from the Control window:

- **Output monitor** dropdown — moves the Output window to cover that monitor
- **Test pattern** list — click a pattern to show it on the Output window
- **−** / **+** buttons — adjust the current pattern's setting (checker cell size,
  motion speed, or LED tile map columns)
- **Rows −** / **Rows +** — adjust LED tile map row count
- **Show info overlay** checkbox — toggles the resolution/pattern info box on the
  Output window
- Closing the Control window also closes the Output window

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

## Building an installer (setup.exe)

Requires [Inno Setup 6](https://jrsoftware.org/isinfo.php). After publishing (above):

```powershell
& "ISCC.exe" installer\AT-ScreenTest.iss
```

This produces `dist\AT-ScreenTest-Setup.exe` — a single installer that puts the app in
Program Files, adds a Start Menu entry and optional desktop shortcut, and registers an
uninstaller. Copy that one file to another Windows machine to install AT ScreenTest
there (no .NET install required — it's fully self-contained).
