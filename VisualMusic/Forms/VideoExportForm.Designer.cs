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
			this.StereoscopicCb = new System.Windows.Forms.CheckBox();
			this.vrMetadataCb = new System.Windows.Forms.CheckBox();
			this.resoComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ssFactorComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.fpsTb = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(87, 173);
			this.okBtn.TabIndex = 60;
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(168, 173);
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
			// resoComboBox
			// 
			this.resoComboBox.FormattingEnabled = true;
			this.resoComboBox.Location = new System.Drawing.Point(75, 65);
			this.resoComboBox.Name = "resoComboBox";
			this.resoComboBox.Size = new System.Drawing.Size(101, 21);
			this.resoComboBox.TabIndex = 40;
			this.resoComboBox.TextChanged += new System.EventHandler(this.resoComboBox_TextChanged);
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
			// ssFactorComboBox
			// 
			this.ssFactorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ssFactorComboBox.FormattingEnabled = true;
			this.ssFactorComboBox.Items.AddRange(new object[] {
            "Disabled",
            "2x",
            "4x",
            "8x"});
			this.ssFactorComboBox.Location = new System.Drawing.Point(74, 92);
			this.ssFactorComboBox.Name = "ssFactorComboBox";
			this.ssFactorComboBox.Size = new System.Drawing.Size(102, 21);
			this.ssFactorComboBox.TabIndex = 50;
			this.ssFactorComboBox.TextChanged += new System.EventHandler(this.ssFactorComboBox_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(33, 95);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 62;
			this.label2.Text = "SSAA";
			// 
			// fpsTb
			// 
			this.fpsTb.Location = new System.Drawing.Point(75, 119);
			this.fpsTb.Name = "fpsTb";
			this.fpsTb.Size = new System.Drawing.Size(46, 20);
			this.fpsTb.TabIndex = 71;
			this.fpsTb.Text = "0";
			this.fpsTb.TextChanged += new System.EventHandler(this.fpsTb_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(42, 122);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(27, 13);
			this.label3.TabIndex = 62;
			this.label3.Text = "FPS";
			// 
			// VideoExportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(255, 208);
			this.Controls.Add(this.fpsTb);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ssFactorComboBox);
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
			this.Controls.SetChildIndex(this.ssFactorComboBox, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.fpsTb, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public System.Windows.Forms.CheckBox StereoscopicCb;
		private System.Windows.Forms.CheckBox sphereCb;
		public System.Windows.Forms.CheckBox vrMetadataCb;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.ComboBox resoComboBox;
		public System.Windows.Forms.ComboBox ssFactorComboBox;
		private System.Windows.Forms.TextBox fpsTb;
		private System.Windows.Forms.Label label3;
	}
}
