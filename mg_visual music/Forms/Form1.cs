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
		
		//EventHandler eh_invalidateSongPanel;
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
		public bool UpdatingControls => updatingControls;
		public ListViewNF TrackList => trackList;

		TrackProps mergedTrackProps;
		const string trackPropsBtnText = "&Track Properties";
		string foldersFileName = Program.Dir+"\\folders";
		//SourceFileForm sourceFileForm;
        public ImportMidiForm importMidiForm;
        public ImportModForm importModForm;
        public ImportSidForm importSidForm;
		public TpartyIntegrationForm tpartyIntegrationForm;
		VideoExportForm vidExpForm;

		static public Type[] projectSerializationTypes = new Type[] { typeof(TrackView), typeof(TrackProps), typeof(NoteTypeMaterial), typeof(TrackPropsTex), typeof(Microsoft.Xna.Framework.Point), typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(NoteStyle_Bar), typeof(NoteStyle_Line), typeof(LineStyleEnum), typeof(LineHlStyleEnum), typeof(NoteStyle[]), typeof(NoteStyleEnum), typeof(List<TrackView>), typeof(Midi.FileType), typeof(MixdownType), typeof(Camera), typeof(List<NoteStyleMod>), typeof(SourceSongType) };
        SongPanel songPanel = new SongPanel();
		public SongPanel SongPanel => songPanel;
		Project project;
		public Project Project => project;
		float PosOffsetScale => Project.Camera.ViewportSize.X / 100.0f; //Pos offset is in percent of screen width
		Settings settings = new Settings();
		ScrollBar songScrollBar = new HScrollBar();
		NoteStyleControl currentNoteStyleControl;

		public Form1(string[] args)
		{
			InitializeComponent();
			//Application.Idle += delegate { songPanel.update(); };

			project = new Project(SongPanel);
			startupArgs = args;
			TrackTexPbHeight = trackTexPb.Height;
			MaxTrackTexPbWidth = trackTexPb.Width;
			//eh_invalidateSongPanel = new EventHandler(invalidateSongPanel);
			trackListGfxObj = trackList.CreateGraphics();
			SetStyle(ControlStyles.OptimizedDoubleBuffer |
			ControlStyles.UserPaint |
			ControlStyles.AllPaintingInWmPaint, true);

			importMidiForm = new ImportMidiForm(this);
            importModForm = new ImportModForm(this);
            importSidForm = new ImportSidForm(this);
			tpartyIntegrationForm = new TpartyIntegrationForm();
			vidExpForm = new VideoExportForm();
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

			addInvalidateEH(this.Controls);

			//barStyleControl = new BarStyleControl(this, songPanel);
			//lineStyleControl = new LineStyleControl(this, songPanel);
			//styleTab.Controls.Add(barStyleControl);
			//styleTab.Controls.Add(lineStyleControl);
			//barStyleControl.init(this);
			//lineStyleControl.init(this);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			try
			{
				loadSettings();
			
				}
				catch
				{
				}
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
				else if (control is ComboBox)
					((ComboBox)control).SelectedValueChanged += invalidateSongPanel;
				
				if (control.Controls.Count > 0)
					addInvalidateEH(control.Controls);
			}
		}

		private void importMidiSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			importMidiForm.ShowDialog(this);
		}
        private void importModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
			importModForm.ShowDialog(this);
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
			
			//if (loaded)
			//{
			createTrackList();
			updateTrackControls();
			//}

			upDownVpWidth.Value = Project.ViewWidthQn;
			audioOffsetS.Value = (decimal)Project.AudioOffset;
			maxPitchUd.Value = Project.MaxPitch;
			minPitchUd.Value = Project.MinPitch;

            songScrollBar.Maximum = Project.SongLengthT;
            songScrollBar.Value = Project.SongPosT;
            upDownVpWidth_ValueChanged(upDownVpWidth, EventArgs.Empty);
        }
        
		public bool openSourceFiles(string notePath, string audioPath, bool eraseCurrent, bool modInsTrack, MixdownType mixdownType, double songLengthS, Midi.FileType noteFileType)
		{
			saveSettings();
			
			if (Project.importSong(notePath, audioPath, eraseCurrent, modInsTrack, mixdownType, songLengthS, noteFileType))
            {
				if (eraseCurrent)
				{
					defaultPitchesBtn.PerformClick();
					project.resetPitchLimits();
					currentProjPath = "";
					updateFormTitle("");
                }
				songLoaded(notePath);
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
					Project.Camera = new Camera(songPanel);
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
			if (Project.Camera.control(e.KeyCode, true))
				e.SuppressKeyPress = true;

			//Start/stop playback
			if (e.KeyCode == Keys.Space)
			{
				startStopToolStripMenuItem_Click(null, null);
				e.SuppressKeyPress = true;
			}
			//Toggle force default note style
			if (e.KeyCode == Keys.Z)
				songPanel.ForceDefaultNoteStyle = true;
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			Project.Camera.control(e.KeyCode, false);
			if (e.KeyCode == Keys.Z)
				songPanel.ForceDefaultNoteStyle = false;
		}

		private void upDownVpWidth_ValueChanged(object sender, EventArgs e)
		{
			Project.ViewWidthQn = (float)((TbSlider)sender).Value;
            songScrollBar.SmallChange = Project.ViewWidthT / 16;
            songScrollBar.LargeChange = Project.ViewWidthT / 1;
		}

		private void upDownVpWidth_MouseUp(object sender, MouseEventArgs e)
		{
			project.createOcTrees();
		}

		private void audioOffsetS_ValueChanged(object sender, EventArgs e)
		{
			Project.AudioOffset = (float)audioOffsetS.Value;
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
			if (trackList.SelectedIndices.Count == 0)
			{
				selectedTrackPropsPanel.Enabled = false;
				defaultPropertiesToolStripMenuItem.Enabled = false;
			}
			else
			{
				transpSlider.Enabled = transpTb.Enabled = alphaLbl.Enabled = trackList.SelectedIndices[0] != 0;// || trackList.SelectedIndices.Count == 1);
				selectedTrackPropsPanel.Enabled = true;
				defaultPropertiesToolStripMenuItem.Enabled = true;
				globalLightCb.Enabled = trackList.SelectedIndices[0] != 0; // || trackList.SelectedIndices.Count == 1
			}
			updateTrackControls();
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
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.Transp = value / 100.0f;
			updateTrackListColors();
		}

		private void hueTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hueSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.Hue = value / (float)(hueSlider.Maximum + 1);
			updateTrackListColors();
		}

		private void normalSatTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(normalSatSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.Normal.Sat = value / 100.0f;
			updateTrackListColors();

		}

		private void normalLumTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(normalLumSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.Normal.Lum = value / 100.0f;
			updateTrackListColors();
		}

		private void hiliteSatTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hiliteSatSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.Hilited.Sat = value / 100.0f;
			updateTrackListColors();
		}

		private void hiliteLumTb_TextChanged(object sender, EventArgs e)
		{
			int value = getTextBoxNumber(sender);
			setTrackBarValue(hiliteLumSlider, value);
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.Hilited.Lum = value / 100.0f;
			updateTrackListColors();
		}
		void loadMtrlTexInPb()
		{
			TrackPropsTex texProps = getActiveTexProps(mergedTrackProps);
			if (!string.IsNullOrEmpty(texProps.Path))
			{
				using (FileStream stream = File.Open(texProps.Path, FileMode.Open))
				{
					Image srcImage = new Bitmap(Image.FromStream(stream));
					trackTexPb.Image = new Bitmap(trackTexPb.Width, trackTexPb.Height);
					using (Graphics g = Graphics.FromImage(trackTexPb.Image))
					{
						if (getActiveTexProps(mergedTrackProps).PointSmp ?? false)
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
		public void updateTrackControls()
		{
			mergedTrackProps = Project.mergeTrackProps(trackList.SelectedIndices);
			Invalidate();
			songPanel.Invalidate();
			updatingControls = true;
			if (mergedTrackProps != null)
			{
				//Note style-----------------
				if (mergedTrackProps.NoteStyleType == null)
					styleList.SelectedIndex = -1;
				else
					styleList.SelectedIndex = (int)mergedTrackProps.NoteStyleType;
				if (currentNoteStyleControl != null)
					currentNoteStyleControl.update(mergedTrackProps.SelectedNoteStyle);
				//barStyleControl.update(selectedTrackProps.getBarNoteStyle());
				//lineStyleControl.update(selectedTrackProps.getLineNoteStyle());
				//--------------------------------------------

				//Material-----------------------------------
				transpTb.Text = normToIntText(mergedTrackProps.Transp);
				hueTb.Text = normToIntText(mergedTrackProps.Hue, 101);
				normalSatTb.Text = normToIntText(mergedTrackProps.Normal.Sat);
				normalLumTb.Text = normToIntText(mergedTrackProps.Normal.Lum);
				hiliteSatTb.Text = normToIntText(mergedTrackProps.Hilited.Sat);
				hiliteLumTb.Text = normToIntText(mergedTrackProps.Hilited.Lum);

				//Texture
				TrackPropsTex texProps = getActiveTexProps(mergedTrackProps);
				texPathTb.Text = texProps.Path;
				loadMtrlTexInPb();
				pointSmpCb.CheckState = toCheckState(texProps.PointSmp);

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

				//Lighting
				setNumericUdValue(ambientAmountUd, mergedTrackProps.AmbientAmount);
				setNumericUdValue(diffuseAmountUd, mergedTrackProps.DiffuseAmount);
				setNumericUdValue(specAmountUd, mergedTrackProps.SpecAmount);
				setNumericUdValue(specPowUd, mergedTrackProps.SpecPower);
				//-------------------------------

				//Spatial---------------------------------
				setNumericUdValue(xoffsetUd, mergedTrackProps.XOffset / PosOffsetScale);
				setNumericUdValue(yoffsetUd, mergedTrackProps.YOffset / PosOffsetScale);
				setNumericUdValue(zoffsetUd, mergedTrackProps.ZOffset / PosOffsetScale);

				globalLightCb.CheckState = toCheckState(mergedTrackProps.UseGlobalLight);
				setNumericUdValue(lightDirXUd, mergedTrackProps.LightDirX);
				setNumericUdValue(lightDirYUd, mergedTrackProps.LightDirY);
				setNumericUdValue(lightDirZUd, mergedTrackProps.LightDirZ);
				//-------------------------------
			}
			updatingControls = false;
		}
		void updateTrackListColors()
		{
			trackList.BeginUpdate();
			for (int i = 1; i < trackList.Items.Count; i++)
			{
				trackList.Items[i].SubItems[1].BackColor = Project.TrackViews[i].TrackProps.getSysColor(false, Project.GlobalTrackProps);
				trackList.Items[i].SubItems[2].BackColor = Project.TrackViews[i].TrackProps.getSysColor(true, Project.GlobalTrackProps);
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
						Project.TrackViews.Insert(index, Project.TrackViews[selectedItems[i].Index]);
						//songPanel.Notes.Tracks.Insert(index, songPanel.Notes.Tracks[selectedItems[i].Index]);
						trackList.Items.Insert(index, newItems[i]);
					}
					for (int i = 0; i < selectedItems.Length; i++)
					{
						Project.TrackViews.RemoveAt(selectedItems[i].Index);
						trackList.Items.Remove(selectedItems[i]);
					}
					for (int i = 0; i < selectedItems.Length; i++)
						newItems[i].Selected = true;	//After removal of old items it's now safe to select new items
				}
				else
				{
					for (int i = 0; i < trackList.SelectedIndices.Count; i++)
					{
						TrackView source = Project.TrackViews[dropIndex];
						TrackView dest = Project.TrackViews[trackList.SelectedIndices[i]];
						source.cloneTrackProps(dest);
					}
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
			if (currentNoteStyleControl != null)
				currentNoteStyleControl.Visible = false;
			if (styleList.SelectedIndex == -1 || styleList.SelectedIndex == (int)NoteStyleEnum.Default)
				currentNoteStyleControl = null;
			else
			{
				if (styleList.SelectedIndex == (int)NoteStyleEnum.Bar)
					currentNoteStyleControl = barStyleControl;
				else if (styleList.SelectedIndex == (int)NoteStyleEnum.Line)
					currentNoteStyleControl = lineStyleControl;
				currentNoteStyleControl.Visible = true;
			}
			
			if (updatingControls)
				return;
			//songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
			{
                NoteStyleEnum type = (NoteStyleEnum)Enum.Parse(typeof(NoteStyleEnum), (string)styleList.SelectedItem);
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.NoteStyleType = type;
			}
			project.createOcTrees();
			updateTrackControls();
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
			return getActiveTexProps(Project.TrackViews[trackList.SelectedIndices[index]].TrackProps);
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
			//try
			//{
				DataContractSerializer dcs = new DataContractSerializer(typeof(Project), projectSerializationTypes);
                using (FileStream stream = File.Open(fileName, FileMode.Open))
				{
                    project = (Project)dcs.ReadObject(stream);
					Project.SongPanel = songPanel;
                }
				updateImportForm();
				
                currentProjPath = fileName;
				songLoaded(currentProjPath);
				updateFormTitle(currentProjPath);
			//}
			//catch (Exception ex)
			//{
			//	MessageBox.Show(ex.Message);
			//}
		}
		
		void updateImportForm()
		{
			if (Project.SourceSongType == Midi.FileType.Midi)
            {
                importMidiForm.NoteFilePath = Project.NoteFilePath;
                importMidiForm.AudioFilePath = Project.AudioFilePath;
            }
            else if (Project.SourceSongType == Midi.FileType.Mod)
            {
                importModForm.NoteFilePath = Project.NoteFilePath;
                importModForm.AudioFilePath = Project.AudioFilePath;
                importModForm.InsTrack = Project.InsTrack;
            }
            else if (Project.SourceSongType == Midi.FileType.Sid)
            {
                importSidForm.NoteFilePath = Project.NoteFilePath;
                importSidForm.AudioFilePath = Project.AudioFilePath;
            }
		}
		void updateFormTitle(string path)
		{
			Text = "Visual Music";
			if (!string.IsNullOrEmpty(path))
				Text += " - " + Path.GetFileName(path);
		}
		private void saveSongToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(currentProjPath))
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
                DataContractSerializer dcs = new DataContractSerializer(typeof(Project), projectSerializationTypes);

                using (FileStream stream = File.Open(currentProjPath, FileMode.Create))
				{

					dcs.WriteObject(stream, project);
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
			if (Project.MixdownType != MixdownType.None && saveMixdownDialog.ShowDialog() == DialogResult.OK)
			{
				File.Copy(Media.getAudioFilePath(), saveMixdownDialog.FileName, true);
				Project.MixdownType = MixdownType.None;
				Project.AudioFilePath = saveMixdownDialog.FileName;
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
			SongPanel.OnSongPosChanged = delegate ()
			{
				if (Project.SongPosT <= songScrollBar.Maximum && Project.SongPosT >= songScrollBar.Minimum)
					songScrollBar.Value = Project.SongPosT;
			};
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
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.UseGlobalLight = globalLightCb.Checked;
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
			Project.resetTrackProps(trackList.SelectedIndices);
			updateTrackControls();
			updateTrackListColors();
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
			updateTrackControls();
		}

		private void resetBtn_Click(object sender, EventArgs e)
		{
			unloadTexBtn_Click(null, null);
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetMaterial();
			updateTrackControls();
			updateTrackListColors();

		}

		private void defaultLightBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetLight();
			updateTrackControls();
		}

		private void defaultSpatialBtn_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.resetSpatial();
			updateTrackControls();
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
			if ((int)maxPitchUd.Value < Project.Notes.MinPitch)
				maxPitchUd.Value = Project.Notes.MinPitch ;
			Project.MaxPitch = (int)maxPitchUd.Value;
			project.createOcTrees();
		}

		private void minPitchUd_ValueChanged(object sender, EventArgs e)
		{
			if ((int)minPitchUd.Value > Project.Notes.MaxPitch)
				minPitchUd.Value = Project.Notes.MaxPitch;
			Project.MinPitch = (int)minPitchUd.Value;
			project.createOcTrees();
		}

		private void defaultPitchesBtn_Click(object sender, EventArgs e)
		{
			project.resetPitchLimits();
			maxPitchUd.Value = (decimal)Project.MaxPitch;
			minPitchUd.Value = (decimal)Project.MinPitch;
        }

		private void pointSmpCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).PointSmp = ((CheckBox)sender).Checked;
			//loadMtrlTexInPb();
			updateTrackControls();
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
				project.createOcTrees();
				updateTrackControls();
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
			project.createOcTrees();
			updateTrackControls();
		}

		private void texVTileCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VTile = ((CheckBox)sender).Checked;
			project.createOcTrees();
			updateTrackControls();
		}

		private void noteAnchorLabel_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = getActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
			project.createOcTrees();
			updateTrackControls();
		}

		private void screenAnchorLabel_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = getActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
			project.createOcTrees();
			updateTrackControls();
		}

		private void songAnchorLabel_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
			project.createOcTrees();
			updateTrackControls();
		}

		private void noteUAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Note;
			project.createOcTrees();
		}

		private void noteVAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VAnchor = TexAnchorEnum.Note;
			project.createOcTrees();
		}

		private void screenUAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Screen;
			project.createOcTrees();
		}

		private void screenVAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).VAnchor = TexAnchorEnum.Screen;
			project.createOcTrees();
		}

		private void songAnchorRb_Click(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).UAnchor = TexAnchorEnum.Song;
			project.createOcTrees();
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
			project.createOcTrees();
		}

        private void importSidSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
			importSidForm.ShowDialog(this);
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
			Project.Camera = new Camera(songPanel);
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
		private void songScrollBar_ValueChanged(object sender, EventArgs e)
		{
			Project.NormSongPos = (double)songScrollBar.Value / songScrollBar.Maximum;
		}

		private void xoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.XOffset = (float)xoffsetUd.Value * PosOffsetScale;
		}

		private void yoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.YOffset = (float)yoffsetUd.Value * PosOffsetScale;
		}

		private void zoffsetUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.ZOffset = (float)zoffsetUd.Value * PosOffsetScale;
		}

		CheckState toCheckState(bool? value)
		{
			return value == null ? CheckState.Indeterminate : ((bool)value ? CheckState.Checked : CheckState.Unchecked);
		}

		void setNumericUdValue(NumericUpDown ud, float? value)
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
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightDirX = (float)((NumericUpDown)sender).Value;
		}

		private void lightDirYUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightDirY = (float)((NumericUpDown)sender).Value;
		}

		private void lightDirZUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.LightDirZ = (float)((NumericUpDown)sender).Value;
		}

		private void ambientAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.AmbientAmount = (float)((NumericUpDown)sender).Value;
		}

		private void diffuseAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.DiffuseAmount = (float)((NumericUpDown)sender).Value;
		}

		private void specAmountUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpecAmount = (float)specAmountUd.Value;
		}

		private void specPowUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				Project.TrackViews[trackList.SelectedIndices[i]].TrackProps.SpecPower = (float)specPowUd.Value;
		}

		private void upDownVpWidth_CommitChanges(object sender, EventArgs e)
		{
			if (project.VertWidthScale != 1)
				project.createOcTrees();
			songPanel.Invalidate();
		}
	}
}
