using Windows.Graphics.DirectX;
using SpecializedDisplay;

namespace SpecializedDisplay.Y70ti;

/// <summary>
/// The HYTE Y70ti display profile: the constants, target selector, native-mode filter, and acquire
/// options that pin <see cref="SpecializedDisplay"/> to this specific panel. Every value here is a
/// hardware finding (see docs/device-notes.md); the core library stays device-agnostic.
/// </summary>
public static class Y70tiDisplay
{
    /// <summary>The panel's landscape scanout framebuffer (the logical portrait canvas is the swap of
    /// these — 682×2560 — at the locked 90°/270° rotation).</summary>
    public const int PhysicalWidth = 2560, PhysicalHeight = 682;

    /// <summary>Native vertical refresh (the 1:1 mode reports 60.001 Hz; the pacing target is 60).</summary>
    public const double NativeRefreshHz = 60.0;

    /// <summary>The on-panel-locked rotation: <c>--rot 90</c>, no flip, reads upright (confirmed 2026-07-03).</summary>
    public const int DefaultRotation = 90;

    /// <summary>
    /// Two-phase target selector, mirroring the renderer's <c>FindY70tiKey</c>/<c>FindTarget</c> exactly:
    /// phase 1 correlates a monitor whose <c>DeviceId</c> contains <c>RTK409A</c> OR whose
    /// <c>DisplayName</c> contains <c>HYTE</c> or <c>Y70</c>; phase 2 falls back to a target whose
    /// <c>StableMonitorId</c> contains <c>RTK409A</c>. This never selects the BenQ XL2586X+ that shares
    /// the machine.
    /// </summary>
    public static DisplaySelector Selector { get; } = new()
    {
        MatchMonitor = m =>
            m.DeviceId.Contains("RTK409A", StringComparison.OrdinalIgnoreCase) ||
            m.DisplayName.Contains("HYTE", StringComparison.OrdinalIgnoreCase) ||
            m.DisplayName.Contains("Y70", StringComparison.OrdinalIgnoreCase),
        MatchTarget = t =>
            t.StableMonitorId is { } id && id.Contains("RTK409A", StringComparison.OrdinalIgnoreCase),
    };

    /// <summary>1:1 <c>2560×682</c> <c>B8G8R8A8UIntNormalized</c> — rejects the RTK409A's upscaled
    /// (src&lt;tgt) modes, which must never be used (docs/device-notes.md).</summary>
    public static Func<DisplayModeDescriptor, bool> NativeMode { get; } =
        ModeSelectors.NativeOneToOne(PhysicalWidth, PhysicalHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);

    /// <summary>Acquire options for the panel: the native-mode filter and the given orientation, with
    /// every resilience knob (throttle / dead-fence revive / bounded reacquire / fence timeouts) left at
    /// the core defaults, which are the renderer's hardware-tuned values.</summary>
    public static AcquireOptions CreateOptions(int rotation = DefaultRotation,
        bool flipX = false, bool flipY = false, Action<LogLevel, string>? log = null)
        => new()
        {
            ModeSelector = NativeMode,
            Rotation = rotation,
            FlipX = flipX,
            FlipY = flipY,
            RefreshHz = NativeRefreshHz,
            Log = log,
        };

    /// <summary>Locate the Y70ti now, or null if it is not currently connected.</summary>
    public static SpecializedDisplayTarget? Find() => SpecializedDisplays.Find(Selector);

    /// <summary>Locate the Y70ti now, throwing <c>TargetNotFoundException</c> if absent.</summary>
    public static SpecializedDisplayTarget Require() => SpecializedDisplays.Require(Selector);
}
