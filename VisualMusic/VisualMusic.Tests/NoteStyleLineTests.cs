using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class NoteStyleLineTests
    {
        static Track TrackWithSeparatedNotes() => new Track
        {
            Notes = new List<Note>
            {
                new Note { start = 100, stop = 200, pitch = 60 },
                new Note { start = 300, stop = 400, pitch = 62 },
            },
        };

        static Curve CurveWithKeys(params (float Position, float Value)[] keys)
        {
            var curve = new Curve();
            foreach (var (position, value) in keys)
                curve.Keys.Add(new CurveKey(position, value));
            curve.ComputeTangents(CurveTangent.Smooth);
            return curve;
        }

        [Theory]
        [InlineData(99.0, -1)]
        [InlineData(100.0, 0)]
        [InlineData(199.999, 0)]
        [InlineData(200.0, -1)]
        [InlineData(250.0, -1)]
        [InlineData(300.0, 1)]
        [InlineData(400.0, -1)]
        [InlineData(500.0, -1)]
        public void Highlight_exists_only_while_selected_note_is_playing(double playbackTick, int expectedIndex)
        {
            Assert.Equal(expectedIndex,
                NoteStyle_Line.GetHighlightedNoteIndex(TrackWithSeparatedNotes(), playbackTick));
        }

        [Fact]
        public void Duplicate_same_tick_keys_are_not_hardware_extrema()
        {
            // t.vmp: two identical notes at tick 3600. The extra key is a zero-duration
            // plateau, not a peak/valley; flattening its incoming tangent shears the
            // hardware offset strip on the following diagonal.
            Curve curve = CurveWithKeys(
                (3360, 70),
                (3600, 69),
                (3600, 69),
                (4320, 67));
            Assert.False(NoteStyle_Line.IsHardwareLocalExtremum(curve, 1));
            Assert.False(NoteStyle_Line.IsHardwareLocalExtremum(curve, 2));
        }

        [Fact]
        public void Peak_key_is_still_a_hardware_extremum()
        {
            Curve curve = CurveWithKeys((0, 70), (100, 82), (200, 69));
            Assert.True(NoteStyle_Line.IsHardwareLocalExtremum(curve, 1));
        }

        [Fact]
        public void Sustained_plateau_start_is_still_a_hardware_extremum()
        {
            Curve curve = CurveWithKeys((0, 70), (100, 69), (200, 69), (300, 67));
            Assert.True(NoteStyle_Line.IsHardwareLocalExtremum(curve, 1));
        }

        [Fact]
        public void Line_segment_bounds_hug_the_stroke_instead_of_a_far_aabb()
        {
            const float halfWidth = 2f;
            BoundingBoxEx horizontal = NoteStyle_Line.CreateLineSegmentBounds(
                new Vector3(0, 50, 0), new Vector3(100, 50, 0), halfWidth, LineType.Standard);

            Assert.InRange(horizontal.Aabb.Max.Y - horizontal.Aabb.Min.Y, 3.9f, 4.1f);
            Assert.True(horizontal.ContainsPoint(new Vector3(50, 50, 0)));
            Assert.True(horizontal.ContainsPoint(new Vector3(50, 51.5f, 0)));
            Assert.False(horizontal.ContainsPoint(new Vector3(50, 70, 0)));

            BoundingBoxEx diagonal = NoteStyle_Line.CreateLineSegmentBounds(
                new Vector3(0, 0, 0), new Vector3(100, 100, 0), halfWidth, LineType.Standard);
            Assert.True(diagonal.ContainsPoint(new Vector3(50, 50, 0)));
            // Control-point / multi-beat AABBs treated this corner as part of the line.
            Assert.False(diagonal.ContainsPoint(new Vector3(100, 0, 0)));
        }

        [Fact]
        public void Horizontal_ribbon_bounds_stay_selectable()
        {
            BoundingBoxEx ribbon = NoteStyle_Line.CreateLineSegmentBounds(
                new Vector3(0, 0, 0), new Vector3(40, 0, 0), 3f, LineType.Ribbon);
            Assert.True(ribbon.ContainsPoint(new Vector3(20, 0, 0)));
            Assert.False(ribbon.ContainsPoint(new Vector3(20, 20, 0)));
        }
    }
}
