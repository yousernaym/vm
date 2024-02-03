namespace VisualMusic
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            openProjDialog = new System.Windows.Forms.OpenFileDialog();
            saveProjDialog = new System.Windows.Forms.SaveFileDialog();
            saveVideoDlg = new System.Windows.Forms.SaveFileDialog();
            audioOffsetS = new System.Windows.Forms.NumericUpDown();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importMidiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importSidSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveSongToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveSongAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tpartyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resetCamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadCamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveCamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tracksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            defaultPropertiesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            loadPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            savePropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertLyricsHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertKeyFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            playbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            startStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            beginningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            endToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            nudgeBackwardsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            nudgeForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            jumpBackwardsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            jumpForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewSongTSMI = new System.Windows.Forms.ToolStripMenuItem();
            viewModBrowserTSMI = new System.Windows.Forms.ToolStripMenuItem();
            viewSidBrowserTSMI = new System.Windows.Forms.ToolStripMenuItem();
            viewMidiBrowserTSMI = new System.Windows.Forms.ToolStripMenuItem();
            label7 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            trackPropsPanel = new System.Windows.Forms.Panel();
            trackList = new ListViewNF();
            trackColumn = new System.Windows.Forms.ColumnHeader();
            normalColumn = new System.Windows.Forms.ColumnHeader();
            hilitedColumn = new System.Windows.Forms.ColumnHeader();
            trackListCM = new System.Windows.Forms.ContextMenuStrip(components);
            selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            invertSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            defaultPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadTrackPropsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveTrackPropsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectedTrackPropsPanel = new System.Windows.Forms.TabControl();
            style = new System.Windows.Forms.TabPage();
            lineStyleControl = new LineStyleControl();
            barStyleControl = new BarStyleControl();
            defaultStyleBtn = new System.Windows.Forms.Button();
            styleList = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            material = new System.Windows.Forms.TabPage();
            groupBox4 = new System.Windows.Forms.GroupBox();
            label37 = new System.Windows.Forms.Label();
            label34 = new System.Windows.Forms.Label();
            label35 = new System.Windows.Forms.Label();
            texVScrollUD = new System.Windows.Forms.NumericUpDown();
            label33 = new System.Windows.Forms.Label();
            texUScrollUD = new System.Windows.Forms.NumericUpDown();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label31 = new System.Windows.Forms.Label();
            label32 = new System.Windows.Forms.Label();
            label36 = new System.Windows.Forms.Label();
            songAnchorLabel = new System.Windows.Forms.Label();
            noteAnchorLabel = new System.Windows.Forms.Label();
            panel2 = new System.Windows.Forms.Panel();
            songAnchorRb = new System.Windows.Forms.RadioButton();
            screenUAnchorRb = new System.Windows.Forms.RadioButton();
            noteUAnchorRb = new System.Windows.Forms.RadioButton();
            screenAnchorLabel = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            screenVAnchorRb = new System.Windows.Forms.RadioButton();
            noteVAnchorRb = new System.Windows.Forms.RadioButton();
            texVTileCb = new System.Windows.Forms.CheckBox();
            texUTileCb = new System.Windows.Forms.CheckBox();
            texKeepAspectCb = new System.Windows.Forms.CheckBox();
            tileTexCb = new System.Windows.Forms.CheckBox();
            loadTexBtn = new System.Windows.Forms.Button();
            unloadTexBtn = new System.Windows.Forms.Button();
            disableTextureCh = new System.Windows.Forms.CheckBox();
            texColBlendCb = new System.Windows.Forms.CheckBox();
            pointSmpCb = new System.Windows.Forms.CheckBox();
            trackTexPb = new System.Windows.Forms.PictureBox();
            defaultMtrlBtn = new System.Windows.Forms.Button();
            alphaLbl = new System.Windows.Forms.Label();
            transpSlider = new System.Windows.Forms.TrackBar();
            hueTb = new System.Windows.Forms.TextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            normalLumTb = new System.Windows.Forms.TextBox();
            normalLumSlider = new System.Windows.Forms.TrackBar();
            label = new System.Windows.Forms.Label();
            normalSatTb = new System.Windows.Forms.TextBox();
            normalSatSlider = new System.Windows.Forms.TrackBar();
            label3 = new System.Windows.Forms.Label();
            hueSlider = new System.Windows.Forms.TrackBar();
            label2 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            hiliteLumTb = new System.Windows.Forms.TextBox();
            hiliteLumSlider = new System.Windows.Forms.TrackBar();
            label5 = new System.Windows.Forms.Label();
            hiliteSatTb = new System.Windows.Forms.TextBox();
            hiliteSatSlider = new System.Windows.Forms.TrackBar();
            label6 = new System.Windows.Forms.Label();
            transpTb = new System.Windows.Forms.TextBox();
            light = new System.Windows.Forms.TabPage();
            defaultLightBtn = new System.Windows.Forms.Button();
            lightPanel = new System.Windows.Forms.Panel();
            masterLightHsBtn = new HueSatButton();
            specHsBtn = new HueSatButton();
            diffuseHsBtn = new HueSatButton();
            ambientHsBtn = new HueSatButton();
            specPowUd = new System.Windows.Forms.NumericUpDown();
            ambientAmountUd = new System.Windows.Forms.NumericUpDown();
            diffuseAmountUd = new System.Windows.Forms.NumericUpDown();
            label24 = new System.Windows.Forms.Label();
            masterLightAmountUd = new System.Windows.Forms.NumericUpDown();
            specAmountUd = new System.Windows.Forms.NumericUpDown();
            label15 = new System.Windows.Forms.Label();
            label14 = new System.Windows.Forms.Label();
            label16 = new System.Windows.Forms.Label();
            label22 = new System.Windows.Forms.Label();
            label23 = new System.Windows.Forms.Label();
            lightDirXUd = new System.Windows.Forms.NumericUpDown();
            label11 = new System.Windows.Forms.Label();
            lightDirYUd = new System.Windows.Forms.NumericUpDown();
            lightDirZUd = new System.Windows.Forms.NumericUpDown();
            label12 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            globalLightCb = new System.Windows.Forms.CheckBox();
            spatial = new System.Windows.Forms.TabPage();
            defaultSpatialBtn = new System.Windows.Forms.Button();
            label41 = new System.Windows.Forms.Label();
            label40 = new System.Windows.Forms.Label();
            label39 = new System.Windows.Forms.Label();
            label20 = new System.Windows.Forms.Label();
            zoffsetUd = new System.Windows.Forms.NumericUpDown();
            yoffsetUd = new System.Windows.Forms.NumericUpDown();
            xoffsetUd = new System.Windows.Forms.NumericUpDown();
            resetDefaultBtn = new System.Windows.Forms.Button();
            openTextureDlg = new System.Windows.Forms.OpenFileDialog();
            songPropsPanel = new System.Windows.Forms.Panel();
            camLabel = new System.Windows.Forms.Label();
            camTb = new System.Windows.Forms.TextBox();
            defaultPitchesBtn = new System.Windows.Forms.Button();
            minPitchUd = new System.Windows.Forms.NumericUpDown();
            maxPitchUd = new System.Windows.Forms.NumericUpDown();
            label26 = new System.Windows.Forms.Label();
            label21 = new System.Windows.Forms.Label();
            label18 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            hnotelabel = new System.Windows.Forms.Label();
            label25 = new System.Windows.Forms.Label();
            label19 = new System.Windows.Forms.Label();
            label17 = new System.Windows.Forms.Label();
            fadeOutUd = new System.Windows.Forms.NumericUpDown();
            fadeInUd = new System.Windows.Forms.NumericUpDown();
            playbackOffsetUd = new System.Windows.Forms.NumericUpDown();
            upDownVpWidth = new TbSlider();
            saveMixdownDialog = new System.Windows.Forms.SaveFileDialog();
            colorDialog1 = new System.Windows.Forms.ColorDialog();
            saveMidiDialog = new System.Windows.Forms.SaveFileDialog();
            openTrackPropsFileDialog = new System.Windows.Forms.OpenFileDialog();
            saveTrackPropsFileDialog = new System.Windows.Forms.SaveFileDialog();
            trackPropsTabCM = new System.Windows.Forms.ContextMenuStrip(components);
            loadTrackPropsTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveTrackPtopsTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openCamFileDialog = new System.Windows.Forms.OpenFileDialog();
            saveCamFileDialog = new System.Windows.Forms.SaveFileDialog();
            lyricsGridView = new System.Windows.Forms.DataGridView();
            TimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            LyricsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            keyFramesDGV = new System.Windows.Forms.DataGridView();
            Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            propsTogglePanel = new System.Windows.Forms.Panel();
            songPropsCb = new System.Windows.Forms.CheckBox();
            trackPropsCb = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)audioOffsetS).BeginInit();
            menuStrip1.SuspendLayout();
            trackPropsPanel.SuspendLayout();
            trackListCM.SuspendLayout();
            selectedTrackPropsPanel.SuspendLayout();
            style.SuspendLayout();
            material.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)texVScrollUD).BeginInit();
            ((System.ComponentModel.ISupportInitialize)texUScrollUD).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackTexPb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)transpSlider).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)normalLumSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)normalSatSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)hueSlider).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)hiliteLumSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)hiliteSatSlider).BeginInit();
            light.SuspendLayout();
            lightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)specPowUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ambientAmountUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)diffuseAmountUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)masterLightAmountUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)specAmountUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lightDirXUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lightDirYUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lightDirZUd).BeginInit();
            spatial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)zoffsetUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)yoffsetUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)xoffsetUd).BeginInit();
            songPropsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)minPitchUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)maxPitchUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fadeOutUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fadeInUd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)playbackOffsetUd).BeginInit();
            trackPropsTabCM.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lyricsGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)keyFramesDGV).BeginInit();
            propsTogglePanel.SuspendLayout();
            SuspendLayout();
            // 
            // openProjDialog
            // 
            openProjDialog.Filter = "Visual Music projects|*.vmp|All files|*.*";
            // 
            // saveProjDialog
            // 
            saveProjDialog.Filter = "Visual Music projects|*.vmp|All files|*.*";
            // 
            // saveVideoDlg
            // 
            saveVideoDlg.Filter = "Mkv files (*.mkv)|*.mkv";
            saveVideoDlg.Title = "Save video file";
            // 
            // audioOffsetS
            // 
            audioOffsetS.DecimalPlaces = 2;
            audioOffsetS.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            audioOffsetS.Location = new System.Drawing.Point(98, 73);
            audioOffsetS.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            audioOffsetS.Minimum = new decimal(new int[] { 99, 0, 0, int.MinValue });
            audioOffsetS.Name = "audioOffsetS";
            audioOffsetS.Size = new System.Drawing.Size(52, 23);
            audioOffsetS.TabIndex = 1;
            audioOffsetS.Tag = "Edit Audio Offset";
            audioOffsetS.ValueChanged += audioOffsetS_ValueChanged;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { importMidiToolStripMenuItem, importModuleToolStripMenuItem, importSidSongToolStripMenuItem, openSongToolStripMenuItem, saveSongToolStripMenuItem, saveSongAsToolStripMenuItem, exportVideoToolStripMenuItem, tpartyToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // importMidiToolStripMenuItem
            // 
            importMidiToolStripMenuItem.Name = "importMidiToolStripMenuItem";
            importMidiToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M;
            importMidiToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            importMidiToolStripMenuItem.Text = "Import midi song...";
            importMidiToolStripMenuItem.Click += importMidiSongToolStripMenuItem_Click;
            // 
            // importModuleToolStripMenuItem
            // 
            importModuleToolStripMenuItem.Name = "importModuleToolStripMenuItem";
            importModuleToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.M;
            importModuleToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            importModuleToolStripMenuItem.Text = "Import module...";
            importModuleToolStripMenuItem.Click += importModuleToolStripMenuItem_Click;
            // 
            // importSidSongToolStripMenuItem
            // 
            importSidSongToolStripMenuItem.Name = "importSidSongToolStripMenuItem";
            importSidSongToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            importSidSongToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            importSidSongToolStripMenuItem.Text = "Import sid song...";
            importSidSongToolStripMenuItem.Click += importSidSongToolStripMenuItem_Click;
            // 
            // openSongToolStripMenuItem
            // 
            openSongToolStripMenuItem.Name = "openSongToolStripMenuItem";
            openSongToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            openSongToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            openSongToolStripMenuItem.Text = "Open project...";
            openSongToolStripMenuItem.Click += openSongToolStripMenuItem_Click;
            // 
            // saveSongToolStripMenuItem
            // 
            saveSongToolStripMenuItem.Enabled = false;
            saveSongToolStripMenuItem.Name = "saveSongToolStripMenuItem";
            saveSongToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            saveSongToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            saveSongToolStripMenuItem.Text = "Save project";
            saveSongToolStripMenuItem.Click += saveSongToolStripMenuItem_Click;
            // 
            // saveSongAsToolStripMenuItem
            // 
            saveSongAsToolStripMenuItem.Enabled = false;
            saveSongAsToolStripMenuItem.Name = "saveSongAsToolStripMenuItem";
            saveSongAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            saveSongAsToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            saveSongAsToolStripMenuItem.Text = "Save project as...";
            saveSongAsToolStripMenuItem.Click += saveSongAsToolStripMenuItem_Click;
            // 
            // exportVideoToolStripMenuItem
            // 
            exportVideoToolStripMenuItem.Enabled = false;
            exportVideoToolStripMenuItem.Name = "exportVideoToolStripMenuItem";
            exportVideoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            exportVideoToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            exportVideoToolStripMenuItem.Text = "Export video...";
            exportVideoToolStripMenuItem.Click += exportVideoToolStripMenuItem_Click;
            // 
            // tpartyToolStripMenuItem
            // 
            tpartyToolStripMenuItem.Name = "tpartyToolStripMenuItem";
            tpartyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T;
            tpartyToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            tpartyToolStripMenuItem.Text = "Third-party integration...";
            tpartyToolStripMenuItem.Click += tpartyToolStripMenuItem_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, playbackToolStripMenuItem, viewToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1179, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, cameraToolStripMenuItem, tracksToolStripMenuItem, insertLyricsHereToolStripMenuItem, insertKeyFrameToolStripMenuItem });
            editToolStripMenuItem.Enabled = false;
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Enabled = false;
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            undoToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            undoToolStripMenuItem.Text = "Undo";
            undoToolStripMenuItem.Click += undoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Z;
            redoToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            redoToolStripMenuItem.Text = "Redo";
            redoToolStripMenuItem.Click += redoToolStripMenuItem_Click;
            // 
            // cameraToolStripMenuItem
            // 
            cameraToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { resetCamToolStripMenuItem, loadCamToolStripMenuItem, saveCamToolStripMenuItem });
            cameraToolStripMenuItem.Name = "cameraToolStripMenuItem";
            cameraToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            cameraToolStripMenuItem.Text = "Camera";
            // 
            // resetCamToolStripMenuItem
            // 
            resetCamToolStripMenuItem.Name = "resetCamToolStripMenuItem";
            resetCamToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R;
            resetCamToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            resetCamToolStripMenuItem.Tag = "Reset Camera";
            resetCamToolStripMenuItem.Text = "Reset";
            resetCamToolStripMenuItem.Click += resetCamToolStripMenuItem_Click;
            // 
            // loadCamToolStripMenuItem
            // 
            loadCamToolStripMenuItem.Enabled = false;
            loadCamToolStripMenuItem.Name = "loadCamToolStripMenuItem";
            loadCamToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M;
            loadCamToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            loadCamToolStripMenuItem.Tag = "";
            loadCamToolStripMenuItem.Text = "Load...";
            loadCamToolStripMenuItem.Click += loadCamToolStripMenuItem_Click;
            // 
            // saveCamToolStripMenuItem
            // 
            saveCamToolStripMenuItem.Enabled = false;
            saveCamToolStripMenuItem.Name = "saveCamToolStripMenuItem";
            saveCamToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.M;
            saveCamToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            saveCamToolStripMenuItem.Text = "Save...";
            saveCamToolStripMenuItem.Click += saveCamToolStripMenuItem_Click;
            // 
            // tracksToolStripMenuItem
            // 
            tracksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { defaultPropertiesToolStripMenuItem1, loadPropertiesToolStripMenuItem, savePropertiesToolStripMenuItem });
            tracksToolStripMenuItem.Name = "tracksToolStripMenuItem";
            tracksToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            tracksToolStripMenuItem.Text = "Tracks";
            tracksToolStripMenuItem.EnabledChanged += tracksToolStripMenuItem_EnabledChanged;
            // 
            // defaultPropertiesToolStripMenuItem1
            // 
            defaultPropertiesToolStripMenuItem1.Name = "defaultPropertiesToolStripMenuItem1";
            defaultPropertiesToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            defaultPropertiesToolStripMenuItem1.Size = new System.Drawing.Size(236, 22);
            defaultPropertiesToolStripMenuItem1.Tag = "Default Track Properties";
            defaultPropertiesToolStripMenuItem1.Text = "Default Properties";
            defaultPropertiesToolStripMenuItem1.Click += defaultPropertiesToolStripMenuItem1_Click;
            // 
            // loadPropertiesToolStripMenuItem
            // 
            loadPropertiesToolStripMenuItem.Enabled = false;
            loadPropertiesToolStripMenuItem.Name = "loadPropertiesToolStripMenuItem";
            loadPropertiesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P;
            loadPropertiesToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            loadPropertiesToolStripMenuItem.Tag = "";
            loadPropertiesToolStripMenuItem.Text = "Load properties...";
            loadPropertiesToolStripMenuItem.Click += loadPropertiesToolStripMenuItem_Click;
            // 
            // savePropertiesToolStripMenuItem
            // 
            savePropertiesToolStripMenuItem.Enabled = false;
            savePropertiesToolStripMenuItem.Name = "savePropertiesToolStripMenuItem";
            savePropertiesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.P;
            savePropertiesToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            savePropertiesToolStripMenuItem.Text = "Save properties...";
            savePropertiesToolStripMenuItem.Click += savePropertiesToolStripMenuItem_Click;
            // 
            // insertLyricsHereToolStripMenuItem
            // 
            insertLyricsHereToolStripMenuItem.Enabled = false;
            insertLyricsHereToolStripMenuItem.Name = "insertLyricsHereToolStripMenuItem";
            insertLyricsHereToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L;
            insertLyricsHereToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            insertLyricsHereToolStripMenuItem.Tag = "Insert Lyrics";
            insertLyricsHereToolStripMenuItem.Text = "Insert Lyrics";
            insertLyricsHereToolStripMenuItem.Click += insertLyricsHereToolStripMenuItem_Click;
            // 
            // insertKeyFrameToolStripMenuItem
            // 
            insertKeyFrameToolStripMenuItem.Enabled = false;
            insertKeyFrameToolStripMenuItem.Name = "insertKeyFrameToolStripMenuItem";
            insertKeyFrameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K;
            insertKeyFrameToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            insertKeyFrameToolStripMenuItem.Tag = "";
            insertKeyFrameToolStripMenuItem.Text = "Insert Key Frame";
            insertKeyFrameToolStripMenuItem.Click += insertKeyFrameToolStripMenuItem_Click;
            // 
            // playbackToolStripMenuItem
            // 
            playbackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { startStopToolStripMenuItem, beginningToolStripMenuItem, endToolStripMenuItem, nudgeBackwardsToolStripMenuItem, nudgeForwardToolStripMenuItem, jumpBackwardsToolStripMenuItem, jumpForwardToolStripMenuItem });
            playbackToolStripMenuItem.Enabled = false;
            playbackToolStripMenuItem.Name = "playbackToolStripMenuItem";
            playbackToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            playbackToolStripMenuItem.Text = "Playback";
            // 
            // startStopToolStripMenuItem
            // 
            startStopToolStripMenuItem.Name = "startStopToolStripMenuItem";
            startStopToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space;
            startStopToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            startStopToolStripMenuItem.Text = "Start/Stop";
            startStopToolStripMenuItem.Click += startStopToolStripMenuItem_Click;
            // 
            // beginningToolStripMenuItem
            // 
            beginningToolStripMenuItem.Name = "beginningToolStripMenuItem";
            beginningToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home;
            beginningToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            beginningToolStripMenuItem.Text = "Go to beginning";
            beginningToolStripMenuItem.Click += beginningToolStripMenuItem_Click;
            // 
            // endToolStripMenuItem
            // 
            endToolStripMenuItem.Name = "endToolStripMenuItem";
            endToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End;
            endToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            endToolStripMenuItem.Text = "Go to end";
            endToolStripMenuItem.Click += endToolStripMenuItem_Click;
            // 
            // nudgeBackwardsToolStripMenuItem
            // 
            nudgeBackwardsToolStripMenuItem.Name = "nudgeBackwardsToolStripMenuItem";
            nudgeBackwardsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left;
            nudgeBackwardsToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            nudgeBackwardsToolStripMenuItem.Text = "Nudge backwards";
            nudgeBackwardsToolStripMenuItem.Click += nudgeBackwardsToolStripMenuItem_Click;
            // 
            // nudgeForwardToolStripMenuItem
            // 
            nudgeForwardToolStripMenuItem.Name = "nudgeForwardToolStripMenuItem";
            nudgeForwardToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right;
            nudgeForwardToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            nudgeForwardToolStripMenuItem.Text = "Nudge forward";
            nudgeForwardToolStripMenuItem.Click += nudgeForwardToolStripMenuItem_Click;
            // 
            // jumpBackwardsToolStripMenuItem
            // 
            jumpBackwardsToolStripMenuItem.Name = "jumpBackwardsToolStripMenuItem";
            jumpBackwardsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left;
            jumpBackwardsToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            jumpBackwardsToolStripMenuItem.Text = "Jump backwards";
            jumpBackwardsToolStripMenuItem.Click += jumpBackwardsToolStripMenuItem_Click;
            // 
            // jumpForwardToolStripMenuItem
            // 
            jumpForwardToolStripMenuItem.Name = "jumpForwardToolStripMenuItem";
            jumpForwardToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right;
            jumpForwardToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            jumpForwardToolStripMenuItem.Text = "Jump forward";
            jumpForwardToolStripMenuItem.Click += jumpForwardToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { viewSongTSMI, viewModBrowserTSMI, viewSidBrowserTSMI, viewMidiBrowserTSMI });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // viewSongTSMI
            // 
            viewSongTSMI.Name = "viewSongTSMI";
            viewSongTSMI.ShortcutKeys = System.Windows.Forms.Keys.F1;
            viewSongTSMI.Size = new System.Drawing.Size(163, 22);
            viewSongTSMI.Text = "Song";
            viewSongTSMI.Click += viewSongTSMI_Click;
            // 
            // viewModBrowserTSMI
            // 
            viewModBrowserTSMI.Name = "viewModBrowserTSMI";
            viewModBrowserTSMI.ShortcutKeys = System.Windows.Forms.Keys.F2;
            viewModBrowserTSMI.Size = new System.Drawing.Size(163, 22);
            viewModBrowserTSMI.Text = "Mod browser";
            viewModBrowserTSMI.Click += viewModBrowserTSMI_Click;
            // 
            // viewSidBrowserTSMI
            // 
            viewSidBrowserTSMI.Name = "viewSidBrowserTSMI";
            viewSidBrowserTSMI.ShortcutKeys = System.Windows.Forms.Keys.F3;
            viewSidBrowserTSMI.Size = new System.Drawing.Size(163, 22);
            viewSidBrowserTSMI.Text = "Sid browser";
            viewSidBrowserTSMI.Click += viewSidBrowserTSMI_Click;
            // 
            // viewMidiBrowserTSMI
            // 
            viewMidiBrowserTSMI.Name = "viewMidiBrowserTSMI";
            viewMidiBrowserTSMI.ShortcutKeys = System.Windows.Forms.Keys.F4;
            viewMidiBrowserTSMI.Size = new System.Drawing.Size(163, 22);
            viewMidiBrowserTSMI.Text = "Midi browser";
            viewMidiBrowserTSMI.Click += viewMidiBrowserTSMI_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(12, 12);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(87, 15);
            label7.TabIndex = 3;
            label7.Text = "Viewport width";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(12, 75);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(72, 15);
            label9.TabIndex = 3;
            label9.Text = "Audio offset";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(156, 75);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(12, 15);
            label10.TabIndex = 3;
            label10.Text = "s";
            // 
            // trackPropsPanel
            // 
            trackPropsPanel.Controls.Add(trackList);
            trackPropsPanel.Controls.Add(selectedTrackPropsPanel);
            trackPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            trackPropsPanel.Location = new System.Drawing.Point(784, 24);
            trackPropsPanel.Name = "trackPropsPanel";
            trackPropsPanel.Size = new System.Drawing.Size(395, 4361);
            trackPropsPanel.TabIndex = 3;
            trackPropsPanel.Visible = false;
            trackPropsPanel.VisibleChanged += trackPropsPanel_VisibleChanged;
            // 
            // trackList
            // 
            trackList.AllowDrop = true;
            trackList.BackColor = System.Drawing.Color.Black;
            trackList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { trackColumn, normalColumn, hilitedColumn });
            trackList.ContextMenuStrip = trackListCM;
            trackList.Dock = System.Windows.Forms.DockStyle.Right;
            trackList.ForeColor = System.Drawing.Color.White;
            trackList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            trackList.Location = new System.Drawing.Point(1, 0);
            trackList.Name = "trackList";
            trackList.Size = new System.Drawing.Size(186, 4361);
            trackList.TabIndex = 0;
            trackList.UseCompatibleStateImageBehavior = false;
            trackList.View = System.Windows.Forms.View.Details;
            trackList.ItemDrag += trackList_ItemDrag;
            trackList.SelectedIndexChanged += trackList_SelectedIndexChanged;
            trackList.DragDrop += trackList_DragDrop;
            trackList.DragEnter += trackList_DragEnter;
            trackList.DragOver += trackList_DragOver;
            trackList.DragLeave += trackList_DragLeave;
            // 
            // trackColumn
            // 
            trackColumn.Text = "Track";
            trackColumn.Width = 112;
            // 
            // normalColumn
            // 
            normalColumn.Text = "N";
            normalColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            normalColumn.Width = 35;
            // 
            // hilitedColumn
            // 
            hilitedColumn.Text = "H";
            hilitedColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            hilitedColumn.Width = 35;
            // 
            // trackListCM
            // 
            trackListCM.ImageScalingSize = new System.Drawing.Size(24, 24);
            trackListCM.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { selectAllToolStripMenuItem, invertSelectionToolStripMenuItem, toolStripSeparator1, defaultPropertiesToolStripMenuItem, loadTrackPropsToolStripMenuItem, saveTrackPropsToolStripMenuItem });
            trackListCM.Name = "trackListContextMenu";
            trackListCM.Size = new System.Drawing.Size(193, 120);
            trackListCM.Opening += trackListCM_Opening;
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            selectAllToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            selectAllToolStripMenuItem.Text = "Select All";
            selectAllToolStripMenuItem.Click += selectAllToolStripMenuItem_Click;
            // 
            // invertSelectionToolStripMenuItem
            // 
            invertSelectionToolStripMenuItem.Name = "invertSelectionToolStripMenuItem";
            invertSelectionToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I;
            invertSelectionToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            invertSelectionToolStripMenuItem.Text = "Invert Selection";
            invertSelectionToolStripMenuItem.Click += invertSelectionToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(189, 6);
            // 
            // defaultPropertiesToolStripMenuItem
            // 
            defaultPropertiesToolStripMenuItem.Name = "defaultPropertiesToolStripMenuItem";
            defaultPropertiesToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            defaultPropertiesToolStripMenuItem.Text = "Default Properties";
            defaultPropertiesToolStripMenuItem.Click += defaultPropertiesToolStripMenuItem_Click;
            // 
            // loadTrackPropsToolStripMenuItem
            // 
            loadTrackPropsToolStripMenuItem.Name = "loadTrackPropsToolStripMenuItem";
            loadTrackPropsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            loadTrackPropsToolStripMenuItem.Text = "Load Properties";
            loadTrackPropsToolStripMenuItem.Click += loadTrackPropsToolStripMenuItem_Click;
            // 
            // saveTrackPropsToolStripMenuItem
            // 
            saveTrackPropsToolStripMenuItem.Name = "saveTrackPropsToolStripMenuItem";
            saveTrackPropsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            saveTrackPropsToolStripMenuItem.Text = "Save Properties";
            saveTrackPropsToolStripMenuItem.Click += saveTrackPropsToolStripMenuItem_Click;
            // 
            // selectedTrackPropsPanel
            // 
            selectedTrackPropsPanel.Controls.Add(style);
            selectedTrackPropsPanel.Controls.Add(material);
            selectedTrackPropsPanel.Controls.Add(light);
            selectedTrackPropsPanel.Controls.Add(spatial);
            selectedTrackPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            selectedTrackPropsPanel.Location = new System.Drawing.Point(187, 0);
            selectedTrackPropsPanel.Name = "selectedTrackPropsPanel";
            selectedTrackPropsPanel.SelectedIndex = 0;
            selectedTrackPropsPanel.Size = new System.Drawing.Size(208, 4361);
            selectedTrackPropsPanel.TabIndex = 14;
            // 
            // style
            // 
            style.AutoScroll = true;
            style.BackColor = System.Drawing.SystemColors.Control;
            style.Controls.Add(lineStyleControl);
            style.Controls.Add(barStyleControl);
            style.Controls.Add(defaultStyleBtn);
            style.Controls.Add(styleList);
            style.Controls.Add(label1);
            style.Location = new System.Drawing.Point(4, 24);
            style.Name = "style";
            style.Size = new System.Drawing.Size(200, 4333);
            style.TabIndex = 2;
            style.Text = "Style";
            // 
            // lineStyleControl
            // 
            lineStyleControl.AutoSize = true;
            lineStyleControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            lineStyleControl.Location = new System.Drawing.Point(1, 72);
            lineStyleControl.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            lineStyleControl.Name = "lineStyleControl";
            lineStyleControl.Size = new System.Drawing.Size(216, 442);
            lineStyleControl.TabIndex = 2;
            lineStyleControl.Visible = false;
            // 
            // barStyleControl
            // 
            barStyleControl.AutoSize = true;
            barStyleControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            barStyleControl.Location = new System.Drawing.Point(3, 72);
            barStyleControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            barStyleControl.Name = "barStyleControl";
            barStyleControl.Size = new System.Drawing.Size(211, 69);
            barStyleControl.TabIndex = 2;
            // 
            // defaultStyleBtn
            // 
            defaultStyleBtn.Location = new System.Drawing.Point(1, 1);
            defaultStyleBtn.Name = "defaultStyleBtn";
            defaultStyleBtn.Size = new System.Drawing.Size(179, 23);
            defaultStyleBtn.TabIndex = 0;
            defaultStyleBtn.Tag = "Default Style";
            defaultStyleBtn.Text = "Default Style";
            defaultStyleBtn.UseVisualStyleBackColor = true;
            defaultStyleBtn.Click += defaultStyleBtn_Click;
            // 
            // styleList
            // 
            styleList.DisplayMember = "Name";
            styleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            styleList.FormattingEnabled = true;
            styleList.Location = new System.Drawing.Point(4, 45);
            styleList.Name = "styleList";
            styleList.Size = new System.Drawing.Size(113, 23);
            styleList.TabIndex = 1;
            styleList.Tag = "Change Note Style";
            styleList.ValueMember = "Value";
            styleList.SelectedIndexChanged += styleList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(1, 29);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(60, 15);
            label1.TabIndex = 9;
            label1.Text = "Note style";
            // 
            // material
            // 
            material.AutoScroll = true;
            material.BackColor = System.Drawing.Color.Transparent;
            material.Controls.Add(groupBox4);
            material.Controls.Add(defaultMtrlBtn);
            material.Controls.Add(alphaLbl);
            material.Controls.Add(transpSlider);
            material.Controls.Add(hueTb);
            material.Controls.Add(groupBox1);
            material.Controls.Add(hueSlider);
            material.Controls.Add(label2);
            material.Controls.Add(groupBox2);
            material.Controls.Add(transpTb);
            material.Location = new System.Drawing.Point(4, 24);
            material.Name = "material";
            material.Padding = new System.Windows.Forms.Padding(3);
            material.Size = new System.Drawing.Size(192, 72);
            material.TabIndex = 0;
            material.Text = "Material";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(label37);
            groupBox4.Controls.Add(label34);
            groupBox4.Controls.Add(label35);
            groupBox4.Controls.Add(texVScrollUD);
            groupBox4.Controls.Add(label33);
            groupBox4.Controls.Add(texUScrollUD);
            groupBox4.Controls.Add(tableLayoutPanel1);
            groupBox4.Controls.Add(texVTileCb);
            groupBox4.Controls.Add(texUTileCb);
            groupBox4.Controls.Add(texKeepAspectCb);
            groupBox4.Controls.Add(tileTexCb);
            groupBox4.Controls.Add(loadTexBtn);
            groupBox4.Controls.Add(unloadTexBtn);
            groupBox4.Controls.Add(disableTextureCh);
            groupBox4.Controls.Add(texColBlendCb);
            groupBox4.Controls.Add(pointSmpCb);
            groupBox4.Controls.Add(trackTexPb);
            groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupBox4.Location = new System.Drawing.Point(1, 485);
            groupBox4.Margin = new System.Windows.Forms.Padding(2);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(2);
            groupBox4.Size = new System.Drawing.Size(182, 422);
            groupBox4.TabIndex = 60;
            groupBox4.TabStop = false;
            groupBox4.Text = "Texture";
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label37.Location = new System.Drawing.Point(113, 400);
            label37.Name = "label37";
            label37.Size = new System.Drawing.Size(68, 13);
            label37.TabIndex = 19;
            label37.Text = "repeats/beat";
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label34.Location = new System.Drawing.Point(113, 380);
            label34.Name = "label34";
            label34.Size = new System.Drawing.Size(68, 13);
            label34.TabIndex = 19;
            label34.Text = "repeats/beat";
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label35.Location = new System.Drawing.Point(3, 400);
            label35.Name = "label35";
            label35.Size = new System.Drawing.Size(43, 13);
            label35.TabIndex = 19;
            label35.Text = "Scroll V";
            // 
            // texVScrollUD
            // 
            texVScrollUD.DecimalPlaces = 3;
            texVScrollUD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            texVScrollUD.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            texVScrollUD.Location = new System.Drawing.Point(52, 398);
            texVScrollUD.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            texVScrollUD.Minimum = new decimal(new int[] { 10, 0, 0, int.MinValue });
            texVScrollUD.Name = "texVScrollUD";
            texVScrollUD.Size = new System.Drawing.Size(55, 20);
            texVScrollUD.TabIndex = 51;
            texVScrollUD.Tag = "Edit Scroll V";
            texVScrollUD.ValueChanged += texVScrollUD_ValueChanged;
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label33.Location = new System.Drawing.Point(2, 380);
            label33.Name = "label33";
            label33.Size = new System.Drawing.Size(44, 13);
            label33.TabIndex = 19;
            label33.Text = "Scroll U";
            // 
            // texUScrollUD
            // 
            texUScrollUD.DecimalPlaces = 3;
            texUScrollUD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            texUScrollUD.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            texUScrollUD.Location = new System.Drawing.Point(52, 378);
            texUScrollUD.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            texUScrollUD.Minimum = new decimal(new int[] { 10, 0, 0, int.MinValue });
            texUScrollUD.Name = "texUScrollUD";
            texUScrollUD.Size = new System.Drawing.Size(55, 20);
            texUScrollUD.TabIndex = 50;
            texUScrollUD.Tag = "Edit Scroll U";
            texUScrollUD.ValueChanged += texUScrollUD_ValueChanged;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.Controls.Add(label31, 2, 0);
            tableLayoutPanel1.Controls.Add(label32, 1, 0);
            tableLayoutPanel1.Controls.Add(label36, 0, 0);
            tableLayoutPanel1.Controls.Add(songAnchorLabel, 0, 3);
            tableLayoutPanel1.Controls.Add(noteAnchorLabel, 0, 1);
            tableLayoutPanel1.Controls.Add(panel2, 1, 1);
            tableLayoutPanel1.Controls.Add(screenAnchorLabel, 0, 2);
            tableLayoutPanel1.Controls.Add(panel1, 2, 1);
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 279);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new System.Drawing.Size(112, 93);
            tableLayoutPanel1.TabIndex = 40;
            // 
            // label31
            // 
            label31.Anchor = System.Windows.Forms.AnchorStyles.None;
            label31.AutoSize = true;
            label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label31.Location = new System.Drawing.Point(92, 6);
            label31.Name = "label31";
            label31.Size = new System.Drawing.Size(14, 13);
            label31.TabIndex = 17;
            label31.Text = "V";
            // 
            // label32
            // 
            label32.Anchor = System.Windows.Forms.AnchorStyles.None;
            label32.AutoSize = true;
            label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label32.Location = new System.Drawing.Point(69, 6);
            label32.Name = "label32";
            label32.Size = new System.Drawing.Size(14, 13);
            label32.TabIndex = 17;
            label32.Text = "U";
            // 
            // label36
            // 
            label36.Anchor = System.Windows.Forms.AnchorStyles.None;
            label36.AutoSize = true;
            label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label36.Location = new System.Drawing.Point(12, 6);
            label36.Name = "label36";
            label36.Size = new System.Drawing.Size(41, 13);
            label36.TabIndex = 17;
            label36.Text = "Anchor";
            // 
            // songAnchorLabel
            // 
            songAnchorLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            songAnchorLabel.AutoSize = true;
            songAnchorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            songAnchorLabel.Location = new System.Drawing.Point(6, 69);
            songAnchorLabel.Name = "songAnchorLabel";
            songAnchorLabel.Size = new System.Drawing.Size(54, 21);
            songAnchorLabel.TabIndex = 17;
            songAnchorLabel.Tag = "Song anchor";
            songAnchorLabel.Text = "Song start";
            songAnchorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            songAnchorLabel.Click += songAnchorLabel_Click;
            // 
            // noteAnchorLabel
            // 
            noteAnchorLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            noteAnchorLabel.AutoSize = true;
            noteAnchorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            noteAnchorLabel.Location = new System.Drawing.Point(6, 25);
            noteAnchorLabel.Name = "noteAnchorLabel";
            noteAnchorLabel.Size = new System.Drawing.Size(54, 19);
            noteAnchorLabel.TabIndex = 1;
            noteAnchorLabel.Tag = "Note anchor";
            noteAnchorLabel.Text = "Note";
            noteAnchorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            noteAnchorLabel.Click += noteAnchorLabel_Click;
            // 
            // panel2
            // 
            panel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel2.BackColor = System.Drawing.Color.Transparent;
            panel2.Controls.Add(songAnchorRb);
            panel2.Controls.Add(screenUAnchorRb);
            panel2.Controls.Add(noteUAnchorRb);
            panel2.Location = new System.Drawing.Point(66, 25);
            panel2.Margin = new System.Windows.Forms.Padding(0);
            panel2.Name = "panel2";
            tableLayoutPanel1.SetRowSpan(panel2, 3);
            panel2.Size = new System.Drawing.Size(20, 65);
            panel2.TabIndex = 20;
            // 
            // songAnchorRb
            // 
            songAnchorRb.AutoSize = true;
            songAnchorRb.Location = new System.Drawing.Point(3, 48);
            songAnchorRb.Name = "songAnchorRb";
            songAnchorRb.Size = new System.Drawing.Size(14, 13);
            songAnchorRb.TabIndex = 24;
            songAnchorRb.TabStop = true;
            songAnchorRb.Tag = "Song anchor";
            songAnchorRb.UseVisualStyleBackColor = true;
            songAnchorRb.Click += songAnchorRb_Click;
            // 
            // screenUAnchorRb
            // 
            screenUAnchorRb.AutoSize = true;
            screenUAnchorRb.Location = new System.Drawing.Point(3, 25);
            screenUAnchorRb.Name = "screenUAnchorRb";
            screenUAnchorRb.Size = new System.Drawing.Size(14, 13);
            screenUAnchorRb.TabIndex = 22;
            screenUAnchorRb.TabStop = true;
            screenUAnchorRb.Tag = "Screen U anchor";
            screenUAnchorRb.UseVisualStyleBackColor = true;
            screenUAnchorRb.Click += screenUAnchorRb_Click;
            // 
            // noteUAnchorRb
            // 
            noteUAnchorRb.AutoSize = true;
            noteUAnchorRb.Location = new System.Drawing.Point(3, 3);
            noteUAnchorRb.Name = "noteUAnchorRb";
            noteUAnchorRb.Size = new System.Drawing.Size(14, 13);
            noteUAnchorRb.TabIndex = 20;
            noteUAnchorRb.TabStop = true;
            noteUAnchorRb.Tag = "Note U anchor";
            noteUAnchorRb.UseVisualStyleBackColor = true;
            noteUAnchorRb.Click += noteUAnchorRb_Click;
            // 
            // screenAnchorLabel
            // 
            screenAnchorLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            screenAnchorLabel.AutoSize = true;
            screenAnchorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            screenAnchorLabel.Location = new System.Drawing.Point(6, 47);
            screenAnchorLabel.Name = "screenAnchorLabel";
            screenAnchorLabel.Size = new System.Drawing.Size(54, 19);
            screenAnchorLabel.TabIndex = 17;
            screenAnchorLabel.Tag = "Screen anchor";
            screenAnchorLabel.Text = "Screen";
            screenAnchorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            screenAnchorLabel.Click += screenAnchorLabel_Click;
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.BackColor = System.Drawing.Color.Transparent;
            panel1.Controls.Add(screenVAnchorRb);
            panel1.Controls.Add(noteVAnchorRb);
            panel1.Location = new System.Drawing.Point(89, 25);
            panel1.Margin = new System.Windows.Forms.Padding(0);
            panel1.Name = "panel1";
            tableLayoutPanel1.SetRowSpan(panel1, 3);
            panel1.Size = new System.Drawing.Size(20, 65);
            panel1.TabIndex = 21;
            // 
            // screenVAnchorRb
            // 
            screenVAnchorRb.AutoSize = true;
            screenVAnchorRb.Location = new System.Drawing.Point(3, 25);
            screenVAnchorRb.Name = "screenVAnchorRb";
            screenVAnchorRb.Size = new System.Drawing.Size(14, 13);
            screenVAnchorRb.TabIndex = 23;
            screenVAnchorRb.TabStop = true;
            screenVAnchorRb.Tag = "Screen V anchor";
            screenVAnchorRb.UseVisualStyleBackColor = true;
            screenVAnchorRb.Click += screenVAnchorRb_Click;
            // 
            // noteVAnchorRb
            // 
            noteVAnchorRb.AutoSize = true;
            noteVAnchorRb.Location = new System.Drawing.Point(3, 3);
            noteVAnchorRb.Name = "noteVAnchorRb";
            noteVAnchorRb.Size = new System.Drawing.Size(14, 13);
            noteVAnchorRb.TabIndex = 21;
            noteVAnchorRb.TabStop = true;
            noteVAnchorRb.Tag = "Note V anchor";
            noteVAnchorRb.UseVisualStyleBackColor = true;
            noteVAnchorRb.Click += noteVAnchorRb_Click;
            // 
            // texVTileCb
            // 
            texVTileCb.AutoSize = true;
            texVTileCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            texVTileCb.Location = new System.Drawing.Point(91, 236);
            texVTileCb.Name = "texVTileCb";
            texVTileCb.Size = new System.Drawing.Size(33, 17);
            texVTileCb.TabIndex = 32;
            texVTileCb.Tag = "V";
            texVTileCb.Text = "V";
            texVTileCb.UseVisualStyleBackColor = true;
            texVTileCb.CheckedChanged += texVTileCb_CheckedChanged;
            // 
            // texUTileCb
            // 
            texUTileCb.AutoSize = true;
            texUTileCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            texUTileCb.Location = new System.Drawing.Point(52, 236);
            texUTileCb.Name = "texUTileCb";
            texUTileCb.Size = new System.Drawing.Size(34, 17);
            texUTileCb.TabIndex = 31;
            texUTileCb.Tag = "U";
            texUTileCb.Text = "U";
            texUTileCb.UseVisualStyleBackColor = true;
            texUTileCb.CheckedChanged += texUTileCb_CheckedChanged;
            // 
            // texKeepAspectCb
            // 
            texKeepAspectCb.AutoSize = true;
            texKeepAspectCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            texKeepAspectCb.Location = new System.Drawing.Point(3, 257);
            texKeepAspectCb.Margin = new System.Windows.Forms.Padding(2);
            texKeepAspectCb.Name = "texKeepAspectCb";
            texKeepAspectCb.Size = new System.Drawing.Size(109, 17);
            texKeepAspectCb.TabIndex = 33;
            texKeepAspectCb.Tag = "Keep aspect ratio";
            texKeepAspectCb.Text = "Keep aspect ratio";
            texKeepAspectCb.UseVisualStyleBackColor = true;
            texKeepAspectCb.CheckedChanged += texKeepAspect_CheckedChanged;
            // 
            // tileTexCb
            // 
            tileTexCb.AutoSize = true;
            tileTexCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tileTexCb.Location = new System.Drawing.Point(4, 236);
            tileTexCb.Margin = new System.Windows.Forms.Padding(2);
            tileTexCb.Name = "tileTexCb";
            tileTexCb.Size = new System.Drawing.Size(43, 17);
            tileTexCb.TabIndex = 30;
            tileTexCb.Tag = "Tile";
            tileTexCb.Text = "Tile";
            tileTexCb.UseVisualStyleBackColor = true;
            tileTexCb.CheckedChanged += tileTexCb_CheckedChanged;
            // 
            // loadTexBtn
            // 
            loadTexBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            loadTexBtn.Location = new System.Drawing.Point(5, 18);
            loadTexBtn.Name = "loadTexBtn";
            loadTexBtn.Size = new System.Drawing.Size(75, 23);
            loadTexBtn.TabIndex = 10;
            loadTexBtn.Tag = "Load texture";
            loadTexBtn.Text = "&Load texture";
            loadTexBtn.UseVisualStyleBackColor = true;
            loadTexBtn.Click += textureLoadBtn_Click;
            // 
            // unloadTexBtn
            // 
            unloadTexBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            unloadTexBtn.Location = new System.Drawing.Point(92, 18);
            unloadTexBtn.Name = "unloadTexBtn";
            unloadTexBtn.Size = new System.Drawing.Size(86, 23);
            unloadTexBtn.TabIndex = 15;
            unloadTexBtn.Tag = "Unload texture";
            unloadTexBtn.Text = "&Unload texture";
            unloadTexBtn.UseVisualStyleBackColor = true;
            unloadTexBtn.Click += unloadTexBtn_Click;
            // 
            // disableTextureCh
            // 
            disableTextureCh.AutoSize = true;
            disableTextureCh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            disableTextureCh.Location = new System.Drawing.Point(6, 173);
            disableTextureCh.Margin = new System.Windows.Forms.Padding(2);
            disableTextureCh.Name = "disableTextureCh";
            disableTextureCh.Size = new System.Drawing.Size(96, 17);
            disableTextureCh.TabIndex = 20;
            disableTextureCh.Tag = "Disable texture";
            disableTextureCh.Text = "Disable texture";
            disableTextureCh.UseVisualStyleBackColor = true;
            disableTextureCh.CheckedChanged += disableTextureCb_CheckedChanged;
            // 
            // texColBlendCb
            // 
            texColBlendCb.AutoSize = true;
            texColBlendCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            texColBlendCb.Location = new System.Drawing.Point(4, 215);
            texColBlendCb.Margin = new System.Windows.Forms.Padding(2);
            texColBlendCb.Name = "texColBlendCb";
            texColBlendCb.Size = new System.Drawing.Size(149, 17);
            texColBlendCb.TabIndex = 20;
            texColBlendCb.Tag = "Blend hue with track color";
            texColBlendCb.Text = "Blend hue with track color";
            texColBlendCb.UseVisualStyleBackColor = true;
            texColBlendCb.CheckedChanged += texColBlendCb_CheckedChanged;
            // 
            // pointSmpCb
            // 
            pointSmpCb.AutoSize = true;
            pointSmpCb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            pointSmpCb.Location = new System.Drawing.Point(6, 194);
            pointSmpCb.Margin = new System.Windows.Forms.Padding(2);
            pointSmpCb.Name = "pointSmpCb";
            pointSmpCb.Size = new System.Drawing.Size(94, 17);
            pointSmpCb.TabIndex = 20;
            pointSmpCb.Tag = "Point sampling";
            pointSmpCb.Text = "Point sampling";
            pointSmpCb.UseVisualStyleBackColor = true;
            pointSmpCb.CheckedChanged += pointSmpCb_CheckedChanged;
            // 
            // trackTexPb
            // 
            trackTexPb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            trackTexPb.Location = new System.Drawing.Point(5, 47);
            trackTexPb.Name = "trackTexPb";
            trackTexPb.Size = new System.Drawing.Size(173, 121);
            trackTexPb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            trackTexPb.TabIndex = 13;
            trackTexPb.TabStop = false;
            // 
            // defaultMtrlBtn
            // 
            defaultMtrlBtn.Location = new System.Drawing.Point(1, 1);
            defaultMtrlBtn.Name = "defaultMtrlBtn";
            defaultMtrlBtn.Size = new System.Drawing.Size(179, 23);
            defaultMtrlBtn.TabIndex = 10;
            defaultMtrlBtn.Tag = "Default Material";
            defaultMtrlBtn.Text = "Default Material";
            defaultMtrlBtn.UseVisualStyleBackColor = true;
            defaultMtrlBtn.Click += defaultMaterial_Click;
            // 
            // alphaLbl
            // 
            alphaLbl.AutoSize = true;
            alphaLbl.Location = new System.Drawing.Point(5, 32);
            alphaLbl.Name = "alphaLbl";
            alphaLbl.Size = new System.Drawing.Size(48, 15);
            alphaLbl.TabIndex = 7;
            alphaLbl.Text = "Opacity";
            // 
            // transpSlider
            // 
            transpSlider.BackColor = System.Drawing.SystemColors.Control;
            transpSlider.LargeChange = 10;
            transpSlider.Location = new System.Drawing.Point(5, 49);
            transpSlider.Maximum = 200;
            transpSlider.Name = "transpSlider";
            transpSlider.Size = new System.Drawing.Size(129, 45);
            transpSlider.TabIndex = 20;
            transpSlider.TickFrequency = 10;
            transpSlider.Value = 50;
            transpSlider.Scroll += transpSlider_Scroll;
            // 
            // hueTb
            // 
            hueTb.Location = new System.Drawing.Point(137, 113);
            hueTb.Name = "hueTb";
            hueTb.Size = new System.Drawing.Size(38, 23);
            hueTb.TabIndex = 31;
            hueTb.Tag = "Edit Hue";
            hueTb.Text = "notset";
            hueTb.TextChanged += hueTb_TextChanged;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = System.Drawing.Color.Transparent;
            groupBox1.Controls.Add(normalLumTb);
            groupBox1.Controls.Add(normalLumSlider);
            groupBox1.Controls.Add(label);
            groupBox1.Controls.Add(normalSatTb);
            groupBox1.Controls.Add(normalSatSlider);
            groupBox1.Controls.Add(label3);
            groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupBox1.Location = new System.Drawing.Point(-1, 164);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(182, 154);
            groupBox1.TabIndex = 40;
            groupBox1.TabStop = false;
            groupBox1.Text = "Normal notes";
            // 
            // normalLumTb
            // 
            normalLumTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            normalLumTb.Location = new System.Drawing.Point(141, 105);
            normalLumTb.Name = "normalLumTb";
            normalLumTb.Size = new System.Drawing.Size(38, 20);
            normalLumTb.TabIndex = 21;
            normalLumTb.Tag = "Edit Normal Brightness";
            normalLumTb.Text = "notset";
            normalLumTb.TextChanged += normalLumTb_TextChanged;
            // 
            // normalLumSlider
            // 
            normalLumSlider.LargeChange = 10;
            normalLumSlider.Location = new System.Drawing.Point(6, 105);
            normalLumSlider.Maximum = 200;
            normalLumSlider.Name = "normalLumSlider";
            normalLumSlider.Size = new System.Drawing.Size(129, 45);
            normalLumSlider.TabIndex = 20;
            normalLumSlider.TickFrequency = 10;
            normalLumSlider.Value = 50;
            normalLumSlider.Scroll += normalLumSlider_Scroll;
            // 
            // label
            // 
            label.AutoSize = true;
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label.Location = new System.Drawing.Point(6, 89);
            label.Name = "label";
            label.Size = new System.Drawing.Size(56, 13);
            label.TabIndex = 13;
            label.Text = "Brightness";
            // 
            // normalSatTb
            // 
            normalSatTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            normalSatTb.Location = new System.Drawing.Point(141, 41);
            normalSatTb.Name = "normalSatTb";
            normalSatTb.Size = new System.Drawing.Size(38, 20);
            normalSatTb.TabIndex = 11;
            normalSatTb.Tag = "Edit Normal Saturation";
            normalSatTb.Text = "notset";
            normalSatTb.TextChanged += normalSatTb_TextChanged;
            // 
            // normalSatSlider
            // 
            normalSatSlider.LargeChange = 10;
            normalSatSlider.Location = new System.Drawing.Point(6, 41);
            normalSatSlider.Maximum = 200;
            normalSatSlider.Name = "normalSatSlider";
            normalSatSlider.Size = new System.Drawing.Size(129, 45);
            normalSatSlider.TabIndex = 10;
            normalSatSlider.TickFrequency = 10;
            normalSatSlider.Value = 50;
            normalSatSlider.Scroll += normalSatSlider_Scroll;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(6, 25);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(55, 13);
            label3.TabIndex = 10;
            label3.Text = "Saturation";
            // 
            // hueSlider
            // 
            hueSlider.BackColor = System.Drawing.SystemColors.Control;
            hueSlider.LargeChange = 10;
            hueSlider.Location = new System.Drawing.Point(5, 112);
            hueSlider.Maximum = 100;
            hueSlider.Name = "hueSlider";
            hueSlider.Size = new System.Drawing.Size(129, 45);
            hueSlider.TabIndex = 30;
            hueSlider.TickFrequency = 10;
            hueSlider.Value = 50;
            hueSlider.Scroll += hueSlider_Scroll;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(5, 96);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(29, 15);
            label2.TabIndex = 7;
            label2.Text = "Hue";
            // 
            // groupBox2
            // 
            groupBox2.BackColor = System.Drawing.Color.Transparent;
            groupBox2.Controls.Add(hiliteLumTb);
            groupBox2.Controls.Add(hiliteLumSlider);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(hiliteSatTb);
            groupBox2.Controls.Add(hiliteSatSlider);
            groupBox2.Controls.Add(label6);
            groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupBox2.Location = new System.Drawing.Point(-1, 325);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(182, 155);
            groupBox2.TabIndex = 50;
            groupBox2.TabStop = false;
            groupBox2.Text = "Highlighted notes";
            // 
            // hiliteLumTb
            // 
            hiliteLumTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            hiliteLumTb.Location = new System.Drawing.Point(141, 105);
            hiliteLumTb.Name = "hiliteLumTb";
            hiliteLumTb.Size = new System.Drawing.Size(38, 20);
            hiliteLumTb.TabIndex = 21;
            hiliteLumTb.Tag = "Edit Highlight Brightness";
            hiliteLumTb.Text = "notset";
            hiliteLumTb.TextChanged += hiliteLumTb_TextChanged;
            // 
            // hiliteLumSlider
            // 
            hiliteLumSlider.LargeChange = 10;
            hiliteLumSlider.Location = new System.Drawing.Point(6, 105);
            hiliteLumSlider.Maximum = 200;
            hiliteLumSlider.Name = "hiliteLumSlider";
            hiliteLumSlider.Size = new System.Drawing.Size(129, 45);
            hiliteLumSlider.TabIndex = 20;
            hiliteLumSlider.TickFrequency = 10;
            hiliteLumSlider.Value = 50;
            hiliteLumSlider.Scroll += hiliteLumSlider_Scroll;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label5.Location = new System.Drawing.Point(6, 89);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(56, 13);
            label5.TabIndex = 13;
            label5.Text = "Brightness";
            // 
            // hiliteSatTb
            // 
            hiliteSatTb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            hiliteSatTb.Location = new System.Drawing.Point(141, 41);
            hiliteSatTb.Name = "hiliteSatTb";
            hiliteSatTb.Size = new System.Drawing.Size(38, 20);
            hiliteSatTb.TabIndex = 11;
            hiliteSatTb.Tag = "Edit Highlight Saturation";
            hiliteSatTb.Text = "notset";
            hiliteSatTb.TextChanged += hiliteSatTb_TextChanged;
            // 
            // hiliteSatSlider
            // 
            hiliteSatSlider.LargeChange = 10;
            hiliteSatSlider.Location = new System.Drawing.Point(6, 41);
            hiliteSatSlider.Maximum = 200;
            hiliteSatSlider.Name = "hiliteSatSlider";
            hiliteSatSlider.Size = new System.Drawing.Size(129, 45);
            hiliteSatSlider.TabIndex = 10;
            hiliteSatSlider.TickFrequency = 10;
            hiliteSatSlider.Value = 50;
            hiliteSatSlider.Scroll += hiliteSatSlider_Scroll;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label6.Location = new System.Drawing.Point(6, 25);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(55, 13);
            label6.TabIndex = 10;
            label6.Text = "Saturation";
            // 
            // transpTb
            // 
            transpTb.Location = new System.Drawing.Point(137, 49);
            transpTb.Name = "transpTb";
            transpTb.Size = new System.Drawing.Size(38, 23);
            transpTb.TabIndex = 21;
            transpTb.Tag = "Edit Opacity";
            transpTb.Text = "notset";
            transpTb.TextChanged += transpTb_TextChanged;
            // 
            // light
            // 
            light.AutoScroll = true;
            light.BackColor = System.Drawing.SystemColors.Control;
            light.Controls.Add(defaultLightBtn);
            light.Controls.Add(lightPanel);
            light.Controls.Add(globalLightCb);
            light.Location = new System.Drawing.Point(4, 24);
            light.Name = "light";
            light.Padding = new System.Windows.Forms.Padding(3);
            light.Size = new System.Drawing.Size(192, 72);
            light.TabIndex = 1;
            light.Text = "Light";
            // 
            // defaultLightBtn
            // 
            defaultLightBtn.Location = new System.Drawing.Point(1, 1);
            defaultLightBtn.Name = "defaultLightBtn";
            defaultLightBtn.Size = new System.Drawing.Size(179, 23);
            defaultLightBtn.TabIndex = 10;
            defaultLightBtn.Tag = "Default Light";
            defaultLightBtn.Text = "Default Light";
            defaultLightBtn.UseVisualStyleBackColor = true;
            defaultLightBtn.Click += defaultLightBtn_Click;
            // 
            // lightPanel
            // 
            lightPanel.AutoSize = true;
            lightPanel.Controls.Add(masterLightHsBtn);
            lightPanel.Controls.Add(specHsBtn);
            lightPanel.Controls.Add(diffuseHsBtn);
            lightPanel.Controls.Add(ambientHsBtn);
            lightPanel.Controls.Add(specPowUd);
            lightPanel.Controls.Add(ambientAmountUd);
            lightPanel.Controls.Add(diffuseAmountUd);
            lightPanel.Controls.Add(label24);
            lightPanel.Controls.Add(masterLightAmountUd);
            lightPanel.Controls.Add(specAmountUd);
            lightPanel.Controls.Add(label15);
            lightPanel.Controls.Add(label14);
            lightPanel.Controls.Add(label16);
            lightPanel.Controls.Add(label22);
            lightPanel.Controls.Add(label23);
            lightPanel.Controls.Add(lightDirXUd);
            lightPanel.Controls.Add(label11);
            lightPanel.Controls.Add(lightDirYUd);
            lightPanel.Controls.Add(lightDirZUd);
            lightPanel.Controls.Add(label12);
            lightPanel.Controls.Add(label13);
            lightPanel.Enabled = false;
            lightPanel.Location = new System.Drawing.Point(2, 51);
            lightPanel.Name = "lightPanel";
            lightPanel.Size = new System.Drawing.Size(179, 223);
            lightPanel.TabIndex = 30;
            // 
            // masterLightHsBtn
            // 
            masterLightHsBtn.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            masterLightHsBtn.Hue = 0F;
            masterLightHsBtn.Location = new System.Drawing.Point(116, 182);
            masterLightHsBtn.Name = "masterLightHsBtn";
            masterLightHsBtn.Saturation = 0F;
            masterLightHsBtn.SelectedColor = System.Drawing.Color.FromArgb(255, 255, 255);
            masterLightHsBtn.Size = new System.Drawing.Size(53, 20);
            masterLightHsBtn.TabIndex = 59;
            masterLightHsBtn.Tag = "Edit Master color";
            masterLightHsBtn.UseVisualStyleBackColor = true;
            masterLightHsBtn.ColorChanged += masterLightHsBtn_ColorChanged;
            // 
            // specHsBtn
            // 
            specHsBtn.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            specHsBtn.Hue = 0F;
            specHsBtn.Location = new System.Drawing.Point(116, 130);
            specHsBtn.Name = "specHsBtn";
            specHsBtn.Saturation = 0F;
            specHsBtn.SelectedColor = System.Drawing.Color.FromArgb(255, 255, 255);
            specHsBtn.Size = new System.Drawing.Size(53, 20);
            specHsBtn.TabIndex = 59;
            specHsBtn.Tag = "Edit Specular color";
            specHsBtn.UseVisualStyleBackColor = true;
            specHsBtn.ColorChanged += specHsBtn_ColorChanged;
            // 
            // diffuseHsBtn
            // 
            diffuseHsBtn.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            diffuseHsBtn.Hue = 0F;
            diffuseHsBtn.Location = new System.Drawing.Point(116, 104);
            diffuseHsBtn.Name = "diffuseHsBtn";
            diffuseHsBtn.Saturation = 0F;
            diffuseHsBtn.SelectedColor = System.Drawing.Color.FromArgb(255, 255, 255);
            diffuseHsBtn.Size = new System.Drawing.Size(53, 20);
            diffuseHsBtn.TabIndex = 59;
            diffuseHsBtn.Tag = "Edit Diffuse  color";
            diffuseHsBtn.UseVisualStyleBackColor = true;
            diffuseHsBtn.ColorChanged += diffuseHsBtn_ColorChanged;
            // 
            // ambientHsBtn
            // 
            ambientHsBtn.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            ambientHsBtn.Hue = 0F;
            ambientHsBtn.Location = new System.Drawing.Point(116, 78);
            ambientHsBtn.Name = "ambientHsBtn";
            ambientHsBtn.Saturation = 0F;
            ambientHsBtn.SelectedColor = System.Drawing.Color.FromArgb(255, 255, 255);
            ambientHsBtn.Size = new System.Drawing.Size(53, 20);
            ambientHsBtn.TabIndex = 59;
            ambientHsBtn.Tag = "Edit Ambient color";
            ambientHsBtn.UseVisualStyleBackColor = true;
            ambientHsBtn.ColorChanged += ambientHsBtn_ColorChanged;
            // 
            // specPowUd
            // 
            specPowUd.Location = new System.Drawing.Point(59, 156);
            specPowUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            specPowUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            specPowUd.Name = "specPowUd";
            specPowUd.Size = new System.Drawing.Size(51, 23);
            specPowUd.TabIndex = 56;
            specPowUd.Tag = "Edit Specular power";
            specPowUd.ValueChanged += specPowUd_ValueChanged;
            // 
            // ambientAmountUd
            // 
            ambientAmountUd.DecimalPlaces = 2;
            ambientAmountUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            ambientAmountUd.Location = new System.Drawing.Point(59, 78);
            ambientAmountUd.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            ambientAmountUd.Name = "ambientAmountUd";
            ambientAmountUd.Size = new System.Drawing.Size(51, 23);
            ambientAmountUd.TabIndex = 53;
            ambientAmountUd.Tag = "Edit Ambient amount";
            ambientAmountUd.ValueChanged += ambientAmountUd_ValueChanged;
            // 
            // diffuseAmountUd
            // 
            diffuseAmountUd.DecimalPlaces = 2;
            diffuseAmountUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            diffuseAmountUd.Location = new System.Drawing.Point(59, 104);
            diffuseAmountUd.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            diffuseAmountUd.Name = "diffuseAmountUd";
            diffuseAmountUd.Size = new System.Drawing.Size(51, 23);
            diffuseAmountUd.TabIndex = 54;
            diffuseAmountUd.Tag = "Edit Diffuse amount";
            diffuseAmountUd.ValueChanged += diffuseAmountUd_ValueChanged;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new System.Drawing.Point(4, 14);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(55, 15);
            label24.TabIndex = 2;
            label24.Text = "Direction";
            // 
            // masterLightAmountUd
            // 
            masterLightAmountUd.DecimalPlaces = 2;
            masterLightAmountUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            masterLightAmountUd.Location = new System.Drawing.Point(59, 182);
            masterLightAmountUd.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            masterLightAmountUd.Name = "masterLightAmountUd";
            masterLightAmountUd.Size = new System.Drawing.Size(51, 23);
            masterLightAmountUd.TabIndex = 55;
            masterLightAmountUd.Tag = "Edit Master brightness";
            masterLightAmountUd.ValueChanged += MasterLightAmountUD_ValueChanged;
            // 
            // specAmountUd
            // 
            specAmountUd.DecimalPlaces = 2;
            specAmountUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            specAmountUd.Location = new System.Drawing.Point(59, 130);
            specAmountUd.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            specAmountUd.Name = "specAmountUd";
            specAmountUd.Size = new System.Drawing.Size(51, 23);
            specAmountUd.TabIndex = 55;
            specAmountUd.Tag = "Edit Specular amount";
            specAmountUd.ValueChanged += specAmountUd_ValueChanged;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(8, 80);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(53, 15);
            label15.TabIndex = 49;
            label15.Text = "Ambient";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(13, 108);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(44, 15);
            label14.TabIndex = 50;
            label14.Text = "Diffuse";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new System.Drawing.Point(14, 184);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(43, 15);
            label16.TabIndex = 52;
            label16.Text = "Master";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(7, 158);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(49, 15);
            label22.TabIndex = 52;
            label22.Text = "S power";
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new System.Drawing.Point(4, 132);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(52, 15);
            label23.TabIndex = 51;
            label23.Text = "Specular";
            // 
            // lightDirXUd
            // 
            lightDirXUd.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lightDirXUd.Location = new System.Drawing.Point(0, 30);
            lightDirXUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            lightDirXUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            lightDirXUd.Name = "lightDirXUd";
            lightDirXUd.Size = new System.Drawing.Size(57, 23);
            lightDirXUd.TabIndex = 34;
            lightDirXUd.Tag = "Edit Light X Direction";
            lightDirXUd.ValueChanged += lightDirXUd_ValueChanged;
            // 
            // label11
            // 
            label11.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(139, 53);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(14, 15);
            label11.TabIndex = 31;
            label11.Text = "Z";
            // 
            // lightDirYUd
            // 
            lightDirYUd.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lightDirYUd.Location = new System.Drawing.Point(60, 30);
            lightDirYUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            lightDirYUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            lightDirYUd.Name = "lightDirYUd";
            lightDirYUd.Size = new System.Drawing.Size(57, 23);
            lightDirYUd.TabIndex = 35;
            lightDirYUd.Tag = "Edit Light Y Direction";
            lightDirYUd.ValueChanged += lightDirYUd_ValueChanged;
            // 
            // lightDirZUd
            // 
            lightDirZUd.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lightDirZUd.Location = new System.Drawing.Point(120, 30);
            lightDirZUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            lightDirZUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            lightDirZUd.Name = "lightDirZUd";
            lightDirZUd.Size = new System.Drawing.Size(57, 23);
            lightDirZUd.TabIndex = 36;
            lightDirZUd.Tag = "Edit Light Z Direction";
            lightDirZUd.ValueChanged += lightDirZUd_ValueChanged;
            // 
            // label12
            // 
            label12.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(82, 53);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(14, 15);
            label12.TabIndex = 32;
            label12.Text = "Y";
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(17, 53);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(14, 15);
            label13.TabIndex = 33;
            label13.Text = "X";
            // 
            // globalLightCb
            // 
            globalLightCb.AutoSize = true;
            globalLightCb.Checked = true;
            globalLightCb.CheckState = System.Windows.Forms.CheckState.Checked;
            globalLightCb.Location = new System.Drawing.Point(3, 30);
            globalLightCb.Name = "globalLightCb";
            globalLightCb.Size = new System.Drawing.Size(81, 19);
            globalLightCb.TabIndex = 20;
            globalLightCb.Tag = "Use global";
            globalLightCb.Text = "Use global";
            globalLightCb.UseVisualStyleBackColor = true;
            globalLightCb.CheckedChanged += globalLightCb_CheckedChanged;
            // 
            // spatial
            // 
            spatial.Controls.Add(defaultSpatialBtn);
            spatial.Controls.Add(label41);
            spatial.Controls.Add(label40);
            spatial.Controls.Add(label39);
            spatial.Controls.Add(label20);
            spatial.Controls.Add(zoffsetUd);
            spatial.Controls.Add(yoffsetUd);
            spatial.Controls.Add(xoffsetUd);
            spatial.Location = new System.Drawing.Point(4, 24);
            spatial.Name = "spatial";
            spatial.Padding = new System.Windows.Forms.Padding(3);
            spatial.Size = new System.Drawing.Size(192, 72);
            spatial.TabIndex = 3;
            spatial.Text = "Spatial";
            spatial.UseVisualStyleBackColor = true;
            // 
            // defaultSpatialBtn
            // 
            defaultSpatialBtn.Location = new System.Drawing.Point(1, 1);
            defaultSpatialBtn.Name = "defaultSpatialBtn";
            defaultSpatialBtn.Size = new System.Drawing.Size(179, 23);
            defaultSpatialBtn.TabIndex = 10;
            defaultSpatialBtn.Tag = "Default Spatial";
            defaultSpatialBtn.Text = "Default Spatial";
            defaultSpatialBtn.UseVisualStyleBackColor = true;
            defaultSpatialBtn.Click += defaultSpatialBtn_Click;
            // 
            // label41
            // 
            label41.AutoSize = true;
            label41.Location = new System.Drawing.Point(151, 80);
            label41.Name = "label41";
            label41.Size = new System.Drawing.Size(14, 15);
            label41.TabIndex = 1;
            label41.Text = "Z";
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.Location = new System.Drawing.Point(87, 80);
            label40.Name = "label40";
            label40.Size = new System.Drawing.Size(14, 15);
            label40.TabIndex = 1;
            label40.Text = "Y";
            // 
            // label39
            // 
            label39.AutoSize = true;
            label39.Location = new System.Drawing.Point(24, 80);
            label39.Name = "label39";
            label39.Size = new System.Drawing.Size(14, 15);
            label39.TabIndex = 1;
            label39.Text = "X";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new System.Drawing.Point(-2, 41);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(83, 15);
            label20.TabIndex = 1;
            label20.Text = "Position offset";
            // 
            // zoffsetUd
            // 
            zoffsetUd.Location = new System.Drawing.Point(121, 57);
            zoffsetUd.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            zoffsetUd.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            zoffsetUd.Name = "zoffsetUd";
            zoffsetUd.Size = new System.Drawing.Size(57, 23);
            zoffsetUd.TabIndex = 40;
            zoffsetUd.Tag = "Edit Z Offset";
            zoffsetUd.ValueChanged += zoffsetUd_ValueChanged;
            // 
            // yoffsetUd
            // 
            yoffsetUd.Location = new System.Drawing.Point(61, 57);
            yoffsetUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            yoffsetUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            yoffsetUd.Name = "yoffsetUd";
            yoffsetUd.Size = new System.Drawing.Size(57, 23);
            yoffsetUd.TabIndex = 30;
            yoffsetUd.Tag = "Edit Y Offset";
            yoffsetUd.ValueChanged += yoffsetUd_ValueChanged;
            // 
            // xoffsetUd
            // 
            xoffsetUd.Location = new System.Drawing.Point(1, 57);
            xoffsetUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            xoffsetUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            xoffsetUd.Name = "xoffsetUd";
            xoffsetUd.Size = new System.Drawing.Size(57, 23);
            xoffsetUd.TabIndex = 20;
            xoffsetUd.Tag = "Edit X Offset";
            xoffsetUd.ValueChanged += xoffsetUd_ValueChanged;
            // 
            // resetDefaultBtn
            // 
            resetDefaultBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            resetDefaultBtn.Location = new System.Drawing.Point(336, 32767);
            resetDefaultBtn.Name = "resetDefaultBtn";
            resetDefaultBtn.Size = new System.Drawing.Size(170, 23);
            resetDefaultBtn.TabIndex = 13;
            resetDefaultBtn.Text = "Reset defaults to default";
            resetDefaultBtn.UseVisualStyleBackColor = true;
            resetDefaultBtn.Visible = false;
            // 
            // openTextureDlg
            // 
            openTextureDlg.Filter = "Textures|*.jpg;*.png;*.gif";
            // 
            // songPropsPanel
            // 
            songPropsPanel.Controls.Add(camLabel);
            songPropsPanel.Controls.Add(camTb);
            songPropsPanel.Controls.Add(defaultPitchesBtn);
            songPropsPanel.Controls.Add(minPitchUd);
            songPropsPanel.Controls.Add(maxPitchUd);
            songPropsPanel.Controls.Add(label26);
            songPropsPanel.Controls.Add(label21);
            songPropsPanel.Controls.Add(label18);
            songPropsPanel.Controls.Add(label10);
            songPropsPanel.Controls.Add(label8);
            songPropsPanel.Controls.Add(hnotelabel);
            songPropsPanel.Controls.Add(label25);
            songPropsPanel.Controls.Add(label19);
            songPropsPanel.Controls.Add(label17);
            songPropsPanel.Controls.Add(fadeOutUd);
            songPropsPanel.Controls.Add(fadeInUd);
            songPropsPanel.Controls.Add(label9);
            songPropsPanel.Controls.Add(playbackOffsetUd);
            songPropsPanel.Controls.Add(audioOffsetS);
            songPropsPanel.Controls.Add(upDownVpWidth);
            songPropsPanel.Controls.Add(label7);
            songPropsPanel.Dock = System.Windows.Forms.DockStyle.Left;
            songPropsPanel.Location = new System.Drawing.Point(0, 24);
            songPropsPanel.Name = "songPropsPanel";
            songPropsPanel.Size = new System.Drawing.Size(197, 4361);
            songPropsPanel.TabIndex = 17;
            songPropsPanel.Visible = false;
            songPropsPanel.VisibleChanged += songPropsPanel_VisibleChanged;
            // 
            // camLabel
            // 
            camLabel.AutoSize = true;
            camLabel.Location = new System.Drawing.Point(12, 320);
            camLabel.Name = "camLabel";
            camLabel.Size = new System.Drawing.Size(48, 15);
            camLabel.TabIndex = 7;
            camLabel.Text = "Camera";
            // 
            // camTb
            // 
            camTb.Location = new System.Drawing.Point(12, 336);
            camTb.Multiline = true;
            camTb.Name = "camTb";
            camTb.Size = new System.Drawing.Size(111, 120);
            camTb.TabIndex = 6;
            camTb.Tag = "Edit Camera";
            camTb.TextChanged += camTb_TextChanged;
            // 
            // defaultPitchesBtn
            // 
            defaultPitchesBtn.Location = new System.Drawing.Point(12, 272);
            defaultPitchesBtn.Name = "defaultPitchesBtn";
            defaultPitchesBtn.Size = new System.Drawing.Size(111, 23);
            defaultPitchesBtn.TabIndex = 4;
            defaultPitchesBtn.Tag = "Reset Pitches";
            defaultPitchesBtn.Text = "Reset pitches";
            defaultPitchesBtn.UseVisualStyleBackColor = true;
            defaultPitchesBtn.Click += defaultPitchesBtn_Click;
            // 
            // minPitchUd
            // 
            minPitchUd.Location = new System.Drawing.Point(71, 229);
            minPitchUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            minPitchUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            minPitchUd.Name = "minPitchUd";
            minPitchUd.Size = new System.Drawing.Size(52, 23);
            minPitchUd.TabIndex = 3;
            minPitchUd.Tag = "Edit Min Pitch";
            minPitchUd.ValueChanged += minPitchUd_ValueChanged;
            // 
            // maxPitchUd
            // 
            maxPitchUd.Location = new System.Drawing.Point(71, 203);
            maxPitchUd.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            maxPitchUd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            maxPitchUd.Name = "maxPitchUd";
            maxPitchUd.Size = new System.Drawing.Size(52, 23);
            maxPitchUd.TabIndex = 2;
            maxPitchUd.Tag = "Edit Max Pitch";
            maxPitchUd.ValueChanged += maxPitchUd_ValueChanged;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new System.Drawing.Point(156, 153);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(12, 15);
            label26.TabIndex = 3;
            label26.Text = "s";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new System.Drawing.Point(156, 127);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(12, 15);
            label21.TabIndex = 3;
            label21.Text = "s";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(156, 101);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(12, 15);
            label18.TabIndex = 3;
            label18.Text = "s";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(15, 231);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(58, 15);
            label8.TabIndex = 3;
            label8.Text = "Min pitch";
            // 
            // hnotelabel
            // 
            hnotelabel.AutoSize = true;
            hnotelabel.Location = new System.Drawing.Point(12, 205);
            hnotelabel.Name = "hnotelabel";
            hnotelabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            hnotelabel.Size = new System.Drawing.Size(60, 15);
            hnotelabel.TabIndex = 3;
            hnotelabel.Text = "Max pitch";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new System.Drawing.Point(12, 153);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(53, 15);
            label25.TabIndex = 3;
            label25.Text = "Fade out";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new System.Drawing.Point(12, 127);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(45, 15);
            label19.TabIndex = 3;
            label19.Text = "Fade in";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(12, 101);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(87, 15);
            label17.TabIndex = 3;
            label17.Text = "Playback offset";
            // 
            // fadeOutUd
            // 
            fadeOutUd.DecimalPlaces = 2;
            fadeOutUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            fadeOutUd.Location = new System.Drawing.Point(98, 151);
            fadeOutUd.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            fadeOutUd.Minimum = new decimal(new int[] { 99, 0, 0, int.MinValue });
            fadeOutUd.Name = "fadeOutUd";
            fadeOutUd.Size = new System.Drawing.Size(52, 23);
            fadeOutUd.TabIndex = 1;
            fadeOutUd.Tag = "Edit Fade Out";
            fadeOutUd.ValueChanged += fadeOutUd_ValueChanged;
            // 
            // fadeInUd
            // 
            fadeInUd.DecimalPlaces = 2;
            fadeInUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            fadeInUd.Location = new System.Drawing.Point(98, 125);
            fadeInUd.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            fadeInUd.Minimum = new decimal(new int[] { 99, 0, 0, int.MinValue });
            fadeInUd.Name = "fadeInUd";
            fadeInUd.Size = new System.Drawing.Size(52, 23);
            fadeInUd.TabIndex = 1;
            fadeInUd.Tag = "Edit Fade In";
            fadeInUd.ValueChanged += fadeInUd_ValueChanged;
            // 
            // playbackOffsetUd
            // 
            playbackOffsetUd.DecimalPlaces = 2;
            playbackOffsetUd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            playbackOffsetUd.Location = new System.Drawing.Point(98, 99);
            playbackOffsetUd.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            playbackOffsetUd.Minimum = new decimal(new int[] { 99, 0, 0, int.MinValue });
            playbackOffsetUd.Name = "playbackOffsetUd";
            playbackOffsetUd.Size = new System.Drawing.Size(52, 23);
            playbackOffsetUd.TabIndex = 1;
            playbackOffsetUd.Tag = "Edit Playback Offset";
            playbackOffsetUd.ValueChanged += playbackOffsetUd_ValueChanged;
            // 
            // upDownVpWidth
            // 
            upDownVpWidth.AutoSize = true;
            upDownVpWidth.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            upDownVpWidth.Decimals = 3;
            upDownVpWidth.Decimals2 = 2;
            upDownVpWidth.ExpBase = 2D;
            upDownVpWidth.Location = new System.Drawing.Point(12, 28);
            upDownVpWidth.Margin = new System.Windows.Forms.Padding(4);
            upDownVpWidth.Max = 10D;
            upDownVpWidth.Min = 0D;
            upDownVpWidth.Name = "upDownVpWidth";
            upDownVpWidth.Size = new System.Drawing.Size(202, 48);
            upDownVpWidth.TabIndex = 0;
            upDownVpWidth.TbWidth = 50;
            upDownVpWidth.TickFreq = 1D;
            upDownVpWidth.Value = 16D;
            upDownVpWidth.ValueChanged += upDownVpWidth_ValueChanged;
            upDownVpWidth.CommitChanges += upDownVpWidth_CommitChanges;
            // 
            // saveMixdownDialog
            // 
            saveMixdownDialog.Filter = "Wav files (*.wav)|*.wav|All files (*.*)|*.*";
            saveMixdownDialog.FileOk += SaveMixdownDialog_FileOk;
            // 
            // colorDialog1
            // 
            colorDialog1.AnyColor = true;
            colorDialog1.FullOpen = true;
            colorDialog1.SolidColorOnly = true;
            // 
            // saveMidiDialog
            // 
            saveMidiDialog.Filter = "Midi files|*.mid|All files|*.*";
            // 
            // trackPropsTabCM
            // 
            trackPropsTabCM.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { loadTrackPropsTypeToolStripMenuItem, saveTrackPtopsTypeToolStripMenuItem });
            trackPropsTabCM.Name = "trackPropsTabCM";
            trackPropsTabCM.Size = new System.Drawing.Size(157, 48);
            // 
            // loadTrackPropsTypeToolStripMenuItem
            // 
            loadTrackPropsTypeToolStripMenuItem.Name = "loadTrackPropsTypeToolStripMenuItem";
            loadTrackPropsTypeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            loadTrackPropsTypeToolStripMenuItem.Text = "Load properties";
            loadTrackPropsTypeToolStripMenuItem.Click += loadTrackPropsTypeToolStripMenuItem_Click;
            // 
            // saveTrackPtopsTypeToolStripMenuItem
            // 
            saveTrackPtopsTypeToolStripMenuItem.Name = "saveTrackPtopsTypeToolStripMenuItem";
            saveTrackPtopsTypeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            saveTrackPtopsTypeToolStripMenuItem.Text = "Save properties";
            saveTrackPtopsTypeToolStripMenuItem.Click += saveTrackPropsTypeToolStripMenuItem_Click;
            // 
            // lyricsGridView
            // 
            lyricsGridView.AllowUserToResizeRows = false;
            lyricsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            lyricsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { TimeColumn, LyricsColumn });
            lyricsGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
            lyricsGridView.Location = new System.Drawing.Point(197, 4335);
            lyricsGridView.Name = "lyricsGridView";
            lyricsGridView.Size = new System.Drawing.Size(587, 50);
            lyricsGridView.TabIndex = 0;
            lyricsGridView.Tag = "Edit Lyrics";
            lyricsGridView.Visible = false;
            lyricsGridView.CellEndEdit += lyricsGridView_CellEndEdit;
            lyricsGridView.CellValidating += lyricsGridView_CellValidating;
            lyricsGridView.DataError += lyricsGridView_DataError;
            lyricsGridView.RowsRemoved += lyricsGridView_RowsRemoved;
            lyricsGridView.Paint += lyricsGridView_Paint;
            // 
            // TimeColumn
            // 
            TimeColumn.DataPropertyName = "Time";
            dataGridViewCellStyle1.Format = "N2";
            dataGridViewCellStyle1.NullValue = "0";
            TimeColumn.DefaultCellStyle = dataGridViewCellStyle1;
            TimeColumn.HeaderText = "Time";
            TimeColumn.Name = "TimeColumn";
            TimeColumn.Width = 68;
            // 
            // LyricsColumn
            // 
            LyricsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            LyricsColumn.DataPropertyName = "Lyrics";
            LyricsColumn.HeaderText = "Lyrics";
            LyricsColumn.Name = "LyricsColumn";
            // 
            // keyFramesDGV
            // 
            keyFramesDGV.AllowUserToAddRows = false;
            keyFramesDGV.AllowUserToResizeRows = false;
            keyFramesDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            keyFramesDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Time, Description });
            keyFramesDGV.Dock = System.Windows.Forms.DockStyle.Left;
            keyFramesDGV.Location = new System.Drawing.Point(197, 24);
            keyFramesDGV.Name = "keyFramesDGV";
            keyFramesDGV.Size = new System.Drawing.Size(210, 4311);
            keyFramesDGV.TabIndex = 23;
            keyFramesDGV.Tag = "Edit Key Frames";
            keyFramesDGV.Visible = false;
            keyFramesDGV.CellEndEdit += keyFramesDGV_CellEndEdit;
            keyFramesDGV.CellValidating += keyFramesDGV_CellValidating;
            keyFramesDGV.CurrentCellChanged += keyFramesDGV_CurrentCellChanged;
            keyFramesDGV.RowsAdded += keyFramesDGV_RowsAdded;
            keyFramesDGV.RowsRemoved += keyFramesDGV_RowsRemoved;
            keyFramesDGV.SelectionChanged += keyFramesDGV_SelectionChanged;
            keyFramesDGV.UserDeletingRow += keyFramesDGV_UserDeletingRow;
            keyFramesDGV.Paint += keyFramesDGV_Paint;
            keyFramesDGV.KeyDown += keyFramesDGV_KeyDown;
            // 
            // Time
            // 
            Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.NullValue = null;
            Time.DefaultCellStyle = dataGridViewCellStyle2;
            Time.FillWeight = 35F;
            Time.HeaderText = "Time";
            Time.Name = "Time";
            Time.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Description
            // 
            Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            Description.FillWeight = 65F;
            Description.HeaderText = "Description";
            Description.Name = "Description";
            Description.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // propsTogglePanel
            // 
            propsTogglePanel.Controls.Add(songPropsCb);
            propsTogglePanel.Controls.Add(trackPropsCb);
            propsTogglePanel.Enabled = false;
            propsTogglePanel.Location = new System.Drawing.Point(276, 0);
            propsTogglePanel.Name = "propsTogglePanel";
            propsTogglePanel.Size = new System.Drawing.Size(207, 24);
            propsTogglePanel.TabIndex = 24;
            // 
            // songPropsCb
            // 
            songPropsCb.Appearance = System.Windows.Forms.Appearance.Button;
            songPropsCb.AutoSize = true;
            songPropsCb.Location = new System.Drawing.Point(3, 1);
            songPropsCb.Name = "songPropsCb";
            songPropsCb.Size = new System.Drawing.Size(110, 25);
            songPropsCb.TabIndex = 0;
            songPropsCb.Text = "Pr&oject properties";
            songPropsCb.UseVisualStyleBackColor = true;
            songPropsCb.CheckedChanged += songPropsCb_CheckedChanged;
            // 
            // trackPropsCb
            // 
            trackPropsCb.Appearance = System.Windows.Forms.Appearance.Button;
            trackPropsCb.AutoSize = true;
            trackPropsCb.FlatAppearance.CheckedBackColor = System.Drawing.Color.White;
            trackPropsCb.Location = new System.Drawing.Point(108, 2);
            trackPropsCb.Name = "trackPropsCb";
            trackPropsCb.Size = new System.Drawing.Size(100, 25);
            trackPropsCb.TabIndex = 1;
            trackPropsCb.Text = "&Track properties";
            trackPropsCb.UseVisualStyleBackColor = true;
            trackPropsCb.CheckedChanged += trackPropsCb_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(542, 4320);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(38, 15);
            label4.TabIndex = 25;
            label4.Text = "label4";
            label4.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoScroll = true;
            AutoSize = true;
            ClientSize = new System.Drawing.Size(1196, 421);
            Controls.Add(label4);
            Controls.Add(propsTogglePanel);
            Controls.Add(keyFramesDGV);
            Controls.Add(lyricsGridView);
            Controls.Add(songPropsPanel);
            Controls.Add(resetDefaultBtn);
            Controls.Add(trackPropsPanel);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Visual Music";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)audioOffsetS).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            trackPropsPanel.ResumeLayout(false);
            trackListCM.ResumeLayout(false);
            selectedTrackPropsPanel.ResumeLayout(false);
            style.ResumeLayout(false);
            style.PerformLayout();
            material.ResumeLayout(false);
            material.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)texVScrollUD).EndInit();
            ((System.ComponentModel.ISupportInitialize)texUScrollUD).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackTexPb).EndInit();
            ((System.ComponentModel.ISupportInitialize)transpSlider).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)normalLumSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)normalSatSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)hueSlider).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)hiliteLumSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)hiliteSatSlider).EndInit();
            light.ResumeLayout(false);
            light.PerformLayout();
            lightPanel.ResumeLayout(false);
            lightPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)specPowUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)ambientAmountUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)diffuseAmountUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)masterLightAmountUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)specAmountUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)lightDirXUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)lightDirYUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)lightDirZUd).EndInit();
            spatial.ResumeLayout(false);
            spatial.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)zoffsetUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)yoffsetUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)xoffsetUd).EndInit();
            songPropsPanel.ResumeLayout(false);
            songPropsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)minPitchUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)maxPitchUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)fadeOutUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)fadeInUd).EndInit();
            ((System.ComponentModel.ISupportInitialize)playbackOffsetUd).EndInit();
            trackPropsTabCM.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lyricsGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)keyFramesDGV).EndInit();
            propsTogglePanel.ResumeLayout(false);
            propsTogglePanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Button defaultMtrlBtn;
        private ListViewNF trackList;
        private System.Windows.Forms.ColumnHeader trackColumn;
        private System.Windows.Forms.ColumnHeader normalColumn;
        private System.Windows.Forms.ColumnHeader hilitedColumn;
        private System.Windows.Forms.Button unloadTexBtn;
        private System.Windows.Forms.Button loadTexBtn;
        private System.Windows.Forms.ToolStripMenuItem openSongToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSongToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSongAsToolStripMenuItem;
        private System.Windows.Forms.PictureBox trackTexPb;
        private System.Windows.Forms.TabControl selectedTrackPropsPanel;
        private System.Windows.Forms.TabPage material;
        private System.Windows.Forms.TabPage light;
        private System.Windows.Forms.ComboBox styleList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage style;
        private System.Windows.Forms.Panel lightPanel;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.CheckBox globalLightCb;
        private System.Windows.Forms.ContextMenuStrip trackListCM;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem defaultPropertiesToolStripMenuItem;
        private System.Windows.Forms.Button defaultStyleBtn;
        private System.Windows.Forms.Button defaultLightBtn;
        private TbSlider upDownVpWidth;
        private System.Windows.Forms.Panel songPropsPanel;
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
        private System.Windows.Forms.TabPage spatial;
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
        private System.Windows.Forms.NumericUpDown lightDirXUd;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown lightDirYUd;
        private System.Windows.Forms.NumericUpDown lightDirZUd;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewSongTSMI;
        private System.Windows.Forms.ToolStripMenuItem viewModBrowserTSMI;
        private System.Windows.Forms.ToolStripMenuItem viewSidBrowserTSMI;
        private System.Windows.Forms.TextBox camTb;
        private System.Windows.Forms.NumericUpDown specPowUd;
        private System.Windows.Forms.NumericUpDown ambientAmountUd;
        private System.Windows.Forms.NumericUpDown diffuseAmountUd;
        private System.Windows.Forms.NumericUpDown specAmountUd;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox disableTextureCh;
        private System.Windows.Forms.CheckBox texColBlendCb;
        private HueSatButton ambientHsBtn;
        private HueSatButton masterLightHsBtn;
        private HueSatButton specHsBtn;
        private HueSatButton diffuseHsBtn;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.NumericUpDown playbackOffsetUd;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.NumericUpDown fadeOutUd;
        private System.Windows.Forms.NumericUpDown fadeInUd;
        private System.Windows.Forms.SaveFileDialog saveMidiDialog;
        private System.Windows.Forms.ToolStripMenuItem viewMidiBrowserTSMI;
        private System.Windows.Forms.ToolStripMenuItem loadTrackPropsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveTrackPropsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openTrackPropsFileDialog;
        private System.Windows.Forms.SaveFileDialog saveTrackPropsFileDialog;
        private System.Windows.Forms.ContextMenuStrip trackPropsTabCM;
        private System.Windows.Forms.ToolStripMenuItem loadTrackPropsTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveTrackPtopsTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadCamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveCamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tracksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultPropertiesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem loadPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePropertiesToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openCamFileDialog;
        private System.Windows.Forms.SaveFileDialog saveCamFileDialog;
        private System.Windows.Forms.DataGridView lyricsGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LyricsColumn;
        private System.Windows.Forms.Label camLabel;
        private System.Windows.Forms.ToolStripMenuItem insertLyricsHereToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertKeyFrameToolStripMenuItem;
        private System.Windows.Forms.DataGridView keyFramesDGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.Panel propsTogglePanel;
        private System.Windows.Forms.CheckBox songPropsCb;
        private System.Windows.Forms.CheckBox trackPropsCb;
        private System.Windows.Forms.NumericUpDown masterLightAmountUd;
        private System.Windows.Forms.ToolStripMenuItem playbackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startStopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem beginningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem endToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nudgeBackwardsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nudgeForwardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpBackwardsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpForwardToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private LineStyleControl lineStyleControl;
    }
}

