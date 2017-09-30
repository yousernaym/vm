using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class LineStyleControl : NoteStyleControl
	{
		public LineStyleControl()
		{
			InitializeComponent();
			Array enumArray = Enum.GetValues(typeof(LineStyleEnum));
			foreach (LineStyleEnum lse in enumArray)
				lineStyleList.Items.Add(lse.ToString());
			enumArray = Enum.GetValues(typeof(LineHlStyleEnum));
			foreach (LineHlStyleEnum lse in enumArray)
				lineHlStyleList.Items.Add(lse.ToString());
		}

		public override void update(NoteStyle noteStyle)
		{
			base.update(noteStyle);
			NoteStyle_Line noteStyle_Line = (NoteStyle_Line)noteStyle;
			lineStyleList.SelectedIndex = (int)noteStyle_Line.Style;
			lineWidthUd.Value = noteStyle_Line.LineWidth;
			qnGapFillUd.Value = (decimal)noteStyle_Line.Qn_gapThreshold;
			continuousCb.Checked = noteStyle_Line.Continuous;
			lineHlStyleList.SelectedIndex = (int)noteStyle_Line.HlStyle;
			hlSizeUpDown.Value = noteStyle_Line.HlSize;
			movingHlCb.Checked = noteStyle_Line.MovingHl;
			shrinkingHlCb.Checked = noteStyle_Line.ShrinkingHl;
			hlBorderCb.Checked = noteStyle_Line.HlBorder;
		}

		
		
		private void lineStyleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			//SongPanel.Invalidate();
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().Style = (LineStyleEnum)lineStyleList.SelectedIndex;
		}

		private void lineWidthUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().LineWidth = (int)lineWidthUd.Value;
			SongPanel.Project.createOcTrees();
		}

		private void qnGapFillUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().Qn_gapThreshold = (int)qnGapFillUd.Value;
			SongPanel.Project.createOcTrees();
		}

		private void lineHlStyleList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			//SongPanel.Invalidate();
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().HlStyle = (LineHlStyleEnum)lineHlStyleList.SelectedIndex;
		}

		private void hlSizeUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().HlSize = (int)hlSizeUpDown.Value;
		}

		private void movingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().MovingHl = ((CheckBox)sender).Checked;
		}

		private void shrinkingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().ShrinkingHl = ((CheckBox)sender).Checked;
		}

		private void hlBorderCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().HlBorder = ((CheckBox)sender).Checked;
		}

		private void LineStyleControl_Load(object sender, EventArgs e)
		{

		}

		private void continuousCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().Continuous = ((CheckBox)sender).Checked;
		}
	}
}
