﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Visual_Music
{
	public partial class NoteStyleControl : UserControl
	{
		new protected Form1 ParentForm => (Form1)base.ParentForm;
		protected bool UpdatingControls => ParentForm == null ? true : ParentForm.UpdatingControls;
		protected ListViewNF TrackList => ParentForm.TrackList;
		protected SongPanel SongPanel => ParentForm.SongPanel;
		protected List<TrackView> TrackViews => ParentForm.Project.TrackViews;
		//protected TrackProps SelectedTrackProps => SongPanel.TrackProps_MergedSelection;
		//NoteStyle SelectedNoteStyle => TrackProps_MergedSelection.SelectedNoteStyle;

		bool ShowModEntryControls
		{
			set
			{
				modGbox.AutoSize = modEntryPanel.Visible = value;
			}
		}

		public NoteStyleControl()
		{
			InitializeComponent();
			modGbox.AutoSize = false;
			modGbox.Height = (int)((float)modEntryCombo.Height * 2.5f);
		}

		public virtual void update(NoteStyle noteStyle)
		{
			//modEntryBs.DataSource = noteStyle.ModEntries;
			modEntryCombo.DisplayMember = "Name";
			modEntryCombo.SelectedIndex = -1;
			modEntryCombo.Items.Clear();
			//modEntryCombo.Text = "";
			if (noteStyle.ModEntries == null)
			{
				modEntryCombo.Enabled = false;
				modEntryCombo.SelectedIndex = -1;
			}
			else
			{
				if (noteStyle.ModEntries.Count > 0)
					modEntryCombo.Items.AddRange(noteStyle.ModEntries.ToArray());
				if (noteStyle.SelectedModEntryIndex == null)
					modEntryCombo.SelectedIndex = -1;
				else
					modEntryCombo.SelectedIndex = (int)noteStyle.SelectedModEntryIndex;
				modEntryCombo.Enabled = true;
			}
			
			NoteStyleMod modEntry = noteStyle.SelectedModEntry;
			if (modEntry != null)
			{
				Form1.setNumericUdValue(xOriginUd, modEntry.Origin.X);
				Form1.setNumericUdValue(yOriginUd, modEntry.Origin.Y);
				xOriginCb.CheckState = Form1.toCheckState(modEntry.XOriginEnable);
				yOriginCb.CheckState = Form1.toCheckState(modEntry.YOriginEnable);
				if (modEntry.CombineXY == null)
					combineXYCombo.SelectedIndex = -1;
				else
					combineXYCombo.SelectedIndex = (int)modEntry.CombineXY;
				colorDestCb.CheckState = Form1.toCheckState(modEntry.ColorDestEnable);
				angleDestCb.CheckState = Form1.toCheckState(modEntry.AngleDestEnable);
				colorDestBtn.BackColor = modEntry.SystemColorDest;
				Form1.setNumericUdValue(angleDestUd, modEntry.AngleDest);
				Form1.setNumericUdValue(startUd, modEntry.Start);
				Form1.setNumericUdValue(stopUd, modEntry.Stop);
				Form1.setNumericUdValue(fadeInUd, modEntry.FadeIn);
				Form1.setNumericUdValue(fadeOutUd, modEntry.FadeOut);
				Form1.setNumericUdValue(powerUd, modEntry.Power);
				discardStopCb.CheckState = Form1.toCheckState(modEntry.DiscardAfterStop);
				invertCb.CheckState = Form1.toCheckState(modEntry.Invert);
			}
		}

		private void bypassModEntryCb_CheckedChanged(object sender, EventArgs e)
		{
			//modEntryPanel.Enabled = !bypassModEntryCb.Checked;
		}

		private void newMi_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.addModEntry(true);
			ParentForm.updateTrackControls();
		}

		private void deleteMi_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.deleteModEntry();
			ParentForm.updateTrackControls();
		}

		private void cloneMi_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.cloneModEntry(true);
			ParentForm.updateTrackControls();
		}

		private void modEntryCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
	
		}

		private void modEntryCombo_SelectedValueChanged(object sender, EventArgs e)
		{
			bool itemSelected = modEntryCombo.SelectedIndex >= 0;
			cloneMi.Enabled = deleteMi.Enabled = ShowModEntryControls = itemSelected;

			if (UpdatingControls)
				return;
			//Debug.Assert(TrackList.SelectedIndices.Count == 1);  //modEntryCombo is disabled if more than one track is selected, and the entire track props panel is disabled if no track is selected, and if controls are updating, the method will return on the above line.
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntryIndex = modEntryCombo.SelectedIndex;
			ParentForm.updateTrackControls();
		}

		private void xOriginUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.XOrigin = (float)xOriginUd.Value;
		}

		private void yOriginUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.YOrigin = (float)yOriginUd.Value;
		}

		private void xOriginCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			combineXYCombo.Enabled = xOriginCb.Checked || yOriginCb.Checked;
			xOriginUd.Enabled = xOriginCb.Checked;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.XOriginEnable = xOriginCb.Checked;
		}

		private void yOriginCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			combineXYCombo.Enabled = xOriginCb.Checked || yOriginCb.Checked;
			yOriginUd.Enabled = yOriginCb.Checked;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.YOriginEnable = yOriginCb.Checked;
		}

		private void combineXYCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.CombineXY = combineXYCombo.SelectedIndex;
		}

		private void colorDestCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.ColorDestEnable = colorDestCb.Checked;
		}

		private void colorDestBtn_Click(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			colorDialog1.ShowDialog();
			colorDestBtn.BackColor = colorDialog1.Color;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.SystemColorDest = colorDestBtn.BackColor;
		}

		private void angleDestCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.AngleDestEnable = angleDestCb.Checked;
		}

		private void angleDestUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.AngleDest = (int)angleDestUd.Value;
		}

		private void startUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.Start = (float)startUd.Value;
		}

		private void stopUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.Stop = (float)stopUd.Value;
		}

		private void fadeInUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.FadeIn = (float)fadeInUd.Value;
		}

		private void fadeOutUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.FadeOut = (float)fadeOutUd.Value;
		}

		private void powerUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.Power = (float)powerUd.Value;
		}

		private void discardStopCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.DiscardAfterStop = discardStopCb.Checked;
		}
		
		private void invertCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.SelectedNoteStyle.SelectedModEntry.Invert = invertCb.Checked;
		}
	}
}
