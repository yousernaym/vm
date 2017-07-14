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

namespace Visual_Music
{
	using GdiPoint = System.Drawing.Point;
	
	//using XnaPoint = Microsoft.Xna.Framework.Point;

	public partial class Form1 : Form
	{
        string[] startupArgs;
		int TrackTexPbHeight;
		int MaxTrackTexPbWidth;
		
		EventHandler eh_invalidateSongPanel;
		public string ProjectFolder
        { get => openProjDialog.InitialDirectory;
          set => openProjDialog.InitialDirectory = saveProjDialog.InitialDirectory = value; 
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

		TrackProps selectedTrackProps;
		const string trackPropsBtnText = "&Track Properties";
		string foldersFileName = Program.Dir+"\\folders";
		//SourceFileForm sourceFileForm;
        public ImportMidiForm importMidiForm;
        public ImportModForm importModForm;
        public ImportSidForm importSidForm;
		public TpartyIntegrationForm tpartyIntegrationForm;
		VideoExportForm vidExpForm;

		static public Type[] projectSerializationTypes = new Type[] { typeof(TrackProps), typeof(Material), typeof(TrackPropsTex), typeof(Microsoft.Xna.Framework.Point), typeof(Vector2), typeof(Vector3), typeof(NoteStyle_Bar), typeof(NoteStyle_Line), typeof(LineStyleEnum), typeof(LineHlStyleEnum), typeof(NoteStyle[]), typeof(NoteStyleEnum), typeof(List<TrackProps>), typeof(SourceSongType), typeof(MixdownType), typeof(Camera)};
        SongPanel songPanel = new SongPanel();
		ScrollBar songScrollBar = new HScrollBar();
		Settings settings = new Settings();
				
		public Form1(string[] args)
		{
			InitializeComponent();
			Application.Idle += delegate { songPanel.update(); };

			startupArgs = args;
			TrackTexPbHeight = trackTexPb.Height;
			MaxTrackTexPbWidth = trackTexPb.Width;
			eh_invalidateSongPanel = new EventHandler(invalidateSongPanel);
			trackListGfxObj = trackList.CreateGraphics();
			SetStyle(ControlStyles.OptimizedDoubleBuffer |
			ControlStyles.UserPaint |
			ControlStyles.AllPaintingInWmPaint, true);

			importMidiForm = new ImportMidiForm(this);
            importModForm = new ImportModForm(this);
            importSidForm = new ImportSidForm(this);
			tpartyIntegrationForm = new TpartyIntegrationForm();
			vidExpForm = new VideoExportForm();
			//trackPropsPanel =  new TrackPropsPanel(songPanel);
			//showTrackPropsBtn.Text = "Show " + trackPropsBtnText;

			ResizeRedraw = true;

            songScrollBar.Dock = DockStyle.Bottom;
			songScrollBar.ValueChanged += songScrollBar_ValueChanged;
			songScrollBar.Scroll += delegate { updatePlaybackPosWhilePlaying(); };
			Controls.Add(songScrollBar);
            songScrollBar.BringToFront();

            initSongPanel(songPanel);
            Array enumArray = Enum.GetValues(typeof(NoteStyleEnum));
            foreach (NoteStyleEnum nse in enumArray)
                styleList.Items.Add(nse.ToString());
   //         styleList.Items.Add(new NoteStyle_Default(null));
			//styleList.Items.Add(new NoteStyle_Bar(null));
			//styleList.Items.Add(new NoteStyle_Line(null));

            enumArray = Enum.GetValues(typeof(LineStyleEnum));
			foreach (LineStyleEnum lse in enumArray)
				lineStyleList.Items.Add(lse.ToString());
			enumArray = Enum.GetValues(typeof(LineHlStyleEnum));
			foreach (LineHlStyleEnum lse in enumArray)
				lineHlStyleList.Items.Add(lse.ToString());

			addInvalidateEH(this.Controls);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//try
			//{
				loadSettings();
			
				//}
				//catch
				//{
				//}
				//upDownVpWidth.Focus();
				//upDownVpWidth.Value = songPanel.Qn_viewWidth;

			bool bSongFile = false;
			bool bMidiFile = false;
            bool bModFile = false;
            bool bImportFiles = false;
            //string noteFile = null;
            string audioFile = null;
			foreach (string arg in startupArgs)
			{
				string ext = Path.GetExtension(arg);
				if (ext == ".mid")
				{
					importMidiForm.NoteFilePath = arg;
                    bMidiFile = bImportFiles = true;
				}
                else if (ext == ".mod" || ext == ".xm" || ext == ".s3m" || ext == ".it" || ext == ".stm")
                {
                    importModForm.NoteFilePath = arg;
                    bModFile = bImportFiles = true;
                }
                else if (ext == ".wav" || ext == ".mp3")
				{
					audioFile = arg;
					bImportFiles = true;
				}
				else if (ext == ".vms")
				{
					openSongFile(arg);
					bSongFile = true;
					break;
				}
			}
			if (!bSongFile && bImportFiles)
			{
                if (bModFile)
                {
                    importModForm.AudioFilePath = audioFile;
                    importModuleToolStripMenuItem.PerformClick();
                }
                else
                {
                    importMidiForm.AudioFilePath = audioFile;
                    importMidiToolStripMenuItem.PerformClick();
                }
                
			}
		}
		void addInvalidateEH(Control.ControlCollection controls)
		{
			foreach (Control control in controls)
			{
				if (control is TextBox)
					((TextBox)control).TextChanged += invalidateSongPanel;
				else if (control is NumericUpDown)
					((NumericUpDown)control).ValueChanged += invalidateSongPanel;
				else if (control is CheckBox)
					((CheckBox)control).CheckedChanged += invalidateSongPanel;
				else if (control is Button)
					((Button)control).Click += invalidateSongPanel;
				else if (control is RadioButton)
					((RadioButton)control).CheckedChanged += invalidateSongPanel;
				
				if (control.Controls.Count > 0)
					addInvalidateEH(control.Controls);
			}
		}

		private void importMidiSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (importMidiForm.ShowDialog(this) == DialogResult.OK)
                songPanel.SourceSongType = SourceSongType.Midi;
		}
        private void importModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (importModForm.ShowDialog(this) == DialogResult.OK)
                songPanel.SourceSongType = SourceSongType.Mod;
        }
                
