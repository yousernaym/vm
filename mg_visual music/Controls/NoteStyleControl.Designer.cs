namespace Visual_Music
{
	partial class NoteStyleControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.modEntryCombo = new System.Windows.Forms.ComboBox();
			this.modEntryCm = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.newMi = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteMi = new System.Windows.Forms.ToolStripMenuItem();
			this.cloneMi = new System.Windows.Forms.ToolStripMenuItem();
			this.modEntryBs = new System.Windows.Forms.BindingSource(this.components);
			this.modGbox = new System.Windows.Forms.GroupBox();
			this.modEntryPanel = new System.Windows.Forms.Panel();
			this.interpolationGb = new System.Windows.Forms.GroupBox();
			this.invertCb = new System.Windows.Forms.CheckBox();
			this.startUd = new System.Windows.Forms.NumericUpDown();
			this.discardStopCb = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.powerUd = new System.Windows.Forms.NumericUpDown();
			this.label19 = new System.Windows.Forms.Label();
			this.label38 = new System.Windows.Forms.Label();
			this.fadeOutUd = new System.Windows.Forms.NumericUpDown();
			this.label42 = new System.Windows.Forms.Label();
			this.stopUd = new System.Windows.Forms.NumericUpDown();
			this.fadeInUd = new System.Windows.Forms.NumericUpDown();
			this.destinationsGb = new System.Windows.Forms.GroupBox();
			this.angleDestCb = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.colorDestCb = new System.Windows.Forms.CheckBox();
			this.colorDestBtn = new System.Windows.Forms.Button();
			this.angleDestUd = new System.Windows.Forms.NumericUpDown();
			this.pixelPosGb = new System.Windows.Forms.GroupBox();
			this.squareAspectCb = new System.Windows.Forms.CheckBox();
			this.yOriginCb = new System.Windows.Forms.CheckBox();
			this.xOriginCb = new System.Windows.Forms.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.yOriginUd = new System.Windows.Forms.NumericUpDown();
			this.xOriginUd = new System.Windows.Forms.NumericUpDown();
			this.combineXYCombo = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.modEntryCm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.modEntryBs)).BeginInit();
			this.modGbox.SuspendLayout();
			this.modEntryPanel.SuspendLayout();
			this.interpolationGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.startUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.powerUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeOutUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.stopUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeInUd)).BeginInit();
			this.destinationsGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.angleDestUd)).BeginInit();
			this.pixelPosGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.yOriginUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.xOriginUd)).BeginInit();
			this.SuspendLayout();
			// 
			// modEntryCombo
			// 
			this.modEntryCombo.ContextMenuStrip = this.modEntryCm;
			this.modEntryCombo.FormattingEnabled = true;
			this.modEntryCombo.Location = new System.Drawing.Point(6, 19);
			this.modEntryCombo.Name = "modEntryCombo";
			this.modEntryCombo.Size = new System.Drawing.Size(158, 21);
			this.modEntryCombo.TabIndex = 10;
			this.modEntryCombo.Tag = "Change Modulation Item";
			this.modEntryCombo.SelectedValueChanged += new System.EventHandler(this.modEntryCombo_SelectedValueChanged);
			// 
			// modEntryCm
			// 
			this.modEntryCm.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.modEntryCm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newMi,
            this.deleteMi,
            this.cloneMi});
			this.modEntryCm.Name = "modEntryCm";
			this.modEntryCm.Size = new System.Drawing.Size(211, 92);
			// 
			// newMi
			// 
			this.newMi.Name = "newMi";
			this.newMi.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newMi.Size = new System.Drawing.Size(210, 22);
			this.newMi.Tag = "New Modulation Entry";
			this.newMi.Text = "New entry";
			this.newMi.Click += new System.EventHandler(this.newMi_Click);
			// 
			// deleteMi
			// 
			this.deleteMi.Enabled = false;
			this.deleteMi.Name = "deleteMi";
			this.deleteMi.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
			this.deleteMi.Size = new System.Drawing.Size(210, 22);
			this.deleteMi.Tag = "Delete Modulation Entry";
			this.deleteMi.Text = "Delete entry";
			this.deleteMi.Click += new System.EventHandler(this.deleteMi_Click);
			// 
			// cloneMi
			// 
			this.cloneMi.Enabled = false;
			this.cloneMi.Name = "cloneMi";
			this.cloneMi.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
			this.cloneMi.Size = new System.Drawing.Size(210, 22);
			this.cloneMi.Tag = "Clone Modulation Entry";
			this.cloneMi.Text = "Clone entry";
			this.cloneMi.Click += new System.EventHandler(this.cloneMi_Click);
			// 
			// modGbox
			// 
			this.modGbox.AutoSize = true;
			this.modGbox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.modGbox.Controls.Add(this.modEntryPanel);
			this.modGbox.Controls.Add(this.modEntryCombo);
			this.modGbox.Location = new System.Drawing.Point(0, 0);
			this.modGbox.Name = "modGbox";
			this.modGbox.Size = new System.Drawing.Size(177, 498);
			this.modGbox.TabIndex = 43;
			this.modGbox.TabStop = false;
			this.modGbox.Text = "Modulation";
			// 
			// modEntryPanel
			// 
			this.modEntryPanel.AutoSize = true;
			this.modEntryPanel.Controls.Add(this.interpolationGb);
			this.modEntryPanel.Controls.Add(this.destinationsGb);
			this.modEntryPanel.Controls.Add(this.pixelPosGb);
			this.modEntryPanel.Location = new System.Drawing.Point(3, 46);
			this.modEntryPanel.Name = "modEntryPanel";
			this.modEntryPanel.Size = new System.Drawing.Size(168, 433);
			this.modEntryPanel.TabIndex = 42;
			this.modEntryPanel.Visible = false;
			// 
			// interpolationGb
			// 
			this.interpolationGb.Controls.Add(this.invertCb);
			this.interpolationGb.Controls.Add(this.startUd);
			this.interpolationGb.Controls.Add(this.discardStopCb);
			this.interpolationGb.Controls.Add(this.label2);
			this.interpolationGb.Controls.Add(this.label1);
			this.interpolationGb.Controls.Add(this.powerUd);
			this.interpolationGb.Controls.Add(this.label19);
			this.interpolationGb.Controls.Add(this.label38);
			this.interpolationGb.Controls.Add(this.fadeOutUd);
			this.interpolationGb.Controls.Add(this.label42);
			this.interpolationGb.Controls.Add(this.stopUd);
			this.interpolationGb.Controls.Add(this.fadeInUd);
			this.interpolationGb.Location = new System.Drawing.Point(3, 218);
			this.interpolationGb.Name = "interpolationGb";
			this.interpolationGb.Size = new System.Drawing.Size(158, 212);
			this.interpolationGb.TabIndex = 40;
			this.interpolationGb.TabStop = false;
			this.interpolationGb.Text = "Interpolation";
			// 
			// invertCb
			// 
			this.invertCb.AutoSize = true;
			this.invertCb.Location = new System.Drawing.Point(4, 188);
			this.invertCb.Name = "invertCb";
			this.invertCb.Size = new System.Drawing.Size(53, 17);
			this.invertCb.TabIndex = 70;
			this.invertCb.Tag = "Invert";
			this.invertCb.Text = "Invert";
			this.invertCb.UseVisualStyleBackColor = true;
			this.invertCb.CheckedChanged += new System.EventHandler(this.invertCb_CheckedChanged);
			// 
			// startUd
			// 
			this.startUd.DecimalPlaces = 2;
			this.startUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.startUd.Location = new System.Drawing.Point(69, 21);
			this.startUd.Name = "startUd";
			this.startUd.Size = new System.Drawing.Size(76, 20);
			this.startUd.TabIndex = 10;
			this.startUd.Tag = "Edit Start";
			this.startUd.ValueChanged += new System.EventHandler(this.startUd_ValueChanged);
			// 
			// discardStopCb
			// 
			this.discardStopCb.AutoSize = true;
			this.discardStopCb.Location = new System.Drawing.Point(4, 166);
			this.discardStopCb.Margin = new System.Windows.Forms.Padding(2);
			this.discardStopCb.Name = "discardStopCb";
			this.discardStopCb.Size = new System.Drawing.Size(111, 17);
			this.discardStopCb.TabIndex = 60;
			this.discardStopCb.Tag = "Discard after Stop";
			this.discardStopCb.Text = "Discard after Stop";
			this.discardStopCb.UseVisualStyleBackColor = true;
			this.discardStopCb.CheckedChanged += new System.EventHandler(this.discardStopCb_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(34, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 13);
			this.label2.TabIndex = 28;
			this.label2.Text = "Stop";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 101);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 13);
			this.label1.TabIndex = 27;
			this.label1.Text = "Fade out";
			// 
			// powerUd
			// 
			this.powerUd.DecimalPlaces = 2;
			this.powerUd.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.powerUd.Location = new System.Drawing.Point(69, 125);
			this.powerUd.Name = "powerUd";
			this.powerUd.Size = new System.Drawing.Size(76, 20);
			this.powerUd.TabIndex = 50;
			this.powerUd.Tag = "Edit Fade power";
			this.powerUd.ValueChanged += new System.EventHandler(this.powerUd_ValueChanged);
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(34, 23);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(29, 13);
			this.label19.TabIndex = 28;
			this.label19.Text = "Start";
			// 
			// label38
			// 
			this.label38.AutoSize = true;
			this.label38.Location = new System.Drawing.Point(0, 127);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(63, 13);
			this.label38.TabIndex = 23;
			this.label38.Text = "Fade power";
			// 
			// fadeOutUd
			// 
			this.fadeOutUd.DecimalPlaces = 2;
			this.fadeOutUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.fadeOutUd.Location = new System.Drawing.Point(69, 99);
			this.fadeOutUd.Name = "fadeOutUd";
			this.fadeOutUd.Size = new System.Drawing.Size(76, 20);
			this.fadeOutUd.TabIndex = 40;
			this.fadeOutUd.Tag = "Edit Fade out";
			this.fadeOutUd.ValueChanged += new System.EventHandler(this.fadeOutUd_ValueChanged);
			// 
			// label42
			// 
			this.label42.AutoSize = true;
			this.label42.Location = new System.Drawing.Point(21, 75);
			this.label42.Name = "label42";
			this.label42.Size = new System.Drawing.Size(42, 13);
			this.label42.TabIndex = 27;
			this.label42.Text = "Fade in";
			// 
			// stopUd
			// 
			this.stopUd.DecimalPlaces = 2;
			this.stopUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.stopUd.Location = new System.Drawing.Point(69, 47);
			this.stopUd.Name = "stopUd";
			this.stopUd.Size = new System.Drawing.Size(76, 20);
			this.stopUd.TabIndex = 20;
			this.stopUd.Tag = "Edit Stop";
			this.stopUd.ValueChanged += new System.EventHandler(this.stopUd_ValueChanged);
			// 
			// fadeInUd
			// 
			this.fadeInUd.DecimalPlaces = 2;
			this.fadeInUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.fadeInUd.Location = new System.Drawing.Point(69, 73);
			this.fadeInUd.Name = "fadeInUd";
			this.fadeInUd.Size = new System.Drawing.Size(76, 20);
			this.fadeInUd.TabIndex = 30;
			this.fadeInUd.Tag = "Edit Fade in";
			this.fadeInUd.ValueChanged += new System.EventHandler(this.fadeInUd_ValueChanged);
			// 
			// destinationsGb
			// 
			this.destinationsGb.Controls.Add(this.angleDestCb);
			this.destinationsGb.Controls.Add(this.label5);
			this.destinationsGb.Controls.Add(this.colorDestCb);
			this.destinationsGb.Controls.Add(this.colorDestBtn);
			this.destinationsGb.Controls.Add(this.angleDestUd);
			this.destinationsGb.Location = new System.Drawing.Point(3, 132);
			this.destinationsGb.Name = "destinationsGb";
			this.destinationsGb.Size = new System.Drawing.Size(158, 80);
			this.destinationsGb.TabIndex = 30;
			this.destinationsGb.TabStop = false;
			this.destinationsGb.Text = "Destinations";
			// 
			// angleDestCb
			// 
			this.angleDestCb.AutoSize = true;
			this.angleDestCb.Location = new System.Drawing.Point(7, 51);
			this.angleDestCb.Name = "angleDestCb";
			this.angleDestCb.Size = new System.Drawing.Size(53, 17);
			this.angleDestCb.TabIndex = 30;
			this.angleDestCb.Tag = "Angle";
			this.angleDestCb.Text = "Angle";
			this.angleDestCb.UseVisualStyleBackColor = true;
			this.angleDestCb.CheckedChanged += new System.EventHandler(this.angleDestCb_CheckedChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(108, 52);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(45, 13);
			this.label5.TabIndex = 28;
			this.label5.Text = "degrees";
			// 
			// colorDestCb
			// 
			this.colorDestCb.AutoSize = true;
			this.colorDestCb.Location = new System.Drawing.Point(7, 23);
			this.colorDestCb.Name = "colorDestCb";
			this.colorDestCb.Size = new System.Drawing.Size(50, 17);
			this.colorDestCb.TabIndex = 10;
			this.colorDestCb.Tag = "Color";
			this.colorDestCb.Text = "Color";
			this.colorDestCb.UseVisualStyleBackColor = true;
			this.colorDestCb.CheckedChanged += new System.EventHandler(this.colorDestCb_CheckedChanged);
			// 
			// colorDestBtn
			// 
			this.colorDestBtn.BackColor = System.Drawing.Color.White;
			this.colorDestBtn.Location = new System.Drawing.Point(63, 19);
			this.colorDestBtn.Name = "colorDestBtn";
			this.colorDestBtn.Size = new System.Drawing.Size(85, 23);
			this.colorDestBtn.TabIndex = 20;
			this.colorDestBtn.Tag = "Edit Color";
			this.colorDestBtn.UseVisualStyleBackColor = false;
			this.colorDestBtn.Click += new System.EventHandler(this.colorDestBtn_Click);
			// 
			// angleDestUd
			// 
			this.angleDestUd.Location = new System.Drawing.Point(63, 50);
			this.angleDestUd.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
			this.angleDestUd.Name = "angleDestUd";
			this.angleDestUd.Size = new System.Drawing.Size(39, 20);
			this.angleDestUd.TabIndex = 40;
			this.angleDestUd.Tag = "Edit Angle";
			this.angleDestUd.Value = new decimal(new int[] {
            45,
            0,
            0,
            0});
			this.angleDestUd.ValueChanged += new System.EventHandler(this.angleDestUd_ValueChanged);
			// 
			// pixelPosGb
			// 
			this.pixelPosGb.Controls.Add(this.squareAspectCb);
			this.pixelPosGb.Controls.Add(this.yOriginCb);
			this.pixelPosGb.Controls.Add(this.xOriginCb);
			this.pixelPosGb.Controls.Add(this.label8);
			this.pixelPosGb.Controls.Add(this.yOriginUd);
			this.pixelPosGb.Controls.Add(this.xOriginUd);
			this.pixelPosGb.Controls.Add(this.combineXYCombo);
			this.pixelPosGb.Controls.Add(this.label9);
			this.pixelPosGb.Location = new System.Drawing.Point(3, 3);
			this.pixelPosGb.Name = "pixelPosGb";
			this.pixelPosGb.Size = new System.Drawing.Size(158, 123);
			this.pixelPosGb.TabIndex = 20;
			this.pixelPosGb.TabStop = false;
			this.pixelPosGb.Text = "Pixel position";
			// 
			// squareAspectCb
			// 
			this.squareAspectCb.AutoSize = true;
			this.squareAspectCb.Location = new System.Drawing.Point(6, 100);
			this.squareAspectCb.Name = "squareAspectCb";
			this.squareAspectCb.Size = new System.Drawing.Size(95, 17);
			this.squareAspectCb.TabIndex = 60;
			this.squareAspectCb.Tag = "Square aspect";
			this.squareAspectCb.Text = "Square aspect";
			this.squareAspectCb.UseVisualStyleBackColor = true;
			this.squareAspectCb.CheckedChanged += new System.EventHandler(this.squareAspectCb_CheckedChanged);
			// 
			// yOriginCb
			// 
			this.yOriginCb.AutoSize = true;
			this.yOriginCb.Location = new System.Drawing.Point(105, 46);
			this.yOriginCb.Name = "yOriginCb";
			this.yOriginCb.Size = new System.Drawing.Size(33, 17);
			this.yOriginCb.TabIndex = 40;
			this.yOriginCb.Tag = "Y Origin";
			this.yOriginCb.Text = "Y";
			this.yOriginCb.UseVisualStyleBackColor = true;
			this.yOriginCb.CheckedChanged += new System.EventHandler(this.yOriginCb_CheckedChanged);
			// 
			// xOriginCb
			// 
			this.xOriginCb.AutoSize = true;
			this.xOriginCb.Location = new System.Drawing.Point(56, 46);
			this.xOriginCb.Name = "xOriginCb";
			this.xOriginCb.Size = new System.Drawing.Size(33, 17);
			this.xOriginCb.TabIndex = 30;
			this.xOriginCb.Tag = "X Origin";
			this.xOriginCb.Text = "X";
			this.xOriginCb.UseVisualStyleBackColor = true;
			this.xOriginCb.CheckedChanged += new System.EventHandler(this.xOriginCb_CheckedChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(18, 23);
			this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(34, 13);
			this.label8.TabIndex = 41;
			this.label8.Text = "Origin";
			// 
			// yOriginUd
			// 
			this.yOriginUd.DecimalPlaces = 2;
			this.yOriginUd.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
			this.yOriginUd.Location = new System.Drawing.Point(105, 21);
			this.yOriginUd.Margin = new System.Windows.Forms.Padding(2);
			this.yOriginUd.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.yOriginUd.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
			this.yOriginUd.Name = "yOriginUd";
			this.yOriginUd.Size = new System.Drawing.Size(43, 20);
			this.yOriginUd.TabIndex = 20;
			this.yOriginUd.Tag = "Edit Y Origin";
			this.yOriginUd.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.yOriginUd.ValueChanged += new System.EventHandler(this.yOriginUd_ValueChanged);
			// 
			// xOriginUd
			// 
			this.xOriginUd.DecimalPlaces = 2;
			this.xOriginUd.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
			this.xOriginUd.Location = new System.Drawing.Point(56, 21);
			this.xOriginUd.Margin = new System.Windows.Forms.Padding(2);
			this.xOriginUd.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.xOriginUd.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
			this.xOriginUd.Name = "xOriginUd";
			this.xOriginUd.Size = new System.Drawing.Size(45, 20);
			this.xOriginUd.TabIndex = 10;
			this.xOriginUd.Tag = "Edit X Origin";
			this.xOriginUd.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.xOriginUd.ValueChanged += new System.EventHandler(this.xOriginUd_ValueChanged);
			// 
			// combineXYCombo
			// 
			this.combineXYCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.combineXYCombo.FormattingEnabled = true;
			this.combineXYCombo.Items.AddRange(new object[] {
            "x + y",
            "x^2 + y^2",
            "Max (x, y) ",
            "Min (x, y)"});
			this.combineXYCombo.Location = new System.Drawing.Point(56, 68);
			this.combineXYCombo.Name = "combineXYCombo";
			this.combineXYCombo.Size = new System.Drawing.Size(92, 21);
			this.combineXYCombo.TabIndex = 50;
			this.combineXYCombo.Tag = "Change Combine";
			this.combineXYCombo.SelectedIndexChanged += new System.EventHandler(this.combineXYCombo_SelectedIndexChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(4, 71);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(48, 13);
			this.label9.TabIndex = 38;
			this.label9.Text = "Combine";
			// 
			// NoteStyleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.modGbox);
			this.Name = "NoteStyleControl";
			this.Size = new System.Drawing.Size(180, 501);
			this.modEntryCm.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.modEntryBs)).EndInit();
			this.modGbox.ResumeLayout(false);
			this.modGbox.PerformLayout();
			this.modEntryPanel.ResumeLayout(false);
			this.interpolationGb.ResumeLayout(false);
			this.interpolationGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.startUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.powerUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeOutUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.stopUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeInUd)).EndInit();
			this.destinationsGb.ResumeLayout(false);
			this.destinationsGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.angleDestUd)).EndInit();
			this.pixelPosGb.ResumeLayout(false);
			this.pixelPosGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.yOriginUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.xOriginUd)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ContextMenuStrip modEntryCm;
		private System.Windows.Forms.ToolStripMenuItem newMi;
		private System.Windows.Forms.BindingSource modEntryBs;
		private System.Windows.Forms.ToolStripMenuItem deleteMi;
		private System.Windows.Forms.ToolStripMenuItem cloneMi;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.Panel modEntryPanel;
		private System.Windows.Forms.CheckBox discardStopCb;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.GroupBox destinationsGb;
		private System.Windows.Forms.CheckBox angleDestCb;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox colorDestCb;
		private System.Windows.Forms.Button colorDestBtn;
		private System.Windows.Forms.NumericUpDown angleDestUd;
		private System.Windows.Forms.Label label42;
		private System.Windows.Forms.NumericUpDown startUd;
		private System.Windows.Forms.NumericUpDown fadeInUd;
		private System.Windows.Forms.GroupBox pixelPosGb;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.NumericUpDown yOriginUd;
		private System.Windows.Forms.NumericUpDown xOriginUd;
		private System.Windows.Forms.ComboBox combineXYCombo;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown stopUd;
		private System.Windows.Forms.NumericUpDown fadeOutUd;
		private System.Windows.Forms.Label label38;
		private System.Windows.Forms.NumericUpDown powerUd;
		private System.Windows.Forms.Label label2;
		protected System.Windows.Forms.GroupBox modGbox;
		private System.Windows.Forms.CheckBox yOriginCb;
		private System.Windows.Forms.CheckBox xOriginCb;
		private System.Windows.Forms.CheckBox invertCb;
		private System.Windows.Forms.GroupBox interpolationGb;
		private System.Windows.Forms.ComboBox modEntryCombo;
		private System.Windows.Forms.CheckBox squareAspectCb;
	}
}
