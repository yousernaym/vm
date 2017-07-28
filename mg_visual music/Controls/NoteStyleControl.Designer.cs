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
			this.colorDestBtn = new System.Windows.Forms.Button();
			this.powerUd = new System.Windows.Forms.NumericUpDown();
			this.label38 = new System.Windows.Forms.Label();
			this.fadeInUd = new System.Windows.Forms.NumericUpDown();
			this.startUd = new System.Windows.Forms.NumericUpDown();
			this.label45 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label42 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.fadeOutUd = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.stopUd = new System.Windows.Forms.NumericUpDown();
			this.colorDestCb = new System.Windows.Forms.CheckBox();
			this.angleDestCb = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.xSourceCombo = new System.Windows.Forms.ComboBox();
			this.ySourceCombo = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.combineXYCombo = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.modEntryCombo = new System.Windows.Forms.ComboBox();
			this.modEntryCm = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.newMi = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteMi = new System.Windows.Forms.ToolStripMenuItem();
			this.cloneMi = new System.Windows.Forms.ToolStripMenuItem();
			this.modEntryBs = new System.Windows.Forms.BindingSource(this.components);
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.angleDestUd = new System.Windows.Forms.NumericUpDown();
			this.modGbox = new System.Windows.Forms.GroupBox();
			this.bypassModEntryPanel = new System.Windows.Forms.Panel();
			this.bypassModEntryCb = new System.Windows.Forms.CheckBox();
			this.modEntryPanel = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.discardStopCb = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.powerUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeInUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.startUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeOutUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.stopUd)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.modEntryCm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.modEntryBs)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.angleDestUd)).BeginInit();
			this.modGbox.SuspendLayout();
			this.bypassModEntryPanel.SuspendLayout();
			this.modEntryPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// colorDestBtn
			// 
			this.colorDestBtn.BackColor = System.Drawing.Color.White;
			this.colorDestBtn.Location = new System.Drawing.Point(102, 29);
			this.colorDestBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.colorDestBtn.Name = "colorDestBtn";
			this.colorDestBtn.Size = new System.Drawing.Size(114, 35);
			this.colorDestBtn.TabIndex = 36;
			this.colorDestBtn.UseVisualStyleBackColor = false;
			this.colorDestBtn.Click += new System.EventHandler(this.colorDestBtn_Click);
			// 
			// powerUd
			// 
			this.powerUd.DecimalPlaces = 2;
			this.powerUd.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.powerUd.Location = new System.Drawing.Point(99, 474);
			this.powerUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.powerUd.Name = "powerUd";
			this.powerUd.Size = new System.Drawing.Size(114, 26);
			this.powerUd.TabIndex = 32;
			this.powerUd.ValueChanged += new System.EventHandler(this.powerUd_ValueChanged);
			// 
			// label38
			// 
			this.label38.AutoSize = true;
			this.label38.Location = new System.Drawing.Point(14, 478);
			this.label38.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(53, 20);
			this.label38.TabIndex = 23;
			this.label38.Text = "Power";
			// 
			// fadeInUd
			// 
			this.fadeInUd.DecimalPlaces = 2;
			this.fadeInUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.fadeInUd.Location = new System.Drawing.Point(99, 394);
			this.fadeInUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.fadeInUd.Name = "fadeInUd";
			this.fadeInUd.Size = new System.Drawing.Size(114, 26);
			this.fadeInUd.TabIndex = 33;
			this.fadeInUd.ValueChanged += new System.EventHandler(this.fadeInUd_ValueChanged);
			// 
			// startUd
			// 
			this.startUd.DecimalPlaces = 2;
			this.startUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.startUd.Location = new System.Drawing.Point(99, 314);
			this.startUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.startUd.Name = "startUd";
			this.startUd.Size = new System.Drawing.Size(114, 26);
			this.startUd.TabIndex = 34;
			this.startUd.ValueChanged += new System.EventHandler(this.startUd_ValueChanged);
			// 
			// label45
			// 
			this.label45.AutoSize = true;
			this.label45.Location = new System.Drawing.Point(210, 374);
			this.label45.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label45.Name = "label45";
			this.label45.Size = new System.Drawing.Size(23, 20);
			this.label45.TabIndex = 30;
			this.label45.Text = "%";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(210, 294);
			this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(23, 20);
			this.label18.TabIndex = 31;
			this.label18.Text = "%";
			// 
			// label42
			// 
			this.label42.AutoSize = true;
			this.label42.Location = new System.Drawing.Point(16, 397);
			this.label42.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label42.Name = "label42";
			this.label42.Size = new System.Drawing.Size(62, 20);
			this.label42.TabIndex = 27;
			this.label42.Text = "Fade in";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(16, 317);
			this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(44, 20);
			this.label19.TabIndex = 28;
			this.label19.Text = "Start";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 437);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 20);
			this.label1.TabIndex = 27;
			this.label1.Text = "Fade out";
			// 
			// fadeOutUd
			// 
			this.fadeOutUd.DecimalPlaces = 2;
			this.fadeOutUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.fadeOutUd.Location = new System.Drawing.Point(99, 434);
			this.fadeOutUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.fadeOutUd.Name = "fadeOutUd";
			this.fadeOutUd.Size = new System.Drawing.Size(114, 26);
			this.fadeOutUd.TabIndex = 33;
			this.fadeOutUd.ValueChanged += new System.EventHandler(this.fadeOutUd_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 357);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 20);
			this.label2.TabIndex = 28;
			this.label2.Text = "Stop";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(210, 334);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(23, 20);
			this.label3.TabIndex = 31;
			this.label3.Text = "%";
			// 
			// stopUd
			// 
			this.stopUd.DecimalPlaces = 2;
			this.stopUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.stopUd.Location = new System.Drawing.Point(99, 354);
			this.stopUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.stopUd.Name = "stopUd";
			this.stopUd.Size = new System.Drawing.Size(114, 26);
			this.stopUd.TabIndex = 34;
			this.stopUd.ValueChanged += new System.EventHandler(this.stopUd_ValueChanged);
			// 
			// colorDestCb
			// 
			this.colorDestCb.AutoSize = true;
			this.colorDestCb.Location = new System.Drawing.Point(10, 35);
			this.colorDestCb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.colorDestCb.Name = "colorDestCb";
			this.colorDestCb.Size = new System.Drawing.Size(72, 24);
			this.colorDestCb.TabIndex = 37;
			this.colorDestCb.Text = "Color";
			this.colorDestCb.UseVisualStyleBackColor = true;
			this.colorDestCb.CheckedChanged += new System.EventHandler(this.colorDestCb_CheckedChanged);
			// 
			// angleDestCb
			// 
			this.angleDestCb.AutoSize = true;
			this.angleDestCb.Location = new System.Drawing.Point(10, 78);
			this.angleDestCb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.angleDestCb.Name = "angleDestCb";
			this.angleDestCb.Size = new System.Drawing.Size(76, 24);
			this.angleDestCb.TabIndex = 37;
			this.angleDestCb.Text = "Angle";
			this.angleDestCb.UseVisualStyleBackColor = true;
			this.angleDestCb.CheckedChanged += new System.EventHandler(this.angleDestCb_CheckedChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 34);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(24, 20);
			this.label6.TabIndex = 38;
			this.label6.Text = "X:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(9, 75);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(24, 20);
			this.label7.TabIndex = 38;
			this.label7.Text = "Y:";
			// 
			// xSourceCombo
			// 
			this.xSourceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.xSourceCombo.FormattingEnabled = true;
			this.xSourceCombo.Items.AddRange(new object[] {
            "Dist from Left",
            "Dist from Center",
            "Dist from Right"});
			this.xSourceCombo.Location = new System.Drawing.Point(44, 29);
			this.xSourceCombo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.xSourceCombo.Name = "xSourceCombo";
			this.xSourceCombo.Size = new System.Drawing.Size(170, 28);
			this.xSourceCombo.TabIndex = 39;
			this.xSourceCombo.SelectedIndexChanged += new System.EventHandler(this.xSourceCombo_SelectedIndexChanged);
			// 
			// ySourceCombo
			// 
			this.ySourceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ySourceCombo.FormattingEnabled = true;
			this.ySourceCombo.Items.AddRange(new object[] {
            "Dist from Left",
            "Dist from Top",
            "Dist from Right"});
			this.ySourceCombo.Location = new System.Drawing.Point(44, 71);
			this.ySourceCombo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.ySourceCombo.Name = "ySourceCombo";
			this.ySourceCombo.Size = new System.Drawing.Size(170, 28);
			this.ySourceCombo.TabIndex = 39;
			this.ySourceCombo.SelectedIndexChanged += new System.EventHandler(this.ySourceCombo_SelectedIndexChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(9, 122);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(76, 20);
			this.label9.TabIndex = 38;
			this.label9.Text = "Combine:";
			// 
			// combineXYCombo
			// 
			this.combineXYCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.combineXYCombo.FormattingEnabled = true;
			this.combineXYCombo.Items.AddRange(new object[] {
            "x + y",
            "x * y",
            "Max (x, y) ",
            "Min (x, y)"});
			this.combineXYCombo.Location = new System.Drawing.Point(94, 117);
			this.combineXYCombo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.combineXYCombo.Name = "combineXYCombo";
			this.combineXYCombo.Size = new System.Drawing.Size(120, 28);
			this.combineXYCombo.TabIndex = 39;
			this.combineXYCombo.SelectedIndexChanged += new System.EventHandler(this.combineCombo_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.xSourceCombo);
			this.groupBox1.Controls.Add(this.combineXYCombo);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.ySourceCombo);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Location = new System.Drawing.Point(4, 5);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Size = new System.Drawing.Size(243, 168);
			this.groupBox1.TabIndex = 40;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Sources";
			// 
			// modEntryCombo
			// 
			this.modEntryCombo.ContextMenuStrip = this.modEntryCm;
			this.modEntryCombo.FormattingEnabled = true;
			this.modEntryCombo.Location = new System.Drawing.Point(14, 29);
			this.modEntryCombo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.modEntryCombo.Name = "modEntryCombo";
			this.modEntryCombo.Size = new System.Drawing.Size(222, 28);
			this.modEntryCombo.TabIndex = 41;
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
			this.modEntryCm.Size = new System.Drawing.Size(287, 94);
			// 
			// newMi
			// 
			this.newMi.Name = "newMi";
			this.newMi.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newMi.Size = new System.Drawing.Size(286, 30);
			this.newMi.Text = "New entry";
			this.newMi.Click += new System.EventHandler(this.newMi_Click);
			// 
			// deleteMi
			// 
			this.deleteMi.Enabled = false;
			this.deleteMi.Name = "deleteMi";
			this.deleteMi.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
			this.deleteMi.Size = new System.Drawing.Size(286, 30);
			this.deleteMi.Text = "Delete entry";
			this.deleteMi.Click += new System.EventHandler(this.deleteMi_Click);
			// 
			// cloneMi
			// 
			this.cloneMi.Enabled = false;
			this.cloneMi.Name = "cloneMi";
			this.cloneMi.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
			this.cloneMi.Size = new System.Drawing.Size(286, 30);
			this.cloneMi.Text = "Clone entry";
			this.cloneMi.Click += new System.EventHandler(this.cloneMi_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.angleDestCb);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.colorDestCb);
			this.groupBox2.Controls.Add(this.colorDestBtn);
			this.groupBox2.Controls.Add(this.angleDestUd);
			this.groupBox2.Location = new System.Drawing.Point(4, 182);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox2.Size = new System.Drawing.Size(243, 123);
			this.groupBox2.TabIndex = 42;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Destinations";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(170, 80);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(67, 20);
			this.label5.TabIndex = 28;
			this.label5.Text = "degrees";
			// 
			// angleDestUd
			// 
			this.angleDestUd.Location = new System.Drawing.Point(102, 77);
			this.angleDestUd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.angleDestUd.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
			this.angleDestUd.Name = "angleDestUd";
			this.angleDestUd.Size = new System.Drawing.Size(58, 26);
			this.angleDestUd.TabIndex = 34;
			this.angleDestUd.Value = new decimal(new int[] {
            45,
            0,
            0,
            0});
			this.angleDestUd.ValueChanged += new System.EventHandler(this.angleDestUd_ValueChanged);
			// 
			// modGbox
			// 
			this.modGbox.AutoSize = true;
			this.modGbox.Controls.Add(this.bypassModEntryPanel);
			this.modGbox.Controls.Add(this.modEntryCombo);
			this.modGbox.Location = new System.Drawing.Point(4, 5);
			this.modGbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.modGbox.Name = "modGbox";
			this.modGbox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.modGbox.Size = new System.Drawing.Size(282, 720);
			this.modGbox.TabIndex = 43;
			this.modGbox.TabStop = false;
			this.modGbox.Text = "Modulation";
			// 
			// bypassModEntryPanel
			// 
			this.bypassModEntryPanel.AutoSize = true;
			this.bypassModEntryPanel.Controls.Add(this.bypassModEntryCb);
			this.bypassModEntryPanel.Controls.Add(this.modEntryPanel);
			this.bypassModEntryPanel.Location = new System.Drawing.Point(9, 71);
			this.bypassModEntryPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.bypassModEntryPanel.Name = "bypassModEntryPanel";
			this.bypassModEntryPanel.Size = new System.Drawing.Size(264, 620);
			this.bypassModEntryPanel.TabIndex = 42;
			this.bypassModEntryPanel.Visible = false;
			// 
			// bypassModEntryCb
			// 
			this.bypassModEntryCb.AutoSize = true;
			this.bypassModEntryCb.Location = new System.Drawing.Point(12, 5);
			this.bypassModEntryCb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.bypassModEntryCb.Name = "bypassModEntryCb";
			this.bypassModEntryCb.Size = new System.Drawing.Size(126, 24);
			this.bypassModEntryCb.TabIndex = 1;
			this.bypassModEntryCb.Text = "Bypass entry";
			this.bypassModEntryCb.UseVisualStyleBackColor = true;
			this.bypassModEntryCb.Visible = false;
			this.bypassModEntryCb.CheckedChanged += new System.EventHandler(this.bypassModEntryCb_CheckedChanged);
			// 
			// modEntryPanel
			// 
			this.modEntryPanel.AutoSize = true;
			this.modEntryPanel.Controls.Add(this.discardStopCb);
			this.modEntryPanel.Controls.Add(this.label1);
			this.modEntryPanel.Controls.Add(this.label19);
			this.modEntryPanel.Controls.Add(this.label4);
			this.modEntryPanel.Controls.Add(this.label45);
			this.modEntryPanel.Controls.Add(this.groupBox2);
			this.modEntryPanel.Controls.Add(this.label42);
			this.modEntryPanel.Controls.Add(this.startUd);
			this.modEntryPanel.Controls.Add(this.fadeInUd);
			this.modEntryPanel.Controls.Add(this.label18);
			this.modEntryPanel.Controls.Add(this.groupBox1);
			this.modEntryPanel.Controls.Add(this.stopUd);
			this.modEntryPanel.Controls.Add(this.fadeOutUd);
			this.modEntryPanel.Controls.Add(this.label38);
			this.modEntryPanel.Controls.Add(this.powerUd);
			this.modEntryPanel.Controls.Add(this.label3);
			this.modEntryPanel.Controls.Add(this.label2);
			this.modEntryPanel.Location = new System.Drawing.Point(8, 40);
			this.modEntryPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.modEntryPanel.Name = "modEntryPanel";
			this.modEntryPanel.Size = new System.Drawing.Size(252, 566);
			this.modEntryPanel.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(210, 414);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(23, 20);
			this.label4.TabIndex = 30;
			this.label4.Text = "%";
			// 
			// discardStopCb
			// 
			this.discardStopCb.AutoSize = true;
			this.discardStopCb.Location = new System.Drawing.Point(17, 518);
			this.discardStopCb.Name = "discardStopCb";
			this.discardStopCb.Size = new System.Drawing.Size(164, 24);
			this.discardStopCb.TabIndex = 43;
			this.discardStopCb.Text = "Discard after Stop";
			this.discardStopCb.UseVisualStyleBackColor = true;
			this.discardStopCb.CheckedChanged += new System.EventHandler(this.discardStopCb_CheckedChanged);
			// 
			// NoteStyleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.modGbox);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "NoteStyleControl";
			this.Size = new System.Drawing.Size(291, 738);
			((System.ComponentModel.ISupportInitialize)(this.powerUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeInUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.startUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fadeOutUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.stopUd)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.modEntryCm.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.modEntryBs)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.angleDestUd)).EndInit();
			this.modGbox.ResumeLayout(false);
			this.modGbox.PerformLayout();
			this.bypassModEntryPanel.ResumeLayout(false);
			this.bypassModEntryPanel.PerformLayout();
			this.modEntryPanel.ResumeLayout(false);
			this.modEntryPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button colorDestBtn;
		private System.Windows.Forms.NumericUpDown powerUd;
		private System.Windows.Forms.Label label38;
		private System.Windows.Forms.NumericUpDown fadeInUd;
		private System.Windows.Forms.NumericUpDown startUd;
		private System.Windows.Forms.Label label45;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label42;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown fadeOutUd;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown stopUd;
		private System.Windows.Forms.CheckBox colorDestCb;
		private System.Windows.Forms.CheckBox angleDestCb;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox xSourceCombo;
		private System.Windows.Forms.ComboBox ySourceCombo;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox combineXYCombo;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox modEntryCombo;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox modGbox;
		private System.Windows.Forms.Panel bypassModEntryPanel;
		private System.Windows.Forms.CheckBox bypassModEntryCb;
		private System.Windows.Forms.Panel modEntryPanel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ContextMenuStrip modEntryCm;
		private System.Windows.Forms.ToolStripMenuItem newMi;
		private System.Windows.Forms.NumericUpDown angleDestUd;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.BindingSource modEntryBs;
		private System.Windows.Forms.ToolStripMenuItem deleteMi;
		private System.Windows.Forms.ToolStripMenuItem cloneMi;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.CheckBox discardStopCb;
	}
}
