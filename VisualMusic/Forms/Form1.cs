using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
//using CefSharp.Example.RequestEventHandler;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
    using GdiColor = System.Drawing.Color;
    using GdiPoint = System.Drawing.Point;
    using XnaColor = Microsoft.Xna.Framework.Color;

    enum TrackPropsType { TPT_Style = 1, TPT_Material = 2, TPT_Light = 4, TPT_Spatial = 8, TPT_Audio = 16, TPT_All = 255 }

    public partial class Form1 : Form
    {
        readonly string[] _trackPropsTypeNames = { "Style", "Material", "Light", "Spatial" };
        string[] _startupArgs;
        int _trackTexPbHeight;
        int _maxTrackTexPbWidth;

        public string ProjectFolder
        {
            get => openProjDialog.InitialDirectory;
            set => openProjDialog.InitialDirectory = saveProjDialog.InitialDirectory = value;
        }

        public string TrackPropsFolder
        {
            get => openTrackPropsFileDialog.InitialDirectory;
            set => openTrackPropsFileDialog.InitialDirectory = saveTrackPropsFileDialog.InitialDirectory = value;
        }

        public string CamFolder
        {
            get => openCamFileDialog.InitialDirectory;
            set => openCamFileDialog.InitialDirectory = saveCamFileDialog.InitialDirectory = value;
        }

        public string BackgroundImageFolder
        {
            get => openBkgDialog.InitialDirectory;
            set => openBkgDialog.InitialDirectory = value;
        }

        public string TrackAudioFolder
        {
            get => openTrackAudioDlg.InitialDirectory;
            set => openTrackAudioDlg.InitialDirectory = value;
        }


        string _currentProjPath = "";

        public ListView.ListViewItemCollection trackListItems
        {
            get { return trackList.Items; }
        }

        bool AudioLoaded { get { return Media.GetAudioLength() > 0; } }// !isEmpty(importMidiForm.AudioFilePath) || !isEmpty(importModForm.AudioFilePath) || !isEmpty(importSidForm.AudioFilePath); } }

        Graphics _trackListGfxObj;
        private static readonly Pen s_pen = new(GdiColor.White);
        readonly Pen _trackListPen = s_pen;
        bool _updatingControls = false;
        bool _updatingCamControls = false;
        public bool UpdatingControls => _updatingControls;
        public ListView TrackList => trackList;

        TrackProps _mergedTrackProps;
        const string trackPropsBtnText = "&Track Properties";
        public static ImportMidiForm ImportMidiForm;
        public static ImportModForm ImportModForm;
        public static ImportSidForm ImportSidForm;
        static TpartyIntegrationForm s_tpartyIntegrationForm;
        public static TpartyIntegrationForm TpartyIntegrationForm => s_tpartyIntegrationForm;
        public static VideoExportForm VidExpForm;
        LocateFile _locateFileDlg;

        static public Type[] projectSerializationTypes => ProjectSerializer.KnownTypes;
        static public SongPanel SongPanel { get; private set; } = new SongPanel();
        SongWebBrowser _modWebBrowser;
        SongWebBrowser _sidWebBrowser;
        SongWebBrowser _midiWebBrowser;
        List<Control> _screens = new List<Control>();
        Control _currentScreen;
        public Project Project { get; private set; } = new Project();
        UndoItems _undoItems = new UndoItems();
        static Settings s_settings = new Settings();
        public static Settings Settings { get => s_settings; }
        ScrollBar _songScrollBar = new HScrollBar();
        NoteStyleControl _currentNoteStyleControl;
        int _keyFrameLockRow = -1;
        bool _unsavedChanges = false;
        bool _viewWidthQnChangedWithCtrl = false;
        static public Process RemuxerProcess;

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        static extern void Keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public Form1(string[] args)
        {
            InitializeComponent();

            UpdateFormTitle("");

            //Turn off caps lock
            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                PressCapsLock();
            }

            Camera.OnUserUpdating = UpdateCamControls;
            Camera.OnUserUpdated = () => AddUndoItem("Edit Camera");

            SongPanel.Project = Project;
            _startupArgs = args;
            _trackTexPbHeight = trackTexPb.Height;
            _maxTrackTexPbWidth = trackTexPb.Width;
            _trackListGfxObj = trackList.CreateGraphics();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint, true);

            ImportMidiForm = new ImportMidiForm(this);
            ImportModForm = new ImportModForm(this);
            ImportSidForm = new ImportSidForm(this);
            s_tpartyIntegrationForm = new TpartyIntegrationForm();
            VidExpForm = new VideoExportForm();
            _locateFileDlg = new LocateFile();
            ResizeRedraw = true;

            _songScrollBar.Dock = DockStyle.Bottom;
            _songScrollBar.ValueChanged += SongScrollBar_ValueChanged;
            _songScrollBar.Scroll += SongScrollBar_Scroll;
            Controls.Add(_songScrollBar);
            _songScrollBar.BringToFront();

            _modWebBrowser = new SongWebBrowser(this, "https://modarchive.org/index.php?request=view_searchbox");
            _sidWebBrowser = new SongWebBrowser(this, "https://www.exotica.org.uk/wiki/Special:HVSC");
            _midiWebBrowser = new SongWebBrowser(this, "https://bitmidi.com/");

            _screens.Add(SongPanel);
            _screens.Add(_modWebBrowser);
            _screens.Add(_sidWebBrowser);
            _screens.Add(_midiWebBrowser);
            foreach (var screen in _screens)
            {
                screen.Dock = DockStyle.Fill;
                screen.Visible = true;
                Controls.Add(screen);
                screen.BringToFront();
            }
            InitSongPanel();

            Array enumArray = Enum.GetValues(typeof(NoteStyleType));
            foreach (NoteStyleType nse in enumArray)
                styleList.Items.Add(nse.ToString());

            AddEventHandlers(this.Controls);

            string[] tptList = Enum.GetNames(typeof(TrackPropsType));
            for (int i = 0; i < selectedTrackPropsPanel.TabPages.Count; i++)
            {
                selectedTrackPropsPanel.TabPages[i].Name = tptList[i];
                selectedTrackPropsPanel.TabPages[i].ContextMenuStrip = trackPropsTabCM;
            }

            TrackPropsFolder = CamFolder = Path.Combine(Program.DefaultUserFilesDir, "Props");
            ProjectFolder = Path.Combine(Program.DefaultUserFilesDir, "Projects");
            saveVideoDlg.InitialDirectory = Path.Combine(Program.DefaultUserFilesDir, "Videos");
            openTrackPropsFileDialog.Filter = saveTrackPropsFileDialog.Filter = "Track property files|*.tp|All files|*.*";
            openCamFileDialog.Filter = saveCamFileDialog.Filter = "Camera files|*.cam|All files|*.*";
            openProjDialog.Filter = $"Visual Music projects |*.{Project.DefaultFileExt}|All files|*.*";
            saveProjDialog.Filter = $"Visual Music projects |*.{Project.DefaultFileExt}|All files|*.*";
            openTrackAudioDlg.Filter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*";
        }

        static public void PressCapsLock()
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            Keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            Keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Download.Init(this);
            try
            {
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in " + Settings.FilePath, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //upDownVpWidth.Focus();
            //upDownVpWidth.Value = SongPanel.Qn_viewWidth;

            bool bSongFile = false;
            bool bMidiFile = false;
            bool bModFile = false;
            bool bSidFile = false;
            bool bImportFiles = false;
            string audioFile = null;

            foreach (string arg in _startupArgs)
            {
                string ext = arg.Split('.').Last().ToLower();
                string inputFilePath = Path.GetFullPath(arg);

                //If no previous note file has been encountered, check if this extension is a note file
                if (!bImportFiles)
                {
                    if (ImportMidiForm.Formats.Contains(ext))
                    {
                        ImportMidiForm.NoteFilePath = inputFilePath;
                        bMidiFile = bImportFiles = true;
                    }
                    else if (ImportModForm.Formats.Contains(ext))
                    {
                        ImportModForm.NoteFilePath = inputFilePath;
                        bModFile = bImportFiles = true;
                    }
                    else if (ImportSidForm.Formats.Contains(ext))
                    {
                        ImportSidForm.NoteFilePath = inputFilePath;
                        bSidFile = bImportFiles = true;
                    }
                }
                if (audioFile == null && ext == "wav")
                {
                    audioFile = inputFilePath;
                }
                else if (ext == Project.DefaultFileExt && !bImportFiles)
                {
                    OpenProjectFile(inputFilePath);
                    bSongFile = true;
                    break;
                }
            }
            if (!bSongFile && bImportFiles)
            {
                try
                {
                    if (bMidiFile)
                    {
                        ImportMidiForm.AudioFilePath = audioFile;
                        ImportMidiForm.ImportFiles();
                    }
                    if (bModFile)
                    {
                        ImportModForm.AudioFilePath = audioFile;
                        ImportModForm.ImportFiles();
                    }
                    else if (bSidFile)
                    {
                        ImportSidForm.AudioFilePath = audioFile;
                        ImportSidForm.ImportFiles();
                    }
                }
                catch (FileImportException ex)
                {
                    ShowErrorMsgBox(ex.Message + "\r\n" + ex.FileName);
                }
            }
        }

        void AddMenuItemEventHandlers(ToolStripItemCollection items)
        {
            foreach (var item in items)
            {
                if (item is ToolStripMenuItem)
                {
                    ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
                    menuItem.Click += AddUndoItem;
                    menuItem.Click += InvalidateSongPanel;
                    AddMenuItemEventHandlers(menuItem.DropDownItems);
                }
            }
        }

        void AddEventHandlers(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control.ContextMenuStrip != null)
                    AddMenuItemEventHandlers(control.ContextMenuStrip.Items);
                if (control.GetType() == typeof(TextBox))
                {
                    ((TextBox)control).Validated += InvalidateSongPanel;
                    ((TextBox)control).Validated += AddUndoItem;
                }
                else if (control.GetType() == typeof(TrackBar))
                {
                    control.Validated += AddUndoItem;
                }
                else if (control.GetType() == typeof(NumericUpDown))
                {
                    ((NumericUpDown)control).ValueChanged += InvalidateSongPanel;
                    ((NumericUpDown)control).Validated += AddUndoItem;
                }
                else if (control.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)control).CheckedChanged += InvalidateSongPanel;
                    ((CheckBox)control).Click += AddUndoItem;
                }
                else if (control.GetType() == typeof(Button))
                {
                    ((Button)control).Click += InvalidateSongPanel;
                    ((Button)control).Click += AddUndoItem;
                }
                else if (control.GetType() == typeof(RadioButton))
                {
                    ((RadioButton)control).CheckedChanged += InvalidateSongPanel;
                    ((RadioButton)control).Click += AddUndoItem;
                }
                else if (control.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)control).SelectedIndexChanged += InvalidateSongPanel;
                    ((ComboBox)control).SelectedIndexChanged += AddUndoItem;
                }
                else if (control.GetType() == typeof(HueSatButton))
                {
                    ((HueSatButton)control).ColorChanged += InvalidateSongPanel;
                    ((HueSatButton)control).ColorSubmitted += AddUndoItem;
                }
                else if (control.GetType() == typeof(DataGridView))
                {
                    ((DataGridView)control).CellEndEdit += AddUndoItem;
                    ((DataGridView)control).RowsRemoved += AddUndoItem;
                }
                else if (control.GetType() == typeof(Label))
                    ((Label)control).Click += AddUndoItem;

                else if (control.GetType() == typeof(MenuStrip))
                    AddMenuItemEventHandlers(((MenuStrip)control).Items);
                else if (control.Controls.Count > 0)
                    AddEventHandlers(control.Controls);
                else
                    control.Click += InvalidateSongPanel;
            }
        }


        private void ImportMidiSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportMidiForm.Hide();
            if (ImportMidiForm.ShowDialog(this) == DialogResult.OK)
                SongPanel.Focus();
        }

        private void ImportModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportModForm.Hide();
            if (ImportModForm.ShowDialog(this) == DialogResult.OK)
                SongPanel.Focus();
        }

        void SongLoaded(string path)
        {
            bool loaded = !string.IsNullOrEmpty(path);
            propsTogglePanel.Enabled = loaded;
            if (!loaded)
            {
                trackPropsCb.Checked = false;
                songPropsCb.Checked = false;
            }

            saveSongToolStripMenuItem.Enabled = loaded;
            saveSongAsToolStripMenuItem.Enabled = loaded;
            exportVideoToolStripMenuItem.Enabled = loaded;
            playbackToolStripMenuItem.Enabled = AudioLoaded;
            editToolStripMenuItem.Enabled = loaded;
            loadCamToolStripMenuItem.Enabled = loaded;
            saveCamToolStripMenuItem.Enabled = loaded;
            insertLyricsHereToolStripMenuItem.Enabled = loaded;
            loadTrackPropsToolStripMenuItem.Enabled = loaded;
            saveTrackPropsToolStripMenuItem.Enabled = loaded;
            insertKeyFrameToolStripMenuItem.Enabled = loaded;

            CreateTrackList();
            UpdateTrackPropsControls();
            UpdateProjPropsControls();

            _undoItems.Clear();
            undoToolStripMenuItem.Enabled = redoToolStripMenuItem.Enabled = false;
            _undoItems.Add("", Project);
            UpdateUndoRedoDesc();
            //project.KeyFrames[0].Camera.SpatialChanged();// = updateCamControls;
            UpdateScrollBarChange();
            ChangeToScreen(SongPanel);
        }

        private void UpdateProjPropsControls()
        {
            _updatingControls = true;
            upDownVpWidth.Value = Project.KeyFrames[0].ProjProps.ViewWidthQn;
            audioOffsetS.Value = (decimal)Project.Props.AudioOffset;
            Project.Props.PlaybackOffsetS = Project.Props.PlaybackOffsetS;
            playbackOffsetUd.Value = (decimal)Project.Props.PlaybackOffsetS;
            _songScrollBar.Maximum = (int)Project.SongLengthT;
            _songScrollBar.Value = (int)Project.SongPosT;
            fadeInUd.Value = (decimal)Project.Props.FadeIn;
            fadeOutUd.Value = (decimal)Project.Props.FadeOut;
            maxPitchUd.Value = Project.Props.MaxPitch;
            minPitchUd.Value = Project.Props.MinPitch;
            BuildKeyFramesDGV();
            UpdateCamControls();
            lyricsGridView.DataSource = Project.Props.LyricsSegments;
            bkgOpacityUd.Value = (decimal)Project.Props.BackgroundImageOpacity;
            bkgSaturationUd.Value = (decimal)Project.Props.BackgroundImageSaturation;
            _updatingControls = false;
        }

        private void BuildKeyFramesDGV()
        {
            keyFramesDGV.Rows.Clear();
            foreach (var frame in Project.KeyFrames)
                keyFramesDGV.Rows.Add(frame.Key, frame.Value.Desc);
            keyFramesDGV.CurrentCell = keyFramesDGV.Rows[0].Cells[0];
        }

        private void UpdateCamControls()
        {
            if (Project == null)
                return;
            _updatingCamControls = true;
            Vector3 pos = Project.Props.Camera.Pos;
            Quaternion orient = Project.Props.Camera.Orientation;
            camTb.Text = $"{pos.X}\r\n{pos.Y}\r\n{pos.Z}\r\n\r\n{orient.X}\r\n{orient.Y}\r\n{orient.Z}\r\n{orient.W}";
            _updatingCamControls = false;
        }

        //Called only when iomporting note and audio files.
        async public Task<bool> OpenSourceFiles(ImportOptions options, Form parentForm)
        {
            SaveSettings();
            ChangeToScreen(SongPanel); //Hide browsers if they haven't been hidden yet. Otherwise the last browser will be brought to front during loading.Hopefully they have had time to initialize.
            try
            {
                SongPanel.SuspendPaint();
                if (!parentForm.Visible)
                    parentForm = this;
                if (!await Project.ImportSong(options, parentForm))
                    return false;
                if (options.EraseCurrent)
                {
                    _currentProjPath = "";
                    UpdateFormTitle("");
                    ResetCameras(false);
                }
                _unsavedChanges = true;
                SongLoaded(options.NotePath);
            }
            finally
            {
                SongPanel.ResumePaint();
            }
            return true;
        }

        void SaveSettings()
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(Settings), Settings.Types);
            using (FileStream stream = File.Open(Settings.FilePath, FileMode.Create))
            {
                dcs.WriteObject(stream, s_settings);
            }
        }

        void LoadSettings()
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(Settings), Settings.Types);
            if (File.Exists(Settings.FilePath))
            {
                using (FileStream stream = File.Open(Settings.FilePath, FileMode.Open))
                {
                    s_settings = (Settings)dcs.ReadObject(stream);
                }
            }
        }

        private void SongPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Project.Notes == null)
                return;
            double delta = (double)Math.Sign(e.Delta);

            if (ModifierKeys.HasFlag(Keys.Control)) //Change view width
            {
                _viewWidthQnChangedWithCtrl = true;
                upDownVpWidth.Value *= (float)Math.Pow(1.1, -delta);
            }
            else //scroll
            {
                bool wasPlaying;
                if (wasPlaying = Project.IsPlaying)
                    Project.TogglePlayback();

                delta /= Project.Notes.SongLengthT; //Scroll one tick
                if (ModifierKeys.HasFlag(Keys.Shift))
                    delta *= Project.LargeScrollStepT;   //default large-step scroll is one "page"
                else
                    delta *= Project.SmallScrollStepT;   //default small-step scroll is 1/16 of one "page" //(=one quarter note with default view width of 16 quarter notes)

                Project.NormSongPos -= delta;

                if (wasPlaying)
                    Project.TogglePlayback();
            }
        }

        private void ExportVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VidExpForm.ShowDialog() != DialogResult.OK)
                return;
            if (saveVideoDlg.ShowDialog(this) != DialogResult.OK)
                return;
            saveVideoDlg.InitialDirectory = Path.GetDirectoryName(saveVideoDlg.FileName);

            SaveSettings();

            //var scBackup = Camera.SpatialChanged;
            //Camera.SpatialChanged = null;
            using (RenderProgressForm renderProgressForm = new RenderProgressForm(SongPanel, saveVideoDlg.FileName, VidExpForm.Options))
                renderProgressForm.ShowDialog();
            //Camera.SpatialChanged = scBackup;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.MediaPlayPause)
                Project.TogglePlayback();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (Project == null || Project.Notes == null)
                return;

            var keyFrame = Project.GetKeyFrameAtSongPos();
            if (keyFrame != null)
                keyFrame.ProjProps.Camera.Control(e.KeyCode, false, Form1.ModifierKeys);

            if (e.KeyCode == Keys.Z)
            {
                SongPanel.ForceDefaultNoteStyle = false;
                for (int t = 1; t < Project.TrackViews.Count; t++)
                {
                    TrackProps tprops = Project.TrackViews[t].TrackProps;
                    Project.TrackViews[t].CreateGeo(Project, Project.GlobalTrackProps);
                }
            }
            else if (e.KeyCode == Keys.ControlKey && _viewWidthQnChangedWithCtrl)
            {
                CommitViewWidthQnChange();
            }
        }

        private void CommitViewWidthQnChange()
        {
            if (Project.ViewWidthQnScale == 1)
                return;
            Project.CreateGeos();
            SongPanel.Invalidate();
            AddUndoItem("Edit Viewport Width");
        }

        private void UpDownVpWidth_ValueChanged(object sender, EventArgs e)
        {
            UpdateScrollBarChange();
            if (_updatingControls)
                return;
            foreach (var keyFrame in Project.KeyFrames.Values)
            {
                if (keyFrame.Selected)
                    keyFrame.ProjProps.ViewWidthQn = (float)((TbSlider)sender).Value;
            }
        }

        private void UpdateScrollBarChange()
        {
            _songScrollBar.SmallChange = Math.Max(Project.SmallScrollStepT, 0);
            _songScrollBar.LargeChange = Math.Max(Project.LargeScrollStepT, 0);
        }

        private void AudioOffsetS_ValueChanged(object sender, EventArgs e)
        {
            Project.Props.AudioOffset = (float)audioOffsetS.Value;
        }

        private void PlaybackOffsetUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            decimal songLengthWithoutPbOffset = (decimal)Project.TicksToSeconds(Project.Notes.SongLengthT);
            if (-playbackOffsetUd.Value > songLengthWithoutPbOffset)
                playbackOffsetUd.Value = -songLengthWithoutPbOffset;

            Project.Props.PlaybackOffsetS = (float)playbackOffsetUd.Value;
            _songScrollBar.Maximum = (int)Project.SongLengthT;
            _songScrollBar.Value = (int)Project.SongPosT;
        }

        private void FadeInUd_ValueChanged(object sender, EventArgs e)
        {
            Project.Props.FadeIn = (float)((NumericUpDown)sender).Value;
        }

        private void FadeOutUd_ValueChanged(object sender, EventArgs e)
        {
            Project.Props.FadeOut = (float)((NumericUpDown)sender).Value;
        }

        private void TrackPropsBtn_Click(object sender, EventArgs e)
        {

        }
        void CreateTrackList()
        {
            trackList.Items.Clear();
            if (Project.Notes == null || Project.Notes.Tracks.Count == 0)
                return;
            trackList.BeginUpdate();
            trackList.Items.Add("Global");
            for (int i = 1; i < Project.Notes.Tracks.Count; i++)
            {
                int trackNumber = Project.TrackViews[i].TrackNumber;
                ListViewItem lvi = new ListViewItem(trackNumber.ToString() + " - " + Project.Notes.Tracks[trackNumber].Name);
                lvi.SubItems.Add(" ");
                lvi.SubItems.Add(" ");
                lvi.UseItemStyleForSubItems = false;
                trackList.Items.Add(lvi);
            }

            UpdateTrackListColors();
            trackList.Items[0].Selected = true;
            trackList.Select();
            trackList.EndUpdate();
            //updateTrackListColors(null, null);
        }

        private void TrackList_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableTrackSpecificMenuItem();

            if (trackList.SelectedIndices.Count == 0)
                selectedTrackPropsPanel.Enabled = false;
            else
            {
                selectedTrackPropsPanel.Enabled = true;
                globalLightCb.Enabled = trackList.SelectedIndices[0] != 0; // || trackList.SelectedIndices.Count == 1
            }
            UpdateTrackPropsControls();
        }

        private void EnableTrackSpecificMenuItem()
        {
            int itemCount = trackList.SelectedIndices.Count;
            defaultPropertiesToolStripMenuItem1.Enabled = defaultPropertiesToolStripMenuItem.Enabled = loadTrackPropsToolStripMenuItem.Enabled = loadPropertiesToolStripMenuItem.Enabled = saveTrackPropsToolStripMenuItem.Enabled = savePropertiesToolStripMenuItem.Enabled = itemCount > 0 || !trackPropsCb.Checked;
            saveTrackPropsToolStripMenuItem.Enabled = savePropertiesToolStripMenuItem.Enabled = itemCount == 1 || !trackPropsCb.Checked;
        }

        float GetTrackBarValueNorm(object sender)
        {
            return ((TrackBar)sender).Value / 100.0f;
        }
        string GetTrackBarValueString(object sender)
        {
            return ((TrackBar)sender).Value.ToString();
        }
        int GetTextBoxNumber(object sender)
        {
            int number = 0;
            string text = ((TextBox)sender).Text;
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            try
            {
                number = Convert.ToInt32(text);
            }
            catch { }
            return number;
        }
        float GetTextBoxNumberF(object sender)
        {
            float number = 0;
            try
            {
                number = Convert.ToSingle(((TextBox)sender).Text);
            }
            catch { }
            return number;
        }
        void SetTrackBarValue(TrackBar trackBar, int value)
        {
            if (value > trackBar.Maximum)
                value = trackBar.Maximum;
            else if (value < trackBar.Minimum)
                value = trackBar.Minimum;
            trackBar.Value = value;
        }
        private void TranspSlider_Scroll(object sender, EventArgs e)
        {
            transpTb.Text = GetTrackBarValueString(sender);
        }

        private void HueSlider_Scroll(object sender, EventArgs e)
        {
            hueTb.Text = GetTrackBarValueString(sender);
        }

        private void NormalSatSlider_Scroll(object sender, EventArgs e)
        {
            normalSatTb.Text = GetTrackBarValueString(sender);
        }

        private void NormalLumSlider_Scroll(object sender, EventArgs e)
        {
            normalLumTb.Text = GetTrackBarValueString(sender);
        }

        private void HiliteSatSlider_Scroll(object sender, EventArgs e)
        {
            hiliteSatTb.Text = GetTrackBarValueString(sender);
        }

        private void HiliteLumSlider_Scroll(object sender, EventArgs e)
        {
            hiliteLumTb.Text = GetTrackBarValueString(sender);
        }

        private void TranspTb_TextChanged(object sender, EventArgs e)
        {
            int value = GetTextBoxNumber(sender);
            SetTrackBarValue(transpSlider, value);
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Transp = value / 100.0f;
            UpdateTrackListColors();
        }

        private void HueTb_TextChanged(object sender, EventArgs e)
        {
            int value = GetTextBoxNumber(sender);
            SetTrackBarValue(hueSlider, value);
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Hue = value / (float)(hueSlider.Maximum + 1);
            UpdateTrackListColors();
        }

        private void NormalSatTb_TextChanged(object sender, EventArgs e)
        {
            int value = GetTextBoxNumber(sender);
            SetTrackBarValue(normalSatSlider, value);
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Normal.Sat = value / 100.0f;
            UpdateTrackListColors();

        }

        private void NormalLumTb_TextChanged(object sender, EventArgs e)
        {
            int value = GetTextBoxNumber(sender);
            SetTrackBarValue(normalLumSlider, value);
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Normal.Lum = value / 100.0f;
            UpdateTrackListColors();
        }

        private void HiliteSatTb_TextChanged(object sender, EventArgs e)
        {
            int value = GetTextBoxNumber(sender);
            SetTrackBarValue(hiliteSatSlider, value);
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Hilited.Sat = value / 100.0f;
            UpdateTrackListColors();
        }

        private void HiliteLumTb_TextChanged(object sender, EventArgs e)
        {
            int value = GetTextBoxNumber(sender);
            SetTrackBarValue(hiliteLumSlider, value);
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Hilited.Lum = value / 100.0f;
            UpdateTrackListColors();
        }
        void LoadMtrlTexInPb()
        {
            trackTexPb.Width = _maxTrackTexPbWidth;
            trackTexPb.Height = _trackTexPbHeight;
            TrackPropsTex texProps = GetActiveTexProps(_mergedTrackProps);
            if (!string.IsNullOrEmpty(texProps.Path))
            {
                try
                {
                    using (FileStream stream = File.Open(texProps.Path, FileMode.Open))
                    {
                        using (Image srcImage = new Bitmap(Image.FromStream(stream)))
                        {
                            trackTexPb.Image = new Bitmap(trackTexPb.Width, trackTexPb.Height);
                            using (Graphics g = Graphics.FromImage(trackTexPb.Image))
                            {
                                if (GetActiveTexProps(_mergedTrackProps).PointSmp ?? false)
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                                g.DrawImage(srcImage, new System.Drawing.Rectangle(0, 0, trackTexPb.Width, trackTexPb.Height));
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                {
                    trackTexPb.Image = (Image)trackTexPb.ErrorImage.Clone();
                    trackTexPb.Width = trackTexPb.ErrorImage.Width;
                    trackTexPb.Height = trackTexPb.ErrorImage.Height;
                    return;
                }


                float whRatio = (float)trackTexPb.Image.Width / trackTexPb.Image.Height;
                trackTexPb.Height = _trackTexPbHeight;
                trackTexPb.Width = (int)(trackTexPb.Height * whRatio);
                if (trackTexPb.Width > _maxTrackTexPbWidth)
                {
                    float scale = (float)_maxTrackTexPbWidth / trackTexPb.Width;
                    trackTexPb.Height = (int)(trackTexPb.Height * scale);
                    trackTexPb.Width = (int)(trackTexPb.Width * scale);
                }
            }
            else
            {
                if (trackTexPb.Image != null)
                    trackTexPb.Image.Dispose();
                trackTexPb.Image = null;
            }
        }
        public void UpdateTrackPropsControls()
        {
            _mergedTrackProps = Project.MergeTrackProps(trackList.SelectedIndices);
            Invalidate();
            SongPanel.Invalidate();
            _updatingControls = true;
            if (_mergedTrackProps != null)
            {
                //Note style-----------------
                if (_mergedTrackProps.StyleProps.Type == null)
                    styleList.SelectedIndex = -1;
                else
                    styleList.SelectedIndex = (int)_mergedTrackProps.StyleProps.Type;
                if (_currentNoteStyleControl != null)
                    _currentNoteStyleControl.Update(_mergedTrackProps.ActiveNoteStyle);
                //barStyleControl.update(selectedTrackProps.getBarNoteStyle());
                //lineStyleControl.update(selectedTrackProps.getLineNoteStyle());
                //--------------------------------------------

                //Material-----------------------------------
                transpTb.Text = NormToIntText(_mergedTrackProps.MaterialProps.Transp);
                hueTb.Text = NormToIntText(_mergedTrackProps.MaterialProps.Hue, 101);
                normalSatTb.Text = NormToIntText(_mergedTrackProps.MaterialProps.Normal.Sat);
                normalLumTb.Text = NormToIntText(_mergedTrackProps.MaterialProps.Normal.Lum);
                hiliteSatTb.Text = NormToIntText(_mergedTrackProps.MaterialProps.Hilited.Sat);
                hiliteLumTb.Text = NormToIntText(_mergedTrackProps.MaterialProps.Hilited.Lum);

                //Texture
                TrackPropsTex texProps = GetActiveTexProps(_mergedTrackProps);
                LoadMtrlTexInPb();
                disableTextureCh.CheckState = ToCheckState(texProps.DisableTexture);
                pointSmpCb.CheckState = ToCheckState(texProps.PointSmp);
                texColBlendCb.CheckState = ToCheckState(texProps.TexColBlend);
                texUTileCb.CheckState = ToCheckState(texProps.UTile);
                texVTileCb.CheckState = ToCheckState(texProps.VTile);
                UpdateTexUVCb(tileTexCb, texUTileCb, texVTileCb);
                texKeepAspectCb.CheckState = ToCheckState(texProps.KeepAspect);
                if (texProps.UAnchor == TexAnchorEnum.Note)
                    noteUAnchorRb.Checked = true;
                else if (texProps.UAnchor == TexAnchorEnum.Screen)
                    screenUAnchorRb.Checked = true;
                else if (texProps.UAnchor == TexAnchorEnum.Song)
                    songAnchorRb.Checked = true;
                else
                    noteUAnchorRb.Checked = screenUAnchorRb.Checked = songAnchorRb.Checked = false;
                if (texProps.VAnchor == TexAnchorEnum.Note)
                    noteVAnchorRb.Checked = true;
                else if (texProps.VAnchor == TexAnchorEnum.Screen)
                    screenVAnchorRb.Checked = true;
                else
                    noteVAnchorRb.Checked = screenVAnchorRb.Checked = false;

                SetNumericUdValue(texUScrollUD, texProps.UScroll);
                SetNumericUdValue(texVScrollUD, texProps.VScroll);

                //Light
                globalLightCb.CheckState = ToCheckState(_mergedTrackProps.LightProps.UseGlobalLight);
                SetNumericUdValue(lightDirXUd, _mergedTrackProps.LightProps.DirX);
                SetNumericUdValue(lightDirYUd, _mergedTrackProps.LightProps.DirY);
                SetNumericUdValue(lightDirZUd, _mergedTrackProps.LightProps.DirZ);

                SetNumericUdValue(ambientAmountUd, _mergedTrackProps.LightProps.AmbientAmount);
                ambientHsBtn.SelectedColor = XnaToGdiCol(_mergedTrackProps.LightProps.AmbientColor);
                SetNumericUdValue(diffuseAmountUd, _mergedTrackProps.LightProps.DiffuseAmount);
                diffuseHsBtn.SelectedColor = XnaToGdiCol(_mergedTrackProps.LightProps.DiffuseColor);
                SetNumericUdValue(specAmountUd, _mergedTrackProps.LightProps.SpecAmount);
                specHsBtn.SelectedColor = XnaToGdiCol(_mergedTrackProps.LightProps.SpecColor);
                SetNumericUdValue(specPowUd, _mergedTrackProps.LightProps.SpecPower);
                SetNumericUdValue(masterLightAmountUd, _mergedTrackProps.LightProps.MasterAmount);
                masterLightHsBtn.SelectedColor = XnaToGdiCol(_mergedTrackProps.LightProps.MasterColor);
                //-------------------------------

                //Spatial---------------------------------
                SetNumericUdValue(xoffsetUd, _mergedTrackProps.SpatialProps.XOffset);
                SetNumericUdValue(yoffsetUd, _mergedTrackProps.SpatialProps.YOffset);
                SetNumericUdValue(zoffsetUd, _mergedTrackProps.SpatialProps.ZOffset);
                //-------------------------------

                //Audio--------------------------
                trackAudioFileTb.Text = _mergedTrackProps.AudioProps.Filename;
                //-------------------------------
            }
            _updatingControls = false;
        }

        void UpdateTrackListColors()
        {
            trackList.BeginUpdate();
            for (int i = 1; i < trackList.Items.Count; i++)
            {
                trackList.Items[i].SubItems[1].BackColor = Project.TrackViews[i].TrackProps.MaterialProps.GetSysColor(false, Project.GlobalTrackProps.MaterialProps);
                trackList.Items[i].SubItems[2].BackColor = Project.TrackViews[i].TrackProps.MaterialProps.GetSysColor(true, Project.GlobalTrackProps.MaterialProps);
            }
            trackList.EndUpdate();
        }

        ListViewItem GetListViewItem(ListView trackList, DragEventArgs e)
        {
            ListViewItem item = null;
            GdiPoint p = trackList.PointToClient(new GdiPoint(e.X, e.Y));

            for (int i = 0; i < trackList.Items.Count; i++)
            {
                System.Drawing.Rectangle bounds = trackList.Items[i].GetBounds(ItemBoundsPortion.Entire);
                if ((e.KeyState & 8) != 8)
                    bounds.Offset(0, bounds.Height / 2);

                if (p.Y < bounds.Bottom && p.Y >= bounds.Top)
                {
                    item = trackList.Items[i];
                    break;
                }
                if (i == trackList.Items.Count - 1 && p.Y > bounds.Bottom)
                    item = trackList.Items[i];

            }
            //if (item == null)
            //item = trackList.Items[trackList.Items.Count - 1];
            return item;
        }
        private void TrackList_DragDrop(object sender, DragEventArgs e)
        {
            ListViewItem dragToItem = GetListViewItem(trackList, e);
            int dropIndex = dragToItem.Index;
            ListViewItem[] selectedItems = new ListViewItem[trackList.SelectedIndices.Count];
            ListViewItem[] newItems = new ListViewItem[trackList.SelectedIndices.Count];

            trackList.BeginUpdate();
            if ((e.KeyState & 8) != 8) //CTRL not pressed
            {
                for (int i = 0; i < selectedItems.Length; i++)
                    selectedItems[i] = trackList.SelectedItems[i];
                for (int i = 0; i < selectedItems.Length; i++)
                    selectedItems[i].Selected = false; //Remove selection of old items before inserting new ones, otherwise all sorts of weird exceptions might occur
                for (int i = 0; i < selectedItems.Length; i++)
                {
                    newItems[i] = (ListViewItem)selectedItems[i].Clone();
                    int index = dropIndex + i + 1;
                    Project.TrackViews.Insert(index, Project.TrackViews[selectedItems[i].Index]);
                    //SongPanel.Notes.Tracks.Insert(index, SongPanel.Notes.Tracks[selectedItems[i].Index]);
                    trackList.Items.Insert(index, newItems[i]);
                }
                for (int i = 0; i < selectedItems.Length; i++)
                {
                    Project.TrackViews.RemoveAt(selectedItems[i].Index);
                    trackList.Items.Remove(selectedItems[i]);
                }
                for (int i = 0; i < selectedItems.Length; i++)
                    newItems[i].Selected = true;    //After removal of old items it's now safe to select new items											

                //for (int i = 0; i < Project.TrackViews.Count; i++)
                //Project.TrackViews[i].TrackNumber = i;
                AddUndoItem("Reorder tracks");
            }
            else //CTRL pressed
            {
                bool onlyCopyCurrentTab = (e.KeyState & 4) != 4; //SHIFT not pressed
                TrackView sourceTrackView = Project.TrackViews[dropIndex];
                TrackProps sourceTrackProps = sourceTrackView.TrackProps;
                for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                {
                    var trackNumber = trackList.SelectedIndices[i];
                    TrackProps destTrackProps = Project.TrackViews[trackNumber].TrackProps;
                    TrackPropsType tpt = TrackPropsType.TPT_All;
                    if (onlyCopyCurrentTab)
                    {
                        string tabName = selectedTrackPropsPanel.SelectedTab.Name;
                        Enum.TryParse(tabName, out tpt);
                    }
                    destTrackProps.CloneFrom(sourceTrackProps, (int)tpt, SongPanel);
                    Project.TrackViews[trackNumber].CreateGeo(Project, Project.GlobalTrackProps);
                }
                UpdateTrackPropsControls();
                UpdateTrackListColors();
                AddUndoItem("Copy track properties");
            }
            trackList.EndUpdate();
        }

        private void TrackList_DragEnter(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.All;
        }

        private void TrackList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (trackList.SelectedIndices.Count == 0 || trackList.SelectedIndices[0] == 0)
                return;
            DoDragDrop(e.Item, DragDropEffects.All);
        }

        private void TrackList_DragOver(object sender, DragEventArgs e)
        {
            ListViewItem dragToItem = GetListViewItem(trackList, e);

            if (dragToItem == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            trackList.RedrawItems(0, trackList.Items.Count - 1, false);
            System.Drawing.Rectangle bounds = dragToItem.GetBounds(ItemBoundsPortion.Entire);
            bounds.Offset(0, -1);
            if ((e.KeyState & 8) != 8)
            {
                e.Effect = DragDropEffects.Scroll | DragDropEffects.Move;
                _trackListGfxObj.DrawLine(_trackListPen, new GdiPoint(bounds.Left, bounds.Bottom), new GdiPoint(bounds.Right, bounds.Bottom));
            }
            else
            {
                e.Effect = DragDropEffects.Scroll | DragDropEffects.Copy;
                _trackListGfxObj.DrawLine(_trackListPen, new GdiPoint(bounds.Left, bounds.Top), new GdiPoint(bounds.Right, bounds.Top));
                _trackListGfxObj.DrawLine(_trackListPen, new GdiPoint(bounds.Left, bounds.Bottom), new GdiPoint(bounds.Right, bounds.Bottom));
                _trackListGfxObj.DrawLine(_trackListPen, new GdiPoint(bounds.Left, bounds.Top), new GdiPoint(bounds.Left, bounds.Bottom));
                _trackListGfxObj.DrawLine(_trackListPen, new GdiPoint(bounds.Right, bounds.Top), new GdiPoint(bounds.Right, bounds.Bottom));
            }
        }

        private void StyleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentNoteStyleControl != null)
                _currentNoteStyleControl.Visible = false;
            if (styleList.SelectedIndex == -1 || styleList.SelectedIndex == (int)NoteStyleType.Default)
                _currentNoteStyleControl = null;
            else
            {
                if (styleList.SelectedIndex == (int)NoteStyleType.Bar)
                    _currentNoteStyleControl = barStyleControl;
                else if (styleList.SelectedIndex == (int)NoteStyleType.Line)
                    _currentNoteStyleControl = lineStyleControl;
                _currentNoteStyleControl.Visible = true;
            }

            if (_updatingControls)
                return;
            //SongPanel.Invalidate();
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
            {
                NoteStyleType type = (NoteStyleType)Enum.Parse(typeof(NoteStyleType), (string)styleList.SelectedItem);
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.StyleProps.Type = type;
            }
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void TextureBrowseBtn_Click(object sender, EventArgs e)
        {
            if (openTextureDlg.ShowDialog(this) == DialogResult.OK)
            {
                openTextureDlg.InitialDirectory = Path.GetDirectoryName(openTextureDlg.FileName);
                SaveSettings();
            }
        }

        private void TextureLoadBtn_Click(object sender, EventArgs e)
        {
            int i = 0;
            try
            {
                if (openTextureDlg.ShowDialog(this) != DialogResult.OK)
                    return;

                openTextureDlg.InitialDirectory = Path.GetDirectoryName(openTextureDlg.FileName);
                SaveSettings();

                for (i = 0; i < trackList.SelectedIndices.Count; i++)
                {
                    //Texture2d.FromStream fails if file is loaded outside of for loop
                    using (FileStream stream = File.Open(openTextureDlg.FileName, FileMode.Open))
                    {
                        GetActiveTexProps(i).LoadTexture(openTextureDlg.FileName, stream, SongPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                GetActiveTexProps(i).Path = "";
                MessageBox.Show(ex.Message);
            }
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private TrackPropsTex GetActiveTexProps(int index)
        {
            return GetActiveTexProps(Project.TrackViews[trackList.SelectedIndices[index]].TrackProps);
        }
        private TrackPropsTex GetActiveTexProps(TrackProps trackProps)
        {
            //TODO: return texProps (0) or hmapProps (1) depending on which of the two is currently being edited.
            return trackProps.MaterialProps.GetTexProps(0);
        }
        private void UnloadTexBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
            {
                GetActiveTexProps(i).UnloadTexture();
            }
            UpdateTrackPropsControls();
        }

        private void OpenSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openProjDialog.FileName = "";
            var dialogResult = openProjDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                //if (currentScreen is SongWebBrowser)
                _currentScreen.Focus();
                return;
            }
            ProjectFolder = Path.GetDirectoryName(openProjDialog.FileName);
            SaveSettings();
            ChangeToScreen(SongPanel); //Hide browsers if they haven't been hidden yet. Otherwise the last browser will be brought to front during loading.Hopefully they have had time to initialize.
            OpenProjectFile(openProjDialog.FileName);
        }
        async void OpenProjectFile(string projectPath)
        {
            Project tempProject;
            DataContractSerializer dcs = new DataContractSerializer(typeof(Project), projectSerializationTypes);
            try
            {
                using (FileStream stream = File.Open(projectPath, FileMode.Open))
                {
                    tempProject = (Project)dcs.ReadObject(stream);
                }
            }
            catch (SerializationException e)
            {
                ShowErrorMsgBox("Invalid project file.\n" + e.Message);
                return;
            }
            do
            {
                try
                {
                    SongPanel.SuspendPaint();
                    SongPanel.Project = tempProject;
                    await tempProject.LoadContent(this);
                    Project = tempProject;
                    break;
                }

                catch (FileImportException ex)
                {
                    string problemFilePath = ex.FileName;
                    if (problemFilePath.Contains(Program.TempDir))
                    {
                        ShowErrorMsgBox("An unknown error occurred");
                        return;
                    }
                    bool audioFileProblem = ex.FileType == ImportFileType.Audio;
                    bool criticalError = !tempProject.ImportOptions.SavedMidi && !audioFileProblem;
                    DialogResult dlgResult = _locateFileDlg.ShowDialog(problemFilePath, Path.GetDirectoryName(projectPath), ex.Error, ex.FileType, criticalError);

                    if (dlgResult == DialogResult.OK)
                    {
                        if (audioFileProblem)
                            tempProject.ImportOptions.AudioPath = _locateFileDlg.FilePath;
                        else if (tempProject.ImportOptions.SavedMidi)
                            tempProject.ImportOptions.MidiOutputPath = _locateFileDlg.FilePath;
                        else
                            tempProject.ImportOptions.RawNotePath = _locateFileDlg.FilePath;
                    }
                    else  //Cancel / Ignore
                    {
                        if (audioFileProblem)
                        {
                            tempProject.ImportOptions.AudioPath = "";
                            continue;
                        }
                        else if (tempProject.ImportOptions.SavedMidi)
                        {
                            tempProject.ImportOptions.SavedMidi = false;
                            tempProject.ImportOptions.MidiOutputPath = "";
                            continue;
                        }
                        else //critical error
                            return;
                    }
                }
                catch (IOException ex)
                {
                    ShowErrorMsgBox("Unexpected error: " + ex.Message);
                    return;
                }
                finally
                {
                    SongPanel.Project = Project; //If loading was cancelled, Project was not updated, so SongPanel's reference will be reset to old project
                    SongPanel.ResumePaint();
                }
            } while (true);

            Project.InitAfterDeserialization();
            
            _currentProjPath = projectPath;
            SongLoaded(_currentProjPath);
            UpdateFormTitle(_currentProjPath);
            Project.DefaultFileName = Path.GetFileName(_currentProjPath);
            _unsavedChanges = false;
            SongPanel.LoadBackgroundImage(Project.Props.BackgroundImagePath);
        }

        void UpdateFormTitle(string path)
        {
            Text = "Visual Music " + Program.FileVersion;
            if (!string.IsNullOrEmpty(path))
                Text += " - " + Path.GetFileName(path);
        }
        private void SaveSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSong();
        }
        bool SaveSong()
        {
            if (string.IsNullOrEmpty(_currentProjPath))
            {
                return SaveSongAs();
            }
            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(Project), projectSerializationTypes);

                string tempPath = Path.Combine(Program.TempDir, "tempprojectfile");
                using (FileStream stream = File.Open(tempPath, FileMode.Create))
                {
                    dcs.WriteObject(stream, Project);
                }
                File.Copy(tempPath, _currentProjPath, true);
                UpdateFormTitle(_currentProjPath);
                _unsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
        bool SaveSongAs()
        {
            saveProjDialog.FileName = Project.DefaultFileName;
            if (saveProjDialog.ShowDialog() != DialogResult.OK)
                return false;
            ProjectFolder = Path.GetDirectoryName(saveProjDialog.FileName);
            SaveSettings();

            //Save audio mixdown
            saveMixdownDialog.FileName = Path.ChangeExtension(saveProjDialog.FileName, "wav");

            if (Project.ImportOptions.MixdownType != Midi.MixdownType.None && saveMixdownDialog.ShowDialog() == DialogResult.OK)
            {
                Project.ImportOptions.MixdownType = Midi.MixdownType.None;
                Project.ImportOptions.AudioPath = saveMixdownDialog.FileName;
                Project.ImportOptions.UpdateImportForm(); //To update audio file path
            }

            //Save midi "mixdown"
            saveMidiDialog.FileName = Path.ChangeExtension(saveProjDialog.FileName, "mid");
            if (!Project.ImportOptions.SavedMidi && Project.ImportOptions.NoteFileType != Midi.FileType.Midi)
            {
                if (saveMidiDialog.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(Project.ImportOptions.MidiOutputPath, saveMidiDialog.FileName, true);
                    Project.ImportOptions.MidiOutputPath = saveMidiDialog.FileName;
                    Project.ImportOptions.SavedMidi = true;
                    Project.ImportOptions.UpdateImportForm(); //To update audio file path
                }
                else
                    Project.ImportOptions.MidiOutputPath = "";
            }

            _currentProjPath = saveProjDialog.FileName;
            return SaveSong();
        }

        private void SaveSongAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSongAs();
        }
        void InitSongPanel()
        {
            SongPanel.TabStop = false;
            SongPanel.MouseWheel += new MouseEventHandler(SongPanel_MouseWheel);
            SongPanel.OnSongPosChanged = delegate ()
            {
                _updatingControls = true;
                SongPanel.Invalidate();
                if (Project.SongPosT <= _songScrollBar.Maximum && Project.SongPosT >= _songScrollBar.Minimum)
                    _songScrollBar.Value = (int)Project.SongPosT;
                upDownVpWidth.Value = Project.Props.ViewWidthQn;
                bkgOpacityUd.Value = (decimal)Project.Props.BackgroundImageOpacity;
                bkgSaturationUd.Value = (decimal)Project.Props.BackgroundImageSaturation;

                if (keyFramesDGV.Rows.Count == 0) //App is closing
                    return;
                int atKeyFrame = -1;
                if (Project.KeyFrames.Keys[0] >= Project.SongPosT)
                    atKeyFrame = 0;
                else if (Project.KeyFrames.Keys[Project.KeyFrames.Count - 1] <= Project.SongPosT)
                    atKeyFrame = Project.KeyFrames.Count - 1;
                else if (Project.KeyFrames.Count > 2)
                {
                    for (int i = Project.KeyFrames.Count - 2; i >= 1; i--)
                    {
                        if (Project.KeyFrames.Keys[i] <= Project.SongPosT)
                        {
                            keyFramesDGV.CurrentCell = keyFramesDGV.Rows[i].Cells[0]; //Point at this row but deselect it
                            break;
                        }
                    }
                }
                if (atKeyFrame >= 0)
                {
                    upDownVpWidth.Enabled = true;
                    keyFramesDGV.CurrentCell = keyFramesDGV.Rows[atKeyFrame].Cells[0];
                }
                else
                {
                    if (keyFramesDGV.CurrentCell != null)
                        keyFramesDGV.CurrentCell.Selected = false;
                }
                UpdateCamControls();
                _updatingControls = false;
            };
            ChangeToScreen(SongPanel, false);
        }

        private void InvalidateSongPanel(object sender, EventArgs e)
        {
            SongPanel.Invalidate();
            //songScrollBar.Value = SongPanel.SongPosT;
        }

        void AddUndoItem(object sender, EventArgs e)
        {
            object tag = null;
            if (sender is Control)
                tag = ((Control)sender).Tag;
            else if (sender is ToolStripMenuItem)
                tag = ((ToolStripMenuItem)sender).Tag;
            if (tag == null)
                return;
            string desc = tag.ToString();
            if (string.IsNullOrEmpty(desc))
                return;

            if (sender.GetType() == typeof(CheckBox))
            {
                string action = ((CheckBox)sender).Checked ? "Check " : "Uncheck ";
                desc = action + desc;
            }
            AddUndoItem(desc);
        }

        void AddUndoItem(string desc)
        {
            if (_updatingControls)
                return;

            _undoItems.Add(desc, Project);


            undoToolStripMenuItem.Enabled = true;
            redoToolStripMenuItem.Enabled = false;
            UpdateUndoRedoDesc();
            _unsavedChanges = true;
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _undoItems--;
            ApplyUndoItem();
            if (_undoItems.Previous == null)
                undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = true;
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _undoItems++;
            ApplyUndoItem();
            if (_undoItems.Next == null)
                redoToolStripMenuItem.Enabled = false;
            undoToolStripMenuItem.Enabled = true;
        }

        void ApplyUndoItem()
        {
            Project.CopyPropsFrom(_undoItems.Current.Project);
            UpdateProjPropsControls();
            UpdateTrackPropsControls();
            if (_undoItems.RedoDesc == "Reorder tracks")
                CreateTrackList();
            else
                UpdateTrackListColors();
            SongPanel.Invalidate();
            UpdateUndoRedoDesc();
        }

        void UpdateUndoRedoDesc()
        {
            undoToolStripMenuItem.Text = "Undo " + _undoItems.UndoDesc;
            redoToolStripMenuItem.Text = "Redo " + _undoItems.RedoDesc;
        }

        private void TrackPropsPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void GlobalLightCb_CheckedChanged(object sender, EventArgs e)
        {
            lightPanel.Enabled = !globalLightCb.Checked;
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.UseGlobalLight = globalLightCb.Checked;
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trackList.Select();
            trackList.BeginUpdate();
            for (int i = 1; i < trackList.Items.Count; i++)
                trackList.Items[i].Selected = true;
            trackList.EndUpdate();
        }

        private void InvertSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trackList.Select();
            trackList.BeginUpdate();
            for (int i = 1; i < trackList.Items.Count; i++)
                trackList.Items[i].Selected = !trackList.Items[i].Selected;
            trackList.EndUpdate();
        }

        private void DefaultPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            defaultPropertiesToolStripMenuItem1.PerformClick();
        }

        private void DefaultStyleBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.ResetStyle();
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void DefaultMaterial_Click(object sender, EventArgs e)
        {
            //unloadTexBtn.PerformClick();// _Click(null, null);
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.ResetMaterial();
            UpdateTrackPropsControls();
            UpdateTrackListColors();

        }

        private void DefaultLightBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.ResetLight();
            UpdateTrackPropsControls();
        }

        private void DefaultSpatialBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.ResetSpatial();
            UpdateTrackPropsControls();
        }

        private void TrackPropsCb_CheckedChanged(object sender, EventArgs e)
        {
            EnableTrackSpecificMenuItem();
            trackPropsPanel.Visible = trackPropsCb.Checked;
            if (trackPropsCb.Checked)
                trackList.Focus();
        }

        private void SongPropsCb_CheckedChanged(object sender, EventArgs e)
        {
            if (songPropsCb.Checked)
            {
                songPropsPanel.Show();
                if (Project.Props.LyricsSegments.Count > 0)
                    lyricsGridView.Show();
                keyFramesDGV.Show();
            }
            else
            {
                songPropsPanel.Hide();
                lyricsGridView.Hide();
                keyFramesDGV.Hide();
            }
        }

        private void MaxPitchUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            if ((int)maxPitchUd.Value < Project.Notes.MinPitch)
            {
                maxPitchUd.Value = Project.Notes.MinPitch;
                return;
            }
            Project.Props.MaxPitch = (int)maxPitchUd.Value;
            Project.CreateGeos();
        }

        private void MinPitchUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            if ((int)minPitchUd.Value > Project.Notes.MaxPitch)
                minPitchUd.Value = Project.Notes.MaxPitch;
            Project.Props.MinPitch = (int)minPitchUd.Value;
            Project.CreateGeos();
        }

        void ResetPitchLimits()
        {
            Project.ResetPitchLimits();
            _updatingControls = true;
            maxPitchUd.Value = (decimal)Project.Props.MaxPitch;
            minPitchUd.Value = (decimal)Project.Props.MinPitch;
            _updatingControls = false;
            Project.CreateGeos();
        }

        private void DefaultPitchesBtn_Click(object sender, EventArgs e)
        {
            ResetPitchLimits();
        }

        private void DisableTextureCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).DisableTexture = ((CheckBox)sender).Checked;
        }

        private void PointSmpCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).PointSmp = ((CheckBox)sender).Checked;
            UpdateTrackPropsControls();
        }

        private void TexColBlendCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).TexColBlend = ((CheckBox)sender).Checked;
        }

        private void TileTexCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            if (((CheckBox)sender).CheckState != CheckState.Indeterminate)
            {
                for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                {
                    GetActiveTexProps(i).UTile = ((CheckBox)sender).Checked;
                    GetActiveTexProps(i).VTile = ((CheckBox)sender).Checked;
                }
                Project.CreateGeos();
                UpdateTrackPropsControls();
            }
        }
        void UpdateTexUVCb(CheckBox uv, CheckBox u, CheckBox v)
        {
            if (u.Checked != v.Checked || u.CheckState == CheckState.Indeterminate || v.CheckState == CheckState.Indeterminate)
            {
                uv.CheckState = CheckState.Indeterminate;
                texKeepAspectCb.Enabled = true;
            }
            else
            {
                texKeepAspectCb.Enabled = false;
                if (u.Checked && v.Checked)
                    uv.CheckState = CheckState.Checked;
                else
                    uv.CheckState = CheckState.Unchecked;
            }
        }
        private void TexUTileCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UTile = ((CheckBox)sender).Checked;
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void TexVTileCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).VTile = ((CheckBox)sender).Checked;
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void NoteAnchorLabel_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UAnchor = GetActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void ScreenAnchorLabel_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UAnchor = GetActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void SongAnchorLabel_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
            Project.CreateGeos();
            UpdateTrackPropsControls();
        }

        private void NoteUAnchorRb_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UAnchor = TexAnchorEnum.Note;
            Project.CreateGeos();
        }

        private void NoteVAnchorRb_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
            Project.CreateGeos();
        }

        private void ScreenUAnchorRb_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UAnchor = TexAnchorEnum.Screen;
            Project.CreateGeos();
        }

        private void ScreenVAnchorRb_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
            Project.CreateGeos();
        }

        private void SongAnchorRb_Click(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
            Project.CreateGeos();
        }

        private void TexUScrollUD_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).UScroll = (float)((NumericUpDown)sender).Value;
        }

        private void TexVScrollUD_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).VScroll = (float)((NumericUpDown)sender).Value;
        }

        private void TexKeepAspect_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                GetActiveTexProps(i).KeepAspect = ((CheckBox)sender).Checked;
            Project.CreateGeos();
        }

        private void ImportSidSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportSidForm.Hide();
            if (ImportSidForm.ShowDialog(this) == DialogResult.OK)
                SongPanel.Focus();
        }

        private void TpartyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TpartyIntegrationForm.ShowDialog();
            SaveSettings();
        }
        public static void ShowErrorMsgBox(string message, string caption = "")
        {
            MessageBox.Show(null, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void ShowWarningMsgBox(string message, string caption = "")
        {
            MessageBox.Show(null, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        void ResetCameras(bool onlySelected, Camera newCam = null)
        {
            foreach (var keyFrame in Project.KeyFrames.Values)
            {
                if (keyFrame.Selected || !onlySelected)
                {
                    keyFrame.ProjProps.Camera = newCam ?? new Camera();
                }
            }
            Project.InterpolateFrames();
            UpdateCamControls();
        }
        private void ResetCamBtn_Click(object sender, EventArgs e)
        {
            ResetCameras(true);
        }

        private void StartStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Project.TogglePlayback();
        }

        private void BeginningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Project.StopPlayback();
        }
        private void EndToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Project.StopPlayback();
            _songScrollBar.Value = _songScrollBar.Maximum;
        }
        private void NudgeBackwardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _songScrollBar.Value = Math.Max(_songScrollBar.Minimum, _songScrollBar.Value - _songScrollBar.SmallChange);
            UpdatePlaybackPosWhilePlaying();
        }

        private void NudgeForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _songScrollBar.Value = Math.Min(_songScrollBar.Maximum, _songScrollBar.Value + _songScrollBar.SmallChange);
            UpdatePlaybackPosWhilePlaying();
        }

        private void JumpBackwardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _songScrollBar.Value = Math.Max(_songScrollBar.Minimum, _songScrollBar.Value - _songScrollBar.LargeChange);
            UpdatePlaybackPosWhilePlaying();
        }

        private void JumpForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _songScrollBar.Value = Math.Min(_songScrollBar.Maximum, _songScrollBar.Value + _songScrollBar.LargeChange);
            UpdatePlaybackPosWhilePlaying();
        }

        void UpdatePlaybackPosWhilePlaying()
        {
            if (Project.IsPlaying)
            {
                Project.TogglePlayback();
                Project.TogglePlayback();
            }
        }
        void SongScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type != ScrollEventType.EndScroll)
                Project.TempPausePlayback();
            else
                Project.ResumeTempPausedPlayback();
        }

        private void SongScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            Project.NormSongPos = _songScrollBar.Value / Project.SongLengthT;
        }

        private void XoffsetUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpatialProps.XOffset = (float)xoffsetUd.Value;
        }

        private void YoffsetUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpatialProps.YOffset = (float)yoffsetUd.Value;
        }

        private void ZoffsetUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpatialProps.ZOffset = (float)zoffsetUd.Value;
        }

        public static CheckState ToCheckState(bool? value)
        {
            return value == null ? CheckState.Indeterminate : ((bool)value ? CheckState.Checked : CheckState.Unchecked);
        }

        public static void SetNumericUdValue(NumericUpDown ud, float? value)
        {
            if (value == null)
                ud.Text = null;
            else
                ud.Value = (decimal)value;
        }

        string NormToIntText(float? value, float scale = 100)
        {
            if (value == null)
                return null;
            else
                return ((int)((float)value * scale + 0.5f)).ToString();
        }

        private void LightDirXUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DirX = (float)((NumericUpDown)sender).Value;
        }

        private void LightDirYUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DirY = (float)((NumericUpDown)sender).Value;
        }

        private void LightDirZUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DirZ = (float)((NumericUpDown)sender).Value;
        }

        private void AmbientAmountUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.AmbientAmount = (float)((NumericUpDown)sender).Value;
        }

        private void DiffuseAmountUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DiffuseAmount = (float)((NumericUpDown)sender).Value;
        }

        private void SpecAmountUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.SpecAmount = (float)specAmountUd.Value;
        }

        private void MasterLightAmountUD_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.MasterAmount = (float)masterLightAmountUd.Value;
        }

        private void SpecPowUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.SpecPower = (float)specPowUd.Value;
        }

        private void AmbientHsBtn_ColorChanged(object sender, ColorChangedTventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.AmbientColor = GdiToXnaCol(ambientHsBtn.SelectedColor);
        }

        private void DiffuseHsBtn_ColorChanged(object sender, ColorChangedTventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DiffuseColor = GdiToXnaCol(diffuseHsBtn.SelectedColor);
        }

        private void SpecHsBtn_ColorChanged(object sender, ColorChangedTventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.SpecColor = GdiToXnaCol(specHsBtn.SelectedColor);
        }

        private void MasterLightHsBtn_ColorChanged(object sender, ColorChangedTventArgs e)
        {
            if (_updatingControls)
                return;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
                Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.MasterColor = GdiToXnaCol(masterLightHsBtn.SelectedColor);
        }

        public static XnaColor GdiToXnaCol(GdiColor gdiCol)
        {
            return new XnaColor(gdiCol.R, gdiCol.G, gdiCol.B);
        }

        public static GdiColor XnaToGdiCol(XnaColor? xnaCol)
        {
            if (xnaCol == null)
                return GdiColor.Black;
            else
            {
                XnaColor c = (XnaColor)xnaCol;
                return GdiColor.FromArgb(c.A, c.R, c.G, c.B);
            }
        }

        private void UpDownVpWidth_CommitChanges(object sender, EventArgs e)
        {
            CommitViewWidthQnChange();
        }

        private void ViewSongTSMI_Click(object sender, EventArgs e)
        {
            ChangeToScreen(SongPanel);
        }

        private void ViewModBrowserTSMI_Click(object sender, EventArgs e)
        {
            ChangeToScreen(_modWebBrowser);
        }

        private void ViewSidBrowserTSMI_Click(object sender, EventArgs e)
        {
            ChangeToScreen(_sidWebBrowser);
        }

        private void ViewMidiBrowserTSMI_Click(object sender, EventArgs e)
        {
            ChangeToScreen(_midiWebBrowser);
        }

        void ChangeToScreen(Control newScreen, bool hideOthers = true)
        {
            if (!hideOthers)
            {
                newScreen.BringToFront();
            }
            else
            {
                foreach (var screen in _screens)
                {
                    if (screen != newScreen)
                        screen.Visible = false;
                }
                newScreen.Visible = true;
            }
            bool isSongScreen = newScreen is SongPanel;
            propsTogglePanel.Enabled = isSongScreen && Project.Notes != null;
            if (!isSongScreen)
                songPropsCb.Checked = trackPropsCb.Checked = false;
            newScreen.Focus();
            _currentScreen = newScreen;
        }

        private void TrackPropsPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (!trackPropsPanel.Visible)
                SongPanel.Focus();
        }

        private void SongPropsPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (!songPropsPanel.Visible)
                SongPanel.Focus();
        }

        private void CamTb_TextChanged(object sender, EventArgs e)
        {
            camTb.ForeColor = GdiColor.Black;
            if (_updatingCamControls)
                return;
            int elementIndex = 0;
            Vector3 pos = new Vector3();
            Quaternion orient = new Quaternion();
            foreach (string line in camTb.Lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                float element;
                try
                {
                    element = float.Parse(line);
                }
                catch (FormatException)
                {
                    camTb.ForeColor = GdiColor.Red;
                    return;
                }

                switch (elementIndex)
                {
                    case 0:
                        pos.X = element;
                        break;
                    case 1:
                        pos.Y = element;
                        break;
                    case 2:
                        pos.Z = element;
                        break;
                    case 3:
                        orient.X = element;
                        break;
                    case 4:
                        orient.Y = element;
                        break;
                    case 5:
                        orient.Z = element;
                        break;
                    case 6:
                        orient.W = element;
                        break;
                }
                elementIndex++;
            }
            foreach (var keyFrame in Project.KeyFrames.Values)
            {
                if (keyFrame.Selected)
                {
                    keyFrame.ProjProps.Camera.Orientation = orient;
                    keyFrame.ProjProps.Camera.Pos = pos;
                }
            }
        }

        private bool ColorDialogButtonClick(Button button)
        {
            colorDialog1.Color = button.BackColor;
            if (colorDialog1.ShowDialog() != DialogResult.OK)
                return false;
            button.BackColor = colorDialog1.Color;
            return true;
        }

        private void LoadTrackPropsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadPropertiesToolStripMenuItem.PerformClick();
        }

        private void SaveTrackPropsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            savePropertiesToolStripMenuItem.PerformClick();
        }

        void LoadTrackProps(int typeFlags)
        {
            //Show open-file dialog.
            if (openTrackPropsFileDialog.ShowDialog() != DialogResult.OK)
                return;
            TrackPropsFolder = Path.GetDirectoryName(openTrackPropsFileDialog.FileName);
            SaveSettings();

            TrackProps props;
            DataContractSerializer dcs = new DataContractSerializer(typeof(TrackProps), projectSerializationTypes);
            try
            {
                using (FileStream stream = File.Open(openTrackPropsFileDialog.FileName, FileMode.Open))
                {
                    props = (TrackProps)dcs.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsgBox(ex.Message);
                return;
            }

            if (typeFlags < 0)
            {
                var trackPropsTypeForm = new TrackPropsTypeForm(props.TypeFlags);
                if (trackPropsTypeForm.ShowDialog() != DialogResult.OK)
                    return;
                typeFlags = trackPropsTypeForm.TypeFlags;
            }
            else //Load props to specific tab.
            {
                //Check if requested prop types exist in file.
                for (int i = 0; i < Enum.GetValues(typeof(TrackPropsType)).Length - 1; i++)
                {
                    if (((typeFlags >> i) & 1) == 1 && ((props.TypeFlags >> i) & 1) != 1)
                        throw new FileFormatException($"File doesn't contain properties of type {_trackPropsTypeNames[i]}");
                }
            }
            if (trackPropsCb.Checked)
            {
                foreach (int selectedTrackIndex in trackList.SelectedIndices)
                {
                    Project.TrackViews[selectedTrackIndex].TrackProps.CloneFrom(props, typeFlags, SongPanel);
                }
            }
            else
                Project.TrackViews[0].TrackProps.CloneFrom(props, typeFlags, SongPanel);

            AddUndoItem("Load Track Properties");
            UpdateTrackListColors();
            UpdateTrackPropsControls();
            if ((typeFlags & (int)TrackPropsType.TPT_Style) != 0)
                Project.CreateGeos();
        }
        void SaveTrackProps(int typeFlags)
        {
            if (typeFlags < 0)
            {
                var trackPropsTypeForm = new TrackPropsTypeForm((int)TrackPropsType.TPT_All);
                if (trackPropsTypeForm.ShowDialog() != DialogResult.OK)
                    return;
                typeFlags = trackPropsTypeForm.TypeFlags;
            }

            //Show save-file dialog.
            if (saveTrackPropsFileDialog.ShowDialog() != DialogResult.OK)
                return;
            TrackPropsFolder = Path.GetDirectoryName(saveTrackPropsFileDialog.FileName);
            SaveSettings();

            int trackIndex = trackPropsCb.Checked ? trackList.SelectedIndices[0] : 0;
            TrackProps props = (TrackProps)Project.TrackViews[trackIndex].TrackProps;
            props.TypeFlags = typeFlags;
            DataContractSerializer dcs = new DataContractSerializer(typeof(TrackProps), projectSerializationTypes);
            try
            {
                using (FileStream stream = File.Open(saveTrackPropsFileDialog.FileName, FileMode.Create))
                {
                    dcs.WriteObject(stream, props);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsgBox(ex.Message);
            }

        }
        private void LoadTrackPropsTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tabName = selectedTrackPropsPanel.SelectedTab.Name;
            int tpt = (int)Enum.Parse(typeof(TrackPropsType), tabName);
            LoadTrackProps(tpt);
        }

        private void SaveTrackPropsTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tabName = selectedTrackPropsPanel.SelectedTab.Name;
            int tpt = (int)Enum.Parse(typeof(TrackPropsType), tabName);
            SaveTrackProps(tpt);
        }

        private void TrackListCM_Opening(object sender, CancelEventArgs e)
        {
            //int numSelectedTracks = TrackList.SelectedIndices.Count;
            //defaultPropertiesToolStripMenuItem.Enabled = numSelectedTracks > 0;
            //saveTrackPropsToolStripMenuItem.Enabled = numSelectedTracks == 1;
            //loadTrackPropsToolStripMenuItem.Enabled = numSelectedTracks > 0;

        }

        private void ResetCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetCameras(true);
        }

        private void LoadCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openCamFileDialog.ShowDialog() != DialogResult.OK)
                return;
            CamFolder = Path.GetDirectoryName(openCamFileDialog.FileName);
            SaveSettings();

            DataContractSerializer dcs = new DataContractSerializer(typeof(Camera), projectSerializationTypes);
            try
            {
                using (FileStream stream = File.Open(openCamFileDialog.FileName, FileMode.Open))
                {
                    Camera cam = (Camera)dcs.ReadObject(stream);
                    ResetCameras(true, cam);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsgBox(ex.Message);
            }
            AddUndoItem("Load Camera");
        }

        private void SaveCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveCamFileDialog.ShowDialog() != DialogResult.OK)
                return;
            CamFolder = Path.GetDirectoryName(saveCamFileDialog.FileName);
            SaveSettings();

            DataContractSerializer dcs = new DataContractSerializer(typeof(Camera), projectSerializationTypes);
            try
            {
                using (FileStream stream = File.Open(saveCamFileDialog.FileName, FileMode.Create))
                {
                    dcs.WriteObject(stream, Project.Props.Camera);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsgBox(ex.Message);
            }
        }

        private void LoadPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTrackProps(-1);
        }

        private void SavePropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTrackProps(-1);
        }

        private void TracksToolStripMenuItem_EnabledChanged(object sender, EventArgs e)
        {
            //When Tracks menu item is disabled, all the sub items need to be disabled, otherwise their shortcut keys will still work.
            //bool enabled = tracksToolStripMenuItem.Enabled;

            //When Tracks menu is enabled, some sub items should remain disadled depending on how many tracks are selected.
            //if (enabled)
            //enableTrackSpecificMenuItem();
        }

        private void DefaultPropertiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //Project.resetTrackProps(trackPropsCb.Checked ? trackList.SelectedIndices : null);
            if (trackList.SelectedIndices != null)
            {
                foreach (int index in trackList.SelectedIndices)
                    Project.TrackViews[index].TrackProps.ResetProps();
            }
            else
                Project.TrackViews[0].TrackProps.ResetProps();
            Project.CreateGeos();
            UpdateTrackPropsControls();
            UpdateTrackListColors();
        }

        private void LyricsGridView_Paint(object sender, PaintEventArgs e)
        {
            lyricsGridView.Height = lyricsGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + lyricsGridView.ColumnHeadersHeight + 2;
        }

        private void LyricsGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //showErrorMsgBox("The entered value has an invalid format.");
            //e.ThrowException = false;
        }

        private void LyricsGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            SongPanel.Invalidate();
            float time;
            if (e.ColumnIndex == 0 && !float.TryParse((string)e.FormattedValue, out time))
            {
                ShowErrorMsgBox("Invalid format.");
                e.Cancel = true;
            }
        }

        private void LyricsGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void InsertLyricsHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            songPropsCb.Checked = true;
            lyricsGridView.Show();
            int row = Project.InsertLyrics();
            var cell = lyricsGridView.Rows[row].Cells[1];
            lyricsGridView.CurrentCell = cell;
            lyricsGridView.BeginEdit(true);
        }

        private void LyricsGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (Project.Props.LyricsSegments.Count == 0)
                lyricsGridView.Hide();
        }

        private void InsertKeyFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int row = Project.InsertKeyFrameAtSongPos();
            if (row < 0)
            {
                ShowErrorMsgBox("A keyframe already exists here.");
                return;
            }

            keyFramesDGV.Rows.Insert(row, Project.SongPosT, "");
            songPropsCb.Checked = true;
            keyFramesDGV.CurrentCell = keyFramesDGV.Rows[row].Cells[0]; //Select cell 0 to update CurrentRow. Needed for SelectionChanged event to go to correct song pos.
            keyFramesDGV.CurrentCell = keyFramesDGV.Rows[row].Cells[1]; //Select cell in Description column
                                                                        //keyFramesDGV.BeginEdit(true);
            AddUndoItem("Insert Key Frame");
        }

        private void KeyFramesDGV_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {

        }

        private void KeyFramesDGV_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (_updatingControls)
                return;
            Project.KeyFrames.RemoveIndex(e.RowIndex);
        }

        private void KeyFramesDGV_SelectionChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            if (_keyFrameLockRow >= 0)
            {
                keyFramesDGV.CurrentCell = keyFramesDGV.Rows[_keyFrameLockRow].Cells[0];
                _keyFrameLockRow = -1;
            }

            UpdateKeyFrameSelection();

            if (keyFramesDGV.CurrentRow != null)
            {
                Project.GoToKeyFrame(keyFramesDGV.CurrentRow.Index);
                _updatingControls = true;
                upDownVpWidth.Value = Project.KeyFrames.Values[keyFramesDGV.CurrentRow.Index].ProjProps.ViewWidthQn;
                _updatingControls = false;

            }
            SongPanel.Invalidate();
        }

        private void KeyFramesDGV_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (keyFramesDGV.Rows.Count == 1)
                e.Cancel = true;
        }

        private void KeyFramesDGV_Paint(object sender, PaintEventArgs e)
        {
            //keyFramesDGV.Height = keyFramesDGV.Rows.GetRowsHeight(DataGridViewElementStates.None) + keyFramesDGV.ColumnHeadersHeight + 2;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false; //Set to false to be able to close even if there is an error in a DataGridView
            if (_unsavedChanges)
            {
                DialogResult dr = MessageBox.Show("Do you want to save unsaved changes before exiting?", "", MessageBoxButtons.YesNoCancel);
                e.Cancel = dr == DialogResult.Yes && !SaveSong() || dr == DialogResult.Cancel;
            }

            if (!e.Cancel)
            {
                //Under special circumstances Cef needs to shut down in order for Application.Run to return
                //Reproducing involves starting the application by double-clicking a vmp file (not through "Open with")
                //AND having Windows Magnify open.
                //So call Program.close (which calls Cef.Shutdown) here, not just in finally block in Program.cs
                //Another way to solve it is to show a message box here, which will magically un-jinx it. Makes perfect sense.
                Program.Close();
            }
        }

        private void KeyFramesDGV_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
        }

        private void KeyFramesDGV_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string str = keyFramesDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
            _keyFrameLockRow = e.RowIndex; //If enter was pressed, the currently selected row will change to the next row, firing the SelectionChanged event. In that event handler we can change back the selected row to keyFrameLockRow.
            if (e.ColumnIndex == 0)
            {
                //Time column edited
                int time;
                if (!int.TryParse(str, out time))
                {
                    ShowErrorMsgBox("Invalid format.");
                }
                else
                {
                    //Check if the entered time value is bigger than the song length
                    if (time >= Project.SongLengthT)
                    {
                        time = (int)Project.SongLengthT;
                        keyFramesDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = time;//.ToString();
                    }

                    if (time != Project.KeyFrames.Keys[e.RowIndex])
                    {
                        //A new time was entered.
                        int newRowIndex = Project.KeyFrames.ChangeTimeOfFrame(e.RowIndex, time);
                        if (newRowIndex < 0)
                        {
                            //Frame with specified time already exists
                            ShowErrorMsgBox("A key frame already exists at this position.");
                            keyFramesDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Project.KeyFrames.Keys[e.RowIndex];
                        }
                        else if (newRowIndex != e.RowIndex)
                        {
                            //The new time caused need for sorting
                            var row = keyFramesDGV.Rows[e.RowIndex];
                            _updatingControls = true;
                            keyFramesDGV.Rows.RemoveAt(e.RowIndex);
                            keyFramesDGV.Rows.Insert(newRowIndex, row);
                            _updatingControls = false;

                            //If the last row is edited, the selection won't go to next row, and the SelectionChanged event won't fire so instead of setting keyFrameLockRow here we set CurrentCell immediately.
                            if (e.RowIndex == keyFramesDGV.Rows.Count - 1)
                                keyFramesDGV.CurrentCell = keyFramesDGV.Rows[newRowIndex].Cells[0];
                            else
                                _keyFrameLockRow = newRowIndex;
                        }
                        else
                        {
                            //No sorting, so no SelectionChanged, so we need to update song pos here
                            Project.GoToKeyFrame(newRowIndex);
                            upDownVpWidth.Enabled = true;
                        }

                    }
                }
            }
            else if (e.ColumnIndex == 1)
            {
                //Description column edited
                Project.KeyFrames.Values[e.RowIndex].Desc = str;
            }
        }

        private void KeyFramesDGV_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void KeyFramesDGV_CurrentCellChanged(object sender, EventArgs e)
        {
            UpdateKeyFrameSelection();
        }

        void UpdateKeyFrameSelection()
        {
            for (int i = 0; i < keyFramesDGV.Rows.Count; i++)
            {
                if (i < Project.KeyFrames.Count)
                    Project.KeyFrames.Values[i].Selected = keyFramesDGV.Rows[i].Selected;
            }
            if (keyFramesDGV.CurrentRow != null)
                Project.KeyFrames.Values[keyFramesDGV.CurrentRow.Index].Selected = true;
        }

        private void SaveMixdownDialog_FileOk(object sender, CancelEventArgs e)
        {
            try { File.Copy(Media.GetAudioFilePath(), saveMixdownDialog.FileName, true); }
            catch (IOException ex)
            {
                ShowErrorMsgBox(ex.Message);
                e.Cancel = true;
            }
        }

        private void TrackList_DragLeave(object sender, EventArgs e)
        {
            trackList.RedrawItems(0, trackList.Items.Count - 1, false);
        }

        // required to use WINAPI for RegainFocus();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static public void RegainFocus(Process process)
        {
            const int SW_RESTORE = 9;
            ShowWindow(process.MainWindowHandle, SW_RESTORE);
            SetForegroundWindow(process.MainWindowHandle);
            SetActiveWindow(process.MainWindowHandle);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (RemuxerProcess == null || RemuxerProcess.HasExited)
                return;
            if (WindowState == FormWindowState.Minimized)
            {
                const int SW_MINIMIZE = 6;
                ShowWindow(RemuxerProcess.MainWindowHandle, SW_MINIMIZE);
            }
            else
                RegainFocus(RemuxerProcess);
        }

        private void Label27_Click(object sender, EventArgs e)
        {

        }

        private void LoadBkgBtn_Click(object sender, EventArgs e)
        {
            var dlgResult = openBkgDialog.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                openBkgDialog.InitialDirectory = Path.GetDirectoryName(openBkgDialog.FileName);
                SaveSettings();
                Project.Props.BackgroundImagePath = openBkgDialog.FileName;
                SongPanel.LoadBackgroundImage(Project.Props.BackgroundImagePath);
            }

        }

        private void UnloadBkgBtn_Click(object sender, EventArgs e)
        {
            Project.Props.BackgroundImagePath = "";
            SongPanel.UnloadBackgroundImage();
        }

        private void BkgOpacityUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            foreach (var keyFrame in Project.KeyFrames.Values)
            {
                if (keyFrame.Selected)
                    keyFrame.ProjProps.BackgroundImageOpacity = (float)bkgOpacityUd.Value;
            }
        }

        private void BkgSaturationUd_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            foreach (var keyFrame in Project.KeyFrames.Values)
            {
                if (keyFrame.Selected)
                    keyFrame.ProjProps.BackgroundImageSaturation = (float)bkgSaturationUd.Value;
            }
        }

        private async void TrackAudioFileTb_TextChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
                return;
            string failedFile = null;
            for (int i = 0; i < trackList.SelectedIndices.Count; i++)
            {
                var audioProps = Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.AudioProps;
                audioProps.Filename = trackAudioFileTb.Text;
                await audioProps.LoadAudioAsync();
                var ch = audioProps.SidWizChannel;
                // Probe from the UI thread. MediaFoundation readers (mp3 etc.) created on the
                // worker thread can't be marshalled to the STA UI render thread; this surfaces
                // such failures before the renderer hits them.
                bool readOk = false;
                try { readOk = ch.TestReadOnCurrentThread(); }
                catch { }
                if (failedFile == null && !ch.IsEmpty && (!string.IsNullOrEmpty(ch.ErrorMessage) || !readOk))
                    failedFile = trackAudioFileTb.Text;
            }
            if (failedFile != null)
                ShowWarningMsgBox($"Couldn't load audio file:\r\n{failedFile}", "Audio load failed");
        }

        private void BrowseTrackAudioBtn_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != openTrackAudioDlg.ShowDialog())
                return;
            openTrackAudioDlg.InitialDirectory = Path.GetDirectoryName(openTrackAudioDlg.FileName);
            SaveSettings();
            trackAudioFileTb.Text = openTrackAudioDlg.FileName;
        }
    }
}
