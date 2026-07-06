using SpecializedDisplay;
using SpecializedDisplay.Touch;
// Both referenced libraries define a LogLevel enum, so the touch sink's type is fully qualified below
// (SpecializedDisplay.Touch.LogLevel) to avoid the ambiguity; DisplayTransform comes from core.

namespace SpecializedDisplay.Y70ti;

/// <summary>
/// The HYTE Y70ti touch profile: the ILITEK digitizer's identity, its measured active-glass
/// calibration, and the factories that wire <see cref="SpecializedDisplay.Touch"/> to this panel. The
/// calibration's display reconciliation is driven by the SAME core <see cref="DisplayTransform"/> the
/// display session uses, so touch and image can never drift (see docs/device-notes.md).
/// </summary>
public static class Y70tiTouch
{
    /// <summary>ILITEK digitizer USB identity (device path carries <c>vid_222a&amp;pid_0001</c>).</summary>
    public const ushort Vid = 0x222A;
    public const ushort Pid = 0x0001;

    /// <summary>
    /// Measured active-glass bounds and axis orientation from the on-panel corner-holds
    /// (docs/device-notes.md): physical horizontal = raw Y (245→9499), physical vertical = raw X
    /// INVERTED (top≈9464 → bottom≈253).
    /// </summary>
    public static TouchCalibrationModel CalibrationModel { get; } = new()
    {
        RawXMin = 253,
        RawXMax = 9464,
        RawYMin = 245,
        RawYMax = 9499,
        HorizontalFromRawY = true,
        InvertHorizontal = false,
        InvertVertical = true,
    };

    /// <summary>
    /// Build a calibration for the given display orientation. The corner calibration was captured at the
    /// locked rot=90 baseline, so the baseline transform is fixed at <c>Compute(90,…)</c>; the display
    /// transform is <c>Compute(rotation, flipX, flipY, …)</c>. The two are produced by the SAME core
    /// function the display session uses, so <c>delta = baseline * inverse(display)</c> is structurally
    /// synced (identity at the default rot=90).
    /// </summary>
    public static TouchCalibration CreateCalibration(int rotation = Y70tiDisplay.DefaultRotation,
        bool flipX = false, bool flipY = false)
    {
        var baseline = DisplayTransform.Compute(Y70tiDisplay.DefaultRotation, false, false,
            Y70tiDisplay.PhysicalWidth, Y70tiDisplay.PhysicalHeight);
        var display = DisplayTransform.Compute(rotation, flipX, flipY,
            Y70tiDisplay.PhysicalWidth, Y70tiDisplay.PhysicalHeight);
        // Canvas is the baseline (rot 90) logical size — 682×2560 — matching the corner captures.
        var (canvasW, canvasH) = DisplayTransform.LogicalSize(Y70tiDisplay.DefaultRotation,
            Y70tiDisplay.PhysicalWidth, Y70tiDisplay.PhysicalHeight);
        return new TouchCalibration(CalibrationModel, canvasW, canvasH, baseline, display);
    }

    /// <summary>Create a raw touch source bound to the ILITEK digitizer (VID/PID filter + the always-on
    /// digitizer-TLC caps check) using the given calibration.</summary>
    public static RawTouchSource CreateTouchSource(TouchCalibration cal,
        Action<SpecializedDisplay.Touch.LogLevel, string>? log = null)
        => new(TouchDeviceFilter.ByVidPid(Vid, Pid), cal, log);
}
