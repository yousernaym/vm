namespace Visual_Music
{
	partial class ImportNotesWithAudioForm
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
			if(disposing && (components != null))
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
            this.existingAudioRbtn = new System.Windows.Forms.RadioButton();
            this.thirdPartyMixdownRbtn = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tpartyAppTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tpartyAudioTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tPartyMixdownPnl = new System.Windows.Forms.Panel();
            this.browseTpartyExeBtn = new System.Windows.Forms.Button();
            this.browseTpartyOutputBtn = new System.Windows.Forms.Button();
            this.openTpartyExeDlg = new System.Windows.Forms.OpenFileDialog();
            this.openTpartyAudioDlg = new System.Windows.Forms.OpenFileDialog();
            this.runTpartyBtn = new System.Windows.Forms.Button();
            this.tpartyArgsTb = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.tPartyMixdownPnl.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.Text = "Note file";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(242, 257);
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(323, 257);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(252, 53);
            this.label1.Visible = false;
            // 
            // eraseCurrent
            // 
            this.eraseCurrent.Location = new System.Drawing.Point(12, 260);
            // 
            // existingAudioRbtn
            // 
            this.existingAudioRbtn.AutoSize = true;
            this.existingAudioRbtn.Location = new System.Drawing.Point(3, 3);
            this.existingAudioRbtn.Name = "existingAudioRbtn";
            this.existingAudioRbtn.Size = new System.Drawing.Size(106, 17);
            this.existingAudioRbtn.TabIndex = 9;
            this.existingAudioRbtn.TabStop = true;
            this.existingAudioRbtn.Text = "Existing audio file";
            this.existingAudioRbtn.UseVisualStyleBackColor = true;
            this.existingAudioRbtn.CheckedChanged += new System.EventHandler(this.existingAudioRbtn_CheckedChanged);
            // 
            // thirdPartyMixdownRbtn
            // 
            this.thirdPartyMixdownRbtn.AutoSize = true;
            this.thirdPartyMixdownRbtn.Location = new System.Drawing.Point(3, 53);
            this.thirdPartyMixdownRbtn.Name = "thirdPartyMixdownRbtn";
            this.thirdPartyMixdownRbtn.Size = new System.Drawing.Size(119, 17);
            this.thirdPartyMixdownRbtn.TabIndex = 10;
            this.thirdPartyMixdownRbtn.TabStop = true;
            this.thirdPartyMixdownRbtn.Text = "Third-party mixdown";
            this.thirdPartyMixdownRbtn.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.thirdPartyMixdownRbtn);
            this.panel1.Controls.Add(this.existingAudioRbtn);
            this.panel1.Location = new System.Drawing.Point(12, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(136, 69);
            this.panel1.TabIndex = 11;
            // 
            // tpartyAppTb
            // 
            this.tpartyAppTb.Location = new System.Drawing.Point(76, 0);
            this.tpartyAppTb.Name = "tpartyAppTb";
            this.tpartyAppTb.Size = new System.Drawing.Size(258, 20);
            this.tpartyAppTb.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Audio file";
            // 
            // tpartyAudioTb
            // 
            this.tpartyAudioTb.Location = new System.Drawing.Point(76, 51);
            this.tpartyAudioTb.Name = "tpartyAudioTb";
            this.tpartyAudioTb.Size = new System.Drawing.Size(258, 20);
            this.tpartyAudioTb.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Application";
            // 
            // tPartyMixdownPnl
            // 
            this.tPartyMixdownPnl.Controls.Add(this.tpartyArgsTb);
            this.tPartyMixdownPnl.Controls.Add(this.label5);
            this.tPartyMixdownPnl.Controls.Add(this.runTpartyBtn);
            this.tPartyMixdownPnl.Controls.Add(this.browseTpartyOutputBtn);
            this.tPartyMixdownPnl.Controls.Add(this.browseTpartyExeBtn);
            this.tPartyMixdownPnl.Controls.Add(this.tpartyAppTb);
            this.tPartyMixdownPnl.Controls.Add(this.label3);
            this.tPartyMixdownPnl.Controls.Add(this.tpartyAudioTb);
            this.tPartyMixdownPnl.Controls.Add(this.label4);
            this.tPartyMixdownPnl.Location = new System.Drawing.Point(15, 128);
            this.tPartyMixdownPnl.Name = "tPartyMixdownPnl";
            this.tPartyMixdownPnl.Size = new System.Drawing.Size(384, 83);
            this.tPartyMixdownPnl.TabIndex = 16;
            // 
            // browseTpartyExeBtn
            // 
            this.browseTpartyExeBtn.Location = new System.Drawing.Point(340, 0);
            this.browseTpartyExeBtn.Name = "browseTpartyExeBtn";
            this.browseTpartyExeBtn.Size = new System.Drawing.Size(28, 20);
            this.browseTpartyExeBtn.TabIndex = 16;
            this.browseTpartyExeBtn.Text = "...";
            this.browseTpartyExeBtn.UseVisualStyleBackColor = true;
            this.browseTpartyExeBtn.Click += new System.EventHandler(this.browseTpartyExeBtn_Click);
            // 
            // browseTpartyOutputBtn
            // 
            this.browseTpartyOutputBtn.Location = new System.Drawing.Point(340, 51);
            this.browseTpartyOutputBtn.Name = "browseTpartyOutputBtn";
            this.browseTpartyOutputBtn.Size = new System.Drawing.Size(28, 20);
            this.browseTpartyOutputBtn.TabIndex = 17;
            this.browseTpartyOutputBtn.Text = "...";
            this.browseTpartyOutputBtn.UseVisualStyleBackColor = true;
            this.browseTpartyOutputBtn.Click += new System.EventHandler(this.browseTpartyOutputBtn_Click);
            // 
            // openTpartyExeDlg
            // 
            this.openTpartyExeDlg.Filter = "Executables (*.exe; *.bat)|*.exe;*.bat|All files (*.*)|*.*";
            // 
            // openTpartyAudioDlg
            // 
            this.openTpartyAudioDlg.CheckFileExists = false;
            this.openTpartyAudioDlg.FileName = "%notefile";
            this.openTpartyAudioDlg.Filter = "Audio files (*.mp3; *.wma; *.wav)|*.mp3; *.wma; *.wav|All files|*.*";
            this.openTpartyAudioDlg.ValidateNames = false;
            // 
            // runTpartyBtn
            // 
            this.runTpartyBtn.Location = new System.Drawing.Point(340, 26);
            this.runTpartyBtn.Name = "runTpartyBtn";
            this.runTpartyBtn.Size = new System.Drawing.Size(39, 20);
            this.runTpartyBtn.TabIndex = 18;
            this.runTpartyBtn.Text = "Run";
            this.runTpartyBtn.UseVisualStyleBackColor = true;
            // 
            // tpartyArgsTb
            // 
            this.tpartyArgsTb.Location = new System.Drawing.Point(76, 26);
            this.tpartyArgsTb.Name = "tpartyArgsTb";
            this.tpartyArgsTb.Size = new System.Drawing.Size(258, 20);
            this.tpartyArgsTb.TabIndex = 19;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Arguments";
            // 
            // ImportNotesWithAudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(415, 292);
            this.Controls.Add(this.tPartyMixdownPnl);
            this.Controls.Add(this.panel1);
            this.Name = "ImportNotesWithAudioForm";
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.tPartyMixdownPnl, 0);
            this.Controls.SetChildIndex(this.BrowseAudioBtn, 0);
            this.Controls.SetChildIndex(this.audioFilePath, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.Ok, 0);
            this.Controls.SetChildIndex(this.Cancel, 0);
            this.Controls.SetChildIndex(this.eraseCurrent, 0);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tPartyMixdownPnl.ResumeLayout(false);
            this.tPartyMixdownPnl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tpartyAppTb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tpartyAudioTb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel tPartyMixdownPnl;
        private System.Windows.Forms.Button browseTpartyExeBtn;
        private System.Windows.Forms.Button browseTpartyOutputBtn;
        private System.Windows.Forms.OpenFileDialog openTpartyExeDlg;
        private System.Windows.Forms.OpenFileDialog openTpartyAudioDlg;
        private System.Windows.Forms.Button runTpartyBtn;
        private System.Windows.Forms.TextBox tpartyArgsTb;
        private System.Windows.Forms.Label label5;
        protected System.Windows.Forms.RadioButton existingAudioRbtn;
        protected System.Windows.Forms.RadioButton thirdPartyMixdownRbtn;
    }
}
