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
		protected List<TrackProps> TrackProps => ParentForm.SongPanel.TrackProps;

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
			//modGbox.ContextMenuStrip = modEntryCm;
		}

		//public void init(Form1 parent)
		//{
		//	parentForm = parent;
		//}

		public void update(NoteStyle noteStyle)
		{
			modEntryBs.DataSource = noteStyle.ModEntries;
			modEntryCombo.DisplayMember = "Name";
			//modEntryCombo.ValueMember = "Nameh";
			//modEntryCombo.SelectedIndex = -1;
			modEntryCombo.SelectedIndex = noteStyle.SelectedModEntryIndex;
			NoteStyleMod modEntry = noteStyle.SelectedModEntry;
			if (modEntry != null)
			{
				xSourceCombo.SelectedIndex = modEntry.XSource;
				ySourceCombo.SelectedIndex = modEntry.YSource;
				combineCombo.SelectedIndex = modEntry.CombineXY;
				colorDestCb.Checked = modEntry.ColorDestEnable;
				colordest

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

		private void NoteStyleControl_Load(object sender, EventArgs e)
		{

		}

}
}
