namespace Visual_Music
{
	partial class SourceFileForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SourceFileForm));
			this.browseNoteBtn = new System.Windows.Forms.Button();
			this.BrowseAudioBtn = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.noteFilePath = new System.Windows.Forms.TextBox();
			this.audioFilePath = new System.Windows.Forms.TextBox();
			this.Ok = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.openNoteFileDlg = new System.Windows.Forms.OpenFileDialog();
			this.openAudioFileDlg = new System.Windows.Forms.OpenFileDialog();
			this.eraseCurrent = new System.Windows.Forms.CheckBox();
			this.modInsTrackRbtn = new System.Windows.Forms.RadioButton();
			this.modChTrackRbtn = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// browseNoteBtn
			// 
			resources.ApplyResources(this.browseNoteBtn, "browseNoteBtn");
			this.browseNoteBtn.Name = "browseNoteBtn";
			this.browseNoteBtn.UseVisualStyleBackColor = true;
			this.browseNoteBtn.Click += new System.EventHandler(this.browseNoteBtn_Click);
			// 
			// BrowseAudioBtn
			// 
			resources.ApplyResources(this.BrowseAudioBtn, "BrowseAudioBtn");
			this.BrowseAudioBtn.Name = "BrowseAudioBtn";
			this.BrowseAudioBtn.UseVisualStyleBackColor = true;
			this.BrowseAudioBtn.Click += new System.EventHandler(this.BrowseAudioBtn_Click);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// noteFilePath
			// 
			resources.ApplyResources(this.noteFilePath, "noteFilePath");
			this.noteFilePath.Name = "noteFilePath";
			this.noteFilePath.TextChanged += new System.EventHandler(this.noteFilePath_TextChanged);
			// 
			// audioFilePath
			// 
			resources.ApplyResources(this.audioFilePath, "audioFilePath");
			this.audioFilePath.Name = "audioFilePath";
			this.audioFilePath.TextChanged += new System.EventHandler(this.audioFilePath_TextChanged);
			// 
			// Ok
			// 
			resources.ApplyResources(this.Ok, "Ok");
			this.Ok.Name = "Ok";
			this.Ok.UseVisualStyleBackColor = true;
			this.Ok.Click += new System.EventHandler(this.Ok_Click);
			// 
			// Cancel
			// 
			resources.ApplyResources(this.Cancel, "Cancel");
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Name = "Cancel";
			this.Cancel.UseVisualStyleBackColor = true;
			// 
			// openNoteFileDlg
			// 
			resources.ApplyResources(this.openNoteFileDlg, "openNoteFileDlg");
			this.openNoteFileDlg.FileOk += new System.ComponentModel.CancelEventHandler(this.openNoteFileDlg_FileOk);
			// 
			// openAudioFileDlg
			// 
			resources.ApplyResources(this.openAudioFileDlg, "openAudioFileDlg");
			this.openAudioFileDlg.FileOk += new System.ComponentModel.CancelEventHandler(this.openAudioFileDlg_FileOk);
			// 
			// eraseCurrent
			// 
			resources.ApplyResources(this.eraseCurrent, "eraseCurrent");
			this.eraseCurrent.Checked = true;
			this.eraseCurrent.CheckState = System.Windows.Forms.CheckState.Checked;
			this.eraseCurrent.Name = "eraseCurrent";
			this.eraseCurrent.UseVisualStyleBackColor = true;
			// 
			// modInsTrackRbtn
			// 
			resources.ApplyResources(this.modInsTrackRbtn, "modInsTrackRbtn");
			this.modInsTrackRbtn.Checked = true;
			this.modInsTrackRbtn.Cursor = System.Windows.Forms.Cursors.Default;
			this.modInsTrackRbtn.Name = "modInsTrackRbtn";
			this.modInsTrackRbtn.TabStop = true;
			this.modInsTrackRbtn.UseVisualStyleBackColor = true;
			// 
			// modChTrackRbtn
			// 
			resources.ApplyResources(this.modChTrackRbtn, "modChTrackRbtn");
			this.modChTrackRbtn.Name = "modChTrackRbtn";
			this.modChTrackRbtn.UseVisualStyleBackColor = true;
			// 
			// SourceFileForm
			// 
			this.AcceptButton = this.Ok;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.Controls.Add(this.modInsTrackRbtn);
			this.Controls.Add(this.modChTrackRbtn);
			this.Controls.Add(this.eraseCurrent);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Ok);
			this.Controls.Add(this.audioFilePath);
			this.Controls.Add(this.noteFilePath);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.BrowseAudioBtn);
			this.Controls.Add(this.browseNoteBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.Name = "SourceFileForm";
			this.Load += new System.EventHandler(this.SourceFileForm_Load);
			this.VisibleChanged += new System.EventHandler(this.SourceFileForm_VisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button browseNoteBtn;
		private System.Windows.Forms.Button BrowseAudioBtn;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox noteFilePath;
		private System.Windows.Forms.TextBox audioFilePath;
		private System.Windows.Forms.Button Ok;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.OpenFileDialog openNoteFileDlg;
		private System.Windows.Forms.OpenFileDialog openAudioFileDlg;
		private System.Windows.Forms.CheckBox eraseCurrent;
		private System.Windows.Forms.RadioButton modInsTrackRbtn;
		private System.Windows.Forms.RadioButton modChTrackRbtn;
	}
}