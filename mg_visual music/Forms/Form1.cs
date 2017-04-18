﻿using System;
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
using System.Runtime.Serialization.Formatters.Binary;

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
		string vmSongFolder = "";
		string currentSongPath = "";
		public ListView.ListViewItemCollection trackListItems
		{
			get { return trackList.Items; }
		}

        bool AudioLoaded { get { return !isEmpty(importMidiForm.AudioFilePath) || !isEmpty(importModForm.AudioFilePath); } }

        Graphics trackListGfxObj;
		Pen trackListPen = new Pen(System.Drawing.Color.White);
		bool updatingControls = false;

		TrackProps selectedTrackProps;
		const string trackPropsBtnText = "&Track Properties";
		string foldersFileName = Program.Path+"\\folders";
		//SourceFileForm sourceFileForm;
        ImportMidiForm importMidiForm;
        ImportModForm importModForm;

        SongPanel songPanel = new SongPanel();
        ScrollBar songScrollBar = new HScrollBar();
        //Panel trackPropsPanel = new Panel();
        //public int SelectedTrack
        //{
        //get { return (int)upDownVpWidth.Value; }
        //}
        public Form1(string[] args)
		{
			InitializeComponent();
						
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
            //trackPropsPanel =  new TrackPropsPanel(songPanel);
            //showTrackPropsBtn.Text = "Show " + trackPropsBtnText;

            try
			{
				//MessageBox.Show(foldersFileName);
				using (StreamReader file = new StreamReader(foldersFileName))
				{
					importMidiForm.NoteFolder = file.ReadLine();
                    importModForm.NoteFolder = file.ReadLine();
                    importMidiForm.AudioFolder = file.ReadLine();
                    importModForm.AudioFolder = file.ReadLine();
                    saveVideoDlg.InitialDirectory = file.ReadLine();
					openTextureDlg.InitialDirectory = file.ReadLine();
					vmSongFolder = file.ReadLine();
					openProjDialog.InitialDirectory = vmSongFolder;
					saveProjDialog.InitialDirectory = vmSongFolder;
				}
			}
			catch
			{
			}
			ResizeRedraw = true;

            songScrollBar.Dock = DockStyle.Bottom;
            songScrollBar.Scroll += delegate {
                songPanel.NormSongPos = (double)songScrollBar.Value / songScrollBar.Maximum;
                songPanel.Invalidate();
            };
            Controls.Add(songScrollBar);
            songScrollBar.BringToFront();

            initSongPanel(songPanel);
			styleList.Items.Add(new NoteStyle_Default(null));
			styleList.Items.Add(new NoteStyle_Bar(null));
			styleList.Items.Add(new NoteStyle_Line(null));
			Array enumArray = Enum.GetValues(typeof(LineStyleEnum));
			foreach (LineStyleEnum lse in enumArray)
				lineStyleList.Items.Add(lse.ToString());
			enumArray = Enum.GetValues(typeof(LineHlStyleEnum));
			foreach (LineHlStyleEnum lse in enumArray)
				lineHlStyleList.Items.Add(lse.ToString());

			addInvalidateEH(this.Controls);
            

		}

		private void Form1_Load(object sender, EventArgs e)
		{
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
                songPanel.IsMod = false;
		}
        private void importModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (importModForm.ShowDialog(this) == DialogResult.OK)
                songPanel.IsMod = true;
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

			upDownVpWidth.Value = songPanel.Qn_viewWidth;
			audioOffsetS.Value = (decimal)songPanel.AudioOffset;
			maxPitchUd.Value = songPanel.Notes.MaxPitch;
			minPitchUd.Value = songPanel.Notes.MinPitch;

            songScrollBar.Maximum = songPanel.SongLengthT;
            songScrollBar.Value = songPanel.SongPosT;
            upDownVpWidth_ValueChanged(upDownVpWidth, EventArgs.Empty);
        }

		//void updateFormWithSongProps(string noteFilename = "")
		//{
		//    Text = "Visual Music";
		//    string songName = isEmpty(currentSongPath) ? noteFilename : currentSongPath;
		//    if (!isEmpty(songName))
		//    {
		//        Text += " - " + songName;
		//        createTrackList();
		//        trackPropsBtn.Enabled = true;
		//        updateTrackControls();
		//    }
		//    else
		//        trackPropsBtn.Enabled = false;
		//}

		public bool openSourceFiles(string notePath, string audioPath, bool eraseCurrent, bool modInsTrack)
		{
            writeFolderNames();
            if (songPanel.importSong(notePath, audioPath, eraseCurrent, modInsTrack))
            {
                songLoaded(notePath);
                if (eraseCurrent)
                {
                    currentSongPath = "";
                    updateFormTitle("");
                }
                return true;
            }
            else
                return false;
		}

		private void panel1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				songPanel.MbPressed = true;
			if (e.Button == MouseButtons.Right)
				songPanel.RightMbPressed = true;
			toggleIdleRendering(true);

		}
		private void panel1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				songPanel.MbPressed = false;
			}
			if (e.Button == MouseButtons.Right)
			{
				//songPanel.showNoteInfo(e.Location);
				songPanel.RightMbPressed = false;
			}
			toggleIdleRendering(false);
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
			writeFolderNames();
			
			RenderProgressForm renderProgressForm = new RenderProgressForm(songPanel, saveVideoDlg.FileName);
			renderProgressForm.ShowDialog(this);
		}
		void writeFolderNames()
		{
			StreamWriter file = new StreamWriter(foldersFileName);
			file.WriteLine(importMidiForm.NoteFolder);
            file.WriteLine(importModForm.NoteFolder);
            file.WriteLine(importMidiForm.AudioFolder);
            file.WriteLine(importModForm.AudioFolder);
            file.WriteLine(saveVideoDlg.InitialDirectory);
			file.WriteLine(openTextureDlg.InitialDirectory);
			openProjDialog.InitialDirectory = vmSongFolder;
			saveProjDialog.InitialDirectory = vmSongFolder;
			file.WriteLine(vmSongFolder);
			file.Close();
		}

		private void panel1_KeyDown(object sender, KeyEventArgs e)
		{

		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				startStopToolStripMenuItem_Click(null, null);
				e.SuppressKeyPress = true;
			}
			if (e.KeyCode == Keys.R)
			{
				resetPositionToolStripMenuItem_Click(null, null);
				e.SuppressKeyPress = true;
			}
			if (e.Control)
			{
				if (e.KeyCode == Keys.D)
				{
					//trackList.Select();
					//songPanel.resetTrackProps(trackList.SelectedIndices);
				}
				if (e.KeyCode == Keys.A)
				{
					
					//trackList.Select();
				}
			}
		}

		private void upDownVpWidth_ValueChanged(object sender, EventArgs e)
		{
			//songPanel.Invalidate();
			songPanel.Qn_viewWidth = (float)((TbSlider)sender).Value;
            songScrollBar.SmallChange = songPanel.ViewWidthT / 100;
            songScrollBar.LargeChange = songPanel.ViewWidthT / 10;
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
				
				styleList.SelectedIndex = selectedTrackProps.NoteStyleIndex;
				lineStyleList.SelectedIndex = (int)selectedTrackProps.LineStyleProps.style;
				lineWidthUpDown.Value = selectedTrackProps.LineStyleProps.lineWidth;
				qnGapFillUd.Value = (decimal)selectedTrackProps.LineStyleProps.qn_gapThreshold;
				blurredEdgeUd.Value = (decimal)selectedTrackProps.LineStyleProps.blurredEdge;
				
				lineHlStyleList.SelectedIndex = (int)selectedTrackProps.LineStyleProps.hlStyle;
				hlSizeUpDown.Value = selectedTrackProps.LineStyleProps.hlSize;
				movingHlCb.Checked = selectedTrackProps.LineStyleProps.movingHl;
				shrinkingHlCb.Checked = selectedTrackProps.LineStyleProps.shrinkingHl;
				hlBorderCb.Checked = selectedTrackProps.LineStyleProps.hlBorder;
				
				fadeoutUd.Value = (decimal)(selectedTrackProps.LineStyleProps.fadeOut * 100);
				shapePowerUD.Value = (decimal)selectedTrackProps.LineStyleProps.shapePower;

				//Light
				globalLightCb.Checked = selectedTrackProps.UseGlobalLight;
				lightDirxTb.Text = selectedTrackProps.LightDir.X.ToString();
				lightDiryTb.Text = selectedTrackProps.LightDir.Y.ToString();
				lightDirzTb.Text = selectedTrackProps.LightDir.Z.ToString();
				specAmountUd.Value = (decimal)selectedTrackProps.SpecAmount;
				specPowUd.Value = (decimal)selectedTrackProps.SpecPower;
				specFovUd.Value = (decimal)selectedTrackProps.SpecFov;
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
			if (styleList.SelectedItem is NoteStyle_Line)
				lineStyleGroup.Visible = true;
			else
				lineStyleGroup.Visible = false;
			if (updatingControls)
				return;
			songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
			{
				Type type = (styleList.SelectedItem).GetType();
				songPanel.TrackProps[trackList.SelectedIndices[i]].NoteStyle = (NoteStyle)Activator.CreateInstance(type);
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
				writeFolderNames();
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
				writeFolderNames();
				
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
			vmSongFolder = Path.GetDirectoryName(openProjDialog.FileName);
			writeFolderNames();
			openSongFile(openProjDialog.FileName);
		}
		void openSongFile(string fileName)
		{
			try
			{
				BinaryFormatter bf = new BinaryFormatter();
				using (FileStream stream = File.Open(fileName, FileMode.Open))
				{
					toggleIdleRendering(false);
					Controls.Remove(songPanel);
					songPanel.Dispose();
					SongFormat.ReadVersion = (int)bf.Deserialize(stream);
					songPanel = (SongPanel)bf.Deserialize(stream);
					//songPanel.deserializeTrackProps(bf, stream);
				}
				initSongPanel(songPanel);

                if (songPanel.IsMod)
                {
                    importModForm.NoteFilePath = songPanel.NoteFilePath;
                    importModForm.AudioFilePath = songPanel.AudioFilePath;
                    importModForm.ModInsTrack = songPanel.ModInsTrack;
                }
                else
                {
                    importMidiForm.NoteFilePath = songPanel.NoteFilePath;
                    importMidiForm.AudioFilePath = songPanel.AudioFilePath;
                }
                currentSongPath = fileName;
				songLoaded(currentSongPath);
				updateFormTitle(currentSongPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
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
			if (isEmpty(currentSongPath))
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
				BinaryFormatter bf = new BinaryFormatter();
				using (FileStream stream = File.Open(currentSongPath, FileMode.Create))
				{
					bf.Serialize(stream, SongFormat.WriteVersion);
					bf.Serialize(stream, songPanel);
					//songPanel.serializeTrackProps(bf, stream);
				}
				updateFormTitle(currentSongPath);
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
			currentSongPath = saveProjDialog.FileName;
			vmSongFolder = Path.GetDirectoryName(currentSongPath);
			writeFolderNames();
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
			songPanel.MouseUp += new MouseEventHandler(panel1_MouseUp);
			songPanel.MouseDown += new MouseEventHandler(panel1_MouseDown);
			songPanel.MouseMove += new MouseEventHandler(panel1_MouseMove);
			songPanel.KeyDown += new KeyEventHandler(panel1_KeyDown);
		}

		private void lineWidthUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.lineWidth = (int)lineWidthUpDown.Value;
		}

		private void qnGapFillUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.qn_gapThreshold = (int)qnGapFillUd.Value;
		}
		private void fadeoutUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.fadeOut = (float)((NumericUpDown)sender).Value / 100.0f;
		}

		private void lineStyleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lineStyleList.SelectedIndex != (int)LineStyleEnum.Ribbon)
				simpleLineStylePanel.Visible = true;
			else
				simpleLineStylePanel.Visible = false;
			if (updatingControls)
				return;
			songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.style = (LineStyleEnum)lineStyleList.SelectedIndex;
		}

		private void blurredEdgeUd_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.blurredEdge = (int)blurredEdgeUd.Value;
		}
		private void invalidateSongPanel(object sender, EventArgs e)
		{
			songPanel.Invalidate();
            songScrollBar.Value = songPanel.SongPosT;
		}

		void toggleIdleRendering(bool b)
		{
			if (b)
			{
				Application.Idle -= eh_invalidateSongPanel;
				songPanel.updateTimeStamp();
				Application.Idle += eh_invalidateSongPanel;
			}
			else
			{
				songPanel.Invalidate();
				Application.Idle -= eh_invalidateSongPanel;
			}
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
			toggleIdleRendering(songPanel.IsPlaying);
		}

		private void resetPositionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			songPanel.stopPlayback();
			songPanel.Invalidate();
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

		private void lineHlStyleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			songPanel.Invalidate();
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.hlStyle = (LineHlStyleEnum)lineHlStyleList.SelectedIndex;
		}

		private void hlSizeUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.hlSize = (int)hlSizeUpDown.Value;
		}

		private void movingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.movingHl = ((CheckBox)sender).Checked;
		}

		private void shriunkingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.shrinkingHl = ((CheckBox)sender).Checked;
		}

		private void hlBorderCb_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.hlBorder = ((CheckBox)sender).Checked;
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
			songPanel.Notes.MaxPitch = (int)maxPitchUd.Value;
		}

		private void minPitchUd_ValueChanged(object sender, EventArgs e)
		{
			if ((int)minPitchUd.Value > songPanel.Notes.MaxPitch)
				minPitchUd.Value = songPanel.Notes.MaxPitch;
			songPanel.Notes.MinPitch = (int)minPitchUd.Value;
		}

		private void defaultPitchesBtn_Click(object sender, EventArgs e)
		{
			songPanel.Notes.MinPitch = -1000;
			maxPitchUd.Value = (decimal)(songPanel.Notes.MaxPitch = songPanel.Notes.DefaultMaxPitch);
			minPitchUd.Value = (decimal)(songPanel.Notes.MinPitch = songPanel.Notes.DefaultMinPitch);
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
				songPanel.TrackProps[trackList.SelectedIndices[i]].LineStyleProps.shapePower = (float)((NumericUpDown)sender).Value;
		}

		private void texKeepAspect_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingControls)
				return;
			for (int i = 0; i < trackList.SelectedIndices.Count; i++)
				getActiveTexProps(i).KeepAspect = ((CheckBox)sender).Checked;
		}

            }
    static class SongFormat
	{
		public const int WriteVersion = 1;
		public static int ReadVersion { get; set; }
	}
}