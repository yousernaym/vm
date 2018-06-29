namespace Visual_Music.Forms
{
	partial class SubSongForm
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
			this.subSongsLB = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(41, 28);
			this.okBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(120, 28);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			// 
			// subSongsLB
			// 
			this.subSongsLB.Dock = System.Windows.Forms.DockStyle.Top;
			this.subSongsLB.FormattingEnabled = true;
			this.subSongsLB.Location = new System.Drawing.Point(0, 0);
			this.subSongsLB.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.subSongsLB.Name = "subSongsLB";
			this.subSongsLB.Size = new System.Drawing.Size(206, 17);
			this.subSongsLB.TabIndex = 3;
			this.subSongsLB.Resize += new System.EventHandler(this.subSongsLB_Resize);
			// 
			// SubSongForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(206, 62);
			this.Controls.Add(this.subSongsLB);
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SubSongForm";
			this.Text = "Select sub song";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.subSongsLB, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox subSongsLB;
	}
}
