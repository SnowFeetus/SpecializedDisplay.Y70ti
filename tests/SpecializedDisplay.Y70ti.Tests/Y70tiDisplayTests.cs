using SpecializedDisplay;
using SpecializedDisplay.Y70ti;
using Windows.Graphics.DirectX;

namespace SpecializedDisplay.Y70ti.Tests;

public class Y70tiDisplayTests
{
    private static DisplayMonitorInfo Monitor(string deviceId, string displayName)
        => new(deviceId, displayName, 0, 0, 0, 0, 0);

    [Theory]
    [InlineData(@"\\?\DISPLAY#RTK409A#5&abc&0&UID256", "Generic PnP Monitor")] // DeviceId carries RTK409A
    [InlineData(@"\\?\DISPLAY#GENERIC#5&def&0&UID257", "HYTE Y70ti")]           // DisplayName HYTE (and Y70)
    [InlineData(@"\\?\DISPLAY#GENERIC#5&ghi&0&UID258", "Custom Y70 Panel")]     // DisplayName Y70 in isolation
    public void Selector_MatchMonitor_AcceptsY70ti(string deviceId, string displayName)
    {
        Assert.True(Y70tiDisplay.Selector.MatchMonitor!(Monitor(deviceId, displayName)));
    }

    [Fact]
    public void Selector_MatchMonitor_RejectsBenQ()
    {
        // The BenQ XL2586X+ shares the machine and must NEVER be selected.
        Assert.False(Y70tiDisplay.Selector.MatchMonitor!(Monitor(@"\\?\DISPLAY#BNQ8034#4&xyz&0&UID4357", "XL2586X+")));
    }

    [Theory]
    [InlineData("RTK409AW09C23CM024L_00000000", true)]  // the documented Y70ti StableMonitorId
    [InlineData("BNQ8034_1E0F2A3B", false)]             // the BenQ
    public void Selector_MatchTarget_MatchesRtk409aStableId(string stableId, bool expected)
    {
        Assert.Equal(expected, Y70tiDisplay.Selector.MatchTarget!(new DisplayTargetInfo(stableId, 0, 0, 0, true)));
    }

    [Fact]
    public void Selector_MatchTarget_HandlesNullStableId()
    {
        Assert.False(Y70tiDisplay.Selector.MatchTarget!(new DisplayTargetInfo(null, 0, 0, 0, true)));
    }

    [Fact]
    public void NativeMode_AcceptsNative1To1_RejectsScaledAndWrongFormat()
    {
        var native = new DisplayModeDescriptor(2560, 682, 2560, 682, DirectXPixelFormat.B8G8R8A8UIntNormalized, 60.001);
        Assert.True(Y70tiDisplay.NativeMode(native));

        // Scaled: src < tgt (the RTK409A upscaler, e.g. 1176x664 -> 2560x682) — must be rejected.
        var scaled = new DisplayModeDescriptor(1176, 664, 2560, 682, DirectXPixelFormat.B8G8R8A8UIntNormalized, 60.0);
        Assert.False(Y70tiDisplay.NativeMode(scaled));

        // Right geometry, wrong pixel format.
        var wrongFmt = new DisplayModeDescriptor(2560, 682, 2560, 682, DirectXPixelFormat.R8G8B8A8UIntNormalized, 60.0);
        Assert.False(Y70tiDisplay.NativeMode(wrongFmt));
    }

    [Fact]
    public void CreateOptions_WiresProfile_AndKeepsResilienceDefaults()
    {
        var opts = Y70tiDisplay.CreateOptions();
        Assert.Same(Y70tiDisplay.NativeMode, opts.ModeSelector);
        Assert.Equal(90, opts.Rotation);
        Assert.False(opts.FlipX);
        Assert.False(opts.FlipY);
        Assert.Equal(60.0, opts.RefreshHz);
        // Resilience knobs stay at the core (hardware-tuned) defaults.
        Assert.Equal(2, opts.BufferCount);
        Assert.Equal(5000, opts.FenceReviveIntervalMs);
        Assert.Equal(6, opts.ReacquireAttempts);
    }

    [Fact]
    public void CreateOptions_PassesRotationFlipsAndLog()
    {
        Action<LogLevel, string> log = (_, _) => { };
        var opts = Y70tiDisplay.CreateOptions(rotation: 270, flipX: true, flipY: true, log: log);
        Assert.Equal(270, opts.Rotation);
        Assert.True(opts.FlipX);
        Assert.True(opts.FlipY);
        Assert.Same(log, opts.Log);
    }

    [Fact]
    public void Constants_MatchPanelSpec()
    {
        Assert.Equal(2560, Y70tiDisplay.PhysicalWidth);
        Assert.Equal(682, Y70tiDisplay.PhysicalHeight);
        Assert.Equal(90, Y70tiDisplay.DefaultRotation);
        Assert.Equal(60.0, Y70tiDisplay.NativeRefreshHz);
    }
}