        void songLoaded(string path)
		{
			bool loaded = !isEmpty(path);
			propsTogglePanel.Enabled = loaded;
			if (!loaded)
			{
				trackPropsCb.Checked = false;
				songPropsCb.Checked = false;
			}
			saveSongToolStripMenuItem.Enabled = loaded;
			saveSongAsToolStripMenuItem.Enabled = loaded;
			exportVideoToolStripMenuItem.Enabled = loaded;
            //startStopToolStripMenuItem.Enabled = !isEmpty(sourceFileForm.AudioFilePath);
            playbackToolStripMenuItem.Enabled = AudioLoaded;
			
			//if (loaded)
			//{
			createTrackList();
			updateTrackControls();
			//}

			upDownVpWidth.Value = songPanel.ViewWidthQn;
			audioOffsetS.Value = (decimal)songPanel.AudioOffset;
			maxPitchUd.Value = songPanel.Notes.MaxPitch;
			minPitchUd.Value = songPanel.Notes.MinPitch;

            songScrollBar.Maximum = songPanel.SongLengthT;
            songScrollBar.Value = songPanel.SongPosT;
            upDownVpWidth_ValueChanged(upDownVpWidth, EventArgs.Empty);
        }
        
		public bool openSourceFiles(string notePath, string audioPath, bool eraseCurrent, bool modInsTrack, MixdownType mixdownType, double songLengthS)
		{
			saveSettings();
			
			if (songPanel.importSong(notePath, audioPath, eraseCurrent, modInsTrack, mixdownType, songLengthS))
            {
				songLoaded(notePath);
                if (eraseCurrent)
                {
                    currentProjPath = "";
                    updateFormTitle("");
                }
                return true;
            }
            else
                return false;
		}

