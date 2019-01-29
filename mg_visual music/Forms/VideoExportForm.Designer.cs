﻿namespace Visual_Music
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
			this.StereoscopicCb = new System.Windows.Forms.CheckBox();
			this.vrMetadataCb = new System.Windows.Forms.CheckBox();
			this.resoComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(116, 112);
			this.okBtn.TabIndex = 50;
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(197, 112);
			this.cancelBtn.TabIndex = 60;
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
			// StereoscopicCb
			// 
			this.StereoscopicCb.AutoSize = true;
			this.StereoscopicCb.Location = new System.Drawing.Point(12, 35);
			this.StereoscopicCb.Name = "StereoscopicCb";
			this.StereoscopicCb.Size = new System.Drawing.Size(88, 17);
			this.StereoscopicCb.TabIndex = 30;
			this.StereoscopicCb.Text = "Stereoscopic";
			this.StereoscopicCb.UseVisualStyleBackColor = true;
			this.StereoscopicCb.CheckedChanged += new System.EventHandler(this.StereoscopicCb_CheckedChanged);
			// 
			// vrMetadataCb
			// 
			this.vrMetadataCb.AutoSize = true;
			this.vrMetadataCb.Enabled = false;
			this.vrMetadataCb.Location = new System.Drawing.Point(103, 12);
			this.vrMetadataCb.Name = "vrMetadataCb";
			this.vrMetadataCb.Size = new System.Drawing.Size(99, 17);
			this.vrMetadataCb.TabIndex = 20;
			this.vrMetadataCb.Text = "Inject metadata";
			this.vrMetadataCb.UseVisualStyleBackColor = true;
			this.vrMetadataCb.CheckedChanged += new System.EventHandler(this.vrMetadataCb_CheckedChanged);
			// 
			// resoCb
			// 
			this.resoComboBox.FormattingEnabled = true;
			this.resoComboBox.Location = new System.Drawing.Point(75, 65);
			this.resoComboBox.Name = "resoCb";
			this.resoComboBox.Size = new System.Drawing.Size(121, 21);
			this.resoComboBox.TabIndex = 61;
			this.resoComboBox.TextChanged += new System.EventHandler(this.resoCb_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 68);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 62;
			this.label1.Text = "Resolution";
			// 
			// VideoExportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(284, 147);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.resoComboBox);
			this.Controls.Add(this.vrMetadataCb);
			this.Controls.Add(this.StereoscopicCb);
			this.Controls.Add(this.sphereCb);
			this.Name = "VideoExportForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VideoExportForm_FormClosing);
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.sphereCb, 0);
			this.Controls.SetChildIndex(this.StereoscopicCb, 0);
			this.Controls.SetChildIndex(this.vrMetadataCb, 0);
			this.Controls.SetChildIndex(this.resoComboBox, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public System.Windows.Forms.CheckBox StereoscopicCb;
		private System.Windows.Forms.CheckBox sphereCb;
		public System.Windows.Forms.CheckBox vrMetadataCb;
		private System.Windows.Forms.ComboBox resoComboBox;
		private System.Windows.Forms.Label label1;
	}
}
