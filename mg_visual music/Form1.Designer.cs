namespace Visual_Music
{
	partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.openProjDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveProjDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveVideoDlg = new System.Windows.Forms.SaveFileDialog();
            this.audioOffsetS = new System.Windows.Forms.NumericUpDown();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSongAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.playbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPlaybackPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.trackPropsPanel = new System.Windows.Forms.Panel();
            this.trackList = new Visual_Music.ListViewNF();
            this.trackColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.normalColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hilitedColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.trackListCM = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leftTrackPropsPanel = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.defaultStyleBtn = new System.Windows.Forms.Button();
            this.styleList = new System.Windows.Forms.ComboBox();
            this.lineStyleGroup = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.hlBorderCb = new System.Windows.Forms.CheckBox();
            this.shrinkingHlCb = new System.Windows.Forms.CheckBox();
            this.lineHlStyleList = new System.Windows.Forms.ComboBox();
            this.movingHlCb = new System.Windows.Forms.CheckBox();
            this.label29 = new System.Windows.Forms.Label();
            this.hlSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label27 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.blurredEdgeUd = new System.Windows.Forms.NumericUpDown();
            this.simpleLineStylePanel = new System.Windows.Forms.Panel();
            this.shapePowerUD = new System.Windows.Forms.NumericUpDown();
            this.fadeoutUd = new System.Windows.Forms.NumericUpDown();
            this.label38 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lineWidthUpDown = new System.Windows.Forms.NumericUpDown();
            this.lineStyleList = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.qnGapFillUd = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.texVScrollUD = new System.Windows.Forms.NumericUpDown();
            this.label33 = new System.Windows.Forms.Label();
            this.texUScrollUD = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label31 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.songAnchorLabel = new System.Windows.Forms.Label();
            this.noteAnchorLabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.songAnchorRb = new System.Windows.Forms.RadioButton();
            this.screenUAnchorRb = new System.Windows.Forms.RadioButton();
            this.noteUAnchorRb = new System.Windows.Forms.RadioButton();
            this.screenAnchorLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.screenVAnchorRb = new System.Windows.Forms.RadioButton();
            this.noteVAnchorRb = new System.Windows.Forms.RadioButton();
            this.texVTileCb = new System.Windows.Forms.CheckBox();
            this.texUTileCb = new System.Windows.Forms.CheckBox();
            this.texKeepAspectCb = new System.Windows.Forms.CheckBox();
            this.tileTexCb = new System.Windows.Forms.CheckBox();
            this.loadTexBtn = new System.Windows.Forms.Button();
            this.unloadTexBtn = new System.Windows.Forms.Button();
            this.pointSmpCb = new System.Windows.Forms.CheckBox();
            this.trackTexPb = new System.Windows.Forms.PictureBox();
            this.defaultMtrlBtn = new System.Windows.Forms.Button();
            this.alphaLbl = new System.Windows.Forms.Label();
            this.transpSlider = new System.Windows.Forms.TrackBar();
            this.hueTb = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.normalLumTb = new System.Windows.Forms.TextBox();
            this.normalLumSlider = new System.Windows.Forms.TrackBar();
            this.label = new System.Windows.Forms.Label();
            this.normalSatTb = new System.Windows.Forms.TextBox();
            this.normalSatSlider = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.hueSlider = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.hiliteLumTb = new System.Windows.Forms.TextBox();
            this.hiliteLumSlider = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.hiliteSatTb = new System.Windows.Forms.TextBox();
            this.hiliteSatSlider = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.transpTb = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.defaultLightBtn = new System.Windows.Forms.Button();
            this.lightPanel = new System.Windows.Forms.Panel();
            this.lightDirzTb = new System.Windows.Forms.TextBox();
            this.somelabel = new System.Windows.Forms.Label();
            this.specularGb = new System.Windows.Forms.GroupBox();
            this.specFovUd = new System.Windows.Forms.NumericUpDown();
            this.specPowUd = new System.Windows.Forms.NumericUpDown();
            this.specAmountUd = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.lightDiryTb = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.lightDirxTb = new System.Windows.Forms.TextBox();
            this.globalLightCb = new System.Windows.Forms.CheckBox();
            this.textureBrowseBtn = new System.Windows.Forms.Button();
            this.texPathTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.resetDefaultBtn = new System.Windows.Forms.Button();
            this.setDefaultBtn = new System.Windows.Forms.Button();
            this.openTextureDlg = new System.Windows.Forms.OpenFileDialog();
            this.trackPropsCb = new System.Windows.Forms.CheckBox();
            this.songPropsPanel = new System.Windows.Forms.Panel();
            this.defaultPitchesBtn = new System.Windows.Forms.Button();
            this.minPitchUd = new System.Windows.Forms.NumericUpDown();
            this.maxPitchUd = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.hnotelabel = new System.Windows.Forms.Label();
            this.upDownVpWidth = new Visual_Music.TbSlider();
            this.songPropsCb = new System.Windows.Forms.CheckBox();
            this.propsTogglePanel = new System.Windows.Forms.Panel();
            this.songPanelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.serviceContainerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.audioOffsetS)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.trackPropsPanel.SuspendLayout();
            this.trackListCM.SuspendLayout();
            this.leftTrackPropsPanel.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.lineStyleGroup.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hlSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blurredEdgeUd)).BeginInit();
            this.simpleLineStylePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shapePowerUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeoutUd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lineWidthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qnGapFillUd)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texVScrollUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.texUScrollUD)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackTexPb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.transpSlider)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalLumSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normalSatSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueSlider)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hiliteLumSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hiliteSatSlider)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.lightPanel.SuspendLayout();
            this.specularGb.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.specFovUd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.specPowUd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.specAmountUd)).BeginInit();
            this.songPropsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minPitchUd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxPitchUd)).BeginInit();
            this.propsTogglePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.songPanelBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.serviceContainerBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // openProjDialog
            // 
            this.openProjDialog.FileName = "openFileDialog1";
            this.openProjDialog.Filter = "Visual Music songs|*.vms|All files|*.*";
            // 
            // saveProjDialog
            // 
            this.saveProjDialog.Filter = "Visual Music songs|*.vms|All files|*.*";
            // 
            // saveVideoDlg
            // 
            this.saveVideoDlg.Filter = "Video files|*.wmv";
            this.saveVideoDlg.Title = "Save video file";
            // 
            // audioOffsetS
            // 
            this.audioOffsetS.DecimalPlaces = 2;
            this.audioOffsetS.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.audioOffsetS.Location = new System.Drawing.Point(81, 77);
            this.audioOffsetS.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.audioOffsetS.Minimum = new decimal(new int[] {
            99,
            0,
            0,
            -2147483648});
            this.audioOffsetS.Name = "audioOffsetS";
            this.audioOffsetS.Size = new System.Drawing.Size(52, 20);
            this.audioOffsetS.TabIndex = 1;
            this.audioOffsetS.ValueChanged += new System.EventHandler(this.audioOffsetS_ValueChanged);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newProjectToolStripMenuItem,
            this.openSongToolStripMenuItem,
            this.saveSongToolStripMenuItem,
            this.saveSongAsToolStripMenuItem,
            this.exportVideoToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newProjectToolStripMenuItem
            // 
            this.newProjectToolStripMenuItem.Name = "newProjectToolStripMenuItem";
            this.newProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.newProjectToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.newProjectToolStripMenuItem.Text = "Import song files...";
            this.newProjectToolStripMenuItem.Click += new System.EventHandler(this.newProjectToolStripMenuItem_Click);
            // 
            // openSongToolStripMenuItem
            // 
            this.openSongToolStripMenuItem.Name = "openSongToolStripMenuItem";
            this.openSongToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openSongToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.openSongToolStripMenuItem.Text = "Open song...";
            this.openSongToolStripMenuItem.Click += new System.EventHandler(this.openSongToolStripMenuItem_Click);
            // 
            // saveSongToolStripMenuItem
            // 
            this.saveSongToolStripMenuItem.Enabled = false;
            this.saveSongToolStripMenuItem.Name = "saveSongToolStripMenuItem";
            this.saveSongToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveSongToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.saveSongToolStripMenuItem.Text = "Save song";
            this.saveSongToolStripMenuItem.Click += new System.EventHandler(this.saveSongToolStripMenuItem_Click);
            // 
            // saveSongAsToolStripMenuItem
            // 
            this.saveSongAsToolStripMenuItem.Enabled = false;
            this.saveSongAsToolStripMenuItem.Name = "saveSongAsToolStripMenuItem";
            this.saveSongAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveSongAsToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.saveSongAsToolStripMenuItem.Text = "Save song as...";
            this.saveSongAsToolStripMenuItem.Click += new System.EventHandler(this.saveSongAsToolStripMenuItem_Click);
            // 
            // exportVideoToolStripMenuItem
            // 
            this.exportVideoToolStripMenuItem.Enabled = false;
            this.exportVideoToolStripMenuItem.Name = "exportVideoToolStripMenuItem";
            this.exportVideoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportVideoToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.exportVideoToolStripMenuItem.Text = "Export video...";
            this.exportVideoToolStripMenuItem.Click += new System.EventHandler(this.exportVideoToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.playbackToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(990, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // playbackToolStripMenuItem
            // 
            this.playbackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startStopToolStripMenuItem,
            this.resetPlaybackPositionToolStripMenuItem});
            this.playbackToolStripMenuItem.Enabled = false;
            this.playbackToolStripMenuItem.Name = "playbackToolStripMenuItem";
            this.playbackToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.playbackToolStripMenuItem.Text = "Playback";
            // 
            // startStopToolStripMenuItem
            // 
            this.startStopToolStripMenuItem.Name = "startStopToolStripMenuItem";
            this.startStopToolStripMenuItem.ShowShortcutKeys = false;
            this.startStopToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.startStopToolStripMenuItem.Text = "Start/Stop          Space";
            this.startStopToolStripMenuItem.Click += new System.EventHandler(this.startStopToolStripMenuItem_Click);
            // 
            // resetPlaybackPositionToolStripMenuItem
            // 
            this.resetPlaybackPositionToolStripMenuItem.Name = "resetPlaybackPositionToolStripMenuItem";
            this.resetPlaybackPositionToolStripMenuItem.ShowShortcutKeys = false;
            this.resetPlaybackPositionToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.resetPlaybackPositionToolStripMenuItem.Text = "Reset position           R";
            this.resetPlaybackPositionToolStripMenuItem.Click += new System.EventHandler(this.resetPositionToolStripMenuItem_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Viewport width";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 79);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "Audio offset";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(139, 79);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(12, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "s";
            // 
            // trackPropsPanel
            // 
            this.trackPropsPanel.Controls.Add(this.trackList);
            this.trackPropsPanel.Controls.Add(this.leftTrackPropsPanel);
            this.trackPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.trackPropsPanel.Location = new System.Drawing.Point(595, 24);
            this.trackPropsPanel.Name = "trackPropsPanel";
            this.trackPropsPanel.Size = new System.Drawing.Size(395, 10052);
            this.trackPropsPanel.TabIndex = 3;
            this.trackPropsPanel.Visible = false;
            this.trackPropsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.trackPropsPanel_Paint);
            this.trackPropsPanel.Enter += new System.EventHandler(this.trackPropsPanel_Enter);
            this.trackPropsPanel.Leave += new System.EventHandler(this.trackPropsPanel_Leave);
            // 
            // trackList
            // 
            this.trackList.AllowDrop = true;
            this.trackList.BackColor = System.Drawing.Color.Black;
            this.trackList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.trackColumn,
            this.normalColumn,
            this.hilitedColumn});
            this.trackList.ContextMenuStrip = this.trackListCM;
            this.trackList.Dock = System.Windows.Forms.DockStyle.Right;
            this.trackList.ForeColor = System.Drawing.Color.White;
            this.trackList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.trackList.HideSelection = false;
            this.trackList.Location = new System.Drawing.Point(1, 0);
            this.trackList.Name = "trackList";
            this.trackList.Size = new System.Drawing.Size(186, 10052);
            this.trackList.TabIndex = 0;
            this.trackList.UseCompatibleStateImageBehavior = false;
            this.trackList.View = System.Windows.Forms.View.Details;
            this.trackList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.trackList_ItemDrag);
            this.trackList.SelectedIndexChanged += new System.EventHandler(this.trackList_SelectedIndexChanged);
            this.trackList.DragDrop += new System.Windows.Forms.DragEventHandler(this.trackList_DragDrop);
            this.trackList.DragEnter += new System.Windows.Forms.DragEventHandler(this.trackList_DragEnter);
            this.trackList.DragOver += new System.Windows.Forms.DragEventHandler(this.trackList_DragOver);
            this.trackList.Enter += new System.EventHandler(this.trackList_Enter);
            // 
            // trackColumn
            // 
            this.trackColumn.Text = "Track";
            this.trackColumn.Width = 112;
            // 
            // normalColumn
            // 
            this.normalColumn.Text = "N";
            this.normalColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.normalColumn.Width = 35;
            // 
            // hilitedColumn
            // 
            this.hilitedColumn.Text = "H";
            this.hilitedColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.hilitedColumn.Width = 35;
            // 
            // trackListCM
            // 
            this.trackListCM.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.invertSelectionToolStripMenuItem,
            this.toolStripSeparator1,
            this.defaultPropertiesToolStripMenuItem});
            this.trackListCM.Name = "trackListContextMenu";
            this.trackListCM.Size = new System.Drawing.Size(211, 76);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // invertSelectionToolStripMenuItem
            // 
            this.invertSelectionToolStripMenuItem.Name = "invertSelectionToolStripMenuItem";
            this.invertSelectionToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.invertSelectionToolStripMenuItem.Text = "Invert Selection";
            this.invertSelectionToolStripMenuItem.Click += new System.EventHandler(this.invertSelectionToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // defaultPropertiesToolStripMenuItem
            // 
            this.defaultPropertiesToolStripMenuItem.Name = "defaultPropertiesToolStripMenuItem";
            this.defaultPropertiesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.defaultPropertiesToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.defaultPropertiesToolStripMenuItem.Text = "Default Properties";
            this.defaultPropertiesToolStripMenuItem.Click += new System.EventHandler(this.defaultPropertiesToolStripMenuItem_Click);
            // 
            // leftTrackPropsPanel
            // 
            this.leftTrackPropsPanel.Controls.Add(this.tabPage3);
            this.leftTrackPropsPanel.Controls.Add(this.tabPage1);
            this.leftTrackPropsPanel.Controls.Add(this.tabPage2);
            this.leftTrackPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.leftTrackPropsPanel.Location = new System.Drawing.Point(187, 0);
            this.leftTrackPropsPanel.Name = "leftTrackPropsPanel";
            this.leftTrackPropsPanel.SelectedIndex = 0;
            this.leftTrackPropsPanel.Size = new System.Drawing.Size(208, 10052);
            this.leftTrackPropsPanel.TabIndex = 14;
            // 
            // tabPage3
            // 
            this.tabPage3.AutoScroll = true;
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.defaultStyleBtn);
            this.tabPage3.Controls.Add(this.styleList);
            this.tabPage3.Controls.Add(this.lineStyleGroup);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(200, 10026);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Style";
            // 
            // defaultStyleBtn
            // 
            this.defaultStyleBtn.Location = new System.Drawing.Point(3, 3);
            this.defaultStyleBtn.Name = "defaultStyleBtn";
            this.defaultStyleBtn.Size = new System.Drawing.Size(179, 23);
            this.defaultStyleBtn.TabIndex = 11;
            this.defaultStyleBtn.Text = "Default Style";
            this.defaultStyleBtn.UseVisualStyleBackColor = true;
            this.defaultStyleBtn.Click += new System.EventHandler(this.defaultStyleBtn_Click);
            // 
            // styleList
            // 
            this.styleList.DisplayMember = "Name";
            this.styleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleList.FormattingEnabled = true;
            this.styleList.Location = new System.Drawing.Point(3, 44);
            this.styleList.Name = "styleList";
            this.styleList.Size = new System.Drawing.Size(125, 21);
            this.styleList.TabIndex = 8;
            this.styleList.ValueMember = "Value";
            this.styleList.SelectedIndexChanged += new System.EventHandler(this.styleList_SelectedIndexChanged);
            // 
            // lineStyleGroup
            // 
            this.lineStyleGroup.Controls.Add(this.groupBox3);
            this.lineStyleGroup.Controls.Add(this.blurredEdgeUd);
            this.lineStyleGroup.Controls.Add(this.simpleLineStylePanel);
            this.lineStyleGroup.Controls.Add(this.label18);
            this.lineStyleGroup.Controls.Add(this.label14);
            this.lineStyleGroup.Controls.Add(this.label13);
            this.lineStyleGroup.Controls.Add(this.lineWidthUpDown);
            this.lineStyleGroup.Controls.Add(this.lineStyleList);
            this.lineStyleGroup.Controls.Add(this.label17);
            this.lineStyleGroup.Controls.Add(this.label11);
            this.lineStyleGroup.Controls.Add(this.qnGapFillUd);
            this.lineStyleGroup.Controls.Add(this.label19);
            this.lineStyleGroup.Controls.Add(this.label12);
            this.lineStyleGroup.Location = new System.Drawing.Point(3, 72);
            this.lineStyleGroup.Name = "lineStyleGroup";
            this.lineStyleGroup.Size = new System.Drawing.Size(178, 342);
            this.lineStyleGroup.TabIndex = 10;
            this.lineStyleGroup.TabStop = false;
            this.lineStyleGroup.Text = "Line properties";
            this.lineStyleGroup.Visible = false;
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
            this.groupBox3.Location = new System.Drawing.Point(6, 128);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(166, 139);
            this.groupBox3.TabIndex = 12;
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
            this.shrinkingHlCb.CheckedChanged += new System.EventHandler(this.shriunkingHlCb_CheckedChanged);
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
            // blurredEdgeUd
            // 
            this.blurredEdgeUd.Location = new System.Drawing.Point(73, 102);
            this.blurredEdgeUd.Name = "blurredEdgeUd";
            this.blurredEdgeUd.Size = new System.Drawing.Size(47, 20);
            this.blurredEdgeUd.TabIndex = 19;
            this.blurredEdgeUd.ValueChanged += new System.EventHandler(this.blurredEdgeUd_ValueChanged);
            // 
            // simpleLineStylePanel
            // 
            this.simpleLineStylePanel.Controls.Add(this.shapePowerUD);
            this.simpleLineStylePanel.Controls.Add(this.fadeoutUd);
            this.simpleLineStylePanel.Controls.Add(this.label38);
            this.simpleLineStylePanel.Controls.Add(this.label15);
            this.simpleLineStylePanel.Controls.Add(this.label16);
            this.simpleLineStylePanel.Location = new System.Drawing.Point(3, 273);
            this.simpleLineStylePanel.Name = "simpleLineStylePanel";
            this.simpleLineStylePanel.Size = new System.Drawing.Size(162, 47);
            this.simpleLineStylePanel.TabIndex = 18;
            // 
            // shapePowerUD
            // 
            this.shapePowerUD.DecimalPlaces = 2;
            this.shapePowerUD.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.shapePowerUD.Location = new System.Drawing.Point(83, 24);
            this.shapePowerUD.Name = "shapePowerUD";
            this.shapePowerUD.Size = new System.Drawing.Size(47, 20);
            this.shapePowerUD.TabIndex = 18;
            this.shapePowerUD.ValueChanged += new System.EventHandler(this.shapePowerUD_ValueChanged);
            // 
            // fadeoutUd
            // 
            this.fadeoutUd.DecimalPlaces = 2;
            this.fadeoutUd.Location = new System.Drawing.Point(83, 3);
            this.fadeoutUd.Name = "fadeoutUd";
            this.fadeoutUd.Size = new System.Drawing.Size(47, 20);
            this.fadeoutUd.TabIndex = 3;
            this.fadeoutUd.ValueChanged += new System.EventHandler(this.fadeoutUd_ValueChanged);
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(7, 26);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(70, 13);
            this.label38.TabIndex = 15;
            this.label38.Text = "Shape power";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 5);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(76, 13);
            this.label15.TabIndex = 15;
            this.label15.Text = "Shape amount";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(136, 5);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(15, 13);
            this.label16.TabIndex = 17;
            this.label16.Text = "%";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(125, 104);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(33, 13);
            this.label18.TabIndex = 17;
            this.label18.Text = "pixels";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(125, 78);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(42, 13);
            this.label14.TabIndex = 17;
            this.label14.Text = "q-notes";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(126, 52);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(33, 13);
            this.label13.TabIndex = 17;
            this.label13.Text = "pixels";
            // 
            // lineWidthUpDown
            // 
            this.lineWidthUpDown.Location = new System.Drawing.Point(73, 50);
            this.lineWidthUpDown.Name = "lineWidthUpDown";
            this.lineWidthUpDown.Size = new System.Drawing.Size(47, 20);
            this.lineWidthUpDown.TabIndex = 1;
            this.lineWidthUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lineWidthUpDown.ValueChanged += new System.EventHandler(this.lineWidthUpDown_ValueChanged);
            // 
            // lineStyleList
            // 
            this.lineStyleList.DisplayMember = "Name";
            this.lineStyleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lineStyleList.FormattingEnabled = true;
            this.lineStyleList.Location = new System.Drawing.Point(73, 22);
            this.lineStyleList.Name = "lineStyleList";
            this.lineStyleList.Size = new System.Drawing.Size(95, 21);
            this.lineStyleList.TabIndex = 8;
            this.lineStyleList.ValueMember = "Value";
            this.lineStyleList.SelectedIndexChanged += new System.EventHandler(this.lineStyleList_SelectedIndexChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 25);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(30, 13);
            this.label17.TabIndex = 9;
            this.label17.Text = "Style";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(1, 52);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(55, 13);
            this.label11.TabIndex = 14;
            this.label11.Text = "Line width";
            // 
            // qnGapFillUd
            // 
            this.qnGapFillUd.DecimalPlaces = 1;
            this.qnGapFillUd.Location = new System.Drawing.Point(73, 76);
            this.qnGapFillUd.Name = "qnGapFillUd";
            this.qnGapFillUd.Size = new System.Drawing.Size(47, 20);
            this.qnGapFillUd.TabIndex = 2;
            this.qnGapFillUd.ValueChanged += new System.EventHandler(this.qnGapFillUd_ValueChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(1, 104);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(67, 13);
            this.label19.TabIndex = 15;
            this.label19.Text = "Blurred edge";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(1, 78);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(57, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "Fill gaps < ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Style";
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.defaultMtrlBtn);
            this.tabPage1.Controls.Add(this.alphaLbl);
            this.tabPage1.Controls.Add(this.transpSlider);
            this.tabPage1.Controls.Add(this.hueTb);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.hueSlider);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.transpTb);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(200, 10026);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Material";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label37);
            this.groupBox4.Controls.Add(this.label34);
            this.groupBox4.Controls.Add(this.label35);
            this.groupBox4.Controls.Add(this.texVScrollUD);
            this.groupBox4.Controls.Add(this.label33);
            this.groupBox4.Controls.Add(this.texUScrollUD);
            this.groupBox4.Controls.Add(this.tableLayoutPanel1);
            this.groupBox4.Controls.Add(this.texVTileCb);
            this.groupBox4.Controls.Add(this.texUTileCb);
            this.groupBox4.Controls.Add(this.texKeepAspectCb);
            this.groupBox4.Controls.Add(this.tileTexCb);
            this.groupBox4.Controls.Add(this.loadTexBtn);
            this.groupBox4.Controls.Add(this.unloadTexBtn);
            this.groupBox4.Controls.Add(this.pointSmpCb);
            this.groupBox4.Controls.Add(this.trackTexPb);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(1, 485);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(182, 397);
            this.groupBox4.TabIndex = 20;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Texture";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label37.Location = new System.Drawing.Point(117, 359);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(52, 13);
            this.label37.TabIndex = 19;
            this.label37.Text = "repeats/s";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(117, 339);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(52, 13);
            this.label34.TabIndex = 19;
            this.label34.Text = "repeats/s";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label35.Location = new System.Drawing.Point(7, 359);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(43, 13);
            this.label35.TabIndex = 19;
            this.label35.Text = "Scroll V";
            // 
            // texVScrollUD
            // 
            this.texVScrollUD.DecimalPlaces = 3;
            this.texVScrollUD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.texVScrollUD.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.texVScrollUD.Location = new System.Drawing.Point(56, 357);
            this.texVScrollUD.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.texVScrollUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.texVScrollUD.Name = "texVScrollUD";
            this.texVScrollUD.Size = new System.Drawing.Size(55, 20);
            this.texVScrollUD.TabIndex = 21;
            this.texVScrollUD.ValueChanged += new System.EventHandler(this.texVScrollUD_ValueChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(6, 339);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(44, 13);
            this.label33.TabIndex = 19;
            this.label33.Text = "Scroll U";
            // 
            // texUScrollUD
            // 
            this.texUScrollUD.DecimalPlaces = 3;
            this.texUScrollUD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.texUScrollUD.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.texUScrollUD.Location = new System.Drawing.Point(56, 337);
            this.texUScrollUD.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.texUScrollUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.texUScrollUD.Name = "texUScrollUD";
            this.texUScrollUD.Size = new System.Drawing.Size(55, 20);
            this.texUScrollUD.TabIndex = 20;
            this.texUScrollUD.ValueChanged += new System.EventHandler(this.texUScrollUD_ValueChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.label31, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label32, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label36, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.songAnchorLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.noteAnchorLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.screenAnchorLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 238);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(112, 93);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // label31
            // 
            this.label31.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(92, 6);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(14, 13);
            this.label31.TabIndex = 17;
            this.label31.Text = "V";
            // 
            // label32
            // 
            this.label32.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.Location = new System.Drawing.Point(69, 6);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(14, 13);
            this.label32.TabIndex = 17;
            this.label32.Text = "U";
            // 
            // label36
            // 
            this.label36.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label36.AutoSize = true;
            this.label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label36.Location = new System.Drawing.Point(12, 6);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(41, 13);
            this.label36.TabIndex = 17;
            this.label36.Text = "Anchor";
            // 
            // songAnchorLabel
            // 
            this.songAnchorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.songAnchorLabel.AutoSize = true;
            this.songAnchorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.songAnchorLabel.Location = new System.Drawing.Point(6, 69);
            this.songAnchorLabel.Name = "songAnchorLabel";
            this.songAnchorLabel.Size = new System.Drawing.Size(54, 21);
            this.songAnchorLabel.TabIndex = 17;
            this.songAnchorLabel.Text = "Song start";
            this.songAnchorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.songAnchorLabel.Click += new System.EventHandler(this.songAnchorLabel_Click);
            // 
            // noteAnchorLabel
            // 
            this.noteAnchorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.noteAnchorLabel.AutoSize = true;
            this.noteAnchorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noteAnchorLabel.Location = new System.Drawing.Point(6, 25);
            this.noteAnchorLabel.Name = "noteAnchorLabel";
            this.noteAnchorLabel.Size = new System.Drawing.Size(54, 19);
            this.noteAnchorLabel.TabIndex = 17;
            this.noteAnchorLabel.Text = "Note";
            this.noteAnchorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.noteAnchorLabel.Click += new System.EventHandler(this.noteAnchorLabel_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.songAnchorRb);
            this.panel2.Controls.Add(this.screenUAnchorRb);
            this.panel2.Controls.Add(this.noteUAnchorRb);
            this.panel2.Location = new System.Drawing.Point(66, 25);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.tableLayoutPanel1.SetRowSpan(this.panel2, 3);
            this.panel2.Size = new System.Drawing.Size(20, 65);
            this.panel2.TabIndex = 20;
            // 
            // songAnchorRb
            // 
            this.songAnchorRb.AutoSize = true;
            this.songAnchorRb.Location = new System.Drawing.Point(3, 48);
            this.songAnchorRb.Name = "songAnchorRb";
            this.songAnchorRb.Size = new System.Drawing.Size(14, 13);
            this.songAnchorRb.TabIndex = 24;
            this.songAnchorRb.TabStop = true;
            this.songAnchorRb.UseVisualStyleBackColor = true;
            this.songAnchorRb.Click += new System.EventHandler(this.songAnchorRb_Click);
            // 
            // screenUAnchorRb
            // 
            this.screenUAnchorRb.AutoSize = true;
            this.screenUAnchorRb.Location = new System.Drawing.Point(3, 25);
            this.screenUAnchorRb.Name = "screenUAnchorRb";
            this.screenUAnchorRb.Size = new System.Drawing.Size(14, 13);
            this.screenUAnchorRb.TabIndex = 22;
            this.screenUAnchorRb.TabStop = true;
            this.screenUAnchorRb.UseVisualStyleBackColor = true;
            this.screenUAnchorRb.Click += new System.EventHandler(this.screenUAnchorRb_Click);
            // 
            // noteUAnchorRb
            // 
            this.noteUAnchorRb.AutoSize = true;
            this.noteUAnchorRb.Location = new System.Drawing.Point(3, 3);
            this.noteUAnchorRb.Name = "noteUAnchorRb";
            this.noteUAnchorRb.Size = new System.Drawing.Size(14, 13);
            this.noteUAnchorRb.TabIndex = 20;
            this.noteUAnchorRb.TabStop = true;
            this.noteUAnchorRb.UseVisualStyleBackColor = true;
            this.noteUAnchorRb.Click += new System.EventHandler(this.noteUAnchorRb_Click);
            // 
            // screenAnchorLabel
            // 
            this.screenAnchorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.screenAnchorLabel.AutoSize = true;
            this.screenAnchorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.screenAnchorLabel.Location = new System.Drawing.Point(6, 47);
            this.screenAnchorLabel.Name = "screenAnchorLabel";
            this.screenAnchorLabel.Size = new System.Drawing.Size(54, 19);
            this.screenAnchorLabel.TabIndex = 17;
            this.screenAnchorLabel.Text = "Screen";
            this.screenAnchorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.screenAnchorLabel.Click += new System.EventHandler(this.screenAnchorLabel_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.screenVAnchorRb);
            this.panel1.Controls.Add(this.noteVAnchorRb);
            this.panel1.Location = new System.Drawing.Point(89, 25);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.tableLayoutPanel1.SetRowSpan(this.panel1, 3);
            this.panel1.Size = new System.Drawing.Size(20, 65);
            this.panel1.TabIndex = 21;
            // 
            // screenVAnchorRb
            // 
            this.screenVAnchorRb.AutoSize = true;
            this.screenVAnchorRb.Location = new System.Drawing.Point(3, 25);
            this.screenVAnchorRb.Name = "screenVAnchorRb";
            this.screenVAnchorRb.Size = new System.Drawing.Size(14, 13);
            this.screenVAnchorRb.TabIndex = 23;
            this.screenVAnchorRb.TabStop = true;
            this.screenVAnchorRb.UseVisualStyleBackColor = true;
            this.screenVAnchorRb.Click += new System.EventHandler(this.screenVAnchorRb_Click);
            // 
            // noteVAnchorRb
            // 
            this.noteVAnchorRb.AutoSize = true;
            this.noteVAnchorRb.Location = new System.Drawing.Point(3, 3);
            this.noteVAnchorRb.Name = "noteVAnchorRb";
            this.noteVAnchorRb.Size = new System.Drawing.Size(14, 13);
            this.noteVAnchorRb.TabIndex = 21;
            this.noteVAnchorRb.TabStop = true;
            this.noteVAnchorRb.UseVisualStyleBackColor = true;
            this.noteVAnchorRb.Click += new System.EventHandler(this.noteVAnchorRb_Click);
            // 
            // texVTileCb
            // 
            this.texVTileCb.AutoSize = true;
            this.texVTileCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.texVTileCb.Location = new System.Drawing.Point(95, 195);
            this.texVTileCb.Name = "texVTileCb";
            this.texVTileCb.Size = new System.Drawing.Size(33, 17);
            this.texVTileCb.TabIndex = 17;
            this.texVTileCb.Text = "V";
            this.texVTileCb.UseVisualStyleBackColor = true;
            this.texVTileCb.CheckedChanged += new System.EventHandler(this.texVTileCb_CheckedChanged);
            // 
            // texUTileCb
            // 
            this.texUTileCb.AutoSize = true;
            this.texUTileCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.texUTileCb.Location = new System.Drawing.Point(56, 195);
            this.texUTileCb.Name = "texUTileCb";
            this.texUTileCb.Size = new System.Drawing.Size(34, 17);
            this.texUTileCb.TabIndex = 16;
            this.texUTileCb.Text = "U";
            this.texUTileCb.UseVisualStyleBackColor = true;
            this.texUTileCb.CheckedChanged += new System.EventHandler(this.texUTileCb_CheckedChanged);
            // 
            // texKeepAspectCb
            // 
            this.texKeepAspectCb.AutoSize = true;
            this.texKeepAspectCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.texKeepAspectCb.Location = new System.Drawing.Point(7, 216);
            this.texKeepAspectCb.Margin = new System.Windows.Forms.Padding(2);
            this.texKeepAspectCb.Name = "texKeepAspectCb";
            this.texKeepAspectCb.Size = new System.Drawing.Size(109, 17);
            this.texKeepAspectCb.TabIndex = 15;
            this.texKeepAspectCb.Text = "Keep aspect ratio";
            this.texKeepAspectCb.UseVisualStyleBackColor = true;
            this.texKeepAspectCb.CheckedChanged += new System.EventHandler(this.texKeepAspect_CheckedChanged);
            // 
            // tileTexCb
            // 
            this.tileTexCb.AutoSize = true;
            this.tileTexCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tileTexCb.Location = new System.Drawing.Point(8, 195);
            this.tileTexCb.Margin = new System.Windows.Forms.Padding(2);
            this.tileTexCb.Name = "tileTexCb";
            this.tileTexCb.Size = new System.Drawing.Size(43, 17);
            this.tileTexCb.TabIndex = 15;
            this.tileTexCb.Text = "Tile";
            this.tileTexCb.UseVisualStyleBackColor = true;
            this.tileTexCb.CheckedChanged += new System.EventHandler(this.tileTexCb_CheckedChanged);
            // 
            // loadTexBtn
            // 
            this.loadTexBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadTexBtn.Location = new System.Drawing.Point(5, 18);
            this.loadTexBtn.Name = "loadTexBtn";
            this.loadTexBtn.Size = new System.Drawing.Size(75, 23);
            this.loadTexBtn.TabIndex = 12;
            this.loadTexBtn.Text = "&Load texture";
            this.loadTexBtn.UseVisualStyleBackColor = true;
            this.loadTexBtn.Click += new System.EventHandler(this.textureLoadBtn_Click);
            // 
            // unloadTexBtn
            // 
            this.unloadTexBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unloadTexBtn.Location = new System.Drawing.Point(92, 18);
            this.unloadTexBtn.Name = "unloadTexBtn";
            this.unloadTexBtn.Size = new System.Drawing.Size(86, 23);
            this.unloadTexBtn.TabIndex = 12;
            this.unloadTexBtn.Text = "&Unload texture";
            this.unloadTexBtn.UseVisualStyleBackColor = true;
            this.unloadTexBtn.Click += new System.EventHandler(this.unloadTexBtn_Click);
            // 
            // pointSmpCb
            // 
            this.pointSmpCb.AutoSize = true;
            this.pointSmpCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pointSmpCb.Location = new System.Drawing.Point(8, 174);
            this.pointSmpCb.Margin = new System.Windows.Forms.Padding(2);
            this.pointSmpCb.Name = "pointSmpCb";
            this.pointSmpCb.Size = new System.Drawing.Size(94, 17);
            this.pointSmpCb.TabIndex = 14;
            this.pointSmpCb.Text = "Point sampling";
            this.pointSmpCb.UseVisualStyleBackColor = true;
            this.pointSmpCb.CheckedChanged += new System.EventHandler(this.pointSmpCb_CheckedChanged);
            // 
            // trackTexPb
            // 
            this.trackTexPb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.trackTexPb.Location = new System.Drawing.Point(5, 47);
            this.trackTexPb.Name = "trackTexPb";
            this.trackTexPb.Size = new System.Drawing.Size(173, 121);
            this.trackTexPb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.trackTexPb.TabIndex = 13;
            this.trackTexPb.TabStop = false;
            // 
            // defaultMtrlBtn
            // 
            this.defaultMtrlBtn.Location = new System.Drawing.Point(2, 6);
            this.defaultMtrlBtn.Name = "defaultMtrlBtn";
            this.defaultMtrlBtn.Size = new System.Drawing.Size(179, 23);
            this.defaultMtrlBtn.TabIndex = 11;
            this.defaultMtrlBtn.Text = "Default Material";
            this.defaultMtrlBtn.UseVisualStyleBackColor = true;
            this.defaultMtrlBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // alphaLbl
            // 
            this.alphaLbl.AutoSize = true;
            this.alphaLbl.Location = new System.Drawing.Point(5, 32);
            this.alphaLbl.Name = "alphaLbl";
            this.alphaLbl.Size = new System.Drawing.Size(43, 13);
            this.alphaLbl.TabIndex = 7;
            this.alphaLbl.Text = "Opacity";
            // 
            // transpSlider
            // 
            this.transpSlider.BackColor = System.Drawing.SystemColors.Control;
            this.transpSlider.Cursor = System.Windows.Forms.Cursors.Default;
            this.transpSlider.LargeChange = 10;
            this.transpSlider.Location = new System.Drawing.Point(5, 49);
            this.transpSlider.Maximum = 100;
            this.transpSlider.Name = "transpSlider";
            this.transpSlider.Size = new System.Drawing.Size(129, 45);
            this.transpSlider.TabIndex = 1;
            this.transpSlider.TickFrequency = 10;
            this.transpSlider.Value = 50;
            this.transpSlider.Scroll += new System.EventHandler(this.transpSlider_Scroll);
            // 
            // hueTb
            // 
            this.hueTb.Location = new System.Drawing.Point(137, 113);
            this.hueTb.Name = "hueTb";
            this.hueTb.Size = new System.Drawing.Size(38, 20);
            this.hueTb.TabIndex = 4;
            this.hueTb.Text = "notset";
            this.hueTb.TextChanged += new System.EventHandler(this.hueTb_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.normalLumTb);
            this.groupBox1.Controls.Add(this.normalLumSlider);
            this.groupBox1.Controls.Add(this.label);
            this.groupBox1.Controls.Add(this.normalSatTb);
            this.groupBox1.Controls.Add(this.normalSatSlider);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(-1, 164);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(182, 154);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Normal notes";
            // 
            // normalLumTb
            // 
            this.normalLumTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.normalLumTb.Location = new System.Drawing.Point(141, 105);
            this.normalLumTb.Name = "normalLumTb";
            this.normalLumTb.Size = new System.Drawing.Size(38, 20);
            this.normalLumTb.TabIndex = 8;
            this.normalLumTb.Text = "notset";
            this.normalLumTb.TextChanged += new System.EventHandler(this.normalLumTb_TextChanged);
            // 
            // normalLumSlider
            // 
            this.normalLumSlider.Cursor = System.Windows.Forms.Cursors.Default;
            this.normalLumSlider.LargeChange = 10;
            this.normalLumSlider.Location = new System.Drawing.Point(6, 105);
            this.normalLumSlider.Maximum = 200;
            this.normalLumSlider.Name = "normalLumSlider";
            this.normalLumSlider.Size = new System.Drawing.Size(129, 45);
            this.normalLumSlider.TabIndex = 7;
            this.normalLumSlider.TickFrequency = 10;
            this.normalLumSlider.Value = 50;
            this.normalLumSlider.Scroll += new System.EventHandler(this.normalLumSlider_Scroll);
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(6, 89);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(56, 13);
            this.label.TabIndex = 13;
            this.label.Text = "Brightness";
            // 
            // normalSatTb
            // 
            this.normalSatTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.normalSatTb.Location = new System.Drawing.Point(141, 41);
            this.normalSatTb.Name = "normalSatTb";
            this.normalSatTb.Size = new System.Drawing.Size(38, 20);
            this.normalSatTb.TabIndex = 6;
            this.normalSatTb.Text = "notset";
            this.normalSatTb.TextChanged += new System.EventHandler(this.normalSatTb_TextChanged);
            // 
            // normalSatSlider
            // 
            this.normalSatSlider.Cursor = System.Windows.Forms.Cursors.Default;
            this.normalSatSlider.LargeChange = 10;
            this.normalSatSlider.Location = new System.Drawing.Point(6, 41);
            this.normalSatSlider.Maximum = 200;
            this.normalSatSlider.Name = "normalSatSlider";
            this.normalSatSlider.Size = new System.Drawing.Size(129, 45);
            this.normalSatSlider.TabIndex = 5;
            this.normalSatSlider.TickFrequency = 10;
            this.normalSatSlider.Value = 50;
            this.normalSatSlider.Scroll += new System.EventHandler(this.normalSatSlider_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Saturation";
            // 
            // hueSlider
            // 
            this.hueSlider.BackColor = System.Drawing.SystemColors.Control;
            this.hueSlider.Cursor = System.Windows.Forms.Cursors.Default;
            this.hueSlider.LargeChange = 10;
            this.hueSlider.Location = new System.Drawing.Point(5, 112);
            this.hueSlider.Maximum = 100;
            this.hueSlider.Name = "hueSlider";
            this.hueSlider.Size = new System.Drawing.Size(129, 45);
            this.hueSlider.TabIndex = 3;
            this.hueSlider.TickFrequency = 10;
            this.hueSlider.Value = 50;
            this.hueSlider.Scroll += new System.EventHandler(this.hueSlider_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Hue";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.hiliteLumTb);
            this.groupBox2.Controls.Add(this.hiliteLumSlider);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.hiliteSatTb);
            this.groupBox2.Controls.Add(this.hiliteSatSlider);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(-1, 325);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(182, 155);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Highlighted notes";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // hiliteLumTb
            // 
            this.hiliteLumTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiliteLumTb.Location = new System.Drawing.Point(141, 105);
            this.hiliteLumTb.Name = "hiliteLumTb";
            this.hiliteLumTb.Size = new System.Drawing.Size(38, 20);
            this.hiliteLumTb.TabIndex = 12;
            this.hiliteLumTb.Text = "notset";
            this.hiliteLumTb.TextChanged += new System.EventHandler(this.hiliteLumTb_TextChanged);
            // 
            // hiliteLumSlider
            // 
            this.hiliteLumSlider.Cursor = System.Windows.Forms.Cursors.Default;
            this.hiliteLumSlider.LargeChange = 10;
            this.hiliteLumSlider.Location = new System.Drawing.Point(6, 105);
            this.hiliteLumSlider.Maximum = 200;
            this.hiliteLumSlider.Name = "hiliteLumSlider";
            this.hiliteLumSlider.Size = new System.Drawing.Size(129, 45);
            this.hiliteLumSlider.TabIndex = 11;
            this.hiliteLumSlider.TickFrequency = 10;
            this.hiliteLumSlider.Value = 50;
            this.hiliteLumSlider.Scroll += new System.EventHandler(this.hiliteLumSlider_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Brightness";
            // 
            // hiliteSatTb
            // 
            this.hiliteSatTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiliteSatTb.Location = new System.Drawing.Point(141, 41);
            this.hiliteSatTb.Name = "hiliteSatTb";
            this.hiliteSatTb.Size = new System.Drawing.Size(38, 20);
            this.hiliteSatTb.TabIndex = 10;
            this.hiliteSatTb.Text = "notset";
            this.hiliteSatTb.TextChanged += new System.EventHandler(this.hiliteSatTb_TextChanged);
            // 
            // hiliteSatSlider
            // 
            this.hiliteSatSlider.Cursor = System.Windows.Forms.Cursors.Default;
            this.hiliteSatSlider.LargeChange = 10;
            this.hiliteSatSlider.Location = new System.Drawing.Point(6, 41);
            this.hiliteSatSlider.Maximum = 200;
            this.hiliteSatSlider.Name = "hiliteSatSlider";
            this.hiliteSatSlider.Size = new System.Drawing.Size(129, 45);
            this.hiliteSatSlider.TabIndex = 9;
            this.hiliteSatSlider.TickFrequency = 10;
            this.hiliteSatSlider.Value = 50;
            this.hiliteSatSlider.Scroll += new System.EventHandler(this.hiliteSatSlider_Scroll);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Saturation";
            // 
            // transpTb
            // 
            this.transpTb.Location = new System.Drawing.Point(137, 49);
            this.transpTb.Name = "transpTb";
            this.transpTb.Size = new System.Drawing.Size(38, 20);
            this.transpTb.TabIndex = 2;
            this.transpTb.Text = "notset";
            this.transpTb.TextChanged += new System.EventHandler(this.transpTb_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.defaultLightBtn);
            this.tabPage2.Controls.Add(this.lightPanel);
            this.tabPage2.Controls.Add(this.globalLightCb);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(200, 10026);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Light";
            // 
            // defaultLightBtn
            // 
            this.defaultLightBtn.Location = new System.Drawing.Point(3, 3);
            this.defaultLightBtn.Name = "defaultLightBtn";
            this.defaultLightBtn.Size = new System.Drawing.Size(179, 23);
            this.defaultLightBtn.TabIndex = 12;
            this.defaultLightBtn.Text = "Default Light";
            this.defaultLightBtn.UseVisualStyleBackColor = true;
            this.defaultLightBtn.Click += new System.EventHandler(this.defaultLightBtn_Click);
            // 
            // lightPanel
            // 
            this.lightPanel.Controls.Add(this.lightDirzTb);
            this.lightPanel.Controls.Add(this.somelabel);
            this.lightPanel.Controls.Add(this.specularGb);
            this.lightPanel.Controls.Add(this.lightDiryTb);
            this.lightPanel.Controls.Add(this.label24);
            this.lightPanel.Controls.Add(this.label26);
            this.lightPanel.Controls.Add(this.label25);
            this.lightPanel.Controls.Add(this.lightDirxTb);
            this.lightPanel.Enabled = false;
            this.lightPanel.Location = new System.Drawing.Point(0, 63);
            this.lightPanel.Name = "lightPanel";
            this.lightPanel.Size = new System.Drawing.Size(180, 176);
            this.lightPanel.TabIndex = 5;
            // 
            // lightDirzTb
            // 
            this.lightDirzTb.Location = new System.Drawing.Point(140, 25);
            this.lightDirzTb.Name = "lightDirzTb";
            this.lightDirzTb.Size = new System.Drawing.Size(37, 20);
            this.lightDirzTb.TabIndex = 4;
            // 
            // somelabel
            // 
            this.somelabel.AutoSize = true;
            this.somelabel.Location = new System.Drawing.Point(123, 28);
            this.somelabel.Name = "somelabel";
            this.somelabel.Size = new System.Drawing.Size(17, 13);
            this.somelabel.TabIndex = 2;
            this.somelabel.Text = "Z:";
            // 
            // specularGb
            // 
            this.specularGb.Controls.Add(this.specFovUd);
            this.specularGb.Controls.Add(this.specPowUd);
            this.specularGb.Controls.Add(this.specAmountUd);
            this.specularGb.Controls.Add(this.label23);
            this.specularGb.Controls.Add(this.label22);
            this.specularGb.Controls.Add(this.label28);
            this.specularGb.Controls.Add(this.label21);
            this.specularGb.Location = new System.Drawing.Point(3, 68);
            this.specularGb.Name = "specularGb";
            this.specularGb.Size = new System.Drawing.Size(170, 103);
            this.specularGb.TabIndex = 3;
            this.specularGb.TabStop = false;
            this.specularGb.Text = "Specular";
            // 
            // specFovUd
            // 
            this.specFovUd.Location = new System.Drawing.Point(55, 75);
            this.specFovUd.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.specFovUd.Name = "specFovUd";
            this.specFovUd.Size = new System.Drawing.Size(59, 20);
            this.specFovUd.TabIndex = 3;
            this.specFovUd.ValueChanged += new System.EventHandler(this.specFovUd_ValueChanged);
            // 
            // specPowUd
            // 
            this.specPowUd.Location = new System.Drawing.Point(55, 49);
            this.specPowUd.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.specPowUd.Name = "specPowUd";
            this.specPowUd.Size = new System.Drawing.Size(59, 20);
            this.specPowUd.TabIndex = 3;
            this.specPowUd.ValueChanged += new System.EventHandler(this.specPowUd_ValueChanged);
            // 
            // specAmountUd
            // 
            this.specAmountUd.DecimalPlaces = 2;
            this.specAmountUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.specAmountUd.Location = new System.Drawing.Point(55, 23);
            this.specAmountUd.Name = "specAmountUd";
            this.specAmountUd.Size = new System.Drawing.Size(59, 20);
            this.specAmountUd.TabIndex = 3;
            this.specAmountUd.ValueChanged += new System.EventHandler(this.specAmountUd_ValueChanged);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 25);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(43, 13);
            this.label23.TabIndex = 2;
            this.label23.Text = "Amount";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(6, 51);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(37, 13);
            this.label22.TabIndex = 2;
            this.label22.Text = "Power";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(120, 77);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(45, 13);
            this.label28.TabIndex = 2;
            this.label28.Text = "degrees";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(6, 77);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(28, 13);
            this.label21.TabIndex = 2;
            this.label21.Text = "FOV";
            // 
            // lightDiryTb
            // 
            this.lightDiryTb.Location = new System.Drawing.Point(80, 25);
            this.lightDiryTb.Name = "lightDiryTb";
            this.lightDiryTb.Size = new System.Drawing.Size(37, 20);
            this.lightDiryTb.TabIndex = 4;
            this.lightDiryTb.TextChanged += new System.EventHandler(this.lightDiryTb_TextChanged);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(3, 9);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(49, 13);
            this.label24.TabIndex = 2;
            this.label24.Text = "Direction";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(63, 28);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(17, 13);
            this.label26.TabIndex = 2;
            this.label26.Text = "Y:";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(3, 28);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(17, 13);
            this.label25.TabIndex = 2;
            this.label25.Text = "X:";
            // 
            // lightDirxTb
            // 
            this.lightDirxTb.Location = new System.Drawing.Point(20, 25);
            this.lightDirxTb.Name = "lightDirxTb";
            this.lightDirxTb.Size = new System.Drawing.Size(37, 20);
            this.lightDirxTb.TabIndex = 4;
            this.lightDirxTb.TextChanged += new System.EventHandler(this.lightDirxTb_TextChanged);
            // 
            // globalLightCb
            // 
            this.globalLightCb.AutoSize = true;
            this.globalLightCb.Checked = true;
            this.globalLightCb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.globalLightCb.Location = new System.Drawing.Point(3, 39);
            this.globalLightCb.Name = "globalLightCb";
            this.globalLightCb.Size = new System.Drawing.Size(115, 17);
            this.globalLightCb.TabIndex = 0;
            this.globalLightCb.Text = "Use global settings";
            this.globalLightCb.UseVisualStyleBackColor = true;
            this.globalLightCb.CheckedChanged += new System.EventHandler(this.globalLightCb_CheckedChanged);
            // 
            // textureBrowseBtn
            // 
            this.textureBrowseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textureBrowseBtn.Location = new System.Drawing.Point(-1954, 10055);
            this.textureBrowseBtn.Name = "textureBrowseBtn";
            this.textureBrowseBtn.Size = new System.Drawing.Size(26, 20);
            this.textureBrowseBtn.TabIndex = 11;
            this.textureBrowseBtn.Text = "...";
            this.textureBrowseBtn.UseVisualStyleBackColor = true;
            this.textureBrowseBtn.Visible = false;
            this.textureBrowseBtn.Click += new System.EventHandler(this.textureBrowseBtn_Click);
            // 
            // texPathTb
            // 
            this.texPathTb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.texPathTb.Location = new System.Drawing.Point(-2066, 10056);
            this.texPathTb.Name = "texPathTb";
            this.texPathTb.Size = new System.Drawing.Size(123, 20);
            this.texPathTb.TabIndex = 10;
            this.texPathTb.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2069, 10040);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Texture";
            this.label4.Visible = false;
            // 
            // resetDefaultBtn
            // 
            this.resetDefaultBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetDefaultBtn.Location = new System.Drawing.Point(336, 32767);
            this.resetDefaultBtn.Name = "resetDefaultBtn";
            this.resetDefaultBtn.Size = new System.Drawing.Size(170, 23);
            this.resetDefaultBtn.TabIndex = 13;
            this.resetDefaultBtn.Text = "Reset defaults to default";
            this.resetDefaultBtn.UseVisualStyleBackColor = true;
            this.resetDefaultBtn.Visible = false;
            // 
            // setDefaultBtn
            // 
            this.setDefaultBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.setDefaultBtn.Location = new System.Drawing.Point(334, 32767);
            this.setDefaultBtn.Name = "setDefaultBtn";
            this.setDefaultBtn.Size = new System.Drawing.Size(170, 23);
            this.setDefaultBtn.TabIndex = 12;
            this.setDefaultBtn.Text = "Set as default";
            this.setDefaultBtn.UseVisualStyleBackColor = true;
            this.setDefaultBtn.Visible = false;
            // 
            // openTextureDlg
            // 
            this.openTextureDlg.FileName = "openFileDialog1";
            this.openTextureDlg.Filter = "Textures|*.jpg;*.png;*.gif";
            // 
            // trackPropsCb
            // 
            this.trackPropsCb.Appearance = System.Windows.Forms.Appearance.Button;
            this.trackPropsCb.AutoSize = true;
            this.trackPropsCb.FlatAppearance.CheckedBackColor = System.Drawing.Color.White;
            this.trackPropsCb.Location = new System.Drawing.Point(99, 0);
            this.trackPropsCb.Name = "trackPropsCb";
            this.trackPropsCb.Size = new System.Drawing.Size(94, 23);
            this.trackPropsCb.TabIndex = 16;
            this.trackPropsCb.Text = "&Track properties";
            this.trackPropsCb.UseVisualStyleBackColor = true;
            this.trackPropsCb.CheckedChanged += new System.EventHandler(this.trackPropsCb_CheckedChanged);
            // 
            // songPropsPanel
            // 
            this.songPropsPanel.Controls.Add(this.defaultPitchesBtn);
            this.songPropsPanel.Controls.Add(this.minPitchUd);
            this.songPropsPanel.Controls.Add(this.maxPitchUd);
            this.songPropsPanel.Controls.Add(this.label10);
            this.songPropsPanel.Controls.Add(this.label8);
            this.songPropsPanel.Controls.Add(this.hnotelabel);
            this.songPropsPanel.Controls.Add(this.label9);
            this.songPropsPanel.Controls.Add(this.audioOffsetS);
            this.songPropsPanel.Controls.Add(this.upDownVpWidth);
            this.songPropsPanel.Controls.Add(this.label7);
            this.songPropsPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.songPropsPanel.Location = new System.Drawing.Point(0, 24);
            this.songPropsPanel.Name = "songPropsPanel";
            this.songPropsPanel.Size = new System.Drawing.Size(209, 10052);
            this.songPropsPanel.TabIndex = 17;
            this.songPropsPanel.Visible = false;
            // 
            // defaultPitchesBtn
            // 
            this.defaultPitchesBtn.Location = new System.Drawing.Point(12, 172);
            this.defaultPitchesBtn.Name = "defaultPitchesBtn";
            this.defaultPitchesBtn.Size = new System.Drawing.Size(108, 23);
            this.defaultPitchesBtn.TabIndex = 18;
            this.defaultPitchesBtn.Text = "Default pitches";
            this.defaultPitchesBtn.UseVisualStyleBackColor = true;
            this.defaultPitchesBtn.Click += new System.EventHandler(this.defaultPitchesBtn_Click);
            // 
            // minPitchUd
            // 
            this.minPitchUd.Location = new System.Drawing.Point(71, 146);
            this.minPitchUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.minPitchUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.minPitchUd.Name = "minPitchUd";
            this.minPitchUd.Size = new System.Drawing.Size(52, 20);
            this.minPitchUd.TabIndex = 17;
            this.minPitchUd.ValueChanged += new System.EventHandler(this.minPitchUd_ValueChanged);
            // 
            // maxPitchUd
            // 
            this.maxPitchUd.Location = new System.Drawing.Point(71, 120);
            this.maxPitchUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.maxPitchUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.maxPitchUd.Name = "maxPitchUd";
            this.maxPitchUd.Size = new System.Drawing.Size(52, 20);
            this.maxPitchUd.TabIndex = 17;
            this.maxPitchUd.ValueChanged += new System.EventHandler(this.maxPitchUd_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 148);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Min pitch";
            // 
            // hnotelabel
            // 
            this.hnotelabel.AutoSize = true;
            this.hnotelabel.Location = new System.Drawing.Point(12, 122);
            this.hnotelabel.Name = "hnotelabel";
            this.hnotelabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.hnotelabel.Size = new System.Drawing.Size(53, 13);
            this.hnotelabel.TabIndex = 3;
            this.hnotelabel.Text = "Max pitch";
            // 
            // upDownVpWidth
            // 
            this.upDownVpWidth.AutoSize = true;
            this.upDownVpWidth.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.upDownVpWidth.Decimals = 3;
            this.upDownVpWidth.Decimals2 = 2;
            this.upDownVpWidth.ExpBase = 2D;
            this.upDownVpWidth.Location = new System.Drawing.Point(12, 28);
            this.upDownVpWidth.Margin = new System.Windows.Forms.Padding(4);
            this.upDownVpWidth.Max = 10D;
            this.upDownVpWidth.Min = 0D;
            this.upDownVpWidth.Name = "upDownVpWidth";
            this.upDownVpWidth.Size = new System.Drawing.Size(180, 48);
            this.upDownVpWidth.TabIndex = 16;
            this.upDownVpWidth.TbWidth = 50;
            this.upDownVpWidth.TickFreq = 1D;
            this.upDownVpWidth.Value = 16D;
            this.upDownVpWidth.ValueChanged += new System.EventHandler(this.upDownVpWidth_ValueChanged);
            // 
            // songPropsCb
            // 
            this.songPropsCb.Appearance = System.Windows.Forms.Appearance.Button;
            this.songPropsCb.AutoSize = true;
            this.songPropsCb.Location = new System.Drawing.Point(3, 0);
            this.songPropsCb.Name = "songPropsCb";
            this.songPropsCb.Size = new System.Drawing.Size(91, 23);
            this.songPropsCb.TabIndex = 18;
            this.songPropsCb.Text = "&Song properties";
            this.songPropsCb.UseVisualStyleBackColor = true;
            this.songPropsCb.CheckedChanged += new System.EventHandler(this.songPropsCb_CheckedChanged);
            // 
            // propsTogglePanel
            // 
            this.propsTogglePanel.Controls.Add(this.songPropsCb);
            this.propsTogglePanel.Controls.Add(this.trackPropsCb);
            this.propsTogglePanel.Enabled = false;
            this.propsTogglePanel.Location = new System.Drawing.Point(235, 0);
            this.propsTogglePanel.Name = "propsTogglePanel";
            this.propsTogglePanel.Size = new System.Drawing.Size(196, 24);
            this.propsTogglePanel.TabIndex = 19;
            // 
            // songPanelBindingSource
            // 
            this.songPanelBindingSource.DataSource = typeof(Visual_Music.SongPanel);
            // 
            // serviceContainerBindingSource
            // 
            this.serviceContainerBindingSource.DataSource = typeof(WinFormsGraphicsDevice.ServiceContainer);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1007, 407);
            this.Controls.Add(this.propsTogglePanel);
            this.Controls.Add(this.songPropsPanel);
            this.Controls.Add(this.resetDefaultBtn);
            this.Controls.Add(this.trackPropsPanel);
            this.Controls.Add(this.textureBrowseBtn);
            this.Controls.Add(this.setDefaultBtn);
            this.Controls.Add(this.texPathTb);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Visual Music";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.audioOffsetS)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.trackPropsPanel.ResumeLayout(false);
            this.trackListCM.ResumeLayout(false);
            this.leftTrackPropsPanel.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.lineStyleGroup.ResumeLayout(false);
            this.lineStyleGroup.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hlSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blurredEdgeUd)).EndInit();
            this.simpleLineStylePanel.ResumeLayout(false);
            this.simpleLineStylePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shapePowerUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeoutUd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lineWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qnGapFillUd)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texVScrollUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.texUScrollUD)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackTexPb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.transpSlider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalLumSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normalSatSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueSlider)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hiliteLumSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hiliteSatSlider)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.lightPanel.ResumeLayout(false);
            this.lightPanel.PerformLayout();
            this.specularGb.ResumeLayout(false);
            this.specularGb.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.specFovUd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.specPowUd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.specAmountUd)).EndInit();
            this.songPropsPanel.ResumeLayout(false);
            this.songPropsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minPitchUd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxPitchUd)).EndInit();
            this.propsTogglePanel.ResumeLayout(false);
            this.propsTogglePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.songPanelBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.serviceContainerBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog openProjDialog;
		private System.Windows.Forms.SaveFileDialog saveProjDialog;
		private System.Windows.Forms.SaveFileDialog saveVideoDlg;
		private System.Windows.Forms.NumericUpDown audioOffsetS;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportVideoToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Panel trackPropsPanel;
		private System.Windows.Forms.TrackBar transpSlider;
		private System.Windows.Forms.Label alphaLbl;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox transpTb;
		private System.Windows.Forms.TextBox normalLumTb;
		private System.Windows.Forms.TrackBar normalLumSlider;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.TextBox normalSatTb;
		private System.Windows.Forms.TrackBar normalSatSlider;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox hueTb;
		private System.Windows.Forms.TrackBar hueSlider;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox hiliteLumTb;
		private System.Windows.Forms.TrackBar hiliteLumSlider;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox hiliteSatTb;
		private System.Windows.Forms.TrackBar hiliteSatSlider;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button resetDefaultBtn;
		private System.Windows.Forms.Button setDefaultBtn;
		private System.Windows.Forms.Button defaultMtrlBtn;
		private ListViewNF trackList;
		private System.Windows.Forms.ColumnHeader trackColumn;
		private System.Windows.Forms.ColumnHeader normalColumn;
		private System.Windows.Forms.ColumnHeader hilitedColumn;
		private System.Windows.Forms.BindingSource songPanelBindingSource;
		private System.Windows.Forms.BindingSource serviceContainerBindingSource;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button textureBrowseBtn;
		private System.Windows.Forms.TextBox texPathTb;
		private System.Windows.Forms.OpenFileDialog openTextureDlg;
		private System.Windows.Forms.Button unloadTexBtn;
		private System.Windows.Forms.Button loadTexBtn;
		private System.Windows.Forms.ToolStripMenuItem openSongToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveSongToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveSongAsToolStripMenuItem;
		private System.Windows.Forms.PictureBox trackTexPb;
		private System.Windows.Forms.TabControl leftTrackPropsPanel;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.GroupBox lineStyleGroup;
		private System.Windows.Forms.NumericUpDown blurredEdgeUd;
		private System.Windows.Forms.Panel simpleLineStylePanel;
		private System.Windows.Forms.NumericUpDown fadeoutUd;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown lineWidthUpDown;
		private System.Windows.Forms.ComboBox lineStyleList;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown qnGapFillUd;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.ComboBox styleList;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Panel lightPanel;
		private System.Windows.Forms.TextBox lightDirzTb;
		private System.Windows.Forms.Label somelabel;
		private System.Windows.Forms.GroupBox specularGb;
		private System.Windows.Forms.NumericUpDown specFovUd;
		private System.Windows.Forms.NumericUpDown specPowUd;
		private System.Windows.Forms.NumericUpDown specAmountUd;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.TextBox lightDiryTb;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.TextBox lightDirxTb;
		private System.Windows.Forms.CheckBox globalLightCb;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.ContextMenuStrip trackListCM;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem invertSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem defaultPropertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem playbackToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startStopToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem resetPlaybackPositionToolStripMenuItem;
		private System.Windows.Forms.Button defaultStyleBtn;
		private System.Windows.Forms.Button defaultLightBtn;
		private TbSlider upDownVpWidth;
		private System.Windows.Forms.ComboBox lineHlStyleList;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.NumericUpDown hlSizeUpDown;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox hlBorderCb;
		private System.Windows.Forms.CheckBox shrinkingHlCb;
		private System.Windows.Forms.CheckBox movingHlCb;
		private System.Windows.Forms.CheckBox trackPropsCb;
		private System.Windows.Forms.Panel songPropsPanel;
		private System.Windows.Forms.CheckBox songPropsCb;
		private System.Windows.Forms.Panel propsTogglePanel;
		private System.Windows.Forms.NumericUpDown maxPitchUd;
		private System.Windows.Forms.Label hnotelabel;
		private System.Windows.Forms.NumericUpDown minPitchUd;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button defaultPitchesBtn;
		private System.Windows.Forms.CheckBox pointSmpCb;
		private System.Windows.Forms.CheckBox tileTexCb;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox texVTileCb;
		private System.Windows.Forms.CheckBox texUTileCb;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Label screenAnchorLabel;
		private System.Windows.Forms.Label songAnchorLabel;
		private System.Windows.Forms.Label label36;
		private System.Windows.Forms.Label noteAnchorLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton noteVAnchorRb;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton noteUAnchorRb;
		private System.Windows.Forms.RadioButton screenVAnchorRb;
		private System.Windows.Forms.RadioButton songAnchorRb;
		private System.Windows.Forms.RadioButton screenUAnchorRb;
		private System.Windows.Forms.Label label37;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.NumericUpDown texVScrollUD;
		private System.Windows.Forms.Label label33;
		private System.Windows.Forms.NumericUpDown texUScrollUD;
		private System.Windows.Forms.NumericUpDown shapePowerUD;
		private System.Windows.Forms.Label label38;
		private System.Windows.Forms.CheckBox texKeepAspectCb;
	}
}

