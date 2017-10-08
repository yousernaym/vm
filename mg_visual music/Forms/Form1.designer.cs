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
			this.importMidiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importSidSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveSongAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tpartyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.playbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.startStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.beginningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.endToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nudgeBackwardsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nudgeForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.jumpBackwardsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.jumpForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			this.selectedTrackPropsPanel = new System.Windows.Forms.TabControl();
			this.styleTab = new System.Windows.Forms.TabPage();
			this.lineStyleControl = new Visual_Music.LineStyleControl();
			this.barStyleControl = new Visual_Music.BarStyleControl();
			this.defaultStyleBtn = new System.Windows.Forms.Button();
			this.styleList = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.lightingGb = new System.Windows.Forms.GroupBox();
			this.specPowUd = new System.Windows.Forms.NumericUpDown();
			this.ambientAmountUd = new System.Windows.Forms.NumericUpDown();
			this.diffuseAmountUd = new System.Windows.Forms.NumericUpDown();
			this.specAmountUd = new System.Windows.Forms.NumericUpDown();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
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
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.lightDirZUd = new System.Windows.Forms.NumericUpDown();
			this.lightDirYUd = new System.Windows.Forms.NumericUpDown();
			this.lightDirXUd = new System.Windows.Forms.NumericUpDown();
			this.label24 = new System.Windows.Forms.Label();
			this.globalLightCb = new System.Windows.Forms.CheckBox();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.defaultSpatialBtn = new System.Windows.Forms.Button();
			this.label41 = new System.Windows.Forms.Label();
			this.label40 = new System.Windows.Forms.Label();
			this.label39 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.zoffsetUd = new System.Windows.Forms.NumericUpDown();
			this.yoffsetUd = new System.Windows.Forms.NumericUpDown();
			this.xoffsetUd = new System.Windows.Forms.NumericUpDown();
			this.textureBrowseBtn = new System.Windows.Forms.Button();
			this.texPathTb = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.resetDefaultBtn = new System.Windows.Forms.Button();
			this.setDefaultBtn = new System.Windows.Forms.Button();
			this.openTextureDlg = new System.Windows.Forms.OpenFileDialog();
			this.trackPropsCb = new System.Windows.Forms.CheckBox();
			this.songPropsPanel = new System.Windows.Forms.Panel();
			this.resetCamBtn = new System.Windows.Forms.Button();
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
			this.saveMixdownDialog = new System.Windows.Forms.SaveFileDialog();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.debugLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.audioOffsetS)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.trackPropsPanel.SuspendLayout();
			this.trackListCM.SuspendLayout();
			this.selectedTrackPropsPanel.SuspendLayout();
			this.styleTab.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.lightingGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.specPowUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ambientAmountUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.diffuseAmountUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.specAmountUd)).BeginInit();
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
			((System.ComponentModel.ISupportInitialize)(this.lightDirZUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lightDirYUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lightDirXUd)).BeginInit();
			this.tabPage4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zoffsetUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.yoffsetUd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.xoffsetUd)).BeginInit();
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
			this.saveVideoDlg.Filter = "Mp4 files (*.mp4)|*.mp4";
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
            this.importMidiToolStripMenuItem,
            this.importModuleToolStripMenuItem,
            this.importSidSongToolStripMenuItem,
            this.openSongToolStripMenuItem,
            this.saveSongToolStripMenuItem,
            this.saveSongAsToolStripMenuItem,
            this.exportVideoToolStripMenuItem,
            this.tpartyToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// importMidiToolStripMenuItem
			// 
			this.importMidiToolStripMenuItem.Name = "importMidiToolStripMenuItem";
			this.importMidiToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
			this.importMidiToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.importMidiToolStripMenuItem.Text = "Import midi song...";
			this.importMidiToolStripMenuItem.Click += new System.EventHandler(this.importMidiSongToolStripMenuItem_Click);
			// 
			// importModuleToolStripMenuItem
			// 
			this.importModuleToolStripMenuItem.Name = "importModuleToolStripMenuItem";
			this.importModuleToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.M)));
			this.importModuleToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.importModuleToolStripMenuItem.Text = "Import module...";
			this.importModuleToolStripMenuItem.Click += new System.EventHandler(this.importModuleToolStripMenuItem_Click);
			// 
			// importSidSongToolStripMenuItem
			// 
			this.importSidSongToolStripMenuItem.Name = "importSidSongToolStripMenuItem";
			this.importSidSongToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.importSidSongToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.importSidSongToolStripMenuItem.Text = "Import sid song...";
			this.importSidSongToolStripMenuItem.Click += new System.EventHandler(this.importSidSongToolStripMenuItem_Click);
			// 
			// openSongToolStripMenuItem
			// 
			this.openSongToolStripMenuItem.Name = "openSongToolStripMenuItem";
			this.openSongToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openSongToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.openSongToolStripMenuItem.Text = "Open project...";
			this.openSongToolStripMenuItem.Click += new System.EventHandler(this.openSongToolStripMenuItem_Click);
			// 
			// saveSongToolStripMenuItem
			// 
			this.saveSongToolStripMenuItem.Enabled = false;
			this.saveSongToolStripMenuItem.Name = "saveSongToolStripMenuItem";
			this.saveSongToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveSongToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.saveSongToolStripMenuItem.Text = "Save project";
			this.saveSongToolStripMenuItem.Click += new System.EventHandler(this.saveSongToolStripMenuItem_Click);
			// 
			// saveSongAsToolStripMenuItem
			// 
			this.saveSongAsToolStripMenuItem.Enabled = false;
			this.saveSongAsToolStripMenuItem.Name = "saveSongAsToolStripMenuItem";
			this.saveSongAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.saveSongAsToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.saveSongAsToolStripMenuItem.Text = "Save project as...";
			this.saveSongAsToolStripMenuItem.Click += new System.EventHandler(this.saveSongAsToolStripMenuItem_Click);
			// 
			// exportVideoToolStripMenuItem
			// 
			this.exportVideoToolStripMenuItem.Enabled = false;
			this.exportVideoToolStripMenuItem.Name = "exportVideoToolStripMenuItem";
			this.exportVideoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.exportVideoToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.exportVideoToolStripMenuItem.Text = "Export video...";
			this.exportVideoToolStripMenuItem.Click += new System.EventHandler(this.exportVideoToolStripMenuItem_Click);
			// 
			// tpartyToolStripMenuItem
			// 
			this.tpartyToolStripMenuItem.Name = "tpartyToolStripMenuItem";
			this.tpartyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
			this.tpartyToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
			this.tpartyToolStripMenuItem.Text = "Third-party integration...";
			this.tpartyToolStripMenuItem.Click += new System.EventHandler(this.tpartyToolStripMenuItem_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.playbackToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1279, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// playbackToolStripMenuItem
			// 
			this.playbackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startStopToolStripMenuItem,
            this.beginningToolStripMenuItem,
            this.endToolStripMenuItem,
            this.nudgeBackwardsToolStripMenuItem,
            this.nudgeForwardToolStripMenuItem,
            this.jumpBackwardsToolStripMenuItem,
            this.jumpForwardToolStripMenuItem});
			this.playbackToolStripMenuItem.Enabled = false;
			this.playbackToolStripMenuItem.Name = "playbackToolStripMenuItem";
			this.playbackToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
			this.playbackToolStripMenuItem.Text = "Playback";
			// 
			// startStopToolStripMenuItem
			// 
			this.startStopToolStripMenuItem.Name = "startStopToolStripMenuItem";
			this.startStopToolStripMenuItem.ShowShortcutKeys = false;
			this.startStopToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.startStopToolStripMenuItem.Text = "Start/Stop      Space";
			this.startStopToolStripMenuItem.Click += new System.EventHandler(this.startStopToolStripMenuItem_Click);
			// 
			// beginningToolStripMenuItem
			// 
			this.beginningToolStripMenuItem.Name = "beginningToolStripMenuItem";
			this.beginningToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
			this.beginningToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.beginningToolStripMenuItem.Text = "Beginning";
			this.beginningToolStripMenuItem.Click += new System.EventHandler(this.beginningToolStripMenuItem_Click);
			// 
			// endToolStripMenuItem
			// 
			this.endToolStripMenuItem.Name = "endToolStripMenuItem";
			this.endToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
			this.endToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.endToolStripMenuItem.Text = "End";
			this.endToolStripMenuItem.Click += new System.EventHandler(this.endToolStripMenuItem_Click);
			// 
			// nudgeBackwardsToolStripMenuItem
			// 
			this.nudgeBackwardsToolStripMenuItem.Name = "nudgeBackwardsToolStripMenuItem";
			this.nudgeBackwardsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
			this.nudgeBackwardsToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.nudgeBackwardsToolStripMenuItem.Text = "Nudge backwards";
			this.nudgeBackwardsToolStripMenuItem.Click += new System.EventHandler(this.nudgeBackwardsToolStripMenuItem_Click);
			// 
			// nudgeForwardToolStripMenuItem
			// 
			this.nudgeForwardToolStripMenuItem.Name = "nudgeForwardToolStripMenuItem";
			this.nudgeForwardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
			this.nudgeForwardToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.nudgeForwardToolStripMenuItem.Text = "Nudge forward";
			this.nudgeForwardToolStripMenuItem.Click += new System.EventHandler(this.nudgeForwardToolStripMenuItem_Click);
			// 
			// jumpBackwardsToolStripMenuItem
			// 
			this.jumpBackwardsToolStripMenuItem.Name = "jumpBackwardsToolStripMenuItem";
			this.jumpBackwardsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
			this.jumpBackwardsToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.jumpBackwardsToolStripMenuItem.Text = "Jump backwards";
			this.jumpBackwardsToolStripMenuItem.Click += new System.EventHandler(this.jumpBackwardsToolStripMenuItem_Click);
			// 
			// jumpForwardToolStripMenuItem
			// 
			this.jumpForwardToolStripMenuItem.Name = "jumpForwardToolStripMenuItem";
			this.jumpForwardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
			this.jumpForwardToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.jumpForwardToolStripMenuItem.Text = "Jump forward";
			this.jumpForwardToolStripMenuItem.Click += new System.EventHandler(this.jumpForwardToolStripMenuItem_Click);
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
			this.trackPropsPanel.Controls.Add(this.selectedTrackPropsPanel);
			this.trackPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.trackPropsPanel.Location = new System.Drawing.Point(884, 24);
			this.trackPropsPanel.Name = "trackPropsPanel";
			this.trackPropsPanel.Size = new System.Drawing.Size(395, 10052);
			this.trackPropsPanel.TabIndex = 3;
			this.trackPropsPanel.Visible = false;
			this.trackPropsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.trackPropsPanel_Paint);
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
			this.trackListCM.ImageScalingSize = new System.Drawing.Size(24, 24);
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
			// selectedTrackPropsPanel
			// 
			this.selectedTrackPropsPanel.Controls.Add(this.styleTab);
			this.selectedTrackPropsPanel.Controls.Add(this.tabPage1);
			this.selectedTrackPropsPanel.Controls.Add(this.tabPage2);
			this.selectedTrackPropsPanel.Controls.Add(this.tabPage4);
			this.selectedTrackPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.selectedTrackPropsPanel.Location = new System.Drawing.Point(187, 0);
			this.selectedTrackPropsPanel.Name = "selectedTrackPropsPanel";
			this.selectedTrackPropsPanel.SelectedIndex = 0;
			this.selectedTrackPropsPanel.Size = new System.Drawing.Size(208, 10052);
			this.selectedTrackPropsPanel.TabIndex = 14;
			// 
			// styleTab
			// 
			this.styleTab.AutoScroll = true;
			this.styleTab.BackColor = System.Drawing.SystemColors.Control;
			this.styleTab.Controls.Add(this.lineStyleControl);
			this.styleTab.Controls.Add(this.barStyleControl);
			this.styleTab.Controls.Add(this.defaultStyleBtn);
			this.styleTab.Controls.Add(this.styleList);
			this.styleTab.Controls.Add(this.label1);
			this.styleTab.Location = new System.Drawing.Point(4, 22);
			this.styleTab.Name = "styleTab";
			this.styleTab.Size = new System.Drawing.Size(200, 10026);
			this.styleTab.TabIndex = 2;
			this.styleTab.Text = "Style";
			// 
			// lineStyleControl
			// 
			this.lineStyleControl.AutoSize = true;
			this.lineStyleControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.lineStyleControl.Location = new System.Drawing.Point(1, 72);
			this.lineStyleControl.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
			this.lineStyleControl.Name = "lineStyleControl";
			this.lineStyleControl.Size = new System.Drawing.Size(183, 347);
			this.lineStyleControl.TabIndex = 2;
			this.lineStyleControl.Visible = false;
			// 
			// barStyleControl
			// 
			this.barStyleControl.AutoSize = true;
			this.barStyleControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.barStyleControl.Location = new System.Drawing.Point(3, 72);
			this.barStyleControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.barStyleControl.Name = "barStyleControl";
			this.barStyleControl.Size = new System.Drawing.Size(180, 55);
			this.barStyleControl.TabIndex = 2;
			// 
			// defaultStyleBtn
			// 
			this.defaultStyleBtn.Location = new System.Drawing.Point(1, 1);
			this.defaultStyleBtn.Name = "defaultStyleBtn";
			this.defaultStyleBtn.Size = new System.Drawing.Size(179, 23);
			this.defaultStyleBtn.TabIndex = 0;
			this.defaultStyleBtn.Text = "Default Style";
			this.defaultStyleBtn.UseVisualStyleBackColor = true;
			this.defaultStyleBtn.Click += new System.EventHandler(this.defaultStyleBtn_Click);
			// 
			// styleList
			// 
			this.styleList.DisplayMember = "Name";
			this.styleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleList.FormattingEnabled = true;
			this.styleList.Location = new System.Drawing.Point(4, 45);
			this.styleList.Name = "styleList";
			this.styleList.Size = new System.Drawing.Size(113, 21);
			this.styleList.TabIndex = 1;
			this.styleList.ValueMember = "Value";
			this.styleList.SelectedIndexChanged += new System.EventHandler(this.styleList_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1, 29);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Note style";
			// 
			// tabPage1
			// 
			this.tabPage1.AutoScroll = true;
			this.tabPage1.BackColor = System.Drawing.Color.Transparent;
			this.tabPage1.Controls.Add(this.lightingGb);
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
			// lightingGb
			// 
			this.lightingGb.Controls.Add(this.specPowUd);
			this.lightingGb.Controls.Add(this.ambientAmountUd);
			this.lightingGb.Controls.Add(this.diffuseAmountUd);
			this.lightingGb.Controls.Add(this.specAmountUd);
			this.lightingGb.Controls.Add(this.label15);
			this.lightingGb.Controls.Add(this.label14);
			this.lightingGb.Controls.Add(this.label23);
			this.lightingGb.Controls.Add(this.label22);
			this.lightingGb.Location = new System.Drawing.Point(0, 892);
			this.lightingGb.Name = "lightingGb";
			this.lightingGb.Size = new System.Drawing.Size(181, 133);
			this.lightingGb.TabIndex = 70;
			this.lightingGb.TabStop = false;
			this.lightingGb.Text = "Lighting";
			// 
			// specPowUd
			// 
			this.specPowUd.Location = new System.Drawing.Point(111, 101);
			this.specPowUd.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.specPowUd.Name = "specPowUd";
			this.specPowUd.Size = new System.Drawing.Size(59, 20);
			this.specPowUd.TabIndex = 40;
			this.specPowUd.ValueChanged += new System.EventHandler(this.specPowUd_ValueChanged);
			// 
			// ambientAmountUd
			// 
			this.ambientAmountUd.DecimalPlaces = 2;
			this.ambientAmountUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.ambientAmountUd.Location = new System.Drawing.Point(111, 23);
			this.ambientAmountUd.Name = "ambientAmountUd";
			this.ambientAmountUd.Size = new System.Drawing.Size(59, 20);
			this.ambientAmountUd.TabIndex = 10;
			this.ambientAmountUd.ValueChanged += new System.EventHandler(this.ambientAmountUd_ValueChanged);
			// 
			// diffuseAmountUd
			// 
			this.diffuseAmountUd.DecimalPlaces = 2;
			this.diffuseAmountUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.diffuseAmountUd.Location = new System.Drawing.Point(111, 49);
			this.diffuseAmountUd.Name = "diffuseAmountUd";
			this.diffuseAmountUd.Size = new System.Drawing.Size(59, 20);
			this.diffuseAmountUd.TabIndex = 20;
			this.diffuseAmountUd.ValueChanged += new System.EventHandler(this.diffuseAmountUd_ValueChanged);
			// 
			// specAmountUd
			// 
			this.specAmountUd.DecimalPlaces = 2;
			this.specAmountUd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.specAmountUd.Location = new System.Drawing.Point(111, 75);
			this.specAmountUd.Name = "specAmountUd";
			this.specAmountUd.Size = new System.Drawing.Size(59, 20);
			this.specAmountUd.TabIndex = 30;
			this.specAmountUd.ValueChanged += new System.EventHandler(this.specAmountUd_ValueChanged);
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(22, 25);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(83, 13);
			this.label15.TabIndex = 4;
			this.label15.Text = "Ambient amount";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(23, 51);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(78, 13);
			this.label14.TabIndex = 5;
			this.label14.Text = "Diffuse amount";
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(18, 77);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(87, 13);
			this.label23.TabIndex = 6;
			this.label23.Text = "Specular amount";
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(24, 103);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(81, 13);
			this.label22.TabIndex = 7;
			this.label22.Text = "Specular power";
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
			this.groupBox4.TabIndex = 60;
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
			this.texVScrollUD.TabIndex = 51;
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
			this.texUScrollUD.TabIndex = 50;
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
			this.tableLayoutPanel1.TabIndex = 40;
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
			this.noteAnchorLabel.TabIndex = 1;
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
			this.texVTileCb.TabIndex = 32;
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
			this.texUTileCb.TabIndex = 31;
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
			this.texKeepAspectCb.TabIndex = 33;
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
			this.tileTexCb.TabIndex = 30;
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
			this.loadTexBtn.TabIndex = 10;
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
			this.unloadTexBtn.TabIndex = 15;
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
			this.pointSmpCb.TabIndex = 20;
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
			this.defaultMtrlBtn.Location = new System.Drawing.Point(1, 1);
			this.defaultMtrlBtn.Name = "defaultMtrlBtn";
			this.defaultMtrlBtn.Size = new System.Drawing.Size(179, 23);
			this.defaultMtrlBtn.TabIndex = 10;
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
			this.transpSlider.TabIndex = 20;
			this.transpSlider.TickFrequency = 10;
			this.transpSlider.Value = 50;
			this.transpSlider.Scroll += new System.EventHandler(this.transpSlider_Scroll);
			// 
			// hueTb
			// 
			this.hueTb.Location = new System.Drawing.Point(137, 113);
			this.hueTb.Name = "hueTb";
			this.hueTb.Size = new System.Drawing.Size(38, 20);
			this.hueTb.TabIndex = 31;
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
			this.groupBox1.TabIndex = 40;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Normal notes";
			// 
			// normalLumTb
			// 
			this.normalLumTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.normalLumTb.Location = new System.Drawing.Point(141, 105);
			this.normalLumTb.Name = "normalLumTb";
			this.normalLumTb.Size = new System.Drawing.Size(38, 20);
			this.normalLumTb.TabIndex = 21;
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
			this.normalLumSlider.TabIndex = 20;
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
			this.normalSatTb.TabIndex = 11;
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
			this.normalSatSlider.TabIndex = 10;
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
			this.hueSlider.TabIndex = 30;
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
			this.groupBox2.TabIndex = 50;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Highlighted notes";
			// 
			// hiliteLumTb
			// 
			this.hiliteLumTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hiliteLumTb.Location = new System.Drawing.Point(141, 105);
			this.hiliteLumTb.Name = "hiliteLumTb";
			this.hiliteLumTb.Size = new System.Drawing.Size(38, 20);
			this.hiliteLumTb.TabIndex = 21;
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
			this.hiliteLumSlider.TabIndex = 20;
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
			this.hiliteSatTb.TabIndex = 11;
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
			this.hiliteSatSlider.TabIndex = 10;
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
			this.transpTb.TabIndex = 21;
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
			this.defaultLightBtn.Location = new System.Drawing.Point(1, 1);
			this.defaultLightBtn.Name = "defaultLightBtn";
			this.defaultLightBtn.Size = new System.Drawing.Size(179, 23);
			this.defaultLightBtn.TabIndex = 10;
			this.defaultLightBtn.Text = "Default Light";
			this.defaultLightBtn.UseVisualStyleBackColor = true;
			this.defaultLightBtn.Click += new System.EventHandler(this.defaultLightBtn_Click);
			// 
			// lightPanel
			// 
			this.lightPanel.Controls.Add(this.label11);
			this.lightPanel.Controls.Add(this.label12);
			this.lightPanel.Controls.Add(this.label13);
			this.lightPanel.Controls.Add(this.lightDirZUd);
			this.lightPanel.Controls.Add(this.lightDirYUd);
			this.lightPanel.Controls.Add(this.lightDirXUd);
			this.lightPanel.Controls.Add(this.label24);
			this.lightPanel.Enabled = false;
			this.lightPanel.Location = new System.Drawing.Point(1, 60);
			this.lightPanel.Name = "lightPanel";
			this.lightPanel.Size = new System.Drawing.Size(179, 76);
			this.lightPanel.TabIndex = 30;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(144, 49);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(14, 13);
			this.label11.TabIndex = 7;
			this.label11.Text = "Z";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(85, 49);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(14, 13);
			this.label12.TabIndex = 8;
			this.label12.Text = "Y";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(23, 49);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(14, 13);
			this.label13.TabIndex = 9;
			this.label13.Text = "X";
			// 
			// lightDirZUd
			// 
			this.lightDirZUd.Location = new System.Drawing.Point(121, 25);
			this.lightDirZUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.lightDirZUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.lightDirZUd.Name = "lightDirZUd";
			this.lightDirZUd.Size = new System.Drawing.Size(57, 20);
			this.lightDirZUd.TabIndex = 30;
			this.lightDirZUd.ValueChanged += new System.EventHandler(this.lightDirZUd_ValueChanged);
			// 
			// lightDirYUd
			// 
			this.lightDirYUd.Location = new System.Drawing.Point(61, 25);
			this.lightDirYUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.lightDirYUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.lightDirYUd.Name = "lightDirYUd";
			this.lightDirYUd.Size = new System.Drawing.Size(57, 20);
			this.lightDirYUd.TabIndex = 20;
			this.lightDirYUd.ValueChanged += new System.EventHandler(this.lightDirYUd_ValueChanged);
			// 
			// lightDirXUd
			// 
			this.lightDirXUd.Location = new System.Drawing.Point(1, 25);
			this.lightDirXUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.lightDirXUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.lightDirXUd.Name = "lightDirXUd";
			this.lightDirXUd.Size = new System.Drawing.Size(57, 20);
			this.lightDirXUd.TabIndex = 10;
			this.lightDirXUd.ValueChanged += new System.EventHandler(this.lightDirXUd_ValueChanged);
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
			// globalLightCb
			// 
			this.globalLightCb.AutoSize = true;
			this.globalLightCb.Checked = true;
			this.globalLightCb.CheckState = System.Windows.Forms.CheckState.Checked;
			this.globalLightCb.Location = new System.Drawing.Point(1, 37);
			this.globalLightCb.Name = "globalLightCb";
			this.globalLightCb.Size = new System.Drawing.Size(119, 17);
			this.globalLightCb.TabIndex = 20;
			this.globalLightCb.Text = "Use global direction";
			this.globalLightCb.UseVisualStyleBackColor = true;
			this.globalLightCb.CheckedChanged += new System.EventHandler(this.globalLightCb_CheckedChanged);
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.defaultSpatialBtn);
			this.tabPage4.Controls.Add(this.label41);
			this.tabPage4.Controls.Add(this.label40);
			this.tabPage4.Controls.Add(this.label39);
			this.tabPage4.Controls.Add(this.label20);
			this.tabPage4.Controls.Add(this.zoffsetUd);
			this.tabPage4.Controls.Add(this.yoffsetUd);
			this.tabPage4.Controls.Add(this.xoffsetUd);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage4.Size = new System.Drawing.Size(200, 10026);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Spatial";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// defaultSpatialBtn
			// 
			this.defaultSpatialBtn.Location = new System.Drawing.Point(1, 1);
			this.defaultSpatialBtn.Name = "defaultSpatialBtn";
			this.defaultSpatialBtn.Size = new System.Drawing.Size(179, 23);
			this.defaultSpatialBtn.TabIndex = 10;
			this.defaultSpatialBtn.Text = "Default Spatial";
			this.defaultSpatialBtn.UseVisualStyleBackColor = true;
			this.defaultSpatialBtn.Click += new System.EventHandler(this.defaultSpatialBtn_Click);
			// 
			// label41
			// 
			this.label41.AutoSize = true;
			this.label41.Location = new System.Drawing.Point(151, 80);
			this.label41.Name = "label41";
			this.label41.Size = new System.Drawing.Size(14, 13);
			this.label41.TabIndex = 1;
			this.label41.Text = "Z";
			// 
			// label40
			// 
			this.label40.AutoSize = true;
			this.label40.Location = new System.Drawing.Point(87, 80);
			this.label40.Name = "label40";
			this.label40.Size = new System.Drawing.Size(14, 13);
			this.label40.TabIndex = 1;
			this.label40.Text = "Y";
			// 
			// label39
			// 
			this.label39.AutoSize = true;
			this.label39.Location = new System.Drawing.Point(24, 80);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(14, 13);
			this.label39.TabIndex = 1;
			this.label39.Text = "X";
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(-3, 41);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(73, 13);
			this.label20.TabIndex = 1;
			this.label20.Text = "Position offset";
			// 
			// zoffsetUd
			// 
			this.zoffsetUd.Location = new System.Drawing.Point(121, 57);
			this.zoffsetUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.zoffsetUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.zoffsetUd.Name = "zoffsetUd";
			this.zoffsetUd.Size = new System.Drawing.Size(57, 20);
			this.zoffsetUd.TabIndex = 40;
			this.zoffsetUd.ValueChanged += new System.EventHandler(this.zoffsetUd_ValueChanged);
			// 
			// yoffsetUd
			// 
			this.yoffsetUd.Location = new System.Drawing.Point(61, 57);
			this.yoffsetUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.yoffsetUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.yoffsetUd.Name = "yoffsetUd";
			this.yoffsetUd.Size = new System.Drawing.Size(57, 20);
			this.yoffsetUd.TabIndex = 30;
			this.yoffsetUd.ValueChanged += new System.EventHandler(this.yoffsetUd_ValueChanged);
			// 
			// xoffsetUd
			// 
			this.xoffsetUd.Location = new System.Drawing.Point(1, 57);
			this.xoffsetUd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.xoffsetUd.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.xoffsetUd.Name = "xoffsetUd";
			this.xoffsetUd.Size = new System.Drawing.Size(57, 20);
			this.xoffsetUd.TabIndex = 20;
			this.xoffsetUd.ValueChanged += new System.EventHandler(this.xoffsetUd_ValueChanged);
			// 
			// textureBrowseBtn
			// 
			this.textureBrowseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textureBrowseBtn.Location = new System.Drawing.Point(-3027, 10055);
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
			this.texPathTb.Location = new System.Drawing.Point(-3139, 10056);
			this.texPathTb.Name = "texPathTb";
			this.texPathTb.Size = new System.Drawing.Size(123, 20);
			this.texPathTb.TabIndex = 10;
			this.texPathTb.Visible = false;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(-3141, 10040);
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
			this.trackPropsCb.TabIndex = 1;
			this.trackPropsCb.Text = "&Track properties";
			this.trackPropsCb.UseVisualStyleBackColor = true;
			this.trackPropsCb.CheckedChanged += new System.EventHandler(this.trackPropsCb_CheckedChanged);
			// 
			// songPropsPanel
			// 
			this.songPropsPanel.Controls.Add(this.resetCamBtn);
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
			// resetCamBtn
			// 
			this.resetCamBtn.Location = new System.Drawing.Point(12, 201);
			this.resetCamBtn.Name = "resetCamBtn";
			this.resetCamBtn.Size = new System.Drawing.Size(85, 23);
			this.resetCamBtn.TabIndex = 5;
			this.resetCamBtn.Text = "&Reset camera";
			this.resetCamBtn.UseVisualStyleBackColor = true;
			this.resetCamBtn.Click += new System.EventHandler(this.resetCamBtn_Click);
			// 
			// defaultPitchesBtn
			// 
			this.defaultPitchesBtn.Location = new System.Drawing.Point(12, 172);
			this.defaultPitchesBtn.Name = "defaultPitchesBtn";
			this.defaultPitchesBtn.Size = new System.Drawing.Size(111, 23);
			this.defaultPitchesBtn.TabIndex = 4;
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
			this.minPitchUd.TabIndex = 3;
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
			this.maxPitchUd.TabIndex = 2;
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
			this.upDownVpWidth.TabIndex = 0;
			this.upDownVpWidth.TbWidth = 50;
			this.upDownVpWidth.TickFreq = 1D;
			this.upDownVpWidth.Value = 16D;
			this.upDownVpWidth.ValueChanged += new System.EventHandler(this.upDownVpWidth_ValueChanged);
			this.upDownVpWidth.CommitChanges += new System.EventHandler(this.upDownVpWidth_CommitChanges);
			// 
			// songPropsCb
			// 
			this.songPropsCb.Appearance = System.Windows.Forms.Appearance.Button;
			this.songPropsCb.AutoSize = true;
			this.songPropsCb.Location = new System.Drawing.Point(3, 0);
			this.songPropsCb.Name = "songPropsCb";
			this.songPropsCb.Size = new System.Drawing.Size(91, 23);
			this.songPropsCb.TabIndex = 0;
			this.songPropsCb.Text = "Song &properties";
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
			// saveMixdownDialog
			// 
			this.saveMixdownDialog.Filter = "Wav files (*.wav)|*.wav|All files (*.*)|*.*";
			// 
			// debugLabel
			// 
			this.debugLabel.AutoSize = true;
			this.debugLabel.Location = new System.Drawing.Point(460, 5);
			this.debugLabel.Name = "debugLabel";
			this.debugLabel.Size = new System.Drawing.Size(41, 13);
			this.debugLabel.TabIndex = 20;
			this.debugLabel.Text = "label16";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1296, 407);
			this.Controls.Add(this.debugLabel);
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
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
			((System.ComponentModel.ISupportInitialize)(this.audioOffsetS)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.trackPropsPanel.ResumeLayout(false);
			this.trackListCM.ResumeLayout(false);
			this.selectedTrackPropsPanel.ResumeLayout(false);
			this.styleTab.ResumeLayout(false);
			this.styleTab.PerformLayout();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.lightingGb.ResumeLayout(false);
			this.lightingGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.specPowUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ambientAmountUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.diffuseAmountUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.specAmountUd)).EndInit();
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
			((System.ComponentModel.ISupportInitialize)(this.lightDirZUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lightDirYUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lightDirXUd)).EndInit();
			this.tabPage4.ResumeLayout(false);
			this.tabPage4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zoffsetUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.yoffsetUd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.xoffsetUd)).EndInit();
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
		private System.Windows.Forms.NumericUpDown audioOffsetS;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importMidiToolStripMenuItem;
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
		private System.Windows.Forms.Button unloadTexBtn;
		private System.Windows.Forms.Button loadTexBtn;
		private System.Windows.Forms.ToolStripMenuItem openSongToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveSongToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveSongAsToolStripMenuItem;
		private System.Windows.Forms.PictureBox trackTexPb;
		private System.Windows.Forms.TabControl selectedTrackPropsPanel;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ComboBox styleList;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage styleTab;
		private System.Windows.Forms.Panel lightPanel;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.CheckBox globalLightCb;
		private System.Windows.Forms.ContextMenuStrip trackListCM;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem invertSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem defaultPropertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem playbackToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startStopToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem beginningToolStripMenuItem;
		private System.Windows.Forms.Button defaultStyleBtn;
		private System.Windows.Forms.Button defaultLightBtn;
		private TbSlider upDownVpWidth;
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
		private System.Windows.Forms.CheckBox texKeepAspectCb;
        private System.Windows.Forms.ToolStripMenuItem importModuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importSidSongToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem tpartyToolStripMenuItem;
		public System.Windows.Forms.SaveFileDialog saveVideoDlg;
		public System.Windows.Forms.OpenFileDialog openTextureDlg;
		private System.Windows.Forms.SaveFileDialog saveMixdownDialog;
		private System.Windows.Forms.Button resetCamBtn;
		private System.Windows.Forms.ToolStripMenuItem endToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem nudgeBackwardsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem nudgeForwardToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem jumpBackwardsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem jumpForwardToolStripMenuItem;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.Button defaultSpatialBtn;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.NumericUpDown xoffsetUd;
		private System.Windows.Forms.Label label41;
		private System.Windows.Forms.Label label40;
		private System.Windows.Forms.Label label39;
		private System.Windows.Forms.NumericUpDown zoffsetUd;
		private System.Windows.Forms.NumericUpDown yoffsetUd;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private BarStyleControl barStyleControl;
		private LineStyleControl lineStyleControl;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown lightDirZUd;
		private System.Windows.Forms.NumericUpDown lightDirYUd;
		private System.Windows.Forms.NumericUpDown lightDirXUd;
		private System.Windows.Forms.GroupBox lightingGb;
		private System.Windows.Forms.NumericUpDown specPowUd;
		private System.Windows.Forms.NumericUpDown ambientAmountUd;
		private System.Windows.Forms.NumericUpDown diffuseAmountUd;
		private System.Windows.Forms.NumericUpDown specAmountUd;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label22;
		public System.Windows.Forms.Label debugLabel;
	}
}

