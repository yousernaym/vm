﻿namespace VisualMusic
{
    partial class ImportMidiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Size = new System.Drawing.Size(42, 13);
			this.label2.Text = "Midi file";
			// 
			// Cancel
			// 
			this.Cancel.Location = new System.Drawing.Point(308, 234);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(314, 53);
			// 
			// eraseCurrent
			// 
			this.eraseCurrent.Location = new System.Drawing.Point(12, 240);
			// 
			// Ok
			// 
			this.Ok.Location = new System.Drawing.Point(227, 234);
			this.Ok.Click += new System.EventHandler(this.Ok_Click);
			// 
			// insTrackBtn
			// 
			this.insTrackBtn.Location = new System.Drawing.Point(90, 217);
			this.insTrackBtn.Visible = false;
			// 
			// chTrackBtn
			// 
			this.chTrackBtn.Location = new System.Drawing.Point(90, 240);
			this.chTrackBtn.Visible = false;
			// 
			// openNoteFileDlg
			// 
			this.openNoteFileDlg.Filter = "Midi files (*.mid)|*.mid|All files (*.*)|*.*";
			this.openNoteFileDlg.Title = "Open midi file";
			// 
			// ImportMidiForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(400, 269);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ImportMidiForm";
			this.Load += new System.EventHandler(this.ImportMidiForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
    }
}
