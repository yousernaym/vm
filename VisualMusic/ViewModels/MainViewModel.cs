using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using VisualMusic.Controls;

namespace VisualMusic.ViewModels
{
    public enum AppScreen { Song, ModBrowser, SidBrowser, MidiBrowser }

    public partial class MainViewModel : ObservableObject, IImportService
    {
        // ---- Window title / screen ----

        [ObservableProperty]
        private string windowTitle = Program.AppName;

        [ObservableProperty]
        private AppScreen currentScreen = AppScreen.Song;

        // ---- Panel toggles ----

        [ObservableProperty] bool showSongProps;
        [ObservableProperty] bool showTrackProps;

        // ---- Child view models ----

        public TrackListViewModel TrackList { get; } = new();
        public TrackPropsViewModel SelectedTrackProps { get; } = new();
        public SongPropsViewModel SongProps { get; } = new();

        // ---- Project state ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasProject))]
        [NotifyPropertyChangedFor(nameof(HasAudio))]
        [NotifyCanExecuteChangedFor(nameof(SaveProjectCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveProjectAsCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExportVideoCommand))]
        [NotifyCanExecuteChangedFor(nameof(TogglePlaybackCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToBeginningCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToEndCommand))]
        [NotifyCanExecuteChangedFor(nameof(NudgeBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(NudgeForwardCommand))]
        [NotifyCanExecuteChangedFor(nameof(JumpBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(JumpForwardCommand))]
        [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
        [NotifyCanExecuteChangedFor(nameof(RedoCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetCameraCommand))]
        [NotifyCanExecuteChangedFor(nameof(LoadCameraCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveCameraCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertLyricsCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertKeyFrameCommand))]
        [NotifyCanExecuteChangedFor(nameof(LoadTrackPropsCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveTrackPropsCommand))]
        private Project project;

        public bool HasProject => project != null;
        public bool HasAudio => project != null && Media.getAudioLength() > 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UndoMenuHeader))]
        private string undoDescription = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RedoMenuHeader))]
        private string redoDescription = "";

        public string UndoMenuHeader => string.IsNullOrEmpty(UndoDescription) ? "Undo" : $"Undo {UndoDescription}";
        public string RedoMenuHeader => string.IsNullOrEmpty(RedoDescription) ? "Redo" : $"Redo {RedoDescription}";

        string currentProjectPath = "";
        UndoItems undoItems = new UndoItems();

        public MainViewModel()
        {
            TrackList.SelectionChanged += OnTrackListSelectionChanged;
            WireTrackPropsCallbacks();
        }

        void WireTrackPropsCallbacks()
        {
            SelectedTrackProps.ApplyToSelected = fn =>
            {
                foreach (var item in TrackList.SelectedItems)
                    fn(item.TrackView.TrackProps);
                // Trigger rendering refresh — the draw host invalidates on next frame tick.
            };

            SelectedTrackProps.ApplyAndRebuild = fn =>
            {
                foreach (var item in TrackList.SelectedItems)
                    fn(item.TrackView.TrackProps);
                project?.createOcTrees();
            };

            SelectedTrackProps.LoadTexture = path =>
            {
                if (project == null) return;
                var drawHost = GetDrawHost?.Invoke();
                if (drawHost == null) return;
                try
                {
                    foreach (var item in TrackList.SelectedItems)
                        item.TrackView.TrackProps.MaterialProps.TexProps.loadTexture(path, drawHost);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                project.createOcTrees();
                OnTrackListSelectionChanged();
            };

            SelectedTrackProps.UnloadTexture = () =>
            {
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.MaterialProps.TexProps.unloadTexture();
                project?.createOcTrees();
                OnTrackListSelectionChanged();
            };

            SelectedTrackProps.BrowseAudioFile = () =>
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Audio files|*.wav;*.flac;*.mp3;*.ogg|All files|*.*"
                };
                var audioDir = AppSettings.Instance.TrackAudioFolder;
                if (!string.IsNullOrEmpty(audioDir)) dlg.InitialDirectory = audioDir;
                if (dlg.ShowDialog() != true) return;
                AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.TrackAudioFolder = dir);
                SelectedTrackProps.AudioFilename = dlg.FileName;
            };
        }

        void OnTrackListSelectionChanged()
        {
            if (project == null) return;
            var indices = TrackList.SelectedItems
                .Select(item => TrackList.Items.IndexOf(item))
                .Where(i => i >= 0);
            SelectedTrackProps.MergedProps = project.mergeTrackProps(indices);
        }

        // ---- Scrollbar binding ----

        public double ScrollPosition
        {
            get => project?.NormSongPos ?? 0;
            set { if (project != null) project.NormSongPos = value; }
        }

        public void NotifyScrollPositionChanged() => OnPropertyChanged(nameof(ScrollPosition));

        partial void OnProjectChanged(Project value)
        {
            ShowSongProps = false;
            ShowTrackProps = false;
            TrackList.Rebuild(value);
            SelectedTrackProps.MergedProps = null;
            NotifyScrollPositionChanged();
        }

        // ---- Renderer callbacks (set by MainWindow after load) ----

        /// <summary>Called after a project is fully loaded; receiver must set the project on the renderer.</summary>
        public Action<Project> OnProjectLoaded { get; set; }
        /// <summary>Called to load the background image on the renderer.</summary>
        public Action<string> OnLoadBackgroundImage { get; set; }
        /// <summary>Returns the active draw host (SongRenderer) for wiring up Project.SetDrawHost.</summary>
        public Func<ISongDrawHost> GetDrawHost { get; set; }

        // ---- Screen commands ----

        [RelayCommand] void ShowSong() => CurrentScreen = AppScreen.Song;
        [RelayCommand] void ShowModBrowser() => CurrentScreen = AppScreen.ModBrowser;
        [RelayCommand] void ShowSidBrowser() => CurrentScreen = AppScreen.SidBrowser;
        [RelayCommand] void ShowMidiBrowser() => CurrentScreen = AppScreen.MidiBrowser;

        // ---- File commands ----

        [RelayCommand]
        async Task OpenProject()
        {
            var dlg = new OpenFileDialog
            {
                Filter = $"Visual Music projects|*.{Project.DefaultFileExt}|All files|*.*",
                InitialDirectory = AppSettings.Instance.ProjectFolderOrDefault
            };
            if (dlg.ShowDialog() != true) return;

            string path = dlg.FileName;
            AppSettings.Instance.RememberFolder(path, dir => AppSettings.Instance.ProjectFolder = dir);

            Project tempProject;
            var dcs = new DataContractSerializer(typeof(Project), ProjectSerializer.KnownTypes);
            try
            {
                using var stream = File.Open(path, FileMode.Open);
                tempProject = (Project)dcs.ReadObject(stream);
            }
            catch (SerializationException e)
            {
                MessageBox.Show("Invalid project file.\n" + e.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (IOException e)
            {
                MessageBox.Show("Could not open file.\n" + e.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Set static references before loadContent so NoteStyle.createGeoChunk and
            // Project.drawSong() (both called from inside loadContent) can access the renderer.
            var drawHost = GetDrawHost?.Invoke();
            if (drawHost != null) Project.SetDrawHost(drawHost);
            NoteStyle.SetProject(tempProject);

            try
            {
                await tempProject.loadContent();
            }
            catch (FileImportException ex)
            {
                NoteStyle.SetProject(null);
                MessageBox.Show($"Could not load project file: {ex.Message}\n\nMissing file: {ex.FileName}",
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                NoteStyle.SetProject(null);
                MessageBox.Show("Error loading project: " + ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var rendererWfp = GetRendererWaveformPanel?.Invoke();
            tempProject.InitAfterDeserialization(rendererWfp);

            // Pre-fill the import dialog with this project's saved import options so that
            // opening the dialog after loading a project shows the correct file paths and settings.
            var io = tempProject.ImportOptions;
            if (io != null)
            {
                // Only show the audio path if it was an explicit user-provided file, not an internal mixdown.
                string audioPath = io.MixdownType == Midi.MixdownType.None ? (io.AudioPath ?? "") : "";
                ImportSongWindow.UpdateSession(io.NoteFileType, erase: true,
                    notePath: io.RawNotePath ?? "", audioPath: audioPath, insTrack: io.InsTrack);
            }

            currentProjectPath = path;
            Project = tempProject;
            tempProject.DefaultFileName = Path.GetFileName(path);
            OnProjectLoaded?.Invoke(tempProject);
            OnLoadBackgroundImage?.Invoke(tempProject.Props.BackgroundImagePath);

            undoItems.clear();
            undoItems.Add("", tempProject);
            UpdateUndoRedo();
            WindowTitle = $"{Program.AppName} — {Path.GetFileName(path)}";
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveProject()
        {
            if (string.IsNullOrEmpty(currentProjectPath)) { SaveProjectAs(); return; }
            SaveToPath(currentProjectPath);
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveProjectAs()
        {
            var dlg = new SaveFileDialog
            {
                Filter = $"Visual Music projects|*.{Project.DefaultFileExt}|All files|*.*",
                InitialDirectory = AppSettings.Instance.ProjectFolderOrDefault,
                FileName = project.DefaultFileName
            };
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.ProjectFolder = dir);
            currentProjectPath = dlg.FileName;
            SaveToPath(currentProjectPath);
        }

        void SaveToPath(string path)
        {
            try
            {
                var dcs = new DataContractSerializer(typeof(Project), ProjectSerializer.KnownTypes);
                string tmp = Path.Combine(Program.TempDir, "tempprojectfile");
                using (var stream = File.Open(tmp, FileMode.Create))
                    dcs.WriteObject(stream, project);
                File.Copy(tmp, path, true);
                WindowTitle = $"{Program.AppName} — {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        async Task ImportMidi()
        {
            var dlg = new ImportSongWindow(Midi.FileType.Midi) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;
            await DoImport(BuildOptions(Midi.FileType.Midi, dlg));
        }

        [RelayCommand]
        async Task ImportMod()
        {
            var dlg = new ImportSongWindow(Midi.FileType.Mod) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;
            await DoImport(BuildOptions(Midi.FileType.Mod, dlg));
        }

        [RelayCommand]
        async Task ImportSid()
        {
            var dlg = new ImportSongWindow(Midi.FileType.Sid) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;

            var options = BuildOptions(Midi.FileType.Sid, dlg);

            // SID files may contain multiple sub-songs — let the user pick.
            if (!string.IsNullOrEmpty(options.NotePath) && File.Exists(options.NotePath))
            {
                var subWin = new SubSongWindow(options.NotePath) { Owner = Application.Current.MainWindow };
                if (subWin.NumSongs > 1 && subWin.ShowDialog() != true) return;
                options.SubSong    = subWin.SelectedSong;
                options.SongLengthS = subWin.SongLengthS;
            }

            await DoImport(options);
        }

        /// <summary>Build an ImportOptions from the dialog fields (WPF path).</summary>
        static ImportOptions BuildOptions(Midi.FileType fileType, ImportSongWindow dlg)
        {
            ImportOptions options;
            switch (fileType)
            {
                case Midi.FileType.Midi: options = new MidiImportOptions(); break;
                case Midi.FileType.Mod:  options = new ModImportOptions();  break;
                default:                 options = new SidImportOptions();  break;
            }

            options.RawNotePath  = dlg.NoteFilePath;
            try { options.setNotePath(); } catch (FileImportException) { }

            if (!string.IsNullOrWhiteSpace(dlg.AudioFilePath))
            {
                options.AudioPath   = dlg.AudioFilePath;
                options.MixdownType = Midi.MixdownType.None;
            }

            options.EraseCurrent = dlg.EraseCurrent;
            options.InsTrack     = dlg.InsTrack;
            return options;
        }

        /// <summary>Shared post-dialog import logic.</summary>
        async Task DoImport(ImportOptions options)
        {
            // Ensure a project object exists to import into.
            if (project == null)
            {
                var fresh = new Project();
                NoteStyle.SetProject(fresh);
                Project = fresh;
            }

            var drawHost = GetDrawHost?.Invoke();
            if (drawHost != null) Project.SetDrawHost(drawHost);
            NoteStyle.SetProject(project);

            try { options.CheckSourceFile(); }
            catch (FileImportException ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.FileName}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (!await project.ImportSong(options, null)) return;
            }
            catch (FileImportException ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.FileName}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import failed: " + ex.Message, Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (options.EraseCurrent)
                currentProjectPath = "";

            var wfp = GetRendererWaveformPanel?.Invoke();
            project.InitAfterDeserialization(wfp);

            TrackList.Rebuild(project);
            OnProjectLoaded?.Invoke(project);
            OnLoadBackgroundImage?.Invoke(project.Props.BackgroundImagePath);

            // Refresh audio-dependent CanExecute (HasAudio depends on Media.getAudioLength()).
            OnPropertyChanged(nameof(HasAudio));
            TogglePlaybackCommand.NotifyCanExecuteChanged();
            GoToBeginningCommand.NotifyCanExecuteChanged();
            GoToEndCommand.NotifyCanExecuteChanged();
            NudgeBackCommand.NotifyCanExecuteChanged();
            NudgeForwardCommand.NotifyCanExecuteChanged();
            JumpBackCommand.NotifyCanExecuteChanged();
            JumpForwardCommand.NotifyCanExecuteChanged();

            undoItems.clear();
            undoItems.Add("", project);
            UpdateUndoRedo();

            string name = Path.GetFileName(options.RawNotePath ?? options.NotePath ?? "");
            WindowTitle = $"{Program.AppName} — {name}";
            CurrentScreen = AppScreen.Song;
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void ExportVideo()
            => MessageBox.Show("Export Video not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand]
        void TpartyIntegration()
            => MessageBox.Show("Third-party integration not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        // ---- Playback commands ----

        [RelayCommand(CanExecute = nameof(HasProject))]
        void TogglePlayback() => project?.togglePlayback();

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToBeginning() => project?.stopPlayback();

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToEnd()
        {
            if (project == null) return;
            if (project.IsPlaying) project.togglePlayback();
            project.NormSongPos = 1;
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void NudgeBack()
        {
            if (project == null) return;
            project.NudgeSongPos(SongRenderer.SmallScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void NudgeForward()
        {
            if (project == null) return;
            project.NudgeSongPos(-SongRenderer.SmallScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void JumpBack()
        {
            if (project == null) return;
            project.NudgeSongPos(SongRenderer.LargeScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void JumpForward()
        {
            if (project == null) return;
            project.NudgeSongPos(-SongRenderer.LargeScrollStep);
            ResyncPlaybackPosition();
        }

        void ResyncPlaybackPosition()
        {
            // When playing, restart audio at the new position (same as WinForms logic)
            if (project?.IsPlaying == true)
            {
                project.togglePlayback();
                project.togglePlayback();
            }
        }

        // ---- Edit commands ----

        [RelayCommand(CanExecute = nameof(CanUndo))]
        void Undo()
        {
            undoItems--;
            ApplyUndoItem();
            UpdateUndoRedo();
        }

        [RelayCommand(CanExecute = nameof(CanRedo))]
        void Redo()
        {
            undoItems++;
            ApplyUndoItem();
            UpdateUndoRedo();
        }

        public bool CanUndo => undoItems.Previous != null;
        public bool CanRedo => undoItems.Next != null;

        void ApplyUndoItem()
        {
            if (undoItems.Current == null) return;
            project?.copyPropsFrom(undoItems.Current.Project);
        }

        void UpdateUndoRedo()
        {
            UndoDescription = undoItems.UndoDesc;
            RedoDescription = undoItems.RedoDesc;
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        public void AddUndoItem(string desc)
        {
            if (project == null) return;
            undoItems.Add(desc, project);
            UpdateUndoRedo();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void ResetCamera()
        {
            if (project == null) return;
            foreach (var kf in project.KeyFrames.Values)
            {
                if (kf.Selected)
                    kf.ProjProps.Camera = new Camera();
            }
            project.interpolateFrames();
            AddUndoItem("Reset Camera");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void LoadCamera()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Camera files|*.cam|All files|*.*",
                InitialDirectory = AppSettings.Instance.CamFolderOrDefault
            };
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.CamFolder = dir);
            try
            {
                var dcs = new DataContractSerializer(typeof(Camera), ProjectSerializer.KnownTypes);
                using var stream = File.Open(dlg.FileName, FileMode.Open);
                var cam = (Camera)dcs.ReadObject(stream);
                var kf = project?.getKeyFrameAtSongPos();
                if (kf != null)
                    kf.ProjProps.Camera = cam;
                AddUndoItem("Load Camera");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveCamera()
        {
            var dlg = new SaveFileDialog { Filter = "Camera files|*.cam|All files|*.*", InitialDirectory = AppSettings.Instance.CamFolderOrDefault };
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.CamFolder = dir);
            try
            {
                var dcs = new DataContractSerializer(typeof(Camera), ProjectSerializer.KnownTypes);
                using var stream = File.Open(dlg.FileName, FileMode.Create);
                dcs.WriteObject(stream, project?.getKeyFrameAtSongPos()?.ProjProps.Camera);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void LoadTrackProps()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Track property files|*.tp|All files|*.*",
                InitialDirectory = AppSettings.Instance.TrackPropsFolderOrDefault
            };
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.TrackPropsFolder = dir);

            TrackProps props;
            var dcs = new DataContractSerializer(typeof(TrackProps), ProjectSerializer.KnownTypes);
            try
            {
                using var stream = File.Open(dlg.FileName, FileMode.Open);
                props = (TrackProps)dcs.ReadObject(stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Apply to the global (index 0) track — full track-list editing comes in a later phase.
            project.TrackViews[0].TrackProps.cloneFrom(props, (int)TrackPropsType.TPT_All);
            if ((props.TypeFlags & (int)TrackPropsType.TPT_Style) != 0)
                project.createOcTrees();
            AddUndoItem("Load Track Properties");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveTrackProps()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Track property files|*.tp|All files|*.*",
                InitialDirectory = AppSettings.Instance.TrackPropsFolderOrDefault,
                FileName = "track.tp"
            };
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.TrackPropsFolder = dir);

            TrackProps props = project.TrackViews[0].TrackProps;
            props.TypeFlags = (int)TrackPropsType.TPT_All;
            var dcs = new DataContractSerializer(typeof(TrackProps), ProjectSerializer.KnownTypes);
            try
            {
                using var stream = File.Open(dlg.FileName, FileMode.Create);
                dcs.WriteObject(stream, props);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void DefaultTrackProps()
        {
            foreach (var tv in project.TrackViews)
                tv.TrackProps.ResetProps();
            project.createOcTrees();
            AddUndoItem("Default Track Properties");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void InsertLyrics()
        {
            project.insertLyrics();
            AddUndoItem("Insert Lyrics");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void InsertKeyFrame()
        {
            int row = project.insertKeyFrameAtSongPos();
            if (row < 0)
            {
                MessageBox.Show("A key frame already exists at this position.", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            AddUndoItem("Insert Key Frame");
        }

        // ---- Renderer WaveformPanel accessor (set by MainWindow) ----

        public Func<WaveformPanel> GetRendererWaveformPanel { get; set; }

        // ---- IImportService (browser download import) ----

        public void ImportFromUrl(string url, string suggestedFileName)
        {
            string ext = suggestedFileName.Split('.').Last().ToLower();

            Midi.FileType? fileType = null;
            if (ImportMidiForm.Formats.Contains(ext))      fileType = Midi.FileType.Midi;
            else if (ImportModForm.Formats.Contains(ext))  fileType = Midi.FileType.Mod;
            else if (ImportSidForm.Formats.Contains(ext))  fileType = Midi.FileType.Sid;

            if (fileType == null)
            {
                MessageBox.Show(
                    $"Unrecognised file type: .{ext}\n\nFile: {suggestedFileName}",
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Browser imports skip the dialog: always erase the current project, use no audio file,
            // and use the saved per-type track-split preference. The note path is the original URL;
            // setNotePath() downloads it to a temp file — the same path the dialog takes when a URL
            // is typed into the note field, so re-importing the URL later behaves identically.
            ImportOptions options;
            switch (fileType.Value)
            {
                case Midi.FileType.Midi: options = new MidiImportOptions(); break;
                case Midi.FileType.Mod:  options = new ModImportOptions();  break;
                default:                 options = new SidImportOptions();  break;
            }
            options.RawNotePath  = url;       // preserve original URL for project save and dialog display
            try { options.setNotePath(); }    // downloads the URL to a temp file (WebClient)
            catch (FileImportException)
            {
                MessageBox.Show("Download failed: " + url, Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            options.EraseCurrent = true;
            options.InsTrack     = AppSettings.Instance.GetInsTrack(fileType.Value);
            ImportSongWindow.UpdateSession(fileType.Value, erase: true, notePath: url, audioPath: "",
                insTrack: AppSettings.Instance.GetInsTrack(fileType.Value));
            _ = DoImport(options);   // fire-and-forget on the UI thread (async void pattern)
        }
    }
}
