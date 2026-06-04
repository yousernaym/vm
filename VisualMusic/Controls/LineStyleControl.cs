using System;
using System.Windows.Forms;

namespace VisualMusic
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

        public override void Update(NoteStyle noteStyle)
        {
            base.Update(noteStyle);
            NoteStyle_Line noteStyle_Line = (NoteStyle_Line)noteStyle;
            if (noteStyle_Line.LineType == null)
                lineTypeList.SelectedIndex = -1;
            else
                lineTypeList.SelectedIndex = (int)noteStyle_Line.LineType;

            Form1.SetNumericUdValue(lineWidthUd, noteStyle_Line.LineWidth);
            Form1.SetNumericUdValue(qnGapFillUd, noteStyle_Line.Qn_gapThreshold);
            continuousCb.CheckState = Form1.ToCheckState(noteStyle_Line.Continuous);
            if (noteStyle_Line.HlType == null)
                lineHlTypeList.SelectedIndex = -1;
            else
                lineHlTypeList.SelectedIndex = (int)noteStyle_Line.HlType;

            Form1.SetNumericUdValue(hlSizeUpDown, noteStyle_Line.HlSize);
            Form1.SetNumericUdValue(hlMovementPowUd, noteStyle_Line.HlMovementPow);
            movingHlCb.CheckState = Form1.ToCheckState(noteStyle_Line.MovingHl);
            shrinkingHlCb.CheckState = Form1.ToCheckState(noteStyle_Line.ShrinkingHl);
            hlBorderCb.CheckState = Form1.ToCheckState(noteStyle_Line.HlBorder);
        }

        private void LineTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            //SongPanel.Invalidate();
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().LineType = (LineType)lineTypeList.SelectedIndex;
            SongPanel.Project.CreateGeos();
        }

        private void LineWidthUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().LineWidth = (float)lineWidthUd.Value;
            SongPanel.Project.CreateGeos();
        }

        private void QnGapFillUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().Qn_gapThreshold = (int)qnGapFillUd.Value;
            SongPanel.Project.CreateGeos();
        }

        private void LineHlTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            //SongPanel.Invalidate();
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().HlType = (LineHlType)lineHlTypeList.SelectedIndex;
        }

        private void HlSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().HlSize = (float)hlSizeUpDown.Value;
        }

        private void HlMovementPowUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().HlMovementPow = (float)hlMovementPowUd.Value;
        }

        private void MovingHlCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().MovingHl = ((CheckBox)sender).Checked;
        }

        private void ShrinkingHlCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().ShrinkingHl = ((CheckBox)sender).Checked;
        }

        private void HlBorderCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().HlBorder = ((CheckBox)sender).Checked;
        }

        private void LineStyleControl_Load(object sender, EventArgs e)
        {

        }

        private void ContinuousCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.StyleProps.GetLineStyle().Continuous = ((CheckBox)sender).Checked;
        }
    }
}
