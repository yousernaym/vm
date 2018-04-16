namespace Visual_Music
{
    partial class ImportSidForm
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
			this.label2.Size = new System.Drawing.Size(38, 13);
			this.label2.Text = "Sid file";
			// 
			// Cancel
			// 
			this.Cancel.Location = new System.Drawing.Point(324, 228);
			// 
			// eraseCurrent
			// 
			this.eraseCurrent.Location = new System.Drawing.Point(12, 234);
			// 
			// openNoteFileDlg
			// 
			this.openNoteFileDlg.Filter = "Sid files (*.sid; *.psid; *.dat; *.rsid; *.mus)|*.sid; *.psid; *.dat; *.rsid; *.m" +
    "us|All files|*.*";
			// 
			// Ok
			// 
			this.Ok.Location = new System.Drawing.Point(243, 228);
			this.Ok.Click += new System.EventHandler(this.Ok_Click);
			// 
			// ImportSidForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(411, 263);
			this.Name = "ImportSidForm";
			this.Load += new System.EventHandler(this.ImportSidForm_Load);
			this.Shown += new System.EventHandler(this.ImportSidForm_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
    }
}
