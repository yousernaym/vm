namespace Visual_Music
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
            this.modChTrackBtn = new System.Windows.Forms.RadioButton();
            this.modInsTrackBtn = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.Text = "Module file";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(227, 242);
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(308, 242);
            // 
            // label1
            // 
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.Text = "Audio file (optional)";
            // 
            // eraseCurrent
            // 
            this.eraseCurrent.Location = new System.Drawing.Point(12, 193);
            // 
            // openNoteFileDlg
            // 
            this.openNoteFileDlg.Filter = "Module files (*.xm; *.mod; *.it; *.s3m; *.stm) | *.xm; *.mod; *.it; *.s3m; *.stm " +
    "| All files (*.*)| *.*";
            this.openNoteFileDlg.Title = "Open module file";
            // 
            // modChTrackBtn
            // 
            this.modChTrackBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.modChTrackBtn.AutoSize = true;
            this.modChTrackBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.modChTrackBtn.Location = new System.Drawing.Point(12, 248);
            this.modChTrackBtn.Name = "modChTrackBtn";
            this.modChTrackBtn.Size = new System.Drawing.Size(168, 17);
            this.modChTrackBtn.TabIndex = 12;
            this.modChTrackBtn.Text = "One track per module channel";
            this.modChTrackBtn.UseVisualStyleBackColor = true;
            // 
            // modInsTrackBtn
            // 
            this.modInsTrackBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.modInsTrackBtn.AutoSize = true;
            this.modInsTrackBtn.Checked = true;
            this.modInsTrackBtn.Cursor = System.Windows.Forms.Cursors.Default;
            this.modInsTrackBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.modInsTrackBtn.Location = new System.Drawing.Point(12, 225);
            this.modInsTrackBtn.Name = "modInsTrackBtn";
            this.modInsTrackBtn.Size = new System.Drawing.Size(178, 17);
            this.modInsTrackBtn.TabIndex = 11;
            this.modInsTrackBtn.TabStop = true;
            this.modInsTrackBtn.Text = "One track per module instrument";
            this.modInsTrackBtn.UseVisualStyleBackColor = true;
            // 
            // ImportModForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(400, 277);
            this.Controls.Add(this.modInsTrackBtn);
            this.Controls.Add(this.modChTrackBtn);
            this.Name = "ImportModForm";
            this.Controls.SetChildIndex(this.BrowseAudioBtn, 0);
            this.Controls.SetChildIndex(this.audioFilePath, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.eraseCurrent, 0);
            this.Controls.SetChildIndex(this.Cancel, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.Ok, 0);
            this.Controls.SetChildIndex(this.modChTrackBtn, 0);
            this.Controls.SetChildIndex(this.modInsTrackBtn, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton modChTrackBtn;
        private System.Windows.Forms.RadioButton modInsTrackBtn;
    }
}
