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
			this.lineHlStyleList = new System.Windows.Forms.ComboBox();
			this.movingHlCb = new System.Windows.Forms.CheckBox();
			this.label29 = new System.Windows.Forms.Label();
			this.hlSizeUpDown = new System.Windows.Forms.NumericUpDown();
			this.label27 = new System.Windows.Forms.Label();
			this.label30 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.lineWidthUd = new System.Windows.Forms.NumericUpDown();
			this.lineStyleList = new System.Windows.Forms.ComboBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.qnGapFillUd = new System.Windows.Forms.NumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.hlSizeUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lineWidthUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.qnGapFillUd)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.hlBorderCb);
			this.groupBox3.Controls.Add(this.shrinkingHlCb);
			this.groupBox3.Controls.Add(this.lineHlStyleList);
			this.groupBox3.Controls.Add(this.movingHlCb);
			this.groupBox3.Controls.Add(this.label29);
			this.groupBox3.Controls.Add(this.hlSizeUpDown);
			this.groupBox3.Controls.Add(this.label27);
			this.groupBox3.Controls.Add(this.label30);
			this.groupBox3.Location = new System.Drawing.Point(25, 652);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(166, 139);
			this.groupBox3.TabIndex = 48;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Highlights properties";
			// 
			// hlBorderCb
			// 
			this.hlBorderCb.AutoSize = true;
			this.hlBorderCb.Location = new System.Drawing.Point(10, 117);
			this.hlBorderCb.Name = "hlBorderCb";
			this.hlBorderCb.Size = new System.Drawing.Size(57, 17);
			this.hlBorderCb.TabIndex = 22;
			this.hlBorderCb.Text = "Border";
			this.hlBorderCb.UseVisualStyleBackColor = true;
			this.hlBorderCb.CheckedChanged += new System.EventHandler(this.hlBorderCb_CheckedChanged);
			// 
			// shrinkingHlCb
			// 
			this.shrinkingHlCb.AutoSize = true;
			this.shrinkingHlCb.Location = new System.Drawing.Point(10, 94);
			this.shrinkingHlCb.Name = "shrinkingHlCb";
			this.shrinkingHlCb.Size = new System.Drawing.Size(70, 17);
			this.shrinkingHlCb.TabIndex = 22;
			this.shrinkingHlCb.Text = "Shrinking";
			this.shrinkingHlCb.UseVisualStyleBackColor = true;
			this.shrinkingHlCb.CheckedChanged += new System.EventHandler(this.shrinkingHlCb_CheckedChanged);
			// 
			// lineHlStyleList
			// 
			this.lineHlStyleList.DisplayMember = "Name";
			this.lineHlStyleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.lineHlStyleList.FormattingEnabled = true;
			this.lineHlStyleList.Location = new System.Drawing.Point(67, 19);
			this.lineHlStyleList.Name = "lineHlStyleList";
			this.lineHlStyleList.Size = new System.Drawing.Size(85, 21);
			this.lineHlStyleList.TabIndex = 20;
			this.lineHlStyleList.ValueMember = "Value";
			this.lineHlStyleList.SelectedIndexChanged += new System.EventHandler(this.lineHlStyleList_SelectedIndexChanged);
			// 
			// movingHlCb
			// 
			this.movingHlCb.AutoSize = true;
			this.movingHlCb.Location = new System.Drawing.Point(10, 71);
			this.movingHlCb.Name = "movingHlCb";
			this.movingHlCb.Size = new System.Drawing.Size(61, 17);
			this.movingHlCb.TabIndex = 22;
			this.movingHlCb.Text = "Moving";
			this.movingHlCb.UseVisualStyleBackColor = true;
			this.movingHlCb.CheckedChanged += new System.EventHandler(this.movingHlCb_CheckedChanged);
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(10, 46);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(27, 13);
			this.label29.TabIndex = 14;
			this.label29.Text = "Size";
			// 
			// hlSizeUpDown
			// 
			this.hlSizeUpDown.Location = new System.Drawing.Point(67, 45);
			this.hlSizeUpDown.Name = "hlSizeUpDown";
			this.hlSizeUpDown.Size = new System.Drawing.Size(47, 20);
			this.hlSizeUpDown.TabIndex = 21;
			this.hlSizeUpDown.ValueChanged += new System.EventHandler(this.hlSizeUpDown_ValueChanged);
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(7, 22);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(30, 13);
			this.label27.TabIndex = 9;
			this.label27.Text = "Style";
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(119, 47);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(33, 13);
			this.label30.TabIndex = 17;
			this.label30.Text = "pixels";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(146, 617);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(42, 13);
			this.label14.TabIndex = 51;
			this.label14.Text = "q-notes";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(147, 591);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(33, 13);
			this.label13.TabIndex = 52;
			this.label13.Text = "pixels";
			// 
			// lineWidthUd
			// 
			this.lineWidthUd.Location = new System.Drawing.Point(83, 589);
			this.lineWidthUd.Name = "lineWidthUd";
			this.lineWidthUd.Size = new System.Drawing.Size(58, 20);
			this.lineWidthUd.TabIndex = 44;
			this.lineWidthUd.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.lineWidthUd.ValueChanged += new System.EventHandler(this.lineWidthUd_ValueChanged);
			// 
			// lineStyleList
			// 
			this.lineStyleList.DisplayMember = "Name";
			this.lineStyleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.lineStyleList.FormattingEnabled = true;
			this.lineStyleList.Location = new System.Drawing.Point(83, 561);
			this.lineStyleList.Name = "lineStyleList";
			this.lineStyleList.Size = new System.Drawing.Size(106, 21);
			this.lineStyleList.TabIndex = 46;
			this.lineStyleList.ValueMember = "Value";
			this.lineStyleList.SelectedIndexChanged += new System.EventHandler(this.lineStyleList_SelectedIndexChanged);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(27, 564);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(51, 13);
			this.label17.TabIndex = 47;
			this.label17.Text = "Line style";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(22, 591);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(55, 13);
			this.label11.TabIndex = 49;
			this.label11.Text = "Line width";
			// 
			// qnGapFillUd
			// 
			this.qnGapFillUd.DecimalPlaces = 1;
			this.qnGapFillUd.Location = new System.Drawing.Point(83, 615);
			this.qnGapFillUd.Name = "qnGapFillUd";
			this.qnGapFillUd.Size = new System.Drawing.Size(58, 20);
			this.qnGapFillUd.TabIndex = 45;
			this.qnGapFillUd.ValueChanged += new System.EventHandler(this.qnGapFillUd_ValueChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(22, 617);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(57, 13);
			this.label12.TabIndex = 50;
			this.label12.Text = "Fill gaps < ";
			// 
			// LineStyleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.lineWidthUd);
			this.Controls.Add(this.lineStyleList);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.qnGapFillUd);
			this.Controls.Add(this.label12);
			this.Name = "LineStyleControl";
			this.Size = new System.Drawing.Size(237, 820);
			this.Controls.SetChildIndex(this.label12, 0);
			this.Controls.SetChildIndex(this.qnGapFillUd, 0);
			this.Controls.SetChildIndex(this.label11, 0);
			this.Controls.SetChildIndex(this.label17, 0);
			this.Controls.SetChildIndex(this.lineStyleList, 0);
			this.Controls.SetChildIndex(this.lineWidthUd, 0);
			this.Controls.SetChildIndex(this.label13, 0);
			this.Controls.SetChildIndex(this.label14, 0);
			this.Controls.SetChildIndex(this.groupBox3, 0);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.hlSizeUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lineWidthUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.qnGapFillUd)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox hlBorderCb;
		private System.Windows.Forms.CheckBox shrinkingHlCb;
		private System.Windows.Forms.ComboBox lineHlStyleList;
		private System.Windows.Forms.CheckBox movingHlCb;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.NumericUpDown hlSizeUpDown;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown lineWidthUd;
		private System.Windows.Forms.ComboBox lineStyleList;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown qnGapFillUd;
		private System.Windows.Forms.Label label12;
	}
}
