using System.Collections.Generic;
using Midi;
using VisualMusic.ViewModels;
using Xunit;

namespace VisualMusic.Tests
{
    public class OpenFailureRestoreTests
    {
        [Fact]
        public void RestoreAfterFailedOpen_rewires_waveform_channels_to_live_project()
        {
            // Open Init clears the panel for tempProject; on failure RestoreAfterFailedOpen must
            // put the live project's SidWiz channels back (not leave temp's channels drawn).
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            try
            {
                var live = BuildProjectWithNoteOnTrack1();
                live.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = live;
                NoteStyle.SetProject(live);
                live.InitAfterDeserialization(wp, loadAudio: false);
                var liveCh = live.TrackViews[1].TrackProps.AudioProps.SidWizChannel;
                Assert.Equal(1, wp.ChannelCount);

                var temp = BuildProjectWithNoteOnTrack1();
                temp.CreateTrackViews(2, eraseCurrent: true);
                temp.InitAfterDeserialization(wp, loadAudio: false);
                var tempCh = temp.TrackViews[1].TrackProps.AudioProps.SidWizChannel;
                Assert.NotSame(liveCh, tempCh);
                Assert.Equal(1, wp.ChannelCount);

                vm.RestoreAfterFailedOpen(wp);

                Assert.True(NoteStyle.HasProject);
                Assert.Equal(1, wp.ChannelCount);
                // Live channel is back on the panel; removing it empties the list.
                wp.RemoveChannel(liveCh);
                Assert.Equal(0, wp.ChannelCount);
            }
            finally
            {
                wp.Dispose();
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void RestoreAfterFailedOpen_with_null_panel_skips_channel_rewire()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            try
            {
                var live = BuildProjectWithNoteOnTrack1();
                live.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = live;
                live.InitAfterDeserialization(wp, loadAudio: false);
                Assert.Equal(1, wp.ChannelCount);

                var temp = BuildProjectWithNoteOnTrack1();
                temp.CreateTrackViews(2, eraseCurrent: true);
                // Simulate Init of temp (panel now has temp channels) then a failure before
                // Restore gets a panel reference — null panel must not ClearChannels again.
                temp.InitAfterDeserialization(wp, loadAudio: false);
                Assert.Equal(1, wp.ChannelCount);

                vm.RestoreAfterFailedOpen(panelTouched: null);

                Assert.True(NoteStyle.HasProject);
                // Panel untouched (still temp's single channel).
                Assert.Equal(1, wp.ChannelCount);
                wp.RemoveChannel(temp.TrackViews[1].TrackProps.AudioProps.SidWizChannel);
                Assert.Equal(0, wp.ChannelCount);
            }
            finally
            {
                wp.Dispose();
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void RestoreAfterFailedOpen_invokes_SyncRendererProject_with_live_project()
        {
            // Open binds SongRenderer.Project to temp via SyncRendererProject; failure must bind live.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var vm = new MainViewModel();
            Project synced = null;
            vm.SyncRendererProject = p => synced = p;
            try
            {
                var live = BuildProjectWithNoteOnTrack1();
                live.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = live;
                NoteStyle.SetProject(live);

                var temp = BuildProjectWithNoteOnTrack1();
                vm.BindDrawProject(temp);
                Assert.Same(temp, synced);

                vm.RestoreAfterFailedOpen(panelTouched: null);

                Assert.Same(live, synced);
                Assert.True(NoteStyle.HasProject);
            }
            finally
            {
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void RewireWaveformChannels_restores_live_channels_after_clear()
        {
            // Import Init failure path: panel was cleared for a partial Init; rewire live channels.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            try
            {
                var live = BuildProjectWithNoteOnTrack1();
                live.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = live;
                live.InitAfterDeserialization(wp, loadAudio: false);
                var liveCh = live.TrackViews[1].TrackProps.AudioProps.SidWizChannel;
                Assert.Equal(1, wp.ChannelCount);

                wp.ClearChannels();
                Assert.Equal(0, wp.ChannelCount);

                vm.RewireWaveformChannels(wp);

                Assert.Equal(1, wp.ChannelCount);
                wp.RemoveChannel(liveCh);
                Assert.Equal(0, wp.ChannelCount);
            }
            finally
            {
                wp.Dispose();
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        static Project BuildProjectWithNoteOnTrack1()
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>
                {
                    new Track { Length = 480 },
                    new Track
                    {
                        Length = 480,
                        Notes = new List<Note>
                        {
                            new Note { start = 0, stop = 120, channel = 0, pitch = 60, velocity = 100 },
                        },
                    },
                },
                SongLengthT = 480,
            };
            var project = new Project
            {
                Notes = song,
                // InitAfterDeserialization calls ImportOptions.UpdateImportForm().
                ImportOptions = new MidiImportOptions(),
            };
            return project;
        }
    }
}
