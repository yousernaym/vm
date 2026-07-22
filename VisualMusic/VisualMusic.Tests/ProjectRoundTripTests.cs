using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Midi;
using VisualMusic.Keyframes;
using Xunit;

namespace VisualMusic.Tests
{
    public class ProjectRoundTripTests
    {
        static Song EmptySong(int trackCount, int lengthT = 480)
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>(),
                SongLengthT = lengthT,
            };
            for (int i = 0; i < trackCount; i++)
                song.Tracks.Add(new Track { Length = lengthT, Name = i == 0 ? "" : $"T{i}" });
            return song;
        }

        static Project BuildSampleProject()
        {
            var song = EmptySong(2);
            var project = new Project();
            project.Notes = song;
            project.TrackViews = new List<TrackView>
            {
                new TrackView(0, 2, song),
                new TrackView(1, 2, song),
            };
            project.TrackViews[1].TrackProps.GlobalProps = project.TrackViews[0].TrackProps;

            project.ImportOptions = new ImportOptions(FileType.Midi)
            {
                RawNotePath = @"C:\songs\sample.mid",
                InsTrack = true,
                SongLengthS = 12.5f,
            };

            project.Props.FadeIn = 1.25f;
            project.Props.Camera.Fov = 0.9f;
            project.Props.LyricsSegments.Add(new LyricsSegment(4.0f) { Lyrics = "hello" });

            project.PropertyKeyframes = new KeyframeSet();
            project.PropertyKeyframes.Add("proj/BackgroundImageOpacity", 0, KfInterpolation.Linear,
                new ScalarKfValue(0.2));
            project.PropertyKeyframes.Add("track/1/Transp", 100, KfInterpolation.Smooth,
                new ScalarKfValue(0.5));

            return project;
        }

        static Project RoundTrip(Project source)
        {
            var dcs = new DataContractSerializer(typeof(Project), ProjectSerializer.KnownTypes);
            using var stream = new MemoryStream();
            dcs.WriteObject(stream, source);
            stream.Position = 0;
            return (Project)dcs.ReadObject(stream);
        }

        [Fact]
        public void DataContractSerializer_round_trips_persisted_fields()
        {
            var source = BuildSampleProject();
            var loaded = RoundTrip(source);

            Assert.NotNull(loaded.ImportOptions);
            Assert.Equal(FileType.Midi, loaded.ImportOptions.NoteFileType);
            Assert.True(loaded.ImportOptions.InsTrack);
            Assert.Equal(12.5f, loaded.ImportOptions.SongLengthS);
            Assert.Equal(@"C:\songs\sample.mid", loaded.ImportOptions.RawNotePath);

            Assert.Equal(1.25f, loaded.Props.FadeIn);
            Assert.Equal(0.9f, loaded.Props.Camera.Fov, 5);
            Assert.Single(loaded.Props.LyricsSegments);
            Assert.Equal("hello", loaded.Props.LyricsSegments[0].Lyrics);
            Assert.Equal(4.0f, loaded.Props.LyricsSegments[0].Beat);

            Assert.Equal(2, loaded.TrackViews.Count);
            Assert.Equal(0, loaded.TrackViews[0].TrackNumber);
            Assert.Equal(1, loaded.TrackViews[1].TrackNumber);

            Assert.True(loaded.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 0));
            Assert.True(loaded.PropertyKeyframes.HasKeyAt("track/1/Transp", 100));
            Assert.Equal(KfInterpolation.Linear,
                loaded.PropertyKeyframes.GetInterpolation("proj/BackgroundImageOpacity", 0));
            Assert.Equal(KfInterpolation.Smooth,
                loaded.PropertyKeyframes.GetInterpolation("track/1/Transp", 100));
            var opacityKf = loaded.PropertyKeyframes.Tracks["proj/BackgroundImageOpacity"].FindBrackets(0).Before;
            var transpKf = loaded.PropertyKeyframes.Tracks["track/1/Transp"].FindBrackets(100).Before;
            Assert.Equal(0.2, ((ScalarKfValue)opacityKf.Value).V, 5);
            Assert.Equal(0.5, ((ScalarKfValue)transpKf.Value).V, 5);

            // Notes are not part of the project file payload.
            Assert.Null(loaded.Notes);
        }

        [Fact]
        public void Clone_shareAudioProps_false_preserves_fields_and_shares_Notes()
        {
            var source = BuildSampleProject();
            var clone = source.Clone(shareAudioProps: false);

            Assert.Same(source.Notes, clone.Notes);
            Assert.Equal(source.Props.FadeIn, clone.Props.FadeIn);
            Assert.Equal(source.Props.Camera.Fov, clone.Props.Camera.Fov, 5);
            Assert.Equal(source.ImportOptions.InsTrack, clone.ImportOptions.InsTrack);
            Assert.True(clone.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 0));
            Assert.True(clone.PropertyKeyframes.HasKeyAt("track/1/Transp", 100));
            Assert.Equal(2, clone.TrackViews.Count);
            Assert.NotSame(
                source.TrackViews[1].TrackProps.AudioProps,
                clone.TrackViews[1].TrackProps.AudioProps);
        }
    }
}
