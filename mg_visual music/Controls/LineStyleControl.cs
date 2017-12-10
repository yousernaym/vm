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
		float SizeScale => SongPanel.Project.Camera.ViewportSize.X / 1000;
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
			if (noteStyle_Line.Style == null)
				lineStyleList.SelectedIndex = -1;
			else
				lineStyleList.SelectedIndex = (int)noteStyle_Line.Style;

			Form1.setNumericUdValue(lineWidthUd, noteStyle_Line.LineWidth / SizeScale);
			Form1.setNumericUdValue(qnGapFillUd, noteStyle_Line.Qn_gapThreshold);
			continuousCb.CheckState = Form1.toCheckState(noteStyle_Line.Continuous);
			if (noteStyle_Line.HlStyle == null)
				lineHlStyleList.SelectedIndex = -1;
			else
				lineHlStyleList.SelectedIndex = (int)noteStyle_Line.HlStyle;

			Form1.setNumericUdValue(hlSizeUpDown, noteStyle_Line.HlSize / SizeScale);
			movingHlCb.CheckState = Form1.toCheckState(noteStyle_Line.MovingHl);
			shrinkingHlCb.CheckState = Form1.toCheckState(noteStyle_Line.ShrinkingHl);
			hlBorderCb.CheckState = Form1.toCheckState(noteStyle_Line.HlBorder);
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
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().LineWidth = (float)lineWidthUd.Value * SizeScale;
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
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.getLineNoteStyle().HlSize = (float)hlSizeUpDown.Value * SizeScale;
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
