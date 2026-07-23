using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using LibSidWiz;
using Microsoft.Xna.Framework.Content;
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

            // Distinctive persisted Style / Material fields (defaults alone wouldn't prove DCS write-back).
            project.TrackViews[0].TrackProps.StyleProps.Type = NoteStyleType.Bar;
            project.TrackViews[1].TrackProps.StyleProps.Type = NoteStyleType.Line;
            project.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth = 7.5f;
            project.TrackViews[1].TrackProps.MaterialProps.Transp = 0.35f;
            project.TrackViews[1].TrackProps.MaterialProps.Hue = 0.6f;
            project.TrackViews[1].TrackProps.SpatialProps.XOffset = 1.25f;
            project.TrackViews[1].TrackProps.LightProps.AmbientAmount = 0.42f;

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
            var previousNumTracks = TrackView.NumTracks;
            try
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

                Assert.Equal(NoteStyleType.Bar, loaded.TrackViews[0].TrackProps.StyleProps.Type);
                Assert.Equal(NoteStyleType.Line, loaded.TrackViews[1].TrackProps.StyleProps.Type);
                Assert.Equal(7.5f, loaded.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth!.Value);
                Assert.Equal(0.35f, loaded.TrackViews[1].TrackProps.MaterialProps.Transp!.Value);
                Assert.Equal(0.6f, loaded.TrackViews[1].TrackProps.MaterialProps.Hue!.Value);
                Assert.Equal(1.25f, loaded.TrackViews[1].TrackProps.SpatialProps.XOffset!.Value);
                Assert.Equal(0.42f, loaded.TrackViews[1].TrackProps.LightProps.AmbientAmount!.Value);

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
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void RoundTrip_CreateTrackViews_and_InitAfterDeserialization_rewire_live_state()
        {
            // Mirrors OpenProject after DCS: re-open notes → CreateTrackViews(preserve) → InitAfterDeserialization.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var loaded = RoundTrip(BuildSampleProject());

                Assert.Null(loaded.Notes);
                Assert.Null(loaded.TrackViews[0].MidiTrack);
                Assert.Null(loaded.TrackViews[1].MidiTrack);
                // Project ctor rewires these during deserialize (not serialized on TrackProps).
                Assert.Same(loaded.TrackViews[0].TrackProps, loaded.TrackViews[1].TrackProps.GlobalProps);
                Assert.Same(loaded.TrackViews[0], loaded.TrackViews[0].TrackProps.TrackView);
                Assert.Same(loaded.TrackViews[1], loaded.TrackViews[1].TrackProps.TrackView);

                // Fresh note data (as OpenNoteFile would assign), with a distinctive track-1 note.
                var notes = EmptySong(2);
                notes.Tracks[1].Notes.Add(new Note
                {
                    start = 10, stop = 20, pitch = 60, velocity = 80, channel = 0,
                });
                loaded.Notes = notes;

                loaded.CreateTrackViews(notes.Tracks.Count, eraseCurrent: false, preserveTrackSet: true);

                Assert.Equal(2, loaded.TrackViews.Count);
                Assert.Same(notes.Tracks[0], loaded.TrackViews[0].MidiTrack);
                Assert.Same(notes.Tracks[1], loaded.TrackViews[1].MidiTrack);
                Assert.Single(loaded.TrackViews[1].MidiTrack.Notes);
                Assert.Same(loaded.TrackViews[0].TrackProps, loaded.TrackViews[1].TrackProps.GlobalProps);

                // CreateTrackViews already re-called StyleProps.LoadFx (soft-skip without Content;
                // SetContent would retry once a real ContentManager is installed).
                loaded.InitAfterDeserialization(
                    waveformPanel: null, loadAudio: false, allowMissingWaveformPanel: true);
                Assert.Null(loaded.TrackViews[1].Geo);

                // 480 ticks at 120 bpm = 0.5s — deferred OnPlaybackOffsetSChanged finally runs with Notes set.
                Assert.Equal(0.5, loaded.SongLengthS, 3);

                loaded.TrackViews[1].TrackProps.MaterialProps.Transp = 0f;
                loaded.NormSongPos = 0.5; // SongPosT == 240; keyframes at 0 and 100 → past end holds 0.5
                loaded.InterpolatePropertyKeyframes();
                Assert.Equal(0.5f, loaded.TrackViews[1].TrackProps.MaterialProps.Transp!.Value, 3);

                // After rewire, installing Content retries FX load (empty root → ContentLoadException).
                NoteStyle.SetProject(loaded);
                var cm = new ContentManager(new EmptyServiceProvider(), "Content-missing-for-test");
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(cm));
                Assert.False(NoteStyle.HasContent);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void InitAfterDeserialization_throws_when_waveform_panel_missing()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var loaded = RoundTrip(BuildSampleProject());
                loaded.Notes = EmptySong(2);
                loaded.CreateTrackViews(2, eraseCurrent: false, preserveTrackSet: true);

                var ex = Assert.Throws<InvalidOperationException>(() =>
                    loaded.InitAfterDeserialization(waveformPanel: null, loadAudio: false));
                Assert.Contains("WaveformPanel", ex.Message);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void InitAfterDeserialization_wires_non_null_waveform_panel()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            try
            {
                var loaded = RoundTrip(BuildSampleProject());
                var notes = EmptySong(2);
                notes.Tracks[1].Notes.Add(new Note
                {
                    start = 10, stop = 20, pitch = 60, velocity = 80, channel = 0,
                });
                loaded.Notes = notes;
                loaded.CreateTrackViews(notes.Tracks.Count, eraseCurrent: false, preserveTrackSet: true);

                // Stale channel must be cleared; track 1's SidWizChannel is then added (track 0 skipped).
                wp.AddChannel(new Channel(autoReloadOnSettingChanged: false));
                Assert.Equal(1, wp.ChannelCount);

                loaded.InitAfterDeserialization(waveformPanel: wp, loadAudio: false);

                Assert.Equal(1, wp.ChannelCount);
                var expected = loaded.TrackViews[1].TrackProps.AudioProps.SidWizChannel;
                wp.RemoveChannel(expected);
                Assert.Equal(0, wp.ChannelCount);
            }
            finally
            {
                wp.Dispose();
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        sealed class EmptyServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        [Fact]
        public void Clone_shareAudioProps_false_preserves_fields_and_shares_Notes()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var source = BuildSampleProject();
                var clone = source.Clone(shareAudioProps: false);

                Assert.Same(source.Notes, clone.Notes);
                Assert.Equal(source.Props.FadeIn, clone.Props.FadeIn);
                Assert.Equal(source.Props.Camera.Fov, clone.Props.Camera.Fov, 5);
                Assert.Equal(source.ImportOptions.InsTrack, clone.ImportOptions.InsTrack);
                Assert.Equal(NoteStyleType.Line, clone.TrackViews[1].TrackProps.StyleProps.Type);
                Assert.Equal(7.5f, clone.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth!.Value);
                Assert.Equal(0.35f, clone.TrackViews[1].TrackProps.MaterialProps.Transp!.Value);
                Assert.True(clone.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 0));
                Assert.True(clone.PropertyKeyframes.HasKeyAt("track/1/Transp", 100));
                Assert.Equal(2, clone.TrackViews.Count);
                Assert.NotSame(
                    source.TrackViews[1].TrackProps.AudioProps,
                    clone.TrackViews[1].TrackProps.AudioProps);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }
    }
}
