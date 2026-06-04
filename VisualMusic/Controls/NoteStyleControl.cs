using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class NoteStyleControl : UserControl
    {
        new protected Form1 ParentForm => (Form1)base.ParentForm;
        protected bool UpdatingControls => ParentForm == null ? true : ParentForm.UpdatingControls;
        protected ListView TrackList => ParentForm.TrackList;
        protected SongPanel SongPanel => Form1.SongPanel;
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

        public virtual void Update(NoteStyle noteStyle)
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
                Form1.SetNumericUdValue(xOriginUd, modEntry.Origin.X);
                Form1.SetNumericUdValue(yOriginUd, modEntry.Origin.Y);
                xOriginCb.CheckState = Form1.ToCheckState(modEntry.XOriginEnable);
                yOriginCb.CheckState = Form1.ToCheckState(modEntry.YOriginEnable);
                if (modEntry.CombineXY == null)
                    combineXYCombo.SelectedIndex = -1;
                else
                    combineXYCombo.SelectedIndex = (int)modEntry.CombineXY;
                UpdateXYOriginEnabled();
                squareAspectCb.CheckState = Form1.ToCheckState(modEntry.SquareAspect);
                colorDestCb.CheckState = Form1.ToCheckState(modEntry.ColorDestEnable);
                angleDestCb.CheckState = Form1.ToCheckState(modEntry.AngleDestEnable);
                colorDestBtn.BackColor = Form1.XnaToGdiCol(modEntry.ColorDest);
                Form1.SetNumericUdValue(angleDestUd, modEntry.AngleDest);
                Form1.SetNumericUdValue(startUd, modEntry.Start);
                Form1.SetNumericUdValue(stopUd, modEntry.Stop);
                Form1.SetNumericUdValue(fadeInUd, modEntry.FadeIn);
                Form1.SetNumericUdValue(fadeOutUd, modEntry.FadeOut);
                Form1.SetNumericUdValue(powerUd, modEntry.Power);
                discardStopCb.CheckState = Form1.ToCheckState(modEntry.DiscardAfterStop);
                invertCb.CheckState = Form1.ToCheckState(modEntry.Invert);
            }
        }

        private void BypassModEntryCb_CheckedChanged(object sender, EventArgs e)
        {
            //modEntryPanel.Enabled = !bypassModEntryCb.Checked;
        }

        private void NewMi_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.AddModEntry(true);
            ParentForm.UpdateTrackPropsControls();
        }

        private void DeleteMi_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.DeleteModEntry();
            ParentForm.UpdateTrackPropsControls();
        }

        private void CloneMi_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.CloneModEntry(true);
            ParentForm.UpdateTrackPropsControls();
        }

        private void ModEntryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ModEntryCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            bool itemSelected = modEntryCombo.SelectedIndex >= 0;
            cloneMi.Enabled = deleteMi.Enabled = ShowModEntryControls = itemSelected;

            if (UpdatingControls)
                return;
            //Debug.Assert(TrackList.SelectedIndices.Count == 1);  //modEntryCombo is disabled if more than one track is selected, and the entire track props panel is disabled if no track is selected, and if controls are updating, the method will return on the above line.
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntryIndex = modEntryCombo.SelectedIndex;
            ParentForm.UpdateTrackPropsControls();
        }

        private void XOriginUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.XOrigin = (float)xOriginUd.Value;
        }

        private void YOriginUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.YOrigin = (float)yOriginUd.Value;
        }

        private void XOriginCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            UpdateXYOriginEnabled();

            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.XOriginEnable = xOriginCb.Checked;
        }

        private void YOriginCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            UpdateXYOriginEnabled();

            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.YOriginEnable = yOriginCb.Checked;
        }

        private void UpdateXYOriginEnabled()
        {
            combineXYCombo.Enabled = xOriginCb.Checked && yOriginCb.Checked;
            xOriginUd.Enabled = xOriginCb.Checked;
            yOriginUd.Enabled = yOriginCb.Checked;
        }

        private void CombineXYCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.CombineXY = combineXYCombo.SelectedIndex;
        }

        private void SquareAspectCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.SquareAspect = squareAspectCb.Checked;
        }

        private void ColorDestCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.ColorDestEnable = colorDestCb.Checked;
        }

        private void ColorDestBtn_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.OK)
                return;
            colorDestBtn.BackColor = colorDialog1.Color;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.ColorDest = Form1.GdiToXnaCol(colorDestBtn.BackColor);
        }

        private void AngleDestCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.AngleDestEnable = angleDestCb.Checked;
        }

        private void AngleDestUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.AngleDest = (int)angleDestUd.Value;
        }

        private void StartUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.Start = (float)startUd.Value;
        }

        private void StopUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.Stop = (float)stopUd.Value;
        }

        private void FadeInUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.FadeIn = (float)fadeInUd.Value;
        }

        private void FadeOutUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.FadeOut = (float)fadeOutUd.Value;
        }

        private void PowerUd_ValueChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.Power = (float)powerUd.Value;
        }

        private void DiscardStopCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.DiscardAfterStop = discardStopCb.Checked;
        }

        private void InvertCb_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingControls)
                return;
            for (int i = 0; i < TrackList.SelectedIndices.Count; i++)
                TrackViews[TrackList.SelectedIndices[i]].TrackProps.ActiveNoteStyle.SelectedModEntry.Invert = invertCb.Checked;
        }
    }
}
