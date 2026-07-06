using SpecializedDisplay.Y70ti;

namespace SpecializedDisplay.Y70ti.Tests;

public class Y70tiTouchTests
{
    // Y70ti logical canvas (682 wide x 2560 tall).
    private const float CanvasW = 682f;
    private const float CanvasH = 2560f;

    [Theory]
    // The four measured corner-holds -> the logical canvas corners, through the profile factory.
    [InlineData(9464, 225, 0f, 0f)]           // top-left
    [InlineData(9377, 9499, CanvasW, 0f)]     // top-right
    [InlineData(274, 9505, CanvasW, CanvasH)] // bottom-right
    [InlineData(253, 264, 0f, CanvasH)]       // bottom-left
    public void CreateCalibration_MeasuredCorners_Within40px(int rawX, int rawY, float ex, float ey)
    {
        Y70tiTouch.CreateCalibration().Map(rawX, rawY, out float x, out float y);
        float err = MathF.Max(MathF.Abs(x - ex), MathF.Abs(y - ey));
        Assert.True(err <= 40f, $"raw=({rawX},{rawY}) -> ({x:F1},{y:F1}) expected ~({ex:F0},{ey:F0}) err={err:F1}");
    }

    [Theory]
    // rot270 is an exact 180 deg rotation of the rot90 mapping in logical space.
    [InlineData(9464, 225, CanvasW, CanvasH)] // rot270 TL -> BR
    [InlineData(274, 9505, 0f, 0f)]           // rot270 BR -> TL
    public void CreateCalibration270_Is180OfRot90(int rawX, int rawY, float ex, float ey)
    {
        Y70tiTouch.CreateCalibration(270).Map(rawX, rawY, out float x, out float y);
        float err = MathF.Max(MathF.Abs(x - ex), MathF.Abs(y - ey));
        Assert.True(err <= 40f, $"raw=({rawX},{rawY}) -> ({x:F1},{y:F1}) expected ~({ex:F0},{ey:F0}) err={err:F1}");
    }

    [Fact]
    public void CalibrationModel_MatchesMeasuredBounds()
    {
        var m = Y70tiTouch.CalibrationModel;
        Assert.Equal(253, m.RawXMin);
        Assert.Equal(9464, m.RawXMax);
        Assert.Equal(245, m.RawYMin);
        Assert.Equal(9499, m.RawYMax);
        Assert.True(m.HorizontalFromRawY);
        Assert.False(m.InvertHorizontal);
        Assert.True(m.InvertVertical);
    }

    [Fact]
    public void VidPid_AreIlitek()
    {
        Assert.Equal((ushort)0x222A, Y70tiTouch.Vid);
        Assert.Equal((ushort)0x0001, Y70tiTouch.Pid);
    }
}
