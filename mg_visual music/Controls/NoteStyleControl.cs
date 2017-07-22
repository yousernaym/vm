using System;
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
		protected List<TrackProps> TrackProps => SongPanel.TrackProps;
		//protected TrackProps SelectedTrackProps => SongPanel.TrackProps_MergedSelection;
		//NoteStyle SelectedNoteStyle => TrackProps_MergedSelection.SelectedNoteStyle;

		bool ShowModEntryControls
		{
			set
			{
				bypassModEntryPanel.Visible = value;
				modGbox.AutoSize = value;
			}
		}

		public NoteStyleControl()
		{
			InitializeComponent();
			modGbox.AutoSize = false;
			modGbox.Height = modEntryCombo.Height * 3;
		}

		public virtual void update(NoteStyle noteStyle)
		{
			//modEntryBs.DataSource = noteStyle.ModEntries;
			modEntryCombo.DisplayMember = "Name";
			modEntryCombo.SelectedIndex = -1;
			modEntryCombo.Items.Clear();
			//modEntryCombo.Text = "";
			if (noteStyle.ModEntries.Count > 0)
				modEntryCombo.Items.AddRange(noteStyle.ModEntries.ToArray());
			modEntryCombo.SelectedIndex = noteStyle.SelectedModEntryIndex;
			NoteStyleMod modEntry = noteStyle.SelectedModEntry;
			if (modEntry != null)
			{
				xSourceCombo.SelectedIndex = modEntry.XSource;
				ySourceCombo.SelectedIndex = modEntry.YSource;
				combineXYCombo.SelectedIndex = modEntry.CombineXY;
				colorDestCb.Checked = modEntry.ColorDestEnable;
				angleDestCb.Checked = modEntry.AngleDestEnable;
				colorDestBtn.BackColor = modEntry.SystemColorDest;
				angleDestUd.Value = (decimal)modEntry.AngleDest;
				startUd.Value = (decimal)modEntry.Start;
				stopUd.Value = (decimal)modEntry.Stop;
				fadeInUd.Value = (decimal)modEntry.FadeIn;
				fadeOutUd.Value = (decimal)modEntry.FadeOut;
				powerUd.Value = (decimal)modEntry.Power;
				scaleUd.Value = (decimal)modEntry.Scale;
			}
		}

		private void bypassModEntryCb_CheckedChanged(object sender, EventArgs e)
		{
			//modEntryPanel.Enabled = !bypassModEntryCb.Checked;
		}

		private void newMi_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.addModEntry(true);
			ParentForm.updateTrackControls();
		}

		private void deleteMi_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.deleteModEntry();
			ParentForm.updateTrackControls();
		}

		private void cloneMi_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.cloneModEntry(true);
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
			Debug.Assert(TrackList.SelectedIndices.Count == 1);  //modEntryCombo is disabled if more than one track is selected, and the entire track props panel is disabled if no track is selected, and if controls are updating the method will return on the above line.
			TrackProps[TrackList.SelectedIndices[0]].SelectedNoteStyle.SelectedModEntryIndex = modEntryCombo.SelectedIndex;
			ParentForm.updateTrackControls();
		}
		private void xSourceCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.XSource = xSourceCombo.SelectedIndex;
		}

		private void ySourceCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.YSource = ySourceCombo.SelectedIndex;
		}

		private void combineCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.CombineXY = combineXYCombo.SelectedIndex;
		}

		private void colorDestCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.ColorDestEnable = colorDestCb.Checked;
		}

		private void colorDestBtn_Click(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.SystemColorDest = colorDestBtn.BackColor;
		}

		private void angleDestCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.AngleDestEnable = angleDestCb.Checked;
		}

		private void angleDestUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.AngleDest = (int)angleDestUd.Value;
		}

		private void startUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.Start = (float)startUd.Value;
		}

		private void stopUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.Stop = (float)stopUd.Value;
		}

		private void fadeInUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.FadeIn = (float)fadeInUd.Value;
		}

		private void fadeOutUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.FadeOut = (float)fadeOutUd.Value;
		}

		private void powerUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.Power = (float)powerUd.Value;
		}

		private void scaleUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackProps[TrackList.SelectedIndices[i]].SelectedNoteStyle.SelectedModEntry.Scale = (float)scaleUd.Value;
		}
	}
}
