using Microsoft.Xna.Framework;
using Xunit;

namespace VisualMusic.Tests
{
    public class NoteStyleBarTests
    {
        [Fact]
        public void Default_note_anchor_maps_texture_from_upper_left_0_0_to_lower_right_1_1()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            try
            {
                var project = new Project();
                NoteStyle.SetProject(project);
                var style = new NoteStyle_Bar();
                var material = new MaterialProps(1, 0f);

                Assert.Equal(TexAnchorEnum.Note, material.TexProps.UAnchor);
                Assert.Equal(TexAnchorEnum.Note, material.TexProps.VAnchor);
                Assert.False(material.TexProps.UTile);
                Assert.False(material.TexProps.VTile);

                // destRect origin is min-Y (visual bottom-left); mesh (0,1) is visual top-left.
                Vector2 topLeftWorld = new Vector2(0.2f, -0.1f);
                Vector2 sizeWorld = new Vector2(0.4f, 0.05f);
                style.CalcRectTexCoords(out Vector2 topLeftTex, out Vector2 sizeTex,
                    new Vector2(128, 64), topLeftWorld, sizeWorld, material);

                Vector2 upperLeft = topLeftTex + new Vector2(0, 1) * sizeTex;
                Vector2 lowerRight = topLeftTex + new Vector2(1, 0) * sizeTex;
                Assert.Equal(0f, upperLeft.X, 5);
                Assert.Equal(0f, upperLeft.Y, 5);
                Assert.Equal(1f, lowerRight.X, 5);
                Assert.Equal(1f, lowerRight.Y, 5);
            }
            finally
            {
                NoteStyle.SetProject(null);
                Project.SetDrawHost(null);
            }
        }
    }
}