		void saveSettings()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(Settings), Settings.Types);
			using (FileStream stream = File.Open(Settings.Filename, FileMode.Create))
			{ 
				dcs.WriteObject(stream, settings);
			}
		}

		void loadSettings()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(Settings), Settings.Types);
			if (File.Exists(Settings.Filename))
			{
				using (FileStream stream = File.Open(Settings.Filename, FileMode.Open))
				{
					settings = (Settings)dcs.ReadObject(stream);
				}
			}			
		}
		
		private void panel1_MouseMove(object sender, MouseEventArgs e)
		{
			//GdiPoint clientP = songPanel.PointToClient(e.Location);
			GdiPoint clientP = e.Location;
			int middle = songPanel.ClientRectangle.Width / 2;
			songPanel.NormMouseX = (float)(clientP.X - middle) * 2 / songPanel.ClientRectangle.Width;
			songPanel.NormMouseY = (float)(clientP.Y) / songPanel.ClientRectangle.Height;
		}


		private void exportVideoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveVideoDlg.ShowDialog(this) != DialogResult.OK)
				return;
			saveVideoDlg.InitialDirectory = Path.GetDirectoryName(saveVideoDlg.FileName);
			saveSettings();

			if (vidExpForm.ShowDialog() != DialogResult.OK)
				return;
			RenderProgressForm renderProgressForm = new RenderProgressForm(songPanel, saveVideoDlg.FileName, vidExpForm);
			renderProgressForm.ShowDialog();
	}
		
		private void panel1_KeyDown(object sender, KeyEventArgs e)
		{

		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (ModifierKeys == Keys.Control)
			{
				//Reset camera
				if (e.KeyCode == Keys.R)
					songPanel.Camera = new Camera(songPanel);
			}

			//Debug rendering options---------------------
			//Skip notes that lies very close in screen space
			if (e.KeyCode == Keys.D1)
			{
				NoteStyle.bSkipClose = e.Control ? true : false;
				songPanel.Invalidate();
			}
			//Cull notes outside of view
			if (e.KeyCode == Keys.D2)
			{
				NoteStyle.bCull = e.Control ? true : false;
				songPanel.Invalidate();
			}
			//Skip line points that lie very close in screen space
			if (e.KeyCode == Keys.D3)
			{
				NoteStyle.bSkipPoints = e.Control ? true : false;
				songPanel.Invalidate();
			}
			//-----------------------------------

			if (ModifierKeys != 0)
				return;

			//Control camera
			if (songPanel.Camera.control(e.KeyCode, true))
				e.SuppressKeyPress = true;

			//Start/stop playback
			if (e.KeyCode == Keys.Space)
			{
				startStopToolStripMenuItem_Click(null, null);
				e.SuppressKeyPress = true;
			}
			//Toggle simple rendering mode (default camera and bar bar style)
			if (e.KeyCode == Keys.Z)
				songPanel.ForceSimpleDrawMode = !songPanel.ForceSimpleDrawMode;
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			songPanel.Camera.control(e.KeyCode, false);
		}

		private void upDownVpWidth_ValueChanged(object sender, EventArgs e)
		{
			//songPanel.Invalidate();
			songPanel.ViewWidthQn = (float)((TbSlider)sender).Value;
            songScrollBar.SmallChange = songPanel.ViewWidthT / 16;
            songScrollBar.LargeChange = songPanel.ViewWidthT / 1;
        }

		private void audioOffsetS_ValueChanged(object sender, EventArgs e)
		{
			songPanel.AudioOffset = (float)audioOffsetS.Value;
		}

		private void trackPropsBtn_Click(object sender, EventArgs e)
		{
			
		}
		void createTrackList()
		{
			trackList.Items.Clear();
			if (songPanel.Notes == null || songPanel.Notes.Tracks.Count == 0)
				return;
			trackList.BeginUpdate();
			trackList.Items.Add("Global");
			for (int i = 1; i < songPanel.Notes.Tracks.Count; i++)
			{
				int trackNumber = songPanel.TrackProps[i].TrackNumber;
				ListViewItem lvi = new ListViewItem(trackNumber.ToString() + " - " + songPanel.Notes.Tracks[trackNumber].Name);
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
			if (trackList.SelectedIndices.Count == 0)
			{
				selectedTrackProps = null;
				leftTrackPropsPanel.Enabled = false;
				defaultPropertiesToolStripMenuItem.Enabled = false;
			}
			else
			{
				selectedTrackProps = songPanel.TrackProps[trackList.SelectedIndices[0]];
				if (trackList.SelectedIndices[0] == 0 && trackList.SelectedIndices.Count == 1)
					transpSlider.Enabled = transpTb.Enabled = alphaLbl.Enabled = false;
				else
					transpSlider.Enabled = transpTb.Enabled = alphaLbl.Enabled = true;
				leftTrackPropsPanel.Enabled = true;
				defaultPropertiesToolStripMenuItem.Enabled = true;

				if (trackList.SelectedIndices.Count == 1 && trackList.SelectedIndices[0] == 0)
					globalLightCb.Enabled = false;
				else
					globalLightCb.Enabled = true;
			}
			updateTrackControls();
			//songPanel.Invalidate();
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
			try
			{
				number = Convert.ToInt32(((TextBox)sender).Text);
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
			for (int i=0;i<trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].Transp = value / 100.0f;
			updateTrackListColors();
		}

		private void hueTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hueSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].Hue = value / (float)(hueSlider.Maximum + 1);
			updateTrackListColors();
		}

		private void normalSatTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(normalSatSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].Normal.Sat = value / 100.0f;
			updateTrackListColors();

		}

		private void normalLumTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(normalLumSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].Normal.Lum = value / 100.0f;
			updateTrackListColors();
		}

		private void hiliteSatTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hiliteSatSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].Hilited.Sat = value / 100.0f;
			updateTrackListColors();
		}

		private void hiliteLumTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hiliteLumSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].Hilited.Lum = value / 100.0f;
			updateTrackListColors();
		}
		void loadMtrlTexInPb()
		{
			TrackPropsTex texProps = getActiveTexProps(selectedTrackProps);
			if (!string.IsNullOrEmpty(texProps.Path))
			{
				using (FileStream stream = File.Open(texProps.Path, FileMode.Open))
				{
					Image srcImage = new Bitmap(Image.FromStream(stream));
					trackTexPb.Image = new Bitmap(trackTexPb.Width, trackTexPb.Height);
					using (Graphics g = Graphics.FromImage(trackTexPb.Image))
					{
						if (getActiveTexProps(selectedTrackProps).PointSmp)
							g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
						g.DrawImage(srcImage, new System.Drawing.Rectangle(0, 0, trackTexPb.Width, trackTexPb.Height));
					}
					srcImage.Dispose();
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
		void updateTrackControls()
		{
			Invalidate();
			songPanel.Invalidate();
			updatingControls = true;
			if (selectedTrackProps != null)
			{
				transpTb.Text = ((int)(selectedTrackProps.Transp * 100 + 0.5f)).ToString();
				hueTb.Text = ((int)(selectedTrackProps.Hue * 99 + 0.5f)).ToString();
				normalSatTb.Text = ((int)(selectedTrackProps.Normal.Sat * 100 + 0.5f)).ToString();
				normalLumTb.Text = ((int)(selectedTrackProps.Normal.Lum * 100 + 0.5f)).ToString();
				hiliteSatTb.Text = ((int)(selectedTrackProps.Hilited.Sat * 100 + 0.5f)).ToString();
				hiliteLumTb.Text = ((int)(selectedTrackProps.Hilited.Lum * 100 + 0.5f)).ToString();
				
				TrackPropsTex texProps = getActiveTexProps(selectedTrackProps);
				texPathTb.Text = texProps.Path;
				loadMtrlTexInPb();
				pointSmpCb.Checked = texProps.PointSmp;
				texUTileCb.Checked = texProps.UTile;
				texVTileCb.Checked = texProps.VTile;
				updateTexUVCb(tileTexCb, texUTileCb, texVTileCb);
				texKeepAspectCb.Checked = texProps.KeepAspect;
				if (texProps.UAnchor == TexAnchorEnum.Note)
					noteUAnchorRb.Checked = true;
				else if (texProps.UAnchor == TexAnchorEnum.Screen)
					screenUAnchorRb.Checked = true;
				else
					songAnchorRb.Checked = true;
				if (texProps.VAnchor == TexAnchorEnum.Note)
					noteVAnchorRb.Checked = true;
				else
					screenVAnchorRb.Checked = true;
				texUScrollUD.Value = (decimal)texProps.UScroll;
				texVScrollUD.Value = (decimal)texProps.VScroll;
                //FixedTexXOriginCb.Checked = selectedTrackProps.FixedTexXOrigin;
                //FixedTexYOriginCb.Checked = selectedTrackProps.FixedTexYOrigin;
                //updateTexXYCb(FixedTexOriginCb, FixedTexXOriginCb, FixedTexYOriginCb);

                //Note style-----------------
                styleList.SelectedIndex = (int)selectedTrackProps.NoteStyleType;

                NoteStyle_Bar barStyle = selectedTrackProps.getBarNoteStyle();
                //Update bar style controls here
                
                NoteStyle_Line lineStyle = selectedTrackProps.getLineNoteStyle();
                lineStyleList.SelectedIndex = (int)lineStyle.Style;
				lineWidthUpDown.Value = lineStyle.LineWidth;
				qnGapFillUd.Value = (decimal)lineStyle.Qn_gapThreshold;
				borderSizeUd.Value = (decimal)lineStyle.BlurredEdge;
				
				lineHlStyleList.SelectedIndex = (int)lineStyle.HlStyle;
				hlSizeUpDown.Value = lineStyle.HlSize;
				movingHlCb.Checked = lineStyle.MovingHl;
				shrinkingHlCb.Checked = lineStyle.ShrinkingHl;
				hlBorderCb.Checked = lineStyle.HlBorder;
				
				fadeoutUd.Value = (decimal)(lineStyle.FadeOut * 100);
				shapePowerUD.Value = (decimal)lineStyle.ShapePower;
                //-------------------------------
				
                //Light---------------------------
				globalLightCb.Checked = selectedTrackProps.UseGlobalLight;
				lightDirxTb.Text = selectedTrackProps.LightDir.X.ToString();
				lightDiryTb.Text = selectedTrackProps.LightDir.Y.ToString();
				lightDirzTb.Text = selectedTrackProps.LightDir.Z.ToString();
				specAmountUd.Value = (decimal)selectedTrackProps.SpecAmount;
				specPowUd.Value = (decimal)selectedTrackProps.SpecPower;
				specFovUd.Value = (decimal)selectedTrackProps.SpecFov;
				//---------------------------------

				//Spatial---------------------------------
				xoffsetUd.Value = (decimal)selectedTrackProps.XOffset;
				yoffsetUd.Value = (decimal)selectedTrackProps.YOffset;
				zoffsetUd.Value = (decimal)selectedTrackProps.ZOffset;                  //---------------------------------------------
			}
			updatingControls = false;
		}
		void updateTrackListColors()
		{
			trackList.BeginUpdate();
			for (int i = 1; i < trackList.Items.Count; i++)
			{
				trackList.Items[i].SubItems[1].BackColor = songPanel.TrackProps[i].getSysColor(false, songPanel.GlobalTrackProps);
				trackList.Items[i].SubItems[2].BackColor = songPanel.TrackProps[i].getSysColor(true, songPanel.GlobalTrackProps);
			}
			trackList.EndUpdate();
		}
		static public bool isEmpty(string s)
		{
			return s == null || s == "";
		}

		private void trackPropsPanel_Enter(object sender, EventArgs e)
		{

		}

		private void leftTrackPropsPanel_Enter(object sender, EventArgs e)
		{

		}

		private void groupBox2_Enter(object sender, EventArgs e)
		{

		}

		private void trackList_Enter(object sender, EventArgs e)
		{

		}

		private void trackPropsPanel_Leave(object sender, EventArgs e)
		{

		}

		private void leftTrackPropsPanel_Leave(object sender, EventArgs e)
		{

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
				if (i==trackList.Items.Count-1 && p.Y > bounds.Bottom)
					item = trackList.Items[i];

			}
			//if (item == null)
				//item = trackList.Items[trackList.Items.Count - 1];
			return item;
		}
		private void trackList_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				ListViewItem dragToItem = getListViewItem(trackList, e);
				int dropIndex = dragToItem.Index;
				ListViewItem[] selectedItems = new ListViewItem[trackList.SelectedIndices.Count];
				ListViewItem[] newItems = new ListViewItem[trackList.SelectedIndices.Count];

				trackList.BeginUpdate();
				if ((e.KeyState & 8) != 8)
				{
					for (int i = 0; i < selectedItems.Length; i++)
						selectedItems[i] = trackList.SelectedItems[i];
					for (int i = 0; i < selectedItems.Length; i++)
						selectedItems[i].Selected = false; //Remove selection of old items before inserting new ones, otherwise all sorts of weird exceptions might occur
					for (int i = 0; i < selectedItems.Length; i++)
					{
						newItems[i] = (ListViewItem)selectedItems[i].Clone();
						int index = dropIndex + i + 1;
						songPanel.TrackProps.Insert(index, songPanel.TrackProps[selectedItems[i].Index]);
						//songPanel.Notes.Tracks.Insert(index, songPanel.Notes.Tracks[selectedItems[i].Index]);
						trackList.Items.Insert(index, newItems[i]);
					}
					for (int i = 0; i < selectedItems.Length; i++)
					{
						songPanel.TrackProps.RemoveAt(selectedItems[i].Index);
						//songPanel.Notes.Tracks.RemoveAt(selectedItems[i].Index);
						trackList.Items.Remove(selectedItems[i]);
					}
					for (int i = 0; i < selectedItems.Length; i++)
						newItems[i].Selected = true;	//After removal of old items it's now safe to select new items
				}
				else
				{
					for (int i = 0; i < trackList.SelectedIndices.Count; i++)
					{
						TrackProps source = songPanel.TrackProps[dropIndex];
						TrackProps dest = songPanel.TrackProps[trackList.SelectedIndices[i]];
						songPanel.TrackProps[trackList.SelectedIndices[i]] = source.copyTo(dest);
					}
					selectedTrackProps = songPanel.TrackProps[trackList.SelectedIndices[0]];
					updateTrackControls();
					updateTrackListColors();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				trackList.EndUpdate();
			}
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

				trackList.RedrawItems(0, trackList.Items.Count-1, false);
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
			if (styleList.SelectedIndex == (int)NoteStyleEnum.Line)
				lineStyleGroup.Visible = true;
			else
				lineStyleGroup.Visible = false;
			if (updatingControls)
				return;
			songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
			{
                NoteStyleEnum type = (NoteStyleEnum)Enum.Parse(typeof(NoteStyleEnum), (string)styleList.SelectedItem);
				songPanel.TrackProps[trackList.SelectedIndices[i]].NoteStyleType = type;
				//if (type == typeof(NoteStyle_Default))
					//songPanel.TrackProps[trackList.SelectedIndices[i]].NoteStyle = new NoteStyle_Default(null);
			}
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
			int i=0;
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
						getActiveTexProps(i).loadTexture(openTextureDlg.FileName, stream, songPanel);
					}
				}
			}
			catch (Exception ex)
			{
				getActiveTexProps(i).Path = "";
				MessageBox.Show(ex.Message);
			}
			updateTrackControls();
		}

		private TrackPropsTex getActiveTexProps(int index)
		{
			return getActiveTexProps(songPanel.TrackProps[trackList.SelectedIndices[index]]);
		}
		private TrackPropsTex getActiveTexProps(TrackProps trackProps)
		{
			//TODO: return texProps (0) or hmapProps (1) depending on which of the two is currently being edited.
			return trackProps.getTexProps(0);
		}
		private void unloadTexBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++ )
			{
				getActiveTexProps(i).unloadTexture();
			}
			updateTrackControls();
		}

		private void openSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openProjDialog.ShowDialog() != DialogResult.OK)
				return;
			ProjectFolder = Path.GetDirectoryName(openProjDialog.FileName);
			saveSettings();
			openSongFile(openProjDialog.FileName);
		}
		void openSongFile(string fileName)
		{
			try
			{
				DataContractSerializer dcs = new DataContractSerializer(typeof(SongPanel), projectSerializationTypes);
                using (FileStream stream = File.Open(fileName, FileMode.Open))
				{
					Controls.Remove(songPanel);
					songPanel.Dispose();
                    songPanel = (SongPanel)dcs.ReadObject(stream);
                }
                initSongPanel(songPanel);
				updateImportForm();
				
                currentProjPath = fileName;
				songLoaded(currentProjPath);
				updateFormTitle(currentProjPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
}
		
		void updateImportForm()
		{
			if (songPanel.SourceSongType == SourceSongType.Midi)
            {
                importMidiForm.NoteFilePath = songPanel.NoteFilePath;
                importMidiForm.AudioFilePath = songPanel.AudioFilePath;
            }
            else if (songPanel.SourceSongType == SourceSongType.Mod)
            {
                importModForm.NoteFilePath = songPanel.NoteFilePath;
                importModForm.AudioFilePath = songPanel.AudioFilePath;
                importModForm.InsTrack = songPanel.InsTrack;
            }
            else if (songPanel.SourceSongType == SourceSongType.Sid)
            {
                importSidForm.NoteFilePath = songPanel.NoteFilePath;
                importSidForm.AudioFilePath = songPanel.AudioFilePath;
            }
		}
		void updateFormTitle(string path)
		{
			Text = "Visual Music";
			if (!isEmpty(path))
				Text += " - " + Path.GetFileName(path);
		}
		private void saveSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (isEmpty(currentProjPath))
			{
				saveSongAs();
				return;
			}
			saveSong();
		}
		void saveSong()
		{
			try
			{
                DataContractSerializer dcs = new DataContractSerializer(typeof(SongPanel), projectSerializationTypes);

                using (FileStream stream = File.Open(currentProjPath, FileMode.Create))
				{

					dcs.WriteObject(stream, songPanel);
                }
				updateFormTitle(currentProjPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		void saveSongAs()
		{
			if (saveProjDialog.ShowDialog() != DialogResult.OK)
				return;
			saveMixdownDialog.FileName = Path.GetFileName(saveProjDialog.FileName);
			if (songPanel.MixdownType != MixdownType.None && saveMixdownDialog.ShowDialog() == DialogResult.OK)
			{
				File.Copy(Media.getAudioFilePath(), saveMixdownDialog.FileName, true);
				songPanel.MixdownType = MixdownType.None;
				songPanel.AudioFilePath = saveMixdownDialog.FileName;
				updateImportForm(); //To update audio file path
			}

			currentProjPath = saveProjDialog.FileName;
			ProjectFolder = Path.GetDirectoryName(currentProjPath);
			saveSettings();
			saveSong();
		}

		private void saveSongAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveSongAs();
		}
		void initSongPanel(SongPanel panel)
		{
			songPanel.Dock = DockStyle.Fill;
			songPanel.TabStop = false;

			songPanel.Visible = true;
			Controls.Add(songPanel);
			songPanel.BringToFront();
			songPanel.MouseMove += new MouseEventHandler(panel1_MouseMove);
			songPanel.KeyDown += new KeyEventHandler(panel1_KeyDown);
			songPanel.OnSongPosChanged = delegate ()
			{
				if (songPanel.SongPosT <= songScrollBar.Maximum && songPanel.SongPosT >= songScrollBar.Minimum)
					songScrollBar.Value = songPanel.SongPosT;
			};
		}

		private void lineWidthUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().LineWidth = (int)lineWidthUpDown.Value;
		}

		private void qnGapFillUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().Qn_gapThreshold = (int)qnGapFillUd.Value;
		}
		private void fadeoutUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().FadeOut = (float)((NumericUpDown)sender).Value / 100.0f;
		}

		private void lineStyleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			//if (lineStyleList.SelectedIndex != (int)LineStyleEnum.Ribbon)
			//	simpleLineStylePanel.Visible = true;
			//else
			//	simpleLineStylePanel.Visible = false;
			if (updatingControls)
				return;
			songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().Style = (LineStyleEnum)lineStyleList.SelectedIndex;
		}

		private void blurredEdgeUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().BlurredEdge = (int)borderSizeUd.Value;
		}
		private void invalidateSongPanel(object sender, EventArgs e)
		{
			songPanel.Invalidate();
            //songScrollBar.Value = songPanel.SongPosT;
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
				songPanel.TrackProps[trackList.SelectedIndices[i]].UseGlobalLight = globalLightCb.Checked;
		}

		private void lightDirxTb_TextChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].setLightDirX(getTextBoxNumberF(sender));
		}

		private void lightDiryTb_TextChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].setLightDirY(getTextBoxNumberF(sender));
		}

		private void lightDirzTb_TextChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].setLightDirZ(getTextBoxNumberF(sender));
		}

		private void specAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].SpecAmount = (float)specAmountUd.Value;
		}

		private void specPowUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].SpecPower = (float)specPowUd.Value;
		}

		private void specFovUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].SpecFov = (float)specFovUd.Value;
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
			songPanel.resetTrackProps(trackList.SelectedIndices);
			updateTrackControls();
			updateTrackListColors();
		}

		private void startStopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songPanel.togglePlayback();
		}

		private void beginningToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songPanel.stopPlayback();
		}
		private void endToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songPanel.stopPlayback();
			songScrollBar.Value = songScrollBar.Maximum;
		}

		private void button1_Click(object sender, EventArgs e)
		{

		}

		private void button2_Click(object sender, EventArgs e)
		{

		}

		private void defaultStyleBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].resetStyle();
			updateTrackControls();
		}

		private void resetBtn_Click(object sender, EventArgs e)
		{
			unloadTexBtn_Click(null, null);
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].resetMaterial();
			updateTrackControls();
			updateTrackListColors();

		}

		private void defaultLightBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].resetLight();
			updateTrackControls();
		}

		private void defaultSpatialBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].resetSpatial();
			updateTrackControls();
		}

		private void lineHlStyleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().HlStyle = (LineHlStyleEnum)lineHlStyleList.SelectedIndex;
		}

		private void hlSizeUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().HlSize = (int)hlSizeUpDown.Value;
		}

		private void movingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().MovingHl = ((CheckBox)sender).Checked;
		}

		private void shriunkingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().ShrinkingHl = ((CheckBox)sender).Checked;
		}

		private void hlBorderCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().HlBorder = ((CheckBox)sender).Checked;
		}

		private void trackPropsCb_CheckedChanged(object sender, EventArgs e)
		{
			if (trackPropsCb.Checked)
			{
				trackPropsPanel.Show();
				trackList.Focus();
			}
			else
				trackPropsPanel.Hide();
			
		}

		private void songPropsCb_CheckedChanged(object sender, EventArgs e)
		{
			if (songPropsCb.Checked)
				songPropsPanel.Show();
			else
				songPropsPanel.Hide();
		}

		private void maxPitchUd_ValueChanged(object sender, EventArgs e)
		{
			if ((int)maxPitchUd.Value < songPanel.Notes.MinPitch)
				maxPitchUd.Value = songPanel.Notes.MinPitch ;
			songPanel.MaxPitch = (int)maxPitchUd.Value;
		}

		private void minPitchUd_ValueChanged(object sender, EventArgs e)
		{
			if ((int)minPitchUd.Value > songPanel.Notes.MaxPitch)
				minPitchUd.Value = songPanel.Notes.MaxPitch;
			songPanel.MinPitch = (int)minPitchUd.Value;
		}

		private void defaultPitchesBtn_Click(object sender, EventArgs e)
		{
			maxPitchUd.Value = (decimal)(songPanel.MaxPitch = songPanel.Notes.MaxPitch);
			minPitchUd.Value = (decimal)(songPanel.MinPitch = songPanel.Notes.MinPitch);
        }

		private void pointSmpCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).PointSmp = ((CheckBox)sender).Checked;
			loadMtrlTexInPb();
		}

		//private void moveTexWithNotesCb_CheckedChanged(object sender, EventArgs e)
		//{
		//    if (updatingControls)
		//        return;
		//    if (((CheckBox)sender).CheckState != CheckState.Indeterminate)
		//    {
		//        for (int i = 0; i < trackList.SelectedIndices.Count; i++)
		//        {
		//            songPanel.TrackProps[trackList.SelectedIndices[i]].FixedTexXOrigin = ((CheckBox)sender).Checked;
		//            songPanel.TrackProps[trackList.SelectedIndices[i]].FixedTexYOrigin = ((CheckBox)sender).Checked;
		//        }
		//        updateTrackControls();
		//    }
		//}

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
				updateTrackControls();
			}
		}
		void updateTexUVCb(CheckBox uv, CheckBox u, CheckBox v)
		{
			if (u.Checked != v.Checked)
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
			updateTrackControls();
		}

		private void texVTileCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VTile = ((CheckBox)sender).Checked;
			updateTrackControls();
		}

		private void noteAnchorLabel_Click(object sender, EventArgs e)
		{
			noteUAnchorRb.PerformClick();
			noteVAnchorRb.PerformClick();
		}

		private void screenAnchorLabel_Click(object sender, EventArgs e)
		{
			screenUAnchorRb.PerformClick();
			screenVAnchorRb.PerformClick();
		}

		private void songAnchorLabel_Click(object sender, EventArgs e)
		{
			songAnchorRb.PerformClick();
		}

		private void noteUAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Note;
		}

		private void noteVAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
		}

		private void screenUAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Screen;
		}

		private void songAnchorRb_CheckedChanged(object sender, EventArgs e)
		{
			
		}

		private void screenVAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
		}

		private void songAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
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

		private void shapePowerUD_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].getLineNoteStyle().ShapePower = (float)((NumericUpDown)sender).Value;
		}

		private void texKeepAspect_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).KeepAspect = ((CheckBox)sender).Checked;
		}

        private void importSidSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (importSidForm.ShowDialog(this) == DialogResult.OK)
                songPanel.SourceSongType = SourceSongType.Sid;
        }

		private void tpartyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tpartyIntegrationForm.ShowDialog();
			saveSettings();
		}
		public static void showErrorMsgBox(IWin32Window owner, string message, string caption = "" )
		{
			MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		public static void showWarningMsgBox(IWin32Window owner, string message, string caption = "")
		{
			MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private void resetCamBtn_Click(object sender, EventArgs e)
		{
			songPanel.Camera = new Camera(songPanel);
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
			if (songPanel.IsPlaying)
			{
				songPanel.togglePlayback();
				songPanel.togglePlayback();
			}
		}
		private void songScrollBar_ValueChanged(object sender, EventArgs e)
		{
			songPanel.NormSongPos = (double)songScrollBar.Value / songScrollBar.Maximum;
		}

		private void xoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].XOffset = (float)xoffsetUd.Value;
		}

		private void yoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].YOffset = (float)yoffsetUd.Value;
		}

		private void zoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].ZOffset = (float)zoffsetUd.Value;
		}
	}
}
