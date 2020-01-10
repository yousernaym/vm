namespace VisualMusic
{
    partial class ImportModForm
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
			// existingAudioRbtn
			// 
			this.existingAudioRbtn.Size = new System.Drawing.Size(215, 17);
			this.existingAudioRbtn.Text = "Audio file (leave empty for module audio)";
			// 
			// label2
			// 
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.Text = "Module file";
			// 
			// Cancel
			// 
			this.Cancel.Location = new System.Drawing.Point(308, 267);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(292, 63);
			this.label1.Size = new System.Drawing.Size(96, 13);
			this.label1.Text = "Audio file (optional)";
			// 
			// eraseCurrent
			// 
			this.eraseCurrent.Location = new System.Drawing.Point(12, 227);
			// 
			// openNoteFileDlg
			// 
			this.openNoteFileDlg.Filter = "Module files (*.xm; *.mod; *.it; *.s3m; *.stm) | *.xm; *.mod; *.it; *.s3m; *.stm " +
    "| All files (*.*)| *.*";
			this.openNoteFileDlg.Title = "Open module file";
			// 
			// Ok
			// 
			this.Ok.Location = new System.Drawing.Point(227, 267);
			this.Ok.Click += new System.EventHandler(this.Ok_Click);
			// 
			// insTrackBtn
			// 
			this.insTrackBtn.Location = new System.Drawing.Point(12, 250);
			// 
			// chTrackBtn
			// 
			this.chTrackBtn.Location = new System.Drawing.Point(12, 273);
			// 
			// ImportModForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(400, 302);
			this.Name = "ImportModForm";
			this.Load += new System.EventHandler(this.ImportModForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
    }
}
