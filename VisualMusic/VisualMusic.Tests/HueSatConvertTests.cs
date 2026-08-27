using VisualMusic.Controls;
using Xunit;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.Tests
{
    public class HueSatConvertTests
    {
        [Fact]
        public void Saturation_zero_is_white_not_mid_grey()
        {
            var c = HueSatConvert.ToXna(0.3f, 0f);
            Assert.Equal(255, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void White_roundtrips_to_white()
        {
            HueSatConvert.FromRgb(255, 255, 255, out float hue, out float sat);
            Assert.Equal(0f, sat, 5);
            var c = HueSatConvert.ToXna(hue, sat);
            Assert.Equal(XnaColor.White, c);
        }

        [Fact]
        public void Primary_red_roundtrips()
        {
            HueSatConvert.FromRgb(255, 0, 0, out float hue, out float sat);
            Assert.Equal(0f, hue, 5);
            Assert.Equal(1f, sat, 5);
            var c = HueSatConvert.ToXna(hue, sat);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Theory]
        [InlineData(255, 0, 0)]
        [InlineData(0, 255, 0)]
        [InlineData(0, 0, 255)]
        [InlineData(255, 255, 0)]
        [InlineData(255, 128, 128)]
        public void Saturated_colors_roundtrip_within_one(byte r, byte g, byte b)
        {
            HueSatConvert.FromRgb(r, g, b, out float hue, out float sat);
            var c = HueSatConvert.ToXna(hue, sat);
            Assert.InRange(c.R, r - 1, r + 1);
            Assert.InRange(c.G, g - 1, g + 1);
            Assert.InRange(c.B, b - 1, b + 1);
        }

        [Fact]
        public void FinishDialog_ok_without_moving_keeps_original_white()
        {
            bool commit = HueSatConvert.FinishDialog(XnaColor.White, accepted: true, pickerMoved: false,
                0f, 0f, out var color);
            Assert.Equal(XnaColor.White, color);
            Assert.False(commit);
        }

        [Fact]
        public void FinishDialog_cancel_restores_original_white()
        {
            bool commit = HueSatConvert.FinishDialog(XnaColor.White, accepted: false, pickerMoved: true,
                0.5f, 1f, out var color);
            Assert.Equal(XnaColor.White, color);
            Assert.False(commit);
        }

        [Fact]
        public void FinishDialog_ok_on_new_color_commits_once()
        {
            bool commit = HueSatConvert.FinishDialog(XnaColor.White, accepted: true, pickerMoved: true,
                0f, 1f, out var color);
            Assert.Equal(new XnaColor(255, 0, 0), color);
            Assert.True(commit);
        }

        [Fact]
        public void FinishDialog_picking_white_from_red_is_white_not_grey()
        {
            var red = new XnaColor(255, 0, 0);
            bool commit = HueSatConvert.FinishDialog(red, accepted: true, pickerMoved: true,
                0f, 0f, out var color);
            Assert.Equal(XnaColor.White, color);
            Assert.True(commit);
        }
    }
}
