using System;
using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class ProjectPlaybackTests
    {
        [Fact]
        public void Start_from_offset_keeps_visual_playback_running_during_clock_transition()
        {
            var host = new FakeSongDrawHost();
            Project.SetDrawHost(host);
            try
            {
                var audio = new FakeAudioPlayback
                {
                    Length = 10,
                    Position = 0.25,
                    Running = false,
                    StartSucceeds = true
                };
                Project project = CreateProject(audio, playbackOffsetS: 0.1f);
                project.SetSongPosS(0.35, updateScreen: true);

                project.TogglePlayback();

                Assert.True(project.IsPlaying);
                Assert.True(project.AudioHasStarted);
                Assert.Equal(0.25, audio.LastStartTime, 3);

                // The native start event has succeeded, but the presentation clock still reports
                // its prior non-running state. Visual playback must continue through that window.
                host.TotalTimeElapsed = TimeSpan.FromSeconds(0.05);
                project.Update(0.05);

                Assert.True(project.IsPlaying);
                Assert.Equal(0.40, project.SongPosS, 3);
            }
            finally
            {
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Resume_after_seek_keeps_visual_playback_running_during_clock_transition()
        {
            var host = new FakeSongDrawHost();
            Project.SetDrawHost(host);
            try
            {
                var audio = new FakeAudioPlayback
                {
                    Length = 10,
                    Position = 0.20,
                    Running = true,
                    StartSucceeds = true
                };
                Project project = CreateProject(audio, playbackOffsetS: 0.1f);
                project.SetSongPosS(0.30, updateScreen: true);
                project.TogglePlayback();
                Assert.True(project.IsPlaying);

                // This is the playback part of right-button scrubbing: pause, move the visual
                // playhead, then resume from the new position.
                project.TogglePlayback();
                project.SetSongPosS(0.50, updateScreen: true);
                audio.Running = false;
                audio.Position = 0.40;
                project.TogglePlayback();

                Assert.True(project.IsPlaying);
                Assert.Equal(0.40, audio.LastStartTime, 3);

                host.TotalTimeElapsed = TimeSpan.FromSeconds(0.05);
                project.Update(0.05);

                Assert.True(project.IsPlaying);
                Assert.Equal(0.55, project.SongPosS, 3);
            }
            finally
            {
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Fast_forward_within_positive_offset_does_not_pause_audio_before_it_starts()
        {
            var host = new FakeSongDrawHost();
            Project.SetDrawHost(host);
            try
            {
                var audio = new FakeAudioPlayback
                {
                    Length = 10,
                    Running = false,
                    StartSucceeds = true,
                    FailStartAfterPrematurePause = true
                };
                Project project = CreateProject(audio, playbackOffsetS: 0.1f);
                project.SetSongPosS(0.02, updateScreen: true);

                project.TogglePlayback();
                Assert.True(project.IsPlaying);
                Assert.False(project.AudioHasStarted);

                // Right-button fast-forward pauses visual pre-roll. Native audio has not started,
                // so sending Media Foundation a Pause here would poison its pending first start.
                project.TogglePlayback();
                Assert.False(project.IsPlaying);
                Assert.Equal(0, audio.PauseCalls);

                project.SetSongPosS(0.08, updateScreen: true);
                project.TogglePlayback();
                host.TotalTimeElapsed = TimeSpan.FromSeconds(0.03);
                project.Update(0.03);

                Assert.True(project.IsPlaying);
                Assert.True(project.AudioHasStarted);
                Assert.Equal(0, audio.PauseCalls);
                Assert.Equal(0.01, audio.LastStartTime, 3);
            }
            finally
            {
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Crossing_zero_retries_transient_first_audio_start_failure()
        {
            var host = new FakeSongDrawHost();
            Project.SetDrawHost(host);
            try
            {
                var audio = new FakeAudioPlayback
                {
                    Length = 10,
                    StartSucceeds = true,
                    StartFailuresRemaining = 1
                };
                Project project = CreateProject(audio, playbackOffsetS: 0.1f);

                project.TogglePlayback();
                host.TotalTimeElapsed = TimeSpan.FromSeconds(0.11);
                project.Update(0.11);

                Assert.True(project.IsPlaying);
                Assert.False(project.AudioHasStarted);
                Assert.Equal(1, audio.StartCalls);
                Assert.Equal(0.11, project.SongPosS, 3);

                // The retry is throttled, but the render clock must keep advancing meanwhile.
                host.TotalTimeElapsed = TimeSpan.FromSeconds(0.15);
                project.Update(0.04);
                Assert.True(project.IsPlaying);
                Assert.Equal(1, audio.StartCalls);
                Assert.Equal(0.15, project.SongPosS, 3);

                host.TotalTimeElapsed = TimeSpan.FromSeconds(0.22);
                project.Update(0.07);

                Assert.True(project.IsPlaying);
                Assert.True(project.AudioHasStarted);
                Assert.Equal(2, audio.StartCalls);
                Assert.Equal(0.12, audio.LastStartTime, 3);
            }
            finally
            {
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Non_running_clock_at_audio_end_stops_playback()
        {
            var host = new FakeSongDrawHost();
            Project.SetDrawHost(host);
            try
            {
                var audio = new FakeAudioPlayback
                {
                    Length = 1,
                    Position = 0.25,
                    Running = true,
                    StartSucceeds = true
                };
                Project project = CreateProject(audio, playbackOffsetS: 0.1f);
                project.SetSongPosS(0.35, updateScreen: true);
                project.TogglePlayback();

                audio.Position = audio.Length;
                audio.Running = false;
                project.Update(0.1);

                Assert.False(project.IsPlaying);
            }
            finally
            {
                Project.SetDrawHost(null);
            }
        }

        static Project CreateProject(IAudioPlayback audio, float playbackOffsetS)
        {
            var song = new Song();
            song.OpenMidiFile(TestFiles.PathTo("midiLib", "minimal.mid"));

            var project = new Project
            {
                TrackViews = new System.Collections.Generic.List<TrackView>(),
                Notes = song,
                AudioPlayback = audio
            };
            project.Props.PlaybackOffsetS = playbackOffsetS;
            return project;
        }

        sealed class FakeAudioPlayback : IAudioPlayback
        {
            public double Length { get; set; }
            public bool Running { get; set; }
            public double Position { get; set; }
            public bool StartSucceeds { get; set; }
            public int StartFailuresRemaining { get; set; }
            public int StartCalls { get; private set; }
            public double LastStartTime { get; private set; } = double.NaN;
            public bool FailStartAfterPrematurePause { get; set; }
            public int PauseCalls { get; private set; }

            public double GetLength() => Length;
            public bool IsRunning() => Running;
            public double GetPosition() => Position;
            public bool StartAtTime(double timeS)
            {
                StartCalls++;
                LastStartTime = timeS;
                if (StartFailuresRemaining > 0)
                {
                    StartFailuresRemaining--;
                    return false;
                }
                return StartSucceeds && (!FailStartAfterPrematurePause || PauseCalls == 0);
            }
            public bool Pause()
            {
                PauseCalls++;
                Running = false;
                return true;
            }
            public bool Stop()
            {
                Running = false;
                Position = 0;
                return true;
            }
        }
    }
}
