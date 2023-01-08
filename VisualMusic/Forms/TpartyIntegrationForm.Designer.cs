namespace VisualMusic
{
	partial class TpartyIntegrationForm
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
            this.importXmPlayBtn = new System.Windows.Forms.Button();
            this.xmPlayLink = new System.Windows.Forms.LinkLabel();
            this.modulesCb = new System.Windows.Forms.CheckBox();
            this.openXmPlayDialog = new System.Windows.Forms.OpenFileDialog();
            this.openSidPlayDialog = new System.Windows.Forms.OpenFileDialog();
            this.songLengthsUrlTb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.updateSongLengthsBtn = new System.Windows.Forms.Button();
            this.defaultSongLengthsBtn = new System.Windows.Forms.Button();
            this.lastUpdatedLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // importXmPlayBtn
            // 
            this.importXmPlayBtn.Location = new System.Drawing.Point(14, 14);
            this.importXmPlayBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importXmPlayBtn.Name = "importXmPlayBtn";
            this.importXmPlayBtn.Size = new System.Drawing.Size(110, 27);
            this.importXmPlayBtn.TabIndex = 1;
            this.importXmPlayBtn.Text = "Import XMPlay";
            this.importXmPlayBtn.UseVisualStyleBackColor = true;
            this.importXmPlayBtn.Click += new System.EventHandler(this.importXmPlayBtn_Click);
            // 
            // xmPlayLink
            // 
            this.xmPlayLink.AutoSize = true;
            this.xmPlayLink.Location = new System.Drawing.Point(131, 20);
            this.xmPlayLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.xmPlayLink.Name = "xmPlayLink";
            this.xmPlayLink.Size = new System.Drawing.Size(61, 15);
            this.xmPlayLink.TabIndex = 2;
            this.xmPlayLink.TabStop = true;
            this.xmPlayLink.Text = "Download";
            this.xmPlayLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.xmPlayLink_LinkClicked);
            // 
            // modulesCb
            // 
            this.modulesCb.AutoSize = true;
            this.modulesCb.Enabled = false;
            this.modulesCb.Location = new System.Drawing.Point(14, 47);
            this.modulesCb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.modulesCb.Name = "modulesCb";
            this.modulesCb.Size = new System.Drawing.Size(207, 19);
            this.modulesCb.TabIndex = 3;
            this.modulesCb.Text = "Use XMPlay for supported formats";
            this.modulesCb.UseVisualStyleBackColor = true;
            this.modulesCb.EnabledChanged += new System.EventHandler(this.modulesCb_EnabledChanged);
            // 
            // openXmPlayDialog
            // 
            this.openXmPlayDialog.Filter = "XMPlay (XMPlay*.zip)|xmplay*.zip|Zip files (*.zip)|*.zip|All files (*.*)|*.*";
            this.openXmPlayDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openXmPlayDialog_FileOk);
            // 
            // openSidPlayDialog
            // 
            this.openSidPlayDialog.Filter = "Sidplayfp (sidplayfp*.zip)|sidplayfp*.zip|Zip files (*.zip)|*.zip|All files (*.*)" +
    "|*.*";
            this.openSidPlayDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openXmPlaySidPluginDialog_FileOk);
            // 
            // songLengthsUrlTb
            // 
            this.songLengthsUrlTb.Location = new System.Drawing.Point(14, 119);
            this.songLengthsUrlTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.songLengthsUrlTb.Name = "songLengthsUrlTb";
            this.songLengthsUrlTb.Size = new System.Drawing.Size(483, 23);
            this.songLengthsUrlTb.TabIndex = 10;
            this.songLengthsUrlTb.TabStop = false;
            this.songLengthsUrlTb.Text = "https://www.hvsc.c64.org/download/C64Music/DOCUMENTS/Songlengths.md5";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 100);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "HVSC song lenghts URL";
            // 
            // updateSongLengthsBtn
            // 
            this.updateSongLengthsBtn.Location = new System.Drawing.Point(14, 148);
            this.updateSongLengthsBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.updateSongLengthsBtn.Name = "updateSongLengthsBtn";
            this.updateSongLengthsBtn.Size = new System.Drawing.Size(66, 27);
            this.updateSongLengthsBtn.TabIndex = 11;
            this.updateSongLengthsBtn.Text = "&Update";
            this.updateSongLengthsBtn.UseVisualStyleBackColor = true;
            this.updateSongLengthsBtn.Click += new System.EventHandler(this.updateSongLengthsBtn_Click);
            // 
            // defaultSongLengthsBtn
            // 
            this.defaultSongLengthsBtn.Location = new System.Drawing.Point(409, 148);
            this.defaultSongLengthsBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.defaultSongLengthsBtn.Name = "defaultSongLengthsBtn";
            this.defaultSongLengthsBtn.Size = new System.Drawing.Size(88, 27);
            this.defaultSongLengthsBtn.TabIndex = 12;
            this.defaultSongLengthsBtn.Text = "&Default URL";
            this.defaultSongLengthsBtn.UseVisualStyleBackColor = true;
            this.defaultSongLengthsBtn.Click += new System.EventHandler(this.defaultSongLengthsBtn_Click);
            // 
            // lastUpdatedLabel
            // 
            this.lastUpdatedLabel.AutoSize = true;
            this.lastUpdatedLabel.Location = new System.Drawing.Point(88, 154);
            this.lastUpdatedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lastUpdatedLabel.Name = "lastUpdatedLabel";
            this.lastUpdatedLabel.Size = new System.Drawing.Size(78, 15);
            this.lastUpdatedLabel.TabIndex = 13;
            this.lastUpdatedLabel.Text = "Last updated:";
            // 
            // TpartyIntegrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 189);
            this.Controls.Add(this.lastUpdatedLabel);
            this.Controls.Add(this.defaultSongLengthsBtn);
            this.Controls.Add(this.updateSongLengthsBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.songLengthsUrlTb);
            this.Controls.Add(this.modulesCb);
            this.Controls.Add(this.xmPlayLink);
            this.Controls.Add(this.importXmPlayBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "TpartyIntegrationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Third-party integration";
            this.Load += new System.EventHandler(this.TpartyIntegrationForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button importXmPlayBtn;
		private System.Windows.Forms.LinkLabel xmPlayLink;
		private System.Windows.Forms.OpenFileDialog openXmPlayDialog;
		public System.Windows.Forms.CheckBox modulesCb;
		private System.Windows.Forms.OpenFileDialog openSidPlayDialog;
		private System.Windows.Forms.TextBox songLengthsUrlTb;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button updateSongLengthsBtn;
		private System.Windows.Forms.Button defaultSongLengthsBtn;
        private System.Windows.Forms.Label lastUpdatedLabel;
    }
}