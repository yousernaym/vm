using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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
        [NotifyCanExecuteChangedFor(nameof(InvertTrackSelectionCommand))]
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

                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Audio files|*.wav;*.flac;*.mp3;*.ogg|All files|*.*",
                    Multiselect = true
                };
                var audioDir = AppSettings.Instance.TrackAudioFolder;
                if (!string.IsNullOrEmpty(audioDir)) dlg.InitialDirectory = audioDir;
                if (dlg.ShowDialog() != true || dlg.FileNames.Length == 0) return;
                AppSettings.Instance.RememberFolder(dlg.FileNames[0], dir => AppSettings.Instance.TrackAudioFolder = dir);

                // In an audio-only project this dialog is the track-creation UI, so default the
                // per-file "create new track" checkboxes on. Note projects keep them off.
                bool isAudioOnly = Project.ImportOptions?.NoteFileType == FileType.Audio;

                var win = new Controls.AssignAudioFilesWindow(
                    tracks.Select(t => t.Name).ToList(), dlg.FileNames, isAudioOnly)
                {
                    Owner = Application.Current.MainWindow
                };
                if (win.ShowDialog() != true ||
                    (win.Assignments.Count == 0 && win.NewTrackFiles.Count == 0)) return;

                // Assign audio to existing tracks.
                foreach (var kv in win.Assignments)
                    tracks[kv.Key].TrackView.TrackProps.AudioProps.Filename = kv.Value;

                // Load audio for exactly the assigned + newly created tracks (Global is selected, so
                // LoadSelectedTracksAudio can't be reused).
                var targets = win.Assignments.Keys
                    .Select(i => tracks[i].TrackView.TrackProps.AudioProps).ToList();

                // Create new tracks for the checked files.
                bool createdTracks = win.NewTrackFiles.Count > 0;
                if (createdTracks)
                {
                    var newViews = Project.AddAudioTracks(win.NewTrackFiles);
                    targets.AddRange(newViews.Select(v => v.TrackProps.AudioProps));
                    TrackList.Rebuild(Project);
                }

                OnTrackListSelectionChanged();                 // refresh merged props

                // One undo step for the whole batch. Snapshots don't capture audio buffers, so
                // taking it before the audio load below is correct; CopyPropsFrom reconciles the
                // track set/order on restore, so track creation is undoable like any other edit.
                AddUndoItem(createdTracks ? "Add audio tracks" : "Assign track audio files");

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
                // Spread the default hue by each track's ordinal position among the current non-global
                // tracks (not the raw, possibly-sparse TrackNumber), so the rainbow stays even after
                // the track count changed via add/remove.
                var nonGlobal = Project?.TrackViews
                    .Where(v => v.TrackNumber != 0)
                    .OrderBy(v => v.TrackNumber)
                    .ToList();
                int hueCount = nonGlobal?.Count ?? 0;
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ResetMaterial(
                        nonGlobal?.IndexOf(item.TrackView) ?? 0, hueCount);
                Keyframes.KeyframeService.CaptureDefaultMaterialAtCurrentTick(affected);
                Project?.CreateGeos();
                TrackList.RefreshColors(); // update the normal/hilited swatches in the track list
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

            SelectedTrackProps.ResetAudio = () =>
            {
                if (!Keyframes.KeyframeService.ConfirmDefaultAudioReset(out var affected))
                    return;
                foreach (var item in TrackList.SelectedItems)
                    item.TrackView.TrackProps.ResetAudioSettings();
                Keyframes.KeyframeService.CaptureDefaultAudioAtCurrentTick(affected);
                OnTrackListSelectionChanged();
                AddUndoItem("Reset audio");
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
            TrackList.RemoveSelectedTracks = RemoveSelectedTracks;
            TrackList.AfterReorder = () => AddUndoItem("Reorder tracks");
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

        void RemoveSelectedTracks()
        {
            if (Project == null) return;
            var views = TrackList.SelectedItems
                .Where(it => it.TrackView.TrackNumber != 0)
                .Select(it => it.TrackView).ToList();
            if (views.Count == 0) return;
            Project.RemoveTracks(views);
            TrackList.Rebuild(Project);              // reselects Global
            OnTrackListSelectionChanged();
            Keyframes.KeyframeService.RaiseKeyframesChanged();
            AddUndoItem(views.Count == 1 ? "Remove track" : "Remove tracks");
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
            // Selection count/global flag drive the save/load/default enable state (main menu +
            // context menus) and selection-dependent property getters (e.g. the trigger-algorithm
            // dropdown shows the global track's unset value as the concrete default). Set them
            // BEFORE MergedProps: its setter refreshes all bound properties, which must not read
            // the previous selection's stale flags.
            SelectedTrackProps.SelectedTrackCount = TrackList.SelectedItems.Count;
            SelectedTrackProps.IsOnlyGlobalSelected =
                TrackList.SelectedItems.Count == 1 &&
                TrackList.SelectedItems[0].TrackView.TrackNumber == 0;
            SelectedTrackProps.MergedProps = Project.MergeTrackProps(indices);
            // TrackNumbers (stable MIDI track indices) for the keyframe service.
            var trackNumbers = TrackList.SelectedItems
                .Select(item => item.TrackView.TrackNumber)
                .ToList();
            Keyframes.KeyframeService.SelectedTrackIds = trackNumbers;
            Keyframes.KeyframeService.RaiseKeyframesChanged();
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
            Camera.OnUserUpdating = () =>
            {
                value?.SyncLiveCameraEdit();
                if (ShowSongProps) SongProps.RefreshCamera();
            };
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

            // BindDrawProject + post-load Init share one try so any failure restores the live
            // NoteStyle / SongRenderer.Project / waveform channels (not a half-open temp project).
            ImportOptions io;
            // Non-null once RequireRendererWaveformPanel succeeds — Init (or a later step) may have
            // cleared/replaced the live panel's channels for tempProject; failure must re-wire Project.
            WaveformPanel panelTouched = null;
            try
            {
                // Align NoteStyle + SongRenderer.Project before LoadContent/Init so the draw loop
                // does not keep drawing the previous Project while NoteStyle reads the temp one.
                BindDrawProject(tempProject);

                // A MOD/SID/HVL project re-runs the external remuxer on load (unless its converted
                // outputs are already cached), so show the same progress window a fresh import does.
                // MIDI and audio-only projects rebuild in-process, so no progress window is needed.
                if (tempProject.ImportOptions != null &&
                    tempProject.ImportOptions.NoteFileType is not (FileType.Midi or FileType.Audio))
                {
                    if (!RunConversionBehindProgress((p, ct) => tempProject.LoadContent(p, ct)))
                    {
                        // Conversion was cancelled or produced nothing — keep the current project.
                        RestoreAfterFailedOpen(panelTouched);
                        return;
                    }
                }
                else
                    await tempProject.LoadContent();

                // Re-point tracks at freshly generated per-track WAVs before Init loads audio.
                io = tempProject.ImportOptions;
                var trackAudioTargets = ApplyGeneratedTrackAudio(tempProject, io);
                bool deferTrackAudioLoad = trackAudioTargets.Count > 0;
                // RequireRendererWaveformPanel switches to Song so a Collapsed HwndHost can BuildWindowCore.
                panelTouched = RequireRendererWaveformPanel();
                tempProject.InitAfterDeserialization(panelTouched, loadAudio: !deferTrackAudioLoad);
                if (deferTrackAudioLoad)
                    await LoadTrackAudioAsync(trackAudioTargets);
            }
            catch (FileImportException ex)
            {
                // Loading failed (e.g. the referenced song file couldn't be downloaded). Restore
                // NoteStyle + SongRenderer.Project to the live project (and re-wire waveforms if
                // Init already replaced the panel's channels for the abandoned temp project).
                RestoreAfterFailedOpen(panelTouched);
                MetroMessageBox.Show($"Could not load project file: {ex.Message}\n\nMissing file: {ex.FileName}",
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                RestoreAfterFailedOpen(panelTouched);
                MetroMessageBox.Show("Error loading project: " + ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Pre-fill the import dialog with this project's saved import options so that
            // opening the dialog after loading a project shows the correct file paths and settings
            // (including whether "Generate audio per track" was on when saved).
            // erase: true — LoadContent clears EraseCurrent for merge semantics; the dialog should
            // still default to replace-project after an Open.
            if (io != null)
            {
                if (io.NoteFileType == FileType.Audio)
                    ImportAudioWindow.UpdateSession(io.AudioPath ?? "", erase: true);
                else
                    ImportSongWindow.UpdateSession(io.NoteFileType, erase: true,
                        notePath: io.RawNotePath ?? "", audioPath: io.AudioPath ?? "", insTrack: io.InsTrack,
                        trackAudio: io.TrackAudio);
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
        /// project. Exported files are written with auto-generated names into a "&lt;project&gt;-sources"
        /// subfolder (no per-file save dialogs). This never blocks the save: cancelling the checkbox
        /// dialog just skips the export and the project is still saved.
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

            // Per-track WAVs are savable when any real track references a WAV under the session
            // temp dir that still exists (i.e. not already copied next to the project).
            var trackWavTracks = GetTempTrackAudioTracks();
            bool trackWavsAvail = trackWavTracks.Count > 0;

            if (!midiAvail && !wavAvail && !trackWavsAvail) return;

            var dlg = new Controls.SaveSourceFilesWindow(midiAvail, wavAvail, trackWavsAvail) { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;   // cancelled — skip export, project is still saved

            string dir = Path.GetDirectoryName(projectPath);
            string name = Path.GetFileNameWithoutExtension(projectPath);

            // All exported source files go into a "<project>-sources" subfolder with auto-generated
            // names based on the project name — no per-file save dialogs.
            string sourcesDir = Path.Combine(dir, name + "-sources");
            bool anySave = (dlg.SaveMidi && midiAvail) || (dlg.SaveWav && wavAvail) || (dlg.SaveTrackWavs && trackWavsAvail);
            if (anySave)
            {
                try { Directory.CreateDirectory(sourcesDir); }
                catch (Exception ex)
                {
                    MetroMessageBox.Show($"Could not create sources folder:\n{ex.Message}", Program.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string savedMidiPath = (dlg.SaveMidi && midiAvail)
                ? CopyToSources(io.MidiOutputPath, Path.Combine(sourcesDir, name + ".mid"), "MIDI file")
                : null;
            string savedWavPath = (dlg.SaveWav && wavAvail)
                ? CopyToSources(io.GeneratedAudioPath, Path.Combine(sourcesDir, name + ".wav"), "WAV file")
                : null;

            // Copy per-track WAVs into the same "<project>-sources" folder (renamed to the project
            // name prefix) and re-point each track's filename there (so the serialized paths survive
            // temp cleanup). Regeneration is no longer needed on load — clear TrackAudio and remember
            // the folder so a later re-enable writes here instead of temp.
            if (dlg.SaveTrackWavs && trackWavsAvail)
            {
                SaveTrackAudioFiles(trackWavTracks, sourcesDir, name);
                io.TrackAudio = false;
                io.TrackAudioOutputDir = sourcesDir;
            }

            // Wire the saved WAV as supplied audio (recorded in the project, reused on load).
            if (savedWavPath != null) io.AudioPath = savedWavPath;
            // Saving MIDI turns this into a MIDI project.
            if (savedMidiPath != null) Project.ConvertToMidiProject(savedMidiPath, io.AudioPath);

            if (dlg.SaveTrackWavs && trackWavsAvail && io.NoteFileType != FileType.Audio)
            {
                ImportSongWindow.UpdateSession(io.NoteFileType, erase: true,
                    notePath: io.RawNotePath ?? "", audioPath: io.AudioPath ?? "",
                    insTrack: io.InsTrack, trackAudio: false);
            }
        }

        /// <summary>
        /// Assigns <see cref="ImportOptions.GeneratedTrackAudioPaths"/> onto the project's track
        /// AudioProps. Returns the AudioProps that received new paths (for a deferred load).
        /// </summary>
        static List<AudioProps> ApplyGeneratedTrackAudio(Project project, ImportOptions options)
        {
            var targets = new List<AudioProps>();
            if (project?.TrackViews == null || options?.GeneratedTrackAudioPaths is not { Count: > 0 } trackAudio)
                return targets;

            // TrackViews[0] = Global = MIDI track 0; real tracks align 1:1 with remuxer track numbers.
            foreach (var group in trackAudio.GroupBy(x => x.Track))
            {
                int track = group.Key;
                if (track < 0 || track >= project.TrackViews.Count)
                    continue;
                var ap = project.TrackViews[track].TrackProps.AudioProps;
                // Channel >= 0 → shared channel WAV(s); Channel -1 → one Filename assignment.
                var voiceEntries = group.Where(x => x.Channel >= 0)
                    .OrderBy(x => x.Channel)
                    .Select(x => (x.Channel, x.Path))
                    .ToList();
                if (voiceEntries.Count > 0)
                {
                    ap.VoiceAudioFiles = voiceEntries;
                    ap.Filename = "";
                }
                else
                {
                    ap.VoiceAudioFiles = null;
                    ap.Filename = group.First().Path;
                }
                targets.Add(ap);
            }
            return targets;
        }

        static async Task LoadTrackAudioAsync(List<AudioProps> trackAudioTargets)
        {
            await Task.WhenAll(trackAudioTargets.Select(ap => ap.LoadAudioAsync()));
            var failedAp = trackAudioTargets.FirstOrDefault(ap => !string.IsNullOrEmpty(ap.SidWizChannel.ErrorMessage));
            if (failedAp != null)
            {
                string fn = !string.IsNullOrEmpty(failedAp.Filename)
                    ? failedAp.Filename
                    : failedAp.VoiceAudioFiles?.FirstOrDefault().Path ?? "(track audio)";
                MetroMessageBox.Show($"Couldn't load audio file:\n{fn}", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Real tracks whose per-track audio filename points under the session temp dir and exists.
        /// </summary>
        List<AudioProps> GetTempTrackAudioTracks()
        {
            var result = new List<AudioProps>();
            var tvs = Project?.TrackViews;
            if (tvs == null) return result;
            string tempRoot = Program.TempDir;
            for (int i = 1; i < tvs.Count; i++)   // index 0 = global
            {
                var ap = tvs[i].TrackProps.AudioProps;
                if (ap.VoiceAudioFiles is { Count: > 0 } voices)
                {
                    if (voices.Any(v => v.Path.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(v.Path)))
                        result.Add(ap);
                    continue;
                }
                string fn = ap.Filename;
                if (string.IsNullOrEmpty(fn)) continue;
                if (fn.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fn))
                    result.Add(ap);
            }
            return result;
        }

        /// <summary>
        /// Copies each track's temp WAV into <paramref name="destDir"/>, renaming it to use
        /// <paramref name="projectName"/> as the prefix (keeping the "-chCC.wav" suffix), and
        /// re-points its filename to the copy. Channel WAVs are whole source channels that may be
        /// shared between tracks, so each distinct file is copied once and every referencing track
        /// re-pointed to the same copy. Best-effort; failures leave the original path intact.
        /// </summary>
        static void SaveTrackAudioFiles(List<AudioProps> tracks, string destDir, string projectName)
        {
            try { Directory.CreateDirectory(destDir); }
            catch { return; }

            // Source path → saved copy, shared across all tracks (channel WAVs may be referenced by
            // every instrument track playing on that channel).
            var copied = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string CopyOnce(string path)
            {
                if (copied.TryGetValue(path, out string existing))
                    return existing;
                // Temp track WAVs are named "<noteFile>-chCC.wav"; swap the imported-file prefix for
                // the project name so exported files match the MIDI/master WAV naming. Also accept
                // legacy "-trackNN-<name>.wav" names from older imports.
                string original = Path.GetFileName(path);
                int idx = original.IndexOf("-ch", StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    idx = original.IndexOf("-track", StringComparison.OrdinalIgnoreCase);
                string destName = idx >= 0 ? projectName + original.Substring(idx) : original;
                string dest = Path.Combine(destDir, destName);
                File.Copy(path, dest, true);
                copied[path] = dest;
                return dest;
            }

            foreach (var ap in tracks)
            {
                try
                {
                    if (ap.VoiceAudioFiles is { Count: > 0 } voices)
                    {
                        var newList = new List<(int Channel, string Path)>(voices.Count);
                        foreach (var (channel, path) in voices)
                            newList.Add((channel, CopyOnce(path)));
                        ap.VoiceAudioFiles = newList;
                    }
                    else
                        ap.Filename = CopyOnce(ap.Filename);
                }
                catch { /* best-effort per file */ }
            }
        }

        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destPath"/> (auto-generated, no picker).
        /// Returns the destination path, or null on failure.
        /// </summary>
        static string CopyToSources(string source, string destPath, string label)
        {
            try
            {
                File.Copy(source, destPath, true);
                return destPath;
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show($"Could not save {label}:\n{ex.Message}", Program.AppName,
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

        [RelayCommand]
        async Task ImportAudio()
        {
            var dlg = new ImportAudioWindow { Owner = Application.Current.MainWindow };
            if (dlg.ShowDialog() != true) return;

            var options = new AudioImportOptions
            {
                AudioPath = dlg.AudioFilePath,
                EraseCurrent = dlg.EraseCurrent,
            };
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
            options.TrackAudio = dlg.TrackAudio && fileType != FileType.Midi;
            return options;
        }

        /// <summary>
        /// Runs a MOD/SID remuxer conversion <paramref name="convert"/> behind the shared progress
        /// window so the user sees the conversion advance and can cancel it. Rethrows any job failure
        /// (so callers can surface it). Returns true only when the conversion completed and produced a
        /// song; false if it was cancelled or nothing was imported.
        /// </summary>
        static bool RunConversionBehindProgress(
            Func<IProgress<float>, CancellationToken, Task<bool>> convert)
        {
            bool imported = false;
            bool cancelled = false;
            Exception failure = null;

            var w = new Controls.ProgressWindow(
                "Importing song",
                async cb =>
                {
                    try { imported = await convert(new Progress<float>(cb.UpdateProgress), cb.CancelToken); }
                    catch (OperationCanceledException) { cancelled = true; }
                    catch (Exception ex) { failure = ex; }
                    return null;
                })
            { Owner = Application.Current?.MainWindow };
            w.ShowDialog();

            if (failure != null) throw failure;
            return imported && !cancelled;
        }

        /// <summary>Shared post-dialog import logic.</summary>
        async Task DoImport(ImportOptions options)
        {
            if (options.EraseCurrent && !ConfirmSaveChangesBefore("importing a new song"))
                return;

            // Keep the "<project>-sources" folder once track WAVs were saved next to the project,
            // so re-enabling "Generate audio per track" regenerates there (not temp).
            if (Project?.ImportOptions is { } prev
                && !string.IsNullOrEmpty(prev.TrackAudioOutputDir)
                && string.Equals(prev.RawNotePath ?? "", options.RawNotePath ?? "",
                    StringComparison.OrdinalIgnoreCase))
            {
                options.TrackAudioOutputDir = prev.TrackAudioOutputDir;
            }

            // Ensure a project object exists to import into.
            if (Project == null)
                Project = new Project();

            // Same SongRenderer / NoteStyle / DrawHost alignment Open uses (first import especially:
            // SongRenderer.Project was previously left null until OnProjectLoaded).
            // SetProject can throw on style FX bake failure — catch here like OpenProject does.
            try
            {
                BindDrawProject(Project);
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show("Import failed: " + ex.Message, Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
                if (options.NoteFileType is not (FileType.Midi or FileType.Audio))
                {
                    // MOD/SID are converted by the external remuxer.exe, which reports percentage
                    // progress on stdout. Run it behind the shared progress window so the user sees
                    // the conversion advance and can cancel it. MIDI and audio-only imports take the
                    // direct in-process path (no remuxer, no progress window).
                    if (!RunConversionBehindProgress((p, ct) => Project.ImportSong(options, p, ct)))
                        return;
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

            WaveformPanel panelTouched = null;
            try
            {
                // Assign freshly generated per-track WAVs before InitAfterDeserialization so ownership
                // prep and the single audio load see the correct paths (avoids a second load racing the
                // first and disposing NAudio readers mid-Read).
                var trackAudioTargets = ApplyGeneratedTrackAudio(Project, options);

                // Skip Init's fire-and-forget loads when we have fresh track WAVs — we await one load below.
                bool deferTrackAudioLoad = trackAudioTargets.Count > 0;
                // RequireRendererWaveformPanel switches to Song so a Collapsed HwndHost can BuildWindowCore.
                panelTouched = RequireRendererWaveformPanel();
                Project.InitAfterDeserialization(panelTouched, loadAudio: !deferTrackAudioLoad);

                TrackList.Rebuild(Project);

                if (deferTrackAudioLoad)
                    await LoadTrackAudioAsync(trackAudioTargets);
            }
            catch (Exception ex)
            {
                // ImportSong already mutated Project; keep waveforms + track list in sync with the
                // new tracks, then surface the failure instead of crashing or leaving stale UI.
                RecoverAfterImportInitFailure(panelTouched);
                MetroMessageBox.Show("Import failed: " + ex.Message, Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OnProjectLoaded?.Invoke(Project);
            LoadStaticBackgroundIfUnkeyframed(Project);
            // The project object is reused across an erase-import, so the Song-props panel bindings
            // are stale (OnProjectChanged didn't fire). Push the reset values to the panel.
            SongProps.RefreshAll();

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
            // Did the restore change the track set or order? If so the track-list rows must be
            // rebuilt (and rebuilt BEFORE OnTrackListSelectionChanged, which reseeds the positional
            // indices the per-frame merged-props path relies on).
            bool tracksChanged = Project != null && !Project.TrackViews.Select(v => v.TrackNumber)
                .SequenceEqual(_undoItems.Current.Project.TrackViews.Select(v => v.TrackNumber));
            Project?.CopyPropsFrom(_undoItems.Current.Project);
            // Reload style FX (DCS clone drops Effect refs) and rebuild geometry for restored props.
            // resetVertScale: true — CopyPropsFrom restores SpatialProps but keeps live Geo; must not
            // re-bake at the pre-undo RefWidthQn (CreateGeos(false) would).
            Project?.LoadStyleFxAndCreateGeos(resetVertScale: true);
            // Refresh the Song and Track property panels so they reflect restored values.
            SongProps.RefreshAll();
            if (tracksChanged) TrackList.Rebuild(Project);
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

        // Global shortcut (Ctrl+I); works regardless of track-list focus. The actual selection
        // toggle lives in the view (it owns the ListView), reached via TrackList.InvertSelection.
        [RelayCommand(CanExecute = nameof(HasProject))]
        void InvertTrackSelection() => TrackList.InvertSelection?.Invoke();

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

        /// <summary>
        /// Optional: force layout so a newly Visible <see cref="MonoGameHost"/> can run BuildWindowCore
        /// before Open/Import call <see cref="Project.InitAfterDeserialization"/>.
        /// </summary>
        public Action EnsureSongHostReady { get; set; }

        /// <summary>
        /// Keeps <see cref="SongRenderer.Project"/> / <see cref="NoteStyle"/> / DrawHost aligned.
        /// Open binds the temp project before LoadContent/Init; failure restore binds the live project.
        /// </summary>
        public Action<Project> SyncRendererProject { get; set; }

        /// <summary>
        /// Point NoteStyle and the live SongRenderer at <paramref name="project"/> so DrawSong and
        /// style globals stay consistent (Open binds temp before VM.Project swaps on success).
        /// </summary>
        internal void BindDrawProject(Project project)
        {
            SyncRendererProject?.Invoke(project);
            // SyncRenderer no-ops when the host is not built yet — still need NoteStyle for LoadContent.
            // Idempotent when SyncRenderer already applied SongRenderer.Project (setter calls SetProject).
            var drawHost = GetDrawHost?.Invoke();
            if (drawHost != null) Project.SetDrawHost(drawHost);
            NoteStyle.SetProject(project);
        }

        /// <summary>
        /// Switches to the Song screen (so the HwndHost is Visible), optionally forces layout, and
        /// returns the live WaveformPanel. Throws if the host still has no panel — Open/Import must
        /// not leave stale SidWiz channels from a previous project.
        /// </summary>
        WaveformPanel RequireRendererWaveformPanel()
        {
            CurrentScreen = AppScreen.Song;
            EnsureSongHostReady?.Invoke();
            var wfp = GetRendererWaveformPanel?.Invoke();
            if (wfp == null)
                throw new InvalidOperationException(
                    "WaveformPanel is unavailable (MonoGame host not ready). Open the Song screen and try again.");
            return wfp;
        }

        /// <summary>
        /// After a failed Open: restore NoteStyle + SongRenderer to the live <see cref="Project"/>.
        /// When <paramref name="panelTouched"/> is non-null, Init (or a later step) may have replaced
        /// that panel's channels for the abandoned temp project — re-wire the live project's channels
        /// without reloading audio.
        /// </summary>
        internal void RestoreAfterFailedOpen(WaveformPanel panelTouched)
        {
            BindDrawProject(Project);
            RewireWaveformChannels(panelTouched);
        }

        /// <summary>
        /// Re-attach the live project's SidWiz channels after Init cleared/replaced the panel.
        /// Best-effort: swallows re-Init failures so the original Open/Import error still surfaces.
        /// </summary>
        internal void RewireWaveformChannels(WaveformPanel panelTouched)
        {
            if (panelTouched == null || Project?.TrackViews == null)
                return;
            try
            {
                Project.InitAfterDeserialization(panelTouched, loadAudio: false);
            }
            catch
            {
                // Leave panel as-is; caller still reports the original failure.
            }
        }

        /// <summary>
        /// After ImportSong succeeded but Init / track-audio load failed: re-bind the renderer
        /// (early BindDrawProject may have run before the MonoGame host existed), always re-run
        /// <see cref="Project.InitAfterDeserialization"/> so ownership/tempo/accessors refresh even
        /// when <paramref name="panelTouched"/> is null (RequireRenderer threw before assignment),
        /// load per-track audio (deferred LoadTrackAudioAsync may have been the failure), and rebuild
        /// the track list / song props so the UI matches the already mutated <see cref="Project"/>
        /// (Import reuses the same object, so OnProjectChanged does not fire).
        /// </summary>
        internal void RecoverAfterImportInitFailure(WaveformPanel panelTouched)
        {
            // RequireRendererWaveformPanel may have built the host after the pre-import BindDrawProject
            // no-op'd (SyncRenderer returns when Renderer is null). Rebind so SongRenderer.Project
            // matches NoteStyle / the mutated Project instead of staying null (black Song view).
            try { BindDrawProject(Project); }
            catch { /* best-effort; caller still reports the original failure */ }

            // Prefer the panel Init already touched; otherwise retry GetRenderer (host may exist now).
            // Unlike Open restore, Import mutates Project in place — always Init so stale channels /
            // missing PrepareVoiceOwnership / unloaded deferred track audio are not left behind.
            var panel = panelTouched ?? GetRendererWaveformPanel?.Invoke();
            try
            {
                if (Project?.TrackViews != null)
                {
                    if (panel != null)
                        Project.InitAfterDeserialization(panel, loadAudio: true);
                    else
                        Project.InitAfterDeserialization(null, loadAudio: true, allowMissingWaveformPanel: true);
                }
            }
            catch
            {
                // Leave panel as-is; caller still reports the original failure.
            }

            TrackList.Rebuild(Project);
            SongProps.RefreshAll();
            OnPropertyChanged(nameof(HasAudio));
            TogglePlaybackCommand.NotifyCanExecuteChanged();
            GoToBeginningCommand.NotifyCanExecuteChanged();
            GoToEndCommand.NotifyCanExecuteChanged();
            NudgeBackCommand.NotifyCanExecuteChanged();
            NudgeForwardCommand.NotifyCanExecuteChanged();
            JumpBackCommand.NotifyCanExecuteChanged();
            JumpForwardCommand.NotifyCanExecuteChanged();
        }

        // ---- IImportService (browser download import) ----

        public void ImportFromUrl(string url, string suggestedFileName, FileType preferredFileType)
        {
            string ext = suggestedFileName.Split('.').Last().ToLower();

            FileType? fileType = ImportFileFormats.FromExtension(ext, preferredFileType);

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
            options.TrackAudio = fileType.Value != FileType.Midi && AppSettings.Instance.GetTrackAudio(fileType.Value);
            ImportSongWindow.UpdateSession(fileType.Value, erase: true, notePath: url, audioPath: "",
                insTrack: AppSettings.Instance.GetInsTrack(fileType.Value),
                trackAudio: AppSettings.Instance.GetTrackAudio(fileType.Value));

            // SID downloads may contain multiple sub-songs — let the user pick.
            if (!SelectSidSubSong(options)) return;

            _ = DoImport(options);   // fire-and-forget on the UI thread (async void pattern)
        }
    }
}
