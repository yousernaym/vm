using System;
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
        public void RestoreAfterFailedOpen_rewires_even_when_BindDrawProject_throws()
        {
            // If restoring live throws (style bake / SyncRenderer), Open must still re-wire the
            // panel and must not leave NoteStyle on the abandoned temp project.
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

                var temp = BuildProjectWithNoteOnTrack1();
                temp.CreateTrackViews(2, eraseCurrent: true);
                temp.InitAfterDeserialization(wp, loadAudio: false);
                NoteStyle.SetProject(temp);
                Assert.NotSame(liveCh, temp.TrackViews[1].TrackProps.AudioProps.SidWizChannel);

                vm.SyncRendererProject = p =>
                {
                    if (p != null)
                        throw new InvalidOperationException("simulated SyncRenderer / bake failure");
                };

                vm.RestoreAfterFailedOpen(wp);

                Assert.True(NoteStyle.HasProject);
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

        [Fact]
        public void RestoreAfterFailedOpen_null_project_clears_panel_channels()
        {
            // First Open with no live project: Init may have put temp channels on the panel;
            // restore must clear them rather than no-op on Project == null.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            try
            {
                Assert.Null(vm.Project);

                var temp = BuildProjectWithNoteOnTrack1();
                temp.CreateTrackViews(2, eraseCurrent: true);
                NoteStyle.SetProject(temp);
                temp.InitAfterDeserialization(wp, loadAudio: false);
                Assert.Equal(1, wp.ChannelCount);

                vm.RestoreAfterFailedOpen(wp);

                Assert.False(NoteStyle.HasProject);
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

        [Fact]
        public void RecoverAfterImportInitFailure_rebuilds_track_list_for_mutated_project()
        {
            // ImportSong mutates Project in place (OnProjectChanged does not fire). If Init fails
            // before TrackList.Rebuild, recover must still rebuild rows for the new track set.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = project;
                Assert.Equal(2, vm.TrackList.Items.Count); // Global + track 1

                // Simulate erase-import growing the track set without assigning Project=.
                project.Notes.Tracks.Add(new Track
                {
                    Length = 480,
                    Notes = new List<Note>
                    {
                        new Note { start = 0, stop = 60, channel = 1, pitch = 64, velocity = 100 },
                    },
                });
                project.CreateTrackViews(3, eraseCurrent: true);
                Assert.Equal(2, vm.TrackList.Items.Count); // still stale
                wp.ClearChannels();

                vm.RecoverAfterImportInitFailure(wp);

                Assert.Equal(3, vm.TrackList.Items.Count); // Global + 2 note tracks
                Assert.Equal(2, wp.ChannelCount);
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
        public void RecoverAfterImportInitFailure_invokes_SyncRendererProject()
        {
            // Pre-import BindDrawProject can no-op while the MonoGame host is still null; after
            // RequireRendererWaveformPanel builds it, Init failure must still SyncRendererProject
            // so SongRenderer.Project is not left null (black Song view with a correct track list).
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var vm = new MainViewModel();
            Project synced = null;
            vm.SyncRendererProject = p => synced = p;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = project;
                NoteStyle.SetProject(project);

                vm.RecoverAfterImportInitFailure(panelTouched: null);

                Assert.Same(project, synced);
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
        public void RecoverAfterImportInitFailure_null_panelTouched_uses_GetRendererWaveformPanel()
        {
            // RequireRendererWaveformPanel can throw before panelTouched is assigned; recovery must
            // still resolve the live panel and re-wire channels for the mutated import Project.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            vm.GetRendererWaveformPanel = () => wp;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = project;
                project.InitAfterDeserialization(wp, loadAudio: false);
                Assert.Equal(1, wp.ChannelCount);
                wp.ClearChannels();

                vm.RecoverAfterImportInitFailure(panelTouched: null);

                Assert.Equal(1, wp.ChannelCount);
                wp.RemoveChannel(project.TrackViews[1].TrackProps.AudioProps.SidWizChannel);
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
        public void RecoverAfterImportInitFailure_with_no_panel_still_refreshes_playback_offset()
        {
            // When no WaveformPanel is available, recovery must still Init (allowMissing) so
            // OnPlaybackOffsetSChanged / SongLengthS refresh after ImportSong mutated notes.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var vm = new MainViewModel();
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = project;
                // Simulate notes loaded after Props (deserialization order) — SongLengthS stale until Init.
                project.Props.PlaybackOffsetS = 0;
                project.Notes.SongLengthT = 960;

                vm.RecoverAfterImportInitFailure(panelTouched: null);

                Assert.True(project.SongLengthS > 0);
                Assert.Equal(2, vm.TrackList.Items.Count); // Global + track 1
            }
            finally
            {
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void BindDrawProject_invokes_SyncRendererProject()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var vm = new MainViewModel();
            Project synced = null;
            vm.SyncRendererProject = p => synced = p;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = project;

                vm.BindDrawProject(project);

                Assert.Same(project, synced);
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
        public void AbandonCreatedImportProject_clears_project_and_draw_binding()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var wp = new WaveformPanel();
            var vm = new MainViewModel();
            Project synced = new Project(); // sentinel: Sync must be invoked with null
            try
            {
                var shell = BuildProjectWithNoteOnTrack1();
                shell.CreateTrackViews(2, eraseCurrent: true);
                vm.Project = shell;
                NoteStyle.SetProject(shell);
                vm.SyncRendererProject = p => synced = p;
                vm.GetRendererWaveformPanel = () => wp;
                shell.InitAfterDeserialization(wp, loadAudio: false);
                Assert.Equal(1, wp.ChannelCount);

                vm.AbandonCreatedImportProject();

                Assert.Null(vm.Project);
                Assert.False(vm.HasProject);
                Assert.False(NoteStyle.HasProject);
                Assert.Null(synced);
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
        public void SyncSongLengthFromOpenAudio_noops_when_media_has_no_audio()
        {
            // Without an open Media file, recovery must not invent a song length.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = new Song
                {
                    TicksPerBeat = 480,
                    TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                    Tracks = new List<Track>
                    {
                        new Track { Length = 0 },
                        new Track { Length = 0 },
                    },
                    SongLengthT = 0,
                };
                var project = new Project
                {
                    Notes = song,
                    ImportOptions = new AudioImportOptions(),
                };
                project.CreateTrackViews(2, eraseCurrent: true);

                project.SyncSongLengthFromOpenAudio(propagateToAudioOnlyTracks: true);

                Assert.Equal(0, project.Notes.SongLengthT);
                Assert.Equal(0, project.Notes.Tracks[1].Length);
            }
            finally
            {
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
