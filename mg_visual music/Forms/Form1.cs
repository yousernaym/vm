using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.IO.Compression;
using CefSharp.Example.RequestEventHandler;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Visual_Music
{
	using GdiPoint = System.Drawing.Point;
	using GdiColor = System.Drawing.Color;
	using XnaColor = Microsoft.Xna.Framework.Color;

	enum TrackPropsType { TPT_Style = 1, TPT_Material = 2, TPT_Light = 4, TPT_Spatial = 8, TPT_All = 255 }

	public partial class Form1 : Form
	{
		readonly string[] TrackPropsTypeNames = { "Style", "Material", "Light", "Spatial" };
		string[] startupArgs;
		int TrackTexPbHeight;
		int MaxTrackTexPbWidth;

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
		
		string currentProjPath = "";

		public ListView.ListViewItemCollection trackListItems
		{
			get { return trackList.Items; }
		}

		bool AudioLoaded { get { return Media.getAudioLength() > 0; } }// !isEmpty(importMidiForm.AudioFilePath) || !isEmpty(importModForm.AudioFilePath) || !isEmpty(importSidForm.AudioFilePath); } }

		Graphics trackListGfxObj;
		Pen trackListPen = new Pen(System.Drawing.Color.White);
		bool updatingControls = false;
		bool updatingCamControls = false;
		public bool UpdatingControls => updatingControls;
		public ListViewNF TrackList => trackList;

		TrackProps mergedTrackProps;
		const string trackPropsBtnText = "&Track Properties";
		public static ImportMidiForm ImportMidiForm;
		public static ImportModForm ImportModForm;
		public static ImportSidForm ImportSidForm;
		static TpartyIntegrationForm tpartyIntegrationForm;
		public static TpartyIntegrationForm TpartyIntegrationForm => tpartyIntegrationForm;
		public static VideoExportForm VidExpForm;

		static public Type[] projectSerializationTypes = new Type[] { typeof(TrackView), typeof(TrackProps), typeof(StyleProps), typeof(MaterialProps), typeof(LightProps), typeof(SpatialProps), typeof(NoteTypeMaterial), typeof(TrackPropsTex), typeof(Microsoft.Xna.Framework.Point), typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(NoteStyle_Bar), typeof(NoteStyle_Line), typeof(LineType), typeof(LineHlType), typeof(NoteStyle[]), typeof(NoteStyleType), typeof(List<TrackView>), typeof(Midi.FileType), typeof(Midi.MixdownType), typeof(Camera), typeof(List<NoteStyleMod>), typeof(SourceSongType), typeof(ImportOptions), typeof(MidiImportOptions), typeof(ModImportOptions), typeof(SidImportOptions), typeof(Quaternion), typeof(XnaColor), typeof(BindingList<LyricsSegment>), typeof(LyricsSegment), typeof(KeyFrames), typeof(SortedList<int, KeyFrame>), typeof(KeyFrame), typeof(ProjProps) };
		static public SongPanel SongPanel { get; private set; } = new SongPanel();
		SongWebBrowser modWebBrowser;
		SongWebBrowser sidWebBrowser;
		SongWebBrowser midiWebBrowser;
		List<Control> screens = new List<Control>();
		Control currentScreen;
		public Project Project { get; private set; } = new Project();
		UndoItems undoItems = new UndoItems();
		static Settings settings = new Settings();
		public static Settings Settings { get => settings; }
		ScrollBar songScrollBar = new HScrollBar();
		NoteStyleControl currentNoteStyleControl;
		int keyFrameLockRow = -1;
		bool unsavedChanges = false;
        bool viewWidthQnChangedWithCtrl = false;
		bool textBoxEdited = false;
		bool focusChanged = false;

		[DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

		public Form1(string[] args)
		{
			InitializeComponent();

			//Turn off caps lock
			if (Control.IsKeyLocked(Keys.CapsLock))
			{
				pressCapsLock();
			}

			Camera.OnUserUpdating = updateCamControls;
			Camera.OnUserUpdated = ()=>addUndoItem("Edit Camera");

			SongPanel.Project = Project;
			startupArgs = args;
			TrackTexPbHeight = trackTexPb.Height;
			MaxTrackTexPbWidth = trackTexPb.Width;
			trackListGfxObj = trackList.CreateGraphics();
			SetStyle(ControlStyles.OptimizedDoubleBuffer |
			ControlStyles.UserPaint |
			ControlStyles.AllPaintingInWmPaint, true);

			ImportMidiForm = new ImportMidiForm(this);
			ImportModForm = new ImportModForm(this);
			ImportSidForm = new ImportSidForm(this);
			tpartyIntegrationForm = new TpartyIntegrationForm();
			VidExpForm = new VideoExportForm();
			ResizeRedraw = true;

			songScrollBar.Dock = DockStyle.Bottom;
			songScrollBar.ValueChanged += songScrollBar_ValueChanged;
			songScrollBar.Scroll += songScrollBar_Scroll;
			Controls.Add(songScrollBar);
			songScrollBar.BringToFront();

			modWebBrowser = new SongWebBrowser(this, "https://modarchive.org/index.php?request=view_searchbox");
			sidWebBrowser = new SongWebBrowser(this, "https://www.exotica.org.uk/wiki/Special:HVSC");
			midiWebBrowser = new SongWebBrowser(this, "https://www.freemidi.org/");

			screens.Add(SongPanel);
			screens.Add(modWebBrowser);
			screens.Add(sidWebBrowser);
			screens.Add(midiWebBrowser);
			foreach (var screen in screens)
			{
				screen.Dock = DockStyle.Fill;
				screen.Visible = true;
				Controls.Add(screen);
				screen.BringToFront();
			}
			initSongPanel();
			
			Array enumArray = Enum.GetValues(typeof(NoteStyleType));
			foreach (NoteStyleType nse in enumArray)
				styleList.Items.Add(nse.ToString());

			addEventHandlers(this.Controls);

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
		}

		static public void pressCapsLock()
		{
			const int KEYEVENTF_EXTENDEDKEY = 0x1;
			const int KEYEVENTF_KEYUP = 0x2;
			keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
			keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Download.init(this);
			try
			{
				loadSettings();
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
			//startupArgs = new string[] { @"cmd\" };

			foreach (string arg in startupArgs)
			{
				//MessageBox.Show(arg);
				string ext = arg.Split('.').Last().ToLower();

				//If no previous note file has been encountered, check if this extension is a note file
				if (!bImportFiles)
				{
					if (ImportMidiForm.Formats.Contains(ext))
					{
						ImportMidiForm.NoteFilePath = arg;
						bMidiFile = bImportFiles = true;
					}
					else if (ImportModForm.Formats.Contains(ext))
					{
						ImportModForm.NoteFilePath = arg;
						bModFile = bImportFiles = true;
					}
					else if (ImportSidForm.Formats.Contains(ext))
					{
						ImportSidForm.NoteFilePath = arg;
						bSidFile = bImportFiles = true;
					}
				}
				if (audioFile == null && (ext == ".wav" || ext == ".mp3"))
				{
					audioFile = arg;
				}
				else if (ext == ".vms" && !bImportFiles)
				{
					openSongFile(arg);
					bSongFile = true;
					break;
				}
			}
			if (!bSongFile && bImportFiles)
			{
				if (bMidiFile)
				{
					ImportMidiForm.AudioFilePath = audioFile;
					//importMidiToolStripMenuItem.PerformClick();
					ImportMidiForm.importFiles();
				}
				if (bModFile)
				{
					ImportModForm.AudioFilePath = audioFile;
					ImportModForm.importFiles();
				}
				else if (bSidFile)
				{
					ImportSidForm.AudioFilePath = audioFile;
					//importSidSongToolStripMenuItem.PerformClick();
					ImportSidForm.importFiles();
				}
			}
		}

		void addMenuItemEventHandlers(ToolStripItemCollection items)
		{
			foreach (var item in items)
			{
				if (item is ToolStripMenuItem)
				{
					ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
					menuItem.Click += onFocusChanged;
					menuItem.Click += addUndoItem;
					menuItem.Click += invalidateSongPanel;
					addMenuItemEventHandlers(menuItem.DropDownItems);
				}
			}
		}

		void addEventHandlers(Control.ControlCollection controls)
		{
			foreach (Control control in controls)
			{
				if (control.ContextMenuStrip != null)
					addMenuItemEventHandlers(control.ContextMenuStrip.Items);
				control.LostFocus += onFocusChanged;
				if (control.GetType() == typeof(TextBox))
				{
					((TextBox)control).TextChanged += invalidateSongPanel;
					((TextBox)control).TextChanged += addUndoItem;
				}
				else if (control.GetType() == typeof(TrackBar))
				{
					control.Validated += addUndoItem;
				}
				else if (control.GetType() == typeof(NumericUpDown))
				{
					((NumericUpDown)control).ValueChanged += invalidateSongPanel;
					((NumericUpDown)control).ValueChanged += addUndoItem;
				}
				else if (control.GetType() == typeof(CheckBox))
				{
					((CheckBox)control).CheckedChanged += invalidateSongPanel;
					((CheckBox)control).Click += addUndoItem;
				}
				else if (control.GetType() == typeof(Button))
				{
					((Button)control).Click += invalidateSongPanel;
					((Button)control).Click += addUndoItem;
				}
				else if (control.GetType() == typeof(RadioButton))
				{
					((RadioButton)control).CheckedChanged += invalidateSongPanel;
					((RadioButton)control).Click += addUndoItem;
				}
				else if (control.GetType() == typeof(ComboBox))
				{
					((ComboBox)control).SelectedIndexChanged += invalidateSongPanel;
					((ComboBox)control).SelectedIndexChanged += addUndoItem;
				}
				else if (control.GetType() == typeof(HueSatButton))
				{
					((HueSatButton)control).ColorChanged += invalidateSongPanel;
					((HueSatButton)control).ColorChanged += addUndoItem;
				}
				else if (control.GetType() == typeof(DataGridView))
				{
					((DataGridView)control).CellEndEdit += addUndoItem;
					((DataGridView)control).RowsRemoved += addUndoItem;
				}
				else if (control.GetType() == typeof(Label))
					((Label)control).Click += addUndoItem;
			
				else if (control.GetType() == typeof(MenuStrip))
					addMenuItemEventHandlers(((MenuStrip)control).Items);
				else if (control.Controls.Count > 0)
					addEventHandlers(control.Controls);
				else
					control.Click += invalidateSongPanel;
			}
		}

		private void onFocusChanged(object sender, EventArgs e)
		{
			focusChanged = true;
		}

		private void importMidiSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ImportMidiForm.Hide();
			if (ImportMidiForm.ShowDialog(this) == DialogResult.OK)
				SongPanel.Focus();
		}

		private void importModuleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ImportModForm.Hide();
			if (ImportModForm.ShowDialog(this) == DialogResult.OK)
				SongPanel.Focus();
		}

		void songLoaded(string path)
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
			actionsToolStripMenuItem.Enabled = loaded;
			loadCamToolStripMenuItem.Enabled = loaded;
			saveCamToolStripMenuItem.Enabled = loaded;
			insertLyricsHereToolStripMenuItem.Enabled = loaded;
			loadTrackPropsToolStripMenuItem.Enabled = loaded;
			saveTrackPropsToolStripMenuItem.Enabled = loaded;
			insertKeyFrameToolStripMenuItem.Enabled = loaded;
			//if (loaded)
			//{
			createTrackList();
			updateTrackPropsControls();
			//}
			updateProjPropsControls();

			undoItems.clear();
			undoToolStripMenuItem.Enabled = redoToolStripMenuItem.Enabled = false;
			undoItems.add("", Project);
			updateUndoRedoDesc();
			//project.KeyFrames[0].Camera.SpatialChanged();// = updateCamControls;
			//upDownVpWidth_ValueChanged(upDownVpWidth, EventArgs.Empty);
			changeToScreen(SongPanel);
		}

		private void updateProjPropsControls()
		{
			updatingControls = true;
			upDownVpWidth.Value = Project.KeyFrames[0].ViewWidthQn;
			audioOffsetS.Value = (decimal)Project.Props.AudioOffset;
			Project.Props.PlaybackOffsetS = Project.Props.PlaybackOffsetS;
			playbackOffsetUd.Value = (decimal)Project.Props.PlaybackOffsetS;
			songScrollBar.Maximum = (int)Project.SongLengthT;
			songScrollBar.Value = (int)Project.SongPosT;
			fadeInUd.Value = (decimal)Project.Props.FadeIn;
			fadeOutUd.Value = (decimal)Project.Props.FadeOut;
			maxPitchUd.Value = Project.Props.MaxPitch;
			minPitchUd.Value = Project.Props.MinPitch;
			buildKeyFramesDGV();
			updateCamControls();
			lyricsGridView.DataSource = Project.Props.LyricsSegments;
			updatingControls = false;
		}

		private void buildKeyFramesDGV()
		{
			keyFramesDGV.Rows.Clear();
			foreach (var frame in Project.KeyFrames)
				keyFramesDGV.Rows.Add(frame.Key, frame.Value.Desc);
			keyFramesDGV.CurrentCell = keyFramesDGV.Rows[0].Cells[0];
		}

		private void updateCamControls()
		{
			if (Project == null)
				return;
			updatingCamControls = true;
			Vector3 pos = Project.Props.Camera.Pos;
			Quaternion orient = Project.Props.Camera.Orientation;
			camTb.Text = $"{pos.X}\r\n{pos.Y}\r\n{pos.Z}\r\n\r\n{orient.X}\r\n{orient.Y}\r\n{orient.Z}\r\n{orient.W}";
			updatingCamControls = false;
		}

		//Called only when iomporting note and audio files.
		public bool openSourceFiles(ImportOptions options)
		{
			saveSettings();
			changeToScreen(SongPanel); //Hide browsers if they haven't been hidden yet. Otherwise the last browser will be brought to front during loading.Hopefully they have had time to initialize.
			try
			{
				SongPanel.SuspendPaint();
				if (!Project.importSong(options))
					return false;
				if (options.EraseCurrent)
				{
					currentProjPath = "";
					updateFormTitle("");
					resetCameras(false);
				}
				unsavedChanges = true;
				songLoaded(options.NotePath);
			}
			finally
			{
				SongPanel.ResumePaint();
			}
			return true;
		}

		void saveSettings()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(Settings), Settings.Types);
			using (FileStream stream = File.Open(Settings.FilePath, FileMode.Create))
			{
				dcs.WriteObject(stream, settings);
			}
		}

		void loadSettings()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(Settings), Settings.Types);
			if (File.Exists(Settings.FilePath))
			{
				using (FileStream stream = File.Open(Settings.FilePath, FileMode.Open))
				{
					settings = (Settings)dcs.ReadObject(stream);
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
                viewWidthQnChangedWithCtrl = true;
                upDownVpWidth.Value *= (float)Math.Pow(1.1, -delta);
			}
			else //scroll
			{
				bool wasPlaying;
				if (wasPlaying = Project.IsPlaying)
					Project.togglePlayback();

				delta /= Project.Notes.SongLengthT; //Scroll one tick
				if (ModifierKeys.HasFlag(Keys.Shift))
					delta *= Project.LargeScrollStepT;   //default large-step scroll is one "page"
				else
					delta *= Project.SmallScrollStepT;   //default small-step scroll is 1/16 of one "page" //(=one quarter note with default view width of 16 quarter notes)

				Project.NormSongPos -= delta;

				if (wasPlaying)
					Project.togglePlayback();
			}
		}

		private void exportVideoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (VidExpForm.ShowDialog() != DialogResult.OK)
				return;
			if (saveVideoDlg.ShowDialog(this) != DialogResult.OK)
				return;
			saveVideoDlg.InitialDirectory = Path.GetDirectoryName(saveVideoDlg.FileName);

			saveSettings();

			//var scBackup = Camera.SpatialChanged;
			//Camera.SpatialChanged = null;
			using (RenderProgressForm renderProgressForm = new RenderProgressForm(SongPanel, saveVideoDlg.FileName, VidExpForm.Options))
				renderProgressForm.ShowDialog();
			//Camera.SpatialChanged = scBackup;
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.MediaPlayPause)
				Project.togglePlayback();
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			if (Project == null || Project.Notes == null)
				return;
			
			//Project.Camera.toggleMouseControl(e.KeyCode, true))
			var keyFrame = Project.getKeyFrameAtSongPos();
			if (keyFrame != null)
				keyFrame.Camera.control(e.KeyCode, false);
			//Project.Camera.control(e.KeyCode, false);
			if (e.KeyCode == Keys.Z)
			{
				SongPanel.ForceDefaultNoteStyle = false;
				for (int t = 1; t < Project.TrackViews.Count; t++)
				{
					TrackProps tprops = Project.TrackViews[t].TrackProps;
					Project.TrackViews[t].createOcTree(Project, Project.GlobalTrackProps);
				}
			}
			else if (e.KeyCode == Keys.ControlKey && viewWidthQnChangedWithCtrl)
			{
				commitViewWidthQnChange();
			}
		}

		private void commitViewWidthQnChange()
		{
			if (Project.FxViewWidthQnScale == 1)
				return;
			Project.createOcTrees();
			SongPanel.Invalidate();
			addUndoItem("Edit Viewport Width");
		}

		private void upDownVpWidth_ValueChanged(object sender, EventArgs e)
		{
			songScrollBar.SmallChange = Project.SmallScrollStepT;
			songScrollBar.LargeChange = Project.LargeScrollStepT;
			if (updatingControls)
				return;
			foreach (var keyFrame in Project.KeyFrames.Values)
			{
				if (keyFrame.Selected)
					keyFrame.ViewWidthQn = (float)((TbSlider)sender).Value;
			}
		}

		private void audioOffsetS_ValueChanged(object sender, EventArgs e)
		{
			Project.Props.AudioOffset = (float)audioOffsetS.Value;
		}

		private void playbackOffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			decimal songLengthWithoutPbOffset = (decimal)Project.ticksToSeconds(Project.Notes.SongLengthT);
			if (-playbackOffsetUd.Value > songLengthWithoutPbOffset)
				playbackOffsetUd.Value = -songLengthWithoutPbOffset;

			Project.Props.PlaybackOffsetS = (float)playbackOffsetUd.Value;
			songScrollBar.Maximum = (int)Project.SongLengthT;
			songScrollBar.Value = (int)Project.SongPosT;
		}

		private void fadeInUd_ValueChanged(object sender, EventArgs e)
		{
			Project.Props.FadeIn = (float)((NumericUpDown)sender).Value;
		}

		private void fadeOutUd_ValueChanged(object sender, EventArgs e)
		{
			Project.Props.FadeOut = (float)((NumericUpDown)sender).Value;
		}

		private void trackPropsBtn_Click(object sender, EventArgs e)
		{

		}
		void createTrackList()
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

			updateTrackListColors();
			trackList.Items[0].Selected = true;
			trackList.Select();
			trackList.EndUpdate();
			//updateTrackListColors(null, null);
		}

		private void trackList_SelectedIndexChanged(object sender, EventArgs e)
		{
			enableTrackSpecificMenuItem();

			if (trackList.SelectedIndices.Count == 0)
				selectedTrackPropsPanel.Enabled = false;
			else
			{
				selectedTrackPropsPanel.Enabled = true;
				globalLightCb.Enabled = trackList.SelectedIndices[0] != 0; // || trackList.SelectedIndices.Count == 1
			}
			updateTrackPropsControls();
		}

		private void enableTrackSpecificMenuItem()
		{
			int itemCount = trackList.SelectedIndices.Count;
			defaultPropertiesToolStripMenuItem1.Enabled = defaultPropertiesToolStripMenuItem.Enabled = loadTrackPropsToolStripMenuItem.Enabled = loadPropertiesToolStripMenuItem.Enabled = saveTrackPropsToolStripMenuItem.Enabled = savePropertiesToolStripMenuItem.Enabled = itemCount > 0 || !trackPropsCb.Checked;
			saveTrackPropsToolStripMenuItem.Enabled = savePropertiesToolStripMenuItem.Enabled = itemCount == 1 || !trackPropsCb.Checked;
		}

		float getTrackBarValueNorm(object sender)
		{
			return ((TrackBar)sender).Value / 100.0f;
		}
		string getTrackBarValueString(object sender)
		{
			return ((TrackBar)sender).Value.ToString();
		}
		int getTextBoxNumber(object sender)
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
		float getTextBoxNumberF(object sender)
		{
			float number = 0;
			try
			{
				number = Convert.ToSingle(((TextBox)sender).Text);
			}
			catch { }
			return number;
		}
		void setTrackBarValue(TrackBar trackBar, int value)
		{
			if (value > trackBar.Maximum)
				value = trackBar.Maximum;
			else if (value < trackBar.Minimum)
				value = trackBar.Minimum;
			trackBar.Value = value;
		}
		private void transpSlider_Scroll(object sender, EventArgs e)
		{
			transpTb.Text = getTrackBarValueString(sender);
		}

		private void hueSlider_Scroll(object sender, EventArgs e)
		{
			hueTb.Text = getTrackBarValueString(sender);
		}

		private void normalSatSlider_Scroll(object sender, EventArgs e)
		{
			normalSatTb.Text = getTrackBarValueString(sender);
		}

		private void normalLumSlider_Scroll(object sender, EventArgs e)
		{
			normalLumTb.Text = getTrackBarValueString(sender);
		}

		private void hiliteSatSlider_Scroll(object sender, EventArgs e)
		{
			hiliteSatTb.Text = getTrackBarValueString(sender);
		}

		private void hiliteLumSlider_Scroll(object sender, EventArgs e)
		{
			hiliteLumTb.Text = getTrackBarValueString(sender);
		}

		private void transpTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(transpSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Transp = value / 100.0f;
			updateTrackListColors();
		}

		private void hueTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hueSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Hue = value / (float)(hueSlider.Maximum + 1);
			updateTrackListColors();
		}

		private void normalSatTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(normalSatSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Normal.Sat = value / 100.0f;
			updateTrackListColors();

		}

		private void normalLumTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(normalLumSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Normal.Lum = value / 100.0f;
			updateTrackListColors();
		}

		private void hiliteSatTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hiliteSatSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Hilited.Sat = value / 100.0f;
			updateTrackListColors();
		}

		private void hiliteLumTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hiliteLumSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.MaterialProps.Hilited.Lum = value / 100.0f;
			updateTrackListColors();
		}
		void loadMtrlTexInPb()
		{
			trackTexPb.Width = MaxTrackTexPbWidth;
			trackTexPb.Height = TrackTexPbHeight;
			TrackPropsTex texProps = getActiveTexProps(mergedTrackProps);
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
								if (getActiveTexProps(mergedTrackProps).PointSmp ?? false)
									g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
								g.DrawImage(srcImage, new System.Drawing.Rectangle(0, 0, trackTexPb.Width, trackTexPb.Height));
							}
						}
					}
				}
				catch (FileNotFoundException)
				{
					trackTexPb.Image = (Image)trackTexPb.ErrorImage.Clone();
					trackTexPb.Width = trackTexPb.ErrorImage.Width;
					trackTexPb.Height = trackTexPb.ErrorImage.Height;
					return;
				}


				float whRatio = (float)trackTexPb.Image.Width / trackTexPb.Image.Height;
				trackTexPb.Height = TrackTexPbHeight;
				trackTexPb.Width = (int)(trackTexPb.Height * whRatio);
				if (trackTexPb.Width > MaxTrackTexPbWidth)
				{
					float scale = (float)MaxTrackTexPbWidth / trackTexPb.Width;
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
		public void updateTrackPropsControls()
		{
			mergedTrackProps = Project.mergeTrackProps(trackList.SelectedIndices);
			Invalidate();
			SongPanel.Invalidate();
			updatingControls = true;
			if (mergedTrackProps != null)
			{
				//Note style-----------------
				if (mergedTrackProps.StyleProps.Type == null)
					styleList.SelectedIndex = -1;
				else
					styleList.SelectedIndex = (int)mergedTrackProps.StyleProps.Type;
				if (currentNoteStyleControl != null)
					currentNoteStyleControl.update(mergedTrackProps.ActiveNoteStyle);
				//barStyleControl.update(selectedTrackProps.getBarNoteStyle());
				//lineStyleControl.update(selectedTrackProps.getLineNoteStyle());
				//--------------------------------------------

				//Material-----------------------------------
				transpTb.Text = normToIntText(mergedTrackProps.MaterialProps.Transp);
				hueTb.Text = normToIntText(mergedTrackProps.MaterialProps.Hue, 101);
				normalSatTb.Text = normToIntText(mergedTrackProps.MaterialProps.Normal.Sat);
				normalLumTb.Text = normToIntText(mergedTrackProps.MaterialProps.Normal.Lum);
				hiliteSatTb.Text = normToIntText(mergedTrackProps.MaterialProps.Hilited.Sat);
				hiliteLumTb.Text = normToIntText(mergedTrackProps.MaterialProps.Hilited.Lum);

				//Texture
				TrackPropsTex texProps = getActiveTexProps(mergedTrackProps);
				texPathTb.Text = texProps.Path;
				loadMtrlTexInPb();
				disableTextureCh.CheckState = toCheckState(texProps.DisableTexture);
				pointSmpCb.CheckState = toCheckState(texProps.PointSmp);
				texColBlendCb.CheckState = toCheckState(texProps.TexColBlend);
				texUTileCb.CheckState = toCheckState(texProps.UTile);
				texVTileCb.CheckState = toCheckState(texProps.VTile);
				updateTexUVCb(tileTexCb, texUTileCb, texVTileCb);
				texKeepAspectCb.CheckState = toCheckState(texProps.KeepAspect);
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

				setNumericUdValue(texUScrollUD, texProps.UScroll);
				setNumericUdValue(texVScrollUD, texProps.VScroll);

				//Light
				globalLightCb.CheckState = toCheckState(mergedTrackProps.LightProps.UseGlobalLight);
				setNumericUdValue(lightDirXUd, mergedTrackProps.LightProps.DirX);
				setNumericUdValue(lightDirYUd, mergedTrackProps.LightProps.DirY);
				setNumericUdValue(lightDirZUd, mergedTrackProps.LightProps.DirZ);

				setNumericUdValue(ambientAmountUd, mergedTrackProps.LightProps.AmbientAmount);
				setNumericUdValue(diffuseAmountUd, mergedTrackProps.LightProps.DiffuseAmount);
				setNumericUdValue(specAmountUd, mergedTrackProps.LightProps.SpecAmount);
				ambientHsBtn.SelectedColor = xnaToGdiCol(mergedTrackProps.LightProps.AmbientColor);
				diffuseHsBtn.SelectedColor = xnaToGdiCol(mergedTrackProps.LightProps.DiffuseColor);
				specHsBtn.SelectedColor = xnaToGdiCol(mergedTrackProps.LightProps.SpecColor);
				lightFilterHsBtn.SelectedColor = xnaToGdiCol(mergedTrackProps.LightProps.Filter);
				setNumericUdValue(specPowUd, mergedTrackProps.LightProps.SpecPower);
				//-------------------------------

				//Spatial---------------------------------
				setNumericUdValue(xoffsetUd, mergedTrackProps.SpatialProps.XOffset);
				setNumericUdValue(yoffsetUd, mergedTrackProps.SpatialProps.YOffset);
				setNumericUdValue(zoffsetUd, mergedTrackProps.SpatialProps.ZOffset);
				//-------------------------------
			}
			updatingControls = false;
		}
		void updateTrackListColors()
		{
			trackList.BeginUpdate();
			for (int i = 1; i < trackList.Items.Count; i++)
			{
				trackList.Items[i].SubItems[1].BackColor = Project.TrackViews[i].TrackProps.MaterialProps.getSysColor(false, Project.GlobalTrackProps.MaterialProps);
				trackList.Items[i].SubItems[2].BackColor = Project.TrackViews[i].TrackProps.MaterialProps.getSysColor(true, Project.GlobalTrackProps.MaterialProps);
			}
			trackList.EndUpdate();
		}

		ListViewItem getListViewItem(ListView trackList, DragEventArgs e)
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
		private void trackList_DragDrop(object sender, DragEventArgs e)
		{
			
			ListViewItem dragToItem = getListViewItem(trackList, e);
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
				addUndoItem("Reorder tracks");
			}
			else //CTRL pressed
			{
				bool onlyCopyCurrentTab = (e.KeyState & 4) != 4; //SHIFT not pressed
				TrackView sourceTrackView = Project.TrackViews[dropIndex];
				TrackProps sourceTrackProps = sourceTrackView.TrackProps;
				for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				{
					TrackProps destTrackProps = Project.TrackViews[trackList.SelectedIndices[i]].TrackProps;
					TrackPropsType tpt = TrackPropsType.TPT_All;
					if (onlyCopyCurrentTab)
					{
						string tabName = selectedTrackPropsPanel.SelectedTab.Name;
						Enum.TryParse(tabName, out tpt);
					}
					destTrackProps.cloneFrom(sourceTrackProps, (int)tpt, SongPanel);
					Project.TrackViews[i].createOcTree(Project, Project.GlobalTrackProps);
				}
				updateTrackPropsControls();
				updateTrackListColors();
				addUndoItem("Copy track propesties");
			}
			trackList.EndUpdate();
		}

		private void trackList_DragEnter(object sender, DragEventArgs e)
		{
			//e.Effect = DragDropEffects.All;
		}

		private void trackList_ItemDrag(object sender, ItemDragEventArgs e)
		{
			if (trackList.SelectedIndices.Count == 0 || trackList.SelectedIndices[0] == 0)
				return;
			DoDragDrop(e.Item, DragDropEffects.All);
		}

		private void trackList_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				ListViewItem dragToItem = getListViewItem(trackList, e);

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
					trackListGfxObj.DrawLine(trackListPen, new GdiPoint(bounds.Left, bounds.Bottom), new GdiPoint(bounds.Right, bounds.Bottom));
				}
				else
				{
					e.Effect = DragDropEffects.Scroll | DragDropEffects.Copy;
					trackListGfxObj.DrawLine(trackListPen, new GdiPoint(bounds.Left, bounds.Top), new GdiPoint(bounds.Right, bounds.Top));
					trackListGfxObj.DrawLine(trackListPen, new GdiPoint(bounds.Left, bounds.Bottom), new GdiPoint(bounds.Right, bounds.Bottom));
					trackListGfxObj.DrawLine(trackListPen, new GdiPoint(bounds.Left, bounds.Top), new GdiPoint(bounds.Left, bounds.Bottom));
					trackListGfxObj.DrawLine(trackListPen, new GdiPoint(bounds.Right, bounds.Top), new GdiPoint(bounds.Right, bounds.Bottom));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void styleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (currentNoteStyleControl != null)
				currentNoteStyleControl.Visible = false;
			if (styleList.SelectedIndex == -1 || styleList.SelectedIndex == (int)NoteStyleType.Default)
				currentNoteStyleControl = null;
			else
			{
				if (styleList.SelectedIndex == (int)NoteStyleType.Bar)
					currentNoteStyleControl = barStyleControl;
				else if (styleList.SelectedIndex == (int)NoteStyleType.Line)
					currentNoteStyleControl = lineStyleControl;
				currentNoteStyleControl.Visible = true;
			}

			if (updatingControls)
				return;
			//SongPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
			{
				NoteStyleType type = (NoteStyleType)Enum.Parse(typeof(NoteStyleType), (string)styleList.SelectedItem);
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.StyleProps.Type = type;
			}
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void textureBrowseBtn_Click(object sender, EventArgs e)
		{
			if (openTextureDlg.ShowDialog(this) == DialogResult.OK)
			{
				openTextureDlg.InitialDirectory = Path.GetDirectoryName(openTextureDlg.FileName);
				texPathTb.Text = openTextureDlg.FileName;
				saveSettings();
			}
		}

		private void textureLoadBtn_Click(object sender, EventArgs e)
		{
			int i = 0;
			try
			{
				if (openTextureDlg.ShowDialog(this) != DialogResult.OK)
					return;

				openTextureDlg.InitialDirectory = Path.GetDirectoryName(openTextureDlg.FileName);
				saveSettings();

				for (i = 0; i < trackList.SelectedIndices.Count; i++)
				{
					//Texture2d.FromStream fails if file is loaded outside of for loop
					using (FileStream stream = File.Open(openTextureDlg.FileName, FileMode.Open))
					{
						getActiveTexProps(i).loadTexture(openTextureDlg.FileName, stream, SongPanel);
					}
				}
			}
			catch (Exception ex)
			{
				getActiveTexProps(i).Path = "";
				MessageBox.Show(ex.Message);
			}
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private TrackPropsTex getActiveTexProps(int index)
		{
			return getActiveTexProps(Project.TrackViews[trackList.SelectedIndices[index]].TrackProps);
		}
		private TrackPropsTex getActiveTexProps(TrackProps trackProps)
		{
			//TODO: return texProps (0) or hmapProps (1) depending on which of the two is currently being edited.
			return trackProps.MaterialProps.getTexProps(0);
		}
		private void unloadTexBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
			{
				getActiveTexProps(i).unloadTexture();
			}
			updateTrackPropsControls();
		}

		private void openSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openProjDialog.FileName = "";
			var dialogResult = openProjDialog.ShowDialog();
			if (dialogResult != DialogResult.OK)
			{
				//if (currentScreen is SongWebBrowser)
				currentScreen.Focus();
				return;
			}
			ProjectFolder = Path.GetDirectoryName(openProjDialog.FileName);
			saveSettings();
			changeToScreen(SongPanel); //Hide browsers if they haven't been hidden yet. Otherwise the last browser will be brought to front during loading.Hopefully they have had time to initialize.
			openSongFile(openProjDialog.FileName);
		}
		void openSongFile(string fileName)
		{
			try
			{
				SongPanel.SuspendPaint();
				DataContractSerializer dcs = new DataContractSerializer(typeof(Project), projectSerializationTypes);
				Project tempProject;
				using (FileStream stream = File.Open(fileName, FileMode.Open))
				{
					tempProject = (Project)dcs.ReadObject(stream);
				}
				SongPanel.Project = tempProject;
				tempProject.loadContent();
				Project = tempProject;
			}
			catch (Exception ex) when (ex is FormatException || ex is SerializationException || ex is FileNotFoundException)
			{
				showErrorMsgBox("Couldn't load song.\n" + ex.Message);
				SongPanel.Project = Project;
				return;
			}
			finally
			{
				SongPanel.ResumePaint();
			}
			if (Project.KeyFrames == null) //Old project file format
				Project.KeyFrames = new KeyFrames();

			Project.ImportOptions.updateImportForm();
			currentProjPath = fileName;
			songLoaded(currentProjPath);
			updateFormTitle(currentProjPath);
			Project.DefaultFileName = Path.GetFileName(currentProjPath);
			unsavedChanges = false;
		}

		void updateFormTitle(string path)
		{
			Text = "Visual Music";
			if (!string.IsNullOrEmpty(path))
				Text += " - " + Path.GetFileName(path);
		}
		private void saveSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveSong();
		}
		bool saveSong()
		{
			if (string.IsNullOrEmpty(currentProjPath))
			{
				return saveSongAs();
			}
			try
			{
				DataContractSerializer dcs = new DataContractSerializer(typeof(Project), projectSerializationTypes);

				string tempPath = Path.Combine(Program.TempDir, "tempprojectfile");
				using (FileStream stream = File.Open(tempPath, FileMode.Create))
				{
					dcs.WriteObject(stream, Project);
				}
				File.Copy(tempPath, currentProjPath, true);
				updateFormTitle(currentProjPath);
				unsavedChanges = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return false;
			}
			return true;
		}
		bool saveSongAs()
		{
			saveProjDialog.FileName = Project.DefaultFileName;
			if (saveProjDialog.ShowDialog() != DialogResult.OK)
				return false;
			ProjectFolder = Path.GetDirectoryName(saveProjDialog.FileName);
			saveSettings();

			//Save audio mixdown
			saveMixdownDialog.FileName = Path.ChangeExtension(saveProjDialog.FileName, "wav");
			if (Project.ImportOptions.MixdownType != Midi.MixdownType.None && saveMixdownDialog.ShowDialog() == DialogResult.OK)
			{
				File.Copy(Media.getAudioFilePath(), saveMixdownDialog.FileName, true);
				Project.ImportOptions.MixdownType = Midi.MixdownType.None;
				Project.ImportOptions.AudioPath = saveMixdownDialog.FileName;
				Project.ImportOptions.updateImportForm(); //To update audio file path
			}

			//Save midi "mixdown"
			saveMidiDialog.FileName = Path.ChangeExtension(saveProjDialog.FileName, "mid");
			if (!Project.ImportOptions.SavedMidi && Project.ImportOptions.NoteFileType != Midi.FileType.Midi && saveMidiDialog.ShowDialog() == DialogResult.OK)
			{
				File.Copy(Project.ImportOptions.MidiOutputPath, saveMidiDialog.FileName, true);
				Project.ImportOptions.MidiOutputPath = saveMidiDialog.FileName;
				Project.ImportOptions.SavedMidi = true;
				Project.ImportOptions.updateImportForm(); //To update audio file path
			}

			currentProjPath = saveProjDialog.FileName;
			return saveSong();
		}

		private void saveSongAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveSongAs();
		}
		void initSongPanel()
		{
			SongPanel.TabStop = false;
			SongPanel.MouseWheel += new MouseEventHandler(SongPanel_MouseWheel);
			SongPanel.OnSongPosChanged = delegate ()
			{
				updatingControls = true;
				SongPanel.Invalidate();
				if (Project.SongPosT <= songScrollBar.Maximum && Project.SongPosT >= songScrollBar.Minimum)
					songScrollBar.Value = (int)Project.SongPosT;
				upDownVpWidth.Value = Project.Props.ViewWidthQn;

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
				updateCamControls();
				updatingControls = false;
			};
			changeToScreen(SongPanel, false);
		}

		private void invalidateSongPanel(object sender, EventArgs e)
		{
			SongPanel.Invalidate();
			//songScrollBar.Value = SongPanel.SongPosT;
		}

		private void addUndoItem(object sender, EventArgs e)
		{
			object tag = null;
			if (sender is Control)
				tag = ((Control)sender).Tag;
			else if (sender is ToolStripMenuItem )
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
			addUndoItem(desc);
		}

		void addUndoItem(string desc)
		{
			if (updatingControls)
				return;

			if (!focusChanged)
				undoItems--;
			undoItems.add(desc, Project);
			//else
				//.replaceLast(desc, Project);
			focusChanged = false;

			undoToolStripMenuItem.Enabled = true;
			redoToolStripMenuItem.Enabled = false;
			updateUndoRedoDesc();
			unsavedChanges = true;
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			undoItems--;
			applyUndoItem();
			if (undoItems.Previous == null)
				undoToolStripMenuItem.Enabled = false;
			redoToolStripMenuItem.Enabled = true;
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			undoItems++;
			applyUndoItem();
			if (undoItems.Next == null)
				redoToolStripMenuItem.Enabled = false;
			undoToolStripMenuItem.Enabled = true;
		}

		void applyUndoItem()
		{
			Project.copyPropsFrom(undoItems.Current.Project);
			updateProjPropsControls();
			updateTrackPropsControls();
			if (undoItems.RedoDesc == "Reorder tracks")
				createTrackList();
			else
				updateTrackListColors();
			SongPanel.Invalidate();
			updateUndoRedoDesc();
		}

		void updateUndoRedoDesc()
		{
			undoToolStripMenuItem.Text = "Undo " + undoItems.UndoDesc;
			redoToolStripMenuItem.Text = "Redo " + undoItems.RedoDesc;
		}

		private void trackPropsPanel_Paint(object sender, PaintEventArgs e)
		{

		}

		private void globalLightCb_CheckedChanged(object sender, EventArgs e)
		{
			lightPanel.Enabled = !globalLightCb.Checked;
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.UseGlobalLight = globalLightCb.Checked;
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			trackList.Select();
			trackList.BeginUpdate();
			for (int i = 1; i < trackList.Items.Count; i++)
				trackList.Items[i].Selected = true;
			trackList.EndUpdate();
		}

		private void invertSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			trackList.Select();
			trackList.BeginUpdate();
			for (int i = 1; i < trackList.Items.Count; i++)
				trackList.Items[i].Selected = !trackList.Items[i].Selected;
			trackList.EndUpdate();
		}

		private void defaultPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			defaultPropertiesToolStripMenuItem1.PerformClick();
		}

		private void startStopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Project.togglePlayback();
		}

		private void beginningToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Project.stopPlayback();
		}
		private void endToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Project.stopPlayback();
			songScrollBar.Value = songScrollBar.Maximum;
		}

		private void defaultStyleBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetStyle();
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void defaultMaterial_Click(object sender, EventArgs e)
		{
			//unloadTexBtn.PerformClick();// _Click(null, null);
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetMaterial();
			updateTrackPropsControls();
			updateTrackListColors();

		}

		private void defaultLightBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetLight();
			updateTrackPropsControls();
		}

		private void defaultSpatialBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetSpatial();
			updateTrackPropsControls();
		}

		private void trackPropsCb_CheckedChanged(object sender, EventArgs e)
		{
			enableTrackSpecificMenuItem();
			trackPropsPanel.Visible = trackPropsCb.Checked;
			if (trackPropsCb.Checked)
				trackList.Focus();
		}

		private void songPropsCb_CheckedChanged(object sender, EventArgs e)
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

		private void maxPitchUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			if ((int)maxPitchUd.Value < Project.Notes.MinPitch)
			{
				maxPitchUd.Value = Project.Notes.MinPitch;
				return;
			}
			Project.Props.MaxPitch = (int)maxPitchUd.Value;
			Project.createOcTrees();
		}

		private void minPitchUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			if ((int)minPitchUd.Value > Project.Notes.MaxPitch)
				minPitchUd.Value = Project.Notes.MaxPitch;
			Project.Props.MinPitch = (int)minPitchUd.Value;
			Project.createOcTrees();
		}

		void resetPitchLimits()
		{
			Project.resetPitchLimits();
			updatingControls = true;
			maxPitchUd.Value = (decimal)Project.Props.MaxPitch;
			minPitchUd.Value = (decimal)Project.Props.MinPitch;
			updatingControls = false;
		}

		private void defaultPitchesBtn_Click(object sender, EventArgs e)
		{
			resetPitchLimits();
		}

		private void disableTextureCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).DisableTexture = ((CheckBox)sender).Checked;
		}

		private void pointSmpCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).PointSmp = ((CheckBox)sender).Checked;
			updateTrackPropsControls();
		}

		private void texColBlendCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).TexColBlend = ((CheckBox)sender).Checked;
		}

		private void tileTexCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			if (((CheckBox)sender).CheckState != CheckState.Indeterminate)
			{
				for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				{
					getActiveTexProps(i).UTile = ((CheckBox)sender).Checked;
					getActiveTexProps(i).VTile = ((CheckBox)sender).Checked;
				}
				Project.createOcTrees();
				updateTrackPropsControls();
			}
		}
		void updateTexUVCb(CheckBox uv, CheckBox u, CheckBox v)
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
		private void texUTileCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UTile = ((CheckBox)sender).Checked;
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void texVTileCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VTile = ((CheckBox)sender).Checked;
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void noteAnchorLabel_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = getActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void screenAnchorLabel_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = getActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void songAnchorLabel_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
			Project.createOcTrees();
			updateTrackPropsControls();
		}

		private void noteUAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Note;
			Project.createOcTrees();
		}

		private void noteVAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
			Project.createOcTrees();
		}

		private void screenUAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Screen;
			Project.createOcTrees();
		}

		private void screenVAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
			Project.createOcTrees();
		}

		private void songAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
			Project.createOcTrees();
		}

		private void texUScrollUD_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UScroll = (float)((NumericUpDown)sender).Value;
		}

		private void texVScrollUD_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VScroll = (float)((NumericUpDown)sender).Value;
		}

		private void texKeepAspect_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).KeepAspect = ((CheckBox)sender).Checked;
			Project.createOcTrees();
		}

		private void importSidSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ImportSidForm.Hide();
			if (ImportSidForm.ShowDialog(this) == DialogResult.OK)
				SongPanel.Focus();
		}

		private void tpartyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TpartyIntegrationForm.ShowDialog();
			saveSettings();
		}
		public static void showErrorMsgBox(string message, string caption = "")
		{
			MessageBox.Show(null, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		public static void showWarningMsgBox(string message, string caption = "")
		{
			MessageBox.Show(null, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		void resetCameras(bool onlySelected, Camera newCam = null)
		{
			foreach (var keyFrame in Project.KeyFrames.Values)
			{
				if (keyFrame.Selected || !onlySelected)
				{
					keyFrame.Camera = newCam ?? new Camera();
				}
			}
			Project.interpolateFrames();
			updateCamControls();
		}
		private void resetCamBtn_Click(object sender, EventArgs e)
		{
			resetCameras(true);
		}

		private void nudgeBackwardsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songScrollBar.Value = Math.Max(songScrollBar.Minimum, songScrollBar.Value - songScrollBar.SmallChange);
			updatePlaybackPosWhilePlaying();
		}

		private void nudgeForwardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songScrollBar.Value = Math.Min(songScrollBar.Maximum, songScrollBar.Value + songScrollBar.SmallChange);
			updatePlaybackPosWhilePlaying();
		}

		private void jumpBackwardsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songScrollBar.Value = Math.Max(songScrollBar.Minimum, songScrollBar.Value - songScrollBar.LargeChange);
			updatePlaybackPosWhilePlaying();
		}

		private void jumpForwardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songScrollBar.Value = Math.Min(songScrollBar.Maximum, songScrollBar.Value + songScrollBar.LargeChange);
			updatePlaybackPosWhilePlaying();
		}

		void updatePlaybackPosWhilePlaying()
		{
			if (Project.IsPlaying)
			{
				Project.togglePlayback();
				Project.togglePlayback();
			}
		}
		void songScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.Type != ScrollEventType.EndScroll)
				Project.tempPausePlayback();
			else
				Project.resumeTempPausedPlayback();
		}

		private void songScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			Project.NormSongPos = songScrollBar.Value / Project.SongLengthT;
		}

		private void xoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpatialProps.XOffset = (float)xoffsetUd.Value;
		}

		private void yoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpatialProps.YOffset = (float)yoffsetUd.Value;
		}

		private void zoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpatialProps.ZOffset = (float)zoffsetUd.Value;
		}

		public static CheckState toCheckState(bool? value)
		{
			return value == null ? CheckState.Indeterminate : ((bool)value ? CheckState.Checked : CheckState.Unchecked);
		}

		public static void setNumericUdValue(NumericUpDown ud, float? value)
		{
			if (value == null)
				ud.Text = null;
			else
				ud.Value = (decimal)value;
		}

		string normToIntText(float? value, float scale = 100)
		{
			if (value == null)
				return null;
			else
				return ((int)((float)value * scale + 0.5f)).ToString();
		}

		private void lightDirXUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DirX = (float)((NumericUpDown)sender).Value;
		}

		private void lightDirYUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DirY = (float)((NumericUpDown)sender).Value;
		}

		private void lightDirZUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DirZ = (float)((NumericUpDown)sender).Value;
		}

		private void ambientAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.AmbientAmount = (float)((NumericUpDown)sender).Value;
		}

		private void diffuseAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DiffuseAmount = (float)((NumericUpDown)sender).Value;
		}

		private void specAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.SpecAmount = (float)specAmountUd.Value;
		}

		private void ambientHsBtn_ColorChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.AmbientColor = gdiToXnaCol(ambientHsBtn.SelectedColor);
		}

		private void diffuseHsBtn_ColorChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.DiffuseColor = gdiToXnaCol(diffuseHsBtn.SelectedColor);
		}

		private void specHsBtn_ColorChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.SpecColor = gdiToXnaCol(specHsBtn.SelectedColor);
		}

		private void lightFilterHsBtn_ColorChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.Filter = gdiToXnaCol(lightFilterHsBtn.SelectedColor);
		}
		

		public static XnaColor gdiToXnaCol(GdiColor gdiCol)
		{
			return new XnaColor(gdiCol.R, gdiCol.G, gdiCol.B);
		}

		public static GdiColor xnaToGdiCol(XnaColor? xnaCol)
		{
			if (xnaCol == null)
				return GdiColor.Black;
			else
			{
				XnaColor c = (XnaColor)xnaCol;
				return GdiColor.FromArgb(c.A, c.R, c.G, c.B);
			}
		}

		private void specPowUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightProps.SpecPower = (float)specPowUd.Value;
		}

		private void upDownVpWidth_CommitChanges(object sender, EventArgs e)
		{
			commitViewWidthQnChange();
		}

		private void viewSongTSMI_Click(object sender, EventArgs e)
		{
			changeToScreen(SongPanel);
		}

		private void viewModBrowserTSMI_Click(object sender, EventArgs e)
		{
			changeToScreen(modWebBrowser);
		}

		private void viewSidBrowserTSMI_Click(object sender, EventArgs e)
		{
			changeToScreen(sidWebBrowser);
		}

		private void viewMidiBrowserTSMI_Click(object sender, EventArgs e)
		{
			changeToScreen(midiWebBrowser);
		}

		void changeToScreen(Control newScreen, bool hideOthers = true)
		{
			if (!hideOthers)
			{
				newScreen.BringToFront();
			}
			else
			{
				foreach (var screen in screens)
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
			currentScreen = newScreen;
		}

		private void trackPropsPanel_VisibleChanged(object sender, EventArgs e)
		{
			if (!trackPropsPanel.Visible)
				SongPanel.Focus();
		}

		private void songPropsPanel_VisibleChanged(object sender, EventArgs e)
		{
			if (!songPropsPanel.Visible)
				SongPanel.Focus();
		}

		private void camTb_TextChanged(object sender, EventArgs e)
		{
			camTb.ForeColor = GdiColor.Black;
			if (updatingCamControls)
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
						//project.Camera.Pos = pos;
						foreach (var keyFrame in Project.KeyFrames.Values)
						{
							if (keyFrame.Selected)
								keyFrame.Camera.Pos = pos;
						}
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
						//project.Camera.Orientation = orient;
						foreach (var keyFrame in Project.KeyFrames.Values)
						{
							if (keyFrame.Selected)
								keyFrame.Camera.Orientation = orient;
						}
						break;
				}
				elementIndex++;
			}
		}

		private bool colorDialogButtonClick(Button button)
		{
			colorDialog1.Color = button.BackColor;
			if (colorDialog1.ShowDialog() != DialogResult.OK)
				return false;
			button.BackColor = colorDialog1.Color;
			return true;
		}

		private void loadTrackPropsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			loadPropertiesToolStripMenuItem.PerformClick();
		}

		private void saveTrackPropsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			savePropertiesToolStripMenuItem.PerformClick();
		}

		void loadTrackProps(int typeFlags)
		{
			//Show open-file dialog.
			if (openTrackPropsFileDialog.ShowDialog() != DialogResult.OK)
				return;
			TrackPropsFolder = Path.GetDirectoryName(openTrackPropsFileDialog.FileName);
			saveSettings();

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
				showErrorMsgBox(ex.Message);
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
						throw new FileFormatException($"File doesn't contain properties of type {TrackPropsTypeNames[i]}");
				}
			}
			if (trackPropsCb.Checked)
			{
				foreach (int selectedTrackIndex in trackList.SelectedIndices)
				{
					Project.TrackViews[selectedTrackIndex].TrackProps.cloneFrom(props, typeFlags, SongPanel);
				}
			}
			else
				Project.TrackViews[0].TrackProps.cloneFrom(props, typeFlags, SongPanel);

			addUndoItem("Load Track Properties");
			updateTrackListColors();
			updateTrackPropsControls();
		}
		void saveTrackProps(int typeFlags)
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
			saveSettings();

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
				showErrorMsgBox(ex.Message);
			}

		}
		private void loadTrackPropsTypeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string tabName = selectedTrackPropsPanel.SelectedTab.Name;
			int tpt = (int)Enum.Parse(typeof(TrackPropsType), tabName);
			loadTrackProps(tpt);
		}

		private void saveTrackPropsTypeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string tabName = selectedTrackPropsPanel.SelectedTab.Name;
			int tpt = (int)Enum.Parse(typeof(TrackPropsType), tabName);
			saveTrackProps(tpt);
		}

		private void trackListCM_Opening(object sender, CancelEventArgs e)
		{
			//int numSelectedTracks = TrackList.SelectedIndices.Count;
			//defaultPropertiesToolStripMenuItem.Enabled = numSelectedTracks > 0;
			//saveTrackPropsToolStripMenuItem.Enabled = numSelectedTracks == 1;
			//loadTrackPropsToolStripMenuItem.Enabled = numSelectedTracks > 0;
			
		}

		private void resetCamToolStripMenuItem_Click(object sender, EventArgs e)
		{
			resetCameras(true);
		}

		private void loadCamToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openCamFileDialog.ShowDialog() != DialogResult.OK)
				return;
			CamFolder = Path.GetDirectoryName(openCamFileDialog.FileName);
			saveSettings();
			
			DataContractSerializer dcs = new DataContractSerializer(typeof(Camera), projectSerializationTypes);
			try
			{
				using (FileStream stream = File.Open(openCamFileDialog.FileName, FileMode.Open))
				{
					Camera cam = (Camera)dcs.ReadObject(stream);
					resetCameras(true, cam);
				}
			}
			catch (Exception ex)
			{
				showErrorMsgBox(ex.Message);
			}
			addUndoItem("Load Camera");
		}

		private void saveCamToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveCamFileDialog.ShowDialog() != DialogResult.OK)
				return;
			CamFolder = Path.GetDirectoryName(saveCamFileDialog.FileName);
			saveSettings();

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
				showErrorMsgBox(ex.Message);
			}
		}

		private void loadPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			loadTrackProps(-1);
		}

		private void savePropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveTrackProps(-1);
		}

		private void tracksToolStripMenuItem_EnabledChanged(object sender, EventArgs e)
		{
			//When Tracks menu item is disabled, all the sub items need to be disabled, otherwise their shortcut keys will still work.
			//bool enabled = tracksToolStripMenuItem.Enabled;

			//When Tracks menu is enabled, some sub items should remain disadled depending on how many tracks are selected.
			//if (enabled)
				//enableTrackSpecificMenuItem();
		}

		private void defaultPropertiesToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			//Project.resetTrackProps(trackPropsCb.Checked ? trackList.SelectedIndices : null);
			if (trackList.SelectedIndices != null)
			{
				foreach (int index in trackList.SelectedIndices)
					Project.TrackViews[index].TrackProps.resetProps();
			}
			else
				Project.TrackViews[0].TrackProps.resetProps();
			Project.createOcTrees();
			updateTrackPropsControls();
			updateTrackListColors();
		}

		private void lyricsGridView_Paint(object sender, PaintEventArgs e)
		{
			lyricsGridView.Height = lyricsGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + lyricsGridView.ColumnHeadersHeight + 2;
		}

		private void lyricsGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			//showErrorMsgBox("The entered value has an invalid format.");
			//e.ThrowException = false;
		}

		private void lyricsGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			SongPanel.Invalidate();
			float time;
			if (e.ColumnIndex == 0 && !float.TryParse((string)e.FormattedValue, out time))
			{
				showErrorMsgBox("Invalid format.");
				e.Cancel = true;
			}
		}

		private void lyricsGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{

		}

		private void insertLyricsHereToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songPropsCb.Checked = true;
			lyricsGridView.Show();
			int row = Project.insertLyrics();
			var cell = lyricsGridView.Rows[row].Cells[1];
			lyricsGridView.CurrentCell = cell;
			lyricsGridView.BeginEdit(true);
		}

		private void lyricsGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			if (Project.Props.LyricsSegments.Count == 0)
				lyricsGridView.Hide();
		}

		private void insertKeyFrameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int row = Project.insertKeyFrameAtSongPos();
			if (row < 0)
			{
				showErrorMsgBox("A keyframe already exists here.");
				return;
			}
			
			keyFramesDGV.Rows.Insert(row, Project.SongPosT, "");
			songPropsCb.Checked = true;
			keyFramesDGV.CurrentCell = keyFramesDGV.Rows[row].Cells[0]; //Select cell 0 to update CurrentRow. Needed for SelectionChanged event to go to correct song pos.
			keyFramesDGV.CurrentCell = keyFramesDGV.Rows[row].Cells[1]; //Select cell in Description column
			//keyFramesDGV.BeginEdit(true);
			addUndoItem("Insert Key Frame");
		}

		private void keyFramesDGV_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
		
		}

		private void keyFramesDGV_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			if (updatingControls)
				return;
			Project.KeyFrames.removeIndex(e.RowIndex);
		}

		private void keyFramesDGV_SelectionChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			if (keyFrameLockRow >= 0)
			{
				keyFramesDGV.CurrentCell = keyFramesDGV.Rows[keyFrameLockRow].Cells[0];
				keyFrameLockRow = -1;
			}

			updateKeyFrameSelection();
			
			if (keyFramesDGV.CurrentRow != null)
			{
				Project.goToKeyFrame(keyFramesDGV.CurrentRow.Index);
				updatingControls = true;
				upDownVpWidth.Value = Project.KeyFrames.Values[keyFramesDGV.CurrentRow.Index].ViewWidthQn;
				updatingControls = false;

			}
			SongPanel.Invalidate();
		}

		private void keyFramesDGV_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			if (keyFramesDGV.Rows.Count == 1)
				e.Cancel = true;
		}

		private void keyFramesDGV_Paint(object sender, PaintEventArgs e)
		{
			//keyFramesDGV.Height = keyFramesDGV.Rows.GetRowsHeight(DataGridViewElementStates.None) + keyFramesDGV.ColumnHeadersHeight + 2;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = false; //Set to false to be able to close even if there is an error in a DataGridView
			if (unsavedChanges)
			{
				DialogResult dr = MessageBox.Show("Do you want to save unsaved changes before exiting?", "", MessageBoxButtons.YesNoCancel);
				e.Cancel = dr == DialogResult.Yes && !saveSong() || dr == DialogResult.Cancel;
			}
		}

		private void keyFramesDGV_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
		}

		private void keyFramesDGV_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			string str = keyFramesDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
			keyFrameLockRow = e.RowIndex; //If enter was pressed, the currently selected row will change to the next row, firing the SelectionChanged event. In that event handler we can change back the selected row to keyFrameLockRow.
			if (e.ColumnIndex == 0)
			{
				//Time column edited
				int time;
				if (!int.TryParse(str, out time))
				{
					showErrorMsgBox("Invalid format.");
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
						int newRowIndex = Project.KeyFrames.changeTimeOfFrame(e.RowIndex, time);
						if (newRowIndex < 0)
						{
							//Frame with specified time already exists
							showErrorMsgBox("A key frame already exists at this position.");
							keyFramesDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Project.KeyFrames.Keys[e.RowIndex];
						}
						else if (newRowIndex != e.RowIndex)
						{
							//The new time caused need for sorting
							var row = keyFramesDGV.Rows[e.RowIndex];
							updatingControls = true;
							keyFramesDGV.Rows.RemoveAt(e.RowIndex);
							keyFramesDGV.Rows.Insert(newRowIndex, row);
							updatingControls = false;

							//If the last row is edited, the selection won't go to next row, and the SelectionChanged event won't fire so instead of setting keyFrameLockRow here we set CurrentCell immediately.
							if (e.RowIndex == keyFramesDGV.Rows.Count - 1)
								keyFramesDGV.CurrentCell = keyFramesDGV.Rows[newRowIndex].Cells[0];
							else
								keyFrameLockRow = newRowIndex;
						}
						else
						{
							//No sorting, so no SelectionChanged, so we need to update song pos here
							Project.goToKeyFrame(newRowIndex);
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

		private void keyFramesDGV_KeyDown(object sender, KeyEventArgs e)
		{
		
		}

		private void keyFramesDGV_CurrentCellChanged(object sender, EventArgs e)
		{
			updateKeyFrameSelection();
		}

		void updateKeyFrameSelection()
		{
			for (int i = 0; i < keyFramesDGV.Rows.Count; i++)
			{
				if (i < Project.KeyFrames.Count)
					Project.KeyFrames.Values[i].Selected = keyFramesDGV.Rows[i].Selected;
			}
			if (keyFramesDGV.CurrentRow != null)
				Project.KeyFrames.Values[keyFramesDGV.CurrentRow.Index].Selected = true;
		}
	}
}
