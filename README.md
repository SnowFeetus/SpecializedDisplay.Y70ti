# SpecializedDisplay.Y70ti

The **HYTE Y70ti device profile** for [`SpecializedDisplay.NET`](../SpecializedDisplay.NET) (exclusive
display acquire + present) and [`SpecializedDisplay.Touch`](../SpecializedDisplay.Touch) (Raw Input
touch). It supplies the panel-specific constants, the target selector, the native-mode filter, the
acquire options, and the touch calibration — so an application can drive this specific panel without
hardcoding any hardware findings itself.

Everything encoded here is documented in [docs/device-notes.md](docs/device-notes.md).

## Public API

Namespace `SpecializedDisplay.Y70ti`:

```csharp
public static class Y70tiDisplay
{
    public const int    PhysicalWidth = 2560, PhysicalHeight = 682;
    public const double NativeRefreshHz = 60.0;
    public const int    DefaultRotation = 90;   // locked on-panel 2026-07-03

    public static DisplaySelector Selector { get; }                    // RTK409A / HYTE / Y70; never the BenQ
    public static Func<DisplayModeDescriptor, bool> NativeMode { get; } // 1:1 2560x682 B8G8R8A8UIntNormalized

    public static AcquireOptions CreateOptions(int rotation = DefaultRotation,
        bool flipX = false, bool flipY = false, Action<LogLevel, string>? log = null);

    public static SpecializedDisplayTarget? Find();
    public static SpecializedDisplayTarget  Require();
}

public static class Y70tiTouch
{
    public const ushort Vid = 0x222A, Pid = 0x0001;                    // ILITEK digitizer
    public static TouchCalibrationModel CalibrationModel { get; }      // measured active-glass bounds

    public static TouchCalibration CreateCalibration(int rotation = 90, bool flipX = false, bool flipY = false);
    public static RawTouchSource   CreateTouchSource(TouchCalibration cal, Action<LogLevel, string>? log = null);
}
```

`CreateCalibration` computes both the rot-90 baseline and the current display transform with the SAME
core `DisplayTransform` the display session uses, so touch and image stay reconciled under any
rotation/flip (the delta is identity at the default rot=90).

### Typical use

```csharp
var target  = Y70tiDisplay.Require();
var session = target.CreateSession(Y70tiDisplay.CreateOptions(log: myLog));
// subscribe to session events here, then:
session.Acquire();

var cal   = Y70tiTouch.CreateCalibration();               // matches the display's default rotation
using var touch = Y70tiTouch.CreateTouchSource(cal, myLog);
touch.Start();
```

## Supported platforms

**Windows x64 only**, target framework `net10.0-windows10.0.22621.0`. The profile builds as AnyCPU and
carries no package references of its own; the consuming application is responsible for
`PlatformTarget=x64` (the touch struct layouts are hand-verified for x64).

## Clone layout

This profile consumes its two dependencies by `ProjectReference` to relative sibling paths, so the
related repos must be cloned **as siblings under one parent directory**:

```
<parent>\
  SpecializedDisplay.NET\      (core display library)
  SpecializedDisplay.Touch\    (Raw Input touch library)
  SpecializedDisplay.Y70ti\    (this repo)
  Y70ti_Exclusive_HWMonitor\   (the application)
```

A later switch to nuget.org packages is planned (tagged at extraction time); until then, keep the
sibling layout.

## Build & test

```
dotnet build -c Release
dotnet test
```

Tests are hardware-free: they cover the selector (accepts RTK409A / HYTE / Y70, rejects the BenQ
XL2586X+, matches the RTK409A `StableMonitorId`), the native-mode filter (accepts 1:1, rejects scaled /
wrong-format), the acquire-options wiring, and the calibration corners through `CreateCalibration()`.

## License

MIT — see [LICENSE](LICENSE).
