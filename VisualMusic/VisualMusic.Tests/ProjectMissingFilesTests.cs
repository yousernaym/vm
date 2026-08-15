using System;
using System.Collections.Generic;
using System.IO;
using Midi;
using VisualMusic.Keyframes;
using Xunit;

namespace VisualMusic.Tests
{
    public class ProjectMissingFilesTests
    {
        const string MissingNote = @"C:\vm-missing-files-tests\does-not-exist.mid";
        const string MissingAudio = @"C:\vm-missing-files-tests\does-not-exist.wav";
        const string MissingTex = @"C:\vm-missing-files-tests\does-not-exist.png";

        static Song EmptySong(int trackCount)
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>(),
                SongLengthT = 480,
            };
            for (int i = 0; i < trackCount; i++)
                song.Tracks.Add(new Track { Length = 480, Name = i == 0 ? "" : $"T{i}" });
            return song;
        }

        static Project BuildProject(FileType noteType = FileType.Midi, string notePath = MissingNote)
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
            project.ImportOptions = new ImportOptions(noteType)
            {
                RawNotePath = notePath,
            };
            return project;
        }

        [Fact]
        public void EnumerateMissing_reports_absent_note_and_skips_present_file()
        {
            string present = Path.Combine(Path.GetTempPath(), "vm-mf-" + Guid.NewGuid().ToString("N") + ".mid");
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                File.WriteAllText(present, "x");
                var missing = BuildProject(notePath: MissingNote);
                var presentProj = BuildProject(notePath: present);

                var missingList = ProjectMissingFiles.EnumerateMissing(missing);
                Assert.Contains(missingList, f => f.Kind == MissingFileKind.Note && f.Path == MissingNote);

                Assert.Empty(ProjectMissingFiles.EnumerateMissing(presentProj));
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                try { File.Delete(present); } catch { }
            }
        }

        [Fact]
        public void EnumerateMissing_SavedMidi_uses_midi_output_only()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProject(notePath: MissingNote);
                project.ImportOptions.SavedMidi = true;
                project.ImportOptions.MidiOutputPath = MissingNote.Replace(".mid", "-saved.mid");

                var list = ProjectMissingFiles.EnumerateMissing(project);
                Assert.Single(list);
                Assert.Equal(MissingFileKind.Note, list[0].Kind);
                Assert.Equal(project.ImportOptions.MidiOutputPath, list[0].Path);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void EnumerateMissing_skips_track_wavs_when_remuxer_will_regenerate()
        {
            string present = Path.Combine(Path.GetTempPath(), "vm-mf-" + Guid.NewGuid().ToString("N") + ".mod");
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                File.WriteAllText(present, "x");
                var project = BuildProject(FileType.Mod, present);
                project.ImportOptions.TrackAudio = true;
                project.TrackViews[1].TrackProps.AudioProps.Filename = MissingAudio;

                Assert.Empty(ProjectMissingFiles.EnumerateMissing(project));
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                try { File.Delete(present); } catch { }
            }
        }

        [Fact]
        public void EnumerateMissing_prompts_track_wavs_when_note_is_missing()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProject(FileType.Mod, MissingNote);
                project.ImportOptions.TrackAudio = true;
                project.TrackViews[1].TrackProps.AudioProps.Filename = MissingAudio;

                var list = ProjectMissingFiles.EnumerateMissing(project);
                Assert.Contains(list, f => f.Kind == MissingFileKind.Note);
                Assert.Contains(list, f => f.Kind == MissingFileKind.TrackAudio && f.Path == MissingAudio);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void EnumerateMissing_dedupes_shared_paths()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = EmptySong(3);
                var project = new Project();
                project.Notes = song;
                project.TrackViews = new List<TrackView>
                {
                    new TrackView(0, 3, song),
                    new TrackView(1, 3, song),
                    new TrackView(2, 3, song),
                };
                project.TrackViews[1].TrackProps.GlobalProps = project.TrackViews[0].TrackProps;
                project.TrackViews[2].TrackProps.GlobalProps = project.TrackViews[0].TrackProps;
                project.ImportOptions = new ImportOptions(FileType.Midi) { RawNotePath = MissingNote };
                project.TrackViews[1].TrackProps.AudioProps.Filename = MissingAudio;
                project.TrackViews[2].TrackProps.AudioProps.Filename = MissingAudio;

                var list = ProjectMissingFiles.EnumerateMissing(project);
                Assert.Single(list, f => f.Kind == MissingFileKind.TrackAudio);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void FindFileByName_finds_nested_match()
        {
            string root = Path.Combine(Path.GetTempPath(), "vm-mf-search-" + Guid.NewGuid().ToString("N"));
            try
            {
                string nested = Path.Combine(root, "sub", "song.mid");
                Directory.CreateDirectory(Path.GetDirectoryName(nested));
                File.WriteAllText(nested, "x");

                string found = ProjectMissingFiles.FindFileByName(root, "song.mid");
                Assert.Equal(nested, found);
                Assert.Null(ProjectMissingFiles.FindFileByName(root, "nope.mid"));
            }
            finally
            {
                try { Directory.Delete(root, recursive: true); } catch { }
            }
        }

        [Fact]
        public void FindFileByName_prefers_matching_path_suffix()
        {
            string root = Path.Combine(Path.GetTempPath(), "vm-mf-search-" + Guid.NewGuid().ToString("N"));
            try
            {
                string shallow = Path.Combine(root, "song.mid");
                string nested = Path.Combine(root, "nested", "song.mid");
                Directory.CreateDirectory(Path.GetDirectoryName(nested));
                File.WriteAllText(shallow, "x");
                File.WriteAllText(nested, "x");

                string found = ProjectMissingFiles.FindFileByName(root, "song.mid",
                    originalPath: @"D:\old\songs\nested\song.mid");
                Assert.Equal(nested, found);

                string shortest = ProjectMissingFiles.FindFileByName(root, "song.mid",
                    originalPath: @"D:\song.mid");
                Assert.Equal(shallow, shortest);
            }
            finally
            {
                try { Directory.Delete(root, recursive: true); } catch { }
            }
        }

        [Fact]
        public void Resolve_replace_updates_note_and_texture_keyframe()
        {
            string replacement = Path.Combine(Path.GetTempPath(), "vm-mf-" + Guid.NewGuid().ToString("N") + ".mid");
            string texReplacement = Path.Combine(Path.GetTempPath(), "vm-mf-" + Guid.NewGuid().ToString("N") + ".png");
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                File.WriteAllText(replacement, "x");
                File.WriteAllText(texReplacement, "x");

                var project = BuildProject();
                project.TrackViews[1].TrackProps.MaterialProps.TexProps.Path = MissingTex;
                project.PropertyKeyframes.Add("track/1/TexturePath", 0, KfInterpolation.Hold,
                    new StringKfValue(MissingTex));

                var prompt = new ScriptedPrompt();
                prompt.Actions.Enqueue(MissingFileAction.Browse);
                prompt.Replacements.Enqueue(replacement);
                prompt.Actions.Enqueue(MissingFileAction.Browse);
                prompt.Replacements.Enqueue(texReplacement);

                var result = ProjectMissingFiles.Resolve(project, prompt);

                Assert.False(result.Aborted);
                Assert.True(result.ReplacedAny);
                Assert.Equal(replacement, project.ImportOptions.RawNotePath);
                Assert.Equal(texReplacement, project.TrackViews[1].TrackProps.MaterialProps.TexProps.Path);
                var kf = project.PropertyKeyframes.Tracks["track/1/TexturePath"].FindBrackets(0).Before;
                Assert.Equal(texReplacement, ((StringKfValue)kf.Value).S);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                try { File.Delete(replacement); } catch { }
                try { File.Delete(texReplacement); } catch { }
            }
        }

        [Fact]
        public void Resolve_ignore_sets_skip_flags_and_continues()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProject();
                project.ImportOptions.AudioPath = MissingAudio;

                var prompt = new ScriptedPrompt();
                prompt.Actions.Enqueue(MissingFileAction.Ignore);
                prompt.Actions.Enqueue(MissingFileAction.Ignore);

                var result = ProjectMissingFiles.Resolve(project, prompt);

                Assert.False(result.Aborted);
                Assert.False(result.ReplacedAny);
                Assert.True(project.ImportOptions.SkipNoteLoad);
                Assert.True(project.ImportOptions.SkipAudioLoad);
                Assert.Equal(MissingNote, project.ImportOptions.RawNotePath);
                Assert.Equal(MissingAudio, project.ImportOptions.AudioPath);
                Assert.Equal(2, prompt.Prompted.Count);
                project.ImportOptions.CheckSourceFile();
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void Resolve_abort_stops_without_changing_paths()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProject();
                var prompt = new ScriptedPrompt();
                prompt.Actions.Enqueue(MissingFileAction.Abort);

                var result = ProjectMissingFiles.Resolve(project, prompt);

                Assert.True(result.Aborted);
                Assert.False(result.ReplacedAny);
                Assert.Equal(MissingNote, project.ImportOptions.RawNotePath);
                Assert.False(project.ImportOptions.SkipNoteLoad);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void Resolve_search_folder_replaces_when_found()
        {
            string root = Path.Combine(Path.GetTempPath(), "vm-mf-search-" + Guid.NewGuid().ToString("N"));
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                string found = Path.Combine(root, "does-not-exist.mid");
                Directory.CreateDirectory(root);
                File.WriteAllText(found, "x");

                var project = BuildProject();
                var prompt = new ScriptedPrompt { Folder = root };
                prompt.Actions.Enqueue(MissingFileAction.SearchFolder);

                var result = ProjectMissingFiles.Resolve(project, prompt);

                Assert.False(result.Aborted);
                Assert.True(result.ReplacedAny);
                Assert.Equal(found, project.ImportOptions.RawNotePath);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                try { Directory.Delete(root, recursive: true); } catch { }
            }
        }

        [Fact]
        public void Resolve_search_folder_not_found_reprompts()
        {
            string root = Path.Combine(Path.GetTempPath(), "vm-mf-empty-" + Guid.NewGuid().ToString("N"));
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                Directory.CreateDirectory(root);
                var project = BuildProject();
                var prompt = new ScriptedPrompt { Folder = root };
                prompt.Actions.Enqueue(MissingFileAction.SearchFolder);
                prompt.Actions.Enqueue(MissingFileAction.Abort);

                var result = ProjectMissingFiles.Resolve(project, prompt);

                Assert.True(result.Aborted);
                Assert.Equal(1, prompt.NotFoundCount);
                Assert.Equal(2, prompt.Prompted.Count);
                Assert.Equal(MissingNote, project.ImportOptions.RawNotePath);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                try { Directory.Delete(root, recursive: true); } catch { }
            }
        }

        [Fact]
        public void CheckSourceFile_still_throws_for_fresh_import()
        {
            var io = new ImportOptions(FileType.Midi);
            io.ReplaceNotePath(MissingNote);
            var ex = Assert.Throws<FileImportException>(() => io.CheckSourceFile());
            Assert.Equal(ImportError.Missing, ex.Error);
            Assert.Equal(ImportFileType.Note, ex.FileType);
        }

        sealed class ScriptedPrompt : IMissingFilePrompt
        {
            public Queue<MissingFileAction> Actions { get; } = new();
            public Queue<string> Replacements { get; } = new();
            public string Folder { get; set; }
            public List<string> Prompted { get; } = new();
            public int NotFoundCount { get; private set; }

            public MissingFileAction Prompt(MissingFileRef file)
            {
                Prompted.Add(file.Path);
                return Actions.Count > 0 ? Actions.Dequeue() : MissingFileAction.Abort;
            }

            public string PickReplacementFile(MissingFileRef file)
                => Replacements.Count > 0 ? Replacements.Dequeue() : null;

            public string PickSearchFolder(MissingFileRef file) => Folder;

            public void NotifyNotFound(string fileName, string directory) => NotFoundCount++;
        }
    }
}
