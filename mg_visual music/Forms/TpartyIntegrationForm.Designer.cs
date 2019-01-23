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
			this.browseHvscBtn = new System.Windows.Forms.Button();
			this.hvscLink = new System.Windows.Forms.LinkLabel();
			this.modulesCb = new System.Windows.Forms.CheckBox();
			this.openXmPlayDialog = new System.Windows.Forms.OpenFileDialog();
			this.openSidPlayDialog = new System.Windows.Forms.OpenFileDialog();
			this.hvscDirTb = new System.Windows.Forms.TextBox();
			this.songLengthsCb = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// importXmPlayBtn
			// 
			this.importXmPlayBtn.Location = new System.Drawing.Point(12, 12);
			this.importXmPlayBtn.Name = "importXmPlayBtn";
			this.importXmPlayBtn.Size = new System.Drawing.Size(121, 23);
			this.importXmPlayBtn.TabIndex = 1;
			this.importXmPlayBtn.Text = "Import XMPlay*.zip";
			this.importXmPlayBtn.UseVisualStyleBackColor = true;
			this.importXmPlayBtn.Click += new System.EventHandler(this.importXmPlayBtn_Click);
			// 
			// xmPlayLink
			// 
			this.xmPlayLink.AutoSize = true;
			this.xmPlayLink.Location = new System.Drawing.Point(139, 17);
			this.xmPlayLink.Name = "xmPlayLink";
			this.xmPlayLink.Size = new System.Drawing.Size(55, 13);
			this.xmPlayLink.TabIndex = 2;
			this.xmPlayLink.TabStop = true;
			this.xmPlayLink.Text = "Download";
			this.xmPlayLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.xmPlayLink_LinkClicked);
			// 
			// browseHvscBtn
			// 
			this.browseHvscBtn.Location = new System.Drawing.Point(12, 64);
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
			this.hvscLink.Location = new System.Drawing.Point(182, 69);
			this.hvscLink.Name = "hvscLink";
			this.hvscLink.Size = new System.Drawing.Size(55, 13);
			this.hvscLink.TabIndex = 8;
			this.hvscLink.TabStop = true;
			this.hvscLink.Text = "Download";
			this.hvscLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.hvscLink_LinkClicked);
			// 
			// modulesCb
			// 
			this.modulesCb.AutoSize = true;
			this.modulesCb.Enabled = false;
			this.modulesCb.Location = new System.Drawing.Point(12, 41);
			this.modulesCb.Name = "modulesCb";
			this.modulesCb.Size = new System.Drawing.Size(165, 17);
			this.modulesCb.TabIndex = 5;
			this.modulesCb.Text = "Use XMPlay for module audio";
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
			// hvscDirTb
			// 
			this.hvscDirTb.Location = new System.Drawing.Point(12, 93);
			this.hvscDirTb.Name = "hvscDirTb";
			this.hvscDirTb.ReadOnly = true;
			this.hvscDirTb.Size = new System.Drawing.Size(225, 20);
			this.hvscDirTb.TabIndex = 10;
			this.hvscDirTb.TabStop = false;
			// 
			// songLengthsCb
			// 
			this.songLengthsCb.AutoSize = true;
			this.songLengthsCb.Location = new System.Drawing.Point(12, 119);
			this.songLengthsCb.Name = "songLengthsCb";
			this.songLengthsCb.Size = new System.Drawing.Size(178, 17);
			this.songLengthsCb.TabIndex = 11;
			this.songLengthsCb.Text = "Override internal songlengths db";
			this.songLengthsCb.UseVisualStyleBackColor = true;
			this.songLengthsCb.EnabledChanged += new System.EventHandler(this.songLengthCb_EnabledChanged);
			// 
			// TpartyIntegrationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(249, 155);
			this.Controls.Add(this.songLengthsCb);
			this.Controls.Add(this.hvscDirTb);
			this.Controls.Add(this.modulesCb);
			this.Controls.Add(this.hvscLink);
			this.Controls.Add(this.xmPlayLink);
			this.Controls.Add(this.browseHvscBtn);
			this.Controls.Add(this.importXmPlayBtn);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
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
		private System.Windows.Forms.Button browseHvscBtn;
		private System.Windows.Forms.LinkLabel hvscLink;
		private System.Windows.Forms.OpenFileDialog openXmPlayDialog;
		public System.Windows.Forms.CheckBox modulesCb;
		private System.Windows.Forms.OpenFileDialog openSidPlayDialog;
		private System.Windows.Forms.TextBox hvscDirTb;
		private System.Windows.Forms.CheckBox songLengthsCb;
	}
}