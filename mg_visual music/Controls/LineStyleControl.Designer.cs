namespace Visual_Music
{
	partial class LineStyleControl
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
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.hlBorderCb = new System.Windows.Forms.CheckBox();
			this.shrinkingHlCb = new System.Windows.Forms.CheckBox();
			this.lineHlTypeList = new System.Windows.Forms.ComboBox();
			this.movingHlCb = new System.Windows.Forms.CheckBox();
			this.label29 = new System.Windows.Forms.Label();
			this.hlSizeUpDown = new System.Windows.Forms.NumericUpDown();
			this.label27 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.lineWidthUd = new System.Windows.Forms.NumericUpDown();
			this.lineTypeList = new System.Windows.Forms.ComboBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.qnGapFillUd = new System.Windows.Forms.NumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.lineControlsPanel = new System.Windows.Forms.Panel();
			this.continuousCb = new System.Windows.Forms.CheckBox();
			this.hlMovementPowUd = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.hlSizeUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lineWidthUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.qnGapFillUd)).BeginInit();
			this.lineControlsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.hlMovementPowUd)).BeginInit();
			this.SuspendLayout();
			// 
			// modGbox
			// 
			this.modGbox.Location = new System.Drawing.Point(4, 323);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.hlBorderCb);
			this.groupBox3.Controls.Add(this.shrinkingHlCb);
			this.groupBox3.Controls.Add(this.lineHlTypeList);
			this.groupBox3.Controls.Add(this.movingHlCb);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.label29);
			this.groupBox3.Controls.Add(this.hlMovementPowUd);
			this.groupBox3.Controls.Add(this.hlSizeUpDown);
			this.groupBox3.Controls.Add(this.label27);
			this.groupBox3.Location = new System.Drawing.Point(0, 131);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(177, 175);
			this.groupBox3.TabIndex = 50;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Highlights properties";
			// 
			// hlBorderCb
			// 
			this.hlBorderCb.AutoSize = true;
			this.hlBorderCb.Location = new System.Drawing.Point(6, 152);
			this.hlBorderCb.Name = "hlBorderCb";
			this.hlBorderCb.Size = new System.Drawing.Size(57, 17);
			this.hlBorderCb.TabIndex = 50;
			this.hlBorderCb.Text = "Border";
			this.hlBorderCb.UseVisualStyleBackColor = true;
			this.hlBorderCb.CheckedChanged += new System.EventHandler(this.hlBorderCb_CheckedChanged);
			// 
			// shrinkingHlCb
			// 
			this.shrinkingHlCb.AutoSize = true;
			this.shrinkingHlCb.Location = new System.Drawing.Point(6, 129);
			this.shrinkingHlCb.Name = "shrinkingHlCb";
			this.shrinkingHlCb.Size = new System.Drawing.Size(70, 17);
			this.shrinkingHlCb.TabIndex = 40;
			this.shrinkingHlCb.Text = "Shrinking";
			this.shrinkingHlCb.UseVisualStyleBackColor = true;
			this.shrinkingHlCb.CheckedChanged += new System.EventHandler(this.shrinkingHlCb_CheckedChanged);
			// 
			// lineHlTypeList
			// 
			this.lineHlTypeList.DisplayMember = "Name";
			this.lineHlTypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.lineHlTypeList.FormattingEnabled = true;
			this.lineHlTypeList.Location = new System.Drawing.Point(43, 19);
			this.lineHlTypeList.Name = "lineHlTypeList";
			this.lineHlTypeList.Size = new System.Drawing.Size(109, 21);
			this.lineHlTypeList.TabIndex = 10;
			this.lineHlTypeList.ValueMember = "Value";
			this.lineHlTypeList.SelectedIndexChanged += new System.EventHandler(this.lineHlTypeList_SelectedIndexChanged);
			// 
			// movingHlCb
			// 
			this.movingHlCb.AutoSize = true;
			this.movingHlCb.Location = new System.Drawing.Point(6, 106);
			this.movingHlCb.Name = "movingHlCb";
			this.movingHlCb.Size = new System.Drawing.Size(61, 17);
			this.movingHlCb.TabIndex = 30;
			this.movingHlCb.Text = "Moving";
			this.movingHlCb.UseVisualStyleBackColor = true;
			this.movingHlCb.CheckedChanged += new System.EventHandler(this.movingHlCb_CheckedChanged);
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(68, 48);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(27, 13);
			this.label29.TabIndex = 14;
			this.label29.Text = "Size";
			// 
			// hlSizeUpDown
			// 
			this.hlSizeUpDown.Location = new System.Drawing.Point(101, 46);
			this.hlSizeUpDown.Name = "hlSizeUpDown";
			this.hlSizeUpDown.Size = new System.Drawing.Size(61, 20);
			this.hlSizeUpDown.TabIndex = 20;
			this.hlSizeUpDown.ValueChanged += new System.EventHandler(this.hlSizeUpDown_ValueChanged);
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(6, 22);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(31, 13);
			this.label27.TabIndex = 9;
			this.label27.Text = "Type";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(126, 61);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(42, 13);
			this.label14.TabIndex = 51;
			this.label14.Text = "q-notes";
			// 
			// lineWidthUd
			// 
			this.lineWidthUd.Location = new System.Drawing.Point(61, 30);
			this.lineWidthUd.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.lineWidthUd.Name = "lineWidthUd";
			this.lineWidthUd.Size = new System.Drawing.Size(58, 20);
			this.lineWidthUd.TabIndex = 20;
			this.lineWidthUd.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.lineWidthUd.ValueChanged += new System.EventHandler(this.lineWidthUd_ValueChanged);
			// 
			// lineTypeList
			// 
			this.lineTypeList.DisplayMember = "Name";
			this.lineTypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.lineTypeList.FormattingEnabled = true;
			this.lineTypeList.Location = new System.Drawing.Point(61, 3);
			this.lineTypeList.Name = "lineTypeList";
			this.lineTypeList.Size = new System.Drawing.Size(102, 21);
			this.lineTypeList.TabIndex = 10;
			this.lineTypeList.ValueMember = "Value";
			this.lineTypeList.SelectedIndexChanged += new System.EventHandler(this.lineTypeList_SelectedIndexChanged);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(24, 6);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(31, 13);
			this.label17.TabIndex = 47;
			this.label17.Text = "Type";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(20, 32);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(35, 13);
			this.label11.TabIndex = 49;
			this.label11.Text = "Width";
			// 
			// qnGapFillUd
			// 
			this.qnGapFillUd.DecimalPlaces = 1;
			this.qnGapFillUd.Location = new System.Drawing.Point(61, 56);
			this.qnGapFillUd.Name = "qnGapFillUd";
			this.qnGapFillUd.Size = new System.Drawing.Size(59, 20);
			this.qnGapFillUd.TabIndex = 30;
			this.qnGapFillUd.ValueChanged += new System.EventHandler(this.qnGapFillUd_ValueChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(1, 61);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(54, 13);
			this.label12.TabIndex = 50;
			this.label12.Text = "Fill gaps <";
			// 
			// lineControlsPanel
			// 
			this.lineControlsPanel.AutoSize = true;
			this.lineControlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.lineControlsPanel.Controls.Add(this.continuousCb);
			this.lineControlsPanel.Controls.Add(this.label17);
			this.lineControlsPanel.Controls.Add(this.groupBox3);
			this.lineControlsPanel.Controls.Add(this.label12);
			this.lineControlsPanel.Controls.Add(this.label14);
			this.lineControlsPanel.Controls.Add(this.qnGapFillUd);
			this.lineControlsPanel.Controls.Add(this.label11);
			this.lineControlsPanel.Controls.Add(this.lineWidthUd);
			this.lineControlsPanel.Controls.Add(this.lineTypeList);
			this.lineControlsPanel.Location = new System.Drawing.Point(0, 0);
			this.lineControlsPanel.Name = "lineControlsPanel";
			this.lineControlsPanel.Size = new System.Drawing.Size(180, 309);
			this.lineControlsPanel.TabIndex = 10;
			// 
			// continuousCb
			// 
			this.continuousCb.AutoSize = true;
			this.continuousCb.Location = new System.Drawing.Point(6, 94);
			this.continuousCb.Name = "continuousCb";
			this.continuousCb.Size = new System.Drawing.Size(79, 17);
			this.continuousCb.TabIndex = 40;
			this.continuousCb.Text = "Continuous";
			this.continuousCb.UseVisualStyleBackColor = true;
			this.continuousCb.CheckedChanged += new System.EventHandler(this.continuousCb_CheckedChanged);
			// 
			// hlMovementPowUd
			// 
			this.hlMovementPowUd.DecimalPlaces = 1;
			this.hlMovementPowUd.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.hlMovementPowUd.Location = new System.Drawing.Point(101, 76);
			this.hlMovementPowUd.Name = "hlMovementPowUd";
			this.hlMovementPowUd.Size = new System.Drawing.Size(61, 20);
			this.hlMovementPowUd.TabIndex = 20;
			this.hlMovementPowUd.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.hlMovementPowUd.ValueChanged += new System.EventHandler(this.hlMovementPowUd_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 78);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(89, 13);
			this.label3.TabIndex = 14;
			this.label3.Text = "Movement power";
			// 
			// LineStyleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Controls.Add(this.lineControlsPanel);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "LineStyleControl";
			this.Size = new System.Drawing.Size(184, 378);
			this.Controls.SetChildIndex(this.modGbox, 0);
			this.Controls.SetChildIndex(this.lineControlsPanel, 0);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.hlSizeUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lineWidthUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.qnGapFillUd)).EndInit();
			this.lineControlsPanel.ResumeLayout(false);
			this.lineControlsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.hlMovementPowUd)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox hlBorderCb;
		private System.Windows.Forms.CheckBox shrinkingHlCb;
		private System.Windows.Forms.ComboBox lineHlTypeList;
		private System.Windows.Forms.CheckBox movingHlCb;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.NumericUpDown hlSizeUpDown;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.NumericUpDown lineWidthUd;
		private System.Windows.Forms.ComboBox lineTypeList;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown qnGapFillUd;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Panel lineControlsPanel;
		private System.Windows.Forms.CheckBox continuousCb;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown hlMovementPowUd;
	}
}
