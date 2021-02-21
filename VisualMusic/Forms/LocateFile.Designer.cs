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
			this.filePathTb = new System.Windows.Forms.TextBox();
			this.findInFolderBtn = new System.Windows.Forms.Button();
			this.retryBtn = new System.Windows.Forms.Button();
			this.errorLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// findFileBtn
			// 
			this.findFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.findFileBtn.Location = new System.Drawing.Point(268, 83);
			this.findFileBtn.Name = "findFileBtn";
			this.findFileBtn.Size = new System.Drawing.Size(67, 23);
			this.findFileBtn.TabIndex = 4;
			this.findFileBtn.Text = "Select file";
			this.findFileBtn.UseVisualStyleBackColor = true;
			this.findFileBtn.Click += new System.EventHandler(this.selectFileBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(341, 83);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 5;
			this.cancelBtn.Text = "Ignore";
			this.cancelBtn.UseVisualStyleBackColor = true;
			// 
			// filePathTb
			// 
			this.filePathTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filePathTb.ForeColor = System.Drawing.SystemColors.WindowText;
			this.filePathTb.Location = new System.Drawing.Point(12, 25);
			this.filePathTb.Multiline = true;
			this.filePathTb.Name = "filePathTb";
			this.filePathTb.ReadOnly = true;
			this.filePathTb.Size = new System.Drawing.Size(404, 52);
			this.filePathTb.TabIndex = 1;
			this.filePathTb.TabStop = false;
			// 
			// findInFolderBtn
			// 
			this.findInFolderBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.findInFolderBtn.Location = new System.Drawing.Point(141, 83);
			this.findInFolderBtn.Name = "findInFolderBtn";
			this.findInFolderBtn.Size = new System.Drawing.Size(121, 23);
			this.findInFolderBtn.TabIndex = 3;
			this.findInFolderBtn.Text = "Find filename in folder";
			this.findInFolderBtn.UseVisualStyleBackColor = true;
			this.findInFolderBtn.Click += new System.EventHandler(this.findInFolderBtn_Click);
			// 
			// retryBtn
			// 
			this.retryBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.retryBtn.Location = new System.Drawing.Point(12, 83);
			this.retryBtn.Name = "retryBtn";
			this.retryBtn.Size = new System.Drawing.Size(64, 23);
			this.retryBtn.TabIndex = 2;
			this.retryBtn.Text = "Retry";
			this.retryBtn.UseVisualStyleBackColor = true;
			this.retryBtn.Click += new System.EventHandler(this.retryBtn_Click);
			// 
			// errorLabel
			// 
			this.errorLabel.AutoSize = true;
			this.errorLabel.Location = new System.Drawing.Point(12, 9);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(35, 13);
			this.errorLabel.TabIndex = 6;
			this.errorLabel.Text = "label1";
			// 
			// LocateFile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(428, 118);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.retryBtn);
			this.Controls.Add(this.filePathTb);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.findInFolderBtn);
			this.Controls.Add(this.findFileBtn);
			this.MaximizeBox = false;
			this.Name = "LocateFile";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button findFileBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.TextBox filePathTb;
		private System.Windows.Forms.Button findInFolderBtn;
		private System.Windows.Forms.Button retryBtn;
		private System.Windows.Forms.Label errorLabel;
	}
}