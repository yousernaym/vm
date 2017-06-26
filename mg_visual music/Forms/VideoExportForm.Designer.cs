namespace Visual_Music
{
	partial class VideoExportForm
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
			this.sphereCb = new System.Windows.Forms.CheckBox();
			this.fullHdRb = new System.Windows.Forms.RadioButton();
			this.resolutionGroup = new System.Windows.Forms.GroupBox();
			this.fourkRb = new System.Windows.Forms.RadioButton();
			this.StereoscopicCb = new System.Windows.Forms.CheckBox();
			this.youtubeCb = new System.Windows.Forms.CheckBox();
			this.resolutionGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// sphereCb
			// 
			this.sphereCb.AutoSize = true;
			this.sphereCb.Location = new System.Drawing.Point(12, 12);
			this.sphereCb.Name = "sphereCb";
			this.sphereCb.Size = new System.Drawing.Size(85, 17);
			this.sphereCb.TabIndex = 10;
			this.sphereCb.Text = "360 degrees";
			this.sphereCb.UseVisualStyleBackColor = true;
			this.sphereCb.CheckedChanged += new System.EventHandler(this.sphereCb_CheckedChanged);
			// 
			// fullHdRb
			// 
			this.fullHdRb.AutoSize = true;
			this.fullHdRb.Location = new System.Drawing.Point(6, 42);
			this.fullHdRb.Name = "fullHdRb";
			this.fullHdRb.Size = new System.Drawing.Size(84, 17);
			this.fullHdRb.TabIndex = 20;
			this.fullHdRb.Text = "1920 x 1080";
			this.fullHdRb.UseVisualStyleBackColor = true;
			this.fullHdRb.CheckedChanged += new System.EventHandler(this.fullHdRb_CheckedChanged);
			// 
			// resolutionGroup
			// 
			this.resolutionGroup.Controls.Add(this.fourkRb);
			this.resolutionGroup.Controls.Add(this.fullHdRb);
			this.resolutionGroup.Location = new System.Drawing.Point(12, 58);
			this.resolutionGroup.Name = "resolutionGroup";
			this.resolutionGroup.Size = new System.Drawing.Size(165, 76);
			this.resolutionGroup.TabIndex = 40;
			this.resolutionGroup.TabStop = false;
			this.resolutionGroup.Text = "Resolution";
			// 
			// fourkRb
			// 
			this.fourkRb.AutoSize = true;
			this.fourkRb.Location = new System.Drawing.Point(6, 19);
			this.fourkRb.Name = "fourkRb";
			this.fourkRb.Size = new System.Drawing.Size(78, 17);
			this.fourkRb.TabIndex = 10;
			this.fourkRb.Text = "3840x2160";
			this.fourkRb.UseVisualStyleBackColor = true;
			this.fourkRb.CheckedChanged += new System.EventHandler(this.fourkRb_CheckedChanged);
			// 
			// StereoscopicCb
			// 
			this.StereoscopicCb.AutoSize = true;
			this.StereoscopicCb.Location = new System.Drawing.Point(12, 35);
			this.StereoscopicCb.Name = "StereoscopicCb";
			this.StereoscopicCb.Size = new System.Drawing.Size(88, 17);
			this.StereoscopicCb.TabIndex = 30;
			this.StereoscopicCb.Text = "Stereoscopic";
			this.StereoscopicCb.UseVisualStyleBackColor = true;
			// 
			// youtubeCb
			// 
			this.youtubeCb.AutoSize = true;
			this.youtubeCb.Enabled = false;
			this.youtubeCb.Location = new System.Drawing.Point(103, 12);
			this.youtubeCb.Name = "youtubeCb";
			this.youtubeCb.Size = new System.Drawing.Size(113, 17);
			this.youtubeCb.TabIndex = 20;
			this.youtubeCb.Text = "Youtube metadata";
			this.youtubeCb.UseVisualStyleBackColor = true;
			// 
			// VideoExportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.resolutionGroup);
			this.Controls.Add(this.youtubeCb);
			this.Controls.Add(this.StereoscopicCb);
			this.Controls.Add(this.sphereCb);
			this.Name = "VideoExportForm";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.sphereCb, 0);
			this.Controls.SetChildIndex(this.StereoscopicCb, 0);
			this.Controls.SetChildIndex(this.youtubeCb, 0);
			this.Controls.SetChildIndex(this.resolutionGroup, 0);
			this.resolutionGroup.ResumeLayout(false);
			this.resolutionGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.RadioButton fullHdRb;
		private System.Windows.Forms.GroupBox resolutionGroup;
		public System.Windows.Forms.CheckBox StereoscopicCb;
		private System.Windows.Forms.CheckBox sphereCb;
		public System.Windows.Forms.CheckBox youtubeCb;
		private System.Windows.Forms.RadioButton fourkRb;
	}
}
