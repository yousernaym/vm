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
		//float SizeScale => SongPanel.Project.Camera.ViewportSize.X / 1000;
		public LineStyleControl()
		{
			InitializeComponent();
			Array enumArray = Enum.GetValues(typeof(LineType));
			foreach (LineType lse in enumArray)
				lineTypeList.Items.Add(lse.ToString());
			enumArray = Enum.GetValues(typeof(LineHlType));
			foreach (LineHlType lse in enumArray)
				lineHlTypeList.Items.Add(lse.ToString());
		}

		public override void update(NoteStyle noteStyle)
		{
			base.update(noteStyle);
			NoteStyle_Line noteStyle_Line = (NoteStyle_Line)noteStyle;
			if (noteStyle_Line.LineType == null)
				lineTypeList.SelectedIndex = -1;
			else
				lineTypeList.SelectedIndex = (int)noteStyle_Line.LineType;

			Form1.setNumericUdValue(lineWidthUd, noteStyle_Line.LineWidth);
			Form1.setNumericUdValue(qnGapFillUd, noteStyle_Line.Qn_gapThreshold);
			continuousCb.CheckState = Form1.toCheckState(noteStyle_Line.Continuous);
			if (noteStyle_Line.HlType == null)
				lineHlTypeList.SelectedIndex = -1;
			else
				lineHlTypeList.SelectedIndex = (int)noteStyle_Line.HlType;

			Form1.setNumericUdValue(hlSizeUpDown, noteStyle_Line.HlSize);
			movingHlCb.CheckState = Form1.toCheckState(noteStyle_Line.MovingHl);
			shrinkingHlCb.CheckState = Form1.toCheckState(noteStyle_Line.ShrinkingHl);
			hlBorderCb.CheckState = Form1.toCheckState(noteStyle_Line.HlBorder);
		}

		
		
		private void lineTypeList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			//SongPanel.Invalidate();
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().LineType = (LineType)lineTypeList.SelectedIndex;
			SongPanel.Project.createOcTrees();
		}

		private void lineWidthUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().LineWidth = (float)lineWidthUd.Value;
			SongPanel.Project.createOcTrees();
		}

		private void qnGapFillUd_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().Qn_gapThreshold = (int)qnGapFillUd.Value;
			SongPanel.Project.createOcTrees();
		}

		private void lineHlTypeList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			//SongPanel.Invalidate();
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().HlType= (LineHlType)lineHlTypeList.SelectedIndex;
		}

		private void hlSizeUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().HlSize = (float)hlSizeUpDown.Value;
		}

		private void movingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().MovingHl = ((CheckBox)sender).Checked;
		}

		private void shrinkingHlCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().ShrinkingHl = ((CheckBox)sender).Checked;
		}

		private void hlBorderCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().HlBorder = ((CheckBox)sender).Checked;
		}

		private void LineStyleControl_Load(object sender, EventArgs e)
		{

		}

		private void continuousCb_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingControls)
				return;
			for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
				TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.getLineStyle().Continuous = ((CheckBox)sender).Checked;
		}
	}
}
