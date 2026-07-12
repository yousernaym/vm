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
        [NotifyCanExecuteChangedFor(nameof(DefaultTrackPropsCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToNextKeyFrameCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToPrevKeyFrameCommand))]
        private Project _project;

        public bool HasProject => Project != null;
        public bool HasAudio => Project != null && Media.GetAudioLength() > 0;
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

        // Ctrl+wheel viewport-width edit: multiply width by this factor per notch, clamp to the
        // slider's range (ExpBase 2, log 0..10 → 1..1024 QN), and coalesce undo via a debounce timer.
        const double ViewWidthWheelFactor = 1.1;
        const float MinViewWidthQn = 1f;
        const float MaxViewWidthQn = 1024f;
        System.Windows.Threading.DispatcherTimer _viewWidthUndoTimer;

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
                Project?.CreateGeos();
            };

            SelectedTrackProps.RefreshTrackColors = TrackList.RefreshColors;

            SelectedTrackProps.LoadTexture = path =>
            {
                if (Project == null) return;
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
                    MetroMessageBox.Show(ex.Message, Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Project.CreateGeos();
                Keyframes.KeyframeService.SyncEditedValue("TexturePath",
                    Keyframes.KeyframeService.KfScope.Track,
                    new Keyframes.StringKfValue(path));
                OnTrackListSelectionChanged();
                AddUndoItem("Load texture");
            };

            SelectedTrackProps.UnloadTexture = () =>
            {
                if (Project == null) return;
                if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("TexturePath",
                    Keyframes.KeyframeService.KfScope.Track))
                    return;

                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.MaterialProps.TexProps.UnloadTexture();
                Project.CreateGeos();
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
                // The AudioFilename setter loads the audio and adds the "Set track audio" undo
                // item (only when the value actually changed).
                SelectedTrackProps.AudioFilename = dlg.FileName;
            };

            SelectedTrackProps.BrowseMultipleAudioFiles = async () =>
            {
                if (Project == null) return;
                var tracks = TrackList.Items.Skip(1).ToList();   // real tracks; Global is Items[0]
                if (tracks.Count == 0)
                {
                    MetroMessageBox.Show("There are no tracks to assign audio files to.", Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Audio files|*.wav;*.flac;*.mp3;*.ogg|All files|*.*",
                    Multiselect = true
                };
                var audioDir = AppSettings.Instance.TrackAudioFolder;
                if (!string.IsNullOrEmpty(audioDir)) dlg.InitialDirectory = audioDir;
                if (dlg.ShowDialog() != true || dlg.FileNames.Length == 0) return;
                AppSettings.Instance.RememberFolder(dlg.FileNames[0], dir => AppSettings.Instance.TrackAudioFolder = dir);

                var win = new Controls.AssignAudioFilesWindow(
                    tracks.Select(t => t.Name).ToList(), dlg.FileNames)
                {
                    Owner = Application.Current.MainWindow
                };
                if (win.ShowDialog() != true || win.Assignments.Count == 0) return;

                foreach (var kv in win.Assignments)
                    tracks[kv.Key].TrackView.TrackProps.AudioProps.Filename = kv.Value;

                OnTrackListSelectionChanged();                 // refresh merged props
                AddUndoItem("Assign track audio files");       // one undo step for the whole batch

                // Load audio for exactly the assigned tracks (Global is selected, so
                // LoadSelectedTracksAudio can't be reused).
                var targets = win.Assignments.Keys
                    .Select(i => tracks[i].TrackView.TrackProps.AudioProps).ToList();
                await Task.WhenAll(targets.Select(ap => ap.LoadAudioAsync()));
                var failed = targets
                    .Select(ap => ap.SidWizChannel)
                    .FirstOrDefault(ch => !string.IsNullOrEmpty(ch.ErrorMessage));
                if (failed != null)
                    MetroMessageBox.Show($"Couldn't load audio file:\n{failed.Filename}", Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MetroMessageBox.Show($"Couldn't load audio file:\n{failedChannel.Filename}", Program.AppName,
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
                Project?.CreateGeos();
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
                Project?.CreateGeos();
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

            // Re-bake geometry at the new effective width on commit (undo snapshot comes from
            // Keyframing.OnCommit on the same CommitChanges event).
            SelectedTrackProps.CommitViewWidth = () => Project?.CreateGeos();

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
                if (Project == null || sourceItem == null) return;
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
                    Project.CreateGeos();   // geometry/texture coords are baked per track
                if (flag == (int)TrackPropsType.TPT_Material)
                    TrackList.RefreshColors(); // update the two color swatches in the list
                OnTrackListSelectionChanged(); // refresh the tabs for the (still-selected) dragged tracks
                AddUndoItem("Copy Track Properties");
            };

            // Track-list context menu → the same selection-based commands the main menu uses.
            TrackList.SaveSelectedProps = SaveTrackProps;
            TrackList.LoadSelectedProps = LoadTrackProps;
            TrackList.DefaultProps = DefaultTrackProps;
            SelectedTrackProps.DefaultProps = DefaultTrackProps;

            // Track-properties context menu → save/load just the currently-open tab.
            SelectedTrackProps.SaveCurrentTab = () =>
            {
                if (Project == null || TrackList.SelectedItems.Count != 1) return;
                int flag = 1 << SelectedTrackProps.SelectedTabIndex;
                SaveTrackPropsFile(TrackList.SelectedItems[0].TrackView.TrackProps, flag);
            };

            SelectedTrackProps.LoadCurrentTab = () =>
            {
                if (Project == null || TrackList.SelectedItems.Count < 1) return;
                int tabIndex = SelectedTrackProps.SelectedTabIndex;
                int flag = 1 << tabIndex;
                if (!TryLoadTrackPropsFile(out TrackProps props)) return;
                if ((props.TypeFlags & flag) == 0)
                {
                    MetroMessageBox.Show($"The selected file does not contain {TabNames[tabIndex]} properties.",
                        Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                ApplyLoadedProps(props, flag);
            };
        }

        void OnTrackListSelectionChanged()
        {
            if (Project == null) return;
            // List positions for MergeTrackProps (which indexes TrackViews[] by position).
            var indices = TrackList.SelectedItems
                .Select(item => TrackList.Items.IndexOf(item))
                .Where(i => i >= 0)
                .ToList();
            _lastSelectedIndices = indices;
            SelectedTrackProps.MergedProps = Project.MergeTrackProps(indices);
            // TrackNumbers (stable MIDI track indices) for the keyframe service.
            var trackNumbers = TrackList.SelectedItems
                .Select(item => item.TrackView.TrackNumber)
                .ToList();
            Keyframes.KeyframeService.SelectedTrackIds = trackNumbers;
            Keyframes.KeyframeService.RaiseKeyframesChanged();

            // Selection count drives the save/load/default enable state (main menu + context menus).
            SelectedTrackProps.SelectedTrackCount = TrackList.SelectedItems.Count;
            SelectedTrackProps.IsOnlyGlobalSelected =
                TrackList.SelectedItems.Count == 1 &&
                TrackList.SelectedItems[0].TrackView.TrackNumber == 0;
            SaveTrackPropsCommand.NotifyCanExecuteChanged();
            LoadTrackPropsCommand.NotifyCanExecuteChanged();
            DefaultTrackPropsCommand.NotifyCanExecuteChanged();
        }

        void WireSongPropsCallbacks()
        {
            SongProps.CreateGeos = () => Project?.CreateGeos();

            SongProps.ResetPitches = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmPitchReset(out var affected))
                    return;
                Project?.ResetPitchLimits();
                Keyframes.KeyframeService.CapturePitchResetAtCurrentTick(affected);
                Project?.CreateGeos();
                SongProps.RefreshAll();
                AddUndoItem("Reset pitches");
            };

            SongProps.NotesMinPitch = () => Project?.Notes?.MinPitch;
            SongProps.NotesMaxPitch = () => Project?.Notes?.MaxPitch;

            SongProps.SongLengthSWithoutPbOffset = () =>
                Project?.Notes != null
                    ? (double?)Project.TicksToSeconds(Project.Notes.SongLengthT)
                    : null;

            SongProps.BrowseBackground = () =>
            {
                if (Project == null) return;
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

                Project.Props.BackgroundImagePath = dlg.FileName;
                OnLoadBackgroundImage?.Invoke(dlg.FileName);

                // Record the chosen path into a keyframe at the current tick (no-op when no keyframes exist).
                Keyframes.KeyframeService.SyncEditedValue("BackgroundImagePath",
                    Keyframes.KeyframeService.KfScope.Project,
                    new Keyframes.StringKfValue(dlg.FileName));
                AddUndoItem("Load background");
            };

            SongProps.UnloadBackground = () =>
            {
                if (Project == null) return;

                // Gate: if background-path keyframes exist but not at this tick, prompt to create one.
                if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("BackgroundImagePath", Keyframes.KeyframeService.KfScope.Project))
                    return;

                Project.Props.BackgroundImagePath = "";
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
            get => Project?.NormSongPos ?? 0;
            set { if (Project != null) Project.NormSongPos = value; }
        }

        public void NotifyScrollPositionChanged()
        {
            OnPropertyChanged(nameof(ScrollPosition));
            // Apply per-property keyframe interpolation for the new position. During playback Project.Update
            // already did this; here it covers paused seeks/scrubs (this method fires on every position
            // change, but NOT during a static control edit — so it never fights an in-progress edit).
            if (Project != null && !Project.IsPlaying)
                Project.InterpolatePropertyKeyframes();
            if (ShowSongProps)
                SongProps.RefreshLiveValues();
            if (ShowTrackProps && Project?.HasTrackKeyframes == true)
            {
                SelectedTrackProps.MergedProps = Project.MergeTrackProps(_lastSelectedIndices);
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
        /// Reacts to any change in the keyframe set (add/remove/move). Restores the static background
        /// when its keyframes are gone, then re-applies keyframe interpolation and refreshes the live
        /// property values. This makes a removed keyframe take effect immediately — e.g. deleting one of
        /// two keyframes updates the property to its remaining interpolated value right away, instead of
        /// only when playback next runs.
        /// </summary>
        void OnKeyframesChangedRestoreBackground()
        {
            if (Project == null) return;
            LoadStaticBackgroundIfUnkeyframed(Project);
            NotifyScrollPositionChanged();
        }

        /// <summary>
        /// Loads the static background image unless background-path keyframes exist. With keyframes,
        /// the serialized live path is stale (whatever was interpolated when the project was saved —
        /// often the last keyframe's image), so keyframe interpolation at the current position must
        /// decide the image/crossfade instead.
        /// </summary>
        void LoadStaticBackgroundIfUnkeyframed(Project project)
        {
            if (!project.PropertyKeyframes.HasAny("proj/BackgroundImagePath"))
                OnLoadBackgroundImage?.Invoke(project.Props.BackgroundImagePath);
            else if (!project.IsPlaying)
                project.InterpolatePropertyKeyframes();
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
                MetroMessageBox.Show("Invalid project file.\n" + e.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (IOException e)
            {
                MetroMessageBox.Show("Could not open file.\n" + e.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
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
                // animation loop is still drawing the previous live project (Project), and its note
                // rendering reads the static NoteStyle.Project — so restore it to Project rather than
                // null, otherwise the next Draw() dereferences a null Project (NoteStyle.DrawTrack).
                NoteStyle.SetProject(Project);
                MetroMessageBox.Show($"Could not load project file: {ex.Message}\n\nMissing file: {ex.FileName}",
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                NoteStyle.SetProject(Project);
                MetroMessageBox.Show("Error loading project: " + ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
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
            LoadStaticBackgroundIfUnkeyframed(tempProject);

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
            if (Project == null) return false;
            if (string.IsNullOrEmpty(_currentProjectPath)) return SaveProjectAsCore();
            return SaveToPath(_currentProjectPath);
        }

        bool SaveProjectAsCore()
        {
            var dlg = new SaveFileDialog
            {
                Filter = $"Visual Music projects|*.{Project.DefaultFileExt}|All files|*.*",
                InitialDirectory = AppSettings.Instance.ProjectFolderOrDefault,
                FileName = Project.DefaultFileName
            };
            if (dlg.ShowDialog() != true) return false;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.ProjectFolder = dir);
            SaveSourceFiles(dlg.FileName);   // optional MIDI/WAV export; cancelling it still saves the project
            if (!SaveToPath(dlg.FileName)) return false;
            _currentProjectPath = dlg.FileName;
            Project.DefaultFileName = Path.GetFileName(dlg.FileName);
            return true;
        }

        /// <summary>
        /// Offers to export the converted/synthesized MIDI and WAV alongside the project (both default
        /// off). MOD/SID/HVL projects can save both; a MIDI project already has the MIDI as its source,
        /// so only the generated WAV can be saved (its MIDI checkbox stays disabled). A saved WAV
        /// becomes the project's supplied audio; a saved MIDI turns a non-MIDI project into a MIDI
        /// project. This never blocks the save: cancelling the checkbox dialog (or an individual file
        /// picker) just skips the export and the project is still saved.
        /// </summary>
        void SaveSourceFiles(string projectPath)
        {
            var io = Project?.ImportOptions;
            if (io == null) return;

            // A MIDI project's MIDI is already its source file, so only the generated WAV is offered.
            bool midiAvail = io.NoteFileType != FileType.Midi
                && !string.IsNullOrEmpty(io.MidiOutputPath) && File.Exists(io.MidiOutputPath);
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
            if (savedMidiPath != null) Project.ConvertToMidiProject(savedMidiPath, io.AudioPath);
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
                MetroMessageBox.Show($"Could not save {title.ToLower()}:\n{ex.Message}", Program.AppName,
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
                    dcs.WriteObject(stream, Project);
                File.Copy(tmp, path, true);
                _undoItems.MarkSaved();
                OnPropertyChanged(nameof(HasUnsavedChanges));
                WindowTitle = $"{Program.AppName} — {Path.GetFileName(path)}";
                return true;
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool ConfirmSaveChangesBefore(string action)
        {
            if (!HasUnsavedChanges) return true;

            var result = MetroMessageBox.Show(
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
            if (Project == null)
            {
                var fresh = new Project();
                NoteStyle.SetProject(fresh);
                Project = fresh;
            }

            var drawHost = GetDrawHost?.Invoke();
            if (drawHost != null) Project.SetDrawHost(drawHost);
            NoteStyle.SetProject(Project);

            try { options.CheckSourceFile(); }
            catch (FileImportException ex)
            {
                MetroMessageBox.Show($"{ex.Message}\n{ex.FileName}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            WarnIfMidiAudioWillBeDisabled(options);

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
                        $"Converting song",
                        async cb =>
                        {
                            try
                            {
                                imported = await Project.ImportSong(
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
                else if (!await Project.ImportSong(options))
                    return;
            }
            catch (FileImportException ex)
            {
                MetroMessageBox.Show($"{ex.Message}\n{ex.FileName}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show("Import failed: " + ex.Message, Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (options.EraseCurrent)
                _currentProjectPath = "";

            var wfp = GetRendererWaveformPanel?.Invoke();
            Project.InitAfterDeserialization(wfp);

            TrackList.Rebuild(Project);
            OnProjectLoaded?.Invoke(Project);
            LoadStaticBackgroundIfUnkeyframed(Project);

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
            _undoItems.Add("", Project);
            if (options.EraseCurrent)
                _undoItems.MarkSaved();
            UpdateUndoRedo();

            WindowTitle = $"{Program.AppName} — {options.DisplayName}";
            CurrentScreen = AppScreen.Song;
        }

        static void WarnIfMidiAudioWillBeDisabled(ImportOptions options)
        {
            if (options.NoteFileType != FileType.Midi || options.HasSuppliedAudio || MidMix.SfLoaded())
                return;

            string soundFontStatus = File.Exists(Program.SoundFontPath)
                ? $"{Program.SoundFontFileName} could not be loaded from the app folder"
                : $"{Program.SoundFontFileName} is missing from the app folder";

            MetroMessageBox.Show(
                $"Audio will be disabled for this MIDI import because no WAV path was specified and {soundFontStatus}.\n\n" +
                $"App folder:\n{Program.Dir}\n\n" +
                $"Place a valid {Program.SoundFontFileName} in that folder, then restart {Program.AppName}, to enable generated MIDI audio.",
                Program.AppName,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void ExportVideo()
        {
            var options = AppSettings.Instance.LoadVideoExportOptions();

            var dlg = new Controls.VideoExportWindow(options) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;

            // Persist the chosen settings as soon as OK is clicked, so they survive even if the
            // user then cancels the save-file dialog below.
            AppSettings.Instance.SaveVideoExportOptions(dlg.Options);

            var save = new SaveFileDialog
            {
                Filter = "Mkv files (*.mkv)|*.mkv",
                Title = "Save video file",
                InitialDirectory = AppSettings.Instance.VideoFolderOrDefault
            };
            if (save.ShowDialog() != true) return;

            AppSettings.Instance.RememberFolder(save.FileName, dir => AppSettings.Instance.VideoFolder = dir);

            Controls.ProgressWindow.RunRender(save.FileName, dlg.Options, RenderVideo);
        }

        [RelayCommand]
        void Settings()
        {
            var dlg = new Controls.SettingsWindow { Owner = Application.Current.MainWindow };
            dlg.ShowDialog();
        }

        // ---- Playback commands ----

        [RelayCommand(CanExecute = nameof(HasProject))]
        void TogglePlayback() => Project?.TogglePlayback();

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToBeginning() => Project?.StopPlayback();

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToEnd()
        {
            if (Project == null) return;
            if (Project.IsPlaying) Project.TogglePlayback();
            Project.NormSongPos = 1;
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void NudgeBack()
        {
            if (Project == null) return;
            Project.NudgeSongPos(SongRenderer.SmallScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void NudgeForward()
        {
            if (Project == null) return;
            Project.NudgeSongPos(-SongRenderer.SmallScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void JumpBack()
        {
            if (Project == null) return;
            Project.NudgeSongPos(SongRenderer.LargeScrollStep);
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void JumpForward()
        {
            if (Project == null) return;
            Project.NudgeSongPos(-SongRenderer.LargeScrollStep);
            ResyncPlaybackPosition();
        }

        void ResyncPlaybackPosition()
        {
            // When playing, restart audio at the new position.
            if (Project?.IsPlaying == true)
            {
                Project.TogglePlayback();
                Project.TogglePlayback();
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToNextKeyFrame()
        {
            if (Project == null) return;
            Keyframes.KeyframeService.GoToNextAny();
            ResyncPlaybackPosition();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void GoToPrevKeyFrame()
        {
            if (Project == null) return;
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
            Project?.CopyPropsFrom(_undoItems.Current.Project);
            // Rebuild geometry for any style/material/spatial changes in the restored snapshot.
            Project?.CreateGeos();
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
            if (Project == null) return;
            _undoItems.Add(desc, Project);
            UpdateUndoRedo();
        }

        /// <summary>
        /// Ctrl+mouse-wheel over the focused song panel: scales the viewport width of the selected
        /// track(s). Positive <paramref name="notches"/> (wheel away) zooms in (narrower width).
        /// </summary>
        public void AdjustSelectedViewWidth(int notches)
        {
            if (Project == null || notches == 0) return;
            var selected = TrackList.SelectedItems.ToList();
            if (selected.Count == 0) return;
            if (!Keyframes.KeyframeService.EnsureKeyframeForEdit("ViewWidthQn",
                    Keyframes.KeyframeService.KfScope.Track))
                return;

            double factor = Math.Pow(ViewWidthWheelFactor, -notches);   // wheel away → narrower
            foreach (var item in selected)
            {
                var tp = item.TrackView.TrackProps;
                float current = Project.EffectiveViewWidthQn(tp);
                tp.SpatialProps.ViewWidthQn =
                    (float)Math.Clamp(current * factor, MinViewWidthQn, MaxViewWidthQn);
            }
            Project.CreateGeos();
            Keyframes.KeyframeService.SyncCurrentValues("ViewWidthQn",
                Keyframes.KeyframeService.KfScope.Track);
            OnTrackListSelectionChanged();   // refresh the Spatial tab slider display
            ScheduleViewWidthUndo();
        }

        // Debounce: fold a burst of wheel notches into a single undo item once scrolling settles.
        void ScheduleViewWidthUndo()
        {
            if (_viewWidthUndoTimer == null)
            {
                _viewWidthUndoTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(600)
                };
                _viewWidthUndoTimer.Tick += (s, e) =>
                {
                    _viewWidthUndoTimer.Stop();
                    AddUndoItem("Edit viewport width");
                };
            }
            _viewWidthUndoTimer.Stop();
            _viewWidthUndoTimer.Start();
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void ResetCamera()
        {
            if (Project == null) return;
            if (!EnsureCameraEditKeyframe()) return;
            ApplyCamera(new Camera());
            Project.SyncLiveCameraEdit();
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
                Project.SyncLiveCameraEdit();
                SongProps.RefreshLiveValues();
                Keyframes.KeyframeService.RaiseRefresh();
                AddUndoItem("Load Camera");
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
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
                dcs.WriteObject(stream, Project?.Props.Camera);
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool EnsureCameraEditKeyframe()
        {
            if (Project == null) return false;

            var scope = Keyframes.KeyframeService.KfScope.Project;
            if (Keyframes.KeyframeService.HasAnyKeyForAny("Camera", scope))
                Keyframes.KeyframeService.PausePlayback();
            return Keyframes.KeyframeService.EnsureKeyframeForEdit("Camera", scope);
        }

        void ApplyCamera(Camera source)
        {
            var dest = Project.Props.Camera;
            dest.Pos = source.Pos;
            dest.Orientation = source.Orientation;
            dest.Fov = source.Fov;
        }

        // ---- Track property save/load (selection-aware, per-tab) ----

        // Tab index (0..4) → display name, for messages. Mirrors TrackPropsType bit order.
        static readonly string[] TabNames = { "Style", "Material", "Light", "Spatial", "Audio" };

        public bool CanSaveTrackProps => HasProject && TrackList.SelectedItems.Count == 1;
        public bool CanLoadTrackProps => HasProject && TrackList.SelectedItems.Count >= 1;
        public bool CanDefaultTrackProps => HasProject && TrackList.SelectedItems.Count >= 1;

        [RelayCommand(CanExecute = nameof(CanSaveTrackProps))]
        void SaveTrackProps()
        {
            if (TrackList.SelectedItems.Count != 1) return;

            var tabDlg = new TrackPropsTabSelectWindow("Save properties",
                (int)TrackPropsType.TPT_All, (int)TrackPropsType.TPT_All)
            { Owner = Application.Current.MainWindow };
            if (tabDlg.ShowDialog() != true || tabDlg.SelectedFlags == 0) return;

            SaveTrackPropsFile(TrackList.SelectedItems[0].TrackView.TrackProps, tabDlg.SelectedFlags);
        }

        [RelayCommand(CanExecute = nameof(CanLoadTrackProps))]
        void LoadTrackProps()
        {
            if (TrackList.SelectedItems.Count < 1) return;
            if (!TryLoadTrackPropsFile(out TrackProps props)) return;

            var tabDlg = new TrackPropsTabSelectWindow("Load properties",
                props.TypeFlags, props.TypeFlags)
            { Owner = Application.Current.MainWindow };
            if (tabDlg.ShowDialog() != true || tabDlg.SelectedFlags == 0) return;

            ApplyLoadedProps(props, tabDlg.SelectedFlags);
        }

        [RelayCommand(CanExecute = nameof(CanDefaultTrackProps))]
        void DefaultTrackProps()
        {
            if (TrackList.SelectedItems.Count < 1) return;
            foreach (var item in TrackList.SelectedItems)
                item.TrackView.TrackProps.ResetProps();
            Project.CreateGeos();
            TrackList.RefreshColors();
            OnTrackListSelectionChanged();
            AddUndoItem("Default Track Properties");
        }

        // ---- Shared file/serialization helpers (used by both the track-list and per-tab paths) ----

        bool TryLoadTrackPropsFile(out TrackProps props)
        {
            props = null;
            var dlg = new OpenFileDialog
            {
                Filter = "Track property files|*.tp|All files|*.*",
                InitialDirectory = AppSettings.Instance.TrackPropsFolderOrDefault
            };
            if (dlg.ShowDialog() != true) return false;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.TrackPropsFolder = dir);

            var dcs = new DataContractSerializer(typeof(TrackProps), ProjectSerializer.KnownTypes);
            try
            {
                using var stream = File.Open(dlg.FileName, FileMode.Open);
                props = (TrackProps)dcs.ReadObject(stream);
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        void SaveTrackPropsFile(TrackProps source, int flags)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Track property files|*.tp|All files|*.*",
                InitialDirectory = AppSettings.Instance.TrackPropsFolderOrDefault,
                FileName = "track.tp"
            };
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.TrackPropsFolder = dir);

            source.TypeFlags = flags;   // marks which tabs the file is meant to carry (see TrackProps.TypeFlags)
            var dcs = new DataContractSerializer(typeof(TrackProps), ProjectSerializer.KnownTypes);
            try
            {
                using var stream = File.Open(dlg.FileName, FileMode.Create);
                dcs.WriteObject(stream, source);
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Applies the chosen tabs of <paramref name="props"/> to every selected track, then refreshes.
        void ApplyLoadedProps(TrackProps props, int flags)
        {
            var drawHost = GetDrawHost?.Invoke();
            foreach (var item in TrackList.SelectedItems)
                item.TrackView.TrackProps.CloneFrom(props, flags, drawHost);

            if ((flags & ((int)TrackPropsType.TPT_Style | (int)TrackPropsType.TPT_Material)) != 0)
                Project.CreateGeos();
            if ((flags & (int)TrackPropsType.TPT_Material) != 0)
                TrackList.RefreshColors();
            OnTrackListSelectionChanged();
            AddUndoItem("Load Track Properties");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        void InsertLyrics()
        {
            Project.InsertLyrics();
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
                MetroMessageBox.Show(
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
                MetroMessageBox.Show("Download failed: " + url, Program.AppName,
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
