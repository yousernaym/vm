using System;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Hue/saturation at value = 1, matching the 2D picker: saturation 0 is white, 1 is the
    /// fully-saturated hue. HSL at lightness 0.5 maps saturation 0 to mid-grey, which is why
    /// picking or cancelling white used to turn the swatch grey.
    /// </summary>
    internal static class HueSatConvert
    {
        public static XnaColor ToXna(float hue, float saturation)
        {
            ToRgb(hue, saturation, out byte r, out byte g, out byte b);
            return new XnaColor(r, g, b, (byte)255);
        }

        public static void ToRgb(float hue, float saturation, out byte r, out byte g, out byte b)
        {
            hue = Math.Clamp(hue, 0f, 1f);
            saturation = Math.Clamp(saturation, 0f, 1f);

            float h = hue * 6f;
            int hi = (int)h;
            if (hi >= 6)
                hi = 0;
            float f = h - hi;
            float p = 1f - saturation;
            float q = 1f - f * saturation;
            float t = 1f - (1f - f) * saturation;

            float rf, gf, bf;
            switch (hi)
            {
                case 0:  rf = 1f; gf = t;  bf = p;  break;
                case 1:  rf = q;  gf = 1f; bf = p;  break;
                case 2:  rf = p;  gf = 1f; bf = t;  break;
                case 3:  rf = p;  gf = q;  bf = 1f; break;
                case 4:  rf = t;  gf = p;  bf = 1f; break;
                default: rf = 1f; gf = p;  bf = q;  break;
            }

            r = (byte)Math.Round(rf * 255f);
            g = (byte)Math.Round(gf * 255f);
            b = (byte)Math.Round(bf * 255f);
        }

        public static void FromRgb(byte r, byte g, byte b, out float hue, out float saturation)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            if (delta < 1e-6f)
                hue = 0f;
            else if (max == rf)
                hue = ((gf - bf) / delta) / 6f;
            else if (max == gf)
                hue = (2f + (bf - rf) / delta) / 6f;
            else
                hue = (4f + (rf - gf) / delta) / 6f;

            if (hue < 0f)
                hue += 1f;

            saturation = max < 1e-6f ? 0f : delta / max;
        }

        /// <summary>
        /// Dialog close policy: OK after a picker move writes the picked color and commits when it
        /// differs from <paramref name="original"/>. Cancel, or OK with no move, restores
        /// <paramref name="original"/> with no commit so an untouched white swatch cannot become grey.
        /// </summary>
        public static bool FinishDialog(XnaColor? original, bool accepted, bool pickerMoved,
            float hue, float saturation, out XnaColor? color)
        {
            if (!accepted || !pickerMoved)
            {
                color = original;
                return false;
            }

            var result = ToXna(hue, saturation);
            color = result;
            return original is not XnaColor o || o != result;
        }
    }
}
