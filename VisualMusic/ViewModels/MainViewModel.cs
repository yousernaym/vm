using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;

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

        // ---- Renderer callbacks (set by MainWindow after load) ----

        /// <summary>Called after a project is fully loaded; receiver must set the project on the renderer.</summary>
        public Action<Project> OnProjectLoaded { get; set; }
        /// <summary>Called to load the background image on the renderer.</summary>
        public Action<string> OnLoadBackgroundImage { get; set; }

        // ---- Folder memory for file dialogs ----

        string projectFolder = Path.Combine(Program.DefaultUserFilesDir, "Projects");
        string camFolder = Path.Combine(Program.DefaultUserFilesDir, "Props");
        string trackPropsFolder = Path.Combine(Program.DefaultUserFilesDir, "Props");

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
                InitialDirectory = projectFolder
            };
            if (dlg.ShowDialog() != true) return;

            string path = dlg.FileName;
            projectFolder = Path.GetDirectoryName(path);

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

            // Set static project reference before loadContent so NoteStyle.createGeoChunk
            // (called from createOcTrees inside loadContent) can access Project.Props.
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
                InitialDirectory = projectFolder,
                FileName = project.DefaultFileName
            };
            if (dlg.ShowDialog() != true) return;
            projectFolder = Path.GetDirectoryName(dlg.FileName);
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
        void ImportMidi()
            => MessageBox.Show("Import MIDI not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand]
        void ImportMod()
            => MessageBox.Show("Import Module not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand]
        void ImportSid()
            => MessageBox.Show("Import SID not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

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
                InitialDirectory = camFolder
            };
            if (dlg.ShowDialog() != true) return;
            camFolder = Path.GetDirectoryName(dlg.FileName);
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
            var dlg = new SaveFileDialog { Filter = "Camera files|*.cam|All files|*.*", InitialDirectory = camFolder };
            if (dlg.ShowDialog() != true) return;
            camFolder = Path.GetDirectoryName(dlg.FileName);
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
            => MessageBox.Show("Load Track Properties not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand(CanExecute = nameof(HasProject))]
        void SaveTrackProps()
            => MessageBox.Show("Save Track Properties not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand(CanExecute = nameof(HasProject))]
        void DefaultTrackProps()
            => MessageBox.Show("Default Track Properties not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand(CanExecute = nameof(HasProject))]
        void InsertLyrics()
            => MessageBox.Show("Insert Lyrics not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        [RelayCommand(CanExecute = nameof(HasProject))]
        void InsertKeyFrame()
            => MessageBox.Show("Insert Key Frame not yet available in WPF mode.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

        // ---- Renderer WaveformPanel accessor (set by MainWindow) ----

        public Func<WaveformPanel> GetRendererWaveformPanel { get; set; }

        // ---- IImportService (browser import — Phase 6) ----

        public void ImportFromUrl(string url, string suggestedFileName)
        {
            string ext = suggestedFileName.Split('.').Last().ToLower();
            MessageBox.Show(
                $"Import from browser not yet supported in WPF mode.\n\nFile: {suggestedFileName}",
                Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
