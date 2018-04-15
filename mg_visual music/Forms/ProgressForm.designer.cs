namespace Visual_Music
{
	partial class ProgressForm
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
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.button1 = new System.Windows.Forms.Button();
			this.percentLabel = new System.Windows.Forms.Label();
			this.estimatedTimeLabel = new System.Windows.Forms.Label();
			this.elapsedTimeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 12);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(369, 23);
			this.progressBar1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.Location = new System.Drawing.Point(306, 65);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Cancel";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// percentLabel
			// 
			this.percentLabel.AutoSize = true;
			this.percentLabel.Location = new System.Drawing.Point(360, 9);
			this.percentLabel.Name = "percentLabel";
			this.percentLabel.Size = new System.Drawing.Size(21, 13);
			this.percentLabel.TabIndex = 2;
			this.percentLabel.Text = "0%";
			this.percentLabel.Visible = false;
			// 
			// estimatedTimeLabel
			// 
			this.estimatedTimeLabel.AutoSize = true;
			this.estimatedTimeLabel.Location = new System.Drawing.Point(12, 75);
			this.estimatedTimeLabel.Name = "estimatedTimeLabel";
			this.estimatedTimeLabel.Size = new System.Drawing.Size(95, 13);
			this.estimatedTimeLabel.TabIndex = 3;
			this.estimatedTimeLabel.Text = "Estimated time left:";
			this.estimatedTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// elapsedTimeLabel
			// 
			this.elapsedTimeLabel.AutoSize = true;
			this.elapsedTimeLabel.Location = new System.Drawing.Point(12, 51);
			this.elapsedTimeLabel.Name = "elapsedTimeLabel";
			this.elapsedTimeLabel.Size = new System.Drawing.Size(70, 13);
			this.elapsedTimeLabel.TabIndex = 4;
			this.elapsedTimeLabel.Text = "Elapsed time:";
			this.elapsedTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ProgressForm
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button1;
			this.ClientSize = new System.Drawing.Size(393, 97);
			this.Controls.Add(this.elapsedTimeLabel);
			this.Controls.Add(this.estimatedTimeLabel);
			this.Controls.Add(this.percentLabel);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.progressBar1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "ProgressForm";
			this.ShowInTaskbar = false;
			this.Text = "Progress";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProgressForm_FormClosed);
			this.VisibleChanged += new System.EventHandler(this.ProgressForm_VisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label percentLabel;
		private System.Windows.Forms.Label estimatedTimeLabel;
		private System.Windows.Forms.Label elapsedTimeLabel;
	}
}