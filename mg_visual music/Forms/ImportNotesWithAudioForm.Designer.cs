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
            this.cmdLineTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmdLineOutputTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tPartyMixdownPnl = new System.Windows.Forms.Panel();
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
            this.Ok.Location = new System.Drawing.Point(227, 211);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(308, 211);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(252, 53);
            this.label1.Visible = false;
            // 
            // eraseCurrent
            // 
            this.eraseCurrent.Location = new System.Drawing.Point(12, 214);
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
            this.thirdPartyMixdownRbtn.Text = "Third party mixdown";
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
            // cmdLineTb
            // 
            this.cmdLineTb.Location = new System.Drawing.Point(76, 0);
            this.cmdLineTb.Name = "cmdLineTb";
            this.cmdLineTb.Size = new System.Drawing.Size(258, 20);
            this.cmdLineTb.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Audio file";
            // 
            // cmdLineOutputTb
            // 
            this.cmdLineOutputTb.Location = new System.Drawing.Point(76, 26);
            this.cmdLineOutputTb.Name = "cmdLineOutputTb";
            this.cmdLineOutputTb.Size = new System.Drawing.Size(258, 20);
            this.cmdLineOutputTb.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Command line";
            // 
            // tPartyMixdownPnl
            // 
            this.tPartyMixdownPnl.Controls.Add(this.cmdLineTb);
            this.tPartyMixdownPnl.Controls.Add(this.label3);
            this.tPartyMixdownPnl.Controls.Add(this.cmdLineOutputTb);
            this.tPartyMixdownPnl.Controls.Add(this.label4);
            this.tPartyMixdownPnl.Location = new System.Drawing.Point(15, 128);
            this.tPartyMixdownPnl.Name = "tPartyMixdownPnl";
            this.tPartyMixdownPnl.Size = new System.Drawing.Size(383, 53);
            this.tPartyMixdownPnl.TabIndex = 16;
            // 
            // ImportNotesWithAudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(400, 246);
            this.Controls.Add(this.tPartyMixdownPnl);
            this.Controls.Add(this.panel1);
            this.Name = "ImportNotesWithAudioForm";
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.tPartyMixdownPnl, 0);
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

        private System.Windows.Forms.RadioButton existingAudioRbtn;
        private System.Windows.Forms.RadioButton thirdPartyMixdownRbtn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox cmdLineTb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox cmdLineOutputTb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel tPartyMixdownPnl;
    }
}
