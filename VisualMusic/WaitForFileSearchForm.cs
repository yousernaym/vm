using System;
using System.IO;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class WaitForFileSearchForm : WaitForTaskForm
    {
        public WaitForFileSearchForm()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(string searchDir, string fileName)
        {
            dirTb.Text = searchDir;
            return base.ShowDialog(() => findFile(searchDir, fileName), "Searching in folder");
        }

        //Search dir recursively
        //Return path to file if found, otherwise null
        string findFile(string searchDir, string fileName)
        {
            WaitForTaskForm.CancellationToken.ThrowIfCancellationRequested();
            string[] paths = null;

            // Exclude some directories according to their attributes
            var dirInfo = new DirectoryInfo(searchDir);
            var isroot = dirInfo.Root.FullName.Equals(dirInfo.FullName);

            // as root dirs (e.g. "C:\") apparently have the system + hidden flags set, we must check whether it's a root dir, if it is, we do NOT skip it even though those attributes are present
            // We must not access such folders/files, or this crashes with UnauthorizedAccessException on folders like $RECYCLE.BIN
            if (!dirInfo.Attributes.HasFlag(FileAttributes.System) || isroot)
            {
                try
                {
                    paths = Directory.GetFiles(searchDir);
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }
                catch (PathTooLongException)
                {
                    return null;
                }
            }
            else
                return null;

            //Check if there is a match in current dir
            foreach (var path in paths)
            {
                var attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) != FileAttributes.Directory && Path.GetFileName(path) == fileName)
                    return path;
            }

            //Check all sub directories
            try
            {
                var dirs = Directory.GetDirectories(searchDir);
                foreach (var dir in dirs)
                {
                    var path = findFile(dir, fileName); // recursive call
                    if (path != null)
                        return path;
                }
            }
            catch (PathTooLongException ex)
            {
                Form1.showErrorMsgBox(ex.Message);
            }
            return null;
        }
    }
}
