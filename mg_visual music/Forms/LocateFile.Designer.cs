namespace Visual_Music
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
			this.fileTb = new System.Windows.Forms.TextBox();
			this.browseBtn = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(251, 74);
			this.okBtn.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(332, 74);
			// 
			// fileTb
			// 
			this.fileTb.Location = new System.Drawing.Point(12, 36);
			this.fileTb.Name = "fileTb";
			this.fileTb.Size = new System.Drawing.Size(339, 20);
			this.fileTb.TabIndex = 3;
			// 
			// browseBtn
			// 
			this.browseBtn.Location = new System.Drawing.Point(357, 36);
			this.browseBtn.Name = "browseBtn";
			this.browseBtn.Size = new System.Drawing.Size(31, 23);
			this.browseBtn.TabIndex = 4;
			this.browseBtn.Text = "...";
			this.browseBtn.UseVisualStyleBackColor = true;
			this.browseBtn.Click += new System.EventHandler(this.browseBtn_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.AutoSize = true;
			this.messageLabel.Location = new System.Drawing.Point(12, 9);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(35, 13);
			this.messageLabel.TabIndex = 5;
			this.messageLabel.Text = "label1";
			// 
			// LocateFile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(419, 109);
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.browseBtn);
			this.Controls.Add(this.fileTb);
			this.Name = "LocateFile";
			this.Text = "Locate file";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.fileTb, 0);
			this.Controls.SetChildIndex(this.browseBtn, 0);
			this.Controls.SetChildIndex(this.messageLabel, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox fileTb;
		private System.Windows.Forms.Button browseBtn;
		private System.Windows.Forms.Label messageLabel;
	}
}