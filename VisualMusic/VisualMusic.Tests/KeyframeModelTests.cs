using Microsoft.Xna.Framework;
using VisualMusic.Keyframes;
using Xunit;

namespace VisualMusic.Tests
{
    public class KeyframeModelTests
    {
        [Fact]
        public void FindBrackets_before_first_and_after_last()
        {
            var track = new PropertyKeyframeTrack();
            track.Add(100, KfInterpolation.Linear, new ScalarKfValue(1));
            track.Add(200, KfInterpolation.Linear, new ScalarKfValue(2));

            var (before, after, t) = track.FindBrackets(50);
            Assert.Null(before);
            Assert.NotNull(after);
            Assert.Equal(0, t);

            (before, after, t) = track.FindBrackets(250);
            Assert.NotNull(before);
            Assert.Null(after);

            (before, after, t) = track.FindBrackets(150);
            Assert.Equal(100, before.Tick);
            Assert.Equal(200, after.Tick);
            Assert.Equal(0.5, t, 5);
        }

        [Theory]
        [InlineData(KfInterpolation.Hold, 0.5, 1.0)]
        [InlineData(KfInterpolation.Linear, 0.5, 2.0)]
        [InlineData(KfInterpolation.Smooth, 0.5, 2.0)]
        public void InterpolateValue_modes(KfInterpolation mode, double t, double expected)
        {
            double result = PropertyKeyframeTrack.InterpolateValue(1, 3, t, mode, logScale: false);
            Assert.Equal(expected, result, 5);
        }

        [Fact]
        public void InterpolateValue_logScale()
        {
            // Midpoint in log2 space between 2 and 8 is 4
            double result = PropertyKeyframeTrack.InterpolateValue(2, 8, 0.5, KfInterpolation.Linear, logScale: true);
            Assert.Equal(4.0, result, 5);
        }

        [Fact]
        public void ColorKfValue_Lerp_hold_and_linear()
        {
            var a = new ColorKfValue(new Color(0, 0, 0, 255));
            var b = new ColorKfValue(new Color(100, 200, 50, 255));
            Assert.Same(a, ColorKfValue.Lerp(a, b, 0.5, KfInterpolation.Hold));
            var mid = ColorKfValue.Lerp(a, b, 0.5, KfInterpolation.Linear);
            Assert.Equal(50, mid.C.R);
            Assert.Equal(100, mid.C.G);
            Assert.Equal(25, mid.C.B);
        }
    }
}
