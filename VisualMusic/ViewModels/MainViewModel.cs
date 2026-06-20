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
        private string _windowTitle = Program.AppName;

        [ObservableProperty]
        private AppScreen _currentScreen = AppScreen.Song;

        // ---- Panel toggles ----

        [ObservableProperty] bool _showSongProps;
        [ObservableProperty] bool _showTrackProps;

        // ---- Camera mouse-look mode ----

        /// <summary>True while mouse-look mode is active; drives the yellow "MOUSE LOOK" label.</summary>
        [ObservableProperty] bool _isMouseLookMode;

        // ---- Child view models ----

        public TrackListViewModel TrackList { get; } = new();
        public TrackPropsViewModel SelectedTrackProps { get; } = new();
        public SongPropsViewModel SongProps { get; } = new();

        // ---- Project state ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasProject))]
        [NotifyPropertyChangedFor(nameof(HasAudio))]
        [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
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
        [NotifyCanExecuteChangedFor(nameof(LoadTrackPropsCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveTrackPropsCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToNextKeyFrameCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToPrevKeyFrameCommand))]
        private Project _project;

        public bool HasProject => _project != null;
        public bool HasAudio => _project != null && Media.GetAudioLength() > 0;
        public bool HasUnsavedChanges => HasProject && _undoItems.Current != null && !_undoItems.IsCurrentSaved;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UndoMenuHeader))]
        private string _undoDescription = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RedoMenuHeader))]
        private string _redoDescription = "";

        public string UndoMenuHeader => string.IsNullOrEmpty(UndoDescription) ? "Undo" : $"Undo {UndoDescription}";
        public string RedoMenuHeader => string.IsNullOrEmpty(RedoDescription) ? "Redo" : $"Redo {RedoDescription}";

        string _currentProjectPath = "";
        UndoItems _undoItems = new UndoItems();
        List<int> _lastSelectedIndices = new List<int>();

        public MainViewModel()
        {
            TrackList.SelectionChanged += OnTrackListSelectionChanged;
            WireTrackPropsCallbacks();
            WireSongPropsCallbacks();
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
                _project?.CreateGeos();
            };

            SelectedTrackProps.RefreshTrackColors = TrackList.RefreshColors;

            SelectedTrackProps.LoadTexture = path =>
            {
                if (_project == null) return;
                var drawHost = GetDrawHost?.Invoke();
                if (drawHost == null) return;
                if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("TexturePath",
                    Keyframes.KeyframeService.KfScope.Track))
                    return;

                try
                {
                    foreach (var item in TrackList.SelectedItems)
                        item.TrackView.TrackProps.MaterialProps.TexProps.LoadTexture(path, drawHost);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _project.CreateGeos();
                Keyframes.KeyframeService.SyncEditedValue("TexturePath",
                    Keyframes.KeyframeService.KfScope.Track,
                    new Keyframes.StringKfValue(path));
                OnTrackListSelectionChanged();
                AddUndoItem("Load texture");
            };

            SelectedTrackProps.UnloadTexture = () =>
            {
                if (_project == null) return;
                if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("TexturePath",
                    Keyframes.KeyframeService.KfScope.Track))
                    return;

                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.MaterialProps.TexProps.UnloadTexture();
                _project.CreateGeos();
                Keyframes.KeyframeService.SyncEditedValue("TexturePath",
                    Keyframes.KeyframeService.KfScope.Track,
                    new Keyframes.StringKfValue(""));
                OnTrackListSelectionChanged();
                AddUndoItem("Unload texture");
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
                AddUndoItem("Set track audio");
            };

            SelectedTrackProps.LoadSelectedTracksAudio = async () =>
            {
                var selectedItems = TrackList.SelectedItems.ToList();
                await Task.WhenAll(
                    selectedItems.Select(item => item.TrackView.TrackProps.AudioProps.LoadAudioAsync()));
                var failedChannel = selectedItems
                    .Select(item => item.TrackView.TrackProps.AudioProps.SidWizChannel)
                    .FirstOrDefault(ch => !string.IsNullOrEmpty(ch.ErrorMessage));
                if (failedChannel != null)
                    MessageBox.Show($"Couldn't load audio file:\n{failedChannel.Filename}", Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
            };

            SelectedTrackProps.ResetStyle = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmDefaultStyleReset(out var affected))
                    return;
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ResetStyle();
                Keyframes.KeyframeService.PurgeModKeyframesForSelectedTracks();
                Keyframes.KeyframeService.CaptureDefaultStyleAtCurrentTick(affected);
                _project?.CreateGeos();
                OnTrackListSelectionChanged();
                AddUndoItem("Reset style");
            };

            SelectedTrackProps.ResetMaterial = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmDefaultMaterialReset(out var affected))
                    return;
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ResetMaterial();
                Keyframes.KeyframeService.CaptureDefaultMaterialAtCurrentTick(affected);
                _project?.CreateGeos();
                OnTrackListSelectionChanged();
                AddUndoItem("Reset material");
            };

            SelectedTrackProps.ResetLight = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmDefaultLightReset(out var affected))
                    return;
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ResetLight();
                Keyframes.KeyframeService.CaptureDefaultLightAtCurrentTick(affected);
                OnTrackListSelectionChanged();
                AddUndoItem("Reset light");
            };

            SelectedTrackProps.ResetSpatial = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmDefaultSpatialReset(out var affected))
                    return;
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ResetSpatial();
                Keyframes.KeyframeService.CaptureDefaultSpatialAtCurrentTick(affected);
                OnTrackListSelectionChanged();
                AddUndoItem("Reset spatial");
            };

            SelectedTrackProps.AddModEntry = () =>
            {
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ActiveNoteStyle?.AddModEntry(true);
                OnTrackListSelectionChanged();
                AddUndoItem("Add modulation");
            };

            SelectedTrackProps.CloneModEntry = () =>
            {
                foreach (var item in TrackList.SelectedItems)
                {
                    var ns = item.TrackView.TrackProps.ActiveNoteStyle;
                    if (ns?.SelectedModEntryIndex >= 0 && ns.ModEntries?.Count > 0)
                        ns.CloneModEntry(true);
                }
                OnTrackListSelectionChanged();
                AddUndoItem("Clone modulation");
            };

            SelectedTrackProps.DeleteModEntry = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmModEntryDelete())
                    return;
                foreach (var item in TrackList.SelectedItems)
                {
                    var ns = item.TrackView.TrackProps.ActiveNoteStyle;
                    if (ns?.SelectedModEntryIndex >= 0 && ns.ModEntries?.Count > 0)
                    {
                        // Capture the stable ids before deleting so we can purge the orphaned keyframes.
                        int tn = item.TrackView.TrackNumber;
                        string eid = ns.SelectedModEntry?.Id;
                        ns.DeleteModEntry();
                        if (eid != null)
                            Keyframes.KeyframeService.RemoveKeyframesWithPrefix($"track/{tn}/mod/{eid}/");
                    }
                }
                OnTrackListSelectionChanged();
                AddUndoItem("Delete modulation");
            };

            SelectedTrackProps.SelectModEntry = idx =>
            {
                if (idx < 0) return;   // guard spurious -1 writes (belt-and-suspenders)
                foreach (var item in TrackList.SelectedItems)
                {
                    var ns = item.TrackView.TrackProps.ActiveNoteStyle;
                    if (ns != null) ns.SelectedModEntryIndex = idx;
                }
                OnTrackListSelectionChanged();
            };

            // Ctrl+drag-drop: copy the currently-open tab's props from the drop-target (source)
            // to all dragged items (destinations). Visual tabs only (Style/Material/Light/Spatial).
            TrackList.CopyTabPropsToDropped = (sourceItem, destItems) =>
            {
                if (_project == null || sourceItem == null) return;
                int tabIndex = SelectedTrackProps.SelectedTabIndex;   // 0=Style…3=Spatial, 4=Audio
                if (tabIndex < 0 || tabIndex > 3) return;             // Audio tab = no-op
                int flag = 1 << tabIndex;
                var drawHost = GetDrawHost?.Invoke();
                var source = sourceItem.TrackView.TrackProps;

                bool changed = false;
                foreach (var item in destItems)
                {
                    if (item == sourceItem || TrackList.Items.IndexOf(item) <= 0) continue;
                    item.TrackView.TrackProps.CloneFrom(source, flag, drawHost);
                    changed = true;
                }
                if (!changed) return;

                if ((flag & ((int)TrackPropsType.TPT_Style | (int)TrackPropsType.TPT_Material)) != 0)
                    _project.CreateGeos();   // geometry/texture coords are baked per track
                if (flag == (int)TrackPropsType.TPT_Material)
                    TrackList.RefreshColors(); // update the two color swatches in the list
                OnTrackListSelectionChanged(); // refresh the tabs for the (still-selected) dragged tracks
                AddUndoItem("Copy Track Properties");
            };
        }

        void OnTrackListSelectionChanged()
        {
            if (_project == null) return;
            // List positions for MergeTrackProps (which indexes TrackViews[] by position).
            var indices = TrackList.SelectedItems
                .Select(item => TrackList.Items.IndexOf(item))
                .Where(i => i >= 0)
                .ToList();
            _lastSelectedIndices = indices;
            SelectedTrackProps.MergedProps = _project.MergeTrackProps(indices);
            // TrackNumbers (stable MIDI track indices) for the keyframe service.
            var trackNumbers = TrackList.SelectedItems
                .Select(item => item.TrackView.TrackNumber)
                .ToList();
            Keyframes.KeyframeService.SelectedTrackIds = trackNumbers;
            Keyframes.KeyframeService.RaiseKeyframesChanged();
        }

        void WireSongPropsCallbacks()
        {
            SongProps.CreateGeos = () => _project?.CreateGeos();

            SongProps.CommitViewWidth = () =>
            {
                _project?.CreateGeos();
            };

            SongProps.ResetPitches = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmPitchReset(out var affected))
                    return;
                _project?.ResetPitchLimits();
                Keyframes.KeyframeService.CapturePitchResetAtCurrentTick(affected);
                _project?.CreateGeos();
                SongProps.RefreshAll();
                AddUndoItem("Reset pitches");
            };

            SongProps.NotesMinPitch = () => _project?.Notes?.MinPitch;
            SongProps.NotesMaxPitch = () => _project?.Notes?.MaxPitch;

            SongProps.SongLengthSWithoutPbOffset = () =>
                _project?.Notes != null
                    ? (double?)_project.TicksToSeconds(_project.Notes.SongLengthT)
                    : null;

            SongProps.BrowseBackground = () =>
            {
                if (_project == null) return;
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff|All files|*.*"
                };
                var bkgDir = AppSettings.Instance.BackgroundFolder;
                if (!string.IsNullOrEmpty(bkgDir)) dlg.InitialDirectory = bkgDir;
                if (dlg.ShowDialog() != true) return;
                AppSettings.Instance.RememberFolder(dlg.FileName,
                    dir => AppSettings.Instance.BackgroundFolder = dir);

                // Gate: if background-path keyframes exist but not at this tick, prompt to create one.
                if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("BackgroundImagePath", Keyframes.KeyframeService.KfScope.Project))
                    return;

                _project.Props.BackgroundImagePath = dlg.FileName;
                OnLoadBackgroundImage?.Invoke(dlg.FileName);

                // Record the chosen path into a keyframe at the current tick (no-op when no keyframes exist).
                Keyframes.KeyframeService.SyncEditedValue("BackgroundImagePath",
                    Keyframes.KeyframeService.KfScope.Project,
                    new Keyframes.StringKfValue(dlg.FileName));
                AddUndoItem("Load background");
            };

            SongProps.UnloadBackground = () =>
            {
                if (_project == null) return;

                // Gate: if background-path keyframes exist but not at this tick, prompt to create one.
                if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("BackgroundImagePath", Keyframes.KeyframeService.KfScope.Project))
                    return;

                _project.Props.BackgroundImagePath = "";
                OnUnloadBackgroundImage?.Invoke();

                // Record the empty path into a keyframe at the current tick (no-op when no keyframes exist).
                Keyframes.KeyframeService.SyncEditedValue("BackgroundImagePath",
                    Keyframes.KeyframeService.KfScope.Project,
                    new Keyframes.StringKfValue(""));
                AddUndoItem("Unload background");
            };
        }

        // ---- Scrollbar binding ----

        public double ScrollPosition
        {
            get => _project?.NormSongPos ?? 0;
            set { if (_project != null) _project.NormSongPos = value; }
        }

        public void NotifyScrollPositionChanged()
        {
            OnPropertyChanged(nameof(ScrollPosition));
            // Apply per-property keyframe interpolation for the new position. During playback Project.Update
            // already did this; here it covers paused seeks/scrubs (this method fires on every position
            // change, but NOT during a static control edit — so it never fights an in-progress edit).
            if (_project != null && !_project.IsPlaying)
                _project.InterpolatePropertyKeyframes();
            if (ShowSongProps)
                SongProps.RefreshLiveValues();
            if (ShowTrackProps && _project?.HasTrackKeyframes == true)
            {
                SelectedTrackProps.MergedProps = _project.MergeTrackProps(_lastSelectedIndices);
                SelectedTrackProps.RefreshLiveValues();
            }
            // Drive per-frame refresh for control coloring and the diamond panel
            Keyframes.KeyframeService.RaiseRefresh();
        }

        partial void OnProjectChanged(Project value)
        {
            ShowSongProps = false;
            ShowTrackProps = false;
            SongProps.Project = value;
            TrackList.Rebuild(value);
            // Rebuild selects the global track (index 0), which drives MergedProps via the
            // selection event. Only clear the panel when there is nothing to select — otherwise
            // we'd wipe the global track's props (incl. its modulation selection) right after load.
            if (TrackList.Items.Count == 0)
                SelectedTrackProps.MergedProps = null;
            // Wire the per-property keyframe service to the new project
            Keyframes.KeyframeService.Project = value;
            Keyframes.KeyframeService.RequestUndoSnapshot = AddUndoItem;
            Keyframes.KeyframeService.KeyframesChanged += OnKeyframesChangedRestoreBackground;
            Keyframes.KeyframeService.RaiseKeyframesChanged();
            Camera.OnUserUpdating = () => value?.SyncLiveCameraEdit();
            Camera.OnUserUpdated = () => AddUndoItem("Edit Camera");
            NotifyScrollPositionChanged();
        }

        /// <summary>
        /// When background-image keyframes are removed entirely, restore the renderer to the static
        /// background path so it doesn't stay stuck on the last crossfade state.
        /// </summary>
        void OnKeyframesChangedRestoreBackground()
        {
            if (_project == null) return;
            if (!_project.PropertyKeyframes.HasAny("proj/BackgroundImagePath"))
                OnLoadBackgroundImage?.Invoke(_project.Props.BackgroundImagePath);
        }

        // The track list and property tabs live in a collapsed panel until ShowTrackProps turns on;
        // re-sync the tabs (incl. the modulation ComboBox) to the current selection when it opens.
        partial void OnShowTrackPropsChanged(bool value)
        {
            if (value) OnTrackListSelectionChanged();
        }

        // ---- Renderer callbacks (set by MainWindow after load) ----

        /// <summary>Called after a project is fully loaded; receiver must set the project on the renderer.</summary>
        public Action<Project> OnProjectLoaded { get; set; }
        /// <summary>Called to load the background image on the renderer.</summary>
        public Action<string> OnLoadBackgroundImage { get; set; }
        /// <summary>Called to unload the background image from the renderer.</summary>
        public Action OnUnloadBackgroundImage { get; set; }
        /// <summary>Returns the active draw host (SongRenderer) for wiring up Project.SetDrawHost.</summary>
        public Func<ISongDrawHost> GetDrawHost { get; set; }
        /// <summary>Invokes SongRenderer.renderVideo on the background thread supplied by the caller.</summary>
        public Action<string, IRenderProgressCallback, VideoExportOptions> RenderVideo { get; set; }

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
            if (!ConfirmSaveChangesBefore("opening another project")) return;
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
                await tempProject.LoadContent();
            }
            catch (FileImportException ex)
            {
                // Loading failed (e.g. the referenced song file couldn't be downloaded). The renderer's
                // animation loop is still drawing the previous live project (_project), and its note
                // rendering reads the static NoteStyle.Project — so restore it to _project rather than
                // null, otherwise the next Draw() dereferences a null Project (NoteStyle.DrawTrack).
                NoteStyle.SetProject(_project);
                MessageBox.Show($"Could not load project file: {ex.Message}\n\nMissing file: {ex.FileName}",
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                NoteStyle.SetProject(_project);
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
                ImportSongWindow.UpdateSession(io.NoteFileType, erase: true,
                    notePath: io.RawNotePath ?? "", audioPath: io.AudioPath ?? "", insTrack: io.InsTrack);
            }

            _currentProjectPath = path;
            Project = tempProject;
            tempProject.DefaultFileName = Path.GetFileName(path);
            OnProjectLoaded?.Invoke(tempProject);
            OnLoadBackgroundImage?.Invoke(tempProject.Props.BackgroundImagePath);

            _undoItems.Clear();
            _undoItems.Add("", tempProject);
            _undoItems.MarkSaved();
            UpdateUndoRedo();
            WindowTitle = $"{Program.AppName} — {Path.GetFileName(path)}";
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveProject()
        {
            SaveProjectCore();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveProjectAs()
        {
            SaveProjectAsCore();
        }

        bool SaveProjectCore()
        {
            if (_project == null) return false;
            if (string.IsNullOrEmpty(_currentProjectPath)) return SaveProjectAsCore();
            return SaveToPath(_currentProjectPath);
        }

        bool SaveProjectAsCore()
        {
            var dlg = new SaveFileDialog
            {
                Filter = $"Visual Music projects|*.{Project.DefaultFileExt}|All files|*.*",
                InitialDirectory = AppSettings.Instance.ProjectFolderOrDefault,
                FileName = _project.DefaultFileName
            };
            if (dlg.ShowDialog() != true) return false;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.ProjectFolder = dir);
            SaveSourceFiles(dlg.FileName);   // optional MIDI/WAV export; cancelling it still saves the project
            if (!SaveToPath(dlg.FileName)) return false;
            _currentProjectPath = dlg.FileName;
            _project.DefaultFileName = Path.GetFileName(dlg.FileName);
            return true;
        }

        /// <summary>
        /// For a non-MIDI project, offers to export the converted MIDI / generated WAV alongside the
        /// project (both default off). A saved WAV becomes the project's supplied audio; a saved MIDI
        /// turns this into a MIDI project. This never blocks the save: cancelling the checkbox dialog
        /// (or an individual file picker) just skips the export and the project is still saved.
        /// </summary>
        void SaveSourceFiles(string projectPath)
        {
            var io = _project?.ImportOptions;
            if (io == null || io.NoteFileType == FileType.Midi) return;   // only non-MIDI projects

            bool midiAvail = !string.IsNullOrEmpty(io.MidiOutputPath) && File.Exists(io.MidiOutputPath);
            // Only offer to save the WAV when there's a freshly generated one and the project doesn't
            // already reference a WAV (supplied audio or one saved earlier).
            bool wavAvail = !io.HasSuppliedAudio
                && !string.IsNullOrEmpty(io.GeneratedAudioPath) && File.Exists(io.GeneratedAudioPath);
            if (!midiAvail && !wavAvail) return;

            var dlg = new Controls.SaveSourceFilesWindow(midiAvail, wavAvail) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;   // cancelled — skip export, project is still saved

            string dir = Path.GetDirectoryName(projectPath);
            string name = Path.GetFileNameWithoutExtension(projectPath);

            string savedMidiPath = dlg.SaveMidi
                ? PickAndCopy(io.MidiOutputPath, "MIDI files (*.mid)|*.mid|All files (*.*)|*.*",
                    Path.Combine(dir, name + ".mid"), "Save MIDI file")
                : null;
            string savedWavPath = dlg.SaveWav
                ? PickAndCopy(io.GeneratedAudioPath, "Wave files (*.wav)|*.wav|All files (*.*)|*.*",
                    Path.Combine(dir, name + ".wav"), "Save WAV file")
                : null;

            // Wire the saved WAV as supplied audio (recorded in the project, reused on load).
            if (savedWavPath != null) io.AudioPath = savedWavPath;
            // Saving MIDI turns this into a MIDI project.
            if (savedMidiPath != null) _project.ConvertToMidiProject(savedMidiPath, io.AudioPath);
        }

        /// <summary>
        /// Prompts for a destination with a SaveFileDialog and copies <paramref name="source"/> there.
        /// Returns the chosen path, or null if the user cancelled the picker.
        /// </summary>
        static string PickAndCopy(string source, string filter, string defaultPath, string title)
        {
            var dlg = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                InitialDirectory = Path.GetDirectoryName(defaultPath),
                FileName = Path.GetFileName(defaultPath),
            };
            if (dlg.ShowDialog() != true) return null;
            try
            {
                File.Copy(source, dlg.FileName, true);
                return dlg.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save {title.ToLower()}:\n{ex.Message}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        bool SaveToPath(string path)
        {
            try
            {
                var dcs = new DataContractSerializer(typeof(Project), ProjectSerializer.KnownTypes);
                string tmp = Path.Combine(Program.TempDir, "tempprojectfile");
                using (var stream = File.Open(tmp, FileMode.Create))
                    dcs.WriteObject(stream, _project);
                File.Copy(tmp, path, true);
                _undoItems.MarkSaved();
                OnPropertyChanged(nameof(HasUnsavedChanges));
                WindowTitle = $"{Program.AppName} — {Path.GetFileName(path)}";
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool ConfirmSaveChangesBefore(string action)
        {
            if (!HasUnsavedChanges) return true;

            var result = MessageBox.Show(
                $"Save changes to the current project before {action}?",
                Program.AppName,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel) return false;
            if (result == MessageBoxResult.No) return true;
            return SaveProjectCore();
        }

        [RelayCommand]
        async Task ImportMidi()
        {
            var dlg = new ImportSongWindow(FileType.Midi) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;
            await DoImport(BuildOptions(FileType.Midi, dlg));
        }

        [RelayCommand]
        async Task ImportMod()
        {
            var dlg = new ImportSongWindow(FileType.Mod) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;
            await DoImport(BuildOptions(FileType.Mod, dlg));
        }

        [RelayCommand]
        async Task ImportHvl()
        {
            var dlg = new ImportSongWindow(FileType.Hvl) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;
            await DoImport(BuildOptions(FileType.Hvl, dlg));
        }

        [RelayCommand]
        async Task ImportSid()
        {
            var dlg = new ImportSongWindow(FileType.Sid) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;

            var options = BuildOptions(FileType.Sid, dlg);

            // SID files may contain multiple sub-songs — let the user pick.
            if (!SelectSidSubSong(options)) return;

            await DoImport(options);
        }

        /// <summary>
        /// For a SID import, shows the sub-song picker when the file has more than one
        /// sub-song and writes the chosen sub-song + its length into <paramref name="options"/>.
        /// Returns false only if the user cancelled the picker (caller should abort the import).
        /// </summary>
        static bool SelectSidSubSong(ImportOptions options)
        {
            if (options is not SidImportOptions) return true;
            if (string.IsNullOrEmpty(options.NotePath) || !File.Exists(options.NotePath)) return true;

            var subWin = new SubSongWindow(options.NotePath) { Owner = Application.Current.MainWindow };
            if (subWin.NumSongs > 1 && subWin.ShowDialog() != true) return false;
            options.SubSong = subWin.SelectedSong;
            options.SongLengthS = subWin.SongLengthS;
            return true;
        }

        /// <summary>Build an ImportOptions from the dialog fields (WPF path).</summary>
        static ImportOptions BuildOptions(FileType fileType, ImportSongWindow dlg)
        {
            ImportOptions options;
            switch (fileType)
            {
                case FileType.Midi: options = new MidiImportOptions(); break;
                case FileType.Mod: options = new ModImportOptions(); break;
                case FileType.Hvl: options = new HvlImportOptions(); break;
                default: options = new SidImportOptions(); break;
            }

            options.RawNotePath = dlg.NoteFilePath;
            try { options.SetNotePath(); } catch (FileImportException) { }

            if (!string.IsNullOrWhiteSpace(dlg.AudioFilePath))
            {
                options.AudioPath = dlg.AudioFilePath;
            }

            options.EraseCurrent = dlg.EraseCurrent;
            options.InsTrack = dlg.InsTrack;
            return options;
        }

        /// <summary>Shared post-dialog import logic.</summary>
        async Task DoImport(ImportOptions options)
        {
            if (options.EraseCurrent && !ConfirmSaveChangesBefore("importing a new song"))
                return;

            // Ensure a project object exists to import into.
            if (_project == null)
            {
                var fresh = new Project();
                NoteStyle.SetProject(fresh);
                Project = fresh;
            }

            var drawHost = GetDrawHost?.Invoke();
            if (drawHost != null) Project.SetDrawHost(drawHost);
            NoteStyle.SetProject(_project);

            try { options.CheckSourceFile(); }
            catch (FileImportException ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.FileName}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (options.NoteFileType != FileType.Midi)
                {
                    // MOD/SID are converted by the external remuxer.exe, which reports percentage
                    // progress on stdout. Run it behind the shared progress window so the user sees
                    // the conversion advance and can cancel it.
                    bool imported = false;
                    bool cancelled = false;
                    Exception failure = null;

                    var w = new Controls.ProgressWindow(
                        $"Converting {options.DisplayName}",
                        async cb =>
                        {
                            try
                            {
                                imported = await _project.ImportSong(
                                    options, new Progress<float>(cb.UpdateProgress), cb.CancelToken);
                            }
                            catch (OperationCanceledException) { cancelled = true; }
                            catch (Exception ex) { failure = ex; }
                            return null;
                        })
                    { Owner = Application.Current?.MainWindow };
                    w.ShowDialog();

                    if (cancelled) return;
                    if (failure != null) throw failure;
                    if (!imported) return;
                }
                else if (!await _project.ImportSong(options))
                    return;
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
                _currentProjectPath = "";

            var wfp = GetRendererWaveformPanel?.Invoke();
            _project.InitAfterDeserialization(wfp);

            TrackList.Rebuild(_project);
            OnProjectLoaded?.Invoke(_project);
            OnLoadBackgroundImage?.Invoke(_project.Props.BackgroundImagePath);

            // Refresh audio-dependent CanExecute (HasAudio depends on Media.getAudioLength()).
            OnPropertyChanged(nameof(HasAudio));
            TogglePlaybackCommand.NotifyCanExecuteChanged();
            GoToBeginningCommand.NotifyCanExecuteChanged();
            GoToEndCommand.NotifyCanExecuteChanged();
            NudgeBackCommand.NotifyCanExecuteChanged();
            NudgeForwardCommand.NotifyCanExecuteChanged();
            JumpBackCommand.NotifyCanExecuteChanged();
            JumpForwardCommand.NotifyCanExecuteChanged();

            _undoItems.Clear();
            _undoItems.Add("", _project);
            if (options.EraseCurrent)
                _undoItems.MarkSaved();
            UpdateUndoRedo();

            WindowTitle = $"{Program.AppName} — {options.DisplayName}";
            CurrentScreen = AppScreen.Song;
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void ExportVideo()
        {
            var options = AppSettings.Instance.LoadVideoExportOptions();

            var dlg = new Controls.VideoExportWindow(options) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;

            var save = new SaveFileDialog
            {
                Filter = "Mkv files (*.mkv)|*.mkv",
                Title = "Save video file",
                InitialDirectory = AppSettings.Instance.VideoFolderOrDefault
            };
            if (save.ShowDialog() != true) return;

            AppSettings.Instance.RememberFolder(save.FileName, dir => AppSettings.Instance.VideoFolder = dir);
            AppSettings.Instance.SaveVideoExportOptions(dlg.Options);

            Controls.ProgressWindow.RunRender(save.FileName, dlg.Options, RenderVideo);
        }

        [RelayCommand]
        void HvscIntegration()
        {
            var dlg = new Controls.HvscIntegrationWindow { Owner = Application.Current.MainWindow };
            dlg.ShowDialog();
        }

        [RelayCommand]
        void Preferences()
        {
            var dlg = new Controls.PreferencesWindow { Owner = Application.Current.MainWindow };
            dlg.ShowDialog();
        }

        // ---- Playback commands ----

        [RelayCommand(CanExecute = nameof(HasProject))]
        void TogglePlayback() => _project?.TogglePlayback();

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToBeginning() => _project?.StopPlayback();

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToEnd()
        {
            if (_project == null) return;
            if (_project.IsPlaying) _project.TogglePlayback();
            _project.NormSongPos = 1;
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void NudgeBack()
        {
            if (_project == null) return;
            _project.NudgeSongPos(SongRenderer.SmallScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void NudgeForward()
        {
            if (_project == null) return;
            _project.NudgeSongPos(-SongRenderer.SmallScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void JumpBack()
        {
            if (_project == null) return;
            _project.NudgeSongPos(SongRenderer.LargeScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void JumpForward()
        {
            if (_project == null) return;
            _project.NudgeSongPos(-SongRenderer.LargeScrollStep);
            ResyncPlaybackPosition();
        }

        void ResyncPlaybackPosition()
        {
            // When playing, restart audio at the new position.
            if (_project?.IsPlaying == true)
            {
                _project.TogglePlayback();
                _project.TogglePlayback();
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToNextKeyFrame()
        {
            if (_project == null) return;
            Keyframes.KeyframeService.GoToNextAny();
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToPrevKeyFrame()
        {
            if (_project == null) return;
            Keyframes.KeyframeService.GoToPrevAny();
            ResyncPlaybackPosition();
        }

        // ---- Edit commands ----

        [RelayCommand(CanExecute = nameof(CanUndo))]
        void Undo()
        {
            _undoItems--;
            ApplyUndoItem();
            UpdateUndoRedo();
        }

        [RelayCommand(CanExecute = nameof(CanRedo))]
        void Redo()
        {
            _undoItems++;
            ApplyUndoItem();
            UpdateUndoRedo();
        }

        public bool CanUndo => _undoItems.Previous != null;
        public bool CanRedo => _undoItems.Next != null;

        void ApplyUndoItem()
        {
            if (_undoItems.Current == null) return;
            _project?.CopyPropsFrom(_undoItems.Current.Project);
            // Rebuild geometry for any style/material/spatial changes in the restored snapshot.
            _project?.CreateGeos();
            // Refresh the Song and Track property panels so they reflect restored values.
            SongProps.RefreshAll();
            OnTrackListSelectionChanged();
            TrackList.RefreshColors();
            // Refresh keyframe panel and list.
            Keyframes.KeyframeService.RaiseKeyframesChanged();
        }

        void UpdateUndoRedo()
        {
            UndoDescription = _undoItems.UndoDesc;
            RedoDescription = _undoItems.RedoDesc;
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        public void AddUndoItem(string desc)
        {
            if (_project == null) return;
            _undoItems.Add(desc, _project);
            UpdateUndoRedo();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void ResetCamera()
        {
            if (_project == null) return;
            if (!EnsureCameraEditKeyframe()) return;
            ApplyCamera(new Camera());
            _project.SyncLiveCameraEdit();
            SongProps.RefreshLiveValues();
            Keyframes.KeyframeService.RaiseRefresh();
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
                if (!EnsureCameraEditKeyframe()) return;
                ApplyCamera(cam);
                _project.SyncLiveCameraEdit();
                SongProps.RefreshLiveValues();
                Keyframes.KeyframeService.RaiseRefresh();
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
                dcs.WriteObject(stream, _project?.Props.Camera);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool EnsureCameraEditKeyframe()
        {
            if (_project == null) return false;

            var scope = Keyframes.KeyframeService.KfScope.Project;
            if (Keyframes.KeyframeService.HasAnyKeyForAny("Camera", scope))
                Keyframes.KeyframeService.PausePlayback();
            return Keyframes.KeyframeService.EnsureKeyframeForEdit("Camera", scope);
        }

        void ApplyCamera(Camera source)
        {
            var dest = _project.Props.Camera;
            dest.Pos = source.Pos;
            dest.Orientation = source.Orientation;
            dest.Fov = source.Fov;
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
            _project.TrackViews[0].TrackProps.CloneFrom(props, (int)TrackPropsType.TPT_All);
            if ((props.TypeFlags & (int)TrackPropsType.TPT_Style) != 0)
                _project.CreateGeos();
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

            TrackProps props = _project.TrackViews[0].TrackProps;
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
            foreach (var tv in _project.TrackViews)
                tv.TrackProps.ResetProps();
            _project.CreateGeos();
            AddUndoItem("Default Track Properties");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void InsertLyrics()
        {
            _project.InsertLyrics();
            AddUndoItem("Insert Lyrics");
        }

        // ---- Renderer WaveformPanel accessor (set by MainWindow) ----

        public Func<WaveformPanel> GetRendererWaveformPanel { get; set; }

        // ---- IImportService (browser download import) ----

        public void ImportFromUrl(string url, string suggestedFileName)
        {
            string ext = suggestedFileName.Split('.').Last().ToLower();

            FileType? fileType = ImportFileFormats.FromExtension(ext);

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
                case FileType.Midi: options = new MidiImportOptions(); break;
                case FileType.Mod: options = new ModImportOptions(); break;
                case FileType.Hvl: options = new HvlImportOptions(); break;
                default: options = new SidImportOptions(); break;
            }
            options.RawNotePath = url;       // preserve original URL for project save and dialog display
            try { options.SetNotePath(); }    // downloads the URL to a temp file (WebClient)
            catch (FileImportException)
            {
                MessageBox.Show("Download failed: " + url, Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            options.EraseCurrent = true;
            options.InsTrack = AppSettings.Instance.GetInsTrack(fileType.Value);
            ImportSongWindow.UpdateSession(fileType.Value, erase: true, notePath: url, audioPath: "",
                insTrack: AppSettings.Instance.GetInsTrack(fileType.Value));

            // SID downloads may contain multiple sub-songs — let the user pick.
            if (!SelectSidSubSong(options)) return;

            _ = DoImport(options);   // fire-and-forget on the UI thread (async void pattern)
        }
    }
}
