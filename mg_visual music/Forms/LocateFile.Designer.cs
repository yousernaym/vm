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
			this.messageLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
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
			this.AcceptButton = null;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(332, 109);
			this.Controls.Add(this.messageLabel);
			this.Name = "LocateFile";
			this.Text = "Locate file";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.messageLabel, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label messageLabel;
	}
}