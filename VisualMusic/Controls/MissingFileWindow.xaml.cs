using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace VisualMusic.Controls
{
    public partial class MissingFileWindow : MetroWindow
    {
        public MissingFileAction Action { get; private set; } = MissingFileAction.Abort;

        public MissingFileWindow(MissingFileRef file)
        {
            InitializeComponent();
            messageBlock.Text = $"{file.RoleLabel} is missing:\n\n{file.Path}";
        }

        void Browse_Click(object sender, RoutedEventArgs e)
        {
            Action = MissingFileAction.Browse;
            DialogResult = true;
        }

        void SearchFolder_Click(object sender, RoutedEventArgs e)
        {
            Action = MissingFileAction.SearchFolder;
            DialogResult = true;
        }

        void Ignore_Click(object sender, RoutedEventArgs e)
        {
            Action = MissingFileAction.Ignore;
            DialogResult = true;
        }

        void Abort_Click(object sender, RoutedEventArgs e)
        {
            Action = MissingFileAction.Abort;
            DialogResult = false;
        }
    }

    /// <summary>WPF implementation of <see cref="IMissingFilePrompt"/> for project Open.</summary>
    public sealed class WpfMissingFilePrompt : IMissingFilePrompt
    {
        readonly Window _owner;

        public WpfMissingFilePrompt(Window owner)
        {
            _owner = owner;
        }

        public MissingFileAction Prompt(MissingFileRef file)
        {
            var dlg = new MissingFileWindow(file) { Owner = _owner };
            dlg.ShowDialog();
            return dlg.Action;
        }

        public string PickReplacementFile(MissingFileRef file)
        {
            var dlg = new OpenFileDialog
            {
                Filter = file.OpenFilter,
                FileName = ProjectMissingFiles.FileNameForSearch(file.Path)
            };
            string startDir = InitialDirectory(file);
            if (!string.IsNullOrEmpty(startDir) && Directory.Exists(startDir))
                dlg.InitialDirectory = startDir;
            if (dlg.ShowDialog(_owner) != true)
                return null;

            RememberFolder(file, dlg.FileName);
            return dlg.FileName;
        }

        public string PickSearchFolder(MissingFileRef file)
        {
            var dlg = new OpenFolderDialog
            {
                Title = "Select folder to search"
            };
            string startDir = InitialDirectory(file);
            if (!string.IsNullOrEmpty(startDir) && Directory.Exists(startDir))
                dlg.InitialDirectory = startDir;
            if (dlg.ShowDialog(_owner) != true)
                return null;
            return dlg.FolderName;
        }

        public void NotifyNotFound(string fileName, string directory)
        {
            MetroMessageBox.Show(_owner,
                $"Could not find {fileName} under:\n{directory}",
                Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        static string InitialDirectory(MissingFileRef file)
        {
            try
            {
                if (!string.IsNullOrEmpty(file.Path) && !file.Path.IsUrl())
                {
                    string dir = Path.GetDirectoryName(file.Path);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        return dir;
                }
            }
            catch { /* fall back */ }

            var s = AppSettings.Instance;
            return file.Kind switch
            {
                MissingFileKind.Note => s.GetNoteFolder(file.NoteFileType) ?? "",
                MissingFileKind.MasterAudio => s.GetAudioFolder(file.NoteFileType) ?? "",
                MissingFileKind.TrackAudio => s.TrackAudioFolder ?? "",
                MissingFileKind.Texture or MissingFileKind.Heightmap => s.TextureFolder ?? "",
                MissingFileKind.Background => s.BackgroundFolder ?? "",
                _ => ""
            };
        }

        static void RememberFolder(MissingFileRef file, string pickedPath)
        {
            var s = AppSettings.Instance;
            switch (file.Kind)
            {
                case MissingFileKind.Note:
                    s.RememberFolder(pickedPath, dir => s.SetNoteFolder(file.NoteFileType, dir));
                    break;
                case MissingFileKind.MasterAudio:
                    s.RememberFolder(pickedPath, dir => s.SetAudioFolder(file.NoteFileType, dir));
                    break;
                case MissingFileKind.TrackAudio:
                    s.RememberFolder(pickedPath, dir => s.TrackAudioFolder = dir);
                    break;
                case MissingFileKind.Texture:
                case MissingFileKind.Heightmap:
                    s.RememberFolder(pickedPath, dir => s.TextureFolder = dir);
                    break;
                case MissingFileKind.Background:
                    s.RememberFolder(pickedPath, dir => s.BackgroundFolder = dir);
                    break;
            }
        }
    }
}
