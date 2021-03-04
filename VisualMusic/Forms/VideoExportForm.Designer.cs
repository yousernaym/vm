namespace VisualMusic
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
			this.stereoscopicCb = new System.Windows.Forms.CheckBox();
			this.sphericalMetadataCb = new System.Windows.Forms.CheckBox();
			this.resoCombo = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ssFactorComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.fpsUd = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.videoQualityLossCombo = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.fpsUd)).BeginInit();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(87, 188);
			this.okBtn.TabIndex = 60;
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(168, 188);
			this.cancelBtn.TabIndex = 70;
			// 
			// sphereCb
			// 
			this.sphereCb.AutoSize = true;
			this.sphereCb.Location = new System.Drawing.Point(12, 12);
			this.sphereCb.Name = "sphereCb";
			this.sphereCb.Size = new System.Drawing.Size(70, 17);
			this.sphereCb.TabIndex = 10;
			this.sphereCb.Text = "Spherical";
			this.sphereCb.UseVisualStyleBackColor = true;
			this.sphereCb.CheckedChanged += new System.EventHandler(this.sphereCb_CheckedChanged);
			// 
			// stereoscopicCb
			// 
			this.stereoscopicCb.AutoSize = true;
			this.stereoscopicCb.Location = new System.Drawing.Point(12, 35);
			this.stereoscopicCb.Name = "stereoscopicCb";
			this.stereoscopicCb.Size = new System.Drawing.Size(156, 17);
			this.stereoscopicCb.TabIndex = 30;
			this.stereoscopicCb.Text = "Stereoscopic (experimental)";
			this.stereoscopicCb.UseVisualStyleBackColor = true;
			this.stereoscopicCb.CheckedChanged += new System.EventHandler(this.StereoscopicCb_CheckedChanged);
			// 
			// sphericalMetadataCb
			// 
			this.sphericalMetadataCb.AutoSize = true;
			this.sphericalMetadataCb.Enabled = false;
			this.sphericalMetadataCb.Location = new System.Drawing.Point(103, 12);
			this.sphericalMetadataCb.Name = "sphericalMetadataCb";
			this.sphericalMetadataCb.Size = new System.Drawing.Size(137, 17);
			this.sphericalMetadataCb.TabIndex = 20;
			this.sphericalMetadataCb.Text = "Add spherical metadata";
			this.sphericalMetadataCb.UseVisualStyleBackColor = true;
			this.sphericalMetadataCb.CheckedChanged += new System.EventHandler(this.vrMetadataCb_CheckedChanged);
			// 
			// resoCombo
			// 
			this.resoCombo.FormattingEnabled = true;
			this.resoCombo.Location = new System.Drawing.Point(104, 65);
			this.resoCombo.Name = "resoCombo";
			this.resoCombo.Size = new System.Drawing.Size(112, 21);
			this.resoCombo.TabIndex = 40;
			this.resoCombo.DropDown += new System.EventHandler(this.resoComboBox_DropDown);
			this.resoCombo.DropDownClosed += new System.EventHandler(this.resoCombo_DropDownClosed);
			this.resoCombo.TextChanged += new System.EventHandler(this.resoComboBox_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(41, 68);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 62;
			this.label1.Text = "Resolution";
			// 
			// ssFactorComboBox
			// 
			this.ssFactorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ssFactorComboBox.FormattingEnabled = true;
			this.ssFactorComboBox.Items.AddRange(new object[] {
            "Disabled",
            "2x",
            "4x",
            "8x"});
			this.ssFactorComboBox.Location = new System.Drawing.Point(104, 92);
			this.ssFactorComboBox.Name = "ssFactorComboBox";
			this.ssFactorComboBox.Size = new System.Drawing.Size(112, 21);
			this.ssFactorComboBox.TabIndex = 50;
			this.ssFactorComboBox.TextChanged += new System.EventHandler(this.ssFactorComboBox_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(63, 95);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 62;
			this.label2.Text = "SSAA";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(70, 148);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(27, 13);
			this.label3.TabIndex = 62;
			this.label3.Text = "FPS";
			// 
			// fpsUd
			// 
			this.fpsUd.DecimalPlaces = 2;
			this.fpsUd.Location = new System.Drawing.Point(103, 146);
			this.fpsUd.Name = "fpsUd";
			this.fpsUd.Size = new System.Drawing.Size(62, 20);
			this.fpsUd.TabIndex = 71;
			this.fpsUd.ValueChanged += new System.EventHandler(this.fpsUd_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 122);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 13);
			this.label4.TabIndex = 62;
			this.label4.Text = "Video quality loss";
			// 
			// videoQualityLossCombo
			// 
			this.videoQualityLossCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoQualityLossCombo.FormattingEnabled = true;
			this.videoQualityLossCombo.Location = new System.Drawing.Point(104, 119);
			this.videoQualityLossCombo.Name = "videoQualityLossCombo";
			this.videoQualityLossCombo.Size = new System.Drawing.Size(112, 21);
			this.videoQualityLossCombo.TabIndex = 50;
			this.videoQualityLossCombo.SelectedIndexChanged += new System.EventHandler(this.videoQualityLossCombo_SelectedIndexChanged);
			// 
			// VideoExportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(255, 223);
			this.Controls.Add(this.fpsUd);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.videoQualityLossCombo);
			this.Controls.Add(this.ssFactorComboBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.resoCombo);
			this.Controls.Add(this.sphericalMetadataCb);
			this.Controls.Add(this.stereoscopicCb);
			this.Controls.Add(this.sphereCb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "VideoExportForm";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.sphereCb, 0);
			this.Controls.SetChildIndex(this.stereoscopicCb, 0);
			this.Controls.SetChildIndex(this.sphericalMetadataCb, 0);
			this.Controls.SetChildIndex(this.resoCombo, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.ssFactorComboBox, 0);
			this.Controls.SetChildIndex(this.videoQualityLossCombo, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.fpsUd, 0);
			((System.ComponentModel.ISupportInitialize)(this.fpsUd)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public System.Windows.Forms.CheckBox stereoscopicCb;
		private System.Windows.Forms.CheckBox sphereCb;
		public System.Windows.Forms.CheckBox sphericalMetadataCb;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.ComboBox resoCombo;
		public System.Windows.Forms.ComboBox ssFactorComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown fpsUd;
		private System.Windows.Forms.Label label4;
		public System.Windows.Forms.ComboBox videoQualityLossCombo;
	}
}
