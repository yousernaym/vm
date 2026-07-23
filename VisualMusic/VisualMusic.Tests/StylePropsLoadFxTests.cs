using System;
using Xunit;

namespace VisualMusic.Tests
{
    public class StylePropsLoadFxTests
    {
        [Fact]
        public void StyleProps_ctor_and_LoadFx_are_safe_without_content()
        {
            Assert.False(NoteStyle.HasContent);

            var styles = new StyleProps(1);

            Assert.NotNull(styles.GetBarStyle());
            Assert.NotNull(styles.GetLineStyle());
            Assert.Equal(NoteStyleType.Default, styles.Type);

            // CreateTrackViews re-calls this once Content exists; must remain a no-op until then.
            styles.LoadFx();
        }

        [Fact]
        public void NoteStyle_LoadFx_fails_loudly_without_content()
        {
            Assert.False(NoteStyle.HasContent);

            var bar = new NoteStyle_Bar();
            var line = new NoteStyle_Line();

            Assert.Throws<InvalidOperationException>(() => bar.LoadFx());
            Assert.Throws<InvalidOperationException>(() => line.LoadFx());
        }
    }
}
