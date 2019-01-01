namespace Visual_Music
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
			this.Cancel.Location = new System.Drawing.Point(308, 224);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(314, 53);
			// 
			// eraseCurrent
			// 
			this.eraseCurrent.Location = new System.Drawing.Point(12, 345);
			// 
			// openNoteFileDlg
			// 
			this.openNoteFileDlg.Filter = "Midi files (*.mid)|*.mid|All files (*.*)|*.*";
			this.openNoteFileDlg.Title = "Open midi file";
			// 
			// Ok
			// 
			this.Ok.Location = new System.Drawing.Point(227, 224);
			this.Ok.Click += new System.EventHandler(this.Ok_Click);
			// 
			// ImportMidiForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(400, 259);
			this.Name = "ImportMidiForm";
			this.Load += new System.EventHandler(this.ImportMidiForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
    }
}
