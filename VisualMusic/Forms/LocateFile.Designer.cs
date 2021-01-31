namespace VisualMusic
{
	partial class LocateFile
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
			this.findFileBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.messageTb = new System.Windows.Forms.TextBox();
			this.findInFolderBtn = new System.Windows.Forms.Button();
			this.retryBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// findFileBtn
			// 
			this.findFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.findFileBtn.Location = new System.Drawing.Point(268, 93);
			this.findFileBtn.Name = "findFileBtn";
			this.findFileBtn.Size = new System.Drawing.Size(67, 23);
			this.findFileBtn.TabIndex = 0;
			this.findFileBtn.Text = "Select file";
			this.findFileBtn.UseVisualStyleBackColor = true;
			this.findFileBtn.Click += new System.EventHandler(this.findFileBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(341, 93);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 0;
			this.cancelBtn.Text = "Ignore";
			this.cancelBtn.UseVisualStyleBackColor = true;
			// 
			// messageTb
			// 
			this.messageTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageTb.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageTb.ForeColor = System.Drawing.SystemColors.WindowText;
			this.messageTb.Location = new System.Drawing.Point(12, 12);
			this.messageTb.Multiline = true;
			this.messageTb.Name = "messageTb";
			this.messageTb.ReadOnly = true;
			this.messageTb.Size = new System.Drawing.Size(404, 70);
			this.messageTb.TabIndex = 1;
			this.messageTb.TabStop = false;
			// 
			// findInFolderBtn
			// 
			this.findInFolderBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.findInFolderBtn.Location = new System.Drawing.Point(141, 93);
			this.findInFolderBtn.Name = "findInFolderBtn";
			this.findInFolderBtn.Size = new System.Drawing.Size(121, 23);
			this.findInFolderBtn.TabIndex = 0;
			this.findInFolderBtn.Text = "Find filename in folder";
			this.findInFolderBtn.UseVisualStyleBackColor = true;
			this.findInFolderBtn.Click += new System.EventHandler(this.findInFolderBtn_Click);
			// 
			// retryBtn
			// 
			this.retryBtn.Location = new System.Drawing.Point(12, 93);
			this.retryBtn.Name = "retryBtn";
			this.retryBtn.Size = new System.Drawing.Size(64, 23);
			this.retryBtn.TabIndex = 2;
			this.retryBtn.Text = "Retry";
			this.retryBtn.UseVisualStyleBackColor = true;
			this.retryBtn.Click += new System.EventHandler(this.retryBtn_Click);
			// 
			// LocateFile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(428, 128);
			this.Controls.Add(this.retryBtn);
			this.Controls.Add(this.messageTb);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.findInFolderBtn);
			this.Controls.Add(this.findFileBtn);
			this.Name = "LocateFile";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button findFileBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.TextBox messageTb;
		private System.Windows.Forms.Button findInFolderBtn;
		private System.Windows.Forms.Button retryBtn;
	}
}