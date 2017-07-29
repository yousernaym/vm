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
			this.groupBox3.Location = new System.Drawing.Point(14, 994);
			this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox3.Size = new System.Drawing.Size(249, 214);
			this.groupBox3.TabIndex = 48;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Highlights properties";
			// 
			// hlBorderCb
			// 
			this.hlBorderCb.AutoSize = true;
			this.hlBorderCb.Location = new System.Drawing.Point(15, 180);
			this.hlBorderCb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.hlBorderCb.Name = "hlBorderCb";
			this.hlBorderCb.Size = new System.Drawing.Size(83, 24);
			this.hlBorderCb.TabIndex = 22;
			this.hlBorderCb.Text = "Border";
			this.hlBorderCb.UseVisualStyleBackColor = true;
			this.hlBorderCb.CheckedChanged += new System.EventHandler(this.hlBorderCb_CheckedChanged);
			// 
			// shrinkingHlCb
			// 
			this.shrinkingHlCb.AutoSize = true;
			this.shrinkingHlCb.Location = new System.Drawing.Point(15, 145);
			this.shrinkingHlCb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.shrinkingHlCb.Name = "shrinkingHlCb";
			this.shrinkingHlCb.Size = new System.Drawing.Size(101, 24);
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
			this.lineHlStyleList.Location = new System.Drawing.Point(100, 29);
			this.lineHlStyleList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.lineHlStyleList.Name = "lineHlStyleList";
			this.lineHlStyleList.Size = new System.Drawing.Size(126, 28);
			this.lineHlStyleList.TabIndex = 20;
			this.lineHlStyleList.ValueMember = "Value";
			this.lineHlStyleList.SelectedIndexChanged += new System.EventHandler(this.lineHlStyleList_SelectedIndexChanged);
			// 
			// movingHlCb
			// 
			this.movingHlCb.AutoSize = true;
			this.movingHlCb.Location = new System.Drawing.Point(15, 109);
			this.movingHlCb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.movingHlCb.Name = "movingHlCb";
			this.movingHlCb.Size = new System.Drawing.Size(85, 24);
			this.movingHlCb.TabIndex = 22;
			this.movingHlCb.Text = "Moving";
			this.movingHlCb.UseVisualStyleBackColor = true;
			this.movingHlCb.CheckedChanged += new System.EventHandler(this.movingHlCb_CheckedChanged);
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(15, 71);
			this.label29.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(40, 20);
			this.label29.TabIndex = 14;
			this.label29.Text = "Size";
			// 
			// hlSizeUpDown
			// 
			this.hlSizeUpDown.Location = new System.Drawing.Point(100, 69);
			this.hlSizeUpDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.hlSizeUpDown.Name = "hlSizeUpDown";
			this.hlSizeUpDown.Size = new System.Drawing.Size(70, 26);
			this.hlSizeUpDown.TabIndex = 21;
			this.hlSizeUpDown.ValueChanged += new System.EventHandler(this.hlSizeUpDown_ValueChanged);
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(10, 34);
			this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(44, 20);
			this.label27.TabIndex = 9;
			this.label27.Text = "Style";
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(178, 72);
			this.label30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(48, 20);
			this.label30.TabIndex = 17;
			this.label30.Text = "pixels";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(202, 957);
			this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(63, 20);
			this.label14.TabIndex = 51;
			this.label14.Text = "q-notes";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(204, 917);
			this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(48, 20);
			this.label13.TabIndex = 52;
			this.label13.Text = "pixels";
			// 
			// lineWidthUd
			// 
			this.lineWidthUd.Location = new System.Drawing.Point(108, 914);
			this.lineWidthUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.lineWidthUd.Name = "lineWidthUd";
			this.lineWidthUd.Size = new System.Drawing.Size(87, 26);
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
			this.lineStyleList.Location = new System.Drawing.Point(102, 863);
			this.lineStyleList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.lineStyleList.Name = "lineStyleList";
			this.lineStyleList.Size = new System.Drawing.Size(157, 28);
			this.lineStyleList.TabIndex = 46;
			this.lineStyleList.ValueMember = "Value";
			this.lineStyleList.SelectedIndexChanged += new System.EventHandler(this.lineStyleList_SelectedIndexChanged);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(16, 868);
			this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(75, 20);
			this.label17.TabIndex = 47;
			this.label17.Text = "Line style";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(16, 917);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(80, 20);
			this.label11.TabIndex = 49;
			this.label11.Text = "Line width";
			// 
			// qnGapFillUd
			// 
			this.qnGapFillUd.DecimalPlaces = 1;
			this.qnGapFillUd.Location = new System.Drawing.Point(108, 954);
			this.qnGapFillUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.qnGapFillUd.Name = "qnGapFillUd";
			this.qnGapFillUd.Size = new System.Drawing.Size(87, 26);
			this.qnGapFillUd.TabIndex = 45;
			this.qnGapFillUd.ValueChanged += new System.EventHandler(this.qnGapFillUd_ValueChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(16, 957);
			this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(84, 20);
			this.label12.TabIndex = 50;
			this.label12.Text = "Fill gaps < ";
			// 
			// LineStyleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.lineWidthUd);
			this.Controls.Add(this.lineStyleList);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.qnGapFillUd);
			this.Controls.Add(this.label12);
			this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
			this.Name = "LineStyleControl";
			this.Size = new System.Drawing.Size(292, 1213);
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
