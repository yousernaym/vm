namespace VisualMusic
{
	partial class StopRenderingMb
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.messageLabel = new System.Windows.Forms.Label();
			this.yesBtn = new System.Windows.Forms.Button();
			this.noBtn = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Window;
			this.panel1.Controls.Add(this.messageLabel);
			this.panel1.Location = new System.Drawing.Point(-9, -3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(236, 55);
			this.panel1.TabIndex = 6;
			// 
			// messageLabel
			// 
			this.messageLabel.AutoSize = true;
			this.messageLabel.Location = new System.Drawing.Point(21, 22);
			this.messageLabel.MaximumSize = new System.Drawing.Size(400, 0);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(200, 13);
			this.messageLabel.TabIndex = 2;
			this.messageLabel.Text = "Are you sure you want to stop rendering?";
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// yesBtn
			// 
			this.yesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.yesBtn.Location = new System.Drawing.Point(52, 67);
			this.yesBtn.Name = "yesBtn";
			this.yesBtn.Size = new System.Drawing.Size(75, 23);
			this.yesBtn.TabIndex = 5;
			this.yesBtn.Text = "Yes";
			this.yesBtn.UseVisualStyleBackColor = true;
			this.yesBtn.Click += new System.EventHandler(this.yesBtn_Click);
			// 
			// noBtn
			// 
			this.noBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.noBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.noBtn.Location = new System.Drawing.Point(133, 67);
			this.noBtn.Name = "noBtn";
			this.noBtn.Size = new System.Drawing.Size(75, 23);
			this.noBtn.TabIndex = 4;
			this.noBtn.Text = "No";
			this.noBtn.UseVisualStyleBackColor = true;
			this.noBtn.Click += new System.EventHandler(this.noBtn_Click);
			// 
			// StopRenderingMb
			// 
			this.AcceptButton = this.yesBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.noBtn;
			this.ClientSize = new System.Drawing.Size(220, 102);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.yesBtn);
			this.Controls.Add(this.noBtn);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StopRenderingMb";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.StopRenderingMb_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button yesBtn;
		private System.Windows.Forms.Button noBtn;
	}
}