namespace Visual_Music
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
			this.sidLink = new System.Windows.Forms.LinkLabel();
			this.importSidBtn = new System.Windows.Forms.Button();
			this.browseHvscBtn = new System.Windows.Forms.Button();
			this.hvscLink = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.modulesCb = new System.Windows.Forms.CheckBox();
			this.sidsCb = new System.Windows.Forms.CheckBox();
			this.songLengthCb = new System.Windows.Forms.CheckBox();
			this.openXmPlayDialog = new System.Windows.Forms.OpenFileDialog();
			this.hvscFolderBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.openXmPlaySidPluginDialog = new System.Windows.Forms.OpenFileDialog();
			this.hvscDirTb = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// importXmPlayBtn
			// 
			this.importXmPlayBtn.Location = new System.Drawing.Point(12, 12);
			this.importXmPlayBtn.Name = "importXmPlayBtn";
			this.importXmPlayBtn.Size = new System.Drawing.Size(164, 23);
			this.importXmPlayBtn.TabIndex = 1;
			this.importXmPlayBtn.Text = "Import XMPlay";
			this.importXmPlayBtn.UseVisualStyleBackColor = true;
			this.importXmPlayBtn.Click += new System.EventHandler(this.importXmPlayBtn_Click);
			// 
			// xmPlayLink
			// 
			this.xmPlayLink.AutoSize = true;
			this.xmPlayLink.Location = new System.Drawing.Point(182, 17);
			this.xmPlayLink.Name = "xmPlayLink";
			this.xmPlayLink.Size = new System.Drawing.Size(55, 13);
			this.xmPlayLink.TabIndex = 2;
			this.xmPlayLink.TabStop = true;
			this.xmPlayLink.Text = "Download";
			this.xmPlayLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.xmPlayLink_LinkClicked);
			// 
			// sidLink
			// 
			this.sidLink.AutoSize = true;
			this.sidLink.Location = new System.Drawing.Point(182, 46);
			this.sidLink.Name = "sidLink";
			this.sidLink.Size = new System.Drawing.Size(55, 13);
			this.sidLink.TabIndex = 4;
			this.sidLink.TabStop = true;
			this.sidLink.Text = "Download";
			this.sidLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.sidLink_LinkClicked);
			// 
			// importSidBtn
			// 
			this.importSidBtn.Location = new System.Drawing.Point(12, 41);
			this.importSidBtn.Name = "importSidBtn";
			this.importSidBtn.Size = new System.Drawing.Size(164, 23);
			this.importSidBtn.TabIndex = 3;
			this.importSidBtn.Text = "Import XMPlay SID plugin";
			this.importSidBtn.UseVisualStyleBackColor = true;
			this.importSidBtn.Click += new System.EventHandler(this.importSidBtn_Click);
			// 
			// browseHvscBtn
			// 
			this.browseHvscBtn.Location = new System.Drawing.Point(12, 118);
			this.browseHvscBtn.Name = "browseHvscBtn";
			this.browseHvscBtn.Size = new System.Drawing.Size(164, 23);
			this.browseHvscBtn.TabIndex = 7;
			this.browseHvscBtn.Text = "Browse for HVSC folder";
			this.browseHvscBtn.UseVisualStyleBackColor = true;
			this.browseHvscBtn.Click += new System.EventHandler(this.browseHvscBtn_Click);
			// 
			// hvscLink
			// 
			this.hvscLink.AutoSize = true;
			this.hvscLink.Location = new System.Drawing.Point(182, 123);
			this.hvscLink.Name = "hvscLink";
			this.hvscLink.Size = new System.Drawing.Size(55, 13);
			this.hvscLink.TabIndex = 8;
			this.hvscLink.TabStop = true;
			this.hvscLink.Text = "Download";
			this.hvscLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.hvscLink_LinkClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 79);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Use XMPlay audio for:";
			// 
			// modulesCb
			// 
			this.modulesCb.AutoSize = true;
			this.modulesCb.Enabled = false;
			this.modulesCb.Location = new System.Drawing.Point(130, 78);
			this.modulesCb.Name = "modulesCb";
			this.modulesCb.Size = new System.Drawing.Size(66, 17);
			this.modulesCb.TabIndex = 5;
			this.modulesCb.Text = "Modules";
			this.modulesCb.UseVisualStyleBackColor = true;
			this.modulesCb.EnabledChanged += new System.EventHandler(this.modulesCb_EnabledChanged);
			// 
			// sidsCb
			// 
			this.sidsCb.AutoSize = true;
			this.sidsCb.Enabled = false;
			this.sidsCb.Location = new System.Drawing.Point(202, 78);
			this.sidsCb.Name = "sidsCb";
			this.sidsCb.Size = new System.Drawing.Size(49, 17);
			this.sidsCb.TabIndex = 6;
			this.sidsCb.Text = "SIDs";
			this.sidsCb.UseVisualStyleBackColor = true;
			this.sidsCb.EnabledChanged += new System.EventHandler(this.sidsCb_EnabledChanged);
			// 
			// songLengthCb
			// 
			this.songLengthCb.AutoSize = true;
			this.songLengthCb.Enabled = false;
			this.songLengthCb.Location = new System.Drawing.Point(12, 173);
			this.songLengthCb.Name = "songLengthCb";
			this.songLengthCb.Size = new System.Drawing.Size(150, 17);
			this.songLengthCb.TabIndex = 9;
			this.songLengthCb.Text = "Use song length database";
			this.songLengthCb.UseVisualStyleBackColor = true;
			this.songLengthCb.EnabledChanged += new System.EventHandler(this.songLengthCb_EnabledChanged);
			// 
			// openXmPlayDialog
			// 
			this.openXmPlayDialog.Filter = "XMPlay (XMPlay*.zip)|xmplay*.zip|Zip files (*.zip)|*.zip";
			this.openXmPlayDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openXmPlayDialog_FileOk);
			// 
			// hvscFolderBrowseDialog
			// 
			this.hvscFolderBrowseDialog.Description = "Browse to C64Music\\DOCUMENTS";
			this.hvscFolderBrowseDialog.ShowNewFolderButton = false;
			// 
			// openXmPlaySidPluginDialog
			// 
			this.openXmPlaySidPluginDialog.Filter = "XMPlay SID plugin (xmp-sid.zip)|xmp-sid.zip|Zip files (*.zip)|*.zip";
			this.openXmPlaySidPluginDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openXmPlaySidPluginDialog_FileOk);
			// 
			// hvscDirTb
			// 
			this.hvscDirTb.Location = new System.Drawing.Point(12, 147);
			this.hvscDirTb.Name = "hvscDirTb";
			this.hvscDirTb.ReadOnly = true;
			this.hvscDirTb.Size = new System.Drawing.Size(234, 20);
			this.hvscDirTb.TabIndex = 10;
			this.hvscDirTb.TabStop = false;
			// 
			// TpartyIntegrationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(258, 202);
			this.Controls.Add(this.hvscDirTb);
			this.Controls.Add(this.songLengthCb);
			this.Controls.Add(this.sidsCb);
			this.Controls.Add(this.modulesCb);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.hvscLink);
			this.Controls.Add(this.sidLink);
			this.Controls.Add(this.xmPlayLink);
			this.Controls.Add(this.browseHvscBtn);
			this.Controls.Add(this.importSidBtn);
			this.Controls.Add(this.importXmPlayBtn);
			this.Name = "TpartyIntegrationForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Audio settings";
			this.Load += new System.EventHandler(this.TpartyIntegrationForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button importXmPlayBtn;
		private System.Windows.Forms.LinkLabel xmPlayLink;
		private System.Windows.Forms.LinkLabel sidLink;
		private System.Windows.Forms.Button importSidBtn;
		private System.Windows.Forms.Button browseHvscBtn;
		private System.Windows.Forms.LinkLabel hvscLink;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.OpenFileDialog openXmPlayDialog;
		private System.Windows.Forms.FolderBrowserDialog hvscFolderBrowseDialog;
		public System.Windows.Forms.CheckBox modulesCb;
		public System.Windows.Forms.CheckBox sidsCb;
		public System.Windows.Forms.CheckBox songLengthCb;
		private System.Windows.Forms.OpenFileDialog openXmPlaySidPluginDialog;
		private System.Windows.Forms.TextBox hvscDirTb;
	}
}